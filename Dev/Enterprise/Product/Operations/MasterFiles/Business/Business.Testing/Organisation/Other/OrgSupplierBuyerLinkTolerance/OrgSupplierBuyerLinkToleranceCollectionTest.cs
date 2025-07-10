using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSupplierBuyerLinkToleranceCollection))]
	public class OrgSupplierBuyerLinkToleranceCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgSupplierBuyerLinkToleranceCollection>
	{
		protected override OrgSupplierBuyerLinkToleranceCollection GetCollectionToTest()
		{
			return new OrgSupplierBuyerLinkToleranceCollection(Factory.New<OrgSupplierBuyerLink>());
		}
	}
}
