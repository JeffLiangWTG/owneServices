using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public class OrgSalesFilterBusinessObjectWithActuals : OrgSalesFilterBusinessObject
	{
		#region Properties

		[BusinessObjectTestExclude]
		public IEnumerable<EntitySalesWrapper> AllProspectSales
		{
			get;
			set;
		}

		#endregion

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			filters.AddTextFilter(FilterDescription.Status, GetStatusQuery, Statuses).MultilingualDescription = ResString.GetMultilingualString("65d3257f-bda2-460b-bcf7-8434803f3b0b", "Status");
			filters.AddDateFilter(FilterDescription.ActualsLastTraded, GetActualsLastTradedQuery).MultilingualDescription = ResString.GetMultilingualString("f8a66d3b-3c4e-4cd8-bd89-26d1875957a6", "Last Traded");

			return filters;
		}

		ZQuery GetStatusQuery(ZString value)
		{
			if (AllProspectSales == null)
			{
				return ZQuery.NoResultQuery;
			}

			var salesPks = AllProspectSales.Where(x => x.ActualsInformation.Status == value).Select(x => x.PK);
			return new ZQuery(OrgSalesSchema.PK, salesPks);
		}

		ZQuery GetHasActualsQuery(ZBool value)
		{
			if (AllProspectSales == null)
			{
				return ZQuery.NoResultQuery;
			}

			var salesPks = AllProspectSales.Where(x => x.ActualsInformation.HasActuals() == value).Select(x => x.PK);
			return new ZQuery(OrgSalesSchema.PK, salesPks);
		}

		ZQuery GetActualsLastTradedQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			if (AllProspectSales == null)
			{
				return ZQuery.NoResultQuery;
			}

			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				return GetHasActualsQuery(false);
			}
			else if (comparisonOperator == DateComparisonOperator.HasDateEntered)
			{
				return GetHasActualsQuery(true);
			}
			else
			{
				var salesPks = AllProspectSales.Where(x => x.ActualsInformation.ActualsLastTraded >= value1 && x.ActualsInformation.ActualsLastTraded <= value2).Select(x => x.PK);
				return new ZQuery(OrgSalesSchema.PK, salesPks);
			}
		}

		#endregion

		#region Lookups

		public ICodeDescriptionPairList Statuses
		{
			get
			{
				if (statuses == null)
				{
					statuses = new OrgSalesActualsStatusList();
				}

				return statuses;
			}
		}
		ICodeDescriptionPairList statuses;

		#endregion

		public new static class FilterDescription
		{
			#region SuppressResourceStringsCheckRegion

			public const string ActualsLastTraded = "ActualsLastTraded";
			public const string Status = "Status";

			#endregion
		}
	}
}
