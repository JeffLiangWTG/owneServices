using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	[TestedType(typeof(DISAdditionalDataCollection))]
	sealed class DISAdditionalDataCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DISAdditionalDataCollection>
	{
		protected override DISAdditionalDataCollection GetCollectionToTest() => new DISAdditionalDataCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new DISAdditionalData(Factory);
	}
}
