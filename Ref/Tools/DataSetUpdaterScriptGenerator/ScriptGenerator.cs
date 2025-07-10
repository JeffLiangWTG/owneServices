using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public class ScriptGenerator : IScriptGenerator
	{
		public ScriptGenerator(ISQLBuilder sQLBuilder, ISchemaInfo schemaInfo, IEnumerable<IDataSetInfo> updaterInfos)
		{
			this.schemaInfo = schemaInfo;
			this.sQLBuilder = sQLBuilder;
			this.updaterInfos = updaterInfos;
		}
		readonly ISchemaInfo schemaInfo;
		readonly ISQLBuilder sQLBuilder;
		readonly IEnumerable<IDataSetInfo> updaterInfos;

		IUpdaterDependencyProvider<IDataSetInfo> UpdaterDependencyProvider
		{
			get
			{
				if (updaterDependencyProvider == null)
				{
					updaterDependencyProvider = new UpdaterDependencyProvider<IDataSetInfo>(updaterInfos.ToArray(), ForeignKeyRelationships.ToArray());
					updaterDependencyProvider.Initialize();
				}
				return updaterDependencyProvider;
			}
		}
		IUpdaterDependencyProvider<IDataSetInfo> updaterDependencyProvider;

		IEnumerable<ForeignKeyRelationship> ForeignKeyRelationships
		{
			get
			{
				if (foreignKeyRelationships == null)
				{
					foreignKeyRelationships = schemaInfo.GetReferencedForeignKeysFromDb(null).ToArray();
				}
				return foreignKeyRelationships;
			}
		}
		IEnumerable<ForeignKeyRelationship> foreignKeyRelationships;

		public IEnumerable<(string, string)> GeneratePrepareTemporaryTablesScripts(IDataSetUpdaterInfo info)
		{
			var idx = 0;
			foreach (var type in info.GetTypeByInsertOrder())
			{
				idx++;
				yield return ((string, string))this.InvokeGenericMethod(nameof(ScriptGenerator.CreateTemporaryTableScript), type.Item1, new[] { typeof(bool) }, idx == 1);
			}
			foreach (var type in info.GetReferenceTypeByInsertOrder())
			{
				yield return ((string, string))this.InvokeGenericMethod(nameof(ScriptGenerator.CreateReferenceTemporaryTable), type.Item1, new[] { typeof(IEnumerable<IndexColumn[]>) },
					schemaInfo.InvokeGenericMethod(nameof(ISchemaInfo.GetAllUniqueIndexes), type.Item1, new[] { typeof(IDbTransaction) }, (IDbTransaction)null));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
		(string, string) CreateTemporaryTableScript<TStorage>(bool shouldCreateDeleteColumn)
		{
			var allColumns = SharedSQLBuilder.GetAllColumnProperties<TStorage>().Select(x => Tuple.Create(x.Name, x.Name)).ToList();
			if (shouldCreateDeleteColumn)
			{
				allColumns.Add(SharedSQLBuilder.CreateDeletedColumn());
			}
			return (SQLBuilder.GetTemporaryTableName<TStorage>(string.Empty), SQLBuilder.GetCreateTemporaryTableSql<TStorage>(string.Empty, SharedSQLBuilder.GetTableName(typeof(TStorage)), allColumns));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
		(string, string) CreateReferenceTemporaryTable<TStorage>(IEnumerable<IndexColumn[]> uniqueIndexColumns)
		{
			var allColumns = uniqueIndexColumns.SelectMany(item => item).Select(x => x.Column).ToArray().Distinct();
			return (SQLBuilder.GetTemporaryTableName<TStorage>(string.Empty), SQLBuilder.GetCreateTemporaryTableSql<TStorage>(string.Empty, SharedSQLBuilder.GetTableName(typeof(TStorage)), allColumns.Concat(new[] { SharedSQLBuilder.GetPKColumn<TStorage>() }).Select(x => Tuple.Create(x, x))));
		}

		public string GenerateMergeScript(IDataSetUpdaterInfo info)
		{
			return (string)this.InvokeGenericMethod(nameof(GenerateMergeScriptCore), info.GetStorageType(), new[] { typeof(IDataSetUpdaterInfo) }, info);
		}

		string GenerateMergeScriptCore<TStorage>(IDataSetUpdaterInfo info)
		{
			var result = new StringBuilder();
			var fks = FKProvider.SetActionToFKS(ForeignKeyRelationships, updaterInfos);
			result.AppendLine(sQLBuilder.CreateDeclarationSqlBeforeMerge());
			result.AppendLine(SharedDeleteSQLBuilder.DeclareDeleteTables<TStorage>(sQLBuilder, fks));
			result.AppendLine(info.GetMergeSqlText(sQLBuilder, fks, schemaInfo));
			result.AppendLine(sQLBuilder.DeleteTemporaryTablesData(string.Empty, GetStorageTypes(info)));
			return result.ToString();
		}

		static IEnumerable<Type> GetStorageTypes(IDataSetUpdaterInfo info)
		{
			return info.GetTypeByInsertOrder().Concat(info.GetReferenceTypeByInsertOrder()).Select(x => x.Item1);
		}

		public IEnumerable<string> GetPrerequisites(IDataSetInfo info)
		{
			var prerequisites = GetPrerequisitesFromDataSet(info);
			prerequisites = prerequisites ?? Enumerable.Empty<string>();
			var storageType = ((IDataSetUpdaterInfo)info).GetStorageType();
			if (extraPrerequisitesDictionary.ContainsKey(storageType))
			{
				var extraPrerequisite = extraPrerequisitesDictionary[storageType];
				foreach (var prerequisite in extraPrerequisite)
				{
					if (!prerequisites.Contains(prerequisite.Name))
					{
						prerequisites = prerequisites.Append(prerequisite.Name);
					}
				}
			}
			return prerequisites;
		}

		IEnumerable<string> GetPrerequisitesFromDataSet(IDataSetInfo info)
		{
			var visited = new HashSet<string>();
			GetPrerequisitesFromDataSet(info, visited);
			return visited;
		}

		void GetPrerequisitesFromDataSet(IDataSetInfo info, HashSet<string> visited)
		{
			var parents = UpdaterDependencyProvider.GetParents(info);
			foreach (var parent in parents)
			{
				if (visited.Add(((IDataSetUpdaterInfo)parent).GetStorageType().Name))
				{
					GetPrerequisitesFromDataSet(parent, visited);	
				}
			}
		}

		//generator can't get below dependencies as these are special cases, hence adding manually
		readonly Dictionary<Type, Type[]> extraPrerequisitesDictionary = new Dictionary<Type, Type[]>()
		{
			{ typeof(IRefCusTariff), new[] { typeof(IRefCusPreference) } },
			{ typeof(IRefCusNomenclatureGroup), new[] { typeof(IRefCusPreference) } },
			{ typeof(IRefCusCodeList), new[] { typeof(IRefCusCodeListAttributeName) } }
		};
	}
}
