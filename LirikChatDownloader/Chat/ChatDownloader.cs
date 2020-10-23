using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using ArgonautCore.Lw;
using ArgonautCore.Network.Http;
using LirikChatDownloader.Chat.Dtos;
using LirikChatDownloader.Streamer.Dtos;
using Newtonsoft.Json;
using Serilog;

namespace LirikChatDownloader.Chat
{
    public class ChatDownloader : IDisposable
    {
        private readonly CoreHttpClient _http;

        public ChatDownloader()
        {
            var http = new HttpClient();
            http.DefaultRequestHeaders.Clear();
            http.DefaultRequestHeaders.Add("Accept", "application/vnd.twitchtv.v5+json; charset=UTF-8");
            http.DefaultRequestHeaders.Add("Client-Id", "***REMOVED***");
            
            _http = new CoreHttpClient(http);
        }

        public async Task<Result<bool, Error>> TryDownloadAndSaveChat(Video video, string filePath)
        {
            if (File.Exists(filePath))
            {
                Log.Error($"File with path {filePath} already exists. Won't override!");
                return new Result<bool, Error>(new Error($"File with path {filePath} already exists. Won't override!"));
            }

            var comments = await this.DownloadChat(video).ConfigureAwait(false);
            if (!comments)
                return new Result<bool, Error>(comments.Err());
            
            var chat = new Dtos.Chat(video, comments.Some());
            var json = JsonConvert.SerializeObject(chat);
            await File.WriteAllTextAsync(filePath, json);
            return true;
        }
        
        public async Task<Result<List<Comment>, Error>> DownloadChat(Video video)
        {
            string queryBase = $"https://api.twitch.tv/v5/videos/{video.Id.TrimStart('v')}/comments?";

            // Reserve a lot of space so we don't get too much re-sizing overhead since there will be a lot of messages
            List<Comment> comments = new List<Comment>(100000);
            // Make first request
            var first = await _http.GetAndMapResponse<CommentsDto>($"{queryBase}content_offset_seconds=0").ConfigureAwait(false);
            if (!first)
            {
                Log.Error("Failed to fetch first batch of comments. Make sure VOD ID is valid!");
                return new Result<List<Comment>, Error>(first.Err());
            }

            var f = (~first);
            comments.AddRange(f.Comments);

            if (string.IsNullOrWhiteSpace(f.Next))
                return comments;
            
            // Walk the linked list and keep downloading
            string next = f.Next;
            uint counter = 0;
            while (true)
            {
                var resp = await _http.GetAndMapResponse<CommentsDto>($"{queryBase}cursor={next}").ConfigureAwait(false);
                if (!resp)
                {
                    Log.Error($"Failed to fetch subsequent comment batch, returning first batch\n{resp.Err().Message.Get()}\n{resp.Err().Trace.SomeOrDefault("")}");
                    return comments;
                }

                var coms = resp.Some();
                comments.AddRange(coms.Comments);

                ++counter;
                if (counter % 10 == 0)
                {
                    Log.Debug(
                        $"{video.Id}: {((coms.Comments[0].ContentOffsetSeconds / (float) video.LengthInSeconds) * 100f).ToString(CultureInfo.InvariantCulture)}% of dump complete.");
                    counter = 0;
                }

                if (string.IsNullOrWhiteSpace(coms.Next))
                    return comments;

                next = coms.Next;
            }
        }

        public void Dispose()
        {
            _http?.Dispose();
        }
    }
}