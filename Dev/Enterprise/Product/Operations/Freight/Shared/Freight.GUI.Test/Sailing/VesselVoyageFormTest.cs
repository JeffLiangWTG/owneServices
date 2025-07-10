using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(VesselVoyageForm))]
	sealed class VesselVoyageFormTest : ZFormBasherTest
	{
		public void TestCancel()
		{
			var shipment = Factory.New<CommonShipmentWithVoyageFinderParent>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "SGSIN";

			VoyageFinder finder = new VoyageFinder(shipment);

			using (VesselVoyageForm form = new VesselVoyageForm(finder, Constants.TransportModes.Sea))
			{
				form.Show();
				Application.DoEvents();

				var vessel = RefVessel.LookupVesselByName("CONDOR", Factory).First();

				finder.JV_RV_NKVessel = vessel.RV_FK;
				finder.JV_VoyageFlight = "061";

				form.PerformCancel();

				AssertEquals(null, form.LastUsedControllerForTest);
				AssertEquals("Should have closed the form.", false, form.Visible);
				AssertEquals("new voyage", false, form.NewSailingCreated);
				AssertEquals("required sailing", null, form.RequiredSailing);
			}
		}

		public void TestMatchSea()
		{
			var vessel = RefVessel.LookupVesselByName("CONDOR", Factory).First();

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "061";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			voyage.GenerateSailings();

			Factory.Save();

			var shipment = Factory.New<CommonShipmentWithVoyageFinderParent>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "SGSIN";

			VoyageFinder finder = new VoyageFinder(shipment);

			using (VesselVoyageForm form = new VesselVoyageForm(finder, Constants.TransportModes.Sea))
			{
				form.Show();
				Application.DoEvents();

				finder.JV_RV_NKVessel = vessel.RV_FK;
				finder.JV_VoyageFlight = "061";

				form.PerformAdd();

				AssertNotNull(form.LastUsedControllerForTest);

				ZJobVoyageForm voyageForm = (ZJobVoyageForm)form.LastUsedControllerForTest.LastShownForm;

				AssertEquals("form should not have closed.", true, form.Visible);
				AssertEquals("popup should be shown", true, voyageForm.Visible);
				AssertEquals("should be editing the existing voyage", voyage.PK, voyageForm.BusinessEntity.Identifier);

				JobVoyage loadedVoyage = (JobVoyage)voyageForm.BusinessEntity;

				AssertContainsExactElementsInAnyOrder("Origin Ports",
					new string[] { "AUBNE", "AUSYD" },
					Array.ConvertAll(loadedVoyage.Origins.ToArray<VoyageOrigin>(), (o) => o.JA_RL_NKPortOfLoading.ToString()));

				AssertContainsExactElementsInAnyOrder("Destination Ports",
					new string[] { "SGSIN", "NLAMS" },
					Array.ConvertAll(loadedVoyage.Destinations.ToArray<VoyageDestination>(), (d) => d.JB_RL_NKPortOfDischarge.ToString()));

				voyageForm.BusinessEntity.Factory.Save();
				voyageForm.Close();

				AssertEquals("popup should have closed.", false, form.Visible);
				AssertEquals("form should have closed.", false, form.Visible);
				AssertEquals("new voyage", true, form.NewSailingCreated);
				AssertEquals("required sailing", loadedVoyage.Sailings.GetSailingFromLoadAndDischarge("AUBNE", "SGSIN"), form.RequiredSailing);
			}
		}

		public void TestCreateSea()
		{
			CommonShipmentWithVoyageFinderParent shipment = Factory.New<CommonShipmentWithVoyageFinderParent>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "SGSIN";

			VoyageFinder finder = new VoyageFinder(shipment);

			using (VesselVoyageForm form = new VesselVoyageForm(finder, Constants.TransportModes.Sea))
			{
				form.Show();
				Application.DoEvents();

				var vessel = RefVessel.LookupVesselByName("CONDOR", Factory).First();

				finder.JV_RV_NKVessel = vessel.RV_FK;
				finder.JV_VoyageFlight = "061";

				form.PerformAdd();

				AssertNotNull(form.LastUsedControllerForTest);

				ZJobVoyageForm voyageForm = (ZJobVoyageForm)form.LastUsedControllerForTest.LastShownForm;

				AssertEquals("form should not have closed.", true, form.Visible);
				AssertEquals("popup should be shown", true, voyageForm.Visible);

				JobVoyage loadedVoyage = (JobVoyage)voyageForm.BusinessEntity;

				AssertEquals("should be editing a new voyage", false, loadedVoyage.IsInDatabase);

				AssertContainsExactElementsInAnyOrder("Origin Ports",
					new string[] { "AUBNE" },
					Array.ConvertAll(loadedVoyage.Origins.ToArray<VoyageOrigin>(), (o) => o.JA_RL_NKPortOfLoading.ToString()));

				AssertContainsExactElementsInAnyOrder("Destination Ports",
					new string[] { "SGSIN" },
					Array.ConvertAll(loadedVoyage.Destinations.ToArray<VoyageDestination>(), (d) => d.JB_RL_NKPortOfDischarge.ToString()));

				voyageForm.BusinessEntity.Factory.Save();
				voyageForm.Close();

				AssertEquals("popup should have closed.", false, form.Visible);
				AssertEquals("form should have closed.", false, form.Visible);
				AssertEquals("new voyage", true, form.NewSailingCreated);
				AssertEquals("required sailing", loadedVoyage.Sailings.GetSailingFromLoadAndDischarge("AUBNE", "SGSIN"), form.RequiredSailing);
			}
		}

		public void TestNoMatchAir()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = "QF061";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			voyage.GenerateSailings();

			Factory.Save();

			CommonShipmentWithVoyageFinderParent shipment = Factory.New<CommonShipmentWithVoyageFinderParent>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "SGSIN";

			VoyageFinder finder = new VoyageFinder(shipment);

			using (VesselVoyageForm form = new VesselVoyageForm(finder, Constants.TransportModes.Air))
			{
				form.Show();
				Application.DoEvents();

				finder.JV_VoyageFlight = "QF061";

				form.PerformAdd();

				AssertNotNull(form.LastUsedControllerForTest);

				ZJobVoyageForm voyageForm = (ZJobVoyageForm)form.LastUsedControllerForTest.LastShownForm;

				AssertEquals("form should not have closed.", true, form.Visible);
				AssertEquals("popup should be shown", true, voyageForm.Visible);

				JobVoyage loadedVoyage = (JobVoyage)voyageForm.BusinessEntity;

				AssertEquals("should be editing a new voyage", false, loadedVoyage.IsInDatabase);

				AssertContainsExactElementsInAnyOrder("Origin Ports",
					new string[] { "AUBNE" },
					Array.ConvertAll(loadedVoyage.Origins.ToArray<VoyageOrigin>(), (o) => o.JA_RL_NKPortOfLoading.ToString()));

				AssertContainsExactElementsInAnyOrder("Destination Ports",
					new string[] { "SGSIN" },
					Array.ConvertAll(loadedVoyage.Destinations.ToArray<VoyageDestination>(), (d) => d.JB_RL_NKPortOfDischarge.ToString()));

				voyageForm.BusinessEntity.Factory.Save();
				voyageForm.Close();

				AssertEquals("popup should have closed.", false, form.Visible);
				AssertEquals("form should have closed.", false, form.Visible);
				AssertEquals("new voyage", true, form.NewSailingCreated);
				AssertEquals("required sailing", loadedVoyage.Sailings.GetSailingFromLoadAndDischarge("AUBNE", "SGSIN"), form.RequiredSailing);
			}
		}

		public void TestVoyageFlightBoundTextBox_Air()
		{
			CommonShipmentWithVoyageFinderParent shipment = Factory.New<CommonShipmentWithVoyageFinderParent>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "SGSIN";

			VoyageFinder finder = new VoyageFinder(shipment);

			using (VesselVoyageForm form = new VesselVoyageForm(finder, Constants.TransportModes.Air))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("Sea panel should NOT be visible.", false, form.Controls.Find("sea_panel", true)[0].Visible);
				AssertEquals("Road panel should NOT be visible.", false, form.Controls.Find("road_panel", true)[0].Visible);
				AssertEquals("Rail panel should NOT be visible.", false, form.Controls.Find("rail_panel", true)[0].Visible);
				AssertEquals("Air panel should be visible.", true, form.Controls.Find("air_panel", true)[0].Visible);

				AssertEquals("air_VoyageFlightBoundTextBox should be visible.", true, form.Controls.Find("air_voyageFlightBoundTextBox", true)[0].Visible);
				ZTextBox textBox = (form.Controls.Find("air_voyageFlightBoundTextBox", true)[0]) as ZTextBox;
				AssertNotNull(textBox);
				AssertEquals("air_VoyageFlightBoundTextBox caption should be visible.", "Flight No", textBox.CaptionResourceString.Caption);
			}
		}

		public void TestVoyageFlightBoundTextBox_Road()
		{
			CommonShipmentWithVoyageFinderParent shipment = Factory.New<CommonShipmentWithVoyageFinderParent>();
			shipment.JS_TransportMode = Constants.TransportModes.Road;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "AUSYD";

			VoyageFinder finder = new VoyageFinder(shipment);

			using (VesselVoyageForm form = new VesselVoyageForm(finder, Constants.TransportModes.Road))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("Sea panel should NOT be visible.", false, form.Controls.Find("sea_panel", true)[0].Visible);
				AssertEquals("Air panel should NOT be visible.", false, form.Controls.Find("air_panel", true)[0].Visible);
				AssertEquals("Rail panel should NOT be visible.", false, form.Controls.Find("rail_panel", true)[0].Visible);
				AssertEquals("Road panel should be visible.", true, form.Controls.Find("road_panel", true)[0].Visible);

				AssertEquals("road_VoyageFlightBoundTextBox should be visible.", true, form.Controls.Find("road_VoyageFlightBoundTextBox", true)[0].Visible);
				ZTextBox textBox = (form.Controls.Find("road_VoyageFlightBoundTextBox", true)[0]) as ZTextBox;
				AssertNotNull(textBox);
				AssertEquals("road_VoyageFlightBoundTextBox caption should be visible.", "Truck Ref.", textBox.CaptionResourceString.Caption);
			}
		}

		public void TestVoyageFlightBoundTextBox_Sea()
		{
			CommonShipmentWithVoyageFinderParent shipment = Factory.New<CommonShipmentWithVoyageFinderParent>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "SGSIN";

			VoyageFinder finder = new VoyageFinder(shipment);

			using (VesselVoyageForm form = new VesselVoyageForm(finder, Constants.TransportModes.Sea))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("Air panel should NOT be visible.", false, form.Controls.Find("air_panel", true)[0].Visible);
				AssertEquals("Road panel should NOT be visible.", false, form.Controls.Find("road_panel", true)[0].Visible);
				AssertEquals("Rail panel should NOT be visible.", false, form.Controls.Find("rail_panel", true)[0].Visible);
				AssertEquals("Sea panel should be visible.", true, form.Controls.Find("sea_panel", true)[0].Visible);

				AssertEquals("sea_VoyageFlightBoundTextEdit should be visible.", true, form.Controls.Find("sea_voyageFlightBoundTextEdit", true)[0].Visible);
				ZTextBox textBox = (form.Controls.Find("sea_voyageFlightBoundTextEdit", true)[0]) as ZTextBox;
				AssertNotNull(textBox);
				AssertEquals("sea_VoyageFlightBoundTextEdit caption should be visible.", "Voyage No", textBox.CaptionResourceString.Caption);

				AssertEquals("sea_vesselBoundCodeFindBox should be visible.", true, form.Controls.Find("sea_vesselBoundCodeFindBox", true)[0].Visible);
				ZCodeFindBox codeBox = (form.Controls.Find("sea_vesselBoundCodeFindBox", true)[0]) as ZCodeFindBox;
				AssertNotNull(codeBox);
				AssertEquals("sea_vesselBoundCodeFindBox caption should be visible.", "Vessel", codeBox.CaptionResourceString.Caption);
			}
		}

		public void TestVoyageFlightBoundTextBox_Rail()
		{
			CommonShipmentWithVoyageFinderParent shipment = Factory.New<CommonShipmentWithVoyageFinderParent>();
			shipment.JS_TransportMode = Constants.TransportModes.Rail;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "SGSIN";

			VoyageFinder finder = new VoyageFinder(shipment);

			using (VesselVoyageForm form = new VesselVoyageForm(finder, Constants.TransportModes.Rail))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("Air panel should NOT be visible.", false, form.Controls.Find("air_panel", true)[0].Visible);
				AssertEquals("Road panel should NOT be visible.", false, form.Controls.Find("road_panel", true)[0].Visible);
				AssertEquals("Sea panel should NOT be visible.", false, form.Controls.Find("sea_panel", true)[0].Visible);
				AssertEquals("Rail panel should be visible.", true, form.Controls.Find("rail_panel", true)[0].Visible);

				AssertEquals("rail_VoyageFlightBoundTextBox should be visible.", true, form.Controls.Find("rail_VoyageFlightBoundTextBox", true)[0].Visible);
				ZTextBox textBox = (form.Controls.Find("rail_VoyageFlightBoundTextBox", true)[0]) as ZTextBox;
				AssertNotNull(textBox);
				AssertEquals("rail_VoyageFlightBoundTextBox caption should be visible.", "Journey No", textBox.CaptionResourceString.Caption);

				AssertEquals("rail_vesselBoundTextBox should be visible.", true, form.Controls.Find("rail_vesselBoundTextBox", true)[0].Visible);
				ZTextBox codeBox = (form.Controls.Find("rail_vesselBoundTextBox", true)[0]) as ZTextBox;
				AssertNotNull(codeBox);
				AssertEquals("rail_vesselBoundTextBox caption should be visible.", "Journey", codeBox.CaptionResourceString.Caption);
			}
		}

		public void TestAddNew_CreateFromJobNotAllowed()
		{
			TestAddNew_CreateFromJobNotAllowed(Constants.TransportModes.Air, Env.Security.FlightScheduleCreateFromJob);
			TestAddNew_CreateFromJobNotAllowed(Constants.TransportModes.Rail, Env.Security.RailScheduleCreateFromJob);
			TestAddNew_CreateFromJobNotAllowed(Constants.TransportModes.Road, Env.Security.TruckScheduleCreateFromJob);
			TestAddNew_CreateFromJobNotAllowed(Constants.TransportModes.Sea, Env.Security.SailingScheduleCreateFromJob);
		}

		void TestAddNew_CreateFromJobNotAllowed(ZString transportMode, SecurityCheckpoint checkPointForCreateFromJob)
		{
			var shipment = Factory.New<CommonShipmentWithVoyageFinderParent>();
			shipment.JS_TransportMode = transportMode;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "SGSIN";
			var finder = new VoyageFinder(shipment);

			var isAllowedCreateFromJob = checkPointForCreateFromJob.IsAllowed;
			using (new DisposableAction(() => { checkPointForCreateFromJob.IsAllowed = isAllowedCreateFromJob; }))
			{
				checkPointForCreateFromJob.IsAllowed = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				using (var form = new VesselVoyageForm(finder, transportMode))
				{
					form.Show();
					Application.DoEvents();
					form.PerformAdd();
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals(checkPointForCreateFromJob.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			CommonShipmentWithVoyageFinderParent shipment = Factory.New<CommonShipmentWithVoyageFinderParent>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			return new VesselVoyageForm(new VoyageFinder(shipment), shipment.JS_TransportMode);
		}

		#endregion
	}
}
