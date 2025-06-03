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

    public class ShortCodeRender
    {
        private readonly static Dictionary<string, IShortCodeRender> GlobalRenderers = new Dictionary<string, IShortCodeRender>(StringComparer.OrdinalIgnoreCase);
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
        private readonly Dictionary<string, IShortCodeRender> _renderers = new Dictionary<string, IShortCodeRender>(StringComparer.OrdinalIgnoreCase);
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
        private IShortCodeRender GetRenderer(string name, Dictionary<string, IShortCodeRender> tempRenderers)
        {
            if (tempRenderers != null && tempRenderers.TryGetValue(name, out var renderer))
                return renderer;
            if (_renderers.TryGetValue(name, out renderer))
                return renderer;
            if (GlobalRenderers.TryGetValue(name, out renderer))
                return renderer;
            return null;
        }

        private static readonly Regex ShortCodePattern = new Regex(@"\[(\w+)([^\]]*)](?:(.*?)(\[/\1]))?", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex ShortCodeInnerAttrPattern = new Regex(@"\[(\w+)](.*?)\[/\1]", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex ShortCodeAttrPattern = new Regex(@"(\w+)\s*=\s*(?:(['""])(.*?)\2|([^\s]+))", RegexOptions.Compiled | RegexOptions.Singleline);


        private  IShortCodeRender Evulate(ref int lastIndex, StringBuilder sb, Match match, Dictionary<string, IShortCodeRender> tempRenderers, out ShortCodeInfo info)
        {
            info = null;
            string name = match.Groups[1].Value;
            if (string.IsNullOrEmpty(name))
            {
                lastIndex = match.Index + match.Length;
                sb.Append(match.Value);
                return null;
            }
            var renderer = GetRenderer(name, tempRenderers);
            if (renderer == null)
            {
                lastIndex = match.Index + match.Length;
                sb.Append(match.Value);
                return null;
            }
            info = new ShortCodeInfo();
            info.Name = name;

            string attrRaw = match.Groups[2].Value;
            if (!string.IsNullOrEmpty(attrRaw))
            {
                foreach (Match matchAttr in ShortCodeAttrPattern.Matches(attrRaw))
                {
                    string key = matchAttr.Groups[1].Value;
                    if (string.IsNullOrEmpty(key))
                        continue;
                    string value = matchAttr.Groups[3].Success ? matchAttr.Groups[3].Value : matchAttr.Groups[4].Value;
                    info.Attributes.Add(key, new ShortCodeAttribute() { IsInnerAttribute = false, Name = key, Value = value });
                }
            }
            string content = match.Groups[3].Value;
            if (renderer.Options.RenderInnerAttributes)
            {
                StringBuilder content2 = new StringBuilder();
                int _lastIndex = 0;
                foreach (Match matchAttr in ShortCodeInnerAttrPattern.Matches(content))
                {
                    content2.Append(content.Substring(_lastIndex, matchAttr.Index - _lastIndex));
                    var key = matchAttr.Groups[1].Value;
                    if (string.IsNullOrEmpty(key))
                    {
                        _lastIndex = matchAttr.Index + matchAttr.Length;
                        continue;
                    }
                    var value = matchAttr.Groups[2].Value;
                    info.Attributes.Add(key, new ShortCodeAttribute() { IsInnerAttribute = true, Name = key, Value = value });
                    _lastIndex = matchAttr.Index + matchAttr.Length;
                }
                if (_lastIndex < content.Length)
                {
                    content2.Append(content.Substring(_lastIndex));
                }
                info.Content = content2.ToString();
            }
            else
            {
                info.Content = content;

            }
            info.IsClosed = match.Groups[4].Success;
            return renderer;
        }
        public string Render(ShortCodeContext ctx, ShortCodeInfo info) => Render(ctx, info, null);

        public string Render(ShortCodeContext ctx, ShortCodeInfo info, Dictionary<string, IShortCodeRender> tempRenderers)
        {
            if (info  == null || string.IsNullOrEmpty(info.Name) || ((tempRenderers == null || tempRenderers.Count == 0) && _renderers.Count == 0 && GlobalRenderers.Count == 0))
                return string.Empty;
            var renderer = GetRenderer(info.Name, tempRenderers);
            if (renderer == null)
                return string.Empty;
            var r = renderer.Render(info);
            if (r != null && !r.IsAsync() && r.Value != null)
            {
                return r.Value as string;
            }
            return string.Empty;
        }
        public string Render(ShortCodeContext ctx, string input) => Render(ctx, input, null);

        public string Render(ShortCodeContext ctx, string input, Dictionary<string, IShortCodeRender> tempRenderers)
        {
            if (string.IsNullOrEmpty(input) || ((tempRenderers == null || tempRenderers.Count == 0) && _renderers.Count == 0 && GlobalRenderers.Count == 0))
                return input;
            var matches = ShortCodePattern.Matches(input);
            var sb = new StringBuilder();
            int lastIndex = 0;
            foreach (Match match in matches)
            {
                sb.Append(input.Substring(lastIndex, match.Index - lastIndex));
                var renderer = Evulate(ref lastIndex, sb, match, tempRenderers, out ShortCodeInfo info);  
                if(renderer == null)
                    continue; 
                var r = renderer.Render(info);
                if(r != null && !r.IsAsync() && r.Value != null)
                {
                    sb.Append(r.Value);
                }
                lastIndex = match.Index + match.Length;
            }
            if (lastIndex < input.Length)
            {
                sb.Append(input.Substring(lastIndex));
            }
            return sb.ToString();
        }
        public Task<string> RenderAsync(ShortCodeContext ctx, ShortCodeInfo info) => RenderAsync(info, null);

        public async Task<string> RenderAsync(ShortCodeInfo info, Dictionary<string, IShortCodeRender> tempRenderers)
        {
            if (info == null || string.IsNullOrEmpty(info.Name) || ((tempRenderers == null || tempRenderers.Count == 0) && _renderers.Count == 0 && GlobalRenderers.Count == 0))
                return string.Empty;
            var renderer = GetRenderer(info.Name, tempRenderers);
            if (renderer == null)
                return string.Empty;
            var r = renderer.Render(info);
            if (r != null)
            {
                if (r.IsAsync())
                {
                    string value = await r.AsTask();
                    if (value != null)
                       return value;
                }
                else if (r.Value != null)
                {
                    return (string) r.Value;
                }
            }
            return string.Empty;
        }

        public  Task<string> RenderAsync(ShortCodeContext ctx, string input) =>  RenderAsync(input, null);
        public async Task<string> RenderAsync(string input, Dictionary<string, IShortCodeRender> tempRenderers)
        {
            if (string.IsNullOrEmpty(input) || ((tempRenderers == null || tempRenderers.Count == 0) && _renderers.Count == 0 && GlobalRenderers.Count == 0))
                return input;
            var matches = ShortCodePattern.Matches(input);
            var sb = new StringBuilder();
            int lastIndex = 0;
            foreach (Match match in matches)
            {
                sb.Append(input.Substring(lastIndex, match.Index - lastIndex));
                var renderer = Evulate(ref lastIndex, sb, match, tempRenderers, out ShortCodeInfo info);
                if (renderer == null)
                    continue;
                var r = renderer.Render(info);
                if(r != null)
                {
                    if(r.IsAsync())
                    {
                        string value = await r.AsTask();
                        if(value != null)
                            sb.Append(value);
                    }
                    else if(r.Value != null)    
                    {
                        sb.Append(r.Value); 
                    }
                }
                lastIndex = match.Index + match.Length;
            }
            if (lastIndex < input.Length)
            {
                sb.Append(input.Substring(lastIndex));
            }
            return sb.ToString();
        }
    }
}
