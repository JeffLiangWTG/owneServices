using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.Foundation.Http;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class CancelAirBookingCommandTest : TestCaseWithFactory
	{
		#region TestSend_ISN

		bool hardRefreshCalled;

		public void TestSend_ISN()
		{
			const string requestUrl = "http://test.com/";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "CCN1406309";
			consol.JK_MasterBillNum = "61873808291";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.WeightVerificationUnit = Core.Constants.Weight.Kilograms;
			consol.VolumeVerificationUnit = Core.Constants.Volume.CubicMetres;
			consol.JK_RequiresTemperatureControl = true;
			consol.JK_RequiredTemperatureUnit = "C";
			consol.JK_RequiredTemperatureMinimum = 15;
			consol.JK_RequiredTemperatureMaximum = 25;

			AssertEquals("prerequisite: consol has 1 transport", 1, consol.Transports.Count);

			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_Status = Core.Constants.TransportStatus.Confirmed;
			transport1.JW_VoyageFlight = "EY7";
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUBNE";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport2.JW_Status = Core.Constants.TransportStatus.Confirmed;
			transport2.JW_VoyageFlight = "EY9";
			transport2.JW_RL_NKLoadPort = "AUBNE";
			transport2.JW_RL_NKDiscPort = "SGSIN";

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = airBookingDataStoreName;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			documentData.CreateMessageSentLog(AirBookingLogConstants.MessageType);
			documentData.CreateDataExportEventLog(sentMessageSentContent);

			var eventParameters = new[]
			{
				new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType,
					AirBookingLogConstants.MessageType),

				new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department,
					AirBookingLogConstants.CarrierDepartment)
			};

			documentData.Logs.CreateOrRecreateEventLog(
				Events.BookingConfirmed,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				ZString.Empty,
				eventParameters);

			Factory.Save();

			using (FreightDataRegistry.Instance.CustomEBookingApiServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requestUrl))
			{
				var documentInfo = new Mock<IDocumentInfo>();
				var document = new Mock<IDocument>();
				var dynamicData = new Mock<IDynamicData>();
				var securityService = new Mock<IDocumentSecurityService>();
				var notificationService = new Mock<IUserNotificationService>();

				securityService.SetupGet(ss => ss.CanSendMessage).Returns(true);
				notificationService.Setup(ns => ns.QueryUserResponse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns("reason");

				var services = new ServiceContainer();
				var broker = new EventBroker();
				services.Register<IEventBroker>(broker);
				services.Register<IDocumentSecurityService>(securityService.Object);
				services.Register<IUserNotificationService>(notificationService.Object);

				var bookingRequestBuilder = new AirBookingRequestBuilder(consol);
				var bookingRequest = bookingRequestBuilder.Build();

				AssertContainsExactElementsInAnyOrder("prerequisite: original flight statues",
					new[]
					{
						Core.Constants.TransportStatus.Confirmed,
						Core.Constants.TransportStatus.Confirmed
					},
					bookingRequest.FlightDetails.Select(f => f.Status.Code));

				documentInfo.SetupGet(di => di.Document).Returns(document.Object);
				documentInfo.SetupGet(di => di.DocumentData).Returns(documentData);
				documentInfo.SetupGet(di => di.Services).Returns(services);

				document.SetupGet(di => di.Data).Returns(dynamicData.Object);
				document.SetupGet(di => di.Name).Returns("Air Booking");
				document.Setup(d => d.DataContext).Returns(DataContext.AirBookingRequest);

				dynamicData.SetupGet(di => di.Value).Returns(bookingRequest);

				var handler = TestHandler.Create(HttpStatusCode.OK, eBookingAPIResponse_ISN);
				ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => handler));

				var command = new CancelAirBookingCommand();
				command.NotifyDocumentInfoCreated(documentInfo.Object);

				var disposable = broker.GetEvent<DocumentHardRefreshEvent>().Subscribe(_ =>
				{
					hardRefreshCalled = true;
				});

				hardRefreshCalled = false;

				var res = command.Invoke();

				Assert("Hard refresh should be called", hardRefreshCalled);
				Assert("Booking cancellation has been sent", res);
				Assert("Booking Cancellation command is disabled", !command.IsEnabled);

				AssertEquals("Method", HttpMethod.Post, handler.Method);
				AssertEquals("RequestUri", "http://test.com/booking/618-73808291/cancel", handler.Url);
				AssertMultilineASCIIEquals("Content", expectedRequestContent, handler.Content);//

				var consolLogs = GetLogs(consol);

				AssertContainsExactElementsInAnyOrder("consol data logs",
					new[]
					{
						"MSN Propagated: All Document Data|DEP=Carrier|MST=Air Booking",
						"BKC Propagated: All Document Data|DEP=Carrier|MST=Air Booking",
						"MWR Propagated: All Document Data|DEP=Carrier|MST=Air Booking|RES=reason",
						"ISN |DEP=WiseTech Global|MST=Air Booking|RFN=618-73808291"
					},
					consolLogs);

				var documentDataLogs = GetLogs(documentData);

				AssertContainsExactElementsInAnyOrder("document data logs",
					new[]
					{
						"MSN |DEP=Carrier|MST=Air Booking",
						"DEX",
						"BKC |DEP=Carrier|MST=Air Booking",
						"MWR |DEP=Carrier|MST=Air Booking|RES=reason",
						"DEX",
						"ISN |DEP=WiseTech Global|MST=Air Booking|RFN=618-73808291"
					},
					documentDataLogs);

				AssertContainsExactElementsInAnyOrder("flight statues were updated",
					new[]
					{
						Core.Constants.TransportStatus.CancellationRequested,
						Core.Constants.TransportStatus.CancellationRequested
					},
					bookingRequest.FlightDetails.Select(f => f.Status.Code));
			}

			AssertEquals("Transport biz obj status has been updated",
				Core.Constants.TransportStatus.CancellationRequested, transport1.JW_Status);

			AssertEquals("Transport biz obj status has been updated",
				Core.Constants.TransportStatus.CancellationRequested, transport2.JW_Status);
		}

		const string eBookingAPIResponse_ISN = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Header>
		<SenderID>Air_Booking_Engine</SenderID>
	</Header>
	<Body>
		<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
			<Event>
				<DataContext>
					<DocumentaryOverride>
						<DocumentName>AirBooking</DocumentName>
						<SubmissionVersion>1</SubmissionVersion>
					</DocumentaryOverride>
					<DataTargetCollection>
						<DataTarget>
							<Key>CCN1406309</Key>
							<Type>ForwardingConsol</Type>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<EventType>ISN</EventType>
				<EventParameters>
					<Department>WiseTech Global</Department>
					<MessageType>Air Booking</MessageType>
					<ReferenceNumber>618-73808291</ReferenceNumber>
				</EventParameters>
				<EventTime>2020-04-08T10:25:21</EventTime>
				<DataContext/>
				<ContextCollection>
					<Context>
						<Type>MAWBNumber</Type>
						<Value>618-73808291</Value>
					</Context>
					<Context>
						<Type>MAWBOriginIATAAirportCode</Type>
						<Value>SYD</Value>
					</Context>
					<Context>
						<Type>MAWBDestinationIATAAirportCode</Type>
						<Value>SIN</Value>
					</Context>
				</ContextCollection>
			</Event>
		</UniversalEvent>
	</Body>
