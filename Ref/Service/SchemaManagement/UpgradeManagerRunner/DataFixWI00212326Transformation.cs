using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixWI00212326Transformation : DataTransformation, IDataTransformationTask
	{
		public DataFixWI00212326Transformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"SELECT DISTINCT
ZZD_PK
FROM RefCusCodeList 
	JOIN RefCusCodeListAttribute on ZZE_ZZD_CodeList = ZZD_PK
WHERE 
ZZD_ZZZ_NKDataGrouping = 'AU'
AND ZZD_ZZK_NKCodeType = 'AQISP'
AND ((ZZD_StartDate = '2018-04-09 00:00:00' AND ZZD_EndDate = '2018-07-22 23:59:00') OR (ZZD_StartDate = '2018-07-23 00:00:00' AND ZZD_EndDate = '2079-06-06 23:59:00'))";
			var zzdPks = new List<string>();

			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.CommandText = sql;
				cmd.Transaction = trans;
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						zzdPks.Add(reader.GetGuid(0).ToString());
					}
				}
			}

			if (zzdPks.Count > 0)
			{
				using (var cmd = trans.Connection?.CreateCommand())
				{
					var zzdPksInQuery = string.Join(",", zzdPks.Select(o => FormattableString.Invariant($"'{o}'")));
					cmd.CommandText = FormattableString.Invariant($"UPDATE RefDbVersionControl SET RVC_Deleted = 1, RVC_LastUpdatedUTC = sysutcdatetime() WHERE RVC_ParentCode = 'ZZD' AND RVC_ParentPK IN ({zzdPksInQuery})");
					cmd.Transaction = trans;
					cmd.ExecuteNonQuery();
				}
			}

			var sqlFixEndDate = @"
UPDATE t
SET t.ZZD_EndDate = '2079-06-06 23:59:00'
FROM
	RefCusCodeList t
	JOIN RefCusCodeListAttribute ON ZZE_ZZD_CodeList = ZZD_PK
WHERE
ZZD_ZZZ_NKDataGrouping = 'AU'
AND ZZD_ZZK_NKCodeType = 'AQISP'
AND ZZD_StartDate = '1900-01-01 12:00:00' AND ZZD_EndDate = '2018-04-08 23:59:00'";

			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.CommandText = sqlFixEndDate;
				cmd.Transaction = trans;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
