namespace Enterprise.ReportTesting.Customs.US
{
	using CargoWise.EntityFramework;
	using Enterprise.Customs.Business;
	using Enterprise.Customs.Module;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;

	[TemplateName("US Potential Free Trade Agreement Duty and MPF Savings Report")]
	public class USPotentialFreeTradeAgreementDutyAndMPFSavingsReportTest : TemplateTestCase
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

	public class TestUSPotentialFreeTradeAgreementDutyAndMPFSavingsReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest => new CustomsReportModule();
		public override string MenuName => "Potential Free Trade Agreement Duty and MPF Savings Report";
		public override string Hint => "This report shows the potential duty and MPF savings that could have been realized if an available SPI had been used on a particular invoice line.";
		protected override TemplateTestCase GetTemplateTestCase() => new USPotentialFreeTradeAgreementDutyAndMPFSavingsReportTest();

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}

		protected override bool ExpectedIsPublished => true;
	}
}