</UniversalInterchange>";

		#endregion

		#region TestSend_IRJ

		public void TestSend_IRJ()
		{
			const string requestUrl = "http://test.com/";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "CCN1406309";
			consol.JK_MasterBillNum = "61873808291";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.WeightVerificationUnit = Core.Constants.Weight.Kilograms;
			consol.VolumeVerificationUnit = Core.Constants.Volume.CubicMetres;
			consol.JK_RequiresTemperatureControl = true;
			consol.JK_RequiredTemperatureUnit = "C";
			consol.JK_RequiredTemperatureMinimum = 15;
			consol.JK_RequiredTemperatureMaximum = 25;

			AssertEquals("prerequisite: consol has 1 transport", 1, consol.Transports.Count);

			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_Status = Core.Constants.TransportStatus.Confirmed;
			transport1.JW_VoyageFlight = "EY7";
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUBNE";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport2.JW_Status = Core.Constants.TransportStatus.Confirmed;
			transport2.JW_VoyageFlight = "EY9";
			transport2.JW_RL_NKLoadPort = "AUBNE";
			transport2.JW_RL_NKDiscPort = "SGSIN";

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = airBookingDataStoreName;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			documentData.CreateMessageSentLog(AirBookingLogConstants.MessageType);
			documentData.CreateDataExportEventLog(sentMessageSentContent);

			var eventParameters = new[]
			{
				new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType,
					AirBookingLogConstants.MessageType),

				new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department,
					AirBookingLogConstants.CarrierDepartment)
			};

			documentData.Logs.CreateOrRecreateEventLog(
				Events.BookingConfirmed,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				ZString.Empty,
				eventParameters);

			Factory.Save();

			using (FreightDataRegistry.Instance.CustomEBookingApiServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requestUrl))
			{
				var documentInfo = new Mock<IDocumentInfo>();
				var document = new Mock<IDocument>();
				var dynamicData = new Mock<IDynamicData>();
				var securityService = new Mock<IDocumentSecurityService>();
				var notificationService = new Mock<IUserNotificationService>();

				securityService.SetupGet(ss => ss.CanSendMessage).Returns(true);
				notificationService.Setup(ns => ns.QueryUserResponse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns("reason");

				var services = new ServiceContainer();
				var broker = new EventBroker();
				services.Register<IEventBroker>(broker);
				services.Register<IDocumentSecurityService>(securityService.Object);
				services.Register<IUserNotificationService>(notificationService.Object);

				var bookingRequestBuilder = new AirBookingRequestBuilder(consol);
				var bookingRequest = bookingRequestBuilder.Build();

				AssertContainsExactElementsInAnyOrder("prerequisite: original flight statues",
					new[]
					{
						Core.Constants.TransportStatus.Confirmed,
						Core.Constants.TransportStatus.Confirmed
					},
					bookingRequest.FlightDetails.Select(f => f.Status.Code));

				documentInfo.SetupGet(di => di.Document).Returns(document.Object);
				documentInfo.SetupGet(di => di.DocumentData).Returns(documentData);
				documentInfo.SetupGet(di => di.Services).Returns(services);

				document.SetupGet(di => di.Data).Returns(dynamicData.Object);
				document.SetupGet(di => di.Name).Returns("Air Booking");
				document.Setup(d => d.DataContext).Returns(DataContext.AirBookingRequest);

				dynamicData.SetupGet(di => di.Value).Returns(bookingRequest);

				var handler = TestHandler.Create(HttpStatusCode.OK, eBookingAPIResponse_IRJ);
				ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => handler));
				var command = new CancelAirBookingCommand();
				command.NotifyDocumentInfoCreated(documentInfo.Object);

				var res = command.Invoke();

				Assert("Booking cancellation has been sent", res);
				Assert("Booking Cancellation command is enabled", command.IsEnabled);

				AssertEquals("Method", HttpMethod.Post, handler.Method);
				AssertEquals("RequestUri", "http://test.com/booking/618-73808291/cancel", handler.Url);
				AssertMultilineASCIIEquals("Content", expectedRequestContent, handler.Content);

				var consolLogs = GetLogs(consol);

				AssertContainsExactElementsInAnyOrder("consol data logs",
					new[]
					{
						"MSN Propagated: All Document Data|DEP=Carrier|MST=Air Booking",
						"BKC Propagated: All Document Data|DEP=Carrier|MST=Air Booking",
						"MWR Propagated: All Document Data|DEP=Carrier|MST=Air Booking|RES=reason",
						"IRJ |DEP=WiseTech Global|MST=Air Booking|RES=[ERROR MESSAGE]|RFN=618-73808291"
					},
					consolLogs);

				var documentDataLogs = GetLogs(documentData);

				AssertContainsExactElementsInAnyOrder("document data logs",
					new[]
					{
						"MSN |DEP=Carrier|MST=Air Booking",
						"DEX",
						"BKC |DEP=Carrier|MST=Air Booking",
						"MWR |DEP=Carrier|MST=Air Booking|RES=reason",
						"DEX",
						"IRJ |DEP=WiseTech Global|MST=Air Booking|RES=[ERROR MESSAGE]|RFN=618-73808291"
					},
					documentDataLogs);

				AssertContainsExactElementsInAnyOrder("flight statues were not updated",
					new[]
					{
						Core.Constants.TransportStatus.Confirmed,
						Core.Constants.TransportStatus.Confirmed
					},
					bookingRequest.FlightDetails.Select(f => f.Status.Code));
			}

			AssertEquals("Transport biz obj status has not been updated",
				Core.Constants.TransportStatus.Confirmed, transport1.JW_Status);

			AssertEquals("Transport biz obj status has not been updated",
				Core.Constants.TransportStatus.Confirmed, transport2.JW_Status);
		}

		const string eBookingAPIResponse_IRJ = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Header>
		<SenderID>Air_Booking_Engine</SenderID>
	</Header>
	<Body>
		<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
			<Event>
				<DataContext>
					<DocumentaryOverride>
						<DocumentName>AirBooking</DocumentName>
						<SubmissionVersion>1</SubmissionVersion>
					</DocumentaryOverride>
					<DataTargetCollection>
						<DataTarget>
							<Key>CCN1406309</Key>
							<Type>ForwardingConsol</Type>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<EventType>IRJ</EventType>
				<EventParameters>
					<Department>WiseTech Global</Department>
					<MessageType>Air Booking</MessageType>
					<ReferenceNumber>618-73808291</ReferenceNumber>
					<Reason>[ERROR MESSAGE]</Reason>
				</EventParameters>
				<EventTime>2020-04-08T10:25:21</EventTime>
				<DataContext/>
				<ContextCollection>
					<Context>
						<Type>MAWBNumber</Type>
						<Value>618-73808291</Value>
					</Context>
					<Context>
						<Type>MAWBOriginIATAAirportCode</Type>
						<Value>SYD</Value>
					</Context>
					<Context>
						<Type>MAWBDestinationIATAAirportCode</Type>
						<Value>SIN</Value>
					</Context>
					<Context>
						<Type>NumberOfPieces</Type>
						<Value>1</Value>
					</Context>
					<Context>
						<Type>WeightOfGoods</Type>
						<Value>1400</Value>
					</Context>
				</ContextCollection>
			</Event>
		</UniversalEvent>
	</Body>
