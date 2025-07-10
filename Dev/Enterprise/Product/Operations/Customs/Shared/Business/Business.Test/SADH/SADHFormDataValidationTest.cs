using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.SADH.Testing
{
	class SADHFormDataValidationTest : BusinessObjectValidationTestCase
	{
		public void TestDeliveryTerms()
		{
			SADHData.D1_DeliveryTerms = Constants.IncoTerms.FreeOnBoard;
			AssertNoErrors(SADHData.D1_DeliveryTermsInfo);

			SADHData.D1_DeliveryTerms = ZString.Empty;
			AssertNoErrors(SADHData.D1_DeliveryTermsInfo);

			SADHData.D1_DeliveryTerms = "zzz";
			AssertHasErrorContaining(SADHData.D1_DeliveryTermsInfo, SADHFormDataValidation.D1_DeliveryTermsIsInvalid);
		}

		public void TestFlightDate()
		{
			SADHData.D1_ModeOfTransportAtTheBorder = TransportTypeList.Codes.Air;

			SADHData.D1_FlightDate = new ZDateTime(2008, 1, 1);
			AssertNoError(SADHData.D1_FlightDateInfo, SADHFormDataValidation.D1_IdentityNationalityActiveTransportBorderFlightDateIsInvalid);

			SADHData.D1_FlightDate = ZDateTime.Empty;
			AssertHasError(SADHData.D1_FlightDateInfo, SADHFormDataValidation.D1_IdentityNationalityActiveTransportBorderFlightDateIsInvalid);

			SADHData.D1_FlightDate = ZDateTime.Invalid;
			AssertHasError(SADHData.D1_FlightDateInfo, SADHFormDataValidation.D1_IdentityNationalityActiveTransportBorderFlightDateIsInvalid);

			SADHData.D1_ModeOfTransportAtTheBorder = TransportTypeList.Codes.Sea;

			SADHData.D1_FlightDate = ZDateTime.Empty;
			AssertNoError(SADHData.D1_FlightDateInfo, SADHFormDataValidation.D1_IdentityNationalityActiveTransportBorderFlightDateIsInvalid);

			SADHData.D1_FlightDate = ZDateTime.Invalid;
			AssertNoError(SADHData.D1_FlightDateInfo, SADHFormDataValidation.D1_IdentityNationalityActiveTransportBorderFlightDateIsInvalid);
		}

		public void TestMarksAndNumbers()
		{
			SADHData.D1_MarksAndNumbers = "SOME MARKS AND NUMBERS 12345";
			AssertNoMessageError(SADHData.D1_MarksAndNumbersInfo, SADHFormDataValidation.D1_PackagesDescriptionGoodsPackageMarksNumberKindEmpty);

			SADHData.D1_MarksAndNumbers = ZString.Empty;
			AssertHasMessageError(SADHData.D1_MarksAndNumbersInfo, SADHFormDataValidation.D1_PackagesDescriptionGoodsPackageMarksNumberKindEmpty);
		}

		public void TestTotalPackagesType()
		{
			SADHData.D1_TotalPackages = 123;
			SADHData.D1_TotalPackagesPackType = "KG";
			AssertNoMessageError(SADHData.D1_TotalPackagesPackTypeInfo, SADHFormDataValidation.D1_TotalPackagesPackTypeEmpty);
			SADHData.D1_TotalPackages = 0;
			AssertNoMessageError(SADHData.D1_TotalPackagesPackTypeInfo, SADHFormDataValidation.D1_TotalPackagesPackTypeEmpty);

			SADHData.D1_TotalPackages = 123;
			SADHData.D1_TotalPackagesPackType = "zz";
			AssertHasMessageError(SADHData.D1_TotalPackagesPackTypeInfo, SADHFormDataValidation.D1_TotalPackagesPackTypeIsInvalid);
			SADHData.D1_TotalPackages = 0;
			AssertHasMessageError(SADHData.D1_TotalPackagesPackTypeInfo, SADHFormDataValidation.D1_TotalPackagesPackTypeIsInvalid);

			SADHData.D1_TotalPackages = 123;
			SADHData.D1_TotalPackagesPackType = ZString.Empty;
			AssertHasMessageError(SADHData.D1_TotalPackagesPackTypeInfo, SADHFormDataValidation.D1_TotalPackagesPackTypeEmpty);
			SADHData.D1_TotalPackages = 0;
			SADHData.D1_TotalPackagesPackType = ZString.Empty;
			AssertNoMessageError(SADHData.D1_TotalPackagesPackTypeInfo, SADHFormDataValidation.D1_TotalPackagesPackTypeEmpty);
		}

		public void TestGoodsCountryOfOrigin()
		{
			SADHData.D1_RN_NKItemCountryOfOrigin = "NZ";
			AssertNoMessageError(SADHData.D1_RN_NKItemCountryOfOriginInfo, SADHFormDataValidation.D1_RN_NKGoodsCountryOriginCodeEmpty);

			SADHData.D1_RN_NKItemCountryOfOrigin = "zz";
			AssertHasMessageError(SADHData.D1_RN_NKItemCountryOfOriginInfo, SADHFormDataValidation.D1_RN_NKGoodsCountryOriginCodeIsInvalid);

			SADHData.D1_RN_NKItemCountryOfOrigin = ZString.Empty;
			AssertHasMessageError(SADHData.D1_RN_NKItemCountryOfOriginInfo, SADHFormDataValidation.D1_RN_NKGoodsCountryOriginCodeEmpty);
		}

		public virtual void TestExchangeRateZero()
		{
			RefCurrencyCollection currencyList = SADHData.Lookups.CurrencyList;

			RefCurrency currencyNotZero = currencyList.AddNew();
			currencyNotZero.RX_Code = "YYY";
			RefExchangeRate exchangeRateNotZero = currencyNotZero.ExchangeRates.AddNew();

			exchangeRateNotZero.RE_ExRateType = "CUS";
			exchangeRateNotZero.RE_SellRate = 0.50m;
			exchangeRateNotZero.RE_StartDate = ZDateTime.Now.AddMonths(-1);
			exchangeRateNotZero.RE_ExpiryDate = ZDateTime.Now.AddMonths(1);
			currencyNotZero.ExchangeRates.Add(exchangeRateNotZero);

			SADHData.D1_RX_InvoiceCurrency = currencyNotZero.PK;
			SADHData.D1_MessageType = JobMessageTypeList.Codes.Export;

			AssertNoMessageError(SADHData.ExchangeRateInfo, SADHFormDataValidation.ExchangeRateZero);

			RefCurrency currencyZero = currencyList.AddNew();
			currencyZero.RX_Code = "ZZZ";
			RefExchangeRate exchangeRateZero = currencyZero.ExchangeRates.AddNew();

			exchangeRateZero.RE_ExRateType = "CUS";
			exchangeRateZero.RE_SellRate = ZDecimal.Zero;
			exchangeRateZero.RE_StartDate = ZDateTime.Now.AddMonths(-1);
			exchangeRateZero.RE_ExpiryDate = ZDateTime.Now.AddMonths(1);
			currencyZero.ExchangeRates.Add(exchangeRateZero);

			SADHData.D1_RX_InvoiceCurrency = currencyZero.PK;

			AssertHasMessageError(SADHData.ExchangeRateInfo, SADHFormDataValidation.ExchangeRateZero);

			SADHData.D1_MessageType = JobMessageTypeList.Codes.Import;
			SADHData.D1_RX_InvoiceCurrency = ZGuid.Empty;
			SADHData.D1_RX_InvoiceCurrency = currencyZero.PK;
			AssertNoMessageError("Import", SADHData.ExchangeRateInfo, SADHFormDataValidation.ExchangeRateZero);
		}

		public void TestCountryOfDestination()
		{
			SADHData.D1_MessageType = JobMessageTypeList.Codes.Import;

			SADHData.D1_RL_NKCountryOfDestination = "NZAKL";
			AssertNoMessageError(SADHData.D1_RL_NKCountryOfDestinationInfo, SADHFormDataValidation.D1_RL_NKCountryDestinationCodeEmpty);

			SADHData.D1_RL_NKCountryOfDestination = "ZZZZZ";
			AssertHasMessageError(SADHData.D1_RL_NKCountryOfDestinationInfo, SADHFormDataValidation.D1_RL_NKCountryDestinationCodeIsInvalid);

			SADHData.D1_RL_NKCountryOfDestination = ZString.Empty;
			AssertHasMessageError(SADHData.D1_RL_NKCountryOfDestinationInfo, SADHFormDataValidation.D1_RL_NKCountryDestinationCodeEmpty);

			SADHData.D1_MessageType = JobMessageTypeList.Codes.Export;
			AssertNoMessageError("Export", SADHData.D1_RL_NKCountryOfDestinationInfo, SADHFormDataValidation.D1_RL_NKCountryDestinationCodeEmpty);
			SADHData.D1_RL_NKCountryOfDestination = "ZZZZZ";
			AssertHasMessageError("Export", SADHData.D1_RL_NKCountryOfDestinationInfo, SADHFormDataValidation.D1_RL_NKCountryDestinationCodeIsInvalid);
		}

		public void TestCountryOfOrigin()
		{
			SADHData.D1_MessageType = JobMessageTypeList.Codes.Import;

			SADHData.D1_RL_NKCountryOfOrigin = "NZAKL";
			AssertNoMessageError(SADHData.D1_RL_NKCountryOfOriginInfo, SADHFormDataValidation.D1_RL_NKCountryOriginCodeEmpty);

			SADHData.D1_RL_NKCountryOfOrigin = "ZZZZZ";
			AssertHasMessageError(SADHData.D1_RL_NKCountryOfOriginInfo, SADHFormDataValidation.D1_RL_NKCountryOriginCodeIsInvalid);

			SADHData.D1_RL_NKCountryOfOrigin = ZString.Empty;
			AssertHasMessageError(SADHData.D1_RL_NKCountryOfOriginInfo, SADHFormDataValidation.D1_RL_NKCountryOriginCodeEmpty);

			SADHData.D1_MessageType = JobMessageTypeList.Codes.Export;
			AssertNoMessageError("Export", SADHData.D1_RL_NKCountryOfOriginInfo, SADHFormDataValidation.D1_RL_NKCountryOriginCodeEmpty);
			SADHData.D1_RL_NKCountryOfOrigin = "ZZZZZ";
			AssertHasMessageError("Export", SADHData.D1_RL_NKCountryOfOriginInfo, SADHFormDataValidation.D1_RL_NKCountryOriginCodeIsInvalid);
		}

		public void TestCDispExp()
		{
			SADHData.D1_RL_NKCountryOfDispatchOrExport = "NZAKL";
			AssertNoMessageError(SADHData.D1_RL_NKCountryOfDispatchOrExportInfo, SADHFormDataValidation.D1_RL_NKCDispExpCodeEmpty);

			SADHData.D1_RL_NKCountryOfDispatchOrExport = "ZZZZZ";
			AssertHasMessageError(SADHData.D1_RL_NKCountryOfDispatchOrExportInfo, SADHFormDataValidation.D1_RL_NKCDispExpCodeIsInvalid);

			SADHData.D1_RL_NKCountryOfDispatchOrExport = ZString.Empty;
			AssertHasMessageError(SADHData.D1_RL_NKCountryOfDispatchOrExportInfo, SADHFormDataValidation.D1_RL_NKCDispExpCodeEmpty);
		}

		public void TestInvoiceCurrency()
		{
			RefCurrencyCollection currencyList = SADHData.Lookups.CurrencyList;
			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
			currencyList.AdditionalFilter = new ZQuery(RefCurrencySchema.PK, currency.PK);
			SADHData.D1_RX_InvoiceCurrency = currency.PK;

			AssertNoError(SADHData.D1_RX_InvoiceCurrencyInfo, SADHFormDataValidation.D1_RX_InvoiceCurrencyEmpty);

			SADHData.D1_RX_InvoiceCurrency = ZGuid.Invalid;
			AssertHasError(SADHData.D1_RX_InvoiceCurrencyInfo, SADHFormDataValidation.D1_RX_InvoiceCurrencyInvalid);

			SADHData.D1_RX_InvoiceCurrency = ZGuid.Empty;
			AssertHasError(SADHData.D1_RX_InvoiceCurrencyInfo, SADHFormDataValidation.D1_RX_InvoiceCurrencyEmpty);
		}

		public void TestConsignor()
		{
			OrgHeaderCollection suppliersList = SADHData.Lookups.SuppliersList;
			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_IsConsignor = ZBool.True;

			SADHData.D1_OH_Consignor = supplier.PK;
			suppliersList.Add(supplier);

			AssertNoMessageError(SADHData.D1_OH_ConsignorInfo, SADHFormDataValidation.D1_OH_ConsignorEmpty);

			SADHData.D1_OH_Consignor = ZGuid.Invalid;
			AssertHasError(SADHData.D1_OH_ConsignorInfo, SADHFormDataValidation.D1_OH_ConsignorInvalid);

			SADHData.D1_OH_Consignor = ZGuid.Empty;
			AssertHasMessageError(SADHData.D1_OH_ConsignorInfo, SADHFormDataValidation.D1_OH_ConsignorEmpty);
		}

		public void TestConsignee()
		{
			OrgHeaderCollection suppliersList = SADHData.Lookups.SuppliersList;
			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_IsConsignee = ZBool.True;

			SADHData.D1_OH_Consignee = supplier.PK;
			suppliersList.Add(supplier);

			AssertNoMessageError(SADHData.D1_OH_ConsigneeInfo, SADHFormDataValidation.D1_OH_ConsigneeEmpty);

			SADHData.D1_OH_Consignee = ZGuid.Invalid;
			AssertHasError(SADHData.D1_OH_ConsigneeInfo, SADHFormDataValidation.D1_OH_ConsigneeInvalid);

			SADHData.D1_OH_Consignee = ZGuid.Empty;
			AssertHasMessageError(SADHData.D1_OH_ConsigneeInfo, SADHFormDataValidation.D1_OH_ConsigneeEmpty);
		}

		public void TestPlaceUnloading()
		{
			SADHData.D1_RL_NKPlaceOfUnloading = "NZAKL";
			AssertNoMessageError(SADHData.D1_RL_NKPlaceOfUnloadingInfo, SADHFormDataValidation.D1_RL_NKPlaceUnloadingEmpty);

			SADHData.D1_RL_NKPlaceOfUnloading = "ZZZZZ";
			AssertHasMessageError(SADHData.D1_RL_NKPlaceOfUnloadingInfo, SADHFormDataValidation.D1_RL_NKPlaceUnloadingIsInvalid);

			SADHData.D1_RL_NKPlaceOfUnloading = ZString.Empty;
			AssertHasMessageError(SADHData.D1_RL_NKPlaceOfUnloadingInfo, SADHFormDataValidation.D1_RL_NKPlaceUnloadingEmpty);
		}

		public void TestPackagesDescriptionGoods()
		{
			SADHData.D1_DescriptionOfGoods = "SOME DESCRIPTION";
			AssertNoWarning(SADHData.D1_DescriptionOfGoodsInfo, SADHFormDataValidation.D1_PackagesDescriptionGoodsEmpty);

			SADHData.D1_DescriptionOfGoods = ZString.Empty;
			AssertHasWarning(SADHData.D1_DescriptionOfGoodsInfo, SADHFormDataValidation.D1_PackagesDescriptionGoodsEmpty);
		}

		public void TestCommodityCode()
		{
			SADHData.D1_CommodityCode = "ZZZZZZZZZ";
			AssertNoMessageError(SADHData.D1_CommodityCodeInfo, SADHFormDataValidation.D1_CommodityCodeEmpty);

			SADHData.D1_CommodityCode = ZString.Empty;
			AssertHasMessageError(SADHData.D1_CommodityCodeInfo, SADHFormDataValidation.D1_CommodityCodeEmpty);
		}

		public void TestModeTransportAtBorder()
		{
			SADHData.D1_ModeOfTransportAtTheBorder = Core.Constants.TransportModes.Air;
			AssertNoMessageError(SADHData.D1_ModeOfTransportAtTheBorderInfo, SADHFormDataValidation.D1_ModeTransportAtBorderEmpty);

			SADHData.D1_ModeOfTransportAtTheBorder = Core.Constants.TransportModes.Sea;
			AssertNoMessageError(SADHData.D1_ModeOfTransportAtTheBorderInfo, SADHFormDataValidation.D1_ModeTransportAtBorderEmpty);

			SADHData.D1_ModeOfTransportAtTheBorder = ZString.Empty;
			AssertHasMessageError(SADHData.D1_ModeOfTransportAtTheBorderInfo, SADHFormDataValidation.D1_ModeTransportAtBorderEmpty);
		}

		public void TestMessageType()
		{
			SADHData.D1_MessageType = JobMessageTypeList.Codes.Import;
			AssertNoMessageError(SADHData.D1_MessageTypeInfo, SADHFormDataValidation.D1_MessageTypeEmpty);

			SADHData.D1_MessageType = ZString.Empty;
			AssertHasMessageError(SADHData.D1_MessageTypeInfo, SADHFormDataValidation.D1_MessageTypeEmpty);

			SADHData.D1_MessageType = JobMessageTypeList.Codes.Export;
			AssertNoMessageError(SADHData.D1_MessageTypeInfo, SADHFormDataValidation.D1_MessageTypeEmpty);

			SADHData.D1_MessageType = "ZZZ";
			AssertHasMessageErrorContaining(SADHData.D1_MessageTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		#region Implementation
		SADHFormData SADHData
		{
			get { return fSADHData ?? (fSADHData = new SADHFormData(Factory, Factory.New<BaseJobDeclaration>())); }
		}
		SADHFormData fSADHData;
		#endregion
	}
}
