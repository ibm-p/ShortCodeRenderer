using ShortCodeRenderer.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShortCodeRenderer.Renderer
{
    public class FuncShortCodeRender : ShortCodeRenderBase, IShortCodeRender
    {
        private Func<ShortCodeInfo, TaskOr<string>> _value;

        public FuncShortCodeRender(Func<ShortCodeInfo, TaskOr<string>> value)
        {
            _value = value;
        }
        public FuncShortCodeRender(Func<ShortCodeInfo, TaskOr<string>> value, ShortCodeOptions options)
        {
            _value = value;
            Options = options;
        }
        public override TaskOr<string> Render(ShortCodeContext context, ShortCodeInfo info)
        {
            return _value(info);
        }
        public override string ToString()
        {
            return _value.ToString();
        }
    }
}
