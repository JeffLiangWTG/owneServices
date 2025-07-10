using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AuthenticationService.Client.Models;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.AIS;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.Forwarding.Routing.S8.Business;
using Enterprise.Freight.Forwarding.Routing.S8.GUI;
using Enterprise.Freight.GUI.OnlineSailingSchedules;
using Enterprise.Freight.Integration;
using Enterprise.Freight.OnlineSailingSchedules;
using Enterprise.Integration.Rating;
using Enterprise.Integration.Schedule;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.TrustedMessaging.Intergration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;
using ServiceModel = Enterprise.Freight.OnlineSailingSchedules.ServiceModel;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class RoutingPluginControlTest : BaseFreightTest
	{
		#region Real-Time Flight Schedules

		[TestDate(2008, 07, 09)]
		public void TestRealTimeRouteLookup()
		{
			Transport transport = Support.Transports.AddNew();

			using (RoutingPluginControlTestForm form = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
			{
				form.Show();
				form.Select(transport);

				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_RL_NKDiscPort = "USORD";
				transport.JW_VoyageFlight = "QF123";
				transport.JW_ETD = new ZDateTime(2008, 6, 13);

				form.control.GlobalSchedulesButton.PerformClick();

				using (RealTimeRoutingForm routingForm = ((TestRoutingPluginControl)form.control).LastShownFormForTest)
				{
					routingForm.Manager.IncludeWeeklyTimetable = false;

					var request = routingForm.Manager.Requests.First();

					AssertEquals("AUSYD", request.OriginUNLOCOCode);
					AssertEquals("USORD", request.DestinationUNLOCOCode);
					AssertEquals("QF", request.AirlineCode);
					AssertEquals(new ZDateTime(2008, 6, 13), request.DepartureDate);

					string testMessageLine = "132 SYD SIN 13:30   20:45 2+08:30 ZZ AA BB    <SYD 1 SIN 3   20:45 2+08:30 ZZ   123            0 9012   >";
					RoutingResponseHeader response = new RoutingResponseHeader(testMessageLine, Factory);
					routingForm.Manager.Routings.Add(response);
					routingForm.FilterControl.FilteredGrid.Select(0);

					routingForm.DialogResult = DialogResult.OK;
					routingForm.Close();

					AssertEquals("AUSYD", transport.JW_RL_NKLoadPort);
					AssertEquals("SGSIN", transport.JW_RL_NKDiscPort);
				}
			}
		}

		[TestDate(2008, 07, 09)]
		public void TestRealTimeRouteLookup_ITransportParentCore()
		{
			var parent = Factory.New<IDtbBookingConsolidation>();
			var parentCore = (ITransportParentCore)parent;
			var transportCollection = new TransportCollection(parentCore);
			var transport = transportCollection.AddNew();
			AssertEquals(1, transportCollection.Count);

			using (var form = new RoutingPluginControlTestForm_TransportBooking(transportCollection))
			{
				form.Show();
				form.Select(transport);

				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_RL_NKDiscPort = "USORD";
				transport.JW_VoyageFlight = "QF123";
				transport.JW_ETD = new ZDateTime(2008, 4, 13);

				form.control.GlobalSchedulesButton.PerformClick();

				using (RealTimeRoutingForm routingForm = ((TestRoutingPluginControl)form.control).LastShownFormForTest)
				{
					routingForm.Manager.IncludeWeeklyTimetable = false;
					var request = routingForm.Manager.Requests.First();

					AssertEquals("AUSYD", request.OriginUNLOCOCode);
					AssertEquals("USORD", request.DestinationUNLOCOCode);
					AssertEquals("QF", request.AirlineCode);
					AssertEquals(new ZDateTime(2008, 4, 13), request.DepartureDate);

					var testMessageLine = "132 SYD BOM 13:30   20:45 2+08:30 ZZ AA BB    <SYD 1 SIN 3   20:45 1+09:45 ZZ   123            0 9012   > <SIN 4 BOM A 1+10:45 2+08:30 XX   090            3 1003   >";
					var response = new RoutingResponseHeader(testMessageLine, Factory);
					routingForm.Manager.Routings.Add(response);
					routingForm.FilterControl.FilteredGrid.Select(0);

					routingForm.DialogResult = DialogResult.OK;
					routingForm.Close();

					AssertEquals("AUSYD", transport.JW_RL_NKLoadPort);
					AssertEquals("SGSIN", transport.JW_RL_NKDiscPort);
					AssertEquals(new ZDateTime(2008, 4, 13, 20, 45, 0), transport.JW_ETD);
					AssertEquals(new ZDateTime(2008, 4, 14, 9, 45, 0), transport.JW_ETA);
					AssertEquals("ZZ123", transport.JW_VoyageFlight);
					AssertEquals(Core.Constants.TransportModes.Air, transport.JW_TransportMode);

					AssertEquals(2, transportCollection.Count);
					AssertEquals("SGSIN", transportCollection[1].JW_RL_NKLoadPort);
					AssertEquals("INBOM", transportCollection[1].JW_RL_NKDiscPort);
					AssertEquals(new ZDateTime(2008, 4, 14, 10, 45, 0), transportCollection[1].JW_ETD);
					AssertEquals(new ZDateTime(2008, 4, 15, 8, 30, 0), transportCollection[1].JW_ETA);
					AssertEquals("XX090", transportCollection[1].JW_VoyageFlight);
					AssertEquals(Core.Constants.TransportModes.Air, transportCollection[1].JW_TransportMode);
				}
			}
		}

		#region RealTimeRouteLookup via Double Clicking Headers

		public void TestRealTimeRouteLookup_DoubleClickHeaders_ShouldNotUseOldVoyageDate()
		{
			var departureDate = ZDateTime.Today.AddDays(-30); // Select past date to avoid real online schedules filling the header grid.
			var oldDate = departureDate.AddDays(-90);

			CreateVoyageAndSailing("ZZ123", oldDate.AddHours(20).AddMinutes(45),
				"AUSYD", oldDate.AddHours(20).AddMinutes(45),
				"SGSIN", oldDate.AddDays(1).AddHours(9).AddMinutes(45));
			CreateVoyageAndSailing("XX090", oldDate.AddDays(1).AddHours(10).AddMinutes(45),
				"SGSIN", oldDate.AddDays(1).AddHours(10).AddMinutes(45),
				"INBOM", oldDate.AddDays(2).AddHours(8).AddMinutes(30));

			Factory.Save();

			AssertMatchingVoyagesCount("ZZ123", 1);
			AssertMatchingVoyagesCount("XX090", 1);

			var parent = Factory.New<IDtbBookingConsolidation>();
			var parentCore = parent as ITransportParentCore;
			AssertNotNull(parentCore);

			var transportCollection = new TransportCollection(parentCore);
			var transport = transportCollection.AddNew();
			AssertEquals(1, transportCollection.Count);

			using (var form = new RoutingPluginControlTestForm_TransportBooking(transportCollection))
			{
				form.Show();
				form.Select(transport);

				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_RL_NKDiscPort = "USORD";
				transport.JW_VoyageFlight = "QF123";
				transport.JW_ETD = departureDate;

				form.control.GlobalSchedulesButton.PerformClick();

				using (var routingForm = ((TestRoutingPluginControl)form.control).LastShownFormForTest)
				{
					routingForm.Manager.IncludeWeeklyTimetable = false;
					var request = routingForm.Manager.Requests.First();

					AssertEquals("AUSYD", request.OriginUNLOCOCode);
					AssertEquals("USORD", request.DestinationUNLOCOCode);
					AssertEquals("QF", request.AirlineCode);
					AssertEquals(departureDate, request.DepartureDate);

					var testMessageLine = "132 SYD BOM 13:30   20:45 2+08:30 ZZ AA BB    <SYD 1 SIN 3   20:45 1+09:45 ZZ   123            0 9012   > <SIN 4 BOM A 1+10:45 2+08:30 XX   090            3 1003   >";
					var response = new RoutingResponseHeader(testMessageLine, Factory);
					routingForm.Manager.Routings.Add(response);
					routingForm.FilterControl.FilteredGrid.Select(0);

					Assert("IsImporting should be false before double click headers", !routingForm.IsImporting);
					routingForm.FilterControl.FilteredGrid.PerformDoubleClickForTest();
					Assert("IsImporting should be false after double click headers", !routingForm.IsImporting);

					routingForm.Close();
				}
			}

			AssertEquals("AUSYD", transport.JW_RL_NKLoadPort);
			AssertEquals("SGSIN", transport.JW_RL_NKDiscPort);
			AssertEquals(departureDate.AddHours(20).AddMinutes(45), transport.JW_ETD);
			AssertEquals(departureDate.AddDays(1).AddHours(9).AddMinutes(45), transport.JW_ETA);
			AssertEquals("ZZ123", transport.JW_VoyageFlight);
			AssertEquals(Core.Constants.TransportModes.Air, transport.JW_TransportMode);
			AssertEquals("transport JW_IsLinked should keep unchecked when double clicking headers.", false, transport.JW_IsLinked);
			AssertEquals("transport should not link to a sailing.", ZGuid.Empty, transport.JW_JX);

			AssertEquals(2, transportCollection.Count);

			var transport2 = transportCollection[1];
			AssertEquals("SGSIN", transport2.JW_RL_NKLoadPort);
			AssertEquals("INBOM", transport2.JW_RL_NKDiscPort);
			AssertEquals(departureDate.AddDays(1).AddHours(10).AddMinutes(45), transport2.JW_ETD);
			AssertEquals(departureDate.AddDays(2).AddHours(8).AddMinutes(30), transport2.JW_ETA);
			AssertEquals("XX090", transport2.JW_VoyageFlight);
			AssertEquals(Core.Constants.TransportModes.Air, transport2.JW_TransportMode);
			AssertEquals("transport2 JW_IsLinked should keep unchecked when double clicking headers.", false, transport2.JW_IsLinked);
			AssertEquals("transport2 should not link to a sailing.", ZGuid.Empty, transport2.JW_JX);

			AssertMatchingVoyagesCount("ZZ123", 1);
			AssertMatchingVoyagesCount("XX090", 1);
		}

		void AssertMatchingVoyagesCount(ZString voyageFlight, int expectedVoyagesCount)
		{
			var query = new ZQuery(JobVoyageSchema.JV_VoyageFlight, voyageFlight);
			query.AddToFilter(JobVoyageSchema.JV_AirSeaRoad, Core.Constants.TransportModes.Air);
			var voyages = Factory.Load<JobVoyage>(query);
			AssertEquals("No new voyage was generated and the count should keep unchanged.", expectedVoyagesCount, voyages.Length);
		}

		void CreateVoyageAndSailing(ZString voyageFlight, ZDateTime flightDate,
			ZString portLoading, ZDateTime originEstimatedDepartureTime,
			ZString portOfDischarge, ZDateTime destEstimatedArrivalTime)
		{
			var voyage = Factory.New<JobVoyage>();
			var origin = Factory.New<VoyageOrigin>();
			var destination = Factory.New<VoyageDestination>();
			var sailing = Factory.New<JobSailing>();

			voyage.JV_VoyageFlight = voyageFlight;
			voyage.JV_FlightDate = flightDate;
			voyage.JV_IsCargoOnly = false;
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_IsActive = true;

			origin.JA_RL_NKPortOfLoading = portLoading;
			origin.JA_AutoCreated = true;
			origin.JA_JV = voyage.PK;
			origin.JA_E_DEP = originEstimatedDepartureTime;

			destination.JB_RL_NKPortOfDischarge = portOfDischarge;
			destination.JB_AutoCreated = true;
			destination.JB_JV = voyage.PK;
			destination.JB_E_ARV = destEstimatedArrivalTime;

			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			sailing.JX_IsPublished = true;
		}

		#endregion

		public void TestRealTimeRouteLookup_SecurityDenied()
		{
			Env.Security.RoutingRealTimeLookup.IsAllowed = false;

			Transport transport = Support.Transports.AddNew();

			using (RoutingPluginControlTestForm form = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
			{
				form.Show();
				form.Select(transport);

				AssertExceptionThrown<SecurityAccessDeniedException>("A new form should not have been created.", () =>
					{
						form.control.GlobalSchedulesButton.PerformClick();
						AssertContains("You do not have the appropriate security rights to run this function.", UnitTestUserNotification.Instance.LastMessage.Text);
					});
			}
		}

		#endregion

		#region Transport Mode

		public void TestPanelSelection()
		{
			Transport transport1 = Support.Transports.AddNew();

			using (RoutingPluginControlTestForm form = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
			{
				form.Show();

				transport1.JW_TransportMode = "";
				form.Select(transport1);

				transport1.JW_TransportMode = Constants.TransportModes.Air;
				AssertEquals("Transport mode set to air", form.OriginDestinationPanel, form.SelectedEndPointPanel);
				AssertIdentifiers(form);

				Transport transport2 = Support.Transports.AddNew();
				form.Select(transport1);
				transport2.JW_TransportMode = Constants.TransportModes.Rail;
				AssertEquals("Transport mode set to air", form.OriginDestinationPanel, form.SelectedEndPointPanel);
				AssertIdentifiers(form);

				form.Select(transport2);
				AssertEquals("Transport mode set to air", form.OriginDestinationPanel, form.SelectedEndPointPanel);
				AssertIdentifiers(form);

				transport2.JW_TransportMode = Constants.TransportModes.Sea;
				AssertEquals("Transport mode set to sea", form.OriginDestinationPanel, form.SelectedEndPointPanel);
				AssertIdentifiers(form);

				transport2.JW_TransportMode = Constants.TransportModes.Road;
				AssertEquals("Transport mode set to road", form.OriginDestinationPanel, form.SelectedEndPointPanel);
				AssertIdentifiers(form);

				transport2.JW_TransportMode = Constants.TransportModes.Storage;
				AssertEquals("Transport mode set to storage", form.ReceivalAvailabilityPanel, form.SelectedEndPointPanel);
			}
		}

		void AssertIdentifiers(RoutingPluginControlTestForm form)
		{
			AssertEquals("CTO Receival", form.control.JW_TerminalReceivalCommencesBoundDateEdit.Extensions.Get<ILabelCaptionRenderer>().Caption);
			AssertEquals("CTO Cut Off", form.control.JW_TerminalCutOffBoundDateEdit.Extensions.Get<ILabelCaptionRenderer>().Caption);
			AssertEquals("CFS Receival", form.control.JW_DepotReceivalCommencesBoundDateEdit.Extensions.Get<ILabelCaptionRenderer>().Caption);
			AssertEquals("CFS Cut Off", form.control.JW_DepotCutOffBoundDateEdit.Extensions.Get<ILabelCaptionRenderer>().Caption);
			AssertEquals("CTO Available", form.control.JW_TerminalAvailabilityDateBoundDateEdit.Extensions.Get<ILabelCaptionRenderer>().Caption);
			AssertEquals("CTO Storage", form.control.JW_TerminalStorageDateBoundDateEdit.Extensions.Get<ILabelCaptionRenderer>().Caption);
			AssertEquals("CFS Available", form.control.JW_DepotAvailabilityDateBoundDateEdit.Extensions.Get<ILabelCaptionRenderer>().Caption);
			AssertEquals("CFS Storage", form.control.JW_DepotStorageDateBoundDateEdit.Extensions.Get<ILabelCaptionRenderer>().Caption);
		}

		public void TestControlVisibility()
		{
			Transport transport = Support.Transports.AddNew();

			using (RoutingPluginControlTestForm form = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
			{
				form.Show();

				transport.JW_TransportMode = "";
				form.Select(transport);

				transport.JW_IsLinked = true;
				transport.JW_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals(true, form.control.JW_VoyageFlightBoundTextBox.Visible);
				AssertEquals(true, form.control.JW_JX_JV_RegistrationNoTextBox.Visible);
				AssertEquals(true, form.control.JW_AircraftTypeBoundTextBox.Visible);
				AssertEquals(false, form.control.JW_VesselBoundCodeFindBox.Visible);
				AssertEquals(false, form.control.JW_VesselBoundTextBox.Visible);
				AssertEquals(true, form.control.JW_IsCargoOnlyCheckBox.Visible);

				transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals(true, form.control.JW_VoyageFlightBoundTextBox.Visible);
				AssertEquals(false, form.control.JW_JX_JV_RegistrationNoTextBox.Visible);
				AssertEquals(false, form.control.JW_AircraftTypeBoundTextBox.Visible);
				AssertEquals(true, form.control.JW_VesselBoundCodeFindBox.Visible);
				AssertEquals(false, form.control.JW_VesselBoundTextBox.Visible);
				AssertEquals(false, form.control.JW_IsCargoOnlyCheckBox.Visible);

				transport.JW_TransportMode = Core.Constants.TransportModes.Road;
				AssertEquals(true, form.control.JW_VoyageFlightBoundTextBox.Visible);
				AssertEquals(false, form.control.JW_JX_JV_RegistrationNoTextBox.Visible);
				AssertEquals(false, form.control.JW_AircraftTypeBoundTextBox.Visible);
				AssertEquals(false, form.control.JW_VesselBoundCodeFindBox.Visible);
				AssertEquals(true, form.control.JW_VesselBoundTextBox.Visible);
				AssertEquals(false, form.control.JW_IsCargoOnlyCheckBox.Visible);

				transport.JW_TransportMode = Core.Constants.TransportModes.Rail;
				AssertEquals(true, form.control.JW_VoyageFlightBoundTextBox.Visible);
				AssertEquals(false, form.control.JW_JX_JV_RegistrationNoTextBox.Visible);
				AssertEquals(false, form.control.JW_AircraftTypeBoundTextBox.Visible);
				AssertEquals(false, form.control.JW_VesselBoundCodeFindBox.Visible);
				AssertEquals(true, form.control.JW_VesselBoundTextBox.Visible);
				AssertEquals(false, form.control.JW_IsCargoOnlyCheckBox.Visible);

				transport.JW_TransportMode = Core.Constants.TransportModes.Storage;
				AssertEquals(false, form.control.JW_VoyageFlightBoundTextBox.Visible);
				AssertEquals(false, form.control.JW_JX_JV_RegistrationNoTextBox.Visible);
				AssertEquals(false, form.control.JW_AircraftTypeBoundTextBox.Visible);
				AssertEquals(false, form.control.JW_VesselBoundCodeFindBox.Visible);
				AssertEquals(false, form.control.JW_VesselBoundTextBox.Visible);
				AssertEquals(false, form.control.JW_IsCargoOnlyCheckBox.Visible);

				transport.JW_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
				AssertEquals(true, form.control.JW_VoyageFlightBoundTextBox.Visible);
				AssertEquals(false, form.control.JW_JX_JV_RegistrationNoTextBox.Visible);
				AssertEquals(false, form.control.JW_AircraftTypeBoundTextBox.Visible);
				AssertEquals(true, form.control.JW_VesselBoundCodeFindBox.Visible);
				AssertEquals(false, form.control.JW_VesselBoundTextBox.Visible);
				AssertEquals(false, form.control.JW_IsCargoOnlyCheckBox.Visible);
			}
		}

		public void TestAdditionalTransportModeVisibility()
		{
			var transport = Support.Transports.AddNew();

			using (var form = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
			{
				form.Show();

				transport.JW_TransportMode = "";
				form.Select(transport);

				transport.JW_TransportMode = Constants.TransportModes.Air;
				transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
				AssertEquals(false, form.control.JW_AdditionalTransportModeBoundDropEdit.Visible);

				transport.JW_TransportMode = Constants.TransportModes.Rail;
				transport.JW_TransportType = Constants.TransportPlanningType.OnForwarding;
				AssertEquals(true, form.control.JW_AdditionalTransportModeBoundDropEdit.Visible);

				transport.JW_TransportMode = Constants.TransportModes.InlandWaterwayTransport;
				transport.JW_TransportType = Constants.TransportPlanningType.PreCarriage;
				AssertEquals(true, form.control.JW_AdditionalTransportModeBoundDropEdit.Visible);
			}
		}

		public void TestIsLinkedDependantControlsVisibility()
		{
			Transport transport = Support.Transports.AddNew();

			using (var form = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
			{
				form.Show();

				transport.JW_TransportMode = "";
				transport.JW_IsLinked = false;
				form.Select(transport);

				Action<bool, bool> isLinkedDependantControlsVisibility = (carrierAddressVisible, cargoonlyvisible) =>
				{
					AssertEquals(carrierAddressVisible, form.control.JW_OA_CarrierAddressZAddressControl.Visible);
					AssertEquals(!carrierAddressVisible, form.control.CarrierPKFindBox.Visible);
					AssertEquals(cargoonlyvisible, form.control.JW_IsCargoOnlyCheckBox.Visible);
				};

				isLinkedDependantControlsVisibility(true, false);

				transport.JW_TransportMode = Core.Constants.TransportModes.Air;
				isLinkedDependantControlsVisibility(true, true);

				transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
				isLinkedDependantControlsVisibility(true, false);

				transport.JW_IsLinked = true;
				isLinkedDependantControlsVisibility(false, false);

				transport.JW_IsLinked = false;
				isLinkedDependantControlsVisibility(true, false);

				transport.JW_TransportMode = Core.Constants.TransportModes.Air;
				isLinkedDependantControlsVisibility(true, true);

				transport.JW_IsLinked = true;
				isLinkedDependantControlsVisibility(true, true);
			}
		}

		#endregion

		#region SetUpContextMenu

		public void TestSetUpContextMenu()
		{
			Transport transport = Support.Transports.AddNew();

			using (RoutingPluginControlTestForm form = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
			{
				form.Show();
				AssertNotNull("Context menu contains Calculate distance", form.Grid.ContextMenu.MenuItems.FindByText("Calculate distance"));
				AssertNotNull("Context menu contains Calculate distance for all", form.Grid.ContextMenu.MenuItems.FindByText("Calculate distance for all"));
			}
		}

		#endregion

		#region Validate Flight against Global Schedule

		public void TestValidateFlightMenuOption_OnClick_SeaTransport()
		{
			var transport = Support.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Sea;

			using (var form = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
			{
				form.Show();
				var menuItem = form.Grid.ContextMenu.MenuItems.FindByText("Validate Flight against Global Flight Schedule");
				AssertNotNull("Precondition: Menu item is visible", menuItem);

				menuItem.PerformClick();

				AssertEquals("Only air legs can be validated", "Error Please select an Air Routing Leg to validate.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		[MasterFiles.Integration.Test.MatchAgainstOnlineFlightsInUnitTest]
		public void TestValidateFlightMenuOption_OnClick_AirTransport()
		{
			var mock = new Mock<IS8Matcher>();
			var matcher = mock.Object;
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).ScheduleStatus).Returns(Constants.FlightScheduleStatus.Matched);
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).MatchedSchedule).Returns(new ScheduleInfo("QF", 11, "SYD", ZDate.Today, "JFK", ZDate.Today.AddDays(1)));
			mock.Setup(r => r.Match(It.IsAny<ScheduleInfo>()).MatchErrorMessage).Returns("");

			using (ObjectFactory.Substitute(matcher))
			{
				var transport = Support.Transports[0];
				transport.JW_TransportMode = Constants.TransportModes.Air;
				transport.JW_VoyageFlight = "QF11";
				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_ETD = ZDate.Today;
				transport.JW_RL_NKDiscPort = "USJFK";
				transport.JW_ETA = ZDate.Today.AddDays(1);

				using (var form = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
				{
					form.Show();
					var menuItem = form.Grid.ContextMenu.MenuItems.FindByText("Validate Flight against Global Flight Schedule");
					AssertNotNull("Precondition: Menu item is visible", menuItem);

					menuItem.PerformClick();

					AssertEquals("Flight is validated and status set", Constants.FlightScheduleStatus.Matched, transport.JW_OnlineScheduleStatus);
				}
			}
		}

		#endregion

		#region TestSetCalculatedDistance

		public void TestSetCalculatedDistance_RoutingCollection()
		{
			var collection = Support.TransportsIncludingRelated;
			using (var form = new RoutingPluginControlTestForm(collection))
			{
				TestSetCalculatedDistance(form, form.Grid, collection);
			}
		}

		public void TestSetCalculatedDistance_TransportCollection()
		{
			var bookingConsolidation = Factory.New<IDtbBookingConsolidation>();
			var transportParentCore = (ITransportParentCommon)bookingConsolidation;
			var collection = new TransportCollection(transportParentCore);
			using (var form = new RoutingPluginControlTestForm_TransportBooking(collection))
			{
				TestSetCalculatedDistance(form, form.Grid, collection);
			}
		}

		void TestSetCalculatedDistance(ZChildForm form, ZGrid grid, BusinessObjectCollection collection)
		{
			var transport1 = (Transport)collection.AddNew();
			transport1.JW_TransportMode = Constants.TransportModes.Road;
			transport1.JW_RL_NKLoadPort = "USCHI";
			transport1.JW_RL_NKDiscPort = "USLAX";

			var transport2 = (Transport)collection.AddNew();
			transport2.JW_TransportMode = Constants.TransportModes.Air;
			transport2.JW_RL_NKLoadPort = "USCHI";
			transport2.JW_RL_NKDiscPort = "USLAX";

			var transport3 = (Transport)collection.AddNew();
			transport3.JW_TransportMode = Constants.TransportModes.Road;
			transport3.JW_RL_NKLoadPort = "USCHI";
			transport3.JW_RL_NKDiscPort = "USNYC";

			var transport4 = (Transport)collection.AddNew();
			transport4.JW_TransportMode = Constants.TransportModes.Sea;
			transport4.JW_RL_NKLoadPort = "USCHI";
			transport4.JW_RL_NKDiscPort = "USLAX";

			form.Show();

			var calcDistanceForAllButton = grid.ContextMenu.MenuItems.FindByText("Calculate distance for all");
			AssertNotNull("Context menu contains Calculate distance for all", calcDistanceForAllButton);

			calcDistanceForAllButton.PerformClick();
			AssertNotEquals("Distance is calculated for Road", 0M, transport1.JW_Distance);
			AssertEquals("Distance is not calculated for Air", 0M, transport2.JW_Distance);
			AssertNotEquals("Distance is calculated for Road", 0M, transport3.JW_Distance);
			AssertEquals("Distance is not calculated for Sea", 0M, transport4.JW_Distance);
		}

		#endregion

		#region TestDisabledTransportGridWhenControlIsDisabled

		public void TestDisabledTransportGridWhenControlIsDisabled()
		{
			var transport1 = Support.Transports.AddNew();

			using (RoutingPluginControlTestForm form = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
			{
				form.Show();
				AssertEquals("Precondition:", false, form.Grid.ReadOnly);

				form.Select(transport1);
				form.control.Enabled = false;
				transport1.JW_TransportMode = Core.Constants.TransportModes.Air; // fire SetControlIDsForTransportMode
				AssertEquals(true, form.Grid.ReadOnly);

				form.control.Enabled = true;
				transport1.JW_TransportMode = Core.Constants.TransportModes.Sea; // fire SetControlIDsForTransportMode
				AssertEquals(false, form.Grid.ReadOnly);
			}
		}

		#endregion

		#region Online Sailing Schedules

		public void TestOnlineSailingSchedulesButtonVisibility_ControlledByRegistry()
		{
			using (FreightDataRegistry.Instance.EnableScheduleFeedService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var onlineSailingSchedules = ObjectFactory.Get<Integration.SailingDataVendor.IOnlineSailingSchedulesDataVendor>() as Business.SailingScheduleDataVendor;
				AssertEquals("prerequisite", false, onlineSailingSchedules.IsEnabled);

				using (var form = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
				{
					form.Show();
					AssertEquals("Invisible when relevant registry is OFF", false, form.control.ImportGlobalScheduleButton.Visible);
				}
			}

			using (FreightDataRegistry.Instance.EnableScheduleFeedService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var onlineSailingSchedules = ObjectFactory.Get<Integration.SailingDataVendor.IOnlineSailingSchedulesDataVendor>() as Business.SailingScheduleDataVendor;
				AssertEquals("prerequisite", true, onlineSailingSchedules.IsEnabled);

				using (var form = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
				{
					form.Show();
					AssertEquals("Visible when relevant registry is ON", true, form.control.ImportGlobalScheduleButton.Visible);
				}
			}
		}

		public void TestOnlineSailingSchedules_DoNotCheckTransport()
		{
			using (FreightDataRegistry.Instance.EnableScheduleFeedService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var transport = Support.Transports[0];

				using (var form = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
				{
					form.Show();

					transport.JW_IsLinked = false;
					transport.JW_TransportMode = Constants.TransportModes.Sea;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					form.control.ImportGlobalScheduleButton.PerformClick();
					AssertEquals("None ", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertEquals(true, ZFormModaliser.LastFormShownForTest is OnlineSchedulesForm);
				}

				using (var form = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
				{
					form.Show();

					transport.JW_IsLinked = true;
					transport.JW_TransportMode = Constants.TransportModes.Air;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					form.control.ImportGlobalScheduleButton.PerformClick();
					AssertEquals("None ", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertEquals(true, ZFormModaliser.LastFormShownForTest is OnlineSchedulesForm);
				}

				using (var form = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
				{
					form.Show();

					transport.JW_IsLinked = true;
					transport.JW_TransportMode = Constants.TransportModes.Sea;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					form.control.ImportGlobalScheduleButton.PerformClick();
					AssertEquals("None ", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertEquals(true, ZFormModaliser.LastFormShownForTest is OnlineSchedulesForm);
				}

				using (var form = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
				{
					form.Show();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					var control = form.control;
					var prop = control.GetType().BaseType.GetField("selectedTransport", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
					prop.SetValue(control, null);

					form.control.ImportGlobalScheduleButton.PerformClick();
					AssertEquals("None ", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertEquals(true, ZFormModaliser.LastFormShownForTest is OnlineSchedulesForm);
				}
			}
		}

		#endregion

		#region Readonly TransportParent DueTo PhaseSecurity

		public void TestSheduleAndFlightButtonsIsReadonlyDueToPhaseSecurity()
		{
			var consol = Factory.New<CommonConsolForTest>();
			consol.PropertiesForcedToReadOnlyDueToPhase.Add("Routing");

			using (var form = new RoutingPluginControlTestForm(((IRoutingSupport)consol).TransportsIncludingRelated))
			{
				form.Show();
				CheckButtonWithMessage(form.control.SelectScheduleButton, "Error Action denied as modifying of routing legs restricted by current phase. Refer Registry > Freight > Consolidations > Phases.");
				CheckButtonWithMessage(form.control.GlobalSchedulesButton, "Error Action denied as modifying of routing legs restricted by current phase. Refer Registry > Freight > Consolidations > Phases.");
				CheckButtonWithMessage(form.control.ImportGlobalScheduleButton, "Error Action denied as modifying of routing legs restricted by current phase. Refer Registry > Freight > Consolidations > Phases.");
			}
		}

		#endregion

		#region First Arrival & Last Foreign Port Matching

		public void TestSuccessfulRouteImport_MatchesPorts_OneLegImported()
		{
			// Arrange
			const string carrierUsCustomsRegNo = "CMAC";
			const string vesselName = "Santa Maria";
			const string lloydsNumber = "9308390";
			const string loadPort = "CNSHA";
			var etd = new DateTime(2023, 3, 1);
			const string dischargePort = "AUMEL";
			var eta = new DateTime(2023, 3, 15);
			const string voyageNumber = "754S";

			CreateCarrierAndVessel(carrierUsCustomsRegNo, vesselName, lloydsNumber);

			var transport = Support.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_IsLinked = true;

			var consol = Support as CommonConsol;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "AUSYD";

			using (var routingPluginControlForm = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
			{
				routingPluginControlForm.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				routingPluginControlForm.control.ImportGlobalScheduleButton.PerformClick();

				var onlineSchedulesForm = (OnlineSchedulesForm)ZFormModaliser.LastFormShownForTest;
				var onlineSchedules = (OnlineSchedules)onlineSchedulesForm.DataSource;
				var route = onlineSchedules.Routes.AddNew();
				route.SetValues(BuildServiceModelRouteWithOneLeg(carrierUsCustomsRegNo, vesselName, lloydsNumber, loadPort, dischargePort, etd, eta, voyageNumber));

				onlineSchedulesForm.FilterControl.FilteredGrid.SelectSingleElement(route);

				var portMatcher = new Mock<IPortMatcher>();
				portMatcher
					.Setup(matcher =>
						matcher.MatchAsync(It.IsAny<VesselMovementsUrlModel>(), It.IsAny<CancellationToken>()))
					.Returns(Task.FromResult<IPortMatches>(new PortMatches(
						lastForeignPort: new PortMatch(unloco: "TWTPE",
							arrivalTime: new DateTimeOffset(2023, 3, 4, 10, 0, 0, TimeSpan.FromHours(+8)),
							departureTime: new DateTimeOffset(2023, 3, 5, 8, 0, 0, TimeSpan.FromHours(+8))),
						firstArrivalPort: new PortMatch(unloco: "AUSYD",
							arrivalTime: new DateTimeOffset(2023, 3, 13, 19, 0, 0, TimeSpan.FromHours(+11)),
							departureTime: new DateTimeOffset(2023, 3, 14, 22, 0, 0, TimeSpan.FromHours(+11))))));

				using (ObjectFactory.Substitute(portMatcher.Object))
				{
					// Act
					DoImportButtonClickOnOnlineScheduleForm(onlineSchedulesForm);
					UnitTestUserNotification.Instance.AddOKAnswer();

					// Assert
					CombineAssertions(() =>
					{
						var expectedVesselMovementsUrlModel = new VesselMovementsUrlModel
						{
							DeparturePortUnloco = loadPort,
							DepartureTime = etd,
							ArrivalPortUnloco = dischargePort,
							ArrivalTime = eta,
							CarrierCode = carrierUsCustomsRegNo,
							LloydsNumber = lloydsNumber,
							VoyageNumber = voyageNumber
						};
						AssertNoExceptionThrown("Port Matching request must be constructed from the only imported leg.",
							() => portMatcher.Verify(matcher => matcher.MatchAsync(expectedVesselMovementsUrlModel, CancellationToken.None), Times.Once()));

						Factory.Save();

						AssertEquals("TWTPE", consol.JK_RL_NKLastForeignPort);
						AssertEquals(new ZDateTime(2023, 3, 5, 8, 0, 0), consol.JK_DateLastForeignPort);

						AssertEquals("AUSYD", consol.JK_RL_NKPortOfFirstArrival);
						AssertEquals(new ZDateTime(2023, 3, 13, 19, 0, 0), consol.JK_DatePortOfFirstArrival);
					});
				}
			}
		}

		public void TestSuccessfulRouteImport_MatchesPorts_MultipleLegImported_MatchConsolDischargePortCountry()
		{
			// Arrange
			const string carrierUsCustomsRegNo = "CMAC";
			const string vesselName = "Santa Maria";
			const string lloydsNumber = "9308390";
			const string loadPort = "AUMEL";
			var etd = new DateTime(2023, 3, 1);
			const string dischargePort = "CNSHA";
			var eta = new DateTime(2023, 3, 15);
			const string voyageNumber = "754S";

			CreateCarrierAndVessel(carrierUsCustomsRegNo, vesselName, lloydsNumber);

			var consol = Support as CommonConsol;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			var transport = Support.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_IsLinked = true;

			using (var routingPluginControlForm = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
			{
				routingPluginControlForm.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				routingPluginControlForm.control.ImportGlobalScheduleButton.PerformClick();

				var onlineSchedulesForm = (OnlineSchedulesForm)ZFormModaliser.LastFormShownForTest;
				var onlineSchedules = (OnlineSchedules)onlineSchedulesForm.DataSource;
				var route = onlineSchedules.Routes.AddNew();
				route.SetValues(BuildServiceModelRouteWithOneLeg(carrierUsCustomsRegNo, vesselName, lloydsNumber, loadPort, dischargePort, etd, eta, voyageNumber));
				var domesticLeg = route.Legs.AddNew();
				domesticLeg.OriginPortUnloco = "CNTAO";
				domesticLeg.DestinationPortUnloco = "CNSHA";
				domesticLeg.Departure = new DateTime(2023, 3, 17);
				domesticLeg.Arrival = new DateTime(2023, 3, 19);

				onlineSchedulesForm.FilterControl.FilteredGrid.SelectSingleElement(route);

				var portMatcher = new Mock<IPortMatcher>();
				portMatcher
					.Setup(matcher =>
						matcher.MatchAsync(It.IsAny<VesselMovementsUrlModel>(), It.IsAny<CancellationToken>()))
					.Returns(Task.FromResult<IPortMatches>(new PortMatches(
						lastForeignPort: new PortMatch(unloco: "AUBNE",
							arrivalTime: new DateTimeOffset(2023, 3, 4, 10, 0, 0, TimeSpan.FromHours(+8)),
							departureTime: new DateTimeOffset(2023, 3, 5, 8, 0, 0, TimeSpan.FromHours(+8))),
						firstArrivalPort: new PortMatch(unloco: "CNTAO",
							arrivalTime: new DateTimeOffset(2023, 3, 13, 19, 0, 0, TimeSpan.FromHours(+11)),
							departureTime: new DateTimeOffset(2023, 3, 14, 22, 0, 0, TimeSpan.FromHours(+11))))));

				using (ObjectFactory.Substitute(portMatcher.Object))
				{
					// Act
					DoImportButtonClickOnOnlineScheduleForm(onlineSchedulesForm);
					UnitTestUserNotification.Instance.AddOKAnswer();

					// Assert
					CombineAssertions(() =>
					{
						var expectedVesselMovementsUrlModel = new VesselMovementsUrlModel
						{
							DeparturePortUnloco = loadPort,
							DepartureTime = etd,
							ArrivalPortUnloco = dischargePort,
							ArrivalTime = eta,
							CarrierCode = carrierUsCustomsRegNo,
							LloydsNumber = lloydsNumber,
							VoyageNumber = voyageNumber
						};
						AssertNoExceptionThrown("Port Matching request must be constructed from the first imported leg with a Discharge Port matching the same country as the Consol Last Disc port.",
							() => portMatcher.Verify(matcher => matcher.MatchAsync(expectedVesselMovementsUrlModel, CancellationToken.None), Times.Once()));

						Factory.Save();

						AssertEquals("AUBNE", consol.JK_RL_NKLastForeignPort);
						AssertEquals(new ZDateTime(2023, 3, 5, 8, 0, 0), consol.JK_DateLastForeignPort);

						AssertEquals("CNTAO", consol.JK_RL_NKPortOfFirstArrival);
						AssertEquals(new ZDateTime(2023, 3, 13, 19, 0, 0), consol.JK_DatePortOfFirstArrival);
					});
				}
			}
		}

		public void TestSuccessfulRouteImport_MatchesPorts_MultipleLegImported_NoMatchConsolDischargePortCountry()
		{
			// Arrange
			const string carrierUsCustomsRegNo = "CMAC";
			const string vesselName = "Santa Maria";
			const string lloydsNumber = "9308390";
			const string loadPort = "CNTAO";
			var etd = new DateTime(2023, 3, 1);
			const string dischargePort = "CNSHG";
			var eta = new DateTime(2023, 3, 15);
			const string voyageNumber = "754S";

			CreateCarrierAndVessel(carrierUsCustomsRegNo, vesselName, lloydsNumber);

			var consol = Support as CommonConsol;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "HKHKG";
			var transport = Support.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_IsLinked = true;

			using (var routingPluginControlForm = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
			{
				routingPluginControlForm.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				routingPluginControlForm.control.ImportGlobalScheduleButton.PerformClick();

				var onlineSchedulesForm = (OnlineSchedulesForm)ZFormModaliser.LastFormShownForTest;
				var onlineSchedules = (OnlineSchedules)onlineSchedulesForm.DataSource;
				var route = onlineSchedules.Routes.AddNew();
				route.SetValues(BuildServiceModelRouteWithOneLeg(carrierUsCustomsRegNo, vesselName, lloydsNumber, loadPort, dischargePort, etd, eta, voyageNumber));
				var auToCnLeg = route.Legs.AddNew();
				auToCnLeg.OriginPortUnloco = "AUSYD";
				auToCnLeg.DestinationPortUnloco = "CNTAO";
				auToCnLeg.Departure = new DateTime(2023, 2, 17);
				auToCnLeg.Arrival = new DateTime(2023, 2, 25);

				onlineSchedulesForm.FilterControl.FilteredGrid.SelectSingleElement(route);

				var portMatcher = new Mock<IPortMatcher>();
				portMatcher
					.Setup(matcher =>
						matcher.MatchAsync(It.IsAny<VesselMovementsUrlModel>(), It.IsAny<CancellationToken>()))
					.Returns(Task.FromResult<IPortMatches>(new PortMatches(
						lastForeignPort: new PortMatch(unloco: "AUBNE",
							arrivalTime: new DateTimeOffset(2023, 3, 4, 10, 0, 0, TimeSpan.FromHours(+8)),
							departureTime: new DateTimeOffset(2023, 3, 5, 8, 0, 0, TimeSpan.FromHours(+8))),
						firstArrivalPort: new PortMatch(unloco: "CNTAO",
							arrivalTime: new DateTimeOffset(2023, 3, 13, 19, 0, 0, TimeSpan.FromHours(+11)),
							departureTime: new DateTimeOffset(2023, 3, 14, 22, 0, 0, TimeSpan.FromHours(+11))))));

				using (ObjectFactory.Substitute(portMatcher.Object))
				{
					// Act
					DoImportButtonClickOnOnlineScheduleForm(onlineSchedulesForm);
					UnitTestUserNotification.Instance.AddOKAnswer();

					// Assert
					CombineAssertions(() =>
					{
						var expectedVesselMovementsUrlModel = new VesselMovementsUrlModel
						{
							DeparturePortUnloco = loadPort,
							DepartureTime = etd,
							ArrivalPortUnloco = dischargePort,
							ArrivalTime = eta,
							CarrierCode = carrierUsCustomsRegNo,
							LloydsNumber = lloydsNumber,
							VoyageNumber = voyageNumber
						};
						AssertNoExceptionThrown("Port Matching request must be constructed from the last imported leg when there's no leg having a Discharge Port matches the same country as the Consol Last Disc port.",
							() => portMatcher.Verify(matcher => matcher.MatchAsync(expectedVesselMovementsUrlModel, CancellationToken.None), Times.Once()));

						Factory.Save();

						AssertEquals(string.Empty, consol.JK_RL_NKLastForeignPort);
						AssertEquals(ZDateTime.Empty, consol.JK_DateLastForeignPort);

						AssertEquals(string.Empty, consol.JK_RL_NKPortOfFirstArrival);
						AssertEquals(ZDateTime.Empty, consol.JK_DatePortOfFirstArrival);
					});
				}
			}
		}

		public void TestSuccessfulRouteImport_UpdateConsolAndDoesNotCallApi_WhenScheduleAlreadyExistsWithData()
		{
			// Arrange
			const string carrierUsCustomsRegNo = "CMAC";
			const string vesselName = "Santa Maria";
			const string lloydsNumber = "9308390";
			var etd = new DateTime(2023, 3, 1);
			var eta = new DateTime(2023, 3, 15);
			const string voyageNumber = "754S";

			CreateCarrierAndVessel(carrierUsCustomsRegNo, vesselName, lloydsNumber);
			CreateSeaVoyage(vesselName, eta, voyageNumber);

			var transport = Support.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_IsLinked = true;

			var consol = Support as CommonConsol;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "AUSYD";

			using (var routingPluginControlForm = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
			{
				routingPluginControlForm.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				routingPluginControlForm.control.ImportGlobalScheduleButton.PerformClick();

				var onlineSchedulesForm = (OnlineSchedulesForm)ZFormModaliser.LastFormShownForTest;
				var onlineSchedules = (OnlineSchedules)onlineSchedulesForm.DataSource;
				var route = onlineSchedules.Routes.AddNew();
				route.SetValues(BuildServiceModelRouteWithOneLeg(carrierUsCustomsRegNo, vesselName, lloydsNumber, "CNSHA", "AUMEL", etd, eta, voyageNumber));
				onlineSchedulesForm.FilterControl.FilteredGrid.SelectSingleElement(route);

				var portMatcher = new Mock<IPortMatcher>();

				using (ObjectFactory.Substitute(portMatcher.Object))
				{
					// Act
					DoImportButtonClickOnOnlineScheduleForm(onlineSchedulesForm);
					UnitTestUserNotification.Instance.AddOKAnswer();

					// Assert
					CombineAssertions(() =>
					{
						AssertNoExceptionThrown("Port Matching must be omitted if update is not required.",
							() => portMatcher.Verify(matcher => matcher.MatchAsync(It.IsAny<VesselMovementsUrlModel>(), It.IsAny<CancellationToken>()), Times.Never()));

						Factory.Save();

						AssertEquals("CNSHA", consol.JK_RL_NKLastForeignPort);
						AssertEquals(new DateTime(2023, 3, 4), consol.JK_DateLastForeignPort);

						AssertEquals("AUSYD", consol.JK_RL_NKPortOfFirstArrival);
						AssertEquals(new DateTime(2023, 3, 10), consol.JK_DatePortOfFirstArrival);
					});
				}
			}
		}

		void CreateSeaVoyage(string vesselName, DateTime eta, string voyageNumber)
		{
			var voyage = Factory.New<JobVoyage>();
			var destination = Factory.New<VoyageDestination>();

			voyage.JV_VoyageFlight = voyageNumber;
			voyage.JV_RV_NKVessel = vesselName;
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage.JV_IsActive = true;
			voyage.JV_OH_Line = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "AAAAA")).PK;

			destination.JB_RL_NKPortOfDischarge = "AUMEL";
			destination.JB_AutoCreated = true;
			destination.JB_JV = voyage.PK;
			destination.JB_E_ARV = eta;

			destination.JB_RL_NKFirstDischargePort = "AUSYD";
			destination.JB_FirstDischargePortETA = new DateTime(2023, 3, 10);
			destination.JB_RL_NKLastForeignPort = "CNSHA";
			destination.JB_LastForeignPortETD = new DateTime(2023, 3, 4);
			Factory.Save();
		}

		public void TestSuccessfulRouteImport_DoesNotMatchPorts_WhenNoMatches()
		{
			// Arrange
			const string carrierUsCustomsRegNo = "CMAC";
			const string vesselName = "Santa Maria";
			const string lloydsNumber = "9308390";
			var etd = new DateTime(2023, 3, 1);
			var eta = new DateTime(2023, 3, 15);
			const string voyageNumber = "754S";

			CreateCarrierAndVessel(carrierUsCustomsRegNo, vesselName, lloydsNumber);

			var transport = Support.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_IsLinked = true;

			var consol = Support as CommonConsol;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "AUSYD";

			using (var routingPluginControlForm = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
			{
				routingPluginControlForm.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				routingPluginControlForm.control.ImportGlobalScheduleButton.PerformClick();

				var onlineSchedulesForm = (OnlineSchedulesForm)ZFormModaliser.LastFormShownForTest;
				var onlineSchedules = (OnlineSchedules)onlineSchedulesForm.DataSource;
				var route = onlineSchedules.Routes.AddNew();
				route.SetValues(BuildServiceModelRouteWithOneLeg(carrierUsCustomsRegNo, vesselName, lloydsNumber, "CNSHA", "AUMEL", etd, eta, voyageNumber));

				onlineSchedulesForm.FilterControl.FilteredGrid.SelectSingleElement(route);

				var portMatcher = new Mock<IPortMatcher>();
				portMatcher
					.Setup(matcher => matcher.MatchAsync(It.IsAny<VesselMovementsUrlModel>(), It.IsAny<CancellationToken>()))
					.Returns(Task.FromResult<IPortMatches>(new PortMatches()));

				using (ObjectFactory.Substitute(portMatcher.Object))
				{
					// Act
					DoImportButtonClickOnOnlineScheduleForm(onlineSchedulesForm);
					UnitTestUserNotification.Instance.AddOKAnswer();

					// Assert
					CombineAssertions(() =>
					{
						AssertNoExceptionThrown("Port Matching must be executed.",
							() => portMatcher.Verify(matcher => matcher.MatchAsync(It.IsAny<VesselMovementsUrlModel>(), It.IsAny<CancellationToken>()), Times.Once()));

						Factory.Save();

						AssertEquals(ZString.Empty, consol.JK_RL_NKLastForeignPort);
						AssertEquals(ZDateTime.Empty, consol.JK_DateLastForeignPort);

						AssertEquals(ZString.Empty, consol.JK_RL_NKPortOfFirstArrival);
						AssertEquals(ZDateTime.Empty, consol.JK_DatePortOfFirstArrival);
					});
				}
			}
		}

		[ExpectNoExceptions]
		public void TestSuccessfulRouteImport_DoesNotMatchPorts_WhenInvalidLloydsNumber()
		{
			// Arrange
			const string carrierUsCustomsRegNo = "CMAC";
			const string vesselName = "Santa Maria";
			const string lloydsNumber = "111";
			var etd = new DateTime(2023, 7, 8);
			var eta = new DateTime(2023, 7, 15);
			const string voyageNumber = "754S";

			CreateCarrierAndVessel(carrierUsCustomsRegNo, vesselName, lloydsNumber);
			var transport = Support.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_IsLinked = true;

			var consol = Support as CommonConsol;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEHAM";

			using (var routingPluginControlForm = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
			{
				routingPluginControlForm.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				routingPluginControlForm.control.ImportGlobalScheduleButton.PerformClick();

				var onlineSchedulesForm = (OnlineSchedulesForm)ZFormModaliser.LastFormShownForTest;
				var onlineSchedules = (OnlineSchedules)onlineSchedulesForm.DataSource;
				var route = onlineSchedules.Routes.AddNew();
				route.SetValues(BuildServiceModelRouteWithOneLeg(carrierUsCustomsRegNo, vesselName, lloydsNumber, "AUSYD", "DEHAM", etd, eta, voyageNumber));

				onlineSchedulesForm.FilterControl.FilteredGrid.SelectSingleElement(route);

				var portMatcher = new Mock<IPortMatcher>();

				using (ObjectFactory.Substitute(portMatcher.Object))
				{
					// Act
					DoImportButtonClickOnOnlineScheduleForm(onlineSchedulesForm);
					UnitTestUserNotification.Instance.AddOKAnswer();

					// Assert
					CombineAssertions(() =>
					{
						AssertNoExceptionThrown("Port Matching must be omitted if LloydsNumber is invalid.",
							() => portMatcher.Verify(matcher => matcher.MatchAsync(It.IsAny<VesselMovementsUrlModel>(), It.IsAny<CancellationToken>()), Times.Never()));
						Factory.Save();

						AssertEquals(ZString.Empty, consol.JK_RL_NKLastForeignPort);
						AssertEquals(ZDateTime.Empty, consol.JK_DateLastForeignPort);

						AssertEquals(ZString.Empty, consol.JK_RL_NKPortOfFirstArrival);
						AssertEquals(ZDateTime.Empty, consol.JK_DatePortOfFirstArrival);
					});
				}
			}
		}

		public void TestSuccessfulRouteImport_ReportsResponse_WhenApiExceptionIsThrown()
		{
			// Arrange
			const string carrierUsCustomsRegNo = "CMAC";
			const string vesselName = "Santa Maria";
			const string lloydsNumber = "9308390";
			var etd = new DateTime(2023, 3, 1);
			var eta = new DateTime(2023, 3, 15);
			const string voyageNumber = "754S";

			CreateCarrierAndVessel(carrierUsCustomsRegNo, vesselName, lloydsNumber);

			var transport = Support.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_IsLinked = true;

			var consol = Support as CommonConsol;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";

			using (var routingPluginControlForm = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
			{
				routingPluginControlForm.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				routingPluginControlForm.control.ImportGlobalScheduleButton.PerformClick();

				var onlineSchedulesForm = (OnlineSchedulesForm)ZFormModaliser.LastFormShownForTest;
				var onlineSchedules = (OnlineSchedules)onlineSchedulesForm.DataSource;
				var route = onlineSchedules.Routes.AddNew();
				route.SetValues(BuildServiceModelRouteWithOneLeg(carrierUsCustomsRegNo, vesselName, lloydsNumber, "CNSHA", "AUMEL", etd, eta, voyageNumber));

				onlineSchedulesForm.FilterControl.FilteredGrid.SelectSingleElement(route);

				var response = "{\"title\":\"One or more validation errors occurred.\",\"status\":400,\"errors\":{\"Imo\":[\"Invalid IMO value '949577'. Valid format: 1234567\"]}}";
				var expectedException = new AisWebApiException(response);
				var portMatcher = new Mock<IPortMatcher>();
				portMatcher
					.Setup(matcher => matcher.MatchAsync(It.IsAny<VesselMovementsUrlModel>(), It.IsAny<CancellationToken>()))
					.Throws(expectedException);

				using (ObjectFactory.Substitute(portMatcher.Object))
				{
					// Act
					DoImportButtonClickOnOnlineScheduleForm(onlineSchedulesForm);
					UnitTestUserNotification.Instance.AddOKAnswer();
					Factory.Save();

					// Assert
					CombineAssertions(() =>
					{
						AssertEquals(ZString.Empty, consol.JK_RL_NKLastForeignPort);
						AssertEquals(ZDateTime.Empty, consol.JK_DateLastForeignPort);

						AssertEquals(ZString.Empty, consol.JK_RL_NKPortOfFirstArrival);
						AssertEquals(ZDateTime.Empty, consol.JK_DatePortOfFirstArrival);

						AssertEquals("No error is reported", 0, ErrorReporter.TotalErrorCount);

						AssertEquals("Developer exception is reported", 1, ExceptionReporterTestListener.Instance.Count);
						AssertEquals("PortMatchingFailed", ExceptionReporterTestListener.Instance.GetExceptionKey(0));
						AssertContains("Exception message contains API response", response, ExceptionReporterTestListener.Instance.GetExceptionMessage(0));

						ExceptionReporterTestListener.Instance.Clear();
					});
				}
			}
		}

		public void TestSuccessfulRouteImport_ReportsAnException_WhenMatcherThrows()
		{
			// Arrange
			const string carrierUsCustomsRegNo = "CMAC";
			const string vesselName = "Santa Maria";
			const string lloydsNumber = "9308390";
			var etd = new DateTime(2023, 3, 1);
			var eta = new DateTime(2023, 3, 15);
			const string voyageNumber = "754S";

			CreateCarrierAndVessel(carrierUsCustomsRegNo, vesselName, lloydsNumber);

			var transport = Support.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_IsLinked = true;

			var consol = Support as CommonConsol;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "AUSYD";

			using (var routingPluginControlForm = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
			{
				routingPluginControlForm.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				routingPluginControlForm.control.ImportGlobalScheduleButton.PerformClick();

				var onlineSchedulesForm = (OnlineSchedulesForm)ZFormModaliser.LastFormShownForTest;
				var onlineSchedules = (OnlineSchedules)onlineSchedulesForm.DataSource;
				var route = onlineSchedules.Routes.AddNew();
				route.SetValues(BuildServiceModelRouteWithOneLeg(carrierUsCustomsRegNo, vesselName, lloydsNumber, "CNSHA", "AUMEL", etd, eta, voyageNumber));

				onlineSchedulesForm.FilterControl.FilteredGrid.SelectSingleElement(route);

				var expectedException = new HttpRequestException();
				var portMatcher = new Mock<IPortMatcher>();
				portMatcher
					.Setup(matcher => matcher.MatchAsync(It.IsAny<VesselMovementsUrlModel>(), It.IsAny<CancellationToken>()))
					.Throws(expectedException);

				using (ObjectFactory.Substitute(portMatcher.Object))
				{
					// Act
					DoImportButtonClickOnOnlineScheduleForm(onlineSchedulesForm);
					UnitTestUserNotification.Instance.AddOKAnswer();

					Factory.Save();

					// Assert
					CombineAssertions(() =>
					{
						AssertEquals(ZString.Empty, consol.JK_RL_NKLastForeignPort);
						AssertEquals(ZDateTime.Empty, consol.JK_DateLastForeignPort);

						AssertEquals(ZString.Empty, consol.JK_RL_NKPortOfFirstArrival);
						AssertEquals(ZDateTime.Empty, consol.JK_DatePortOfFirstArrival);

						AssertEquals("PortMatchingFailed", ErrorReporter.LastKeyReported);

						ErrorReporter.Clear();
					});
				}
			}
		}

		void DoImportButtonClickOnOnlineScheduleForm(OnlineSchedulesForm form)
		{
			var importButton = form.Controls.Find("ImportButton", true).FirstOrDefault() as ZButton;
			AssertNotNull(importButton);
			importButton.PerformClick();
		}

		void CreateCarrierAndVessel(string carrierUsCustomsRegNo, string vesselName, string lloydsNumber)
		{
			var carrierOrg = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrg.OH_Code = "AAAAA";
			var cusCode = carrierOrg.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = carrierUsCustomsRegNo;
			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedStates;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = vesselName;
			vessel.RV_IsActive = true;
			vessel.RV_LloydsNumber = lloydsNumber;

			Factory.Save();
		}

		static ServiceModel.Route BuildServiceModelRouteWithOneLeg(
			string carrierUsCustomsRegNo, string vesselName, string lloydsNumber, string loadPort, string dischargePort,
			DateTime? etd, DateTime? eta, string voyageNumber)
		{
			var carrier = new ServiceModel.Carrier
			{
				Code = carrierUsCustomsRegNo,
				Name = "CMA CGM"
			};

			var voyage = new ServiceModel.Voyage
			{
				Code = voyageNumber,
				TradeLane = new ServiceModel.TradeLane { Name = "AAA" },
				Operator = new ServiceModel.Carrier { Code = "CMAC", Name = "CMA CGM" },
				Vessel = new ServiceModel.Vessel { VesselName = vesselName, ImoNumber = lloydsNumber }
			};

			var leg = new ServiceModel.Leg
			{
				LoadPort = new ServiceModel.Port { Unloco = loadPort },
				DischargePort = new ServiceModel.Port { Unloco = dischargePort },
				Etd = etd,
				Eta = eta,
				Voyage = voyage,
			};

			var serviceRoute = new ServiceModel.Route { Carrier = carrier, Legs = new[] { leg } };

			return serviceRoute;
		}

		#endregion

		#region TestTransportsGrid

		public void TestTransportsGrid_ColumnExists()
		{
			using (var form = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
			{
				form.Show();
				AssertNotNull(form.Grid);
				AssertGridColumn(form.Grid, Transport.Schema.JW_STAForBinding, false);
				AssertGridColumn(form.Grid, Transport.Schema.JW_STDForBinding, false);
				AssertGridColumn(form.Grid, Transport.Schema.JW_AircraftTypeForBinding, false);

				AssertGridColumn(form.Grid, Transport.Schema.JW_EmptyReceivalCommencesForBinding, false);
				AssertGridColumn(form.Grid, Transport.Schema.JW_EmptyCutOffForBinding, false);
				AssertGridColumn(form.Grid, Transport.Schema.JW_ReeferReceivalCommencesForBinding, false);
				AssertGridColumn(form.Grid, Transport.Schema.JW_ReeferCutOffForBinding, false);
				AssertGridColumn(form.Grid, Transport.Schema.JW_DGReceivalCommencesForBinding, false);
				AssertGridColumn(form.Grid, Transport.Schema.JW_DGCutOffForBinding, false);

				AssertGridColumn(form.Grid, Transport.Schema.JW_ArrivalPortRouteId, false);
				AssertGridColumn(form.Grid, Transport.Schema.JW_DeparturePortRouteId, false);
			}
		}

		public void TestTransportsGrid_ReadOnlyDueToPhaseSecurity()
		{
			var consol = Factory.New<CommonConsolForTest>();
			consol.PropertiesForcedToReadOnlyDueToPhase.Add("SomeOtherChildProperty");

			using (var form = new RoutingPluginControlTestForm(((IRoutingSupport)consol).TransportsIncludingRelated))
			{
				form.Show();
				AssertNotNull(form.Grid);
				AssertEquals("Should not be read only by default", false, form.Grid.ReadOnly);
			}

			consol = Factory.New<CommonConsolForTest>();
			consol.PropertiesForcedToReadOnlyDueToPhase.Add("Routing");

			using (var form = new RoutingPluginControlTestForm(((IRoutingSupport)consol).TransportsIncludingRelated))
			{
				form.Show();
				AssertNotNull(form.Grid);
				AssertEquals("Should be read-only due to phase security", true, form.Grid.ReadOnly);
			}
		}

		public void TestTransportsGrid_ReadOnlyDueToPhaseSecurity_WithAttachedShipment()
		{
			var consol = Factory.New<CommonConsolForTest>();
			consol.Shipments.AddNew();
			consol.PropertiesForcedToReadOnlyDueToPhase.Add("Routing");
			Factory.Save();

			using (var form = new RoutingPluginControlTestForm(((IRoutingSupport)consol).TransportsIncludingRelated))
			{
				form.Show();
				AssertNotNull(form.Grid);
				AssertEquals("Should be read-only due to phase security, shipment should not override this", true, form.Grid.ReadOnly);

				form.Validate();

				AssertEquals("Should stay read-only after validation", true, form.Grid.ReadOnly);
			}
		}

		public void TestTransportsGrid_CO2e()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(false))
			using (var form = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
			{
				form.Show();
				var cO2eColumn = form.Grid.Columns.FirstOrDefault(x => x.ColumnName == "TotalCO2eForSorting");
				AssertNull(cO2eColumn);
			}

			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (var form = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
			{
				form.Show();
				AssertGridColumn(form.Grid, "TotalCO2eForSorting", false);
			}
		}

		void AssertGridColumn(ZGrid grid, string columnName, bool isVisible)
		{
			var column = grid.Columns.FirstOrDefault(x => x.ColumnName == columnName);
			AssertNotNull(column);
			AssertEquals(columnName + ": IsVisible", isVisible, column.IsVisible);
		}

		class CommonConsolForTest : CommonConsol
		{
			public CommonConsolForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override bool IsPropertyReadOnlyDueToPhaseCore(ZString propertyName)
			{
				return base.IsPropertyReadOnlyDueToPhaseCore(propertyName) || PropertiesForcedToReadOnlyDueToPhase.Contains(propertyName);
			}

			public List<string> PropertiesForcedToReadOnlyDueToPhase = new List<string>();
		}

		#endregion

		#region Vessel Movements Button

		public void TestVesselMovements_Button_OnlyVisibleToSupportUser_When_RegistryDisabled()
		{
			using (FreightDataRegistry.Instance.EnableRouteVisualizer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				using (var form = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
				{
					form.Show();
					AssertEquals("Show Map Button should always be visible to support user", true, form.control.ShowMapButton.Visible);
				}

				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "SS1";
				staff.GS_LoginName = "SS1";
				Factory.Save();

				using (EnvProxy.Instance.SetTemporaryUserContext("SS1", GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				using (var form = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
				{
					form.Show();
					AssertEquals("Show Map Button should not be visible to ordinary user when registry is disabled", false, form.control.ShowMapButton.Visible);
				}
			}
		}

		public void TestVesselMovements_Button_VisibleToAllUsers_When_RegistryEnabled()
		{
			using (FreightDataRegistry.Instance.EnableRouteVisualizer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				using (var form = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
				{
					form.Show();
					AssertEquals("Show Map Button should always be visible to support user", true, form.control.ShowMapButton.Visible);
				}

				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "SS1";
				staff.GS_LoginName = "SS1";
				Factory.Save();

				using (EnvProxy.Instance.SetTemporaryUserContext("SS1", GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				using (var form = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
				{
					form.Show();
					AssertEquals("Show Map Button should be visible to ordinary user when enabled via registry", true, form.control.ShowMapButton.Visible);
				}
			}
		}

		public void TestVesselMovements_Click_ErrorMessageShown_When_NoAuthTokenCanBeObtained()
		{
			SetUpTransportForVesselMovements();

			using (RegisterMockAuthTokenProviderToReturn(validationMessage: "unable to connect to the authentication service, try again later"))
			using (var form = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessages();

				form.control.ShowMapButton.PerformClick();
				CombineAssertions("When an auth token cannot be obtained, the Show Map Button should show an error message", () =>
				{
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals("Unable to show map: Unable to get permission to show map: unable to connect to the authentication service, try again later", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestVesselMovements_Click_ErrorMessageShown_When_InvalidData()
		{
			var transport = Support.TransportsIncludingRelated[0];
			transport.JW_TransportMode = "AIR";

			using (RegisterMockAuthTokenProviderToReturn(authToken: "MockAuthToken"))
			using (var form = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessages();

				form.control.ShowMapButton.PerformClick();
				CombineAssertions("When data is invalid, the Show Map Button should show an error message", () =>
				{
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals("Unable to show map: Routing leg Transport Mode must be SEA.", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestVesselMovements_Click_WebBrowserOpened()
		{
			SetUpTransportForVesselMovements();

			using (FreightDataRegistry.Instance.EnableRouteVisualizerMyAccountLogin.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (RegisterMockMyAccountTermsChecker(true))
			using (RegisterMockAuthTokenProviderToReturn(authToken: "MockAuthToken"))
			using (var form = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessages();
				WebUrlLauncher.ClearLastUrlLaunched();

				form.control.ShowMapButton.PerformClick();
				CombineAssertions("When leg is valid, the Show Map Button should load the URL in web browser", () =>
				{
					Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
					var expectedUrl = "https://vt.wisegrid.net/movements/1234567?token=MockAuthToken&departureTime=20191028T020409&arrivalTime=20191031T020409";
					AssertEquals(expectedUrl, WebUrlLauncher.LastUrlLaunched);
				});
			}
		}

		public void TestVesselMovements_Click_WebBrowserOpenedWithMyAccountAuth()
		{
			SetUpTransportForVesselMovements();

			using (FreightDataRegistry.Instance.EnableRouteVisualizerMyAccountLogin.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (RegisterMockMyAccountTermsChecker(true))
			using (RegisterMockUserPortalClient(true))
			using (var form = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessages();
				WebUrlLauncher.ClearLastUrlLaunched();

				form.control.ShowMapButton.PerformClick();
				CombineAssertions("When leg is valid, the Show Map Button should load the URL in web browser", () =>
				{
					Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
					var expectedUrl = "https://vt.wisegrid.net/movements/1234567?myaccount_token=123456&departureTime=20191028T020409&arrivalTime=20191031T020409";
					AssertEquals(expectedUrl, WebUrlLauncher.LastUrlLaunched);
				});
			}
		}

		public void TestVesselMovements_Click_ErrorMessageShown_WhenMyAccountTermsDenied()
		{
			using (FreightDataRegistry.Instance.EnableRouteVisualizerMyAccountLogin.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (RegisterMockMyAccountTermsChecker(false))
			using (var form = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessages();
				WebUrlLauncher.ClearLastUrlLaunched();

				form.control.ShowMapButton.PerformClick();
				CombineAssertions("When My Account Usage Terms denied, the Show Map Button should show an error message", () =>
				{
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals("Unable to show map: My Account terms of use have not been accepted.", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		void SetUpTransportForVesselMovements()
		{
			var transport = Support.TransportsIncludingRelated[0];
			transport.JW_TransportMode = "SEA";
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "1234567";
			transport.JW_Vessel = vessel.RV_FK;
			transport.JW_ATD = new ZDateTime(2019, 10, 28, 2, 4, 9);
			transport.JW_ATA = new ZDateTime(2019, 10, 31, 2, 4, 9);
		}

		IDisposable RegisterMockMyAccountTermsChecker(bool isSuccess)
		{
			var agreementCheckerMock = new Mock<ISystemUserAccountCollectionTermChecker>(MockBehavior.Loose);
			agreementCheckerMock.Setup(s => s.CheckTermAcknowledged()).Returns(Task.FromResult(isSuccess));

			return ObjectFactory.Substitute(agreementCheckerMock.Object);
		}

		IDisposable RegisterMockAuthTokenProviderToReturn(string authToken = null, string validationMessage = "")
		{
			var token = (authToken, validationMessage);
			var authProviderMock = new Mock<IAuthTokenProvider>(MockBehavior.Loose);
			authProviderMock.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<int>(), It.IsAny<Action<LoginInfo>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>())).Returns(token);

			return ObjectFactory.Substitute(authProviderMock.Object);
		}

		IDisposable RegisterMockUserPortalClient(bool isSuccess)
		{
			var userPortalClientMock = new Mock<IUserPortalClient>(MockBehavior.Loose);
			var response = isSuccess
				? new TrustedResponse<OAuthLoginResponse>(true, "{\"url\":\"https://vt.wisegrid.net?token=123456\",\"token\":\"123456\"}")
				: new TrustedResponse<OAuthLoginResponse>("2000", "something went wrong");
			userPortalClientMock.Setup(s => s.OAuthAutoLoginAsync(It.IsAny<Uri>())).Returns(Task.FromResult(response));
			return ObjectFactory.Substitute(userPortalClientMock.Object);
		}
		#endregion

		public void TestDeleting()
		{
			var expectedMessage = "Error Cannot delete read-only rows.";

			var transport1 = Support.Transports[0];
			var transport2 = Support.Transports.AddNew();
			var transport3 = Support.Transports.AddNew();

			using (RoutingPluginControlTestForm form = new RoutingPluginControlTestForm(Support.TransportsIncludingRelated))
			{
				form.Show();
				var deleteMenuItem = form.Grid.ContextMenu.MenuItems.FindByText("&Delete");
				AssertEquals(3, form.Grid.ListManager.Count);

				transport1.MakeNonPersistent();
				transport1.ReadOnly = true;

				transport2.MakeNonPersistent();
				transport2.ReadOnly = false;

				var index1 = GetRowIndex(form.Grid, transport1);
				var index2 = GetRowIndex(form.Grid, transport2);
				var index3 = GetRowIndex(form.Grid, transport3);

				var hit = form.Grid.HitTest(form.Grid.RowHeaderWidth - 1, form.Grid.PreferredRowHeight + 10);
				typeof(ZGrid).GetField("MouseUpInfo", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(form.Grid, hit);

				form.Grid.Select(index1);
				AssertEquals("Selected Transport 1", true, form.Grid.IsSelected(index1));
				AssertEquals("Selected Transport 2", false, form.Grid.IsSelected(index2));
				AssertEquals("Selected Transport 3", false, form.Grid.IsSelected(index3));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				deleteMenuItem.PerformClick();
				AssertEquals(3, form.Grid.ListManager.Count);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.ToString());

				form.Grid.Select(index2);
				AssertEquals("Selected Transport 1", true, form.Grid.IsSelected(index1));
				AssertEquals("Selected Transport 2", true, form.Grid.IsSelected(index2));
				AssertEquals("Selected Transport 3", false, form.Grid.IsSelected(index3));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				deleteMenuItem.PerformClick();
				AssertEquals(2, form.Grid.ListManager.Count);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.ToString());

				index1 = GetRowIndex(form.Grid, transport1);
				index3 = GetRowIndex(form.Grid, transport3);
				form.Grid.UnSelectAll();
				form.Grid.Select(index3);
				AssertEquals("Selected Transport 1", false, form.Grid.IsSelected(index1));
				AssertEquals("Selected Transport 3", true, form.Grid.IsSelected(index3));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				deleteMenuItem.PerformClick();
				AssertEquals(1, form.Grid.ListManager.Count);
				AssertEquals("None ", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals(transport1, form.Grid.ListManager.GetCurrent());
			}
		}

		int GetRowIndex(ZGrid grid, Transport transport)
		{
			for (int i = 0; i < grid.VisibleRowCount; i++)
			{
				grid.CurrentRowIndex = i;
				if (grid.ListManager.GetCurrent() == transport)
				{
					return i;
				}
			}

			return -1;
		}

		[ExpectNoExceptions]
		public void TestColourCodeNullReferenceOnRoutingCollection()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var transport = consol.Transports.AddNew();

			var collection = ((IRoutingSupport)consol).TransportsIncludingRelated;

			using (var form = new RoutingPluginControlTestForm(collection))
			{
				form.Show();
				form.Disposed += (s, e) => GetGridRowColour(form.Grid, transport);
			}
		}

		Color GetGridRowColour(ZGrid grid, object objectAtRow)
		{
			var handler = (EventHandler<ColourDecidingEventArgs>)typeof(ZGrid).GetField("ColourDeciding", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(grid);
			var e = new ColourDecidingEventArgs(objectAtRow);
			handler(null, e);
			return e.Colour;
		}

		public void TestDisableSheduleAndFlightButtons()
		{
			using (var control = new TestRoutingPluginControl())
			{
				control.DisableScheduleAndFlightButtonsWithMessage();

				CheckButtonWithMessage(control.SelectScheduleButton, "Error You can only view Schedules via the parent");
				CheckButtonWithMessage(control.GlobalSchedulesButton, "Error You can only import Global Flight Schedules via the parent");
				CheckButtonWithMessage(control.ImportGlobalScheduleButton, "Error You can only import Global Sailing Schedules via the parent");
			}
		}

		static void CheckButtonWithMessage(Button button, string message)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			button.PerformClick();
			AssertEquals("Reason", message, UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		#region Details and Actual Tab Pages

		public void TestDetailsAndActualTabPages()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			var transport = consol.Transports.AddNew();
			var collection1 = ((IRoutingSupport)consol).TransportsIncludingRelated;

			AssertDetailsAndActualTabPages(collection1, true);

			var shipment = consol.Shipments.AddNew();
			var collection2 = ((IRoutingSupport)shipment).TransportsIncludingRelated;

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertDetailsAndActualTabPages(collection2, false);

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertDetailsAndActualTabPages(collection1, false);

			void AssertDetailsAndActualTabPages(RoutingCollection routingCollection, bool expectedActualPageVisible)
			{
				using (var form = new RoutingPluginControlTestForm(routingCollection))
				{
					form.Show();

					var detailsPage = form.Controls.Find("DetailsTabPage", true).FirstOrDefault() as ZTabPage;
					AssertNotNull(detailsPage);
					AssertEquals(true, detailsPage.TabVisible);

					var actualPage = form.Controls.Find("ActualTabPage", true).FirstOrDefault() as ZTabPage;
					if (expectedActualPageVisible)
					{
						AssertNotNull(actualPage);
					}
					else
					{
						AssertNull(actualPage);
					}
				}
			}
		}

		public void TestUpdateRoutingLeg_ForOneRowSelection()
		{
			var consol = Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			var transport = consol.Transports_AddNew();
			var collection1 = ((IRoutingSupport)consol).TransportsIncludingRelated;

			using (var form = new RoutingPluginControlTestForm(collection1))
			{
				form.Show();

				var bottomTab = form.Controls.Find("BottomTabControl", true).FirstOrDefault() as ZTabControl;
				AssertNotNull(bottomTab);

				var actualPage = form.Controls.Find("ActualTabPage", true).FirstOrDefault() as ZTabPage;
				AssertNotNull(actualPage);

				bottomTab.SelectTab(actualPage);

				AssertUpdateRoutingLeg_ForOneRowSelection(actualPage, "BookingConfirmationUpdateButton", "BookingConfirmationGrid", "Please select an item from Booking Confirmation grid.");
				AssertUpdateRoutingLeg_ForOneRowSelection(actualPage, "ActualEventsUpdateButton", "ActualEventsGrid", "Please select an item from Actual Events grid.");
			}

			#region AssertUpdateRoutingLeg_ForOneRowSelection

			void AssertUpdateRoutingLeg_ForOneRowSelection(ZTabPage actualPage, string buttonName, string gridName, string expectedErrorMessage)
			{
				var updateButton = actualPage.Controls.Find(buttonName, true).FirstOrDefault() as ZButton;
				AssertNotNull(updateButton);

				var grid = actualPage.Controls.Find(gridName, true).FirstOrDefault() as ZGrid;
				AssertNotNull(grid);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				updateButton.PerformClick();
				AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			#endregion
		}

		public void TestUpdateRoutingLeg_ForBookingConfirmationRow()
		{
			var consol = Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var bookingConfirmation = (consol as CommonConsol).BookingConfirmations.AddNew();
			bookingConfirmation.LoadPort = "AUSYD";
			bookingConfirmation.DischargePort = "HKHKG";
			bookingConfirmation.VoyageFlight = "QF1534";
			bookingConfirmation.BookedPieces = 20;
			bookingConfirmation.DepartureTime = new ZDateTime(2023, 5, 16, 9, 30, 0);
			bookingConfirmation.ArrivalTime = new ZDateTime(2023, 5, 16, 20, 45, 0);

			var transport = consol.Transports_AddNew() as Transport;
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "HKHKG";
			transport.JW_TransportType = Constants.TransportPlanningType.Flight1;
			transport.JW_VoyageFlight = "QF1533";
			transport.JW_ETD = new ZDateTime(2023, 5, 16, 9, 0, 0);
			transport.JW_ETA = new ZDateTime(2023, 5, 16, 20, 0, 0);

			var collection1 = ((IRoutingSupport)consol).TransportsIncludingRelated;

			using (var form = new RoutingPluginControlTestForm(collection1))
			{
				form.Show();

				var bottomTab = form.Controls.Find("BottomTabControl", true).FirstOrDefault() as ZTabControl;
				AssertNotNull(bottomTab);

				var actualPage = form.Controls.Find("ActualTabPage", true).FirstOrDefault() as ZTabPage;
				AssertNotNull(actualPage);

				bottomTab.SelectTab(actualPage);

				var updateButton = actualPage.Controls.Find("BookingConfirmationUpdateButton", true).FirstOrDefault() as ZButton;
				AssertNotNull(updateButton);

				var grid = actualPage.Controls.Find("BookingConfirmationGrid", true).FirstOrDefault() as ZGrid;
				AssertNotNull(grid);
				grid.Select(0);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				updateButton.PerformClick();
				var expectedConfirmation = "Are you sure you want to update/replace the routing leg with the selected Booking Confirmation leg details for the same Load Port/Discharge?";
				AssertEquals(expectedConfirmation, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No update", "QF1533", transport.JW_VoyageFlight);
				AssertEquals("No update", new ZDateTime(2023, 5, 16, 9, 0, 0), transport.JW_STD);
				AssertEquals("No update", new ZDateTime(2023, 5, 16, 20, 0, 0), transport.JW_STA);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				updateButton.PerformClick();
				AssertEquals(expectedConfirmation, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("QF1534", transport.JW_VoyageFlight);
				AssertEquals(new ZDateTime(2023, 5, 16, 9, 30, 0), transport.JW_STD);
				AssertEquals(new ZDateTime(2023, 5, 16, 20, 45, 0), transport.JW_STA);

				transport.JW_RL_NKDiscPort = "SGSIN";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				updateButton.PerformClick();
				var expectedMessage = "There is no matching planned leg for the selected event (based on Load Port/Discharge). Please review the selected event and manually update any planned routing leg(s) if required.";
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Update Routing Leg", UnitTestUserNotification.Instance.LastMessage.Caption);
			}
		}

		public void TestUpdateRoutingLeg_ForActualEventRow()
		{
			var consol = Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var actualEvent = (consol as CommonConsol).ActualEvents.AddNew();
			actualEvent.EventCode = Events.ArrivalCode;
			actualEvent.LoadPort = "AUSYD";
			actualEvent.DischargePort = "HKHKG";
			actualEvent.VoyageFlight = "QF1534";
			actualEvent.ShippedPieces = 20;
			actualEvent.DepartedTime = new ZDateTime(2023, 5, 16, 9, 30, 0);
			actualEvent.ArrivedPieces = 20;
			actualEvent.ArrivedTime = new ZDateTime(2023, 5, 16, 20, 45, 0);
			actualEvent.IsPartial = false;

			var transport = consol.Transports_AddNew() as Transport;
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "HKHKG";
			transport.JW_TransportType = Constants.TransportPlanningType.Flight1;
			transport.JW_VoyageFlight = "QF1533";
			transport.JW_ETD = new ZDateTime(2023, 5, 16, 9, 0, 0);
			transport.JW_ETA = new ZDateTime(2023, 5, 16, 20, 0, 0);

			var collection1 = ((IRoutingSupport)consol).TransportsIncludingRelated;

			using (var form = new RoutingPluginControlTestForm(collection1))
			{
				form.Show();

				var bottomTab = form.Controls.Find("BottomTabControl", true).FirstOrDefault() as ZTabControl;
				AssertNotNull(bottomTab);

				var actualPage = form.Controls.Find("ActualTabPage", true).FirstOrDefault() as ZTabPage;
				AssertNotNull(actualPage);

				bottomTab.SelectTab(actualPage);

				var updateButton = actualPage.Controls.Find("ActualEventsUpdateButton", true).FirstOrDefault() as ZButton;
				AssertNotNull(updateButton);

				var grid = actualPage.Controls.Find("ActualEventsGrid", true).FirstOrDefault() as ZGrid;
				AssertNotNull(grid);
				grid.Select(0);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				updateButton.PerformClick();
				var expectedConfirmation = "Are you sure you want to update/replace the routing leg with the selected Actual Event leg details for the same Load Port/Discharge?";
				AssertEquals(expectedConfirmation, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No update", "QF1533", transport.JW_VoyageFlight);
				AssertEquals("No update", ZDateTime.Empty, transport.JW_ATD);
				AssertEquals("No update", ZDateTime.Empty, transport.JW_ATA);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				updateButton.PerformClick();
				AssertEquals(expectedConfirmation, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("QF1534", transport.JW_VoyageFlight);
				AssertEquals(new ZDateTime(2023, 5, 16, 9, 30, 0), transport.JW_ATD);
				AssertEquals(new ZDateTime(2023, 5, 16, 20, 45, 0), transport.JW_ATA);

				transport.JW_RL_NKDiscPort = "SGSIN";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				updateButton.PerformClick();
				var expectedMessage = "There is no matching planned leg for the selected event (based on Load Port/Discharge). Please review the selected event and manually update any planned routing leg(s) if required.";
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Update Routing Leg", UnitTestUserNotification.Instance.LastMessage.Caption);
			}
		}

		#endregion

		#region Consol

		public IRoutingSupport Support
		{
			get
			{
				if (support == null)
				{
					support = Factory.New<CommonConsol>();
				}

				return support;
			}
		}
		IRoutingSupport support;

		#endregion

		#region TestFirstRoutingLegGenerates_WhenClickImportGlobalFlightsButton

		public void TestFirstRoutingLegGenerates_WhenClickImportGlobalFlightsButton()
		{
			var parent = Factory.New<IDtbBookingConsolidation>();
			var parentCore = (ITransportParentCore)parent;
			var transportCollection = new TransportCollection(parentCore);
			var transport = transportCollection.AddNew();
			transportCollection.RemoveAndDeleteAll();
			AssertEquals(0, transportCollection.Count);

			using (var form = new RoutingPluginControlTestForm_TransportBooking(transportCollection))
			{
				form.Show();
				form.control.GlobalSchedulesButton.PerformClick();
				AssertEquals(1, transportCollection.Count);
			}
		}

		#endregion
	}
}

namespace Enterprise.Freight.GUI
{
	internal class TestRoutingPluginControl : RoutingPluginControl
	{
		protected override void SetupTestAttributes()
		{
			TypeDescriptor.AddAttributes(JW_VesselBoundTextBox, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(JW_VoyageFlightBoundTextBox, new SuppressFormsLocalizedTestAttribute());
		}

		protected override void SetLastShownFormForTest(RealTimeRoutingForm form)
		{
			LastShownFormForTest = form;
		}

		internal RealTimeRoutingForm LastShownFormForTest
		{
			get;
			set;
		}
	}
}
