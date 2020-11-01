using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LirikChatDownloader.Streamer.Dtos
{
    public class StreamStatusDto
    {
        [JsonPropertyName("data")]
        public List<StatusData> Data { get; set; }
    }

    public class StatusData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("user_name")]
        public string UserName { get; set; }
    }
}