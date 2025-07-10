using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataSetPKTransformationOnTariffAttributeTask : DataTransformation, IDataTransformationTask
	{
		public DataSetPKTransformationOnTariffAttributeTask(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sqlText = @"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'ZZ3_DataSetPK' AND object_id = OBJECT_ID('dbo.RefCusTariffAttribute'))
BEGIN
	ALTER TABLE RefCusTariffAttribute
	ADD 
		ZZ3_DataSetPK UNIQUEIDENTIFIER,
		ZZ3_DataSetCode VARCHAR(3);

	DISABLE TRIGGER ALL ON RefCusTariffAttribute;
	EXEC ('UPDATE RefCusTariffAttribute SET ZZ3_DataSetPK = ZZ3_ZZ1_Tariff, ZZ3_DataSetCode = ''ZZ1''');
	ENABLE TRIGGER ALL ON RefCusTariffAttribute;
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
