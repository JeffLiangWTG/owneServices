namespace Enterprise.ReportTesting.Recruiter
{
	using CargoWise.Data;
	using Enterprise.ReportTesting;

	[TemplateName("Accreditation Attempts Report")]
	public class TestAccreditationAttemptsReport : TemplateTestCase
	{
		protected override void FillReportWithDefaultValues()
		{
			var insertAccreditation = "INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_SystemCreateTimeUtc, HAC_SystemCreateUser, HAC_SystemLastEditTimeUtc, HAC_SystemLastEditUser) VALUES (NEWID(), 'NEW', 'New Accred', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			Db.Connection.ExecuteNonQuery(insertAccreditation);
			base.FillReportWithDefaultValues();
		}
	}

	public class TestAccreditationAttemptsReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.MasterFiles.Module.HRReports(); }
		}

		public override string MenuName
		{
			get { return "Accreditation Attempts Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"Use this report to generate a list of accreditation attempts.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestAccreditationAttemptsReport();
		}
	}
}
