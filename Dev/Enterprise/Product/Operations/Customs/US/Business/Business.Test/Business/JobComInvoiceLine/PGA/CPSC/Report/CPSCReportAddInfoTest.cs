using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CPSCReportAddInfo))]
	sealed class CPSCReportAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var report = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().CPSCHeaders.AddNew().RuleAndLabs.AddNew().ReportAndLabs.AddNew();
			return new CPSCReportAddInfo(report.B7_AddInfoDataInfo);
		}
	}
}
