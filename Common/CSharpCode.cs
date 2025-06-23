using ShortCodeRenderer.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShortCodeRenderer.Common
{
    public abstract class BaseCSharpCode
    {
        public abstract TaskOr<string> Render(ShortCodeContext context, ShortCodeInfo info);
    }

}
