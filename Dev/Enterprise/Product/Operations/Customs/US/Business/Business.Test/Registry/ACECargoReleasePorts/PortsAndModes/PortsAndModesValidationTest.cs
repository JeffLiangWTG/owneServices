using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	sealed class PortsAndModesValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckProperties()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3901", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var data = new ACECargoReleaseTypePortMapping();
			var ports = data.PortsAndModes.AddNew();
			ports.TransportMode = "!";
			ports.Port = "!";
			AssertHasErrorContaining(ports.TransportModeInfo, "Enter a valid selection.");
			AssertHasErrorContaining(ports.PortInfo, "Enter a valid selection.");
			ports.Port = "3901";
			ports.TransportMode = Enterprise.Customs.US.Business.TransportTypeList.Codes.Sea;
			AssertNoErrorContaining(ports.TransportModeInfo, "Enter a valid selection.");
			AssertNoErrorContaining(ports.PortInfo, "Enter a valid selection.");
		}
	}
}
