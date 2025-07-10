using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgInvoiceRollupOrGroupValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPG_NullRef()
		{
			var factory = new BusinessObjectFactory();

			var orgInDB = factory.LoadFromNaturalKey<OrgHeader>(Enterprise.ZArchitecture.Schema.OrgHeaderSchema.OH_Code, "DEMORG");
			var testInvoiceRollup = orgInDB.CompanyData.InvoiceRollupOrGroups.AddNew();
			DataRow row = ((IBusinessObjectInternals)testInvoiceRollup).Row;

			var invoiceRollup = new AutoOrgInvoiceRollupOrGroupCompanyDataNullRef(factory, row);
			var validation = new OrgInvoiceRollupOrGroupValidationForTest(invoiceRollup);

			AssertNoExceptionThrown(delegate
			{
				validation.CheckPG_AllInOne();
			});
		}
	}
}
