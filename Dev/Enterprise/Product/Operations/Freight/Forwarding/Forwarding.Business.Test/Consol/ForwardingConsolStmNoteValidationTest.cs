using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	public class ForwardingConsolStmNoteValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateOriginalBillNotesIsSystemOnly()
		{
			var note = Factory.New<ForwardingConsolStmNote>();

			note.ST_Description = PredefinedNoteTypes.Instance.OriginalBillNotes.Description;
			AssertHasError(note.ST_DescriptionInfo, "Original Bill Notes is reserved for system use and cannot be manually entered.");

			note.ST_Description = "original bill notes";
			AssertHasError(note.ST_DescriptionInfo, "Original Bill Notes is reserved for system use and cannot be manually entered.");
		}

		public void TestSkipValidationWhenOriginalBillNotesIsAlreadyInDatabase()
		{
			var consol = Factory.New<ForwardingConsol>();

			var note = consol.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.OriginalBillNotes.Description;
			note.ST_NoteText = "test";

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var noteInOtherFactory = otherFactory.Load<ForwardingConsolStmNote>(note.PK);
			noteInOtherFactory.Validation.ValidateST_Description();

			Assert(noteInOtherFactory.IsInDatabase);
			AssertNoErrors(noteInOtherFactory.ST_DescriptionInfo);
		}

		public void TestShouldSkipValidateDescriptionIsUnique()
		{
			var consol = Factory.New<ForwardingConsol>();

			var note = consol.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.OriginalBillNotes.Description;
			note.ST_NoteText = "test";

			Factory.Save();

			var note2 = consol.Notes.AddNew();
			note2.ST_Description = PredefinedNoteTypes.Instance.OriginalBillNotes.Description;
			AssertHasError(note.ST_DescriptionInfo, "Original Bill Notes is reserved for system use and cannot be manually entered.");
			AssertEquals("There should only be one error message here.", 1, note.ST_DescriptionInfo.Notifications.Count());
		}
	}
}
