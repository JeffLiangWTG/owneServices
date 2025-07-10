using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	[TestedType(typeof(WhsTransitDispatchConsolDataContextManager))]
	sealed class WhsTransitDispatchConsolDataContextManagerTest : WhsTransitConsignmentConsolDataContextManagerTest<WhsTransitDispatchConsolDataContextManager, WhsTransitDispatchConsol>
	{
		#region TestImportingConsol_ReImportDLL

		public void TestImport_TransitDispatchConsol_ReImportDLL()
		{
			Data.SetupForForwardingImport();

			AssertEquals(0, Factory.BOFactory.Load<WhsItemDispatchConsignment>(new ZQuery()).Length);
			AssertEquals(0, Factory.BOFactory.Load<WhsItemDispatchLoadList>(new ZQuery()).Length);
			AssertEquals(0, Factory.BOFactory.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Length);
			AssertEquals(0, Factory.BOFactory.Load<WhsItemPackageState>(new ZQuery()).Length);

			var transitDispatchConsolWithoutContainer = resourceRetriever.Value.GetString("Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing.Universal.TestFiles.TransitDispatchConsol - DLL With No Container.xml");
			var message = GetQueuedUniversalShipmentMessage(transitDispatchConsolWithoutContainer);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var newFactory = new UniversalObjectFactory();
			var dll = newFactory.BOFactory.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals("No WhsItemDispatchTransportationUnit should be created.", 0, dll.DispatchTransportationUnits.Count);

			var transitDispatchConsolWithContainer = resourceRetriever.Value.GetString("Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing.Universal.TestFiles.TransitDispatchConsol - DLL With Container.xml");
			var resendMessage = GetQueuedUniversalShipmentMessage(transitDispatchConsolWithContainer);
			var serviceTaskLogResend = new ServiceTaskLogForTesting();
			var managerResend = new UniversalMessageProcessingManager(serviceTaskLogResend);
			managerResend.Process(resendMessage);

			var newFactoryResend = new UniversalObjectFactory();
			var dcn = newFactoryResend.BOFactory.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			AssertEquals("WDC_ConsignmentID should be TEST2.", "TEST2", dcn.WDC_ConsignmentID);

			var dllAfterResend = newFactoryResend.BOFactory.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals("Existing dll should be updated.", dll.PK, dllAfterResend.PK);
			AssertEquals("WhsItemDispatchTransportationUnit should be created.", 1, dllAfterResend.DispatchTransportationUnits.Count);
		}

		#endregion

		public void TestImport_TransitDispatchConsol_NoDataTargetKey_For_DTW()
		{
			Data.SetupForForwardingImport();

			AssertEquals(0, Factory.BOFactory.Load<WhsItemDispatchConsignment>(new ZQuery()).Length);
			AssertEquals(0, Factory.BOFactory.Load<WhsItemDispatchLoadList>(new ZQuery()).Length);
			AssertEquals(0, Factory.BOFactory.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Length);
			AssertEquals(0, Factory.BOFactory.Load<WhsItemPackageState>(new ZQuery()).Length);

			var transitDispatchConsolRecipientDTW = resourceRetriever.Value.GetString("Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing.Universal.TestFiles.TransitDispatchConsol - DataTarget No Key - Recipient DTW.xml");
			var message = GetQueuedUniversalShipmentMessage(transitDispatchConsolRecipientDTW);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var newFactory = new UniversalObjectFactory();
			var dcn = newFactory.BOFactory.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			AssertEquals("TEST2", dcn.WDC_ConsignmentID);
			AssertEquals("TEST2", dcn.WDC_HouseBillNumber);

			var dll = newFactory.BOFactory.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals(1, dll.DispatchTransportationUnits.Count);
			AssertEquals("MABTEST2", dll.MasterBillNumber);

			var dtu = newFactory.BOFactory.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Single();
			AssertEquals("LD3TEST", dtu.WDH_VehicleReference);
			AssertEquals("AUSASI", dtu.TransportCompany.Organisation.OH_Code);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status shoul be error as booking party is missing.", EDIMessageStatusList.Codes.Error, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log",
@"Added Dispatch Consignment DC00000001 from UniversalShipment.
Added Dispatch Load List DLL00000001 from UniversalShipment.
Updated record from UniversalShipment.
Successfully saved record with 1 x WhsItemDispatchConsignment, 1 x CusEntryNumber, 1 x WhsItemDispatchLoadList, 1 x PkgPackage, 1 x WhsItemDispatchTransportationUnit.".Trim(), serviceTaskLog.ToString());
			});

			var message_Resend = GetQueuedUniversalShipmentMessage(transitDispatchConsolRecipientDTW);

			var serviceTaskLog_Resend = new ServiceTaskLogForTesting();
			var manager_Resend = new UniversalMessageProcessingManager(serviceTaskLog_Resend);
			manager_Resend.Process(message_Resend);

			var newFactory_Resend = new UniversalObjectFactory();
			var dcn_Resend = newFactory_Resend.BOFactory.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			AssertEquals("TEST2", dcn_Resend.WDC_ConsignmentID);

			var dll_Resend = newFactory_Resend.BOFactory.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals("MABTEST2", dll_Resend.MasterBillNumber);
			AssertEquals(dll.PK, dll_Resend.PK);
			AssertEquals(1, dll_Resend.DispatchTransportationUnits.Count);

			var dtu_Resend = newFactory_Resend.BOFactory.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Single();
			AssertEquals(dll_Resend.PK, dtu_Resend.DispatchLoadLists.Single().PK);
			AssertEquals("LD3TEST", dtu_Resend.WDH_VehicleReference);
			AssertEquals("AUSASI", dtu_Resend.TransportCompany.Organisation.OH_Code);
			AssertEquals(dtu.PK, dtu_Resend.PK);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status should be error as booking party is missing.", EDIMessageStatusList.Codes.Error, message_Resend.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log",
