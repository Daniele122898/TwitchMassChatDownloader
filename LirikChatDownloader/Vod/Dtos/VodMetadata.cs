using System.Collections.Generic;
using System.Text.Json.Serialization;
using LirikChatDownloader.Streamer.Dtos;

namespace LirikChatDownloader.Vod.Dtos
{
    public class VodMetadata
    {
        [JsonPropertyName("video")]
        public Video Video { get; set; }

        [JsonPropertyName("games")]
        public List<GameInfo> Games { get; set; }
    }

    public class GameInfo
    {
        [JsonPropertyName("durationMilliseconds")]
        public long DurationMilliseconds { get; set; }

        [JsonPropertyName("positionMilliseconds")]
        public long PositionMilliseconds { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("displayName")]
        public string Title { get; set; }

        [JsonPropertyName("boxArtURL")]
        public string BoxArtUrl { get; set; }
    }
}