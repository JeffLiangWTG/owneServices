using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	class DefaultAddressRelatedDeterminerTest : TestCaseWithFactory
	{
		public void TestGetMIDAddress()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgAddress address1 = org.MainAddress;
			AssertEquals(address1.PK, DefaultAddressRelatedDeterminer.GetMIDAddress(org));
			address1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, ZString.Empty);
			OrgAddress address2 = org.Addresses.AddNew();
			address2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "MID23423423", Factory.Load<RefCountry>(Core.Constants.CountryGuids.Australia));
			OrgAddress address3 = org.Addresses.AddNew();
			address3.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "MID59438434");

			AssertEquals(address3.PK, DefaultAddressRelatedDeterminer.GetMIDAddress(org));
		}

		public void TestGetFEIAddress()
		{
			var org = Factory.New<OrgHeader>();
			var address1 = org.MainAddress;
			AssertEquals(address1.PK, DefaultAddressRelatedDeterminer.GetFEIAddress(org));
			address1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, ZString.Empty);
			var address2 = org.Addresses.AddNew();
			address2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "FEI23423423", Factory.Load<RefCountry>(Core.Constants.CountryGuids.Australia));
			var address3 = org.Addresses.AddNew();
			address3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "FEI59438434");

			AssertEquals(address3.PK, DefaultAddressRelatedDeterminer.GetFEIAddress(org));
		}

		public void TestGetCBPAddress()
		{
			var org = Factory.New<OrgHeader>();
			var address1 = org.MainAddress;
			AssertEquals("Will return Main Address by default", address1.PK, DefaultAddressRelatedDeterminer.GetCBPAddress(org));
			address1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, ZString.Empty);
			var address2 = org.Addresses.AddNew();
			address2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "97-8888-12345", Factory.Load<RefCountry>(Core.Constants.CountryGuids.UnitedKingdom));
			var address3 = org.Addresses.AddNew();
			address3.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "97-8888-12345");

			AssertEquals("Should find Address with CBN code", address3.PK, DefaultAddressRelatedDeterminer.GetCBPAddress(org));
		}

		public void TestGetDUNS_FEIAddress()
		{
			var org = Factory.New<OrgHeader>();
			AssertEquals(org.MainAddress.PK, DefaultAddressRelatedDeterminer.GetDUNS_FEIAddress(org));

			var address1 = org.MainAddress;
			address1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, ZString.Empty);
			var address2 = org.Addresses.AddNew();
			address2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "FEI23423423", Factory.Load<RefCountry>(Core.Constants.CountryGuids.Australia));
			AssertEquals(org.MainAddress.PK, DefaultAddressRelatedDeterminer.GetDUNS_FEIAddress(org));

			address1.CustomsCodes.DeleteAll();
			AssertEquals(address2.PK, DefaultAddressRelatedDeterminer.GetDUNS_FEIAddress(org));

			var address3 = org.Addresses.AddNew();
			address3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "FEI59438434");
			AssertEquals(org.MainAddress.PK, DefaultAddressRelatedDeterminer.GetDUNS_FEIAddress(org));

			address2.CustomsCodes.DeleteAll();
			AssertEquals(address3.PK, DefaultAddressRelatedDeterminer.GetDUNS_FEIAddress(org));
		}

		public void TestGetMainAddress()
		{
			var org = Factory.New<OrgHeader>();
			org.Addresses.AddNew();
			AssertEquals(org.MainAddress.PK, DefaultAddressRelatedDeterminer.GetMainAddress(org));
		}
	}
}
