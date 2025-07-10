using System;
using System.Data;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class DataFixCheckConstraintsTransformation : DataTransformation, IDataTransformationTask
	{
		public DataFixCheckConstraintsTransformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			//disable all constraints in db
			var disableConstraintsSql = @"EXEC sp_MSforeachtable ""ALTER TABLE ? NOCHECK CONSTRAINT all""";
			DbHelper.ExecuteNonQuery(trans, disableConstraintsSql, 0);

			var checkConstraintsDeleteScripts = @"
select CONCAT('delete A from '+TableName+' A',
' where NOT ', ConstraintDefinition) DeleteScript
from (select A.definition ConstraintDefinition, object_name(A.parent_object_id) TableName
from sys.check_constraints A
where A.is_not_trusted=1) FK
UNION ALL
select CONCAT('delete A from '+TableName+' A left join '+ReferenceTable,
' B on A.'+FKColumn+' = B.'+ReferenceColumn,
' where A.'+FKColumn+' is not null and B.'+ReferenceColumn+' is null;') DeleteScript
from (select A.name FKName, object_name(B.parent_object_id) TableName, C.name FKColumn,
object_name(B.referenced_object_id) ReferenceTable, D.name ReferenceColumn
from sys.foreign_keys A
inner join sys.foreign_key_columns B on A.object_id=b.constraint_object_id
inner join sys.columns C on B.parent_object_id=C.object_id and B.parent_column_id=C.column_id
inner join sys.columns D on B.referenced_object_id=d.object_id and B.referenced_column_id=D.column_id
where A.is_not_trusted=1) FK
";

			using (var cmdCheckConstraints = DbHelper.CreateCommand(trans, checkConstraintsDeleteScripts))
			using (var readerCheckConstraints = cmdCheckConstraints.ExecuteReader())
			{
				while (readerCheckConstraints.Read())
				{
					var sql = readerCheckConstraints.GetString(0);
					DbHelper.ExecuteNonQuery(trans, sql, 0);
				}
			}

			//Check for further foreign key problems
			bool shouldCheck = true;
			while (shouldCheck)
			{
				shouldCheck = FKConstraintsCheck(trans);
			}

			//enable all constraints in db
			var enableConstraintsSql = @"EXEC sp_MSforeachtable ""ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all""";
			DbHelper.ExecuteNonQuery(trans, enableConstraintsSql, 0);
		}

		static bool FKConstraintsCheck(IDbTransaction trans)
		{
			var selectQuery = @"select CONCAT('SELECT COUNT(*) from '+TableName+' A left join '+ReferenceTable,
' B on A.'+FKColumn+' = B.'+ReferenceColumn,
' where A.'+FKColumn+' is not null and B.'+ReferenceColumn+' is null;') DeleteScript
from (select A.name FKName, object_name(B.parent_object_id) TableName, C.name FKColumn,
object_name(B.referenced_object_id) ReferenceTable, D.name ReferenceColumn
from sys.foreign_keys A
inner join sys.foreign_key_columns B on A.object_id=b.constraint_object_id
inner join sys.columns C on B.parent_object_id=C.object_id and B.parent_column_id=C.column_id
inner join sys.columns D on B.referenced_object_id=d.object_id and B.referenced_column_id=D.column_id
where A.is_not_trusted=1) FK";
			var result = 0;

			using (var cmdCheckConstraints = DbHelper.CreateCommand(trans, selectQuery))
			using (var readerCheckConstraints = cmdCheckConstraints.ExecuteReader())
			{
				while (readerCheckConstraints.Read())
				{
					var sql = readerCheckConstraints.GetString(0);
					result = Convert.ToInt32(DbHelper.ExecuteScalar(trans, sql, 0), CultureInfo.InvariantCulture);
					if (result > 0)
					{
						break;
					}
				}
			}

			if (result == 0)
			{
				return false;
			}

			var deleteQuery = @"select CONCAT('delete A from '+TableName+' A left join '+ReferenceTable,
' B on A.'+FKColumn+' = B.'+ReferenceColumn,
' where A.'+FKColumn+' is not null and B.'+ReferenceColumn+' is null;') DeleteScript
from (select A.name FKName, object_name(B.parent_object_id) TableName, C.name FKColumn,
object_name(B.referenced_object_id) ReferenceTable, D.name ReferenceColumn
from sys.foreign_keys A
inner join sys.foreign_key_columns B on A.object_id=b.constraint_object_id
inner join sys.columns C on B.parent_object_id=C.object_id and B.parent_column_id=C.column_id
inner join sys.columns D on B.referenced_object_id=d.object_id and B.referenced_column_id=D.column_id
where A.is_not_trusted=1) FK";

			using (var cmdCheckConstraints = DbHelper.CreateCommand(trans, deleteQuery))
			using (var readerCheckConstraints = cmdCheckConstraints.ExecuteReader())
			{
				while (readerCheckConstraints.Read())
				{
					var sql = readerCheckConstraints.GetString(0);
					DbHelper.ExecuteNonQuery(trans, sql, 0);
				}
			}
			return true;
		}
	}
}
