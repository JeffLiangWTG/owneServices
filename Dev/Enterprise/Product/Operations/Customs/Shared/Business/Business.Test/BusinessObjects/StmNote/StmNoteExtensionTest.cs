using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(StmNoteExtension))]
	sealed class StmNoteExtensionTest : TestCaseWithFactory
	{
		public void TestGetNoteText()
		{
			CombineAssertions(() =>
			{
				AssertEquals("DatabaseCount", 0, declaration.Notes.DatabaseCount);
				AssertEquals("Note is null", ZString.Empty, declaration.Notes.GetNoteText(PredefinedNoteTypes.Instance.MarksAndNumbers.Description));

				_ = declaration.Notes.AddNew(isCustomDescription: false, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, "TEST NOTE TEXT FOR GET EXTENSION");
				AssertEquals("Note isn't null", "TEST NOTE TEXT FOR GET EXTENSION", declaration.Notes.GetNoteText(PredefinedNoteTypes.Instance.MarksAndNumbers.Description));
			});
		}

		public void TestSetNoteText()
		{
			CombineAssertions(() =>
			{
				declaration.Notes.SetNoteText(declaration, declaration.JE_MarksAndNumbersInfo, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, ZString.Empty);
				AssertEquals("Set to empty, parent.HasChanges is false", expected: false, declaration.HasChanges);
				AssertEquals("DatabaseCount is 0", 0, declaration.Notes.DatabaseCount);
				declaration.Notes.SetNoteText(declaration, declaration.JE_MarksAndNumbersInfo, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, "TEST NOTE TEXT FOR SET EXTENSION");
				AssertEquals("Set to non-empty, parent.HasChanges is true", expected: true, declaration.HasChanges);
				Factory.Save();
				AssertEquals("DatabaseCount is 1", 1, declaration.Notes.DatabaseCount);

				var marksAndNumbersNote = declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.MarksAndNumbers.Description).Single();
				AssertEquals("ST_NoteText", "TEST NOTE TEXT FOR SET EXTENSION", marksAndNumbersNote.ST_NoteText);
				AssertEquals("ST_NoteType", nameof(StmNoteVisibility.PUB), marksAndNumbersNote.ST_NoteType);

				declaration.Notes.SetNoteText(declaration, declaration.JE_MarksAndNumbersInfo, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, ZString.Empty);
				AssertEquals("Set from non-empty to empty, parent.HasChanges is true", expected: true, declaration.HasChanges);
				Factory.Save();
				AssertEquals("Note is deleted", 0, declaration.Notes.DatabaseCount);

				declaration.HasChanges = true;
				declaration.Notes.SetNoteText(declaration, declaration.JE_MarksAndNumbersInfo, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, ZString.Empty);
				AssertEquals("Set to old value, parent.HasChanges shouldn't reset", expected: true, declaration.HasChanges);
			});
		}

		public void TestCreateStmNoteForEdiMessage()
		{
			var messagePk = ZGuid.NewZGuid();
			Factory.CreateStmNoteForEdiMessage(messagePk, "Test Note Text");

			var query = new ZQuery(StmNoteSchema.ST_ParentID, messagePk);
			_ = query.AddToFilter(StmNoteSchema.ST_Table, EDIMessageSchema.Constants.TableName);
			var messageNote = Factory.LoadTop1<StmNote>(query);
			CombineAssertions(() =>
			{
				AssertEquals("ST_Table", expected: EDIMessageSchema.Constants.TableName, messageNote.ST_Table);
				AssertEquals("ST_IsCustomDescription", expected: true, messageNote.ST_IsCustomDescription);
				AssertEquals("ST_ForceRead", expected: true, messageNote.ST_ForceRead);
				AssertEquals("ST_NoteType", expected: nameof(StmNoteVisibility.INT), messageNote.ST_NoteType);
				AssertEquals("ST_NoteContext", expected: "AAA", messageNote.ST_NoteContext);
				AssertEquals("ST_Description", expected: "Processing Log", messageNote.ST_Description);
				AssertMultilineASCIIEquals("ST_NoteText", expected: "Test Note Text", messageNote.ST_NoteText);
			});
		}

		public void TestCreateStmNoteForEdiMessageWithEmptyNote()
		{
			var messagePk = ZGuid.NewZGuid();
			Factory.CreateStmNoteForEdiMessage(messagePk, ZString.Empty);

			var query = new ZQuery(StmNoteSchema.ST_ParentID, messagePk);
			_ = query.AddToFilter(StmNoteSchema.ST_Table, EDIMessageSchema.Constants.TableName);
			var messageNote = Factory.LoadTop1<StmNote>(query);
			AssertEquals("Should be empty", expected: null, messageNote);
		}

		public void TestCreateStmNote()
		{
			declaration.CreateStmNote("Test Note Text", "Test Description");

			CombineAssertions(() =>
			{
				var note = declaration.Notes.FindByDescription("Test Description").Single();
				AssertEquals("ST_Table", expected: JobDeclarationSchema.Constants.TableName, note.ST_Table);
				AssertEquals("ST_IsCustomDescription", expected: true, note.ST_IsCustomDescription);
				AssertEquals("ST_ForceRead", expected: true, note.ST_ForceRead);
				AssertEquals("ST_NoteType", expected: nameof(StmNoteVisibility.INT), note.ST_NoteType);
				AssertEquals("ST_NoteContext", expected: "AAA", note.ST_NoteContext);
				AssertEquals("ST_Description", expected: "Test Description", note.ST_Description);
				AssertMultilineASCIIEquals("ST_NoteText", expected: "Test Note Text", note.ST_NoteText);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<BaseJobDeclaration>();
		}
		BaseJobDeclaration declaration;
	}
}
