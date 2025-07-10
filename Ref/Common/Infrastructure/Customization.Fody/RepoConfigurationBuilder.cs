using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.Common.Customization.Fody
{
	public class RepoConfigurationBuilder : IConfigurationBuilder
	{
		public IDictionary<string, object> Properties { get; } = new Dictionary<string, object>();

		public IList<IConfigurationSource> Sources { get; } = new List<IConfigurationSource>();

		public IConfigurationBuilder Add(IConfigurationSource source)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			Sources.Add(source);
			return this;
		}

		public IConfigurationRoot Build()
		{
			var providers = new List<IConfigurationProvider>();
			foreach (IConfigurationSource source in Sources)
			{
				IConfigurationProvider provider = source.Build(this);
				providers.Add(provider);
			}
			providers.Add(RepoConfigProvider);
			return new ConfigurationRoot(providers);
		}

		IConfigurationProvider RepoConfigProvider
		{
			get
			{
				if (repoConfigProvider == null)
				{
					var applicationConfigAssemblyName = "CargoWise.RefDbRepo.Staging.ApplicationConfig";
					var configProviderClassName = "RepoConfigProvider";
					var assembly = Assembly.Load(applicationConfigAssemblyName);
					var type = assembly.GetType($"{applicationConfigAssemblyName}.{configProviderClassName}");
					repoConfigProvider = (IConfigurationProvider)Activator.CreateInstance(type);
				}
				return repoConfigProvider;
			}
		}

		IConfigurationProvider repoConfigProvider;
	}
}
