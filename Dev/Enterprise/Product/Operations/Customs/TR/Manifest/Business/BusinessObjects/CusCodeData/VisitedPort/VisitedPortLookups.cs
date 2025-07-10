using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class VisitedPortLookups : CusCodeDataLookups
	{
		public VisitedPortLookups(CusCodeData parent) : base(parent)
		{
		}
		public ZZRefCusCodeListCombinedCollection VisitedPortList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today);
		public RefUNLOCOCollection UNLOCOCodeList => new RefUNLOCOCollection(Factory);
	}
}
