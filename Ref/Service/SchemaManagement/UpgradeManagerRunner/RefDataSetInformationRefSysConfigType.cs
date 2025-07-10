using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationRefSysConfigType : DataTransformation, IDataTransformationTask
	{
		public RefDataSetInformationRefSysConfigType(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"
IF (SELECT COUNT(*) FROM RefDataSetInformation WHERE RDS_TableName = 'RefSysConfigType') = 0
BEGIN
	INSERT RefDataSetInformation(RDS_PK, RDS_DataSetId, RDS_DataSetName, RDS_TableName, RDS_DataSetTableCode, RDS_PriorityLevel)
	VALUES(newid(), 41, 'RefSysConfigType', 'RefSysConfigType', 'ZRT', 0)
END
";

			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}
