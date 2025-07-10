using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVItemLine))]
	class HVLVItemLineValidationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidate_CustomsValue_ErrorIfNegative()
		{
			var item = Factory.NewWithValidTestData<HVLVItem>();

			var itemLine = item.Lines.AddNew();

			itemLine.HVS_CustomsValue = -1;
			AssertHasError(itemLine.HVS_CustomsValueInfo, "Please enter a 'Total Customs Value' greater than or equal to 0.");

			itemLine.HVS_CustomsValue = 0;
			AssertNoWarnings(itemLine.HVS_CustomsValueInfo);

			itemLine.HVS_CustomsValue = 1;
			AssertNoWarnings(itemLine.HVS_CustomsValueInfo);
		}

		public void TestValidate_IntrinsicValue_ErrorIfNegative()
		{
			var item = Factory.NewWithValidTestData<HVLVItem>();

			var itemLine = item.Lines.AddNew();

			itemLine.HVS_IntrinsicValue = -1;
			AssertHasError(itemLine.HVS_IntrinsicValueInfo, "Please enter a 'Total Intrinsic Value' greater than or equal to 0.");

			itemLine.HVS_IntrinsicValue = 0;
			AssertNoWarnings(itemLine.HVS_IntrinsicValueInfo);

			itemLine.HVS_IntrinsicValue = 1;
			AssertNoWarnings(itemLine.HVS_IntrinsicValueInfo);
		}

		public void TestValidate_GrossWeight()
		{
			var item = Factory.NewWithValidTestData<HVLVItem>();

			var line = item.Lines.AddNew();

			line.HVS_GrossWeight = -1;
			AssertHasError(line.HVS_GrossWeightInfo, "Please enter a 'Gross Weight' greater than or equal to 0.");

			line.HVS_GrossWeight = 0;
			AssertNoWarnings(line.HVS_GrossWeightInfo);

			line.HVS_GrossWeight = 1;
			AssertNoWarnings(line.HVS_GrossWeightInfo);
		}

		public void TestValidate_NetWeight()
		{
			var item = Factory.NewWithValidTestData<HVLVItem>();

			var line = item.Lines.AddNew();

			line.HVS_NetWeight = -1;
			AssertHasError(line.HVS_NetWeightInfo, "Please enter a 'Net Weight' greater than or equal to 0.");

			line.HVS_NetWeight = 0;
			AssertNoWarnings(line.HVS_NetWeightInfo);

			line.HVS_NetWeight = 1;
			AssertNoWarnings(line.HVS_NetWeightInfo);
		}

		public void TestValidate_Quantity()
		{
			var item = Factory.NewWithValidTestData<HVLVItem>();

			var line = item.Lines.AddNew();

			line.HVS_Quantity = -1;
			AssertHasError(line.HVS_QuantityInfo, "Please enter a 'Quantity' greater than or equal to 1.");

			line.HVS_Quantity = 0;
			AssertHasError(line.HVS_QuantityInfo, "Please enter a 'Quantity' greater than or equal to 1.");

			line.HVS_Quantity = 1;
			AssertNoWarnings(line.HVS_QuantityInfo);
		}

		public void TestValidate_OriginCountryCode()
		{
			var item = Factory.New<HVLVItem>();
			var line = item.Lines.AddNew();

			line.HVS_RN_NKOriginCountryCode = "XX";
			AssertHasError(line.HVS_RN_NKOriginCountryCodeInfo, "Enter a valid Goods Origin.");

			line.HVS_RN_NKOriginCountryCode = "FR";
			AssertNoErrors(line.HVS_RN_NKOriginCountryCodeInfo);
		}

		public void TestValidate_WeightUnit()
		{
			var item = Factory.New<HVLVConsignment>().Items.AddNew();
			var line = item.Lines.AddNew();

			line.HVS_WeightUnit = "XX";
			AssertHasError(line.HVS_WeightUnitInfo, "Enter a valid Weight Unit.");

			line.HVS_NetWeight = 1;
			line.HVS_WeightUnit = "";
			AssertHasError(line.HVS_WeightUnitInfo, "Please enter a Weight Unit.");

			line.HVS_WeightUnit = "KG";
			AssertNoErrors(line.HVS_WeightUnitInfo);
		}

		public void TestValidate_OriginCountryCodeAndOriginTariff()
		{
			var item = Factory.New<HVLVItem>();
			var line = item.Lines.AddNew();

			line.HVS_RN_NKOriginCountryCode = "AU";
			line.HVS_OriginTariff = "";

			AssertNoErrors(line.HVS_OriginTariffInfo);
			AssertHasWarning(line.HVS_OriginTariffInfo, "You have not entered a value.");

			line.HVS_OriginTariff = "123456";
			AssertNoErrors(line.HVS_OriginTariffInfo);
			AssertNoWarnings(line.HVS_OriginTariffInfo);

			var line2 = item.Lines.AddNew();

			line2.HVS_OriginTariff = "123456";
			line2.HVS_RN_NKOriginCountryCode = "";

			AssertHasError(line2.HVS_RN_NKOriginCountryCodeInfo, "Please enter a Goods Origin.");

			line2.HVS_RN_NKOriginCountryCode = "AU";
			AssertNoErrors(line2.HVS_RN_NKOriginCountryCodeInfo);
		}

		public void TestValidate_FormattedOriginTariff()
		{
			var item = Factory.New<HVLVItem>();
			var line = item.Lines.AddNew();

			line.HVS_RN_NKOriginCountryCode = "AU";
			line.HVS_OriginTariff = "";

			AssertNoErrors(line.HVS_FormattedOriginTariffInfo);
			AssertHasWarning(line.HVS_FormattedOriginTariffInfo, "You have not entered a value.");

			line.HVS_OriginTariff = "123456";
			AssertNoErrors(line.HVS_FormattedOriginTariffInfo);
			AssertNoWarnings(line.HVS_FormattedOriginTariffInfo);
		}

		public void TestValidate_FormattedDestinationTariff()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var itemLine = CreateHVLVItemLineTestObject();
				var shipment = itemLine.ParentItem.Shipment;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";

				AssertEquals("precondition: shipment direction is import", Directions.Import, shipment.JobDirection);

				itemLine.HVS_DestinationTariff = string.Empty;
				Assert(itemLine.HVS_FormattedDestinationTariffInfo.HasMessageError("You have not entered a value."));

				itemLine.HVS_DestinationTariff = "2930904";
				Assert(itemLine.HVS_FormattedDestinationTariffInfo.HasMessageError("Tariff unable to be found."));

				var tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = "2930904310";
				tariff.UE_DateFrom = ZDate.Today;
				tariff.UE_DateTo = ZDate.Today.AddDays(1);

				Factory.Save();

				itemLine.HVS_DestinationTariff = "2930904310";
				Assert(!itemLine.HVS_FormattedDestinationTariffInfo.HasMessageErrors());
			}
		}

		public void TestValidate_DestinationHSCode_WhenShipmentIsImportToUS()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var itemLine = CreateHVLVItemLineTestObject();
				var shipment = itemLine.ParentItem.Shipment;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";

				AssertEquals("precondition: shipment direction is import", Directions.Import, shipment.JobDirection);

				itemLine.HVS_DestinationTariff = string.Empty;
				Assert(itemLine.HVS_DestinationTariffInfo.HasMessageError("You have not entered a value."));

				itemLine.HVS_DestinationTariff = "2930904";
				Assert(itemLine.HVS_DestinationTariffInfo.HasMessageError("Tariff unable to be found."));

				var tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = "2930904310";
				tariff.UE_DateFrom = ZDate.Today;
				tariff.UE_DateTo = ZDate.Today.AddDays(1);

				Factory.Save();

				itemLine.HVS_DestinationTariff = "2930904310";
				Assert(!itemLine.HVS_DestinationTariffInfo.HasMessageErrors());
			}
		}

		public void TestValidate_GoodsDescription_WarnIfAnyDiacritics()
		{
			var line = Factory.New<HVLVItemLine>();

			line.HVS_GoodsDescription = "Dïácrïtïcs";
			line.Validation.ValidateHVS_GoodsDescription();
			AssertHasWarning(line.HVS_GoodsDescriptionInfo, "Goods Description should not contain any characters with diacritics.");

			line.HVS_GoodsDescription = "GoodsDesc";
			line.Validation.ValidateHVS_GoodsDescription();
			AssertNoWarnings(line.HVS_GoodsDescriptionInfo);
		}

		public void TestValidate_OriginGoodsDescription_WarnIfAnyDiacritics()
		{
			var line = Factory.New<HVLVItemLine>();

			line.HVS_OriginGoodsDescription = "Dïácrïtïcs";
			line.Validation.ValidateHVS_OriginGoodsDescription();
			AssertHasWarning(line.HVS_OriginGoodsDescriptionInfo, "Origin Goods Description should not contain any characters with diacritics.");

			line.HVS_OriginGoodsDescription = "GoodsDesc";
			line.Validation.ValidateHVS_OriginGoodsDescription();
			AssertNoWarnings(line.HVS_OriginGoodsDescriptionInfo);
		}

		public void TestValidate_ItemURL()
		{
			var line = Factory.New<HVLVItemLine>();

			line.HVS_ItemURL = "";
			line.Validation.ValidateHVS_ItemURL();
			AssertNoWarnings(line.HVS_ItemURLInfo);

			line.HVS_ItemURL = "invalid";
			line.Validation.ValidateHVS_ItemURL();
			AssertHasWarning(line.HVS_ItemURLInfo, "Invalid Item URL.");

			line.HVS_ItemURL = "www.valid.com";
			line.Validation.ValidateHVS_ItemURL();
			AssertNoWarnings(line.HVS_ItemURLInfo);
		}

		public void TestValidate_ItemURL_WarnIfAnyDiacritics()
		{
			var line = Factory.New<HVLVItemLine>();

			line.HVS_ItemURL = "www.dïácrïtïcs.com";
			line.Validation.ValidateHVS_ItemURL();
			AssertHasWarning(line.HVS_ItemURLInfo, "Item Line Website should not contain any characters with diacritics.");

			line.HVS_ItemURL = "www.itemurl.com";
			line.Validation.ValidateHVS_ItemURL();
			AssertNoWarnings(line.HVS_ItemURLInfo);
		}

		#region Implementation

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var consignment = factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			var itemLine = item.Lines.AddNew();
			itemLine.HVS_RN_NKOriginCountryCode = "AU";
			itemLine.HVS_OriginTariff = Guid.NewGuid().ToString().Substring(0, 10);
			itemLine.HVS_CustomsValue = 100m;
			itemLine.HVS_DestinationTariff = Guid.NewGuid().ToString().Substring(0, 10);
			itemLine.HVS_GoodsDescription = "This is something for description.";
			itemLine.HVS_OriginGoodsDescription = "This is something for origin description.";
			itemLine.HVS_GrossWeight = 2.1m;
			itemLine.HVS_IntrinsicValue = 100.2m;
			itemLine.HVS_ItemURL = $"http://www.amazon.com/item/{Guid.NewGuid()}";
			itemLine.HVS_NetWeight = 2.0m;
			itemLine.HVS_ProductCode = Guid.NewGuid().ToString().Substring(0, 10);
			itemLine.HVS_Quantity = 3;
			itemLine.HVS_WeightUnit = "KG";

			return itemLine;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return CreateHVLVItemLineTestObject();
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			return CreateHVLVItemLineTestObject();
		}

		HVLVItemLine CreateHVLVItemLineTestObject()
		{
			var booking = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = booking.Consignments.AddNew();
			var item = consignment.Items.AddNew();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKDestination = "AU";

			item.HVI_JS_LoadedOnShipment = shipment.PK;

			var itemLine = item.Lines.AddNew();
			itemLine.HVS_OriginTariff = "123456";
			itemLine.HVS_RN_NKOriginCountryCode = "AU";
			itemLine.HVS_CustomsValue = 100m;
			itemLine.HVS_DestinationTariff = "HS123456";
			itemLine.HVS_GoodsDescription = "This is something for description.";
			itemLine.HVS_OriginGoodsDescription = "This is something for origin description.";
			itemLine.HVS_GrossWeight = 2.1m;
			itemLine.HVS_IntrinsicValue = 100.2m;
			itemLine.HVS_ItemURL = "http://www.amazon.com/item/3333";
			itemLine.HVS_NetWeight = 2.0m;
			itemLine.HVS_ProductCode = "PROD1234";
			itemLine.HVS_Quantity = 3;
			itemLine.HVS_WeightUnit = "KG";

			return itemLine;
		}

		#endregion
	}
}
