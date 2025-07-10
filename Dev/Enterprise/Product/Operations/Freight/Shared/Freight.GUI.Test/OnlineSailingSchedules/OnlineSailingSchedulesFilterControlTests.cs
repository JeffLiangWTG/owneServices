using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.OnlineSailingSchedules;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using ServiceModel = Enterprise.Freight.OnlineSailingSchedules.ServiceModel;

namespace Enterprise.Freight.GUI.OnlineSailingSchedules.Testing
{
	sealed class OnlineSailingSchedulesFilterControlTests : ZFilterStripControlTest
	{
		public void TestSelectSameFiltersMultipleTimes()
		{
			var factory = new BusinessObjectFactory();
			var routesProvider = new RoutesProvider(factory);
			var onlineSchedules = new OnlineSchedules(factory, routesProvider);

			using (var form = new OnlineSchedulesFormForTest(onlineSchedules))
			{
				form.Show();
				var filterControl = form.FilterControl;
				filterControl.AddFilterStrip(filterControl.FilterBusinessObject.FilterStrips[0]);
				filterControl.AddFilterStrip(filterControl.FilterBusinessObject.FilterStrips[2]);

				form.FilterControl.Find();

				var errorMessage = "A filter can be selected only once. The following filter(s) have been selected multiple times: Origin, Departure Date.";

				AssertEquals(errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		public void TestDisableFilterCategory()
		{
			var factory = new BusinessObjectFactory();
			var routesProvider = new RoutesProvider(factory);
			var onlineSchedules = new OnlineSchedules(factory, routesProvider);

			using (var form = new OnlineSchedulesFormForTest(onlineSchedules))
			{
				form.Show();
				var filterControl = form.FilterControl;
				filterControl.AddNewFilterStrip();

				var values = filterControl.Strips.Select(s => s.IsFilterCategoriesEnabled).ToArray();
				Assert("All filterstrips should have IsFilterCategoriesEnabled false. Actual: " + string.Join(", ", values), !values.Contains(true));
			}
		}

		public void TestNewZFilterStrip()
		{
			var factory = new BusinessObjectFactory();
			var routesProvider = new RoutesProvider(factory);
			var onlineSchedules = new OnlineSchedules(factory, routesProvider);

			using (var form = new OnlineSchedulesFormForTest(onlineSchedules))
			{
				form.Show();
				var filterControl = form.FilterControl;
				filterControl.AddNewFilterStrip();

				var hasCustomFilterStrip = filterControl.Strips.OfType<OnlineSailingSchedulesFilterStrip>().Any();
				Assert("Online Sailing Schedules filter contains custom filter strip", hasCustomFilterStrip);
			}
		}

		public void TestRoutesColumnStyles()
		{
			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object);

			using (var form = new OnlineSchedulesFormForTest(onlineSchedules))
			{
				var filterControl = form.FilterControl;
				form.Show();
				AssertEquals("Origin", filterControl.FilteredGrid.GetColumnStyle(Route.Schema.OriginPortUnloco).CaptionResourceString.Caption);
				Assert(filterControl.FilteredGrid.GetColumnStyle(Route.Schema.OriginPortUnloco).IsVisible);
				AssertEquals("Origin Port Name", filterControl.FilteredGrid.GetColumnStyle(Route.Schema.OriginPortName).CaptionResourceString.Caption);
				Assert(filterControl.FilteredGrid.GetColumnStyle(Route.Schema.OriginPortName).IsVisible);
				AssertEquals("Destination", filterControl.FilteredGrid.GetColumnStyle(Route.Schema.DestinationPortUnloco).CaptionResourceString.Caption);
				Assert(filterControl.FilteredGrid.GetColumnStyle(Route.Schema.DestinationPortUnloco).IsVisible);
				AssertEquals("Destination Port Name", filterControl.FilteredGrid.GetColumnStyle(Route.Schema.DestinationPortName).CaptionResourceString.Caption);
				Assert(filterControl.FilteredGrid.GetColumnStyle(Route.Schema.DestinationPortName).IsVisible);
				AssertEquals("Departure", filterControl.FilteredGrid.GetColumnStyle(Route.Schema.Departure).CaptionResourceString.Caption);
				Assert(filterControl.FilteredGrid.GetColumnStyle(Route.Schema.Departure).IsVisible);
				AssertEquals("Arrival", filterControl.FilteredGrid.GetColumnStyle(Route.Schema.Arrival).CaptionResourceString.Caption);
				Assert(filterControl.FilteredGrid.GetColumnStyle(Route.Schema.Arrival).IsVisible);
				AssertEquals("Carrier SCAC", filterControl.FilteredGrid.GetColumnStyle(Route.Schema.CarrierSCAC).CaptionResourceString.Caption);
				Assert(filterControl.FilteredGrid.GetColumnStyle(Route.Schema.CarrierSCAC).IsVisible);
				AssertEquals("Carrier Code", filterControl.FilteredGrid.GetColumnStyle(Route.Schema.CarrierCode).CaptionResourceString.Caption);
				Assert(filterControl.FilteredGrid.GetColumnStyle(Route.Schema.CarrierCode).IsVisible);
				AssertEquals("Legs Count", filterControl.FilteredGrid.GetColumnStyle(Route.Schema.LegsCount).CaptionResourceString.Caption);
				Assert(filterControl.FilteredGrid.GetColumnStyle(Route.Schema.LegsCount).IsVisible);
				AssertEquals("Transit Time", filterControl.FilteredGrid.GetColumnStyle(Route.Schema.TransitTime).CaptionResourceString.Caption);
				Assert(filterControl.FilteredGrid.GetColumnStyle(Route.Schema.TransitTime).IsVisible);
				AssertEquals("Is Gateway/Transhipment", filterControl.FilteredGrid.GetColumnStyle(Route.Schema.IsGateway).CaptionResourceString.Caption);
				Assert(filterControl.FilteredGrid.GetColumnStyle(Route.Schema.IsGateway).IsVisible);
			}
		}

		public void TestRoutesColumnStyles_Co2ColumnsExistWhenGSSEmissionsCalculationEnabledInRegistry()
		{
			using (FreightDataRegistry.Instance.EnableGlobalSailingSchedulesCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
				var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object);

				using (var form = new OnlineSchedulesFormForTest(onlineSchedules))
				{
					var filterControl = form.FilterControl;
					form.Show();
					AssertEquals("CO2e (kg/TEU)", filterControl.FilteredGrid.GetColumnStyle(nameof(Route.Co2eKgPerTeu)).CaptionResourceString.Caption);
					Assert(!filterControl.FilteredGrid.GetColumnStyle(nameof(Route.Co2eKgPerTeu)).IsVisible);
					AssertEquals("CO2e (kg/t)", filterControl.FilteredGrid.GetColumnStyle(nameof(Route.Co2eKgPerTonne)).CaptionResourceString.Caption);
					Assert(!filterControl.FilteredGrid.GetColumnStyle(nameof(Route.Co2eKgPerTonne)).IsVisible);
				}
			}
		}

		public void TestRoutesColumnStyles_Co2ColumnsDoNotExistWhenGSSEmissionsCalculationDisabledInRegistry()
		{
			using (FreightDataRegistry.Instance.EnableGlobalSailingSchedulesCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
				var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object);

				using (var form = new OnlineSchedulesFormForTest(onlineSchedules))
				{
					var filterControl = form.FilterControl;
					form.Show();
					AssertNull(filterControl.FilteredGrid.GetColumnStyle(nameof(Route.Co2eKgPerTeu)));
					AssertNull(filterControl.FilteredGrid.GetColumnStyle(nameof(Route.Co2eKgPerTonne)));
				}
			}
		}

