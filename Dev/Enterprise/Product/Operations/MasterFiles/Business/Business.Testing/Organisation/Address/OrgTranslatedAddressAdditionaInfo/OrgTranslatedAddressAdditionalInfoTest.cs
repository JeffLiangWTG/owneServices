using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgTranslatedAddressAdditionalInfo))]
	public class OrgTranslatedAddressAdditionalInfoTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<OrgTranslatedAddressAdditionalInfo>();
		}

		public void TestTranslatedAddressWithValidAdditionalInfo()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Code = "Test Address";
			address.OA_OH = header.PK;

			var addressInfo = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			addressInfo.OAI_AdditionalInfo = "Test Additional Info 1";
			addressInfo.OAI_OA_Address = address.PK;

			var translatedAddress = Factory.NewWithValidTestData<OrgTranslatedAddress>();
			translatedAddress.OTA_OA = address.PK;
			translatedAddress.OTA_Language = Core.SharedConstants.Languages.ChineseSimplified;

			var translatedInfo = Factory.NewWithValidTestData<OrgTranslatedAddressAdditionalInfo>();
			translatedInfo.OTI_AdditionalInfo = "测试附加信息 1";
			translatedInfo.OTI_Language = Core.SharedConstants.Languages.ChineseSimplified;
			translatedInfo.OTI_OAI = addressInfo.PK;

			Factory.Save();

			AssertEquals(address.PK, addressInfo.OAI_OA_Address);
			AssertEquals("Test Additional Info 1", addressInfo.OAI_AdditionalInfo);
			AssertEquals(Core.SharedConstants.Languages.English, address.OA_Language);
			AssertEquals(addressInfo.PK, translatedInfo.OTI_OAI);
			AssertEquals("测试附加信息 1", translatedInfo.OTI_AdditionalInfo);
			AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, translatedInfo.OTI_Language);
		}

		public void TestSetOTI_AdditionalInfo()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Code = "Test";
			address.OA_AdditionalAddressInformation = string.Empty;

			var translatedAddress = Factory.NewWithValidTestData<OrgTranslatedAddress>();
			translatedAddress.OTA_Language = "EN";
			translatedAddress.OTA_AdditionalAddressInformation = "Translated";
			translatedAddress.OTA_OA = address.PK;

			var additionalInfo = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			additionalInfo.OAI_AdditionalInfo = "B";
			additionalInfo.OAI_IsPrimary = true;
			additionalInfo.OAI_OA_Address = address.PK;

			var translatedAddressAdditionalInfo = Factory.NewWithValidTestData<OrgTranslatedAddressAdditionalInfo>();
			translatedAddressAdditionalInfo.OTI_Language = "EN";
			translatedAddressAdditionalInfo.OTI_AdditionalInfo = "Translated";
			translatedAddressAdditionalInfo.OTI_OAI = additionalInfo.PK;
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(1, address.TranslatedAddresses.Count);
				AssertEquals("EN", address.TranslatedAddresses[0].OTA_Language);
				AssertEquals("Translated", address.TranslatedAddresses[0].OTA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(1, address.AdditionalInfos[0].TranslatedInfos.Count);
				AssertEquals("EN", address.AdditionalInfos[0].TranslatedInfos[0].OTI_Language);
				AssertEquals("Translated", address.AdditionalInfos[0].TranslatedInfos[0].OTI_AdditionalInfo);
			});

			translatedAddressAdditionalInfo.OTI_AdditionalInfo = "12345";
			CombineAssertions(() =>
			{
				AssertEquals(1, address.TranslatedAddresses.Count);
				AssertEquals("EN", address.TranslatedAddresses[0].OTA_Language);
				AssertEquals("12345", address.TranslatedAddresses[0].OTA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(1, address.AdditionalInfos[0].TranslatedInfos.Count);
				AssertEquals("EN", address.AdditionalInfos[0].TranslatedInfos[0].OTI_Language);
				AssertEquals("12345", address.AdditionalInfos[0].TranslatedInfos[0].OTI_AdditionalInfo);
			});
		}

		public void TestSetOTI_Language()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Code = "Test";
			address.OA_AdditionalAddressInformation = string.Empty;

			var translatedAddress = Factory.NewWithValidTestData<OrgTranslatedAddress>();
			translatedAddress.OTA_Language = "EN";
			translatedAddress.OTA_AdditionalAddressInformation = "Translated";
			translatedAddress.OTA_OA = address.PK;

			var additionalInfo = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			additionalInfo.OAI_AdditionalInfo = "B";
			additionalInfo.OAI_IsPrimary = true;
			additionalInfo.OAI_OA_Address = address.PK;

			var translatedAddressAdditionalInfo = Factory.NewWithValidTestData<OrgTranslatedAddressAdditionalInfo>();
			translatedAddressAdditionalInfo.OTI_Language = "EN";
			translatedAddressAdditionalInfo.OTI_AdditionalInfo = "Translated";
			translatedAddressAdditionalInfo.OTI_OAI = additionalInfo.PK;
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(1, address.TranslatedAddresses.Count);
				AssertEquals("EN", address.TranslatedAddresses[0].OTA_Language);
				AssertEquals("Translated", address.TranslatedAddresses[0].OTA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(1, address.AdditionalInfos[0].TranslatedInfos.Count);
				AssertEquals("EN", address.AdditionalInfos[0].TranslatedInfos[0].OTI_Language);
				AssertEquals("Translated", address.AdditionalInfos[0].TranslatedInfos[0].OTI_AdditionalInfo);
			});

			translatedAddressAdditionalInfo.OTI_Language = "EN-US";
			CombineAssertions(() =>
			{
				AssertEquals(1, address.TranslatedAddresses.Count);
				AssertEquals("EN-US", address.TranslatedAddresses[0].OTA_Language);
				AssertEquals("Translated", address.TranslatedAddresses[0].OTA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(1, address.AdditionalInfos[0].TranslatedInfos.Count);
				AssertEquals("EN-US", address.AdditionalInfos[0].TranslatedInfos[0].OTI_Language);
				AssertEquals("Translated", address.AdditionalInfos[0].TranslatedInfos[0].OTI_AdditionalInfo);
			});
		}
	}
}
