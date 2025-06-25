using System;
using System.Xml.Serialization;

namespace CargoWise.Billing.CollectorService.Plugin
{
#nullable disable
	[Serializable]
	public sealed class PluginParameter : IEquatable<PluginParameter>
	{
		public PluginParameter()
		{
		}

		public PluginParameter(string name, string value)
		{
			Name = name;
			Value = value;
		}

		public PluginParameter(PluginParameter copy)
			: this(copy.Name, copy.Value)
		{
		}

		[XmlAttribute]
		public string Name { get; set; }

		[XmlText]
		public string Value { get; set; }

		#region Equality
		
		public bool Equals(PluginParameter other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(Name, other.Name) && Equals(Value, other.Value);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is PluginParameter && Equals((PluginParameter) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Value != null ? Value.GetHashCode() : 0);
			}
		}

		#endregion // Equality
	}
}
