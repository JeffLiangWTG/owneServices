using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderCollection))]
	sealed class CusEntryHeaderCollectionTest : Customs.Business.Testing.CusEntryHeaderCollectionTest
	{
		public void TestCusEntryHeaderCollection()
		{
			JobDeclaration aDeclaration = Factory.New<JobDeclaration>();
			CusEntryHeaderCollection aCollection = new CusEntryHeaderCollection(aDeclaration, Factory);
			AssertNotNull("Failed to create Collection", aCollection);
			aCollection.AddNew();
			AssertNotNull("Failed to add new container", aCollection[0]);
		}

		public new void TestTypedAddNew()
		{
			JobDeclaration aDeclaration = Factory.New<JobDeclaration>();
			CusEntryHeaderCollection aCollection = new CusEntryHeaderCollection(aDeclaration, Factory);
			CusEntryHeader container = aCollection.AddNew();
			Assert(container != null);
		}

		public void TestClearPackages()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.CustomsEntryHeaders.AddNew().CH_Packages = 123;
			AssertEquals("Precondition", 123, testDec.CustomsEntryHeaders[0].CH_Packages);
			testDec.CustomsEntryHeaders.ClearPackages();
			AssertEquals(0, testDec.CustomsEntryHeaders[0].CH_Packages);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new CusEntryHeaderCollection((JobDeclaration)jobDec, Factory);
	}
}
