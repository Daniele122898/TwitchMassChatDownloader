using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LirikChatDownloader.Streamer.Dtos
{
    public class VideosDto
    {
        [JsonPropertyName("_total")]
        public int Total { get; set; }

        [JsonPropertyName("videos")]
        public List<Video> Videos { get; set; }
        
    }

    public class Video
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }
        
        [JsonPropertyName("broadcast_id")] 
        public ulong BroadcastId { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("length")] 
        public int LengthInSeconds { get; set; }
    }
}