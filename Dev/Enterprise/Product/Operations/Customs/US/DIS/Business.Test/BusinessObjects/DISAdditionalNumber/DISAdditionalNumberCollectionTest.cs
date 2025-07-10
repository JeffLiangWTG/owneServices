using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	[TestedType(typeof(DISAdditionalNumberCollection))]
	sealed class DISAdditionalNumberCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DISAdditionalNumberCollection>
	{
		protected override DISAdditionalNumberCollection GetCollectionToTest() => new DISAdditionalNumberCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new DISAdditionalNumber(Factory);
	}
}