</UniversalInterchange>";

		#endregion

		#region TestSend_BKL

		public void TestSend_BKL()
		{
			AssertSend_BKL(false);
		}

		public void TestSend_BKL_AirlineHasTermsAndConditons()
		{
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

			if (!AirBookingTestHelper.SetAirlineTermsAndConditions(Factory, "618", "terms & conditions from airline\r\nhttp://some-link.com"))
			{
				Fail("Prereq: failed to set up T&Cs for test");
			}

			AssertSend_BKL(true);
		}

		void AssertSend_BKL(bool saveToEDocs)
		{
			const string requestUrl = "http://test.com/";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "CCN1406309";
			consol.JK_MasterBillNum = "61873808291";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.WeightVerificationUnit = Core.Constants.Weight.Kilograms;
			consol.VolumeVerificationUnit = Core.Constants.Volume.CubicMetres;
			consol.JK_RequiresTemperatureControl = true;
			consol.JK_RequiredTemperatureUnit = "C";
			consol.JK_RequiredTemperatureMinimum = 15;
			consol.JK_RequiredTemperatureMaximum = 25;

			AssertEquals("prerequisite: consol has 1 transport", 1, consol.Transports.Count);

			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_VoyageFlight = "EY7";
			transport1.JW_Status = Core.Constants.TransportStatus.Confirmed;
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUBNE";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport2.JW_VoyageFlight = "EY9";
			transport2.JW_Status = Core.Constants.TransportStatus.Confirmed;
			transport2.JW_RL_NKLoadPort = "AUBNE";
			transport2.JW_RL_NKDiscPort = "SGSIN";

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = airBookingDataStoreName;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			documentData.CreateMessageSentLog(AirBookingLogConstants.MessageType);
			documentData.CreateDataExportEventLog(sentMessageSentContent);

			var eventParameters = new[]
			{
				new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType,
					AirBookingLogConstants.MessageType),

				new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department,
					AirBookingLogConstants.CarrierDepartment)
			};

			documentData.Logs.CreateOrRecreateEventLog(
				Events.BookingConfirmed,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				ZString.Empty,
				eventParameters);

			Factory.Save();

			using (FreightDataRegistry.Instance.CustomEBookingApiServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requestUrl))
			{
				var documentInfo = new Mock<IDocumentInfo>();
				var descriptor = new Mock<IDocumentDescriptor>();
				var printInstructions = new Mock<IPrintInstructions>();
				var eDocsInstructions = new Mock<IEDocsInstructions>();
				var dynamicData = new Mock<IDynamicData>();

				var document = new DummyBookingRequestDocument(dynamicData.Object);

				var securityService = new Mock<IDocumentSecurityService>();
				var notificationService = new Mock<IUserNotificationService>();

				securityService.SetupGet(ss => ss.CanSendMessage).Returns(true);
				notificationService.Setup(ns => ns.QueryUserResponse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns("reason");

				var services = new ServiceContainer();
				var broker = new EventBroker();
				services.Register<IEventBroker>(broker);
				services.Register<IDocumentSecurityService>(securityService.Object);
				services.Register<IUserNotificationService>(notificationService.Object);

				var bookingRequestBuilder = new AirBookingRequestBuilder(consol);
				var bookingRequest = bookingRequestBuilder.Build();

				AssertContainsExactElementsInAnyOrder("prerequisite: original flight statues",
					new[]
					{
						Core.Constants.TransportStatus.Confirmed,
						Core.Constants.TransportStatus.Confirmed
					},
					bookingRequest.FlightDetails.Select(f => f.Status.Code));

				documentInfo.SetupGet(di => di.Document).Returns(document);
				documentInfo.SetupGet(di => di.Descriptor).Returns(descriptor.Object);
				documentInfo.SetupGet(di => di.DocumentData).Returns(documentData);
				documentInfo.SetupGet(di => di.Services).Returns(services);

				descriptor.SetupGet(di => di.Name).Returns("eBooking");
				descriptor.SetupGet(di => di.DocumentType).Returns("BKC");
				descriptor.SetupGet(di => di.PrintInstructions).Returns(printInstructions.Object);
				descriptor.SetupGet(di => di.EDocsInstructions).Returns(eDocsInstructions.Object);

				printInstructions.SetupGet(pi => pi.Title).Returns("eBooking Request");

				eDocsInstructions.SetupGet(edoc => edoc.SaveCopyToEDocs).Returns(false);
				eDocsInstructions.SetupGet(edoc => edoc.Parent).Returns(consol);

				dynamicData.SetupGet(di => di.Value).Returns(bookingRequest);

				var handler = TestHandler.Create(HttpStatusCode.OK, eBookingAPIResponse_BKL);
				ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => handler));

				var command = new CancelAirBookingCommand();
				command.NotifyDocumentInfoCreated(documentInfo.Object);

				var res = command.Invoke();

				Assert("Booking cancellation has been sent", res);
				Assert("Booking Cancellation command is disabled", !command.IsEnabled);

				AssertEquals("Method", HttpMethod.Post, handler.Method);
				AssertEquals("RequestUri", "http://test.com/booking/618-73808291/cancel", handler.Url);
				AssertMultilineASCIIEquals("Content", expectedRequestContent, handler.Content);

				var consolLogs = GetLogs(consol);

				AssertContainsExactElementsInAnyOrder("consol data logs",
					new[]
					{
						"MSN Propagated: All Document Data|DEP=Carrier|MST=Air Booking",
						"BKC Propagated: All Document Data|DEP=Carrier|MST=Air Booking",
						"MWR Propagated: All Document Data|DEP=Carrier|MST=Air Booking|RES=reason",
						"BKL Propagated: All Document Data|DEP=Carrier|FDT=2020-05-09|LOC=AUSYD|MST=Air Booking|RES=[MESSAGE IF AVAILABLE]|TYP=AWB|VFL=EY7",
						"BKL Propagated: All Document Data|DEP=Carrier|FDT=2020-05-09|LOC=AUBNE|MST=Air Booking|RES=[MESSAGE IF AVAILABLE]|TYP=AWB|VFL=EY9"
					},
					consolLogs);

				var documentDataLogs = GetLogs(documentData);

				AssertContainsExactElementsInAnyOrder("document data logs",
					new[]
					{
						"MSN |DEP=Carrier|MST=Air Booking",
						"DEX",
						"BKC |DEP=Carrier|MST=Air Booking",
						"MWR |DEP=Carrier|MST=Air Booking|RES=reason",
						"DEX",
						"BKL |DEP=Carrier|FDT=2020-05-09|LOC=AUSYD|MST=Air Booking|RES=[MESSAGE IF AVAILABLE]|TYP=AWB|VFL=EY7",
						"BKL |DEP=Carrier|FDT=2020-05-09|LOC=AUBNE|MST=Air Booking|RES=[MESSAGE IF AVAILABLE]|TYP=AWB|VFL=EY9"
					},
					documentDataLogs);

				AssertContainsExactElementsInAnyOrder("flight statues were updated",
					new[]
					{
						Core.Constants.TransportStatus.Cancelled,
						Core.Constants.TransportStatus.Cancelled
					},
					bookingRequest.FlightDetails.Select(f => f.Status.Code));
			}

			AssertEquals("Transport biz obj status has been updated",
				Core.Constants.TransportStatus.Cancelled, transport1.JW_Status);

			AssertEquals("Transport biz obj status has been updated",
				Core.Constants.TransportStatus.Cancelled, transport2.JW_Status);

			var printJobsQuery = new ZQuery();
			printJobsQuery.AddToFilter(StmPrintJobSchema.SP_ParentTableName, JobConsolSchema.Constants.TableName);
			printJobsQuery.AddToFilter(StmPrintJobSchema.SP_ParentGuid, consol.PK);

			var printJobs = Factory.Load<StmPrintJob>(printJobsQuery);

			if (saveToEDocs)
			{
				AssertEquals("Document was added to eDocs", 1, printJobs.Length);
				AssertEquals("SP_EmailSubjectLine will become eDoc description",
					"Eagle Datamation International - BN - AUBNE - Air Booking Cancellation (1)", printJobs[0].SP_EmailSubjectLine);
			}
			else
			{
				AssertEquals("Document was not added to eDocs", 0, printJobs.Length);
			}
		}

		const string eBookingAPIResponse_BKL = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Header>
		<SenderID>Air_Booking_Engine</SenderID>
	</Header>
	<Body>
		<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
			<Event>
				<DataContext>
					<DocumentaryOverride>
						<DocumentName>AirBooking</DocumentName>
						<SubmissionVersion>1</SubmissionVersion>
					</DocumentaryOverride>
					<DataTargetCollection>
						<DataTarget>
							<Key>CCN1406309</Key>
							<Type>ForwardingConsol</Type>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<EventTime>2020-05-05T09:46:00</EventTime>
				<EventType>BKL</EventType>
				<EventParameters>
					<Department>Carrier</Department>
					<Type>AWB</Type>
					<Location>SYD</Location>
					<VoyageFlightNumber>EY7</VoyageFlightNumber>
					<FlightDate>2020-05-09</FlightDate>
					<Reason>[MESSAGE IF AVAILABLE]</Reason>
					<MessageType>Air Booking</MessageType>
				</EventParameters>
				<ContextCollection>
					<Context>
						<Type>MAWBNumber</Type>
						<Value>618-73808291</Value>
					</Context>
					<Context>
						<Type>MAWBOriginIATAAirportCode</Type>
						<Value>SYD</Value>
					</Context>
					<Context>
						<Type>MAWBDestinationIATAAirportCode</Type>
						<Value>SIN</Value>
					</Context>
					<Context>
						<Type>OriginIATAAirportCode</Type>
						<Value>SYD</Value>
					</Context>
					<Context>
						<Type>DestinationIATAAirportCode</Type>
						<Value>BNE</Value>
					</Context>
					<Context>
						<Type>FlightNumber</Type>
						<Value>EY7</Value>
					</Context>
					<Context>
						<Type>FlightDate</Type>
						<Value>2020-05-09</Value>
					</Context>
					<Context>
						<Type>BookingStatus</Type>
						<Value>CAN</Value>
					</Context>
				</ContextCollection>
			</Event>
		</UniversalEvent>

		<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
			<Event>
				<DataContext>
					<DocumentaryOverride>
						<DocumentName>AirBooking</DocumentName>
						<SubmissionVersion>1</SubmissionVersion>
					</DocumentaryOverride>
					<DataTargetCollection>
						<DataTarget>
							<Key>CCN1406309</Key>
							<Type>ForwardingConsol</Type>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<EventTime>2020-05-05T09:46:00</EventTime>
				<EventType>BKL</EventType>
				<EventParameters>
					<Department>Carrier</Department>
					<Type>AWB</Type>
					<Location>BNE</Location>
					<VoyageFlightNumber>EY9</VoyageFlightNumber>
					<FlightDate>2020-05-09</FlightDate>
					<Reason>[MESSAGE IF AVAILABLE]</Reason>
					<MessageType>Air Booking</MessageType>
				</EventParameters>
				<ContextCollection>
					<Context>
						<Type>MAWBNumber</Type>
						<Value>618-73808291</Value>
					</Context>
					<Context>
						<Type>MAWBOriginIATAAirportCode</Type>
						<Value>SYD</Value>
					</Context>
					<Context>
						<Type>MAWBDestinationIATAAirportCode</Type>
						<Value>SIN</Value>
					</Context>
					<Context>
						<Type>OriginIATAAirportCode</Type>
						<Value>BNE</Value>
					</Context>
					<Context>
						<Type>DestinationIATAAirportCode</Type>
						<Value>SIN</Value>
					</Context>
					<Context>
						<Type>FlightNumber</Type>
						<Value>EY9</Value>
					</Context>
					<Context>
						<Type>FlightDate</Type>
						<Value>2020-05-09</Value>
					</Context>
					<Context>
						<Type>BookingStatus</Type>
						<Value>CAN</Value>
					</Context>
				</ContextCollection>
			</Event>
		</UniversalEvent>
	</Body>
