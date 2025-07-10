using CargoWise.EntityFramework;
using Enterprise.MasterData.Common;

namespace Enterprise.MasterFiles.Business
{
	public interface IRelatedShippingProviderScreeningStatusHelper
	{
		IRelatedOrgPartyScreeningStatusCollection GetRelatedOrgPartyScreeningStatusCollection(BusinessObjectFactory factory, ScreeningParty[] screeningParties);
	}
}
