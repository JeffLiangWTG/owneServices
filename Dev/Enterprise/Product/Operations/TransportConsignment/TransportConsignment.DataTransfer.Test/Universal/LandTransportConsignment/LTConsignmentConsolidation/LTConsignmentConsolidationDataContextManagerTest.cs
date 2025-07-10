using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	[TestedType(typeof(LTConsignmentConsolidationDataContextManager))]
	class LTConsignmentConsolidationDataContextManagerTest : ShipmentDataContextManagerTestCase<LTConsignmentConsolidationDataContextManager, LTConsignmentConsolidation>
	{
		#region Context
		#region TestDataContextType
		public void TestDataContextType()
		{
			AssertEquals(DataContextType.LandTransportConsignmentConsol, new LTConsignmentConsolidationDataContextManager().DataContextType);
		}

		#endregion
		#region TestDataContextKey
		public void TestDataContextKey()
		{
			var consolidation = new LTConsignmentConsolidation();
			AssertEquals("Non persistent consol", consolidation.GetUniversalDataContextManager().DataContextKey);
		}

		#endregion
		#endregion
		#region Shipments
		#region TestShipmentDataObjectWriter
		public void TestShipmentDataObjectWriter()
		{
			IShipmentDataContextManager manager = new LTConsignmentConsolidationDataContextManager();
			var dataWritingManager = new DataWritingManager(new DummyActionInfo());
			AssertNull(manager.GetShipmentDataObjectWriter(dataWritingManager));
		}

		#endregion
		#region TestManagesShipments
		public void TestManagesShipments()
		{
			AssertEquals(true, new LTConsignmentConsolidationDataContextManager().ManagesShipments);
		}

		#endregion
		#endregion
		#region TestShipmentDataObjectReader
		public void TestShipmentDataObjectReader()
		{
			var bookingBO = (DtbBooking)HelperBK.CreateBooking("TB123");
			var pkg1 = HelperBK.CreatePackage("123", 1);
			bookingBO.PackageJob.Packages.Add(pkg1);
			var pickupInstruction = HelperBK.CreateInstruction(bookingBO, InstructionTypes.Codes.PickUp, "", null);
			HelperBK.CreatePackageDivot(pickupInstruction, pkg1, 1);
			Factory.SaveForTesting();
			var writer = ((IShipmentDataContextManager)bookingBO.GetUniversalDataContextManager()).GetShipmentDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var bookingDataObject = writer.GetDataObject(bookingBO);
			bookingDataObject.DataContext = DataContextFactory.New();
			bookingDataObject.DataContext.AddDataTarget(DataContextType.LandTransportConsignmentConsol, "");
			bookingDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			var message = GetQueuedUniversalShipmentMessage((UniversalShipment)bookingDataObject);
			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			var results = manager.Process(message).ImportResults;
			var importResult = results.Single(o => o.DataContextType == DataContextType.LandTransportConsignmentConsol);
			AssertEquals(true, importResult.WasSuccessful);
			var importedConsignment = (DtbConsignment)results.Single(o => o.DataContextType == DataContextType.LandTransportConsignment).GetBizOForTesting(bookingDataObject, new BusinessObjectFactory());
			AssertNotNull("Should have created a DtbConsignment.", importedConsignment);
		}

		#endregion
		#region TestEventContextValues
		public void TestEventContextValues()
		{
			var consolidation = new LTConsignmentConsolidation();
			var manager = consolidation.GetUniversalDataContextManager() as IEventDataContextManager;
			AssertEquals("", manager.EventContextValues.ToStringContents(o => o.Key + " - " + o.Value));
		}

		#endregion
		#region TestDefaultOutputDirectory
		public void TestDefaultOutputDirectory()
		{
			AssertNull(new LTConsignmentConsolidationDataContextManager().DefaultOutputDirectory);
		}

		#endregion
		#region Helper
		TransportBookingConsignmentTestHelper HelperBK
		{
			get
			{
				return helperBK ?? (helperBK = new TransportBookingConsignmentTestHelper(Factory.BOFactory));
			}
		}

		TransportBookingConsignmentTestHelper helperBK;
		#endregion
		#region Implementation
		protected override RecipientRoleType[] SupportedRecipientRoleTypes
		{
			get
			{
				return System.Array.Empty<RecipientRoleType>();
			}
		}

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get
			{
				return @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>TransportBookingConsolidation</Type>
          <Key>CM00009398</Key>
        </DataSource>
        <DataSource>
          <Type>TransportBooking</Type>
          <Key>TB00009210</Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
        <Code>DAU</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>AU Demo Company</Name>
      </Company>

      <DataTargetCollection>
        <DataTarget>
          <Type>LandTransportConsignmentConsol</Type>
        </DataTarget>
      </DataTargetCollection>

    </DataContext>

    <ShipmentType>
      <Code>BKG</Code>
      <Description>Booking</Description>
    </ShipmentType>

    <PackingLineCollection Content=""Complete"">
      <PackingLine>
       <Link>0</Link>
        <PackQty>1</PackQty>
        <PackType>
          <Code>PLT</Code>
          <Description>Pallet</Description>
        </PackType>
      </PackingLine>
    </PackingLineCollection>

    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSourceCollection>
            <DataSource>
              <Type>TransportBooking</Type>
              <Key>TB00009210</Key>
            </DataSource>
          </DataSourceCollection>
        </DataContext>

        <Branch>
          <Code>SYD</Code>
          <Name>SYDNEY</Name>
        </Branch>
        <ContainerMode>
          <Code>LSE</Code>
          <Description>Air/Loose/Less than Truck Load</Description>
        </ContainerMode>
        <ShipmentStatus>
          <Code>AVL</Code>
          <Description>Available</Description>
        </ShipmentStatus>

        <InstructionCollection>
          <Instruction>
            <Address>
              <AddressType>LocalCartageImporter</AddressType>
              <Address1>PO BOX 132</Address1>
              <AddressOverride>false</AddressOverride>
              <City>RAINBOW BEACH</City>
              <CompanyName>DEANS ICEWORX PTY LTD</CompanyName>
              <Country>
                <Code>AU</Code>
                <Name>Australia</Name>
              </Country>
              <Postcode>4581</Postcode>
              <State Description=""Queensland"">QLD</State>

            </Address>
            <Sequence>1</Sequence>
            <Status>
              <Code>AVL</Code>
              <Description>Available</Description>
            </Status>
            <Type>
              <Code>PIC</Code>
              <Description>Pickup</Description>
            </Type>

            <InstructionPackingLineLinkCollection>
              <InstructionPackingLineLink>
                <PackingLineLink>0</PackingLineLink>
                <Quantity>1</Quantity>

                <ConfirmationCollection>
                  <Confirmation>
                    <DateDescription>PIC</DateDescription>
                    <Quantity>1</Quantity>
                  </Confirmation>
                </ConfirmationCollection>
              </InstructionPackingLineLink>
            </InstructionPackingLineLinkCollection>
          </Instruction>
        </InstructionCollection>

        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>TransportCompanyDocumentaryAddress</AddressType>
            <Address1>72 O'RIORDAN STREET</Address1>
            <City>ALEXANDRIA</City>
            <CompanyName>CARGOWISE EDI SYDNEY</CompanyName>
            <Country>
              <Code>AU</Code>
              <Name>Australia</Name>
            </Country>

          </OrganizationAddress>
        </OrganizationAddressCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>
";
			}
		}

		protected override LTConsignmentConsolidation GetNewBusinessObjectForTesting()
		{
			return new LTConsignmentConsolidation();
		}
		#endregion
	}
}
