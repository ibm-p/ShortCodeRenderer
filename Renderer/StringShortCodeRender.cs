using ShortCodeRenderer.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShortCodeRenderer.Renderer
{
    public class StringShortCodeRender : ShortCodeRenderBase, IShortCodeRender
    {
        private TaskOr<string> _value;
        public StringShortCodeRender(TaskOr<string> value)
        {
            _value = value;
        }
        public StringShortCodeRender(TaskOr<string> value, ShortCodeOptions options)
        {
            _value = value;
            Options = options;
        }
        public  override TaskOr<string> Render(ShortCodeContext context, ShortCodeInfo info)
        {
            return _value;
        }
        public override string ToString()
        {
            return _value.ToString();
        }
    }
}
