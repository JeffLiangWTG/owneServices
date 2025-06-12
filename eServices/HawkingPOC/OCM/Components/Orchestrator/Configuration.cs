using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using OcmPoc.Utils.Config;

namespace OcmPoc.Components.Orchestrator
{
	public class Configuration
	{
		readonly IConfigurationRoot config;

		public Configuration(IConfigurationRoot config)
		{
			this.config = config;
		}

		public RabbitMqConfig RabbitMq { get; } = new RabbitMqConfig();
		public ExchangeConfig Exchange { get; } = new ExchangeConfig();

		public IEnumerable<string> Queues => GetSectionValues();
		public IEnumerable<string> Exchanges => GetSectionValues();
		public IEnumerable<string> Bindings => GetSectionValues();
		public IEnumerable<string> Providers => GetSectionValues();

		IEnumerable<string> GetSectionValues([CallerMemberName] string sectionPlural = null)
		{
			var sectionName = sectionPlural.TrimEnd('s');

			return config.GetSection(sectionName)
						 .AsEnumerable()
						 .OrderBy(kvp => kvp.Key)
						 .Where(kvp => kvp.Value != null)
						 .Select(kvp => kvp.Value);
		}

		public class ExchangeConfig
		{
			public string Recipient { get; set; }
		}

		public override string ToString()
		{
			return string.Join(Environment.NewLine,
				String.Join(";", Queues),
				String.Join(";", Exchanges),
				String.Join(";", Bindings),
				String.Join(";", Providers));
		}
	}
}
