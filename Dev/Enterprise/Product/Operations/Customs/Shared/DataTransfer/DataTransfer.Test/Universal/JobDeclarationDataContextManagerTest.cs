using System;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.DataTransfer.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	[TestedType(typeof(JobDeclarationDataContextManager))]
	sealed class JobDeclarationDataContextManagerTest : ShipmentDataContextManagerTestCase<JobDeclarationDataContextManager, BaseJobDeclaration>
	{
		public void TestProcessForwardingContainerEventWithoutParameters()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "CDB0298305";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "TCLU3697767";

			Factory.SaveForTesting();

			var eventMappings = new (string eventCode, string facility, string location, ZPropertyInfo propertyInfo)[]
			{
				(Events.FreightLoadedCode, "CTO", "AUSYD", container.JC_FCLOnBoardVesselInfo),
				(Events.FreightUnloadedCode, "CTO", "AUSYD", container.JC_FCLUnloadFromVesselInfo),
				(Events.DehireCode, "CTO", "AUSYD", container.JC_ContainerYardEmptyReturnGateInInfo),
				(Events.GateOutCode, "CTO", "AUSYD", null),
				(Events.GateInCode, "CY", "SGSIN", null),
			};

			AssertProcessContainerEvent(container, "UniversalEventWithContainer.xml", eventMappings);
		}

		public void TestProcessForwardingContainerEventWithParameters()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "CDB0298305";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "TCLU3697767";

			Factory.SaveForTesting();

			var eventMappings = new (string eventCode, string facility, string location, ZPropertyInfo propertyInfo)[]
			{
				(Events.FreightLoadedCode, "CTO", "AUSYD", container.JC_FCLOnBoardVesselInfo),
				(Events.FreightUnloadedCode, "CTO", "SGSIN", container.JC_FCLUnloadFromVesselInfo),
				(Events.DehireCode, "CTO", "AUSYD", container.JC_ContainerYardEmptyReturnGateInInfo),
				(Events.GateOutCode, "CY", "SGSIN", container.JC_ContainerYardEmptyPickupGateOutInfo),
				(Events.GateInCode, "CTO", "AUSYD", container.JC_FCLWharfGateInInfo),
			};

			AssertProcessContainerEvent(container, "UniversalEventWithContainerAndParameters.xml", eventMappings);
		}

		public void TestProcessCustomsContainerEventWithoutParameters()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MasterBill = "CDB0298305";

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "TCLU3697767";

			Factory.SaveForTesting();

			var eventMappings = new (string eventCode, string facility, string location, ZPropertyInfo propertyInfo)[]
			{
				(Events.FreightLoadedCode, "CTO", "AUSYD", container.JobContainer.JC_FCLOnBoardVesselInfo),
				(Events.FreightUnloadedCode, "CTO", "AUSYD", container.JobContainer.JC_FCLUnloadFromVesselInfo),
				(Events.DehireCode, "CTO", "AUSYD", container.JobContainer.JC_ContainerYardEmptyReturnGateInInfo),
				(Events.GateOutCode, "CY", "SGSIN", null),
				(Events.GateInCode, "CTO", "AUSYD", null),
			};

			AssertProcessContainerEvent(container.JobContainer, "UniversalEventWithContainer.xml", eventMappings);
		}

		public void TestProcessCustomsContainerEventWithParameters()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MasterBill = "CDB0298305";
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "SGSIN";

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "TCLU3697767";

			Factory.SaveForTesting();

			var eventMappings = new (string eventCode, string facility, string location, ZPropertyInfo propertyInfo)[]
			{
				(Events.FreightLoadedCode, "CTO", "AUSYD", container.JobContainer.JC_FCLOnBoardVesselInfo),
				(Events.FreightUnloadedCode, "CTO", "SGSIN", container.JobContainer.JC_FCLUnloadFromVesselInfo),
				(Events.DehireCode, "CTO", "AUSYD", container.JobContainer.JC_ContainerYardEmptyReturnGateInInfo),
				(Events.GateOutCode, "CY", "SGSIN", container.JobContainer.JC_ContainerYardEmptyPickupGateOutInfo),
				(Events.GateInCode, "CTO", "AUSYD", container.JobContainer.JC_FCLWharfGateInInfo),
			};

			AssertProcessContainerEvent(container.JobContainer, "UniversalEventWithContainerAndParameters.xml", eventMappings);
		}

		public void TestGetShipmentDataObjectWriter_CountrySpecificWriterHasLowPriorityThanApplicationCodeSpecific()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_ApplicationCode = "EMC";
				var euDataTransferAssembly = AssemblyLoader.LoadAssembly("Enterprise.Customs.EU.EMCS.DataTransfer");
				var typeOfEMCSDeclarationDataObjectWriter = euDataTransferAssembly.GetType("Enterprise.Customs.EU.EMCS.DataTransfer.EMCSDeclarationDataObjectWriter");
				var writerManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration));
				Assert(typeOfEMCSDeclarationDataObjectWriter.IsInstanceOfType(JobDeclarationDataContextManager.GetShipmentDataObjectWriter(writerManager, declaration)));

				declaration.JE_ApplicationCode = ZString.Empty;
				var gbDataTransferAssembly = AssemblyLoader.LoadAssembly("Enterprise.Customs.GB.DataTransfer");
				var typeOfGbDeclarationDataObjectWriter = gbDataTransferAssembly.GetType("Enterprise.Customs.GB.DataTransfer.Universal.DeclarationDataObjectWriter");
				Assert(typeOfGbDeclarationDataObjectWriter.IsInstanceOfType(JobDeclarationDataContextManager.GetShipmentDataObjectWriter(writerManager, declaration)));
			}
		}

		public void TestProcessingDataObjectShouldBeCalledOnceForEachReader()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Interfaced;

			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customsInterface))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var newFactory = new BusinessObjectFactory();
				var consol = newFactory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_RL_NKLoadPort = "HKWNI";
				consol.JK_MasterBillNum = "MB324334";
				consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
				consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
				consol.JK_SendingForwarderHandlingType = "GTT";
				consol.JK_ReceivingForwarderHandlingType = "GTT";

				var shipment = consol.Shipments.AddNew();
				shipment.JS_HouseBill = "HB9863521";
				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_RL_NKDestination = "AUBNE";
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

				var declaration = newFactory.New<BaseJobDeclaration>();
				declaration.JE_JS = shipment.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.ShipmentSynchroniser.Synchronise(true);

				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipment)), true, true);
				var dataObject = writer.GetDataObject(shipment);
				dataObject.DataContext.AddDataTarget(DataContextType.ForwardingConsol, null);
				dataObject.DataContext.AddDataTarget(DataContextType.ForwardingShipment, null);
				var subShipmentDataObject = dataObject.SubShipmentCollection[0];
				subShipmentDataObject.DataContext.AddDataTarget(DataContextType.ForwardingShipment, null);
				subShipmentDataObject.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
				subShipmentDataObject.MessagingApplicationCode.Code = DeclarationApplicationCodeList.Codes.Interfaced;

				var message = GetQueuedUniversalShipmentMessage(dataObject);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
Added Declaration (Master Bill='MB324334' House Bill='HB9863521') from UniversalShipment.
Added Shipment (House Bill='HB9863521') from UniversalShipment.
Added Consol (Master Bill='MB324334') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='MB324334') with 1 x Transport, 2 x Bill, 1 x JobDeclaration, 1 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());

					var logNoteText = message.GetLogNoteText();
					AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
