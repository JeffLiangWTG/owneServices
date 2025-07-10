
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NZ.Business.Declaration.FormalEntry.Testing
{
	using NUnit.Framework;

	[TestedType(typeof(CusEntryHeaderCollection))]
	public class CusEntryHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			return new CusEntryHeaderCollection(declaration, Factory);
		}

		[ExpectNoExceptions]
		public void TestIndexer()
		{
			testCollection.AddNew();
			CusEntryHeader entryHeader = testCollection[0];
		}

		public void TestAddNewSpecificType()
		{
			BusinessObject bO = testCollection.AddNew();
			Assert("BO is typeof CusEntryHeader", bO is CusEntryHeader);
		}

		protected CusEntryHeaderCollection testCollection;
		protected override void SetUp()
		{
			base.SetUp();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			testCollection = new CusEntryHeaderCollection(declaration, Factory);
		}
	}
}
