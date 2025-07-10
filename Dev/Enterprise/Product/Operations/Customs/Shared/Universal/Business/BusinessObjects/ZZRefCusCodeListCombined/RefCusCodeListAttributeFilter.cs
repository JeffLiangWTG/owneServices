using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class RefCusCodeListAttributeFilter
	{
		public RefCusCodeListAttributeFilter(ZString name, JoinCondition joinCondition, params ZString[] values)
			: this(name, joinCondition, false, values: values)
		{
		}

		public RefCusCodeListAttributeFilter(ZString name, SQLComparisonOperator comparisonOperator, params ZString[] values)
			: this(name, JoinCondition.And, false, values: values)
		{
			this.comparisonOperator = comparisonOperator;
		}

		public RefCusCodeListAttributeFilter(ZString name, ZDateTime date, SQLComparisonOperator comparisonOperator, params ZString[] values)
			: this(name, JoinCondition.And, false, values: values)
		{
			this.comparisonOperator = comparisonOperator;
			this.date = date;
		}

		public RefCusCodeListAttributeFilter(ZString name, JoinCondition joinCondition, bool assumeItIsAMatchWhenNoAttributeExists, ZString[] notExistValues = null, params ZString[] values)
		{
			this.name = name;
			this.values = values;
			JoinCondition = joinCondition;
			this.assumeItIsAMatchWhenNoAttributeExists = assumeItIsAMatchWhenNoAttributeExists;
			this.notExistValues = notExistValues;
			comparisonOperator = SQLComparisonOperator.Equal;
		}

		public ZDBOnlyQuery Filter
		{
			get
			{
				var result = new ZDBOnlyQuery(typeof(ZZRefCusCodeListCombined));

				var subQuery = new ZDBOnlySubQuery(typeof(ZZRefCusCodeListAttributeCombined), ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList, ZZRefCusCodeListCombinedSchema.PK);
				subQuery.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZXE_NKName, name);
				if (values.Any())
				{
					subQuery.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_Value, comparisonOperator, values);
				}

				if (!date.IsEmpty)
				{
					var dateFilter = new ZQuery();
					dateFilter.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_StartDate, null)
						.AddToFilter(JoinCondition.Or, ZZRefCusCodeListAttributeCombinedSchema.ZZE_StartDate, SQLComparisonOperator.LessThanOrEqualTo, date);

					var endDateFilter = new ZQuery();
					endDateFilter.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_EndDate, null)
						.AddToFilter(JoinCondition.Or, ZZRefCusCodeListAttributeCombinedSchema.ZZE_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, date);
					dateFilter.AddToFilter(endDateFilter);
					subQuery.AddToFilter(dateFilter);
				}
				result.AddSubQuery(subQuery, JoinCondition.And);

				if (notExistValues != null && notExistValues.Any())
				{
					var notExistsCodeListAttributeSubQuery = new ZDBOnlySubQuery(typeof(ZZRefCusCodeListAttributeCombined), ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList, true);
					notExistsCodeListAttributeSubQuery.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZXE_NKName, name);
					notExistsCodeListAttributeSubQuery.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_Value, notExistValues);
					result.AddSubQuery(notExistsCodeListAttributeSubQuery, JoinCondition.And);
				}

				if (assumeItIsAMatchWhenNoAttributeExists)
				{
					var noAttributeSql = ZString.Format((NoResString)@"{0} NOT IN ( SELECT {1} FROM {2} WHERE {3} = @Name)",
						ZZRefCusCodeListCombinedSchema.Constants.PK,
						RefCusCodeListAttributeSchema.Constants.ZZE_ZZD_CodeList,
						RefCusCodeListAttributeSchema.Constants.TableName,
						RefCusCodeListAttributeSchema.Constants.ZZE_ZXE_NKName
					);
					result.AddFilterAndZSQLParameterCollection(noAttributeSql, new ZSqlParameterCollection(ZSqlParameter.New("@Name", name, RefCusCodeListAttributeSchema.ZZE_ZXE_NKName)), JoinCondition.Or);
				}
				return result;
			}
		}

		public string Key => string.Join("_", name, JoinCondition, assumeItIsAMatchWhenNoAttributeExists, comparisonOperator, string.Join("^", values), notExistValues != null && notExistValues.Any() ? string.Join("%", notExistValues) : string.Empty,
			date.IsEmpty ? string.Empty : date.ToString("yyyyMMdd"));

		readonly ZString name;
		readonly ZString[] values;
		readonly ZString[] notExistValues;
		readonly ZDateTime date;
		public JoinCondition JoinCondition { get; private set; }
		readonly bool assumeItIsAMatchWhenNoAttributeExists;
		readonly SQLComparisonOperator comparisonOperator;
	}
}
