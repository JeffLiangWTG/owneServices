using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.SE.NCTS.Module.Testing;

[TestedType(typeof(NctsMovementController))]
sealed class NctsMovementControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
{
	protected override string CountryCode => Core.Constants.CountryCodes.Sweden;

	protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.NctsMovementController;

	protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		Factory.Save();
		return nctsHeader;
	}
}
