using ShortCodeRenderer.Renderer;
using ShortCodeRenderer.Tasks;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ShortCodeRenderer
{

    public class ShortCodeContainer
    {
        internal readonly static Dictionary<string, IShortCodeRender> GlobalRenderers = new Dictionary<string, IShortCodeRender>(StringComparer.OrdinalIgnoreCase);
        public  bool Contains(string name, bool searchInGlobalRenders = true) => (searchInGlobalRenders && GlobalRenderers.ContainsKey(name)) || _renderers.ContainsKey(name);
        public static void AddGlobalRenderer(string name, string value)
        {
            GlobalRenderers[name] = new StringShortCodeRender(value);
        }
        public static void AddGlobalRenderer(string name, Func<ShortCodeInfo, TaskOr<string>> value)
        {
            GlobalRenderers[name] = new FuncShortCodeRender(value);
        }
        public static void AddGlobalRenderer(string name, IShortCodeRender renderer)
        {
            GlobalRenderers[name] = renderer;
        }
        public static void ClearGlobalRenderers()
        {
            GlobalRenderers.Clear();
        }
        internal readonly Dictionary<string, IShortCodeRender> _renderers = new Dictionary<string, IShortCodeRender>(StringComparer.OrdinalIgnoreCase);
        public void AddRenderer(string name, string value)
        {
            _renderers[name] = new StringShortCodeRender(value);
        }
        public void AddRenderer(string name, Func<ShortCodeInfo, TaskOr<string>> value)
        {
            _renderers[name] = new FuncShortCodeRender(value);
        }
        public void AddRenderer(string name, IShortCodeRender renderer)
        {
            _renderers[name] = renderer;
        }
        public void ClearRenderers()
        {
            _renderers.Clear();
        }
        internal IShortCodeRender GetRenderer(string name, Dictionary<string, IShortCodeRender> tempRenderers)
        {
            if (tempRenderers != null && tempRenderers.TryGetValue(name, out var renderer))
                return renderer;
            if (_renderers.TryGetValue(name, out renderer))
                return renderer;
            if (GlobalRenderers.TryGetValue(name, out renderer))
                return renderer;
            return null;
        }




       
    }
}
