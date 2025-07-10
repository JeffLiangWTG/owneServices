using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccComplianceSequenceSplitViewModel))]
	sealed class AccComplianceSequenceSplitModelTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSequences()
		{
			AssertEquals("Sequences should have new book & exist book", 2, model.Sequences.Count);
			var existBook = model.Sequences.Single(x => x.PK.Equals(sequence.PK));
			var newBook = model.Sequences.Single(x => !x.PK.Equals(sequence.PK));
			AssertEquals(false, existBook.IsNewBook);
			AssertEquals(true, newBook.IsNewBook);

			AssertEquals("Assert book initialization", existBook.XD_SequenceClass, newBook.XD_SequenceClass);
			AssertEquals(existBook.XD_Description, newBook.XD_Description);
			AssertEquals(existBook.XD_GC_Company, newBook.XD_GC_Company);
			AssertEquals(existBook.XD_AllocationLevel, newBook.XD_AllocationLevel);
			AssertEquals(existBook.XD_GB_BranchOwner, newBook.XD_GB_BranchOwner);
			AssertEquals(existBook.XD_GE_Department, newBook.XD_GE_Department);
			AssertEquals(existBook.XD_Prefix, newBook.XD_Prefix);
			AssertEquals(ZDecimal.Zero, newBook.XD_StartNumber);
			AssertEquals(existBook.OriginalEndNumber, newBook.XD_EndNumber);
			AssertEquals(ZDecimal.Zero, newBook.XD_NextNumber);
			AssertEquals(ZDate.Empty, newBook.XD_StartDate);
			AssertEquals(existBook.OriginalExpiryDate, newBook.XD_ExpiryDate);
			AssertEquals(existBook.XD_MaximumNumberDigits, newBook.XD_MaximumNumberDigits);
			AssertEquals(existBook.XD_SU_MenuItem, newBook.XD_SU_MenuItem);
			AssertEquals(existBook.XD_MaxChargesPerTransaction, newBook.XD_MaxChargesPerTransaction);
			AssertEquals(existBook.XD_RollupBehaviourWhenMaxExceeded, newBook.XD_RollupBehaviourWhenMaxExceeded);
			AssertEquals(existBook.XD_SO_ComplianceTemplate, newBook.XD_SO_ComplianceTemplate);
			AssertEquals(existBook.XD_SQ_DocumentPrintQueue, newBook.XD_SQ_DocumentPrintQueue);
			AssertEquals(existBook.XD_NumberFormat, newBook.XD_NumberFormat);

			AssertEquals(ZDecimal.Zero, existBook.XD_EndNumber);
			AssertEquals(ZDate.Empty, existBook.XD_ExpiryDate);

			AssertEquals(model, existBook.SplitController);
			AssertEquals(100m, existBook.OriginalEndNumber);
			AssertEquals(new ZDateTime(2018, 10, 5), existBook.OriginalExpiryDate);

			AssertEquals(model, newBook.SplitController);
			AssertEquals(100m, newBook.OriginalEndNumber);
			AssertEquals(new ZDateTime(2018, 10, 5), newBook.OriginalExpiryDate);
		}

		public void TestPropertiesOfNewSequenceBeEvaluatedWhenInitialize()
		{
			var listInclude = new[] { "XD_AllocationLevel", "XD_Description", "XD_EndNumber", "XD_ExpiryDate", "XD_GB_BranchOwner", "XD_GC_Company", "XD_GE_Department", "XD_MaxChargesPerTransaction", "XD_MaximumNumberDigits", "XD_NextNumber", "XD_Prefix", "XD_RollupBehaviourWhenMaxExceeded", "XD_SequenceClass", "XD_SO_ComplianceTemplate", "XD_SQ_DocumentPrintQueue", "XD_StartDate", "XD_StartNumber", "XD_SU_MenuItem", "XD_PrintingAuthorizationNumber", "XD_NumberFormat" };
			var listExclude = new[] { "XD_Code", "XD_IsActive", "XD_LockBy", "XD_PermanentDisableTimeUtc", "XD_SystemCreateTimeUtc", "XD_SystemCreateUser", "XD_SystemCreateBranch", "XD_SystemCreateDepartment", "XD_SystemLastEditTimeUtc", "XD_SystemLastEditUser" };
			var propertyNames = typeof(AccComplianceSequenceForSplit).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Select(x => x.Name);

			var propertiesNotEvaluated = propertyNames.Where(x => x.StartsWith("XD_") && !x.StartsWith("XD_Calc_") && !x.EndsWith("Info") && !x.EndsWith("ForTestOnly") && !listInclude.Contains(x) && !listExclude.Contains(x));
			AssertEquals(@"Some of the new sequence properties are not evaluated in 'void Initialize(AccComplianceSequenceForSplit existBook, AccComplianceSequenceForSplit newBook)'.
Please add evaluation logic in Initialize method, also update 'listInclude' and 'listExclude' in this UT. 
Those properties are: " + new ZStringBuilder(propertiesNotEvaluated).ToStringWithDelimiterBetweenAppends(", "), 0, propertiesNotEvaluated.Count());
		}

		public void TestSequencesWithInvalidSequencePK()
		{
			var invalidModel = new AccComplianceSequenceSplitViewModel(Factory, ZGuid.Empty);
			AssertEquals(0, invalidModel.Sequences.Count);
		}

		public void TestUpdateNewSequenceStartNumber()
		{
			var existBook = model.Sequences.Single(x => x.PK.Equals(sequence.PK));
			var newBook = model.Sequences.Single(x => !x.PK.Equals(sequence.PK));

			AssertEquals("Precondition", ZDecimal.Zero, existBook.XD_EndNumber);
			AssertEquals(ZDecimal.Zero, newBook.XD_StartNumber);

			existBook.XD_EndNumber = 20;

			AssertEquals("When existBook.endNumber is updated, newBook.startNumber should be updated to exist.endNumber + 1",
						 20m, existBook.XD_EndNumber);
			AssertEquals(21m, newBook.XD_StartNumber);
			AssertEquals(21m, newBook.XD_NextNumber);

			existBook.XD_EndNumber = Decimal.Zero;

			AssertEquals("When existBook.endNumber is ZERO, newBook.startNumber should be updated to exist.endNumber",
						 ZDecimal.Zero, existBook.XD_EndNumber);
			AssertEquals(ZDecimal.Zero, newBook.XD_StartNumber);
			AssertEquals(ZDecimal.Zero, newBook.XD_NextNumber);
		}

		public void TestUpdateNewSequenceStartDate()
		{
			var existBook = model.Sequences.Single(x => x.PK.Equals(sequence.PK));
			var newBook = model.Sequences.Single(x => !x.PK.Equals(sequence.PK));

			var oldDate = new ZDateTime(2018, 10, 5);
			var newDate = new ZDateTime(2018, 6, 30);

			AssertEquals("Precondition", ZDate.Empty, existBook.XD_ExpiryDate);
			AssertEquals(ZDate.Empty, newBook.XD_StartDate);

			existBook.XD_ExpiryDate = newDate;

			AssertEquals("When existBook.expiryDate is updated, newBook.startDate should be updated to exist.expiryDate + 1",
						 newDate, existBook.XD_ExpiryDate);
			AssertEquals(newDate.AddDays(1), newBook.XD_StartDate);

			existBook.XD_ExpiryDate = ZDateTime.Empty;

			AssertEquals("When existBook.expiryDate is empty, newBook.startDate should be updated to empty date",
						 ZDateTime.Empty, existBook.XD_ExpiryDate);
			AssertEquals(ZDate.Empty, newBook.XD_StartDate);
		}

		public void TestGetExistSequencePK()
		{
			var collection = model.Sequences;
			AssertEquals(sequence.PK, (model as ISplitComplianceSequenceController).ExistSequence.PK);
		}

		public void TestEventRecords()
		{
			var existBook = model.Sequences.Single(x => x.PK.Equals(sequence.PK));
			var newBook = model.Sequences.Single(x => !x.PK.Equals(sequence.PK));

			existBook.XD_ExpiryDate = existBook.OriginalExpiryDate.AddDays(-5);
			existBook.XD_EndNumber = existBook.OriginalEndNumber - 5;
			newBook.XD_Code = "CS2";

			AssertEquals("Precondition", false, newBook.IsInDatabase);
			Factory.Save();

			AssertEquals("Events should be recorded when first time save the new sequence.",
						 1, existBook.Logs.Find(x => x.SL_Reference == "Book split into CS2 with End Number = 000000095, Expiry Date = 30-Sep-18" && x.SL_SE_NKEvent == Events.EditedARecord.Code).Count());
			AssertEquals(1, newBook.Logs.Find(x => x.SL_Reference == "Book split from CS1 with Start Number = 000000096, Valid From Date = 01-Oct-18" && x.SL_SE_NKEvent == Events.EditedARecord.Code).Count());

			existBook.XD_EndNumber = existBook.XD_EndNumber - 1;

			AssertEquals("Precondition", true, newBook.IsInDatabase);
			Factory.Save();

			AssertEquals("Modify the saved sequence should NOT add new split event",
						 0, existBook.Logs.Find(x => x.SL_Reference == "Book split into CS2 with End Number = 000000094, Expiry Date = 30-Sep-18" && x.SL_SE_NKEvent == Events.EditedARecord.Code).Count());
			AssertEquals(0, newBook.Logs.Find(x => x.SL_Reference == "Book split from CS1 with Start Number = 000000095, Valid From Date = 01-Oct-18" && x.SL_SE_NKEvent == Events.EditedARecord.Code).Count());
			AssertEquals(1, existBook.Logs.Find(x => x.SL_Reference.StartsWith("Book split into")).Count());
			AssertEquals(1, newBook.Logs.Find(x => x.SL_Reference.StartsWith("Book split from")).Count());
		}

		#region Overrides

		protected override void SetUp()
		{
			base.SetUp();

			branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			branch.GB_Code = "AAA";

			sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_SequenceClass = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;
			sequence.XD_AllocationLevel = ComplianceBookAllocationLevel.Branch;
			sequence.XD_Code = "CS1";
			sequence.XD_StartNumber = 1;
			sequence.XD_EndNumber = 100;
			sequence.XD_NextNumber = 1;
			sequence.XD_MaximumNumberDigits = 9;
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			sequence.XD_GB_BranchOwner = branch.PK;
			sequence.XD_StartDate = new ZDate(2018, 5, 20);
			sequence.XD_ExpiryDate = new ZDateTime(2018, 10, 5);

			Factory.Save();

			model = new AccComplianceSequenceSplitViewModel(Factory, sequence.PK);
		}

		#endregion

		AccComplianceSequence sequence;
		AccComplianceSequenceSplitViewModel model;
		GlbBranch branch;
	}
}
