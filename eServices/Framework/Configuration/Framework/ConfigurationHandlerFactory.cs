using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using eServices.Configuration.Schemas;

namespace eServices.Configuration.Framework
{
	public class ConfigurationHandlerFactory
	{
		public static IConfigurationHandler GetConfigurationHandler(ConfigurationMessage configuration)
		{
			return GetConfigurationHandler(configuration.Name);
		}

		public static IConfigurationHandler GetConfigurationHandler(string name)
		{
			Type handlerType;
			if (HandlerTypes.Value.TryGetValue(name, out handlerType))
				return (IConfigurationHandler)Activator.CreateInstance(handlerType);
			else
				throw new NotImplementedException("Handler for configuration '" + name + "' is not implemented.");
		}

		static readonly Lazy<IDictionary<string, Type>> HandlerTypes = new Lazy<IDictionary<string, Type>>(LoadHandlerTypes);

		static IDictionary<string, Type> LoadHandlerTypes()
		{
#if NET48
			IEnumerable<Assembly> assemblies = System.Web.HttpContext.Current == null
				? AppDomain.CurrentDomain.GetAssemblies()
				: System.Web.Compilation.BuildManager.GetReferencedAssemblies().Cast<Assembly>();
#else
			IEnumerable<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies();
#endif
			var wtgAssemblies = assemblies.AsParallel()
				.Where(a =>
				{
					var company = a.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false).Cast<AssemblyCompanyAttribute>()
						.Select(p => p.Company).FirstOrDefault();
					return company != null && (company.StartsWith("CargoWise") || company.StartsWith("WiseTech"));
				});

			var handlerTypes = new ConcurrentDictionary<string, Type>();

			foreach (var assembly in assemblies.AsParallel())
			{
				try
				{
					foreach (var handler in assembly.GetTypes().AsParallel().Where(t => t.IsClass)
						.SelectMany(t => t.GetCustomAttributes(typeof(ConfigurationHandlerAttribute), false)
							.Cast<ConfigurationHandlerAttribute>()
							.Select(h => new KeyValuePair<string, Type>(h.Name, t))))
						handlerTypes[handler.Key] = handler.Value;
				}
				catch (ReflectionTypeLoadException)
				{
					// Ignoring unloadable types.
				}
			}

			return handlerTypes;
		}
	}
}