Successfully loaded matching Transport.
Populating Transport...
Transport Leg: Origin: HKWNI Destination: AUSYD
Attempting to get Schedule for the Transport Leg
Unable to link to schedule because the Transport Leg has insufficient information.
Transport Leg updated.
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Successfully loaded matching Transport.
Populating Transport...
Transport Leg: Origin: HKWNI Destination: AUSYD
Attempting to get Schedule for the Transport Leg
Unable to link to schedule because the Transport Leg has insufficient information.
Transport Leg updated.
No matching JobDeclaration found, creating new JobDeclaration.
Populating JobDeclaration...
No matching Bill found, creating new Bill.
Populating Bill...
No matching Bill found, creating new Bill.
Populating Bill...
Added Declaration (Master Bill='MB324334' House Bill='HB9863521') from UniversalShipment.
Added Shipment (House Bill='HB9863521') from UniversalShipment.
Added Consol (Master Bill='MB324334') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='MB324334') with 1 x Transport, 2 x Bill, 1 x JobDeclaration, 1 x ForwardingShipment.
				".Trim(), logNoteText);

					shipment = Factory.LoadFromNaturalKey<ForwardingShipment>(JobShipmentSchema.JS_UniqueConsignRef, "S00001000");
					AssertNotNull("shipment", shipment);
					AssertEquals("JS_HouseBill", "HB9863521", shipment.JS_HouseBill);
					AssertEquals("JS_TransportMode", Core.Constants.TransportModes.Sea, shipment.JS_TransportMode);
					AssertEquals("JS_RL_NKOrigin", "HKHKG", shipment.JS_RL_NKOrigin);
					AssertEquals("JS_RL_NKDestination", "AUBNE", shipment.JS_RL_NKDestination);

					declaration = (BaseJobDeclaration)shipment.DeclarationForDocuments;
					AssertNotNull("declaration", declaration);
					AssertEquals("JE_TransportMode", Core.Constants.TransportModes.Sea, declaration.JE_TransportMode);
					AssertEquals("JE_MasterBill", "MB324334", declaration.JE_MasterBill);
					AssertEquals("JE_HouseBill", "HB9863521", declaration.JE_HouseBill);
					AssertEquals("JE_RL_NKOrigin", "HKHKG", declaration.JE_RL_NKOrigin);
					AssertEquals("JE_RL_NKPortOfLoading", "HKWNI", declaration.JE_RL_NKPortOfLoading);
					AssertEquals("JE_RL_NKPortOfArrival", "AUSYD", declaration.JE_RL_NKPortOfArrival);
					AssertEquals("JE_RL_NKFinalDestination", "AUBNE", declaration.JE_RL_NKFinalDestination);

					AssertEquals("shipment.Consols.Count", 1, shipment.Consols.Count);
					consol = shipment.Consols[0];
					AssertNotNull("consol", consol);
					AssertEquals("JK_MasterBillNum", "MB324334", consol.JK_MasterBillNum);
					AssertEquals("JK_TransportMode", Core.Constants.TransportModes.Sea, consol.JK_TransportMode);
					AssertEquals("JK_RL_NKLoadPort", "HKWNI", consol.JK_RL_NKLoadPort);
					AssertEquals("JK_RL_NKDischargePort", "AUSYD", consol.JK_RL_NKDischargePort);
				});
			}
		}

		public void TestGetDataContextKeyMatchingQueryUseCurrentCompany()
		{
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			var otherBranch = Factory.NewWithValidTestData<GlbBranch>();
			otherBranch.GB_GC = otherCompany.PK;

			var declarationInOtherCompany = Factory.New<BaseJobDeclaration>();
			declarationInOtherCompany.JE_DeclarationReference = "B00001003";
			declarationInOtherCompany.JE_HouseBill = "HOUSE_BILL_2";
			declarationInOtherCompany.JE_GB = otherBranch.PK;

			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(otherCompany);
			dataContext.CodesMappedToTarget = false;
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, "B00001003");
			var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				Branch = new Branch { Code = "B99" },
				WayBillNumber = "NEWHOUSE232",
				WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House },
			};
			var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
			message.EM_MessageText = message.EM_MessageText.Replace("				      <DataProvider>EDIDATDAN</DataProvider>", "").Replace("      <EnterpriseID>EDI</EnterpriseID>", "");
			Factory.SaveForTesting();
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertContains("ERROR - Match couldn't be found for CustomsDeclaration with Key B00001003", serviceTaskLog.ToString());
			declarationInOtherCompany.Reload();
			AssertEquals("declarationInOtherCompany.JE_HouseBill", "HOUSE_BILL_2", declarationInOtherCompany.JE_HouseBill);
			AssertEquals("declarationInOtherCompany.JE_GB", otherBranch.PK, declarationInOtherCompany.JE_GB);
		}

		public void TestGetDataContextKeyMatchingQueryFromCountryCode()
		{
			#region Company, Branch

			var company5 = Factory.NewWithValidTestData<GlbCompany>();
			company5.GC_Code = "FJC";
			var branch5 = Factory.NewWithValidTestData<GlbBranch>();
			branch5.GB_Code = "FJB";
			branch5.GB_GC = company5.PK;

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "CP1";
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_Code = "BR1";
			branch1.GB_GC = company1.PK;

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "CP2";
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_Code = "BR2";
			branch1.GB_GC = company2.PK;

			var company3 = Factory.NewWithValidTestData<GlbCompany>();
			company3.GC_Code = "CP3";
			var branch3 = Factory.NewWithValidTestData<GlbBranch>();
			branch3.GB_Code = "BR3";
			branch3.GB_GC = company3.PK;

			var company4 = Factory.NewWithValidTestData<GlbCompany>();
			company4.GC_Code = "CP4";
			company4.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Fiji;
			var branch4 = Factory.NewWithValidTestData<GlbBranch>();
			branch4.GB_Code = "BR4";
			branch4.GB_GC = company4.PK;

			#endregion

			#region Create JobDeclaration

			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration2.JE_DeclarationReference = "B00001024";
			declaration2.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration2.JE_VesselName = "AALSMEERGRACHT";
			declaration2.JE_VoyageFlightNo = "919P";
			declaration2.JE_RL_NKPortOfArrival = "USPHL";
			declaration2.JE_GB = branch1.PK;

			var declaration3 = Factory.New<BaseJobDeclaration>();
			declaration3.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration3.JE_DeclarationReference = "B00001024";
			declaration3.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration3.JE_VesselName = "AULSMEER";
			declaration3.JE_VoyageFlightNo = "920P";
			declaration3.JE_RL_NKPortOfArrival = "AUSYD";
			declaration3.JE_GB = branch3.PK;

			var declaration1 = Factory.New<BaseJobDeclaration>();
			declaration1.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration1.JE_DeclarationReference = "B00001021";
			declaration1.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration1.JE_VesselName = "AALSMEERGRAC";
			declaration1.JE_VoyageFlightNo = "900P";
			declaration1.JE_RL_NKPortOfArrival = "USPHL";
			declaration1.JE_GB = branch1.PK;

			var declaration4 = Factory.New<BaseJobDeclaration>();
			declaration4.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration4.JE_DeclarationReference = "B00001021";
			declaration4.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration4.JE_VesselName = "AULSMEER";
			declaration4.JE_VoyageFlightNo = "920P";
			declaration4.JE_RL_NKPortOfArrival = "AUSYD";
			declaration4.JE_GB = branch4.PK;
			Factory.SaveForTesting();

			#endregion

			var message = GetQueuedUniversalEventMessage(IncomingEventForDFC); // B00001024
			message.EM_GB = branch2.PK;
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var sessionTracker = manager.Process(message);
			var attemptedImports = sessionTracker.ImportResults;
			AssertMultilineASCIIEquals("rejected", @"False|Warning - No Module found a Business Entity to link this Universal Event to.|NULL", attemptedImports.FormatAndOrderImportAttempts());

			message = GetQueuedUniversalEventMessage(IncomingEventForDFC2);  // B00001021
			message.EM_GB = branch2.PK;
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			sessionTracker = manager.Process(message);
			attemptedImports = sessionTracker.ImportResults;

			AssertMultilineASCIIEquals("ProcessedOK", @"True|Linked Event to Declaration B00001021.|CustomsDeclaration-B00001021", attemptedImports.FormatAndOrderImportAttempts());
			AssertEquals(1, declaration4.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "DCF")).Length);
		}

		public void TestGetDataContextKeyMatchingQueryFromDifferentCompany()
		{
			#region Company, Branch

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "CP1";
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_Code = "BR1";
			branch1.GB_GC = company1.PK;

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "CP2";
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_Code = "BR2";
			branch1.GB_GC = company2.PK;

			var company3 = Factory.NewWithValidTestData<GlbCompany>();
			company3.GC_Code = "CP3";
			var branch3 = Factory.NewWithValidTestData<GlbBranch>();
			branch3.GB_Code = "BR3";
			branch3.GB_GC = company3.PK;

			var company4 = Factory.NewWithValidTestData<GlbCompany>();
			company4.GC_Code = "FJC";
			var branch4 = Factory.NewWithValidTestData<GlbBranch>();
			branch4.GB_Code = "FJB";
			branch4.GB_GC = company4.PK;
			#endregion

			#region Create JobDeclaration

			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration2.JE_DeclarationReference = "B00001024";
			declaration2.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration2.JE_VesselName = "AALSMEERGRACHT";
			declaration2.JE_VoyageFlightNo = "919P";
			declaration2.JE_RL_NKPortOfArrival = "USPHL";
			declaration2.JE_GB = branch1.PK;

			var declaration3 = Factory.New<BaseJobDeclaration>();
			declaration3.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration3.JE_DeclarationReference = "B00001024";
			declaration3.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration3.JE_VesselName = "AULSMEER";
			declaration3.JE_VoyageFlightNo = "920P";
			declaration3.JE_RL_NKPortOfArrival = "AUSYD";
			declaration3.JE_GB = branch3.PK;

			var declaration1 = Factory.New<BaseJobDeclaration>();
			declaration1.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration1.JE_DeclarationReference = "B00001021";
			declaration1.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration1.JE_VesselName = "AALSMEERGRAC";
			declaration1.JE_VoyageFlightNo = "900P";
			declaration1.JE_RL_NKPortOfArrival = "USPHL";
			declaration1.JE_GB = branch1.PK;

			var declaration4 = Factory.New<BaseJobDeclaration>();
			declaration4.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration4.JE_DeclarationReference = "B00001021";
			declaration4.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration4.JE_VesselName = "AULSMEER";
			declaration4.JE_VoyageFlightNo = "920P";
			declaration4.JE_RL_NKPortOfArrival = "AUSYD";
			declaration4.JE_GB = branch4.PK;

			Factory.SaveForTesting();

			#endregion

			var message = GetQueuedUniversalEventMessage(IncomingEventForDFC); //B00001024
			message.EM_GB = branch2.PK;
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var sessionTracker = manager.Process(message);
			var attemptedImports = sessionTracker.ImportResults;
			AssertMultilineASCIIEquals("rejected", @"False|Warning - No Module found a Business Entity to link this Universal Event to.|NULL", attemptedImports.FormatAndOrderImportAttempts());

			message = GetQueuedUniversalEventMessage(IncomingEventForDFC2); // B00001021
			message.EM_GB = branch2.PK;
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			sessionTracker = manager.Process(message);
			attemptedImports = sessionTracker.ImportResults;

			AssertMultilineASCIIEquals("ProcessedOK", @"True|Linked Event to Declaration B00001021.|CustomsDeclaration-B00001021", attemptedImports.FormatAndOrderImportAttempts());
			AssertEquals(1, declaration4.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "DCF")).Length);
		}

		public void TestImportUsingTargetKeyOnSubShipment()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var newFactory = new BusinessObjectFactory();
				var consol = newFactory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_RL_NKLoadPort = "HKWNI";
				consol.JK_MasterBillNum = "MB324334";
				consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
				consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
				consol.JK_SendingForwarderHandlingType = "GTT";
				consol.JK_ReceivingForwarderHandlingType = "GTT";

				var shipment = consol.Shipments.AddNew();
				shipment.JS_HouseBill = "HB9863521";
				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_RL_NKDestination = "AUBNE";
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				shipment.JS_GoodsDescription = "HELLO WORLD";

				var declaration = newFactory.New<BaseJobDeclaration>();
				declaration.JE_JS = shipment.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.ShipmentSynchroniser.Synchronise(true);
				newFactory.Save();

				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipment)), true, true);
				var dataObject = writer.GetDataObject(shipment);
				dataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

				var message = GetQueuedUniversalShipmentMessage(dataObject);
				message.EM_MessageText = message.EM_MessageText.Replace("DataSource", "DataTarget");
				Factory.SaveForTesting();
				shipment.JS_GoodsDescription = "BYE WORLD";
				declaration.JE_GoodsDescription = "YO WORLD";
				newFactory.Save();
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
Updated Declaration S00001000 from UniversalShipment.
Updated Shipment S00001000 (House Bill='HB9863521') from UniversalShipment.
Updated Consol C00001000 (Master Bill='MB324334') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='MB324334') with 1 x Transport, 1 x ForwardingPackLine, 2 x Bill, 1 x Package, 1 x JobDeclaration, 1 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());

					var logNoteText = message.GetLogNoteText();
					AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Successfully loaded matching ForwardingConsol.
