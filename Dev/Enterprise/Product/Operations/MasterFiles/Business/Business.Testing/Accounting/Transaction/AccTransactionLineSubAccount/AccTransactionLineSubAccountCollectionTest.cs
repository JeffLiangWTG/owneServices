using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTransactionLineSubAccountCollection<AccTransactionLineSubAccount, AccTransactionLines>))]
	sealed class AccTransactionLineSubAccountCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new AccTransactionLineSubAccountCollection<AccTransactionLineSubAccount, AccTransactionLines>(Factory.New<AccTransactionLines>());
		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<AccTransactionLineSubAccount>();
	}
}
