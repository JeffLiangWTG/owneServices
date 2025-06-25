using System;
using System.Linq;
using System.Xml.Serialization;

namespace CargoWise.Billing.CollectorService.Plugin
{
#nullable disable
	[Serializable]
	public sealed class PluginSettings : IEquatable<PluginSettings>
	{
		public PluginSettings()
		{
		}

		public PluginSettings(PluginSettings settings)
		{
			if (settings.Parameters != null)
			{
				Parameters = settings.Parameters.Select(p => new PluginParameter(p)).ToArray();
			}
		}

		[XmlElement("Param")]
		public PluginParameter[] Parameters { get; set; }

		#region Equality

		public static bool Equals(PluginSettings first, PluginSettings second)
		{
			if (ReferenceEquals(first, second)) return true;
			if (ReferenceEquals(null, first)) return false;
			if (ReferenceEquals(null, second)) return false;
			if (ReferenceEquals(first.Parameters, second.Parameters)) return true;
			if (ReferenceEquals(null, first.Parameters)) return false;
			if (ReferenceEquals(null, second.Parameters)) return false;

			return first.Parameters.OrderBy(p => p.Name).SequenceEqual(second.Parameters.OrderBy(p => p.Name));
		}

		public bool Equals(PluginSettings other)
		{
			return Equals(this, other);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is PluginSettings && Equals((PluginSettings) obj);
		}

		public override int GetHashCode()
		{
			if (Parameters == null) return 0;
			unchecked
			{
				return Parameters.Aggregate(0, (hash, p) => (hash * 397) ^ (p != null ? p.GetHashCode() : 0));
			}
		}

		#endregion // Equality
	}
}
