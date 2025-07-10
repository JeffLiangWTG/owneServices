using System;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemCycleCountLocationInfo
	{
		public WhsItemCycleCountLocationInfo(Guid locationPK, byte priority)
		{
			LocationPK = locationPK;
			Priority = priority;
		}

		public Guid LocationPK { get; }

		public byte Priority { get; }

		#region Equals & GetHashCode

		public override bool Equals(object obj)
			=> obj is WhsItemCycleCountLocationInfo pwa && pwa == this;

		public static bool operator ==(WhsItemCycleCountLocationInfo lhs, WhsItemCycleCountLocationInfo rhs) =>
			ReferenceEquals(lhs, rhs) ||
				(!(lhs is null) && !(rhs is null)
				&& lhs.LocationPK == rhs.LocationPK
				&& lhs.Priority == rhs.Priority);

		public static bool operator !=(WhsItemCycleCountLocationInfo info1, WhsItemCycleCountLocationInfo info2)
			=> !(info1 == info2);

		public override int GetHashCode()
			=> LocationPK.GetHashCode()
				^ Priority.GetHashCode();

		#endregion
	}
}
