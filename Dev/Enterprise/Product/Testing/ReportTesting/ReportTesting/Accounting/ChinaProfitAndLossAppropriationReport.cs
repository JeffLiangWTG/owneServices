using System;
using CargoWise.Types;

namespace Enterprise.ReportTesting.Accounting
{
	using Core;
	using Enterprise.MasterFiles.Business;

	[TemplateName("ChinaProfitAndLossAppropriationReport")]
	public class ChinaProfitAndLossAppropriationReport : TemplateTestCase
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
