namespace Enterprise.ReportTesting.Customs.US
{
	using CargoWise.EntityFramework;
	using Enterprise.Customs.Business;
	using Enterprise.Customs.Module;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;

	[TemplateName("US Free Trade Agreement Duty and MPF Savings Realized Report")]
	public class USFreeTradeAgreementDutyAndMPFSavingsRealizedReportTest : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}

		protected override void FillReportWithDefaultValues()
		{
			base.FillReportWithDefaultValues();
			var declaration = Factory.New<Integration.Customs.US.IJobDeclaration>();
			((BusinessObject)declaration).FillWithValidTestData();
			declaration.JE_MessageType = "IMP";
			declaration.US_EntryType = "01";
			declaration.US_EnableENS = true;
			var entry = Factory.New<Integration.Customs.US.ICusEntryHeader>();
			entry.CH_JE = declaration.PK;
			entry.CH_MessageType = "ENS";
			var entryLine = (CusEntryLine)Factory.New<Integration.Customs.US.ICusEntryLine>();
			entryLine.CL_CH = entry.PK;
			entryLine.CL_LineNumber = 1;
			var invoice = Factory.New<Integration.Customs.US.IJobComInvoiceHeader>();
			invoice.JZ_JE = declaration.PK;
			var invoiceLine = Factory.New<Integration.Customs.US.IJobComInvoiceLine>();
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.JI_AddInfo = "CustomsValue=10000*NonFTADuty=777*FTADuty=111.56*NonFTAPayableMPF=34.64*SupDuty=1234*FTAPayableMPF=14.54*SupTariff=99038501";
			Factory.Save();
		}
	}

	public class TestUSFreeTradeAgreementDutyAndMPFSavingsRealizedReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest => new CustomsReportModule();
		public override string MenuName => "Free Trade Agreement Duty and MPF Savings Realized Report";
		public override string Hint => "This report shows the actual duty and MPF savings on an invoice line when a Free Trade Agreement was used verses what the duty and MPF would have been if no SPI had been used on that line.";
		protected override TemplateTestCase GetTemplateTestCase() => new USFreeTradeAgreementDutyAndMPFSavingsRealizedReportTest();

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}

		protected override bool ExpectedIsPublished => true;
	}
}
