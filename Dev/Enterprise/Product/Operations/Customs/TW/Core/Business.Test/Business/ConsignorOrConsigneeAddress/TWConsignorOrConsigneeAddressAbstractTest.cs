using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestsSubclassesOf(typeof(TWConsignorOrConsigneeAddress))]
	abstract class TWConsignorOrConsigneeAddressAbstractTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var address = GetConsignorOrConsigneeAddress();
			AssertEquals(ZString.Empty, address.E2_GovRegNumType);
		}

		public void TestE2_GovRegNum()
		{
			var address = GetConsignorOrConsigneeAddress();
			address.E2_GovRegNum = "1234567890";
			CombineAssertions(() =>
			{
				AssertEquals("E2_GovRegNum", "1234567890", address.E2_GovRegNum);
				AssertEquals("E2_GovRegNum.ReadOnly", false, address.E2_GovRegNumInfo.ReadOnly);
			});
		}

		public void TestE2_GovRegNumType()
		{
			var address = GetConsignorOrConsigneeAddress();
			address.E2_GovRegNumType = OrgCusCode.CodeTypes.VATCode;
			CombineAssertions(() =>
			{
				AssertEquals("E2_GovRegNumType", OrgCusCode.CodeTypes.VATCode, address.E2_GovRegNumType);
				AssertEquals("E2_GovRegNumType", false, address.E2_GovRegNumTypeInfo.ReadOnly);
			});
		}

		public abstract void TestDefaultValueWhenAddressChanged();

		public abstract void TestDefaultValueWhenGovRegNumTypeChanged();

		protected override BusinessObject GetNewBusinessObject()
		{
			var address = GetConsignorOrConsigneeAddress();
			address.E2_GovRegNum = "1234567890";
			address.E2_GovRegNumType = OrgCusCode.CodeTypes.VATCode;
			return address;
		}

		public override BusinessObject GetNewBusinessObjectSafeSaving()
		{
			var address = GetNewBusinessObject();
			Factory.Save();
			return address;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected abstract TWConsignorOrConsigneeAddress GetConsignorOrConsigneeAddress();

		protected (OrgHeader, OrgAddress) CreateTestOrgForWarehouse()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			var testOrgMainAddress = testOrg.MainAddress;
			testOrgMainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			testOrgMainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "11111", Core.Constants.CountryCodes.Taiwan);
			return (testOrg, testOrgMainAddress);
		}

		protected (OrgHeader, OrgAddress) CreateTestOrgForDocumentaryAddress()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_RL_NKClosestPort = "TWTPE";
			var testOrgAddress = testOrg.Addresses.AddNew();
			testOrgAddress.Address1 = "Address1";
			testOrgAddress.Address2 = "Address2";
			return (testOrg, testOrgAddress);
		}
	}
}
