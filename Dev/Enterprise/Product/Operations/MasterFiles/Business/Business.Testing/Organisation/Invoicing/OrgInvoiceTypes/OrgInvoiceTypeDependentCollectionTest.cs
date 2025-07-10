using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgInvoiceTypeCollection))]
	sealed class OrgInvoiceTypeDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgHeader testHeader = Factory.NewWithValidTestData<OrgHeader>();
			return new OrgInvoiceTypeCollection(testHeader.CompanyData);
		}
	}
}
