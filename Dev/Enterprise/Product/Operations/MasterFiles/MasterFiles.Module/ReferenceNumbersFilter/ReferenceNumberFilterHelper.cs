using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class ReferenceNumberFilterHelper<ParentT>
		where ParentT : BusinessObject
	{
		public ZQuery GetReferenceNumberFilter(SQLComparisonOperator @operator, ZString country, ZString type, ZString number)
		{
			if (@operator == SpecialComparisonOperator.IsNotBlank)
			{
				return GetReferenceNumberFilterCore(SQLComparisonOperator.NotEqual, country, type, "", false, true);
			}
			else if (@operator == SpecialComparisonOperator.IsBlank)
			{
				return GetReferenceNumberFilterCore(SQLComparisonOperator.NotEqual, country, type, "", true, true);
			}
			else if (@operator == SQLComparisonOperator.NotEqual)
			{
				return GetReferenceNumberFilterCore(SQLComparisonOperator.Equal, country, type, number, true, false);
			}
			else if (@operator == SQLComparisonOperator.NotContains)
			{
				return GetReferenceNumberFilterCore(SQLComparisonOperator.Contains, country, type, number, true, false);
			}
			else if (@operator == SQLComparisonOperator.DoesNotStartWith)
			{
				return GetReferenceNumberFilterCore(SQLComparisonOperator.StartsWith, country, type, number, true, false);
			}
			else if (@operator == SQLComparisonOperator.Equal)
			{
				return GetReferenceNumberFilterCore(SQLComparisonOperator.Equal, country, type, number, false, true);
			}
			else
			{
				return GetReferenceNumberFilterCore(@operator, country, type, number, false, false);
			}
		}

		protected virtual ZQuery GetReferenceNumberFilterCore(SQLComparisonOperator @operator, ZString country, ZString type, ZString number, bool notIn, bool filterOnEmptyNumber)
		{
			ZDBOnlySubQuery entryNumFilter = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Customs.ICusEntryNumber), CusEntryNumSchema.CE_ParentID, notIn);
			entryNumFilter.AddToFilter(GetCusEntryNumFilter(@operator, country, type, number, filterOnEmptyNumber));

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ParentT));
			result.AddSubQuery(entryNumFilter, JoinCondition.And);

			return result;
		}

		protected static ZQuery GetCusEntryNumFilter(SQLComparisonOperator @operator, ZString country, ZString type, ZString number, bool filterOnEmptyNumber)
		{
			ZQuery numFilter = new ZQuery();
			numFilter.AddToFilter(CusEntryNumSchema.CE_Category, SQLComparisonOperator.Equal, ObjectFactory.Get<Enterprise.Integration.Customs.ICusEntryNumHelper>().AdditionalReferenceNumberCategory);

			if (!country.IsEmpty)
			{
				numFilter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, country);
			}

			if (!type.IsEmpty)
			{
				numFilter.AddToFilter(CusEntryNumSchema.CE_EntryType, type);
			}

			if (!number.IsEmpty || filterOnEmptyNumber)
			{
				numFilter.AddToFilter_PossiblyCommaSeparated(CusEntryNumSchema.CE_EntryNum, @operator, number);
			}

			return numFilter;
		}
	}
}
