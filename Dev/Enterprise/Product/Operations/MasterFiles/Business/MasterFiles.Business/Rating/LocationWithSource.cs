using System;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class LocationWithSource : IEquatable<LocationWithSource>
	{
		LocationWithSource(ZGuid agentPK, ILocation location)
		{
			this.AgentPk = agentPK;
			this.Location = location;
		}

		public static LocationWithSource New(ZGuid agentPK, ILocation location)
		{
			if (!agentPK.IsValid || location == null)
			{
				return null;
			}

			return new LocationWithSource(agentPK, location);
		}

		public ZGuid AgentPk { get; }
		public ILocation Location { get; }

		public bool Equals(LocationWithSource other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}
			return AgentPk.Equals(other.AgentPk) && Equals(Location, other.Location);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != this.GetType())
			{
				return false;
			}
			return Equals((LocationWithSource)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (AgentPk.GetHashCode() * 397) ^ (Location != null ? Location.GetHashCode() : 0);
			}
		}
	}
}
