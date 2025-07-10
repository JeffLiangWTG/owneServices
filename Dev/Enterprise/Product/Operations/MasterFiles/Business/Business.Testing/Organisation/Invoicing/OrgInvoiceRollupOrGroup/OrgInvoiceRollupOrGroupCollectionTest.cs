using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgInvoiceRollupOrGroupCollection))]
	sealed class OrgInvoiceRollupOrGroupCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			return new OrgInvoiceRollupOrGroupCollection(org.CompanyData);
		}
	}
}
