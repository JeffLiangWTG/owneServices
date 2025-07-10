using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Integration;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class QuoteTradeDetailImporter
	{
		public QuoteTradeDetailImporter(Quote quote)
		{
			this.quote = quote;
		}

		readonly Quote quote;

		public bool Import(OrgTradeDetail tradeDetail)
		{
			Argument.NotNull(tradeDetail, "tradeDetail");

			if (tradeDetail.Parent == null)
			{
				return false;
			}

			if (quote.TH_OH.IsEmpty)
			{
				var parentOrganisation = tradeDetail.Parent.ParentOrganisation;
				if (parentOrganisation != null)
				{
					quote.TH_OH = parentOrganisation.PK;
				}
			}

			var categoryAndMode = GetCategoryAndMode(tradeDetail);
			if (categoryAndMode == null)
			{
				return false;
			}

			var newEntry = GetOrCreateRateEntry(categoryAndMode.Item1, categoryAndMode.Item2, tradeDetail);
			PopulateRateLines(newEntry, tradeDetail);

			return true;
		}

		static Tuple<string, string> GetCategoryAndMode(OrgTradeDetail tradeDetail)
		{
			if (tradeDetail.Parent.ProductCode == SystemDefinedSalesProductList.Codes.CustomsBrokerage)
			{
				return Tuple.Create(
					tradeDetail.PA_TradeType == OrgTradeDetailLookups.CustomsBrokerageTradeTypes.Import ? RatingConstants.RateCategory.DST : RatingConstants.RateCategory.ORG,
					tradeDetail.PA_TradeMode.ToString());
			}
			else if (tradeDetail.Parent.ProductCode == SystemDefinedSalesProductList.Codes.Warehouse)
			{
				return Tuple.Create(RatingConstants.RateCategory.WHS, (string)null);
			}

			var freightMode = ObjectFactory.Get<IFreightRatingHelper>().CalculateFreightMode(
				tradeDetail.TransportMode,
				tradeDetail.PA_TradeType,
				() => (tradeDetail.PA_TradeType == Constants.ContainerModes.LCL) ? FreightMode.NonContainerised : FreightMode.Containerised);

			if (
				freightMode == FreightMode.UKN ||
				freightMode == FreightMode.FullLoad ||
				freightMode == FreightMode.Containerised ||
				freightMode == FreightMode.NonContainerised)  // only has container mode
			{
				if (freightMode == FreightMode.UKN)
				{
					if (tradeDetail.SalesProductCode == SystemDefinedSalesProductList.Codes.ForwardingShipment && tradeDetail.PA_TradeMode == Constants.TransportModes.Courier)
					{
						return Tuple.Create(
							RatingConstants.RateCategory.AIR,
							Core.Constants.RateMode.LSE);
					}
				}
				return null;
			}

			if (tradeDetail.Parent.ProductCode == SystemDefinedSalesProductList.Codes.ForwardingShipment)
			{
				if (freightMode.HasFlag(FreightMode.AIR))
				{
					return Tuple.Create(
						RatingConstants.RateCategory.AIR,
						freightMode.ToString());
				}
				else if (freightMode.HasFlag(FreightMode.Containerised))
				{
					return Tuple.Create(
						RatingConstants.RateCategory.FCL,
						tradeDetail.PA_TradeMode.ToString());
				}
				else
				{
					return Tuple.Create(
						 RatingConstants.RateCategory.LCL,
						 freightMode.ToString());
				}
			}
			else if (tradeDetail.Parent.ProductCode == SystemDefinedSalesProductList.Codes.LinerAgency)
			{
				return Tuple.Create(
					 freightMode.HasFlag(FreightMode.Containerised) ? RatingConstants.RateCategory.SCO : RatingConstants.RateCategory.SNC,
					 freightMode.HasFlag(FreightMode.Containerised) ? Core.Constants.RateMode.SEA : Core.Constants.RateMode.LCL);
			}

			return null;
		}

		RateEntry GetOrCreateRateEntry(string category, string mode, OrgTradeDetail tradeDetail)
		{
			var collection = quote.EntryCollections[category].LazyLoadingCollection;
			var newEntry = collection.AddNew();

			if (tradeDetail.Parent.ProductCode == SystemDefinedSalesProductList.Codes.CustomsBrokerage)
			{
				if (tradeDetail.PA_TradeType == OrgTradeDetailLookups.CustomsBrokerageTradeTypes.Import)
				{
					newEntry.TI_DestinationLRC = ToLocationCode(tradeDetail.Parent.Origin);
				}
				else if (tradeDetail.PA_TradeType == OrgTradeDetailLookups.CustomsBrokerageTradeTypes.Export)
				{
					newEntry.TI_OriginLRC = ToLocationCode(tradeDetail.Parent.Origin);
				}
			}
			else if (tradeDetail.Parent.ProductCode == SystemDefinedSalesProductList.Codes.Warehouse)
			{
				newEntry.TI_WW_Warehouse = tradeDetail.Parent.OW_WW;
			}
			else
			{
				newEntry.TI_OriginLRC = ToLocationCode(tradeDetail.Parent.Origin);
				newEntry.TI_DestinationLRC = ToLocationCode(tradeDetail.Parent.Destination);
			}

			newEntry.TI_OH_Consignee = tradeDetail.PA_Calc_OH_Buyer;
			newEntry.TI_OH_Consignor = tradeDetail.PA_Calc_OH_Supplier;
			newEntry.TI_OH_TransportProvider = tradeDetail.ProspectDetail.PAP_OH_ServiceProvider;

			if (mode != null)
			{
				newEntry.TI_Mode = new ZString(mode).SubstringSafe(0, RateEntrySchema.TI_Mode.MaxLength);
			}

			newEntry.TI_RH_NKCommodityCode = tradeDetail.ProspectDetail.PAP_RH_NKCommodityCode;
			newEntry.TI_RS_NKServiceLevel_NI = tradeDetail.ProspectDetail.PAP_RS_NKServiceLevel;
			newEntry.TI_RX_NKCurrency = tradeDetail.CurrentProspectPeriod.PAS_RX_NKCurrency;

			if (newEntry.IsContainerTypeAllowed() && tradeDetail.ProspectDetail.Container != null)
			{
				newEntry.TI_RC = tradeDetail.ProspectDetail.Container.PK;
			}

			foreach (var entry in collection.Cast<RateEntry>().ToArray())
			{
				if (entry != newEntry && newEntry.IsDuplicate(entry))
				{
					collection.RemoveAndDelete(newEntry);
					newEntry = entry;
					break;
				}
			}

			return newEntry;
		}

		static string ToLocationCode(ViewLocation viewLocation)
		{
			if (viewLocation == null)
			{
				return string.Empty;
			}

			if (viewLocation.IsInternationalZone || viewLocation.IsCountry || viewLocation.IsUNLOCO)
			{
				return viewLocation.VLO_Code
					.Substring(0, 5); // these types of ViewLocations should all have a code length of <=5, but this substring is needed so that CodeAnalysis EDI003 doesn't complain
			}
			else
			{
				return viewLocation.VLO_CountryCode;
			}
		}

		static void PopulateRateLines(RateEntry newEntry, OrgTradeDetail tradeDetail)
		{
			if (tradeDetail.Parent.ProductCode == SystemDefinedSalesProductList.Codes.Warehouse)
			{
				if (!tradeDetail.PA_OP.IsEmpty)
				{
					var hasExistingLine = newEntry.RateLines.Cast<RateLine>().Any(x => x.TL_OP_ProductNumber == tradeDetail.PA_OP);
					if (!hasExistingLine)
					{
						var rateLine = newEntry.RateLines.AddNew();
						rateLine.TL_OP_ProductNumber = tradeDetail.PA_OP;
					}
				}
			}

			foreach (RateLine rateLine in newEntry.RateLines)
			{
				PopulateRateLineCalculatorFromTradeDetails(rateLine, tradeDetail);
			}
		}

		static void PopulateRateLineCalculatorFromTradeDetails(RateLine rateLine, OrgTradeDetail tradeDetail)
		{
			if (!tradeDetail.CurrentProspectPeriod.EstimatedProfitPerUnitType.IsEmpty)
			{
				var unit = tradeDetail.CurrentProspectPeriod.EstimatedProfitPerUnitType;
				if (unit == Constants.PkgUnit.Container)
				{
					unit = QuantityUnit.CN;
				}
				rateLine.TL_WeightVolume = unit;
			}

			if (rateLine.Calculator != null)
			{
				if (rateLine.Calculator is CombinedCalculator)
				{
					PopulateRateLineCalculatorFromTradeDetails((CombinedCalculator)rateLine.Calculator, tradeDetail);
				}
				else if (rateLine.Calculator is UnitCalculator)
				{
					PopulateRateLineCalculatorFromTradeDetails((UnitCalculator)rateLine.Calculator, tradeDetail);
				}
				else if (rateLine.Calculator is MinimumOrPerUnitCalculator)
				{
					PopulateRateLineCalculatorFromTradeDetails((MinimumOrPerUnitCalculator)rateLine.Calculator, tradeDetail);
				}
			}
		}

		static void PopulateRateLineCalculatorFromTradeDetails(BaseCombinedCalculator calculator, OrgTradeDetail tradeDetail)
		{
			var breakItem = calculator.GetBreakItemByAmount(new Quantity(tradeDetail.CurrentProspectPeriod.PAS_Chargeable, tradeDetail.CurrentProspectPeriod.PAS_Calc_ChargeableUQ), calculator.Line.ChildRateLineItems);
			if (breakItem == null)
			{
				return;
			}

			var rateLineItem = breakItem as RateLineItem
				?? throw new InvalidOperationException($"Rate lines items of type {nameof(RateLineItem)} are expected while {breakItem.GetType().Name} were found");

			rateLineItem.TM_RelevantValue = tradeDetail.CurrentProspectPeriod.PAS_RateOffered;
		}

		static void PopulateRateLineCalculatorFromTradeDetails(UnitCalculator calculator, OrgTradeDetail tradeDetail)
		{
			calculator.PerUnit = tradeDetail.CurrentProspectPeriod.PAS_RateOffered;
		}

		static void PopulateRateLineCalculatorFromTradeDetails(MinimumOrPerUnitCalculator calculator, OrgTradeDetail tradeDetail)
		{
			calculator.PerUnit = tradeDetail.CurrentProspectPeriod.PAS_RateOffered;
		}
	}
}

