using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class OrgAddressDeciderTest : TestCaseWithFactory
	{
		public void TestGetAddressWhenMultipleAddressesForTheSameTypeWhenDefaultSet()
		{
			OrgHeader party = Factory.New<OrgHeader>();
			AddAddressToParty(party, OrgConstants.AddressType.Delivery, "1", false);
			AddAddressToParty(party, OrgConstants.AddressType.Pickup, "2", true);
			AddAddressToParty(party, OrgConstants.AddressType.Pickup, "3", false);

			OrgAddressDecider addressDecider = new OrgAddressDecider(party, CargoAddressType.Pickup);
			AssertEquals("Pickup 2 should be decided", "2", addressDecider.Address1);
		}

		public void TestDeliveryAddress()
		{
			var orgHeader = GetOrgWith_DVL_PAD_PIC();
			var addressDecider = new OrgAddressDecider(orgHeader, CargoAddressType.Delivery);

			AssertEquals("Organisation", orgHeader.PK, addressDecider.Organisation.PK);
			AssertEquals("Company Name", "OrgWith_DVL_PAD_PIC", addressDecider.CompanyName);
			Assert("Address 1", addressDecider.Address1.StartsWith(OrgConstants.AddressType.Delivery));
			Assert("Address 2", addressDecider.Address2.StartsWith(OrgConstants.AddressType.Delivery));
			Assert("Address 3", addressDecider.Address3.StartsWith(OrgConstants.AddressType.Delivery));
			Assert("City", addressDecider.City.StartsWith(OrgConstants.AddressType.Delivery));
			Assert("State", addressDecider.State.StartsWith(OrgConstants.AddressType.Delivery));
			Assert("PostCode", addressDecider.PostCode.StartsWith(OrgConstants.AddressType.Delivery));
			Assert("Phone", addressDecider.Phone.StartsWith(OrgConstants.AddressType.Delivery));
			Assert("Fax", addressDecider.Fax.StartsWith(OrgConstants.AddressType.Delivery));

			orgHeader = GetOrgWith_DVL_PIC();
			addressDecider = new OrgAddressDecider(orgHeader, CargoAddressType.Delivery);
			AssertEquals("Organisation", orgHeader.PK, addressDecider.Organisation.PK);
			AssertEquals("Company Name", "OrgWith_DVL_PIC", addressDecider.CompanyName);
			Assert("Address 1", addressDecider.Address1.StartsWith(OrgConstants.AddressType.Delivery));
			Assert("Address 2", addressDecider.Address2.StartsWith(OrgConstants.AddressType.Delivery));
			Assert("Address 3", addressDecider.Address3.StartsWith(OrgConstants.AddressType.Delivery));
			Assert("City", addressDecider.City.StartsWith(OrgConstants.AddressType.Delivery));
			Assert("State", addressDecider.State.StartsWith(OrgConstants.AddressType.Delivery));
			Assert("PostCode", addressDecider.PostCode.StartsWith(OrgConstants.AddressType.Delivery));
			Assert("Phone", addressDecider.Phone.StartsWith(OrgConstants.AddressType.Delivery));
			Assert("Fax", addressDecider.Fax.StartsWith(OrgConstants.AddressType.Delivery));

			orgHeader = GetOrgWith_PAD();
			addressDecider = new OrgAddressDecider(orgHeader, CargoAddressType.Delivery);
			AssertEquals("Organisation", orgHeader.PK, addressDecider.Organisation.PK);
			AssertEquals("Company Name", "OrgWith_PAD", addressDecider.CompanyName);
			Assert("Address 1", addressDecider.Address1.StartsWith(OrgConstants.AddressType.PickupAndDelivery));
			Assert("Address 2", addressDecider.Address2.StartsWith(OrgConstants.AddressType.PickupAndDelivery));
			Assert("Address 3", addressDecider.Address3.StartsWith(OrgConstants.AddressType.PickupAndDelivery));
			Assert("City", addressDecider.City.StartsWith(OrgConstants.AddressType.PickupAndDelivery));
			Assert("State", addressDecider.State.StartsWith(OrgConstants.AddressType.PickupAndDelivery));
			Assert("PostCode", addressDecider.PostCode.StartsWith(OrgConstants.AddressType.PickupAndDelivery));
			Assert("Phone", addressDecider.Phone.StartsWith(OrgConstants.AddressType.PickupAndDelivery));
			Assert("Fax", addressDecider.Fax.StartsWith(OrgConstants.AddressType.PickupAndDelivery));

			orgHeader = GetOrgWithoutAdditionalAddresses();
			addressDecider = new OrgAddressDecider(orgHeader, CargoAddressType.Delivery);
			AssertEquals("Organisation", orgHeader.PK, addressDecider.Organisation.PK);
			AssertEquals("Company Name", "OrgWithoutAdditionalAddresses", addressDecider.CompanyName);
			Assert("Address 1", addressDecider.Address1.StartsWith("OH"));
			Assert("Address 2", addressDecider.Address2.StartsWith("OH"));
			Assert("Address 3", addressDecider.Address3.StartsWith("OH"));
			Assert("City", addressDecider.City.StartsWith("OH"));
			Assert("State", addressDecider.State.StartsWith("OH"));
			Assert("PostCode", addressDecider.PostCode.StartsWith("OH"));
			Assert("Phone", addressDecider.Phone.StartsWith("OH"));
			Assert("Fax", addressDecider.Fax.StartsWith("OH"));
		}

		public void TestPickupAddress()
		{
			OrgAddressDecider addressDecider = new OrgAddressDecider(GetOrgWith_DVL_PAD_PIC(), CargoAddressType.Pickup);
			AssertEquals("Company Name", "OrgWith_DVL_PAD_PIC", addressDecider.CompanyName);
			Assert("Address 1", addressDecider.Address1.StartsWith(OrgConstants.AddressType.Pickup));
			Assert("Address 2", addressDecider.Address2.StartsWith(OrgConstants.AddressType.Pickup));
			Assert("Address 3", addressDecider.Address3.StartsWith(OrgConstants.AddressType.Pickup));
			Assert("City", addressDecider.City.StartsWith(OrgConstants.AddressType.Pickup));
			Assert("State", addressDecider.State.StartsWith(OrgConstants.AddressType.Pickup));
			Assert("PostCode", addressDecider.PostCode.StartsWith(OrgConstants.AddressType.Pickup));
			Assert("Phone", addressDecider.Phone.StartsWith(OrgConstants.AddressType.Pickup));
			Assert("Fax", addressDecider.Fax.StartsWith(OrgConstants.AddressType.Pickup));

			addressDecider = new OrgAddressDecider(GetOrgWith_DVL_PIC(), CargoAddressType.Pickup);
			AssertEquals("Company Name", "OrgWith_DVL_PIC", addressDecider.CompanyName);
			Assert("Address 1", addressDecider.Address1.StartsWith(OrgConstants.AddressType.Pickup));
			Assert("Address 2", addressDecider.Address2.StartsWith(OrgConstants.AddressType.Pickup));
			Assert("Address 3", addressDecider.Address3.StartsWith(OrgConstants.AddressType.Pickup));
			Assert("City", addressDecider.City.StartsWith(OrgConstants.AddressType.Pickup));
			Assert("State", addressDecider.State.StartsWith(OrgConstants.AddressType.Pickup));
			Assert("PostCode", addressDecider.PostCode.StartsWith(OrgConstants.AddressType.Pickup));
			Assert("Phone", addressDecider.Phone.StartsWith(OrgConstants.AddressType.Pickup));
			Assert("Fax", addressDecider.Fax.StartsWith(OrgConstants.AddressType.Pickup));

			addressDecider = new OrgAddressDecider(GetOrgWith_PAD(), CargoAddressType.Pickup);
			AssertEquals("Company Name", "OrgWith_PAD", addressDecider.CompanyName);
			Assert("Address 1", addressDecider.Address1.StartsWith(OrgConstants.AddressType.PickupAndDelivery));
			Assert("Address 2", addressDecider.Address2.StartsWith(OrgConstants.AddressType.PickupAndDelivery));
			Assert("Address 3", addressDecider.Address3.StartsWith(OrgConstants.AddressType.PickupAndDelivery));
			Assert("City", addressDecider.City.StartsWith(OrgConstants.AddressType.PickupAndDelivery));
			Assert("State", addressDecider.State.StartsWith(OrgConstants.AddressType.PickupAndDelivery));
			Assert("PostCode", addressDecider.PostCode.StartsWith(OrgConstants.AddressType.PickupAndDelivery));
			Assert("Phone", addressDecider.Phone.StartsWith(OrgConstants.AddressType.PickupAndDelivery));
			Assert("Fax", addressDecider.Fax.StartsWith(OrgConstants.AddressType.PickupAndDelivery));

			addressDecider = new OrgAddressDecider(GetOrgWithoutAdditionalAddresses(), CargoAddressType.Pickup);
			AssertEquals("Company Name", "OrgWithoutAdditionalAddresses", addressDecider.CompanyName);
			Assert("Address 1", addressDecider.Address1.StartsWith("OH"));
			Assert("Address 2", addressDecider.Address2.StartsWith("OH"));
			Assert("Address 3", addressDecider.Address3.StartsWith("OH"));
			Assert("City", addressDecider.City.StartsWith("OH"));
			Assert("State", addressDecider.State.StartsWith("OH"));
			Assert("PostCode", addressDecider.PostCode.StartsWith("OH"));
			Assert("Phone", addressDecider.Phone.StartsWith("OH"));
			Assert("Fax", addressDecider.Fax.StartsWith("OH"));
		}

		public void TestNotifyAddress()
		{
			OrgAddressDecider addressDecider = new OrgAddressDecider(GetOrgWith_DVL_PAD_PIC(), CargoAddressType.Notify);
			AssertEquals("Company Name", "OrgWith_DVL_PAD_PIC", addressDecider.CompanyName);
			Assert("Address 1", addressDecider.Address1.StartsWith("OH"));
			Assert("Address 2", addressDecider.Address2.StartsWith("OH"));
			Assert("Address 3", addressDecider.Address3.StartsWith("OH"));
			Assert("City", addressDecider.City.StartsWith("OH"));
			Assert("State", addressDecider.State.StartsWith("OH"));
			Assert("PostCode", addressDecider.PostCode.StartsWith("OH"));
			Assert("Phone", addressDecider.Phone.StartsWith("OH"));
			Assert("Fax", addressDecider.Fax.StartsWith("OH"));

			addressDecider = new OrgAddressDecider(GetOrgWith_DVL_PIC(), CargoAddressType.Notify);
			AssertEquals("Company Name", "OrgWith_DVL_PIC", addressDecider.CompanyName);
			Assert("Address 1", addressDecider.Address1.StartsWith("OH"));
			Assert("Address 2", addressDecider.Address2.StartsWith("OH"));
			Assert("Address 3", addressDecider.Address3.StartsWith("OH"));
			Assert("City", addressDecider.City.StartsWith("OH"));
			Assert("State", addressDecider.State.StartsWith("OH"));
			Assert("PostCode", addressDecider.PostCode.StartsWith("OH"));
			Assert("Phone", addressDecider.Phone.StartsWith("OH"));
			Assert("Fax", addressDecider.Fax.StartsWith("OH"));

			addressDecider = new OrgAddressDecider(GetOrgWith_PAD(), CargoAddressType.Notify);
			AssertEquals("Company Name", "OrgWith_PAD", addressDecider.CompanyName);
			Assert("Address 1", addressDecider.Address1.StartsWith("OH"));
			Assert("Address 2", addressDecider.Address2.StartsWith("OH"));
			Assert("Address 3", addressDecider.Address3.StartsWith("OH"));
			Assert("City", addressDecider.City.StartsWith("OH"));
			Assert("State", addressDecider.State.StartsWith("OH"));
			Assert("PostCode", addressDecider.PostCode.StartsWith("OH"));
			Assert("Phone", addressDecider.Phone.StartsWith("OH"));
			Assert("Fax", addressDecider.Fax.StartsWith("OH"));

			addressDecider = new OrgAddressDecider(GetOrgWithoutAdditionalAddresses(), CargoAddressType.Notify);
			AssertEquals("Company Name", "OrgWithoutAdditionalAddresses", addressDecider.CompanyName);
			Assert("Address 1", addressDecider.Address1.StartsWith("OH"));
			Assert("Address 2", addressDecider.Address2.StartsWith("OH"));
			Assert("Address 3", addressDecider.Address3.StartsWith("OH"));
			Assert("City", addressDecider.City.StartsWith("OH"));
			Assert("State", addressDecider.State.StartsWith("OH"));
			Assert("PostCode", addressDecider.PostCode.StartsWith("OH"));
			Assert("Phone", addressDecider.Phone.StartsWith("OH"));
			Assert("Fax", addressDecider.Fax.StartsWith("OH"));
		}

		public void TestMaximumLengthIsNotExceeded()
		{
			var testOrgHeader = GetOrgWithoutAdditionalAddresses();
			var mainAddress = testOrgHeader.MainAddress;
			mainAddress.OA_Address1 = ZString.Empty.PadRight(OrgAddress.Schema.OA_Address1MaxLength, 'A');
			mainAddress.OA_Address2 = ZString.Empty.PadRight(OrgAddress.Schema.OA_Address2MaxLength, 'D');
			mainAddress.OA_City = ZString.Empty.PadRight(OrgAddress.Schema.OA_CityMaxLength, 'C');
			mainAddress.OA_State = ZString.Empty.PadRight(OrgAddress.Schema.OA_StateMaxLength, 'S');
			mainAddress.OA_PostCode = ZString.Empty.PadRight(OrgAddress.Schema.OA_PostCodeMaxLength, '9');
			mainAddress.OA_Phone = ZString.Empty.PadRight(OrgAddress.Schema.OA_PhoneMaxLength, '1');
			mainAddress.OA_Fax = ZString.Empty.PadRight(OrgAddress.Schema.OA_FaxMaxLength, '2');

			var maximumLength = 35;
			var address3MaximumLength = 25;
			var postcodeMaxLength = 9;
			var stateMaxLength = 25;
			var phoneFaxMaxLength = 20;

			var decider = new OrgAddressDecider(testOrgHeader, CargoAddressType.Delivery);
			AssertEquals("Address 1", maximumLength, decider.Address1.Length);
			AssertEquals("Address 2", maximumLength, decider.Address2.Length);
			AssertEquals("Address 3", address3MaximumLength, decider.Address3.Length);
			AssertEquals("City", maximumLength, decider.City.Length);
			AssertEquals("State", stateMaxLength, decider.State.Length);
			AssertEquals("PostCode", postcodeMaxLength, decider.PostCode.Length);
			AssertEquals(phoneFaxMaxLength, decider.Phone.Length);
			AssertEquals(phoneFaxMaxLength, decider.Fax.Length);
		}

		public void TestPortAndCountryCode()
		{
			OrgHeader testOrgHeader = GetOrgWithoutAdditionalAddresses();
			testOrgHeader.MainAddress.OA_RL_NKRelatedPortCode = ZString.Empty.PadRight(5, 'Z');

			OrgAddressDecider decider = new OrgAddressDecider(testOrgHeader, CargoAddressType.Delivery);
			AssertEquals("RelatedPortCode", "ZZZZZ", decider.RelatedPortCode);
			AssertEquals("CountryCode", "ZZ", decider.CountryCode);
		}

		public void TestDefaultAddressIsSelectedOverNonDefaultAddresses()
		{
			OrgHeader testOrg = GetOrgWithoutAdditionalAddresses();
			OrgAddress address1 = GetNewOrgAddress(OrgConstants.AddressType.PickupAndDelivery);
			OrgAddress address2 = GetNewOrgAddress("PDD");
			testOrg.Addresses.Add(address1);
			testOrg.Addresses.Add(address2);

			address2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);

			address1.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.PickupAndDelivery);
			OrgAddressDecider addressDecider = new OrgAddressDecider(testOrg, CargoAddressType.Delivery);

			Assert("Address 1", addressDecider.Address1.StartsWith(OrgConstants.AddressType.PickupAndDelivery));
			Assert("Address 2", addressDecider.Address2.StartsWith(OrgConstants.AddressType.PickupAndDelivery));
			Assert("Address 3", addressDecider.Address3.StartsWith(OrgConstants.AddressType.PickupAndDelivery));
			Assert("City", addressDecider.City.StartsWith(OrgConstants.AddressType.PickupAndDelivery));
			Assert("State", addressDecider.State.StartsWith(OrgConstants.AddressType.PickupAndDelivery));
			Assert("PostCode", addressDecider.PostCode.StartsWith(OrgConstants.AddressType.PickupAndDelivery));
			Assert("Phone", addressDecider.Phone.StartsWith(OrgConstants.AddressType.PickupAndDelivery));
			Assert("Fax", addressDecider.Fax.StartsWith(OrgConstants.AddressType.PickupAndDelivery));
			address1.AddressCapability.SetIsNotMainAddress(OrgConstants.AddressType.PickupAndDelivery);
			address2.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.PickupAndDelivery);
			addressDecider = new OrgAddressDecider(testOrg, CargoAddressType.Delivery);
			Assert("Address 1", addressDecider.Address1.StartsWith("PDD"));
			Assert("Address 2", addressDecider.Address2.StartsWith("PDD"));
			Assert("Address 3", addressDecider.Address3.StartsWith("PDD"));
			Assert("City", addressDecider.City.StartsWith("PDD"));
			Assert("State", addressDecider.State.StartsWith("PDD"));
			Assert("PostCode", addressDecider.PostCode.StartsWith("PDD"));
			Assert("Phone", addressDecider.Phone.StartsWith("PDD"));
			Assert("Fax", addressDecider.Fax.StartsWith("PDD"));
		}

		public void TestWhiteSpacesAtEndTrimmed()
		{
			OrgHeader party = Factory.New<OrgHeader>();
			AddAddressToParty(party, OrgConstants.AddressType.Pickup, "2", true);
			party.Addresses[1].OA_State = "";
			OrgAddressDecider addressDecider = new OrgAddressDecider(party, CargoAddressType.Pickup);
			AssertEquals("Address 3 should not have a white space at the end", addressDecider.Address3.TrimEnd(), addressDecider.Address3);
		}

		public void TestAddress3MaxLength()
		{
			OrgHeader testOrg = GetOrgWithoutAdditionalAddresses();
			OrgAddress address = GetNewOrgAddress(OrgConstants.AddressType.PickupAndDelivery);
			address.OA_City = "012345678910111";
			address.OA_State = "0123456789";
			address.OA_PostCode = "41527415";
			OrgAddressDecider addressDecider = new OrgAddressDecider(testOrg, CargoAddressType.All);
			Assert("Address Decider Address 3 Max Length - Must be less then 25", addressDecider.Address3.Length <= 25);
		}

		#region Implementation

		void AddAddressToParty(OrgHeader party, string cargoAddressType, string street, bool isDefault)
		{
			OrgAddress address = party.Addresses.AddNew();
			address.AddressCapability.SetCapabilityEnabled(cargoAddressType);
			if (isDefault)
			{
				address.AddressCapability.SetIsMainAddress(cargoAddressType);
			}
			address.OA_Address1 = street;
		}

		OrgHeader GetOrgWith_DVL_PAD_PIC()
		{
			OrgHeader result = GetOrgWithoutAdditionalAddresses();
			result.OH_FullName = "OrgWith_DVL_PAD_PIC";
			result.Addresses.Add(GetNewOrgAddress(OrgConstants.AddressType.Delivery));
			result.Addresses.Add(GetNewOrgAddress(OrgConstants.AddressType.PickupAndDelivery));
			result.Addresses.Add(GetNewOrgAddress(OrgConstants.AddressType.Pickup));
			return result;
		}

		OrgHeader GetOrgWith_DVL_PIC()
		{
			OrgHeader result = GetOrgWithoutAdditionalAddresses();
			result.OH_FullName = "OrgWith_DVL_PIC";
			result.Addresses.Add(GetNewOrgAddress(OrgConstants.AddressType.Delivery));
			result.Addresses.Add(GetNewOrgAddress(OrgConstants.AddressType.Pickup));
			return result;
		}

		OrgHeader GetOrgWith_PAD()
		{
			OrgHeader result = GetOrgWithoutAdditionalAddresses();
			result.OH_FullName = "OrgWith_PAD";
			result.Addresses.Add(GetNewOrgAddress(OrgConstants.AddressType.PickupAndDelivery));
			return result;
		}

		OrgHeader GetOrgWithoutAdditionalAddresses()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = "OrgWithoutAdditionalAddresses";
			result.MainAddress.OA_Address1 = "OH Address1";
			result.MainAddress.OA_Address2 = "OH Address2";
			result.MainAddress.OA_City = "OH City";
			result.MainAddress.OA_State = "OH State";
			result.MainAddress.OA_PostCode = "OH Post";
			result.MainAddress.OA_Phone = "OH Phone";
			result.MainAddress.OA_Fax = "OH Fax";
			return result;
		}

		OrgAddress GetNewOrgAddress(string cargoAddressType)
		{
			OrgAddress result = Factory.New<OrgAddress>();
			result.AddressCapability.SetCapabilityEnabled(cargoAddressType);
			result.OA_Address1 = cargoAddressType + " Address1";
			result.OA_Address2 = cargoAddressType + " Address2";
			result.OA_City = cargoAddressType + " City";
			result.OA_State = cargoAddressType + " State";
			result.OA_PostCode = cargoAddressType + " Post";
			result.OA_Phone = cargoAddressType + " Phone";
			result.OA_Fax = cargoAddressType + " Fax";
			return result;
		}

		#endregion

	}
}
