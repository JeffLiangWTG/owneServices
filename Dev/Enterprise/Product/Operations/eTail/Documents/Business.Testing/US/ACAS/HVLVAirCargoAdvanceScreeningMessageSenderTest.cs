using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.eTail.Integration.HVLVConstants;
using ShipmentDocumentDataStoreNames = Enterprise.Freight.Forwarding.Documents.ShipmentDocumentDataStoreNames;

namespace Enterprise.eTail.Documents.Business.Testing.US.ACAS
{
	public class HVLVAirCargoAdvanceScreeningMessageSenderTest : TestCaseWithFactory
	{
		public void TestSendACASReport()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = consignmentHeader.Consignments.AddNew();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item = consignment1.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			var consignment2 = consignmentHeader.Consignments.AddNew();
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item2 = consignment2.Items.AddNew();
			item2.HVI_JS_LoadedOnShipment = shipment.PK;

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				new HVLVAirCargoAdvanceScreeningMessageSender(shipment).TrySendACASReports(ACASReportAction.SendOriginal, out var message);

				var messages = Factory.Load<EDIMessage>(new ZQuery());

				CombineAssertions("Expected EDIMessages to be created", () =>
				{
					AssertEquals("Expected 2 EDIMessages generated", 2, messages.Length);

					AssertEquals("Expected EDIMessage to be an ACAS report", "ADVANCE_AIR_CARGO_REPORT", messages[0].EM_InterchangeReceiver);
					AssertEquals("Expected EDIMessage to be an ACAS report", "ADVANCE_AIR_CARGO_REPORT", messages[1].EM_InterchangeReceiver);
				});
			}
		}

		public void TestSendACASReport_EDIMessageHasAdvanceAirCargoApplicationCode()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				new HVLVAirCargoAdvanceScreeningMessageSender(shipment).TrySendACASReports(ACASReportAction.SendOriginal, out var message);

				var messages = Factory.Load<EDIMessage>(new ZQuery());

				CombineAssertions(() =>
				{
					AssertEquals("Correct number of EDIMessages generated", 1, messages.Length);
					AssertEquals("Message application code is Advance Air Cargo Report", "ACS", messages[0].EM_ApplicationCode);
				});
			}
		}

		public void TestSendACASReport_DeliverPerConsignment()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = consignmentHeader.Consignments.AddNew();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item = consignment1.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			var consignment2 = consignmentHeader.Consignments.AddNew();
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item2 = consignment2.Items.AddNew();
			item2.HVI_JS_LoadedOnShipment = shipment.PK;

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				new HVLVAirCargoAdvanceScreeningMessageSender(shipment).TrySendACASReports(ACASReportAction.SendOriginal, out var message);

				var interchanges = Factory.Load<EDIInterchange>(new ZQuery());

				CombineAssertions("Expected EDIInterchanges to be created", () =>
				{
					AssertEquals("Expected 2 EDIInterchanges generated", 2, interchanges.Length);

					AssertEquals("Expected EDIInterchanges to be an ACAS report", "ADVANCE_AIR_CARGO_REPORT", interchanges[0].EI_To);
					AssertEquals("Expected EDIInterchanges to be an ACAS report", "ADVANCE_AIR_CARGO_REPORT", interchanges[1].EI_To);
				});
			}
		}

		public void TestSendACASReport_WillIncludeConsignmentsWithItemsLoadedOnShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_UniqueConsignRef = "S002";
			shipment.JS_RL_NKDestination = "USLAX";

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment2.JS_TransportMode = TransportModes.Air;
			shipment2.JS_UniqueConsignRef = "S003";
			shipment2.JS_RL_NKDestination = "USLAX";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item1 = consignment.Items.AddNew();
			item1.HVI_JS_LoadedOnShipment = shipment2.PK;

			using (Factory.AddDisposableService())
			{
				new HVLVAirCargoAdvanceScreeningMessageSender(shipment).TrySendACASReports(ACASReportAction.SendOriginal, out var message1);
				var messages = Factory.Load<EDIMessage>(new ZQuery());

				AssertEquals("Expected No EDIMessages generated", 0, messages.Length);
				AssertEquals("No message should be created since item is loaded on a different shipment", 0, messages.Length);
				item1.HVI_JS_LoadedOnShipment = shipment.PK;

				new HVLVAirCargoAdvanceScreeningMessageSender(shipment).TrySendACASReports(ACASReportAction.SendOriginal, out var message2);

				messages = Factory.Load<EDIMessage>(new ZQuery());

				CombineAssertions("Expected EDIMessages to be created", () =>
				{
					AssertEquals("Expected 1 EDIMessage generated", 1, messages.Length);

					AssertEquals("Expected EDIMessage to be an ACAS report", "ADVANCE_AIR_CARGO_REPORT", messages[0].EM_InterchangeReceiver);
				});
			}
		}

		public void TestSendACASReport_ReturnDeliveryResult_SingleConsignment()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				var result = new HVLVAirCargoAdvanceScreeningMessageSender(shipment).TrySendACASReports(ACASReportAction.SendOriginal, out var message);
				Assert("ACAS report sent successfully", result);
				AssertEquals("1 report successfully sent.", message);
			}
		}

		public void TestSendACASReport_ReturnDeliveryResult_MultipleConsignments()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = consignmentHeader.Consignments.AddNew();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item1 = consignment1.Items.AddNew();
			item1.HVI_JS_LoadedOnShipment = shipment.PK;

			var consignment2 = consignmentHeader.Consignments.AddNew();
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item2 = consignment2.Items.AddNew();
			item2.HVI_JS_LoadedOnShipment = shipment.PK;

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				var result = new HVLVAirCargoAdvanceScreeningMessageSender(shipment).TrySendACASReports(ACASReportAction.SendOriginal, out var message);
				Assert("ACAS reports sent successfully", result);
				AssertEquals("2 reports successfully sent.", message);
			}
		}

		public void TestSendACASReport_WhenConsignmentHasNoShipment_ThenIgnoreAndDoNotSendConsignmentACASDoc()
		{
			var consignment1 = Factory.New<HVLVConsignment>();
			consignment1.HVC_WaybillNumber = "Consignment001";

			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_WaybillNumber = "Consignment002";

			var item = consignment2.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			AssertEquals("Expected consignment1 not to have a shipment", ZGuid.Empty, consignment1.HVC_JS_ManifestedOnShipment);
			AssertNotEquals("Expected consignment2 to have a shipment", ZGuid.Empty, consignment2.HVC_JS_ManifestedOnShipment);

			using (Factory.AddDisposableService())
			{
				var result = new HVLVAirCargoAdvanceScreeningMessageSender(shipment).TrySendACASReports(ACASReportAction.SendOriginal, out var message);

				Assert("ACAS report sent successfully", result);
				AssertEquals("1 report successfully sent.", message);

				var interchanges = Factory.Load<EDIMessage>(new ZQuery());

				CombineAssertions("Expected EDIMessages to be created", () =>
				{
					AssertEquals("Expected 1 EDIMessage generated", 1, interchanges.Length);

					AssertEquals("Expected EDIMessage to be an ACAS report", "ADVANCE_AIR_CARGO_REPORT", interchanges[0].EM_InterchangeReceiver);
					Assert("Expected EDIMessage text body to contain the consignment waybill number", interchanges[0].EM_MessageText.Contains("Consignment002"));
				});
			}
		}

		public void TestSendACASReport_InterchangeContentsDocumentOverride()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			Factory.Save();

			var docDataObjectProvider = new HVLVConsignmentDocDataObjectProvider(shipment);

			using (Factory.AddDisposableService())
			{
				var docDataObject = new HVLVAirCargoAdvanceScreeningMessageSender(shipment).GetDocDataObject(consignment, docDataObjectProvider, ACASReportAction.SendOriginal);
				AssertEquals(ShipmentDocumentDataStoreNames.HVLVAdvancedCargoReportUS, docDataObject.DataContext.DocumentaryOverride.DocumentName);
			}
		}

		public void TestSendACASReport_InterchangeContentsDocumentOverride_PurposeCode()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			var docDataObjectProvider = new HVLVConsignmentDocDataObjectProvider(shipment);

			using (Factory.AddDisposableService())
			{
				var docDataObject = new HVLVAirCargoAdvanceScreeningMessageSender(shipment).GetDocDataObject(consignment, docDataObjectProvider, ACASReportAction.SendOriginal);
				AssertEquals(MessagePurposes.Codes.Original, docDataObject.DataContext.DocumentaryOverride.Purpose.Code);

				docDataObject = new HVLVAirCargoAdvanceScreeningMessageSender(shipment).GetDocDataObject(consignment, docDataObjectProvider, ACASReportAction.SendAmendment);
				AssertEquals(MessagePurposes.Codes.Amendment, docDataObject.DataContext.DocumentaryOverride.Purpose.Code);
			}
		}

		public void TestSendACASReport_AddDEXEventForEachConsignment()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = consignmentHeader.Consignments.AddNew();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item = consignment1.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			var consignment2 = consignmentHeader.Consignments.AddNew();
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item2 = consignment2.Items.AddNew();
			item2.HVI_JS_LoadedOnShipment = shipment.PK;

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				new HVLVAirCargoAdvanceScreeningMessageSender(shipment).TrySendACASReports(ACASReportAction.SendOriginal, out var message);

				Assert(consignment1.Logs.GetAllLogs().OfType<StmALog>().Any(l => l.SL_SE_NKEvent == AutoEvents.DataExportCode));
				Assert(consignment2.Logs.GetAllLogs().OfType<StmALog>().Any(l => l.SL_SE_NKEvent == AutoEvents.DataExportCode));
			}
		}

		[TestDate(2021, 2, 2, 2, 2, 2)]
		public void TestSendACASReport_WillPopulateHVI_SecurityFilingFirstUsageTimeUtc()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item1 = consignment.Items.AddNew();
			item1.HVI_JS_LoadedOnShipment = shipment.PK;

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				new HVLVAirCargoAdvanceScreeningMessageSender(shipment).TrySendACASReports(ACASReportAction.SendOriginal, out var message);
				AssertEquals("1", new ZDateTime(2021, 2, 2, 2, 2, 2), item1.HVI_SecurityFilingFirstUsageTimeUtc);
			}
		}

		[TestDate(2021, 2, 2, 2, 2, 2)]
		public void TestSendACASReport_WillNotRepopulateHVI_SecurityFilingFirstUsageTimeUtc()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			var item1 = consignment.Items.AddNew();
			item1.HVI_SecurityFilingFirstUsageTimeUtc = new ZDateTime(2019, 5, 3);
			item1.HVI_JS_LoadedOnShipment = shipment.PK;

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				new HVLVAirCargoAdvanceScreeningMessageSender(shipment).TrySendACASReports(ACASReportAction.SendOriginal, out var message);
				AssertNotEquals(new ZDateTime(2021, 2, 2, 2, 2, 2), item1.HVI_SecurityFilingFirstUsageTimeUtc);
			}
		}

		public void TestTrySendACASReport_SendOriginal_PopulatesHVI_LastUsageCode()
		{
			AssertTrySendACASReports_HVI_LastUsageCode(1, sendOriginalAction: true, "ZZZ", ExpectedACASUsageCode);
		}

		public void TestTrySendACASReports_SendOriginal_PopulatesHVI_LastUsageCode()
		{
			AssertTrySendACASReports_HVI_LastUsageCode(3, sendOriginalAction: true, "ZZZ", ExpectedACASUsageCode);
		}

		public void TestTrySendACASReport_NotSendOriginal_DoesNotChangeHVI_LastUsageCode()
		{
			AssertTrySendACASReports_HVI_LastUsageCode(1, sendOriginalAction: false, "ZZZ", "ZZZ");
		}

		public void TestTrySendACASReports_NotSendOriginal_DoesNotChangeHVI_LastUsageCode()
		{
			AssertTrySendACASReports_HVI_LastUsageCode(3, sendOriginalAction: false, "ZZZ", "ZZZ");
		}

		void AssertTrySendACASReports_HVI_LastUsageCode(int numberOfConsignments, bool sendOriginalAction, string startingLastUsageCode, string expectedLastUsageCode)
		{
			ACASReportAction[] reportActionsToTest;
			if (sendOriginalAction)
			{
				reportActionsToTest = new[] { ACASReportAction.SendOriginal };
			}
			else
			{
				reportActionsToTest = Enum.GetValues(typeof(ACASReportAction)).Cast<ACASReportAction>().Where(x => x != ACASReportAction.SendOriginal).ToArray();
			}

			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			for (var i = 0; i < numberOfConsignments; i++)
			{
				var consignment = consignmentHeader.Consignments.AddNew();
				var item = consignment.Items.AddNew();
			}
			Factory.Save();

			foreach (HVLVConsignment consignment in consignmentHeader.Consignments)
			{
				foreach (HVLVItem item in consignment.Items)
				{
					item.HVI_LastUsageCode = startingLastUsageCode;
				}
			}
			Factory.Save();

			using (Factory.AddDisposableService())
			{
				var sender = new HVLVAirCargoAdvanceScreeningMessageSender(shipment);

				CombineAssertions(() =>
				{
					foreach (var reportAction in reportActionsToTest)
					{
						var messageStatus = MessageStatusRequiredForReportAction(reportAction);
						foreach (var c in shipment.HVLVConsignments)
						{
							c.HVC_ACASMessageStatus = messageStatus;
						}

						if (numberOfConsignments == 1)
						{
							var consignment = shipment.HVLVConsignments.Single();
							var item = consignment.Items.Cast<IHVLVItem>().Single();

							var result = sender.TrySendACASReport(consignment, reportAction, out _);
							Assert($"{reportAction}: send should succeed", result);
							AssertEquals($"{reportAction}: LastUsageCode", expectedLastUsageCode, item.HVI_LastUsageCode);
						}
						else
						{
							var result = sender.TrySendACASReports(reportAction, out var message);
							Assert($"{reportAction}: send should succeed", result);
							AssertContains($"{reportAction}: Number of consignments", $"{numberOfConsignments} reports successfully sent", message);
							var actualLastUsageCodes = shipment.HVLVItems.Select(x => x.HVI_LastUsageCode);
							Assert($"{reportAction}: All HVI_LastUsageCodes are same", actualLastUsageCodes.AllSame());
							AssertEquals($"{reportAction}: LastUsageCode", expectedLastUsageCode, actualLastUsageCodes.First());
						}
					}
				});
			}
		}

		string MessageStatusRequiredForReportAction(ACASReportAction reportAction)
		{
			string messageStatus;
			switch (reportAction)
			{
				case ACASReportAction.SendAcknowledgement:
					messageStatus = HVLVACASMessageStatusList.Codes.AcknowledgementRequired;
					break;
				case ACASReportAction.SendAmendment:
					messageStatus = HVLVACASMessageStatusList.Codes.AmendmentRequired;
					break;
				default:
					messageStatus = string.Empty;
					break;
			}
			return messageStatus;
		}

		public void TestUsageCodeAndCategory()
		{
			AssertEquals("UsageCode", "ACA", UsageCodes.ACAS);
			AssertEquals("UsageCategory", "SEC", UsageCategories.LookupByUsageCode[ExpectedACASUsageCode]);
		}

		const string ExpectedACASUsageCode = "ACA";

		public void TestSendACASReport_Validation_SendersACASCodeRequired()
		{
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.RemoveAll();
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Factory.Save();

			var errorMessage = new HVLVAirCargoAdvanceScreeningMessageSender(shipment).ValidateACASReportBasicRequirments();
			AssertEquals("Sender's ACAS code is required for ACAS messaging. Raise an eRequest to register your interest. Once provided by WTG, enter the code against the Branch or Company Organization Proxy > Config > Registration Numbers/Codes tab using Type = US ACA.", errorMessage);
		}

		public void TestSendACASReport_Validation_CTOFIRMSCodeIsRequiredWhenCTOIsEntered()
		{
			using (Factory.AddDisposableService())
			{
				var country = RefCountry.LoadFromCountryCode(Factory, "US");
				GlbCompany.CurrentCompany.OrgProxy.SetCustomsCode(OrgCusCode.USACodeTypes.ACASOriginatorCode, country, "Code");

				var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();

				var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

				var consignment = consignmentHeader.Consignments.AddNew();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

				Factory.Save();

				AssertCTOFIRMSCodeIsRequiredWhenCTOFIRMSCodeIsNotNull(string.Empty);

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_OA_ArrivalCTOAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
				shipment.Consols.Add(consol);

				Factory.Save();

				AssertCTOFIRMSCodeIsRequiredWhenCTOFIRMSCodeIsNotNull("CTO FIRMS code is required when CTO is entered.");

				var ctoAddress = Factory.New<OrgHeader>();
				ctoAddress.OH_FullName = "CTO";
				ctoAddress.OH_RL_NKClosestPort = "USLAX";
				ctoAddress.MainAddress.Address1 = "House 16777214";
				ctoAddress.MainAddress.Address2 = "Coelosis inermis";
				ctoAddress.MainAddress.City = "The Big City";
				ctoAddress.MainAddress.Postcode = "1234";
				ctoAddress.MainAddress.OA_RN_NKCountryCode = "US";
				ctoAddress.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "123", "US");
				consol.JK_OA_ArrivalCTOAddress = ctoAddress.MainAddress.PK;

				Factory.Save();

				AssertCTOFIRMSCodeIsRequiredWhenCTOFIRMSCodeIsNotNull(string.Empty);

				void AssertCTOFIRMSCodeIsRequiredWhenCTOFIRMSCodeIsNotNull(string expectErrorMessage)
				{
					var errorMessage = new HVLVAirCargoAdvanceScreeningMessageSender(shipment).ValidateACASReportBasicRequirments();
					AssertEquals(expectErrorMessage, errorMessage);
				}
			}
		}

		public void TestACASMessageStatus_WhenMessageSentAndNoResponseYet_IsOST()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_UniqueConsignRef = "S002";
			shipment.JS_RL_NKDestination = "USLAX";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_ACASMessageStatus = string.Empty;
			consignment.HVC_ACASStatus = string.Empty;

			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				new HVLVAirCargoAdvanceScreeningMessageSender(shipment).TrySendACASReports(ACASReportAction.SendOriginal, out var message);

				AssertEquals("ACAS message is sent", "OST", consignment.HVC_ACASMessageStatus);
			}
		}

		public void TestACASReportUniversalShipmentNameSpace_AirCargoAdvanceScreening_WhenACASReportActionIsNotSendAcknowledgement()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				new HVLVAirCargoAdvanceScreeningMessageSender(shipment).TrySendACASReports(ACASReportAction.SendOriginal, out var sendMessage);
				var message = Factory.LoadTop1<EDIMessage>(new ZQuery()) as IXmlEDIMessage;

				AssertEquals("http://www.cargowise.com/Schemas/Universal/2012/11/AirCargoAdvanceScreening/1", message.Content.FirstAttribute.Value);
			}
		}

		public void TestACASReportUniversalShipmentNameSpace_AcknowledgementOfHold_WhenACASReportActionIsSendAcknowledgement()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.AcknowledgementRequired;
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				new HVLVAirCargoAdvanceScreeningMessageSender(shipment).TrySendACASReports(ACASReportAction.SendAcknowledgement, out var sendMessage);
				var message = Factory.LoadTop1<EDIMessage>(new ZQuery()) as IXmlEDIMessage;

				AssertEquals("http://www.cargowise.com/Schemas/Universal/2012/11/AcknowledgementOfHold/1", message.Content.FirstAttribute.Value);
			}
		}

		public void TestGetACASCode_WhenBranchOrgProxyIsNull_DoesNotThrow()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var country = RefCountry.LoadFromCountryCode(Factory, "US");
			var branch = company.Branches.AddNew();
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			company.GC_OH_OrgProxy = org.PK;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_OA_ArrivalCTOAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			shipment.Consols.Add(consol);

			Factory.Save();

			AssertNull("Pre condition: branch OrgProxy is null", branch.OrgProxy);
			AssertNotNull("Pre condition: company OrgProxy is NOT null", company.OrgProxy);

			using (Factory.AddDisposableService())
			using (branch.SetAsTemporaryContext())
			{
				AssertNoExceptionThrown(() => new HVLVAirCargoAdvanceScreeningMessageSender(shipment).ValidateACASReportBasicRequirments());
			}
		}

		public void TestGetACASCode_WhenBranchOrgProxyAndCompanyOrgProxyAreBothNull_DoesNotThrow()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var country = RefCountry.LoadFromCountryCode(Factory, "US");
			var branch = company.Branches.AddNew();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_OA_ArrivalCTOAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			shipment.Consols.Add(consol);

			Factory.Save();

			AssertNull("Pre condition: company OrgProxy is null", company.OrgProxy);
			AssertNull("Pre condition: branch OrgProxy is null", branch.OrgProxy);

			using (Factory.AddDisposableService())
			using (branch.SetAsTemporaryContext())
			{
				AssertNoExceptionThrown(() => new HVLVAirCargoAdvanceScreeningMessageSender(shipment).ValidateACASReportBasicRequirments());
			}
		}

		public void TestSendACASAmendment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_UniqueConsignRef = "S001";
			shipment.JS_RL_NKDestination = "USLAX";

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = consignmentHeader.Consignments.AddNew();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.AmendmentRequired;
			var item1 = consignment1.Items.AddNew();
			item1.HVI_JS_LoadedOnShipment = shipment.PK;

			var consignment2 = consignmentHeader.Consignments.AddNew();
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item2 = consignment2.Items.AddNew();
			item2.HVI_JS_LoadedOnShipment = shipment.PK;

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				var messageSender = new HVLVAirCargoAdvanceScreeningMessageSender(shipment);
				var docDataObjectProvider = new HVLVConsignmentDocDataObjectProvider(shipment);
				var docDataObject = messageSender.GetDocDataObject(consignment1, docDataObjectProvider, ACASReportAction.SendAmendment);

				AssertEquals("Expected consignment to have ARQ ACAS message status", consignment1.HVC_ACASMessageStatus, HVLVACASMessageStatusList.Codes.AmendmentRequired);
				AssertEquals("The purpose code should be Amendment", MessagePurposes.Codes.Amendment, docDataObject.DataContext.DocumentaryOverride.Purpose.Code);

				messageSender.TrySendACASReports(ACASReportAction.SendAmendment, out var message);

				AssertEquals("Expected consignment to have AST ACAS message status", consignment1.HVC_ACASMessageStatus, HVLVACASMessageStatusList.Codes.AmendmentSent);

				var messages = Factory.Load<EDIMessage>(new ZQuery());

				CombineAssertions("Expected a single EDIMessage to be created", () =>
				{
					AssertEquals("Expected EDIMessage to be generated for the consignment which requires amendment", 1, messages.Length);

					AssertEquals("Expected EDIMessage to be an ACAS report", "ADVANCE_AIR_CARGO_REPORT", messages[0].EM_InterchangeReceiver);
				});
			}
		}

		public void TestSendACASAmendment_ReturnDeliveryResult_SingleConsignment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_UniqueConsignRef = "S001";
			shipment.JS_RL_NKDestination = "USLAX";

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.AmendmentRequired;
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			AssertEquals("Expected consignment to have ARQ ACAS message status", consignment.HVC_ACASMessageStatus, HVLVACASMessageStatusList.Codes.AmendmentRequired);

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				var result = new HVLVAirCargoAdvanceScreeningMessageSender(shipment).TrySendACASReports(ACASReportAction.SendAmendment, out var message);
				Assert("ACAS repor sent successfully", result);
				AssertEquals("1 report successfully sent.", message);
				AssertEquals("Expected consignment with ARQ status to change to AST", consignment.HVC_ACASMessageStatus, HVLVACASMessageStatusList.Codes.AmendmentSent);
			}
		}

		public void TestSendACASAmendment_ReturnDeliveryResult_SingleAmendableConsignment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_UniqueConsignRef = "S001";
			shipment.JS_RL_NKDestination = "USLAX";

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.AmendmentRequired;
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			var consignment2 = consignmentHeader.Consignments.AddNew();
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.OriginalSent;
			var item2 = consignment2.Items.AddNew();
			item2.HVI_JS_LoadedOnShipment = shipment.PK;

			AssertEquals("Expected consignment to have ARQ ACAS message status", consignment.HVC_ACASMessageStatus, HVLVACASMessageStatusList.Codes.AmendmentRequired);

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				var result = new HVLVAirCargoAdvanceScreeningMessageSender(shipment).TrySendACASReports(ACASReportAction.SendAmendment, out var message);
				Assert("ACAS report sent successfully", result);
				AssertEquals("1 report successfully sent.", message);
				CombineAssertions("Expected consignments to have correct ACAS message status", () =>
				{
					AssertEquals("Expected consignment with ARQ status to change to AST", consignment.HVC_ACASMessageStatus, HVLVACASMessageStatusList.Codes.AmendmentSent);
					AssertEquals("Expected consignment with OST status to not change", consignment2.HVC_ACASMessageStatus, HVLVACASMessageStatusList.Codes.OriginalSent);
				});
			}
		}

		public void TestSendACASAmendment_ReturnDeliveryResult_MultipleAmendableConsignments()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_UniqueConsignRef = "S001";
			shipment.JS_RL_NKDestination = "USLAX";

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = consignmentHeader.Consignments.AddNew();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.AmendmentRequired;
			var item1 = consignment1.Items.AddNew();
			item1.HVI_JS_LoadedOnShipment = shipment.PK;

			var consignment2 = consignmentHeader.Consignments.AddNew();
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.AmendmentRequired;
			var item2 = consignment2.Items.AddNew();
			item2.HVI_JS_LoadedOnShipment = shipment.PK;

			var consignment3 = consignmentHeader.Consignments.AddNew();
			consignment3.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment3.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.OriginalSent;
			var item3 = consignment3.Items.AddNew();
			item3.HVI_JS_LoadedOnShipment = shipment.PK;

			CombineAssertions("Expected consignments to have ARQ ACAS message status", () =>
			{
				AssertEquals("Expected consignment 1 to have correct ACAS message status", consignment1.HVC_ACASMessageStatus, HVLVACASMessageStatusList.Codes.AmendmentRequired);
				AssertEquals("Expected consignment 2 to have correct ACAS message status", consignment2.HVC_ACASMessageStatus, HVLVACASMessageStatusList.Codes.AmendmentRequired);
			});

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				var result = new HVLVAirCargoAdvanceScreeningMessageSender(shipment).TrySendACASReports(ACASReportAction.SendAmendment, out var message);
				Assert("ACAS reports sent successfully", result);
				AssertEquals("2 reports successfully sent.", message);
				CombineAssertions("Expected consignments to have correct ACAS message status", () =>
				{
					AssertEquals("Expected consignment 1 to have AST status", consignment1.HVC_ACASMessageStatus, HVLVACASMessageStatusList.Codes.AmendmentSent);
					AssertEquals("Expected consignment 2 to have AST status", consignment2.HVC_ACASMessageStatus, HVLVACASMessageStatusList.Codes.AmendmentSent);
					AssertEquals("Expected consignment 3 to have OST status", consignment3.HVC_ACASMessageStatus, HVLVACASMessageStatusList.Codes.OriginalSent);
				});
			}
		}

		public void TestSendACASAcknowledgement()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_UniqueConsignRef = "S001";
			shipment.JS_RL_NKDestination = "USLAX";

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = consignmentHeader.Consignments.AddNew();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.AcknowledgementRequired;
			var item1 = consignment1.Items.AddNew();
			item1.HVI_JS_LoadedOnShipment = shipment.PK;

			var consignment2 = consignmentHeader.Consignments.AddNew();
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item2 = consignment2.Items.AddNew();
			item2.HVI_JS_LoadedOnShipment = shipment.PK;

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				var messageSender = new HVLVAirCargoAdvanceScreeningMessageSender(shipment);
				var docDataObjectProvider = new HVLVConsignmentDocDataObjectProvider(shipment);
				var docDataObject = messageSender.GetDocDataObject(consignment1, docDataObjectProvider, ACASReportAction.SendAcknowledgement);

				AssertEquals("Expected consignment to have KRQ ACAS message status", consignment1.HVC_ACASMessageStatus, HVLVACASMessageStatusList.Codes.AcknowledgementRequired);

				messageSender.TrySendACASReports(ACASReportAction.SendAcknowledgement, out var message);

				AssertEquals("Expected consignment to have KST ACAS message status", consignment1.HVC_ACASMessageStatus, HVLVACASMessageStatusList.Codes.AcknowledgementSent);

				var messages = Factory.Load<EDIMessage>(new ZQuery());

				CombineAssertions("Expected a single EDIMessage to be created", () =>
				{
					AssertEquals("Expected EDIMessage to be generated for the consignment which requires acknowledgement", 1, messages.Length);
					AssertEquals("Expected EDIMessage to be an ACAS report", "ADVANCE_AIR_CARGO_REPORT", messages[0].EM_InterchangeReceiver);
				});
			}
		}

		public void TestSendACASAcknowledgement_ReturnDeliveryResult_SingleConsignment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_UniqueConsignRef = "S001";
			shipment.JS_RL_NKDestination = "USLAX";

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.AcknowledgementRequired;
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			AssertEquals("Expected consignment to have KRQ ACAS message status", consignment.HVC_ACASMessageStatus, HVLVACASMessageStatusList.Codes.AcknowledgementRequired);

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				var result = new HVLVAirCargoAdvanceScreeningMessageSender(shipment).TrySendACASReports(ACASReportAction.SendAcknowledgement, out var message);
				Assert("ACAS report sent successfully", result);
				AssertEquals("1 report successfully sent.", message);
				AssertEquals("Expected consignment with KRQ status to change to KST", consignment.HVC_ACASMessageStatus, HVLVACASMessageStatusList.Codes.AcknowledgementSent);
			}
		}

		public void TestSendACASAcknowledgement_ReturnDeliveryResult_SingleAcknowledgeableConsignment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_UniqueConsignRef = "S001";
			shipment.JS_RL_NKDestination = "USLAX";

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.AcknowledgementRequired;
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			var consignment2 = consignmentHeader.Consignments.AddNew();
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.OriginalSent;
			var item2 = consignment2.Items.AddNew();
			item2.HVI_JS_LoadedOnShipment = shipment.PK;

			AssertEquals("Expected consignment to have KRQ ACAS message status", consignment.HVC_ACASMessageStatus, HVLVACASMessageStatusList.Codes.AcknowledgementRequired);

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				var result = new HVLVAirCargoAdvanceScreeningMessageSender(shipment).TrySendACASReports(ACASReportAction.SendAcknowledgement, out var message);
				Assert("ACAS report sent successfully", result);
				AssertEquals("1 report successfully sent.", message);
				CombineAssertions("Expected consignments to have correct ACAS message status", () =>
				{
					AssertEquals("Expected consignment with KRQ status to change to KST", consignment.HVC_ACASMessageStatus, HVLVACASMessageStatusList.Codes.AcknowledgementSent);
					AssertEquals("Expected consignment with OST status to not change", consignment2.HVC_ACASMessageStatus, HVLVACASMessageStatusList.Codes.OriginalSent);
				});
			}
		}

		public void TestSendACASAcknowledgement_ReturnDeliveryResult_MultipleAcknowledgeableConsignments()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_UniqueConsignRef = "S001";
			shipment.JS_RL_NKDestination = "USLAX";

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = consignmentHeader.Consignments.AddNew();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.AcknowledgementRequired;
			var item1 = consignment1.Items.AddNew();
			item1.HVI_JS_LoadedOnShipment = shipment.PK;

			var consignment2 = consignmentHeader.Consignments.AddNew();
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.AcknowledgementRequired;
			var item2 = consignment2.Items.AddNew();
			item2.HVI_JS_LoadedOnShipment = shipment.PK;

			var consignment3 = consignmentHeader.Consignments.AddNew();
			consignment3.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment3.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.OriginalSent;
			var item3 = consignment3.Items.AddNew();
			item3.HVI_JS_LoadedOnShipment = shipment.PK;

			CombineAssertions("Expected consignments to have ARQ ACAS message status", () =>
			{
				AssertEquals("Expected consignment 1 to have correct ACAS message status", consignment1.HVC_ACASMessageStatus, HVLVACASMessageStatusList.Codes.AcknowledgementRequired);
				AssertEquals("Expected consignment 2 to have correct ACAS message status", consignment2.HVC_ACASMessageStatus, HVLVACASMessageStatusList.Codes.AcknowledgementRequired);
			});

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				var result = new HVLVAirCargoAdvanceScreeningMessageSender(shipment).TrySendACASReports(ACASReportAction.SendAcknowledgement, out var message);
				Assert("ACAS reports sent successfully", result);
				AssertEquals("2 reports successfully sent.", message);
				CombineAssertions("Expected consignments to have correct ACAS message status", () =>
				{
					AssertEquals("Expected consignment 1 to have AST status", consignment1.HVC_ACASMessageStatus, HVLVACASMessageStatusList.Codes.AcknowledgementSent);
					AssertEquals("Expected consignment 2 to have AST status", consignment2.HVC_ACASMessageStatus, HVLVACASMessageStatusList.Codes.AcknowledgementSent);
					AssertEquals("Expected consignment 3 to have OST status", consignment3.HVC_ACASMessageStatus, HVLVACASMessageStatusList.Codes.OriginalSent);
				});
			}
		}

		public void TestSendACASAmendment_ForIndividualConsignment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_UniqueConsignRef = "S001";
			shipment.JS_RL_NKDestination = "USLAX";

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.AmendmentRequired;
			var item = consignment.Items.AddNew();
			item.HVI_GoodsDescription = "RTX3060";
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				var result = new HVLVAirCargoAdvanceScreeningMessageSender(shipment).TrySendACASReport(consignment, ACASReportAction.SendAmendment, out var message);
				Assert("ACAS amendment message successfully sent.", result);
				AssertEquals("Expected consignment to have AST status", consignment.HVC_ACASMessageStatus, HVLVACASMessageStatusList.Codes.AmendmentSent);
			}
		}

		public void TestTrySendACASReports_WhenFailed_DoesNotUpdateACASMessageStatus()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			Factory.Save();

			var calls = 0;
			var mockHVLVAirCargoAdvanceScreeningMessageSender = new Mock<HVLVAirCargoAdvanceScreeningMessageSender>(shipment) { CallBase = true };
			mockHVLVAirCargoAdvanceScreeningMessageSender.Setup(mock => mock.SendACASReportCore(consignment, ACASReportAction.SendOriginal))
					.Callback(() => calls++)
					.Returns<IDeliveryResult>(null);

			var result = mockHVLVAirCargoAdvanceScreeningMessageSender.Object.TrySendACASReports(ACASReportAction.SendOriginal, out var message);

			AssertEquals("precondition", 1, calls);
			Assert("Send failed", !result);
			AssertEquals("ACAS message status is not updated", ZString.Empty, consignment.HVC_ACASMessageStatus);
		}

		public void TestTrySendACASReports_WhenOneFailed_StopsSend()
		{
			using (Factory.AddDisposableService())
			{
				var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();

				var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

				var consignment1 = consignmentHeader.Consignments.AddNew();
				consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
				var item1 = consignment1.Items.AddNew();
				item1.HVI_JS_LoadedOnShipment = shipment.PK;

				var consignment2 = consignmentHeader.Consignments.AddNew();
				consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
				var item2 = consignment2.Items.AddNew();
				item2.HVI_JS_LoadedOnShipment = shipment.PK;

				var consignment3 = consignmentHeader.Consignments.AddNew();
				consignment3.HVC_JS_ManifestedOnShipment = shipment.PK;
				var item3 = consignment3.Items.AddNew();
				item3.HVI_JS_LoadedOnShipment = shipment.PK;

				Factory.Save();

				var calls = 0;
				var mockHVLVAirCargoAdvanceScreeningMessageSender = new Mock<HVLVAirCargoAdvanceScreeningMessageSender>(shipment) { CallBase = true };
				mockHVLVAirCargoAdvanceScreeningMessageSender.Setup(mock => mock.SendACASReportCore(consignment2, ACASReportAction.SendOriginal))
						.Callback(() => calls++)
						.Returns<IDeliveryResult>(null);

				var result = mockHVLVAirCargoAdvanceScreeningMessageSender.Object.TrySendACASReports(ACASReportAction.SendOriginal, out var message);

				AssertEquals("precondition", 1, calls);
				Assert("Send failed", !result);

				AssertEquals("ACAS message status for sent consignment is updated", HVLVACASMessageStatusList.Codes.OriginalSent, consignment1.HVC_ACASMessageStatus);
				AssertEquals("ACAS message status for failed consignment is not updated", ZString.Empty, consignment2.HVC_ACASMessageStatus);

				var messages = Factory.Load<EDIMessage>(new ZQuery());

				CombineAssertions("Expected a single EDIMessage to be created", () =>
				{
					AssertEquals("Expected EDIMessage to be generated for the consignment which sent successfully", 1, messages.Length);
					AssertEquals("Expected EDIMessage to be an ACAS report", "ADVANCE_AIR_CARGO_REPORT", messages[0].EM_InterchangeReceiver);
				});
			}
		}
	}
}
