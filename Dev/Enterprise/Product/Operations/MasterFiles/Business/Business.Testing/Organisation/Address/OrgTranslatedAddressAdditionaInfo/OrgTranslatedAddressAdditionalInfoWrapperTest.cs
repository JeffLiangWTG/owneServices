using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgTranslatedAddressAdditionalInfoWrapper))]
	public class OrgTranslatedAddressAdditionalInfoWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var orgTranslatedAddress = Factory.NewWithValidTestData<OrgTranslatedAddress>();
			var orgAddressAdditionalInfo = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			return new OrgTranslatedAddressAdditionalInfoWrapper(orgAddressAdditionalInfo, orgTranslatedAddress);
		}

		public void TestTranslatedAdditionalInfo()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var translatedAddress1 = address.AddNewTranslatedAddress();
			translatedAddress1.OTA_Address1 = "Test Translate Address 1";
			translatedAddress1.OTA_Language = SharedConstants.Languages.ChineseSimplified;

			var translatedAddress2 = address.AddNewTranslatedAddress();
			translatedAddress2.OTA_Address1 = "Test Translate Address 2";
			translatedAddress2.OTA_Language = SharedConstants.Languages.French;

			var translatedAddress3 = address.AddNewTranslatedAddress();
			translatedAddress3.OTA_Address1 = "Test Translate Address 3";
			translatedAddress3.OTA_Language = SharedConstants.Languages.German;

			var additionalInfo = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			var translatedAdditionalInfo1 = Factory.NewWithValidTestData<OrgTranslatedAddressAdditionalInfo>();
			translatedAdditionalInfo1.OTI_OAI = additionalInfo.PK;
			translatedAdditionalInfo1.OTI_Language = SharedConstants.Languages.ChineseSimplified;
			translatedAdditionalInfo1.OTI_AdditionalInfo = "Test Info 1";

			var translatedAdditionalInfo2 = Factory.NewWithValidTestData<OrgTranslatedAddressAdditionalInfo>();
			translatedAdditionalInfo2.OTI_OAI = additionalInfo.PK;
			translatedAdditionalInfo2.OTI_Language = SharedConstants.Languages.French;
			translatedAdditionalInfo2.OTI_AdditionalInfo = "Test Info 2";

			Factory.Save();

			var wrapper1 = new OrgTranslatedAddressAdditionalInfoWrapper(additionalInfo, translatedAddress1);
			var wrapper2 = new OrgTranslatedAddressAdditionalInfoWrapper(additionalInfo, translatedAddress2);
			var wrapper3 = new OrgTranslatedAddressAdditionalInfoWrapper(additionalInfo, translatedAddress3);

			AssertEquals("Test Info 1", wrapper1.TranslatedAdditionalInfo);
			AssertEquals("Test Info 2", wrapper2.TranslatedAdditionalInfo);
			AssertEquals(ZString.Empty, wrapper3.TranslatedAdditionalInfo);

			wrapper1.TranslatedAdditionalInfo = "Test Change 1";
			wrapper2.TranslatedAdditionalInfo = "Test Change 2";
			wrapper3.TranslatedAdditionalInfo = "Test Change 3";

			var translatedAdditionalInfo3 = additionalInfo.TranslatedInfos.FirstOrDefault(u => u.OTI_AdditionalInfo == "Test Change 3");

			AssertNotNull(translatedAdditionalInfo3);
			AssertEquals("Test Change 1", translatedAdditionalInfo1.OTI_AdditionalInfo);
			AssertEquals("Test Change 2", translatedAdditionalInfo2.OTI_AdditionalInfo);

			wrapper1.TranslatedAdditionalInfo = "";

			Assert(translatedAdditionalInfo1.IsDeleted);
		}

		public void TestOrgAddressAdditionalInfoProperties()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var translatedAddress = address.AddNewTranslatedAddress();
			translatedAddress.OTA_Address1 = "Test Translate Address";
			translatedAddress.OTA_Language = SharedConstants.Languages.ChineseSimplified;

			var additionalInfo1 = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			additionalInfo1.OAI_AdditionalInfo = "Test Info 1";
			additionalInfo1.OAI_IsPrimary = true;

			var additionalInfo2 = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			additionalInfo2.OAI_AdditionalInfo = "Test Info 2";
			additionalInfo2.OAI_IsPrimary = false;

			Factory.Save();

			var wrapper1 = new OrgTranslatedAddressAdditionalInfoWrapper(additionalInfo1, translatedAddress);
			var wrapper2 = new OrgTranslatedAddressAdditionalInfoWrapper(additionalInfo2, translatedAddress);

			AssertEquals("Test Info 1", wrapper1.AdditionalInfo);
			AssertEquals("Test Info 2", wrapper2.AdditionalInfo);
			Assert(wrapper1.IsPrimary);
			Assert(!wrapper2.IsPrimary);
		}

		public void TestDeleteTranslateAdditionalInfo()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var additionalInfo = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			additionalInfo.OAI_IsPrimary = true;
			additionalInfo.OAI_AdditionalInfo = "Address Additional Info";
			additionalInfo.OAI_OA_Address = address.PK;

			var translatedAddress = address.AddNewTranslatedAddress();
			translatedAddress.OTA_Address1 = "Test Translate Address 1";
			translatedAddress.OTA_Language = SharedConstants.Languages.ChineseSimplified;

			var translatedAdditionalInfo = Factory.NewWithValidTestData<OrgTranslatedAddressAdditionalInfo>();
			translatedAdditionalInfo.OTI_OAI = additionalInfo.PK;
			translatedAdditionalInfo.OTI_Language = SharedConstants.Languages.ChineseSimplified;
			translatedAdditionalInfo.OTI_AdditionalInfo = "Test Info 1";

			var wrapper = new OrgTranslatedAddressAdditionalInfoWrapper(additionalInfo, translatedAddress);
			wrapper.Delete();

			Assert(!additionalInfo.IsDeleted);
			Assert(translatedAdditionalInfo.IsDeleted);
		}

		public void TestAdditionalInformation_ShouldUpdatePrimaryAddressAdditionalInformation()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;
			var additionalInfo = address.AdditionalInfos.AddNew();
			additionalInfo.OAI_AdditionalInfo = "Main Information";
			additionalInfo.OAI_IsPrimary = false;

			var additionalInfo2 = address.AdditionalInfos.AddNew();
			additionalInfo2.OAI_AdditionalInfo = "Other Information";
			additionalInfo2.OAI_IsPrimary = true;

			AssertEquals("Other Information", address.OA_AdditionalAddressInformation);

			var translatedAddress = address.TranslatedAddresses.AddNew();
			translatedAddress.Address1 = "Translated Address";
			translatedAddress.Language = SharedConstants.Languages.French;

			Factory.Save();

			var wrapperCollection = new OrgTranslatedAddressAdditionalInfoWrapperCollection(translatedAddress);
			wrapperCollection[0].TranslatedAdditionalInfo = "Main Information - FR";
			wrapperCollection[1].TranslatedAdditionalInfo = "Other Information - FR";

			Factory.Save();

			CombineAssertions(() =>
			{
				Assert(!wrapperCollection[0].IsPrimary);
				AssertEquals("Main Information", wrapperCollection[0].AdditionalInfo);
				Assert(wrapperCollection[1].IsPrimary);
				AssertEquals("Other Information", wrapperCollection[1].AdditionalInfo);
				AssertEquals("Other Information - FR", translatedAddress.OTA_AdditionalAddressInformation);
			});
		}
	}
}
