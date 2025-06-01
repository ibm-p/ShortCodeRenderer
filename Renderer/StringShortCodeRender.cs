using ShortCodeRenderer.Task;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShortCodeRenderer.Renderer
{
    public class StringShortCodeRender : IShortCodeRender
    {
        private TaskOr<string> _value;

        public ShortCodeOptions Options { get; set; } = new ShortCodeOptions();

        public StringShortCodeRender(TaskOr<string> value)
        {
            _value = value;
        }
        public StringShortCodeRender(TaskOr<string> value, ShortCodeOptions options)
        {
            _value = value;
            Options = options;
        }
        public TaskOr<string> Render(ShortCodeInfo info)
        {
            return _value;
        }
        public override string ToString()
        {
            return _value.ToString();
        }
    }
}
