using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public class CustomsOfficeCodeTestHelper
	{
		public CustomsOfficeCodeTestHelper(UniversalReferenceTestDataHelper helper)
		{
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, "Northern Ireland Part of Great Britain");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", eun);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Currency, "Currency");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "Package Types");
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");

			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, "MainCustomsOffice", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, "MainCustomsOffice", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, "MainCustomsOffice", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Currency, Core.Constants.CurrencyCodes.EuropeanUnion, "EUR DESC", dateMin, dateMax);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE003478", "DE003478 DESC", dateMin, dateMax);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "BOX", "BOX DESC", dateMin, dateMax);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT008734", "IT008734 DESC", dateMin, dateMax);

			CusofXI = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "XI005342", "XI005342 DESC", dateMin, dateMax);
			CusofIT = helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT009278", "ITALIAN OFFICE", dateMin, dateMax, RefCusCodeListAttributeTypes.Codes.ROLE, EoriRegistrationAuthoritiesCodeType);
			CusofDE = helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE004323", "GERMAN OFFICE", dateMin, dateMax, RefCusCodeListAttributeTypes.Codes.ROLE, CompetentAuthorityOfEnquiryCodeType);
		}

		public RefCusCodeList CusofIT { get; }

		public RefCusCodeList CusofDE { get; }

		public RefCusCodeList CusofXI { get; }

		public const string EoriRegistrationAuthoritiesCodeType = "REG";
		public const string CompetentAuthorityOfEnquiryCodeType = "ENQ";
		readonly ZDateTime dateMin = ZDateTime.MinSmallDateTimeValue;
		readonly ZDateTime dateMax = ZDateTime.MaxSmallDateTimeValue;
	}
}
