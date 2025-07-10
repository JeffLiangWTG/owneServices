using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business
{
	[TestedType(typeof(WebUserEditableNote))]
	sealed class WebUserEditableNoteTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			WebUserEditableNote note = new WebUserEditableNote(TestBizOWithEditableNote, PredefinedNoteTypes.Instance.SpecialInstructions);
			AssertEquals("Parent", TestBizOWithEditableNote.PK, note.parent.NotesParentBO.NotesParentPK);
			AssertEquals("NoteType", PredefinedNoteTypes.Instance.SpecialInstructions, note.EditableNoteType);
		}

		public void TestHasChanges()
		{
			Assert(!TestEditableNote.HasChanges);
			TestEditableNote.EditableNoteText = "text";
			Assert(TestEditableNote.HasChanges);
		}

		public void TestMultiLineTextbox()
		{
			TestEditableNote.EditableNoteText = @"Line1
Line2
Line3
Line4";

			Factory.Save();
			AssertEquals("EditableNoteText should be as assigned", "Line1\r\nLine2\r\nLine3\r\nLine4", TestEditableNote.EditableNoteText);
		}

		public void TestEditableNoteNonReadOnlyAfterAdd()
		{
			AssertEquals("EditableNoteText should be empty", ZString.Empty, TestEditableNote.EditableNoteText);
			AssertEquals("FullNoteText should be empty", ZString.Empty, TestEditableNote.FullNoteText);
			AssertNull("No StmNote", TestEditableNote.FindEditableNote());

			TestEditableNote.EditableNoteText = "Test User Note";

			AssertEquals("EditableNoteText should be as assigned", "Test User Note", TestEditableNote.EditableNoteText);
			StmNote note = TestEditableNote.FindEditableNote();
			AssertNotNull("StmNote should exist", note);
			AssertEquals("EditableNoteText should be the same as ST_NoteText", note.ST_NoteText, TestEditableNote.EditableNoteText);
			AssertEquals("FullNoteText should be the same as ST_NoteText", note.ST_NoteText, TestEditableNote.FullNoteText);
			AssertEquals("IsNoteAdded should be True", true, TestEditableNote.IsNoteAdded);
			AssertEquals("IsNoteChanged should be False", false, TestEditableNote.IsNoteChanged);

			Factory.Save();

			AssertEquals("IsNoteAdded should be False after saving", false, TestEditableNote.IsNoteAdded);
			AssertEquals("IsNoteChanged should be False after saving", false, TestEditableNote.IsNoteChanged);

			TestEditableNote.EditableNoteText = "Test User Note Changed";

			AssertEquals("EditableNoteText should be as assigned", "Test User Note Changed", TestEditableNote.EditableNoteText);
			StmNote note1 = TestEditableNote.FindEditableNote();
			AssertNotNull("StmNote should exist", note1);
			AssertEquals("Should be the same StmNote", note, note1);
			AssertEquals("EditableNoteText should be the same as ST_NoteText", note1.ST_NoteText, TestEditableNote.EditableNoteText);
			AssertEquals("FullNoteText should be the same as ST_NoteText", note1.ST_NoteText, TestEditableNote.FullNoteText);
			AssertEquals("IsNoteAdded should be False", false, TestEditableNote.IsNoteAdded);
			AssertEquals("IsNoteChanged should be True", true, TestEditableNote.IsNoteChanged);
		}

		[HttpContextEnabledTest]
		[TestDate(2008, 12, 15, 16, 32, 0)]
		public void TestEditableNoteReadOnlyAfterAdd()
		{
			ZWebTestHelper helper = new ZWebTestHelper(Factory);
			TrackingSiteUser siteUser = new TrackingSiteUser();
			Factory.Save();
			siteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);

			MyTestBusinessObjectWithReadOnlyAfterAddNote editableNoteSupport = Factory.New<MyTestBusinessObjectWithReadOnlyAfterAddNote>();
			WebUserEditableNote editableNote = editableNoteSupport.UserEditableNoteHelper;
			AssertEquals("EditableNoteText should be empty", ZString.Empty, editableNote.EditableNoteText);
			AssertEquals("FullNoteText should be empty", ZString.Empty, editableNote.FullNoteText);
			AssertNull("No StmNote", editableNote.FindEditableNote());

			editableNote.EditableNoteText = "Test User Note";

			AssertEquals("EditableNoteText should be empty after assignment", string.Empty, editableNote.EditableNoteText);
			StmNote note = editableNote.FindEditableNote();
			AssertNotNull("StmNote should exist", note);
			string expectedNoteText = string.Format("15-Dec-08 16:32 ({0}) Test User Note", helper.TestContact.OC_ContactName);
			AssertEquals("ST_NoteText should be as expectedt", expectedNoteText, note.ST_NoteText);
			AssertEquals("FullNoteText should be the same as ST_NoteText", note.ST_NoteText, editableNote.FullNoteText);
			AssertEquals("IsNoteAdded should be True", true, editableNote.IsNoteAdded);
			AssertEquals("IsNoteChanged should be False", false, editableNote.IsNoteChanged);

			Factory.Save();

			AssertEquals("IsNoteAdded should be False after saving", false, editableNote.IsNoteAdded);
			AssertEquals("IsNoteChanged should be False after saving", false, editableNote.IsNoteChanged);

			editableNote.EditableNoteText = "Test User Note Changed";

			AssertEquals("EditableNoteText should be empty after assignment", string.Empty, editableNote.EditableNoteText);
			StmNote note1 = editableNote.FindEditableNote();
			AssertNotNull("StmNote should exist", note1);
			AssertEquals("Should be the same StmNote", note.PK, note1.PK);
			expectedNoteText = string.Format("15-Dec-08 16:32 ({0}) Test User Note Changed\r\n\r\n{1}", helper.TestContact.OC_ContactName, expectedNoteText);
			AssertEquals("ST_NoteText should be as expected", expectedNoteText, note1.ST_NoteText);
			AssertEquals("FullNoteText should be the same as ST_NoteText", note1.ST_NoteText, editableNote.FullNoteText);
			AssertEquals("IsNoteAdded should be False", false, editableNote.IsNoteAdded);
			AssertEquals("IsNoteChanged should be True", true, editableNote.IsNoteChanged);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return TestBizOWithEditableNote.UserEditableNoteHelper;
		}

		WebUserEditableNote TestEditableNote
		{
			get
			{
				if (fTestEditableNote == null)
				{
					fTestEditableNote = GetNewBusinessObject() as WebUserEditableNote;
				}
				return fTestEditableNote;
			}
		}
		WebUserEditableNote fTestEditableNote;

		MyTestBusinessObject TestBizOWithEditableNote
		{
			get
			{
				if (fTestBizOWithEditableNote == null)
				{
					fTestBizOWithEditableNote = Factory.New<MyTestBusinessObject>();
				}
				return fTestBizOWithEditableNote;
			}
		}
		MyTestBusinessObject fTestBizOWithEditableNote;

		#region Test class

		public class MyTestBusinessObject : DummyEnterpriseBusinessObject, IWebUserEditableNoteSupport
		{
			public MyTestBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public IStmNoteParent NotesParentBO
			{
				get { return this; }
			}

			public WebUserEditableNote UserEditableNoteHelper
			{
				get { return fUserEditableNoteHelper ?? (fUserEditableNoteHelper = GetNewNoteHelper()); }
			}
			WebUserEditableNote fUserEditableNoteHelper;

			protected virtual WebUserEditableNote GetNewNoteHelper()
			{
				return new WebUserEditableNote(this, PredefinedNoteTypes.Instance.SpecialInstructions);
			}

			public bool IsWebUserEditableNoteAdded
			{
				get { return fIsWebUserEditableNoteAdded; }
			}
			public bool fIsWebUserEditableNoteAdded;

			public bool IsWebUserEditableNoteChanged
			{
				get { return fIsWebUserEditableNoteChanged; }
			}
			public bool fIsWebUserEditableNoteChanged;

			public void ResetEmailNotificationStatusAfterSaving()
			{
				fIsWebUserEditableNoteAdded = false;
				fIsWebUserEditableNoteChanged = false;
			}
		}

		public class MyTestBusinessObjectWithReadOnlyAfterAddNote : MyTestBusinessObject
		{
			public MyTestBusinessObjectWithReadOnlyAfterAddNote(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override WebUserEditableNote GetNewNoteHelper()
			{
				return new WebUserEditableNote(this, PredefinedNoteTypes.Instance.ContainerReleaseNote);
			}
		}

		#endregion

		#endregion
	}
}
