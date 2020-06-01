using daemon_core;
using System;

namespace daemon_console
{
    public class GalleryApp : IGalleryApp
    {
        public string toString()
        {
            return id + " - " + displayName;
        }
        public string id { get; set; }
        public string displayName { get; set; }
    }
}