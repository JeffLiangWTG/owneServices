using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.Business.Testing
{
	public class EntryInstructionProviderTest : TestCaseWithFactory
	{
		public void TestNoEntryInstructionProvider()
		{
			var testProvider = EntryInstructionProvider.NoEntryInstructionProvider;
			Assert(testProvider.IsNoEntryInstruction);
			AssertNull(testProvider.CustomsEntryInstructions);
			AssertNull(testProvider.EntryInstructionComparer);
			AssertEquals(0, testProvider.SortedEntryInstructionList.Count);
			AssertNoExceptionThrown(() => { testProvider.RefreshSortedEntryInstructionList(); });
			AssertNoExceptionThrown(() => { testProvider.DeleteAll(); });
		}

		public void TestIsNoEntryInstruction()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var testProvider = new EntryInstructionProvider(declaration);
			Assert(!testProvider.IsNoEntryInstruction);
		}

		public void TestDefaultComparer()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var testProvider = new EntryInstructionProvider(declaration);
			AssertType<CusEntryInstructionComparer>(testProvider.EntryInstructionComparer);
		}

		public void TestConstructorComparer()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var comparer = new CusEntryInstructionComparer();
			var testProvider = new EntryInstructionProvider(declaration, comparer);
			AssertSame(comparer, testProvider.EntryInstructionComparer);
		}

		public void TestCustomsEntryInstructions()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var testInstruction1 = Factory.New<CusEntryInstruction>();
			testInstruction1.CEI_Style = "11";
			testInstruction1.CEI_JE = declaration.PK;
			var testInstruction2 = Factory.New<CusEntryInstruction>();
			testInstruction2.CEI_Style = "12";
			testInstruction2.CEI_JE = declaration.PK;

			var testProvider = new EntryInstructionProvider(declaration);
			AssertEquals(2, testProvider.CustomsEntryInstructions.Count);
			AssertEquals(1, testProvider.CustomsEntryInstructions.Count(a => a.PK == testInstruction1.PK));
			AssertEquals(1, testProvider.CustomsEntryInstructions.Count(a => a.PK == testInstruction2.PK));
		}

		public virtual void TestSortedEntryInstructionList()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var testInstruction1 = Factory.New<CusEntryInstruction>();
			testInstruction1.CEI_Style = "11";
			testInstruction1.CEI_JE = declaration.PK;
			var testInstruction2 = Factory.New<CusEntryInstruction>();
			testInstruction2.CEI_Style = "12";
			testInstruction2.CEI_JE = declaration.PK;

			var testProvider = new EntryInstructionProvider(declaration);
			AssertEquals(2, testProvider.SortedEntryInstructionList.Count);
			Assert(testProvider.SortedEntryInstructionList.ContainsCode("11"));
			Assert(testProvider.SortedEntryInstructionList.ContainsCode("12"));
		}

		public void TestRefreshSortedEntryInstructionList()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var testProvider = new EntryInstructionProvider(declaration);
			var testInstruction1 = testProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = "11";
			var testInstruction2 = testProvider.CustomsEntryInstructions.AddNew();
			testInstruction2.CEI_Style = "12";

			CombineAssertions("Start", () =>
			{
				AssertEquals(2, testProvider.SortedEntryInstructionList.Count);
				Assert(testProvider.SortedEntryInstructionList.ContainsCode("11"));
				Assert(testProvider.SortedEntryInstructionList.ContainsCode("12"));
			});
			var testInstruction3 = testProvider.CustomsEntryInstructions.AddNew();
			testInstruction3.CEI_Style = "13";
			CombineAssertions("without refresh", () =>
			{
				AssertEquals(2, testProvider.SortedEntryInstructionList.Count);
				Assert(testProvider.SortedEntryInstructionList.ContainsCode("11"));
				Assert(testProvider.SortedEntryInstructionList.ContainsCode("12"));
			});

			testProvider.RefreshSortedEntryInstructionList();
			CombineAssertions("with refresh", () =>
			{
				AssertEquals(3, testProvider.SortedEntryInstructionList.Count);
				Assert(testProvider.SortedEntryInstructionList.ContainsCode("11"));
				Assert(testProvider.SortedEntryInstructionList.ContainsCode("12"));
				Assert(testProvider.SortedEntryInstructionList.ContainsCode("13"));
			});
		}

		public void TestDeleteAll()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var testInstruction1 = Factory.New<CusEntryInstruction>();
			testInstruction1.CEI_Style = "11";
			testInstruction1.CEI_JE = declaration.PK;
			var testInstruction2 = Factory.New<CusEntryInstruction>();
			testInstruction2.CEI_Style = "12";
			testInstruction2.CEI_JE = declaration.PK;
			var testProvider = new EntryInstructionProvider(declaration);

			Assert(!testInstruction1.IsDeleted);
			Assert(!testInstruction2.IsDeleted);
			testProvider.DeleteAll();
			Assert(testInstruction1.IsDeleted);
			Assert(testInstruction2.IsDeleted);
		}

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new EntryInstructionProvider(null));
		}
	}
}
