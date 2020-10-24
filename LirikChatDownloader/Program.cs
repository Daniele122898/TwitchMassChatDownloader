using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LirikChatDownloader.Chat;
using LirikChatDownloader.Streamer;
using Serilog;
using Serilog.Events;

namespace LirikChatDownloader
{
    class Program
    {
        // ReSharper disable once UnusedParameter.Local
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
            
            string chatDir = Path.Combine(Directory.GetCurrentDirectory(), "ChatLogs");
            if (!Directory.Exists(chatDir))
                Directory.CreateDirectory(chatDir);
            
            var streamDownloader = new StreamerDownloader();
            var chatDownloader = new ChatDownloader();
            
            Log.Information("Get Lirik channel ID");
            var channelId = await streamDownloader.GetChannelIdByName("Lirik");
            if (!channelId)
            {
                Log.Fatal("Failed to fetch Channel ID");
                Environment.Exit(-1);
            }
            
            Log.Information("Starting Video Information Download");
            
            var videos = await streamDownloader.GetChannelVods(channelId.Some());
            
            if (videos.Count == 0)
            {
                Log.Fatal("Failed to fetch videos");
                Environment.Exit(-1);
            }
            Log.Information($"Fetched information about {videos.Count.ToString()} videos");
            
            Log.Information($"Start parallel chat dump into {chatDir}.");
            int offset = 0;
            #if DEBUG
            int streams = 20;
            #else
            int streams = 4;
            #endif
            int bound = 0;
            List<Task> tasks = new List<Task>(streams);
            
            // Let's cache the entire directory in case that helps the inconsistency
            var fileDict = Directory.GetFiles(chatDir)
                .Select(x => Path.GetFileName(x))
                .ToDictionary(x => x);
            
            // This script had to be done very quick thus if you'd want to do this properly you'd use a window approach like TCP.
            // Where once one download is done another would start and you'd always saturate the streams amount. 
            // Rn we wait until all streams are done but such is life.
            do
            {
                bound = offset + streams;
                if (bound > videos.Count)
                    bound = videos.Count;
                
                Log.Information($"Dumped {offset.ToString()} Chats.");
                
                for (int i = offset; i < bound; ++i)
                {
                    var video = videos[i];
                    string fileName = $"{video.CreatedAt:dd_MM_yyyy}-{video.Id}.json";
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
                offset += bound;
            } while (bound < videos.Count);
            Log.Information("Finished parallel chat dump :)");
        }
    }
}