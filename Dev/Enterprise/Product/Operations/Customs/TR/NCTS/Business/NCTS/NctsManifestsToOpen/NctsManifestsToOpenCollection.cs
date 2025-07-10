using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsManifestsToOpenCollection : Customs.Business.CusSupportingInfoCollection<NctsManifestsToOpen>
	{
		public NctsManifestsToOpenCollection(BusinessObject parent, ZString cSI_Type) : base(parent, cSI_Type)
		{
		}
	}
}
