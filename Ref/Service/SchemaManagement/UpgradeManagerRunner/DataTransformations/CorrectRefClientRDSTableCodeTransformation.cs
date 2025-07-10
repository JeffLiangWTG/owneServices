using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataTransformations
{
	public class CorrectRefClientRDSTableCodeTransformation : DataTransformation, IDataTransformationTask
	{
		public CorrectRefClientRDSTableCodeTransformation(int version) : base(version)
		{ }
		public void Run(IDbTransaction trans)
		{
			var sql = @"
IF EXISTS (SELECT TOP 1 1 FROM RefDataSetInformation WHERE RDS_DataSetTableCode = 'ZCT' AND RDS_TableName = 'RefClient')
BEGIN
	UPDATE RefDataSetInformation SET RDS_DataSetTableCode = 'RCT' WHERE RDS_TableName = 'RefClient'
END
";
			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}
