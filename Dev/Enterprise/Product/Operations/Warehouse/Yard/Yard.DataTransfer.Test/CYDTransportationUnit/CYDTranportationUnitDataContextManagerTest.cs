using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal.Test
{
	[TestedType(typeof(CYDTransportationUnitDataContextManager))]
	public class CYDTransportationUnitDataContextManagerTest : ShipmentDataContextManagerTestCase<CYDTransportationUnitDataContextManager, CYDTransportationUnit>
	{
		public void TestMatchingByDataContextKey()
		{
			var transportationUnit = Factory.NewWithValidTestData<CYDTransportationUnit>();
			transportationUnit.YTU_TransportationUnitID = "TRN00001";
			transportationUnit.YTU_TransportationReference = "DEF-023";

			Factory.SaveForTesting();

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataTarget(DataContextType.CYDTransportationUnit, "TRN00001");
			shipment.DataContext.AddDataSource(DataContextType.GateBooking, "GBK00001");
			shipment.DataContext.DataProviderForCodeMapping = GlbCompany.CurrentCompany.OrgProxy.OH_Code;

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress {
					AddressType = nameof(DocAddressType.LocalCartageYard),
					OrganizationCode = "WUFSHIJNB"
				},
				new OrganizationAddress {
					AddressType = nameof(DocAddressType.TransportCompanyDocumentaryAddress),
					OrganizationCode = "WUFSHIJNB"
				},
			});

			shipment.SetPreCarriageShipmentCollection(() => new List<Shipment>
			{
				new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					VehicleRun = new VehicleRun
					{
						Vehicle = new Vehicle
						{
							Registration = new Registration
							{
								Number = "REG1"
							}
						}
					}
				}
			});

			var subShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.SetDateCollection(() => new List<Date>
			{
				new ()
				{
					Type = DateType.Start,
					Value = ZDateTime.Today,
				}
			});
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>
			{
				subShipment
			});

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var message = GetQueuedUniversalShipmentMessage(shipment);
			manager.Process(message);

			AssertEquals("Expected message.EM_Status to be processed OK", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			AssertMultilineASCIIEquals("Expected serviceTaskLog to contain the following message: ", @"
Updated Transportation Unit TRN00001 from UniversalShipment.
Successfully saved Transportation Unit TRN00001.
".Trim(), serviceTaskLog.ToString());
		}

		public void TestDataObjectReader_CYDGateBookingDLV()
		{
			var gateBookingXml = ResourceRetriever.Value.GetString("Enterprise.Warehouse.Yard.DataTransfer.Test.TestFiles.CYDGateBookingDLV.xml");
			var message = GetQueuedUniversalShipmentMessage(gateBookingXml);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			manager.Process(message);
			AssertEquals("Expected message.EM_Status to be processed OK", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertMultilineASCIIEquals("Expected serviceTaskLog to contain the following message: ", @"
Added CYDDelivery from UniversalShipment.
Added Transportation Unit TPU00000001 from UniversalShipment.
Successfully saved Transportation Unit TPU00000001 with 1 x CYDDelivery.
".Trim(), serviceTaskLog.ToString());

			var businessObjectFactory = new BusinessObjectFactory();
			var transportationUnits = businessObjectFactory.Load<CYDTransportationUnit>(new ZQuery());
			Assert("Transportation reference should be populated.", transportationUnits.Any(u => u.YTU_TransportationReference.Equals("DEF-023")));

			serviceTaskLog.ClearLogs();
			manager.Process(message);
			AssertEquals("Expected message.EM_Status to be processed OK", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertMultilineASCIIEquals("Expected serviceTaskLog to contain the following message: ", @"
Updated CYDDelivery from UniversalShipment.
Updated Transportation Unit TPU00000001 from UniversalShipment.
Successfully saved Transportation Unit TPU00000001 with 1 x CYDDelivery.
".Trim(), serviceTaskLog.ToString());
		}

		public void TestDataObjectReader_CYDGateBookingDLV_BlindDropOff()
		{
			var gateBookingXml = ResourceRetriever.Value.GetString("Enterprise.Warehouse.Yard.DataTransfer.Test.TestFiles.CYDGateBookingDLV_BlindDropOff.xml");
			var message = GetQueuedUniversalShipmentMessage(gateBookingXml);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			manager.Process(message);
			AssertEquals("Expected message.EM_Status to be processed OK", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertMultilineASCIIEquals("Expected serviceTaskLog to contain the following message: ", @"
Added CYDDelivery from UniversalShipment.
Added Transportation Unit TPU00000001 from UniversalShipment.
Successfully saved Transportation Unit TPU00000001 with 1 x CYDDelivery.
".Trim(), serviceTaskLog.ToString());

			var businessObjectFactory = new BusinessObjectFactory();
			var transportationUnits = businessObjectFactory.Load<CYDTransportationUnit>(new ZQuery());
			Assert("Transportation reference should be populated.", transportationUnits.Any(u => u.YTU_TransportationReference.Equals("DEF-023")));

			serviceTaskLog.ClearLogs();
			manager.Process(message);
			AssertEquals("Expected message.EM_Status to be processed OK", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertContainsExactElementsInExactOrder("Expected serviceTaskLog to contain the following message: ",
				new[]
				{
					"Updated CYDDelivery from UniversalShipment.",
					"Updated Transportation Unit TPU00000001 from UniversalShipment.",
					"Successfully saved Transportation Unit TPU00000001 with 1 x CYDDelivery."
				}, serviceTaskLog.Logs.Select(c => c.Message));
		}

		public void TestDataObjectReader_CYDGateBookingPIC()
		{
			var gateBookingXml = ResourceRetriever.Value.GetString("Enterprise.Warehouse.Yard.DataTransfer.Test.TestFiles.CYDGateBookingPIC.xml");
			var message = GetQueuedUniversalShipmentMessage(gateBookingXml);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			manager.Process(message);
			AssertEquals("Expected message.EM_Status to be processed OK", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertMultilineASCIIEquals("Expected serviceTaskLog to contain the following message: ", @"
Added CYDPickup from UniversalShipment.
Added Transportation Unit TPU00000001 from UniversalShipment.
Successfully saved Transportation Unit TPU00000001 with 1 x CYDPickup.
".Trim(), serviceTaskLog.ToString());

			var businessObjectFactory = new BusinessObjectFactory();
			var transportationUnits = businessObjectFactory.Load<CYDTransportationUnit>(new ZQuery());
			Assert("Transportation reference should be populated.", transportationUnits.Any(u => u.YTU_TransportationReference.Equals("DEF-023")));

			serviceTaskLog.ClearLogs();
			manager.Process(message);
			AssertEquals("Expected message.EM_Status to be ProcessedOK", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertMultilineASCIIEquals("Expected serviceTaskLog to contain the following message: ", @"
Updated CYDPickup from UniversalShipment.
Updated Transportation Unit TPU00000001 from UniversalShipment.
Successfully saved Transportation Unit TPU00000001 with 1 x CYDPickup.
".Trim(), serviceTaskLog.ToString());
		}

		public void TestDataObjectReader_CYDGateBookingPIC_WithIncorrectContainerCode_WhileTransportationUnitExists()
		{
			TestCaseGateBookingWithIncorrectContainerCode(true);
		}

		public void TestDataObjectReader_CYDGateBookingPIC_WithIncorrectContainerCode_WhileTransportationUnitDoesNotExist()
		{
			TestCaseGateBookingWithIncorrectContainerCode(false);
		}

		void TestCaseGateBookingWithIncorrectContainerCode(bool withTransportationUnit)
		{
			var gateBookingXml = ResourceRetriever.Value.GetString("Enterprise.Warehouse.Yard.DataTransfer.Test.TestFiles.CYDGateBookingPIC_WithIncorrectContainerCode.xml");
			var message = GetQueuedUniversalShipmentMessage(gateBookingXml);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			if (withTransportationUnit)
			{
				var transportationUnit = GetNewBusinessObjectForTesting();
				transportationUnit.YTU_WW_Yard = Data.Yard.PK;
				transportationUnit.YTU_EstimatedGateInTime = new DateTime(2022, 05, 01, 8, 0, 0);
				var transportProvider = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "WUFSHIJNB"));
				UniversalTestHelper.CreateJobDocAddress(Factory, transportProvider.Addresses[0], AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportationUnit);
				Factory.SaveForTesting();
			}

			manager.Process(message);
			var logArray = serviceTaskLog.Logs.ToArray();
			AssertEquals("Expected message.EM_Status to be Discarded", EDIMessageStatusList.Codes.Discarded,
				message.EM_Status);
			AssertEquals($"Release Advice Line (BKR01 / 22G0) is not found or has 0 balance quantity.", logArray[0].Message);
		}

		public void TestDataObjectManager_GetLinkedEntityDLVIntegration()
		{
			var gateBookingXml = ResourceRetriever.Value.GetString("Enterprise.Warehouse.Yard.DataTransfer.Test.TestFiles.CYDGateBookingDLVIntegration.xml");
			var message = GetQueuedUniversalShipmentMessage(gateBookingXml);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var maanger = new UniversalMessageProcessingManager(serviceTaskLog);

			maanger.Process(message);
			AssertEquals("Expected message.EM_Status to be processed OK", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			var check = serviceTaskLog.ToString();
			AssertMultilineASCIIEquals("Expected serviceTaskLog to contain the following message: ", @"
Added Gate Movement Booking from UniversalShipment.
Added GteVehicleMovementBooking from UniversalShipment.
Added GteVehicleDriverBooking from UniversalShipment.
Added CYDDelivery from UniversalShipment.
Added Transportation Unit TPU00000001 from UniversalShipment.
Added Gate Booking from UniversalShipment.
Successfully saved Gate Booking GB00001000 with 1 x GteGateMovementBooking, 1 x GteVehicleMovementBooking, 1 x GteVehicleDriverBooking, 1 x CYDDelivery, 1 x CYDTransportationUnit.
".Trim(), serviceTaskLog.ToString());

			var businessObjectFactory = new BusinessObjectFactory();
			var transportationUnits = businessObjectFactory.Load<CYDTransportationUnit>(new ZQuery());
			Assert("Transportation reference should be populated.", transportationUnits.Any(u => u.YTU_TransportationReference.Equals("DEF-023")));

			var gateMovementBooking = businessObjectFactory.Load<GteGateMovementBooking>(new ZQuery()).FirstOrDefault();
			AssertNotNull("A gate job should have been created", gateMovementBooking);

			var delivery = businessObjectFactory.LoadTop1<CYDDelivery>(new ZQuery());
			AssertNotNull("A delivery job should have been created", delivery);

			AssertEquals("The gate and delivery jobs should be linked", delivery.PK, gateMovementBooking.GBM_FacilityJobId);
		}

		public void TestDataObjectManager_GetLinkedEntityPICIntegration()
		{
			var gateBookingXml = ResourceRetriever.Value.GetString("Enterprise.Warehouse.Yard.DataTransfer.Test.TestFiles.CYDGateBookingPICIntegration.xml");
			var message = GetQueuedUniversalShipmentMessage(gateBookingXml);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var maanger = new UniversalMessageProcessingManager(serviceTaskLog);

			maanger.Process(message);
			AssertEquals("Expected message.EM_Status to be processed OK", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			var check = serviceTaskLog.ToString();
			AssertMultilineASCIIEquals("Expected serviceTaskLog to contain the following message: ", @"
Added Gate Movement Booking from UniversalShipment.
Added GteVehicleMovementBooking from UniversalShipment.
Added GteVehicleDriverBooking from UniversalShipment.
Added CYDPickup from UniversalShipment.
Added Transportation Unit TPU00000001 from UniversalShipment.
Added Gate Booking from UniversalShipment.
Successfully saved Gate Booking GB00001000 with 1 x GteGateMovementBooking, 1 x GteVehicleMovementBooking, 1 x GteVehicleDriverBooking, 1 x CYDPickup, 1 x CYDTransportationUnit.
".Trim(), serviceTaskLog.ToString());

			var businessObjectFactory = new BusinessObjectFactory();
			var transportationUnits = businessObjectFactory.Load<CYDTransportationUnit>(new ZQuery());
			Assert("Transportation reference should be populated.", transportationUnits.Any(u => u.YTU_TransportationReference.Equals("DEF-023")));

			var gateMovementBooking = businessObjectFactory.Load<GteGateMovementBooking>(new ZQuery()).FirstOrDefault();
			AssertNotNull("A gate job should have been created", gateMovementBooking);

			var pickup = businessObjectFactory.LoadTop1<CYDPickup>(new ZQuery());
			AssertNotNull("A pickup job should have been created", pickup);

			AssertEquals("The gate and delivery jobs should be linked", pickup.PK, gateMovementBooking.GBM_FacilityJobId);
		}

		public void TestDataObjectReader_CYDGateVehicleMovementIntegration()
		{					
			var gateVehicleMovementXml = ResourceRetriever.Value.GetString("Enterprise.Warehouse.Yard.DataTransfer.Test.TestFiles.CYDGateVehicleMovementIntegration.xml");
			var message = GetQueuedUniversalShipmentMessage(gateVehicleMovementXml);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			manager.Process(message);
			AssertEquals("Expected message.EM_Status to be processed OK", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertMultilineASCIIEquals("Expected serviceTaskLog to contain the following message: ", @"
Added CYDPickup from UniversalShipment.
Added CYDDelivery from UniversalShipment.
Added Transportation Unit TPU00000001 from UniversalShipment.
Successfully saved Transportation Unit TPU00000001 with 1 x CYDPickup, 1 x CYDDelivery.
".Trim(), serviceTaskLog.ToString());

			var businessObjectFactory = new BusinessObjectFactory();
			var transportationUnits = businessObjectFactory.Load<CYDTransportationUnit>(new ZQuery());
			Assert("Transportation reference should be populated.", transportationUnits.Any(u => u.YTU_TransportationReference.Equals("DEF-023")));
		}

		public void TestReadFromDataObject_DataSourceIsNotFound()
		{
			var xmlNoDataSource = ResourceRetriever.Value.GetString("Enterprise.Warehouse.Yard.DataTransfer.Test.TestFiles.CYDTransportationUnitNoDataSource.xml");
			var message = GetQueuedUniversalShipmentMessage(xmlNoDataSource);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var logger = manager.Process(message);
			var logs = logger.Logs.ToArray();

			AssertEquals("Expected message.EM_Status is to be Discarded when no matching data source.", EDIMessageStatusList.Codes.Discarded, message.EM_Status);
			AssertEquals("Should display 3 lines of messages.",3, logs.Length);
			AssertEquals("Message type should be set to Warning instead of Error.", LogType.Warning, logs[0].Type);
			AssertContains("Should contain 'No matching data source found from UXML.'", "No matching data source found from UXML.", logs[0].Message);
		}

		public void TestDataObjectReader_GateBooking_WithoutDataTarget_ShouldCreateDataTargetBasedOnRecipientRole()
		{
			var gateBookingXml = ResourceRetriever.Value.GetString("Enterprise.Warehouse.Yard.DataTransfer.Test.TestFiles.CYDGateBooking_WithoutDataTarget.xml");

			TestCaseImportMessageWithoutDataTarget(gateBookingXml);
		}

		public void TestDataObjectReader_VehicleMovement_WithoutDataTarget_ShouldCreateDataTargetBasedOnRecipientRole()
		{
			var gateBookingXml = ResourceRetriever.Value.GetString("Enterprise.Warehouse.Yard.DataTransfer.Test.TestFiles.CYDVehicleMovement_WithoutDataTarget.xml");

			TestCaseImportMessageWithoutDataTarget(gateBookingXml);
		}

		void TestCaseImportMessageWithoutDataTarget(string universalShipmentXml)
		{
			var message = GetQueuedUniversalShipmentMessage(universalShipmentXml);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			manager.Process(message);
			AssertEquals("Expected message.EM_Status to be processed OK", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
		}

		#region Reverse GateIn Cases

		public void TestOnUniversalEventAddedCore_SuccessfulReverse()
		{
			var tpu = GetGatedInTransportationUnitForTesting();
			Factory.SaveForTesting();
			UniversalTestHelper.CreateStmUniversalJobLink(new UniversalObjectFactory(), tpu.PK, "TPU123", nameof(DataContextType.GateVehicleMovement));

			var logger = ProcessCNCEvent_ReverseGateIn();
			var logs = logger.Logs.ToArray();

			AssertEquals(LogType.Information, logs[0].Type);
			AssertContains($"Linked Event to Transportation Unit {tpu.YTU_TransportationUnitID}.", logs[0].Message);
			AssertEquals("GateInTime field should be cleared when reversing gate in.", ZDateTimeOffset.Empty, tpu.YTU_GateInTime);
			AssertEquals("WaitingBayLocation field should be cleared when reversing gate in.", ZGuid.Empty, tpu.YTU_WL_WaitingBayLocation);
		}

		public void TestOnUniversalEventAddedCore_FailedReverse_AsPickupHasBeenLoaded()
		{
			var tpu = GetGatedInTransportationUnitForTesting();
			var pickup = Factory.NewWithValidTestData<CYDPickup>();
			pickup.YPL_YTU_PickupTransportationUnit = tpu.PK;
			pickup.YPL_GS_NKLoadUser = "TST";
			Factory.SaveForTesting();
			UniversalTestHelper.CreateStmUniversalJobLink(new UniversalObjectFactory(), tpu.PK, "TPU123", nameof(DataContextType.GateVehicleMovement));

			var logger = ProcessCNCEvent_ReverseGateIn();
			var logs = logger.Logs.ToArray();
			
			AssertEquals("Expected message.EM_Status is to be Discarded.", EDIMessageStatusList.Codes.Discarded, logger.SourceMessage.EM_Status);
			AssertEquals("Message type should be set to Error.", LogType.Error, logs[0].Type);
			AssertContains("This record cannot be reversed as there are attached units that are either currently being handled, or have been processed.", logs[0].Message);
			AssertNotEquals("GateInTime field shouldn't be cleared if the reverse fails.", ZDateTimeOffset.Empty, tpu.YTU_GateInTime);
			AssertNotEquals("WaitingBayLocation field shouldn't be cleared if the reverse fails.", ZGuid.Empty, tpu.YTU_WL_WaitingBayLocation);
		}

		public void TestOnUniversalEventAddedCore_FailedReverse_AsDeliveryHasBeenUnloaded()
		{
			var tpu = GetGatedInTransportationUnitForTesting();
			var delivery = Factory.NewWithValidTestData<CYDDelivery>();
			delivery.YDL_YTU_DeliveryTransportationUnit = tpu.PK;
			delivery.YDL_GS_NKUnloadUser = "TST";
			Factory.SaveForTesting();
			UniversalTestHelper.CreateStmUniversalJobLink(new UniversalObjectFactory(), tpu.PK, "TPU123", nameof(DataContextType.GateVehicleMovement));

			var logger = ProcessCNCEvent_ReverseGateIn();
			var logs = logger.Logs.ToArray();

			AssertEquals("Expected message.EM_Status is to be Discarded.", EDIMessageStatusList.Codes.Discarded, logger.SourceMessage.EM_Status);
			AssertEquals("Message type should be set to Error.", LogType.Error, logs[0].Type);
			AssertContains("This record cannot be reversed as there are attached units that are either currently being handled, or have been processed.", logs[0].Message);
			AssertNotEquals("GateInTime field shouldn't be cleared if the reverse fails.", ZDateTimeOffset.Empty, tpu.YTU_GateInTime);
			AssertNotEquals("WaitingBayLocation field shouldn't be cleared if the reverse fails.", ZGuid.Empty, tpu.YTU_WL_WaitingBayLocation);
		}

		public void TestOnUniversalEventAddedCore_FailedReverse_AsTPUHasNotBeenGatedIn()
		{
			var tpu = GetNewBusinessObjectForTesting();
			AssertEquals(ZDateTimeOffset.Empty, tpu.YTU_GateInTime);
			AssertEquals(ZGuid.Empty, tpu.YTU_WL_WaitingBayLocation);

			var delivery = Factory.NewWithValidTestData<CYDDelivery>();
			delivery.YDL_YTU_DeliveryTransportationUnit = tpu.PK;
			delivery.YDL_GS_NKUnloadUser = "TST";
			Factory.SaveForTesting();
			UniversalTestHelper.CreateStmUniversalJobLink(new UniversalObjectFactory(), tpu.PK, "TPU123", nameof(DataContextType.GateVehicleMovement));

			var logger = ProcessCNCEvent_ReverseGateIn();
			var logs = logger.Logs.ToArray();

			AssertEquals("Expected message.EM_Status is to be Discarded.", EDIMessageStatusList.Codes.Discarded, logger.SourceMessage.EM_Status);
			AssertEquals("Message type should be set to Error.", LogType.Error, logs[0].Type);
			AssertContains("TPU has not been gated in.", logs[0].Message);
		}

		#endregion

		#region Helpers

		protected CYDTransportationUnit GetGatedInTransportationUnitForTesting()
		{
			var area = Factory.NewWithValidTestData<WhsArea>();
			var waitingBayLocation = Factory.NewWithValidTestData<WhsLocation>();
			waitingBayLocation.WLV_WA_PutawayArea = area.PK;
			var tpu = GetNewBusinessObjectForTesting();
			tpu.YTU_GateInTime = UniversalTestHelper.TrimSeconds(DateTimeOffset.Now);
			tpu.YTU_WL_WaitingBayLocation = waitingBayLocation.PK;
			return tpu;
		}

		protected IXmlSessionTracker ProcessCNCEvent_ReverseGateIn()
		{
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			var xml = UniversalTestHelper.BuildEventXMLWithEventReference(AutoEvents.CancelledCode, nameof(DataContextType.GateVehicleMovement), "TPU123", "|EVT=GIN|OFF=011:00|RES=Test 1|RFN=DEF-023|STF=AAA|TYP=Gate In");
			var message = GetQueuedUniversalEventMessage(xml);

			return manager.Process(message);
		}

		#endregion

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => [RecipientRoleType.CYD];
		Lazy<EmbeddedResourceRetriever> ResourceRetriever;
		UniversalTestData Data;

		protected override void SetUp()
		{
			base.SetUp();
			ResourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
			Data = new UniversalTestData(Factory, new TestErrorLogger());
			var today = ZDateTime.Today.Date;
			var tomorrow = ZDateTime.Today.AddDays(1).Date;
			Data.SetupDataForPickup("BKR01", "BKR01", today, tomorrow, "20GP", new string[] { "GENL" });
		}

		protected override CYDTransportationUnit GetNewBusinessObjectForTesting()
		{
			var transportationUnit = Factory.NewWithValidTestData<CYDTransportationUnit>();
			transportationUnit.YTU_TransportationReference = "DEF-023";
			return transportationUnit;
		}

		protected override string ValidPopulatedUniversalShipmentXML => ResourceRetriever.Value.GetString("Enterprise.Warehouse.Yard.DataTransfer.Test.TestFiles.CYDGateBookingDLV.xml");
	}
}
