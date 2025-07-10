using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngine;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Environment;
using Enterprise.ExcelTemplates.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Supporters;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin.Base;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class ForwardingShipmentVisualizableDocumentSupporterTest : TestCaseWithFactory
	{
		#region TestCustomizeFormCheckpoint

		public void TestCustomizeFormCheckpoint()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
			var docDataSource = new object();

			AssertEquals("CustomizeFormCheckpoint", Env.Security.MaintainShipmentCustomiseForms, supporter.CustomizeFormCheckpoint);
		}

		#endregion

		#region TestGetMessageLogCreator_AdvancedCargoReport

		public void TestGetMessageLogCreator_AdvancedCargoReport_US() => AssertGetMessageLogCreator_AdvancedCargoReport(Core.Constants.CountryCodes.UnitedStates, DataContext.AirCargoAdvanceScreening);
		public void TestGetMessageLogCreator_AdvancedCargoReport_BR() => AssertGetMessageLogCreator_AdvancedCargoReport(Core.Constants.CountryCodes.Brazil, DataContext.CargoControlAndTransit);

		void AssertGetMessageLogCreator_AdvancedCargoReport(string countryCode, string dataContext)
		{
			var shipment = Factory.New<ForwardingShipment>();
			var documentData = Factory.New<VisualizerDocumentData>();
			var document = new Mock<IDocument>();
			var dynamicData = new Mock<IDynamicData>();

			document.SetupGet(d => d.DataContext).Returns(dataContext);

			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);

			var logCreator = supporter.GetMessageLogCreator(document.Object);

			AssertNotNull($"{nameof(IMessageLogCreator)} for {countryCode}", logCreator);
			Assert("log has been created",
				logCreator.CreateMessageSentLog(documentData, dynamicData.Object, DocumentNames.AdvancedCargoReport, "Customs"));

			var msn = documentData.Logs.MostRecentLogByEventTime(Events.MessageSent);

			AssertNotNull("MSN event has been created", msn);
			AssertContainsExactElementsInAnyOrder("MSN event parameters",
				new[]
				{
					"MST=Advanced Cargo Report",
					$"LOC={countryCode}",
					"DEP=Customs"
				},
				msn.Parameters.Select(p => $"{p.Key}={p.Value}"));
		}

		#endregion

		#region TestGetEventParent

		public void TestGetEventParent_ShippersDeclarationForDangerousGoods() => AssertGetEventParent(ShipmentDocumentNames.ShippersDeclarationForDangerousGoods, ShipmentDocumentDataStoreNames.ShippersDangerousGoodsDeclaration);
		public void TestGetEventParent_AdvancedCargoReport_BR() => AssertGetEventParent(ShipmentDocumentNames.AdvancedCargoReport, ShipmentDocumentDataStoreNames.AdvancedCargoReportBR, Core.Constants.CountryCodes.Brazil);
		public void TestGetEventParent_AdvancedCargoReport_BR_UNLOCO() => AssertGetEventParent(ShipmentDocumentNames.AdvancedCargoReport, ShipmentDocumentDataStoreNames.AdvancedCargoReportBR, Core.Constants.CountryCodes.Brazil, "BRSAA");
		public void TestGetEventParent_AdvancedCargoReport_US() => AssertGetEventParent(ShipmentDocumentNames.AdvancedCargoReport, ShipmentDocumentDataStoreNames.AdvancedCargoReportUS, Core.Constants.CountryCodes.UnitedStates);
		public void TestGetEventParent_AdvancedCargoReport_US_UNLOCO() => AssertGetEventParent(ShipmentDocumentNames.AdvancedCargoReport, ShipmentDocumentDataStoreNames.AdvancedCargoReportUS, Core.Constants.CountryCodes.UnitedStates, "USLAX");

		void AssertGetEventParent(string documentName, string dataStoreName, string countryCode = null, string unloco = null)
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = $"{countryCode ?? "NZ"}ZZZ";
			shipment.JS_UniqueConsignRef = "S00000069";

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentID = shipment.PK;
			documentData.JDD_ParentTableCode = shipment.TablePrefix;
			documentData.JDD_Name = dataStoreName;

			Factory.Save();

			var universalEvent = new Event
			{
				DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext
				{
					DocumentaryOverride = new DocumentaryOverride
					{
						DocumentName = documentName
					},
					DataTargetCollection = new List<DataTarget>
					{
						new DataTarget
						{
							Key = "S00000069",
							Type = "ForwardingShipment"
						}
					}
				},
				EventTime = new ZDateTimeOffset(2019, 12, 04),
				EventType = "MAA"
			};

			if (!string.IsNullOrEmpty(countryCode) || !string.IsNullOrEmpty(unloco))
			{
				universalEvent.EventParameters = new EventParameters
				{
					Location = !string.IsNullOrEmpty(unloco) ? unloco : countryCode
				};
			}

			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
			var eventParent = supporter.GetEventParent(universalEvent);

			AssertEquals("found document data", documentData, eventParent);
		}

		public void TestGetEventParent_ChaftaCertificateOfOrigin()
		{
			string documentName = ShipmentDocumentNames.ChaftaCertificateOfOrigin;
			string dataStoreName = ShipmentDocumentDataStoreNames.ChaftaCertificateOfOrigin;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "CNZZZ";
			shipment.JS_UniqueConsignRef = "S00TEST99";

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentID = shipment.PK;
			documentData.JDD_ParentTableCode = shipment.TablePrefix;
			documentData.JDD_Name = dataStoreName;

			Factory.Save();

			var universalEvent = new Event
			{
				DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext
				{
					DocumentaryOverride = new DocumentaryOverride
					{
						DocumentName = documentName
					},
					DataTargetCollection = new List<DataTarget>
					{
						new DataTarget
						{
							Key = shipment.JS_UniqueConsignRef,
							Type = nameof(ForwardingShipment)
						}
					}
				},
				EventTime = new ZDateTimeOffset(2021, 09, 06),
				EventType = "IRJ",
				EventParameters = new EventParameters()
				{
					Department = "CargoWise",
					MessageType = "Certificate of Origin",
					Reason = "You are not registered with this service provider for this message type. Contact WTG to register."
				}
			};

			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
			var eventParent = supporter.GetEventParent(universalEvent);

			AssertEquals("found document data", documentData, eventParent);
		}

		#endregion

		#region TestGetAdditionalData

		#region TestGetAdditionalData_AdvancedAirCargoReport

		public void TestGetAdditionalData_MasterSentAdvancedCargoReport()
		{
			var shipment = CreateShipmentDestinedForBrazil();

			var masterShipment = Factory.New<ForwardingShipment>();
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var menuItem = Factory.Load<DocumentCommand>(ShipmentSystemFormMenuItems.DocumentMenuCCTShipmentReport);
			AssertNotNull("Precondition: menu item exists", menuItem);

			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);

			var result = supporter.GetAdditionalData(shipment, menuItem);
			Assert($"No error is expected when ASM shipment did not Sent AdvancedCargoReport", result.IsRight);

			masterShipment.Logs.AddNew(Events.MessageSent, "|DEP=Customs|MST=Advanced Cargo Report", ZDateTimeOffset.Today.AddDays(-10));
			result = supporter.GetAdditionalData(shipment, menuItem);
			Assert($"An error is expected when the shipment Sent AdvancedCargoReport", result.IsLeft);
			AssertEquals($"This error message is expected when shipment Sent AdvancedCargoReport",
				"Advanced Air Cargo Reporting has been sent from Assembly master shipment. In Assembly master scenario, Advanced Air Cargo Reporting can be done from master or subs not from both. Please withdraw the message sent from Assembly master shipment prior to send from sub-Shipment.",
				result.Left);

			masterShipment.Logs.AddNew(Events.InterchangeRejected, "|DEP=Customs|MST=Advanced Cargo Report", ZDateTimeOffset.Today.AddDays(-9));
			result = supporter.GetAdditionalData(shipment, menuItem);
			Assert($"No error is expected when ASM shipment did not Sent AdvancedCargoReport", result.IsRight);

			masterShipment.Logs.AddNew(Events.MessageSent, "|DEP=Customs|MST=Advanced Cargo Report", ZDateTimeOffset.Today.AddDays(-8));
			result = supporter.GetAdditionalData(shipment, menuItem);
			Assert($"An error is expected when the shipment Sent AdvancedCargoReport", result.IsLeft);
			AssertEquals($"This error message is expected when shipment Sent AdvancedCargoReport",
				"Advanced Air Cargo Reporting has been sent from Assembly master shipment. In Assembly master scenario, Advanced Air Cargo Reporting can be done from master or subs not from both. Please withdraw the message sent from Assembly master shipment prior to send from sub-Shipment.",
				result.Left);

			masterShipment.Logs.AddNew(Events.MessageRejected, "|DEP=Customs|MST=Advanced Cargo Report", ZDateTimeOffset.Today.AddDays(-7));
			result = supporter.GetAdditionalData(shipment, menuItem);
			Assert($"No error is expected when ASM shipment did not Sent AdvancedCargoReport", result.IsRight);
		}

		public void TestGetAdditionalData_SubSentAdvancedCargoReport()
		{
			var shipment1 = CreateShipmentDestinedForBrazil();
			shipment1.JS_UniqueConsignRef = "S00001001";
			var shipment2 = CreateShipmentDestinedForBrazil();
			shipment2.JS_UniqueConsignRef = "S00001002";
			var shipment3 = CreateShipmentDestinedForBrazil();
			shipment3.JS_UniqueConsignRef = "S00001003";

			var masterShipment = Factory.New<ForwardingShipment>();
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			shipment1.JS_JS_ColoadMasterShipment = masterShipment.PK;
			shipment2.JS_JS_ColoadMasterShipment = masterShipment.PK;
			shipment3.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var menuItem = Factory.Load<DocumentCommand>(ShipmentSystemFormMenuItems.DocumentMenuCCTShipmentReport);
			AssertNotNull("Precondition: menu item exists", menuItem);

			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(masterShipment);
			var result = supporter.GetAdditionalData(masterShipment, menuItem);

			Assert($"No error is expected when sub shipment did not Sent AdvancedCargoReport", result.IsRight);

			shipment1.Logs.AddNew(Events.MessageSent, "|DEP=Customs|MST=Advanced Cargo Report", ZDateTimeOffset.Today.AddDays(-1));
			shipment2.Logs.AddNew(Events.MessageSent, "|DEP=Customs|MST=Advanced Cargo Report", ZDateTimeOffset.Today.AddDays(-1));
			result = supporter.GetAdditionalData(masterShipment, menuItem);
			Assert($"An error is expected when the shipment Sent AdvancedCargoReport", result.IsLeft);
			AssertEquals($"This error message is expected when shipment Sent AdvancedCargoReport",
				"Advanced Air Cargo Reporting has been sent from sub-shipments < S00001001,S00001002 >. In Assembly master scenario, Advanced Air Cargo Reporting can be done from master or subs not from both. Please withdraw the message sent from above each sub-shipment prior to send from Assembly master.",
				result.Left);
		}

		public void TestGetAdditionalData_AdvancedAirCargoReport_CCTShipmentReport_DocumentMenuItem()
		{
			var shipment = CreateShipmentDestinedForBrazil();
			var menuItem = Factory.Load<DocumentCommand>(ShipmentSystemFormMenuItems.DocumentMenuCCTShipmentReport);

			AssertNotNull("Precondition: menu item exists", menuItem);

			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);

			Action<bool, string> assertErrorReturned = (bool isErrorExpected, string shipmentType) =>
			{
				shipment.JS_ShipmentType = shipmentType;
				var result = supporter.GetAdditionalData(shipment, menuItem);
				if (isErrorExpected)
				{
					Assert($"An error is expected when the shipment type is {shipmentType}", result.IsLeft);
					AssertEquals($"This error message is expected when shipment type is {shipmentType}",
						"Advanced Air Cargo Reporting is only available from Shipments with types STD, BCN, CLD, ASM or HVL.",
						result.Left);
				}
				else
				{
					Assert($"No error is expected when shipment type is {shipmentType}", result.IsRight);
					AssertEquals($"No error is expected when shipment type is {shipmentType}.", false, result.IsLeft);
				}
			};

			assertErrorReturned(true, Core.Constants.ShipmentTypes.BlindCoLoadMaster);
			assertErrorReturned(false, Core.Constants.ShipmentTypes.AssemblyMaster);
			assertErrorReturned(false, Core.Constants.ShipmentTypes.StandardHouse);
			assertErrorReturned(false, Core.Constants.ShipmentTypes.BuyersConsolLead);
			assertErrorReturned(false, Core.Constants.ShipmentTypes.CoLoadMaster);
			assertErrorReturned(false, Core.Constants.ShipmentTypes.HighVolumeLowValue);

			AssertAdvancedAirCargoReport_ErrorIsReturned_WhenThereIsACoLoadMaster(menuItem, shipment);
		}

		public void TestGetAdditionalData_AdvancedAirCargoReport_ACASShipmentReport_DocumentMenuItem()
		{
			var shipment = CreateShipmentDestinedForUnitedStates();
			var menuItem = Factory.Load<DocumentCommand>(ShipmentSystemFormMenuItems.DocumentMenuACASShipmentReport);

			AssertNotNull("Precondition: menu item exists", menuItem);

			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);

			Action<bool, string> assertErrorReturned = (bool isErrorExpected, string shipmentType) =>
			{
				shipment.JS_ShipmentType = shipmentType;
				var result = supporter.GetAdditionalData(shipment, menuItem);
				if (isErrorExpected)
				{
					Assert($"An error is expected when the shipment type is {shipmentType}", result.IsLeft);
					AssertEquals($"This error message is expected when shipment type is {shipmentType}",
						"Advanced Air Cargo Reporting is only available from Shipments with types STD, BCN, CLD or HVL.",
						result.Left);
				}
				else
				{
					Assert($"No error is expected when shipment type is {shipmentType}", result.IsRight);
					AssertEquals($"No error is expected when shipment type is {shipmentType}.", false, result.IsLeft);
				}
			};

			assertErrorReturned(true, Core.Constants.ShipmentTypes.BlindCoLoadMaster);
			assertErrorReturned(true, Core.Constants.ShipmentTypes.AssemblyMaster);
			assertErrorReturned(false, Core.Constants.ShipmentTypes.StandardHouse);
			assertErrorReturned(false, Core.Constants.ShipmentTypes.BuyersConsolLead);
			assertErrorReturned(false, Core.Constants.ShipmentTypes.CoLoadMaster);
			assertErrorReturned(false, Core.Constants.ShipmentTypes.HighVolumeLowValue);
		}

		void AssertAdvancedAirCargoReport_ErrorIsReturned_WhenThereIsACoLoadMaster(DocumentCommand menuItem, ForwardingShipment shipment)
		{
			AssertNotNull("Precondition: menu item exists", menuItem);
			AssertEquals("Precondition: the shipment does not have a co-load master shipment", ZGuid.Empty, shipment.JS_JS_ColoadMasterShipment);

			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);

			var result = supporter.GetAdditionalData(shipment, menuItem);
			Assert("No error is expected when there is not a co-load master shipment", result.IsRight);
			AssertEquals("No error is expected when there is not a co-load master shipment", false, result.IsLeft);

			var masterShipment = Factory.New<ForwardingShipment>();
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			result = supporter.GetAdditionalData(shipment, menuItem);
			Assert("An error is expected when the shipment has a co-load master shipment.", result.IsLeft);
			AssertEquals("This error message is expected when the shipment has a co-load master shipment.",
				"In co-load scenario, Advance Air Cargo Reporting should be done from the Co-Load Master (CLD) shipment.",
				result.Left);
		}

		#endregion

		#region TestGetAdditionalData_Carrier

		public void TestGetAdditionalData_Carrier_EManifest()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_RL_NKOrigin = "CNSHA";
			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var menuItem = Factory.Load<DocumentCommand>(ShipmentSystemFormMenuItems.EasipassPK);
			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
			var result = supporter.GetAdditionalData(shipment, menuItem);
			Assert("No error messages are expected when the shipment is attached to a Consolidation whose origin port is in China, destination port is outside of china, and transport mode is SEA.",
				result.IsRight);

			Action<string> assertReturnsError = (string assertionMessage) =>
			{
				supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
				result = supporter.GetAdditionalData(shipment, menuItem);
				Assert(assertionMessage, result.IsLeft);
				AssertEquals(assertionMessage,
					"The Shipment is not attached to a Consolidation with a transport leg loading in China. The form will display once this Shipment is attached to a Consolidation with a relevant transport leg.",
					result.Left);
			};

			// shipment not from China
			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USJFK";
			consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USJFK";
			assertReturnsError("The error message is as expected when neither origin nor destination port is in China.");

			// shipment with no consol attached
			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "CNSHA";
			assertReturnsError("The error message is as expected when there is no consol attached.");

			// consols with both origin and destination port as China show the error
			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "CNSHA";
			consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "CNSHA";
			assertReturnsError("The error message is as expected when both origin and destination ports are in China.");

			// Air consols should show the error
			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_RL_NKOrigin = "CNSHA";
			consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";
			assertReturnsError("The error message is as expected for non-Sea transport types");
		}

		#endregion

		#region DataContext is HouseBill

		public void TestGetAdditionalData_IsElectronicShippingInstructionReceived()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var electronicBillOfLadingMenuItem = Factory.Load<DocumentCommand>(ShipmentSystemFormMenuItems.DocumentMenuCarrierBillOfLadingPK);
			var billOfLadingMenuItem = Factory.Load<DocumentCommand>(ShipmentSystemFormMenuItems.BillOfLadingPK);

			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
			var result = supporter.GetAdditionalData(shipment, electronicBillOfLadingMenuItem);

			Assert("No Electronic Shipping Instructions received, so ask confirmation to user.", !result.IsRight);
			AssertEquals("There is no electronic Shipping Instruction received from the Booking Party. Select OK to generate the Draft Bill of Lading or Cancel.", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessages();

			result = supporter.GetAdditionalData(shipment, billOfLadingMenuItem);

			Assert("Bill Of Lading Menu Item, so no questions asked.", result.IsRight);

			var log = shipment.Logs.AddNew(Events.StatusUpdated, new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));
			log.UpdateReference($"|{Params.Type}={Core.Constants.EventReferenceMessageTypes.ShipmentStatus}|{Params.New}={ShipmentStatusList.Codes.ElectronicShippingInstruction}");

			supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
			result = supporter.GetAdditionalData(shipment, electronicBillOfLadingMenuItem);

			Assert("Electronic Shipping Instructions received, so no questions asked.", result.IsRight);
		}

		public void TestGetAdditionalData_IsElectronicShippingInstructionReceived_BOLIsOk()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				shipment.JS_RL_NKOrigin = "USCHI";
				shipment.JS_RL_NKDestination = "AUSYD";
				var packline = shipment.OuterPackLines.AddNew();
				packline.JL_LinePrice = 10000;
				packline.JL_HarmonisedCode = "12345678";

				var consignor = Factory.NewWithValidTestData<OrgHeader>();
				consignor.AddRelatedParty(GlbCompany.CurrentCompany.GC_OH_OrgProxy, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.PickupAndDelivery, Core.Constants.TransportModes.All, Core.Constants.ContainerModes.FCL, GlbCompany.CurrentCompany);
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

				Factory.Save();

				AssertGreaterThan<decimal>("Precondition: PackLine line price should be greater than ShipmentHTSMaximumValue", packline.JL_LinePrice, (decimal)ObjectFactory.Get<Enterprise.Integration.Customs.US.IUSCustomsDataRegistry>().ShipmentHTSMaximumValue.Value);

				Env.Security.AllowPrintingOfAWBHBLIfNoExportDeclarationFiled.IsAllowed = true;

				var menuItem = Factory.Load<DocumentCommand>(ShipmentSystemFormMenuItems.DocumentMenuCarrierBillOfLadingPK);

				var log = shipment.Logs.AddNew(Events.StatusUpdated, new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus));
				log.UpdateReference($"|{Params.Type}={Core.Constants.EventReferenceMessageTypes.ShipmentStatus}|{Params.New}={ShipmentStatusList.Codes.ElectronicShippingInstruction}");

				UnitTestUserNotification.Instance.ClearMessages();
				var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
				var result = supporter.GetAdditionalData(shipment, menuItem);

				Assert(result.IsRight);
				AssertEquals(@"The aggregate values of merchandise within shipment S00001000 that are covered by any single HTS number exceed $2500 and Customs Entry Number is not entered but you have the necessary security access to continue running this document.

Are you sure you want to run this document?", UnitTestUserNotification.Instance.LastMessage.Text);

				log.Delete();

				UnitTestUserNotification.Instance.ClearMessages();
				supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
				result = supporter.GetAdditionalData(shipment, menuItem);

				Assert(result.IsLeft);
				AssertEquals("No electronic Shipping Instruction", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("There is no electronic Shipping Instruction received from the Booking Party. Select OK to generate the Draft Bill of Lading or Cancel.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region TestGetAdditionalData_FrenchPort

		public void TestShipmentDOS_noECVandAgentReferenceAndNoCRESASend()
		{
			var shipment = CreateFRShipment();
			var consol = CreateFRConsol(isImport: true);
			shipment.Consols.Add(consol);

			var menuItem = CreateMenuItem(DataContext.FRPortsIntegrationDossier);
			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
			var result = supporter.GetAdditionalData(shipment, menuItem);

			AssertEquals($"Error when ECV refs but no RCN refs and no CRESA send", true, result.IsLeft);
			AssertEquals($"Error when ECV refs but no RCN refs and no CRESA send", false, result.IsRight);
			AssertEquals($"Error when no ECV/RCN refs and no CRESA send", "Cargo belongs to all pack lines not received into the warehouse and CRESA is not finalized with CCS. You are not allowed to send File Creation Request (DOS) message until finalized the CRESA.", result.Left);

			var i = 0;
			foreach (ForwardingPackLine packline in shipment.OuterPackLines)
			{
				i++;
				var ecvNumber = Factory.New<CusEntryNumber>();
				ecvNumber.Parent = packline;
				ecvNumber.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN;
				ecvNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
				ecvNumber.CE_EntryNum = $"ECV_{i}";
				ecvNumber.CE_Category = "PRT";
			}
			supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
			result = supporter.GetAdditionalData(shipment, menuItem);

			AssertEquals($"Error when ECV refs but no RCN refs and no CRESA send", true, result.IsLeft);
			AssertEquals($"Error when ECV refs but no RCN refs and no CRESA send", false, result.IsRight);
			AssertEquals($"Error when ECV refs but no RCN refs and no CRESA send", "Cargo belongs to all pack lines not received into the warehouse and CRESA is not finalized with CCS. You are not allowed to send File Creation Request (DOS) message until finalized the CRESA.", result.Left);

			i = 0;
			foreach (ForwardingPackLine packline in shipment.OuterPackLines)
			{
				i++;
				var ercNumber = packline.AdditionalReferenceNumbers.AddNew();
				ercNumber.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC;
				ercNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
				ercNumber.CE_EntryNum = $"ERC_{i}";
			}
			supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
			result = supporter.GetAdditionalData(shipment, menuItem);

			AssertEquals($"No Error when ECV/RCN refs and no CRESA send", true, result.IsRight);
			if (result.Right is List<ZString> notSendRefs)
			{
				AssertEquals($"No Error when ECV/RCN refs and no CRESA send", "ERC_1, ERC_2, ERC_3", string.Join(", ", notSendRefs));
			}
			AssertEquals($"No Error when ECV/RCN refs and no CRESA send", false, result.IsLeft);

			foreach (ForwardingPackLine packline in shipment.OuterPackLines)
			{
				packline.AdditionalReferenceNumbers.RemoveAndDeleteAll();
			}
			CreateEventFrenchPorts(shipment, AutoEvents.MessageAccepted, documentName: FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA);

			supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
			result = supporter.GetAdditionalData(shipment, menuItem);

			AssertEquals($"No Error when no ECV/RCN refs and CRESA send", true, result.IsRight);
			AssertEquals($"No Error when no ECV/RCN refs and CRESA send", false, result.IsLeft);
		}

		[TestDate(2024, 1, 1)]
		public void TestShipmentDOS_noECVandAgentReferenceAndNoCRESASend_ErrorReport()
		{
			ErrorReporter.Clear();

			var registration = ObjectFactory.Get<IProductRegistration>();
			registration.KeyForTest.EnterpriseCodeForTest = "B52";
			registration.KeyForTest.ServerCodeForTest = "PRO";

			var shipment = CreateFRShipment();
			var consol = CreateFRConsol(isImport: true);
			shipment.Consols.Add(consol);

			var index = 0;
			foreach (var packline in shipment.OuterPackLines.Cast<ForwardingPackLine>())
			{
				index++;
				var ecvNumber = Factory.New<CusEntryNumber>();
				ecvNumber.Parent = packline;
				ecvNumber.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC;
				ecvNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
				ecvNumber.CE_EntryNum = $"Number_{index}";
				ecvNumber.CE_Category = "PRT";
				ecvNumber.CE_EntryStatus = "TST";
				ecvNumber.CE_EntryLineReference = "reference";
				ecvNumber.CE_IssueDate = ZDateTime.UtcNow;
				ecvNumber.CE_ExpiryDate = ZDateTime.UtcNow;
				ecvNumber.CE_SystemCreateTimeUtc = ZDateTime.UtcNow;
				ecvNumber.CE_SystemCreateUser = "~BP";

				var ercNumber = packline.AdditionalReferenceNumbers.AddNew();
				ercNumber.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN;
				ercNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
				ercNumber.CE_EntryNum = $"Number_{index}";
				ercNumber.CE_EntryStatus = Events.MessageSentCode;
				ercNumber.CE_EntryLineReference = "reference";
				ercNumber.CE_IssueDate = ZDateTime.UtcNow;
				ercNumber.CE_ExpiryDate = ZDateTime.UtcNow;
				ercNumber.CE_SystemCreateTimeUtc = ZDateTime.UtcNow;
				ercNumber.CE_SystemCreateUser = "~BP";
			}

			var menuItem = CreateMenuItem(DataContext.FRPortsIntegrationDossier);
			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
			var result = supporter.GetAdditionalData(shipment, menuItem);

			var packline1 = shipment.OuterPackLines[0];
			var portReferencesInPackLine1 = packline1.PortReferences.ToList<CusEntryNumber>();
			var additionalReferenceNumbersInPackLine1 = packline1.AdditionalReferenceNumbers.ToList<CusEntryNumber>();
			var cusEutryNumsInPackLine1 = packline1.CusEntryNums.OrderBy(x => x.CE_Category).ToList();

			var packline2 = shipment.OuterPackLines[1];
			var portReferencesInPackLine2 = packline2.PortReferences.ToList<CusEntryNumber>();
			var additionalReferenceNumbersInPackLine2 = packline2.AdditionalReferenceNumbers.ToList<CusEntryNumber>();
			var cusEutryNumsInPackLine2 = packline2.CusEntryNums.OrderBy(x => x.CE_Category).ToList();

			var packline3 = shipment.OuterPackLines[2];
			var portReferencesInPackLine3 = packline3.PortReferences.ToList<CusEntryNumber>();
			var additionalReferenceNumbersInPackLine3 = packline3.AdditionalReferenceNumbers.ToList<CusEntryNumber>();
			var cusEutryNumsInPackLine3 = packline3.CusEntryNums.OrderBy(x => x.CE_Category).ToList();

			var expectedErrorReport = $@"log with SL_SE_NKEvent == MAA and matches document name Goods Received (CRESA) was not found in shipment SH0001000 logs.
AllPackingLinesHavePANAndERC return false.
cfsCountryCode: FR
packLine ({packline1.PK}): {packline1.JL_PackLineId} has 1 PortReference(s) and 1 AdditionalReferenceNumber(s).
cusEntryNumber in PortReferences:
PK: {portReferencesInPackLine1[0].PK}, CE_ParentID: {packline1.PK}, CE_ParentTable: JobPackLines, CE_EntryNum: Number_1, CE_RN_NKCountryCode: FR, CE_EntryType: ERC,
CE_EntryStatus: TST, CE_Category: PRT, CE_EntryLineReference: reference, CE_IssueDate: 01-Jan-24 00:00:00, CE_ExpiryDate: 01-Jan-24 00:00:00, CE_EntryIsSystemGenerated: Y,
SystemLastEditTimeUTC: , SystemLastEditUser: , CE_SystemCreateTimeUtc: 01-Jan-24 00:00:00, CE_SystemCreateUser: ~BP

cusEntryNumber in AdditionalReferenceNumbers:
PK: {additionalReferenceNumbersInPackLine1[0].PK}, CE_ParentID: {packline1.PK}, CE_ParentTable: JobPackLines, CE_EntryNum: Number_1, CE_RN_NKCountryCode: FR, CE_EntryType: PAN,
CE_EntryStatus: MSN, CE_Category: OTH, CE_EntryLineReference: reference, CE_IssueDate: 01-Jan-24 00:00:00, CE_ExpiryDate: 01-Jan-24 00:00:00, CE_EntryIsSystemGenerated: N,
SystemLastEditTimeUTC: , SystemLastEditUser: , CE_SystemCreateTimeUtc: 01-Jan-24 00:00:00, CE_SystemCreateUser: ~BP

cusEntryNumber in CusEntryNums:
PK: {cusEutryNumsInPackLine1[0].PK}, CE_ParentID: {packline1.PK}, CE_ParentTable: JobPackLines, CE_EntryNum: Number_1, CE_RN_NKCountryCode: FR, CE_EntryType: PAN,
CE_EntryStatus: MSN, CE_Category: OTH, CE_EntryLineReference: reference, CE_IssueDate: 01-Jan-24 00:00:00, CE_ExpiryDate: 01-Jan-24 00:00:00, CE_EntryIsSystemGenerated: N,
SystemLastEditTimeUTC: , SystemLastEditUser: , CE_SystemCreateTimeUtc: 01-Jan-24 00:00:00, CE_SystemCreateUser: ~BP

PK: {cusEutryNumsInPackLine1[1].PK}, CE_ParentID: {packline1.PK}, CE_ParentTable: JobPackLines, CE_EntryNum: Number_1, CE_RN_NKCountryCode: FR, CE_EntryType: ERC,
CE_EntryStatus: TST, CE_Category: PRT, CE_EntryLineReference: reference, CE_IssueDate: 01-Jan-24 00:00:00, CE_ExpiryDate: 01-Jan-24 00:00:00, CE_EntryIsSystemGenerated: Y,
SystemLastEditTimeUTC: , SystemLastEditUser: , CE_SystemCreateTimeUtc: 01-Jan-24 00:00:00, CE_SystemCreateUser: ~BP

packLine ({packline2.PK}): {packline2.JL_PackLineId} has 1 PortReference(s) and 1 AdditionalReferenceNumber(s).
cusEntryNumber in PortReferences:
PK: {portReferencesInPackLine2[0].PK}, CE_ParentID: {packline2.PK}, CE_ParentTable: JobPackLines, CE_EntryNum: Number_2, CE_RN_NKCountryCode: FR, CE_EntryType: ERC,
CE_EntryStatus: TST, CE_Category: PRT, CE_EntryLineReference: reference, CE_IssueDate: 01-Jan-24 00:00:00, CE_ExpiryDate: 01-Jan-24 00:00:00, CE_EntryIsSystemGenerated: Y,
SystemLastEditTimeUTC: , SystemLastEditUser: , CE_SystemCreateTimeUtc: 01-Jan-24 00:00:00, CE_SystemCreateUser: ~BP

cusEntryNumber in AdditionalReferenceNumbers:
PK: {additionalReferenceNumbersInPackLine2[0].PK}, CE_ParentID: {packline2.PK}, CE_ParentTable: JobPackLines, CE_EntryNum: Number_2, CE_RN_NKCountryCode: FR, CE_EntryType: PAN,
CE_EntryStatus: MSN, CE_Category: OTH, CE_EntryLineReference: reference, CE_IssueDate: 01-Jan-24 00:00:00, CE_ExpiryDate: 01-Jan-24 00:00:00, CE_EntryIsSystemGenerated: N,
SystemLastEditTimeUTC: , SystemLastEditUser: , CE_SystemCreateTimeUtc: 01-Jan-24 00:00:00, CE_SystemCreateUser: ~BP

cusEntryNumber in CusEntryNums:
PK: {cusEutryNumsInPackLine2[0].PK}, CE_ParentID: {packline2.PK}, CE_ParentTable: JobPackLines, CE_EntryNum: Number_2, CE_RN_NKCountryCode: FR, CE_EntryType: PAN,
CE_EntryStatus: MSN, CE_Category: OTH, CE_EntryLineReference: reference, CE_IssueDate: 01-Jan-24 00:00:00, CE_ExpiryDate: 01-Jan-24 00:00:00, CE_EntryIsSystemGenerated: N,
SystemLastEditTimeUTC: , SystemLastEditUser: , CE_SystemCreateTimeUtc: 01-Jan-24 00:00:00, CE_SystemCreateUser: ~BP

PK: {cusEutryNumsInPackLine2[1].PK}, CE_ParentID: {packline2.PK}, CE_ParentTable: JobPackLines, CE_EntryNum: Number_2, CE_RN_NKCountryCode: FR, CE_EntryType: ERC,
CE_EntryStatus: TST, CE_Category: PRT, CE_EntryLineReference: reference, CE_IssueDate: 01-Jan-24 00:00:00, CE_ExpiryDate: 01-Jan-24 00:00:00, CE_EntryIsSystemGenerated: Y,
SystemLastEditTimeUTC: , SystemLastEditUser: , CE_SystemCreateTimeUtc: 01-Jan-24 00:00:00, CE_SystemCreateUser: ~BP

packLine ({packline3.PK}): {packline3.JL_PackLineId} has 1 PortReference(s) and 1 AdditionalReferenceNumber(s).
cusEntryNumber in PortReferences:
PK: {portReferencesInPackLine3[0].PK}, CE_ParentID: {packline3.PK}, CE_ParentTable: JobPackLines, CE_EntryNum: Number_3, CE_RN_NKCountryCode: FR, CE_EntryType: ERC,
CE_EntryStatus: TST, CE_Category: PRT, CE_EntryLineReference: reference, CE_IssueDate: 01-Jan-24 00:00:00, CE_ExpiryDate: 01-Jan-24 00:00:00, CE_EntryIsSystemGenerated: Y,
SystemLastEditTimeUTC: , SystemLastEditUser: , CE_SystemCreateTimeUtc: 01-Jan-24 00:00:00, CE_SystemCreateUser: ~BP

cusEntryNumber in AdditionalReferenceNumbers:
PK: {additionalReferenceNumbersInPackLine3[0].PK}, CE_ParentID: {packline3.PK}, CE_ParentTable: JobPackLines, CE_EntryNum: Number_3, CE_RN_NKCountryCode: FR, CE_EntryType: PAN,
CE_EntryStatus: MSN, CE_Category: OTH, CE_EntryLineReference: reference, CE_IssueDate: 01-Jan-24 00:00:00, CE_ExpiryDate: 01-Jan-24 00:00:00, CE_EntryIsSystemGenerated: N,
SystemLastEditTimeUTC: , SystemLastEditUser: , CE_SystemCreateTimeUtc: 01-Jan-24 00:00:00, CE_SystemCreateUser: ~BP

cusEntryNumber in CusEntryNums:
PK: {cusEutryNumsInPackLine3[0].PK}, CE_ParentID: {packline3.PK}, CE_ParentTable: JobPackLines, CE_EntryNum: Number_3, CE_RN_NKCountryCode: FR, CE_EntryType: PAN,
CE_EntryStatus: MSN, CE_Category: OTH, CE_EntryLineReference: reference, CE_IssueDate: 01-Jan-24 00:00:00, CE_ExpiryDate: 01-Jan-24 00:00:00, CE_EntryIsSystemGenerated: N,
SystemLastEditTimeUTC: , SystemLastEditUser: , CE_SystemCreateTimeUtc: 01-Jan-24 00:00:00, CE_SystemCreateUser: ~BP

PK: {cusEutryNumsInPackLine3[1].PK}, CE_ParentID: {packline3.PK}, CE_ParentTable: JobPackLines, CE_EntryNum: Number_3, CE_RN_NKCountryCode: FR, CE_EntryType: ERC,
CE_EntryStatus: TST, CE_Category: PRT, CE_EntryLineReference: reference, CE_IssueDate: 01-Jan-24 00:00:00, CE_ExpiryDate: 01-Jan-24 00:00:00, CE_EntryIsSystemGenerated: Y,
SystemLastEditTimeUTC: , SystemLastEditUser: , CE_SystemCreateTimeUtc: 01-Jan-24 00:00:00, CE_SystemCreateUser: ~BP";

			AssertEquals($"Error when no ECV/RCN refs and no CRESA send", "Cargo belongs to all pack lines not received into the warehouse and CRESA is not finalized with CCS. You are not allowed to send File Creation Request (DOS) message until finalized the CRESA.", result.Left);
			AssertEquals("Log for CS01641584 - BOLLOGPAR1 - ERROR MESSAGE TO PUSH \"DOS\" TO SONE CCS", ErrorReporter.LastKeyReported);
			AssertStartsWith("Exception Message", expectedErrorReport, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestShipmentDOS_RCNAreSendInOtherShipment()
		{
			var shipmentThatAlreadySendRefs = CreateFRShipment("SH0001001");
			var consolThatAlreadySendRefs = CreateFRConsol("C00001001", isImport: true);
			shipmentThatAlreadySendRefs.Consols.Add(consolThatAlreadySendRefs);

			var i = 0;
			foreach (ForwardingPackLine packline in shipmentThatAlreadySendRefs.OuterPackLines)
			{
				i++;
				var ecvNumber = Factory.New<CusEntryNumber>();
				ecvNumber.Parent = packline;
				ecvNumber.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN;
				ecvNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
				ecvNumber.CE_EntryNum = $"ECV_{i}";
				ecvNumber.CE_Category = "PRT";
				var ercNumber = packline.AdditionalReferenceNumbers.AddNew();
				ercNumber.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC;
				ercNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
				ercNumber.CE_EntryNum = $"ERC_{i}";
				ercNumber.CE_EntryStatus = Events.MessageSentCode;
			}
			CreateEventFrenchPorts(shipmentThatAlreadySendRefs, AutoEvents.MessageAccepted, documentName: FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA);

			var shipmentWithTheSameRefs = CreateFRShipment("SH0001002");
			var consolWithTheSameRefs = CreateFRConsol("C00001002", isImport: true);
			shipmentWithTheSameRefs.Consols.Add(consolWithTheSameRefs);

			i = 0;
			foreach (ForwardingPackLine packline in shipmentWithTheSameRefs.OuterPackLines)
			{
				i++;
				var ecvNumber = Factory.New<CusEntryNumber>();
				ecvNumber.Parent = packline;
				ecvNumber.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN;
				ecvNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
				ecvNumber.CE_EntryNum = $"ECV_{i}";
				ecvNumber.CE_Category = "PRT";
				var ercNumber = packline.AdditionalReferenceNumbers.AddNew();
				ercNumber.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC;
				ercNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
				ercNumber.CE_EntryNum = $"ERC_{i}";
			}

			var menuItem = CreateMenuItem(DataContext.FRPortsIntegrationDossier);
			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipmentWithTheSameRefs);
			var result = supporter.GetAdditionalData(shipmentWithTheSameRefs, menuItem);

			AssertEquals($"Error when refs send on another shipment", true, result.IsLeft);
			AssertEquals($"Error when refs send on another shipment", false, result.IsRight);
			AssertEquals($"Error when refs send on another shipment", $"File Creation Request (DOS) being sent from Shipment {shipmentThatAlreadySendRefs.JS_UniqueConsignRef} for all received consignment(s). You are not allowed to send File Creation Request message from this Shipment.", result.Left);

			shipmentWithTheSameRefs.OuterPackLines[1].PortReferences.Cast<CusEntryNumber>().First().CE_EntryNum = "ECV-xxx-2";
			shipmentWithTheSameRefs.OuterPackLines[1].AdditionalReferenceNumbers.Cast<CusEntryNumber>().First().CE_EntryNum = "ERC-xxx-2";
			Factory.Save();

			menuItem = CreateMenuItem(DataContext.FRPortsIntegrationDossier);
			supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipmentWithTheSameRefs);
			result = supporter.GetAdditionalData(shipmentWithTheSameRefs, menuItem);

			AssertEquals($"Error when some refs send on another shipment", false, result.IsLeft);
			AssertEquals($"Error when some refs send on another shipment", true, result.IsRight);
			if (result.Right is List<ZString> notSendRefs)
			{
				AssertEquals("return object contains not send refs ERC-xxx-2", "ERC-xxx-2", string.Join(", ", notSendRefs));

				AssertEquals($"File Creation Request (DOS) being sent from Shipment {shipmentThatAlreadySendRefs.JS_UniqueConsignRef} for received consignment(s) ERC_1, ERC_3. The File Creation Request (DOS) message from this Shipment will send with remaining received consignment(s).", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			}

			i = 0;
			foreach (ForwardingPackLine packline in shipmentWithTheSameRefs.OuterPackLines)
			{
				packline.AdditionalReferenceNumbers.RemoveAndDeleteAll();
				i++;
				var ecvNumber = Factory.New<CusEntryNumber>();
				ecvNumber.Parent = packline;
				ecvNumber.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN;
				ecvNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
				ecvNumber.CE_EntryNum = $"ECV-xxx_{i}";
				ecvNumber.CE_Category = "PRT";
				var ercNumber = packline.AdditionalReferenceNumbers.AddNew();
				ercNumber.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC;
				ercNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
				ercNumber.CE_EntryNum = $"ERC-xxx_{i}";
			}

			menuItem = CreateMenuItem(DataContext.FRPortsIntegrationDossier);
			supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipmentWithTheSameRefs);
			result = supporter.GetAdditionalData(shipmentWithTheSameRefs, menuItem);

			AssertEquals($"No Error when different ECV/RCN refs and no CRESA send", true, result.IsRight);
			AssertEquals($"No Error when different ECV/RCN refs and no CRESA send", false, result.IsLeft);
		}

		public void TestGetAdditionalData_FrenchPort_CRESA()
		{
			var shipment = CreateFRShipment();
			var consol = CreateFRConsol(isImport: true);
			shipment.Consols.Add(consol);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "FRMRS";
			shipment.JS_RL_NKDestination = "NZAKL";

			var context = DataContext.FRPortsGoodsReceivedCRESA;
			var errorMessage = "Goods Received (CRESA) Reporting is only available from Shipments with types STD, BCN or HVL.";

			AssertForTestGetAdditionalData_FrenchPort(shipment, true, Core.Constants.ShipmentTypes.AssemblyMaster, context, errorMessage);
			AssertForTestGetAdditionalData_FrenchPort(shipment, true, Core.Constants.ShipmentTypes.BlindCoLoadMaster, context, errorMessage);
			AssertForTestGetAdditionalData_FrenchPort(shipment, true, Core.Constants.ShipmentTypes.CoLoadMaster, context, errorMessage);
			AssertForTestGetAdditionalData_FrenchPort(shipment, true, Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy, context, errorMessage);
			AssertForTestGetAdditionalData_FrenchPort(shipment, true, Core.Constants.ShipmentTypes.ShippersConsolLead, context, errorMessage);
			AssertForTestGetAdditionalData_FrenchPort(shipment, true, Core.Constants.ShipmentTypes.ThirdPartyOwnershipHouse, context, errorMessage);

			AssertForTestGetAdditionalData_FrenchPort(shipment, false, Core.Constants.ShipmentTypes.BuyersConsolLead, context);
			AssertForTestGetAdditionalData_FrenchPort(shipment, false, Core.Constants.ShipmentTypes.StandardHouse, context);
			AssertForTestGetAdditionalData_FrenchPort(shipment, false, Core.Constants.ShipmentTypes.HighVolumeLowValue, context);
		}

		public void TestGetAdditionalData_FrenchPort_DOS()
		{
			var shipment = CreateFRShipment();
			var consol = CreateFRConsol(isImport: true);
			shipment.Consols.Add(consol);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "FRMRS";
			shipment.JS_RL_NKDestination = "NZAKL";
			foreach (ForwardingPackLine packline in shipment.OuterPackLines)
			{
				var ecvNumber = Factory.New<CusEntryNumber>();
				ecvNumber.Parent = packline;
				ecvNumber.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN;
				ecvNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
				ecvNumber.CE_EntryNum = "ECV";
				ecvNumber.CE_Category = "PRT";
				var ercNumber = packline.AdditionalReferenceNumbers.AddNew();
				ercNumber.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC;
				ercNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
				ercNumber.CE_EntryNum = "ERC";
			}
			CreateEventFrenchPorts(shipment, AutoEvents.MessageSent, documentName: FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA);

			var context = DataContext.FRPortsIntegrationDossier;
			var errorMessage = "File Creation Request (DOS) Reporting is only available from Shipments with types STD, BCN or HVL.";

			AssertForTestGetAdditionalData_FrenchPort(shipment, true, Core.Constants.ShipmentTypes.AssemblyMaster, context, errorMessage);
			AssertForTestGetAdditionalData_FrenchPort(shipment, true, Core.Constants.ShipmentTypes.BlindCoLoadMaster, context, errorMessage);
			AssertForTestGetAdditionalData_FrenchPort(shipment, true, Core.Constants.ShipmentTypes.CoLoadMaster, context, errorMessage);
			AssertForTestGetAdditionalData_FrenchPort(shipment, true, Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy, context, errorMessage);
			AssertForTestGetAdditionalData_FrenchPort(shipment, true, Core.Constants.ShipmentTypes.ShippersConsolLead, context, errorMessage);
			AssertForTestGetAdditionalData_FrenchPort(shipment, true, Core.Constants.ShipmentTypes.ThirdPartyOwnershipHouse, context, errorMessage);

			AssertForTestGetAdditionalData_FrenchPort(shipment, false, Core.Constants.ShipmentTypes.BuyersConsolLead, context);
			AssertForTestGetAdditionalData_FrenchPort(shipment, false, Core.Constants.ShipmentTypes.StandardHouse, context);
			AssertForTestGetAdditionalData_FrenchPort(shipment, false, Core.Constants.ShipmentTypes.HighVolumeLowValue, context);
		}

		void AssertForTestGetAdditionalData_FrenchPort(ForwardingShipment shipment, bool isErrorExpected, string shipmentType, string menuItemType, string errorMessage = null)
		{
			shipment.JS_ShipmentType = shipmentType;

			var menuItem = CreateMenuItem(menuItemType);
			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
			var result = supporter.GetAdditionalData(shipment, menuItem);

			if (isErrorExpected)
			{
				Assert($"An error is expected when the shipment type is {shipmentType}", result.IsLeft);
				AssertEquals($"This error message is expected when shipment type is {shipmentType}", errorMessage, result.Left);
			}
			else
			{
				Assert($"No error is expected when shipment type is {shipmentType}", result.IsRight);
				AssertEquals($"No error is expected when shipment type is {shipmentType}.", false, result.IsLeft);
			}
		}

		public void TestGetAdditionalData_FrenchPort_CRESA_RestrictionOnPortAuthorityControlled()
		{
			var shipment = CreateFRShipment();
			var consol = CreateFRConsol(isImport: true);
			shipment.Consols.Add(consol);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "FRMRS";
			shipment.JS_RL_NKDestination = "NZAKL";

			var cfs = Factory.New<OrgHeader>();
			cfs.OH_Code = "PACTST";
			cfs.OH_FullName = "Port Authority Controlled Test";
			cfs.OH_RL_NKClosestPort = "FRPAR";
			cfs.MainAddress.Address1 = "Unit 15";
			cfs.MainAddress.Address2 = "5 Lost Lane";
			cfs.MainAddress.City = "Marseille";
			cfs.MainAddress.Postcode = "2000";
			cfs.MainAddress.OA_RN_NKCountryCode = "FR";
			cfs.MainAddress.OA_RL_NKRelatedPortCode = "FRPAR";

			shipment.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;

			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse("WH1");
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			warehouse.WW_IsCustomsControlled = true;
			warehouse.WW_IsPortAuthorityControlled = true;

			warehouse.WW_OA_WarehouseAddress = cfs.MainAddress.PK;

			var context = DataContext.FRPortsGoodsReceivedCRESA;
			var errorMessage = $"The Goods Received (CRESA) message cannot be sent from Shipment for the CFS/Transit Warehouse [{cfs.NameAndCode}, {cfs.MainAddress.Address1}].\r\nThis CFS/Transit Warehouse is configured to send Port Authority Messages themselves, and as such CRESA message should be sent from the Transit Warehouse > Received Consignment Module.";

			AssertForTestGetAdditionalData_FrenchPort(shipment, true, Core.Constants.ShipmentTypes.StandardHouse, context, errorMessage);
		}

		#endregion

		static readonly string[] CertificateOfOriginMenuItems =
		{
			"894ad385-11da-477c-aad3-7567d2d0f7f0", // NZCFTA
			"64944e3e-c8be-4069-b38d-f5eeedb95f96", // AANZFTA
			"0628b566-68dc-4f75-b703-6591a4be135a", // ChAFTA
			"25f8dd69-d756-492f-a47a-e4430f24bf32", // JAEPA
			"c5b75347-4bec-4194-b1a0-946b413fdf42", // RCEP
			"1f737f08-7eda-41cf-8962-a6679fded290"  // NZCOO
		};
		
		public void TestGetAdditionalData_CertificateOfOrigin_NoPackLines_NoInvoiceLines()
		{
				foreach (var id in CertificateOfOriginMenuItems)
				{
					TestCase(id);
				}
				void TestCase(string menuItemString)
				{
					var shipment = Factory.New<ForwardingShipment>();
					var menuItem = Factory.Load<DocumentCommand>(ZGuid.ParseSafe(menuItemString));
					var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
					var result = supporter.GetAdditionalData(shipment, menuItem);
					Assert(result.IsLeft);
					AssertEquals("To create a Certificate of Origin, this Shipment requires at least one pack line entry in the Packing tab or one invoice line entry in the Brokerage tab.", result.Left);
				}
		}

		public void TestGetAdditionalData_CertificateOfOrigin_PackLinesPresent_InvoiceLinesAbsent()
		{
			foreach (var id in CertificateOfOriginMenuItems)
			{
				TestCase(id);
			}
			void TestCase(string menuItemString)
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.OuterPackLines.AddNew();
				var menuItem = Factory.Load<DocumentCommand>(ZGuid.ParseSafe(menuItemString));
				var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
				var result = supporter.GetAdditionalData(shipment, menuItem);

				Assert(shipment.OuterPackLines.Count > 0);
				var hasInvoices = shipment.Declarations.Cast<BaseJobDeclaration>().Any(declaration => declaration.Invoices != null && declaration.Invoices.Count > 0);
				Assert(!hasInvoices);
				Assert("$No error is expected when pack lines are present but invoice lines are absent", result.IsRight);
			}
		}

		public void TestGetAdditionalData_CertificateOfOrigin_PackLinesAbsent_InvoiceLinesPresent()
		{
			foreach (var id in CertificateOfOriginMenuItems)
			{
				TestCase(id);
			}
			void TestCase(string menuItemString)
			{
				var shipment = Factory.New<ForwardingShipmentForTest>();
				var menuItem = Factory.Load<DocumentCommand>(ZGuid.ParseSafe(menuItemString));
				var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
				var declaration = Factory.New<BaseJobDeclaration>();
				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.InvoiceLines.AddNew();
				var declarations = new List<BaseJobDeclaration>(1);
				declarations.Add(declaration);
				shipment.SetDeclaration(declarations.ToArray());

				var result = supporter.GetAdditionalData(shipment, menuItem);

				Assert(shipment.OuterPackLines.Count == 0);
				var hasInvoices = shipment.Declarations.Cast<BaseJobDeclaration>().Any(declaration => declaration.Invoices != null && declaration.Invoices.Count > 0);
				Assert(hasInvoices);
				Assert("$No error is expected when invoice lines are present but pack lines are absent", result.IsRight);
			}
		}

		public void TestGetAdditionalData_CertificateOfOrigin_NZCFTA_TooManyPackLines()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var menuItem = Factory.Load<DocumentCommand>(ZGuid.ParseSafe("894ad385-11da-477c-aad3-7567d2d0f7f0"));
			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);

			for (var i = 0; i < 30; i++)
			{
				shipment.OuterPackLines.AddNew();
			}

			var result = supporter.GetAdditionalData(shipment, menuItem);

			Assert(result.IsLeft);
			AssertEquals("An NZCFTA Certificate can only contain a maximum of 20 pack lines. Please edit in the Packing tab to be able to create the Certificate.", result.Left);
		}

		public void TestGetAdditionalData_USATF6A()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var shipment = Factory.New<ForwardingShipment>();
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_JS = shipment.PK;

				var template = new Mock<IStmTemplate>();
				template.SetupGet(x => x.SO_DataContext).Returns(DocumentVisualizer.Integration.DataContext.USATF6A);
				var pivot = new Mock<IStmMenuTemplatePivot>();
				pivot.SetupGet(x => x.Template).Returns(template.Object);
				var menu = new Mock<IStmMenuItem>();
				menu.SetupGet(x => x.Documents).Returns(new[] { pivot.Object });

				var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
				var result = supporter.GetAdditionalData(shipment, menu.Object);
				Assert(result.IsRight);
				var resultData = result.Right as ZString[];
				AssertEquals(0, resultData.Length);
			}
		}

		public void TestGetAdditionalData_PackLines_GoodsDescriptionLineLimit()
		{
			foreach (var menuItemString in CertificateOfOriginMenuItems)
			{
				var shipment = Factory.New<ForwardingShipment>();
				var menuItem = Factory.Load<DocumentCommand>(ZGuid.ParseSafe(menuItemString));
				var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);

				shipment.OuterPackLines.AddNew().JL_Description = new ZString("One\nTwo\nThree\nFour\nFive");

				var result = supporter.GetAdditionalData(shipment, menuItem);
				AssertEquals("A maximum number of four lines for Goods Description is allowed for each line item. Only the first four lines of the Goods Description will be included on the Certificate of Origin. Click OK to proceed or press Cancel to go back and amend the goods description.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		public void TestGetAdditionalData_InvoiceLines_GoodsDescriptionLineLimit()
		{
			foreach (var menuItemString in CertificateOfOriginMenuItems)
			{
				var shipment = Factory.New<ForwardingShipmentForTest>();
				var menuItem = Factory.Load<DocumentCommand>(ZGuid.ParseSafe(menuItemString));
				var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);

				var declaration = Factory.New<BaseJobDeclaration>();
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

				invoiceLine.JI_Description = new ZString("Line1\nLine2\nLine3\nLine4\nLine5");
				var declarations = new List<BaseJobDeclaration>(1);
				declarations.Add(declaration);
				shipment.SetDeclaration(declarations.ToArray());

				var result = supporter.GetAdditionalData(shipment, menuItem);

				Assert(shipment.OuterPackLines.Count == 0);
				AssertEquals("A maximum number of four lines for Goods Description is allowed for each line item. Only the first four lines of the Goods Description will be included on the Certificate of Origin. Click OK to proceed or press Cancel to go back and amend the goods description.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		#endregion

		#region TestGetCustomCommands

		public void TestGetCustomCommands_ConsolsBRSent_BookingRequest()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_UniqueConsignRef = "C00000069";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "CNSHA";
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_UniqueConsignRef = "S000001";

			Factory.Save();

			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
			var commands = supporter.GetCustomCommands(DataContext.BookingRequest);

			bool HasSendMessageCommand(IEnumerable<ICommand> commandsCollection)
			{
				return commandsCollection
					.OfType<DisabledCommand>()
					.Any(cmd => cmd.Id == CommandIds.SendMessage);
			}

			Assert("Send menu is enabled without consols BR sending", !HasSendMessageCommand(commands));

			CreateEvent(consol, Events.MessageSent, ConsolDocumentNames.BookingRequest);
			commands = supporter.GetCustomCommands(DataContext.BookingRequest);

			Assert("Send menu is disabled with consols BR sending", HasSendMessageCommand(commands));
		}

		public void TestGetCustomCommands_BookingIsNotSent_BookingRequest()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_UniqueConsignRef = "C00000069";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "CNSHA";
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_UniqueConsignRef = "S000001";

			Factory.Save();

			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
			var commands = supporter.GetCustomCommands(DataContext.BookingRequest);

			Assert("Withdraw/Cancel menu is disabled by the custom command", commands.OfType<DisabledCommand>().Any());
			Assert("Reset to Original menu is disabled by the custom command", commands.OfType<DisabledCommand>().Any());

			CreateEvent(shipment, Events.MessageSent, ConsolDocumentNames.BookingRequest);

			commands = supporter.GetCustomCommands(DataContext.BookingRequest);

			Assert("Using default menu", !commands.OfType<DisabledCommand>().Any());
			Assert("Using default menu", !commands.OfType<DisabledCommand>().Any());
		}

		public void TestGetCustomCommands_HasOpenCOOrderFormCommand()
		{
			foreach (var context in CertificateOfOriginConstants.DataContexts)
			{
				TestGetCustomCommands_HasOpenCOOrderFormCommand(context);
			}
		}

		void TestGetCustomCommands_HasOpenCOOrderFormCommand(string dataContext)
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
			var commands = supporter.GetCustomCommands(dataContext);

			AssertEquals(1, commands.Count());
			Assert(commands.OfType<OpenCOOrderFormCommand>().Any());
		}

		public void TestGetCustomCommands_CCTShipmentReport()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
			var commands = supporter.GetCustomCommands(DataContext.CargoControlAndTransit);

			AssertEquals(1, commands.Count());
			Assert(commands.OfType<SendWithdrawCargoControlAndTransitCommand>().Any());
		}

		public void TestGetCustomCommands_ILDeliveryOrder()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
			var commands = supporter.GetCustomCommands(DataContext.ILDeliveryOrder);

			AssertEquals(3, commands.Count());
			Assert(commands.OfType<ILDLOSendMessageCommand>().Any());
			Assert(commands.OfType<ILDLOSendWithdrawalMessageCommand>().Any());
			Assert(commands.OfType<ILDLOResetToOriginalMessageCommand>().Any());
		}

		public void TestGetCustomCommands_ILGatePassMovement()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
			var commands = supporter.GetCustomCommands(DataContext.ILGatePassMovement);
			AssertEquals(3, commands.Count());
			Assert(commands.OfType<ILGPMSendMessageCommand>().Any());
			Assert(commands.OfType<ILGPMSendWithdrawalMessageCommand>().Any());
			Assert(commands.OfType<ILGPMResetToOriginalMessageCommand>().Any());
		}

		public void TestGetCustomCommands_HouseBill()
		{
			var shipment = Factory.New<ForwardingShipment>();

			shipment.JS_HouseBillOfLadingType = Core.Constants.HouseBillOfLadingTypes.Code.DataHawkBill;
			shipment.IsEditingElectronicBOL = false;

			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
			var commands = supporter.GetCustomCommands(DataContext.HouseBill);
			AssertEquals(0, commands.Count());

			shipment.IsEditingElectronicBOL = true;
			commands = supporter.GetCustomCommands(DataContext.HouseBill);
			AssertEquals(1, commands.Count());
			Assert(commands.OfType<PublishHouseBillCommand>().Any());

			shipment.JS_HouseBillOfLadingType = Core.Constants.HouseBillOfLadingTypes.Code.FIATAHBL;

			supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
			commands = supporter.GetCustomCommands(DataContext.HouseBill);

			AssertEquals(0, commands.Count());
		}

		#endregion

		#region TestGetAdditionalDocuments

		public void TestGetAdditionalDocuments_ElectronicBOL()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.EnglishBritish))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				shipment.IsEditingElectronicBOL = true;
				shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.ITClubAustralia;
				shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;

				var documentData = Factory.New<VisualizerDocumentData>();
				var document = new Mock<IDocument>();
				var messageInstructions = new Mock<IMessageInstructions>();

				document.SetupGet(d => d.Data).Returns(new HouseBill(nameof(ForwardingShipment), shipment.JS_UniqueConsignRef).MakeDynamic());
				messageInstructions.SetupGet(d => d.DocumentName).Returns("Electronic Bill Of Lading");
				messageInstructions.SetupGet(d => d.DataContext).Returns("HouseBill");

				var boleroEBLConfiguration = new BoleroEBLConfiguration()
				{
					EnableEBLIntegration = true,
					GalileoEndPointUrl = "http://test.test",
					GalileoAudience = Guid.NewGuid().ToString(),
					GalileoTestEndPointUrl = "http://test.test",
					GalileoTestAudience = Guid.NewGuid().ToString()
				};

				using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
				{
					var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
					var result = supporter.GetAdditionalDocuments(document.Object, messageInstructions.Object);

					AssertEquals(1, result.Count());
					var carrierHouseBill = result.First().Data.Value as HouseBill;
					Assert(carrierHouseBill.IsElectronicBOL);
					AssertEquals("Cubic Meters", carrierHouseBill.TotalVolume.Unit.Description);
					AssertEquals("HouseBill", result.First().DataContext);

					shipment.IsEditingElectronicBOL = false;

					supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
					result = supporter.GetAdditionalDocuments(document.Object, messageInstructions.Object);

					AssertEquals(0, result.Count());

					shipment.IsEditingElectronicBOL = true;
					shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.FIATAHBL;

					supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
					result = supporter.GetAdditionalDocuments(document.Object, messageInstructions.Object);

					AssertEquals(0, result.Count());
				}

				boleroEBLConfiguration.EnableEBLIntegration = false;

				using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
				{
					shipment.IsEditingElectronicBOL = true;
					shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.CargowiseBill;

					var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
					var result = supporter.GetAdditionalDocuments(document.Object, messageInstructions.Object);

					AssertEquals(0, result.Count());
				}
			}
		}

		public void TestGetAdditionalDocumentsWithOverriddenData_ElectronicBOL()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.IsEditingElectronicBOL = true;
			shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.ITClubAustralia;

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = "BillOfLading";
			documentData.JDD_ParentID = shipment.PK;
			documentData.JDD_ParentTableCode = shipment.TablePrefix;

			#region Overridden XML

			var xml = @"
