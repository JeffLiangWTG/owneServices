using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ReconChangedLineCollection))]
	sealed class ReconChangedLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ReconChangedLineCollection>
	{
		protected override ReconChangedLineCollection GetCollectionToTest() => new ReconChangedLineCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new ReconChangedLine(Factory);
	}
}
