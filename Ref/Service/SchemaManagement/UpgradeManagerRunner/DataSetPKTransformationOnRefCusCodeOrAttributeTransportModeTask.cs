using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataSetPKTransformationOnRefCusCodeOrAttributeTransportModeTask : DataTransformation, IDataTransformationTask
	{
		public DataSetPKTransformationOnRefCusCodeOrAttributeTransportModeTask(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sqlText = @"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'ZZU_DataSetPK' AND object_id = OBJECT_ID('dbo.RefCusCodeOrAttributeTransportMode'))
BEGIN
	ALTER TABLE RefCusCodeOrAttributeTransportMode 
	ADD 
		ZZU_DataSetPK UNIQUEIDENTIFIER,
		ZZU_DataSetCode VARCHAR(3);

	DISABLE TRIGGER ALL ON RefCusCodeOrAttributeTransportMode;
	EXEC('UPDATE t 
	SET ZZU_DataSetPK = ISNULL(ZZU_ZZD_CodeList,ZZE_ZZD_CodeList) 
	,ZZU_DataSetCode = ''ZZD''
FROM RefCusCodeOrAttributeTransportMode t 
LEFT JOIN RefCusCodeListAttribute a ON t.ZZU_ZZE_Attribute = a.ZZE_PK');
	ENABLE TRIGGER ALL ON RefCusCodeOrAttributeTransportMode;
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
