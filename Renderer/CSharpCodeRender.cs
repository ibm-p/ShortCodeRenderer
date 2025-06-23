using ShortCodeRenderer.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ShortCodeRenderer.Renderer
{




    /*
    public class CSharpCodeRender : ShortCodeRenderBase, IShortCodeCache
    {
        private string _content;
        private bool _isFile = false;
        BaseCSharpCode csharpCode = null;
        public static string DefaultPath = "";
        public CSharpCodeRender(string content) : this(content, false)
        {
        }
        public CSharpCodeRender(string content, bool isFile = false) : this(content, isFile, null)
        {
        }
        public CSharpCodeRender(string content, ShortCodeOptions options) : this(content, false, options)
        {

        }
        public CSharpCodeRender(string content, bool isFile = false, ShortCodeOptions options = null)
        {
            if (isFile)
            {
                if (content.StartsWith("@"))
                    _content = Path.Combine(DefaultPath, content.Substring(1));
                else
                    _content = content;
            }
            else
            {
                _content = content;
            }
            Options = options;
        }

        public override TaskOr<string> Render(ShortCodeContext context, ShortCodeInfo info)
        {
            if(csharpCode == null || string.IsNullOrEmpty(_content))
            {
                return string.Empty;
            }
            if(csharpCode == null)
            {
                string content = "";
                if(_isFile)
                {
                    if (System.IO.File.Exists(_content))
                    {

                        using (FileStream fs = new FileStream(_content, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (StreamReader reader = new StreamReader(fs))
                        {
                            content = reader.ReadToEnd();
                        }
                    }
                }
                else
                {
                    content = _content;
                }
                csharpCode = CodeUtils.CompileAndLoad(content);
                if (csharpCode == null)
                    return string.Empty;
               
            }
            return csharpCode.Render(context, info);
        }
        public bool IsCached()
        {
            return csharpCode != null;
        }
        public bool CanCached()
        {
            return true;
        }
        public void Flush()
        {
            csharpCode = null;
        }
    }*/
}
