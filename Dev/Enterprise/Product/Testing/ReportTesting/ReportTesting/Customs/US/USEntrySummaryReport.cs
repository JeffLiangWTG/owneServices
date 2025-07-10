namespace Enterprise.ReportTesting.Customs.US
{
	using CargoWise.Types;
	using Enterprise.Customs.Business;
	using Enterprise.Customs.Module;
	using Enterprise.Customs.Universal.Testing;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;

	[TemplateName("US Entry Summary Report")]
	public class TestUSEntrySummaryReport : TemplateTestCase
	{
		public void TestReportSql()
		{
			PrepareReportForRender();
			AssertEquals("ReportData: SELECT <ReportData.SelectList> FROM USEntrySummary(<CurrentCompany>, <Importer>, <Supplier>, <Job Registered On.FromDateUtc>, <Job Registered On.ToNextDateUtc>, <Arrival Date.FromDateForSQLParameter>, <Arrival Date.ToNextDate>, <Entry Port>)",
				Report.Analyser.ReportSQLSources[0].TableNameAndSelectStatement);
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}

		protected override void FillReportWithDefaultValues()
		{
			base.FillReportWithDefaultValues();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "8888", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));

			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.US.IJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = "SEA";
			declaration.JE_MasterBill = "MB24102019";
			var entry = (CusEntryHeader)Factory.New<Integration.Customs.US.ICusEntryHeader>();
			entry.CH_JE = declaration.PK;
			entry.CH_MessageType = "ENS";
			var entryLine = (CusEntryLine)Factory.New<Integration.Customs.US.ICusEntryLine>();
			entryLine.CL_CH = entry.PK;
			entryLine.CL_LineNumber = 1;
			entryLine.CL_AdValoremTariff = "1101100000";
			var invoiceHeader = (BaseJobComInvoiceHeader)Factory.New<Integration.Customs.US.IJobComInvoiceHeader>();
			invoiceHeader.JZ_JE = declaration.PK;
			invoiceHeader.JZ_InvoiceNumber = "INV241019";
			var invoiceLine = (BaseJobComInvoiceLine)Factory.New<Integration.Customs.US.IJobComInvoiceLine>();
			invoiceLine.JI_JZ = invoiceHeader.PK;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Tariff = "1101100000";
			Factory.Save();
		}
	}

	public class TestUSEntrySummaryReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Entry Summary Report"; }
		}

		public override string Hint
		{
			get { return "This report makes entry summary lines information available."; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestUSEntrySummaryReport();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}
	}
}
