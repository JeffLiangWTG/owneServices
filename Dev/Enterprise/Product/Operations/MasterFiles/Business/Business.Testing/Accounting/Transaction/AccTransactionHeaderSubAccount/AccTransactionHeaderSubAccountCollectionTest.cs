using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTransactionHeaderSubAccountCollection<AccTransactionHeaderSubAccount, AccTransactionHeader>))]
	sealed class AccTransactionHeaderSubAccountCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new AccTransactionHeaderSubAccountCollection<AccTransactionHeaderSubAccount, AccTransactionHeader>(Factory.New<AccTransactionHeader>());

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<AccTransactionHeaderSubAccount>();
	}
}
