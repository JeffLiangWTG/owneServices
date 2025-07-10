using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.NCTS.Module
{
	public class NctsMovementFilterStripBusinessObject : EU.NCTS.Module.NctsMovementFilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			AddLrnRegistrationNumberFilter(filters);
			AddLrnRegistrationDateFilter(filters);
			return filters;
		}

		void AddLrnRegistrationNumberFilter(ModuleFilterCollection filters)
		{
			var lrnRegistrationNumberFilter = filters.AddTextFilter("LRN",
				(comparisonOperator, filterText) => GetRegistrationNumberQuery(comparisonOperator, filterText, CusEntryNumberTypes.Standard.LocalReferenceNumber));

			lrnRegistrationNumberFilter.Category = FilterCategories.NumbersAndReferences;
			lrnRegistrationNumberFilter.MultilingualDescription = ResString.GetMultilingualString("242483DE-BED1-4F34-A92C-B29867023D6C", "LRN");
		}

		void AddLrnRegistrationDateFilter(ModuleFilterCollection filters)
		{
			var lrnRegistrationDateFilter = filters.AddDateFilter("LRN Date", GetRegistrationDateQuery);
			lrnRegistrationDateFilter.Category = FilterCategories.NumbersAndReferences;
			lrnRegistrationDateFilter.MultilingualDescription = ResString.GetMultilingualString("0E3FAA15-A471-4ABE-A7CF-BDEB25FDE55C", "LRN Date");

			ZQuery GetRegistrationDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2) => GetIssueDateQuery(comparisonOperator, value1, value2, CusEntryNumberTypes.Standard.LocalReferenceNumber);
		}

		ZQuery GetRegistrationNumberQuery(SQLComparisonOperator comparisonOperator, ZString filterText, ZString entryType)
		{
			var query = GetNctsHeaderZOnlyQuery();
			var releaseQuery = GetEntryNumSubQuery(entryType);

			if (comparisonOperator == SQLComparisonOperator.IsBlank)
			{
				var noRegistrationSubQuery = GetEntryNumSubQuery(entryType, notIn: true);
				query.AddSubQuery(noRegistrationSubQuery, JoinCondition.Or);
			}
			else
			{
				releaseQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, comparisonOperator, filterText);
				query.AddSubQuery(releaseQuery, JoinCondition.And);
			}

			return query;
		}

		ZQuery GetIssueDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2, ZString entryType)
		{
			var query = GetNctsHeaderZOnlyQuery();

			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				AddHasNotInSubQuery(query, entryType);
			}
			else
			{
				var releaseQuery = GetEntryNumSubQuery(entryType);

				AddDateTimeRange(releaseQuery, comparisonOperator, JoinCondition.And, CusEntryNumSchema.CE_IssueDate, value1, value2);
				query.AddSubQuery(releaseQuery, JoinCondition.And);
			}

			return query;
		}

		void AddHasNotInSubQuery(ZDBOnlyQuery query, ZString entryType)
		{
			var noRegistrationSubQuery = GetEntryNumSubQuery(entryType, notIn: true);
			query.AddSubQuery(noRegistrationSubQuery, JoinCondition.Or);
		}

		ZDBOnlySubQuery GetEntryNumSubQuery(ZString entryType, bool notIn = false)
		{
			var cusEntryNumSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, notIn);
			cusEntryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, entryType);

			return cusEntryNumSubQuery;
		}
		ZDBOnlyQuery GetNctsHeaderZOnlyQuery() => new ZDBOnlyQuery(typeof(NctsHeader));
	}
}
