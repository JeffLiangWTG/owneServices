using System;
using CargoWise.Types;

namespace Enterprise.ReportTesting.Accounting
{
	using Enterprise.Core;
	using Enterprise.MasterFiles.Business;
	using NUnit.Framework;

	[TemplateName("BalanceSheet_Chinese")]
	public class TestGLBalanceSheetCNReport : TemplateTestCase
	{
		[ExpectNoExceptions]
		[TestDate(2011, 11, 15)]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();
			RunReport();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.China;
			ComplianceReportTypeCollection list = new ComplianceReportTypeCollection(Constants.CountryCodes.China);
			AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsCN.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = country;
		}

		protected override bool ReportRequiresColumnHeadings
		{
			get
			{
				return false;
			}
		}

		readonly ZString country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
	}
}
