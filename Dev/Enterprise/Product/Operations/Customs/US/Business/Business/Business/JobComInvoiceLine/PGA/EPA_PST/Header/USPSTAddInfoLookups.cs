using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USPSTAddInfoLookups : AutoUSPSTAddInfoLookups
	{
		public USPSTAddInfoLookups(AutoUSPSTAddInfo parent)
			: base(parent)
		{
		}

		public PSTIntendedUseCodesList IntendedUseCodeList
		{
			get { return Factory.GetCachedValue<PSTIntendedUseCodesList>(); }
		}

		public PSTProductTypeList ProductTypeList
		{
			get { return Factory.GetCachedValue<PSTProductTypeList>(); }
		}

		public PSTRemarksCodeList ReasonCodeList
		{
			get { return Factory.GetCachedValue<PSTRemarksCodeList>(); }
		}

		public OrganisationsFindBoxCollection Organizations
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		public ShippingOrPackingingUnitList PackingTypes
		{
			get { return Factory.GetCachedValue<ShippingOrPackingingUnitList>(); }
		}
		public FDAUQList OuterPackingTypes
		{
			get { return Factory.GetCachedValue<FDAUQList>(); }
		}

		public ACE_FDABaseUQList InnerPackingTypes
		{
			get { return Factory.GetCachedValue<ACE_FDABaseUQList>(); }
		}

		public CodeDescriptionPairList PSTCertifyingIndividualList
		{
			get { return PartyTypeList.GetListForPSTCertifyingIndividual(Factory); }
		}

		public CodeDescriptionPairList NotifyPartyList
		{
			get { return PartyTypeList.GetListForPSTNotifyPartyList(Factory); }
		}

		public CodeDescriptionPairList PSTWeightUQList
		{
			get { return Factory.GetCachedValue("GetPSTWeightUQList", GetPSTWeightUQList); }
		}

		CodeDescriptionPairList GetPSTWeightUQList()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.CustomType);
			list.AddPair(Core.Constants.Weight.Grams, "Grams");
			list.AddPair(Core.Constants.Weight.Kilograms, "Kilograms");
			list.AddPair(Core.Constants.Weight.Milligrams, "Milligrams");
			list.AddPair(Core.Constants.Weight.Ounces, "Ounces");
			list.AddPair("OTL", "Quarts");
			list.AddPair("ML", "Milliliters");
			list.AddPair("GAL", "Gallons");

			return list;
		}
	}
}
