using System;
using System.Collections.Generic;
using System.Text;

namespace daemon_core
{
    public interface IGalleryApp
    {
        string id { get; set; }
        string displayName { get; set; }
        string appId { get; set; }
        string spId { get; set; }
        IEnumerable<string> ReplyUrls { get; set; }
        IEnumerable<string> Identifier { get; set; }
        string PreferredSingleSignOnMode { get; set; }
    }
}
