using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgTranslatedAddressAdditionalInfoWrapperCollection))]
	sealed class OrgTranslatedAddressAdditionalInfoWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<OrgTranslatedAddressAdditionalInfoWrapperCollection>
	{
		protected override OrgTranslatedAddressAdditionalInfoWrapperCollection GetCollectionToTest()
		{
			return new OrgTranslatedAddressAdditionalInfoWrapperCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OrgTranslatedAddressAdditionalInfoWrapper(Factory);
		}

		public void TestCreateTranslatableAdditionalInfos()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var translatedAddress = address.AddNewTranslatedAddress();
			translatedAddress.OTA_Address1 = "Test Translate Address";
			translatedAddress.OTA_Language = SharedConstants.Languages.ChineseSimplified;

			var additionalInfo1 = address.AdditionalInfos.AddNew();
			additionalInfo1.OAI_AdditionalInfo = "Test Info 1";

			var additionalInfo2 = address.AdditionalInfos.AddNew();
			additionalInfo2.OAI_AdditionalInfo = "Test Info 2";

			Factory.Save();

			var wrapperCollection = new OrgTranslatedAddressAdditionalInfoWrapperCollection(translatedAddress);
			AssertNotNull(wrapperCollection.FirstOrDefault(u => ((OrgTranslatedAddressAdditionalInfoWrapper)u).AddressAdditionalInfo.Equals(additionalInfo1)));
			AssertNotNull(wrapperCollection.FirstOrDefault(u => ((OrgTranslatedAddressAdditionalInfoWrapper)u).AddressAdditionalInfo.Equals(additionalInfo2)));
		}

		public void TestRebuild()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var translatedAddress = address.AddNewTranslatedAddress();
			translatedAddress.OTA_Address1 = "Test Translate Address";
			translatedAddress.OTA_Language = SharedConstants.Languages.ChineseSimplified;

			var additionalInfo1 = address.AdditionalInfos.AddNew();
			additionalInfo1.OAI_AdditionalInfo = "Test Info 1";

			var additionalInfo2 = address.AdditionalInfos.AddNew();
			additionalInfo2.OAI_AdditionalInfo = "Test Info 2";

			Factory.Save();

			var wrapperCollection = new OrgTranslatedAddressAdditionalInfoWrapperCollection(translatedAddress);

			AssertEquals(2, wrapperCollection.Count);

			var additionalInfo3 = address.AdditionalInfos.AddNew();
			additionalInfo3.OAI_AdditionalInfo = "Test Info 3";

			Factory.Save();

			AssertEquals(2, wrapperCollection.Count);

			wrapperCollection.ReBuild();

			AssertEquals(3, wrapperCollection.Count);
			AssertNotNull(wrapperCollection.FirstOrDefault(u => ((OrgTranslatedAddressAdditionalInfoWrapper)u).AddressAdditionalInfo.Equals(additionalInfo3)));
		}
	}
}
