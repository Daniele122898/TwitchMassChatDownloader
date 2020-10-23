# Twitch Mass Chat Downloader

This tool downloads the entire Chat, thus each message with timestamps emotes etc, from the entire Archive of a twitch streamer. It is completely parallelized and is currently in production downloading with 200 parallel downloads at 280-300 mb/s with about 10 GB ram usage and 250-300% CPU usage (on a linux server).

## Change code required
This has been written in a hurry to download as many Chats as possible from my favorite streamer [Lirik](https://www.twitch.tv/lirik) before he deletes all the VODs because of DMCA (his mods are downloading the VODs already). 

Because of this Lirik has been hardcoded into the programm. The classes and methods to do the actual downloading are decoupled from any hardcoded values thought so all you gotta do is change the "Lirik" in the program.cs to whatever streamer you wish or fork it and make it a programm argument.

## Why this over others?
I've seen a couple tools that allowed you to download Chats but most of them use UIs and only allow single downloads. This is unusable for a task that should be as quick as possible.

Also most of the code was old or just completely unoptimized. Creating Http clients in for loops or using the JParse dictionaries is just incredibly slow especially when trying to walk a linked list of REST requests. My implementation in a single download turned out to be noticably faster. 

By allowing it to download 200 Chats simultaneously the speedup is ofcourse tremendous. If you are in a hurry this is the perfect tool :)

## Feedback
Since this has been created in 2 hours as quickly as possible there might be bugs. Feel free to contribute or just create an Issue!