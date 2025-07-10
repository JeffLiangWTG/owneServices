
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	[TestedType(typeof(WhsTransitReceiveConsignmentDataContextManager))]
	sealed class WhsTransitReceiveConsignmentDataContextManagerTest : WhsTransitConsignmentDataContextManagerTest<WhsTransitReceiveConsignmentDataContextManager, WhsItemReceiveConsignment>
	{
		public void Test_Import_TransitReceive_DataTarget_WithMatchingKey_ShouldUpdatesRCN()
		{
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK, "RC00000001");
			AssertEquals(0, receiveConsignment.PackageStates.Count);

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(TransitReceiveWithKey);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var newFactory = new UniversalObjectFactory();
			var rcn = newFactory.BOFactory.LoadTop1<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_JobID, "RC00000001"));
			var packageImported = rcn.PackageStates.Single();
			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status should be warning as no matching transit warehouse found.", EDIMessageStatusList.Codes.Warning, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"Updated Receive Consignment RC00000001 from UniversalShipment.
Successfully saved Receive Consignment RC00000001 with 1 x PkgPackageJob, 1 x PkgPackage, 1 x WhsItemPackageState.".Trim(), serviceTaskLog.ToString());
				AssertEquals(TransitWarehouseStatuses.Codes.Booked, packageImported.WPS_Status);
				AssertEquals("PLT", packageImported.Package.KP_F3_NKPackType);
				AssertEquals("PACKAGEIMPORTED", packageImported.Package.KP_PackageID);
			});
		}

		public void Test_Import_TransitReceive_DataTarget_WithNonMatchingKey_ShouldDiscardsMessage()
		{
			Helper.CreateReceiveConsignment("RC00000001", "STD", warehouse.PK, "RC00000010");

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(TransitReceiveWithKey);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);
			AssertMultilineASCIIEquals("Service Task Log", @"ERROR - Match couldn't be found for TransitReceive with Key RC00000001
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.".Trim(), serviceTaskLog.ToString());
		}

		public void Test_Import_TransitReceiveConsignment_NoDataTargetKey_WayBillNumber()
		{
			Data.SetupForForwardingImport();
			var transitReceiveWithWayBillNumber = resourceRetriever.Value.GetString("Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing.Universal.TestFiles.TransitReceive - DataTarget No Key With WayBillNumber.xml");
			var message = GetQueuedUniversalShipmentMessage(transitReceiveWithWayBillNumber);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("message.EM_Status should be error as booking party is missing.", EDIMessageStatusList.Codes.Error, message.EM_Status);
			AssertMultilineASCIIEquals(
@"Added Receive Consignment RC00000001 from UniversalShipment.
Successfully saved Receive Consignment RC00000001 with 1 x PkgPackageJob, 1 x PkgPackage, 1 x WhsItemPackageState.".Trim(), serviceTaskLog.ToString());

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcn = factory.Load<WhsItemReceiveConsignment>(new ZQuery()).Single();
			AssertEquals(Data.Orgs.INTHEMSYD.PK, rcn.ConsignorDocAddress.Organisation.PK);
			AssertEquals(Data.Orgs.CRAHOLSYD.PK, rcn.ConsigneeDocAddress.Organisation.PK);

			var messageForReImport = GetQueuedUniversalShipmentMessage(transitReceiveWithWayBillNumber);
			var serviceTaskLogForReImport = new ServiceTaskLogForTesting();
			var managerForReImport = new UniversalMessageProcessingManager(serviceTaskLogForReImport);
			managerForReImport.Process(messageForReImport);
			AssertEquals("message.EM_Status should be error as booking party is missing.", EDIMessageStatusList.Codes.Error, messageForReImport.EM_Status);
			AssertMultilineASCIIEquals(
@"Updated Receive Consignment RC00000001 from UniversalShipment.
Successfully saved Receive Consignment RC00000001 with 1 x PkgPackageJob, 1 x PkgPackage, 1 x WhsItemPackageState.".Trim(), serviceTaskLogForReImport.ToString());

			var factoryAfterReImport = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcnAfterReImport = factoryAfterReImport.Load<WhsItemReceiveConsignment>(new ZQuery()).Single();
			AssertEquals(Data.Orgs.INTHEMSYD.PK, rcnAfterReImport.ConsignorDocAddress.Organisation.PK);
			AssertEquals(Data.Orgs.CRAHOLSYD.PK, rcnAfterReImport.ConsigneeDocAddress.Organisation.PK);
		}

		public void TestEventContextValues_AIR()
		{
			var warehouse1 = CreateWarehouse("W1");
			var warehouse2 = CreateWarehouse("W2");

			var consignmentWithAll = Factory.New<WhsItemReceiveConsignment>();
			var consignmentWithMasterBill = Factory.New<WhsItemReceiveConsignment>();
			var consignmentWithHouseBill = Factory.New<WhsItemReceiveConsignment>();
			var consignmentWithWarehouse = Factory.New<WhsItemReceiveConsignment>();
			SetJobID(consignmentWithAll, "ALL");
			SetJobID(consignmentWithMasterBill, "WithMAB");
			SetJobID(consignmentWithHouseBill, "WithHSB");
			SetJobID(consignmentWithWarehouse, "WithWHSOnly");

			consignmentWithAll.WRC_HouseBillNumber = "HSB1";
			consignmentWithHouseBill.WRC_HouseBillNumber = "HSBOnly";

			PopulateAdditionalReference(consignmentWithAll.PK, consignmentWithAll.TablePrefix, TransportAdditionalReferenceTypes.Codes.MasterBill, "MAB1");
			PopulateAdditionalReference(consignmentWithMasterBill.PK, consignmentWithMasterBill.TablePrefix, TransportAdditionalReferenceTypes.Codes.MasterBill, "MABOnly");

			consignmentWithAll.WRC_TransportMode = TransportModes.Air;
			consignmentWithMasterBill.WRC_TransportMode = TransportModes.Air;
			consignmentWithHouseBill.WRC_TransportMode = TransportModes.Air;
			consignmentWithWarehouse.WRC_TransportMode = TransportModes.Air;

			SetWarehouse(consignmentWithAll, warehouse1);
			SetWarehouse(consignmentWithMasterBill, warehouse2);
			SetWarehouse(consignmentWithHouseBill, warehouse2);
			SetWarehouse(consignmentWithWarehouse, warehouse2);

			AssertEventContextValues_AIR(consignmentWithAll, "W1", "HSB1", "MAB1", "ALL");
			AssertEventContextValues_AIR(consignmentWithMasterBill, "W2", "", "MABOnly", "WithMAB");
			AssertEventContextValues_AIR(consignmentWithHouseBill, "W2", "HSBOnly", "", "WithHSB");
			AssertEventContextValues_AIR(consignmentWithWarehouse, "W2", "", "", "WithWHSOnly");
		}

		public void Test_Import_TransitReceiveConsignment_PremiseID()
		{
			Data.SetupForForwardingImport();
			WhsTransitTestHelper.SetPremiseIDForWarehouse(Data.Warehouse, "9914N", CountryCodes.Australia);

			var message = GetQueuedUniversalShipmentMessage(resourceRetriever.Value.GetString("Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing.Universal.TestFiles.TransitReceive - PremiseId.xml"));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var newFactory = new UniversalObjectFactory();
			var rcns = newFactory.BOFactory.Load<WhsItemReceiveConsignment>(new ZQuery());
			var rcn1 = rcns.Where(r => r.WRC_JobID == "RC00000001").Single();
			var rcn2 = rcns.Where(r => r.WRC_JobID == "RC00000002").Single();
			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status should be error as booking party is missing.", EDIMessageStatusList.Codes.Error, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"Added Receive Consignment RC00000001 from UniversalShipment.
Added Receive Consignment RC00000002 from UniversalShipment.
Added Receive ASN TRT00000001 from UniversalShipment.
Updated record from UniversalShipment.
Successfully saved record with 5 x CusEntryNumber, 2 x PkgPackageJob, 2 x WhsItemPackageState, 2 x WhsItemReceiveConsignment, 1 x WhsItemReceiveASN.".Trim(), serviceTaskLog.ToString());
				AssertEquals("2 receive consignments should have been created for two house bills.", 2, rcns.Length);
				AssertEquals("Warehouse should be picked with PremiseID.", Data.Warehouse.PK, rcn1.Warehouse.PK);
				AssertEquals("Warehouse should be picked with PremiseID.", Data.Warehouse.PK, rcn2.Warehouse.PK);
			});
		}

		protected override void TestDataContextKeyCore()
		{
			var consignment = Factory.New<WhsItemReceiveConsignment>();
			consignment.WRC_JobID = "JOBID";
			AssertEquals("DataContextKey should be returning Job ID.", "JOBID", consignment.GetUniversalDataContextManager().DataContextKey);
		}

		protected override DataContextType ExpectedDataContextType => DataContextType.TransitReceive;

		protected override ServiceCodeType?[] SupportedRecipientServices(RecipientRoleType recipientRole)
		{
			return SupportedRecipientRoleTypes.Contains(recipientRole) ? new ServiceCodeType?[] { ServiceCodeType.TWR, ServiceCodeType.TWX } : base.SupportedRecipientServices(recipientRole);
		}

		protected override void SetWarehouse(WhsItemReceiveConsignment consignment, WhsWarehouse warehouse)
		{
			consignment.WRC_WW_IntendedWarehouse = warehouse.PK;
		}

		protected override void SetJobID(WhsItemReceiveConsignment consignment, string jobID)
		{
			consignment.WRC_JobID = jobID;
		}

		protected override void SetHouseBillNumber(WhsItemReceiveConsignment consignment, string houseBillNumber)
		{
			consignment.WRC_HouseBillNumber = houseBillNumber;
		}

		string TransitReceiveWithKey => resourceRetriever.Value.GetString("Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing.Universal.TestFiles.TransitReceive - DataTarget With Key.xml");

		WhsTransitTestHelper Helper => new WhsTransitTestHelper(Factory.BOFactory);

		WhsWarehouse warehouse => Data.Warehouse;
	}
}
