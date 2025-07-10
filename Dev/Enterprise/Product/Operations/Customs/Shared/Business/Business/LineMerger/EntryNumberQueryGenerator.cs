using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public static class EntryNumberQueryGenerator
	{
		public static ZQuery GetEntryNumberQuery(SQLComparisonOperator @operator, ZString value, ZString countryCode)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(BaseJobDeclaration));

			ZStringBuilder queryText = new ZStringBuilder();

			var isNegative = @operator.IsNegativeSQLOperator();
			var doNotSupplyNumberFilter = false;
			var operatorMaybeNegative = isNegative ? @operator.GetNegatingSQLOperatorIfNotInSubquery() : @operator;
			if (@operator == SpecialComparisonOperator.IsBlank || @operator == SpecialComparisonOperator.IsNotBlank)
			{
				isNegative = @operator == SpecialComparisonOperator.IsBlank;
				doNotSupplyNumberFilter = true;
			}
			ZDBOnlyQuery tempQuery = new ZDBOnlyQuery(typeof(CusEntryNumber));
			if (!doNotSupplyNumberFilter)
			{
				tempQuery.AddToFilter_PossiblyCommaSeparated(CusEntryNumSchema.CE_EntryNum, operatorMaybeNegative, value);
			}
			else
			{
				tempQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, SQLComparisonOperator.NotEqual, string.Empty);  // need some kind of filter here otherwise we miss ourt the word "WHERE" which turns the country and category parts into JOIN conditions, not WHERE conditions
			}

			ZString sql = @"{0} {1} IN (";
			queryText.Append(ZString.Format(sql,
				JobDeclarationSchema.Constants.PK, //0
				(isNegative ? "NOT" : "")) //1
				);

			queryText.Append(CusEntryHeaderSubquery(tempQuery));
			queryText.Append(" UNION ");
			queryText.Append(JobDeclarationSubquery(tempQuery));
			queryText.Append(")");
			ZSqlParameterCollection sqlParams = null;
			if (!Globals.IsWeb)
			{
				sqlParams = new ZSqlParameterCollection(ZSqlParameter.New("@countryCode", countryCode, CusEntryNumSchema.CE_RN_NKCountryCode));
			}
			result.AddFilterAndZSQLParameterCollection(queryText.ToString(), sqlParams);
			return result;
		}

		static ZString CusEntryHeaderSubquery(ZDBOnlyQuery tmpQuery)
		{
			ZStringBuilder result = new ZStringBuilder(
				ZString.Format(@"
					SELECT {0}
					FROM {1}  
					INNER JOIN {2} ON {0} = {3}
					INNER JOIN {4} ON {5} = {6} ",
				JobDeclarationSchema.Constants.PK, //0
				JobDeclarationSchema.Constants.TableName, //1
				CusEntryHeaderSchema.Constants.TableName, //2
				CusEntryHeaderSchema.Constants.CH_JE, //3
				CusEntryNumSchema.Constants.TableName, //4
				CusEntryHeaderSchema.Constants.PK, //5
				CusEntryNumSchema.Constants.CE_ParentID //6
			));
			result.Append(GetCusEntryNumberWhereClause(tmpQuery));
			return result.ToString();
		}

		static ZString GetCusEntryNumberWhereClause(ZDBOnlyQuery tmpQuery)
		{
			ZString sql;
			ZString result;
			if (!Globals.IsWeb)
			{
				sql = @"
						{0} 
						AND {1} = @countryCode 
						AND {2} not in ('MUC')
						AND {3} = 'CUS'";
				result = ZString.Format(sql,
							tmpQuery.ContainsOrOperator ? tmpQuery.GetAsWhereClause(true).Replace("WHERE", "WHERE (") + ")" : (string)tmpQuery.GetAsWhereClause(true), //0
							CusEntryNumSchema.Constants.CE_RN_NKCountryCode, //1
							CusEntryNumSchema.Constants.CE_EntryType, //2
							CusEntryNumSchema.Constants.CE_Category //3
					);
			}
			else
			{
				sql = @"
						{0} 
						AND {1} not in ('MUC')
						AND {2} = 'CUS'";
				result = ZString.Format(sql,
							tmpQuery.ContainsOrOperator ? tmpQuery.GetAsWhereClause(true).Replace("WHERE", "WHERE (") + ")" : (string)tmpQuery.GetAsWhereClause(true), //0
							CusEntryNumSchema.Constants.CE_EntryType, //1
							CusEntryNumSchema.Constants.CE_Category //2
					);
			}
			return result;
		}

		static ZString JobDeclarationSubquery(ZDBOnlyQuery tmpQuery)
		{
			ZStringBuilder result = new ZStringBuilder(
				 ZString.Format(@"
						SELECT {0}
						FROM {1} 
						INNER JOIN {2} ON {0} = {3}",
					JobDeclarationSchema.Constants.PK, //0
					JobDeclarationSchema.Constants.TableName, //1
					CusEntryNumSchema.Constants.TableName, //2
					CusEntryNumSchema.Constants.CE_ParentID //3
				));
			result.Append(GetCusEntryNumberWhereClause(tmpQuery));
			return result.ToString();
		}

		public static ZQuery GetEntryNumberByEntryTypeQuery(ZGuid parentID, ZString countryCode, ZString entryType, ZBool isInDatabase)
		{
			var result = new ZQuery(CusEntryNumSchema.CE_ParentID, parentID);
			result.FetchOnlyFromLocalCache = !isInDatabase;
			result.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
			result.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, countryCode);
			result.OrderBy = CusEntryNumSchema.CE_SystemCreateTimeUtc.Name;
			return result;
		}
	}
}
