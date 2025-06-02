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
        public ShortCodeAttributes Attributes { get; set; }
        public ShortCodeInfo()
        {
            Attributes = new ShortCodeAttributes();
        }
        public ShortCodeInfo(string name) : this(name, string.Empty, new ShortCodeAttributes())
        {
        }
        public ShortCodeInfo(string name, string content) : this( name, content, new ShortCodeAttributes())
        {

        }
        public ShortCodeInfo(string name, string content, ShortCodeAttributes attributes) : this(name, content, attributes, false)
        {
        }
        public ShortCodeInfo(string name, string content, ShortCodeAttributes attributes, bool isClosed)
        {
                Name = name;
                Content = content;
                IsClosed = isClosed;
                Attributes = attributes;
        }
    }
}
