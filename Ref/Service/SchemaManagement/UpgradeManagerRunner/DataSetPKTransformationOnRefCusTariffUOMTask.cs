using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataSetPKTransformationOnRefCusTariffUOMTask : DataTransformation, IDataTransformationTask
	{
		public DataSetPKTransformationOnRefCusTariffUOMTask(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sqlText = @"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'ZZ8_DataSetPK' AND object_id = OBJECT_ID('dbo.RefCusTariffUOM'))
BEGIN
	ALTER TABLE RefCusTariffUOM
	ADD 
		ZZ8_DataSetPK UNIQUEIDENTIFIER,
		ZZ8_DataSetCode VARCHAR(3);

	DISABLE TRIGGER ALL ON RefCusTariffUOM;
	EXEC ('UPDATE RefCusTariffUOM SET ZZ8_DataSetPK = ZZ8_ZZ1_Tariff, ZZ8_DataSetCode = ''ZZ1''');
	ENABLE TRIGGER ALL ON RefCusTariffUOM;
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
