using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.OperationalActions.Testing
{
	[TestedType(typeof(OperatorTransactionSelectionCollection))]
	class OperatorTransactionSelectionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<OperatorTransactionSelectionCollection>
	{
		protected override OperatorTransactionSelectionCollection GetCollectionToTest() => new OperatorTransactionSelectionCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new OperatorTransactionSelection();
	}
}
