using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	class StmNoteRepository
	{
		internal StmNoteRepository(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		internal StmNote FindStmNote(IEntityInfo parent, string description, bool isMatchingStTable = true)
		{
			var query = new ZQuery();
			query.AddToFilter(StmNoteSchema.ST_ParentID, SQLComparisonOperator.Equal, parent.InternalPK);
			query.AddToFilter(StmNoteSchema.ST_Description, SQLComparisonOperator.Equal, description);
			if (isMatchingStTable)
			{
				query.AddToFilter(StmNoteSchema.ST_Table, SQLComparisonOperator.Equal, parent.TableName);
			}
			else
			{
				query.AddToFilter(StmNoteSchema.ST_Table, SQLComparisonOperator.NotEqual, "x"); //HACK: this code needs to search not matching a particular ST_Table
			}

			return factory.Load<StmNote>(query).FirstOrDefault();
		}

		internal StmNote CreateStmNote(IEntityInfo parent, string description)
		{
			var note = factory.New<StmNote>();
			note.ST_Description = description;
			note.ST_NoteText = String.Empty;
			note.ST_ParentID = parent.InternalPK;
			note.ST_Table = parent.TableName;

			return note;
		}
	}
}
