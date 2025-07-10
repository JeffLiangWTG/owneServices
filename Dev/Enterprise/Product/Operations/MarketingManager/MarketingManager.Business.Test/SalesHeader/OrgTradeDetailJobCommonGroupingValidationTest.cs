using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class OrgTradeDetailJobCommonGroupingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestTradeMode()
		{
			var grouping = GetNewGrouping();

			grouping.TradeMode = Constants.TransportModes.Air;
			AssertMandatoryValidationError(grouping.TradeModeInfo, false);
			AssertListValidationInvalidCodeError(grouping.TradeModeInfo, false);

			grouping.TradeMode = "XXX";
			AssertMandatoryValidationError(grouping.TradeModeInfo, false);
			AssertListValidationInvalidCodeError(grouping.TradeModeInfo, true);

			grouping.TradeMode = "";
			AssertMandatoryValidationError(grouping.TradeModeInfo, true);
			AssertListValidationInvalidCodeError(grouping.TradeModeInfo, false);
		}

		public void TestTradeMode_Brokerage()
		{
			var org = Factory.New<OrgHeader>();
			var sales = Factory.New<OrgSales>();
			var entitySales = EntitySalesWrapper.Get(sales, org);
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.CustomsBrokerage);
			var grouping = new OrgTradeDetailJobCommonGrouping(entitySales.EntityTradeDetailsCompanyView, product);

			grouping.TradeMode = Constants.TransportModes.Air;
			AssertMandatoryValidationError(grouping.TradeModeInfo, false);
			AssertListValidationInvalidCodeError(grouping.TradeModeInfo, false);

			grouping.TradeMode = "XXX";
			AssertMandatoryValidationError(grouping.TradeModeInfo, false);
			AssertListValidationInvalidCodeError(grouping.TradeModeInfo, true);

			grouping.TradeMode = "";
			AssertMandatoryValidationError(grouping.TradeModeInfo, false);
			AssertListValidationInvalidCodeError(grouping.TradeModeInfo, false);
		}

		public void TestTradeType()
		{
			var grouping = GetNewGrouping();
			grouping.TradeMode = Constants.TransportModes.Air;

			grouping.TradeType = Constants.ContainerModes.Loose;
			AssertMandatoryValidationError(grouping.TradeTypeInfo, false);
			AssertListValidationInvalidCodeError(grouping.TradeTypeInfo, false);

			grouping.TradeType = "XXX";
			AssertMandatoryValidationError(grouping.TradeTypeInfo, false);
			AssertListValidationInvalidCodeError(grouping.TradeTypeInfo, true);

			grouping.TradeType = "";
			AssertMandatoryValidationError(grouping.TradeTypeInfo, false);
			AssertListValidationInvalidCodeError(grouping.TradeTypeInfo, false);
		}

		public void TestTradeType_Brokerage()
		{
			var org = Factory.New<OrgHeader>();
			var sales = Factory.New<OrgSales>();
			var entitySales = EntitySalesWrapper.Get(sales, org);
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.CustomsBrokerage);
			var grouping = new OrgTradeDetailJobCommonGrouping(entitySales.EntityTradeDetailsCompanyView, product);
			grouping.TradeMode = Constants.TransportModes.Air;

			grouping.TradeType = OrgTradeDetailLookups.CustomsBrokerageTradeTypes.Import;
			AssertMandatoryValidationError(grouping.TradeTypeInfo, false);
			AssertListValidationInvalidCodeError(grouping.TradeTypeInfo, false);

			grouping.TradeType = "XXX";
			AssertMandatoryValidationError(grouping.TradeTypeInfo, false);
			AssertListValidationInvalidCodeError(grouping.TradeTypeInfo, true);

			grouping.TradeType = "";
			AssertMandatoryValidationError(grouping.TradeTypeInfo, true);
			AssertListValidationInvalidCodeError(grouping.TradeTypeInfo, false);
		}

		#region Implementation

		OrgTradeDetailJobCommonGrouping GetNewGrouping()
		{
			var org = Factory.New<OrgHeader>();
			var sales = Factory.New<OrgSales>();
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var entitySales = EntitySalesWrapper.Get(sales, org);
			var grouping = new OrgTradeDetailJobCommonGrouping(entitySales.EntityTradeDetailsCompanyView, product);
			return grouping;
		}

		#endregion
	}
}
