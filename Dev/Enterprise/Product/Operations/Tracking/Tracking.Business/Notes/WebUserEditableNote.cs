using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Business
{
	public class WebUserEditableNote : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string PK = "PK";
			public const string EditableNoteText = "EditableNoteText";
		}

		#endregion

		public WebUserEditableNote(IWebUserEditableNoteSupport parent, PredefinedNoteType editableNoteType)
			: base(((BusinessObject)parent.NotesParentBO).Factory)
		{
			this.parent = parent;
			this.editableNoteType = editableNoteType;
			Factory.Saved += new BusinessObjectFactory.SavedEventHandler(ResetNoteStatusAfterSaving);
			originalNoteText = FullNoteText;
		}

		public PredefinedNoteType EditableNoteType
		{
			get { return editableNoteType; }
		}

		StmNote EditableNote
		{
			get { return editableNote ?? (editableNote = FindEditableNote()); }
		}

		readonly PredefinedNoteType editableNoteType;
		internal readonly IWebUserEditableNoteSupport parent;
		StmNote editableNote;

		#region OriginalNoteText

		public ZString OriginalNoteText
		{
			get { return originalNoteText; }
		}

		readonly ZString originalNoteText;

		#endregion

		public ZString FullNoteText
		{
			get { return EditableNote != null ? EditableNote.ST_NoteText : ZString.Empty; }
		}

		#region EditableNoteText

		[CargoWise.ComponentModel.MaxLength(AutoStmNote.Schema.ST_NoteTextMaxLength)]
		public ZString EditableNoteText
		{
			get
			{
				return EditableNote != null && !EditableNoteType.IsReadOnlyAfterAdd ? EditableNote.ST_NoteText : ZString.Empty;
			}
			set
			{
				ZString note = EditableNoteType.IsReadOnlyAfterAdd ? ZString.Format("{0} ({1}) {2}", ZDateTime.Now.ToLongTimeString(), WebEnv.AppInstance.SiteUser.LoggedInUserName, value) : value;

				if (EditableNote == null && !value.Trim().IsEmpty)
				{
					CreateNewEditableNote(note);
				}
				else if (EditableNote != null)
				{
					if (EditableNoteType.IsReadOnlyAfterAdd)
					{
						note += "\r\n\r\n" + EditableNote.ST_NoteText;
					}

					bool notReadOnlyAndHasChanges = !EditableNoteType.IsReadOnlyAfterAdd && EditableNote.ST_NoteText != value;
					bool isReadOnlyAndHasTextToAdd = EditableNoteType.IsReadOnlyAfterAdd && !value.Trim().IsEmpty;

					if (notReadOnlyAndHasChanges || isReadOnlyAndHasTextToAdd)
					{
						note = Regex.Replace(note, "(?<!\r)\n", System.Environment.NewLine);
						EditableNote.ST_NoteText = note;
						isNoteChanged = true;
					}
				}
				EditableNoteTextInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo EditableNoteTextInfo
		{
			get { return GetZPropertyInfo(Schema.EditableNoteText); }
		}

		#endregion

		#region NoteStatus

		public bool IsNoteAdded
		{
			get { return fIsNoteAdded; }
		}
		bool fIsNoteAdded;

		public bool IsNoteChanged
		{
			get { return isNoteChanged; }
		}
		bool isNoteChanged;

		public override bool HasChanges
		{
			get
			{
				return base.HasChanges || IsNoteChanged || IsNoteAdded;
			}
			set
			{
				base.HasChanges = value;
				if (!value)
				{
					fIsNoteAdded = false;
					isNoteChanged = false;
				}
			}
		}

		#endregion

		#region Implementation

		string EditableNoteTypeDescription
		{
			get { return EditableNoteType.Description; }
		}

		internal StmNote FindEditableNote()
		{
			StmNote[] searchResult = parent.NotesParentBO.Notes.FindByDescription(EditableNoteTypeDescription);
			return searchResult.Length > 0 ? searchResult[0] : null;
		}

		void CreateNewEditableNote(ZString noteText)
		{
			editableNote = parent.NotesParentBO.Notes.AddNew(false, EditableNoteTypeDescription, noteText);
			fIsNoteAdded = true;
		}

		void ResetNoteStatusAfterSaving(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				fIsNoteAdded = false;
				isNoteChanged = false;
			}
		}

		#endregion
	}
}
