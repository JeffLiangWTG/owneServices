using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace CargoWise.Billing.CollectorService.Plugin
{
#nullable disable
	// The state XML file is read on startup and written to file whenever a change occurs to the objects.
	[Serializable]
	[XmlRoot("Plugins")]
	public sealed class ServiceState : IEquatable<ServiceState>
	{
		public ServiceState()
		{
			Plugins = new List<PluginControllerState>();
		}

		public ServiceState(ServiceState state)
		{
			if (state is { Plugins: not null })
			{
				Plugins = state.Plugins.Select(p => new PluginControllerState(p)).ToList();
			}
		}

		[XmlElement("Plugin")]
		public List<PluginControllerState> Plugins { get; set; }

		public void Reload(string filePath, object lockObject)
		{
			lock (lockObject)
			{
				foreach (PluginControllerState newState in ReadFromFile(filePath).Plugins)
				{
					var exists = false;
					foreach (PluginControllerState oldState in Plugins)
					{
						if (oldState.Key == newState.Key)
						{
							exists = true;
							oldState.LastSuccessfulRun = newState.LastSuccessfulRun;
							oldState.LastTransactionTimestamp = newState.LastTransactionTimestamp;
						}
					}
					if (!exists)
					{
						Plugins.Add(newState);
					}
				}
			}
		}

		public static ServiceState ReadFromFile(string filePath)
		{
			if (File.Exists(filePath))
			{
				using (var stream = File.OpenRead(filePath))
				{
					var serializer = new XmlSerializer(typeof(ServiceState));
					var serviceState = (ServiceState)serializer.Deserialize(stream);
					if (serviceState.Plugins == null || serviceState.Plugins.Count == 0)
					{
						throw new FormatException($"No plugins found in the state file: {filePath}");
					}
					return serviceState;
				}
			}
			else
			{
				return new ServiceState();
			}
		}

		public void WriteToFile(string filePath)
		{
			var serializer = new XmlSerializer(typeof(ServiceState));
			using var stream = File.Create(filePath);
			serializer.Serialize(stream, this);
		}

		#region Equality

		public static bool Equals(ServiceState first, ServiceState second)
		{
			if (ReferenceEquals(first, second)) return true;
			if (ReferenceEquals(null, first)) return false;
			if (ReferenceEquals(null, second)) return false;
			if (ReferenceEquals(first.Plugins, second.Plugins)) return true;
			if (ReferenceEquals(null, first.Plugins)) return false;
			if (ReferenceEquals(null, second.Plugins)) return false;

			return first.Plugins.OrderBy(p => p.Key).SequenceEqual(second.Plugins.OrderBy(p => p.Key));
		}

		public bool Equals(ServiceState other)
		{
			return Equals(this, other);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is ServiceState && Equals((ServiceState)obj);
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
