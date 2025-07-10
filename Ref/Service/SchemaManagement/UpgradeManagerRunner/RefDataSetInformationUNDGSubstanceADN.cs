using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RefDataSetInformationUNDGSubstanceADN : DataTransformation, IDataTransformationTask
	{
		public RefDataSetInformationUNDGSubstanceADN(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"
IF (SELECT COUNT(*) FROM RefDataSetInformation WHERE RDS_TableName = 'UNDGSubstanceADN') = 0
BEGIN
	INSERT RefDataSetInformation(RDS_PK, RDS_DataSetId, RDS_DataSetName, RDS_TableName, RDS_DataSetTableCode, RDS_PriorityLevel)
	VALUES(newid(), 40, 'UNDGSubstanceADN', 'UNDGSubstanceADN', 'ADN', 0)
END
";

			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}
