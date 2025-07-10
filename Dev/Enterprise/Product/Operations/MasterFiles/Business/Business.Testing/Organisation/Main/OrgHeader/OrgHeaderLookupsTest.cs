using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCategories()
		{
			Assert("Categories returned", Lookups.Categories.Count > 0);
		}
		public void TestLanguages()
		{
			Assert("Languages returned", Lookups.Languages.Count > 0);
		}

		public void TestStaff()
		{
			Assert("Staff returned", Lookups.Staff.Count > 0);
		}

		public void TestDirectionList()
		{
			ICodeDescriptionPairList list = Lookups.DirectionList;
			Assert(list.ContainsCode("None"));
			Assert(list.ContainsCode("Both"));
			Assert(list.ContainsCode("Import"));
			Assert(list.ContainsCode("Export"));
		}

		public void TestDateFilterList()
		{
			CodeDescriptionPairList list = Lookups.DateFilterList;
			Assert(list.ContainsCode(OrgHeaderLookups.LookupConstants.DateFilterListConstants.None));
			Assert(list.ContainsCode(OrgHeaderLookups.LookupConstants.DateFilterListConstants.ClosedDate));
			Assert(list.ContainsCode(OrgHeaderLookups.LookupConstants.DateFilterListConstants.RecallDate));
		}

		public void TestScreeningStatusesList()
		{
			CodeDescriptionPairList list = Lookups.ScreeningStatusesList;
			AssertEquals(typeof(ScreeningStatusesList), list.GetType());
			Assert(list.ContainsCode(ScreeningStatusesList.Codes.NotScreened));
		}

		public void TestContactsFilterOptionList()
		{
			var list = Lookups.ContactsFilterOptionList;
			Assert(list.ContainsCode(OrgHeaderLookups.LookupConstants.Contacts));
			AssertEquals("Search for all contacts", list.GetDescriptionFromCode(OrgHeaderLookups.LookupConstants.Contacts));
			Assert(list.ContainsCode(OrgHeaderLookups.LookupConstants.AllocatedContact));
			AssertEquals("Search for allocated contact", list.GetDescriptionFromCode(OrgHeaderLookups.LookupConstants.AllocatedContact));
		}

		#region Implementation

		OrgHeaderLookups Lookups;

		protected override void SetUp()
		{
			base.SetUp();
			Lookups = new OrgHeaderLookups(OrgHeader.New(Factory));
		}

		#endregion
	}
}
