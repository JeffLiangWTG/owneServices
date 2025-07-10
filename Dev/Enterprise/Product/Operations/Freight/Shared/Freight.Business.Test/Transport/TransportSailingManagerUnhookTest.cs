using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	sealed class TransportSailingManagerUnhookTest : TestCaseWithFactory
	{
		CommonConsol Consol;
		Transport Transport;
		JobVoyage Voyage;
		JobSailing Sailing;

		protected override void SetUp()
		{
			base.SetUp();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "VESSEL 111";

			Consol = Factory.New<CommonConsol>();
			Consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_RL_NKDischargePort = "USLAX";
			Transport = Consol.Transports.AddNew();
			Voyage = Factory.New<JobVoyage>();
			Voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			Voyage.JV_RV_NKVessel = vessel.RV_FK;
			Voyage.JV_VoyageFlight = "111S";
			Voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSMV";
			Voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			Voyage.GenerateSailings();

			Sailing = Voyage.Sailings[0];
			Transport.JW_IsLinked = true;
			Transport.JW_JX = Sailing.PK;

			Factory.Save();

			RunAdditionalValidations();
			AssertEquals("pre: no error reports", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestValidationUnhookedOnDelete_OnUpdatedByDataRefresh()
		{
			var factory2 = new BusinessObjectFactory();
			var consol2 = factory2.Load<CommonConsol>(Consol.PK);
			consol2.Transports[1].JW_VoyageFlight = "AA111";
			factory2.Save();

			Consol.Transports.RemoveAndDelete(Transport);
			RunAdditionalValidations();
			AssertEquals("no errors after deleting", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestValidationUnhookedOnDelete_TransportMode()
		{
			Transport.JW_TransportMode = "";

			Consol.Transports.RemoveAndDelete(Transport);
			RunAdditionalValidations();
			AssertEquals("no errors after deleting", 0, ErrorReporter.TotalErrorCount);
		}

		void RunAdditionalValidations()
		{
			Sailing.Origin.Validation.ValidateJA_CutOff();
			Sailing.Destination.Validation.ValidateJB_RL_NKPortOfDischarge();
			Sailing.Validation.ValidateJX_DepotCutOff();
		}
	}
}
