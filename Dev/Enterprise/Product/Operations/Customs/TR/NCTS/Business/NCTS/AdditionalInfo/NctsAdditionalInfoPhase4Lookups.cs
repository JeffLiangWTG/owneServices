using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsAdditionalInfoPhase4Lookups : EU.NCTS.Business.NctsAdditionalInfoPhase4Lookups, INctsAdditionalInfoLookups
	{
		public NctsAdditionalInfoPhase4Lookups(NctsAdditionalInfo parent) : base(parent)
		{
		}

		protected new NctsAdditionalInfo Parent => (NctsAdditionalInfo)base.Parent;

		public ZZRefCusCodeListCombinedCollection AdditionalInfoCodesList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory
		, Core.Constants.CountryCodes.Turkey
		, AdditionalInfoCodesListType
		, ZDateTime.Today);

		const string AdditionalInfoCodesListType = "DC44A";
	}
}
