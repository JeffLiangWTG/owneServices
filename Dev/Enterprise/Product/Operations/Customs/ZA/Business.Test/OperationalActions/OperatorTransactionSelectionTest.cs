using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.OperationalActions.Testing
{
	[TestedType(typeof(OperatorTransactionSelection))]
	class OperatorTransactionSelectionTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2023, 01, 01)]
		public void TestNoValidationWarningsOnTransactionDate()
		{
			var selection = (OperatorTransactionSelection)GetNewBusinessObject();

			selection.TransactionDate = ZDateTime.Now.AddYears(-1).AddDays(-1);
			AssertNoWarnings(selection.TransactionDateInfo);
		}
	}
}
