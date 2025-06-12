using System;
using System.IO;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace XT.Internal.API.tasks
{
    public class ResolveAssemblies : Task
	{
        public override bool Execute()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            Log.LogMessage(MessageImportance.High, $"Assembly Resolve Hook was created for {SearchDirectory}");
            return true;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = $"{new AssemblyName(args.Name).Name}.dll";
            if (File.Exists(Path.Combine(SearchDirectory, name)))
            {
                return Assembly.LoadFrom(Path.Combine(SearchDirectory, name));
            }
            return null;
        }

        [Required]
        public string SearchDirectory { get; set; }
    }
}
