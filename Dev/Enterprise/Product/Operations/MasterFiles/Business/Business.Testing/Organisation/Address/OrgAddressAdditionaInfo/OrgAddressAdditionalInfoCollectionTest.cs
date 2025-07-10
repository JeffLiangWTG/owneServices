using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgAddressAdditionalInfoCollection))]
	sealed class OrgAddressAdditionalInfoCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgAddressAdditionalInfoCollection>
	{
		public void TestAddressWithValidAdditionalInfoCollection()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Code = "Test Address";
			address.OA_OH = header.PK;

			var address2 = Factory.NewWithValidTestData<OrgAddress>();
			address2.OA_Code = "Test Address 2";
			address2.OA_OH = header.PK;

			var addressInfo1 = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			addressInfo1.OAI_AdditionalInfo = "Test Additional Info 1";
			addressInfo1.OAI_OA_Address = address.PK;

			var addressInfo2 = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			addressInfo2.OAI_AdditionalInfo = "Test Additional Info 2";
			addressInfo2.OAI_OA_Address = address.PK;

			var addressInfo3 = Factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			addressInfo3.OAI_AdditionalInfo = "Test Additional Info 3";
			addressInfo3.OAI_OA_Address = address2.PK;

			Factory.Save();

			var collection = new OrgAddressAdditionalInfoCollection(address);

			AssertEquals(address.PK, addressInfo1.OAI_OA_Address);
			AssertEquals("Test Additional Info 1", addressInfo1.OAI_AdditionalInfo);
			AssertEquals(address.PK, addressInfo2.OAI_OA_Address);
			AssertEquals("Test Additional Info 2", addressInfo2.OAI_AdditionalInfo);
			AssertContainsExactElementsInAnyOrder(new[] { addressInfo1, addressInfo2 }, collection);
		}

		public void TestGetAsCodeDescriptionPair()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();

			AssertEquals("Precondition : AdditionalAddressInfoList should be CodeDescriptionPairList", typeof(CodeDescriptionPairList), address.AdditionalAddressInfoList.GetType());
			AssertEquals("Precondition : AdditionalAddressInfoList should be zero", 0, address.AdditionalAddressInfoList.Count);

			var additionalInfo = address.AdditionalInfos.AddNew();
			additionalInfo.OAI_AdditionalInfo = "Additional Info";
			additionalInfo.OAI_IsPrimary = true;

			Factory.Save();

			AssertEquals(address.PK, additionalInfo.OAI_OA_Address);
			AssertEquals(1, address.AdditionalAddressInfoList.Count);
		}
	}
}