<Entity>
<Id>794af3fe-d25e-43bd-b70b-cb1d7a32cee0</Id>
<Property Name=""NotifyParty"">
<Entity>
    <Id>0de6aaef-a516-481d-9388-0a688ed59818</Id>
    <Property Name=""AddressFormatted"">
    <Value>TESTNOTIFY
350 PITT ST
SYDNEY NSW 2000
AUSTRALIA</Value>
    </Property>
	<Property Name=""CompanyName"">
    <Value>TestNotify</Value>
    </Property>
	<Property Name=""AddressLine1"">
    <Value>350 Pitt St</Value>
    </Property>
</Entity>
</Property>
</Entity>
";
			#endregion

			var xmlDoc = XDocument.Parse(xml);
			documentData.WriteXml(xmlDoc);

			var document = new Mock<IDocument>();
			var messageInstructions = new Mock<IMessageInstructions>();

			document.SetupGet(d => d.Data).Returns(new HouseBill(nameof(ForwardingShipment), shipment.JS_UniqueConsignRef).MakeDynamic());
			messageInstructions.SetupGet(d => d.DocumentName).Returns("Electronic Bill Of Lading");
			messageInstructions.SetupGet(d => d.DataContext).Returns("HouseBill");

			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
				var result = supporter.GetAdditionalDocuments(document.Object, messageInstructions.Object);

				AssertEquals(1, result.Count());
				var carrierHouseBill = result.First().Data.Value as HouseBill;
				AssertNotNull(carrierHouseBill);
				Assert(carrierHouseBill.IsElectronicBOL);

				AssertEquals("TESTNOTIFY\r\n350 PITT ST\r\nSYDNEY NSW 2000\r\nAUSTRALIA", carrierHouseBill.NotifyParty.AddressFormatted);
				AssertEquals("TestNotify", carrierHouseBill.NotifyParty.CompanyName);
				AssertEquals("350 Pitt St", carrierHouseBill.NotifyParty.AddressLine1);
			}
		}

		#endregion

		#region TestGetBusinessObjectInAnotherFactory

		public void TestGetBusinessObjectInAnotherFactory()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.IsEditingElectronicBOL = false;
			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);

			var result = supporter.GetBusinessObjectInAnotherFactory(new BusinessObjectFactory(), shipment) as ForwardingShipment;

			AssertEquals(shipment.PK, result.PK);
			AssertNotEquals(shipment.Factory, result.Factory);
			Assert(!result.IsEditingElectronicBOL);

			shipment.IsEditingElectronicBOL = true;
			result = supporter.GetBusinessObjectInAnotherFactory(new BusinessObjectFactory(), shipment) as ForwardingShipment;

			AssertEquals(shipment.PK, result.PK);
			AssertNotEquals(shipment.Factory, result.Factory);
			Assert(result.IsEditingElectronicBOL);
		}

		#endregion

		#region Implementation

		ForwardingShipment CreateShipmentDestinedForBrazil()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BRSAO";
			return shipment;
		}

		ForwardingShipment CreateShipmentDestinedForUnitedStates()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USJFK";
			return shipment;
		}

		StmMenuItem CreateMenuItem(string context)
		{
			var template = Factory.New<StmTemplate>();
			template.SO_DataContext = context;
			var menuItem = Factory.New<DocumentCommand>();
			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menuItem.PK;

			return menuItem;
		}

		void CreateEvent(IStmALogParent logParent, ZArchitecture.Business.Event @event, string documentName)
		{
			var parameters = new List<KeyValuePair<string, string>>();
			parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, documentName));

			logParent.Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, parameters.ToArray());

			Factory.Save();
			Thread.Sleep(1);
		}

		#endregion

		#region FrenchPort Implementation

		void CreateEventFrenchPorts(ForwardingShipment shipment, ZArchitecture.Business.Event @event, string reference = "",
			string messageType = CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType,
			string documentName = FrenchPortsConstants.DocumentNames.DOSImport)
		{
			var parameters = new List<KeyValuePair<string, string>>();
			parameters.Add(new KeyValuePair<string, string>(messageType, documentName));

			if (!string.IsNullOrEmpty(reference))
			{
				parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, reference));
			}

			shipment.Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, parameters.ToArray());

			Factory.Save();
			Thread.Sleep(1);
		}

		ForwardingConsol CreateFRConsol(string consolNumber = "C00001000", bool isAddressAvailableToCreate = true, bool isImport = true)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = consolNumber;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			if (isImport)
			{
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "FRPRA";
			}
			else
			{
				consol.JK_RL_NKLoadPort = "FRPRA";
				consol.JK_RL_NKDischargePort = "AUSYD";
			}

			consol.JK_BookingReference = "BookingReference";
			consol.JK_MasterBillNum = "BOL";

			if (isAddressAvailableToCreate)
			{
				CreateFRAddresses(consol);
			}

			return consol;
		}

		ForwardingShipment CreateFRShipment(string shipmentNumber = "SH0001000")
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = shipmentNumber;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_RL_NKDischargePort = "FRMRS";
			shipment.JS_RL_NKDestination = "FRNCE";
			shipment.JS_RL_NKLoadPort = "FRPAR";
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_CY;
			shipment.JS_INCO = Core.Constants.IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(5);
			shipment.JS_ConsolReference = "WhiskyTreasure";
			shipment.JS_F3_NKPackType = "PLT";
			shipment.JS_GoodsDescription = "goods description";
			shipment.DetailedGoodsDescriptionNoteText = ZString.Empty;
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_UnitOfWeight = "T";
			shipment.JS_UnitOfVolume = "D3";

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 4;
			packline1.JL_F3_NKPackType = "PLT";
			packline1.JL_ActualWeight = 400;
			packline1.JL_ActualWeightUQ = "KG";
			packline1.JL_ActualVolume = 300;
			packline1.JL_ActualVolumeUQ = "M3";
			packline1.JL_Length = 1000;
			packline1.JL_Width = 1000;
			packline1.JL_Height = 300;
			packline1.JL_UnitOfDimension = "CM";
			packline1.JL_HarmonisedCode = "WHISKY";
			packline1.JL_RefNumber = "AMR-57";
			packline1.JL_DetailedDescription = "Amrut Indian Peated Single Malt Detailed Description";
			packline1.JL_MarksAndNumbers = "MarksAndNumbers1";
			packline1.JL_Description = "ALCOHOLIC BEVERAGES 57%";
			packline1.JL_LastKnownTransitWarehouseStatus = "RCV";

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 6;
			packline2.JL_F3_NKPackType = "PLT";
			packline2.JL_ActualWeight = 600;
			packline2.JL_ActualWeightUQ = "KG";
			packline2.JL_ActualVolume = 450;
			packline2.JL_ActualVolumeUQ = "M3";
			packline2.JL_Length = 15;
			packline2.JL_Width = 10;
			packline2.JL_Height = 3;
			packline2.JL_UnitOfDimension = "M";
			packline2.JL_HarmonisedCode = "WHISKY";
			packline2.JL_RefNumber = "AMR-43";
			packline2.JL_DetailedDescription = "Amrut Indian Chill Filter Single Malt Detailed Description";
			packline2.JL_MarksAndNumbers = "MarksAndNumbers2";
			packline2.JL_Description = "ALCOHOLIC BEVERAGES 43%";
			packline2.JL_LastKnownTransitWarehouseStatus = "RCV";

			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.JL_PackageCount = 6;
			packline3.JL_F3_NKPackType = "PLT";
			packline3.JL_ActualWeight = 600;
			packline3.JL_ActualWeightUQ = "KG";
			packline3.JL_ActualVolume = 450;
			packline3.JL_ActualVolumeUQ = "M3";
			packline3.JL_Length = 15;
			packline3.JL_Width = 10;
			packline3.JL_Height = 3;
			packline3.JL_UnitOfDimension = "M";
			packline3.JL_HarmonisedCode = "WHISKY";
			packline3.JL_RefNumber = "AMR-43";
			packline3.JL_DetailedDescription = "Amrut Indian Chill Filter Single Malt Detailed Description";
			packline3.JL_MarksAndNumbers = "MarksAndNumbers2";
			packline3.JL_Description = "ALCOHOLIC BEVERAGES 43%";
			packline3.JL_LastKnownTransitWarehouseStatus = "RCV";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Handsome";
			contact.OC_Phone = "1234567";

			var cfs = Factory.New<OrgHeader>();
			cfs.OH_FullName = "CONSPA";
			cfs.OH_RL_NKClosestPort = "FRPAR";
			cfs.MainAddress.Address1 = "Unit 15";
			cfs.MainAddress.Address2 = "5 Lost Lane";
			cfs.MainAddress.City = "Marseille";
			cfs.MainAddress.Postcode = "2000";
			cfs.MainAddress.OA_RN_NKCountryCode = "FR";
			cfs.MainAddress.OA_RL_NKRelatedPortCode = "FRPAR";
			shipment.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;

			Factory.Save();

			return shipment;
		}

		void CreateFRAddresses(ForwardingConsol consol)
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "DK";

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var bookingAgent = Factory.New<OrgHeader>();
			bookingAgent.OH_FullName = "I'm Handling Stuff";
			bookingAgent.OH_RL_NKClosestPort = "AUSYD";
			bookingAgent.MainAddress.Address1 = "Unit 2";
			bookingAgent.MainAddress.Address2 = "60 What Lane";
			bookingAgent.MainAddress.City = "Sydney";
			bookingAgent.MainAddress.Postcode = "2023";
			bookingAgent.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.CarrierBookingAgentDocumentaryAddress.E2_OA_Address = bookingAgent.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "YUMMY";
			receivingForwarder.OH_RL_NKClosestPort = "AUSYD";
			receivingForwarder.MainAddress.Address1 = "Unit 200";
			receivingForwarder.MainAddress.Address2 = "55 Why Lane";
			receivingForwarder.MainAddress.City = "Sydney";
			receivingForwarder.MainAddress.Postcode = "2000";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "YUMMY";
			sendingForwarder.OH_RL_NKClosestPort = "AUSYD";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "55 Why Lane";
			sendingForwarder.MainAddress.City = "Sydney";
			sendingForwarder.MainAddress.Postcode = "2000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
		}

		#endregion

		#region IL Delivery Order

		public void TestILDeliveryOrder_IsReceivingAgentVATValid()
		{
			const string expectedMessage = "Consol Receiving Agent VAT # is not configured";
			var menuItem = Factory.Load<DocumentCommand>(ShipmentSystemFormMenuItems.SendDeliveryOrderPK);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Israel;
			shipment.JS_TransportMode = "ROA";

			var importBroker = Factory.NewWithValidTestData<OrgHeader>();
			importBroker.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("VAT", "400009", "IL");
			shipment.JS_OH_ImportBroker = importBroker.PK;

			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);

			var result = supporter.GetAdditionalData(shipment, menuItem);
			Assert("There should be an error message when Arrival Consol is null", result.IsLeft);
			AssertEquals("When Arrival Consol is null", expectedMessage, result.Left);
			var consol = shipment.Consols.AddNew();

			result = supporter.GetAdditionalData(shipment, menuItem);
			Assert("There should be an error message when Receiving Forwarder is null", result.IsLeft);
			AssertEquals("When Receiving Forwarder is null", expectedMessage, result.Left);

			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			receivingForwarder.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("VAT", "", "IL");
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			result = supporter.GetAdditionalData(shipment, menuItem);
			Assert("There should be an error message when Receiving Forwarder is null", result.IsLeft);
			AssertEquals("When Receiving Forwarder has not valid IL VAT", expectedMessage, result.Left);
			receivingForwarder.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("VAT", "400006", "IL");
			result = supporter.GetAdditionalData(shipment, menuItem);
			Assert("There should not be an error message when Receiving Forwarder has valid IL VAT", !result.IsLeft && result.IsRight);
		}

		public void TestILDeliveryOrder_IsImportBrokerVATValid()
		{
			const string expectedMessage = "Import Broker VAT # is not configured";
			var menuItem = Factory.Load<DocumentCommand>(ShipmentSystemFormMenuItems.SendDeliveryOrderPK);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Israel;
			shipment.JS_TransportMode = "ROA";
			var consol = shipment.Consols.AddNew();
			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			receivingForwarder.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("VAT", "400006", "IL");
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);

			var result = supporter.GetAdditionalData(shipment, menuItem);
			Assert("There should be an error message when ImportBroker is null", result.IsLeft);
			AssertEquals("ImportBroker is null", expectedMessage, result.Left);

			var importBroker = Factory.NewWithValidTestData<OrgHeader>();
			shipment.JS_OH_ImportBroker = importBroker.PK;

			result = supporter.GetAdditionalData(shipment, menuItem);
			Assert("There should be an error message when ImportBroker is not valid", result.IsLeft);
			AssertEquals("When ImportBroker is not valid", expectedMessage, result.Left);

			importBroker.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("VAT", "400009", "IL");
			result = supporter.GetAdditionalData(shipment, menuItem);
			Assert("There should not be an error message when ImportBroker is valid", !result.IsLeft && result.IsRight);
		}

		#endregion IL Delivery Order

		#region IL Send GatePass Movement

		public void TestSendGatePassMovement_ValidationOrder()
		{
			const string expectedMessage1st = "Consol Receiving Agent VAT # is not configured for country IL, update Organization – Config tab";
			const string expectedMessage2nd = "Shipment's Container Mode doesn't match, please sent the request from the related Consol";
			const string expectedMessage3rd = "You must update the Transit Warehouse for sending Gatepass Movement request";
			const string expectedMessage4th = "Customs Controlled Premises Code (CCP) is not configured for country IL, update Organization – Config tab";

			var factory = Factory;
			var menuItem = factory.Load<DocumentCommand>(ShipmentSystemFormMenuItems.SendGatePassMovementPK);
			AssertNotNull("The menu and the Excel should be loaded in DB", menuItem);

			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Israel;
			shipment.JS_TransportMode = "ROA";
			shipment.JS_PackingMode = "LCL";

			var warehouse = GetValidTransitWarehouse();
			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);

			var arrivalConsol = shipment.Consols.AddNew();
			var receivingForwarder = factory.NewWithValidTestData<OrgHeader>();
			arrivalConsol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			arrivalConsol.JK_ConsolMode = "GRP";

			var result = supporter.GetAdditionalData(shipment, menuItem);
			Assert("There should be an error message when the Receiving agent is not valid", result.IsLeft);
			AssertEquals("When the Receiving agent is not valid, the first message", expectedMessage1st, result.Left);

			receivingForwarder.CustomsCodes.AddNew("VAT", "400006", "IL");
			arrivalConsol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			result = supporter.GetAdditionalData(shipment, menuItem);
			Assert("There should be an error message when the shipment is LCL and the ArrivalConsol Container Mode is Groupage", result.IsLeft);
			AssertEquals("When the shipment is LCL and the ArrivalConsol Container Mode is Groupage, the second message", expectedMessage2nd, result.Left);

			shipment.JS_PackingMode = "FCL";
			arrivalConsol.JK_ConsolMode = "GRP";
			result = supporter.GetAdditionalData(shipment, menuItem);
			Assert("There should be an error message when the shipment is not LCL, but the ArrivalConsol Container Mode is Groupage", result.IsLeft);
			AssertEquals("When the shipment is not LCL, but the ArrivalConsol Container Mode is Groupage, the second message", expectedMessage2nd, result.Left);

			shipment.JS_PackingMode = "LCL";
			arrivalConsol.JK_ConsolMode = "FCL";
			result = supporter.GetAdditionalData(shipment, menuItem);
			Assert("There should be an error message when the ArrivalConsol Container Mode is not Groupage, but the shipment is LCL", result.IsLeft);
			AssertEquals("When the ArrivalConsol Container Mode is not Groupage, but the shipment is LCL, the second message", expectedMessage2nd, result.Left);

			shipment.JS_PackingMode = "FCL";
			result = supporter.GetAdditionalData(shipment, menuItem);
			Assert("There should be an error message when the Transit Warehouse CFS is null", result.IsLeft);
			AssertEquals("When the Transit Warehouse CFS is null, the third message", expectedMessage3rd, result.Left);

			shipment.JS_OA_ImportReleaseDepot = warehouse.PK;
			result = supporter.GetAdditionalData(shipment, menuItem);
			Assert("When all validations pass", !result.IsLeft && result.IsRight);

			warehouse.Header.CustomsCodes.RemoveAndDeleteAll();
			result = supporter.GetAdditionalData(shipment, menuItem);
			Assert("There should be an error message when the Transit Warehouse does not have a valid IL CCP", result.IsLeft);
			AssertEquals("When the Transit Warehouse does not have a valid IL CCP, the fourth message", expectedMessage4th, result.Left);
		}

		public void TestShouldUseDraftWatermark()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.IsEditingElectronicBOL = true;

			var supporter = new ForwardingShipmentVisualizableDocumentSupporter(shipment);
			var document = new Mock<IDocument>();
			document.SetupGet(d => d.DataContext).Returns(DataContext.HouseBill);

			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				AssertEquals(true, supporter.ShouldUseDraftWatermark(document.Object));
			}
		}

		OrgAddress GetValidTransitWarehouse()
		{
			var warehouse = Factory.New<OrgHeader>();
			warehouse.CustomsCodes.AddNew("CCP", "815444", "IL");

			var warehouseAddress = Factory.New<OrgAddress>();
			warehouseAddress.OA_Address1 = "TEST 2 ADDRESS";
			warehouseAddress.OA_RN_NKCountryCode = "AU";
			warehouseAddress.OA_OH = warehouse.PK;

			return warehouseAddress;
		}
		#endregion IL Delivery Order

		#region TestGetMessageBroker

		public void TestGetMessageBroker()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			Assert(!shipment.IsEditingElectronicBOL);

			var supporter = shipment.GetSupporter();
			AssertNotNull(supporter);
			AssertEquals(EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, supporter.GetMessageBroker());

			shipment.IsEditingElectronicBOL = true;
			AssertEquals(EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface, supporter.GetMessageBroker());
		}

		#endregion
	}
}
