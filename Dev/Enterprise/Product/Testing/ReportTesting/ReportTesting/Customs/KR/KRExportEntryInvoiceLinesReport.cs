using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Module;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ReportTesting.Customs.KR
{
	[TemplateName("KR Export Entry Invoice Lines Report")]
	public class TestKRExportEntryInvoiceLinesReport : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("KR");
		}

		protected override void FillReportWithDefaultValues()
		{
			base.FillReportWithDefaultValues();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSOF", "Customs Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.KoreaSouth, "CUSOF", "020", "인천세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "1111111111111";
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			Factory.Save();
		}
	}

	public class TestKRExportEntryInvoiceLinesReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "KR Export Entry Invoice Lines Report"; }
		}

		public override string Hint
		{
			get { return "This report makes export invoice lines available."; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestKRExportEntryInvoiceLinesReport();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("KR");
		}
	}
}
