using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	class AddressListOverriderTest : TestCaseWithFactory
	{
		public void TestShowCBPNoInAddressList()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "DUMMY TEST COMPANY";
			ZAddressList list = new ZAddressList();
			list.AddAddress(org.MainAddress.PK, "Testing Purpose", "Hello World", new AddressCapabilityItem() { Capability = "CBN", IsDefault = false });

			ZAddressList newList = AddressListOverrider.ShowCBPNoInAddressList(Factory, list);
			AssertZAddressList(newList, org.MainAddress.PK, "CBN", "Testing Purpose", "Hello World", false);
			org.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "97-8888-12345");

			newList = AddressListOverrider.ShowCBPNoInAddressList(Factory, list);
			AssertZAddressList(newList, org.MainAddress.PK, "CBN", "97-8888-12345 (Testing Purpose)", "Hello World", false);
		}

		public void TestShowManufacturerIDInAddressList()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "DUMMY TEST COMP";
			ZAddressList list = new ZAddressList();
			list.AddAddress(org.MainAddress.PK, "Testing Purpose", "Hello World", new AddressCapabilityItem() { Capability = "PST", IsDefault = false });

			ZAddressList newList = AddressListOverrider.ShowManufacturerIDInAddressList(Factory, list);
			AssertZAddressList(newList, org.MainAddress.PK, "PST", "Testing Purpose", "Hello World", false);
			org.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "MA121223KD");

			newList = AddressListOverrider.ShowManufacturerIDInAddressList(Factory, list);
			AssertZAddressList(newList, org.MainAddress.PK, "PST", "MA121223KD (Testing Purpose)", "Hello World", false);
		}

		public void TestShowFDAEstablishmentIdentifierInAddressList()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "DUMMY TEST COMP";
			var list = new ZAddressList();
			list.AddAddress(org.MainAddress.PK, "Testing Purpose", "Hello World", new AddressCapabilityItem() { Capability = "PST", IsDefault = false });

			var newList = AddressListOverrider.ShowFDAEstablishmentIdentifierInAddressList(Factory, list);
			AssertZAddressList(newList, org.MainAddress.PK, "PST", "Testing Purpose", "Hello World", false);
			org.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "MA121223KD");

			newList = AddressListOverrider.ShowFDAEstablishmentIdentifierInAddressList(Factory, list);
			AssertZAddressList(newList, org.MainAddress.PK, "PST", "MA121223KD (Testing Purpose)", "Hello World", false);
		}

		static void AssertZAddressList(ZAddressList list, ZGuid addressPK, ZString addressType, ZString usageComment, ZString addressDescription, ZBool isDefault)
		{
			AssertEquals(1, list.Count);
			ZAddressItem addressItem = (ZAddressItem)((IList)list.List)[0];
			AssertEquals("AddressPK", addressPK, addressItem.PK);
			AssertEquals("UsageComment", usageComment, addressItem.UsageComment);
			AssertEquals("AddressDescription", addressDescription, addressItem.AddressDescription);

			AssertEquals(1, addressItem.Capabilities.Length);
			AssertEquals("AddressType", addressType, addressItem.Capabilities[0].Capability);
			AssertEquals("IsDefault", isDefault, addressItem.Capabilities[0].IsDefault);
		}

		public static void AssertZAddressShowManufacturerIDInAddressList(TestCase testCase, ZAddress zAddress)
		{
			BusinessObjectFactory factory = zAddress.Factory;
			OrgHeader org = factory.New<OrgHeader>();
			org.OH_FullName = "DUMMY TEST COMP";
			OrgAddress mainAddress = org.MainAddress;
			mainAddress.OA_Address1 = "Address 1";
			mainAddress.OA_Code = "Testing Purpose";
			mainAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			mainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "MA121223KD");
			zAddress.OrgPK = org.PK;

			AssertZAddressList(zAddress.OrgAddress_List, org.MainAddress.PK, OrgAddressType.Office.Code, "MA121223KD (Testing Purpose)", ((IOrgAddress)mainAddress).AddressDetailedOnSingleLine + " (OFC)", true);
		}
	}
}
