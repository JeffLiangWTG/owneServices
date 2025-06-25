using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace CargoWise.Billing.CollectorService.Plugin
{
#nullable disable
	// The settings XML file is read on startup and reread whenever a change occurs to the file.
	[Serializable]
	[XmlRoot("Plugins")]
	public sealed class ServiceSettings : IEquatable<ServiceSettings>
	{
		public ServiceSettings()
		{
		}

		public ServiceSettings(ServiceSettings settings)
		{
			if (settings is { Plugins: not null })
			{
				Plugins = settings.Plugins.Select(p => new PluginControllerSettings(p)).ToArray();
			}
		}

		[XmlElement("Plugin")]
		public PluginControllerSettings[] Plugins { get; set; }

		public static ServiceSettings ReadFromFile(string filePath)
		{
			using (var stream = File.OpenRead(filePath))
			{
				var serializer = new XmlSerializer(typeof(ServiceSettings));
				var serviceSettings = (ServiceSettings)serializer.Deserialize(stream);
				if (serviceSettings.Plugins == null || serviceSettings.Plugins.Length == 0)
				{
					throw new FormatException($"No plugins found in the settings file: {filePath}");
				}
				return serviceSettings;
			}
		}

		#region Equality

		public static bool Equals(ServiceSettings first, ServiceSettings second)
		{
			if (ReferenceEquals(first, second)) return true;
			if (ReferenceEquals(null, first)) return false;
			if (ReferenceEquals(null, second)) return false;
			if (ReferenceEquals(first.Plugins, second.Plugins)) return true;
			if (ReferenceEquals(null, first.Plugins)) return false;
			if (ReferenceEquals(null, second.Plugins)) return false;

			return first.Plugins.OrderBy(p => p.TypeName).SequenceEqual(second.Plugins.OrderBy(p => p.TypeName));
		}

		public bool Equals(ServiceSettings other)
		{
			return Equals(this, other);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is ServiceSettings && Equals((ServiceSettings)obj);
		}

		public override int GetHashCode()
		{
			if (Plugins == null) return 0;
			unchecked
			{
				return Plugins.Aggregate(0, (hash, p) => (hash * 397) ^ (p != null ? p.GetHashCode() : 0));
			}
		}

		#endregion // Equality
	}
}
