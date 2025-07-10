using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixShippingLineIsShippingLineTransformation : DataTransformation, IDataTransformationTask
	{
		public DataFixShippingLineIsShippingLineTransformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"
IF COL_LENGTH('dbo.RefShippingLine', 'RSL_IsShippingLine') IS NOT NULL
	UPDATE RefShippingLine SET RSL_IsShippingLine = 1 WHERE RSL_IsNVO = 0
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
