using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(StatementDeleteAndSendingActionCollection))]
	sealed class StatementSendingActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<StatementDeleteAndSendingActionCollection>
	{
		public void TestHasAtLeastOneToSendMessageFor()
		{
			StatementDeleteAndSendingActionCollection collection = new StatementDeleteAndSendingActionCollection(StatementHeader);
			AssertEquals("HasAtLeastOneToSendMessageFor", true, collection.HasAtLeastOneToSendMessageFor);
		}

		public void TestAllowNew()
		{
			StatementDeleteAndSendingActionCollection collection = new StatementDeleteAndSendingActionCollection(StatementHeader);
			AssertEquals("Users do not add a new element to this collection in the grid", false, collection.AllowNew);
		}

		public void TestIsCancelled()
		{
			StatementDeleteAndSendingActionCollection collection = new StatementDeleteAndSendingActionCollection(StatementHeader);
			AssertEquals("IsCancelled_getter", typeof(ZBool), collection.IsCancelled.GetType());
			collection.IsCancelled = ZBool.True;
			AssertEquals("IsCancalled_setter", ZBool.True, collection.IsCancelled);
		}

		public void TestStatementDeleteAndSendingActionCollection()
		{
			StatementDeleteAndSendingActionCollection collectionStatement = new StatementDeleteAndSendingActionCollection(StatementHeader);
			AssertEquals("Number StatementHeader in StatementDeleteAndSendingActionCollection.", 1, collectionStatement.Count);
			AssertEquals("US_SendMessage", ZBool.True, collectionStatement[0].US_SendMessage);
			AssertEquals(false, collectionStatement.IsReconciliationAction);

			Declaration.US_EnableENS = true;

			CusEntryHeader entry = Declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Declaration.US_PaymentType = "2";

			StatementDeleteAndSendingActionCollection collectionEntry = new StatementDeleteAndSendingActionCollection(Declaration);
			AssertEquals("Number EntryHeader in StatementDeleteAndSendingActionCollection.", 1, collectionEntry.Count);
			AssertEquals("US_SendMessage", ZBool.True, collectionEntry[0].US_SendMessage);
			AssertEquals(false, collectionEntry.IsReconciliationAction);

			var reconDeclaration = new ReconDeclaration(Declaration);
			StatementDeleteAndSendingActionCollection collectionRecon = new StatementDeleteAndSendingActionCollection(reconDeclaration);
			AssertEquals("Number Of Actions in StatementDeleteAndSendingActionCollection.", 1, collectionEntry.Count);
			AssertEquals("US_SendMessage", ZBool.True, collectionEntry[0].US_SendMessage);
			AssertEquals(true, collectionRecon.IsReconciliationAction);
		}

		public void TestHasMessageErrors()
		{
			Declaration.US_EnableENS = true;

			var entry = Declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Declaration.US_PaymentType = "2";

			var coll = new StatementDeleteAndSendingActionCollection(Declaration);
			var action = coll[0];
			action.US_SendMessage = false;
			action.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			action.US_PreliminaryStatementPrintDate = new ZDateTime(2011, 1, 2);
			action.US_PeriodicStatementMonth = "01";
			AssertHasMessageErrors("PreCondition", action.US_PeriodicStatementMonthInfo);
			Assert(!coll.HasMessageErrors());

			action.US_SendMessage = true;
			Assert(coll.HasMessageErrors());
		}

		public override void TestAdd()
		{
			StatementHeader.StatementLines.AddNew();
			AssertEquals(2, StatementHeader.StatementLines.Count);
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			CusStatementHeader statementHeader1 = Factory.New<CusStatementHeader>();
			statementHeader1.StatementLines.AddNew();
			AssertEquals(1, statementHeader1.StatementLines.Count);
		}

		public override void TestRemoveFromRelationship()
		{
			CusStatementHeader statementHeader1 = Factory.New<CusStatementHeader>();
			CusStatementLine line = statementHeader1.StatementLines.AddNew();
			AssertEquals(1, statementHeader1.StatementLines.Count);

			statementHeader1.StatementLines.Remove(line);
			AssertEquals(0, statementHeader1.StatementLines.Count);
		}

		public override void TestDelete()
		{
			CusStatementHeader statementHeader1 = Factory.New<CusStatementHeader>();
			CusStatementLine line1 = statementHeader1.StatementLines.AddNew();
			CusStatementLine line2 = statementHeader1.StatementLines.AddNew();

			statementHeader1.StatementLines.RemoveAndDelete(line1);
			AssertEquals(1, statementHeader1.StatementLines.Count);
			AssertEquals(true, statementHeader1.StatementLines.Contains(line2));
		}

		protected override StatementDeleteAndSendingActionCollection GetCollectionToTest() => new StatementDeleteAndSendingActionCollection(StatementHeader);

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<StatementDeleteAndSendingAction>();

		CusStatementHeader statementHeader;
		CusStatementHeader StatementHeader
		{
			get
			{
				if (statementHeader == null)
				{
					statementHeader = Factory.New<CusStatementHeader>();
					statementHeader.StatementLines.AddNew();
					statementHeader.StatementLines[0].B3_Status = StatementLineStatusList.Codes.Active;
				}
				return statementHeader;
			}
		}

		JobDeclaration declaration;
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				}
				return declaration;
			}
		}
	}
}