</UniversalInterchange>";

		#endregion

		#region TestSend_401

		public void TestSend_401()
		{
			const string requestUrl = "http://test.com/";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "CCN1406309";
			consol.JK_MasterBillNum = "61873808291";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.WeightVerificationUnit = Core.Constants.Weight.Kilograms;
			consol.VolumeVerificationUnit = Core.Constants.Volume.CubicMetres;
			consol.JK_RequiresTemperatureControl = true;
			consol.JK_RequiredTemperatureUnit = "C";
			consol.JK_RequiredTemperatureMinimum = 15;
			consol.JK_RequiredTemperatureMaximum = 25;

			AssertEquals("prerequisite: consol has 1 transport", 1, consol.Transports.Count);

			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_Status = Core.Constants.TransportStatus.Confirmed;
			transport1.JW_VoyageFlight = "EY7";
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUBNE";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport2.JW_Status = Core.Constants.TransportStatus.Confirmed;
			transport2.JW_VoyageFlight = "EY9";
			transport2.JW_RL_NKLoadPort = "AUBNE";
			transport2.JW_RL_NKDiscPort = "SGSIN";

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = airBookingDataStoreName;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			documentData.CreateMessageSentLog(AirBookingLogConstants.MessageType);
			documentData.CreateDataExportEventLog(sentMessageSentContent);

			var eventParameters = new[]
			{
				new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType,
					AirBookingLogConstants.MessageType),

				new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department,
					AirBookingLogConstants.CarrierDepartment)
			};

			documentData.Logs.CreateOrRecreateEventLog(
				Events.BookingConfirmed,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				ZString.Empty,
				eventParameters);

			Factory.Save();

			using (FreightDataRegistry.Instance.CustomEBookingApiServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requestUrl))
			{
				var documentInfo = new Mock<IDocumentInfo>();
				var document = new Mock<IDocument>();
				var dynamicData = new Mock<IDynamicData>();
				var securityService = new Mock<IDocumentSecurityService>();
				var notificationService = new Mock<IUserNotificationService>();

				securityService.SetupGet(ss => ss.CanSendMessage).Returns(true);
				notificationService.Setup(ns => ns.QueryUserResponse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns("reason");

				var services = new ServiceContainer();
				var broker = new EventBroker();
				services.Register<IEventBroker>(broker);
				services.Register<IDocumentSecurityService>(securityService.Object);
				services.Register<IUserNotificationService>(notificationService.Object);

				var bookingRequestBuilder = new AirBookingRequestBuilder(consol);
				var bookingRequest = bookingRequestBuilder.Build();

				AssertContainsExactElementsInAnyOrder("prerequisite: original flight statues",
					new[]
					{
						Core.Constants.TransportStatus.Confirmed,
						Core.Constants.TransportStatus.Confirmed
					},
					bookingRequest.FlightDetails.Select(f => f.Status.Code));

				documentInfo.SetupGet(di => di.Document).Returns(document.Object);
				documentInfo.SetupGet(di => di.DocumentData).Returns(documentData);
				documentInfo.SetupGet(di => di.Services).Returns(services);

				document.SetupGet(di => di.Data).Returns(dynamicData.Object);
				document.SetupGet(di => di.Name).Returns("Air Booking");
				document.Setup(d => d.DataContext).Returns(DataContext.AirBookingRequest);

				dynamicData.SetupGet(di => di.Value).Returns(bookingRequest);

				var handler = TestHandler.Create(HttpStatusCode.Unauthorized);
				ObjectFactory.Substitute<IHttpClientFactory>(new HttpClientFactory(() => handler));

				var command = new CancelAirBookingCommand();
				command.NotifyDocumentInfoCreated(documentInfo.Object);

				var res = command.Invoke();

				Assert("Booking cancellation has not been sent", !res);
				Assert("Booking Cancellation command is enabled", command.IsEnabled);

				AssertEquals("Method", HttpMethod.Post, handler.Method);
				AssertEquals("RequestUri", "http://test.com/booking/618-73808291/cancel", handler.Url);
				AssertMultilineASCIIEquals("Content", expectedRequestContent, handler.Content);

				var consolLogs = GetLogs(consol);

				AssertContainsExactElementsInAnyOrder("consol data logs",
					new[]
					{
						"MSN Propagated: All Document Data|DEP=Carrier|MST=Air Booking",
						"BKC Propagated: All Document Data|DEP=Carrier|MST=Air Booking"
					},
					consolLogs);

				var documentDataLogs = GetLogs(documentData);

				AssertContainsExactElementsInAnyOrder("document data logs",
					new[]
					{
						"MSN |DEP=Carrier|MST=Air Booking",
						"DEX",
						"BKC |DEP=Carrier|MST=Air Booking"
					},
					documentDataLogs);

				AssertContainsExactElementsInAnyOrder("flight statues were not updated",
					new[]
					{
						Core.Constants.TransportStatus.Confirmed,
						Core.Constants.TransportStatus.Confirmed
					},
					bookingRequest.FlightDetails.Select(f => f.Status.Code));
			}

			AssertEquals("Transport biz obj status has not been updated",
				Core.Constants.TransportStatus.Confirmed, transport1.JW_Status);

			AssertEquals("Transport biz obj status has not been updated",
				Core.Constants.TransportStatus.Confirmed, transport2.JW_Status);
		}

		#endregion

		#region Implementation

		const string airBookingDataStoreName = "AirBooking";

		const string sentMessageSentContent = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>EDIEDIDAT</SenderID>
  </Header>
  <Body>
    <UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
      <Shipment>
        <DataContext>
          <DataSource>
            <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
            <Key>CCN1406309</Key>
            <Type>ForwardingConsol</Type>
          </DataSource>
          <DocumentaryOverride>
            <DocumentName>AirBooking</DocumentName>
          </DocumentaryOverride>
          <Workflow>
            <Company>
              <Code>EDI</Code>
              <Country Name=""Australia"">AU</Country>
              <Name>Eagle Datamation International</Name>
            </Company>
            <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
            <EventDepartment Name=""Department"">BRN</EventDepartment>
            <EventUser Name=""CargoWise Support"">E</EventUser>
          </Workflow>
        </DataContext>
        <BookingConfirmationReference>CCN1406309</BookingConfirmationReference>
        <GoodsDescription></GoodsDescription>
        <PortOfDestination Name=""Singapore"">SIN</PortOfDestination>
        <PortOfOrigin Name=""Sydney"">SYD</PortOfOrigin>
        <TotalNoOfPacks>0</TotalNoOfPacks>
        <TotalVolume>0</TotalVolume>
        <TotalVolumeUnit Description=""Cubic Meters"">M3</TotalVolumeUnit>
        <TotalWeight>0</TotalWeight>
        <TotalWeightUnit Description=""Kilograms"">KG</TotalWeightUnit>
        <WayBillNumber>618-73808291</WayBillNumber>
        <WayBillType Description=""Master Waybill"">MWB</WayBillType>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>Airline</AddressType>
            <CompanyName></CompanyName>
            <OrganizationCode></OrganizationCode>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>Agent</AddressType>
            <CompanyName>EDI CUSTOMS BROKERS</CompanyName>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""IATA CASS Number"">CAS</Type>
                <Value></Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
        <TransportLegCollection>
          <TransportLeg>
            <PortOfDischarge Name=""Brisbane"">BNE</PortOfDischarge>
            <PortOfLoading Name=""Sydney"">SYD</PortOfLoading>
            <LegOrder>1</LegOrder>
            <BookingStatus Description=""Requested"">RQD</BookingStatus>
            <EstimatedArrival></EstimatedArrival>
            <EstimatedDeparture></EstimatedDeparture>
            <VoyageFlightNo></VoyageFlightNo>
            <CustomizedFieldCollection>
              <CustomizedField>
                <DataType>String</DataType>
                <Key>AllotmentId</Key>
                <Value></Value>
              </CustomizedField>
            </CustomizedFieldCollection>
          </TransportLeg>
          <TransportLeg>
            <PortOfDischarge Name=""Singapore"">SIN</PortOfDischarge>
            <PortOfLoading Name=""Brisbane"">BNE</PortOfLoading>
            <LegOrder>2</LegOrder>
            <BookingStatus Description=""Requested"">RQD</BookingStatus>
            <EstimatedArrival></EstimatedArrival>
            <EstimatedDeparture></EstimatedDeparture>
            <VoyageFlightNo></VoyageFlightNo>
            <CustomizedFieldCollection>
              <CustomizedField>
                <DataType>String</DataType>
                <Key>AllotmentId</Key>
                <Value></Value>
              </CustomizedField>
            </CustomizedFieldCollection>
          </TransportLeg>
        </TransportLegCollection>
      </Shipment>
    </UniversalShipment>
  </Body>
