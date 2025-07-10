using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(FTZ214EntryLineCollection))]
	sealed class FTZ214EntryLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<FTZ214EntryLineCollection>
	{
		protected override FTZ214EntryLineCollection GetCollectionToTest() => new FTZ214EntryLineCollection(Factory.NewWithValidTestData<JobDeclaration>());

		protected override BusinessObject GetNewElementToAddToTheCollection() => new FTZ214EntryLine(Factory);
	}
}
