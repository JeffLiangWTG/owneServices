using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class PopulateDatasetPKInDPRTransformation : DataTransformation, IDataTransformationTask
	{
		public PopulateDatasetPKInDPRTransformation(int version) : base(version)
		{ }

		string safeDbName = ApplicationConfig.SafeDBName ?? "RefDbRepoSafe";
		public void Run(IDbTransaction trans)
		{
			Argument.IsFalse(string.IsNullOrEmpty(safeDbName), nameof(safeDbName));
			var expectedTableCodes = TableCodeAndNameInfo.RootTables.Select(o => o.TableCode).Union(TableCodeAndNameInfo.TablesWithParentFKOrDataSetPK.Select(o => o.TableCode)).Union(TableCodeAndNameInfo.TablesWithMatchValue.Select(o => o.TableCode));
			var sqlCheck = $@"select count(*) from DataProcessingResult where DPR_ParentTableCode not in ({string.Join(',', expectedTableCodes.Select(o => $"'{o}'"))})";

			if (Convert.ToInt32(DbHelper.ExecuteScalar(trans, sqlCheck), CultureInfo.InvariantCulture) > 0)
			{
				Console.WriteLine("Here are some Data Tablecodes which are not in transformation list");
			}

			var tempID = "6D24DB1FBEB347D1A035C70271C75B2F";
			var sql = $"CREATE INDEX IX_DataProcessingResult_ParentTableCode_{tempID} ON DataProcessingResult (DPR_ParentTableCode);";
			sql += $@"update DataProcessingResult
set DPR_DatasetPK = DPR_ParentPK
where DPR_DatasetPK is null and DPR_ParentTableCode in ({string.Join(",", TableCodeAndNameInfo.RootTables.Select(x => $"'{x.TableCode}'"))});
";

			foreach (var item in TableCodeAndNameInfo.TablesWithParentFKOrDataSetPK)
			{
				var datasetField = $"{item.TableCode}_{(string.IsNullOrEmpty(item.DatasetNameOrParentPK) ? "DataSetPK" : item.DatasetNameOrParentPK)}";
				sql += $@"update DataProcessingResult
set DPR_DatasetPK = {datasetField}
from DataProcessingResult
inner join [{safeDbName}].[dbo].[{item.TableName}]
on DPR_ParentPK = {item.TableCode}_PK
where DPR_DatasetPK is null and DPR_ParentTableCode='{item.TableCode}';
";
			}

			foreach (var item in TableCodeAndNameInfo.TablesWithMatchValue)
			{
				sql += $@"update DataProcessingResult
set DPR_DatasetPK = {item.ParentTableCode}_PK
from DataProcessingResult
inner join [{safeDbName}].[dbo].[{item.TableName}]
on DPR_ParentPK = {item.TableCode}_PK
inner join [{safeDbName}].[dbo].[{item.ParentTableName}]
on {item.MatchCondition}
where DPR_DatasetPK is null and DPR_ParentTableCode='{item.TableCode}';
";
			}

			sql += $"drop index IX_DataProcessingResult_ParentTableCode_{tempID} ON DataProcessingResult;";
			DbHelper.ExecuteNonQuery(trans, sql, 0);
		}
	}
}