</UniversalInterchange>";

		const string expectedRequestContent = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>EDIEDIDAT</SenderID>
  </Header>
  <Body>
    <UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
      <Shipment>
        <DataContext>
          <DataSource>
            <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
            <Key>CCN1406309</Key>
            <Type>ForwardingConsol</Type>
          </DataSource>
          <DocumentaryOverride>
            <DocumentName>AirBooking</DocumentName>
          </DocumentaryOverride>
          <Workflow>
            <Company>
              <Code>EDI</Code>
              <Country Name=""Australia"">AU</Country>
              <Name>Eagle Datamation International</Name>
            </Company>
            <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
            <EventDepartment Name=""Department"">BRN</EventDepartment>
            <EventUser Name=""CargoWise Support"">E</EventUser>
          </Workflow>
        </DataContext>
        <BookingConfirmationReference>CCN1406309</BookingConfirmationReference>
        <GoodsDescription></GoodsDescription>
        <PortOfDestination Name=""Singapore"">SIN</PortOfDestination>
        <PortOfOrigin Name=""Sydney"">SYD</PortOfOrigin>
        <RequiredTemperatureMaximum>25</RequiredTemperatureMaximum>
        <RequiredTemperatureMinimum>15</RequiredTemperatureMinimum>
        <RequiredTemperatureUnit Description=""Centigrade"">C</RequiredTemperatureUnit>
        <RequiresTemperatureControl>true</RequiresTemperatureControl>
        <TotalNoOfPacks>0</TotalNoOfPacks>
        <TotalVolume>0</TotalVolume>
        <TotalVolumeUnit Description=""Cubic Meters"">M3</TotalVolumeUnit>
        <TotalWeight>0</TotalWeight>
        <TotalWeightUnit Description=""Kilograms"">KG</TotalWeightUnit>
        <WayBillNumber>618-73808291</WayBillNumber>
        <WayBillType Description=""Master Waybill"">MWB</WayBillType>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>Airline</AddressType>
            <CompanyName></CompanyName>
            <OrganizationCode></OrganizationCode>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>Agent</AddressType>
            <CompanyName>EDI CUSTOMS BROKERS</CompanyName>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""IATA CASS Number"">CAS</Type>
                <Value></Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
        <TransportLegCollection>
          <TransportLeg>
            <PortOfDischarge Name=""Brisbane"">BNE</PortOfDischarge>
            <PortOfLoading Name=""Sydney"">SYD</PortOfLoading>
            <LegOrder>1</LegOrder>
            <BookingStatus Description=""Cancellation Requested"">CRQ</BookingStatus>
            <EstimatedArrival></EstimatedArrival>
            <EstimatedDeparture></EstimatedDeparture>
            <LegType>Flight1</LegType>
            <TransportMode>Air</TransportMode>
            <VoyageFlightNo>EY7</VoyageFlightNo>
            <CustomizedFieldCollection>
              <CustomizedField>
                <DataType>String</DataType>
                <Key>AllotmentId</Key>
                <Value></Value>
              </CustomizedField>
            </CustomizedFieldCollection>
          </TransportLeg>
          <TransportLeg>
            <PortOfDischarge Name=""Singapore"">SIN</PortOfDischarge>
            <PortOfLoading Name=""Brisbane"">BNE</PortOfLoading>
            <LegOrder>2</LegOrder>
            <BookingStatus Description=""Cancellation Requested"">CRQ</BookingStatus>
            <EstimatedArrival></EstimatedArrival>
            <EstimatedDeparture></EstimatedDeparture>
            <LegType>Flight2</LegType>
            <TransportMode>Air</TransportMode>
            <VoyageFlightNo>EY9</VoyageFlightNo>
            <CustomizedFieldCollection>
              <CustomizedField>
                <DataType>String</DataType>
                <Key>AllotmentId</Key>
                <Value></Value>
              </CustomizedField>
            </CustomizedFieldCollection>
          </TransportLeg>
        </TransportLegCollection>
        <NoteCollection>
          <Note>
            <Description>ReasonForMessageCancellation</Description>
            <IsCustomDescription>true</IsCustomDescription>
            <NoteText>reason</NoteText>
          </Note>
        </NoteCollection>
      </Shipment>
    </UniversalShipment>
  </Body>