@"Updated Dispatch Consignment DC00000001 from UniversalShipment.
Updated Dispatch Load List DLL00000001 from UniversalShipment.
Updated record from UniversalShipment.
Successfully saved record with 1 x WhsItemDispatchConsignment, 1 x CusEntryNumber, 1 x WhsItemDispatchLoadList, 1 x PkgPackage, 1 x WhsItemDispatchTransportationUnit.".Trim(), serviceTaskLog_Resend.ToString());
			});
		}

		public void TestImport_TransitDispatchConsol_DLLMatchingWithMasterBillNumber()
		{
			Data.SetupForForwardingImport();

			AssertEquals(0, Factory.BOFactory.Load<WhsItemDispatchConsignment>(new ZQuery()).Length);
			AssertEquals(0, Factory.BOFactory.Load<WhsItemDispatchLoadList>(new ZQuery()).Length);
			AssertEquals(0, Factory.BOFactory.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Length);
			AssertEquals(0, Factory.BOFactory.Load<WhsItemPackageState>(new ZQuery()).Length);

			var transitDispatchConsolMasterBillNumber = resourceRetriever.Value.GetString("Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing.Universal.TestFiles.TransitDispatchConsol - DataTarget No Key MasterBillNumber.xml");
			var message = GetQueuedUniversalShipmentMessage(transitDispatchConsolMasterBillNumber);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var newFactory = new UniversalObjectFactory();
			var dcn = newFactory.BOFactory.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			AssertEquals("TEST2", dcn.WDC_ConsignmentID);
			AssertEquals("TEST2", dcn.HouseBillNumber);

			var dll = newFactory.BOFactory.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals("MAB1234", dll.MasterBillNumber);
			AssertEquals(1, dll.DispatchTransportationUnits.Count);

			var dtu = newFactory.BOFactory.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Single();
			AssertEquals("", dtu.WDH_VehicleReference);
			AssertEquals(dll.PK, dtu.DispatchLoadLists.Single().PK);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status should be error as booking party is missing.", EDIMessageStatusList.Codes.Error, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log",
@"Added Dispatch Consignment DC00000001 from UniversalShipment.
Added Dispatch Load List DLL00000001 from UniversalShipment.
Updated record from UniversalShipment.
Successfully saved record with 1 x WhsItemDispatchConsignment, 1 x CusEntryNumber, 1 x WhsItemDispatchLoadList, 1 x PkgPackage, 1 x WhsItemDispatchTransportationUnit.".Trim(), serviceTaskLog.ToString());
			});

			var unUsedDLL = Helper.CreateDispatchLoadList("DLL1", dcn.Warehouse.PK);
			unUsedDLL.WDL_IsActive = true;

			var receiveHeader = Helper.CreateReceiveTransportationUnit("R123", dcn.Warehouse.PK, dcn.Warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", dcn.Warehouse.PK);
			Helper.CreatePackageState(rcn, 1, "PKG", "PKGDEP1", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn, dispatchLoadList: unUsedDLL);
			Helper.CreateAdditionalReference(unUsedDLL, "MAB1234", WarehouseAdditionalReferenceTypes.Codes.MasterBill);
			Factory.SaveForTesting();

			// resend dispatch instructions
			var message_Resend = GetQueuedUniversalShipmentMessage(transitDispatchConsolMasterBillNumber);

			var serviceTaskLog_Resend = new ServiceTaskLogForTesting();
			var manager_Resend = new UniversalMessageProcessingManager(serviceTaskLog_Resend);
			manager_Resend.Process(message_Resend);

			var newFactory_Resend = new UniversalObjectFactory();
			var dcn_Resend = newFactory_Resend.BOFactory.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			AssertEquals("TEST2", dcn_Resend.WDC_ConsignmentID);

			var dlls_AfterResend = newFactory_Resend.BOFactory.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals(2, dlls_AfterResend.Length);
			var dll_Resend = dlls_AfterResend.Single(l => l.PK == dll.PK);
			AssertEquals("MAB1234", dll_Resend.MasterBillNumber);
			AssertEquals(1, dll_Resend.DispatchTransportationUnits.Count);
			AssertEquals(0, dlls_AfterResend.Single(l => l.PK == unUsedDLL.PK).PackageStates.Count);

			var dtu_Resend = newFactory_Resend.BOFactory.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Single();
			AssertEquals("", dtu_Resend.WDH_VehicleReference);
			AssertEquals(dtu.PK, dtu_Resend.PK);
			AssertEquals(dll_Resend.PK, dtu_Resend.DispatchLoadLists.Single().PK);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status should be error as booking party is missing.", EDIMessageStatusList.Codes.Error, message_Resend.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log",
@"Updated Dispatch Consignment DC00000001 from UniversalShipment.
Updated Dispatch Load List DLL00000001 from UniversalShipment.
Updated record from UniversalShipment.
Successfully saved record with 1 x WhsItemPackageState, 1 x WhsItemDispatchConsignment, 1 x CusEntryNumber, 1 x WhsItemDispatchLoadList, 1 x PkgPackage, 1 x WhsItemDispatchTransportationUnit.".Trim(), serviceTaskLog_Resend.ToString());
			});
		}

		protected override DataContextType ExpectedDataContextType => DataContextType.TransitDispatchConsol;

		protected override ServiceCodeType?[] SupportedRecipientServices(RecipientRoleType recipientRole)
		{
			return SupportedRecipientRoleTypes.Contains(recipientRole) ? new ServiceCodeType?[] { ServiceCodeType.TWD, ServiceCodeType.TWP } : base.SupportedRecipientServices(recipientRole);
		}

		protected override void SetupDataForDataContextManagerTestCase()
		{
			base.SetupDataForDataContextManagerTestCase();
			var shipment = new UniversalDataBuss.DataObjects.Universal.Shipment();
			var logger = new DummyLogger();
			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(ValidPopulatedUniversalShipmentXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
			}
			_ = Data.Orgs.Warehouse_WUFSHIJNB;
			_ = Data.Warehouse;

			Data.CreateReceiveConsolAndConsignmentsInDB(shipment);
			Factory.SaveForTesting();
		}

		protected override WhsTransitDispatchConsol GetNewBusinessObjectForTesting() => new WhsTransitDispatchConsol();

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get
			{
				return @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment Version=""0.1"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Shipment>
	  <DataContext>
		  <DataSourceCollection>
			<DataSource>
			  <Type>ForwardingConsol</Type>
			  <Key>C00032740</Key>
			</DataSource>
		  </DataSourceCollection>
		</DataContext>
		<WayBillNumber>HouseBill123</WayBillNumber>
		<WayBillType>
		  <Code>HWB</Code>
		  <Description>House Waybill</Description>
		</WayBillType>
   
		<OrganizationAddressCollection>
			<OrganizationAddress>
				<AddressType>ConsignorDocumentaryAddress</AddressType>
				<OrganizationCode>CRAHOLSYD</OrganizationCode>
				<CompanyName>CRACKERJACK HOLDINGS</CompanyName>
				<Address1>1804 Fudrucker Way</Address1>
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>ConsigneeDocumentaryAddress</AddressType>
				<OrganizationCode>INTHEMSYD</OrganizationCode>
				<CompanyName>In The Moment</CompanyName>
				<Address1>Unit 12, Level 3</Address1>
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>LocalCartageCFS</AddressType>
				<OrganizationCode>WUFSHIJNB</OrganizationCode>
				<CompanyName>WUFU SHIPPING LINE</CompanyName>
				<Address1>Level 2, Building G</Address1>
			</OrganizationAddress>
		</OrganizationAddressCollection>

	  <SubShipmentCollection>
		<SubShipment>
		  <DataContext>
			<DataSourceCollection>
			  <DataSource>
				<Type>ForwardingShipment</Type>
				<Key>S00001000</Key>
			  </DataSource>
			</DataSourceCollection>
		  </DataContext>
		  <OrganizationAddressCollection>
		   <OrganizationAddress>
		   	<AddressType>ConsignorDocumentaryAddress</AddressType>
		   	<OrganizationCode>CRAHOLSYD</OrganizationCode>
		   	<CompanyName>CRACKERJACK HOLDINGS</CompanyName>
		   	<Address1>1804 Fudrucker Way</Address1>
		   </OrganizationAddress>
			<OrganizationAddress>
				<AddressType>ConsigneeDocumentaryAddress</AddressType>
				<OrganizationCode>INTHEMSYD</OrganizationCode>
				<CompanyName>In The Moment</CompanyName>
				<Address1>Unit 12, Level 3</Address1>
			</OrganizationAddress>
		  </OrganizationAddressCollection>
		  <PackingLineCollection>
		    <PackingLine>    
		  	<Height>1.000</Height>
		  	<Length>1.000</Length>
		  	<LengthUnit>
		  	  <Code>M</Code>
		  	  <Description>Metres</Description>
		  	</LengthUnit>
		  	<PackQty>2</PackQty>
		  	<PackType>
		  	  <Code>PLT</Code>
		  	  <Description>Pallet</Description>
		  	</PackType>
		  	<ReferenceNumber></ReferenceNumber>
		  	<Volume>2.000</Volume>
		  	<VolumeUnit>
		  	  <Code>M3</Code>
		  	  <Description>Cubic Metres</Description>
		  	</VolumeUnit>
		  	<Weight>150.000</Weight>
		  	<WeightUnit>
		  	  <Code>KG</Code>
		  	  <Description>Kilograms</Description>
		  	</WeightUnit>
		  	<Width>1.000</Width>
		  	<PackedItemCollection>
		  	</PackedItemCollection>
		    </PackingLine>
			<PackingLine>    
		  	 <Height>2.000</Height>
		  	 <Length>2.000</Length>
		   	<LengthUnit>
		  	  <Code>M</Code>
		  	  <Description>Metres</Description>
		  	</LengthUnit>
		  	<PackQty>5</PackQty>
		  	<PackType>
		  	  <Code>PLT</Code>
		  	  <Description>Pallet</Description>
		  	</PackType>
		  	<ReferenceNumber></ReferenceNumber>
		  	<Volume>2.000</Volume>
		  	<VolumeUnit>
		  	  <Code>M3</Code>
		  	  <Description>Cubic Metres</Description>
		  	</VolumeUnit>
		  	<Weight>250.000</Weight>
		  	<WeightUnit>
		  	  <Code>KG</Code>
		  	  <Description>Kilograms</Description>
		  	</WeightUnit>
		  	<Width>1.000</Width>
		  	<PackedItemCollection>
		  	</PackedItemCollection>
		    </PackingLine>
		  </PackingLineCollection>
        </SubShipment>
		<SubShipment>
		  <DataContext>
			<DataSourceCollection>
			  <DataSource>
				<Type>ForwardingShipment</Type>
				<Key>S00001001</Key>
			  </DataSource>
			</DataSourceCollection>
		  </DataContext>
		  <OrganizationAddressCollection>
			<OrganizationAddress>
				<AddressType>ConsignorDocumentaryAddress</AddressType>
				<OrganizationCode>CRAHOLSYD</OrganizationCode>
				<CompanyName>CRACKERJACK HOLDINGS</CompanyName>
				<Address1>1804 Fudrucker Way</Address1>
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>ConsigneeDocumentaryAddress</AddressType>
				<OrganizationCode>INTHEMSYD</OrganizationCode>
				<CompanyName>In The Moment</CompanyName>
				<Address1>Unit 12, Level 3</Address1>
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>DepartureCFSAddress</AddressType>
				<OrganizationCode>WUFSHIJNB</OrganizationCode>
				<CompanyName>WUFU SHIPPING LINE</CompanyName>
				<Address1>Level 2, Building G</Address1>
		   </OrganizationAddress>
		  </OrganizationAddressCollection>
		  <PackingLineCollection>
		   <PackingLine>    
		  	 <Height>2.000</Height>
		  	 <Length>2.000</Length>
		   	<LengthUnit>
		  	  <Code>M</Code>
		  	  <Description>Metres</Description>
		  	</LengthUnit>
		  	<PackQty>5</PackQty>
		  	<PackType>
		  	  <Code>PLT</Code>
		  	  <Description>Pallet</Description>
		  	</PackType>
		  	<ReferenceNumber></ReferenceNumber>
		  	<Volume>2.000</Volume>
		  	<VolumeUnit>
		  	  <Code>M3</Code>
		  	  <Description>Cubic Metres</Description>
		  	</VolumeUnit>
		  	<Weight>250.000</Weight>
		  	<WeightUnit>
		  	  <Code>KG</Code>
		  	  <Description>Kilograms</Description>
		  	</WeightUnit>
		  	<Width>1.000</Width>
		  	<PackedItemCollection>
		  	</PackedItemCollection>
		    </PackingLine>
		  </PackingLineCollection>
        </SubShipment>
      </SubShipmentCollection>
  </Shipment>
</UniversalShipment>
";
			}
		}

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory.BOFactory));
		WhsTransitTestHelper helper;

		TestDataForUniversal Data => data ?? (data = new TestDataForUniversal(Factory, new TestErrorLogger()));
		TestDataForUniversal data;
	}
}
