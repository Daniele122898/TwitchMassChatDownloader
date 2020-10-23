using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LirikChatDownloader.Chat.Dtos
{
    public class CommentsDto
    {
        [JsonPropertyName("comments")]
        public List<Comment> Comments { get; set; }

        [JsonPropertyName("_next")]
        public string Next { get; set; }
    }

    public class Comment
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("content_offset_seconds")]
        public float ContentOffsetSeconds { get; set; }

        [JsonPropertyName("commenter")]
        public Commenter Commenter { get; set; }

        [JsonPropertyName("message")]
        public MessageContent MessageContent { get; set; }
    }

    public class MessageContent
    {
        [JsonPropertyName("body")]
        public string Body { get; set; }

        [JsonPropertyName("emoticons")]
        public List<Emote> Emotes { get; set; }

        [JsonPropertyName("user_color")]
        public string UserColor { get; set; }
    }

    public class Emote
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("begin")]
        public int Begin { get; set; }

        [JsonPropertyName("end")]
        public int End { get; set; }
    }

    public class Commenter
    {
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }
    }
}