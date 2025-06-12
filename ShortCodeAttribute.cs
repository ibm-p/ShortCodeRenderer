using System;
using System.Collections.Generic;
using System.Text;

namespace ShortCodeRenderer
{
    public class ShortCodeAttribute
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool IsInnerAttribute { get; set; }
    }
    public class ShortCodeAttributes : Dictionary<string, ShortCodeAttribute>
    {
        public new  ShortCodeAttribute this[string name]
        {
            get => Get(name);
            set => base[name] = value;
        }
        public ShortCodeAttributes() : base(StringComparer.OrdinalIgnoreCase)
        {
        }
        public ShortCodeAttribute Get(string name) =>
            TryGetValue(name, out var attribute) ? attribute : null;
        public ShortCodeAttribute GetOrAdd(string name)
        {
            if (!TryGetValue(name, out var attribute))
            {
                attribute = new ShortCodeAttribute { Name = name };
                this[name] = attribute;
            }
            return attribute;
        }
    }
}
