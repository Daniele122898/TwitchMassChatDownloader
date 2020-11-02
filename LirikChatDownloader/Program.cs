using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LirikChatDownloader.Chat;
using LirikChatDownloader.Streamer;
using LirikChatDownloader.Streamer.Dtos;
using LirikChatDownloader.Vod;
using Serilog;

namespace LirikChatDownloader
{
    class Program
    {
        // ReSharper disable once UnusedParameter.Local

        private static Timer _timer;
        
        static async Task Main(string[] args)
        {
            
            #if DEBUG
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("log_debug.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            #else
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            #endif
            
            _timer = new Timer(PeriodicCheck, null, TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(15));

            // Wait so we never quit.
            await Task.Delay(-1);
        }

        private static async void PeriodicCheck(object state)
        {
            var streamDownloader = new StreamerDownloader();

            if (await streamDownloader.IsChannelLive("Lirik"))
            {
                Log.Information("Lirik is currently live. Not downloading anything!");
                return;
            }
            
            Log.Debug("Get Lirik channel ID");
            var channelId = await streamDownloader.GetChannelIdByName("Lirik");
            if (!channelId)
            {
                Log.Fatal("Failed to fetch Channel ID");
                Environment.Exit(-1);
            }
            
            Log.Debug("Starting Video Information Download");
            
            var videos = await streamDownloader.GetChannelVods(channelId.Some());
            
            if (videos.Count == 0)
            {
                Log.Fatal("Failed to fetch videos");
                Environment.Exit(-1);
            }
            Log.Debug($"Fetched information about {videos.Count.ToString()} videos");


            var metaTask = GetVodMetadataDump(videos);
            var chatTask = GetChatDump(videos);
            
            Task.WaitAll(metaTask, chatTask);
        }

        static async Task GetVodMetadataDump(List<Video> videos)
        {
            string vodDir = Path.Combine(Directory.GetCurrentDirectory(), "VodLogs");
            if (!Directory.Exists(vodDir))
                Directory.CreateDirectory(vodDir);
            
            var vodInfoDownloader = new VodInfoDownloader();
            bool dumped = false;
            foreach (var video in videos)
            {
                string fileName = $"{video.CreatedAt:yyyy_MM_dd}-{video.Id}.json";
                string path = Path.Combine(vodDir, fileName);
                if (File.Exists(path))
                {
                    Log.Debug($"{video.Id} already exists. Skipping.");
                    continue; // In case the service gets shut down we dont re-download everything everytime.
                }

                dumped = true;
                await vodInfoDownloader.TryDownloadAndSaveVodMetadata(video, path);
            }
            if (dumped)
                Log.Information("Finished Metadata Dump");
        }

        static async Task GetChatDump(List<Video> videos)
        {
            string chatDir = Path.Combine(Directory.GetCurrentDirectory(), "ChatLogs");
            if (!Directory.Exists(chatDir))
                Directory.CreateDirectory(chatDir);
            
            var chatDownloader = new ChatDownloader();
            
            Log.Debug($"Start parallel chat dump into {chatDir}.");
            int offset = 0;
            #if DEBUG
            int streams = 20;
            #else
            int streams = 100;
            #endif
            int bound = 0;
            List<Task> tasks = new List<Task>(streams);
            
            // Let's cache the entire directory in case that helps the inconsistency
            var fileDict = Directory.GetFiles(chatDir)
                .Select(x => Path.GetFileName(x))
                .ToDictionary(x => x);

            bool dumped = false;
            
            // This script had to be done very quick thus if you'd want to do this properly you'd use a window approach like TCP.
            // Where once one download is done another would start and you'd always saturate the streams amount. 
            // Rn we wait until all streams are done but such is life.
            do
            {
                bound = offset + streams;
                if (bound > videos.Count)
                    bound = videos.Count;
                
                Log.Debug($"Dumped {offset.ToString()} Chats.");
                
                for (int i = offset; i < bound; ++i)
                {
                    var video = videos[i];
                    string fileName = $"{video.CreatedAt:yyyy_MM_dd}-{video.Id}.json";
                    string path = Path.Combine(chatDir, fileName);
                    //if (File.Exists(path))
                    if (fileDict.ContainsKey(fileName))
                    {
                        Log.Debug($"{video.Id} already exists. Skipping.");
                        ++bound;
                        if (bound > videos.Count)
                            bound = videos.Count;
                        continue; // In case the service gets shut down we dont re-download everything everytime.
                    }

                    dumped = true;
                    var t = chatDownloader.TryDownloadAndSaveChat(video, path);
                    tasks.Add(t);
                }

                try
                {
                    await Task.WhenAll(tasks);
                }
                catch (Exception e)
                {
                    Log.Error("Exception in Task list: ", e);
                }
                tasks.Clear();
                offset += (bound - offset);
            } while (bound < videos.Count);
            if (dumped)
                Log.Information("Finished parallel chat dump :)");
        }
    }
}