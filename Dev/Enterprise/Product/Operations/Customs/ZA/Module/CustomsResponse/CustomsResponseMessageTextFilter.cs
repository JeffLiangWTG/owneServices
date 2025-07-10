using System;
using System.Collections;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Module
{
	public class CustomsResponseMessageTextFilter : ModuleTextFilter
	{
		#region Construction

		public CustomsResponseMessageTextFilter(ZString description, SchemaStringColumn textSchemaColumn, ZString[] segmentPatterns)
			: this(description, textSchemaColumn, segmentPatterns, null, null)
		{
		}

		public CustomsResponseMessageTextFilter(ZString description, SchemaStringColumn textSchemaColumn, ZString[] segmentPatterns, int? elementSeq, int? subElementSeq)
			: base(description, GetTextQueryWithOperatorDelegate(textSchemaColumn, segmentPatterns, elementSeq, subElementSeq))
		{
		}

		public CustomsResponseMessageTextFilter(ZString description, SchemaStringColumn textSchemaColumn, ZString[] segmentPatterns, IList list)
			: this(description, textSchemaColumn, segmentPatterns, null, null, list)
		{
		}

		public CustomsResponseMessageTextFilter(ZString description, SchemaStringColumn textSchemaColumn, ZString[] segmentPatterns, int? elementSeq, int? subElementSeq, IList list)
			: base(description, GetTextQueryDelegate(textSchemaColumn, segmentPatterns, elementSeq, subElementSeq), list)
		{
		}

		#endregion

		#region Query Delegate

		static GetTextQuery GetTextQueryDelegate(SchemaStringColumn textSchemaColumn, ZString[] segmentPatterns, int? elementSeq, int? subElementSeq)
		{
			if (textSchemaColumn.TableName == EDIMessageSchema.Constants.TableName)
			{
				return (value) => GetEDIMessageTextQuery(SQLComparisonOperator.Equal, value, textSchemaColumn, segmentPatterns, elementSeq, subElementSeq);
			}
			else if (textSchemaColumn.TableName == EDIInterchangeSchema.Constants.TableName)
			{
				return (value) => GetEDIInterchangeTextQuery(SQLComparisonOperator.Equal, value, textSchemaColumn, segmentPatterns, elementSeq, subElementSeq);
			}
			else
			{
				return (value) => ZQuery.NoResultQuery;
			}
		}

		static GetTextQueryWithOperator GetTextQueryWithOperatorDelegate(SchemaStringColumn textColumn, ZString[] segmentPatterns, int? elementSeq, int? subElementSeq)
		{
			if (textColumn.TableName == EDIMessageSchema.Constants.TableName)
			{
				return (comparisonOperator, value) => GetEDIMessageTextQuery(comparisonOperator, value, textColumn, segmentPatterns, elementSeq, subElementSeq);
			}
			else if (textColumn.TableName == EDIInterchangeSchema.Constants.TableName)
			{
				return (comparisonOperator, value) => GetEDIInterchangeTextQuery(comparisonOperator, value, textColumn, segmentPatterns, elementSeq, subElementSeq);
			}
			else
			{
				return (comparisonOperator, value) => ZQuery.NoResultQuery;
			}
		}

		static ZQuery GetEDIInterchangeTextQuery(SQLComparisonOperator comparisonOperator, ZString value, SchemaStringColumn textSchemaColumn, ZString[] segmentPatterns, int? elementSeq, int? subElementSeq)
		{
			return GetMessageTextQuery(typeof(EDIInterchange), EDIInterchangeSchema.Constants.TableName, EDIMessageSchema.EM_EI, EDIInterchangeSchema.PK, textSchemaColumn,
				comparisonOperator, value, segmentPatterns, elementSeq, subElementSeq);
		}

		static ZQuery GetEDIMessageTextQuery(SQLComparisonOperator comparisonOperator, ZString value, SchemaStringColumn textSchemaColumn, ZString[] segmentPatterns, int? elementSeq, int? subElementSeq)
		{
			return GetMessageTextQuery(typeof(EDIMessage), EDIMessageSchema.Constants.TableName, EDIMessageSchema.PK, EDIMessageSchema.PK, textSchemaColumn,
				comparisonOperator, value, segmentPatterns, elementSeq, subElementSeq);
		}

		static ZQuery GetMessageTextQuery(Type typeOfBusinessObjectToQuery, string tableName, SchemaGuidColumn fkSchemaColumn, SchemaPKColumn pkSchemaColumn, SchemaStringColumn textSchemaColumn,
			SQLComparisonOperator filterOperator, ZString value, ZString[] segmentPatterns, int? elementSeq = null, int? subElementSeq = null)
		{
			ZDBOnlyQuery result = null;

			if (filterOperator == SQLComparisonOperator.Contains || filterOperator == SQLComparisonOperator.NotContains)
			{
				result = new ZDBOnlyQuery(typeOfBusinessObjectToQuery);
				result.IncludeBlob(textSchemaColumn);

				var isNegativeSQLOperator = filterOperator.IsNegativeSQLOperator();

				foreach (var segmentPattern in segmentPatterns)
				{
					var filterBuilder = new ZStringBuilder(fkSchemaColumn.Name);
					filterBuilder.Append(isNegativeSQLOperator ? "NOT IN" : "IN");
					filterBuilder.AppendFormat(@"(SELECT {0} FROM {1}.{2} CROSS APPLY dbo.csfn_GetEdifactElementInline({3}, '{4}', {5}, {6}, 0) AS Data WHERE Data.Element LIKE @element)",
						pkSchemaColumn.Name, pkSchemaColumn.TableSchema.SqlSchemaName, tableName, textSchemaColumn.Name, segmentPattern.TrimEnd(':', '+'),
						elementSeq.HasValue ? elementSeq.Value.ToString(CultureInfo.InvariantCulture) : (segmentPattern.EndsWith("+", StringComparison.CurrentCultureIgnoreCase) ? "1" : "0"),
						subElementSeq.HasValue ? subElementSeq.Value.ToString(CultureInfo.InvariantCulture) : (segmentPattern.EndsWith(":", StringComparison.CurrentCultureIgnoreCase) ? "1" : "0"));

					result.AddFilterAndZSQLParameterCollection(filterBuilder.ToStringWithDelimiterBetweenAppends(" "),
						new ZSqlParameterCollection(ZSqlParameter.New("@element", ZString.Format("%{0}%", value), textSchemaColumn)), isNegativeSQLOperator ? JoinCondition.And : JoinCondition.Or);
				}
			}
			else
			{
				var textQuery = new ZDBOnlyQuery(typeOfBusinessObjectToQuery);
				textQuery.IncludeBlob(textSchemaColumn);

				if (filterOperator == SpecialComparisonOperator.IsBlank)
				{
					foreach (var segmentPattern in segmentPatterns)
					{
						textQuery.AddFilterAndZSQLParameterCollection(ZString.Format("({0} LIKE '%{1}[:+'']%' OR {0} NOT LIKE '%{1}%')", textSchemaColumn.Name, segmentPattern), null, JoinCondition.And);
					}
				}
				else if (filterOperator == SpecialComparisonOperator.IsNotBlank)
				{
					foreach (var segmentPattern in segmentPatterns)
					{
						textQuery.AddFilterAndZSQLParameterCollection(ZString.Format("({0} LIKE '%{1}%' AND {0} NOT LIKE '%{1}[:+'']%')", textSchemaColumn.Name, segmentPattern), null, JoinCondition.Or);
					}
				}
				else
				{
					var isNegativeSQLOperator = filterOperator.IsNegativeSQLOperator();

					foreach (var segmentPattern in segmentPatterns)
					{
						var filterBuilder = new ZStringBuilder();
						filterBuilder.Append(textSchemaColumn.Name);
						filterBuilder.Append(isNegativeSQLOperator ? "NOT LIKE" : "LIKE");
						filterBuilder.Append("@value");
						var newValue = value;
						if (filterOperator == SQLComparisonOperator.Equal || filterOperator == SQLComparisonOperator.NotEqual)
						{
							newValue = ZString.Format("{0}{1}[:+']", segmentPattern, value);
						}
						else if (filterOperator == SQLComparisonOperator.StartsWith || filterOperator == SQLComparisonOperator.DoesNotStartWith)
						{
							newValue = ZString.Format("{0}{1}", segmentPattern, value);
						}
						textQuery.AddFilterAndZSQLParameterCollection(filterBuilder.ToStringWithDelimiterBetweenAppends(" "),
							new ZSqlParameterCollection(ZSqlParameter.New("@value", ZString.Format("%{0}%", newValue), textSchemaColumn)), isNegativeSQLOperator ? JoinCondition.And : JoinCondition.Or);
					}
				}

				if (typeOfBusinessObjectToQuery == typeof(EDIMessage))
				{
					result = textQuery;
				}
				else
				{
					result = new ZDBOnlyQuery(typeof(EDIMessage));
					var subQuery = new ZDBOnlySubQuery(typeOfBusinessObjectToQuery, fkSchemaColumn);
					subQuery.AddToFilter(textQuery);
					result.AddSubQuery(subQuery, JoinCondition.And);
				}
			}

			return result;
		}

		#endregion
	}
}
