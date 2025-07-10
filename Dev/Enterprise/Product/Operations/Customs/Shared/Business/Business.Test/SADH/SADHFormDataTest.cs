using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.SADH.Testing
{
	[TestedType(typeof(SADHFormData))]
	public class SADHFormDataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCurrency()
		{
			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
			currency.RX_Code = "ABC";
			FormData.D1_RX_InvoiceCurrency = currency.PK;
			AssertEquals("FormData.Currency", "ABC", FormData.Currency);
			AssertEquals("FormData.CurrencyInfo.ReadOnly", true, FormData.CurrencyInfo.ReadOnly);

			currency.RX_Code = "ZYX";
			AssertEquals("FormData.Currency", "ZYX", FormData.Currency);
			AssertEquals("FormData.CurrencyInfo.ReadOnly", true, FormData.CurrencyInfo.ReadOnly);
		}

		public void TestIsImportAndIsExport()
		{
			FormData.D1_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("FormData.IsImport", true, FormData.IsImport);
			AssertEquals("FormData.IsExport", false, FormData.IsExport);
			FormData.D1_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("FormData.IsImport", false, FormData.IsImport);
			AssertEquals("FormData.IsExport", true, FormData.IsExport);
			FormData.D1_MessageType = "ZZZ";
			AssertEquals("FormData.IsImport", false, FormData.IsImport);
			AssertEquals("FormData.IsExport", false, FormData.IsExport);
		}

		public void TestWontAllowANullDeclarationPassedIntoTheConstructor()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{ SADHFormData x = new SADHFormData(Factory, null); });
		}

		public void TestExchangeRate()
		{
			RefCurrency currency = Factory.New<RefCurrency>();
			currency.RX_Code = "XXX";
			RefExchangeRate exchangeRate = Factory.New<RefExchangeRate>();

			exchangeRate.RE_ExRateType = "CUS";
			exchangeRate.RE_StartDate = ZDateTime.Now.AddMonths(-1);
			exchangeRate.RE_ExpiryDate = ZDateTime.Now.AddMonths(1);
			exchangeRate.RE_SellRate = 0.5m;
			currency.ExchangeRates.Add(exchangeRate);

			FormData.D1_RX_InvoiceCurrency = currency.PK;

			AssertEquals("FormData.ExchangeRate", 0.5m, FormData.ExchangeRate);
			AssertEquals("FormData.ExchangeRate", FormData.CurrConverter.GetExchangeRate(currency), FormData.ExchangeRate);

			exchangeRate.RE_SellRate = 0.7m;

			AssertEquals("FormData.ExchangeRate", 0.7m, FormData.ExchangeRate);
			AssertEquals("FormData.ExchangeRate", FormData.CurrConverter.GetExchangeRate(currency), FormData.ExchangeRate);
		}

		public void TestCountryOfDispatchExportCode()
		{
			FormData.D1_RL_NKCountryOfDispatchOrExport = "NZAKL";
			AssertEquals("FormData.CountryOfDispatchExportCode", "NZ - New Zealand", FormData.CountryOfDispatchExportCode);
			AssertEquals("FormData.CountryOfDispatchExportCodeInfo.ReadOnly", true, FormData.CountryOfDispatchExportCodeInfo.ReadOnly);
			FormData.D1_RL_NKCountryOfDispatchOrExport = "AUMEL";
			AssertEquals("FormData.CountryOfDispatchExportCode", "AU - Australia", FormData.CountryOfDispatchExportCode);
			AssertEquals("FormData.CountryOfDispatchExportCodeInfo.ReadOnly", true, FormData.CountryOfDispatchExportCodeInfo.ReadOnly);
			FormData.D1_RL_NKCountryOfDispatchOrExport = ZString.Empty;
			AssertEquals("FormData.CountryOfDispatchExportCode", ZString.Empty, FormData.CountryOfDispatchExportCode);
			AssertEquals("FormData.CountryOfDispatchExportCodeInfo.ReadOnly", true, FormData.CountryOfDispatchExportCodeInfo.ReadOnly);
			FormData.D1_RL_NKCountryOfDispatchOrExport = "=-inv";
			AssertEquals("FormData.CountryOfDispatchExportCode", ZString.Empty, FormData.CountryOfDispatchExportCode);
			AssertEquals("FormData.CountryOfDispatchExportCodeInfo.ReadOnly", true, FormData.CountryOfDispatchExportCodeInfo.ReadOnly);
		}

		public void TestConsignor()
		{
			OrgHeader consignor = Factory.New<OrgHeader>();

			FormData.D1_OH_Consignor = consignor.PK;
			AssertEquals("FormData.Consignor (valid)", consignor, FormData.Consignor);

			FormData.D1_OH_Consignor = ZGuid.Empty;
			AssertEquals("FormData.Consignor (empty)", null, FormData.Consignor);

			FormData.D1_OH_Consignor = ZGuid.Invalid;
			AssertEquals("FormData.Consignor (invalid)", null, FormData.Consignor);
		}

		public void TestConsignee()
		{
			OrgHeader consignee = Factory.New<OrgHeader>();

			FormData.D1_OH_Consignee = consignee.PK;
			AssertEquals("FormData.Consignee (valid)", consignee, FormData.Consignee);

			FormData.D1_OH_Consignee = ZGuid.Empty;
			AssertEquals("FormData.Consignee (empty)", null, FormData.Consignee);

			FormData.D1_OH_Consignee = ZGuid.Invalid;
			AssertEquals("FormData.Consignee (invalid)", null, FormData.Consignee);
		}

		public void TestDeclarantRepresentativeFormattedAddress()
		{
			GlbCompany.CurrentCompany.OrgProxy.OH_FullName = "LIQUID CRYSTAL";
			OrgAddress address = GlbCompany.CurrentCompany.OrgProxy.MainAddress;
			address.OA_Address1 = "102 BOURKE ROAD";
			address.OA_Address2 = "ALEXANDRIA";
			GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort = "AUSYD";

			AssertEquals("LIQUID CRYSTAL\r\n102 BOURKE ROAD\r\nALEXANDRIA\r\n4010\r\nNSW AUSTRALIA", FormData.DeclarantRepresentativeFormattedAddress);
		}

		public void TestCountryOfOriginCode()
		{
			FormData.D1_RL_NKCountryOfOrigin = "NZAKL";
			AssertEquals("FormData.CountryOfOrigin", "NZ - New Zealand", FormData.CountryOfOriginFormatted);
			AssertEquals("FormData.CountryOfOriginCodeInfo.ReadOnly", true, FormData.CountryOfOriginFormattedInfo.ReadOnly);
			FormData.D1_RL_NKCountryOfOrigin = "AUMEL";
			AssertEquals("FormData.CountryOfOrigin", "AU - Australia", FormData.CountryOfOriginFormatted);
			AssertEquals("FormData.CountryOfOriginCodeInfo.ReadOnly", true, FormData.CountryOfOriginFormattedInfo.ReadOnly);
			FormData.D1_RL_NKCountryOfOrigin = ZString.Empty;
			AssertEquals("FormData.CountryOfOrigin", ZString.Empty, FormData.CountryOfOriginFormatted);
			AssertEquals("FormData.CountryOfOriginCodeInfo.ReadOnly", true, FormData.CountryOfOriginFormattedInfo.ReadOnly);
			FormData.D1_RL_NKCountryOfOrigin = "=-inv";
			AssertEquals("FormData.CountryOfOrigin", ZString.Empty, FormData.CountryOfOriginFormatted);
			AssertEquals("FormData.CountryOfOriginCodeInfo.ReadOnly", true, FormData.CountryOfOriginFormattedInfo.ReadOnly);
		}

		public void TestCountryOfDestinationCode()
		{
			FormData.D1_RL_NKCountryOfDestination = "NZAKL";
			AssertEquals("FormData.CountryOfDestinationFormatted", "NZ - New Zealand", FormData.CountryOfDestinationFormatted);
			AssertEquals("FormData.CountryOfDestinationCodeInfo.ReadOnly", true, FormData.CountryOfDestinationFormattedInfo.ReadOnly);
			FormData.D1_RL_NKCountryOfDestination = "AUMEL";
			AssertEquals("FormData.CountryOfDestinationFormatted", "AU - Australia", FormData.CountryOfDestinationFormatted);
			AssertEquals("FormData.CountryOfDestinationCodeInfo.ReadOnly", true, FormData.CountryOfDestinationFormattedInfo.ReadOnly);
			FormData.D1_RL_NKCountryOfDestination = ZString.Empty;
			AssertEquals("FormData.CountryOfDestinationFormatted", ZString.Empty, FormData.CountryOfDestinationFormatted);
			AssertEquals("FormData.CountryOfDestinationCodeInfo.ReadOnly", true, FormData.CountryOfDestinationFormattedInfo.ReadOnly);
			FormData.D1_RL_NKCountryOfDestination = "=-inv";
			AssertEquals("FormData.CountryOfDestinationFormatted", ZString.Empty, FormData.CountryOfDestinationFormatted);
			AssertEquals("FormData.CountryOfDestinationCodeInfo.ReadOnly", true, FormData.CountryOfDestinationFormattedInfo.ReadOnly);
		}

		public void TestConsignorFormattedAddress()
		{
			AssertEquals("", FormData.ConsignorFormattedAddress);

			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "BEN'S SHOES STINKY FEET";
			OrgAddress address = consignor.MainAddress;
			address.OA_Address1 = "102 BOURKE ROAD";
			address.OA_Address2 = "dd";
			address.OA_City = "LONDON";
			address.OA_PostCode = "4121";
			consignor.OH_RL_NKClosestPort = "GBLON";

			FormData.D1_OH_Consignor = consignor.PK;
			AssertEquals("BEN'S SHOES STINKY FEET\r\n102 BOURKE ROAD\r\ndd\r\nLONDON 4121\r\nLND UNITED KINGDOM", FormData.ConsignorFormattedAddress);

			OrgHeader consignor2 = Factory.New<OrgHeader>();
			consignor2.OH_FullName = "UNFORMATTED LOGS";
			OrgAddress address2 = consignor2.MainAddress;
			address2.OA_Address1 = "102 BOURKE ROAD";
			address2.OA_Address2 = "dd";

			FormData.D1_OH_Consignor = consignor2.PK;
			AssertEquals("UNFORMATTED LOGS\r\n102 BOURKE ROAD\r\ndd", FormData.ConsignorFormattedAddress);

			FormData.D1_OH_Consignor = ZGuid.Invalid;
			AssertEquals("", FormData.ConsignorFormattedAddress);

			FormData.D1_OH_Consignor = ZGuid.Empty;
			AssertEquals("", FormData.ConsignorFormattedAddress);
		}

		public void TestConsigneeFormattedAddress()
		{
			AssertEquals("", FormData.ConsigneeFormattedAddress);

			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "LONDON'S FINEST";
			OrgAddress address = consignee.MainAddress;
			address.OA_Address1 = "24/152 WESTINGTON STREET";
			address.OA_Address2 = "CHELTENHAM";
			address.OA_City = "LONDON";
			address.OA_PostCode = "4121";
			consignee.OH_RL_NKClosestPort = "GBLON";

			FormData.D1_OH_Consignee = consignee.PK;
			AssertEquals("LONDON'S FINEST\r\n24/152 WESTINGTON STREET\r\nCHELTENHAM\r\nLONDON 4121\r\nLND UNITED KINGDOM", FormData.ConsigneeFormattedAddress);

			OrgHeader consignee2 = Factory.New<OrgHeader>();
			consignee2.OH_FullName = "I CAN'T BELIEVE IT'S NOT BUTTER";
			OrgAddress address2 = consignee2.MainAddress;
			address2.OA_Address1 = "24/152 WESTINGTON STREET";
			address2.OA_Address2 = "CHELTENHAM";

			FormData.D1_OH_Consignee = consignee2.PK;
			AssertEquals("I CAN'T BELIEVE IT'S NOT BUTTER\r\n24/152 WESTINGTON STREET\r\nCHELTENHAM", FormData.ConsigneeFormattedAddress);

			FormData.D1_OH_Consignee = ZGuid.Invalid;
			AssertEquals("", FormData.ConsigneeFormattedAddress);

			FormData.D1_OH_Consignee = ZGuid.Empty;
			AssertEquals("", FormData.ConsigneeFormattedAddress);
		}

		#region Implementation
		BaseJobDeclaration Declaration
		{
			get { return fDeclaration ?? (fDeclaration = Factory.New<BaseJobDeclaration>()); }
		}
		BaseJobDeclaration fDeclaration;

		SADHFormData FormData
		{
			get { return fFormData ?? (fFormData = new SADHFormData(Factory, Declaration)); }
		}
		SADHFormData fFormData;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SADHFormData(Factory, Factory.New<BaseJobDeclaration>());
		}
		#endregion
	}
}
