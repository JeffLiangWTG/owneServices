using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocSending;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.DocSending
{
	[TestedType(typeof(DocSendingBusinessObjectCollection))]
	sealed class DocSendingBusinessObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocSendingBusinessObjectCollection>
	{
		protected override DocSendingBusinessObjectCollection GetCollectionToTest()
			=> new DocSendingBusinessObjectCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new DocSendingBusinessObject
		{
		};
	}
}
