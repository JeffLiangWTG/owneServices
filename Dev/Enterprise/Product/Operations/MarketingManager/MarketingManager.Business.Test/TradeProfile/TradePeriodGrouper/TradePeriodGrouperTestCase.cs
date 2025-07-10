using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	public abstract class TradePeriodGrouperTestCase : TestCaseWithFactory
	{
		protected OrgTradePeriod NewTradePeriodForModeAndType(ZGuid orgPk, ZString mode, ZString type)
		{
			var sales = Factory.New<OrgSales>();
			var tradeDetail = sales.TradeDetails.AddNew();
			tradeDetail.PA_TradeMode = mode;
			tradeDetail.PA_TradeType = type;
			var tradePeriod = tradeDetail.TradedPeriods.AddNew();
			tradePeriod.PAS_OH_Client = orgPk;

			return tradePeriod;
		}

		protected OrgTradePeriod NewTradePeriodForSupplierAndBuyer(ZGuid orgPk, ZGuid buyerPk, ZGuid supplierPk)
		{
			var sales = Factory.New<OrgSales>();
			sales.OW_OH_Buyer = buyerPk;
			sales.OW_OH_Supplier = supplierPk;
			var tradeDetail = sales.TradeDetails.AddNew();
			var tradePeriod = tradeDetail.TradedPeriods.AddNew();
			tradePeriod.PAS_OH_Client = orgPk;

			return tradePeriod;
		}

		protected OrgTradePeriod NewTradePeriodForCountries(ZGuid orgPk, ZString? originCountryCode, ZString? destinationCountryCode)
		{
			var sales = Factory.New<OrgSales>();

			if (originCountryCode.HasValue)
			{
				sales.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, originCountryCode.Value, RefCountrySchema.Constants.Prefix).PK;
				sales.OW_OriginTableCode = RefCountrySchema.Constants.Prefix;
			}

			if (destinationCountryCode.HasValue)
			{
				sales.OW_DestinationID = ViewLocationHelper.GetLocationFromString(Factory, destinationCountryCode.Value, RefCountrySchema.Constants.Prefix).PK;
				sales.OW_DestinationTableCode = RefCountrySchema.Constants.Prefix;
			}

			var tradeDetail = sales.TradeDetails.AddNew();
			var tradePeriod = tradeDetail.TradedPeriods.AddNew();
			tradePeriod.PAS_OH_Client = orgPk;

			return tradePeriod;
		}

		protected OrgTradePeriod NewTradePeriodForStates(ZGuid orgPk, RefCountryStates originState, RefCountryStates destinationState)
		{
			var sales = Factory.New<OrgSales>();

			if (originState != null)
			{
				sales.OW_OriginID = originState.PK;
				sales.OW_OriginTableCode = RefCountryStatesSchema.Constants.Prefix;
			}

			if (destinationState != null)
			{
				sales.OW_DestinationID = destinationState.PK;
				sales.OW_DestinationTableCode = RefCountryStatesSchema.Constants.Prefix;
			}

			var tradeDetail = sales.TradeDetails.AddNew();
			var tradePeriod = tradeDetail.TradedPeriods.AddNew();
			tradePeriod.PAS_OH_Client = orgPk;

			return tradePeriod;
		}

		protected OrgTradePeriod NewTradePeriodForUnlocos(ZGuid orgPk, ZString? originUnlocoCode, ZString? destinationUnlocoCode)
		{
			var sales = Factory.New<OrgSales>();

			if (originUnlocoCode.HasValue)
			{
				sales.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, originUnlocoCode.Value, RefUNLOCOSchema.Constants.Prefix).PK;
				sales.OW_OriginTableCode = RefUNLOCOSchema.Constants.Prefix;
			}

			if (destinationUnlocoCode.HasValue)
			{
				sales.OW_DestinationID = ViewLocationHelper.GetLocationFromString(Factory, destinationUnlocoCode.Value, RefUNLOCOSchema.Constants.Prefix).PK;
				sales.OW_DestinationTableCode = RefUNLOCOSchema.Constants.Prefix;
			}

			var tradeDetail = sales.TradeDetails.AddNew();
			var tradePeriod = tradeDetail.TradedPeriods.AddNew();
			tradePeriod.PAS_OH_Client = orgPk;

			return tradePeriod;
		}

		protected OrgTradePeriod NewTradePeriodWithOriginAndDestination(ZGuid orgPk, BusinessObject origin, BusinessObject destination)
		{
			var sales = Factory.New<OrgSales>();

			if (origin != null)
			{
				sales.OW_OriginID = origin.PK;
				sales.OW_OriginTableCode = origin.TablePrefix;
			}

			if (destination != null)
			{
				sales.OW_DestinationID = destination.PK;
				sales.OW_DestinationTableCode = destination.TablePrefix;
			}

			var tradeDetail = sales.TradeDetails.AddNew();
			var tradePeriod = tradeDetail.TradedPeriods.AddNew();
			tradePeriod.PAS_OH_Client = orgPk;

			return tradePeriod;
		}

		protected IEnumerable<OrgTradePeriod> GetInAnotherFactory(BusinessObjectFactory targetFactory, IEnumerable<OrgTradePeriod> tradePeriods)
		{
			return targetFactory.Load<OrgTradePeriod>(new ZQuery(OrgTradePeriodSchema.PK, tradePeriods.Select(x => x.PK)));
		}
	}
}
