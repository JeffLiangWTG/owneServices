using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration.ZArchitecture;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class NotesCollectionReader : DataObjectCollectionReader<Note, StmNote>
	{
		public NotesCollectionReader(DataObjectList<Note> notes, IXmlImportLogger logger, UniversalObjectFactory factory, IUniversalXMLNoteParent noteParent, IEnumerable<ZString> skippedNoteDescriptions = null, INoteTypeCollection masterBizoNoteTypes = null)
			: base(notes)
		{
			this.logger = Argument.NotNull(logger, "logger");
			this.factory = Argument.NotNull(factory, "factory");
			this.noteParent = Argument.NotNull(noteParent, "noteParent");
			this.skipNoteDescriptions = skippedNoteDescriptions;
			this.masterBizoNoteTypes = masterBizoNoteTypes;
		}

		protected readonly IXmlImportLogger logger;
		protected readonly UniversalObjectFactory factory;
		protected readonly IUniversalXMLNoteParent noteParent;
		readonly IEnumerable<ZString> skipNoteDescriptions;
		readonly INoteTypeCollection masterBizoNoteTypes;

		#region Implementation

		protected override CollectionContent DefaultCollectionContent
		{
			get { return CollectionContent.Partial; }
		}

		protected override StmNote[] BusinessObjects
		{
			get { return notes ?? (notes = noteParent.Notes.GetAllNotes().Cast<StmNote>().ToArray()); }
		}

		StmNote[] notes;

		protected override void AddToCollection(StmNote note)
		{
			noteParent.Notes.Add(note);
		}

		string UnMatchedOrgNoteDescription
		{
			get { return unMatchedOrgNoteDescription ?? (unMatchedOrgNoteDescription = PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description); }
		}
		string unMatchedOrgNoteDescription;

		protected sealed override void RemoveFromCollection(StmNote note)
		{
			if (note.ST_Description != UnMatchedOrgNoteDescription)
			{
				note.Delete();
			}
		}

		protected override StmNote FindMatchingBusinessObject(Note dataObject)
		{
			var finder = new NoteBusinessObjectFinder(dataObject);
			return finder.Find(noteParent);
		}

		protected override StmNote ReadIntoBusinessObject(Note dataObject, StmNote note)
		{
			var reader = new NoteDataObjectReader(dataObject, logger, factory, noteParent, (dataObj) => note, masterBizoNoteTypes);
			return reader.ReadIntoBusinessObject();
		}

		protected override bool SkipEntity(Note dataObject) =>
			skipNoteDescriptions != null
			&& (dataObject?.Description.HasValue ?? false)
			&& skipNoteDescriptions.Contains(dataObject.Description.Value);

		#endregion
	}
}
