using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business
{
	[TestedType(typeof(DocumentContainerCollection))]
	public class DocumentContainerCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocumentContainerCollection>
	{
		protected override DocumentContainerCollection GetCollectionToTest()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.ContainerBookedMoves.AddNew();
			return new DocumentContainerCollection(cartage.Containers, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			return new DocumentContainer(container);
		}
	}
}
