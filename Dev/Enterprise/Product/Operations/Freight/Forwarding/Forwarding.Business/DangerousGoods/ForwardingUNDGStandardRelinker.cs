using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class ForwardingUNDGStandardRelinker
	{
		public static void TryRelinkAllDataItemsToTargetStandard(BusinessObjectFactory factory, IReadOnlyCollection<UNDGDataItem> dataItems, ZString targetStandard)
		{
			var allSubstances = dataItems
				.Select(dg => dg.Substance)
				.WhereNotNull();

			var allDistinctUNNOs = allSubstances
				.Select(substance => substance.DG_UNNO)
				.Distinct();

			if (!allDistinctUNNOs.Any())
			{
				return;
			}

			var query = ConstructQueryForSpecifiedUNDGsOfTargetStandard(allDistinctUNNOs, targetStandard);
			var allLoadedSubstances = factory
				.Load<UNDGSubstance>(query);

			foreach (var dataItem in dataItems)
			{
				if (allLoadedSubstances.FirstOrDefault(substance => substance.DG_UNNO == (dataItem.Substance?.DG_UNNO ?? ZString.Empty))
					is UNDGSubstance matchedSubstance)
				{
					RelinkToNewSubstance(dataItem, matchedSubstance);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		static ZQuery ConstructQueryForSpecifiedUNDGsOfTargetStandard(IEnumerable<ZString> allUNNOs, ZString targetStandard)
		{
			var mainQuery = new ZQuery();

			foreach (var unno in allUNNOs)
			{
				var singleQuery = ConstructQueryForSingleUNNOOfTargetStandard(unno, targetStandard);
				mainQuery.AddToFilter(singleQuery, JoinCondition.Or);
			}

			return mainQuery;
		}

		static ZQuery ConstructQueryForSingleUNNOOfTargetStandard(ZString unno, ZString targetStandard)
		{
			var singleQuery = new ZQuery();
			singleQuery.AddToFilter(new ZQuery(UNDGSubstanceSchema.DG_UNNO, unno));
			singleQuery.AddToFilter(new ZQuery(UNDGSubstanceSchema.DG_Variant, ZString.Empty));
			singleQuery.AddToFilter(new ZQuery(UNDGSubstanceSchema.DG_Standard, targetStandard));

			return singleQuery;
		}

		static void RelinkToNewSubstance(UNDGDataItem dataItem, UNDGSubstance substance)
		{
			dataItem.DI_DG = substance.PK;

			var defaultPivot = dataItem.UNDGSubstancePivotCollection.First(pivot => pivot.DP_IsDefault);
			defaultPivot.DP_Variant = ZString.Empty;
			defaultPivot.DP_Standard = substance.DG_Standard;
		}
	}
}
