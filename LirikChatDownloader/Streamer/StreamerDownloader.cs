using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ArgonautCore.Lw;
using ArgonautCore.Network.Http;
using LirikChatDownloader.Streamer.Dtos;
using Serilog;

namespace LirikChatDownloader.Streamer
{
    public class StreamerDownloader : IDisposable
    {
        private readonly CoreHttpClient _http;
        
        private const int _MAX_VOD_AMOUNT = 100;
        private const string _CHANNEL_VIDEOS_URL = "https://api.twitch.tv/kraken/channels/{0}/videos";

        public StreamerDownloader()
        {
            var http = new HttpClient();
            http.DefaultRequestHeaders.Clear();
            http.DefaultRequestHeaders.Add("Accept", "application/vnd.twitchtv.v5+json; charset=UTF-8");
            http.DefaultRequestHeaders.Add("Client-Id", "");
            http.DefaultRequestHeaders.Add("Authorization", "Bearer ");
            
            _http = new CoreHttpClient(http);
        }

        public async Task<bool> IsChannelLive(string name)
        {
            var res = await _http.GetAndMapResponse<StreamStatusDto>(
                $"https://api.twitch.tv/helix/streams?user_login={name}");
            if (!res)
            {
                Log.Error($"Failed to get Channel Stream Status {name}\n{(res.Err().Message.Get())}\n{res.Err().Trace.SomeOrDefault("")}");
                return false;
            }

            return res.Some().Data?.Count > 0;
        }

        public async Task<List<Video>> GetChannelVods(string channelId, int amount = int.MaxValue)
        {
            if (amount < 1)
                amount = int.MaxValue;

            int offset = 0;
            List<Video> videos = new List<Video>();
            string defaultParams = $"?broadcast_type=Archive&limit={_MAX_VOD_AMOUNT.ToString()}";
            do
            {
                string queryParams = $"{defaultParams}&offset={offset.ToString()}";
                string query = $"{string.Format(_CHANNEL_VIDEOS_URL, channelId.ToString())}{queryParams}";

                var videoResp = await _http.GetAndMapResponse<VideosDto>(query).ConfigureAwait(false);
                if (!videoResp || (~videoResp).Total == 0)
                    break;
                
                var v = (~videoResp);
                if (amount == int.MaxValue)
                    amount = v.Total;
                else if (amount > v.Total)
                    amount = v.Total;

                offset += v.Videos.Count;

                videos.AddRange(v.Videos);
                
            } while (offset < amount);

            return videos;
        }

        public async Task<Result<string, Error>> GetChannelIdByName(string name)
        {
            var streamers = await _http.GetAndMapResponse<StreamerDto>($"https://api.twitch.tv/kraken/users?login={name}").ConfigureAwait(false);
            if (!streamers)
            {
                Log.Error($"Failed to get Channel ID by Name {name}\n{(streamers.Err().Message.Get())}\n{streamers.Err().Trace.SomeOrDefault("")}");
                return new Result<string, Error>(streamers.Err());
            }

            var sstream = streamers.Some();
            if (sstream.Total == 0)
            {
                Log.Error($"No streamers found with Name: {name}");
                return new Result<string, Error>(new Error($"No streamer found with name: {name}"));
            }

            if (sstream.Total > 1)
            {
                Log.Error($"More than one streamer found with name {name}");
                return new Result<string, Error>(new Error($"More than one streamer found with name {name}. Please specify an exact name"));
            }

            return sstream.Streamers[0].Id;
        }


        public void Dispose()
        {
            this._http?.Dispose();
        }
    }
}