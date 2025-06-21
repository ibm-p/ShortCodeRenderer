using ShortCodeRenderer.Renderer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ShortCodeRenderer
{
    public class ShortCodeContext
    {
        private static readonly Regex ShortCodePattern = new Regex(@"\[(\w+)([^\]]*)](?:(.*?)(\[/\1]))?", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex ShortCodeInnerAttrPattern = new Regex(@"\[(\w+)](.*?)\[/\1]", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex ShortCodeAttrPattern = new Regex(@"(\w+)\s*=\s*(?:(['""])(.*?)\2|([^\s]+))", RegexOptions.Compiled | RegexOptions.Singleline);
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        public Dictionary<string, object> Variables { get; internal set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        private ShortCodeContext()
        {

        }
        public ShortCodeContext Register<T>(T instance)
        {
            _services[typeof(T)] = instance;
            return this;
        }


        public T GetVariable<T>(string key)
        {
            if (Variables.TryGetValue(key, out var value) && value is T variable)
            {
                return variable;
            }
            return default;
        }
        public ShortCodeContext SetVariable<T>(string key, T value)
        {
            Variables[key] = value;
            return this;
        }
        public T Resolve<T>()
        {
            if (_services.TryGetValue(typeof(T), out var service))
                return (T)service;
            return default;
        }
        public bool IsRegistered<T>()
        {
            return _services.ContainsKey(typeof(T));
        }
        public ShortCodeContext Unregister<T>()
        {
            if (_services.ContainsKey(typeof(T)))
            {
                _services.Remove(typeof(T));
            }
            return this;
        }
        public void Clear()
        {
            _services.Clear();
        }
        private ShortCodeContainer _container;
        public static ShortCodeContext Create(ShortCodeContainer container)
        {
            return new ShortCodeContext()
            {
                _container = container
            };
        }
        public string Render(ShortCodeContext ctx, string code, Dictionary<string, IShortCodeRender> tempRenderers)
        {
            ShortCodeTokenizer tokenizer = new ShortCodeTokenizer();
            var results = tokenizer.Tokenize(ref code, true);
            //tokenizer.LinkTags(results);
            StringBuilder sb = new StringBuilder();
            ShortCodeTokenizeValue linker = null;
            StringBuilder sbLinked = new StringBuilder();
            foreach (var token in results)
            {
                if (linker != null)
                {
                    if (linker.Linked != token)
                    {
                        if (!token.InTag)
                            sbLinked.Append(token.Content);
                        else
                            sbLinked.Append(code.Substring(token.Index, token.Length));
                        continue;
                    }
                    //int start = linker.EndIndex + 1;
                    //int endIndex = token.Index;
                    //int length = endIndex - start;
                    //sb.Append(code.Substring(start, length));
                    var info = new ShortCodeInfo();
                    info.Name = linker.TagName;
                    info.Attributes = linker.Attributes;
                    info.Content = sbLinked.ToString();
                    var renderer = _container.GetRenderer(token.TagName, null);
                    if (renderer == null)
                    {
                        sb.Append(code.Substring(linker.Index, linker.Length));
                        sb.Append(sbLinked);
                        sb.Append(code.Substring(token.Index, token.Length));
                        continue;
                    }
                    info.Name = token.TagName;
                    info.Attributes = token.Attributes;
                    var r = renderer.Render(ctx, info);
                    if (r != null && !r.IsAsync() && r.Value != null)
                    {
                        sb.Append(r.Value);
                    }
                    linker = null;

                    continue;
                }
                if (token.InTag)
                {
                    var renderer =token.IsSlashUsed ? null : _container.GetRenderer(token.TagName, null);
                    if (token.Linked != null && renderer != null)
                    {
                        linker = token;
                        sbLinked.Clear();
                        continue;
                    }
                    if (renderer == null)
                    {
                        sb.Append(code.Substring(token.Index, token.Length));
                        continue;
                    }
                    if (token.IsSlashUsed && !token.Unclosed)
                        continue;
                    var info = new ShortCodeInfo();
                    info.Name = token.TagName;
                    info.Attributes = token.Attributes;
                    var r = renderer.Render(ctx, info);
                    if (r != null && !r.IsAsync() && r.Value != null)
                    {
                        sb.Append(r.Value);
                    }
                }
                else
                {
                    sb.Append(token.Content);
                }

            }
            return sb.ToString();

        }
        private IShortCodeRender Evulate(ref int lastIndex, StringBuilder sb, Match match, Dictionary<string, IShortCodeRender> tempRenderers, out ShortCodeInfo info)
        {
            info = null;
            string name = match.Groups[1].Value;
            if (string.IsNullOrEmpty(name))
            {
                lastIndex = match.Index + match.Length;
                sb.Append(match.Value);
                return null;
            }
            var renderer = _container.GetRenderer(name, tempRenderers);
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
        public string Render(ShortCodeInfo info) => Render(this, info, null);
        private string Render(ShortCodeContext ctx, ShortCodeInfo info) => Render(ctx, info, null);

        public string Render(ShortCodeInfo info, Dictionary<string, IShortCodeRender> tempRenderers) => Render(this, info, tempRenderers);

        private string Render(ShortCodeContext ctx, ShortCodeInfo info, Dictionary<string, IShortCodeRender> tempRenderers)
        {
            if (info == null || string.IsNullOrEmpty(info.Name) || ((tempRenderers == null || tempRenderers.Count == 0) && _container._renderers.Count == 0 && ShortCodeContainer.GlobalRenderers.Count == 0))
                return string.Empty;
            var renderer = _container.GetRenderer(info.Name, tempRenderers);
            if (renderer == null)
                return string.Empty;
            var r = renderer.Render(ctx, info);
            if (r != null && !r.IsAsync() && r.Value != null)
            {
                return r.Value as string;
            }
            return string.Empty;
        }
        public string Render(string input) => Render(this, input, null);

        public string Render(string input, Dictionary<string, IShortCodeRender> tempRenderers) => Render(this, input, tempRenderers);

        private string RenderRegex(ShortCodeContext ctx, string input, Dictionary<string, IShortCodeRender> tempRenderers)
        {
            if (string.IsNullOrEmpty(input) || ((tempRenderers == null || tempRenderers.Count == 0) && _container._renderers.Count == 0 && ShortCodeContainer.GlobalRenderers.Count == 0))
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
                var r = renderer.Render(ctx, info);
                if (r != null && !r.IsAsync() && r.Value != null)
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


        public Task<string> RenderAsync(ShortCodeInfo info) => RenderAsync(info, null);
        public Task<string> RenderAsync(ShortCodeInfo info, Dictionary<string, IShortCodeRender> tempRenderers) => RenderAsync(this, info, tempRenderers);

        private async Task<string> RenderAsync(ShortCodeContext ctx, ShortCodeInfo info, Dictionary<string, IShortCodeRender> tempRenderers)
        {
            if (info == null || string.IsNullOrEmpty(info.Name) || ((tempRenderers == null || tempRenderers.Count == 0) && _container._renderers.Count == 0 && ShortCodeContainer.GlobalRenderers.Count == 0))
                return string.Empty;
            var renderer = _container.GetRenderer(info.Name, tempRenderers);
            if (renderer == null)
                return string.Empty;
            var r = renderer.Render(ctx, info);
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
                    return (string)r.Value;
                }
            }
            return string.Empty;
        }
        public Task<string> RenderAsync(string input) => RenderAsync(this, input, null);

        private Task<string> RenderAsync(ShortCodeContext ctx, string input) => RenderAsync(input, null);
        public Task<string> RenderAsync(string input, Dictionary<string, IShortCodeRender> tempRenderers) => RenderAsync(this, input, tempRenderers);

        private async Task<string> RenderAsync(ShortCodeContext ctx, string input, Dictionary<string, IShortCodeRender> tempRenderers)
        {
            if (string.IsNullOrEmpty(input) || ((tempRenderers == null || tempRenderers.Count == 0) && _container._renderers.Count == 0 && ShortCodeContainer.GlobalRenderers.Count == 0))
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
                var r = renderer.Render(ctx, info);
                if (r != null)
                {
                    if (r.IsAsync())
                    {
                        string value = await r.AsTask();
                        if (value != null)
                            sb.Append(value);
                    }
                    else if (r.Value != null)
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
