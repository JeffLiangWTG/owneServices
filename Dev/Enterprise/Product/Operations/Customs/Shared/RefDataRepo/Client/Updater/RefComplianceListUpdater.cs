using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;
using CargoWise.RefDbRepo.Common.Contract_0_9;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public class RefComplianceListUpdater : DataSetUpdater<RefComplianceList, IRefComplianceList>
	{
		public RefComplianceListUpdater(IServerProxy proxy, IDBHelper dbHelper, IRefVersionControlManager versionControlManager)
			: base(proxy, dbHelper, versionControlManager, new SqlServerSQLBuilder(), new ValueConverter())
		{
		}

		public override int UpdaterVersion => 2;

		public override IEnumerable<Tuple<Type, Type>> GetTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefComplianceList), typeof(RefComplianceList));
		}

		string GetMergInsertStatement()
		{
			var columns = SharedSQLBuilder.GetAllColumnProperties<IRefComplianceList>().Select(x => x.Name).ToDictionary(x => x, x => x);

			var complianceListRegistry = Enterprise.MasterFiles.Business.ReferenceFilesDataRegistry.Instance.ComplianceListDefaults;
			if (complianceListRegistry.Value == null || complianceListRegistry.Value.Count == 0)
			{
				return string.Format(CultureInfo.InvariantCulture, (NoResString)@"INSERT ({0}, {2}) VALUES ({1}, {3})", string.Join(", ", columns.Keys), string.Join(", ", columns.Values.Select(x => "s." + x)),
					SharedSQLBuilder.GetSystemTimeAndUserColumnsInOrder(typeof(IRefComplianceList)), SharedSQLBuilder.SystemTimeAndUserValueStatement);
			}
			else
			{
				var isExcludedValueStatement = $@"CASE RCL_ListType
";
				foreach (CodeDescriptionBool item in complianceListRegistry.Value)
				{
					isExcludedValueStatement += (NoResString)"WHEN '" + item.Description.GetUnresolvedString() + (NoResString)"' THEN " + (item.Bool ? "1" : "0");
					isExcludedValueStatement += @"
";
				}
				isExcludedValueStatement += (NoResString)@"ELSE 0
END";

				return string.Format(CultureInfo.InvariantCulture, (NoResString)@"INSERT ({0}, {2}, {4}) VALUES ({1}, {3}, {5})", string.Join(", ", columns.Keys), string.Join(", ", columns.Values.Select(x => "s." + x)),
					SharedSQLBuilder.GetSystemTimeAndUserColumnsInOrder(typeof(IRefComplianceList)), SharedSQLBuilder.SystemTimeAndUserValueStatement, "RCL_IsExcluded", isExcludedValueStatement);
			}
		}

		protected override string GetMergeSqlTextCore(IDbTransaction transaction, IDictionary<Type, ForeignKeyRelationship[]> fks, string dataSetName)
		{
			var result = new StringBuilder();
			var uniqueConstraintIndexColumnsArray = DBHelper.GetAllUniqueIndexes<IRefComplianceList>(transaction);

			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteRecord<IRefComplianceList>(sQLBuilder, dataSetName, fks, uniqueConstraintIndexColumnsArray));
			result.AppendLine(MergeSQLBuilder.CreateMergeSql<IRefComplianceList>(dataSetName, uniqueConstraintIndexColumnsArray, ComplianceListPKChangesTable, true, GetMergInsertStatement(), true));
			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteOrUpdateDependentRecord<IRefComplianceList>(sQLBuilder, fks));
			result.AppendLine(SharedDeleteSQLBuilder.Delete<IRefComplianceList>(sQLBuilder, fks));
			return result.ToString();
		}

		const string ComplianceListPKChangesTable = "@ComplianceListPKChangesTable";
	}
}
