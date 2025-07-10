using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class CusDecHouseBillLookups : Customs.Business.CusDecHouseBillLookups
	{
		public CusDecHouseBillLookups(Bill houseBill)
			: base(houseBill)
		{
		}

		public Bill HouseBill
		{
			get { return Parent; }
		}

		protected new Bill Parent
		{
			get { return (Bill)base.Parent; }
		}

		public CodeDescriptionPairList WeightUQList
		{
			get
			{
				return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);
			}
		}

		public CodeDescriptionPairList VolumeUQList
		{
			get
			{
				return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume);
			}
		}

		public override CodeDescriptionPairList NoOfPacksPackType_List
		{
			get { return Factory.GetCachedValue<ShippingOrPackingingUnitList>(); }
		}

		public OrgHeaderCollection HouseIssuers
		{
			get { return houseIssuers ?? (houseIssuers = new BillIssuerOrganisationFindBoxCollection(Factory)); }
		}
		OrgHeaderCollection houseIssuers;

		public OrgHeaderCollection MasterIssuers
		{
			get { return masterIssuers ?? (masterIssuers = new BillIssuerOrganisationFindBoxCollection(Factory)); }
		}
		OrgHeaderCollection masterIssuers;

		public OrganisationsFindBoxCollection ForeignShippers
		{
			get { return foreignShippers ?? (foreignShippers = new BillIssuerOrganisationFindBoxCollection(Factory)); }
		}
		OrganisationsFindBoxCollection foreignShippers;

		public OrganisationsFindBoxCollection NotifyParties
		{
			get { return notifyParties ?? (notifyParties = new OrganisationsFindBoxCollection(Factory)); }
		}
		OrganisationsFindBoxCollection notifyParties;

		public USCarrierCombinedCollection USCarrierList
		{
			get { return usCarrierList ?? (usCarrierList = new USCarrierCombinedCollection(Factory)); }
		}
		USCarrierCombinedCollection usCarrierList;

		public CodeDescriptionPairList YesNoList
		{
			get
			{
				return Factory.GetCachedValue("YesNoList",
				delegate
				{
					CodeDescriptionPairList result = new YesNoDefaultList();
					result.RemoveCode(YesNoDefaultList.Codes.Default);
					return result;
				});
			}
		}

		public override CodeDescriptionPairList MessageStatusList
		{
			get { return Factory.GetCachedValue<ImportMessageStatusList>(); }
		}
	}
}
