using ShortCodeRenderer.Task;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShortCodeRenderer.Renderer
{
    public interface IShortCodeRender
    {
        ShortCodeOptions Options { get; set; }
        TaskOr<string> Render(ShortCodeInfo info);
    }
}
