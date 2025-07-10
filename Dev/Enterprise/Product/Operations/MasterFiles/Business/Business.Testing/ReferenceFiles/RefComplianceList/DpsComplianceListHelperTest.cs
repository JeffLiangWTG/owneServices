using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class DpsComplianceListHelperTest : TestCaseWithFactory
	{
		public void TestGetExcludedComplianceListItems()
		{
			var complianceList1 = InitList("A1");
			var complianceList2 = InitList("A2", false);
			var complianceList3 = InitList("A3");

			complianceList1.RCL_IsExcluded = true;
			complianceList2.RCL_IsExcluded = true;
			complianceList3.RCL_IsExcluded = false;

			Factory.Save();

			var excludedList = DpsComplianceListHelper.GetExcludedComplianceListItems(Factory);
			AssertEquals(2, excludedList.Length);
			AssertItemInfo("A1", excludedList.First(u => u.Code == "A1"), false);
			AssertItemInfo("A2", excludedList.First(u => u.Code == "A2"), false);

			excludedList = DpsComplianceListHelper.GetExcludedComplianceListItems(null);
			AssertEquals("Still working when null factory sent", 2, excludedList.Length);
		}

		public void TestGetActiveAndInactiveComplianceListItems()
		{
			var complianceList1 = InitList("A1");
			var complianceList2 = InitList("A2", isExcluded: false);
			var complianceList3 = InitList("A3", isExcluded: true);
			var complianceList4 = InitList("A4", false);
			var complianceList5 = InitList("A5", false, true);

			Factory.Save();

			var complianceList = DpsComplianceListHelper.GetActiveIncludedAndExcludedComplianceListItems(Factory);
			var activeIncludedList = complianceList.ActiveIncludedList;
			var activeExcludedList = complianceList.ActiveExcludedList;

			CombineAssertions(() =>
			{
				AssertEquals(2, activeIncludedList.Length);
				AssertItemInfo("A1", activeIncludedList.First(u => u.Code == "A1"), false);
				AssertItemInfo("A2", activeIncludedList.First(u => u.Code == "A2"), false);
				AssertEquals(1, activeExcludedList.Length);
				AssertItemInfo("A3", activeExcludedList.First(u => u.Code == "A3"), false);
			});

			complianceList = DpsComplianceListHelper.GetActiveIncludedAndExcludedComplianceListItems(null);
			AssertEquals("Still working when null factory sent", 2, complianceList.ActiveIncludedList.Length);
			AssertEquals("Still working when null factory sent", 1, complianceList.ActiveExcludedList.Length);
		}

		public void TestGetComplianceListItems()
		{
			InitList("A1", false);
			InitList("A2");
			InitList("A3");

			Factory.Save();

			var listItems = DpsComplianceListHelper.GetComplianceListItems(Factory);

			AssertEquals(2, listItems.Length);
			AssertItemInfo("A2", listItems.First(u => u.Code == "A2"), false);
			AssertItemInfo("A3", listItems.First(u => u.Code == "A3"), false);

			listItems = DpsComplianceListHelper.GetComplianceListItems(null);
			AssertEquals("Still working when null factory sent", 2, listItems.Length);
		}

		public void TestHasExcludedList()
		{
			var list = InitList("A1");
			list.RCL_IsExcluded = true;
			Factory.Save();

			AssertEquals(true, DpsComplianceListHelper.HasExcludedList(Factory));
			AssertEquals("Still working when null factory sent", true, DpsComplianceListHelper.HasExcludedList(null));

			list.RCL_IsExcluded = false;
			Factory.Save();

			AssertEquals(false, DpsComplianceListHelper.HasExcludedList(Factory));
			AssertEquals("Still working when null factory sent", false, DpsComplianceListHelper.HasExcludedList(null));
		}

		public void TestIncludedAndExcludedListItems()
		{
			InitList("AAA");
			InitList("BBB");
			InitList("CCC");
			InitList("DDD", isExcluded: true);
			InitList("EEE", isExcluded: true);
			InitList("FFF", isExcluded: true);

			Factory.Save();

			var includedAndExcludedItems = DpsComplianceListHelper.GetComplianceListIncludedAndExcludedItems(Factory, new[] { "AAA", "CCC", "DDD", "EEE" });
			AssertContainsExactElementsInAnyOrder(new[] { "AAA", "CCC" }, includedAndExcludedItems.Included.Select(u => u.Code));
			AssertContainsExactElementsInAnyOrder(new[] { "DDD", "EEE" }, includedAndExcludedItems.Excluded.Select(u => u.Code));

			includedAndExcludedItems = DpsComplianceListHelper.GetComplianceListIncludedAndExcludedItems(Factory, new[] { "DDD" });
			AssertEquals(0, includedAndExcludedItems.Included.Length);
			AssertContainsExactElementsInAnyOrder(new[] { "DDD" }, includedAndExcludedItems.Excluded.Select(u => u.Code));
			AssertExceptionThrown<ArgumentNullException>(() => _ = DpsComplianceListHelper.GetComplianceListIncludedAndExcludedItems(Factory, null));
		}

		public void TestIncludedAndExcludedListItems_NotImportedListCodes()
		{
			InitList("AAA");
			InitList("BBB");
			InitList("CCC", isExcluded: true);
			InitList("DDD", isExcluded: true);

			Factory.Save();

			var includedAndExcludedItems = DpsComplianceListHelper.GetComplianceListIncludedAndExcludedItems(Factory, new[] { "AAA", "BBB", "CCC", "DDD", "EEE" });
			AssertContainsExactElementsInAnyOrder(new[] { "AAA", "BBB", "EEE" }, includedAndExcludedItems.Included.Select(u => u.Code));
			AssertContainsExactElementsInAnyOrder(new[] { "CCC", "DDD" }, includedAndExcludedItems.Excluded.Select(u => u.Code));

			includedAndExcludedItems = DpsComplianceListHelper.GetComplianceListIncludedAndExcludedItems(Factory, new[] { "EEE", "FFF", "FFF" });

			AssertContainsExactElementsInAnyOrder(new[] { ("EEE", "EEE"), ("FFF", "FFF") }, includedAndExcludedItems.Included.Select(u => (u.Code.ToString(), u.Name.ToString())));
			AssertEquals(0, includedAndExcludedItems.Excluded.Length);
		}

		RefComplianceList InitList(string code, bool isActive = true, bool? isExcluded = null)
		{
			var complianceList = Factory.New<RefComplianceList>();
			complianceList.RCL_ListCode = code;
			complianceList.RCL_ListType = code + " Type";
			complianceList.RCL_ListName = code + " Name";
			complianceList.RCL_ListDescription = code + " Description";
			complianceList.RCL_ListPublisher = code + " Publisher";
			complianceList.RCL_IsActive = isActive;
			complianceList.RCL_LastUpdatedDate = new ZDate(2021, 6, 30);

			if (isExcluded.HasValue)
			{
				complianceList.RCL_IsExcluded = isExcluded.Value;
			}

			return complianceList;
		}

		void AssertItemInfo(string code, DpsComplianceListItem deniedPartyScreeningComplianceListItem, bool hasContact)
		{
			AssertEquals(code, deniedPartyScreeningComplianceListItem.Code);
			AssertEquals(code + " Name", deniedPartyScreeningComplianceListItem.Name);
			AssertEquals(code + " Description", deniedPartyScreeningComplianceListItem.Description);
			AssertEquals(code + " Publisher", deniedPartyScreeningComplianceListItem.Publisher);
			AssertEquals(hasContact ? code + " Contact" : string.Empty, deniedPartyScreeningComplianceListItem.Contact);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestConnection.ExecuteNonQuery("delete from dbo.RefComplianceList"); //to fix "...very large number for a test. Change the test so it uses fewer objects" error
		}
	}
}
