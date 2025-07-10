using System;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using Types = Enterprise.Core.Constants.EventReferenceParameterTypes;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(ConsolTrackingUpdater))]
	class ConsolTrackingUpdaterTest : ContainerAutomationSubscriptionUpdaterTest<ConsolTrackingUpdater>
	{
		public void TestProcessLogs_UserDepartmentAndBranchAreSerialized_Consol()
		{
			ProcessLogs_UserDepartmentAndBranchAreSerialized(() => CreateConsol());
		}

		public void TestProcessLogs_TransportLegsAreSerialized_Consol()
		{
			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			{
				var transportCollection = CreateConsol().Transports;

				AddOrUpdateSeaTransport(transportCollection);
				AddAirTransport(transportCollection);
				Factory.Save();

				RunLogWalkerCycleForTest();

				var xmlEvent = GetLastMessageAsUniversalEvent();

				CombineAssertions(() =>
				{
					AssertEquals("EventType", AutoEvents.SubscriptionRequested.Code, xmlEvent.EventType);
					var transportLegs = xmlEvent.ContextCollection.Where(x => x.Type == ContainerAutomationEventContextType.TransportLeg).ToArray();
					AssertEquals(2, transportLegs.Length);
					AssertFirstLeg(transportLegs);
					AssertSecondLeg(transportLegs);
				});
			}
		}

		public void TestProcessLogs_TransportLegsWithEmptyValuesAreSerialized_Consol()
		{
			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			{
				var transportCollection = CreateConsol().Transports;

				AddOrUpdateSeaTransportWithEmptyValues(transportCollection);
				Factory.Save();

				RunLogWalkerCycleForTest();

				var xmlEvent = GetLastMessageAsUniversalEvent();

				CombineAssertions(() =>
				{
					AssertEquals("EventType", AutoEvents.SubscriptionRequested.Code, xmlEvent.EventType);
					var transportLegs = xmlEvent.ContextCollection.Where(x => x.Type == ContainerAutomationEventContextType.TransportLeg).ToArray();
					AssertEquals(1, transportLegs.Length);
					AssertFirstEmptyLeg(transportLegs);
				});
			}
		}

		public void TestProcessLogs_ConsolHasLog_ProcessLog()
		{
			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			{
				var consol = CreateConsol();

				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = "AAAA2222220";
				consol.Logs.AddNew(AutoEvents.SubscriptionRequested, Params.Type.AsKeyFor(Types.ContainerTracking));
				consol.JK_UniqueConsignRef = "McLaren";

				Factory.Save();
				RunLogWalkerCycleForTest();

				var message = GetLastMessage();
				AssertNotNull("Message is null", message);

				var document = new XmlDocument();
				document.LoadXml(message.EM_MessageText);

				void AssertContextContent(XmlDocument doc)
				{
					var consolContext = doc.SelectNodes("//*[local-name()='Event']/*[local-name()='ContextCollection']/*[local-name()='Context']");
					AssertEquals("MAWB2345", consolContext[0].ChildNodes[1].InnerText);
					AssertEquals("CCCD", consolContext[1].ChildNodes[1].InnerText);
					AssertEquals("TransportLeg", consolContext[2].ChildNodes[0].InnerText);
					AssertEquals("1", consolContext[2].ChildNodes[1].InnerText);
					AssertEquals("AcceptsOnlyProvidedContainers", consolContext[4].ChildNodes[0].InnerText);
				}

				AssertContextContent(document);

				var containerContext = document.SelectNodes("//*[local-name()='Event']/*[local-name()='AdditionalContextCollection']//*[local-name()='ContextCollection']/*[local-name()='Context']");
				AssertEquals("XML namespace", UniversalXmlInfo.Namespace_2012_11, document.DocumentElement.NamespaceURI);
				AssertEquals(1, containerContext.Count);
				AssertEquals("AAAA2222220", containerContext[0].ChildNodes[1].InnerText);

				AssertMultilineASCIIEquals("Log", @"Processing Consol McLaren (Master Bill='MAWB2345')", GetLogsAsString(Notifier));

				var container2 = consol.Containers.AddNew();
				container2.JC_ContainerNum = "AAAA2232220";
				Factory.Save();
				RunLogWalkerCycleForTest();

				var messages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));
				var message2 = messages.OrderByDescending(m => m.EM_SystemCreateTimeUtc).FirstOrDefault();
				AssertNotNull("Message2 is null", message2);

				var document2 = new XmlDocument();
				document2.LoadXml(message2.EM_MessageText);
				AssertContextContent(document2);

				var containerContext2 = document2.SelectNodes("//*[local-name()='Event']/*[local-name()='AdditionalContextCollection']//*[local-name()='ContextCollection']/*[local-name()='Context']");
				AssertEquals(2, containerContext2.Count);
				AssertEquals("XML namespace", UniversalXmlInfo.Namespace_2012_11, document.DocumentElement.NamespaceURI);
				AssertEquals("AAAA2222220", containerContext2[0].ChildNodes[1].InnerText);
				AssertEquals("AAAA2232220", containerContext2[1].ChildNodes[1].InnerText);
			}
		}

		public void TestProcessLogs_AddCarrierContext()
		{
			// Arrange
			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			{
				var consol = CreateConsol();
				var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
				shippingLine.OH_FullName = "LIANG SHAN CORP";
				consol.JK_OA_ShippingLineAddress = shippingLine.MainAddress.PK;

				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = "AAAA2222220";
				consol.Logs.AddNew(AutoEvents.SubscriptionRequested, Params.Type.AsKeyFor(Types.ContainerTracking));
				consol.JK_UniqueConsignRef = "McLaren";
				Factory.Save();

				// Act
				RunLogWalkerCycleForTest();

				// Assert
				var xmlEvent = GetLastMessageAsUniversalEvent();

				AssertNotNull(
					"Carrier context has been populated from Carrier of consol",
					xmlEvent.ContextCollection.SingleOrDefault(c =>
						c.Type == ContainerAutomationEventContextType.Carrier
						&& c.Value.Value == shippingLine.OH_FullName));
			}
		}

		public void TestProcessLogs_AddCoLoadAttributes()
		{
			// Arrange
			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			{
				var consol = CreateConsolWithFilledCoLoadAttributes();
				Factory.Save();

				// Act
				RunLogWalkerCycleForTest();

				// Assert
				var xmlEvent = GetLastMessageAsUniversalEvent();
				AssessCoLoadAttributes(consol, xmlEvent);
			}
		}

		public void TestProcessLogs_AddCoLoadAttributes_OnlyWhenAgentTypeIsCoLoad()
		{
			// Arrange
			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			{
				var consol = CreateConsol();
				consol.JK_AgentType = Constants.AgentType.Agent;
				consol.JK_CoLoadBookingReference = "Booking123456";
				consol.JK_CoLoadMasterBill = "MaterBill123456";

				var shippingLine = Factory.New<RefShippingLine>();
				shippingLine.RSL_CargoWiseOneCode = "c1bb";
				shippingLine.RSL_StandardCarrierAlphaCode = "1234";
				shippingLine.RSL_CarrierName = "testship";
				shippingLine.RSL_IsNVO = true;
				shippingLine.RSL_IsShippingLine = false;

				var creditor = Factory.NewWithValidTestData<OrgHeader>();
				creditor.OH_Code = "BBG";
				creditor.OH_RSL_ShippingLine = shippingLine.PK;
				creditor.OH_FullName = "Creditor";
				consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
				Factory.Save();

				// Act
				RunLogWalkerCycleForTest();

				// Assert
				var xmlEvent = GetLastMessageAsUniversalEvent();
				AssessNoCoLoadAttributes(xmlEvent);
			}
		}

		public void TestProcessLogs_AssessContainerModeIsSerialized()
		{
			// Arrange
			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			{
				var consol = CreateConsol();
				consol.JK_ConsolMode = Constants.ContainerModes.FCLMixedShipper;
				Factory.Save();

				// Act
				RunLogWalkerCycleForTest();

				// Assert
				var xmlEvent = GetLastMessageAsUniversalEvent();
				AssertEquals(
					"ContainerMode set to ForwardingConsol.JK_ConsolMode for consol",
					consol.JK_ConsolMode,
					xmlEvent.ContextCollection.Single(c => c.Type == ContainerAutomationEventContextType.ContainerMode).Value.Value);
			}
		}

		public void TestProcessingLogs_CoLoad_SendsSubscriptionWithCoLoadData_When_CoLoadMBNIsNotEmpty_And_CoLoadWithIsNotEmpty()
		{
			// Arrange
			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			{
				var consol = CreateConsolWithFilledCoLoadAttributes();
				consol.JK_CoLoadBookingReference = null;

				Factory.Save();

				// Act
				RunLogWalkerCycleForTest();

				// Assert
				var xmlEvent = GetLastMessageAsUniversalEvent();
				AssessCoLoadAttributes(consol, xmlEvent);
			}
		}

		public void TestProcessingLogs_CoLoad_SendsSubscriptionWithCoLoadData_When_CoLoadCBRIsNotEmpty_And_CoLoadWithIsNotEmpty()
		{
			// Arrange
			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			{
				var consol = CreateConsolWithFilledCoLoadAttributes();
				consol.JK_CoLoadMasterBill = null;

				Factory.Save();

				// Act
				RunLogWalkerCycleForTest();

				// Assert
				var xmlEvent = GetLastMessageAsUniversalEvent();
				AssessCoLoadAttributes(consol, xmlEvent);
			}
		}

		public void TestProcessingLogs_CoLoad_SendsSubscriptionWithMainData_When_CoLoadMBNIsNotEmpty_And_CoLoadCBRIsNotEmpty_And_CoLoadWithIsEmpty()
		{
			// Arrange
			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			{
				var consol = CreateConsolWithFilledCoLoadAttributes();
				consol.JK_OA_CreditorAddress = ZGuid.Empty;

				Factory.Save();

				// Act
				RunLogWalkerCycleForTest();

				// Assert
				var xmlEvent = GetLastMessageAsUniversalEvent();
				AssessNoCoLoadAttributes(xmlEvent);
			}
		}

		public void TestProcessingLogs_CoLoad_SendsSubscriptionWithMainData_When_CoLoadMBNIsEmpty_And_CoLoadCBRIsEmpty_And_CoLoadWithIsNotEmpty()
		{
			// Arrange
			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			{
				var consol = CreateConsolWithFilledCoLoadAttributes();
				consol.JK_CoLoadMasterBill = null;
				consol.JK_CoLoadBookingReference = null;

				Factory.Save();

				// Act
				RunLogWalkerCycleForTest();

				// Assert
				var xmlEvent = GetLastMessageAsUniversalEvent();
				AssessNoCoLoadAttributes(xmlEvent);
			}
		}

		public void TestProcessingLogs_CoLoad_SendsSubscriptionWithCoLoadData_When_CoLoadDataIsNotEmpty_And_MainCBRIsEmpty_And_MainMBNIsEmpty()
		{
			// Arrange
			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			{
				var consol = CreateConsolWithFilledCoLoadAttributes();
				consol.JK_MasterBillNum = null;
				consol.JK_BookingReference = null;

				Factory.Save();

				// Act
				RunLogWalkerCycleForTest();

				// Assert
				var xmlEvent = GetLastMessageAsUniversalEvent();
				AssessCoLoadAttributes(consol, xmlEvent);
			}
		}

		public void TestProcessingLogs_CoLoad_SendsSubscriptionWithCoLoadData_When_CoLoadDataIsNotEmpty_And_MainCarrierIsEmpty()
		{
			// Arrange
			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			{
				var consol = CreateConsolWithFilledCoLoadAttributes();
				consol.JK_OA_ShippingLineAddress = ZGuid.Empty;

				Factory.Save();

				// Act
				RunLogWalkerCycleForTest();

				// Assert
				var xmlEvent = GetLastMessageAsUniversalEvent();
				AssessCoLoadAttributes(consol, xmlEvent);
			}
		}

		public void TestProcessingLogs_CoLoad_SendsSubscriptionWithCoLoadData_When_CoLoadDataIsNotEmpty()
		{
			// Arrange
			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			{
				var consol = CreateConsolWithFilledCoLoadAttributes();
				var shippingLine = Factory.New<RefShippingLine>();
				shippingLine.RSL_CargoWiseOneCode = "c1bb";
				shippingLine.RSL_StandardCarrierAlphaCode = "1234";
				shippingLine.RSL_CarrierName = "testship";
				shippingLine.RSL_IsNVO = true;
				shippingLine.RSL_IsShippingLine = false;
				var creditor = Factory.NewWithValidTestData<OrgHeader>();
				creditor.OH_Code = "BBG";
				creditor.OH_RSL_ShippingLine = shippingLine.PK;
				consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

				Factory.Save();

				// Act
				RunLogWalkerCycleForTest();

				// Assert
				var xmlEvent = GetLastMessageAsUniversalEvent();
				AssessCoLoadAttributes(consol, xmlEvent);
			}
		}

		public void TestProcessLogs_AddAcceptsOnlyProvidedContainersFlag_False_Direct()
		{
			AssertProcessLogs_AddAcceptsOnlyProvidedContainersFlag_False_Direct(false);
			AssertProcessLogs_AddAcceptsOnlyProvidedContainersFlag_False_Direct(true);

			void AssertProcessLogs_AddAcceptsOnlyProvidedContainersFlag_False_Direct(bool isNeverCreate)
			{
				// Arrange
				var automaticContainerCreation = new AutomaticContainerCreation();
				automaticContainerCreation.IsNeverCreate = isNeverCreate;
				using (SetContainerAutomationEnabled())
				using (SetEHubId())
				using (FreightDataRegistry.Instance.AutomaticContainerCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, automaticContainerCreation))
				{
					var consol = CreateConsol();
					consol.JK_MasterBillNum = "081";
					Factory.Save();

					// Act
					RunLogWalkerCycleForTest();

					// Assert
					var xmlEvent = GetLastMessageAsUniversalEvent();

					AssertEquals(
						"AcceptsOnlyProvidedContainers set to 'False' for direct consol regardless of AutomaticContainerCreation value",
						false.Serialize(),
						xmlEvent.ContextCollection.Single(c => c.Type == ContainerAutomationEventContextType.AcceptsOnlyProvidedContainers).Value.Value);
				}
			}
		}

		public void TestProcessLogs_AddAcceptsOnlyProvidedContainersFlag_True_CoLoad()
		{
			AssertProcessLogs_AddAcceptsOnlyProvidedContainersFlag_True_CoLoad(false);
			AssertProcessLogs_AddAcceptsOnlyProvidedContainersFlag_True_CoLoad(true);

			void AssertProcessLogs_AddAcceptsOnlyProvidedContainersFlag_True_CoLoad(bool isNeverCreate)
			{
				// Arrange
				var automaticContainerCreation = new AutomaticContainerCreation();
				automaticContainerCreation.IsNeverCreate = isNeverCreate;
				using (SetContainerAutomationEnabled())
				using (SetEHubId())
				using (FreightDataRegistry.Instance.AutomaticContainerCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, automaticContainerCreation))
				{
					var consol = CreateConsol();
					consol.JK_AgentType = Constants.AgentType.CoLoad;
					Factory.Save();

					// Act
					RunLogWalkerCycleForTest();

					// Assert
					var xmlEvent = GetLastMessageAsUniversalEvent();

					AssertEquals(
						"AcceptsOnlyProvidedContainers set to 'True' for direct consol regardless of AutomaticContainerCreation value",
						true.Serialize(),
						xmlEvent.ContextCollection.Single(c => c.Type == ContainerAutomationEventContextType.AcceptsOnlyProvidedContainers).Value.Value);
				}
			}
		}

		public void TestProcessingLogs_CoLoad_SendsInvalidContainerNumbers()
		{
			// Arrange
			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			{
				var consol = CreateConsol();
				consol.JK_AgentType = Constants.AgentType.CoLoad;

				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = "IAMNOTVALID";

				Factory.Save();

				// Act
				RunLogWalkerCycleForTest();

				// Assert
				var xmlEvent = GetLastMessageAsUniversalEvent();

				AssertNotNull(
					"Includes invalid container number in additional context",
					xmlEvent.AdditionalContextCollection
						.Single(additionalContext =>
						{
							var forwardingContainerDataSource = additionalContext.DataContext.DataSourceCollection
								.SingleOrDefault(dataSource => dataSource.Type.Value == "ForwardingContainer");
							return forwardingContainerDataSource != null;
						})
						.ContextCollection.SingleOrDefault(context =>
							context.Type == "ContainerNumber"
							&& context.Value.Value == "IAMNOTVALID"));
			}
		}

		public void TestProcessingLogs_DeletedContainerIsNotSentInAdditionalContext()
		{
			// Arrange
			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			{
				var consol = CreateConsol();
				var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
				shippingLine.OH_FullName = "LIANG SHAN CORP";
				consol.JK_OA_ShippingLineAddress = shippingLine.MainAddress.PK;

				var container1 = consol.Containers.AddNew();
				container1.JC_ContainerNum = "AAAA0000007";
				var container2 = consol.Containers.AddNew();
				container2.JC_ContainerNum = "AAAA2222220";
				consol.JK_UniqueConsignRef = "McLaren";
				Factory.Save();

				container2.Delete();
				Factory.Save();

				// Act
				RunLogWalkerCycleForTest();

				// Assert
				var xmlEvent = GetLastMessageAsUniversalEvent();

				AssertNotNull(
					"Container 1 is sent",
					xmlEvent.AdditionalContextCollection.SingleOrDefault(additionalContext =>
					{
						var withContainer1Number = additionalContext.ContextCollection.SingleOrDefault(context =>
							context.Type == "ContainerNumber"
							&& context.Value.Value == container1.JC_ContainerNum);
						return withContainer1Number != null;
					}));
				Assert(
					"Container 2 is not sent",
					!xmlEvent.AdditionalContextCollection.Any(additionalContext =>
						additionalContext.ContextCollection.Any(context =>
							context.Type == "ContainerNumber"
							&& context.Value.Value == "AAAA2222220")));
			}
		}

		public void TestProcessLogs_TransportModeContext()
		{
			// Arrange
			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			{
				var consol = CreateConsol();
				Factory.Save();

				// Act
				RunLogWalkerCycleForTest();

				// Assert
				var xmlEvent = GetLastMessageAsUniversalEvent();
				AssertEquals(
					"TransportMode context is added",
					"SEA",
					xmlEvent.ContextCollection.Single(c => c.Type == ContainerAutomationEventContextType.TransportMode).Value.Value);
			}
		}

		public void TestProcessLogs_LegDestinationTerminalCodeContext_MatchByPremiseAddress()
		{
			// Arrange
			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			{
				var arrivalCTO = Factory.NewWithValidTestData<OrgHeader>();
				arrivalCTO.OH_IsSeaCTO = true;

				var refFacility = Factory.NewWithValidTestData<RefFacility>();
				refFacility.RFT_Code = "00000000001";
				refFacility.RFT_FacilityType = Core.Constants.FacilityType.Code.Terminal;

				var orgRefFacility = arrivalCTO.OrgRefFacilities.AddNew();
				orgRefFacility.OFC_OA_PremisesAddress = arrivalCTO.MainAddress.PK;
				orgRefFacility.OFC_RFT_Facility = refFacility.PK;

				var consol = CreateConsol();
				consol.JK_OA_ArrivalCTOAddress = arrivalCTO.MainAddress.PK;

				Factory.Save();

				// Act
				RunLogWalkerCycleForTest();

				// Assert
				var xmlEvent = GetLastMessageAsUniversalEvent();
				AssertEquals(
					"LegDestinationTerminalCode context is added",
					"00000000001",
					xmlEvent.ContextCollection.Single(c => c.Type == ContainerAutomationEventContextType.TransportLeg).SubContextCollection.Single(c => c.Type == "LegDestinationTerminalCode").Value.Value);
			}
		}

		public void TestProcessLogs_LegDestinationTerminalCodeContext_MatchByLocation()
		{
			// Arrange
			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			{
				var arrivalCTO = Factory.NewWithValidTestData<OrgHeader>();
				arrivalCTO.OH_IsSeaCTO = true;

				var refFacility = Factory.NewWithValidTestData<RefFacility>();
				refFacility.RFT_Code = "00000000001";
				refFacility.RFT_FacilityType = Core.Constants.FacilityType.Code.Terminal;
				refFacility.RFT_RL_NKLocationCode = "BEANR";
				refFacility.RFT_RN_NKCountryCode = "BE";

				var orgRefFacility = arrivalCTO.OrgRefFacilities.AddNew();
				orgRefFacility.OFC_OA_PremisesAddress = ZGuid.Empty;
				orgRefFacility.OFC_RFT_Facility = refFacility.PK;

				var consol = CreateConsol();
				consol.JK_RL_NKDischargePort = "BEANR";
				consol.JK_OA_ArrivalCTOAddress = arrivalCTO.MainAddress.PK;

				Factory.Save();

				// Act
				RunLogWalkerCycleForTest();

				// Assert
				var xmlEvent = GetLastMessageAsUniversalEvent();
				AssertEquals(
					"LegDestinationTerminalCode context is added",
					"00000000001",
					xmlEvent.ContextCollection.Single(c => c.Type == ContainerAutomationEventContextType.TransportLeg).SubContextCollection.Single(c => c.Type == "LegDestinationTerminalCode").Value.Value);
			}
		}

		public void TestProcessLogs_DoNotAddConsolTypeContext()
		{
			// Arrange
			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			{
				var consol = CreateConsol();
				consol.JK_AgentType = Constants.AgentType.Agent;
				Factory.Save();

				// Act
				RunLogWalkerCycleForTest();

				// Assert
				var xmlEvent = GetLastMessageAsUniversalEvent();

				AssertEquals(0, xmlEvent.ContextCollection.Where(c => c.Type == "ConsolType").Count());
			}
		}

		protected override EnterpriseBusinessObject GetBusinessObjectInstance()
		{
			return CreateConsol();
		}

		static void AssessCoLoadAttributes(ForwardingConsol consol, UniversalEvent xmlEvent)
		{
			if (!string.IsNullOrWhiteSpace(consol.JK_CoLoadMasterBill))
			{
				AssertEquals(
					"CoLoadBillNumber set to ForwardingConsol.JK_CoLoadMasterBill for direct consol",
					consol.JK_CoLoadMasterBill,
					xmlEvent.ContextCollection.Single(c => c.Type == ContainerAutomationEventContextType.CoLoadBillNumber).Value.Value);
			}
			if (!string.IsNullOrWhiteSpace(consol.JK_CoLoadBookingReference))
			{
				AssertEquals(
				"CoLoadBookingReference set to ForwardingConsol.JK_CoLoadBookingReference for direct consol",
				consol.JK_CoLoadBookingReference,
				xmlEvent.ContextCollection.Single(c => c.Type == ContainerAutomationEventContextType.CoLoadBookingReference).Value.Value);
			}
			if (consol.Creditor != null && !string.IsNullOrWhiteSpace(consol.Creditor.SCACCode))
			{
				AssertEquals(
				"CoLoadWithCode set to ForwardingConsol.Creditor.SCACCode for direct consol",
				consol.Creditor.SCACCode,
				xmlEvent.ContextCollection.Single(c => c.Type == ContainerAutomationEventContextType.CoLoadWithCode).Value.Value);
			}
			if (consol.Creditor != null && !string.IsNullOrWhiteSpace(consol.Creditor.OH_FullName))
			{
				AssertEquals(
				"CoLoadWithName set to ForwardingConsol.Creditor.OH_Code for direct consol",
				consol.Creditor.OH_FullName,
				xmlEvent.ContextCollection.Single(c => c.Type == ContainerAutomationEventContextType.CoLoadWithName).Value.Value);
			}
			if (consol.Creditor != null && !string.IsNullOrWhiteSpace(consol.Creditor.C1CCode))
			{
				AssertEquals(
				"CoLoadWithC1CCode set to ForwardingConsol.Creditor.C1CCode for direct consol",
				consol.Creditor.C1CCode,
				xmlEvent.ContextCollection.Single(c => c.Type == ContainerAutomationEventContextType.CoLoadWithC1CCode).Value.Value);
			}
		}

		static void AssessNoCoLoadAttributes(UniversalEvent xmlEvent)
		{
			AssertNull(xmlEvent.ContextCollection
				.SingleOrDefault(c => c.Type == ContainerAutomationEventContextType.CoLoadBillNumber));

			AssertNull(
				xmlEvent.ContextCollection
					.SingleOrDefault(c => c.Type == ContainerAutomationEventContextType.CoLoadBookingReference));

			AssertNull(xmlEvent.ContextCollection.SingleOrDefault(c =>
				c.Type == ContainerAutomationEventContextType.CoLoadWithCode));

			AssertNull(
				xmlEvent.ContextCollection.SingleOrDefault(c => c.Type == ContainerAutomationEventContextType.CoLoadWithName));

			AssertNull(
				xmlEvent.ContextCollection.SingleOrDefault(c => c.Type == ContainerAutomationEventContextType.CoLoadWithC1CCode));
		}

		ForwardingConsol CreateConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_OA_ShippingLineAddress = Org.MainAddress.PK;
			consol.MasterBillMAWB = "MAWB2345";
			consol.Transports[0].JW_ETA = ZDateTime.Today;
			return consol;
		}

		ForwardingConsol CreateConsolWithFilledCoLoadAttributes()
		{
			var consol = CreateConsol();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_CoLoadBookingReference = "Booking123456";
			consol.JK_CoLoadMasterBill = "MaterBill123456";
			consol.JK_OA_CreditorAddress = new ZGuid("2bf0030e-27eb-42b9-8936-5a76074e82da");
			return consol;
		}
	}
}
