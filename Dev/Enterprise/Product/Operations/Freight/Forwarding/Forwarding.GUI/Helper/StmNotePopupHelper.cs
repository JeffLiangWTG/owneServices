using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public static class StmNotePopupHelper
	{
		public static void ShowNoteForm(
			Control control,
			IStmNoteParent bizO,
			string noteType,
			bool createNewNoteIfNotFound,
			Action noteChanged)
		{
			var resetHasChangesIfNoteCancelled = !((BusinessObject)bizO).HasChanges;
			var deleteNoteIfNoteCancelled = false;

			var noteTypeToEdit = NoteTypeFromDescription(noteType);
			var noteToEdit = GetCurrentNote(bizO, noteType);

			if (noteToEdit == null)
			{
				if (createNewNoteIfNotFound)
				{
					noteToEdit = bizO.Notes.AddNew();
					noteToEdit.ST_Description = noteTypeToEdit.Description;
					noteToEdit.ST_NoteType = noteTypeToEdit.DefaultVisibility.ToString();
					noteToEdit.ST_IsCustomDescription = false;
					deleteNoteIfNoteCancelled = true;
				}
				else
				{
					_ = Globals.Message.Show(NoNoteExistsErrorDefault, Res.GetString("044745E1-1EF8-45D5-8C9D-BADA07BD3696", "No Note Found"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}

			if (noteToEdit != null)
			{
				var notePopupForm = new ZStmNotePopupForm(noteToEdit, bizO, resetHasChangesIfNoteCancelled, deleteNoteIfNoteCancelled);
				if (noteChanged != null)
				{
					notePopupForm.NoteHasChangesChanged += new ZStmNotePopupForm.NoteHasChangesEventHandler(noteChanged);
				}
				ZFormModaliser.Show(notePopupForm, control.FindForm());
			}
		}

		static string NoNoteExistsErrorDefault => Res.GetString("ab1d9322-82c2-4a13-81b1-7a4ce492d45a", "Log is available upon Redistribution and Sharing of G/W Consol Profit is processed by your current login company.");

		static PredefinedNoteType NoteTypeFromDescription(string description)
			=> PredefinedNoteTypes.Instance.NoteTypeByDescription(description);

		static StmNote GetCurrentNote(IStmNoteParent businessObject, string noteType)
		{
			return noteType != null && businessObject != null
				? businessObject.Notes.FindByDescription(noteType).FirstOrDefault(x => !x.IsNull && x.IsBelongingToCurrentLoginCompany)
				: null;
		}
	}
}
