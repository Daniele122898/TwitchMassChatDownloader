using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LirikChatDownloader.Streamer.Dtos
{
    public class StreamerDto
    {
        [JsonPropertyName("_total")]
        public int Total { get; set; }

        [JsonPropertyName("users")]
        public List<Streamer> Streamers { get; set; }
    }

    public class Streamer
    {
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }
        
        [JsonPropertyName("_id")]
        public string Id { get; set; }
    }
}