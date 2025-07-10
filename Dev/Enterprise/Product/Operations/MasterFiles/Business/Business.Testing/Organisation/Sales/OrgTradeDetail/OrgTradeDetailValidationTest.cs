using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MarketingManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgTradeDetailValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPA_TradeMode()
		{
			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment).Identifier;
			var tradeDetail = sales.TradeDetails.AddNew();

			tradeDetail.PA_TradeMode = Constants.TransportModes.Air;
			AssertMandatoryValidationError(tradeDetail.PA_TradeModeInfo, false);
			AssertListValidationInvalidCodeError(tradeDetail.PA_TradeModeInfo, false);

			tradeDetail.PA_TradeMode = "";
			AssertMandatoryValidationError(tradeDetail.PA_TradeModeInfo, true);
			AssertListValidationInvalidCodeError(tradeDetail.PA_TradeModeInfo, false);

			tradeDetail.PA_TradeMode = "XXX";
			AssertMandatoryValidationError(tradeDetail.PA_TradeModeInfo, false);
			AssertListValidationInvalidCodeError(tradeDetail.PA_TradeModeInfo, true);
		}

		public void TestCheckPA_TradeMode_Brokerage()
		{
			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.CustomsBrokerage).Identifier;
			var tradeDetail = sales.TradeDetails.AddNew();

			tradeDetail.PA_TradeMode = Constants.TransportModes.Air;
			AssertMandatoryValidationError(tradeDetail.PA_TradeModeInfo, false);
			AssertListValidationInvalidCodeError(tradeDetail.PA_TradeModeInfo, false);

			tradeDetail.PA_TradeMode = "";
			AssertMandatoryValidationError(tradeDetail.PA_TradeModeInfo, false);
			AssertListValidationInvalidCodeError(tradeDetail.PA_TradeModeInfo, false);

			tradeDetail.PA_TradeMode = "XXX";
			AssertMandatoryValidationError(tradeDetail.PA_TradeModeInfo, false);
			AssertListValidationInvalidCodeError(tradeDetail.PA_TradeModeInfo, true);
		}

		public void TestCheckPA_TradeType()
		{
			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment).Identifier;
			var tradeDetail = sales.TradeDetails.AddNew();
			tradeDetail.PA_TradeMode = Constants.TransportModes.Air;

			tradeDetail.PA_TradeType = Constants.ContainerModes.Loose;
			AssertMandatoryValidationError(tradeDetail.PA_TradeTypeInfo, false);
			AssertListValidationInvalidCodeError(tradeDetail.PA_TradeTypeInfo, false);

			tradeDetail.PA_TradeType = "";
			AssertMandatoryValidationError(tradeDetail.PA_TradeTypeInfo, false);
			AssertListValidationInvalidCodeError(tradeDetail.PA_TradeTypeInfo, false);

			tradeDetail.PA_TradeType = "XXX";
			AssertMandatoryValidationError(tradeDetail.PA_TradeTypeInfo, false);
			AssertListValidationInvalidCodeError(tradeDetail.PA_TradeTypeInfo, true);
		}

		public void TestCheckPA_TradeType_Brokerage()
		{
			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.CustomsBrokerage).Identifier;
			var tradeDetail = sales.TradeDetails.AddNew();
			tradeDetail.PA_TradeMode = Constants.TransportModes.Air;

			tradeDetail.PA_TradeType = OrgTradeDetailLookups.CustomsBrokerageTradeTypes.Import;
			AssertMandatoryValidationError(tradeDetail.PA_TradeTypeInfo, false);
			AssertListValidationInvalidCodeError(tradeDetail.PA_TradeTypeInfo, false);

			tradeDetail.PA_TradeType = "";
			AssertMandatoryValidationError(tradeDetail.PA_TradeTypeInfo, true);
			AssertListValidationInvalidCodeError(tradeDetail.PA_TradeTypeInfo, false);

			tradeDetail.PA_TradeType = "XXX";
			AssertMandatoryValidationError(tradeDetail.PA_TradeTypeInfo, false);
			AssertListValidationInvalidCodeError(tradeDetail.PA_TradeTypeInfo, true);
		}

		public void TestValidateProspectPeriodStartAndEnd()
		{
			var sales = Factory.New<OrgSales>();
			var tradeDetail = sales.TradeDetails.AddNew();

			AssertNoErrors(tradeDetail.ProspectPeriodStartInfo);
			AssertNoErrors(tradeDetail.ProspectPeriodEndInfo);

			tradeDetail.ProspectPeriodEndType = OrgTradeProspectPeriodEndTypeList.Codes.ManuallyEnter;
			tradeDetail.Validation.ValidateProspectPeriodStart();
			tradeDetail.Validation.ValidateProspectPeriodEnd();
			AssertHasErrors(tradeDetail.ProspectPeriodStartInfo);
			AssertHasErrors(tradeDetail.ProspectPeriodEndInfo);

			tradeDetail.ProspectPeriodStart = new ZDate(2018, 1, 1);
			tradeDetail.ProspectPeriodEnd = new ZDate(2018, 3, 1);

			AssertNoErrors(tradeDetail.ProspectPeriodStartInfo);
			AssertNoErrors(tradeDetail.ProspectPeriodEndInfo);

			tradeDetail.ProspectPeriodEnd = new ZDate(2017, 3, 1);

			AssertNoErrors(tradeDetail.ProspectPeriodStartInfo);
			AssertHasError("End Period validation", tradeDetail.ProspectPeriodEndInfo, "The End Period must be later than the Start Period");

			tradeDetail.ProspectPeriodStart = new ZDate(2018, 3, 1);

			AssertHasError("Start Period validation", tradeDetail.ProspectPeriodStartInfo, "The Start Period must be earlier than the End Period");
		}
	}
}
