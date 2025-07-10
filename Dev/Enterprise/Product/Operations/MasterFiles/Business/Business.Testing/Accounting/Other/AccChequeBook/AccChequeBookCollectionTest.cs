using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccChequeBookCollection))]
	class AccChequeBookCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			AccBankAccount bankAccount = Factory.New<AccBankAccount>();
			return new AccChequeBookCollection(Factory, bankAccount);
		}
	}
}