		public void TestLegsColumnStyles()
		{
			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object);

			using (var form = new OnlineSchedulesFormForTest(onlineSchedules))
			{
				var filterControl = form.FilterControl;
				form.Show();
				AssertEquals("Origin", filterControl.LegsGrid.GetColumnStyle(Leg.Schema.OriginPortUnloco).CaptionResourceString.Caption);
				Assert(filterControl.LegsGrid.GetColumnStyle(Leg.Schema.OriginPortUnloco).IsVisible);
				AssertEquals("Origin Port Name", filterControl.LegsGrid.GetColumnStyle(Leg.Schema.OriginPortName).CaptionResourceString.Caption);
				Assert(filterControl.LegsGrid.GetColumnStyle(Leg.Schema.OriginPortName).IsVisible);
				AssertEquals("Destination", filterControl.LegsGrid.GetColumnStyle(Leg.Schema.DestinationPortUnloco).CaptionResourceString.Caption);
				Assert(filterControl.LegsGrid.GetColumnStyle(Leg.Schema.DestinationPortUnloco).IsVisible);
				AssertEquals("Destination Port Name", filterControl.LegsGrid.GetColumnStyle(Leg.Schema.DestinationPortName).CaptionResourceString.Caption);
				Assert(filterControl.LegsGrid.GetColumnStyle(Leg.Schema.DestinationPortName).IsVisible);
				AssertEquals("Departure", filterControl.LegsGrid.GetColumnStyle(Leg.Schema.Departure).CaptionResourceString.Caption);
				Assert(filterControl.LegsGrid.GetColumnStyle(Leg.Schema.Departure).IsVisible);
				AssertEquals("Arrival", filterControl.LegsGrid.GetColumnStyle(Leg.Schema.Arrival).CaptionResourceString.Caption);
				Assert(filterControl.LegsGrid.GetColumnStyle(Leg.Schema.Arrival).IsVisible);
				AssertEquals("Voyage Code", filterControl.LegsGrid.GetColumnStyle(Leg.Schema.VoyageCode).CaptionResourceString.Caption);
				Assert(filterControl.LegsGrid.GetColumnStyle(Leg.Schema.VoyageCode).IsVisible);
				AssertEquals("Service String", filterControl.LegsGrid.GetColumnStyle(Leg.Schema.TradeLaneName).CaptionResourceString.Caption);
				Assert(filterControl.LegsGrid.GetColumnStyle(Leg.Schema.TradeLaneName).IsVisible);
				AssertEquals("Vessel Name", filterControl.LegsGrid.GetColumnStyle(Leg.Schema.VesselName).CaptionResourceString.Caption);
				Assert(filterControl.LegsGrid.GetColumnStyle(Leg.Schema.VesselName).IsVisible);
				AssertEquals("IMO Number", filterControl.LegsGrid.GetColumnStyle(Leg.Schema.LloydsNumber).CaptionResourceString.Caption);
				Assert(filterControl.LegsGrid.GetColumnStyle(Leg.Schema.LloydsNumber).IsVisible);
				AssertEquals("Carrier SCAC", filterControl.LegsGrid.GetColumnStyle(Leg.Schema.CarrierSCAC).CaptionResourceString.Caption);
				Assert(filterControl.LegsGrid.GetColumnStyle(Leg.Schema.CarrierSCAC).IsVisible);
				AssertEquals("Carrier Code", filterControl.LegsGrid.GetColumnStyle(Leg.Schema.CarrierCode).CaptionResourceString.Caption);
				Assert(filterControl.LegsGrid.GetColumnStyle(Leg.Schema.CarrierCode).IsVisible);
			}
		}

