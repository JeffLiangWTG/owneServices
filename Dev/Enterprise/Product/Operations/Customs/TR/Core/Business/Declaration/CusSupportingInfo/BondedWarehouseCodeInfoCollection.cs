using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class BondedWarehouseCodeInfoCollection : Customs.Business.CusSupportingInfoCollection<BondedWarehouseCodeInfo>
	{
		public BondedWarehouseCodeInfoCollection(BusinessObject parent) : base(parent, CusSupportingInfoTypeList.Codes.BondedWarehouse)
		{
		}
	}
}

