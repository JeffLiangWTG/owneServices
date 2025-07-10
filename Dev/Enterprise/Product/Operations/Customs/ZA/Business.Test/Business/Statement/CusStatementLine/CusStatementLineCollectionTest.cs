using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CusStatementLineCollection))]
	sealed class CusStatementLineCollectionTest : ActiveBusinessObjectCollectionTestCase<CusStatementLineCollection>
	{
		public void TestAllowNew()
		{
			var cusStatementHeader = Factory.New<CusStatementHeader>();
			var collection = new CusStatementLineCollectionForTest(cusStatementHeader);
			AssertEquals(false, collection.AllowNew);
		}

		public void TestGetStatementLineFor()
		{
			var line = Collection.AddNew();
			AssertEquals("Just checking dependent object set correctly", statementHeader.PK, line.B3_B2);
			line.B3_EntryNum = "100006789";
			line.B3_BrokerReference = "REF";
			var findLine = Collection.GetStatementLineFor("100006789");
			AssertNotNull("Found", findLine);
			AssertEquals("The expected line", line.PK, findLine.PK);
			findLine = Collection.GetStatementLineFor("200006789");
			AssertNull("Not Found", findLine);
		}

		protected override CusStatementLineCollection GetCollectionToTest()
		{
			statementHeader = Factory.New<CusStatementHeader>();
			return new CusStatementLineCollection(statementHeader);
		}
		CusStatementHeader statementHeader;
	}

	sealed class CusStatementLineCollectionForTest : CusStatementLineCollection
	{
		public CusStatementLineCollectionForTest(CusStatementHeader statementHeader)
		: base(statementHeader)
		{
		}

		public new bool AllowNew => base.AllowNew;
	}
}
