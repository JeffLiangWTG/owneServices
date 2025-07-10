using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class DpsComplianceListHelper
	{
		public static DpsComplianceListItem[] GetExcludedComplianceListItems(BusinessObjectFactory businessObjectFactory)
		{
			return GetNotNullFactory(businessObjectFactory).Load<RefComplianceList>(GetFilterIncludedOrExcludedListQuery()).Select(GetPartyScreeningComplianceListItem).ToArray();
		}

		public static (DpsComplianceListItem[] ActiveExcludedList, DpsComplianceListItem[] ActiveIncludedList) GetActiveIncludedAndExcludedComplianceListItems(BusinessObjectFactory businessObjectFactory)
		{
			return (
				GetNotNullFactory(businessObjectFactory)
					.Load<RefComplianceList>(GetActiveQuery().AddToFilter(GetFilterIncludedOrExcludedListQuery()))
					.Select(GetPartyScreeningComplianceListItem).ToArray(),
				GetNotNullFactory(businessObjectFactory)
					.Load<RefComplianceList>(GetActiveQuery().AddToFilter(GetFilterIncludedOrExcludedListQuery(false)))
					.Select(GetPartyScreeningComplianceListItem).ToArray());
		}

		public static DpsComplianceListItem[] GetComplianceListItems(BusinessObjectFactory businessObjectFactory)
		{
			return GetNotNullFactory(businessObjectFactory).Load<RefComplianceList>(GetActiveQuery()).Select(GetPartyScreeningComplianceListItem).ToArray();
		}

		public static (DpsComplianceListItem[] Included, DpsComplianceListItem[] Excluded) GetComplianceListIncludedAndExcludedItems(BusinessObjectFactory businessObjectFactory, string[] sourceListCodes)
		{
			Argument.NotNull(sourceListCodes, nameof(sourceListCodes));

			var notNullFactory = GetNotNullFactory(businessObjectFactory);

			var excludedItems = notNullFactory.Load<RefComplianceList>(GetFilterIncludedOrExcludedListQuery(sourceListCodes: sourceListCodes)).Select(GetPartyScreeningComplianceListItem).ToArray();

			var includedCodes = sourceListCodes.Where(u => excludedItems.All(v => v.Code != u));

			var includedItems = notNullFactory.Load<RefComplianceList>(new ZQuery(RefComplianceListSchema.RCL_ListCode, includedCodes).AddToFilter(GetActiveQuery())).Select(GetPartyScreeningComplianceListItem).ToList();

			var notImportedComplianceListCodes = sourceListCodes.Where(code => excludedItems.All(v => v.Code != code) && includedItems.All(v => v.Code != code)).Distinct().ToArray();

			notImportedComplianceListCodes.ForEach(code =>
			{
				includedItems.Add(new DpsComplianceListItem(code, code, ZString.Empty, ZString.Empty, ZString.Empty));
			});

			return (includedItems.ToArray(), excludedItems);
		}

		public static bool HasExcludedList(BusinessObjectFactory businessObjectFactory)
		{
			return GetNotNullFactory(businessObjectFactory).Exists(typeof(RefComplianceList), GetFilterIncludedOrExcludedListQuery());
		}

		static DpsComplianceListItem GetPartyScreeningComplianceListItem(RefComplianceList complianceList) => new DpsComplianceListItem(complianceList.RCL_ListCode, complianceList.RCL_ListName, complianceList.RCL_ListDescription, complianceList.RCL_ListPublisher, ZString.Empty);

		static ZQuery GetFilterIncludedOrExcludedListQuery(bool isExcluded = true, string[] sourceListCodes = null)
		{
			var query = new ZDBOnlyQuery(typeof(RefComplianceList));
			ZDBOnlySubQuery dbOnlySubQuery;
			if (isExcluded)
			{
				dbOnlySubQuery = new ZDBOnlySubQuery(typeof(RefComplianceList), RefComplianceListSchema.PK);
			}
			else
			{
				dbOnlySubQuery = new ZDBOnlySubQuery(typeof(RefComplianceList), RefComplianceListSchema.PK, true); //using NOT IN
			}

			if (sourceListCodes != null)
			{
				query.AddToFilter(RefComplianceListSchema.RCL_ListCode, sourceListCodes);
				query.AddToFilter(GetActiveQuery());
			}

			var subQuery = new ZQuery(RefComplianceListSchema.RCL_IsExcluded, true);
			dbOnlySubQuery.AddToFilter(subQuery);
			query.AddSubQuery(dbOnlySubQuery, JoinCondition.And);

			return query;
		}

		static BusinessObjectFactory GetNotNullFactory(BusinessObjectFactory factory)
		{
			return factory ?? new ReadOnlyBusinessObjectFactory();
		}

		static ZQuery GetActiveQuery()
		{
			return new ZQuery(RefComplianceListSchema.RCL_IsActive, true);
		}
	}
}
