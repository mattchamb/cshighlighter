using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HighlighterLib.Templating
{
    public class ImageContent
    {
        public ImageContent(string name, byte[] contents)
        {
            Name = name;
            Contents = contents;
        }

        public byte[] Contents { get; private set; }

        public string Name { get; private set; }
    }
}