using System;
using System.Collections.Generic;
using System.Text;

namespace ShortCodeRenderer
{
    public class ShortCodeInfo
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public bool IsClosed { get; set; }
        public ShortCodeAttributes Attributes { get; set; } = new ShortCodeAttributes();
    }
}
