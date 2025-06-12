using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using OcmPoc.Mapping.Interface;

namespace OcmPoc.Mapping.Mapper
{
	class MappingProvider : IMappingProvider
	{
		readonly ConcurrentDictionary<string, IMapping> mappings;

		public MappingProvider()
		{
			mappings = new ConcurrentDictionary<string, IMapping>();
		}

		public IMapping GetMapping(string name)
		{
			//Console.WriteLine($"{DateTime.Now.Ticks}: Looking for mapping: {name}");

			if (!mappings.TryGetValue(name, out IMapping mapping))
			{
				var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				var assemblyPath = Path.Combine(path, $"OcmPoc.Mapping.{name}.dll");

				Console.WriteLine($"{DateTime.Now.Ticks}: Loading mapping assembly: {assemblyPath}");

				var assembly = Assembly.LoadFile(assemblyPath);
				var mappingType = assembly.GetTypes().First(t => typeof(IMapping).IsAssignableFrom(t));
				mapping = Activator.CreateInstance(mappingType) as IMapping;

				Console.WriteLine($"{DateTime.Now.Ticks}: Loaded mapping: {mapping.GetType().FullName}");
				mapping.Initialise();

				mappings.AddOrUpdate(name, mapping, (n,m) => mapping);
			}

			return mapping;
		}
    }
}
