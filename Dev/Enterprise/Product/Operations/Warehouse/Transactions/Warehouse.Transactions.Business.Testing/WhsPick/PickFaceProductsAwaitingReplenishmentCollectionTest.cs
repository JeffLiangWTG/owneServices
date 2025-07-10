using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(PickFaceProductsAwaitingReplenishmentCollection))]
	public class PickFaceProductsAwaitingReplenishmentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PickFaceProductsAwaitingReplenishmentCollection>
	{
		protected override PickFaceProductsAwaitingReplenishmentCollection GetCollectionToTest()
		{
			return new PickFaceProductsAwaitingReplenishmentCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var client = Factory.New<OrgHeader>();
			var product = Factory.New<OrgSupplierPart>();
			return new PickFaceProductsAwaitingReplenishment(client.PK, product.PK, Factory);
		}
	}
}
