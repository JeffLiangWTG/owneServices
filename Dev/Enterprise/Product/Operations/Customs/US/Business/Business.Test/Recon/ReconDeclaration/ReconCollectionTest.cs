using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ReconCollection))]
	sealed class ReconCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ReconCollection>
	{
		protected override ReconCollection GetCollectionToTest() => new ReconCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new ReconDeclaration(Factory.New<JobDeclaration>());
	}
}
