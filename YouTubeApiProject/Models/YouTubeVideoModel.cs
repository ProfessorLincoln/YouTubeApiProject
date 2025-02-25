﻿namespace YouTubeApiProject.Models
{
    public class YouTubeVideoModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public string VideoUrl { get; set; }
        public string PublishedDate { get; set; }

        // 🔹 New Properties for Channel Info
        public string ChannelName { get; set; }
        public string ChannelProfileUrl { get; set; }
    }
}
