using ShortCodeRenderer.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShortCodeRenderer.Renderer
{
    public interface IShortCodeCache
    {
        bool IsCached();
        bool CanCached();
        void Flush();
    }
    public interface IShortCodeRender
    {
        ShortCodeOptions Options { get; set; }
        TaskOr<string> Render(ShortCodeContext context, ShortCodeInfo info);
    }
    public abstract class ShortCodeRenderBase : IShortCodeRender
    {
        public string Source { get; set; }
        public ShortCodeOptions Options { get; set; } = new ShortCodeOptions();
        public abstract TaskOr<string> Render(ShortCodeContext context, ShortCodeInfo info);
        public override string ToString()
        {
            return GetType().Name;
        }
        
    }
}
