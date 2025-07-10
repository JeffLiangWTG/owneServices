using System.Collections.Generic;
using System.Data;

namespace CargoWise.RefDbRepo.Common.DbUpgrade
{
	public abstract class RenameColumnsDataTransformation : IDataTransformationTask
	{
		public int Version
		{
			get;
		}

		protected RenameColumnsDataTransformation(int version)
		{
			Version = version;
		}

		protected abstract IEnumerable<RenameColumnObject> GetColumnsToRename();
		protected abstract string GetTableName();

		public virtual void ExecuteCommandBeforeRename(IDbTransaction trans) { }
		public virtual void ExecuteCommandAfterRename(IDbTransaction trans) { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			Argument.Argument.NotNull(trans, nameof(trans));
			var columnsObjects = GetColumnsToRename();
			var tableName = GetTableName();

			if (SQLRenameColumnsHelper.HasOldColumn(columnsObjects, tableName, trans))
			{
				ExecuteCommandBeforeRename(trans);
				using (var cmd = trans.Connection?.CreateCommand())
				{
					cmd.CommandText = SQLRenameColumnsHelper.DropConstraintsAndRenameColumns(tableName, columnsObjects, trans);
					cmd.Transaction = trans;

					cmd.ExecuteNonQuery();
				}
				ExecuteCommandAfterRename(trans);
			}
		}
	}

	public class RenameColumnObject
	{
		public string OldColumn => oldColumn;
		public string NewColumn => newColumn;
		public RenameColumnObject(string oldColumn, string newColumn)
		{
			Argument.Argument.NotNullOrEmpty(oldColumn, nameof(oldColumn));
			Argument.Argument.NotNullOrEmpty(newColumn, nameof(newColumn));

			this.oldColumn = oldColumn;
			this.newColumn = newColumn;
		}

		readonly string oldColumn;
		readonly string newColumn;
	}
}
