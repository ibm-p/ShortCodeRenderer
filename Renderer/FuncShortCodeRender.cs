using ShortCodeRenderer.Task;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShortCodeRenderer.Renderer
{
    public class FuncShortCodeRender : IShortCodeRender
    {
        private Func<ShortCodeInfo, TaskOr<string>> _value;

        public ShortCodeOptions Options { get; set; } = new ShortCodeOptions();

        public FuncShortCodeRender(Func<ShortCodeInfo, TaskOr<string>> value)
        {
            _value = value;
        }
        public FuncShortCodeRender(Func<ShortCodeInfo, TaskOr<string>> value, ShortCodeOptions options)
        {
            _value = value;
            Options = options;
        }
        public TaskOr<string> Render(ShortCodeInfo info)
        {
            return _value(info);
        }
        public override string ToString()
        {
            return _value.ToString();
        }
    }
}
