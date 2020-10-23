using System.Collections.Generic;
using System.Text.Json.Serialization;
using LirikChatDownloader.Streamer.Dtos;

namespace LirikChatDownloader.Chat.Dtos
{
    public class Chat
    {
        [JsonPropertyName("video")]
        public Video Video { get; set; }
        
        [JsonPropertyName("comments")]
        public List<Comment> Comments { get; set; }

        public Chat(Video video, List<Comment> comments)
        {
            this.Video = video;
            this.Comments = comments;
        }
    }
}