		public void TestLegsColumnStyles_Co2ColumnsExistWhenGSSEmissionsCalculationEnabledInRegistry()
		{
			using (FreightDataRegistry.Instance.EnableGlobalSailingSchedulesCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
				var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object);

				using (var form = new OnlineSchedulesFormForTest(onlineSchedules))
				{
					var filterControl = form.FilterControl;
					form.Show();
					AssertEquals("CO2e (kg/TEU)", filterControl.LegsGrid.GetColumnStyle(nameof(Leg.Co2eKgPerTeu)).CaptionResourceString.Caption);
					Assert(!filterControl.LegsGrid.GetColumnStyle(nameof(Leg.Co2eKgPerTeu)).IsVisible);
					AssertEquals("CO2e (kg/t)", filterControl.LegsGrid.GetColumnStyle(nameof(Leg.Co2eKgPerTonne)).CaptionResourceString.Caption);
					Assert(!filterControl.LegsGrid.GetColumnStyle(nameof(Leg.Co2eKgPerTonne)).IsVisible);
				}
			}
		}

		public void TestLegsColumnStyles_Co2ColumnsDoNotExistWhenGSSEmissionsCalculationDisabledInRegistry()
		{
			using (FreightDataRegistry.Instance.EnableGlobalSailingSchedulesCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
				var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object);

				using (var form = new OnlineSchedulesFormForTest(onlineSchedules))
				{
					var filterControl = form.FilterControl;
					form.Show();
					AssertNull(filterControl.LegsGrid.GetColumnStyle(nameof(Leg.Co2eKgPerTeu)));
					AssertNull(filterControl.LegsGrid.GetColumnStyle(nameof(Leg.Co2eKgPerTonne)));
				}
			}
		}

		public void TestLegsGridBound()
		{
			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object);

			using (var form = new OnlineSchedulesFormForTest(onlineSchedules))
			{
				var filterControl = form.FilterControl;
				form.Show();
				AssertNotNull(filterControl.LegsGrid.DataSource);
			}
		}

		public void TestShouldShowErrorsInGrids()
		{
			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object);

			using (var form = new OnlineSchedulesFormForTest(onlineSchedules))
			{
				var filterControl = form.FilterControl;
				form.Show();

				var routesGrid = filterControl.FilteredGrid;
				var legsGrid = filterControl.LegsGrid;

				Assert((bool)routesGrid.GetType().GetProperty("ShouldShowNotifications", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(routesGrid, null));
				Assert((bool)legsGrid.GetType().GetProperty("ShouldShowNotifications", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(legsGrid, null));
			}
		}

		public void TestLegsGridSpecificMenuItemsEnabled()
		{
			#region Create Routes

			const string vesselName1 = "TmpNameABC";
			const string vesselName2 = "TmpNameDEF";
			const string imoNumber1 = "9308390";
			const string imoNumber2 = "9463085";

			var carrier = new ServiceModel.Carrier
			{
				Code = "CMAC",
				Name = "CMA CGM"
			};

			var voyage1 = new ServiceModel.Voyage
			{
				Code = "754N",
				TradeLane = new ServiceModel.TradeLane { Name = "AAA" },
				Operator = new ServiceModel.Carrier { Code = "CMAC", Name = "CMA CGM" },
				Vessel = new ServiceModel.Vessel { VesselName = vesselName1, ImoNumber = imoNumber1 }
			};

			var leg1 = new ServiceModel.Leg
			{
				LoadPort = new ServiceModel.Port { Unloco = "AUSYD" },
				DischargePort = new ServiceModel.Port { Unloco = "AUBNE" },
				Etd = new DateTime(2016, 8, 10),
				Eta = new DateTime(2016, 8, 15),
				Voyage = voyage1
			};

			var voyage2 = new ServiceModel.Voyage
			{
				Code = "758N",
				TradeLane = new ServiceModel.TradeLane { Name = "AAA" },
				Operator = new ServiceModel.Carrier { Code = "NONA", Name = "NONA ABC" },
				Vessel = new ServiceModel.Vessel { VesselName = vesselName2, ImoNumber = imoNumber2 }
			};

			var leg2 = new ServiceModel.Leg
			{
				LoadPort = new ServiceModel.Port { Unloco = "AUSYD" },
				DischargePort = new ServiceModel.Port { Unloco = "AUBNE" },
				Etd = new DateTime(2016, 8, 16),
				Eta = new DateTime(2016, 8, 21),
				Voyage = voyage2
			};

			var serviceRoute = new ServiceModel.Route { Carrier = carrier, Legs = new[] { leg1, leg2 } };

			#endregion

			var todaysDate = DateTime.Today;

			var filterRequest = new OnlineSchedulesFilterRequest
			{
				LoadPort = "AUSYD",
				DischargePort = "AUBNE",
				EtdFrom = todaysDate.ToString("yyyy-MM-dd"),
				EtaFrom = todaysDate.ToString("yyyy-MM-dd"),
				IncludeRelatedPorts = true.ToString(),
				SameCarrierRoutes = false.ToString()
			};

			var urlParams = UrlHelper.ConvertToParams(filterRequest);
			var route = new Route(Factory);
			route.SetValues(serviceRoute);

			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);

			routesProvider.Setup(m => m.GetRoutes(urlParams, It.IsAny<UserInitiatedServiceRequestManager>()))
				.Returns(new[] { route });

			var vessel1 = Factory.New<RefVessel>();
			vessel1.RV_Name = vesselName1;
			vessel1.RV_LloydsNumber = "9071208";
			vessel1.RV_IsActive = true;

			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Name = "TmpNameKKK";
			vessel2.RV_LloydsNumber = imoNumber2;
			vessel2.RV_IsActive = true;

			var entCarrier = Factory.NewWithValidTestData<OrgHeader>();
			entCarrier.OH_Code = "CMACGM";
			var cusCode = entCarrier.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = "CMAC";
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			Factory.Save();

			var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object) { Request = filterRequest };

			using (var form = new OnlineSchedulesFormForTest(onlineSchedules))
			{
				form.Show();

				var originFilter = (ModuleNkFilter)form.FilterControl.FilterBusinessObject["Origin"];
				originFilter.IsActive = true;
				originFilter.Property = "AUSYD";

				var destinationFilter = (ModuleNkFilter)form.FilterControl.FilterBusinessObject["Destination"];
				destinationFilter.IsActive = true;
				destinationFilter.Property = "AUBNE";

				var departureFilter = (ModuleDateFilter)form.FilterControl.FilterBusinessObject["Departure"];
				departureFilter.IsActive = true;
				departureFilter.Property1 = todaysDate;

				var arrivalFilter = (ModuleDateFilter)form.FilterControl.FilterBusinessObject["Arrival"];
				arrivalFilter.IsActive = true;
				arrivalFilter.Property1 = todaysDate;

				form.FilterControl.Find();
				form.FilterControl.FilteredGrid.SelectAllElements();

				var filterControl = form.FilterControl;
				var legsGrid = filterControl.LegsGrid;

				CheckMenuItemEnabledAndDialogMessage(legsGrid, 0, legsGrid.CreateNewVesselMenuItem, Env.Security.VesselsModify, false);
				CheckMenuItemEnabledAndDialogMessage(legsGrid, 0, legsGrid.UpdateVesselImoMenuItem, Env.Security.VesselsModify, true, "During this operation the IMO 9071208 of existing vessel with name TmpNameABC will be substituted with IMO 9308390. Are you sure you want to proceed?");
				CheckMenuItemEnabledAndDialogMessage(legsGrid, 0, legsGrid.AssignScacCodeToCarrierMenuItem, Env.Security.OrgConfigModifyRegistrationNumbers, false);

				CheckMenuItemEnabledAndDialogMessage(legsGrid, 1, legsGrid.CreateNewVesselMenuItem, Env.Security.VesselsModify, true, "During this operation the existing vessel with name TmpNameKKK will be deactivated and the vessel with name TmpNameDEF will be created if it does not exist or activated if it is inactive. Are you sure you want to proceed?");
				CheckMenuItemEnabledAndDialogMessage(legsGrid, 1, legsGrid.UpdateVesselImoMenuItem, Env.Security.VesselsModify, false);
				CheckMenuItemEnabledAndDialogMessage(legsGrid, 1, legsGrid.AssignScacCodeToCarrierMenuItem, Env.Security.OrgConfigModifyRegistrationNumbers, true);

				CheckMenuItemEnabledAndDialogMessage(legsGrid, 2, legsGrid.UpdateVesselImoMenuItem, Env.Security.VesselsModify, false);
				CheckMenuItemEnabledAndDialogMessage(legsGrid, 2, legsGrid.CreateNewVesselMenuItem, Env.Security.VesselsModify, false);
				CheckMenuItemEnabledAndDialogMessage(legsGrid, 2, legsGrid.AssignScacCodeToCarrierMenuItem, Env.Security.OrgConfigModifyRegistrationNumbers, false);
			}
		}

		void CheckMenuItemEnabledAndDialogMessage(ZGrid legsGrid, int clickRow, MenuItem menuItem, SecurityCheckpoint securityCheckpoint, bool enabled, string expectedDialogMessage = null)
		{
			var oldIsAllowed = securityCheckpoint.IsAllowed;

			try
			{
				securityCheckpoint.IsAllowed = false;
				CheckMenuItemEnabledAndDialogMessage(legsGrid, clickRow, menuItem, false);

				securityCheckpoint.IsAllowed = true;
				CheckMenuItemEnabledAndDialogMessage(legsGrid, clickRow, menuItem, enabled, expectedDialogMessage);
			}
			finally
			{
				securityCheckpoint.IsAllowed = oldIsAllowed;
			}
		}

		void CheckMenuItemEnabledAndDialogMessage(ZGrid legsGrid, int clickRow, MenuItem menuItem, bool enabled, string expectedDialogMessage = null)
		{
			var topLeftCellRect = legsGrid.RectangleToScreen(legsGrid.GetCellBounds(0, 0));
			var clickPoint = new Point(topLeftCellRect.Left + topLeftCellRect.Width / 2, topLeftCellRect.Top + topLeftCellRect.Height / 2 + topLeftCellRect.Height * clickRow);
			var savedPoint = legsGrid.MousePositionForTesting;

			try
			{
				legsGrid.MousePositionForTesting = clickPoint;
				legsGrid.CurrentRowIndex = clickRow;
				legsGrid.ContextMenu.ShowPopupMenu();
				AssertEquals(enabled, menuItem.Enabled);

				if (enabled && expectedDialogMessage != null)
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

					menuItem.PerformClick();

					var actualDialogMessage = UnitTestUserNotification.Instance.LastMessage.Text;
					AssertEquals(expectedDialogMessage, actualDialogMessage);
				}
			}
			finally
			{
				legsGrid.MousePositionForTesting = savedPoint;
			}
		}

		public void TestNoDropdownForCarrierScacFilterStrip()
		{
			TestNoDropdownForFilterStripOnlyHasExactOperator(OnlineSchedulesFilterStripBusinessObject.Descriptions.CarrierScac);
		}

		public void TestNoDropdownForTransitTimeFilterFilterStrip()
		{
			TestNoDropdownForFilterStripOnlyHasExactOperator(OnlineSchedulesFilterStripBusinessObject.Descriptions.TransitTime);
		}

		public void TestNoDropdownForLegsCountFilterFilterStrip()
		{
			TestNoDropdownForFilterStripOnlyHasExactOperator(OnlineSchedulesFilterStripBusinessObject.Descriptions.LegsCount);
		}

		public void TestNoDropdownForServiceStringFilterStrip()
		{
			TestNoDropdownForFilterStripOnlyHasExactOperator(OnlineSchedulesFilterStripBusinessObject.Descriptions.ServiceString);
		}

		void TestNoDropdownForFilterStripOnlyHasExactOperator(string filterName)
		{
			string dropdownName = "OperatorDropEdit";
			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object);
			var filterStripBizo = new OnlineSchedulesFilterStripBusinessObject(onlineSchedules);

			using (var form = new OnlineSchedulesFormForTest(onlineSchedules))
			{
				var filterControl = form.FilterControl;
				var strip = filterStripBizo.FilterStrips.AddNew(filterName);
				filterControl.AddFilterStrip(strip);

				var targetStrip = filterControl.Strips.FirstOrDefault(s => s.CurrentDataItem != null && s.CurrentDataItem.FilterDescription.ToString() == filterName);
				AssertNotNull(targetStrip);
				AssertEquals(0, targetStrip.Controls.Find(dropdownName, true).Length);
			}
		}

		public class OnlineSchedulesFormForTest : OnlineSchedulesForm
		{
			public OnlineSchedulesFormForTest(OnlineSchedules onlineSchedules)
				: base(onlineSchedules)
			{
			}

			protected override OnlineSailingSchedulesFilterControl NewFilterControl(RoutesCollection routes,
				OnlineSchedulesFilterStripBusinessObject filterBO)
			{
				return new OnlineSailingSchedulesFilterControlForTest(routes, filterBO);
			}

			internal new OnlineSailingSchedulesFilterControlForTest FilterControl => (OnlineSailingSchedulesFilterControlForTest)(base.FilterControl);
		}

		internal class OnlineSailingSchedulesFilterControlForTest : OnlineSailingSchedulesFilterControl
		{
			public OnlineSailingSchedulesFilterControlForTest(RoutesCollection routesCollection, OnlineSchedulesFilterStripBusinessObject filterBO)
				: base(routesCollection, filterBO)
			{
			}

			public new LegsGrid LegsGrid => base.LegsGrid;
			public new List<ZFilterStrip> Strips => base.Strips;
		}
	}
}
