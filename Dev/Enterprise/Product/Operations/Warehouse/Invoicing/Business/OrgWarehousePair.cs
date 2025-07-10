using CargoWise.Types;

namespace Enterprise.Warehouse.Invoicing.Business
{
	public class OrgWarehousePair
	{
		public ZGuid OrgPK;
		public ZGuid WarehousePK;
		public ZBool HasInvoice;

		public override bool Equals(object obj) => obj != null && obj is OrgWarehousePair otherPair && otherPair == this;
		public static bool operator ==(OrgWarehousePair x, OrgWarehousePair y) => x.OrgPK == y.OrgPK && x.WarehousePK == y.WarehousePK;
		public static bool operator !=(OrgWarehousePair x, OrgWarehousePair y) => !(x == y);
		public override int GetHashCode() => OrgPK.GetHashCode() ^ WarehousePK.GetHashCode();
		public static OrgWarehousePair Create(ZGuid orgPK, ZGuid warehousePK)
		{
			return new OrgWarehousePair { OrgPK = orgPK, WarehousePK = warehousePK };
		}
	}
}
