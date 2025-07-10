using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.Customs.Universal;
using RefCusTradeGroupCodes = Enterprise.Core.Constants.Customs.Universal.RefCusTradeGroup.Codes;

namespace Enterprise.Freight.Business.Extensions
{
	public static class ShipmentExtensions
	{
		public static ZString LogReference(this CommonShipment shipment, bool checkIsInDatabase)
		{
			return checkIsInDatabase && !shipment.IsInDatabase ? (ZString)shipment.PK.ToString() : shipment.JS_UniqueConsignRef;
		}

		/// <summary>
		/// Checks if the shipment is an import to a specified country by origin/destination ports. Not a company specific check.
		/// </summary>
		public static bool IsImportTo(this CommonShipment shipment, string countryCode)
		{
			return shipment != null
				&& !string.IsNullOrEmpty(shipment.JS_RL_NKOrigin)
				&& !shipment.JS_RL_NKOrigin.StartsWith(countryCode, System.StringComparison.OrdinalIgnoreCase)
				&& !string.IsNullOrEmpty(shipment.JS_RL_NKDestination)
				&& shipment.JS_RL_NKDestination.StartsWith(countryCode, System.StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Checks if the shipment is an export from a specified country by origin/destination ports. Not a company specific check.
		/// </summary>
		public static bool IsExportFrom(this CommonShipment shipment, string countryCode)
		{
			return shipment != null
				&& !string.IsNullOrEmpty(shipment.JS_RL_NKOrigin)
				&& shipment.JS_RL_NKOrigin.StartsWith(countryCode, System.StringComparison.OrdinalIgnoreCase)
				&& !string.IsNullOrEmpty(shipment.JS_RL_NKDestination)
				&& !shipment.JS_RL_NKDestination.StartsWith(countryCode, System.StringComparison.OrdinalIgnoreCase);
		}

		public static bool IsLoadingIn(this CommonShipment shipment, string countryCode)
		{
			return shipment != null
				&& (shipment.JS_RL_NKOrigin.StartsWith(countryCode, System.StringComparison.OrdinalIgnoreCase)
					|| shipment.TransportsIncludingRelated.Cast<Transport>().Any(transport => transport.JW_RL_NKLoadPort.StartsWith(countryCode, System.StringComparison.OrdinalIgnoreCase)));
		}

		public static bool IsDischargingIn(this CommonShipment shipment, string countryCode)
		{
			return shipment != null
				&& (shipment.JS_RL_NKDestination.StartsWith(countryCode, System.StringComparison.OrdinalIgnoreCase)
					|| shipment.TransportsIncludingRelated.Cast<Transport>().Any(transport => transport.JW_RL_NKDiscPort.StartsWith(countryCode, System.StringComparison.OrdinalIgnoreCase)));
		}

		public static IEnumerable<string> CommonTransitButNotEUCountries(this BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("CommonTransitButNotEUCountries", () =>
			{
				var subEUC = new ZDBOnlySubQuery(typeof(CusRefTradeGroupCountryView), RefCusTradeGroupCountrySchema.ZZB_RN_NKTradeGroupCountryCode, true);
				subEUC.AddSubQuery(RefCusTradeGroupCountrySchema.ZZB_ZZA_TradeGroup, GetEUTradeGroupSubQuery(RefCusTradeGroupCodes.EuropeanUnionForCustoms), JoinCondition.And);

				var qry = new ZDBOnlyQuery(typeof(CusRefTradeGroupCountryView));
				qry.AddSubQuery(RefCusTradeGroupCountrySchema.ZZB_RN_NKTradeGroupCountryCode, subEUC, JoinCondition.And);
				qry.AddSubQuery(RefCusTradeGroupCountrySchema.ZZB_ZZA_TradeGroup, GetEUTradeGroupSubQuery(RefCusTradeGroupCodes.EUCommonTransitProcedure), JoinCondition.And);

				var resList = new List<string>();
				foreach (var c in factory.Load<CusRefTradeGroupCountryView>(qry))
				{
					if (!resList.Contains(c.ZZB_RN_NKTradeGroupCountryCode))
					{
						resList.Add(c.ZZB_RN_NKTradeGroupCountryCode);
					}
				}
				return resList;
			});
		}

		static ZDBOnlySubQuery GetEUTradeGroupSubQuery(ZString tradeGroup)
		{
			var result = new ZDBOnlySubQuery(typeof(CusRefTradeGroupView), RefCusTradeGroupSchema.PK);
			result.AddToFilter(RefCusTradeGroupSchema.ZZA_TradeGroup, tradeGroup);
			result.AddToFilter(RefCusTradeGroupSchema.ZZA_ZZZ_NKDataGrouping, RefDataGrouping.Codes.EuropeanUnionEUN);
			result.AddToFilter(RefCusTradeGroupCountrySchema.ZZB_StartDate, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.Now);
			result.AddToFilter(RefCusTradeGroupCountrySchema.ZZB_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Now);
			return result;
		}

		public static bool IsShipmentPickupPenaltyApplicable(this CommonShipment shipment, bool hasSeaTransportModeConsol)
			=> hasSeaTransportModeConsol && shipment.JS_PackingMode == ContainerModes.FCL;

		public static bool IsShipmentPickupPenaltyApplicable(this CommonShipment shipment)
			=> shipment.IsShipmentPickupPenaltyApplicable(shipment.HasSeaTransportModeConsol());

		public static bool IsShipmentDeliveryPenaltyApplicable(this CommonShipment shipment, bool hasSeaTransportModeConsol)
			=> hasSeaTransportModeConsol && (shipment.JS_PackingMode == ContainerModes.FCL || shipment.JS_PackingMode == ContainerModes.BuyersConsol && shipment.JS_ShipmentType == ShipmentTypes.BuyersConsolLead);

		public static bool IsShipmentDeliveryPenaltyApplicable(this CommonShipment shipment)
			=> shipment.IsShipmentDeliveryPenaltyApplicable(shipment.HasSeaTransportModeConsol());

		public static bool HasSeaTransportModeConsol(this CommonShipment shipment)
			=> shipment.Consols.Any(consol => ((CommonConsol)consol).JK_TransportMode == TransportModes.Sea);
	}
}
