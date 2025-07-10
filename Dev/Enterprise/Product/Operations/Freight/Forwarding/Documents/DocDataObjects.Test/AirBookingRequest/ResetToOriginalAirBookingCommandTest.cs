using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class ResetToOriginalAirBookingCommandTest : TestCaseWithFactory
	{
		#region TestInvoke

		const string airBookingDataStoreName = "AirBooking";

		bool hardRefreshCalled;

		public void TestInvoke()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "CCN1406309";
			consol.JK_MasterBillNum = "61873808291";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";

			AssertEquals("prerequisite: consol has 1 transport", 1, consol.Transports.Count);

			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_Status = Core.Constants.TransportStatus.Confirmed;
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUBNE";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport2.JW_Status = Core.Constants.TransportStatus.Confirmed;
			transport2.JW_RL_NKLoadPort = "AUBNE";
			transport2.JW_RL_NKDiscPort = "SGSIN";

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = airBookingDataStoreName;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			documentData.CreateMessageSentLog(AirBookingLogConstants.MessageType);

			Factory.Save();

			var documentInfo = new Mock<IDocumentInfo>();
			var document = new Mock<IDocument>();
			var dynamicData = new Mock<IDynamicData>();
			var securityService = new Mock<IDocumentSecurityService>();
			var notificationService = new Mock<IUserNotificationService>();

			securityService.SetupGet(ss => ss.CanSendMessage).Returns(true);
			notificationService.Setup(ns => ns.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

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

			dynamicData.SetupGet(di => di.Value).Returns(bookingRequest);

			var command = new ResetToOriginalAirBookingCommand();
			command.NotifyDocumentInfoCreated(documentInfo.Object);

			var disposable = broker.GetEvent<DocumentHardRefreshEvent>().Subscribe(_ =>
			{
				hardRefreshCalled = true;
			});

			var res = command.Invoke();

			Assert("Hard refresh should be called", hardRefreshCalled);
			Assert("reset to original has been done", res);

			var consolLogs = GetLogs(consol);

			AssertContainsExactElementsInAnyOrder("consol data logs",
				new[]
				{
					"ADD",
					"PAA",
					"WBA Master Bill Number \"61873808291\" Was Entered",
					"MSN Propagated: All Document Data|DEP=Carrier|MST=Air Booking",
					"STU Propagated: All Document Data|DEP=CargoWise Support|MST=Air Booking|TYP=Reset To Original"
				},
				consolLogs);

			var documentDataLogs = GetLogs(documentData);

			AssertContainsExactElementsInAnyOrder("document data logs",
				new[]
				{
					"MSN |DEP=Carrier|MST=Air Booking",
					"STU |DEP=CargoWise Support|MST=Air Booking|TYP=Reset To Original"
				},
				documentDataLogs);

			AssertContainsExactElementsInAnyOrder("flight statues were updated",
				new[]
				{
					Core.Constants.TransportStatus.Planned,
					Core.Constants.TransportStatus.Planned
				},
				bookingRequest.FlightDetails.Select(f => f.Status.Code));

			AssertEquals("Transport biz obj status has been updated",
				Core.Constants.TransportStatus.Planned, transport1.JW_Status);

			AssertEquals("Transport biz obj status has been updated",
				Core.Constants.TransportStatus.Planned, transport2.JW_Status);
		}

		string[] GetLogs(IStmALogParent logParent)
		{
			return logParent
				.Logs
				.GetAllLogs()
				.Cast<StmALog>()
				.Where(l => l.SL_SE_NKEvent != Events.WorkflowTemplateAppliedCode)
				.OrderBy(l => l.SL_PostedTimeUtc)
				.Select(l => $"{l.SL_SE_NKEvent} {l.SL_Reference}".TrimEnd())
				.ToArray();
		}

		protected override void TearDown()
		{
			base.TearDown();
			AirBookingCarrierConfigurationManager.ClearSupportedAirlinesApplicationCache();
		}

		#endregion
	}
}
