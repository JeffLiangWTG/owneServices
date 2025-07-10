using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class UnmatchNoteCreator : IUnmatchNoteCreator
	{
		public UnmatchNoteCreator(BusinessObjectFactory factory)
		{
			this.factory = factory;
			noteRepository = new StmNoteRepository(factory);
		}
		readonly BusinessObjectFactory factory;
		readonly StmNoteRepository noteRepository;

		public void Create(IEntityInfo parentInfo, UnmatchOrgRecord newRecord)
		{
			var description = PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description;

			var foundNote = noteRepository.FindStmNote(parentInfo, description, false)
							?? noteRepository.CreateStmNote(parentInfo, description);

			var records = Deserialize(foundNote);
			AddUnmatchRecord(records, newRecord);
			foundNote.ST_NoteText = records.AsXml();
		}

		void AddUnmatchRecord(UnmatchOrgRecords records, UnmatchOrgRecord newRecord)
		{
			var list = records.OrgDetailsList;
			records.OrgDetailsList = list.Add(newRecord, new UnmatchOrgRecordComparer());
		}

		UnmatchOrgRecords Deserialize(StmNote note)
		{
			if (CanDeserialize(note))
			{
				return SerializableNoteText.FromXml<UnmatchOrgRecords>(note);
			}
			return new UnmatchOrgRecords(factory);
		}

		bool CanDeserialize(StmNote note)
		{
			return note != null && SerializableNoteText.CanSerialize(note, typeof(UnmatchOrgRecords));
		}
	}
}
