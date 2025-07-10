using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;
using CargoWise.RefDbRepo.Common.Contract_0_9;
using Enterprise.ZArchitecture.Core;
using SqlBulkCopyOptions = CargoWise.Data.Providers.Common.SqlBulkCopyOptions;

namespace CargoWise.RefDataRepo.Ent.Client.DataStorage
{
	public class DBHelper : SchemaInfo, IDBHelper
	{
		public DBHelper(IDbConnection connection, ISqlBulkCopyProvider bulkCopyProvider = null)
			: base(connection)
		{
			Argument.NotNull(connection, nameof(connection));
			this.bulkCopyProvider = bulkCopyProvider ?? new SqlBulkCopyProvider();
		}

		readonly ISqlBulkCopyProvider bulkCopyProvider;

		public async Task BulkInsertAsync(string dataSetName, DataTable dataTable, IDbTransaction transaction)
		{
			await BulkInsertAsync(dataSetName, dataTable, transaction, SQLBuilder.GetTemporaryTableName(dataSetName, dataTable.TableName));
		}

		public async Task BulkInsertAsync(string dataSetName, DataTable dataTable, IDbTransaction transaction, string destinationTable)
		{
			if (dataTable.Rows.Count > 0)
			{
				using (var bulkCopy = bulkCopyProvider.GetSqlBulkCopy((SqlConnection)connection, SqlBulkCopyOptions.Default, (SqlTransaction)transaction))
				{
					bulkCopy.BatchSize = dataTable.Rows.Count;
					bulkCopy.DestinationTableName = destinationTable;
					foreach (DataColumn column in dataTable.Columns)
					{
						bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
					}
					await RefServiceExceptionHandler.ExecuteAsync<Task>(async () => await bulkCopy.WriteToServerAsync(dataTable), RefServiceExceptionHandler.IgnoreOption.IgnoreTimeoutException);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void AddAdditionalFKs(List<ForeignKeyRelationship> foreignKeyRelationshipList)
		{
			if (!foreignKeyRelationshipList.Any(o => o.ReferencedTable == typeof(IRefCurrency) && o.Table == typeof(IRefLanguageText)))
			{
				foreignKeyRelationshipList.Add(new ForeignKeyRelationship
				{
					ReferencedTable = typeof(IRefCurrency),
					ReferencedColumn = nameof(IRefCurrency.RX_PK),
					Table = typeof(IRefLanguageText),
					Column = nameof(IRefLanguageText.RLT_ParentId)
				});
			}
			if (!foreignKeyRelationshipList.Any(o => o.ReferencedTable == typeof(IRefAirline) && o.Table == typeof(IStmNote)))
			{
				foreignKeyRelationshipList.Add(new ForeignKeyRelationship
				{
					ReferencedTable = typeof(IRefAirline),
					ReferencedColumn = nameof(IRefAirline.RM_PK),
					Table = typeof(IStmNote),
					Column = nameof(IStmNote.ST_ParentId),
					Filter = $"[{nameof(IStmNote.ST_Description)}] = 'Terms and Conditions'"
				});
			}
		}

		public (bool, IDisposable) TryToGetDataSetLock(string dataSetName)
		{
			return RefServiceExceptionHandler.Execute(() => TryToGetLock((NoResString)"RefDataRepoLock -" + dataSetName, (NoResString)"Session", null), RefServiceExceptionHandler.IgnoreOption.IgnoreTimeoutException);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		(bool, IDisposable) TryToGetLock(string resource, string lockOwner, IDbTransaction transaction)
		{
			var resultObj = connection.ExecuteScalar(SQLBuilder.GetGettingLockSql(resource, lockOwner), transaction);
			var result = (int)resultObj == 0;
			var retLock = result && lockOwner != "Transaction" ? new RefDbRepo.Common.DisposableAction(() =>
			{
				connection.ExecuteNonQuery(SQLBuilder.GetReleaseLockSql(resource, lockOwner), transaction);
			}) : null;
			return (result, retLock);
		}

		public void CreateTemporaryTable<TStorage>(string dataSetName, IEnumerable<Tuple<string, string>> columnsAndAlias, IDbTransaction transaction = null)
		{
			CreateTemporaryTable<TStorage>(dataSetName, columnsAndAlias, SharedSQLBuilder.GetTableName(typeof(TStorage)), transaction);
		}

		public void CreateTemporaryTable<TStorage>(string dataSetName, IEnumerable<Tuple<string, string>> columnsAndAlias, string tableName, IDbTransaction transaction = null)
		{
			connection.ExecuteNonQuery(SQLBuilder.GetCreateTemporaryTableSql<TStorage>(dataSetName, tableName, columnsAndAlias), transaction);
		}

		public void DeleteTemporaryTable<TStorage>(string dataSetName, IDbTransaction transaction)
		{
			var tempTableName = SQLBuilder.GetTemporaryTableName<TStorage>(dataSetName);
			connection.ExecuteNonQuery(SQLBuilder.GetDeleteTemporaryTableSql(tempTableName), transaction);
		}

		public IDbTransaction BeginTransaction()
		{
			return connection.BeginTransaction();
		}

		public IDataReader ExecuteReader(string sqlText, IDbTransaction transaction = null, int? timeout = null, params Func<IDbCommand, IDbDataParameter>[] createParams)
		{
			return connection.ExecuteReader(sqlText, transaction, timeout, createParams);
		}

		public int ExecuteNonQuery(string sqlText, IDbTransaction transaction = null, int? timeout = null, params Func<IDbCommand, IDbDataParameter>[] createParams)
		{
			return connection.ExecuteNonQuery(sqlText, transaction, timeout, createParams);
		}
		public string LoadDbExtendedProperty(string propertyName, IDbTransaction transaction)
		{
			string sqlText = FormattableString.Invariant($@"SELECT value FROM sys.extended_properties WHERE class = 0 AND name = '{propertyName}'");
			var objectValue = connection.ExecuteScalar(sqlText, transaction);
			var result = (objectValue == null || objectValue == DBNull.Value) ? null : objectValue.ToString();
			return result;
		}

		public void SaveDbExtendedProperty(string propertyName, string value, IDbTransaction transaction)
		{
			var sqlText = SQLBuilder.SaveDbExtendedProperty(propertyName, value);

			ExecuteNonQuery(sqlText, transaction, null);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClassesAnalyzer", Justification = "Baseline")]
		public void Fill(string sqlText, DataTable dataTable)
		{
			using (var adapter = new SqlDataAdapter(sqlText, (SqlConnection)connection))
			{
				adapter.Fill(dataTable);
			}
		}

		public IEnumerable<IDataSetInfo> GetDataSetInfos(IServerProxy proxy)
		{
			return new MainDbUpdaterRegistration().Get(proxy, this);
		}

		public bool IsRefDatabaseSynonymAllPointToSRDb()
		{
			var sqlText = $@"IF EXISTS (SELECT NULL FROM sys.synonyms WHERE name LIKE '{RefDbTableNameResolver.SingleRefDatabaseNameSynonymPrefix}_%' AND base_object_name NOT LIKE '\[{RefDbTableNameResolver.SingleRefDatabaseName}].%' ESCAPE '\') SELECT 0 ELSE SELECT 1";
			return Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture);
		}

		protected override IEnumerable<Type> GetAllStorageTypes()
		{
#if DEBUG
			if (this.types != null)
			{
				return this.types;
			}
#endif
			return typeof(IDBHelper).Assembly.GetTypes().Concat(Assembly.GetExecutingAssembly().GetTypes())
				.Where(x => typeof(IDataSetStorage).IsAssignableFrom(x));
		}

#if DEBUG
		public void SetAllStorageTypes(IEnumerable<Type> types)
		{
			this.types = types;
		}
		IEnumerable<Type> types;
#endif

		public string GetFKColumn(Type storageType, Type childStorageType, PropertyInfo prop = null)
		{
			var fkColumn = string.Empty;
			if (storageType == typeof(IRefTimeZoneSet))
			{
				switch (prop.Name)
				{
					case nameof(RefTimeZoneSet.RefTimeZoneStandardZone):
						{
							fkColumn = nameof(IRefTimeZoneSet.R3_R2_StandardZone);
							break;
						}
					case nameof(RefTimeZoneSet.RefTimeZoneDaylightSavingZone):
						{
							fkColumn = nameof(IRefTimeZoneSet.R3_R2_DaylightSavingZone);
							break;
						}
				}
			}

			if (string.IsNullOrEmpty(fkColumn))
			{
				fkColumn = SharedSQLBuilder.GetFKColumns(storageType, childStorageType).FirstOrDefault();
			}
			return fkColumn;
		}

		public IEnumerable<ForeignKeyRelationship> GetReferencedForeignKeys()
		{
			var foreignKeyRelationshipList = new List<ForeignKeyRelationship>();
			foreignKeyRelationshipList.AddRange(ForeignKeyRelationships);
			foreignKeyRelationshipList.AddRange(FKProvider.AdditionalFKs);
			AddAdditionalFKs(foreignKeyRelationshipList);
			return foreignKeyRelationshipList;
		}

		IEnumerable<ForeignKeyRelationship> ForeignKeyRelationships
		{
			get
			{
				yield return new ForeignKeyRelationship
				{
					Table = typeof(IRefAirlineEFreightRule),
					Column = nameof(IRefAirlineEFreightRule.RME_RM),
					ReferencedTable = typeof(IRefAirline),
					ReferencedColumn = nameof(IRefAirline.RM_PK),
					IsNullable = false,
					ColumnPartOfUniqueIndex = true
				};
				yield return new ForeignKeyRelationship
				{
					Table = typeof(IRefShippingLineEBLProvider),
					Column = nameof(IRefShippingLineEBLProvider.RSE_RSL_ShippingLine),
					ReferencedTable = typeof(IRefShippingLine),
					ReferencedColumn = nameof(IRefShippingLine.RSL_PK),
					IsNullable = false,
					ColumnPartOfUniqueIndex = true
				};
				yield return new ForeignKeyRelationship
				{
					Table = typeof(IRefShippingLineMessagingRequirement),
					Column = nameof(IRefShippingLineMessagingRequirement.RSR_RSL_ShippingLine),
					ReferencedTable = typeof(IRefShippingLine),
					ReferencedColumn = nameof(IRefShippingLine.RSL_PK),
					IsNullable = false,
					ColumnPartOfUniqueIndex = true
				};
				yield return new ForeignKeyRelationship
				{
					Table = typeof(IOrgHeader),
					Column = nameof(IOrgHeader.OH_RSL_ShippingLine),
					ReferencedTable = typeof(IRefShippingLine),
					ReferencedColumn = nameof(IRefShippingLine.RSL_PK),
					IsNullable = true,
					ColumnPartOfUniqueIndex = false
				};

				yield return new ForeignKeyRelationship
				{
					Table = typeof(IRefTimeZoneRule),
					Column = nameof(IRefTimeZoneRule.R4_R2),
					ReferencedTable = typeof(IRefTimeZone),
					ReferencedColumn = nameof(IRefTimeZone.R2_PK),
					IsNullable = false,
					ColumnPartOfUniqueIndex = true
				};
				yield return new ForeignKeyRelationship
				{
					Table = typeof(IRefTimeZoneSet),
					Column = nameof(IRefTimeZoneSet.R3_R2_DaylightSavingZone),
					ReferencedTable = typeof(IRefTimeZone),
					ReferencedColumn = nameof(IRefTimeZone.R2_PK),
					IsNullable = true,
					ColumnPartOfUniqueIndex = true
				};
				yield return new ForeignKeyRelationship
				{
					Table = typeof(IRefTimeZoneSet),
					Column = nameof(IRefTimeZoneSet.R3_R2_StandardZone),
					ReferencedTable = typeof(IRefTimeZone),
					ReferencedColumn = nameof(IRefTimeZone.R2_PK),
					IsNullable = false,
					ColumnPartOfUniqueIndex = true
				};

				yield return new ForeignKeyRelationship
				{
					Table = typeof(IRefUNLOCO),
					Column = nameof(IRefUNLOCO.RL_R3),
					ReferencedTable = typeof(IRefTimeZoneSet),
					ReferencedColumn = nameof(IRefTimeZoneSet.R3_PK),
					IsNullable = true,
					ColumnPartOfUniqueIndex = false
				};
				yield return new ForeignKeyRelationship
				{
					Table = typeof(IRefUNLOCO),
					Column = nameof(IRefUNLOCO.RL_RW),
					ReferencedTable = typeof(IRefCountryStates),
					ReferencedColumn = nameof(IRefCountryStates.RW_PK),
					IsNullable = true,
					ColumnPartOfUniqueIndex = false
				};
				yield return new ForeignKeyRelationship
				{
					Table = typeof(IRefLocoMap),
					Column = nameof(IRefLocoMap.RY_RN),
					ReferencedTable = typeof(IRefCountry),
					ReferencedColumn = nameof(IRefCountry.RN_PK),
					IsNullable = true,
					ColumnPartOfUniqueIndex = true
				};
				yield return new ForeignKeyRelationship
				{
					Table = typeof(IUNDGAttribute),
					Column = nameof(IUNDGAttribute.DA_DG),
					ReferencedTable = typeof(IZZUNDGSubstance),
					ReferencedColumn = nameof(IZZUNDGSubstance.DG_PK),
					IsNullable = false,
					ColumnPartOfUniqueIndex = true
				};
				yield return new ForeignKeyRelationship
				{
					Table = typeof(IUNDGCountryReferencePivot),
					Column = nameof(IUNDGCountryReferencePivot.DCP_DCR),
					ReferencedTable = typeof(IUNDGCountryReference),
					ReferencedColumn = nameof(IUNDGCountryReference.DCR_PK),
					IsNullable = false,
					ColumnPartOfUniqueIndex = true
				};
			}
		}
	}
}
