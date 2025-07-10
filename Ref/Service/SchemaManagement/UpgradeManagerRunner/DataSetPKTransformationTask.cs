using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataSetPKTransformationTask : DataTransformation, IDataTransformationTask
	{
		public DataSetPKTransformationTask(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sqlText = @"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'ZZ2_DataSetPK' AND object_id = OBJECT_ID('dbo.RefCusRate'))
BEGIN
	ALTER TABLE RefCusRate
	ADD 
		ZZ2_DataSetPK UNIQUEIDENTIFIER,
		ZZ2_DataSetCode VARCHAR(3);

	DISABLE TRIGGER ALL ON RefCusRate;
	EXEC ('UPDATE RefCusRate SET ZZ2_DataSetPK = ZZ2_ZZ1_Tariff, ZZ2_DataSetCode = ''ZZ1''');
	ENABLE TRIGGER ALL ON RefCusRate;
END
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'ZZJ_DataSetPK' AND object_id = OBJECT_ID('dbo.RefCusRateAttribute'))
BEGIN
	ALTER TABLE RefCusRateAttribute
	ADD 
		ZZJ_DataSetPK UNIQUEIDENTIFIER,
		ZZJ_DataSetCode VARCHAR(3);

	DISABLE TRIGGER ALL ON RefCusRateAttribute;
	EXEC ('UPDATE attr SET ZZJ_DataSetPK = ZZ2_ZZ1_Tariff, ZZJ_DataSetCode = ''ZZ1''
		FROM RefCusRateAttribute attr
		JOIN RefCusRate ON ZZJ_ZZ2_Rate = ZZ2_PK');
	ENABLE TRIGGER ALL ON RefCusRate;
END
";
			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Transaction = trans;
				cmd.CommandText = sqlText;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