Populating ForwardingConsol...
Successfully loaded matching Transport.
Populating Transport...
Transport Leg: Origin: HKWNI Destination: AUSYD
Transport Leg updated.
Successfully loaded matching ForwardingShipment.
Populating ForwardingShipment...
Successfully loaded matching Transport.
Populating Transport...
Transport Leg: Origin: HKWNI Destination: AUSYD
Transport Leg updated.
Successfully loaded matching ForwardingPackLine.
Populating ForwardingPackLine...
Successfully loaded matching JobDeclaration.
Populating JobDeclaration...
Successfully loaded matching Bill.
Populating Bill...
Successfully loaded matching Bill.
Populating Bill...
No matching Package found, creating new Package.
Populating Package...
Updated Declaration S00001000 from UniversalShipment.
Updated Shipment S00001000 (House Bill='HB9863521') from UniversalShipment.
Updated Consol C00001000 (Master Bill='MB324334') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='MB324334') with 1 x Transport, 1 x ForwardingPackLine, 2 x Bill, 1 x Package, 1 x JobDeclaration, 1 x ForwardingShipment.
".Trim(), logNoteText);

					shipment = Factory.LoadFromNaturalKey<ForwardingShipment>(JobShipmentSchema.JS_UniqueConsignRef, "S00001000");
					AssertNotNull("shipment", shipment);
					AssertEquals("JS_HouseBill", "HB9863521", shipment.JS_HouseBill);
					AssertEquals("JS_TransportMode", Core.Constants.TransportModes.Sea, shipment.JS_TransportMode);
					AssertEquals("JS_RL_NKOrigin", "HKHKG", shipment.JS_RL_NKOrigin);
					AssertEquals("JS_RL_NKDestination", "AUBNE", shipment.JS_RL_NKDestination);
					AssertEquals("JS_GoodsDescription", "HELLO WORLD", shipment.JS_GoodsDescription);

					declaration = (BaseJobDeclaration)shipment.DeclarationForDocuments;
					AssertNotNull("declaration", declaration);
					AssertEquals("JE_TransportMode", Core.Constants.TransportModes.Sea, declaration.JE_TransportMode);
					AssertEquals("JE_MasterBill", "MB324334", declaration.JE_MasterBill);
					AssertEquals("JE_HouseBill", "HB9863521", declaration.JE_HouseBill);
					AssertEquals("JE_RL_NKOrigin", "HKHKG", declaration.JE_RL_NKOrigin);
					AssertEquals("JE_RL_NKPortOfLoading", "HKWNI", declaration.JE_RL_NKPortOfLoading);
					AssertEquals("JE_RL_NKPortOfArrival", "AUSYD", declaration.JE_RL_NKPortOfArrival);
					AssertEquals("JE_RL_NKFinalDestination", "AUBNE", declaration.JE_RL_NKFinalDestination);
					AssertEquals("JE_GoodsDescription", "HELLO WORLD", declaration.JE_GoodsDescription);

					AssertEquals("shipment.Consols.Count", 1, shipment.Consols.Count);
					consol = shipment.Consols[0];
					AssertNotNull("consol", consol);
					AssertEquals("JK_MasterBillNum", "MB324334", consol.JK_MasterBillNum);
					AssertEquals("JK_TransportMode", Core.Constants.TransportModes.Sea, consol.JK_TransportMode);
					AssertEquals("JK_RL_NKLoadPort", "HKWNI", consol.JK_RL_NKLoadPort);
					AssertEquals("JK_RL_NKDischargePort", "AUSYD", consol.JK_RL_NKDischargePort);
				});
			}
		}

		public void TestUseUnmatchedOrganisationForMatchingFunctionalityWorks()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var unmatchedOrgPK = OrganizationAddressTestHelper.SetUseUnmatchedOrganisationForMatchingRegistry(true);
			var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(TestFileHelper.GetPathForUniversalTestFiles("UniversalShipmentWithUnmatchedOrganisations.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Declaration (Master Bill='I_DO_NOT_EXIST') from UniversalShipment.
Successfully saved Declaration B00001000.
			".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No matching BaseJobDeclaration found, creating new BaseJobDeclaration.
Populating BaseJobDeclaration...
Matching 'SupplierDocumentaryAddress':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Matching 'ImporterDocumentaryAddress':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Matching 'ShippingLine':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Matching 'Forwarder':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Warning - Matching 'SupplierDocumentaryAddress':- No match found for '[Company Name: VAPOUR CORPORATION; Address 1: UNIT 0, -1 FANTASY LANE; City: FAKE HILL]'.
Warning - Matching 'ImporterDocumentaryAddress':- No match found for '[Company Name: TERRY TOWELLERS INC; Address 1: 238 APTITUDE PLAZA; Address 2: FANTASY VALLEY BUSINESS CENTRE; City: FANTASY VALLEY]'.
Warning - Matching 'NotifyParty':- No match found for '[Company Name: TERRY TOWELLERS INC; Address 1: 238 APTITUDE PLAZA; Address 2: FANTASY VALLEY BUSINESS CENTRE; City: FANTASY VALLEY]'.
Warning - Matching 'NotifyParty2':- No match found for '[Company Name: TERRY TROWELLERS INC; Address 1: 238 APTITUDE PLAZA; Address 2: FANTASY VALLEY BUSINESS CENTRE; City: FANTASY VALLEY]'.
Warning - Matching 'NotifyParty3':- No match found for '[Company Name: TERRY TWOELLERS INC; Address 1: 238 APTITUDE PLAZA; Address 2: FANTASY VALLEY BUSINESS CENTRE; City: FANTASY VALLEY]'.
Added Declaration (Master Bill='I_DO_NOT_EXIST') from UniversalShipment.
Successfully saved Declaration B00001000.
				".Trim(), logNoteText);
			});

			var reloadedDeclaration = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));

			var unmatchedOrgNotes = reloadedDeclaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
			AssertEquals("unmatchedOrgNotes.Length", 1, unmatchedOrgNotes.Length);
			var unmatchedOrgNote = unmatchedOrgNotes[0];
			AssertMultilineASCIIEquals("Unmatched Orgs Note Content", @"Organisation Type: Consignor
Owner Code: 
EDI Code: 
Organisation Name: VAPOUR CORPORATION
Address Line 1: UNIT 0, -1 FANTASY LANE
Address Line 2: 
City: FAKE HILL
Post Code: 2987
State or Province: NSW
Country: AU
Doc Address Type: 
 
Organisation Type: Consignee
Owner Code: 
EDI Code: 
Organisation Name: TERRY TOWELLERS INC
Address Line 1: 238 APTITUDE PLAZA
Address Line 2: FANTASY VALLEY BUSINESS CENTRE
City: FANTASY VALLEY
Post Code: 4006
State or Province: QLD
Country: AU
Doc Address Type: 
 
Organisation Type: Carrier
Owner Code: 
EDI Code: 
Organisation Name: FLOGGED OGGIN LTD
Address Line 1: 556 WORN OUT ALLEY
Address Line 2: 
City: NIGHTMAREIA
Post Code: 7007
State or Province: WA
Country: AU
Doc Address Type: 
 
Organisation Type: Forwarder
Owner Code: 
EDI Code: 
Organisation Name: VAPOUR CORPORATION
Address Line 1: UNIT 0, -1 FANTASY LANE
Address Line 2: 
City: FAKE HILL
Post Code: 2987
State or Province: NSW
Country: AU
Doc Address Type: 
 ".TrimStart(), unmatchedOrgNote.ST_NoteText);

			AssertEquals("reloadedDeclaration.JE_OH_ShippingLine", unmatchedOrgPK, reloadedDeclaration.JE_OH_ShippingLine);
			AssertEquals("reloadedDeclaration.JE_OH_Forwarder", unmatchedOrgPK, reloadedDeclaration.JE_OH_Forwarder);
			AssertEquals("reloadedDeclaration.JE_OH_Supplier", unmatchedOrgPK, reloadedDeclaration.JE_OH_Supplier);
			AssertEquals("reloadedDeclaration.JE_OH_Importer", unmatchedOrgPK, reloadedDeclaration.JE_OH_Importer);

			var docAddressTypesPresent = reloadedDeclaration.DocAddresses
				.OfType<JobDocAddress>()
				.Select(a => a.DocAddressType.ToString())
				.OrderBy(t => t)
				.ToArray();
			AssertEquals("docAddressTypesPresent should not include the 'Client' address or any other 'Extras' not present in the incoming XML", @"
ImporterDocumentaryAddress
NotifyParty
NotifyParty2
NotifyParty3
SupplierDocumentaryAddress
".Trim(), string.Join("\r\n", docAddressTypesPresent));
		}

		public void TestMeaninglessDocAddressWillNotCreateMeaninglessAddressRow()
		{
			var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(TestFileHelper.GetPathForUniversalTestFiles("UniversalShipmentWithUnmatchedOrganisationsWithMeaninglessDocAddress.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Declaration (Master Bill='I_DO_NOT_EXIST') from UniversalShipment.
Successfully saved Declaration B00001000.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No matching BaseJobDeclaration found, creating new BaseJobDeclaration.
Populating BaseJobDeclaration...
Warning - Matching 'SupplierDocumentaryAddress':- No match found for '[Org. Code: WHATEVER]'.
Warning - Matching 'SupplierDocumentaryAddress':- No match found for '[Org. Code: WHATEVER]'.
Matching 'ImporterDocumentaryAddress':- Matched to 'ABABEU' by code, main address used.
Warning - Matching 'SupplierDocumentaryAddress':- No match found for '[Org. Code: WHATEVER]'.
Warning - Matching 'SupplierPickupDeliveryAddress':- No match found for '[Org. Code: WHATEVER]'.
Matching 'ImporterDocumentaryAddress':- Matched to 'ABABEU' by code, main address used.
Matching 'ImporterPickupDeliveryAddress':- Matched to 'ABABEU' by code, main address used.
Added Declaration (Master Bill='I_DO_NOT_EXIST') from UniversalShipment.
Successfully saved Declaration B00001000.
".Trim(), logNoteText);
			});

			var reloadedDeclaration = new BusinessObjectFactory().LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));

			var unmatchedOrgNotes = reloadedDeclaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
			AssertEquals("unmatchedOrgNotes.Length", 0, unmatchedOrgNotes.Length);
			AssertEquals("reloadedDeclaration.JE_OH_Supplier", ZGuid.Empty, reloadedDeclaration.JE_OH_Supplier);
			AssertNotEquals("reloadedDeclaration.JE_OH_Importer", ZGuid.Empty, reloadedDeclaration.JE_OH_Importer);

			var docAddressTypesPresent = reloadedDeclaration.DocAddresses
				.OfType<JobDocAddress>()
				.Select(a => a.DocAddressType.ToString())
				.OrderBy(t => t)
				.ToArray();
			AssertEquals("docAddressTypesPresent should not include the 'Client' address or any other 'Extras' not present in the incoming XML or address with empty content", @"
ImporterDocumentaryAddress
".Trim(), string.Join("\r\n", docAddressTypesPresent));

			var message_1 = GetQueuedUniversalShipmentMessage(File.ReadAllText(TestFileHelper.GetPathForUniversalTestFiles("UniversalShipmentWithUnmatchedOrganisationsWithMeaninglessDocAddressWithTarget.xml")));
			manager.Process(message_1);
			reloadedDeclaration = new BusinessObjectFactory().LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));
			unmatchedOrgNotes = reloadedDeclaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
			AssertEquals("unmatchedOrgNotes.Length", 0, unmatchedOrgNotes.Length);
			AssertEquals("reloadedDeclaration.JE_OH_Supplier", ZGuid.Empty, reloadedDeclaration.JE_OH_Supplier);
			AssertNotEquals("reloadedDeclaration.JE_OH_Importer", ZGuid.Empty, reloadedDeclaration.JE_OH_Importer);

			docAddressTypesPresent = reloadedDeclaration.DocAddresses
				.OfType<JobDocAddress>()
				.Select(a => a.DocAddressType.ToString())
				.OrderBy(t => t)
				.ToArray();
			AssertEquals("Updating to meaningless address will delete current DocAddress", string.Empty, string.Join("\r\n", docAddressTypesPresent));
		}

		public void TestDepotAddressIsNotCreatedWhenNotMatched()
		{
			var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(TestFileHelper.GetPathForUniversalTestFiles("UniversalShipmentWithUnmatchedOrganisationsWithDoNotSupportOverridesType.xml")));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var reloadedDeclaration = new BusinessObjectFactory().LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));
			var docAddressTypesPresent = reloadedDeclaration.DocAddresses
				.Cast<JobDocAddress>()
				.Select(a => a.DocAddressType.ToString())
				.OrderBy(t => t);
			AssertEquals("docAddressTypesPresent should not include CustomsDepotAddress or CustomsContainerYardAddress",
				"ImporterDocumentaryAddress, NotifyParty, SupplierDocumentaryAddress", string.Join(", ", docAddressTypesPresent));

			var depotAddress = reloadedDeclaration.DepotDocAddress;
			Assert("Address not overrridden", !depotAddress.E2_AddressOverride);
			AssertEquals("Has no company name", String.Empty, depotAddress.E2_CompanyName);
			AssertEquals("Has no address", ZGuid.Empty, depotAddress.E2_OA_Address);

			var containerYardAddress = reloadedDeclaration.ContainerYardDocAddress;
			Assert("Address not overrridden", !containerYardAddress.E2_AddressOverride);
			AssertEquals("Has no company name", String.Empty, containerYardAddress.E2_CompanyName);
			AssertEquals("Has no address", ZGuid.Empty, containerYardAddress.E2_OA_Address);
		}

		public void TestDepotAddressIsUpdatedWhenMatched()
		{
			var depotOrg = Factory.New<OrgHeader>();
			depotOrg.OH_Code = "TROBONSYD";
			depotOrg.OH_FullName = "TROJAN BOND";
			var depotOrgAddress = Factory.New<OrgAddress>();
			depotOrgAddress.OA_OH = depotOrg.PK;
			depotOrgAddress.OA_Address1 = "11 Bumborah Point Rd";

			var containerYardOrg = Factory.New<OrgHeader>();
			containerYardOrg.OH_Code = "DECONSOL2";
			containerYardOrg.OH_FullName = "DECONSOLIDATOR2";
			var containerYardOrgAddress = Factory.New<OrgAddress>();
			containerYardOrgAddress.OA_OH = containerYardOrg.PK;
			containerYardOrgAddress.OA_Address1 = "ADDRESS12";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(TestFileHelper.GetPathForUniversalTestFiles("UniversalShipmentWithUnmatchedOrganisationsWithDoNotSupportOverridesType.xml")));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var reloadedDeclaration = new BusinessObjectFactory().LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));
			var docAddressTypesPresent = reloadedDeclaration.DocAddresses
				.Cast<JobDocAddress>()
				.Select(a => a.DocAddressType.ToString())
				.OrderBy(t => t);
			AssertEquals("docAddressTypesPresent should include CustomsDepotAddress and CustomsContainerYardAddress",
				"CustomsContainerYardAddress, CustomsDepotAddress, ImporterDocumentaryAddress, NotifyParty, SupplierDocumentaryAddress", string.Join(", ", docAddressTypesPresent));

			var depotAddress = reloadedDeclaration.DepotDocAddress;
			Assert("Address not overrridden", !depotAddress.E2_AddressOverride);
			AssertEquals("Has found company name", "TROJAN BOND", depotAddress.E2_CompanyName);
			AssertEquals("Using found org address", depotOrgAddress.PK, depotAddress.E2_OA_Address);

			var containerYardAddress = reloadedDeclaration.ContainerYardDocAddress;
			Assert("Address not overrridden as override is not allowed", !containerYardAddress.E2_AddressOverride);
			AssertEquals("Has found company name", "DECONSOLIDATOR2", containerYardAddress.E2_CompanyName);
			AssertEquals("Using found org address", containerYardOrgAddress.PK, containerYardAddress.E2_OA_Address);
		}

		public void TestIncomingEventForShipmentLinkedDeclarationPicksDeclarationInRightCompany()
		{
			var declarationInRightCompany = Factory.New<BaseJobDeclaration>();
			declarationInRightCompany.JE_DeclarationReference = "S00001003";
			declarationInRightCompany.JE_HouseBill = "HOUSE_BILL_1";

			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			var otherBranch = Factory.NewWithValidTestData<GlbBranch>();
			otherBranch.GB_GC = otherCompany.PK;

			var declarationInOtherCompany = Factory.New<BaseJobDeclaration>();
			declarationInOtherCompany.JE_DeclarationReference = "S00001003";
			declarationInOtherCompany.JE_HouseBill = "HOUSE_BILL_2";
			declarationInOtherCompany.JE_GB = otherBranch.PK;

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(TestFileHelper.GetPathForUniversalTestFiles("DuplicateUniversalShipment.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertAllLinesStartWith("serviceTaskLog.ToString()", @"
Updated Declaration S00001003 from UniversalShipment.
Successfully saved Declaration S00001003.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertAllLinesStartWith("message.GetLogNoteText()", string.Format(@"
Successfully loaded matching BaseJobDeclaration.
Populating BaseJobDeclaration...
Matching 'ImporterDocumentaryAddress':- Matched to 'ABABEU' by code, address 'PST: DIESLSTR 11' by short code.
Warning - Matching 'ShippingLine':- No match found for '[Company Name: FLOGGED OGGIN LTD; Address 1: 556 WORN OUT ALLEY; City: NIGHTMAREIA]'.
Warning - Matching 'Forwarder':- No match found for '[Company Name: VAPOUR CORPORATION; Address 1: UNIT 0, -1 FANTASY LANE; City: FAKE HILL]'.
Matching 'ImporterDocumentaryAddress':- Matched to 'ABABEU' by code, address 'PST: DIESLSTR 11' by short code.
Warning - Matching 'NotifyParty':- No match found for '[Company Name: TERRY TOWELLERS INC; Address 1: 238 APTITUDE PLAZA; Address 2: FANTASY VALLEY BUSINESS CENTRE; City: FANTASY VALLEY]'.
Warning - Matching 'NotifyParty2':- No match found for '[Company Name: JIMMY SMITS BURGERS; Address 1: SHOP 10 HAPPY VALLEY PLAZA; Address 2: HAPPY VALLEY; City: HAPPY VALLEY]'.
Updated Declaration S00001003 from UniversalShipment.
Successfully saved Declaration S00001003.
".Trim(), declarationInRightCompany.PK, declarationInOtherCompany.PK), logNoteText);

				declarationInOtherCompany.Reload();
				AssertNotEquals("declarationInOtherCompany.JE_GoodsDescription", "ALAN SEALE'S GRO RITE", declarationInOtherCompany.JE_GoodsDescription);

				declarationInRightCompany.Reload();
				AssertEquals("declarationInRightCompany.JE_GoodsDescription", "ALAN SEALE'S GRO RITE", declarationInRightCompany.JE_GoodsDescription);
			});
		}

		public void TestImportEventWithDataTargetDoesNotUseContextInformationIfDataTargetMatches()
		{
			var declarationWithMatchingJobNumber = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declarationWithMatchingJobNumber.JE_DeclarationReference = "B00001010";
			declarationWithMatchingJobNumber.JE_TransportMode = Core.Constants.TransportModes.Air;
			declarationWithMatchingJobNumber.JE_HouseBill = "ONTHEHOUSE";

			var declarationWithMatchingHouseBill = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declarationWithMatchingHouseBill.JE_DeclarationReference = "B00002010";
			declarationWithMatchingHouseBill.JE_TransportMode = Core.Constants.TransportModes.Air;
			declarationWithMatchingHouseBill.JE_HouseBill = "FRED235478923";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(File.ReadAllText(TestFileHelper.GetPathForUniversalTestFiles("UniversalEventWithDataTarget.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Declaration B00001010.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Declaration B00001010.
".Trim(), message.GetLogNoteText());

				declarationWithMatchingJobNumber.Reload();
				var logs = declarationWithMatchingJobNumber.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AuthorisedCode));
				AssertEquals("[ATH] - Action Authorised event count", 1, logs.Length);
				var log = logs[0];

				var contextItems = log.SourceInfoItems;
				var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
				AssertEquals("Context Items on Event", @"
HAWB Number - FRED235478923
HAWB Origin IATA Airport Code - FRA
HAWB Destination IATA Airport Code - HKG
Data Source Company - EDI - Eagle Datamation International
Data Source Enterprise ID - EDI
Data Source Server ID - DAT
".Trim(), actualContextItems);
			});
		}

		public void TestImportWithOwnerReferenceDoesNotFallBackToBillMatching()
		{
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var decWithOwnerRef = Factory.NewWithValidTestData<BaseJobDeclaration>();
			decWithOwnerRef.JE_TransportMode = Core.Constants.TransportModes.Air;
			decWithOwnerRef.JE_OwnerRef = "0WN3D1";
			decWithOwnerRef.JE_MasterBill = "IEXIST";
			decWithOwnerRef.JE_GoodsDescription = "CHANGEME";
			decWithOwnerRef.JE_OH_Importer = OrgHeader.LoadFromCode(Factory.BOFactory, "ABABEU").PK;

			var decWithoutOwnerRef = Factory.NewWithValidTestData<BaseJobDeclaration>();
			decWithoutOwnerRef.JE_TransportMode = Core.Constants.TransportModes.Air;
			decWithoutOwnerRef.JE_OwnerRef = "";
			decWithoutOwnerRef.JE_MasterBill = "MB32432";
			decWithoutOwnerRef.JE_GoodsDescription = "CHANGEME";
			decWithoutOwnerRef.JE_OH_Importer = decWithOwnerRef.JE_OH_Importer;

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(TestFileHelper.GetPathForUniversalTestFiles("UniversalShipmentWithOwnerReference.xml")));

			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			manager.Process(message);

			decWithOwnerRef.Reload();
			AssertEquals("HATSOFF", decWithOwnerRef.JE_GoodsDescription);
			decWithoutOwnerRef.Reload();
			AssertEquals("CHANGEME", decWithoutOwnerRef.JE_GoodsDescription);
		}

		public void TestImportWithoutBillNumberUnderPackLine_Issue00853032()
		{
			var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(TestFileHelper.GetPathForUniversalTestFiles("UniversalShipmentWithoutBillNumber.xml")));

			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			AssertNoExceptionThrown(delegate
			{ manager.Process(message); });
		}

		public void TestCanImportEventViaUniversalDataBuss()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B00001010";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_HouseBill = "FRED235478923";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(File.ReadAllText(TestFileHelper.GetPathForUniversalTestFiles("UniversalEvent.xml")));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Linked Event to Declaration B00001010.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Linked Event to Declaration B00001010.
