namespace Enterprise.ReportTesting.Customs.US
{
	using CargoWise.Types;
	using Enterprise.Customs.Module;
	using Enterprise.Customs.Universal.Testing;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;

	[TemplateName("Declaration Accounting Recon Report")]
	public class TestDeclarationAccountingReconReport : TemplateTestCase
	{
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

			Factory.Save();
		}
	}

	public class TestDeclarationAccountingReconReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Declaration Accounting Reconciliation Report"; }
		}

		public override string Hint
		{
			get
			{
				return "The report lists all the declarations matching the selected criteria and shows Account Payable and Account Receivable amounts for each entry.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestDeclarationAccountingReconReport();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}
	}
}
