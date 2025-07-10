using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class NoteBusinessObjectFinder : MatchingBusinessObjectFinder<Note, StmNote>
	{
		public NoteBusinessObjectFinder(Note dataObject)
			: base(dataObject)
		{
		}

		public StmNote Find(IStmNoteParent noteParent)
		{
			Argument.NotNull(noteParent, "noteParent");
			StmNote matchedNote = null;

			if (dataObject.Description.HasValue && !dataObject.Description.Value.IsEmpty)
			{
				Func<ZString, bool> isAAAContext = noteContext => { return noteContext == ZString.Empty || noteContext == "AAA"; };
				StmNote aaaNote = null;

				var context = dataObject.NoteContext != null && dataObject.NoteContext.Code.HasValue
					? dataObject.NoteContext.Code.Value
					: ZString.Empty;

				var notes = noteParent.Notes.FindByDescription(dataObject.Description.Value);
				foreach (var note in notes)
				{
					if (note.ST_NoteContext == context)
					{
						matchedNote = note;
						break;
					}
					else if (isAAAContext(note.ST_NoteContext))
					{
						aaaNote = note;
					}
				}

				if (matchedNote == null && isAAAContext(context))
				{
					matchedNote = aaaNote;
				}
			}

			return matchedNote;
		}

		[Obsolete("notes search should be performed using IStmNoteParent", true), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "businessObjects")]
		public new StmNote Find(IEnumerable<StmNote> businessObjects)
		{
			return null;
		}

		protected override StmNote FindCore(IEnumerable<StmNote> businessObjects)
		{
			throw new InvalidOperationException("search should be performed using IStmNoteParent");
		}
	}
}
