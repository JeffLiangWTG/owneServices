using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class CusVehicleProviderTest : TestCaseWithFactory
	{
		public void TestCusVehicleMember()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetProviderHeader();
				var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
				var lines = declaration.EntryLines.ToArray();
				var cusVehicle = lines[0].CusVehicle.ToArray();

				AssertEquals("Export Vehicle Count", 4, cusVehicle.Length);

				CombineAssertions("Vehicle Model Case | with Registration no", () =>
				{
					AssertEquals("BrandType", "1", cusVehicle[0].BrandType);
					AssertEquals("BrandRegistrationNo", "232423", cusVehicle[0].BrandRegistrationNo);
					AssertEquals("BrandName", string.Empty, cusVehicle[0].BrandName);
					AssertEquals("BrandAmount", 60000m, cusVehicle[0].BrandAmount);
					AssertEquals("ReferenceNo", "444555232", cusVehicle[0].ReferenceNo);
					AssertEquals("ModelYear", "2021", cusVehicle[0].ModelYear);
					AssertEquals("Model", "XC60", cusVehicle[0].Model);
					AssertEquals("EngineVolume", "2000", cusVehicle[0].EngineVolume);
					AssertEquals("NumberOfCylinders", 4, cusVehicle[0].NumberOfCylinders);
					AssertEquals("Color", "MAVİ", cusVehicle[0].Color);
					AssertEquals("EngineType", "1", cusVehicle[0].EngineType);
					AssertEquals("EngineNo", "12345678901234567", cusVehicle[0].EngineNo);
					AssertEquals("EnginePower", "200", cusVehicle[0].EnginePower);
					AssertEquals("GearShift", "1", cusVehicle[0].GearShift);
					AssertEquals("IMEINo", "IMEI NO", cusVehicle[0].IMEINo);
				});

				CombineAssertions("Vehicle Model Case | NON Registration no", () =>
				{
					AssertEquals("BrandType", "0", cusVehicle[1].BrandType);
					AssertEquals("BrandRegistrationNo", ZString.Empty, cusVehicle[1].BrandRegistrationNo);
					AssertEquals("BrandName", "BRAND", cusVehicle[1].BrandName);
					AssertEquals("BrandAmount", 60000m, cusVehicle[1].BrandAmount);
					AssertEquals("ReferenceNo", "444555233", cusVehicle[1].ReferenceNo);
					AssertEquals("ModelYear", "2021", cusVehicle[1].ModelYear);
					AssertEquals("Model", "XC60", cusVehicle[1].Model);
					AssertEquals("EngineVolume", "2000", cusVehicle[1].EngineVolume);
					AssertEquals("NumberOfCylinders", 4, cusVehicle[1].NumberOfCylinders);
					AssertEquals("Color", "MAVİ", cusVehicle[1].Color);
					AssertEquals("EngineType", "1", cusVehicle[1].EngineType);
					AssertEquals("EngineNo", "12345678901234568", cusVehicle[1].EngineNo);
					AssertEquals("EnginePower", "200", cusVehicle[1].EnginePower);
					AssertEquals("GearShift", "1", cusVehicle[1].GearShift);
					AssertEquals("IMEINo", "IMEI NO", cusVehicle[1].IMEINo);
				});

				headerJobDeclaration = helper.GetExportProviderHeader();
				declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
				lines = declaration.EntryLines.ToArray();
				cusVehicle = lines[0].CusVehicle.ToArray();
				AssertEquals("Export Vehicle Count", 1, cusVehicle.Length);
			}
		}
	}
}
