using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.Customs.TR.NCTS.GUI;
using Enterprise.Customs.TR.NCTS.Module;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Module.Testing
{
	[TestedType(typeof(TRNctsMovementController))]
	public class TRNctsMovementControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.NctsMovementController;

		protected override string CountryCode => Core.Constants.CountryCodes.Turkey;

		public void TestGetForm()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			header.BH_ApplicationCode = "NCT";
			using (var form = ((ZControllerInternals)new TRNctsMovementController()).GetForm(header))
			{
				AssertType<TRNctsMovementForm>(form);
			}

			header.BH_ApplicationCode = "NC5";
			using (var form = ((ZControllerInternals)new TRNctsMovementController()).GetForm(header))
			{
				AssertType<Phase5DepartureMovementForm>(form);
			}
		}

		public void TestNctsMovementFormPhase4()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = "NCT";
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			AssertNctsMovementFormType<TRNctsMovementForm>(nctsHeader);
		}

		public void TestNctsMovementFormPhase5Departure()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = "NC5";
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			AssertNctsMovementFormType<Phase5DepartureMovementForm>(nctsHeader);
		}

		void AssertNctsMovementFormType<TForm>(NctsHeader nctsHeader) where TForm : ZForm
		{
			var nctsMovementController = (ZControllerInternals)Controller;
			using (var form = nctsMovementController.GetForm(nctsHeader))
			{
				AssertType<TForm>("NCTS Form Type", form);
			}
		}
	}
}
