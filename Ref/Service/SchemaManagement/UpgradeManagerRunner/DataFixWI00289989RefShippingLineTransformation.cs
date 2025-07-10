using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixWI00289989RefShippingLineTransformation : DataTransformation, IDataTransformationTask
	{
		public DataFixWI00289989RefShippingLineTransformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"
UPDATE RefShippingLine set [RSL_IsCW1User] = 1,[RSL_EHubIds] = 'NEPPRD'
Where RSL_CargoWiseOneCode = 'C1NT' and RSL_EHubIds = '';

UPDATE RefShippingLine set [RSL_IsCW1User] = 1,[RSL_EHubIds] = 'ILMMEL'
Where RSL_CargoWiseOneCode = 'C1PX' and RSL_EHubIds = '';

UPDATE RefShippingLine set [RSL_IsCW1User] = 1,[RSL_EHubIds] = 'PSCCHC'
Where RSL_CargoWiseOneCode = 'C1PS' and RSL_EHubIds = '';

UPDATE RefShippingLine set [RSL_IsCW1User] = 1,[RSL_EHubIds] = 'SSTJAX,SSTSEA'
Where RSL_CargoWiseOneCode = 'C1TM' and RSL_EHubIds = '';
";

			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Connection = trans.Connection;
				cmd.Transaction = trans;
				cmd.CommandText = sql;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
