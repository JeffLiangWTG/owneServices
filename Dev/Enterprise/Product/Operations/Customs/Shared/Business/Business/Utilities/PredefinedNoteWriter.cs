using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class PredefinedNoteWriter
	{
		public PredefinedNoteWriter(BusinessObject parent, PredefinedNoteType noteType)
		{
			this.parent = parent;
			this.noteType = noteType;
			this.notes = parent.GetNotes();
		}

		public PredefinedNoteWriter(BusinessObject parent, ZString description, StmNoteVisibility visibility)
		{
			this.parent = parent;
			this.description = description;
			this.visibility = visibility;
		}

		readonly BusinessObject parent;
		readonly Notes notes;
		readonly PredefinedNoteType noteType;
		readonly ZString description;
		readonly StmNoteVisibility visibility;

		public ZString Value
		{
			get
			{
				StmNote stmNote = GetNote();
				return stmNote != null ? stmNote.ST_NoteDataAsText : ZString.Empty;
			}
		}

		public bool UpdateValue(ZString value)
		{
			bool updated = false;

			StmNote stmNote = GetNote();
			if (value.IsEmpty)
			{
				if (stmNote != null)
				{
					stmNote.Delete();
					parent.UnRegisterEditableChildObject(stmNote);
					updated = true;
				}
			}
			else if (stmNote == null)
			{
				if (noteType != null)
				{
					stmNote = notes.AddNew(false, noteType.Description, value);
					parent.RegisterEditableChildObject(stmNote);
				}
				else if (!description.IsEmpty)
				{
					stmNote = parent.Factory.New<StmNote>();
					stmNote.ST_ParentID = parent.PK;
					stmNote.ST_Table = parent.TableName;
					stmNote.ST_NoteType = visibility.ToString();
					stmNote.ST_Description = description;
					stmNote.ST_NoteDataAsText = value;
				}

				fNote = stmNote;
				updated = true;
			}
			else if (stmNote.ST_NoteDataAsText != value)
			{
				stmNote.ST_NoteDataAsText = value;
				updated = true;
			}

			return updated;
		}

		StmNote GetNote()
		{
			if (fNote != null && fNote.IsDeleted)
			{
				fNote = null;
			}
			if (fNote == null)
			{
				if (noteType != null)
				{
					fNote = notes.FindByDescription(noteType.Description).FirstOrDefault();

					if (fNote != null)
					{
						parent.RegisterEditableChildObject(fNote);
					}
				}
				else if (!description.IsEmpty)
				{
					ZQuery filter = new ZQuery(StmNoteSchema.ST_ParentID, parent.PK);
					filter.AddToFilter(StmNoteSchema.ST_Description, description);
					filter.AddToFilter(StmNoteSchema.ST_NoteType, visibility.ToString());
					filter.AddToFilter(StmNoteSchema.ST_Table, parent.TableName);
					filter.IncludeBlob(StmNoteSchema.ST_NoteData);
					filter.IncludeBlob(StmNoteSchema.ST_NoteText);
					fNote = parent.Factory.LoadTop1<StmNote>(filter);
				}
			}
			return fNote;
		}
		StmNote fNote;
	}
}
