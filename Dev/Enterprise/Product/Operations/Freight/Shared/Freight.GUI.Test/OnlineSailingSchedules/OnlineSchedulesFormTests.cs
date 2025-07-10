using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration.ApiClient;
using Enterprise.Freight.OnlineSailingSchedules;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceModel = Enterprise.Freight.OnlineSailingSchedules.ServiceModel;

namespace Enterprise.Freight.GUI.OnlineSailingSchedules.Testing
{
	[TestedType(typeof(OnlineSchedulesForm))]
	sealed class OnlineSchedulesFormTests : ZFormBasherTest
	{
		[TestDate(2016, 10, 22)]
		public void TestPerformSearch_ValidateRequest_VoyageAndVessel()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Santa Maria ZZ";
			vessel.RV_IsActive = true;

			Factory.Save();

			var route = CreateRoute("754N", "Santa Maria ZZ", "9308390", "CMA CGM", "AUSYD", new DateTime(2016, 8, 10), "AUBNE", new DateTime(2016, 8, 15));

			var todaysDate = ZDateTime.Today;

			var filterRequest = new OnlineSchedulesFilterRequest
			{
				VoyageNumber = "754N",
				VesselName = "Santa Maria ZZ",
				EtdFrom = todaysDate.ToString("yyyy-MM-dd"),
				EtaFrom = todaysDate.ToString("yyyy-MM-dd"),
				IncludeRelatedPorts = "True",
				SameCarrierRoutes = "False"
			};

			var urlParams = UrlHelper.ConvertToParams(filterRequest);

			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);

			routesProvider.Setup(m => m.GetRoutes(urlParams, It.IsAny<UserInitiatedServiceRequestManager>()))
				.Returns(new[] { route });

