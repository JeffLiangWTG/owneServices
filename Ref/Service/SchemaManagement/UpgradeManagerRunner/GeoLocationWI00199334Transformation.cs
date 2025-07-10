using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class GeoLocationWI00199334Transformation : DataTransformation, IDataTransformationTask
	{
		public GeoLocationWI00199334Transformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sqlTransform = @"Update RefUNLOCO
	Set RL_GeoLocation = CASE
		WHEN RL_CoOrdinates not like '[0-9][0-9][0-9][0-9][NS] [0-9][0-9][0-9][0-9][0-9][WE]'
		THEN CONVERT(geography, 'POINT EMPTY')
		ELSE geography::STPointFromText('POINT(' 
			+ (select case when SUBSTRING(RL_CoOrdinates, 12, 1) = 'E' then '' else '-' end) 
			+ CONVERT(varchar, CONVERT(decimal(8,5), SUBSTRING(RL_CoOrdinates, 7, 3)) + CONVERT(decimal(8, 5), CONVERT(decimal(8,5), SUBSTRING(RL_CoOrdinates, 10, 2)) / 60.0)) 
			+ ' ' 
			+ (select case when SUBSTRING(RL_CoOrdinates, 5, 1) = 'N' then '' else '-' end) 
			+ CONVERT(varchar,CONVERT(decimal(8,5), SUBSTRING(RL_CoOrdinates, 1, 2)) + CONVERT(decimal(8, 5), CONVERT(decimal(8,5), SUBSTRING(RL_CoOrdinates, 3, 2)) / 60.0)) +')', 4326)
			END
	Where RL_CoOrdinates <> '' AND RL_GeoLocation.STEquals(CONVERT(geography, 'POINT EMPTY')) = 1;";

			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Connection = trans.Connection;
				cmd.Transaction = trans;
				cmd.CommandText = sqlTransform;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
