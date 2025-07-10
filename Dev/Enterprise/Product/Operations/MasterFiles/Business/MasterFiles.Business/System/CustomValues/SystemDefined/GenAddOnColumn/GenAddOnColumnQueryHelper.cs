using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	public class GenAddOnColumnQueryHelper
	{
		#region Foreign Key Link

		public class ForeignKeyLink
		{
			public ForeignKeyLink(Type entity, SchemaColumn foreignKey)
			{
				this.Entity = entity;
				this.ForeignKey = foreignKey;
			}

			public readonly Type Entity;
			public readonly SchemaColumn ForeignKey;
		}

		#endregion

		public GenAddOnColumnQueryHelper(Type topLevel, List<ForeignKeyLink> orderedLinksTopLevelDownToEntityWithAddonColumn)
		{
			typeOfBusinessObject = topLevel;
			this.orderedLinksTopLevelDownToEntityWithAddonColumn = orderedLinksTopLevelDownToEntityWithAddonColumn;
		}

		public GenAddOnColumnQueryHelper(Type typeOfBusinessObject, Type typeOfBusinessObjectToSubQuery, SchemaColumn foreignKey)
		{
			this.typeOfBusinessObject = typeOfBusinessObject;

			orderedLinksTopLevelDownToEntityWithAddonColumn = new List<ForeignKeyLink>();
			if (typeOfBusinessObjectToSubQuery != null && foreignKey != null)
			{
				orderedLinksTopLevelDownToEntityWithAddonColumn.Add(new ForeignKeyLink(typeOfBusinessObjectToSubQuery, foreignKey));
			}
		}

		public GenAddOnColumnQueryHelper(Type typeOfBusinessObject)
			: this(typeOfBusinessObject, null, null)
		{
		}

		readonly Type typeOfBusinessObject;
		readonly List<ForeignKeyLink> orderedLinksTopLevelDownToEntityWithAddonColumn;

		#region Queries

		public ZDBOnlySubQuery GetContainsValueForAnyQuery(bool notIn, params string[] columnNames)
		{
			var query = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID, notIn);
			query.AddToFilter(GenAddOnColumnSchema.XA_Name, columnNames);
			return query;
		}

		public ZQuery GetQueryWithSubQueryOnGenAddOnColumn(string columnName, ZString value)
		{
			return GetQueryWithSubQueryOnGenAddOnColumn(columnName, SQLComparisonOperator.Equal, value, false);
		}

		public ZQuery GetQueryWithSubQueryOnGenAddOnColumn(string columnName, ZString value, bool notIn)
		{
			return GetQueryWithSubQueryOnGenAddOnColumn(columnName, SQLComparisonOperator.Equal, value, notIn);
		}

		public ZQuery GetQueryWithSubQueryOnGenAddOnColumn(string columnName, SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetQueryWithSubQueryOnGenAddOnColumn(columnName, comparisonOperator, value, false);
		}

		public ZQuery GetQueryWithSubQueryOnGenAddOnColumn(string columnName, SQLComparisonOperator comparisonOperator, ZString value, bool notIn)
		{
			if (!orderedLinksTopLevelDownToEntityWithAddonColumn.Any())
			{
				throw new InvalidOperationException(InvalidOperationExceptionMessage);
			}

			var result = new ZQuery();

			if (!value.IsEmpty)
			{
				var addOnColumnSubQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID, notIn);
				addOnColumnSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, columnName);
				addOnColumnSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, comparisonOperator, value);

				result = GetQuery(addOnColumnSubQuery);
			}

			return result;
		}

		public ZQuery GetQueryWithSubQueryOnGenAddOnColumn_PossiblyCommaSeparated(string columnName, ZString value)
		{
			return GetQueryWithSubQueryOnGenAddOnColumn_PossiblyCommaSeparated(columnName, SQLComparisonOperator.Equal, value, false);
		}

		public ZQuery GetQueryWithSubQueryOnGenAddOnColumn_PossiblyCommaSeparated(string columnName, ZString value, bool notIn)
		{
			return GetQueryWithSubQueryOnGenAddOnColumn(columnName, SQLComparisonOperator.Equal, value, notIn);
		}

		public ZQuery GetQueryWithSubQueryOnGenAddOnColumn_PossiblyCommaSeparated(string columnName, SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetQueryWithSubQueryOnGenAddOnColumn(columnName, comparisonOperator, value, false);
		}

		public ZQuery GetQueryWithSubQueryOnGenAddOnColumn_PossiblyCommaSeparated(string columnName, SQLComparisonOperator comparisonOperator, ZString value, bool notIn)
		{
			if (!orderedLinksTopLevelDownToEntityWithAddonColumn.Any())
			{
				throw new InvalidOperationException(InvalidOperationExceptionMessage);
			}

			var result = new ZQuery();

			if (!value.IsEmpty)
			{
				var addOnColumnSubQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID, notIn);
				addOnColumnSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, columnName);
				addOnColumnSubQuery.AddToFilter_PossiblyCommaSeparated(GenAddOnColumnSchema.XA_Data, comparisonOperator, value);

				result = GetQuery(addOnColumnSubQuery);
			}

			return result;
		}

		public ZQuery GetQueryWithSubQueryOnGenAddOnColumn(string columnName, DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			if (!orderedLinksTopLevelDownToEntityWithAddonColumn.Any())
			{
				throw new InvalidOperationException(InvalidOperationExceptionMessage);
			}

			var result = new ZQuery();

			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered || comparisonOperator == DateComparisonOperator.HasDateEntered)
			{
				var notIn = comparisonOperator == DateComparisonOperator.HasNoDateEntered;

				var addOnColumnSubQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID, notIn);
				addOnColumnSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, columnName);

				result = GetQuery(addOnColumnSubQuery);
			}
			else if (comparisonOperator == DateComparisonOperator.HasDateInRange)
			{
				var addOnColumnSubQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
				addOnColumnSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, columnName);

				if (startDate.IsValid)
				{
					addOnColumnSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, startDate.SqlFormat);
				}

				if (endDate.IsValid)
				{
					addOnColumnSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, SQLComparisonOperator.LessThan, endDate.Date.ToZDateTime().AddDays(1).SqlFormat);
				}

				result = GetQuery(addOnColumnSubQuery);
			}

			return result;
		}

		public ZQuery GetQueryWithSubQueryOnDataColumn(SchemaColumn columnName, ZDBOnlySubQuery subQuery)
		{
			var result = new ZDBOnlyQuery(typeOfBusinessObject);
			var query = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			query.AddToFilter(GenAddOnColumnSchema.XA_Name, columnName.Name);
			query.AddSubQuery(GenAddOnColumnSchema.XA_Data, subQuery, JoinCondition.And);
			result.AddSubQuery(query, JoinCondition.And);
			return result;
		}

		ZQuery GetQuery(ZDBOnlySubQuery addOnColumnSubQuery)
		{
			var result = new ZDBOnlyQuery(typeOfBusinessObject);

			var entityToLinkAddOnColumnTo = orderedLinksTopLevelDownToEntityWithAddonColumn.Last();
			var query = new ZDBOnlySubQuery(entityToLinkAddOnColumnTo.Entity, entityToLinkAddOnColumnTo.ForeignKey);
			query.AddSubQuery(addOnColumnSubQuery, JoinCondition.And);

			if (orderedLinksTopLevelDownToEntityWithAddonColumn != null)
			{
				for (int i = orderedLinksTopLevelDownToEntityWithAddonColumn.Count - 2; i >= 0; i--)
				{
					var link = orderedLinksTopLevelDownToEntityWithAddonColumn[i];

					var linkToSubQuery = new ZDBOnlySubQuery(link.Entity, link.ForeignKey);
					linkToSubQuery.AddSubQuery(query, JoinCondition.And);
					query = linkToSubQuery;
				}
			}

			result.AddSubQuery(query, JoinCondition.And);

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception Message")]
		internal const string InvalidOperationExceptionMessage = "For GetQueryWithSubQueryOnGenAddOnColumn you should use a contructor with TypeOfBusinessObjectToSubQuery and ForeignKey. TypeOfBusinessObjectToSubQuery and ForeignKey should NOT be null";

		#endregion

		#region Simple Query

		public ZDBOnlyQuery GetQueryOnGenAddOnColumn(string columnName, ZString value)
		{
			return GetQueryOnGenAddOnColumn(columnName, SQLComparisonOperator.Equal, value, false);
		}

		public ZDBOnlyQuery GetQueryOnGenAddOnColumn(string columnName, ZString value, bool notIn)
		{
			return GetQueryOnGenAddOnColumn(columnName, SQLComparisonOperator.Equal, value, notIn);
		}

		public ZDBOnlyQuery GetQueryOnGenAddOnColumn(string columnName, SQLComparisonOperator comparisonOperator, ZString value, string parentTableCode = null)
		{
			return GetQueryOnGenAddOnColumn(columnName, comparisonOperator, value, false, parentTableCode);
		}

		public ZDBOnlyQuery GetQueryOnGenAddOnColumn(string columnName, SQLComparisonOperator comparisonOperator, ZString value, bool notIn, string parentTableCode = null)
		{
			var result = new ZDBOnlyQuery(typeOfBusinessObject);
			if (!value.IsEmpty)
			{
				result.AddSubQuery(GetSubQuery(columnName, comparisonOperator, new[] { value }, notIn, parentTableCode), JoinCondition.And);
			}
			return result;
		}

		public ZDBOnlyQuery GetQueryOnGenAddOnColumnNotInWhenValueIsEmpty(string columnName, SQLComparisonOperator comparisonOperator, ZString value, bool notIn, string parentTableCode = null)
		{
			var result = new ZDBOnlyQuery(typeOfBusinessObject);
			result.AddSubQuery(GetSubQuery(columnName, comparisonOperator, new[] { value }, notIn, parentTableCode), JoinCondition.And);
			return result;
		}

		public ZDBOnlyQuery GetQueryOnGenAddOnColumn(string columnName, SQLComparisonOperator comparisonOperator, ZString[] values, bool notIn, string parentTableCode = null)
		{
			var result = new ZDBOnlyQuery(typeOfBusinessObject);
			if (values.All(x => !x.IsEmpty))
			{
				result.AddSubQuery(GetSubQuery(columnName, comparisonOperator, values, notIn, parentTableCode), JoinCondition.And);
			}
			return result;
		}
		public ZDBOnlyQuery GetQueryOnGenAddOnColumn(string columnName, SQLComparisonOperator comparisonOperator, ZString[] values, bool notIn, SchemaColumn overrideKeyColumn, string parentTableCode = null)
		{
			var result = new ZDBOnlyQuery(typeOfBusinessObject);
			if (values.All(x => !x.IsEmpty))
			{
				result.AddSubQuery(overrideKeyColumn, GetSubQuery(columnName, comparisonOperator, values, notIn, parentTableCode), JoinCondition.And);
			}
			return result;
		}

		public ZQuery GetQueryOnGenAddOnColumn(string columnName, DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			var result = new ZQuery();

			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				result = new ZDBOnlyQuery(typeOfBusinessObject);
				var addOnColumnSubQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID, true);
				addOnColumnSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, columnName);
				((ZDBOnlyQuery)result).AddSubQuery(addOnColumnSubQuery, JoinCondition.And);
			}
			else if (comparisonOperator == DateComparisonOperator.HasDateEntered)
			{
				result = new ZDBOnlyQuery(typeOfBusinessObject);
				var addOnColumnSubQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID, false);
				addOnColumnSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, columnName);
				((ZDBOnlyQuery)result).AddSubQuery(addOnColumnSubQuery, JoinCondition.And);
			}
			else if (comparisonOperator == DateComparisonOperator.HasDateInRange)
			{
				result = new ZDBOnlyQuery(typeOfBusinessObject);
				var addOnColumnSubQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
				addOnColumnSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, columnName);

				if (startDate.IsValid)
				{
					addOnColumnSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, startDate.SqlFormat);
				}

				if (endDate.IsValid)
				{
					addOnColumnSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, SQLComparisonOperator.LessThan, endDate.Date.ToZDateTime().AddDays(1).SqlFormat);
				}

				((ZDBOnlyQuery)result).AddSubQuery(addOnColumnSubQuery, JoinCondition.And);
			}

			return result;
		}

		public ZQuery GetQueryOnGenAddOnColumn(string columnName, INumericZType value1, INumericZType value2)
		{
			var result = new ZDBOnlyQuery(typeOfBusinessObject);
			var addOnColumnSubQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			addOnColumnSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, columnName);
			addOnColumnSubQuery.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "TRY_PARSE({0} AS DECIMAL) BETWEEN {1} AND {2}", GenAddOnColumnSchema.Constants.XA_Data, value1, value2), null);
			result.AddSubQuery(addOnColumnSubQuery, JoinCondition.And);

			return result;
		}

		public ZDBOnlyQuery GetQueryHandlingBlanks(string columnName, SQLComparisonOperator op, ZString value, bool ignoreEmpty = true)
		{
			var result = new ZDBOnlyQuery(typeOfBusinessObject);

			var isNegativeComparisonOperator = OperatorsDictionary.ContainsKey(op);
			if (value.IsEmpty && (!ignoreEmpty || op == SpecialComparisonOperator.IsBlank || op == SpecialComparisonOperator.IsNotBlank))
			{
				result.AddSubQuery(GetContainsValueForAnyQuery(!isNegativeComparisonOperator, columnName), JoinCondition.And);
			}
			else if (!value.IsEmpty)
			{
				if (isNegativeComparisonOperator)
				{
					op = OperatorsDictionary[op];
				}

				result.AddSubQuery(GetSubQuery(columnName, op, new[] { value }, isNegativeComparisonOperator), JoinCondition.And);
			}
			return result;
		}

		static ZDBOnlySubQuery GetSubQuery(string columnName, SQLComparisonOperator comparisonOperator, ZString[] values, bool notIn, string parentTableCode = null)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID, notIn);
			subQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, columnName);

			if (values.Length > 1)
			{
				subQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, comparisonOperator, values);
			}
			else if (values.Length == 1)
			{
				subQuery.AddToFilter_PossiblyCommaSeparated(GenAddOnColumnSchema.XA_Data, comparisonOperator, values.First());
			}

			if (parentTableCode != null)
			{
				subQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, parentTableCode);
			}
			return subQuery;
		}

		#region OperatorsDictionary

		public Dictionary<SQLComparisonOperator, SQLComparisonOperator> OperatorsDictionary
		{
			get
			{
				return operatorsDictionary
					   ?? (operatorsDictionary =
						   new Dictionary<SQLComparisonOperator, SQLComparisonOperator>
							{
								{ SQLComparisonOperator.NotEqual, SQLComparisonOperator.Equal },
								{ SQLComparisonOperator.DoesNotStartWith, SQLComparisonOperator.StartsWith },
								{ SQLComparisonOperator.NotContains, SQLComparisonOperator.Contains },
								{ SpecialComparisonOperator.IsNotBlank, SpecialComparisonOperator.IsBlank },
							});
			}
		}

		Dictionary<SQLComparisonOperator, SQLComparisonOperator> operatorsDictionary;

		#endregion

		#endregion
	}
}
