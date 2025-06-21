using ShortCodeRenderer.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShortCodeRenderer.Renderer
{
    public class FileShortCodeRender : ShortCodeRenderBase, IShortCodeCache
    {
        public static string DefaultPath = "";
        private string _filePath;
        private bool _cached = false;
        private string _cachedContent = null;
        private DateTime _cachedTime = DateTime.MinValue;
        public FileShortCodeRender(string filePath)
        {
            if (filePath.StartsWith("@"))
                _filePath = DefaultPath + filePath.Substring(1);
            else
                _filePath = filePath;
            _cached = true;
        }
        public FileShortCodeRender(string filePath, bool cached)
        {
            _filePath = filePath;
            _cached = cached;
        }
        public FileShortCodeRender(string filePath, bool cached, ShortCodeOptions options)
        {
            _filePath = filePath;
            Options = options;
            _cached = cached;
        }
        public override TaskOr<string> Render(ShortCodeContext context, ShortCodeInfo info)
        {
            // Eğer cache kullanılıyorsa ve önbellekteki içerik geçerliyse, önbellekten döner.
            if (_cached && _cachedContent != null && _cachedTime.AddMinutes(10) > DateTime.Now)
            {
                return _cachedContent;
            }

            if (System.IO.File.Exists(_filePath))
            {
                string content = System.IO.File.ReadAllText(_filePath);
                if (_cached)
                {
                    _cachedContent = content;
                    _cachedTime = DateTime.Now;
                }

                return content;
            }
            return string.Empty;
        }
        public override string ToString()
        {
            return _filePath.ToString();
        }

        public void Flush()
        {
            _cachedContent = null;
        }
        public bool IsCached()
        {
            return _cached && _cachedContent != null && _cachedTime.AddMinutes(10) > DateTime.Now;
        }
        public bool CanCached()
        {
            return _cached;
        }
    }
}
