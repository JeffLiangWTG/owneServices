using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class OrgSalesActualsInformationBulkPopulater
	{
		public OrgSalesActualsInformationBulkPopulater(BusinessObjectFactory factory, ZGuid viewpointOrgPk)
		{
			this.factory = factory;
			this.viewpointOrgPk = viewpointOrgPk;
		}

		readonly BusinessObjectFactory factory;
		readonly ZGuid viewpointOrgPk;

		#region Execute

		public void Execute(IEnumerable<EntitySalesWrapper> prospectSales, bool matchOnBuyerSupplier)
		{
			if (!prospectSales.Any() || viewpointOrgPk.IsEmpty)
			{
				return;
			}

			if (matchOnBuyerSupplier)
			{
				if (prospectToActualsLookup_MatchOnBuyerSupplier == null)
				{
					prospectToActualsLookup_MatchOnBuyerSupplier = CreateLookupAndAddFetchHints(true);
				}

				PopulateActuals(prospectSales, prospectToActualsLookup_MatchOnBuyerSupplier);
			}
			else
			{
				if (prospectToActualsLookup_NotMatchOnBuyerSupplier == null)
				{
					prospectToActualsLookup_NotMatchOnBuyerSupplier = CreateLookupAndAddFetchHints(false);
				}

				PopulateActuals(prospectSales, prospectToActualsLookup_NotMatchOnBuyerSupplier);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Part of Sql, Sql is used here for performance")]
		Dictionary<ZGuid, List<ZGuid>> CreateLookupAndAddFetchHints(bool matchOnBuyerSupplier)
		{
			var result = new Dictionary<ZGuid, List<ZGuid>>();

			var loadSql = string.Format(CultureInfo.InvariantCulture, @"
SElECT
	VOW_OW_Prospect,
	VOW_OW_Actual
FROM
	dbo.ViewSalesProspectToActualPivot
WHERE
	@OrgPk IN (VOW_ProspectPrimary, VOW_ProspectBuyer, VOW_ProspectSupplier)
	AND @OrgPk IN (VOW_ActualPrimary, VOW_ActualBuyer, VOW_ActualSupplier)
	{0}",
			(matchOnBuyerSupplier ? "AND VOW_IsBuyerSupplierMatching = 1" : ""));

			using (var command = Db.Connection.Command(loadSql))
			{
				command.AddParameter("@OrgPk", SqlDbType.UniqueIdentifier, viewpointOrgPk.ToGuid());

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var prospect = (Guid)reader[ViewSalesProspectToActualPivotSchema.Constants.VOW_OW_Prospect];
						var actual = (Guid)reader[ViewSalesProspectToActualPivotSchema.Constants.VOW_OW_Actual];

						if (!result.TryGetValue(prospect, out List<ZGuid> actuals))
						{
							actuals = new List<ZGuid>();
							result[prospect] = actuals;
						}

						actuals.Add(actual);
					}
				}
			}

			foreach (var list in result.Values)
			{
				var query = new ZQuery(OrgSalesSchema.PK, list);
				factory.AddFetchHint(OrgSalesSchema.Instance, query);
			}

			return result;
		}

		void PopulateActuals(IEnumerable<EntitySalesWrapper> prospectSales, Dictionary<ZGuid, List<ZGuid>> prospectToActualsLookup)
		{
			var allActualSales = new List<OrgSales>(prospectSales.Count());

			foreach (var prospect in prospectSales)
			{
				if (prospectToActualsLookup.TryGetValue(prospect.PK, out List<ZGuid> actuals))
				{
					var actualsQuery = new ZQuery(OrgSalesSchema.PK, actuals);
					var actualSales = factory.Load<OrgSales>(actualsQuery);
					prospect.ActualsInformation.SetActuals(actualSales);
					allActualSales.AddRange(actualSales);
				}
				else
				{
					prospect.ActualsInformation.SetActuals(Array.Empty<OrgSales>());
				}
			}

			foreach (var sales in allActualSales)
			{
				factory.AddFetchHint(OrgTradeDetailSchema.PA_OW, sales.PK);
			}
			foreach (var sales in allActualSales)
			{
				foreach (var detail in sales.TradeDetails)
				{
					factory.AddFetchHint(OrgTradePeriodSchema.PAS_PA, detail.PK);
				}
			}
		}

		#endregion

		Dictionary<ZGuid, List<ZGuid>> prospectToActualsLookup_MatchOnBuyerSupplier;
		Dictionary<ZGuid, List<ZGuid>> prospectToActualsLookup_NotMatchOnBuyerSupplier;
	}
}
