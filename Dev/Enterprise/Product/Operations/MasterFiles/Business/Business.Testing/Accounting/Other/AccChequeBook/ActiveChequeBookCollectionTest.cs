using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ActiveChequeBookCollection))]
	sealed class ActiveChequeBookCollectionTest : AccChequeBookCollectionTest
	{
		public void TestCreateRelationshipFilter()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_GB = GlbBranch.CurrentBranch.PK;
			AccChequeBook testBook1 = Factory.NewWithValidTestData<AccChequeBook>();
			testBook1.AK_AB = testBank.PK;
			AccChequeBook testBook2 = Factory.NewWithValidTestData<AccChequeBook>();
			testBook2.AK_AB = testBank.PK;

			testBook1.AK_IsActive = ZBool.False;
			testBook2.AK_IsActive = ZBool.True;
			Factory.Save();

			ActiveChequeBookCollection testCollection = new ActiveChequeBookCollection(Factory);
			testCollection.Load();
			AssertEquals("Collection should contain only 1 record", 1, testCollection.Count);
			Assert(testCollection.Contains(testBook2));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			AccBankAccount bankAccount = Factory.New<AccBankAccount>();
			return new ActiveChequeBookCollection(Factory, bankAccount);
		}
	}
}