".Trim(), message.GetLogNoteText());

				declaration.Reload();
				var logs = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AuthorisedCode));
				AssertEquals("[ATH] - Action Authorised event count", 1, logs.Length);
				var log = logs[0];

				var contextItems = log.SourceInfoItems;
				var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
				AssertEquals("Context Items on Event", @"
HAWB Number - FRED235478923
HAWB Origin IATA Airport Code - FRA
HAWB Destination IATA Airport Code - HKG
AMS Number - 134FREGT
COC - 267AIRGT
".Trim(), actualContextItems);
			});
		}

		public void TestImportAndSupplierPickupDeliveryAddressAreNotCreatedForSJobs_CS00242495()
		{
			var messageData = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Shipment>
	<DataContext>
	  <DataTargetCollection>{0}
		<DataTarget>
		  <Type>CustomsDeclaration</Type>
		  <Key />
		</DataTarget>
	  </DataTargetCollection>
	</DataContext>
	<BookingConfirmationReference>989456</BookingConfirmationReference>
	<TransportMode>
	  <Code>AIR</Code>
	</TransportMode>
	<ContainerMode>
	  <Code>LSE</Code>
	</ContainerMode>
	<CommercialInfo>
	  <Name>All Invoices</Name>
	  <CommercialInvoiceCollection>
		<CommercialInvoice>
		  <InvoiceNumber>0000989456</InvoiceNumber>
		  <InvoiceAmount>2136.29</InvoiceAmount>
		  <InvoiceCurrency>
			<Code>GBP</Code>
		  </InvoiceCurrency>
		  <Supplier>
			<AddressType>Supplier</AddressType>
			<CompanyName>VWR INTERNATIONAL LTD</CompanyName>
			<Address1></Address1>
			<Address2></Address2>
			<AddressOverride>false</AddressOverride>
			<City>Lyndhurst</City>
			<Country>
			  <Code>GB</Code>
			</Country>
			<Phone>+44 1455 558 600</Phone>
		  </Supplier>
		  <AddInfoGroupCollection>
			<AddInfoGroup>
			  <Type>
				<Code>GPD</Code>
			  </Type>
			  <AddInfoCollection>
				<AddInfo>
				  <Key>Reference</Key>
				  <Value>989456</Value>
				</AddInfo>
			  </AddInfoCollection>
			</AddInfoGroup>
		  </AddInfoGroupCollection>
		  <CommercialInvoiceLineCollection>
			<CommercialInvoiceLine>
			  <LineNo>1</LineNo>
			  <ContainerMode>
				<Code>LSE</Code>
			  </ContainerMode>
			  <HarmonisedCode>29130000</HarmonisedCode>
			  <LinePrice>41.98</LinePrice>
			  <NetWeight>0.61</NetWeight>
			  <NetWeightUnit>
				<Code>KG</Code>
			  </NetWeightUnit>
			  <AddInfoCollection>
				<AddInfo>
				  <Key>ProcedureCode</Key>
				  <Value>1000001</Value>
				</AddInfo>
			  </AddInfoCollection>
			</CommercialInvoiceLine>
			<CommercialInvoiceLine>
			  <LineNo>2</LineNo>
			  <ContainerMode>
				<Code>LSE</Code>
			  </ContainerMode>
			  <HarmonisedCode>39269097</HarmonisedCode>
			  <LinePrice>19.16</LinePrice>
			  <NetWeight>0.96</NetWeight>
			  <NetWeightUnit>
				<Code>KG</Code>
			  </NetWeightUnit>
			  <AddInfoCollection>
				<AddInfo>
				  <Key>ProcedureCode</Key>
				  <Value>1000001</Value>
				</AddInfo>
			  </AddInfoCollection>
			</CommercialInvoiceLine>
			<CommercialInvoiceLine>
			  <LineNo>3</LineNo>
			  <ContainerMode>
				<Code>LSE</Code>
			  </ContainerMode>
			  <HarmonisedCode>62101010</HarmonisedCode>
			  <LinePrice>515.90</LinePrice>
			  <NetWeight>225.93</NetWeight>
			  <NetWeightUnit>
				<Code>KG</Code>
			  </NetWeightUnit>
			  <AddInfoCollection>
				<AddInfo>
				  <Key>ProcedureCode</Key>
				  <Value>1000001</Value>
				</AddInfo>
			  </AddInfoCollection>
			</CommercialInvoiceLine>
			<CommercialInvoiceLine>
			  <LineNo>4</LineNo>
			  <ContainerMode>
				<Code>LSE</Code>
			  </ContainerMode>
			  <HarmonisedCode>62101098</HarmonisedCode>
			  <LinePrice>21.38</LinePrice>
			  <NetWeight>6.32</NetWeight>
			  <NetWeightUnit>
				<Code>KG</Code>
			  </NetWeightUnit>
			  <AddInfoCollection>
				<AddInfo>
				  <Key>ProcedureCode</Key>
				  <Value>1000001</Value>
				</AddInfo>
			  </AddInfoCollection>
			</CommercialInvoiceLine>
			<CommercialInvoiceLine>
			  <LineNo>5</LineNo>
			  <ContainerMode>
				<Code>LSE</Code>
			  </ContainerMode>
			  <HarmonisedCode>63079010</HarmonisedCode>
			  <LinePrice>11.32</LinePrice>
			  <NetWeight>3.44</NetWeight>
			  <NetWeightUnit>
				<Code>KG</Code>
			  </NetWeightUnit>
			  <AddInfoCollection>
				<AddInfo>
				  <Key>ProcedureCode</Key>
				  <Value>1000001</Value>
				</AddInfo>
			  </AddInfoCollection>
			</CommercialInvoiceLine>
			<CommercialInvoiceLine>
			  <LineNo>6</LineNo>
			  <ContainerMode>
				<Code>LSE</Code>
			  </ContainerMode>
			  <HarmonisedCode>84239000</HarmonisedCode>
			  <LinePrice>329.81</LinePrice>
			  <NetWeight>45.30</NetWeight>
			  <NetWeightUnit>
				<Code>KG</Code>
			  </NetWeightUnit>
			  <AddInfoCollection>
				<AddInfo>
				  <Key>ProcedureCode</Key>
				  <Value>1000001</Value>
				</AddInfo>
			  </AddInfoCollection>
			</CommercialInvoiceLine>
			<CommercialInvoiceLine>
			  <LineNo>7</LineNo>
			  <ContainerMode>
				<Code>LSE</Code>
			  </ContainerMode>
			  <HarmonisedCode>84798200</HarmonisedCode>
			  <LinePrice>1067.09</LinePrice>
			  <NetWeight>8.32</NetWeight>
			  <NetWeightUnit>
				<Code>KG</Code>
			  </NetWeightUnit>
			  <AddInfoCollection>
				<AddInfo>
				  <Key>ProcedureCode</Key>
				  <Value>1000001</Value>
				</AddInfo>
			  </AddInfoCollection>
			</CommercialInvoiceLine>
			<CommercialInvoiceLine>
			  <LineNo>8</LineNo>
			  <ContainerMode>
				<Code>LSE</Code>
			  </ContainerMode>
			  <HarmonisedCode>90189084</HarmonisedCode>
			  <LinePrice>50.34</LinePrice>
			  <NetWeight>0.62</NetWeight>
			  <NetWeightUnit>
				<Code>KG</Code>
			  </NetWeightUnit>
			  <AddInfoCollection>
				<AddInfo>
				  <Key>ProcedureCode</Key>
				  <Value>1000001</Value>
				</AddInfo>
			  </AddInfoCollection>
			</CommercialInvoiceLine>
			<CommercialInvoiceLine>
			  <LineNo>9</LineNo>
			  <ContainerMode>
				<Code>LSE</Code>
			  </ContainerMode>
			  <HarmonisedCode>90318098</HarmonisedCode>
			  <LinePrice>79.31</LinePrice>
			  <NetWeight>1.62</NetWeight>
			  <NetWeightUnit>
				<Code>KG</Code>
			  </NetWeightUnit>
			  <AddInfoCollection>
				<AddInfo>
				  <Key>ProcedureCode</Key>
				  <Value>1000001</Value>
				</AddInfo>
			  </AddInfoCollection>
			</CommercialInvoiceLine>
		  </CommercialInvoiceLineCollection>
		</CommercialInvoice>
	  </CommercialInvoiceCollection>
	</CommercialInfo>
	<DocumentedWeight>0</DocumentedWeight>
	<GoodsValue>2136.29</GoodsValue>
	<GoodsValueCurrency>
	  <Code>GBP</Code>
	</GoodsValueCurrency>
	<PortOfDestination>
	  <Code></Code>
	</PortOfDestination>
	<PortOfOrigin>
	  <Code></Code>
	</PortOfOrigin>
	<ShipmentIncoTerm>
	  <Code>EXW</Code>
	</ShipmentIncoTerm>
	<TotalNoOfPacks>0</TotalNoOfPacks>
	<TotalNoOfPacksPackageType>
	  <Code>PKG</Code>
	</TotalNoOfPacksPackageType>
	<TotalWeight>0</TotalWeight>
	<TotalWeightUnit>
	  <Code>KG</Code>
	</TotalWeightUnit>
	<TransportMode>
	  <Code>AIR</Code>
	</TransportMode>
	<LocalProcessing>
	  <EstimatedPickup></EstimatedPickup>
	  <OrderNumberCollection>
		<OrderNumber>
		  <OrderReference>989456</OrderReference>
		</OrderNumber>
	  </OrderNumberCollection>
	</LocalProcessing>
	<WayBillNumber>HB32327524965</WayBillNumber>
	<WayBillType>
	  <Code>HWB</Code>
	  <Description>House Waybill</Description>
	</WayBillType>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>ConsigneeDocumentaryAddress</AddressType>
		<CompanyName>Monitoring &amp; Control Laboratories (PTY)</CompanyName>
		<Address1></Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<City>Lyndhurst</City>
		<Country>
		  <Code>ZA</Code>
		</Country>
		<Phone>00 27 11 327 6524</Phone>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsignorDocumentaryAddress</AddressType>
		<CompanyName>VWR INTERNATIONAL LTD</CompanyName>
		<Address1></Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<City>Lyndhurst</City>
		<Country>
		  <Code>GB</Code>
		</Country>
		<Phone>+44 1455 558 600</Phone>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>NotifyParty</AddressType>
		<CompanyName>DUNNET and JOHNSTON GROUP PTY LMTD</CompanyName>
		<Address1></Address1>
		<Address2></Address2>
		<AddressOverride>true</AddressOverride>
		<City>WEST CHATSWOOD</City>
		<Country>
		  <Code></Code>
		</Country>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>NotifyParty2</AddressType>
		<CompanyName>Smith and Jones Pty. Ltd.</CompanyName>
		<Address1></Address1>
		<Address2></Address2>
		<AddressOverride>true</AddressOverride>
		<City>Liverpool</City>
		<Country>
		  <Code></Code>
		</Country>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>NotifyParty3</AddressType>
		<CompanyName>Will Anderson</CompanyName>
		<Address1></Address1>
		<Address2></Address2>
		<AddressOverride>true</AddressOverride>
		<City>Melbourne</City>
		<Country>
		  <Code></Code>
		</Country>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ImporterDocumentaryAddress</AddressType>
		<CompanyName>Monitoring &amp; Control Laboratories (PTY)</CompanyName>
		<Address1></Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<City>Lyndhurst</City>
		<Country>
		  <Code>ZA</Code>
		</Country>
		<Phone>00 27 11 327 6524</Phone>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>{1}</AddressType>
		<CompanyName>Monitoring &amp; Control Laboratories (PTY)</CompanyName>
		<Address1></Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<City>Lyndhurst</City>
		<Country>
		  <Code>ZA</Code>
		</Country>
		<Phone>00 27 11 327 6524</Phone>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>SupplierDocumentaryAddress</AddressType>
		<CompanyName>VWR INTERNATIONAL LTD</CompanyName>
		<Address1></Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<City>Lyndhurst</City>
		<Country>
		  <Code>GB</Code>
		</Country>
		<Phone>+44 1455 558 600</Phone>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>{2}</AddressType>
		<CompanyName>VWR INTERNATIONAL LTD</CompanyName>
		<Address1></Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<City>Lyndhurst</City>
		<Country>
		  <Code>GB</Code>
		</Country>
		<Phone>+44 1455 558 600</Phone>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
	<PackingLineCollection>
	  <PackingLine>
		<PackType>
		  <Code>PKG</Code>
		</PackType>
		<ReferenceNumber>20131008094955</ReferenceNumber>
		<Weight>0</Weight>
		<WeightUnit>
		  <Code>KG</Code>
		</WeightUnit>
	  </PackingLine>
	</PackingLineCollection>
  </Shipment>
</UniversalShipment>";

			var message = GetQueuedUniversalShipmentMessage(string.Format(messageData, "", "ImporterPickupDeliveryAddress", "SupplierPickupDeliveryAddress"));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			var query = new ZQuery(JobDeclarationSchema.JE_HouseBill, "HB32327524965");
			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);
				var declaration = Factory.LoadTop1<BaseJobDeclaration>(query);
				AssertNull("declaration should not be linked to a Shipment", declaration.Shipment);
				AssertNotNull("ImporterPickupDeliveryAddress should be created", declaration.DocAddresses.FindByDocAddressType(DocAddressType.ImporterPickupDeliveryAddress));
				AssertNotNull("SupplierPickupDeliveryAddress should be created", declaration.DocAddresses.FindByDocAddressType(DocAddressType.SupplierPickupDeliveryAddress));
				AssertNotNull("Notify Party should be created", declaration.DocAddresses.FindByDocAddressType(DocAddressType.NotifyParty));
				AssertNotNull("Notify Party 2 should be created", declaration.DocAddresses.FindByDocAddressType(DocAddressType.NotifyParty2));
				AssertNotNull("Notify Party 3 should be created", declaration.DocAddresses.FindByDocAddressType(DocAddressType.NotifyParty3));
				declaration.Delete();
				Factory.SaveForTesting();
			});

			message = GetQueuedUniversalShipmentMessage(string.Format(messageData, "", "ConsigneePickupDeliveryAddress", "ConsignorPickupDeliveryAddress"));
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);
				var declaration = Factory.LoadTop1<BaseJobDeclaration>(query);
				AssertNull("declaration should not be linked to a Shipment", declaration.Shipment);
				AssertNotNull("ImporterPickupDeliveryAddress should be created from ConsigneePickupDeliveryAddress", declaration.DocAddresses.FindByDocAddressType(DocAddressType.ImporterPickupDeliveryAddress));
				AssertNotNull("SupplierPickupDeliveryAddress should be created from ConsignorPickupDeliveryAddress", declaration.DocAddresses.FindByDocAddressType(DocAddressType.SupplierPickupDeliveryAddress));
				declaration.Delete();
				Factory.SaveForTesting();
			});

			message = GetQueuedUniversalShipmentMessage(string.Format(messageData, @"
		<DataTarget>
		  <Type>ForwardingShipment</Type>
		  <Key />
		</DataTarget>
", "ImporterPickupDeliveryAddress", "SupplierPickupDeliveryAddress"));
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);
				var declaration = Factory.LoadTop1<BaseJobDeclaration>(query);
				AssertNotNull("declaration should be linked to a Shipment", declaration.Shipment);
				AssertNull("ImporterPickupDeliveryAddress should not be created for a S-Job", declaration.DocAddresses.FindByDocAddressType(DocAddressType.ImporterPickupDeliveryAddress));
				AssertNull("SupplierPickupDeliveryAddress should not be created for a S-Job", declaration.DocAddresses.FindByDocAddressType(DocAddressType.SupplierPickupDeliveryAddress));
				AssertNull("NotifyParty should not be created for a S-job", declaration.DocAddresses.FindByDocAddressType(DocAddressType.NotifyParty));
				declaration.JE_JS = ZGuid.Empty;
				declaration.Delete();
				Factory.SaveForTesting();
			});

			message = GetQueuedUniversalShipmentMessage(string.Format(messageData, @"
		<DataTarget>
		  <Type>ForwardingShipment</Type>
		  <Key />
		</DataTarget>
", "ConsigneePickupDeliveryAddress", "ConsignorPickupDeliveryAddress"));
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);
				var declaration = Factory.LoadTop1<BaseJobDeclaration>(query);
				AssertNotNull("declaration should be linked to a Shipment", declaration.Shipment);
				AssertNull("ImporterPickupDeliveryAddress should not be created for a S-Job", declaration.DocAddresses.FindByDocAddressType(DocAddressType.ImporterPickupDeliveryAddress));
				AssertNull("SupplierPickupDeliveryAddress should not be created for a S-Job", declaration.DocAddresses.FindByDocAddressType(DocAddressType.SupplierPickupDeliveryAddress));
			});
		}

		public void TestContextInformationIsAllThereForAir()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			declaration.JE_MasterBill = "08112345675";
			declaration.JE_AgentsReference = "DOUBLEAGENT";
			declaration.JE_RL_NKPortOfLoading = "USLAX";
			declaration.JE_RL_NKPortOfArrival = "AUSYD";

			declaration.JE_HouseBill = "AIRHOUSE";
			declaration.JE_RL_NKOrigin = "USDAL";
			declaration.JE_RL_NKFinalDestination = "AUBDG";

			var cusEntryNumber1 = declaration.AdditionalReferenceNumbers.AddNew();
			cusEntryNumber1.CE_EntryNum = "CE00001";
			cusEntryNumber1.CE_EntryType = "AMS";
			cusEntryNumber1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			cusEntryNumber1.CE_ParentID = declaration.PK;
			cusEntryNumber1.CE_ParentTable = declaration.TableName;

			var cusEntryNumber2 = declaration.AdditionalReferenceNumbers.AddNew();
			cusEntryNumber2.CE_EntryNum = "CE00002";
			cusEntryNumber2.CE_EntryType = "COC";
			cusEntryNumber2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			cusEntryNumber2.CE_ParentID = declaration.PK;
			cusEntryNumber2.CE_ParentTable = declaration.TableName;

			var manager = declaration.GetUniversalDataContextManager() as IEventDataContextManager;

			var eventContextValues = string.Join("\r\n", manager.EventContextValues.Select(o => o.Key + " - " + o.Value).ToArray());

			AssertMultilineASCIIEquals("manager.EventContextValues", @"
MAWBNumber - 081-12345675
MAWBOriginIATAAirportCode - LAX
MAWBDestinationIATAAirportCode - SYD
MBOLOriginUNLOCO - USLAX
MBOLDestinationUNLOCO - AUSYD
AgentsReference - DOUBLEAGENT
HAWBNumber - AIRHOUSE
HAWBOriginIATAAirportCode - DFW
HAWBDestinationIATAAirportCode - BXG
HBOLOriginUNLOCO - USDAL
HBOLDestinationUNLOCO - AUBDG
AMS Number - CE00001
Customs Office Code (Override) - CE00002
".Trim(), eventContextValues);
		}

		public void TestContextInformationIsAllThereForSea()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			declaration.JE_MasterBill = "MB8112345675";
			declaration.JE_AgentsReference = "DOUBLEAGENT";
			declaration.JE_RL_NKPortOfLoading = "USLAX";
			declaration.JE_RL_NKPortOfArrival = "AUSYD";

			declaration.JE_HouseBill = "SEAHOUSE";
			declaration.JE_RL_NKOrigin = "USDAL";
			declaration.JE_RL_NKFinalDestination = "AUBDG";

			var cusEntryNumber1 = declaration.AdditionalReferenceNumbers.AddNew();
			cusEntryNumber1.CE_EntryNum = "CE00001";
			cusEntryNumber1.CE_EntryType = "AMS";
			cusEntryNumber1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			cusEntryNumber1.CE_ParentID = declaration.PK;
			cusEntryNumber1.CE_ParentTable = declaration.TableName;

			var cusEntryNumber2 = declaration.AdditionalReferenceNumbers.AddNew();
			cusEntryNumber2.CE_EntryNum = "CE00002";
			cusEntryNumber2.CE_EntryType = "COC";
			cusEntryNumber2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			cusEntryNumber2.CE_ParentID = declaration.PK;
			cusEntryNumber2.CE_ParentTable = declaration.TableName;

			var manager = declaration.GetUniversalDataContextManager() as IEventDataContextManager;

			var eventContextValues = string.Join("\r\n", manager.EventContextValues.Select(o => o.Key + " - " + o.Value).ToArray());

			AssertMultilineASCIIEquals("manager.EventContextValues", @"
MBOLNumber - MB8112345675
MBOLOriginUNLOCO - USLAX
MBOLDestinationUNLOCO - AUSYD
AgentsReference - DOUBLEAGENT
HBOLNumber - SEAHOUSE
HBOLOriginUNLOCO - USDAL
HBOLDestinationUNLOCO - AUBDG
AMS Number - CE00001
Customs Office Code (Override) - CE00002
".Trim(), eventContextValues);
		}

		public void TestIEventTransformer_DLVEvent()
		{
			const string eventXmlMessage = @"
<UniversalEvent>
	<Event>
	<DataContext>
	  <DataSourceCollection>
		<DataSource>
		  <Type>TransportBookingConfirmation</Type>
		  <Key></Key>
		</DataSource>
	  </DataSourceCollection>

	  <Company>
		<Code>DAU</Code>
		<Country>
		  <Code>AU</Code>
		  <Name>Australia</Name>
		</Country>
		<Name>Your Australia Demo Company</Name>
	  </Company>
	  <DataProvider>HYECMTDAU</DataProvider>
	  <EnterpriseID>HYE</EnterpriseID>
	  <EventBranch>
		<Code>BNE</Code>
		<Name>Brisbane</Name>
	  </EventBranch>
	  <EventDepartment>
		<Code>BRN</Code>
		<Name>Branch</Name>
	  </EventDepartment>
	  <EventType>
		<Code></Code>
	  </EventType>
	  <EventUser>
		<Code>E</Code>
		<Name>CargoWise Support</Name>
	  </EventUser>
	  <ServerID>CMT</ServerID>
	  <TriggerCount>1</TriggerCount>
	  <TriggerDate>2018-01-08T11:55:46.66</TriggerDate>
	  <TriggerDescription></TriggerDescription>
	  <TriggerType>Manual</TriggerType>

	  <DataTargetCollection>
		<DataTarget>
		  <Type>CustomsDeclaration</Type>
		  <Key>B00165869</Key>
		</DataTarget>
	  </DataTargetCollection>
	</DataContext>

	<EventTime>2018-01-08T11:55:00</EventTime>
	<EventType>DLV</EventType>
	<CreatedTime>2018-01-08T01:55:46.65</CreatedTime>
	<EventReference>TB00000325, 1 CNT X00001|DEP=Transport Provider|FAC=CY|TYP=EMT</EventReference>
	<IsEstimate>false</IsEstimate>

	<ContextCollection>
	  <Context>
		<Type>ContainerNumber</Type>
		<Value>X00001</Value>
	  </Context>
	</ContextCollection>
  </Event>
</UniversalEvent>
";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DeclarationReference = "B00165869";

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "X00001";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(eventXmlMessage);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("Prerequisite: message processed", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			var deliveredQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DeliveredCode);
			var gateInQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.GateInCode);

			var deliveredQueryLogs = declaration.Logs.Find(deliveredQuery);
			AssertEquals($"Delivered event should not be added to {declaration.HumanReadableName}", 0, deliveredQueryLogs.Length);
			var gateInLogs = declaration.Logs.Find(gateInQuery);
			AssertEquals($"GateIn event should be added to {declaration.HumanReadableName}", 1, gateInLogs.Length);
			var reference = gateInLogs.First().SL_Reference;
			AssertContains("Event reference has location injected", "LOC=AUSYD", reference);
		}

		public void TestIEventTransformer_PUPEvent()
		{
			const string eventXmlMessage = @"
<UniversalEvent>
  <Event>
	<DataContext>
	  <DataSourceCollection>
		<DataSource>
		  <Type>TransportBookingConfirmation</Type>
		  <Key></Key>
		</DataSource>
	  </DataSourceCollection>

	  <Company>
		<Code>DAU</Code>
		<Country>
		  <Code>AU</Code>
		  <Name>Australia</Name>
		</Country>
		<Name>Your Australia Demo Company</Name>
	  </Company>
	  <DataProvider>HYECMTDAU</DataProvider>
	  <EnterpriseID>HYE</EnterpriseID>
	  <EventBranch>
		<Code>BNE</Code>
		<Name>Brisbane</Name>
	  </EventBranch>
	  <EventDepartment>
		<Code>BRN</Code>
		<Name>Branch</Name>
	  </EventDepartment>
	  <EventType>
		<Code></Code>
	  </EventType>
	  <EventUser>
		<Code>E</Code>
		<Name>CargoWise Support</Name>
	  </EventUser>
	  <ServerID>CMT</ServerID>
	  <TriggerCount>1</TriggerCount>
	  <TriggerDate>2018-01-08T17:36:22.757</TriggerDate>
	  <TriggerDescription></TriggerDescription>
	  <TriggerType>Manual</TriggerType>

	  <DataTargetCollection>
		<DataTarget>
		  <Type>CustomsDeclaration</Type>
		  <Key>B00165870</Key>
		</DataTarget>
	  </DataTargetCollection>
	</DataContext>

	<EventTime>2018-01-09T17:35:00</EventTime>
	<EventType>PUP</EventType>
	<CreatedTime>2018-01-08T07:36:22.753</CreatedTime>
	<EventReference>TB00000326, 1 CNT X00002|DEP=Transport Provider|FAC=CTO|TYP=FUL</EventReference>
	<IsEstimate>false</IsEstimate>

	<ContextCollection>
	  <Context>
		<Type>ContainerNumber</Type>
		<Value>X00002</Value>
	  </Context>
	</ContextCollection>
  </Event>
</UniversalEvent>
";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DeclarationReference = "B00165870";

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "X00002";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalEventMessage(eventXmlMessage);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("Prerequisite: message processed", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			var pickedUpQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.PickedUpCode);
			var gateOutQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.GateOutCode);

			var pickedUpQueryLogs = declaration.Logs.Find(pickedUpQuery);
			AssertEquals($"PickedUp event should not be added to {declaration.HumanReadableName}", 0, pickedUpQueryLogs.Length);
			var gateOutLogs = declaration.Logs.Find(gateOutQuery);
			AssertEquals($"GateOut event should be added to {declaration.HumanReadableName}", 1, gateOutLogs.Length);
			var reference = gateOutLogs.First().SL_Reference;
			AssertContains("Event reference has location injected", "LOC=AUSYD", reference);
		}

		public void TestSetGrossWeightFromCombinedWeights_Prevent_JC_GrossWeight_OutOfDecimalRange()
		{
			#region XML message

			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Shipment>
	<DataContext>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>CustomsDeclaration</Type>
		  <Key>B00165869</Key>
		</DataTarget>
	  </DataTargetCollection>
	</DataContext>

	<ConsolidatedCargoStatus>
	  <Code></Code>
	</ConsolidatedCargoStatus>
	<ContainerCount>1</ContainerCount>
	<CustomsContainerMode>
	  <Code>FCL</Code>
	  <Description>Full Container Load</Description>
	</CustomsContainerMode>
	<CustomsOffice>
	  <Code></Code>
	</CustomsOffice>
	<CustomsProfileIdentifier>
	  <Type>UserName</Type>
	  <Value></Value>
	</CustomsProfileIdentifier>
	<CustomsValuationPort>
	  <Code></Code>
	</CustomsValuationPort>
	<DeclarantType>
	  <Code></Code>
	</DeclarantType>
	<DefermentAccountNumber></DefermentAccountNumber>
	<EFTMode>
	  <Code></Code>
	</EFTMode>
	<EntryStatus>
	  <Code></Code>
	</EntryStatus>
	<ExportGoodsType>
	  <Code>OT</Code>
	  <Description>General Consigned Cargo</Description>
	</ExportGoodsType>
	<Folio></Folio>
	<GoodsDescription></GoodsDescription>
	<GoodsOrigin>
	  <Code></Code>
	</GoodsOrigin>
	<IsPersonalEffects>false</IsPersonalEffects>
	<LloydsIMO></LloydsIMO>
	<LocationAtClearance>
	  <Code></Code>
	  <Description></Description>
	</LocationAtClearance>
	<MergeBy>
	  <Code>TRF</Code>
	  <Description>Tariff</Description>
	</MergeBy>
	<MessageStatus>
	  <Code></Code>
	  <Description>Not Sent</Description>
	</MessageStatus>
	<MessageSubType>
	  <Code>FRM</Code>
	  <Description>Formal Entry</Description>
	</MessageSubType>
	<MessageType>
	  <Code>IMP</Code>
	  <Description>Import</Description>
	</MessageType>
	<MessagingApplicationCode>
	  <Code>BLT</Code>
	  <Description>Only allow entry submission directly to Customs Authority</Description>
	</MessagingApplicationCode>
	<OperationalStatus>
	  <Code></Code>
	</OperationalStatus>
	<OuterPacks>0</OuterPacks>
	<OuterPacksPackageType>
	  <Code>PKG</Code>
	  <Description>Package</Description>
	</OuterPacksPackageType>
	<OwnerRef></OwnerRef>
	<PaymentMethod>
	  <Code>DEF</Code>
	  <Description>Default</Description>
	</PaymentMethod>
	<PortOfDestination>
	  <Code></Code>
	</PortOfDestination>
	<PortOfDischarge>
	  <Code></Code>
	</PortOfDischarge>
	<PortOfFirstArrival>
	  <Code></Code>
	</PortOfFirstArrival>
	<PortOfLoading>
	  <Code>DEFRA</Code>
	  <Name>Frankfurt am Main</Name>
	</PortOfLoading>
	<PortOfOrigin>
	  <Code>DEFRA</Code>
	  <Name>Frankfurt am Main</Name>
	</PortOfOrigin>
	<ScreeningStatus>
	  <Code>UNK</Code>
	  <Description>Unknown</Description>
	</ScreeningStatus>
	<ServiceLevel>
	  <Code>STD</Code>
	  <Description>Standard</Description>
	</ServiceLevel>
	<ShipmentIncoTerm>
	  <Code>FOB</Code>
	  <Description>Free On Board</Description>
	</ShipmentIncoTerm>
	<SubLocationAtClearance>
	  <Code></Code>
	  <Description></Description>
	</SubLocationAtClearance>
	<TotalNoOfPacksDecimal>0.0000</TotalNoOfPacksDecimal>
	<TotalNoOfPieces>0</TotalNoOfPieces>
	<TotalVolume>0.000</TotalVolume>
	<TotalVolumeUnit>
	  <Code>M3</Code>
	  <Description>Cubic Meters</Description>
	</TotalVolumeUnit>
	<TotalWeight>0.000</TotalWeight>
	<TotalWeightUnit>
	  <Code>KG</Code>
	  <Description>Kilograms</Description>
	</TotalWeightUnit>
	<TransportMode>
	  <Code>SEA</Code>
	  <Description>Sea Freight</Description>
	</TransportMode>
	<TransportNationality>
	  <Code></Code>
	</TransportNationality>
	<VesselName></VesselName>
	<VoyageFlightNo></VoyageFlightNo>
	<WarehouseReleaseStatus>
	  <Code></Code>
	</WarehouseReleaseStatus>

	<ContainerCollection Content=""Partial"">
	  <Container>
		<ContainerCount>1</ContainerCount>
		<ContainerNumber>X00001</ContainerNumber>
		<ContainerQuality>
		  <Code></Code>
		</ContainerQuality>
		<ContainerStatus>
		  <Code></Code>
		</ContainerStatus>
		<ContainerType>
		  <Code>20FR</Code>
		  <Category>
			<Code>FLT</Code>
			<Description>Flat Rack</Description>
		  </Category>
		  <Description>Twenty foot flatrack</Description>
		  <ISOCode>22P1</ISOCode>
		</ContainerType>
		<GoodsValue>0.0000</GoodsValue>
		<GoodsValueCurrency>
		  <Code></Code>
		</GoodsValueCurrency>
		<GoodsWeight>999999.000</GoodsWeight>
		<GrossWeightVerificationType>
		  <Code>NON</Code>
		  <Description>Not Verified</Description>
		</GrossWeightVerificationType>
		<VolumeCapacity>0.000</VolumeCapacity>
		<VolumeUnit>
		  <Code>M3</Code>
		  <Description>Cubic Meters</Description>
		</VolumeUnit>
		<WeightCapacity>0.000</WeightCapacity>
		<WeightUnit>
		  <Code>KG</Code>
		  <Description>Kilograms</Description>
		</WeightUnit>
	  </Container>
	</ContainerCollection>
  </Shipment>
</UniversalShipment>";

			#endregion

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DeclarationReference = "B00165869";
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "X00001";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(xmlMessage);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			ErrorReporter.Clear();

			manager.Process(message);
			Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			AssertEquals("Prerequisite: message processed", EDIMessageStatusList.Codes.Warning, message.EM_Status);
			AssertContains("JC_GrossWeight truncated message", "Warning - Attempted to insert '1002949.000' into Field [JC_GrossWeight] which has a maximum numeric value of '999999'. Field was truncated to the max value.",
				manager.Logger.ToString());

			var newFactory = new BusinessObjectFactory();
			var newForwardingContainer = newFactory.Load<ForwardingContainer>(container.JobContainer.PK);
			AssertNotNull(newForwardingContainer);
			AssertEquals("JC_GrossWeight should not be out of decimal(9,3) and is set to 999999.", 999999m, newForwardingContainer.JC_GrossWeight);

			ErrorReporter.Clear();
		}

		public void TestExistingContainerModeInDeclarationIsNotOverridden()
		{
			#region XML message
			const string xmlMessage = @"<UniversalShipment version=""1.0"" xmlns =""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Shipment>
<DataContext>
<DataTargetCollection>
<DataTarget>
<Type>CustomsDeclaration</Type>
<Key>B00182543</Key>
</DataTarget>
</DataTargetCollection>
</DataContext>
<ContainerCollection Content = ""Partial"">
<Container>
<ContainerNumber>HASU1037546</ContainerNumber>
<ArrivalCartageAdvised>2020-10-20T16:45:16</ArrivalCartageAdvised>
<FCLAvailable>2020-11-03T09:00:00</FCLAvailable>
<FCLWharfGateOut>2020-11-03T21:01:00</FCLWharfGateOut>
<ArrivalCartageComplete>2020-11-08T10:17:00</ArrivalCartageComplete>
<ContainerParkEmptyReturnGateIn>2020-11-08T21:37:00</ContainerParkEmptyReturnGateIn>
<EmptyReadyForReturn>2020-11-08T10:17:00</EmptyReadyForReturn>
<EmptyReturnedBy>2020-11-21T23:00:00</EmptyReturnedBy>
</Container>
</ContainerCollection>
</Shipment>
</UniversalShipment>";
			#endregion

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DeclarationReference = "B00182543";

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "HASU1037546";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(xmlMessage);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("Prerequisite: message processed", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			var newFactory = new BusinessObjectFactory();
			var declarationInNewFactory = newFactory.Load<BaseJobDeclaration>(declaration.PK);
			AssertNotNull(declarationInNewFactory);
			AssertEquals("JE_ContainerMode in an existing dec should not be overidden.", "FCL", declarationInNewFactory.JE_ContainerMode);
		}

		public void TestOnLogParentFoundFromEDIMessageForUSeBondMessageToSuretyAgent()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				string incomingEvent = @"<UniversalEvent>
	  <Event>
		<DataContext>
		  <DataTargetCollection>
			<DataTarget>
			  <Key>B00158390</Key>
			  <Type>CustomsDeclaration</Type>
			</DataTarget>
		  </DataTargetCollection>
		</DataContext>
		<EventTime>2014-09-09T09:30:10</EventTime>
		<EventType>IRJ</EventType>
		<EventParameters>
		  <Reason>You are not registered with eHub. Contact WTG to register.</Reason>
		  <MessageType>eBond Message to Surety Agent</MessageType>
		</EventParameters>
	  </Event>
</UniversalEvent>";

				var groupZZ1 = Factory.New<GlbGroup>();
				groupZZ1.GG_Code = "ZZ1";
				var staffZ1 = groupZZ1.Staff.AddNew();
				staffZ1.GS_Code = "Z1";
				staffZ1.GS_LoginName = "z1";
				staffZ1.GS_EmailAddress = "dong@pretend.email.com";
				var uSCustomsRegistry = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_USCustomsDataRegistry");
				var bondStatusNotificationGroup = (GuidRegistryItem)uSCustomsRegistry.FindByName("BondStatusNotificationGroup");
				bondStatusNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, groupZZ1.PK.ToGuid());

				var eventDeserializer = new XmlEventDeserializer();
				var xmlEvent = eventDeserializer.Parse(incomingEvent);
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration.JE_DeclarationReference = "B00158390";

				var message = GetQueuedUniversalShipmentMessage(incomingEvent);
				var logger = new TestErrorLogger();
				var manager = new JobDeclarationDataContextManager();
				manager.OnLogParentFoundFromEDIMessage(logger, xmlEvent, message, declaration);

				var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Body.Contains("B00158390"));
				AssertNotNull(email);
			}
		}

		public void TestOnLogParentFoundFromEDIMessageForCNCSWResponseMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var incomingEvent = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataProvider>CSW</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key></Key>
					<Type>CustomsDeclaration</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2019-4-16T20:17:41</EventTime>
		<EventType>MDL</EventType>
		<EventReference>MST=SW</EventReference>
		<ContextCollection>
			<Context>
				<Type>MessageReferenceNumber</Type>
				<Value>000000000000194233</Value>
			</Context>
			<Context>
				<Type>ResponseCode</Type>
				<Value>0</Value>
			</Context>
			<Context>
				<Type>ResponseDetail</Type>
				<Value>暂存成功</Value>
			</Context>
			<Context>
				<Type>DeclarationUnifiedNumber</Type>
				<Value>I20180000144486227</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

				var message = GetQueuedUniversalShipmentMessage(incomingEvent);
				var logger = new TestErrorLogger();
				var manager = new JobDeclarationDataContextManager();
				var eventDeserializer = new XmlEventDeserializer();
				var xmlEvent = eventDeserializer.Parse(incomingEvent);
				var entryHeader = Factory.New<CusEntryHeader>();
				manager.OnLogParentFoundFromEDIMessage(logger, xmlEvent, message, entryHeader);

				AssertEquals("The message has been updated.", entryHeader.PK, message.EM_LinkUniqueID);
			}
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => new RecipientRoleType[]
		{
			RecipientRoleType.BRO,
			RecipientRoleType.BRI,
			RecipientRoleType.BRE
		};

		protected override void SetUp()
		{
			setupCreator = ((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
			base.SetUp();

			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Interfaced;
			localCountryCustomsInterface = CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customsInterface);
		}
		IDisposable setupCreator;
		IDisposable localCountryCustomsInterface;

		TestFileHelper TestFileHelper => testFileHelper ??= new ();
		TestFileHelper testFileHelper;

		protected override void TearDown()
		{
			base.TearDown();
			if (setupCreator != null)
			{
				setupCreator.Dispose();
				setupCreator = null;
			}
			localCountryCustomsInterface?.Dispose();
			testFileHelper?.Dispose();
			testFileHelper = null;
		}

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get
			{
				return @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <Shipment>
	<DataContext>
	  <DataSourceCollection>
		<DataSource>
		  <Type>CustomsDeclaration</Type>
		</DataSource>
	  </DataSourceCollection>
	</DataContext>

	<CustomsContainerMode>
	  <Code>CNT</Code>
	  <Description>Containerized</Description>
	</CustomsContainerMode>
	<GoodsDescription>HATS</GoodsDescription>
	<GoodsValue>22.33</GoodsValue>
	<GoodsValueCurrency>
	  <Code>AUD</Code>
	</GoodsValueCurrency>
	<IsForwardRegistered>true</IsForwardRegistered>
	<PortOfDestination>
	  <Code>AUBNE</Code>
	  <Name>Brisbane</Name>
	</PortOfDestination>
	<PortOfOrigin>
	  <Code>KRSEL</Code>
	  <Name>Seoul</Name>
	</PortOfOrigin>
	<ShipmentIncoTerm>
	  <Code>FOB</Code>
	  <Description>Free On Board</Description>
	</ShipmentIncoTerm>
	<TransportMode>
	  <Code>SEA</Code>
	  <Description>Sea Freight</Description>
	</TransportMode>
	<WayBillNumber>I_DO_NOT_EXIST</WayBillNumber>
	<WayBillType>
	  <Code>MWB</Code>
	  <Description>Master Waybill</Description>
	</WayBillType>

	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>SupplierDocumentaryAddress</AddressType>
		<CompanyName>VAPOUR CORPORATION</CompanyName>
		<Address1>UNIT 0, -1 FANTASY LANE</Address1>
		<AddressOverride>false</AddressOverride>
		<City>FAKE HILL</City>
		<Country>
		  <Code>AU</Code>
		  <Name>Australia</Name>
		</Country>
		<Postcode>2987</Postcode>
		<State>NSW</State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ImporterDocumentaryAddress</AddressType>
		<CompanyName>TERRY TOWELLERS INC</CompanyName>
		<Address1>238 APTITUDE PLAZA</Address1>
		<Address2>FANTASY VALLEY BUSINESS CENTRE</Address2>
		<AddressOverride>false</AddressOverride>
		<City>FANTASY VALLEY</City>
		<Country>
		  <Code>AU</Code>
		  <Name>Australia</Name>
		</Country>
		<Port>
		  <Code>AUBNE</Code>
		  <Name>Brisbane</Name>
		</Port>
		<Postcode>4006</Postcode>
		<State>QLD</State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>Forwarder</AddressType>
		<CompanyName>VAPOUR CORPORATION</CompanyName>
		<Address1>UNIT 0, -1 FANTASY LANE</Address1>
		<AddressOverride>false</AddressOverride>
		<City>FAKE HILL</City>
		<Country>
		  <Code>AU</Code>
		  <Name>Australia</Name>
		</Country>
		<Postcode>2987</Postcode>
		<State>NSW</State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>NotifyParty</AddressType>
		<CompanyName>TERRY TOWELLERS INC</CompanyName>
		<Address1>238 APTITUDE PLAZA</Address1>
		<Address2>FANTASY VALLEY BUSINESS CENTRE</Address2>
		<AddressOverride>false</AddressOverride>
		<City>FANTASY VALLEY</City>
		<Country>
		  <Code>AU</Code>
		  <Name>Australia</Name>
		</Country>
		<Port>
		  <Code>AUBNE</Code>
		  <Name>Brisbane</Name>
		</Port>
		<Postcode>4006</Postcode>
		<State>QLD</State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ShippingLine</AddressType>
		<CompanyName>FLOGGED OGGIN LTD</CompanyName>
		<Address1>556 WORN OUT ALLEY</Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<City>NIGHTMAREIA</City>
		<Country>
		  <Code>AU</Code>
		  <Name>Australia</Name>
		</Country>
		<Port>
		  <Code>AUPER</Code>
		  <Name>Perth</Name>
		</Port>
		<Postcode>7007</Postcode>
		<State>WA</State>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>
";
			}
		}

		void AssertProcessContainerEvent(CommonContainer container, string xmlName, (string eventCode, string facility, string location, ZPropertyInfo propertyInfo)[] eventMappings)
		{
			var logs = container.Logs;

			var xml = File.ReadAllText(TestFileHelper.GetPathForUniversalTestFiles(xmlName));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			CombineAssertions(() =>
			{
				var eventTime = ZDateTime.Now;
				var propertyInfos = eventMappings.Where(c => c.propertyInfo != null).Select(c => c.propertyInfo);

				foreach (var mapping in eventMappings)
				{
					var eventCode = mapping.eventCode;

					eventTime = eventTime.AddDays(1);
					serviceTaskLog.ClearLogs();
					propertyInfos.ForEach(c => c.ClearValue());

					var messageText = xml
						.Replace(@"<EventType>###</EventType>", $@"<EventType>{eventCode}</EventType>")
						.Replace(@"<EventTime>###</EventTime>", $@"<EventTime>{eventTime.ToString()}</EventTime>")
						.Replace(@"<Facility>###</Facility>", $@"<Facility>{mapping.facility}</Facility>")
						.Replace(@"<Location>###</Location>", $@"<Location>{mapping.location}</Location>");

					var message = GetQueuedUniversalEventMessage(messageText);
					manager.Process(message);

					var log = logs.Find(c => c.SL_SE_NKEvent == eventCode && !c.SL_IsCancelled).First();

					AssertEquals($"{eventCode} - Message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
					AssertEquals($"{eventCode} - Should set the expected event time on new log.", eventTime.ToString(), log.SL_EventTime.ToString());
					AssertMultilineASCIIEquals($"{eventCode} -Service Task Log", "Linked Event to Container 'TCLU3697767'.", serviceTaskLog.ToString().Trim());

					var propertyInfo = mapping.propertyInfo;
					if (propertyInfo != null)
					{
						AssertEquals($"{eventCode} - Should set the value on {propertyInfo.Name}.", eventTime.ToString(), propertyInfo.Value.ToString());
					}
				}
			});
		}

		const string IncomingEventForDFC = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns = ""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Event>
	<DataContext>
	  <DataSource>
		<DataProvider Type=""EnterpriseID"">EDIDATFJC</DataProvider>
				<Key>CM00000004</Key>
				<Type>TransportBookingConsolidation</Type>
			</DataSource>

			<Workflow>
				<Company>
					<Code>FJC</Code>
					<Country Name=""Fiji"">FJ</Country>
		  <Name>Fiji company</Name>
		</Company>
		<EventBranch Name=""Fiji branch"">FJB</EventBranch>
				<EventDepartment Name=""Branch"">BRN</EventDepartment>
				<EventType></EventType>

				<EventUser Name=""CargoWise One Service"">~BP</EventUser>
				<TriggerCount>1</TriggerCount>
				<TriggerDate>2017-03-16T07:50:39.4</TriggerDate>
		<TriggerDescription></TriggerDescription>
		<TriggerType>Manual</TriggerType>
	  </Workflow>

	  <DataTargetCollection>
		<DataTarget>
		  <Key>B00001024</Key>
		  <Type>CustomsDeclaration</Type>
		</DataTarget>
	  </DataTargetCollection>
	</DataContext>

	<EventTime>2017-02-28T06:30:00</EventTime>
	<EventType>DCF</EventType>
	<CreatedTime>2017-03-15T19:50:39.343</CreatedTime>
	<EventReference>LUCY SELTH</EventReference>
	<IsEstimate>false</IsEstimate>

	<ContextCollection>
	</ContextCollection>
  </Event>
</UniversalEvent>";

		const string IncomingEventForDFC2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns = ""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Event>
	<DataContext>
	  <DataSource>
		<DataProvider Type=""EnterpriseID"">EDIDATFJC</DataProvider>
				<Key>CM00000004</Key>
				<Type>TransportBookingConsolidation</Type>
			</DataSource>

			<Workflow>
				<Company>
					<Code>FJC</Code>
					<Country Name=""Fiji"">FJ</Country>
		  <Name>Fiji company</Name>
		</Company>
		<EventBranch Name=""Fiji branch"">FJB</EventBranch>
				<EventDepartment Name=""Branch"">BRN</EventDepartment>
				<EventType></EventType>

				<EventUser Name=""CargoWise One Service"">~BP</EventUser>
				<TriggerCount>1</TriggerCount>
				<TriggerDate>2017-03-16T07:50:39.4</TriggerDate>
		<TriggerDescription></TriggerDescription>
		<TriggerType>Manual</TriggerType>
	  </Workflow>

	  <DataTargetCollection>
		<DataTarget>
		  <Key>B00001021</Key>
		  <Type>CustomsDeclaration</Type>
		</DataTarget>
	  </DataTargetCollection>
	</DataContext>

	<EventTime>2017-02-28T06:30:00</EventTime>
	<EventType>DCF</EventType>
	<CreatedTime>2017-03-15T19:50:39.343</CreatedTime>
	<EventReference>LUCY SELTH</EventReference>
	<IsEstimate>false</IsEstimate>

	<ContextCollection>
	</ContextCollection>
  </Event>
</UniversalEvent>";
	}
}
