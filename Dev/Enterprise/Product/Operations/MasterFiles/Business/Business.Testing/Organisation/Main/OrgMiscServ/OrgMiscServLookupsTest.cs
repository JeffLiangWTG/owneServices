using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgMiscServLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestEnablePromptToCreateProductsList

		public void TestEnablePromptToCreateProductsList()
		{
			var miscServ = Factory.New<OrgMiscServ>();

			AssertSame(Factory.GetCachedValue<EnablePromptToCreateProductsList>(), miscServ.Lookups.EnablePromptToCreateProductsList);
			AssertEquals("Default from Registry", miscServ.Lookups.EnablePromptToCreateProductsList["DEF"].Description);
			AssertEquals("Prompt to create products is disabled", miscServ.Lookups.EnablePromptToCreateProductsList["NO"].Description);
			AssertEquals("Prompt to create products is enabled", miscServ.Lookups.EnablePromptToCreateProductsList["YES"].Description);
		}

		#endregion

		#region TestABCAnalysisPeriodsList

		public void TestABCAnalysisPeriodsList()
		{
			var miscServ = Factory.New<OrgMiscServ>();

			AssertSame(Factory.GetCachedValue<WhsABCAnalysisPeriodCodeList>(), miscServ.Lookups.ABCAnalysisPeriodsList);
			AssertEquals("Default from Registry", miscServ.Lookups.ABCAnalysisPeriodsList["DEF"].Description);
		}

		#endregion

		#region TestABCAnalysisMethodsList

		public void TestABCAnalysisMethodsList()
		{
			var miscServ = Factory.New<OrgMiscServ>();

			AssertSame(Factory.GetCachedValue<WhsABCAnalysisMethodCodeList>(), miscServ.Lookups.ABCAnalysisMethodsList);
			AssertEquals("Default from Registry", miscServ.Lookups.ABCAnalysisMethodsList["DEF"].Description);
		}

		#endregion

		#region TestProductAuditActions

		public void TestProductAuditActions()
		{
			var miscServ = Factory.New<OrgMiscServ>();
			AssertSame(Factory.GetCachedValue<ProductAuditActions>(), miscServ.Lookups.ProductAuditActions);
		}

		#endregion

		public void TestConsigneeDefaultOptionList()
		{
			var actualList = Factory.New<OrgMiscServ>().Lookups.ConsigneeDefaultOptionList;
			AssertType<ConsigneeDefaultOptionList>(actualList);
		}

		public void TestOM_EXMergeCustomsInvoiceLinesBy_List()
		{
			var org = Factory.New<OrgHeader>();
			org.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			AssertEquals(OrgCodeLists.GetMergeInvoiceLinesByListByCountryCode(Core.Constants.CountryCodes.Australia).CodesAsString, org.MiscServ.Lookups.OM_EXMergeCustomsInvoiceLinesBy_List.CodesAsString);
		}

		public void TestInvoiceDetailReportSortList1()
		{
			InvoiceDetailGroupBy item = new InvoiceDetailGroupBy();
			item.Group1 = "PRD";
			WarehouseDataRegistry.Instance.InvoiceDetailGroupBy.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, item);
			CodeDescriptionPairList list = Factory.New<OrgMiscServ>().Lookups.InvoiceDetailReportSortList1;
			AssertInvoiceDetailReportSortList(list, "PRD");
		}

		public void TestInvoiceDetailReportSortList2()
		{
			InvoiceDetailGroupBy item = new InvoiceDetailGroupBy();
			item.Group2 = "PRD";
			WarehouseDataRegistry.Instance.InvoiceDetailGroupBy.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, item);
			CodeDescriptionPairList list = Factory.New<OrgMiscServ>().Lookups.InvoiceDetailReportSortList2;
			AssertInvoiceDetailReportSortList(list, "PRD");
		}

		public void TestInvoiceDetailReportSortList3()
		{
			InvoiceDetailGroupBy item = new InvoiceDetailGroupBy();
			item.Group2 = "CCO";
			item.Group3 = "PRD";
			WarehouseDataRegistry.Instance.InvoiceDetailGroupBy.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, item);
			CodeDescriptionPairList list = Factory.New<OrgMiscServ>().Lookups.InvoiceDetailReportSortList3;
			AssertInvoiceDetailReportSortList(list, "PRD");
		}

		public void TestCartonGroups()
		{
			AssertType(ObjectFactory.GetType<IWhsCartonGroupCollection>(), Factory.New<OrgMiscServ>().Lookups.CartonGroups);
		}

		public void TestAsAgentOptions()
		{
			var asAgentOptions = Factory.New<OrgMiscServ>().Lookups.AsAgentOptions;
			AssertContainsExactElementsInAnyOrder(new[]
			{
				("CAR", "AS CARRIER"),
				("AGT", "AS AGENT"),
				("ATC", "AS AGENT FOR CARRIER"),
				("ATF", "AS AGENT FOR"),
			}, asAgentOptions.ToArray().Select(u => (u.Code, u.Description)));
		}

		public void TestProductReceiveWeightOrDimsCheckTypeList()
		{
			var checkReceiveOfProductsWithoutWeightOrDimsOptions = Factory.New<OrgMiscServ>().Lookups.ProductReceiveWeightOrDimsCheckTypeList;
			AssertContainsExactElementsInAnyOrder(new[]
			{
				("NON", "Do Not Check Weight Or Dims"),
				("ALL", "Conversions And Stock Keeping Unit"),
				("SKU", "Stock Keeping Unit Only"),
			}, checkReceiveOfProductsWithoutWeightOrDimsOptions.ToArray().Select(u => (u.Code, u.Description)));
		}

		public void TestOrgSecurityGroups()
		{
			var groupA = Factory.NewWithValidTestData<GlbGroup>();
			groupA.GG_Desc = "groupA1";
			var groupB = Factory.NewWithValidTestData<GlbGroup>();
			var orgA = Factory.NewWithValidTestData<OrgHeader>();
			groupA.Organisation.Add(orgA);
			Factory.Save();

			var orgB = Factory.NewWithValidTestData<OrgHeader>();
			var groups = orgB.MiscServ.Lookups.OrgSecurityGroups;
			groups.Load();
			Assert(groups.Contains(groupA.PK));
			Assert(groups.Contains(groupB.PK));
			AssertEquals("groupA1", (groups as IFindBoxListProvider).DescriptionFromPrimaryKey(groupA.PK));
			AssertEquals("Unassigned", (groups as IFindBoxListProvider).DescriptionFromPrimaryKey(ZGuid.Empty));
		}

		void AssertInvoiceDetailReportSortList(CodeDescriptionPairList list, ZString defCode)
		{
			AssertEquals(list.Count, new InvoiceDetailReportSortList().Count + new JobChargeAttribTypeList().Count);
			AssertEquals(true, list.ContainsCode(InvoiceDetailReportSortList.Codes.ChargeCode));
			AssertEquals(true, list.ContainsCode(JobChargeAttribTypeList.Codes.DocketReference));
			AssertEquals(true, list.ContainsCode("DEF"));
			AssertEquals(false, list.ContainsCode(JobChargeAttribTypeList.Codes.DocketLinePK));
			AssertEquals("Default from Registry - " + list.GetDescriptionFromCode(defCode), list.GetDescriptionFromCode("DEF"));
		}
	}
}
