using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	internal class OriginalBillNotesService
	{
		readonly BusinessObjectFactory factory;

		public OriginalBillNotesService(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public StmNote LoadOrCreateStmNoteForReaderUpdate(ZGuid parentPK, ZString tableName, bool isParentInDatabase, ZString noteDescription, CargoWise.Definitions.StmNoteVisibility noteVisibility = CargoWise.Definitions.StmNoteVisibility.INT, string noteContext = "AAA")
		{
			var noteType = noteVisibility.ToString();
			return LoadStmNote(parentPK, tableName, noteDescription, noteType, noteContext, isParentInDatabase) ?? CreateNewStmNote(parentPK, tableName, noteDescription, noteType, noteContext);
		}

		StmNote LoadStmNote(ZGuid parentPK, ZString tableName, ZString noteDescription, ZString noteType, ZString noteContext, bool isParentInDatabase)
		{
			var query = new ZQuery(StmNoteSchema.ST_ParentID, parentPK);
			query.AddToFilter(StmNoteSchema.ST_Table, tableName);
			query.AddToFilter(StmNoteSchema.ST_Description, noteDescription);
			query.AddToFilter(StmNoteSchema.ST_NoteType, noteType);
			query.AddToFilter(StmNoteSchema.ST_NoteContext, noteContext);
			query.FetchOnlyFromLocalCache = !isParentInDatabase;

			return factory.LoadTop1<ForwardingConsolStmNote>(query);
		}

		StmNote CreateNewStmNote(ZGuid parentPK, ZString tableName, ZString noteDescription, ZString noteType, ZString noteContext)
		{
			var note = factory.New<ForwardingConsolStmNote>();
			note.ST_ParentID = parentPK;
			note.ST_Table = tableName;
			note.ST_Description = noteDescription;
			note.ST_NoteType = noteType;
			note.ST_NoteContext = noteContext;
			return note;
		}
	}
}
