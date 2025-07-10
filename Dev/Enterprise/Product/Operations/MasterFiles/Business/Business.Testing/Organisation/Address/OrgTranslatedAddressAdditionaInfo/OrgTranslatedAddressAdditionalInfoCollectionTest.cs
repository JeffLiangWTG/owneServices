using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgTranslatedAddressAdditionalInfoCollection))]
	public class OrgTranslatedAddressAdditionalInfoCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgTranslatedAddressAdditionalInfoCollection>
	{
		protected override OrgTranslatedAddressAdditionalInfoCollection GetCollectionToTest()
		{
			return new OrgTranslatedAddressAdditionalInfoCollection(Factory.New<OrgAddressAdditionalInfo>());
		}

		public void TestCollectionLoad()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Code = "Test Address";
			address.OA_OH = header.PK;

			var addressInfo1 = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			addressInfo1.OAI_AdditionalInfo = "Test Additional Info 1";
			addressInfo1.OAI_OA_Address = address.PK;

			var addressInfo2 = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			addressInfo2.OAI_AdditionalInfo = "Test Additional Info 2";
			addressInfo2.OAI_OA_Address = address.PK;

			var translateAddressInfo1 = Factory.NewWithValidTestData<OrgTranslatedAddressAdditionalInfo>();
			translateAddressInfo1.OTI_AdditionalInfo = "Test Translated Additional Info 1";
			translateAddressInfo1.OTI_OAI = addressInfo1.PK;
			translateAddressInfo1.OTI_Language = Core.SharedConstants.Languages.English;

			var translateAddressInfo2 = Factory.NewWithValidTestData<OrgTranslatedAddressAdditionalInfo>();
			translateAddressInfo2.OTI_AdditionalInfo = "Test Translated Additional Info 2";
			translateAddressInfo2.OTI_OAI = addressInfo1.PK;
			translateAddressInfo1.OTI_Language = Core.SharedConstants.Languages.ChineseSimplified;

			var translateAddressInfo3 = Factory.NewWithValidTestData<OrgTranslatedAddressAdditionalInfo>();
			translateAddressInfo3.OTI_AdditionalInfo = "Test Translated Additional Info 3";
			translateAddressInfo3.OTI_OAI = addressInfo2.PK;

			Factory.Save();

			var collection = new OrgTranslatedAddressAdditionalInfoCollection(addressInfo1);

			AssertContainsExactElementsInAnyOrder(new[] { translateAddressInfo1, translateAddressInfo2 }, collection);
			AssertCollectionNotContains(translateAddressInfo3, collection);
		}
	}
}
