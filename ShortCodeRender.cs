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
        public static void AddGlobalRenderer(string name, string value)
        {
            GlobalRenderers.Add(name, new StringShortCodeRender(value));
        }
        public static void AddGlobalRenderer(string name, Func<ShortCodeInfo, TaskOr<string>> value)
        {
            GlobalRenderers.Add(name, new FuncShortCodeRender(value));
        }
        public static void AddGlobalRenderer(string name, IShortCodeRender renderer)
        {
            GlobalRenderers.Add(name, renderer);
        }
        public static void ClearGlobalRenderers()
        {
            GlobalRenderers.Clear();
        }
        private readonly Dictionary<string, IShortCodeRender> _renderers = new Dictionary<string, IShortCodeRender>(StringComparer.OrdinalIgnoreCase);
        public void AddRenderer(string name, string value)
        {
            _renderers.Add(name, new StringShortCodeRender(value));
        }
        public void AddRenderer(string name, Func<ShortCodeInfo, TaskOr<string>> value)
        {
            _renderers.Add(name, new FuncShortCodeRender(value));
        }
        public void AddRenderer(string name, IShortCodeRender renderer)
        {
            _renderers.Add(name, renderer);
        }
        public void ClearRenderers()
        {
            _renderers.Clear();
        }
        private IShortCodeRender GetRendrer(string name)
        {
            if (_renderers.TryGetValue(name, out var renderer))
                return renderer;
            if (GlobalRenderers.TryGetValue(name, out renderer))
                return renderer;
            return null;
        }

        private static readonly Regex ShortCodePattern = new Regex(@"\[(\w+)([^\]]*)](?:(.*?)(\[/\1]))?", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex ShortCodeInnerAttrPattern = new Regex(@"\[(\w+)](.*?)\[/\1]", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex ShortCodeAttrPattern = new Regex(@"(\w+)\s*=\s*(?:(['""])(.*?)\2|([^\s]+))", RegexOptions.Compiled | RegexOptions.Singleline);


        private  IShortCodeRender Evulate(ref int lastIndex, StringBuilder sb, Match match, out ShortCodeInfo info)
        {
            info = null;
            string name = match.Groups[1].Value;
            if (string.IsNullOrEmpty(name))
            {
                lastIndex = match.Index + match.Length;
                sb.Append(match.Value);
                return null;
            }
            var renderer = GetRendrer(name);
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

        public string Render(string input)
        {
            if (string.IsNullOrEmpty(input) || (_renderers.Count == 0 && GlobalRenderers.Count == 0))
                return input;
            var matches = ShortCodePattern.Matches(input);
            var sb = new StringBuilder();
            int lastIndex = 0;
            foreach (Match match in matches)
            {
                sb.Append(input.Substring(lastIndex, match.Index - lastIndex));
                var renderer = Evulate(ref lastIndex, sb, match, out ShortCodeInfo info);  
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
        public async Task<string> RenderAsync(string input)
        {
            if (string.IsNullOrEmpty(input) || (_renderers.Count == 0 && GlobalRenderers.Count == 0))
                return input;
            var matches = ShortCodePattern.Matches(input);
            var sb = new StringBuilder();
            int lastIndex = 0;
            foreach (Match match in matches)
            {
                sb.Append(input.Substring(lastIndex, match.Index - lastIndex));
                var renderer = Evulate(ref lastIndex, sb, match, out ShortCodeInfo info);
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
