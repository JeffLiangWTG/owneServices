using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(CusISFHeaderInvoicingSupporter))]
	sealed class CusISFHeaderInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public void TestSecurity()
		{
			var supporter = GetNewBusinessObject().InvoicingSupporter;
			AssertEquals("AuditSecurity", Env.Security.ImporterSecurityFilingAuditBilling, supporter.AuditSecurity);
			AssertEquals("JobInvoicingSecurity", Env.Security.ImporterSecurityFilingJobInvoicing, supporter.JobInvoicingSecurity);
		}

		protected override IJobInvoicingPlugIn GetNewBusinessObject() => Factory.NewWithValidTestData<CusISFHeader>();

		protected override ZString TestingCountry => Core.Constants.CountryCodes.UnitedStates;
	}
}
