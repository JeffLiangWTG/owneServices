using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(DocumentViewCollection))]
	sealed class DocumentViewCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocumentViewCollection>
	{
		protected override DocumentViewCollection GetCollectionToTest()
		{
			return new DocumentViewCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DocumentView(Factory);
		}
	}
}
