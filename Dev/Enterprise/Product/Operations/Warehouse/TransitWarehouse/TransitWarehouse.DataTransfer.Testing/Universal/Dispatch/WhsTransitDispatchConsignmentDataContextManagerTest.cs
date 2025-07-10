using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
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
	[TestedType(typeof(WhsTransitDispatchConsignmentDataContextManager))]
	sealed class WhsTransitDispatchConsignmentDataContextManagerTest : WhsTransitConsignmentDataContextManagerTest<WhsTransitDispatchConsignmentDataContextManager, WhsItemDispatchConsignment>
	{
		public void Test_Import_TransitDispatch_DataTarget_WithMatchingKey_ShouldUpdateDCN()
		{
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN", "STD", Warehouse.PK);
			var packageState = Helper.CreatePackageState(receiveConsignment, 1, "PLT", "PACKAGEIMPORTED", TransitWarehouseStatuses.Codes.Booked);
			var reference = Helper.CreateAdditionalReference(receiveConsignment, "PACKAGEIMPORTED", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);

			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN", Warehouse.PK, "STD", "DC00000001");
			dispatchConsignment.WDC_HouseBillNumber = "HSB01";
			AssertEquals(0, dispatchConsignment.PackageStates.Count);

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(TransitDispatchWithKey);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var newFactory = new UniversalObjectFactory();
			var dcn = newFactory.BOFactory.LoadTop1<WhsItemDispatchConsignment>(new ZQuery(WhsItemDispatchConsignmentSchema.WDC_JobID, "DC00000001"));
			AssertEquals("HOUSEBILL", dcn.HouseBillNumber);

			var packageImported = dcn.PackageStates.Single();
			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status should be error as booking party is missing.", EDIMessageStatusList.Codes.Error, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"Updated Dispatch Consignment DC00000001 from UniversalShipment.
Successfully saved Dispatch Consignment DC00000001 with 1 x WhsItemPackageState.".Trim(), serviceTaskLog.ToString());
				AssertEquals(TransitWarehouseStatuses.Codes.Booked, packageImported.WPS_Status);
				AssertEquals("PLT", packageImported.Package.KP_F3_NKPackType);
				AssertEquals("PACKAGEIMPORTED", packageImported.Package.KP_PackageID);
			});
		}

		public void Test_Import_TransitDispatch_DataTarget_WithNonMatchingKey_ShouldDiscardMessage()
		{
			var dispatchConsignment = Helper.CreateDispatchConsignment("DC00000001", Warehouse.PK, "STD", "DC00000010");

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(TransitDispatchWithKey);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);
			AssertMultilineASCIIEquals("Service Task Log", @"ERROR - Match couldn't be found for TransitDispatch with Key DC00000001
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.".Trim(), serviceTaskLog.ToString());
		}

		public void TestEventContextValues_AIR()
		{
			var warehouse1 = CreateWarehouse("W1");
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse1.PK);
			var receive = Helper.CreateReceiveConsignment("RCN1", warehouse1.PK);

			var consignmentWithAll = Helper.CreateDispatchConsignment("DCN1", warehouse1.PK, "STD", "ALL", transportMode: TransportModes.Air);
			var consignmentWithMasterBill = Helper.CreateDispatchConsignment("DCN2", warehouse1.PK, "STD", "WithMAB", transportMode: TransportModes.Air);
			var consignmentWithHouseBill = Helper.CreateDispatchConsignment("DCN3", warehouse1.PK, "STD", "WithHSB", transportMode: TransportModes.Air);
			var consignmentWithWarehouse = Helper.CreateDispatchConsignment("DCN4", warehouse1.PK, "STD", "WithWHSOnly", transportMode: TransportModes.Air);

			consignmentWithAll.WDC_HouseBillNumber = "HSB1";
			consignmentWithHouseBill.WDC_HouseBillNumber = "HSBOnly";

			PopulateAdditionalReference(consignmentWithAll.PK, consignmentWithAll.TableName, TransportAdditionalReferenceTypes.Codes.MasterBill, "MAB1");
			PopulateAdditionalReference(consignmentWithMasterBill.PK, consignmentWithMasterBill.TableName, TransportAdditionalReferenceTypes.Codes.MasterBill, "MABOnly");

			Helper.CreatePackageState(receive, 1, "PLT", "PKG1", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: consignmentWithAll, dispatchLoadList: dll);
			Helper.CreatePackageState(receive, 1, "PLT", "PKG2", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: consignmentWithMasterBill, dispatchLoadList: dll);
			Helper.CreatePackageState(receive, 1, "PLT", "PKG3", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: consignmentWithHouseBill, dispatchLoadList: dll);
			Helper.CreatePackageState(receive, 1, "PLT", "PKG4", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: consignmentWithWarehouse, dispatchLoadList: dll);

			Factory.SaveForTesting();

			AssertEventContextValues_AIR(consignmentWithAll, "W1", "HSB1", "MAB1", "ALL");
			AssertEventContextValues_AIR(consignmentWithMasterBill, "W1", "", "MABOnly", "WithMAB");
			AssertEventContextValues_AIR(consignmentWithHouseBill, "W1", "HSBOnly", "", "WithHSB");
			AssertEventContextValues_AIR(consignmentWithWarehouse, "W1", "", "", "WithWHSOnly");
		}

		protected override void TestDataContextKeyCore()
		{
			var consignment = Factory.New<WhsItemDispatchConsignment>();
			consignment.WDC_JobID = "JOBID";
			AssertEquals("DataContextKey should be returning Job ID.", "JOBID", consignment.GetUniversalDataContextManager().DataContextKey);
		}

		protected override DataContextType ExpectedDataContextType => DataContextType.TransitDispatch;

		protected override void SetupDataForDataContextManagerTestCase()
		{
			base.SetupDataForDataContextManagerTestCase();

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var logger = new DummyLogger();
			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(ValidPopulatedUniversalShipmentXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
			}

			Data.CreateReceiveConsignmentInDB(shipment);
		}

		protected override ServiceCodeType?[] SupportedRecipientServices(RecipientRoleType recipientRole)
		{
			return SupportedRecipientRoleTypes.Contains(recipientRole) ? new ServiceCodeType?[] { ServiceCodeType.TWD, ServiceCodeType.TWP } : base.SupportedRecipientServices(recipientRole);
		}

		protected override void SetWarehouse(WhsItemDispatchConsignment consignment, WhsWarehouse warehouse)
		{
			consignment.WDC_WW_Warehouse = warehouse.PK;
		}

		protected override void SetJobID(WhsItemDispatchConsignment consignment, string jobID)
		{
			consignment.WDC_JobID = jobID;
		}

		protected override void SetHouseBillNumber(WhsItemDispatchConsignment consignment, string houseBillNumber)
		{
			consignment.WDC_HouseBillNumber = houseBillNumber;
		}

		string TransitDispatchWithKey => resourceRetriever.Value.GetString("Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing.Universal.TestFiles.TransitDispatch - DataTarget With Key.xml");

		WhsTransitTestHelper Helper => new WhsTransitTestHelper(Factory.BOFactory);

		WhsWarehouse Warehouse => Data.Warehouse;
	}
}
