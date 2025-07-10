using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	class TrackingMapUrlServiceTest : TestCaseWithFactory
	{
		public void TestGetActiveTransportMapUrl_NoTransport()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();
			var helper = new TransportOrderHelper(shipment.Transports);

			(var url, var errorMessage) = service.GetActiveTransportMapUrl(helper, contact, CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertNull(url);
				AssertEquals("Transport not found.", errorMessage);
			});
		}

		public void TestGetActiveTransportMapUrl_NoVesselAuthTokenCanBeObtained()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();

			mockVesselMovementsUrlGenerator.Setup(x => x.GetTokenAsync(It.IsAny<string>(), contact, CancellationToken.None)).Returns(Task.FromResult(new VesselMovementsAuthTokenResult { ErrorMessage = "Unable to connect to the authentication service, try again later." }));

			var helper = new TransportOrderHelper(shipment.Transports);
			(var url, var errorMessage) = service.GetActiveTransportMapUrl(helper, contact, CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertNull(url);
				AssertEquals("Unable to show map: Unable to connect to the authentication service, try again later.", errorMessage);
			});
		}

		public void TestGetActiveTransportMapUrl_VesselTokenRequiresRedirect()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();

			var myAccountUrl = new Uri("http://myaccount.com/verifyyouremail");
			mockVesselMovementsUrlGenerator.Setup(x => x.GetTokenAsync(It.IsAny<string>(), contact, CancellationToken.None)).Returns(Task.FromResult(new VesselMovementsAuthTokenResult { RedirectUrl = myAccountUrl }));

			var helper = new TransportOrderHelper(shipment.Transports);
			(var url, var errorMessage) = service.GetActiveTransportMapUrl(helper, contact, CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertEquals(myAccountUrl, url);
				AssertNull(errorMessage);
			});
		}

		public void TestGetActiveTransportMapUrl_VesselGeneratorError()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();

			mockVesselMovementsUrlGenerator.Setup(x => x.GetTokenAsync(It.IsAny<string>(), contact, CancellationToken.None)).Returns(Task.FromResult(new VesselMovementsAuthTokenResult { Token = vmToken }));

			mockVesselMovementsUrlGenerator
				.Setup(x => x.Generate(vmToken, It.Is<Transport>(t => t.PK == transport.PK), VesselMovementsUrlGeneratorOptions.CollapseInfoPanel | VesselMovementsUrlGeneratorOptions.CollapsePortCallsPanel))
				.Returns((null, "Error from generator."));

			var helper = new TransportOrderHelper(shipment.Transports);
			(var url, var errorMessage) = service.GetActiveTransportMapUrl(helper, contact, CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertNull(url);
				AssertEquals("Unable to show map: Error from generator.", errorMessage);
			});
		}

		public void TestGetActiveTransportMapUrl_NoCargoTrackerAuthTokenCanBeObtained()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001234";
			var transport = consol.Transports[0];
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			Factory.Save();

			mockCargoTrackerUrlGenerator.Setup(x => x.GetToken(It.IsAny<string>(), contact, CancellationToken.None)).Returns(new CargoTrackerAuthTokenResult { ErrorMessage = "Unable to connect to the authentication service, try again later." });

			var helper = new TransportOrderHelper(consol.Transports);
			(var url, var errorMessage) = service.GetActiveTransportMapUrl(helper, contact, CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertNull(url);
				AssertEquals("Unable to show map: Unable to connect to the authentication service, try again later.", errorMessage);
			});
		}

		public void TestGetActiveTransportMapUrl_TransportModeNotSupported()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001234";
			var transport = consol.Transports[0];
			transport.JW_TransportMode = Core.Constants.TransportModes.Road;
			Factory.Save();

			mockCargoTrackerUrlGenerator.Setup(x => x.GetToken(It.IsAny<string>(), contact, CancellationToken.None)).Returns(new CargoTrackerAuthTokenResult { ErrorMessage = "Unable to connect to the authentication service, try again later." });

			var helper = new TransportOrderHelper(consol.Transports);
			(var url, var errorMessage) = service.GetActiveTransportMapUrl(helper, contact, CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertNull(url);
				AssertEquals($"The shipment is in transit via " + Core.Constants.TransportModeDescriptions.Road + " mode.", errorMessage);
			});
		}

		public void TestGetActiveTransportMapUrl_CargoTrackerTokenRequiresRedirect()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001234";
			var transport = consol.Transports[0];
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			Factory.Save();

			var myAccountUrl = new Uri("http://myaccount.com/verifyyouremail");
			mockCargoTrackerUrlGenerator.Setup(x => x.GetToken(It.IsAny<string>(), contact, CancellationToken.None)).Returns(new CargoTrackerAuthTokenResult { RedirectUrl = myAccountUrl });

			var helper = new TransportOrderHelper(consol.Transports);
			(var url, var errorMessage) = service.GetActiveTransportMapUrl(helper, contact, CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertEquals(myAccountUrl, url);
				AssertNull(errorMessage);
			});
		}

		public void TestGetActiveTransportMapUrl_CargoTrackerGeneratorError()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001234";
			var transport = consol.Transports[0];
			transport.JW_TransportMode =  Core.Constants.TransportModes.Air;

			Factory.Save();

			mockCargoTrackerUrlGenerator.Setup(x => x.GetToken(It.IsAny<string>(), contact, CancellationToken.None)).Returns(new CargoTrackerAuthTokenResult { Token = ctToken });

			mockCargoTrackerUrlGenerator
				.Setup(x => x.Generate(ctToken, licenseCode, "C00001234"))
				.Returns((null, "Error from generator."));

			var helper = new TransportOrderHelper(consol.Transports);
			(var url, var errorMessage) = service.GetActiveTransportMapUrl(helper, contact, CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertNull(url);
				AssertEquals("Unable to show map: Error from generator.", errorMessage);
			});
		}

		[TestDate(2019, 7, 28)]
		public void TestGetActiveTransportMapUrl_Sea_CurrentTransport()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			AddNewTransportForTest(shipment.Transports, Core.Constants.TransportModes.Sea, "1234567", new ZDateTime(2019, 7, 10, 1, 2, 3), new ZDateTime(2019, 7, 19, 1, 2, 3));
			var expectedTransport = AddNewTransportForTest(shipment.Transports, Core.Constants.TransportModes.Sea, "8181812", new ZDateTime(2019, 7, 20, 1, 2, 3), new ZDateTime(2019, 7, 31, 1, 2, 3));
			AddNewTransportForTest(shipment.Transports, Core.Constants.TransportModes.Sea, "9191917", new ZDateTime(2019, 8, 1, 1, 2, 3), new ZDateTime(2019, 8, 11, 1, 2, 3));
			Factory.Save();

			mockVesselMovementsUrlGenerator.Setup(x => x.GetTokenAsync(It.IsAny<string>(), contact, CancellationToken.None)).Returns(Task.FromResult(new VesselMovementsAuthTokenResult { Token = vmToken }));

			var expectedUrl = new Uri("https://wisegrid.net/mapurl");
			mockVesselMovementsUrlGenerator
				.Setup(x => x.Generate(vmToken, It.Is<Transport>(t => t.PK == expectedTransport.PK), VesselMovementsUrlGeneratorOptions.CollapseInfoPanel | VesselMovementsUrlGeneratorOptions.CollapsePortCallsPanel))
				.Returns((expectedUrl, string.Empty));

			var helper = new TransportOrderHelper(shipment.Transports);
			(var url, var errorMessage) = service.GetActiveTransportMapUrl(helper, contact, CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertEquals(expectedUrl, url);
				AssertNull(errorMessage);
			});
		}

		[TestDate(2019, 7, 28)]
		public void TestGetActiveTransportMapUrl_Air_CurrentTransport()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001234";
			AddNewTransportForTest(consol.Transports, Core.Constants.TransportModes.Air, "1234567", new ZDateTime(2019, 7, 10, 1, 2, 3), new ZDateTime(2019, 7, 19, 1, 2, 3));
			var expectedTransport = AddNewTransportForTest(consol.Transports, Core.Constants.TransportModes.Air, "8181812", new ZDateTime(2019, 7, 20, 1, 2, 3), new ZDateTime(2019, 7, 31, 1, 2, 3));
			AddNewTransportForTest(consol.Transports, Core.Constants.TransportModes.Air, "9191917", new ZDateTime(2019, 8, 1, 1, 2, 3), new ZDateTime(2019, 8, 11, 1, 2, 3));
			Factory.Save();

			mockCargoTrackerUrlGenerator.Setup(x => x.GetToken(It.IsAny<string>(), contact, CancellationToken.None)).Returns(new CargoTrackerAuthTokenResult { Token = ctToken });

			var expectedUrl = new Uri("https://cargoTracker.net/mapurl");
			mockCargoTrackerUrlGenerator
				.Setup(x => x.Generate(ctToken, licenseCode, "C00001234"))
				.Returns((expectedUrl, string.Empty));

			var helper = new TransportOrderHelper(consol.Transports);
			(var url, var errorMessage) = service.GetActiveTransportMapUrl(helper, contact, CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertEquals(expectedUrl, url);
				AssertNull(errorMessage);
			});
		}

		[TestDate(2019, 7, 28)]
		public void TestGetActiveTransportMapUrl_EstimateTimeFallback()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			AddNewTransportForTest(shipment.Transports, Core.Constants.TransportModes.Sea, "1234567", new ZDateTime(2019, 7, 10, 1, 2, 3), new ZDateTime(2019, 7, 19, 1, 2, 3), true);
			var expectedTransport = AddNewTransportForTest(shipment.Transports, Core.Constants.TransportModes.Sea, "8181812", new ZDateTime(2019, 7, 20, 1, 2, 3), new ZDateTime(2019, 7, 31, 1, 2, 3), true);
			AddNewTransportForTest(shipment.Transports, Core.Constants.TransportModes.Sea, "9191917", new ZDateTime(2019, 8, 1, 1, 2, 3), new ZDateTime(2019, 8, 11, 1, 2, 3), true);
			Factory.Save();

			mockVesselMovementsUrlGenerator.Setup(x => x.GetTokenAsync(It.IsAny<string>(), contact, CancellationToken.None)).Returns(Task.FromResult(new VesselMovementsAuthTokenResult { Token = vmToken }));

			var expectedUrl = new Uri("https://wisegrid.net/mapurl");
			mockVesselMovementsUrlGenerator
				.Setup(x => x.Generate(vmToken, It.Is<Transport>(t => t.PK == expectedTransport.PK), VesselMovementsUrlGeneratorOptions.CollapseInfoPanel | VesselMovementsUrlGeneratorOptions.CollapsePortCallsPanel))
				.Returns((expectedUrl, string.Empty));

			var helper = new TransportOrderHelper(shipment.Transports);
			(var url, var errorMessage) = service.GetActiveTransportMapUrl(helper, contact, CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertEquals(expectedUrl, url);
				AssertNull(errorMessage);
			});
		}

		[TestDate(2019, 7, 20)]
		[TestUtcOffset(13, 0, 0)]
		public void TestGetActiveTransportMapUrl_UtcTime()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			AddNewTransportForTest(shipment.Transports, Core.Constants.TransportModes.Sea, "1234567", new ZDateTime(2019, 7, 10, 1, 2, 3), new ZDateTime(2019, 7, 19, 1, 2, 3));
			var expectedTransport = AddNewTransportForTest(shipment.Transports, Core.Constants.TransportModes.Sea, "8181812", new ZDateTime(2019, 7, 20, 1, 2, 3), new ZDateTime(2019, 7, 31, 1, 2, 3));
			AddNewTransportForTest(shipment.Transports, Core.Constants.TransportModes.Sea, "9191917", new ZDateTime(2019, 8, 1, 1, 2, 3), new ZDateTime(2019, 8, 11, 1, 2, 3));
			Factory.Save();

			mockVesselMovementsUrlGenerator.Setup(x => x.GetTokenAsync(It.IsAny<string>(), contact, CancellationToken.None)).Returns(Task.FromResult(new VesselMovementsAuthTokenResult { Token = vmToken }));

			var expectedUrl = new Uri("https://wisegrid.net/mapurl");
			mockVesselMovementsUrlGenerator
				.Setup(x => x.Generate(vmToken, It.Is<Transport>(t => t.PK == expectedTransport.PK), VesselMovementsUrlGeneratorOptions.CollapseInfoPanel | VesselMovementsUrlGeneratorOptions.CollapsePortCallsPanel))
				.Returns((expectedUrl, string.Empty));

			var helper = new TransportOrderHelper(shipment.Transports);
			(var url, var errorMessage) = service.GetActiveTransportMapUrl(helper, contact, CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertEquals(expectedUrl, url);
				AssertNull(errorMessage);
			});
		}

		[TestDate(2019, 6, 1)]
		public void TestGetActiveTransportMapUrl_AllDatesInFuture()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var expectedTransport = AddNewTransportForTest(shipment.Transports, Core.Constants.TransportModes.Sea, "1234567", new ZDateTime(2019, 7, 10, 1, 2, 3), new ZDateTime(2019, 7, 19, 1, 2, 3));
			AddNewTransportForTest(shipment.Transports, Core.Constants.TransportModes.Sea, "8181812", new ZDateTime(2019, 7, 20, 1, 2, 3), new ZDateTime(2019, 7, 31, 1, 2, 3));
			AddNewTransportForTest(shipment.Transports, Core.Constants.TransportModes.Sea, "9191917", new ZDateTime(2019, 8, 1, 1, 2, 3), new ZDateTime(2019, 8, 11, 1, 2, 3));
			Factory.Save();

			mockVesselMovementsUrlGenerator.Setup(x => x.GetTokenAsync(It.IsAny<string>(), contact, CancellationToken.None)).Returns(Task.FromResult(new VesselMovementsAuthTokenResult { Token = vmToken }));

			var expectedUrl = new Uri("https://wisegrid.net/mapurl");
			mockVesselMovementsUrlGenerator
				.Setup(x => x.Generate(vmToken, It.Is<Transport>(t => t.PK == expectedTransport.PK), VesselMovementsUrlGeneratorOptions.CollapseInfoPanel | VesselMovementsUrlGeneratorOptions.CollapsePortCallsPanel))
				.Returns((expectedUrl, string.Empty));

			var helper = new TransportOrderHelper(shipment.Transports);
			(var url, var errorMessage) = service.GetActiveTransportMapUrl(helper, contact, CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertEquals(expectedUrl, url);
				AssertNull(errorMessage);
			});
		}

		[TestDate(2019, 9, 1)]
		public void TestGetActiveTransportMapUrl_AllDatesInPast()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			AddNewTransportForTest(shipment.Transports, Core.Constants.TransportModes.Sea, "1234567", new ZDateTime(2019, 7, 10, 1, 2, 3), new ZDateTime(2019, 7, 19, 1, 2, 3));
			AddNewTransportForTest(shipment.Transports, Core.Constants.TransportModes.Sea, "8181812", new ZDateTime(2019, 7, 20, 1, 2, 3), new ZDateTime(2019, 7, 31, 1, 2, 3));
			var expectedTransport = AddNewTransportForTest(shipment.Transports, Core.Constants.TransportModes.Sea, "9191917", new ZDateTime(2019, 8, 1, 1, 2, 3), new ZDateTime(2019, 8, 11, 1, 2, 3));
			Factory.Save();

			mockVesselMovementsUrlGenerator.Setup(x => x.GetTokenAsync(It.IsAny<string>(), contact, CancellationToken.None)).Returns(Task.FromResult(new VesselMovementsAuthTokenResult { Token = vmToken }));

			var expectedUrl = new Uri("https://wisegrid.net/mapurl");
			mockVesselMovementsUrlGenerator
				.Setup(x => x.Generate(vmToken, It.Is<Transport>(t => t.PK == expectedTransport.PK), VesselMovementsUrlGeneratorOptions.CollapseInfoPanel | VesselMovementsUrlGeneratorOptions.CollapsePortCallsPanel))
				.Returns((expectedUrl, string.Empty));

			var helper = new TransportOrderHelper(shipment.Transports);
			(var url, var errorMessage) = service.GetActiveTransportMapUrl(helper, contact, CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertEquals(expectedUrl, url);
				AssertNull(errorMessage);
			});
		}

		[TestDate(2019, 9, 1)]
		public void TestGetActiveTransportMapUrl_UseSeaTransports()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			AddNewTransportForTest(shipment.Transports, Core.Constants.TransportModes.Sea, "1234567", new ZDateTime(2019, 7, 10, 1, 2, 3), new ZDateTime(2019, 7, 19, 1, 2, 3));
			var expectedTransport = AddNewTransportForTest(shipment.Transports, Core.Constants.TransportModes.Sea, "8181812", new ZDateTime(2019, 7, 20, 1, 2, 3), new ZDateTime(2019, 7, 31, 1, 2, 3));
			AddNewTransportForTest(shipment.Transports, Core.Constants.TransportModes.Rail, "9191917", new ZDateTime(2019, 8, 1, 1, 2, 3), new ZDateTime(2019, 8, 11, 1, 2, 3));
			Factory.Save();

			mockVesselMovementsUrlGenerator.Setup(x => x.GetTokenAsync(It.IsAny<string>(), contact, CancellationToken.None)).Returns(Task.FromResult(new VesselMovementsAuthTokenResult { Token = vmToken }));

			var expectedUrl = new Uri("https://wisegrid.net/mapurl");
			mockVesselMovementsUrlGenerator
				.Setup(x => x.Generate(vmToken, It.Is<Transport>(t => t.PK == expectedTransport.PK), VesselMovementsUrlGeneratorOptions.CollapseInfoPanel | VesselMovementsUrlGeneratorOptions.CollapsePortCallsPanel))
				.Returns((expectedUrl, string.Empty));

			var helper = new TransportOrderHelper(shipment.Transports);
			(var url, var errorMessage) = service.GetActiveTransportMapUrl(helper, contact, CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertEquals(expectedUrl, url);
				AssertNull(errorMessage);
			});
		}

		[TestDate(2019, 9, 1)]
		public void TestGetActiveTransportMapUrl_UseAirTransports()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001234";
			AddNewTransportForTest(consol.Transports, Core.Constants.TransportModes.Air, "1234567", new ZDateTime(2019, 7, 10, 1, 2, 3), new ZDateTime(2019, 7, 19, 1, 2, 3));
			var expectedTransport = AddNewTransportForTest(consol.Transports, Core.Constants.TransportModes.Air, "8181812", new ZDateTime(2019, 7, 20, 1, 2, 3), new ZDateTime(2019, 7, 31, 1, 2, 3));
			AddNewTransportForTest(consol.Transports, Core.Constants.TransportModes.Rail, "9191917", new ZDateTime(2019, 8, 1, 1, 2, 3), new ZDateTime(2019, 8, 11, 1, 2, 3));
			Factory.Save();

			mockCargoTrackerUrlGenerator.Setup(x => x.GetToken(It.IsAny<string>(), contact, CancellationToken.None)).Returns(new CargoTrackerAuthTokenResult { Token = ctToken });

			var expectedUrl = new Uri("https://cargotracker.net/mapurl");
			mockCargoTrackerUrlGenerator
				.Setup(x => x.Generate(ctToken, licenseCode, "C00001234"))
				.Returns((expectedUrl, string.Empty));

			var helper = new TransportOrderHelper(consol.Transports);
			(var url, var errorMessage) = service.GetActiveTransportMapUrl(helper, contact, CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertEquals(expectedUrl, url);
				AssertNull(errorMessage);
			});
		}

		public void TestGetOrderMapUrl_NoAuthTokenCanBeObtained()
		{
			var order = Factory.NewWithValidTestData<Order>();
			Factory.Save();

			mockVesselMovementsUrlGenerator.Setup(x => x.GetTokenAsync(It.IsAny<string>(), contact, CancellationToken.None)).Returns(Task.FromResult(new VesselMovementsAuthTokenResult { ErrorMessage = "Unable to connect to the authentication service, try again later." }));

			(var url, var errorMessage) = service.GetOrderMapUrl(order, contact, CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertNull(url);
				AssertEquals("Unable to show map: Unable to connect to the authentication service, try again later.", errorMessage);
			});
		}

		public void TestGetOrderMapUrl_TokenRequiresRedirect()
		{
			var order = Factory.NewWithValidTestData<Order>();
			Factory.Save();

			var myAccountUrl = new Uri("http://myaccount.com/verifyyouremail");
			mockVesselMovementsUrlGenerator.Setup(x => x.GetTokenAsync(It.IsAny<string>(), contact, CancellationToken.None)).Returns(Task.FromResult(new VesselMovementsAuthTokenResult { RedirectUrl = myAccountUrl }));

			(var url, var errorMessage) = service.GetOrderMapUrl(order, contact, CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertEquals(myAccountUrl, url);
				AssertNull(errorMessage);
			});
		}

		public void TestGetOrderMapUrl_GeneratorError()
		{
			var order = Factory.NewWithValidTestData<Order>();
			Factory.Save();

			mockVesselMovementsUrlGenerator.Setup(x => x.GetTokenAsync(It.IsAny<string>(), contact, CancellationToken.None)).Returns(Task.FromResult(new VesselMovementsAuthTokenResult { Token = vmToken }));

			mockVesselMovementsUrlGenerator
				.Setup(x => x.Generate(vmToken, It.Is<OrderVesselMovementsUrlHelper>(h => h.Order.PK == order.PK), VesselMovementsUrlGeneratorOptions.CollapseInfoPanel | VesselMovementsUrlGeneratorOptions.CollapsePortCallsPanel))
				.Returns((null, "Error from generator."));

			(var url, var errorMessage) = service.GetOrderMapUrl(order, contact, CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertNull(url);
				AssertEquals("Unable to show map: Error from generator.", errorMessage);
			});
		}

		public void TestGetOrderMapUrl()
		{
			var order = Factory.NewWithValidTestData<Order>();
			Factory.Save();

			mockVesselMovementsUrlGenerator.Setup(x => x.GetTokenAsync(It.IsAny<string>(), contact, CancellationToken.None)).Returns(Task.FromResult(new VesselMovementsAuthTokenResult { Token = vmToken }));

			var expectedUrl = new Uri("https://wisegrid.net/mapurl");
			mockVesselMovementsUrlGenerator
				.Setup(x => x.Generate(vmToken, It.Is<OrderVesselMovementsUrlHelper>(h => h.Order.PK == order.PK), VesselMovementsUrlGeneratorOptions.CollapseInfoPanel | VesselMovementsUrlGeneratorOptions.CollapsePortCallsPanel))
				.Returns((expectedUrl, string.Empty));

			(var url, var errorMessage) = service.GetOrderMapUrl(order, contact, CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertEquals(expectedUrl, url);
				AssertNull(errorMessage);
			});
		}

		static Transport AddNewTransportForTest(TransportCollection transports, string transportMode, string lloydsNumber, ZDateTime departureTime, ZDateTime arrivalTime, bool setEstimates = false)
		{
			var transport = transports.AddNew();
			transport.JW_TransportMode = transportMode;
			var vessel = transports.Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = lloydsNumber;
			transport.JW_Vessel = vessel.RV_Code;
			if (setEstimates)
			{
				transport.JW_ETD = departureTime;
				transport.JW_ETA = arrivalTime;
			}
			else
			{
				transport.JW_ATD = departureTime;
				transport.JW_ATA = arrivalTime;
			}

			return transport;
		}

		protected override void SetUp()
		{
			service = new TrackingMapUrlService();

			mockVesselMovementsUrlGenerator = new Mock<IVesselMovementsUrlGenerator>();
			ObjectFactory.Substitute(nameof(IVesselMovementsUrlGenerator), mockVesselMovementsUrlGenerator.Object);

			mockCargoTrackerUrlGenerator = new Mock<ICargoTrackerUrlGenerator>();
			ObjectFactory.Substitute(nameof(ICargoTrackerUrlGenerator), mockCargoTrackerUrlGenerator.Object);

			contact = Factory.NewWithValidTestData<OrgContact>();

			vmToken = new VesselMovementsAuthToken(VesselMovementsAuthTokenType.Rating, "vesselMovementsAuthToken");
			ctToken = new CargoTrackerAuthToken(CargoTrackerAuthTokenType.Rating, "cargoTrackerAuthToken");

			licenseCode = GlbCompany.CurrentCompany.GetLicenceCode(); // ????
		}

		TrackingMapUrlService service;
		Mock<IVesselMovementsUrlGenerator> mockVesselMovementsUrlGenerator;
		Mock<ICargoTrackerUrlGenerator> mockCargoTrackerUrlGenerator;
		OrgContact contact;
		VesselMovementsAuthToken vmToken;
		CargoTrackerAuthToken ctToken;
		string licenseCode;
	}
}
