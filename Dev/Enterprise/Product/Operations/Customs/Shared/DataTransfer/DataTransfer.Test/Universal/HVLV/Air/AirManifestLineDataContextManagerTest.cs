using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.DataTransfer.Universal.AirManifest.Testing
{
	[TestedType(typeof(AirManifestLineDataContextManager))]
	sealed class AirManifestLineDataContextManagerTest : ShipmentDataContextManagerTestCase<AirManifestLineDataContextManager, CusHAWB>
	{
		public void TestDefaultOutputDirectory()
		{
			var directory = "\\WHERE\\IS\\THIS\\DIRECTORY";
			SystemDataRegistry.Instance.CustomDeclarationExportDirectory.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, directory);
			var manager = new AirManifestLineDataContextManager();
			AssertEquals("DefaultOutputDirectory", directory, manager.DefaultOutputDirectory);
		}

		public void TestOnlyProcessAirShipment()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			IShipmentDataContextManager manager = new AirManifestLineDataContextManager();
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.AirManifestLine, null);

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

		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("CusMAWB doesn't have any unique jobnumber", true);
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
			var subHawb2 = mawb.ChildBills.AddNew();
			subHawb2.CS_HAWB = "SB2";
			subHawb2.CS_MasterHouseBill = "HB1";
			subHawb2.CS_CS_MasterHouseBill = hawb1.PK;
			var hawb2 = mawb.ChildBills.AddNew();
			hawb2.CS_HAWB = "HB2";
			var subHawb3 = mawb.ChildBills.AddNew();
			subHawb3.CS_HAWB = "SB3";
			subHawb3.CS_MasterHouseBill = "HB2";
			subHawb3.CS_CS_MasterHouseBill = hawb2.PK;
			IShipmentDataContextManager manager = new AirManifestLineDataContextManager();
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, hawb1)));
			var mawbData = (UniversalShipment)writer.GetDataObject(hawb1);
			var sources = mawbData.DataContext.DataSourceCollection.ToArray();
			AssertEquals("mawbData.DataContext.DataSourceCollection.Count", 2, sources.Length);
			AssertEquals(nameof(DataContextType.AirManifest), sources[0].Type);
			AssertEquals(nameof(DataContextType.AirManifestLine), sources[1].Type);
			AssertEquals("mawbData.WayBillNumber", "MB1", mawbData.WayBillNumber);
			AssertEquals("mawbData.SubShipmentCollection.Count", 1, mawbData.SubShipmentCollection.Count);
			var hawb1Data = mawbData.SubShipmentCollection[0];
			AssertEquals("hawb1Data.WayBillNumber", "HB1", hawb1Data.WayBillNumber);
			AssertEquals("hawb1Data.SubShipmentCollection.Count", 2, hawb1Data.SubShipmentCollection.Count);
			var subHawb1Data = hawb1Data.SubShipmentCollection[0];
			AssertEquals("subHawb1Data.WayBillNumber", "SB1", subHawb1Data.WayBillNumber);
			AssertEquals("subHawb1Data.AdditionalBillCollection.Count", 1, subHawb1Data.AdditionalBillCollection.Count);
			var subHawb1BillData = subHawb1Data.AdditionalBillCollection[0];
			AssertEquals("subHawb1BillData.ParentBillNumber", "HB1", subHawb1BillData.ParentBillNumber);
			var subHawb2Data = hawb1Data.SubShipmentCollection[1];
			AssertEquals("subHawb2Data.WayBillNumber", "SB2", subHawb2Data.WayBillNumber);
			AssertEquals("subHawb2Data.AdditionalBillCollection.Count", 1, subHawb2Data.AdditionalBillCollection.Count);
			var subHawb2BillData = subHawb2Data.AdditionalBillCollection[0];
			AssertEquals("subHawb2BillData.ParentBillNumber", "HB1", subHawb2BillData.ParentBillNumber);

			mawbData = (UniversalShipment)writer.GetDataObject(subHawb2);
			sources = mawbData.DataContext.DataSourceCollection.ToArray();
			AssertEquals("mawbData.DataContext.DataSourceCollection.Count", 2, sources.Length);
			AssertEquals(nameof(DataContextType.AirManifest), sources[0].Type);
			AssertEquals(nameof(DataContextType.AirManifestLine), sources[1].Type);
			AssertEquals("mawbData.WayBillNumber", "MB1", mawbData.WayBillNumber);
			AssertEquals("mawbData.SubShipmentCollection.Count", 1, mawbData.SubShipmentCollection.Count);
			hawb1Data = mawbData.SubShipmentCollection[0];
			AssertEquals("hawb1Data.WayBillNumber", "HB1", hawb1Data.WayBillNumber);
			AssertEquals("hawb1Data.SubShipmentCollection.Count", 1, hawb1Data.SubShipmentCollection.Count);
			subHawb2Data = hawb1Data.SubShipmentCollection[0];
			AssertEquals("subHawb2Data.WayBillNumber", "SB2", subHawb2Data.WayBillNumber);
			AssertEquals("subHawb2Data.AdditionalBillCollection.Count", 1, subHawb2Data.AdditionalBillCollection.Count);
			subHawb2BillData = subHawb2Data.AdditionalBillCollection[0];
			AssertEquals("subHawb2BillData.ParentBillNumber", "HB1", subHawb2BillData.ParentBillNumber);
		}

		protected override string ValidPopulatedUniversalShipmentXML => @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <Shipment>
	<DataContext>
	  <DataSourceCollection>
		<DataSource>
		  <Type>AirManifestLine</Type>
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
				shipmentWithRecipientRole.DataContext.AddDataSource(DataContextType.AirManifestLine, null);
			}
		}

		protected override string GetExpectedDataSourceForUniversalShipmentTopLevelDataObject(IDataContextManager manager)
			=> nameof(DataContextType.AirManifest) + " [], " + nameof(DataContextType.AirManifestLine) + " []";

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => new[] { RecipientRoleType.HCA, RecipientRoleType.AAD };

		protected override bool ManagerChecksDataTargetToImport => false;
	}
}
