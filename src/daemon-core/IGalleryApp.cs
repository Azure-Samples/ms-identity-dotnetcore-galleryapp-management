using System;
using System.Collections.Generic;
using System.Text;

namespace daemon_core
{
    public interface IGalleryApp
    {
        string id { get; set; }
        string displayName { get; set; }
    }
}
