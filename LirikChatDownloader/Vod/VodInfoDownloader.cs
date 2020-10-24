using System.Net.Http;
using ArgonautCore.Network.Http;

namespace LirikChatDownloader.Vod
{
    public class VodInfoDownloader
    {
        private readonly CoreHttpClient _http;

        public VodInfoDownloader()
        {
            var http = new HttpClient();
            http.DefaultRequestHeaders.Clear();
            http.DefaultRequestHeaders.Add("Accept", "application/vnd.twitchtv.v5+json; charset=UTF-8");
            http.DefaultRequestHeaders.Add("Client-Id", "***REMOVED***");
            
            _http = new CoreHttpClient(http);
        }
    }
}