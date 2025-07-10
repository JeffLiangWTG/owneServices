using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.InBond.Business
{
	public class WarehouseDetailCollection : DependentCusAddInfoCollection<WarehouseDetail, CusInBondMoveDetail>
	{
		public WarehouseDetailCollection(CusInBondMoveDetail master)
			: base(master, CusAddInfoTypeAttribute.Codes.USWarehouseDetail)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers", Justification = "I want indexer by warehouseNumber")]
		public WarehouseDetail this[ZString warehouseNumber]
		{
			get
			{
				WarehouseDetail result = null;
				foreach (WarehouseDetail detail in this)
				{
					if (detail.US_WarehouseNumber == warehouseNumber)
					{
						result = detail;
						break;
					}
				}
				return result;
			}
		}
	}
}
