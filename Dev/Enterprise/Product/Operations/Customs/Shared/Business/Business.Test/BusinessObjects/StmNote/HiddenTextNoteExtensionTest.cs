using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class HiddenTextNoteExtensionTest : TestCaseWithFactory
	{
		public void TestNoExceptionsForNullValidatior()
		{
			var testHiddenMarksAndNumbersNote = new HiddenTextNote(declaration, PredefinedNoteTypes.Instance.MarksAndNumbers.Description);
			AssertNoExceptionThrown(() => testHiddenMarksAndNumbersNote.SetNoteText(declaration, declaration.JE_MarksAndNumbersInfo, "TEST NOTE TEXT FOR SET EXTENSION", null));
		}

		public void TestSetNoteText()
		{
			Assert(!declaration.HasChanges);
			var testHiddenMarksAndNumbersNote = new HiddenTextNote(declaration, PredefinedNoteTypes.Instance.MarksAndNumbers.Description);
			testHiddenMarksAndNumbersNote.SetNoteText(declaration, declaration.JE_MarksAndNumbersInfo, "TEST NOTE TEXT FOR SET EXTENSION");
			Assert(declaration.HasChanges);
			AssertEquals("TEST NOTE TEXT FOR SET EXTENSION", testHiddenMarksAndNumbersNote.Text);
			testHiddenMarksAndNumbersNote.SetNoteText(declaration, declaration.JE_MarksAndNumbersInfo, ZString.Empty);
			Assert(testHiddenMarksAndNumbersNote.Text.IsEmpty);
			Factory.Save();
			AssertEquals(0, Factory.GetDatabaseCount(typeof(StmNote), HiddenNoteQuery));
			testHiddenMarksAndNumbersNote.SetNoteText(declaration, declaration.JE_MarksAndNumbersInfo, "TEST TO CONFIRM PERSISTENCE");
			Factory.Save();
			AssertEquals(1, Factory.GetDatabaseCount(typeof(StmNote), HiddenNoteQuery));
		}

		public void TestDeleteHiddenNotes()
		{
			var testHiddenMarksAndNumbersNote = new HiddenTextNote(declaration, PredefinedNoteTypes.Instance.MarksAndNumbers.Description);
			testHiddenMarksAndNumbersNote.SetNoteText(declaration, declaration.JE_MarksAndNumbersInfo, "TEST MARKS AND NUMBERS");
			var testHiddenTradersRemarksNote = new HiddenTextNote(declaration, PredefinedNoteTypes.Instance.SGTradersRemarks.Description);
			testHiddenTradersRemarksNote.SetNoteText(declaration, declaration.JE_MarksAndNumbersInfo, "TEST SG TRADERS REMARKS");
			Factory.Save();
			AssertEquals(2, Factory.GetDatabaseCount(typeof(StmNote), HiddenNoteQuery));
			declaration.DeleteHiddenNotes();
			Factory.Save();
			AssertEquals(0, Factory.GetDatabaseCount(typeof(StmNote), HiddenNoteQuery));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<BaseJobDeclaration>();
		}
		BaseJobDeclaration declaration;

		ZQuery HiddenNoteQuery
		{
			get
			{
				var query = new ZQuery(StmNoteSchema.ST_ParentID, declaration.PK);
				query.AddToFilter(StmNoteSchema.ST_Table, declaration.TableName);
				query.AddToFilter(StmNoteSchema.ST_NoteType, nameof(StmNoteVisibility.DOC));
				return query;
			}
		}
	}
}