</UniversalInterchange>";

		string[] GetLogs(IStmALogParent logParent)
		{
			return logParent
				.Logs
				.GetAllLogs()
				.Cast<StmALog>()
				.Where(l => l.SL_SE_NKEvent != Events.WorkflowTemplateAppliedCode
					&& l.SL_SE_NKEvent != Events.SubscriptionRequestedCode
					&& l.SL_SE_NKEvent != Events.PreAllocatedAmountExceededCode
					&& l.SL_SE_NKEvent != Events.AddedARecordToTheSystemCode
					&& l.SL_SE_NKEvent != Events.WaybillBillOfLadingAssignedCode)
				.OrderBy(l => l.SL_PostedTimeUtc)
				.Select(l => $"{l.SL_SE_NKEvent} {l.SL_Reference}".TrimEnd())
				.ToArray();
		}

		IDisposable airBookingProgressManagerSubstitute;

		protected override void SetUp()
		{
			base.SetUp();

			airBookingProgressManagerSubstitute = ObjectFactory.Substitute<IAirBookingProgressManager>(new NoShowAirBookingProgressManager());
		}

		protected override void TearDown()
		{
			base.TearDown();
			ObjectFactory.DisposeSubstitutions();
			airBookingProgressManagerSubstitute?.Dispose();
			AirBookingCarrierConfigurationManager.ClearSupportedAirlinesApplicationCache();
		}

		#endregion
	}
}
