using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Hawking.eHub.Model.eHubTransactions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Unity;

namespace Hawking.SqlExport
{
    class Program
    {
        static void Main(string[] args)
        {
            var baseDir = Directory.GetCurrentDirectory();

            Unity.Config.ApplicationConfig.Initialise();
            Unity.DependencyFactory.Container.RegisterType<IeHubTransactionsContext, eHubTransactionsContext>();

            var serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Include;
            serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            serializer.PreserveReferencesHandling = PreserveReferencesHandling.None;
            serializer.Formatting = Formatting.Indented;

            var dbSetProperties = typeof(eHubTransactionsContext).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var configuration = Unity.DependencyFactory.Container.Resolve<IConfiguration>();
            foreach(var tableName in configuration.GetSection("Tables").GetChildren())
            {
                var ctx = Unity.DependencyFactory.Container.Resolve<IeHubTransactionsContext>();

                try
                {
                    Console.WriteLine(tableName.Value);
                    var property = dbSetProperties.FirstOrDefault(x => x.Name == tableName.Value);
                    if (property != null)
                    {
                        var dbset = property.GetValue(ctx) as IEnumerable;
                        if (dbset != null)
                        {
                            
                            var filePath = Path.Combine(baseDir, $"{tableName.Value}.json");
                            if (File.Exists(filePath))
                            {
                                File.Delete(filePath);
                            }

                            using (var streamWriter = new StreamWriter(filePath, true, Encoding.UTF8))
                            using (var jsonWriter = new JsonTextWriter(streamWriter))
                            {
                                streamWriter.WriteLine("[");

                                var enumerator = dbset.GetEnumerator();
                                if (enumerator.MoveNext())
                                {
                                    while(true)
                                    {
                                        var row = enumerator.Current;
                                        serializer.Serialize(jsonWriter, row);

                                        if (enumerator.MoveNext())
                                        {
                                            streamWriter.WriteLine(",");
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }

                                streamWriter.WriteLine("]");
                            }
                        }
                    }
                }
                finally
                {
                    ctx.Dispose();
                    ctx = null;
                }
            }
        }
    }
}
