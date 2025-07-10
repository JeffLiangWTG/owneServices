using System;
using CargoWise.Common;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsCycleCountLocationInfo
	{
		public WhsCycleCountLocationInfo(Guid locationPK, string granularity, byte priority)
		{
			LocationPK = locationPK;
			Granularity = Argument.NotNullOrEmpty(granularity, nameof(granularity));
			Priority = priority;
		}

		public Guid LocationPK { get; }

		public string Granularity { get; }

		public byte Priority { get; }

		#region Equals & GetHashCode

		public override bool Equals(object obj)
			=> obj is WhsCycleCountLocationInfo pwa && pwa == this;

		public static bool operator ==(WhsCycleCountLocationInfo lhs, WhsCycleCountLocationInfo rhs) =>
			ReferenceEquals(lhs, rhs) ||
				(!(lhs is null) && !(rhs is null)
				&& lhs.LocationPK == rhs.LocationPK
				&& lhs.Granularity == rhs.Granularity
				&& lhs.Priority == rhs.Priority);

		public static bool operator !=(WhsCycleCountLocationInfo info1, WhsCycleCountLocationInfo info2)
			=> !(info1 == info2);

		public override int GetHashCode()
			=> LocationPK.GetHashCode()
				^ Granularity.GetHashCode()
				^ Priority.GetHashCode();

		#endregion
	}
}
