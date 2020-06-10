using daemon_core;
using System;
using System.Collections.Generic;

namespace daemon_console
{
    public class GalleryApp : IGalleryApp
    {
        public string id { get; set; }
        public string displayName { get; set; }
        public string appId { get; set; }
        public string spId { get; set; }
        public IEnumerable<string> ReplyUrls { get; set ; }
        public IEnumerable<string> Identifier { get; set ; }
        public string PreferredSingleSignOnMode { get; set; }
    }
}