			var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object);

			using (var form = new OnlineSchedulesFormForTest(onlineSchedules))
			{
				form.Show();

				var voyageVesselFilter = (OnlineSchedulesVoyageVesselFilter)form.FilterControl.FilterBusinessObject["Voyage # and Vessel"];
				voyageVesselFilter.IsActive = true;
				voyageVesselFilter.VoyageFlightNo = "754N";
				voyageVesselFilter.Vessel = "Santa Maria ZZ";

				form.FilterControl.Find();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, onlineSchedules.Routes.Count);
			}
		}

		[TestDate(2016, 10, 22)]
		public void TestPerformSearch_ValidateRequest_VoyageNumber()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Santa Maria ZZ";
			vessel.RV_IsActive = true;

			Factory.Save();

			var route = CreateRoute("754N", "Santa Maria", "9308390", "CMA CGM", "AUSYD", new DateTime(2016, 8, 10), "AUBNE", new DateTime(2016, 8, 15));

			var todaysDate = ZDateTime.Today;

			var filterRequest = new OnlineSchedulesFilterRequest
			{
				VesselName = "Santa Maria ZZ",
				VoyageNumber = "754N",
				EtdFrom = todaysDate.ToString("yyyy-MM-dd"),
				EtaFrom = todaysDate.ToString("yyyy-MM-dd"),
				IncludeRelatedPorts = "True",
				SameCarrierRoutes = "False"
			};

			var urlParams = UrlHelper.ConvertToParams(filterRequest);

			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);

			routesProvider.Setup(m => m.GetRoutes(urlParams, It.IsAny<UserInitiatedServiceRequestManager>()))
				.Returns(new[] { route });

			var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object);

			using (var form = new OnlineSchedulesFormForTest(onlineSchedules))
			{
				form.Show();

				var voyageVesselFilter = (OnlineSchedulesVoyageVesselFilter)form.FilterControl.FilterBusinessObject["Voyage # and Vessel"];
				voyageVesselFilter.IsActive = true;
				voyageVesselFilter.Vessel = "Santa Maria ZZ";
				voyageVesselFilter.VoyageFlightNo = "754N";

				form.FilterControl.Find();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, onlineSchedules.Routes.Count);
			}
		}

		[TestDate(2016, 10, 22)]
		public void TestPerformSearch_ValidateRequest_VoyageName()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Santa Maria ZZ";
			vessel.RV_IsActive = true;

			Factory.Save();

			var route = CreateRoute("754N", "Santa Maria ZZ", "1234567", "CMA CGM", "AUSYD", new DateTime(2016, 8, 10), "AUBNE", new DateTime(2016, 8, 15));

			var todaysDate = ZDateTime.Today;

			var filterRequest = new OnlineSchedulesFilterRequest
			{
				VesselName = "Santa Maria ZZ",
				EtdFrom = todaysDate.ToString("yyyy-MM-dd"),
				EtaFrom = todaysDate.ToString("yyyy-MM-dd"),
				IncludeRelatedPorts = "True",
				SameCarrierRoutes = "False"
			};

			var urlParams = UrlHelper.ConvertToParams(filterRequest);

			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			routesProvider.Setup(m => m.GetRoutes(urlParams, It.IsAny<UserInitiatedServiceRequestManager>()))
				.Returns(new[] { route });

			var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object);

			using (var form = new OnlineSchedulesFormForTest(onlineSchedules))
			{
				form.Show();

				var voyageVesselFilter = (OnlineSchedulesVoyageVesselFilter)form.FilterControl.FilterBusinessObject["Voyage # and Vessel"];
				voyageVesselFilter.IsActive = true;
				voyageVesselFilter.Vessel = "Santa Maria ZZ";

				form.FilterControl.Find();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, onlineSchedules.Routes.Count);
			}
		}

		[TestDate(2016, 10, 22)]
		public void TestPerformSearch_ValidateRequest_LoadDischargeAndDates()
		{
			var todaysDate = ZDateTime.Now;

			var filterRequest = new OnlineSchedulesFilterRequest
			{
				LoadPort = "AUSYD",
				DischargePort = "AUBNE",
				EtdFrom = todaysDate.ToString("yyyy-MM-dd"),
				EtaFrom = todaysDate.ToString("yyyy-MM-dd"),
				IncludeRelatedPorts = "True",
				SameCarrierRoutes = "False"
			};

			var onlineSchedules = GetOnlineSchedulesForTestFilter(filterRequest);

			using (var form = new OnlineSchedulesFormForTest(onlineSchedules))
			{
				form.Show();

				var originFilter = (ModuleNkFilter)form.FilterControl.FilterBusinessObject[OnlineSchedulesFilterStripBusinessObject.Descriptions.Origin];
				originFilter.IsActive = true;
				originFilter.Property = "AUSYD";

				form.FilterControl.Find();

				var message = @"Filter doesn't meet the minimum parameter requirements. Please specify Origin, Destination and either of ETD or ETA, or Vessel information.";

				AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();

				var destinationFilter = (ModuleNkFilter)form.FilterControl.FilterBusinessObject[OnlineSchedulesFilterStripBusinessObject.Descriptions.Destination];
				destinationFilter.IsActive = true;
				destinationFilter.Property = "AUBNE";

				form.FilterControl.Find();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, onlineSchedules.Routes.Count);
			}
		}

		[TestDate(2017, 05, 30)]
		public void TestPerformSearch_ValidateRequest_CarrierAndCarrierScac_ShouldPopUpMessage()
		{
			var carrier = CreateCarrier();
			var todaysDate = ZDateTime.Now;

			var filterRequest = new OnlineSchedulesFilterRequest
			{
				LoadPort = "AUSYD",
				DischargePort = "AUBNE",
				EtdFrom = todaysDate.ToString("yyyy-MM-dd"),
				EtaFrom = todaysDate.ToString("yyyy-MM-dd"),
				CarrierCode = "SUDU"
			};

			var onlineSchedules = GetOnlineSchedulesForTestFilter(filterRequest);
			using (var form = new OnlineSchedulesFormForTest(onlineSchedules))
			{
				form.Show();

				var filterControl = form.FilterControl;
				var originFilter = (ModuleNkFilter)filterControl.FilterBusinessObject[OnlineSchedulesFilterStripBusinessObject.Descriptions.Origin];
				originFilter.IsActive = true;
				originFilter.Property = "AUSYD";

				var destinationFilter = (ModuleNkFilter)filterControl.FilterBusinessObject[OnlineSchedulesFilterStripBusinessObject.Descriptions.Destination];
				destinationFilter.IsActive = true;
				destinationFilter.Property = "AUBNE";

				var carrierFilter = (ModuleGuidFilter)filterControl.FilterBusinessObject[OnlineSchedulesFilterStripBusinessObject.Descriptions.Carrier];
				carrierFilter.IsActive = true;
				carrierFilter.Property = carrier.PK;

				// select both Carrier SCAC and Carrier Code filters
				var carrierScacFilter = (ModuleTextFilter)filterControl.FilterBusinessObject[OnlineSchedulesFilterStripBusinessObject.Descriptions.CarrierScac];
				carrierScacFilter.IsActive = true;
				carrierScacFilter.Property = "SUDU";

				filterControl.Find();

				var message = @"Both Carrier Code and Carrier SCAC filters are used. The Carrier SCAC filter will be applied by default.";
				AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2017, 05, 30)]
		public void TestPerformSearch_ValidateRequest_CarrierAndCarrierScac_Should_Not_PopUpMessage()
		{
			var carrier = CreateCarrier();
			var todaysDate = ZDateTime.Now;

			var filterRequest = new OnlineSchedulesFilterRequest
			{
				LoadPort = "AUSYD",
				DischargePort = "AUBNE",
				EtdFrom = todaysDate.ToString("yyyy-MM-dd"),
				EtaFrom = todaysDate.ToString("yyyy-MM-dd"),
				CarrierCode = "SCAC",
				IncludeRelatedPorts = "True",
				SameCarrierRoutes = "False"
			};

			var onlineSchedules = GetOnlineSchedulesForTestFilter(filterRequest);
			using (var form = new OnlineSchedulesFormForTest(onlineSchedules))
			{
				form.Show();

				var filterControl = form.FilterControl;
				var originFilter = (ModuleNkFilter)filterControl.FilterBusinessObject[OnlineSchedulesFilterStripBusinessObject.Descriptions.Origin];
				originFilter.IsActive = true;
				originFilter.Property = "AUSYD";

				var destinationFilter = (ModuleNkFilter)filterControl.FilterBusinessObject[OnlineSchedulesFilterStripBusinessObject.Descriptions.Destination];
				destinationFilter.IsActive = true;
				destinationFilter.Property = "AUBNE";

				var carrierFilter = (ModuleGuidFilter)filterControl.FilterBusinessObject[OnlineSchedulesFilterStripBusinessObject.Descriptions.Carrier];
				carrierFilter.IsActive = true;
				carrierFilter.Property = carrier.PK;

				filterControl.Find();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		OrgHeader CreateCarrier()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "AAAAA";

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "SCAC";
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			Factory.Save();

			return carrier;
		}

		OnlineSchedules GetOnlineSchedulesForTestFilter(OnlineSchedulesFilterRequest filterRequest)
		{
			var route = CreateRoute("754N", "Santa Maria", "1234567", "CMA CGM", "AUSYD", new DateTime(2016, 8, 10), "AUBNE", new DateTime(2016, 8, 15));

			var urlParams = UrlHelper.ConvertToParams(filterRequest);

			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			routesProvider.Setup(m => m.GetRoutes(urlParams, It.IsAny<UserInitiatedServiceRequestManager>()))
				.Returns(new[] { route });

			return new OnlineSchedules(Factory, routesProvider.Object);
		}

		void PerformSearch_ThrowException(Exception exception, string notification, bool errorReport)
		{
			var todaysDate = ZDateTime.Today;

			var routesProviderForTesting = new RoutesProviderForTesting(Factory)
			{
				MockResponseGetter = (httpClient, requestUri) =>
				{
					throw exception;
				}
			};

			var onlineSchedules = new OnlineSchedules(Factory, routesProviderForTesting);

			using (var form = new OnlineSchedulesForm(onlineSchedules))
			{
				var originFilter = (ModuleNkFilter)form.FilterControl.FilterBusinessObject["Origin"];
				originFilter.IsActive = true;
				originFilter.Property = "AUSYD";

				var destinationFilter = (ModuleNkFilter)form.FilterControl.FilterBusinessObject["Destination"];
				destinationFilter.IsActive = true;
				destinationFilter.Property = "AUBNE";

				var departureFilter = (ModuleDateFilter)form.FilterControl.FilterBusinessObject["Departure"];
				departureFilter.IsActive = true;
				departureFilter.Property1 = todaysDate;

				form.FilterControl.Find();

				AssertEquals(notification, UnitTestUserNotification.Instance.LastMessage.Text);

				if (errorReport)
				{
					AssertEquals(1, ErrorReporter.TotalErrorCount);
					AssertContains(notification, ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
				}
			}
		}

		[TestDate(2016, 10, 22)]
		public void TestPerformSearch_ConnectionClosed()
		{
			var exception = new HttpRequestException(string.Empty, new WebException(string.Empty, WebExceptionStatus.ConnectionClosed));
			var notification = "Unable to connect to Global Sailing Schedules. Please check that connection URL specified in the registry is correct.";
			PerformSearch_ThrowException(exception, notification, false);
		}

		[TestDate(2016, 10, 22)]
		public void TestPerformSearch_UnknownException()
		{
			var exception = new Exception("Unknown exception");
			var notification = "Error occurred while loading routes from Global Schedules service.";
			PerformSearch_ThrowException(exception, notification, true);
		}

		[TestDate(2016, 10, 22)]
		public void TestPerformSearch_NoRoutesFound()
		{
			var todaysDate = ZDateTime.Today;

			var filterRequest = new OnlineSchedulesFilterRequest
			{
				LoadPort = "AUSYD",
				DischargePort = "AUBNE",
				EtdFrom = todaysDate.ToString("yyyy-MM-dd"),
				IncludeRelatedPorts = "True",
				SameCarrierRoutes = "False"
			};

			var urlParams = UrlHelper.ConvertToParams(filterRequest);

			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);

			routesProvider.Setup(m => m.GetRoutes(urlParams, It.IsAny<UserInitiatedServiceRequestManager>()))
				.Returns(Array.Empty<Route>());

			var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object) { Request = filterRequest };

			using (var form = new OnlineSchedulesForm(onlineSchedules))
			{
				var originFilter = (ModuleNkFilter)form.FilterControl.FilterBusinessObject["Origin"];
				originFilter.IsActive = true;
				originFilter.Property = "AUSYD";

				var destinationFilter = (ModuleNkFilter)form.FilterControl.FilterBusinessObject["Destination"];
				destinationFilter.IsActive = true;
				destinationFilter.Property = "AUBNE";

				var departureFilter = (ModuleDateFilter)form.FilterControl.FilterBusinessObject["Departure"];
				departureFilter.IsActive = true;
				departureFilter.Property1 = todaysDate;

				form.FilterControl.Find();

				AssertEquals("There are no routes found by provided criteria.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCloseButton_Click()
		{
			var routeProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			var onlineSchedules = new OnlineSchedules(Factory, routeProvider.Object);

			using (var form = new OnlineSchedulesFormForTest(onlineSchedules))
			{
				form.Show();
				Assert(!form.IsDisposed);

				form.CloseButton.PerformClick();
				Assert(form.IsDisposed);
			}
		}

		public void TestImportButton_Click_InvalidRoute()
		{
			var entCarrier = Factory.NewWithValidTestData<OrgHeader>();
			entCarrier.OH_Code = "CMACGM";
			var cusCode = entCarrier.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = "CMAC";
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			Factory.Save();

			var route = CreateRoute("754N", "Santa Maria", "9308390", "CMA CGM", "AUSYD", new DateTime(2016, 8, 20), "AUBNE", new DateTime(2016, 8, 15));

			var todaysDate = ZDateTime.Today;

			var filterRequest = new OnlineSchedulesFilterRequest
			{
				LoadPort = "AUSYD",
				DischargePort = "AUBNE",
				EtdFrom = todaysDate.ToString("yyyy-MM-dd"),
				EtaFrom = todaysDate.ToString("yyyy-MM-dd"),
				IncludeRelatedPorts = "True",
				SameCarrierRoutes = "False"
			};

			var urlParams = UrlHelper.ConvertToParams(filterRequest);

			var routesProvider = new Mock<IRoutesProvider>();

			routesProvider.Setup(m => m.GetRoutes(urlParams, It.IsAny<UserInitiatedServiceRequestManager>()))
				.Returns(new[] { route });

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

				form.FilterControl.Find();
				form.FilterControl.FilteredGrid.SelectAllElements();

				form.ImportButton.PerformClick();

				var voyages = GetVoyages(route.Carrier, route.Legs[0]);

				AssertEquals("AUSYD (20-Aug-16 00:00:00) -> AUBNE (15-Aug-16 00:00:00): Arrival time cannot be before Departure time.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, voyages.Length);
			}
		}

		public void TestImportButton_Click_CheckUserHasSecurityRightsToCreateNewSailings()
		{
			var routeProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);

			var onlineSchedules = new OnlineSchedules(Factory, routeProvider.Object);
			using (var form = new OnlineSchedulesFormForTest(onlineSchedules))
			{
				form.Show();

				var isAllowedNew = Env.Security.SailingScheduleNew.IsAllowed;
				var isAllowedCreateFromJob = Env.Security.SailingScheduleCreateFromJob.IsAllowed;
				using (new DisposableAction(() =>
				{
					Env.Security.SailingScheduleNew.IsAllowed = isAllowedNew;
					Env.Security.SailingScheduleCreateFromJob.IsAllowed = isAllowedCreateFromJob;
				}))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Env.Security.SailingScheduleNew.IsAllowed = false;
					form.ImportButton.PerformClick();
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals(Env.Security.SailingScheduleNew.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Env.Security.SailingScheduleNew.IsAllowed = true;
					Env.Security.SailingScheduleCreateFromJob.IsAllowed = false;
					form.SailingScheduleCreateFromJob = true;
					form.ImportButton.PerformClick();
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals(Env.Security.SailingScheduleCreateFromJob.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					form.SailingScheduleCreateFromJob = false;
					form.ImportButton.PerformClick();
					AssertNotEquals(Env.Security.SailingScheduleCreateFromJob.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		[TestDate(2016, 10, 22)]
		public void TestImportButton_Click_OneRouteHasBeenImported()
		{
			var entCarrier = Factory.NewWithValidTestData<OrgHeader>();
			entCarrier.OH_Code = "CMACGM";
			var cusCode = entCarrier.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = "CMAC";
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			Factory.Save();

			var route = CreateRoute("754N", "Santa Maria", "9308390", "CMA CGM", "AUSYD", new DateTime(2016, 8, 10), "AUBNE", new DateTime(2016, 8, 15));

			var todaysDate = ZDateTime.Today;

			var filterRequest = new OnlineSchedulesFilterRequest
			{
				LoadPort = "AUSYD",
				DischargePort = "AUBNE",
				EtdFrom = todaysDate.ToString("yyyy-MM-dd"),
				EtaFrom = todaysDate.ToString("yyyy-MM-dd"),
				IncludeRelatedPorts = "True",
				SameCarrierRoutes = "False"
			};

			var urlParams = UrlHelper.ConvertToParams(filterRequest);

			var routesProvider = new Mock<IRoutesProvider>();

			routesProvider.Setup(m => m.GetRoutes(urlParams, It.IsAny<UserInitiatedServiceRequestManager>()))
				.Returns(new[] { route });

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

				form.FilterControl.Find();
				form.FilterControl.FilteredGrid.SelectAllElements();

				form.ImportButton.PerformClick();

				AssertEquals("1 schedule has been imported.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, GetVoyages(route.Carrier, route.Legs[0]).Length);
			}
		}

		[TestDate(2016, 10, 22)]
		public void TestImportButton_Click_TwoRoutesHaveBeenImported()
		{
			var entCarrier = Factory.NewWithValidTestData<OrgHeader>();
			entCarrier.OH_Code = "CMACGM";
			var cusCode = entCarrier.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = "CMAC";
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			Factory.Save();

			var route1 = CreateRoute("754N", "Santa Maria", "9308390", "CMA CGM", "AUSYD", new DateTime(2016, 8, 10), "AUBNE", new DateTime(2016, 8, 15));
			var route2 = CreateRoute("758N", "Admiral Grant", "9463085", "CMA CGM", "AUSYD", new DateTime(2016, 8, 16), "AUBNE", new DateTime(2016, 8, 21));

			var todaysDate = ZDateTime.Today;

			var filterRequest = new OnlineSchedulesFilterRequest
			{
				LoadPort = "AUSYD",
				DischargePort = "AUBNE",
				EtdFrom = todaysDate.ToString("yyyy-MM-dd"),
				EtaFrom = todaysDate.ToString("yyyy-MM-dd"),
				IncludeRelatedPorts = "True",
				SameCarrierRoutes = "False"
			};

			var urlParams = UrlHelper.ConvertToParams(filterRequest);

			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);

			routesProvider.Setup(m => m.GetRoutes(urlParams, It.IsAny<UserInitiatedServiceRequestManager>()))
				.Returns(new[] { route1, route2 });

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

				form.FilterControl.Find();
				form.FilterControl.FilteredGrid.SelectAllElements();

				Route[] importedRoutes = null;
				form.SailingSchedulesImported += delegate(object sender, OnlineSailingSchedulesImportedEventArgs eventArgs)
				{
					importedRoutes = eventArgs.ImportedRoutes.ToArray();
				};

				form.ImportButton.PerformClick();

				AssertEquals("2 schedules have been imported.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, GetVoyages(route1.Carrier, route1.Legs[0]).Length);
				AssertEquals(1, GetVoyages(route2.Carrier, route2.Legs[0]).Length);

				AssertContainsExactElementsInAnyOrder(importedRoutes, new[] { route1, route2 });
			}
		}

		[TestDate(2016, 10, 22)]
		public void TestImportButton_Click_ValidationMessageShown()
		{
			var route = CreateRoute("754N", "Santa Maria", "9308390", "CMA CGM", "AUSYD", new DateTime(2016, 8, 10), "AUBNE", new DateTime(2016, 8, 15));

			var todaysDate = ZDateTime.Today;

			var filterRequest = new OnlineSchedulesFilterRequest
			{
				LoadPort = "AUSYD",
				DischargePort = "AUBNE",
				EtdFrom = todaysDate.ToString("yyyy-MM-dd"),
				EtaFrom = todaysDate.ToString("yyyy-MM-dd"),
				IncludeRelatedPorts = "True",
				SameCarrierRoutes = "False"
			};

			var urlParams = UrlHelper.ConvertToParams(filterRequest);

			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);

			routesProvider.Setup(m => m.GetRoutes(urlParams, It.IsAny<UserInitiatedServiceRequestManager>()))
				.Returns(new[] { route });

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

				form.FilterControl.Find();
				form.FilterControl.FilteredGrid.SelectAllElements();

				form.ImportButton.PerformClick();

				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2016, 10, 22)]
		public void TestImportButton_WhenEmptySelection()
		{
			var todaysDate = ZDateTime.Today;

			var filterRequest = new OnlineSchedulesFilterRequest
			{
				LoadPort = "AUSYD",
				DischargePort = "AUBNE",
				EtdFrom = todaysDate.ToString("yyyy-MM-dd"),
				EtaFrom = todaysDate.ToString("yyyy-MM-dd"),
				IncludeRelatedPorts = "True",
				SameCarrierRoutes = "False"
			};

			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);

			var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object) { Request = filterRequest };
			using (var form = new OnlineSchedulesFormForTest(onlineSchedules))
			{
				form.Show();
				AssertEquals("Nothing should be selected", true, form.FilterControl.FilteredGrid.SelectedElements.Cast<Route>().ToArray().IsNullOrEmpty());

				form.ImportButton.PerformClick();

				AssertEquals("Please select an item from the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2016, 10, 22)]
		public void TestAllowMultipleSelection_IsDisabled_NotificationWhenMultipleRoutesSelected()
		{
			var today = ZDateTime.Today;

			var route1 = CreateRoute(
				"100",
				"OLGA MAERSK",
				"9308390",
				"MAERSK",
				"AUSYD", today.AddDays(10).ToDateTime(),
				"NZAKL", today.AddDays(15).ToDateTime());

			var route2 = CreateRoute(
				"100",
				"OLGA MAERSK",
				"9308390",
				"MAERSK",
				"AUSYD", today.AddDays(1).ToDateTime(),
				"NZAKL", today.AddDays(5).ToDateTime());

			var filterRequest = new OnlineSchedulesFilterRequest
			{
				LoadPort = "AUSYD",
				DischargePort = "NZAKL",
				EtdFrom = today.ToString("yyyy-MM-dd"),
				EtaFrom = today.ToString("yyyy-MM-dd"),
				IncludeRelatedPorts = "True",
				SameCarrierRoutes = "False"
			};

			var urlParams = UrlHelper.ConvertToParams(filterRequest);

			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);

			routesProvider.Setup(m => m.GetRoutes(urlParams, It.IsAny<UserInitiatedServiceRequestManager>()))
				.Returns(new[] { route1, route2 });

			var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object) { Request = filterRequest };

			using (var form = new OnlineSchedulesFormForTest(onlineSchedules))
			{
				form.AllowMultipleSelection = false;
				form.Show();

				var originFilter = (ModuleNkFilter)form.FilterControl.FilterBusinessObject[OnlineSchedulesFilterStripBusinessObject.Descriptions.Origin];
				originFilter.IsActive = true;
				originFilter.Property = "AUSYD";

				var destinationFilter = (ModuleNkFilter)form.FilterControl.FilterBusinessObject[OnlineSchedulesFilterStripBusinessObject.Descriptions.Destination];
				destinationFilter.IsActive = true;
				destinationFilter.Property = "NZAKL";

				var departureFilter = (ModuleDateFilter)form.FilterControl.FilterBusinessObject[OnlineSchedulesFilterStripBusinessObject.Descriptions.Departure];
				departureFilter.IsActive = true;
				departureFilter.Property1 = today;

				form.FilterControl.Find();
				form.FilterControl.FilteredGrid.SelectAllElements();

				Route importedRoute = null;
				form.SailingSchedulesImported += delegate(object sender, OnlineSailingSchedulesImportedEventArgs eventArgs)
				{
					importedRoute = eventArgs.ImportedRoutes.Single();
				};

				form.ImportButton.PerformClick();

				AssertEquals("Please select only one route.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Nothing was imported", null, importedRoute);
			}
		}

		[TestDate(2016, 10, 22)]
		public void TestAllowMultipleSelection_IsDisabled_SingleRouteHaveBeenImported()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "MAERSK";

			var cusCode = carrier.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = "MAER";
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			Factory.Save();

			var today = ZDateTime.Today;

			var route = CreateRoute(
				"100",
				"OLGA MAERSK",
				"9308390",
				"MAERSK",
				"AUSYD", today.AddDays(10).ToDateTime(),
				"NZAKL", today.AddDays(15).ToDateTime());

			var filterRequest = new OnlineSchedulesFilterRequest
			{
				LoadPort = "AUSYD",
				DischargePort = "NZAKL",
				EtdFrom = today.ToString("yyyy-MM-dd"),
				EtaFrom = today.ToString("yyyy-MM-dd"),
				IncludeRelatedPorts = "True",
				SameCarrierRoutes = "False"
			};

			var urlParams = UrlHelper.ConvertToParams(filterRequest);

			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);

			routesProvider.Setup(m => m.GetRoutes(urlParams, It.IsAny<UserInitiatedServiceRequestManager>()))
				.Returns(new[] { route });

			var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object) { Request = filterRequest };

			using (var form = new OnlineSchedulesFormForTest(onlineSchedules))
			{
				form.AllowMultipleSelection = false;
				form.Show();

				var originFilter = (ModuleNkFilter)form.FilterControl.FilterBusinessObject[OnlineSchedulesFilterStripBusinessObject.Descriptions.Origin];
				originFilter.IsActive = true;
				originFilter.Property = "AUSYD";

				var destinationFilter = (ModuleNkFilter)form.FilterControl.FilterBusinessObject[OnlineSchedulesFilterStripBusinessObject.Descriptions.Destination];
				destinationFilter.IsActive = true;
				destinationFilter.Property = "NZAKL";

				var departureFilter = (ModuleDateFilter)form.FilterControl.FilterBusinessObject[OnlineSchedulesFilterStripBusinessObject.Descriptions.Departure];
				departureFilter.IsActive = true;
				departureFilter.Property1 = today;

				form.FilterControl.Find();
				form.FilterControl.FilteredGrid.SelectAllElements();

				Route importedRoute = null;
				form.SailingSchedulesImported += delegate(object sender, OnlineSailingSchedulesImportedEventArgs eventArgs)
				{
					importedRoute = eventArgs.ImportedRoutes.Single();
				};

				form.ImportButton.PerformClick();

				var routeKeyValues = new[]
				{
					importedRoute.OriginPort.RL_Code,
					importedRoute.DestinationPort.RL_Code,
					importedRoute.Legs[0].VoyageCode,
					importedRoute.Legs[0].VesselName,
				};

				AssertEquals("AUSYD-NZAKL-100-OLGA MAERSK", string.Join("-", routeKeyValues));
			}
		}

		public void TestSetFilterDefaults_FiltersAreVisible_And_HaveDefaultValues()
		{
			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);

			var onlineSchedules = new OnlineSchedules(Factory, routesProvider.Object);

			using (var form = new OnlineSchedulesFormForTest(onlineSchedules))
			{
				form.Show();

				var filterDefaults = new OnlineSchedulesFilterStripBusinessObject.FilterDefaults()
				{
					Origin = "AUSYD",
					Voyage = "100"
				};

				form.SetFilterDefaults(filterDefaults);

				var filterStripBizo = form.FilterControl.FilterBusinessObject as OnlineSchedulesFilterStripBusinessObject;

				var origin = ((ModuleNkFilter)filterStripBizo[OnlineSchedulesFilterStripBusinessObject.Descriptions.Origin]);
				AssertEquals(FilterVisibility.AlwaysVisible, origin.Visibility);
				AssertEquals("AUSYD", origin.Property);

				var voyageVesselFilter = ((OnlineSchedulesVoyageVesselFilter)filterStripBizo[OnlineSchedulesFilterStripBusinessObject.Descriptions.VoyageVessel]);
				AssertEquals(FilterVisibility.AlwaysVisible, voyageVesselFilter.Visibility);
				AssertEquals("100", voyageVesselFilter.Property);
			}
		}

		[TestDate(2016, 10, 22)]
		public void TestImportButton_Click_CopyDepartureReference()
		{
			var entCarrier = Factory.NewWithValidTestData<OrgHeader>();
			entCarrier.OH_Code = "CMACGM";
			var cusCode = entCarrier.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = "CMAC";
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			Factory.Save();

			JobVoyage Import(string departureReference)
			{
				var route = CreateRoute("754N", "Santa Maria", "9308390", "CMA CGM", "AUSYD", new DateTime(2016, 8, 10), "AUBNE", new DateTime(2016, 8, 15), departureReference);

				var todaysDate = ZDateTime.Today;

				var filterRequest = new OnlineSchedulesFilterRequest
				{
					LoadPort = "AUSYD",
					DischargePort = "AUBNE",
					EtdFrom = todaysDate.ToString("yyyy-MM-dd"),
					EtaFrom = todaysDate.ToString("yyyy-MM-dd"),
					IncludeRelatedPorts = "True",
					SameCarrierRoutes = "False"
				};

				var urlParams = UrlHelper.ConvertToParams(filterRequest);

				var routesProvider = new Mock<IRoutesProvider>();

				routesProvider.Setup(m => m.GetRoutes(urlParams, It.IsAny<UserInitiatedServiceRequestManager>()))
					.Returns(new[] { route });

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

					form.FilterControl.Find();
					form.FilterControl.FilteredGrid.SelectAllElements();

					form.ImportButton.PerformClick();

					var voyages = GetVoyages(route.Carrier, route.Legs[0]);
					AssertEquals("1 schedule has been imported.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(1, voyages.Length);

					return voyages[0];
				}
			}

			var voyage1 = Import("ABCD");
			AssertEquals("ABCD", voyage1.Sailings[0].JX_DeparturePortRouteId);

			Import("BCDE");
			AssertEquals("BCDE", voyage1.Sailings[0].JX_DeparturePortRouteId);
		}

		#region Implementation

		Route CreateRoute(
			string voyageNumber = "",
			string vesselName = "",
			string imoNumber = "9308390",
			string carrierName = "",
			string loadPort = "",
			DateTime? departureDate = null,
			string dischargePort = "",
			DateTime? arrivalDate = null,
			string departureReference = "")
		{
			var carrier = new ServiceModel.Carrier
			{
				Name = carrierName,
				Code = new ZString(carrierName.Replace(" ", "")).SubstringSafe(0, 4)
			};

			var serviceLeg = new ServiceModel.Leg
			{
				Voyage = new ServiceModel.Voyage
				{
					Code = voyageNumber,
					Vessel = new ServiceModel.Vessel
					{
						VesselName = vesselName,
						ImoNumber = imoNumber
					},
					TradeLane = new ServiceModel.TradeLane { Name = "Tradeline Inc." },
					Operator = carrier
				},

				LoadPort = new ServiceModel.Port { Unloco = loadPort },
				DischargePort = new ServiceModel.Port { Unloco = dischargePort },

				Etd = departureDate != null && departureDate.HasValue
					? departureDate.Value
					: ZDateTime.Today.ToDateTime(),

				Eta = arrivalDate != null && arrivalDate.HasValue
					? arrivalDate.Value
					: ZDateTime.Today.ToDateTime(),

				DepartureReference = departureReference
			};

			var serviceRoute = new ServiceModel.Route
			{
				Carrier = carrier,
				Legs = new[] { serviceLeg }
			};

			var route = new Route(Factory);
			route.SetValues(serviceRoute);

			return route;
		}

		JobVoyage[] GetVoyages(OrgHeader carrier, Leg leg)
		{
			var query = new ZQuery(JobVoyageSchema.JV_VoyageFlight, leg.VoyageCode);
			query.AddToFilter(JobVoyageSchema.JV_RV_NKVessel, leg.VesselName);
			query.AddToFilter(JobVoyageSchema.JV_OH_Line, carrier.PK);
			query.AddToFilter(JobVoyageSchema.JV_AirSeaRoad, Core.Constants.TransportModes.Sea);
			query.AddToFilter(JobVoyageSchema.JV_IsActive, true);

			return Factory.Load<JobVoyage>(query);
		}

		protected override Form GetFormToBashCore()
		{
			return new OnlineSchedulesForm(new OnlineSchedules(Factory, new RoutesProvider(Factory)));
		}

		protected override bool AllowSaveOnFormForTestHasChanges => false;

		class OnlineSchedulesFormForTest : OnlineSchedulesForm
		{
			public OnlineSchedulesFormForTest(OnlineSchedules onlineSchedules)
				: base(onlineSchedules)
			{
			}

			public new ZButton ImportButton => base.ImportButton;
			public new ZButton CloseButton => base.CloseButton;
		}

		class RoutesProviderForTesting : RoutesProvider
		{
			public RoutesProviderForTesting(BusinessObjectFactory factory) : base(factory)
			{
			}

			public Func<IApiClient, string, HttpResponseMessage> MockResponseGetter;

			protected override IApiResponse<ServiceModel.Route[]> TryGetResponse(string endpoint, IApiClient client)
			{
				MockResponseGetter.Invoke(client, endpoint);
				return base.TryGetResponse(endpoint, client);
			}
		}

		#endregion
	}
}
