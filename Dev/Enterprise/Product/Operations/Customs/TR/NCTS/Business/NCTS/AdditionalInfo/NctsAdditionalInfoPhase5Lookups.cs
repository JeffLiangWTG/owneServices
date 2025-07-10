using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsAdditionalInfoPhase5Lookups : EU.NCTS.Business.NctsAdditionalInfoPhase5Lookups, INctsAdditionalInfoLookups
	{
		public NctsAdditionalInfoPhase5Lookups(EU.NCTS.Business.NctsAdditionalInfo parent) : base(parent)
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
