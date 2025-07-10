using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsWarehouseToOpenCollection : Customs.Business.CusSupportingInfoCollection<NctsWarehouseToOpen>
	{
		public NctsWarehouseToOpenCollection(BusinessObject parent, ZString cSI_Type) : base(parent, cSI_Type)
		{
		}
	}
}
