using System;
using System.Collections.Generic;
using System.Text;

namespace ShortCodeRenderer
{
    public class ShortCodeOptions
    {
        /// <summary>
        /// Allow render inner attributes in shortcodes. (e.g] [code][attr1][/attr1][/code])
        /// </summary>
        public bool RenderInnerAttributes { get; set; }
    }
}
