using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class ModelViewColumnQueryHelper<T> where T : BusinessObject
	{
		public ZQuery GetModuleFilterQuery(ZString foreignKey, ZString modelViewPK, ZString modelView, bool notIn, (ZString filterColumn, object filterValue)[] filterColumnValuePairs)
		{
			return GetModuleFilterQueryCore(foreignKey, modelViewPK, modelView, notIn, filterColumnValuePairs.Select(filterColumnValuePair => (filterColumnValuePair.filterColumn, SQLComparisonOperator.Equal, filterColumnValuePair.filterValue)));
		}

		public ZQuery GetModuleFilterQuery(ZString foreignKey, ZString modelViewPK, ZString modelView, bool notIn, (ZString filterColumn, SQLComparisonOperator op, object filterValue)[] filterColumnValuePairs)
		{
			return GetModuleFilterQueryCore(foreignKey, modelViewPK, modelView, notIn, filterColumnValuePairs);
		}

		public ZQuery GetModuleFilterQuery(ZString foreignKey, ZString modelViewPK, ZString modelView, ZString filterColumn, SQLComparisonOperator op, object value)
		{
			return GetModuleFilterQueryCore(foreignKey, modelViewPK, modelView, notIn: false, filterColumnValuePairs: new [] { (filterColumn, op, value) });
		}

		ZQuery GetModuleFilterQueryCore(ZString foreignKey, ZString modelViewPK, ZString modelView, bool notIn, IEnumerable<(ZString filterColumn, SQLComparisonOperator op, object filterValue)> filterColumnValuePairs)
		{
			var notOrBlank = ZString.Empty;
			if (notIn)
			{
				notOrBlank = "NOT ";
			}

			var sqlFilterParams = new ZSqlParameterCollection();
			var list = new List<ZString>();
			var parameterIndex = 0;

			foreach (var filter in filterColumnValuePairs)
			{
				var filterOperator = GetOperatorString(filter.op);
				var filterColumn = filter.filterColumn;
				var filterValue = filter.filterValue;
				var filterValueType = filterValue.GetType();

				if (string.IsNullOrEmpty(filterOperator) && (filterValueType == typeof(ZString) || filterValueType == typeof(string)))
				{
					if (filter.op == SQLComparisonOperator.StartsWith)
					{
						filterOperator = "LIKE";
						filterValue = filterValue + "%";
					}
					else if (filter.op == SQLComparisonOperator.Contains)
					{
						filterOperator = "LIKE";
						filterValue = "%" + filterValue + "%";
					}
					else if (filter.op == SQLComparisonOperator.DoesNotStartWith)
					{
						filterOperator = (NoResString)"NOT LIKE";
						filterValue = filterValue + "%";
					}
					else if (filter.op == SQLComparisonOperator.NotContains)
					{
						filterOperator = (NoResString)"NOT LIKE";
						filterValue = "%" + filterValue + "%";
					}
				}

				list.Add($"{filterColumn} {filterOperator} @p{parameterIndex}");
				sqlFilterParams.Add($"@p{parameterIndex}", filterValue, GetDummySchemaColumnForColumnInModelView(filterColumn, filterValueType));
				parameterIndex++;
			}
			var queryText = $"{foreignKey} {notOrBlank}IN (SELECT {modelViewPK} FROM dbo.{modelView} WHERE {ZString.Join(" AND ", list.ToArray())})";
			var result = new ZDBOnlyQuery(typeof(T));
			result.AddFilterAndZSQLParameterCollection(queryText, sqlFilterParams);
			return result;
		}

		public ZQuery GetModuleFilterQueryForNull(ZString foreignKey, ZString modelViewPK, ZString modelView, bool notIn, ZString filterColumn, bool isNot = false)
		{
			ZString notOrBlank = notIn ? "NOT " : ZString.Empty;
			ZString isNotOrBlank = isNot ? "NOT " : ZString.Empty;

			var queryText = $@"{foreignKey} {notOrBlank}IN (SELECT {modelViewPK} FROM dbo.{modelView} WHERE {filterColumn} IS {isNotOrBlank}NULL)";
			var result = new ZDBOnlyQuery(typeof(T));
			result.AddFilterAndZSQLParameterCollection(queryText, new ZSqlParameterCollection());
			return result;
		}

		string GetOperatorString(SQLComparisonOperator op)
		{
			if (op == SQLComparisonOperator.Equal || op == SQLComparisonOperator.IsBlank)
			{
				return "=";
			}

			if (op == SQLComparisonOperator.NotEqual || op == SQLComparisonOperator.IsNotBlank)
			{
				return "<>";
			}

			if (op == SQLComparisonOperator.GreaterThan)
			{
				return ">";
			}

			if (op == SQLComparisonOperator.GreaterThanOrEqualTo)
			{
				return ">=";
			}

			if (op == SQLComparisonOperator.LessThan)
			{
				return "<";
			}

			if (op == SQLComparisonOperator.LessThanOrEqualTo)
			{
				return "<=";
			}

			return null;
		}

		public ZQuery GetDateFilterQuery(ZString foreignKey, ZString modelViewPK, ZString modelView, ZString filterColumn, DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			var result = new ZQuery();
			var queryText = $"{foreignKey} IN (SELECT {modelViewPK} FROM dbo.{modelView} WHERE {filterColumn} ";
			var sqlFilterParams = new ZSqlParameterCollection();
			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered || comparisonOperator == DateComparisonOperator.HasDateEntered)
			{
				var notOrBlank = comparisonOperator == DateComparisonOperator.HasDateEntered ? "NOT " : "";
				queryText += $"IS {notOrBlank}NULL)";
			}
			else if (comparisonOperator == DateComparisonOperator.HasDateInRange)
			{
				if (startDate.IsValid && endDate.IsValid)
				{
					queryText += $">= @StartDate AND {filterColumn} < @EndDate)";
					endDate = endDate.Date.ToZDateTime().AddDays(1);
				}
				else if (startDate.IsValid)
				{
					queryText += $">= @StartDate)";
				}
				else if (endDate.IsValid)
				{
					queryText += $"< @EndDate)";
					endDate = endDate.Date.ToZDateTime().AddDays(1);
				}
				else
				{
					queryText += ")";
				}
				var schemaColumn = GetDummySchemaColumnForColumnInModelView(filterColumn, typeof(ZDateTime));
				sqlFilterParams.Add("@StartDate", startDate, schemaColumn);
				sqlFilterParams.Add("@EndDate", endDate, schemaColumn);
			}

			result.AddFilterAndZSQLParameterCollection(queryText, sqlFilterParams);
			return result;
		}

		SchemaColumn GetDummySchemaColumnForColumnInModelView(ZString columnName, Type propertyType)
		{
			if (propertyType == typeof(ZDecimal) || propertyType == typeof(decimal))
			{
				return new SchemaDecimalColumn(Schema.GenericTableSchema, columnName, 0, SqlDbType.Money, (decimal)0, false, 19, 4);
			}
			if (propertyType == typeof(ZInt) || propertyType == typeof(int) || propertyType == typeof(ZShort) || propertyType == typeof(short) || propertyType == typeof(ZByte) || propertyType == typeof(byte))
			{
				return new SchemaIntColumn(Schema.GenericTableSchema, columnName, 0, 0, false);
			}
			if (propertyType == typeof(ZBool) || propertyType == typeof(bool))
			{
				return new SchemaBoolColumn(Schema.GenericTableSchema, columnName, 0, false, true, false);
			}
			if (propertyType == typeof(ZGuid) || propertyType == typeof(Guid))
			{
				return new SchemaGuidColumn(Schema.GenericTableSchema, columnName, 0, Guid.Empty, false);
			}
			if (propertyType == typeof(ZDateTime) || propertyType == typeof(DateTime) || propertyType == typeof(ZDate))
			{
				return new SchemaDateTimeColumn(Schema.GenericTableSchema, columnName, 0, SqlDbType.DateTime, ZDateTime.Now, false);
			}
			if (propertyType == typeof(ZDateTimeOffset) || propertyType == typeof(DateTimeOffset))
			{
				return new SchemaDateTimeOffsetColumn(Schema.GenericTableSchema, columnName, 0, SqlDbType.DateTimeOffset, ZDateTimeOffset.Now, false, 4);
			}

			return new SchemaStringColumn(Schema.GenericTableSchema, columnName, 0, SqlDbType.NVarChar, "", false, 0);
		}
	}
}
