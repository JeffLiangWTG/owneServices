using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CanadianCustomsCodeValidatorTest : TestCase
	{
		public void TestGetServiceProviderAuthorizationIDError()
		{
			AssertEquals("No Error", ZString.Empty, CanadianCustomsCodeValidator.GetServiceProviderAuthorizationIDError("AA2312"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.ServiceProviderAuthorizationIDRightFormat, CanadianCustomsCodeValidator.GetServiceProviderAuthorizationIDError("BS2312"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.ServiceProviderAuthorizationIDRightFormat, CanadianCustomsCodeValidator.GetServiceProviderAuthorizationIDError("BAF312"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.ServiceProviderAuthorizationIDRightFormat, CanadianCustomsCodeValidator.GetServiceProviderAuthorizationIDError("3A3312"));
		}

		public void TestGetCarrierCode()
		{
			AssertEquals("No Error", ZString.Empty, CanadianCustomsCodeValidator.GetCarrierCodeError("BLAH"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.CarrierCodeRightFormat, CanadianCustomsCodeValidator.GetCarrierCodeError("A22312"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.CarrierCodeRightFormat, CanadianCustomsCodeValidator.GetCarrierCodeError("BLA"));
		}

		public void TestGetAuthorizationIDError()
		{
			AssertEquals("No Error", ZString.Empty, CanadianCustomsCodeValidator.GetAuthorizationIDError("AS2312"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.AuthorizationIDRightFormat, CanadianCustomsCodeValidator.GetAuthorizationIDError("A22312"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.AuthorizationIDRightFormat, CanadianCustomsCodeValidator.GetAuthorizationIDError("AAF312"));
		}

		public void TestGetExportLicenceNumber()
		{
			AssertEquals("No Error", ZString.Empty, CanadianCustomsCodeValidator.GetExportLicenceNumberError("ABC123"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.ExportLicenceNumberCodeRightFormat, CanadianCustomsCodeValidator.GetExportLicenceNumberError("1234567"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.ExportLicenceNumberCodeRightFormat, CanadianCustomsCodeValidator.GetExportLicenceNumberError("12345"));
		}

		public void TestGetBusinessNumberForPayrollDeductionsError()
		{
			AssertEquals("No Error", ZString.Empty, CanadianCustomsCodeValidator.GetBusinessNumberForPayrollDeductionsError("123456789RP1234"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.BusinessNumberForPayrollDeductionsRightFormat, CanadianCustomsCodeValidator.GetBusinessNumberForPayrollDeductionsError("123456789RC1234"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.BusinessNumberForPayrollDeductionsRightFormat, CanadianCustomsCodeValidator.GetBusinessNumberForPayrollDeductionsError("123456789RP12A4"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.BusinessNumberForPayrollDeductionsRightFormat, CanadianCustomsCodeValidator.GetBusinessNumberForPayrollDeductionsError("A23456789RP12A4"));
		}

		public void TestGetBusinessNumberForImportExportError()
		{
			AssertEquals("No Error", ZString.Empty, CanadianCustomsCodeValidator.GetBusinessNumberForImportExportError("123456789RM1234"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.BusinessNumberForImportExportFormat, CanadianCustomsCodeValidator.GetBusinessNumberForImportExportError("123456789RC1234"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.BusinessNumberForImportExportFormat, CanadianCustomsCodeValidator.GetBusinessNumberForImportExportError("123456789RM12A4"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.BusinessNumberForImportExportFormat, CanadianCustomsCodeValidator.GetBusinessNumberForImportExportError("A23456789RM12A4"));
		}

		public void TestGetBusinessNumberCustomsBrokerError()
		{
			AssertEquals("No Error", ZString.Empty, CanadianCustomsCodeValidator.GetBusinessNumberCustomsBrokerError("123456789RM1234"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.BusinessNumberCustomsBrokerFormat, CanadianCustomsCodeValidator.GetBusinessNumberCustomsBrokerError("123456789RC1234"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.BusinessNumberCustomsBrokerFormat, CanadianCustomsCodeValidator.GetBusinessNumberCustomsBrokerError("123456789RM12A4"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.BusinessNumberCustomsBrokerFormat, CanadianCustomsCodeValidator.GetBusinessNumberCustomsBrokerError("A23456789RM12A4"));
		}

		public void TestGetBusinessNumberImporterNonCommercialError()
		{
			AssertEquals("No Error", ZString.Empty, CanadianCustomsCodeValidator.GetBusinessNumberImporterNonCommercialError("123456789RM1234"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.BusinessNumberImporterNonCommercialFormat, CanadianCustomsCodeValidator.GetBusinessNumberImporterNonCommercialError("123456789RC1234"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.BusinessNumberImporterNonCommercialFormat, CanadianCustomsCodeValidator.GetBusinessNumberImporterNonCommercialError("123456789RM12A4"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.BusinessNumberImporterNonCommercialFormat, CanadianCustomsCodeValidator.GetBusinessNumberImporterNonCommercialError("A23456789RM12A4"));
		}

		public void TestGetBusinessNumberForExportError()
		{
			AssertEquals("No Error", ZString.Empty, CanadianCustomsCodeValidator.GetBusinessNumberForExportError("123456789RM1234"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.BusinessNumberForExportFormat, CanadianCustomsCodeValidator.GetBusinessNumberForExportError("123456789RC1234"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.BusinessNumberForExportFormat, CanadianCustomsCodeValidator.GetBusinessNumberForExportError("123456789RM12A4"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.BusinessNumberForExportFormat, CanadianCustomsCodeValidator.GetBusinessNumberForExportError("A23456789RM12A4"));
		}

		public void TestGetBusinessNumberForLowValueShipmentsError()
		{
			AssertEquals("No Error", ZString.Empty, CanadianCustomsCodeValidator.GetBusinessNumberForLowValueShipmentsError("123456789RM1234"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.BusinessNumberForLowValueShipmentsFormat, CanadianCustomsCodeValidator.GetBusinessNumberForLowValueShipmentsError("123456789RC1234"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.BusinessNumberForLowValueShipmentsFormat, CanadianCustomsCodeValidator.GetBusinessNumberForLowValueShipmentsError("123456789RM12A4"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.BusinessNumberForLowValueShipmentsFormat, CanadianCustomsCodeValidator.GetBusinessNumberForLowValueShipmentsError("A23456789RM12A4"));
		}

		public void TestGetBusinessNumberForGoodsServicesHarmonizedSalesTaxError()
		{
			AssertEquals("No Error", ZString.Empty, CanadianCustomsCodeValidator.GetBusinessNumberForGoodsServicesHarmonizedSalesTaxError("123456789RT1234"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.BusinessNumberForGoodsServicesHarmonizedSalesTaxFormat, CanadianCustomsCodeValidator.GetBusinessNumberForGoodsServicesHarmonizedSalesTaxError("123456789RC1234"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.BusinessNumberForGoodsServicesHarmonizedSalesTaxFormat, CanadianCustomsCodeValidator.GetBusinessNumberForGoodsServicesHarmonizedSalesTaxError("123456789RT12A4"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.BusinessNumberForGoodsServicesHarmonizedSalesTaxFormat, CanadianCustomsCodeValidator.GetBusinessNumberForGoodsServicesHarmonizedSalesTaxError("A23456789RT12A4"));
		}

		public void TestGetBusinessNumberForCorporateIncomeTaxError()
		{
			AssertEquals("No Error", ZString.Empty, CanadianCustomsCodeValidator.GetBusinessNumberForCorporateIncomeTaxError("123456789RC1234"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.BusinessNumberForCorporateIncomeTaxFormat, CanadianCustomsCodeValidator.GetBusinessNumberForCorporateIncomeTaxError("123456789RT1234"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.BusinessNumberForCorporateIncomeTaxFormat, CanadianCustomsCodeValidator.GetBusinessNumberForCorporateIncomeTaxError("123456789RC12A4"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.BusinessNumberForCorporateIncomeTaxFormat, CanadianCustomsCodeValidator.GetBusinessNumberForCorporateIncomeTaxError("A23456789RC12A4"));
		}

		public void TestImporterNumberError()
		{
			AssertEquals("No Error", ZString.Empty, CanadianCustomsCodeValidator.GetBusinessNumberImporterCommercialError("123456789"));
			AssertEquals("No Error", ZString.Empty, CanadianCustomsCodeValidator.GetBusinessNumberImporterCommercialError("12345678A"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.BusinessNumberImporterCommercialFormat, CanadianCustomsCodeValidator.GetBusinessNumberImporterCommercialError("12345678A9"));
			AssertEquals("Has Error", CanadianCustomsCodeValidator.Constants.BusinessNumberImporterCommercialFormat, CanadianCustomsCodeValidator.GetBusinessNumberImporterCommercialError("123456"));
		}
	}
}
