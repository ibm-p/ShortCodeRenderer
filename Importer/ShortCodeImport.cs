using System;
using System.Collections.Generic;
using System.Text;

namespace ShortCodeRenderer.Importer
{
    public class ShortCodeImport
    {
        public List<ShortCodeImportItem> ShortCodes { get; set; } = new List<ShortCodeImportItem>();
    }
    public class ShortCodeImportItem
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Type { get; set; } // e.g. "string", "file"
    }
}
