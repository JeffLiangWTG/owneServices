using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.eTail.Documents.Business;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	abstract class HVLVValidateAndSendACASReportProcessorTest : TestCaseWithFactory
	{
		public void TestProcess_GivenShipment_WhenShipmentIsNotAir_ThenIgnore()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.JS_RL_NKDestination = "USLAX";

			GetTestConsignment(shipment.PK, "CONSIGN001");
			GetTestConsignment(shipment.PK, "CONSIGN002");
			GetTestConsignment(shipment.PK, "CONSIGN003");

			var processor = GetSendACASReportProcessor(shipment);
			processor.Process(new NotificationCollection());

			var interchanges = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals("Expected 0 EDIInterchange generated", 0, interchanges.Length);
		}

		public void TestProcess_GivenShipment_WhenShipmentIsNotUS_ThenIgnore()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;

			GetTestConsignment(shipment.PK, "CONSIGN001");
			GetTestConsignment(shipment.PK, "CONSIGN002");
			GetTestConsignment(shipment.PK, "CONSIGN003");

			var processor = GetSendACASReportProcessor(shipment);
			processor.Process(new NotificationCollection());

			var interchanges = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals("Expected 0 EDIInterchange generated", 0, interchanges.Length);
		}

		public void TestProcess_GivenShipment_WhenShipmentHasNoConsignments_ThenIgnore()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKDestination = "USLAX";

			var processor = GetSendACASReportProcessor(shipment);
			processor.Process(new NotificationCollection());

			var interchanges = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals("Expected 0 EDIInterchange generated", 0, interchanges.Length);
		}

		public void TestProcess_GivenShipmentWithInactiveConsignments_ThenDoNoSendACASReportForInactiveConsignments()
		{
			using (Factory.AddDisposableService())
			{
				var country = RefCountry.LoadFromCountryCode(Factory, "US");
				GlbCompany.CurrentCompany.OrgProxy.SetCustomsCode(OrgCusCode.USACodeTypes.ACASOriginatorCode, country, "Code");

				var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKDestination = "USLAX";

				var consignment1 = GetTestConsignment(shipment.PK, "CONSIGN001");
				var consignment2 = GetTestConsignment(shipment.PK, "CONSIGN002");
				consignment1.HVC_IsActive = true;

				Factory.Save();

				var factory2 = new BusinessObjectFactory();
				var reloadedConsignment2 = factory2.Load<HVLVConsignment>(consignment2.PK);
				reloadedConsignment2.HVC_IsActive = false;
				factory2.Save();

				var processor = GetSendACASReportProcessor(shipment);
				processor.Process(new NotificationCollection());

				var interchanges = Factory.Load<EDIInterchange>(new ZQuery());
				AssertEquals("Expected 1 EDIInterchange generated", 1, interchanges.Length);
				AssertEquals("Expected EDIInterchange message to be ACAS type", "ADVANCE_AIR_CARGO_REPORT", interchanges[0].EI_To);

				Assert("Expected EDIInterchange's essage text body to contain CONSIGN001", interchanges[0].EI_InterchangeText.Contains("CONSIGN001"));
			}
		}

		public void TestProcess_GivenShipment()
		{
			using (Factory.AddDisposableService())
			{
				var country = RefCountry.LoadFromCountryCode(Factory, "US");
				GlbCompany.CurrentCompany.OrgProxy.SetCustomsCode(OrgCusCode.USACodeTypes.ACASOriginatorCode, country, "Code");

				var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKDestination = "USLAX";

				GetTestConsignment(shipment.PK, "CONSIGN001");
				GetTestConsignment(shipment.PK, "CONSIGN002");
				GetTestConsignment(shipment.PK, "CONSIGN003");

				Factory.Save();

				var processor = GetSendACASReportProcessor(shipment);
				processor.Process(new NotificationCollection());

				var interchanges = Factory.Load<EDIInterchange>(new ZQuery());
				AssertEquals("Expected 3 EDIInterchange generated", 3, interchanges.Length);

				AssertEquals("Expected EDIInterchange to be an ACAS report", "ADVANCE_AIR_CARGO_REPORT", interchanges[0].EI_To);
				AssertEquals("Expected EDIInterchange to be an ACAS report", "ADVANCE_AIR_CARGO_REPORT", interchanges[1].EI_To);
				AssertEquals("Expected EDIInterchange to be an ACAS report", "ADVANCE_AIR_CARGO_REPORT", interchanges[2].EI_To);

				CombineAssertions("Expected combined EDIInterchange's text body to contain their respective consignment waybill number", () =>
				{
					Assert("CONSIGN001", interchanges.Any(n => n.EI_InterchangeText.Contains("CONSIGN001")));
					Assert("CONSIGN002", interchanges.Any(n => n.EI_InterchangeText.Contains("CONSIGN002")));
					Assert("CONSIGN003", interchanges.Any(n => n.EI_InterchangeText.Contains("CONSIGN003")));
				});
			}
		}

		public void TestProcess_ShouldTryAcquireApplicationLockForShipment()
		{
			var country = RefCountry.LoadFromCountryCode(Factory, "US");
			GlbCompany.CurrentCompany.OrgProxy.SetCustomsCode(OrgCusCode.USACodeTypes.ACASOriginatorCode, country, "Code");

			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKDestination = "USLAX";

			GetTestConsignment(shipment.PK, "CONSIGN001");

			Factory.Save();

			var logs = new NotificationCollection();
			var connection = Db.NewExtraConnectionToMainDb();
			var appLockKey = ("HVLV ACAS Report," + shipment.PK.ToString()).ToUpperInvariant();

			Assert("pre-condition", connection.TryGetLock(appLockKey, out var appLock));
			using (appLock)
			{
				var processor = GetSendACASReportProcessor(shipment);
				processor.Process(logs);
			}

			AssertEquals("Processor should fail because shipment has been locked", "Failed to acquire lock for rows in table JobShipment, data being processed by other user.", logs.Last().Message);
		}

		public void TestAddMVPLogWhenValidationPass()
		{
			using (Factory.AddDisposableService())
			{
				var country = RefCountry.LoadFromCountryCode(Factory, "US");
				GlbCompany.CurrentCompany.OrgProxy.SetCustomsCode(OrgCusCode.USACodeTypes.ACASOriginatorCode, country, "Code");

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_RS_NKServiceLevel = "STD";
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKDestination = "USLAX";

				GetTestConsignment(shipment.PK, "CONSIGN001");
				Factory.Save();

				var processor = GetSendACASReportProcessor(shipment);
				processor.Process(new NotificationCollection());

				var mvpEvent = shipment.Logs.MostRecentLogByEventTime(AutoEvents.MessageValidationPassed);
				AssertNotNull(mvpEvent);
			}
		}

		public void TestAddMVFLogWhenACASValidationFailed_BasicRequirments()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RS_NKServiceLevel = "STD";
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKDestination = "USLAX";

			GetTestConsignment(shipment.PK, "CONSIGN001");
			Factory.Save();

			var processor = GetSendACASReportProcessor(shipment);
			processor.Process(new NotificationCollection());

			var mvfEvent = shipment.Logs.MostRecentLogByEventTime(AutoEvents.MessageValidationFailed);
			AssertNotNull(mvfEvent);
			AssertEquals("|LOC=US|MST=HVLV Advanced Air Cargo Report|RES=Check HVLV Advanced Air Cargo Report Form for errors.", mvfEvent.SL_Reference);
		}

		public void TestAddMVFLogWhenACASValidationFailed_MandatoryPropertyEmpty()
		{
			var country = RefCountry.LoadFromCountryCode(Factory, "US");
			GlbCompany.CurrentCompany.OrgProxy.SetCustomsCode(OrgCusCode.USACodeTypes.ACASOriginatorCode, country, "Code");

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RS_NKServiceLevel = "STD";
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKDestination = "USLAX";

			var consignment = GetTestConsignment(shipment.PK, "CONSIGN001");
			consignment.HVC_GoodsDescription = string.Empty;

			Factory.Save();

			var wrapper = new HVLVConsignmentForACASWrapper(consignment);
			wrapper.RunPreSaveValidation();

			var processor = GetSendACASReportProcessor(shipment);
			processor.Process(new NotificationCollection());

			Assert("Pre-condition: consignment has message error", wrapper.HasMessageErrors);

			var mvfEvent = shipment.Logs.MostRecentLogByEventTime(AutoEvents.MessageValidationFailed);
			AssertNotNull(mvfEvent);
			AssertEquals("|LOC=US|MST=HVLV Advanced Air Cargo Report|RES=Check HVLV Advanced Air Cargo Report Form for errors.", mvfEvent.SL_Reference);
		}

		public void TestAddTFLLogWhenSendACASReportsFailed()
		{
			var country = RefCountry.LoadFromCountryCode(Factory, "US");
			GlbCompany.CurrentCompany.OrgProxy.SetCustomsCode(OrgCusCode.USACodeTypes.ACASOriginatorCode, country, "Code");

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RS_NKServiceLevel = "STD";
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKDestination = "USLAX";

			GetTestConsignment(shipment.PK, "CONSIGN001");
			Factory.Save();

			var processor = GetSendACASReportProcessor(shipment);

			var mockHVLVAirCargoAdvanceScreeningMessageSender = new Mock<HVLVAirCargoAdvanceScreeningMessageSender>(shipment);

			var message = string.Empty;
			mockHVLVAirCargoAdvanceScreeningMessageSender.Setup(mock => mock.TrySendACASReports(It.IsAny<ACASReportAction>(), out message))
				.Callback(new SendACASReportsCallBack((ACASReportAction a, out string msg) => msg = "Failed because of something"))
				.Returns(() => false);

			processor.ACASMessageSender_ForTesting = mockHVLVAirCargoAdvanceScreeningMessageSender.Object;
			processor.Process(new NotificationCollection());

			var tflEvent = shipment.Logs.MostRecentLogByEventTime(AutoEvents.InterchangeFailedToBeSent);
			AssertNotNull(tflEvent);
			AssertEquals("|LOC=US|MST=HVLV Advanced Air Cargo Report|RES=Failed because of something", tflEvent.SL_Reference);
		}

		protected virtual HVLVConsignment GetTestConsignment(ZGuid shipmentPK, string waybillNumber)
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_ConsigneeName = "Anthony";
			consignment.HVC_ConsigneeAddress1 = "Parliament House";
			consignment.HVC_ConsigneeCity = "Canberra";
			consignment.HVC_ConsigneeState = "ACT";
			consignment.HVC_ConsigneePostcode = "2600";
			consignment.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment.HVC_ConsigneeEmail = "Anthony@gmail.com";
			consignment.HVC_ConsigneeMobile = "+1234567890";

			consignment.HVC_ShipperName = "Linus";
			consignment.HVC_ShipperAddress1 = "LTT Studio";
			consignment.HVC_ShipperCity = "Ontario";
			consignment.HVC_ShipperState = "TOR";
			consignment.HVC_ShipperPostcode = "1234";
			consignment.HVC_RN_NKShipperCountryCode = "CA";
			consignment.HVC_ShipperEmail = "Linus@gmail.com";
			consignment.HVC_ShipperMobile = "+0987654321";

			consignment.HVC_GoodsDescription = "some goods";
			consignment.HVC_JS_ManifestedOnShipment = shipmentPK;
			consignment.HVC_WaybillNumber = waybillNumber;

			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipmentPK;

			return consignment;
		}

		protected abstract HVLVValidateAndSendACASReportProcessor GetSendACASReportProcessor(ForwardingShipment shipment);

		delegate void SendACASReportsCallBack(ACASReportAction action, out string message);
	}
}
