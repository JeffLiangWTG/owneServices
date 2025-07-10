using CargoWise.EntityFramework;
using Enterprise.Customs.NL.NCTS.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Module.Testing;

[TestedType(typeof(NctsMovementController))]
sealed class NctsMovementControllerTest : ZControllerBasherTest
{
	protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		Factory.Save();
		return nctsHeader;
	}

	protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.NctsMovementController;

	protected override string CountryCode => Core.Constants.CountryCodes.Netherlands;
}
