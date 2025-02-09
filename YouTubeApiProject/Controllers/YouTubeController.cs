using Microsoft.AspNetCore.Mvc;
using YouTubeApiProject.Services;
using YouTubeApiProject.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YouTubeApiProject.Controllers
{
    public class YouTubeController : Controller
    {
        private readonly YouTubeApiService _youtubeService;

        public YouTubeController(YouTubeApiService youtubeService)
        {
            _youtubeService = youtubeService;
        }

        // Show Empty Search Page
        public IActionResult Index()
        {
            return View(new Pagination<YouTubeVideoModel>(new List<YouTubeVideoModel>(), 0, 1, 3));
        }

        // Handle Search Query and Redirect to Paginated Results
        [HttpPost]
        public async Task<IActionResult> Search(string query, string uploadDate, string videoDuration)
        {
            await _youtubeService.SearchVideosAsync(query, uploadDate, videoDuration); // Fetch and store filtered videos
            return RedirectToAction("PaginatedIndex", new { page = 1 });
        }

        // Paginated Index
        public IActionResult PaginatedIndex(int page = 1)
        {
            var videos = _youtubeService.GetCachedVideos(); // ✅ Fetch filtered results

            if (videos == null || videos.Count == 0)
            {
                Console.WriteLine("[DEBUG] No videos found in _cachedVideos!"); // Debugging
                return View("Index", new Pagination<YouTubeVideoModel>(new List<YouTubeVideoModel>(), 0, page, 3));
            }

            int pageSize = 6;
            int totalItems = videos.Count;

            var items = videos.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var pagination = new Pagination<YouTubeVideoModel>(items, totalItems, page, pageSize);

            return View("Index", pagination); // ✅ Ensure we're passing the correct view model
        }
    }
}
