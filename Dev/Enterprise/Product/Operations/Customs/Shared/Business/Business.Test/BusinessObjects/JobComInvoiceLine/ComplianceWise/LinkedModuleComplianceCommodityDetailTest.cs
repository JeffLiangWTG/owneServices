using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(LinkedModuleComplianceCommodityDetail))]
	public class LinkedModuleComplianceCommodityDetailTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var invoiceLine = CreateDeclarationWithInvoiceLine();
			return invoiceLine.LinkedModuleCommodity;
		}

		public void TestLinkedModuleComplianceCommodityDetailInit()
		{
			var invoiceLine = CreateDeclarationWithInvoiceLine();
			var linkedModuleComplianceCommodityDetail = new LinkedModuleComplianceCommodityDetail(invoiceLine, invoiceLine.Declaration);

			AssertEquals(invoiceLine, linkedModuleComplianceCommodityDetail.Parent);
			AssertEquals(invoiceLine.Declaration, linkedModuleComplianceCommodityDetail.CommodityRiskStatusProvider);
		}

		public void TestInitializeIfNeeded()
		{
			var helper = new DummyInteractionWithComplianceWiseCommoditiesHelper();
			var invoiceLine = CreateDeclarationWithInvoiceLine(helper);

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityInvoiceLineMocksForTest(true))
			{
				invoiceLine.LinkedModuleCommodity.InitializeIfNeeded();

				AssertEquals("CLR", invoiceLine.LinkedModuleCommodity.RiskStatus);
				AssertEquals("Clear", invoiceLine.LinkedModuleCommodity.RiskStatusDescription);
				AssertEquals(false, invoiceLine.LinkedModuleCommodity.RiskStatusDescription_ReadOnly);
				AssertEquals(false, invoiceLine.LinkedModuleCommodity.AssessmentNotes_ReadOnly);
				AssertEquals(true, invoiceLine.LinkedModuleCommodity.CommodityExists);
				AssertEquals("Test Notes", invoiceLine.LinkedModuleCommodity.AssessmentNotes);
				AssertEquals("Test Harmonized Border Wise Textual", invoiceLine.LinkedModuleCommodity.HarmonizedBorderWiseTextual);
				AssertEquals("Clear", invoiceLine.LinkedModuleCommodity.ImportAlertStatus);
			}
		}

		public void TestBatchInitializeAndSetCommodityInfo()
		{
			var helper = new DummyInteractionWithComplianceWiseCommoditiesHelper();
			var invoiceLine = CreateDeclarationWithInvoiceLine(helper);

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityInvoiceLineMocksForTest(true))
			{
				var complianceResult = new ComplianceResultFromCpw
				{
					HarmonizedCode = "520620",
					GroupingOrCountry = "WCO",
					GoodsDescription = "",
					OriginOfGoods = "US",
					LinkVisible = true,
					HarmonizedBorderWiseTextual = "Test Harmonized Border Wise Textual: TestBatchInitialize",
					RiskStatus = "BLK",
					RiskNotes = "Test Notes1",
					AssessmentInitialized = true,
					ImportAlertStatus = "High Risk"
				};

				invoiceLine.LinkedModuleCommodity.BatchInitialize(complianceResult);

				AssertEquals("BLK", invoiceLine.LinkedModuleCommodity.RiskStatus);
				AssertEquals("Blocked", invoiceLine.LinkedModuleCommodity.RiskStatusDescription);
				AssertEquals(false, invoiceLine.LinkedModuleCommodity.RiskStatusDescription_ReadOnly);
				AssertEquals(false, invoiceLine.LinkedModuleCommodity.AssessmentNotes_ReadOnly);
				AssertEquals(true, invoiceLine.LinkedModuleCommodity.CommodityExists);
				AssertEquals("Test Notes1", invoiceLine.LinkedModuleCommodity.AssessmentNotes);
				AssertEquals("Test Harmonized Border Wise Textual: TestBatchInitialize", invoiceLine.LinkedModuleCommodity.HarmonizedBorderWiseTextual);
				AssertEquals("High Risk", invoiceLine.LinkedModuleCommodity.ImportAlertStatus);

				complianceResult.RiskStatus = "CLR";
				complianceResult.RiskNotes = "Test Notes2";

				invoiceLine.LinkedModuleCommodity.SetCommodityInfo(complianceResult);
				AssertEquals("CLR", invoiceLine.LinkedModuleCommodity.RiskStatus);
				AssertEquals("Clear", invoiceLine.LinkedModuleCommodity.RiskStatusDescription);
				AssertEquals("Test Notes2", invoiceLine.LinkedModuleCommodity.AssessmentNotes);
			}
		}

		public void TestCommodity_ValueChanged()
		{
			var helper = new DummyInteractionWithComplianceWiseCommoditiesHelper();
			var invoiceLine = CreateDeclarationWithInvoiceLine(helper);

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityInvoiceLineMocksForTest(true))
			{
				invoiceLine.LinkedModuleCommodity.InitializeIfNeeded();

				AssertEquals("CLR", invoiceLine.LinkedModuleCommodity.RiskStatus);
				AssertEquals("Clear", invoiceLine.LinkedModuleCommodity.RiskStatusDescription);
				AssertEquals("Test Notes", invoiceLine.LinkedModuleCommodity.AssessmentNotes);
				AssertEquals("Test Harmonized Border Wise Textual", invoiceLine.LinkedModuleCommodity.HarmonizedBorderWiseTextual);
				AssertEquals("Clear", invoiceLine.LinkedModuleCommodity.ImportAlertStatus);
				AssertEquals(0, helper.commodityChangedCount);

				invoiceLine.JI_Tariff = "520621";

				AssertEquals(1, helper.commodityChangedCount);
				AssertEquals("BLK", invoiceLine.LinkedModuleCommodity.RiskStatus);
				AssertEquals("Blocked", invoiceLine.LinkedModuleCommodity.RiskStatusDescription);
				AssertEquals("Test Commodity Changed Note", invoiceLine.LinkedModuleCommodity.AssessmentNotes);
				AssertEquals("Test Harmonized Border Wise Textual: Commodity Changed", invoiceLine.LinkedModuleCommodity.HarmonizedBorderWiseTextual);
				AssertEquals("High Risk", invoiceLine.LinkedModuleCommodity.ImportAlertStatus);

				invoiceLine.JI_CountryOfOrigin = "CA";
				AssertEquals(2, helper.commodityChangedCount);

				invoiceLine.JI_Description = "Test";
				AssertEquals(3, helper.commodityChangedCount);
			}
		}

		public void TestCommodityRisk_AssessmentChanged()
		{
			var helper = new DummyInteractionWithComplianceWiseCommoditiesHelper();
			var invoiceLine = CreateDeclarationWithInvoiceLine(helper);

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityInvoiceLineMocksForTest(true))
			{
				invoiceLine.LinkedModuleCommodity.InitializeIfNeeded();

				AssertEquals("Clear", invoiceLine.LinkedModuleCommodity.RiskStatusDescription);
				AssertEquals("Test Notes", invoiceLine.LinkedModuleCommodity.AssessmentNotes);
				AssertEquals(0, helper.commodityAssessmentChangedCount);

				invoiceLine.LinkedModuleCommodity.RiskStatus = "BLK";
				AssertEquals(1, helper.commodityAssessmentChangedCount);

				invoiceLine.LinkedModuleCommodity.AssessmentNotes = "Initialized";
				AssertEquals(2, helper.commodityAssessmentChangedCount);
			}
		}

		public void TestCommodityImportAlertForExportCodeList()
		{
			var invoiceLine = CreateDeclarationWithInvoiceLine();

			CombineAssertions("Commodity Import Alert For Export Job Code List", () =>
			{
				Assert(invoiceLine.LinkedModuleCommodity.CommodityImportAlertForExportCodeList.Count == 3);
				AssertContainsExactElementsInAnyOrder(new[]
				{
					"CLR",
					"PRS",
					"HSK"
				}, invoiceLine.LinkedModuleCommodity.CommodityImportAlertForExportCodeList.Cast<CodeDescriptionPair>().Select(u => u.Code).ToList());
			});
		}

		public void TestViewBorderWisePortalIfAvailable()
		{
			var helper = new DummyInteractionWithComplianceWiseCommoditiesHelper();
			var invoiceLine = CreateDeclarationWithInvoiceLine(helper);

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityInvoiceLineMocksForTest(true))
			{
				AssertEquals(false, invoiceLine.LinkedModuleCommodity.CommodityExists);
				AssertEquals(string.Empty, invoiceLine.LinkedModuleCommodity.HarmonizedBorderWiseTextual);

				invoiceLine.LinkedModuleCommodity.ViewBorderWisePortalIfAvailable().Wait();
				AssertEquals(0, helper.ViewBorderWisePortalCount);
				AssertEquals(string.Empty, invoiceLine.LinkedModuleCommodity.LegalBookLink);

				invoiceLine.LinkedModuleCommodity.InitializeIfNeeded();
				AssertEquals(true, invoiceLine.LinkedModuleCommodity.CommodityExists);
				AssertEquals("Test Harmonized Border Wise Textual", invoiceLine.LinkedModuleCommodity.HarmonizedBorderWiseTextual);
				AssertEquals("View", invoiceLine.LinkedModuleCommodity.LegalBookLink);

				invoiceLine.LinkedModuleCommodity.ViewBorderWisePortalIfAvailable().Wait();
				AssertEquals(1, helper.ViewBorderWisePortalCount);
				AssertEquals("520620", helper.ViewBorderWiseCommodity.HarmonizedCode);
				AssertEquals("WCO", helper.ViewBorderWiseCommodity.GroupingOrCountry);
				AssertEquals(string.Empty, helper.ViewBorderWiseCommodity.GoodsDescription);
				AssertEquals("US", helper.ViewBorderWiseCommodity.OriginOfGoods);
			}
		}

		BaseJobComInvoiceLine CreateDeclarationWithInvoiceLine(IInteractionWithComplianceWiseCommoditiesHelper helper = null)
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			if (helper != null)
			{
				((ISupportInteractionWithComplianceWiseCommodities)declaration).Helper = helper;
			}

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "520620";
			invoiceLine.JI_CountryOfOrigin = "US";

			return invoiceLine;
		}
	}

	public class DummyInteractionWithComplianceWiseCommoditiesHelper : IInteractionWithComplianceWiseCommoditiesHelper
	{
		public DummyInteractionWithComplianceWiseCommoditiesHelper()
		{
			SourceSideCommodities = new SourceSideCommodities() { GetCommodityStatusFromCpw = GetCommoditiesStatusFromCpw, CommoditiesChanged = CommoditiesChanged, CommoditiesAssessmentChanged = CommoditiesAssessmentChanged, ViewBorderWisePortalIfAvailable = ViewBorderWisePortalIfAvailable };
			CpwSideCommodities = new CpwSideCommodities();
		}

		public ISourceSideCommodities SourceSideCommodities { get; }
		public ICpwSideCommodities CpwSideCommodities { get; }

		public int ViewBorderWisePortalCount { get; set; }

		public ComplianceCommodityFromSource ViewBorderWiseCommodity { get; set; }

		Task ViewBorderWisePortalIfAvailable(ComplianceCommodityFromSource line)
		{
			ViewBorderWisePortalCount++;
			ViewBorderWiseCommodity = line;
			return Task.CompletedTask;
		}

		void SetComplianceResultFromCpwForTest(ComplianceResultFromCpw complianceResult)
		{
			this.complianceResult = complianceResult;
		}

		ComplianceResultFromCpw complianceResult = new ComplianceResultFromCpw
		{
			HarmonizedCode = "520620",
			GroupingOrCountry = "WCO",
			GoodsDescription = "",
			OriginOfGoods = "US",
			LinkVisible = true,
			HarmonizedBorderWiseTextual = "Test Harmonized Border Wise Textual",
			RiskStatus = "CLR",
			RiskNotes = "Test Notes",
			AssessmentInitialized = true,
			ImportAlertStatus = "Clear"
		};

		ComplianceResultFromCpw? GetCommoditiesStatusFromCpw(ComplianceCommodityFromSource commodityFromSource)
		{
			return complianceResult;
		}

		void CommoditiesChanged(IComplianceCommodity[] changedCommodities)
		{
			commodityChangedCount++;
			var changedCommodity = changedCommodities[0];

			var complianceResult = new ComplianceResultFromCpw
			{
				HarmonizedCode = changedCommodity.HarmonizedCode,
				GroupingOrCountry = "WCO",
				GoodsDescription = changedCommodity.GoodsDescription,
				OriginOfGoods = changedCommodity.GroupingOrCountry,
				LinkVisible = true,
				HarmonizedBorderWiseTextual = "Test Harmonized Border Wise Textual: Commodity Changed",
				RiskStatus = "BLK",
				RiskNotes = "Test Commodity Changed Note",
				AssessmentInitialized = true,
				ImportAlertStatus = "High Risk"
			};
			SetComplianceResultFromCpwForTest(complianceResult);
		}

		public int commodityChangedCount;

		void CommoditiesAssessmentChanged(ComplianceCommodityFromSource[] changedCommodities)
		{
			commodityAssessmentChangedCount++;
		}

		public int commodityAssessmentChangedCount;
	}
}
