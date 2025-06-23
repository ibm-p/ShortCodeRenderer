using Microsoft.CodeAnalysis;
using ShortCodeRenderer.Common;

//using Microsoft.CodeAnalysis.CSharp;
using ShortCodeRenderer.Renderer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ShortCodeRenderer.Utils
{
    public class CodeUtils
    {
        //public static List<MetadataReference> GetAllReferences()
        //{
        //    return AppDomain.CurrentDomain
        //        .GetAssemblies()
        //        .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
        //        .Distinct()
        //        .Select(a => MetadataReference.CreateFromFile(a.Location) as MetadataReference)
        //        .ToList();
        //}

        public static BaseCSharpCode AddReference(string filePath)
        {
            if(!File.Exists(filePath))
            {
                return null;
            }   
            byte[] dllBytes = File.ReadAllBytes(filePath);
            var assembly = Assembly.Load(dllBytes);


            var pluginType = assembly.GetTypes().FirstOrDefault(t => typeof(BaseCSharpCode).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            if (pluginType == null)
                return null;
            return Activator.CreateInstance(pluginType) as BaseCSharpCode;

        }
        //public static BaseCSharpCode CompileAndLoad(string code)
        //{
        //    var syntaxTree = CSharpSyntaxTree.ParseText(code);
        //    var references = new List<MetadataReference>
        //{
        //    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        //    MetadataReference.CreateFromFile(typeof(BaseCSharpCode).Assembly.Location), // Interface'in tanımlı olduğu yer
        //    MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
        //    MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
        //    MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
        //    MetadataReference.CreateFromFile(typeof(HttpClient).Assembly.Location),
        //};
        //    references = GetAllReferences();

        //    var compilation = CSharpCompilation.Create(
        //        "DynamicAsm",
        //        new[] { syntaxTree },
        //        references: references,
        //        options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        //    );

        //    using (var ms = new MemoryStream())
        //    {

        //        var result = compilation.Emit(ms);

        //        if (!result.Success)
        //        {
        //            var errors = string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        //            return null;
        //        }
        //        ms.Seek(0, SeekOrigin.Begin);
        //        var assembly = Assembly.Load(ms.ToArray());


        //        var pluginType = assembly.GetTypes().FirstOrDefault(t => typeof(BaseCSharpCode).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        //        if (pluginType == null)
        //            return null;
        //        return (BaseCSharpCode)Activator.CreateInstance(pluginType);

        //    }


        //}
   
    
    }
}
