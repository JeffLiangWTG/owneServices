using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefCusProcedureChangeWarehouseColumnsDataType : DataTransformation, IDataTransformationTask
	{
		public RefCusProcedureChangeWarehouseColumnsDataType(int version)
			: base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'RefCusProcedure' AND COLUMN_NAME = 'ZZ6_IntoWarehouse' AND DATA_TYPE = 'bit')
BEGIN
	DISABLE TRIGGER ALL ON RefCusProcedure;

	ALTER TABLE RefCusProcedure DROP CONSTRAINT DF_RefCusProcedure_ZZ6_IntoWarehouse;
	ALTER TABLE RefCusProcedure DROP CONSTRAINT DF_RefCusProcedure_ZZ6_OutOfWarehouse;

	EXEC sp_rename 'RefCusProcedure.ZZ6_IntoWarehouse', 'ZZ6_IntoWarehouse_', 'COLUMN';
	ALTER TABLE RefCusProcedure ADD ZZ6_IntoWarehouse CHAR(1) NOT NULL CONSTRAINT [DF_RefCusProcedure_ZZ6_IntoWarehouse] DEFAULT ('N');
	EXEC('UPDATE RefCusProcedure SET ZZ6_IntoWarehouse = CASE WHEN ZZ6_IntoWarehouse_ = 1 THEN ''Y'' ELSE ''N'' END');
	ALTER TABLE RefCusProcedure DROP COLUMN ZZ6_IntoWarehouse_;

	EXEC sp_rename 'RefCusProcedure.ZZ6_OutOfWarehouse', 'ZZ6_OutOfWarehouse_', 'COLUMN';
	ALTER TABLE RefCusProcedure ADD ZZ6_OutOfWarehouse CHAR(1) NOT NULL CONSTRAINT[DF_RefCusProcedure_ZZ6_OutOfWarehouse] DEFAULT('N');
	EXEC('UPDATE RefCusProcedure SET ZZ6_OutOfWarehouse = CASE WHEN ZZ6_OutOfWarehouse_ = 1 THEN ''Y'' ELSE ''N'' END');
	ALTER TABLE RefCusProcedure DROP COLUMN ZZ6_OutOfWarehouse_;

	ENABLE TRIGGER ALL ON RefCusProcedure;
END
";
			using (var cmd = trans.Connection.CreateCommand())
			{
				cmd.Transaction = trans;
				cmd.CommandText = sql;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
