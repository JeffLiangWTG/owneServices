using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Module.Testing;

[TestedType(typeof(NctsMovementController))]
sealed class NctsMovementControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
{
	public void TestNctsMovementFormPhase4()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = "NCT";
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

		AssertNctsMovementFormType<NctsMovementForm>(nctsHeader);
	}

	public void TestNctsMovementFormPhase5Departure()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = "NC5";
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

		AssertNctsMovementFormType<GUI.Phase5DepartureMovementForm>(nctsHeader);
	}

	public void TestNctsMovementFormPhase5Arrival()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = "NC5";
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);

		AssertNctsMovementFormType<GUI.Phase5ArrivalMovementForm>(nctsHeader);
	}

	void AssertNctsMovementFormType<TForm>(NctsHeader nctsHeader) where TForm : ZForm
	{
		var nctsMovementController = (ZControllerInternals)Controller;
		using (var form = nctsMovementController.GetForm(nctsHeader))
		{
			AssertType<TForm>("NCTS Form Type", form);
		}
	}

	protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.NctsMovementController;
	protected override string CountryCode => Core.Constants.CountryCodes.Poland;
	public override Type ControllerToBashType => typeof(NctsMovementController);
	protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		Factory.Save();
		return nctsHeader;
	}
}
