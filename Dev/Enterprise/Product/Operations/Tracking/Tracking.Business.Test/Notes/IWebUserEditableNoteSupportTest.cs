using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Tracking.Business.Testing
{
	public abstract class IWebUserEditableNoteSupportTest : TestCaseWithFactory
	{
		public void TestInterfacePropertiesAreNotNull()
		{
			AssertNotNull("NotesParentBO", BizObj.NotesParentBO);
			AssertNotNull("UserEditableNoteHelper", BizObj.UserEditableNoteHelper);
		}

		public void TestUserEditableNoteIsEmptyByDefault()
		{
			Assert("EditableNote should be Empty", BizObj.UserEditableNoteHelper.EditableNoteText.IsEmpty);
		}

		public void TestNotesParentBO()
		{
			AssertNotNull("Expected Notes Parent", ExpectedNotesParentBO);
			AssertEquals("NotesParentBO should be as expected", ExpectedNotesParentBO.PK, BizObj.NotesParentBO.NotesParentPK);
		}

		protected virtual BusinessObject ExpectedNotesParentBO
		{
			get { return BizObj as BusinessObject; }
		}

		public void TestUserEditableNoteAddsNoteWhenTextAssigned()
		{
			int notesCount = BizObjNotes.GetAllNotes().Count;
			Assert("EditableNote should be Empty", BizObj.UserEditableNoteHelper.EditableNoteText.IsEmpty);

			BizObj.UserEditableNoteHelper.EditableNoteText = "Web User Editable Note";
			int notesCount1 = BizObjNotes.GetAllNotes().Count;
			AssertEquals("New note added", notesCount + 1, notesCount1);

			BizObj.UserEditableNoteHelper.EditableNoteText = ZString.Empty;
			int notesCount2 = BizObjNotes.GetAllNotes().Count;
			AssertEquals("New note still there", notesCount1, notesCount2);
		}

		#region Setup

		protected Notes BizObjNotes;
		protected IWebUserEditableNoteSupport BizObj;

		protected override void SetUp()
		{
			base.SetUp();
			BizObj = GetNewBusinessObject();
			BizObjNotes = BizObj.NotesParentBO.Notes;
		}

		protected abstract IWebUserEditableNoteSupport GetNewBusinessObject();

		#endregion
	}
}
