namespace daemon_console
{
    public class GalleryApp
    {
        public string toString()
        {
            return id + " - " + displayName;
        }
        public string id { get; set; }
        public string displayName { get; set; }
    }
}