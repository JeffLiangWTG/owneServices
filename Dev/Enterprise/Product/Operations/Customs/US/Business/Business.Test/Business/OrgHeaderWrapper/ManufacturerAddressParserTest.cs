using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class ManufacturerAddressParserTest : TestCaseWithFactory
	{
		public void TestGetMIDCodeFromAddressPK()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "TEST ORG DUMMY";
			org.OH_Code = "ORG" + new System.Random().Next(1000000).ToString();
			var mainAddress = org.MainAddress;
			mainAddress.OA_Address1 = "ADDRESS 1";
			var cusCode = mainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "MID234323");
			Factory.Save();

			AssertEquals("MID234323", AddressParser.GetMIDCodeFromAddressPK(mainAddress.PK, Factory));
		}

		public void TestGetAddressPKFromMatchingCode()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "TEST ORG DUMMY";
			org.OH_Code = "ORG" + new System.Random().Next(1000000).ToString();
			OrgAddress mainAddress = org.MainAddress;
			mainAddress.OA_Address1 = "ADDRESS 1";
			OrgCusCode cusCode = mainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "MID234323");
			AssertEquals(ZGuid.Invalid, AddressParser.GetAddressPKFromMatchingMIDCode("MID234323"));
			Factory.Save();
			AssertEquals(mainAddress.PK, AddressParser.GetAddressPKFromMatchingMIDCode("MID234323"));
			AssertEquals(ZGuid.Invalid, AddressParser.GetAddressPKFromMatchingMIDCode("MDS234323"));
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			Factory.Save();
			AssertEquals(ZGuid.Invalid, AddressParser.GetAddressPKFromMatchingMIDCode("MID234323"));
		}
	}
}
