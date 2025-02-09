using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using YouTubeApiProject.Models;

namespace YouTubeApiProject.Services
{
    public class YouTubeApiService
    {
        private readonly string _apiKey;
        private List<YouTubeVideoModel> _cachedVideos = new List<YouTubeVideoModel>(); // Store last search results

        public YouTubeApiService(IConfiguration configuration)
        {
            _apiKey = configuration["YouTubeApiKey"]; // Fetch API key from appsettings.json
        }

        public async Task<List<YouTubeVideoModel>> SearchVideosAsync(string query, string uploadDate = "", string videoDuration = "")
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = _apiKey,
                ApplicationName = "YoutubeProject"
            });

            var searchRequest = youtubeService.Search.List("snippet");
            searchRequest.Q = query;
            searchRequest.MaxResults = 27;
            searchRequest.Type = "video"; // Ensure only videos are fetched

            // 📌 Apply Upload Date Filter (PublishedAfter)
            if (!string.IsNullOrEmpty(uploadDate))
            {
                DateTime now = DateTime.UtcNow;
                switch (uploadDate.ToLower())
                {
                    case "today":
                        searchRequest.PublishedAfter = now.AddDays(-1).ToString("yyyy-MM-ddTHH:mm:ssZ");
                        break;
                    case "week":
                        searchRequest.PublishedAfter = now.AddDays(-7).ToString("yyyy-MM-ddTHH:mm:ssZ");
                        break;
                    case "month":
                        searchRequest.PublishedAfter = now.AddMonths(-1).ToString("yyyy-MM-ddTHH:mm:ssZ");
                        break;
                    case "year":
                        searchRequest.PublishedAfter = now.AddYears(-1).ToString("yyyy-MM-ddTHH:mm:ssZ");
                        break;
                }
            }

            // 📌 Apply Video Duration Filter
            if (!string.IsNullOrEmpty(videoDuration))
            {
                switch (videoDuration.ToLower())
                {
                    case "short":
                        searchRequest.VideoDuration = SearchResource.ListRequest.VideoDurationEnum.Short__;
                        break;
                    case "medium":
                        searchRequest.VideoDuration = SearchResource.ListRequest.VideoDurationEnum.Medium;
                        break;
                    case "long":
                        searchRequest.VideoDuration = SearchResource.ListRequest.VideoDurationEnum.Long__;
                        break;
                }
            }

            var searchResponse = await searchRequest.ExecuteAsync();
            var videoList = new List<YouTubeVideoModel>();

            foreach (var item in searchResponse.Items)
            {
                // 🔹 Get Channel ID from video snippet
                string channelId = item.Snippet.ChannelId;

                // 🔹 Fetch Channel Info (Name & Profile Picture)
                var channelRequest = youtubeService.Channels.List("snippet");
                channelRequest.Id = channelId;
                var channelResponse = await channelRequest.ExecuteAsync();

                string channelName = channelResponse.Items.FirstOrDefault()?.Snippet.Title ?? "Unknown Channel";
                string channelProfileUrl = channelResponse.Items.FirstOrDefault()?.Snippet.Thumbnails.Default__.Url ?? "";

                // 🔹 Store Video Data with Channel Info
                videoList.Add(new YouTubeVideoModel
                {
                    Title = item.Snippet.Title,
                    Description = item.Snippet.Description,
                    ThumbnailUrl = item.Snippet.Thumbnails.Medium.Url,
                    VideoUrl = "https://www.youtube.com/watch?v=" + item.Id.VideoId,
                    PublishedDate = item.Snippet.PublishedAt?.ToString("yyyy-MM-dd"),
                    ChannelName = channelName,
                    ChannelProfileUrl = channelProfileUrl
                });
            }

            _cachedVideos = videoList; // ✅ Store the new video list
            Console.WriteLine($"[DEBUG] Cached Videos Count: {_cachedVideos.Count}"); // Debugging
            return _cachedVideos;
        }

        public List<YouTubeVideoModel> GetCachedVideos()
        {
            return _cachedVideos; // Return last searched videos
        }
    }
}
