using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsExportToOpenCollection : Customs.Business.CusSupportingInfoCollection<NctsExportToOpen>
	{
		public NctsExportToOpenCollection(BusinessObject parent, ZString cSI_Type) : base(parent, cSI_Type)
		{
		}
	}
}
