using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class InvoiceLineNewOwnerPartDetailsTest : TestCaseWithFactory
	{
		public void TestIInvoiceLinePartDetailsMembers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var invoiceLine = Factory.New<JobComInvoiceLine>();
				IInvoiceLinePartDetails partDetails = new InvoiceLineNewOwnerPartDetails(invoiceLine);
				AssertEquals(Core.Constants.CountryCodes.SouthAfrica, partDetails.CustomsCountryCode);
				AssertEquals(typeof(OrgSupplierPart), partDetails.TypeOfPartUsed);
			}
		}
	}
}
