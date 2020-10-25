using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ArgonautCore.Lw;
using ArgonautCore.Network.Enums;
using ArgonautCore.Network.Http;
using LirikChatDownloader.Streamer.Dtos;
using LirikChatDownloader.Vod.Dtos;
using Newtonsoft.Json;
using Serilog;

namespace LirikChatDownloader.Vod
{
    public class VodInfoDownloader
    {
        private readonly CoreHttpClient _http;

        public VodInfoDownloader()
        {
            var http = new HttpClient();
            http.DefaultRequestHeaders.Clear();
            http.DefaultRequestHeaders.Add("Accept", "application/vnd.twitchtv.v5+json; charset=UTF-8");
            http.DefaultRequestHeaders.Add("Client-Id", "***REMOVED***");
            
            _http = new CoreHttpClient(http);
        }

        public async Task<Result<bool, Error>> TryDownloadAndSaveVodMetadata(Video video, string filePath)
        {
            if (File.Exists(filePath))
            {
                Log.Error($"File with path {filePath} already exists. Won't override!");
                return new Result<bool, Error>(new Error($"File with path {filePath} already exists. Won't override!"));
            }

            var resp = await this.GetVodMetadata(video);
            if (!resp)
            {
                Log.Error($"Failed to fetch Vod Metadata\n{resp.Err().Message.Get()}");
                return new Result<bool, Error>(resp.Err());
            }
            
            // Transforming metadata
            Log.Debug("Got Video metadata, transforming into usable format.");
            VodMetadata data = new VodMetadata()
            {
                Video = video,
                Games = resp.Some()[0]?.Data.Video.Moments.Edges.Select(x => new GameInfo()
                {
                    DurationMilliseconds = x.Node.DurationMilliseconds,
                    PositionMilliseconds = x.Node.PositionMilliseconds,
                    Id = x.Node.Details.Game.Id,
                    Title = x.Node.Details.Game.DisplayName,
                    BoxArtUrl = x.Node.Details.Game.BoxArtUrl
                }).ToList()
            };
            
            // Saving to file
            var json = JsonConvert.SerializeObject(data);
            await File.WriteAllTextAsync(filePath, json);
            Log.Information($"Dumped Vod Metadata to {filePath}");
            return true;
        }

        public async Task<Result<List<Dtos.Vod>, Error>> GetVodMetadata(Video video)
        {
            string query = "[{\"operationName\":\"VideoPlayer_ChapterSelectButtonVideo\",\"variables\":{\"videoID\":\"{0}\"},\"extensions\":{\"persistedQuery\":{\"version\":1,\"sha256Hash\":\"b63c615e4fec1fbc3e6dd2d471c818886c4cf528c0cf99c136ba3981024f5e98\"}}}]";
            // We cant use string.Format bcs of the amount of {} in the string. this is hacky as hell but whatever this entire program is one hack
            string json = query.Replace("{0}", video.Id.TrimStart('v'));
            HttpContent content = new StringContent(json);
            return await _http.GetAndMapResponse<List<Dtos.Vod>>("https://gql.twitch.tv/gql", HttpMethods.Post, httpContent: content)
                .ConfigureAwait(false);
        }
    }
}