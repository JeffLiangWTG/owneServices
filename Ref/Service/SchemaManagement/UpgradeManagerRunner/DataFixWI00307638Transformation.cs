using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixWI00307638Transformation : DataTransformation, IDataTransformationTask
	{
		public DataFixWI00307638Transformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"
IF EXISTS (SELECT * FROM RefDataSetInformation WHERE RDS_DataSetTableCode = 'ZZ0')
BEGIN
	--fix datasetId for RefVesselZZ
	UPDATE RefDbVersionControl SET RVC_DataSetId = 22 WHERE RVC_ParentCode = 'ZZO'

	-- fix datasetCode in datasetInformation
	UPDATE RefDataSetInformation
	SET RDS_DataSetTableCode = 'ZZO', RDS_LastUpdatedUTC = (SELECT MAX(RVC_LastUpdatedUTC) FROM RefDbVersionControl WHERE RVC_ParentCode = 'ZZO')
	WHERE RDS_DataSetTableCode = 'ZZ0'
END";

			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}
