using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Z.EntityFramework.Plus;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public class BulkInsertDuplicateKeyExceptionHandler<T> where T : class
	{
		readonly DatabaseFacade _database;
		readonly SqlException _exception;
		readonly Type _elementType = typeof(T);
		readonly SafeDbContext _entities;

		readonly string _parentTableName;

		IEnumerable<string> _dataSetStructure;

		public BulkInsertDuplicateKeyExceptionHandler(SqlException exception, SafeDbContext entities)
		{
			_exception = exception;
			_database = entities.Database;
			_entities = entities;

			_parentTableName = _elementType.Name;
		}

		static ConcurrentDictionary<string, List<string>> IndexColumnsCache
		{
			get
			{
				_indexColumnsCache ??= new ConcurrentDictionary<string, List<string>>();
				return _indexColumnsCache;
			}
		}
		static ConcurrentDictionary<string, List<string>> _indexColumnsCache;

		public async Task<bool> DeleteDuplicateRecord()
		{
			if (_exception != null
				&& _exception.Message.StartsWith("Cannot insert duplicate key row in object", StringComparison.OrdinalIgnoreCase))
			{
				_dataSetStructure = DataSetStructureProvider.StructuredDataSets?.FirstOrDefault(x => x.Contains(_parentTableName))?.Reverse();
				if (_dataSetStructure == null)
				{
					return false;
				}
				var exceptionMessage = _exception.Message;
				var (record, dbVersionControl) = await FindADuplicateDeletedRecord(exceptionMessage);
				if (record == null || !record.Any())
				{
					return false;
				}
				//create a copy of dbVersionControl as the original is removed from the "query" after RecursiveDeleteAsync is executed.
				var dbVersionControlsCopy = dbVersionControl.ToList();
				var recursiveDeletion = new RecursiveDeletion(_entities);

				//Special case for RefCusTaxOrFeeType dataset.
				//This should be removed when RecursiveDeleteAsync supports NK based deletion.
				if (record is IQueryable<RefCusTaxOrFeeType> taxOrFeeTypes)
				{
					var taxOrFeeRecords = FindRecordsForRefCusTaxOrFeeTypeDeletion(taxOrFeeTypes.First());
					if (taxOrFeeRecords != null && taxOrFeeRecords.Any())
					{
						await recursiveDeletion.RecursiveDeleteAsync(taxOrFeeRecords);
					}
					await record.DeleteAsync();
				}
				else
				{
					await recursiveDeletion.RecursiveDeleteAsync(record);
				}
				//use dbcontext method to remove, as it is not an actual "query" from EF.
				_entities.Set<RefDbVersionControl>().RemoveRange(dbVersionControlsCopy);
				return true;
			}
			return false;
		}

		IQueryable<RefCusTaxOrFee> FindRecordsForRefCusTaxOrFeeTypeDeletion(RefCusTaxOrFeeType record)
		{
			return _entities.Set<RefCusTaxOrFee>().AsNoTracking().Where(x => x.ZZF_ZX0_NKTaxOrFeeType == record.ZX0_TaxOrFeeType);
		}

		async Task<(IQueryable<T> record, IQueryable<RefDbVersionControl> dbVersionControl)> FindADuplicateDeletedRecord(string exceptionMessage)
		{
			var columnNames = await GetColumnsFromIndexName(exceptionMessage);
			if (!columnNames.Any())
			{
				return (null, null);
			}
			var values = GetValuesFromExceptionMessage(exceptionMessage);

			var tblPrefix = typeof(T).GetTablePrefix();
			var pkProperty = typeof(T).GetPKPropertyInfo();
			var pkExp = ExpressionHelper.GetPKExpression<T>();
			var data = _entities.Set<T>().AsNoTracking();
			data = SetWhereConditionToFindDuplicateRecord(columnNames, values, data);
			var dataWithDbVersionControl = data.Join(_entities.Set<RefDbVersionControl>().AsNoTracking().Where(x => x.RVC_Deleted),
						pkExp, x => x.RVC_ParentPK, (r, a) => new { record = r, dbVersionControl = a });
			return (dataWithDbVersionControl.Select(x => x.record), dataWithDbVersionControl.Select(x => x.dbVersionControl));
		}

		static IQueryable<T> SetWhereConditionToFindDuplicateRecord(IEnumerable<string> columnNames, IEnumerable<string> values, IQueryable<T> data)
		{
			if (columnNames.Count() != values.Count())
			{
				throw new InvalidOperationException("Column names and values have different size, therefore can't find a duplicate record.");
			}
			IQueryable<T> result = data;
			for (var x = 0; x < columnNames.Count(); x++)
			{
				var value = values.ElementAt(x);
				if (value.Contains("NULL", StringComparison.InvariantCultureIgnoreCase))
				{
					value = null;
				}
				var property = typeof(T).GetProperty(columnNames.ElementAt(x));
				var convertedValue = TypeExtension.ChangeType(value, property.PropertyType);
				var expression = ExpressionHelper.GetPropertyFiltersExpressionEqualsUsingLikeDbFunction<T>(property, convertedValue);
				result = data.Where(expression);
			}
			return result;
		}

		async Task<IEnumerable<string>> GetColumnsFromIndexName(string exceptionMessage)
		{
			var result = new List<string>();
			var indexNameRegex = Regex.Match(exceptionMessage, @"with unique index '(\w*)'\.");
			if (indexNameRegex.Success
				&& indexNameRegex.Groups.Count > 1)
			{
				var indexName = indexNameRegex.Groups[1].Value;
				if (IndexColumnsCache.TryGetValue(indexName, out var value))
				{
					return value;
				}
				else
				{
					var sql = @"
select 
c.name
from sys.indexes i
join sys.index_columns ic on i.index_id = ic.index_id and ic.object_id = i.object_id
join sys.columns c on c.column_id = ic.column_id and i.object_id = c.object_id
where
i.name = @indexName and ic.is_included_column = 0
order by ic.key_ordinal";

					using (var cmd = _database.GetDbConnection().CreateCommand())
					{
						cmd.CommandText = sql;
						cmd.Transaction = _database.CurrentTransaction.GetDbTransaction();

						var indexNameParam = cmd.CreateParameter();
						indexNameParam.ParameterName = "@indexName";
						indexNameParam.Value = indexName;
						cmd.Parameters.Add(indexNameParam);

						using (var reader = await cmd.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								result.Add(reader.GetString(0));
							}
							_indexColumnsCache[indexName] = result;
						}
					}
				}
			}
			return result;
		}

#if DEBUG
		public static
#endif
		IEnumerable<string> GetValuesFromExceptionMessage(string exceptionMessage)
		{
			var valuesMatch = Regex.Match(exceptionMessage, @"The duplicate key value is \((.*)\)");
			if (valuesMatch.Success
				&& valuesMatch.Groups.Count > 1)
			{
				var values = valuesMatch.Groups[1].Value.Split(',');
				foreach (var val in values)
				{
					yield return val.Trim();
				}
			}
			else
			{
				throw new InvalidOperationException($"The exception message contains a character not recognize by the BulkInsertDuplicateKeyExceptionHandler. \n {exceptionMessage}");
			}
		}
	}
}
