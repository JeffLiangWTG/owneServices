using System;
using CargoWise.Types;

namespace Enterprise.ReportTesting.Accounting
{
	using Core;
	using Enterprise.MasterFiles.Business;
	using NUnit.Framework;

	[TemplateName("ProfitAndLossMonthly_Chinese")]
	public
	class ProfitAndLossMonthly_Chinese : TemplateTestCase
	{
		[ExpectNoExceptions]
		[TestDate(2013, 03, 24)]
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
			var list = new ComplianceReportTypeCollection(Constants.CountryCodes.China);
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
