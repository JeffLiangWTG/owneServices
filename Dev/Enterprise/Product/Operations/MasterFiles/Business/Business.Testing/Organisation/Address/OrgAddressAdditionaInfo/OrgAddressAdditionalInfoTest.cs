using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgAddressAdditionalInfo))]
	public class OrgAddressAdditionalInfoTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
		}

		public void TestAddressWithValidAdditionalInfo()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Code = "Test Address";
			address.OA_OH = header.PK;

			var addressInfo1 = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			addressInfo1.OAI_OA_Address = address.PK;
			addressInfo1.OAI_AdditionalInfo = "Test Additional Info 1";

			var addressInfo2 = Factory.New<OrgAddressAdditionalInfo>();
			addressInfo2.OAI_OA_Address = address.PK;
			addressInfo2.OAI_AdditionalInfo = "Test Additional Info 2";

			Factory.Save();

			AssertEquals(address.PK, addressInfo1.OAI_OA_Address);
			AssertEquals("Test Additional Info 1", addressInfo1.OAI_AdditionalInfo);
			AssertEquals(address.PK, addressInfo2.OAI_OA_Address);
			AssertEquals("Test Additional Info 2", addressInfo2.OAI_AdditionalInfo);
		}

		public void TestDeleteRelatedTranslatedAdditionalAddress()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Code = "Test Address";
			address.OA_OH = header.PK;

			var addressInfo = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			addressInfo.OAI_OA_Address = address.PK;
			addressInfo.OAI_AdditionalInfo = "Test Additional Info 1";

			var translatedAddressInfo = Factory.NewWithValidTestData<OrgTranslatedAddressAdditionalInfo>();
			translatedAddressInfo.OTI_OAI = addressInfo.PK;

			Factory.Save();

			addressInfo.Delete();
			Assert(translatedAddressInfo.IsDeleted);
		}

		public void TestDeleteMainAdditionalInformation_ResetOrgAddressAdditionalAddressInformation()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Code = "Test Address";
			address.OA_OH = header.PK;

			var additionalText = "Test Additional Info 1";
			var additionalInfo = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			additionalInfo.OAI_OA_Address = address.PK;
			additionalInfo.OAI_AdditionalInfo = additionalText;
			additionalInfo.OAI_IsPrimary = true;

			var translatedAdditionalText = "Translated additional info";
			var translatedAddress = Factory.NewWithValidTestData<OrgTranslatedAddress>();
			translatedAddress.OTA_OA = address.PK;
			translatedAddress.OTA_AdditionalAddressInformation = translatedAdditionalText;

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("OA_AdditionalAddressInformation has value", additionalText, address.OA_AdditionalAddressInformation);
				AssertEquals("OTA_AdditionalAddressInformation has value", translatedAdditionalText, translatedAddress.OTA_AdditionalAddressInformation);
			});

			additionalInfo.Delete();

			CombineAssertions(() =>
			{
				AssertEquals("OA_AdditionalAddressInformation should be empty", string.Empty, address.OA_AdditionalAddressInformation);
				AssertEquals("OTA_AdditionalAddressInformation should be empty", string.Empty, translatedAddress.OTA_AdditionalAddressInformation);
			});
		}

		public void TestSetOAI_AdditionalInfo()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Code = "Test";
			address.OA_AdditionalAddressInformation = string.Empty;

			var additionalInfo_Primary = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			additionalInfo_Primary.OAI_OA_Address = address.PK;
			additionalInfo_Primary.OAI_IsPrimary = true;
			additionalInfo_Primary.OAI_AdditionalInfo = "A";

			var additionalInfo_NotPrimary = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			additionalInfo_NotPrimary.OAI_OA_Address = address.PK;
			additionalInfo_NotPrimary.OAI_AdditionalInfo = "B";
			additionalInfo_NotPrimary.OAI_IsPrimary = false;

			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals("A", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals("A", additionalInfo_Primary.OAI_AdditionalInfo);
				AssertEquals("B", additionalInfo_NotPrimary.OAI_AdditionalInfo);
			});

			additionalInfo_Primary.OAI_AdditionalInfo = "C";
			additionalInfo_NotPrimary.OAI_AdditionalInfo = "D";

			CombineAssertions(() =>
			{
				AssertEquals("C", address.OA_AdditionalAddressInformation);
				AssertEquals(2, address.AdditionalInfos.Count);
				AssertEquals("C", additionalInfo_Primary.OAI_AdditionalInfo);
				AssertEquals("D", additionalInfo_NotPrimary.OAI_AdditionalInfo);
			});
		}

		public void TestOnlyOneItemSetOAI_IsPrimaryTrue_ShouldReplaceOaAdditionalAddress()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Code = "Test";
			address.OA_AdditionalAddressInformation = string.Empty;

			var additionalInfo_Primary = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			additionalInfo_Primary.OAI_OA_Address = address.PK;
			additionalInfo_Primary.OAI_IsPrimary = true;
			additionalInfo_Primary.OAI_AdditionalInfo = "A";

			Factory.Save();

			AssertEquals("A", address.OA_AdditionalAddressInformation);

			additionalInfo_Primary.OAI_AdditionalInfo = "B";
			AssertEquals("B", address.OA_AdditionalAddressInformation);
		}

		public void TestMoreThanOneItemsSetOAI_IsPrimaryTrue_ShouldNotReplaceOaAdditionalAddress()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Code = "Test";
			address.OA_AdditionalAddressInformation = string.Empty;

			var additionalInfo1 = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			additionalInfo1.OAI_OA_Address = address.PK;
			additionalInfo1.OAI_IsPrimary = true;
			additionalInfo1.OAI_AdditionalInfo = "A";

			var additionalInfo2 = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			additionalInfo2.OAI_OA_Address = address.PK;
			additionalInfo2.OAI_IsPrimary = false;
			additionalInfo2.OAI_AdditionalInfo = "B";

			Factory.Save();

			additionalInfo1.OAI_IsPrimary = true;
			AssertEquals("A", address.OA_AdditionalAddressInformation);

			additionalInfo2.OAI_AdditionalInfo = "C";
			AssertEquals("A", address.OA_AdditionalAddressInformation);
		}

		public void TestSetOAI_IsPrimaryTrue_WhenAAIIsEmpty()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Code = "Test";
			address.OA_AdditionalAddressInformation = "Original";

			AssertEquals("Original", address.PrimaryOrgAddressAdditionalInfoDetail);
			AssertEquals(1, address.PrimaryOrgAddressAdditionalInfoCount);
			AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);

			var additionalInfo_Primary1 = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			additionalInfo_Primary1.OAI_OA_Address = address.PK;
			additionalInfo_Primary1.OAI_IsPrimary = true;
			additionalInfo_Primary1.OAI_AdditionalInfo = string.Empty;
			AssertEquals("Original", address.PrimaryOrgAddressAdditionalInfoDetail);

			var additionalInfo_Primary2 = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			additionalInfo_Primary2.OAI_OA_Address = address.PK;
			additionalInfo_Primary2.OAI_IsPrimary = false;
			additionalInfo_Primary2.OAI_AdditionalInfo = "A";
			AssertEquals("Original", address.PrimaryOrgAddressAdditionalInfoDetail);

			var additionalInfo_Primary3 = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			additionalInfo_Primary3.OAI_OA_Address = address.PK;
			additionalInfo_Primary3.OAI_IsPrimary = false;
			additionalInfo_Primary3.OAI_AdditionalInfo = "B";
			AssertEquals("Original", address.PrimaryOrgAddressAdditionalInfoDetail);

			additionalInfo_Primary2.OAI_IsPrimary = true;
			AssertEquals("Original", address.PrimaryOrgAddressAdditionalInfoDetail);

			additionalInfo_Primary1.OAI_IsPrimary = false;
			address.AdditionalInfos[0].OAI_IsPrimary = false;
			AssertEquals("A", address.PrimaryOrgAddressAdditionalInfoDetail);
		}

		public void TestSetOAI_IsPrimaryFalse_ShouldConditionallyAffectOaAdditionalAddress()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Code = "Test";
			address.OA_AdditionalAddressInformation = "Original";

			AssertEquals("Original", address.PrimaryOrgAddressAdditionalInfoDetail);
			AssertEquals(1, address.PrimaryOrgAddressAdditionalInfoCount);
			AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);

			var additionalInfo_Primary1 = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			additionalInfo_Primary1.OAI_OA_Address = address.PK;
			additionalInfo_Primary1.OAI_IsPrimary = true;
			additionalInfo_Primary1.OAI_AdditionalInfo = "A";
			AssertEquals("Original", address.PrimaryOrgAddressAdditionalInfoDetail);

			var additionalInfo_Primary2 = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			additionalInfo_Primary2.OAI_OA_Address = address.PK;
			additionalInfo_Primary2.OAI_IsPrimary = true;
			additionalInfo_Primary2.OAI_AdditionalInfo = "B";
			AssertEquals("Original", address.PrimaryOrgAddressAdditionalInfoDetail);

			var additionalInfo_Primary3 = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			additionalInfo_Primary3.OAI_OA_Address = address.PK;
			additionalInfo_Primary3.OAI_IsPrimary = true;
			additionalInfo_Primary3.OAI_AdditionalInfo = "C";
			AssertEquals("Original", address.PrimaryOrgAddressAdditionalInfoDetail);

			additionalInfo_Primary2.OAI_IsPrimary = false;
			AssertEquals("Original", address.OA_AdditionalAddressInformation);

			additionalInfo_Primary1.OAI_IsPrimary = false;
			address.AdditionalInfos[0].OAI_IsPrimary = false;
			AssertEquals("C", address.OA_AdditionalAddressInformation);
		}

		public void TestSetOAI_IsPrimaryWithoutTranslatedAddress()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Code = "Test";
			address.OA_AdditionalAddressInformation = string.Empty;

			var additionalInfo_NotPrimary = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			additionalInfo_NotPrimary.OAI_OA_Address = address.PK;
			additionalInfo_NotPrimary.OAI_AdditionalInfo = "B";
			additionalInfo_NotPrimary.OAI_IsPrimary = false;
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(string.Empty, address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(false, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals("B", address.AdditionalInfos[0].OAI_AdditionalInfo);
			});

			additionalInfo_NotPrimary.OAI_IsPrimary = true;
			CombineAssertions(() =>
			{
				AssertEquals("B", address.OA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals("B", address.AdditionalInfos[0].OAI_AdditionalInfo);
			});
		}

		public void TestSetOAI_IsPrimaryWithTranslatedAddress_Case1()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Code = "Test";
			address.OA_AdditionalAddressInformation = string.Empty;

			var translatedAddress = Factory.NewWithValidTestData<OrgTranslatedAddress>();
			translatedAddress.OTA_Language = "EN";
			translatedAddress.OTA_AdditionalAddressInformation = "Translated";
			translatedAddress.OTA_OA = address.PK;

			var additionalInfo = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			additionalInfo.OAI_OA_Address = address.PK;
			additionalInfo.OAI_AdditionalInfo = "B";
			additionalInfo.OAI_IsPrimary = false;

			var translatedAddressAdditionalInfo = Factory.NewWithValidTestData<OrgTranslatedAddressAdditionalInfo>();
			translatedAddressAdditionalInfo.OTI_Language = "EN";
			translatedAddressAdditionalInfo.OTI_AdditionalInfo = "ABC";
			translatedAddressAdditionalInfo.OTI_OAI = additionalInfo.PK;
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(1, address.TranslatedAddresses.Count);
				AssertEquals("EN", address.TranslatedAddresses[0].OTA_Language);
				AssertEquals("Translated", address.TranslatedAddresses[0].OTA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(false, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(1, address.AdditionalInfos[0].TranslatedInfos.Count);
				AssertEquals("EN", address.AdditionalInfos[0].TranslatedInfos[0].OTI_Language);
				AssertEquals("ABC", address.AdditionalInfos[0].TranslatedInfos[0].OTI_AdditionalInfo);
			});

			additionalInfo.OAI_IsPrimary = true;
			CombineAssertions(() =>
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
		}

		public void TestSetOAI_IsPrimaryWithTranslatedAddress_Case2()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Code = "Test";
			address.OA_AdditionalAddressInformation = string.Empty;

			var translatedAddress = Factory.NewWithValidTestData<OrgTranslatedAddress>();
			translatedAddress.OTA_Language = "EN";
			translatedAddress.OTA_AdditionalAddressInformation = "Translated";
			translatedAddress.OTA_OA = address.PK;

			var additionalInfo = Factory.New<OrgAddressAdditionalInfo>();
			additionalInfo.OAI_OA_Address = address.PK;
			additionalInfo.OAI_AdditionalInfo = "B";
			additionalInfo.OAI_IsPrimary = false;

			var translatedAddressAdditionalInfo = Factory.NewWithValidTestData<OrgTranslatedAddressAdditionalInfo>();
			translatedAddressAdditionalInfo.OTI_Language = "EN-US";
			translatedAddressAdditionalInfo.OTI_AdditionalInfo = "ABC";
			translatedAddressAdditionalInfo.OTI_OAI = additionalInfo.PK;
			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(1, address.TranslatedAddresses.Count);
				AssertEquals("EN", address.TranslatedAddresses[0].OTA_Language);
				AssertEquals("Translated", address.TranslatedAddresses[0].OTA_AdditionalAddressInformation);
				AssertEquals(1, address.AdditionalInfos.Count);
				AssertEquals(false, address.AdditionalInfos[0].OAI_IsPrimary);
				AssertEquals(1, address.AdditionalInfos[0].TranslatedInfos.Count);
				AssertEquals("EN-US", address.AdditionalInfos[0].TranslatedInfos[0].OTI_Language);
				AssertEquals("ABC", address.AdditionalInfos[0].TranslatedInfos[0].OTI_AdditionalInfo);
			});

			additionalInfo.OAI_IsPrimary = true;
			CombineAssertions(() =>
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
		}

		public void TestDeleteAdditionalInfo_ShouldValidate()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Code = "Test";
			address.OA_AdditionalAddressInformation = string.Empty;

			var additionalInfo1 = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			additionalInfo1.OAI_OA_Address = address.PK;
			additionalInfo1.OAI_AdditionalInfo = "B";
			additionalInfo1.OAI_IsPrimary = false;

			var additionalInfo2 = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			additionalInfo2.OAI_OA_Address = address.PK;
			additionalInfo2.OAI_AdditionalInfo = "B";
			additionalInfo2.OAI_IsPrimary = false;

			additionalInfo2.Delete();

			AssertEquals(true, additionalInfo1.OAI_IsPrimaryInfo.HasError("Must specified one main address additional information."));
		}

		public void TestDeletePrimaryOAI_FirstlyUncheckIsPrimaryTrue_ShouldSynchronized()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Code = "Test";
			address.OA_AdditionalAddressInformation = "Original";

			Factory.Save();

			AssertEquals("Original", address.PrimaryOrgAddressAdditionalInfoDetail);
			AssertEquals(1, address.AdditionalInfos.Count);
			AssertEquals(true, address.AdditionalInfos[0].OAI_IsPrimary);

			address.AdditionalInfos[0].OAI_IsPrimary = false;
			address.AdditionalInfos[0].Delete();

			Factory.Save();

			AssertNullOrEmpty(address.OA_AdditionalAddressInformation);
			AssertEquals(0, address.AdditionalInfos.Count);
		}
	}
}
