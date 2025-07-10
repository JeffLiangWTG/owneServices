using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ReconFlattenedDataLineCollection))]
	sealed class ReconFlattenedDataLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ReconFlattenedDataLineCollection>
	{
		protected override ReconFlattenedDataLineCollection GetCollectionToTest() => new ReconFlattenedDataLineCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new ReconFlattenedDataLine();
	}
}
