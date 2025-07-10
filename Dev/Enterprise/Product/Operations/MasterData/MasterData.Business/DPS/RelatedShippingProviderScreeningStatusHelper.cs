using CargoWise.EntityFramework;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business
{
	public class RelatedShippingProviderScreeningStatusHelper : IRelatedShippingProviderScreeningStatusHelper
	{
		public IRelatedOrgPartyScreeningStatusCollection GetRelatedOrgPartyScreeningStatusCollection(BusinessObjectFactory factory, ScreeningParty[] screeningParties)
		{
			return RelatedOrgPartyScreeningStatusHelper.GetRelatedOrgPartyScreeningStatusCollection(factory, screeningParties);
		}
	}
}
