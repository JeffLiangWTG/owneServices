using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgHeaderLookups : AutoOrgHeaderLookups
	{
		public OrgHeaderLookups(AutoOrgHeader parent)
			: base(parent)
		{
		}

		#region Constants

		public static class LookupConstants
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public static class DateFilterListConstants
			{
				public const string None = "NONE";
				public const string ClosedDate = "CLOSED DATE";
				public const string RecallDate = "RECALL DATE";
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string Contacts = "Contacts";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string AllocatedContact = "Allocated Contact";
		}

		#endregion

		#region Languages

		public CodeDescriptionPairList Languages
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Language); }
		}

		#endregion

		#region Categories

		public CodeDescriptionPairList Categories
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.OrgHeaderCategory); }
		}

		#endregion

		#region Locations

		public LocationCollection Locations
		{
			get { return new LocationCollection(Factory); }
		}

		#endregion

		#region Staff

		public GlbStaffCollection Staff
		{
			get
			{
				if (fStaff == null)
				{
					fStaff = new GlbStaffCollection(Factory);
				}

				return fStaff;
			}
		}

		GlbStaffCollection fStaff;

		#endregion

		#region DirectionList

		public ICodeDescriptionPairList DirectionList
		{
			get
			{
				if (fDirectionList == null)
				{
					fDirectionList = new CodeDescriptionPairList();
					fDirectionList.AddPair(ResString.GetMultilingualString("e8d01889-c6bc-4fc1-8606-8d10d111fbaa", "NONE"), ResString.GetMultilingualString("e8d01889-c6bc-4fc1-8606-8d10d111fbaa", "NONE"));
					fDirectionList.AddPair(ResString.GetMultilingualString("af099913-99fe-47f3-a7f2-93bbad2a6db3", "BOTH"), ResString.GetMultilingualString("af099913-99fe-47f3-a7f2-93bbad2a6db3", "BOTH"));
					fDirectionList.AddPair(ResString.GetMultilingualString("5d8875c8-4bf7-4e5a-a616-0143cce8e64d", "IMPORT"));
					fDirectionList.AddPair(ResString.GetMultilingualString("ac6cfa2b-0dae-41e7-850a-f1962e6cc153", "EXPORT"));
				}

				return fDirectionList;
			}
		}
		[ThreadStatic]
		static CodeDescriptionPairList fDirectionList;

		public CodeDescriptionPairList FilterFreightDirectionList
		{
			get
			{
				if (filterFreightDirectionList == null)
				{
					filterFreightDirectionList = new CodeDescriptionPairList();
					filterFreightDirectionList.AddPair(RelatedPartyDirectionList.Codes.Delivery, RelatedPartyDirectionList.Descriptions.Delivery);
					filterFreightDirectionList.AddPair(RelatedPartyDirectionList.Codes.Pickup, RelatedPartyDirectionList.Descriptions.Pickup);
					filterFreightDirectionList.AddPair(RelatedPartyDirectionList.Codes.PickupAndDelivery, RelatedPartyDirectionList.Descriptions.PickupAndDelivery);
				}
				return filterFreightDirectionList;
			}
		}
		CodeDescriptionPairList filterFreightDirectionList;

		public CodeDescriptionPairList FilterPartyTypeList
		{
			get
			{
				if (filterPartyTypeList == null)
				{
					filterPartyTypeList = new RelatedPartyTypeList();
				}
				return filterPartyTypeList;
			}
		}

		CodeDescriptionPairList filterPartyTypeList;

		#endregion

		#region DateFilterList

		public CodeDescriptionPairList DateFilterList
		{
			get
			{
				return Factory.GetCachedValue("Enterprise.MasterFiles.Business.OrgHeaderLookups.DateFilterList", () =>
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(ResString.GetMultilingualString("4b2cda5c-f3bb-491c-b054-c2f2b365d1ca", "NONE"));
						result.AddPair(ResString.GetMultilingualString("0e3343e4-15c1-43cd-ab45-b2e7e459e70b", "CLOSED DATE"));
						result.AddPair(ResString.GetMultilingualString("5a48719e-629f-4b84-9cda-de839987f80d", "RECALL DATE"));

						return result;
					});
			}
		}

		#endregion

		#region Opportunities

		public OrgOpportunityCollection Opportunities
		{
			get
			{
				if (fOpportunities == null)
				{
					fOpportunities = new OrgOpportunityCollection(Factory);
				}
				return fOpportunities;
			}
		}
		OrgOpportunityCollection fOpportunities;

		#endregion

		#region ScreeningStatusesList

		public CodeDescriptionPairList ScreeningStatusesList
		{
			get { return GetScreeningStatusesList(Factory); }
		}

		public static ScreeningStatusesList GetScreeningStatusesList(BusinessObjectFactory factory) => factory != null ? factory.GetCachedValue<ScreeningStatusesList>() : new ScreeningStatusesList();

		#endregion

		#region ContactsFilterOption

		public CodeDescriptionPairList ContactsFilterOptionList
		{
			get
			{
				if (fContactsFilterOptionList == null)
				{
					fContactsFilterOptionList = new CodeDescriptionPairList();
					fContactsFilterOptionList.AddPair(ResString.GetMultilingualString("3713F2B6-D55F-4E2E-9FB0-DF6D126D5A72", "Contacts"), ResString.GetMultilingualString("31E4ADF0-8C1C-4A1E-A021-EBF7BA11889D", "Search for all contacts"));
					fContactsFilterOptionList.AddPair(ResString.GetMultilingualString("56E8E305-6EEC-4997-AED3-B51081FD7A41", "Allocated Contact"), ResString.GetMultilingualString("9938CA86-1962-41D4-8F4F-5A4D7A7D872F", "Search for allocated contact"));
				}
				return fContactsFilterOptionList;
			}
		}
		CodeDescriptionPairList fContactsFilterOptionList;

		#endregion

		#region ShippingLines

		public override RefShippingLineCollection ShippingLines => new RefShippingLineCollection(Factory, Parent as OrgHeader);

		#endregion
	}
}
