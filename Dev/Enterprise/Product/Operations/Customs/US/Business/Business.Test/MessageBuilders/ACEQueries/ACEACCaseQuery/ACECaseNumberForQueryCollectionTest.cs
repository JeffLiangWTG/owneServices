using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ACECaseNumberForQueryCollection))]
	sealed class ACECaseNumberForQueryCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ACECaseNumberForQueryCollection>
	{
		protected override ACECaseNumberForQueryCollection GetCollectionToTest() => new ACECaseNumberForQueryCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new ACECaseNumberForQuery();
	}
}
