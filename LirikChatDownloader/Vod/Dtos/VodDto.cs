using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LirikChatDownloader.Vod.Dtos
{
    public class VodDto
    {
        public List<Vod> Vods { get; set; }
    }

    public class Vod
    {
        [JsonProperty("data")] 
        public Data Data { get; set; }
    }

    public class Data
    {
        [JsonProperty("video")] 
        public DataVideo Video { get; set; }
    }

    public class DataVideo
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("moments")] 
        public Moments Moments { get; set; }
    }

    public class Node
    {
        [JsonProperty("durationMilliseconds")] 
        public long DurationMilliseconds { get; set; }

        [JsonProperty("positionMilliseconds")] 
        public long PositionMilliseconds { get; set; }

        [JsonProperty("description")] 
        public string Description { get; set; }

        [JsonProperty("details")] 
        public Details Details { get; set; }

        [JsonProperty("video")] 
        public NodeVideo Video { get; set; }
    }

    public class Edge
    {
        [JsonProperty("node")] 
        public Node Node { get; set; }
    }

    public class Moments
    {
        [JsonProperty("edges")] 
        public List<Edge> Edges { get; set; }
    }

    public class Details
    {
        [JsonProperty("game")] 
        public Game Game { get; set; }
    }

    public class Game
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("displayName")] 
        public string DisplayName { get; set; }

        [JsonProperty("boxArtURL")] 
        public string BoxArtUrl { get; set; }
    }

    public class NodeVideo
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("lengthSeconds")] 
        public long LengthSeconds { get; set; }
    }
}