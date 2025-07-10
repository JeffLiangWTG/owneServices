using System;
using System.IO;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.DataTransfer.Universal.AirManifest.Testing
{
	[TestedType(typeof(AirManifestDataContextManager))]
	sealed class AirManifestDataContextManagerTest : ShipmentDataContextManagerTestCase<AirManifestDataContextManager, CusMAWB>
	{
		public void TestMultipleHVLVConsolidations()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(TestFileHelper.GetPathForUniversalHVLVAirTestFiles("MultipleHVLVConsolidations.xml")));

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				var mawbs = new CusMAWB.Loader(Factory.BOFactory).FindMatchingMAWBs("12583833890");
				AssertEquals(2, mawbs.Length);
				AssertNotNull(mawbs.FirstOrDefault(x => x.CM_MasterHouseBill == "S00046520"));
				AssertNotNull(mawbs.FirstOrDefault(x => x.CM_MasterHouseBill == "HLSATTACHCOLOAD"));
			}
		}

		public void TestDefaultOutputDirectory()
		{
			var directory = "\\WHERE\\IS\\THIS\\DIRECTORY";
			SystemDataRegistry.Instance.CustomDeclarationExportDirectory.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, directory);
			var manager = new AirManifestDataContextManager();
			AssertEquals("DefaultOutputDirectory", directory, manager.DefaultOutputDirectory);
		}

		public void TestExportData_SubShipment()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "MB1";
			var hawb1 = mawb.ChildBills.AddNew();
			hawb1.CS_HAWB = "HB1";
			var subHawb1 = mawb.ChildBills.AddNew();
			subHawb1.CS_HAWB = "SB1";
			subHawb1.CS_MasterHouseBill = "HB1";
			subHawb1.CS_CS_MasterHouseBill = hawb1.PK;
			var hawb2 = mawb.ChildBills.AddNew();
			hawb2.CS_HAWB = "HB2";
			var subHawb2 = mawb.ChildBills.AddNew();
			subHawb2.CS_HAWB = "SB2";
			subHawb2.CS_MasterHouseBill = "HB2";
			subHawb2.CS_CS_MasterHouseBill = hawb2.PK;
			IShipmentDataContextManager manager = new AirManifestDataContextManager();
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, mawb)));
			var mawbData = (UniversalShipment)writer.GetDataObject(mawb);
			AssertEquals("mawbData.DataContext.DataSourceCollection.Count", 1, mawbData.DataContext.DataSourceCollection.Count());
			AssertNotNull("Should have AirManifest", mawbData.GetMatchingDataSource(DataContextType.AirManifest));
			AssertEquals("mawbData.WayBillNumber", "MB1", mawbData.WayBillNumber);
			AssertEquals("mawbData.SubShipmentCollection.Count", 2, mawbData.SubShipmentCollection.Count);
			var hawb1Data = mawbData.SubShipmentCollection[0];
			var hawb2Data = mawbData.SubShipmentCollection[1];
			if (hawb2Data.WayBillNumber.GetValueOrDefault() == "HB1")
			{
				hawb1Data = mawbData.SubShipmentCollection[1];
				hawb2Data = mawbData.SubShipmentCollection[0];
			}
			AssertEquals("hawb1Data.WayBillNumber", "HB1", hawb1Data.WayBillNumber);
			AssertEquals("hawb1Data.SubShipmentCollection.Count", 1, hawb1Data.SubShipmentCollection.Count);
			var subHawb1Data = hawb1Data.SubShipmentCollection[0];
			AssertEquals("subHawb1Data.WayBillNumber", "SB1", subHawb1Data.WayBillNumber);
			AssertEquals("subHawb1Data.AdditionalBillCollection.Count", 1, subHawb1Data.AdditionalBillCollection.Count);
			var subHawb1BillData = subHawb1Data.AdditionalBillCollection[0];
			AssertEquals("subHawb1BillData.ParentBillNumber", "HB1", subHawb1BillData.ParentBillNumber);

			AssertEquals("hawb2Data.WayBillNumber", "HB2", hawb2Data.WayBillNumber);
			AssertEquals("hawb2Data.SubShipmentCollection.Count", 1, hawb2Data.SubShipmentCollection.Count);
			var subHawb2Data = hawb2Data.SubShipmentCollection[0];
			AssertEquals("subHawb2Data.WayBillNumber", "SB2", subHawb2Data.WayBillNumber);
			AssertEquals("subHawb2Data.AdditionalBillCollection.Count", 1, subHawb2Data.AdditionalBillCollection.Count);
			var subHawb2BillData = subHawb2Data.AdditionalBillCollection[0];
			AssertEquals("subHawb2BillData.ParentBillNumber", "HB2", subHawb2BillData.ParentBillNumber);
		}

		public void TestOnlyProcessAirShipment()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			IShipmentDataContextManager manager = new AirManifestDataContextManager();
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.AirManifest, null);

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "MB1234",
				WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
				TransportMode = new CodeDescriptionPair() { Code = Core.Constants.TransportModes.Sea }
			};
			var logger = new TestErrorLogger();
			AssertEquals("Should not be used as it's not Air", false, manager.UseIncomingShipmentData(shipment, logger, new UniversalObjectFactory()));
			shipment.TransportMode.Code = Core.Constants.TransportModes.Air;
			AssertEquals("Should be used as it's Air", true, manager.UseIncomingShipmentData(shipment, logger, new UniversalObjectFactory()));
		}

		public void TestGetDataContextKeyMatchesOnDataTarget()
		{
			var someCompany = Factory.NewWithValidTestData<GlbCompany>();
			var someBranch = Factory.NewWithValidTestData<GlbBranch>();
			someBranch.GB_GC = someCompany.PK;

			var cusMawbInSomeCompany = Factory.New<CusMAWB>();
			cusMawbInSomeCompany.CM_MessageReference = "X00001001";
			cusMawbInSomeCompany.CM_MAWB = "0810495827";
			cusMawbInSomeCompany.CM_GB = someBranch.PK;
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(someCompany);
			dataContext.CodesMappedToTarget = false;
			dataContext.AddDataTarget(DataContextType.AirManifest, "X00001001");
			var mawbDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				Branch = new Branch { Code = someBranch.GB_Code },
				WayBillNumber = "0810495827",
				WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master },
			};

			var message = GetQueuedUniversalShipmentMessage(mawbDataObject);
			message.EM_MessageText = message.EM_MessageText.Replace("				      <DataProvider>EDIDATDAN</DataProvider>", "").Replace("      <EnterpriseID>EDI</EnterpriseID>", "");
			Factory.SaveForTesting();

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			cusMawbInSomeCompany.Reload();
			AssertEquals("cusMawbInSomeCompany.CM_MessageReference", "X00001001", cusMawbInSomeCompany.CM_MessageReference);
			AssertEquals("cusMawbInSomeCompany.CM_MAWB", "0810495827", cusMawbInSomeCompany.CM_MAWB);
			AssertEquals("cusMawbInSomeCompany.CM_GB", someBranch.PK, cusMawbInSomeCompany.CM_GB);
		}

		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("CusMAWB doesn't have any unique jobnumber", true);
		}

		protected override string ValidPopulatedUniversalShipmentXML => @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AirManifest</Type>
          <Key>X00001001</Key>
        </DataSource>
      </DataSourceCollection>
    </DataContext>

    <ContainerMode>
      <Code>LSE</Code>
      <Description>Loose</Description>
    </ContainerMode>
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

		protected override void MakeShipmentUsableForThisRole(RecipientRoleType recipientRoleType, UniversalShipment shipmentWithRecipientRole)
		{
			if (recipientRoleType == RecipientRoleType.HCA || recipientRoleType == RecipientRoleType.AAD)
			{
				shipmentWithRecipientRole.DataContext.ClearDataSourceCollection();
				shipmentWithRecipientRole.DataContext.AddDataSource(DataContextType.AirManifest, null);
			}
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => new[] { RecipientRoleType.HCA, RecipientRoleType.AAD };

		protected override bool ManagerChecksDataTargetToImport => false;

		protected override void TearDown()
		{
			base.TearDown();
			testFileHelper?.Dispose();
			testFileHelper = null;
		}

		TestFileHelper TestFileHelper => testFileHelper ??= new ();
		TestFileHelper testFileHelper;
	}
}
