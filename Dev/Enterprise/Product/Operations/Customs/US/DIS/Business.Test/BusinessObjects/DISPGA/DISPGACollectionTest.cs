using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	[TestedType(typeof(DISPGACollection))]
	sealed class DISPGACollectionTest : NonPersistentBusinessObjectCollectionTestCase<DISPGACollection>
	{
		protected override DISPGACollection GetCollectionToTest() => new DISPGACollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new DISPGA(Factory);
	}
}
