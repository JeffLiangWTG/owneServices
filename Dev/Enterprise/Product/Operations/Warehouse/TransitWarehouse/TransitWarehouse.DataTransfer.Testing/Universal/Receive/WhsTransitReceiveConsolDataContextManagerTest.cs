
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Transit.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing.Universal.Receive
{
	[TestedType(typeof(WhsTransitReceiveConsolDataContextManager))]
	sealed class WhsTransitReceiveConsolDataContextManagerTest : WhsTransitConsignmentConsolDataContextManagerTest<WhsTransitReceiveConsolDataContextManager, WhsTransitReceiveConsol>
	{
		public void Test_Import_TransitReceiveConsol_WithNonMatchingDataTargetKey_And_TransitReceive_WithNonMatchingDataTargetKey_ShouldDiscardMessage()
		{
			var transitReceiveConsolWithKey = resourceRetriever.Value.GetString("Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing.Universal.TestFiles.TransitReceiveConsol - DataTarget With Key and TransitReceive - With Key.xml");
			var message = GetQueuedUniversalShipmentMessage(transitReceiveConsolWithKey);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"ERROR - Cannot import without a valid Warehouse supplied. Please ensure there is a valid departure/arrival CFS address and that you targeted the correct recipient type.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.".Trim(), serviceTaskLog.ToString());
			});
		}

		public void Test_Import_TransitReceiveConsol_With_NoDataTargetKey_And_TransitReceive_NoDataTargetKey_ShouldCreateRCN_For_ATW()
		{
			Data.SetupForForwardingImport();

			AssertEquals(0, Factory.BOFactory.Load<WhsItemReceiveConsignment>(new ZQuery()).Length);

			var transitReceiveConsolRecipientATW = resourceRetriever.Value.GetString("Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing.Universal.TestFiles.TransitReceiveConsol - DataTarget No Key and TransitReceive - DataTarget No Key - Recipient ATW.xml");
			var message = GetQueuedUniversalShipmentMessage(transitReceiveConsolRecipientATW);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status should be error as booking party is missing.", EDIMessageStatusList.Codes.Error, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"Added Receive Consignment RC00000001 from UniversalShipment.
Updated record from UniversalShipment.
Successfully saved record with 1 x WhsItemReceiveConsignment.".Trim(), serviceTaskLog.ToString());
				AssertNotNull("New Receive Consignment Created.", Factory.BOFactory.Load<WhsItemReceiveConsignment>(new ZQuery()).Single(rcn => rcn.WRC_ConsignmentID == "RC00000001"));
			});
		}

		public void Test_Import_TransitReceiveConsol_With_NoDataTargetKey_And_TransitReceive_NoDataTargetKey_ShouldCreateRCN_For_DTW()
		{
			Data.SetupForForwardingImport();

			AssertEquals(0, Factory.BOFactory.Load<WhsItemReceiveConsignment>(new ZQuery()).Length);

			var transitReceiveConsolRecipientDTW = resourceRetriever.Value.GetString("Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing.Universal.TestFiles.TransitReceiveConsol - DataTarget No Key and TransitReceive - DataTarget No Key - Recipient DTW.xml");
			var message = GetQueuedUniversalShipmentMessage(transitReceiveConsolRecipientDTW);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status should be error as booking party is missing.", EDIMessageStatusList.Codes.Error, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"Added Receive Consignment RC00000001 from UniversalShipment.
Updated record from UniversalShipment.
Successfully saved record with 1 x WhsItemReceiveConsignment.".Trim(), serviceTaskLog.ToString());
				AssertNotNull("New Receive Consignment Created.", Factory.BOFactory.Load<WhsItemReceiveConsignment>(new ZQuery()).Single(rcn => rcn.WRC_ConsignmentID == "RC00000001"));
			});
		}

		public void Test_Import_TransitReceiveConsol_NoDataTargetKey_And_TransitReceive_NoDataTargetKey_NonMatchingWayBillNumber_And_ValidCFSAddress_ShouldImportConsolAndShipmentData_For_ATW()
		{
			Data.SetupForForwardingImport();

			AssertEquals(0, Factory.BOFactory.Load<WhsItemReceiveASN>(new ZQuery()).Length);
			AssertEquals(0, Factory.BOFactory.Load<WhsItemReceiveConsignment>(new ZQuery()).Length);
			AssertEquals(0, Factory.BOFactory.Load<WhsItemPackageState>(new ZQuery()).Length);

			var transitReceiveConsolRecipientATWWithWayBillNumbers = resourceRetriever.Value.GetString("Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing.Universal.TestFiles.TransitReceiveConsol - DataTarget No Key - Recipient ATW - With Way Bill Numbers and Valid CFS.xml");
			var message = GetQueuedUniversalShipmentMessage(transitReceiveConsolRecipientATWWithWayBillNumbers);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			// should create 2xASN 2xRCN 5xPackages (2 are RTU containers) 4xCusEntryNumber (2 are from ASN and 2 are from RTU contaienrs)
			var newFactory = new UniversalObjectFactory();
			var asns = newFactory.BOFactory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals(2, asns.Length);

			var newASN1 = asns.Single(a => a.WRP_VehicleReference == "CONTAINER 1");
			var newASN2 = asns.Single(a => a.WRP_VehicleReference == "CONTAINER 2");
			var newRcnWithOnePackline = newASN1.ReceiveConsignments.Single(rcn => rcn.WRC_ConsignmentID == "HOUSEBILL 1");
			var newRcnWithTwoPackline = newASN2.ReceiveConsignments.Single(rcn => rcn.WRC_ConsignmentID == "HOUSEBILL 2");
			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status should be error as booking party is missing.", EDIMessageStatusList.Codes.Error, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"Added Receive Consignment RC00000001 from UniversalShipment.
Added Receive Consignment RC00000002 from UniversalShipment.
Added Receive ASN TRT00000001 from UniversalShipment.
Added Receive ASN TRT00000002 from UniversalShipment.
Updated record from UniversalShipment.
Successfully saved record with 6 x CusEntryNumber, 2 x PkgPackageJob, 5 x PkgPackage, 3 x WhsItemPackageState, 2 x WhsItemReceiveConsignment, 2 x WhsItemReceiveTransportationUnit, 2 x WhsItemReceiveASN.".Trim(), serviceTaskLog.ToString());
				AssertNotNull("New Receive Consignment Has Consignor.", newRcnWithOnePackline.ConsignorDocAddress);
				AssertNotNull("New Receive Consignment Has Consignor.", newRcnWithTwoPackline.ConsignorDocAddress);
				AssertEquals("New Receive Consignment Has 1 Package.", 1, newRcnWithOnePackline.PackageStates.Count);
				AssertEquals("New Receive Consignment Has 2 Packages.", 2, newRcnWithTwoPackline.PackageStates.Count);
				AssertNotNull("New Receive Consignment Has PackType PKG.", newRcnWithOnePackline.PackageStates.Single(ps => ps.Package.KP_F3_NKPackType == "PKG" && ps.Package.KP_PackageQty == 1));
				AssertNotNull("New Receive Consignment Has PackType BOX.", newRcnWithTwoPackline.PackageStates.Single(ps => ps.Package.KP_F3_NKPackType == "BOX" && ps.Package.KP_PackageQty == 2));
				AssertNotNull("New Receive Consignment Has PackType BOT.", newRcnWithTwoPackline.PackageStates.Single(ps => ps.Package.KP_F3_NKPackType == "BOT" && ps.Package.KP_PackageQty == 3));
			});
		}

		public void Test_Import_TransitReceiveConsol_NoDataTargetKey_And_TransitReceive_NoDataTargetKey_NonMatchingWayBillNumber_And_ValidCFSAddress_ShouldImportConsolAndShipmentData_For_DTW()
		{
			Data.SetupForForwardingImport();

			AssertEquals(0, Factory.BOFactory.Load<WhsItemReceiveASN>(new ZQuery()).Length);
			AssertEquals(0, Factory.BOFactory.Load<WhsItemReceiveConsignment>(new ZQuery()).Length);
			AssertEquals(0, Factory.BOFactory.Load<WhsItemPackageState>(new ZQuery()).Length);

			var transitReceiveConsolRecipientDTWWithWayBillNumbers = resourceRetriever.Value.GetString("Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing.Universal.TestFiles.TransitReceiveConsol - DataTarget No Key - Recipient DTW - With Way Bill Numbers and Valid CFS.xml");
			var message = GetQueuedUniversalShipmentMessage(transitReceiveConsolRecipientDTWWithWayBillNumbers);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			// should create 1xASN 2xRCN 3xPackages
			var newFactory = new UniversalObjectFactory();
			var newASN = newFactory.BOFactory.Load<WhsItemReceiveASN>(new ZQuery()).Single();
			AssertEquals("TEST CONSOL", newASN.WRP_VehicleReference);
			var newRcnWithOnePackline = newASN.ReceiveConsignments.Single(rcn => rcn.WRC_ConsignmentID == "HOUSEBILL 1");
			var newRcnWithTwoPackline = newASN.ReceiveConsignments.Single(rcn => rcn.WRC_ConsignmentID == "HOUSEBILL 2");
			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status should be error as booking party is missing.", EDIMessageStatusList.Codes.Error, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"Added Receive Consignment RC00000001 from UniversalShipment.
Added Receive Consignment RC00000002 from UniversalShipment.
Added Receive ASN TRT00000001 from UniversalShipment.
Updated record from UniversalShipment.
Successfully saved record with 2 x PkgPackageJob, 3 x PkgPackage, 3 x WhsItemPackageState, 2 x WhsItemReceiveConsignment, 1 x WhsItemReceiveASN.".Trim(), serviceTaskLog.ToString());
				AssertNotNull("New Receive Consignment Has Consignor.", newRcnWithOnePackline.ConsignorDocAddress);
				AssertNotNull("New Receive Consignment Has Consignor.", newRcnWithTwoPackline.ConsignorDocAddress);
				AssertEquals("New Receive Consignment Has 1 Package.", 1, newRcnWithOnePackline.PackageStates.Count);
				AssertEquals("New Receive Consignment Has 2 Packages.", 2, newRcnWithTwoPackline.PackageStates.Count);
				AssertNotNull("New Receive Consignment Has PackType PKG.", newRcnWithOnePackline.PackageStates.Single(ps => ps.Package.KP_F3_NKPackType == "PKG" && ps.Package.KP_PackageQty == 1));
				AssertNotNull("New Receive Consignment Has PackType BOX.", newRcnWithTwoPackline.PackageStates.Single(ps => ps.Package.KP_F3_NKPackType == "BOX" && ps.Package.KP_PackageQty == 2));
				AssertNotNull("New Receive Consignment Has PackType BOT.", newRcnWithTwoPackline.PackageStates.Single(ps => ps.Package.KP_F3_NKPackType == "BOT" && ps.Package.KP_PackageQty == 3));
			});
		}

		public void Test_Import_TransitReceiveConsol_NoDataTargetKey_And_TransitReceive_NoDataTargetKey_NonMatchingWayBillNumber_And_NoCFS_ShouldThrowError()
		{
			Data.SetupForForwardingImport();

			var transitReceiveConsolWithWayBillNumbersNoCFS = resourceRetriever.Value.GetString("Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing.Universal.TestFiles.TransitReceiveConsol - DataTarget No Key - Way Bill Numbers and No CFS.xml");
			var message = GetQueuedUniversalShipmentMessage(transitReceiveConsolWithWayBillNumbersNoCFS);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"ERROR - Cannot import without a valid Warehouse supplied. Please ensure there is a valid departure/arrival CFS address and that you targeted the correct recipient type.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.".Trim(), serviceTaskLog.ToString());
			});
		}

		public void Test_Import_TransitReceiveConsol_NoDataTargetKey_And_TransitReceive_NonMatchingDataSourceKey_ShouldCreateRCN_For_ATW()
		{
			Data.SetupForForwardingImport();

			AssertEquals(0, Factory.BOFactory.Load<WhsItemReceiveConsignment>(new ZQuery()).Length);

			var transitReceiveConsolRecipientATW = resourceRetriever.Value.GetString("Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing.Universal.TestFiles.TransitReceiveConsol - DataTarget No Key and TransitReceive - DataSource With Key - Recipient ATW.xml");
			var message = GetQueuedUniversalShipmentMessage(transitReceiveConsolRecipientATW);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status should be error as booking party is missing.", EDIMessageStatusList.Codes.Error, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"Added Receive Consignment RC00000001 from UniversalShipment.
Updated record from UniversalShipment.
Successfully saved record with 1 x WhsItemReceiveConsignment.".Trim(), serviceTaskLog.ToString());
				AssertNotNull("New Receive Consignment Created.", Factory.BOFactory.Load<WhsItemReceiveConsignment>(new ZQuery()).Single(rcn => rcn.WRC_ConsignmentID == "RC00000001"));
			});
		}

		public void Test_Import_TransitReceiveConsol_NoDataTargetKey_And_TransitReceive_NonMatchingDataSourceKey_ShouldCreateRCN_For_DTW()
		{
			Data.SetupForForwardingImport();

			AssertEquals(0, Factory.BOFactory.Load<WhsItemReceiveConsignment>(new ZQuery()).Length);

			var transitReceiveConsolRecipientDTW = resourceRetriever.Value.GetString("Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing.Universal.TestFiles.TransitReceiveConsol - DataTarget No Key and TransitReceive - DataSource With Key - Recipient DTW.xml");
			var message = GetQueuedUniversalShipmentMessage(transitReceiveConsolRecipientDTW);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status should be error as booking party is missing.", EDIMessageStatusList.Codes.Error, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"Added Receive Consignment RC00000001 from UniversalShipment.
Updated record from UniversalShipment.
Successfully saved record with 1 x WhsItemReceiveConsignment.".Trim(), serviceTaskLog.ToString());
				AssertNotNull("New Receive Consignment Created.", Factory.BOFactory.Load<WhsItemReceiveConsignment>(new ZQuery()).Single(rcn => rcn.WRC_ConsignmentID == "RC00000001"));
			});
		}

		protected override DataContextType ExpectedDataContextType => DataContextType.TransitReceiveConsol;

		protected override ServiceCodeType?[] SupportedRecipientServices(RecipientRoleType recipientRole)
		{
			return SupportedRecipientRoleTypes.Contains(recipientRole) ? new ServiceCodeType?[] { ServiceCodeType.TWR, ServiceCodeType.TWX } : base.SupportedRecipientServices(recipientRole);
		}

		protected override void SetupDataForDataContextManagerTestCase()
		{
			base.SetupDataForDataContextManagerTestCase();
			_ = Data.Orgs.CRAHOLSYD;
			_ = Data.Warehouse;
		}

		protected override WhsTransitReceiveConsol GetNewBusinessObjectForTesting() => new WhsTransitReceiveConsol();

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get { return @"
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
				<Key>S00001001</Key>
			  </DataSource>
			</DataSourceCollection>
		  </DataContext>
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
		  </PackingLineCollection>
        </SubShipment>
      </SubShipmentCollection>
  </Shipment>
</UniversalShipment>
"; }
		}

		TestDataForUniversal Data => data ?? (data = new TestDataForUniversal(Factory, new TestErrorLogger()));
		TestDataForUniversal data;
	}
}
