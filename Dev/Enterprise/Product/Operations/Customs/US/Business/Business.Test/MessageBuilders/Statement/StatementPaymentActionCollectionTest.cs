using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(StatementPaymentActionCollection))]
	sealed class StatementPaymentActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<StatementPaymentActionCollection>
	{
		public void TestAllowNewCore()
		{
			var collection = new StatementPaymentActionCollection(Factory);
			collection.PopulateStatementHeaderElements(new ZGuid[] { StatementHeader.PK });
			Assert(!collection.AllowNew);
			Assert(!collection.AllowRemove);
		}

		public void TestPopulateStatementHeaderElements()
		{
			var statementHeader1 = Factory.New<CusStatementHeader>();
			var statementHeader2 = Factory.New<CusStatementHeader>();
			var collection = new StatementPaymentActionCollection(Factory);
			collection.PopulateStatementHeaderElements(new ZGuid[] { statementHeader1.PK, statementHeader2.PK });
			AssertEquals(2, collection.Count);
		}

		public override void TestAdd()
		{
			Assert("Users cannot create a new element in the grid.", true);
		}

		public override void TestDelete()
		{
			var statementHeader1 = Factory.New<CusStatementHeader>();
			var statementHeader2 = Factory.New<CusStatementHeader>();
			var collection = new StatementPaymentActionCollection(Factory);
			collection.PopulateStatementHeaderElements(new ZGuid[] { statementHeader1.PK, statementHeader2.PK });
			AssertEquals(2, collection.Count);
			collection.RemoveAndDelete(collection[0]);
			AssertEquals(1, collection.Count);
		}

		public override void TestTypedget_Item()
		{
			Assert("Users cannot create a new element in the grid.", true);
		}

		public override void TestRemoveFromRelationship()
		{
			Assert("No Relationship", true);
		}

		protected override StatementPaymentActionCollection GetCollectionToTest() => new StatementPaymentActionCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<StatementDeleteAndSendingAction>();

		CusStatementHeader statementHeader;
		CusStatementHeader StatementHeader => statementHeader ?? (statementHeader = Factory.New<CusStatementHeader>());
	}
}
