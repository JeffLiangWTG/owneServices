using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class IEnumerableExtensionsTest : TestCaseWithFactory
	{
		public void TestGetSCAC()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "OC1A";
			carrier.UI_ModeOfTransportation = "10";
			var carrier2 = Factory.New<USCarrierCombined>();
			carrier2.UI_Code = "OC2A";
			carrier2.UI_ModeOfTransportation = "10";
			var carrier3 = Factory.New<USCarrierCombined>();
			carrier3.UI_Code = "OC1B";
			carrier3.UI_ModeOfTransportation = "31";
			var carrier4 = Factory.New<USCarrierCombined>();
			carrier4.UI_Code = "OC3B";
			carrier4.UI_ModeOfTransportation = "31";
			var carrier5 = Factory.New<USCarrierCombined>();
			carrier5.UI_Code = "OC4B";
			carrier5.UI_ModeOfTransportation = "31";
			var carrier6 = Factory.New<USCarrierCombined>();
			carrier6.UI_Code = "OC4A";
			carrier6.UI_ModeOfTransportation = "10";
			var carrier7 = Factory.New<USCarrierCombined>();
			carrier7.UI_Code = "OC1C";
			carrier7.UI_ModeOfTransportation = "10";

			var org1 = Factory.New<OrgHeader>();
			var org1CusCode0 = org1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CarrierPrefixCode, "OC1C", Core.Constants.CountryCodes.UnitedStates);
			var org1CusCode1 = org1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "OC1A", Core.Constants.CountryCodes.UnitedStates);
			var org1CusCode2 = org1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.TruckCarrierCode, "OC1B", Core.Constants.CountryCodes.UnitedStates);
			var org2 = Factory.New<OrgHeader>();
			var org2CusCode1 = org2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "OC2A", Core.Constants.CountryCodes.UnitedStates);
			var org3 = Factory.New<OrgHeader>();
			var org3CusCode2 = org3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.TruckCarrierCode, "OC3B", Core.Constants.CountryCodes.UnitedStates);
			var org4 = Factory.New<OrgHeader>();
			var org4CusCode1 = org4.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "OC4A", Core.Constants.CountryCodes.Canada);
			var org4CusCode2 = org4.CustomsCodes.AddNew(OrgCusCode.CodeTypes.TruckCarrierCode, "OC4B", Core.Constants.CountryCodes.Canada);

			AssertEquals("OC1C", org1.GetSCAC(TransportTypeList.Codes.Sea));
			AssertEquals("OC1B", org1.GetSCAC(TransportTypeList.Codes.Truck));

			AssertEquals("OC2A", org2.GetSCAC(TransportTypeList.Codes.Sea));
			AssertEquals(ZString.Empty, org2.GetSCAC(TransportTypeList.Codes.Truck));
			AssertEquals(ZString.Empty, org3.GetSCAC(TransportTypeList.Codes.Sea));
			AssertEquals("OC3B", org3.GetSCAC(TransportTypeList.Codes.Truck));
			AssertEquals(ZString.Empty, org4.GetSCAC(TransportTypeList.Codes.Sea));
			AssertEquals(ZString.Empty, org4.GetSCAC(TransportTypeList.Codes.Truck));

			AssertContainsExactElementsInAnyOrder(new List<ZString> { "OC1A", "OC1C" }, new List<OrgHeader> { org1 }.GetValidSCACs(TransportTypeList.Codes.Sea));
		}

		public void TestToGroupsOf()
		{
			var aList = new List<ZString>();
			for (int i = 0; i < 24; i++)
			{
				aList.Add("aString");
			}
			AssertEquals("5 groups", 5, aList.ToGroupsOf(5).Count());
			aList.Add("25th string");
			AssertEquals("5 groups", 5, aList.ToGroupsOf(5).Count());
			aList.Add("26th string");
			AssertEquals("6 groups", 6, aList.ToGroupsOf(5).Count());
		}
	}
}
