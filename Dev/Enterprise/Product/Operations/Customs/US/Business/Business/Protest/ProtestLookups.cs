using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Protest
{
	public class ProtestLookups : ZLookups
	{
		public ProtestLookups(Protest protest)
			: base(protest)
		{
		}

		Protest Protest
		{
			get { return (Protest)Parent; }
		}

		public TariffActCitationList TariffActCitations
		{
			get { return Factory.GetCachedValue<TariffActCitationList>(); }
		}

		public ProtestPeriodBaseDateQualifierList ProtestPeriodBaseDateQualifiers
		{
			get { return Factory.GetCachedValue<ProtestPeriodBaseDateQualifierList>(); }
		}

		public FurtherReviewAnswersList FurtherReviewAnswers
		{
			get { return Factory.GetCachedValue<FurtherReviewAnswersList>(); }
		}

		public ProtestantTypeList ProtestantTypes
		{
			get { return Factory.GetCachedValue<ProtestantTypeList>(); }
		}

		public ProtestRefundCOPartyTypeList ProtestRefundCOPartyTypes
		{
			get { return Factory.GetCachedValue<ProtestRefundCOPartyTypeList>(); }
		}

		public OrgHeaderCollection Organisations
		{
			get { return organisations ?? (organisations = new OrgHeaderCollection(Factory)); }
		}
		OrgHeaderCollection organisations;

		public ZZRefCusCodeListCombinedCollection SchDPortList
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		public ProtestCollection Associated514Protests
		{
			get { return GetCollectionWithDefaultFilters(Protest.US_P_Assoc514ProtestNo); }
		}

		public ProtestCollection Associated5204Protests
		{
			get { return GetCollectionWithDefaultFilters(Protest.US_P_Assoc520PetitionNo); }
		}

		public ProtestCollection LeadProtests
		{
			get { return GetCollectionWithDefaultFilters(Protest.US_P_LeadProtestNo); }
		}

		ProtestCollection GetCollectionWithDefaultFilters(ZString protestNumber)
		{
			var result = new ProtestCollection(Factory);
			var filter = new FilterBusinessObjectDefault(Enterprise.Customs.US.Business.Protest.Protest.Schema.ProtestNumber, "Property", protestNumber);
			result.FilterBusinessObjectDefaults.Add(filter);
			return result;
		}

		public GlbStaffCollection CusAgents
		{
			get { return Protest.Declaration.Lookups.CusAgents; }
		}

		public GlbBranchCollection Branches
		{
			get { return Protest.Declaration.Lookups.Branches; }
		}
	}
}
