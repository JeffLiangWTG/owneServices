using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Recruiter.Business
{
	[CodeProperty(Schema.HJ_JobTitle), DescriptionProperty(Schema.HJ_JobTitle)]
	public class HRJobRole : AutoHRJobRole, IDocManagerSupport
	{
		public HRJobRole(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Deleting

		public override void Delete()
		{
			JobRoleSkills.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.JobRole);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region Related Business Objects		

		[ChildEditable(true)]
		public HRJobRoleSkillPivotDependentCollection JobRoleSkills
		{
			get
			{
				if (fJobRoleSkills == null)
				{
					fJobRoleSkills = new HRJobRoleSkillPivotDependentCollection(this);
					fJobRoleSkills.Load();
					RegisterEditableChildObject(fJobRoleSkills);
				}

				return fJobRoleSkills;
			}
		}

		HRJobRoleSkillPivotDependentCollection fJobRoleSkills;

		#endregion

		#region Full Job Role Description

		StmNote FullJobRoleDescriptionNote
		{
			get
			{
				if (fullJobRoleDescriptionNote == null || fullJobRoleDescriptionNote.IsDeleted)
				{
					fullJobRoleDescriptionNote = GetOrCreateNote(PredefinedNoteTypes.Instance.FullJobRoleDescription);
				}
				return fullJobRoleDescriptionNote;
			}
		}

		StmNote fullJobRoleDescriptionNote;

		#region FullJobRoleDescription

		[CargoWise.ComponentModel.MaxLength(10000)]
		public ZString FullJobRoleDescription
		{
			get { return FullJobRoleDescriptionNote.ST_NoteText; }
			set
			{
				if (FullJobRoleDescriptionNote.ST_NoteText != value)
				{
					CheckMaximumLength(FullJobRoleDescriptionInfo, value);
					FullJobRoleDescriptionNote.ST_NoteText = value;
					FullJobRoleDescriptionInfo.RefreshBinding();
					Validation.ValidateFullJobRoleDescription();
				}
			}
		}

		public ZPropertyInfo FullJobRoleDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(FullJobRoleDescription)); }
		}

		#endregion

		#endregion

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				NoteTypeCollection noteTypes = base.NoteTypesCore;
				noteTypes.Add(PredefinedNoteTypes.Instance.FullJobRoleDescription);
				return noteTypes;
			}
		}

		#region Implementation

		#region GetOrCreateNote

		protected StmNote GetOrCreateNote(PredefinedNoteType noteType)
		{
			StmNote note = null;
			StmNote[] foundNotes = Notes.FindByDescription(noteType.Description);
			if (foundNotes.Length > 0)
			{
				note = foundNotes[0];
			}
			else
			{
				note = CreateNote(noteType);
			}

			RegisterEditableChildObject(note);

			return note;
		}

		StmNote CreateNote(PredefinedNoteType noteType)
		{
			StmNote result;

			using (SuspendSettingHasChanges())
			using (Notes.SuspendSettingHasChanges())
			{
				result = Notes.AddNew();
				result.HasChanges = false;
				using (result.SuspendSettingHasChanges())
				{
					result.ST_Description = noteType.Description;
					result.ST_IsCustomDescription = false;
					result.ST_NoteType = noteType.DefaultVisibility.ToString();
				}
			}

			return result;
		}

		#endregion

		#endregion
	}
}
