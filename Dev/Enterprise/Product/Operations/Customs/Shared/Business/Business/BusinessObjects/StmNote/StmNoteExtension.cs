using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public static class StmNoteExtension
	{
		public static ZString GetNoteText(this Notes notes, string description) => notes.GetNote(description)?.ST_NoteText ?? ZString.Empty;

		public static void SetNoteText(this Notes notes, BusinessObject parent, ZPropertyInfo propertyInfo, string description, ZString value)
		{
			var note = notes.GetNote(description);
			var oldValue = note?.ST_NoteText ?? ZString.Empty;
			if (!value.IsEmpty)
			{
				if (note == null)
				{
					note = notes.AddNew();
					note.ST_Description = description;
				}
				BusinessObject.CheckMaximumLength(propertyInfo, value);
				note.ST_NoteText = value;
			}
			else
			{
				note?.Delete();
			}

			if (oldValue != value)
			{
				parent.HasChanges = true;
			}

			propertyInfo.RefreshBinding();
		}

		public static void CreateStmNoteForEdiMessage(this BusinessObjectFactory factory, ZGuid messagePk, ZString noteText)
		{
			CreateStmNoteForBusinessObject(factory, messagePk, EDIMessageSchema.Constants.TableName, noteText, (NoResString)"Processing Log");
		}

		public static void CreateStmNote(this BusinessObject bo, ZString noteText, ZString description)
		{
			CreateStmNoteForBusinessObject(bo.Factory, bo.PK, bo.TableName, noteText, description);
		}

		static StmNote GetNote(this Notes notes, string description) => notes.FindByDescription(description).FirstOrDefault();

		static void CreateStmNoteForBusinessObject(BusinessObjectFactory factory, ZGuid parentId, ZString tableName, ZString noteText, ZString description)
		{
			if (noteText.IsEmpty)
			{
				return;
			}

			var stmNote = factory.New<StmNote>();
			stmNote.ST_Table = tableName;
			stmNote.ST_ParentID = parentId;
			stmNote.ST_IsCustomDescription = true;
			stmNote.ST_ForceRead = true;
			stmNote.ST_NoteText = noteText;
			stmNote.ST_NoteType = nameof(StmNoteVisibility.INT);
			stmNote.ST_NoteContext = "AAA";
			stmNote.ST_Description = description;
		}
	}
}
