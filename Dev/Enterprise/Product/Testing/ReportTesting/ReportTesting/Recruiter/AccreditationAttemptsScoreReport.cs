namespace Enterprise.ReportTesting.Recruiter
{
	using CargoWise.Data;
	using Enterprise.ReportTesting;

	[TemplateName("Accreditation Attempts Score Report")]
	public class TestAccreditationAttemptsScoreReport : TemplateTestCase
	{
		protected override void FillReportWithDefaultValues()
		{
			var insertAccreditation = "INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_SystemCreateTimeUtc, HAC_SystemCreateUser, HAC_SystemLastEditTimeUtc, HAC_SystemLastEditUser) VALUES (NEWID(), 'NEW', 'New Accred', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			Db.Connection.ExecuteNonQuery(insertAccreditation);
			base.FillReportWithDefaultValues();
		}
	}

	public class TestAccreditationAttemptsScoreReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.MasterFiles.Module.HRReports(); }
		}

		public override string MenuName
		{
			get { return "Accreditation Attempts Score Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"Use this report to generate a list of accreditation attempts with their corresponding scores.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestAccreditationAttemptsScoreReport();
		}
	}
}
