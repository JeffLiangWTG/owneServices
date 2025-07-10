using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("ReportSetupsProfile")]
	public class ReportSetupsProfile : TemplateTestCase
	{
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

		readonly ZString country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
	}
}
