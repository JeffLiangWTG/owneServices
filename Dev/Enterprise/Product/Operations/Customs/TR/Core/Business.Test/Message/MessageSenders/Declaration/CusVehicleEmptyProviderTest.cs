using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class CusVehicleEmptyProviderTest : TestCaseWithFactory
	{
		public void TestCusVehicleMember()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetExportProviderHeader();
				var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
				var lines = declaration.EntryLines.ToArray();
				var cusVehicle = lines[0].CusVehicle.ToArray();

				CombineAssertions("Vehicle Model Case | with Registration no", () =>
				{
					AssertEquals("Export Vehicle Count", 1, cusVehicle.Length);
					AssertEquals("BrandType", ZString.Empty, cusVehicle[0].BrandType);
					AssertEquals("BrandRegistrationNo", ZString.Empty, cusVehicle[0].BrandRegistrationNo);
					AssertEquals("BrandName", ZString.Empty, cusVehicle[0].BrandName);
					AssertEquals("BrandAmount", 0m, cusVehicle[0].BrandAmount);
					AssertEquals("ReferenceNo", ZString.Empty, cusVehicle[0].ReferenceNo);
					AssertEquals("ModelYear", "0000", cusVehicle[0].ModelYear);
					AssertEquals("Model", ZString.Empty, cusVehicle[0].Model);
					AssertEquals("EngineVolume", "0", cusVehicle[0].EngineVolume);
					AssertEquals("NumberOfCylinders", ZInt.Zero, cusVehicle[0].NumberOfCylinders);
					AssertEquals("Color", ZString.Empty, cusVehicle[0].Color);
					AssertEquals("EngineType", ZString.Empty, cusVehicle[0].EngineType);
					AssertEquals("EngineNo", ZString.Empty, cusVehicle[0].EngineNo);
					AssertEquals("EnginePower", "0", cusVehicle[0].EnginePower);
					AssertEquals("GearShift", ZString.Empty, cusVehicle[0].GearShift);
					AssertEquals("IMEINo", ZString.Empty, cusVehicle[0].IMEINo);
				});
			}
		}
	}
}
