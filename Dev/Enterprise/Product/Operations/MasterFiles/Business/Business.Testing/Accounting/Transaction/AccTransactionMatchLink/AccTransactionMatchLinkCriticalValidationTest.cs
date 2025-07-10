using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class AccTransactionMatchLinkCriticalValidationTest : CriticalValidationTest<AccTransactionMatchLink>
	{
		protected override List<TestCaseDefinitionWithDelegate_Obsolete> GetTestCases()
		{
			Assert(true);
			return new List<TestCaseDefinitionWithDelegate_Obsolete>();
		}

		public void TestLinkedTransactionHasNotChanged_TransactionNotInLocalCache()
		{
			var matchLink = GetGroupMemberWithGroupNumberMatchLink(Factory);
			var originalTransactionPK = matchLink.TransactionHeader.PK;
			matchLink.AP_AH = new ZGuid("2d39c837-7378-4db3-a3b0-5ba1655cd074");

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("MatchLink without linked transaction in local cache.",
																			 true,
																			 CriticalValidationErrorType.MatchLinkLinkedTransactionHasNotChanged_1,
																			 CriticalValidationMessageTemplate.MatchLinkLinkedTransactionHasNotChanged,
																			 "Match Link: Group Number =",
																			 "Linked Transaction Info :",
																			 "Linked Transaction was not in local cache.");
			AssertOnSavingCheck(matchLink, testCase);
		}

		public void TestLinkedTransactionHasNotChanged_TransactionInLocalCacheAndNotInDatabase()
		{
			var matchLink = GetGroupMemberWithGroupNumberMatchLink(Factory);
			Assert("Precondtion - Transaction is not in DB.", !matchLink.TransactionHeader.IsInDatabase);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("MatchLink with linked transaction in local cache, linked transaction is not in DB.",
																			false,
																			CriticalValidationErrorType.NoError,
																			string.Empty);
			AssertOnSavingCheck(matchLink, testCase);
		}

		public void TestLinkedTransactionHasNotChanged_TransactionInLocalCacheAndInDatabaseAndHasNotChanged()
		{
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_InvoiceAmount = 20m;
			header.AH_OutstandingAmount = 10m;
			var matchLink = Factory.New<AccTransactionMatchLink>();
			matchLink.AP_AH = header.PK;
			matchLink.AP_MatchDate = ZDate.Today;
			matchLink.AP_MatchGroupNum = "1234567890";
			matchLink.AP_Amount = 10m;
			new MatchLinkGroupForTest(Factory).Add(matchLink);
			Factory.Save();

			Assert("Precondtion - Transaction is in DB.", matchLink.TransactionHeader.IsInDatabase);
			AssertNotEquals("Precondtion - Transaction has no changes", System.Data.DataRowState.Modified, ((INeedRow)matchLink.TransactionHeader).Row.RowState);
			AssertNotEquals("Precondtion - Transaction outstanding amount is not zero", 0m, header.AH_OutstandingAmount);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("MatchLink with linked transaction in local cache, linked transaction is in DB, but has no changes.",
																			 true,
																			 CriticalValidationErrorType.MatchLinkLinkedTransactionHasNotChanged_1,
																			 CriticalValidationMessageTemplate.MatchLinkLinkedTransactionHasNotChanged,
																			 "Match Link: Group Number =",
																			 "Linked Transaction Info :",
																			 "Header: PK");
			AssertOnSavingCheck(matchLink, testCase);
		}

		public void TestLinkedTransactionHasNotChanged_TransactionInLocalCacheAndInDatabaseAndHasNotChangedWithOutstandingAmountAndTotalAmountAndMatchlinkAmountAllZero()
		{
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			Factory.Save();

			var matchLink = Factory.New<AccTransactionMatchLink>();
			matchLink.AP_AH = header.PK;
			matchLink.AP_MatchDate = ZDate.Today;
			matchLink.AP_MatchGroupNum = "1234567890";
			new MatchLinkGroupForTest(Factory).Add(matchLink);

			Assert("Precondtion - Transaction is in DB.", header.IsInDatabase);
			AssertNotEquals("Precondtion - Transaction has no changes", System.Data.DataRowState.Modified, ((INeedRow)header).Row.RowState);
			AssertEquals("Precondtion - Transaction outstanding amount is zero", 0m, header.AH_OutstandingAmount);
			AssertEquals("Precondtion - Transaction invoice amount is zero", 0m, header.AH_InvoiceAmount);
			AssertEquals("Precondtion - Transaction tax amount is zero", 0m, header.AH_GSTAmount);
			AssertEquals("Precondtion - Match link amount is zero", 0m, matchLink.AP_Amount);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"MatchLink with linked transaction in local cache, linked transaction is in DB and has no changes with total amount, outstanding amount and matchlink amount all zero.",
				false,
				CriticalValidationErrorType.NoError,
				string.Empty);

			AssertOnSavingCheck(matchLink, testCase);
		}

		public void TestLinkedTransactionHasNotChanged_TransactionInLocalCacheAndInDatabaseAndHasNotChangedWithOutstandingAmountAndTotalAmountAreZeroAndMatchLinkAmountNotZero()
		{
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			Factory.Save();

			var matchLink = Factory.New<AccTransactionMatchLink>();
			matchLink.AP_Amount = 10M;
			matchLink.AP_AH = header.PK;
			matchLink.AP_MatchDate = ZDate.Today;
			matchLink.AP_MatchGroupNum = "1234567890";
			new MatchLinkGroupForTest(Factory).Add(matchLink);

			Assert("Precondtion - Transaction is in DB.", header.IsInDatabase);
			AssertNotEquals("Precondtion - Transaction has no changes", System.Data.DataRowState.Modified, ((INeedRow)header).Row.RowState);
			AssertEquals("Precondtion - Transaction outstanding amount is zero", 0m, header.AH_OutstandingAmount);
			AssertEquals("Precondtion - Transaction invoice amount is zero", 0m, header.AH_InvoiceAmount);
			AssertEquals("Precondtion - Transaction tax amount is zero", 0m, header.AH_GSTAmount);
			AssertEquals("Precondtion - Match link amount is not zero", 10m, matchLink.AP_Amount);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"MatchLink with linked transaction in local cache, linked transaction is in DB and has no changes with total amount and outstanding amount both zero but matchlink amount is not zero.",
				true,
				CriticalValidationErrorType.MatchLinkLinkedTransactionHasNotChanged_1,
				CriticalValidationMessageTemplate.MatchLinkLinkedTransactionHasNotChanged,
				"Match Link: Group Number =",
				"Linked Transaction Info :",
				"Header: PK");

			AssertOnSavingCheck(matchLink, testCase);
		}

		public void TestLinkedTransactionHasNotChanged_TransactionInLocalCacheAndInDatabaseAndHasNotChangedWithOutstandingAmountIsZeroAndTotalAmountIsNotZero()
		{
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_InvoiceAmount = 10M;

			var matchLink = Factory.New<AccTransactionMatchLink>();
			matchLink.AP_Amount = 10M;
			matchLink.AP_AH = header.PK;
			matchLink.AP_MatchDate = ZDate.Today;
			matchLink.AP_MatchGroupNum = "1234567890";
			new MatchLinkGroupForTest(Factory).Add(matchLink);
			Factory.Save();

			var testLink2 = Factory.New<AccTransactionMatchLink>();
			testLink2.AP_MatchGroupNum = "1234567891";
			testLink2.AP_Amount = 0m;
			testLink2.AP_AH = header.PK;
			testLink2.AP_MatchDate = ZDate.Today;

			var group2 = new MatchLinkGroupForTest(Factory);
			group2.Add(testLink2);

			Assert("Precondtion - Transaction is in DB.", header.IsInDatabase);
			AssertNotEquals("Precondtion - Transaction has no changes", System.Data.DataRowState.Modified, ((INeedRow)header).Row.RowState);
			AssertEquals("Precondtion - Transaction outstanding amount is zero", 0m, header.AH_OutstandingAmount);
			AssertEquals("Precondtion - Transaction invoice amount is not zero", 10m, header.AH_InvoiceAmount);
			AssertEquals("Precondtion - Transaction tax amount is zero", 0m, header.AH_GSTAmount);
			AssertEquals("Precondtion - Match link amount is zero", 0m, testLink2.AP_Amount);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"MatchLink with linked transaction in local cache, linked transaction is in DB and has no changes with zero outstanding amount but total amount is not zero.",
				true,
				CriticalValidationErrorType.MatchLinkLinkedTransactionHasNotChanged_1,
				CriticalValidationMessageTemplate.MatchLinkLinkedTransactionHasNotChanged,
				"Match Link: Group Number =",
				"Linked Transaction Info :",
				"Header: PK");

			AssertOnSavingCheck(testLink2, testCase);
		}

		public void TestLinkedTransactionHasNotChanged_TransactionInLocalCacheAndInDatabaseAndHasChanged()
		{
			var matchLink = GetGroupMemberWithGroupNumberMatchLink(Factory);
			Factory.Save();
			matchLink.TransactionHeader.AH_OutstandingAmount = 20m;

			Assert("Precondtion - Transaction is in DB.", matchLink.TransactionHeader.IsInDatabase);
			AssertEquals("Precondtion - Transaction has changes", System.Data.DataRowState.Modified, ((INeedRow)matchLink.TransactionHeader).Row.RowState);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("MatchLink with linked transaction in local cache, linked transaction is in DB and has changes.",
																			false,
																			CriticalValidationErrorType.NoError,
																			string.Empty);
			AssertOnSavingCheck(matchLink, testCase);
		}

		public void TestMatchLinkIsNotAMemberOfMatchGroup()
		{
			var matchLink = GetStandaloneMatchLink(Factory);
			var developerMessage = GetMatchLinkDeveloperMessage(matchLink);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Standalone MatchLink",
																			 true,
																			 CriticalValidationErrorType.MatchLinkIsNotAMemberOfMatchGroup_1,
																			 "The Match Link should always be a member of Match Link Group.",
																			 developerMessage);
			AssertOnSavingCheck(matchLink, testCase);

			matchLink = GetCollectionMemberMatchLink(Factory);
			developerMessage = GetMatchLinkDeveloperMessage(matchLink);
			testCase = new TestCaseDefinition_ForSeparateTestsMethods("MatchLink as a member of Collection but not a Group",
																			 true,
																			 CriticalValidationErrorType.MatchLinkIsNotAMemberOfMatchGroup_1,
																			 "The Match Link should always be a member of Match Link Group.",
																			 developerMessage);
			AssertOnSavingCheck(matchLink, testCase);
		}

		public void TestMatchLinkWithoutGroupNumber()
		{
			var matchLink = GetGroupMemberWithoutGroupNumberMatchLink(Factory);
			var developerMessage = GetMatchLinkDeveloperMessage(matchLink);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("MatchLink as a member of Group buth without GroupNumber",
																			 true,
																			 CriticalValidationErrorType.MatchLinkWithoutGroupNumber_1,
																			 "All Match Links should have Group Number assigned.",
																			 developerMessage);
			AssertOnSavingCheck(matchLink, testCase);
		}

		public void TestMatchLinkWithoutError()
		{
			var matchLink = GetGroupMemberWithGroupNumberMatchLink(Factory);
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("MatchLink as a member of Group with GroupNumber",
																			 false,
																			 CriticalValidationErrorType.NoError, string.Empty);
			AssertOnSavingCheck(matchLink, testCase);
		}

		public void TestSavedMachLinkCannotBeModified()
		{
			var matchLink = GetSavedThenModifiedMatchLink(Factory);
			var developerMessage = GetMatchLinkDeveloperMessage(matchLink) + "\r\n	Fields with changes: AP_Amount (10, 100).";

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Attempt to modify a previously saved MatchLink",
																			 true,
																			 CriticalValidationErrorType.SavedMachLinkCannotBeModified_1,
																			 "The Match Link already saved in database should not be modified.",
																			 developerMessage);
			AssertOnSavingCheck(matchLink, testCase);
		}

		string GetMatchLinkDeveloperMessage(AccTransactionMatchLink matchLink)
		{
			return string.Format("Match Link: Group Number = {0}, Amount = {1}, OS Amount = {2}, Match Date = {3}, Transaction PK = {4}, Is In DB = {5}, Has Changes = {6}.",
				matchLink.AP_MatchGroupNum, matchLink.AP_Amount, matchLink.AP_OSAmount, matchLink.AP_MatchDate.ToAUString(), matchLink.AP_AH, matchLink.IsInDatabase.ToYesNoString(), matchLink.HasChanges.ToYesNoString());
		}

		AccTransactionMatchLink GetStandaloneMatchLink(BusinessObjectFactory factory)
		{
			AccTransactionHeader header = factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_InvoiceAmount = 10m;

			AccTransactionMatchLink testLink = factory.New<AccTransactionMatchLink>();
			testLink.AP_Amount = 10m;
			testLink.AP_AH = header.PK;
			testLink.AP_MatchDate = ZDate.Today;
			AssertEquals("Should have no RowErrors", false, testLink.HasRowErrors);

			return testLink;
		}

		AccTransactionMatchLink GetCollectionMemberMatchLink(BusinessObjectFactory factory)
		{
			AccTransactionMatchLink testLink = GetStandaloneMatchLink(factory);
			testLink.AP_MatchGroupNum = "1234567890";

			MatchLinkCollectionForTest collection = new MatchLinkCollectionForTest(factory);
			collection.Add(testLink);

			return testLink;
		}

		AccTransactionMatchLink GetGroupMemberWithoutGroupNumberMatchLink(BusinessObjectFactory factory)
		{
			AccTransactionMatchLink testLink = GetStandaloneMatchLink(factory);

			MatchLinkGroupForTest group = new MatchLinkGroupForTest(factory);
			group.Add(testLink);

			return testLink;
		}

		AccTransactionMatchLink GetGroupMemberWithGroupNumberMatchLink(BusinessObjectFactory factory)
		{
			AccTransactionMatchLink testLink = GetStandaloneMatchLink(factory);
			testLink.AP_MatchGroupNum = "1234567890";

			MatchLinkGroupForTest group = new MatchLinkGroupForTest(factory);
			group.Add(testLink);

			return testLink;
		}

		AccTransactionMatchLink GetSavedThenModifiedMatchLink(BusinessObjectFactory factory)
		{
			AccTransactionMatchLink testLink = GetGroupMemberWithGroupNumberMatchLink(factory);
			factory.Save();
			testLink.AP_Amount = 100M;
			testLink.TransactionHeader.AH_OutstandingAmount = 20m;

			return testLink;
		}

		#region Implementation

		protected class MatchLinkCollectionForTest : BusinessObjectCollection<AccTransactionMatchLink>
		{
			public MatchLinkCollectionForTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}

		internal class MatchLinkGroupForTest : BusinessObjectCollection<AccTransactionMatchLink>, ISupportCriticalValidation
		{
			public MatchLinkGroupForTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			ICriticalValidation ISupportCriticalValidation.CriticalValidation
			{
				get { return new DummyCriticalValidation(); }
			}

			void IConflictWithCriticalFields.SetConflictWithCriticalFieldsBusinessContext()
			{
				throw new NotImplementedException();
			}
		}

		protected class DummyCriticalValidation : ICriticalValidation
		{
			void ICriticalValidation.RegisterOnSavingCheck()
			{
			}

			void ICriticalValidation.RunOnSavingCheck()
			{
			}

			void ICriticalValidation.RunDeletedObjectOnSavingCheck()
			{
			}

			public void RunAfterSavingCheck()
			{
			}
		}

		#endregion

	}
}
