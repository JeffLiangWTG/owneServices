using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.BE.Testing
{
	sealed class ExportNotificationWriterTest : DataObjectWriterTest
	{
		public void TestPopulateDataObject()
		{
			var exportNotification = PrepareTestData();
			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new ExportNotificationWriter(manager);
			var dataObject = writer.GetDataObject(exportNotification);

			AssertUXml(dataObject, expectedXml);

			exportNotification.TransportModeToTerminal = null;
			exportNotification.PortOfOrigin = null;

			dataObject = writer.GetDataObject(exportNotification);

			AssertUXml(dataObject, expectedXmlBlancoAddInfo);
		}

		CommonContext context;

		ExportNotification PrepareTestData()
		{
			var exportNotification = new ExportNotification("ForwardingConsol", "C20201025");

			exportNotification.ConsolNumber = "C20201025";
			exportNotification.PortOfOrigin = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "BEANR",
				Name = "Antwerp"
			};
			exportNotification.PortOfDestination = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "AUSYD",
				Name = "Sydney"
			};
			exportNotification.ContainerMode = new DummyCodeDescription { Code = "FCL", Description = "Full Container Load" };
			exportNotification.ShipmentType = new DummyCodeDescription { Code = "SEA", Description = "Sea Freight" };
			exportNotification.SendingPartyCode = CrateRegistrationNumber(OrgCusCode.CodeTypes.PortSystemNumber, "PSN111");
			exportNotification.TransportModeToTerminal = new DummyCodeDescription { Code = "ROA", Description = "Road" };
			exportNotification.VesselType = "BA";
			exportNotification.Terminal = "Terminal111";
			exportNotification.BookingReference = "BKG111";

			var packlines = new List<DocDataObjects.BE.PackingLine>();
			packlines.Add(CreatePackingLine("TBNU1111111", "VIN111", "MRN111;MRN456", "CMS111"));
			packlines.Add(CreatePackingLine("TBNU1111112", "VIN112", "MRN111", "CMS111"));
			packlines.Add(CreatePackingLine("TBNU1111113", "VIN113", "MRN111;MRN789", "CMS111"));
			packlines.Add(CreatePackingLine("TBNU2222221", "VIN211", "MRN222", "CMS222"));
			packlines.Add(CreatePackingLine("TBNU2222222", "VIN212", "MRN222", "CMS222"));
			exportNotification.PackLines = packlines;

			return exportNotification;
		}

		RegistrationNumber CrateRegistrationNumber(string type, string value)
		{
			return new RegistrationNumber()
			{
				CountryOfIssue = new Country(context.Factory, context.Countries)
				{
					Code = Core.Constants.CountryCodes.France
				},
				Type = new CodeDescription(new OrgCodeLists().CustomsCodes_List(Core.Constants.CountryCodes.France))
				{
					Code = type
				},
				Value = value
			};
		}

		DocDataObjects.BE.PackingLine CreatePackingLine(ZString containerNumber, ZString vin, ZString mrns, ZString customsOfficeCode)
		{
			var packingLine = new DocDataObjects.BE.PackingLine(1);
			packingLine.ContainerNumber = containerNumber;
			packingLine.VIN = vin;

			var movementReferenceNumbers = new List<MovementReferenceNumber>();
			foreach (var mrn in mrns.Split(';'))
			{
				var movementReferenceNumber = new MovementReferenceNumber(1);
				movementReferenceNumber.CustomsDocumentCode = new DummyCodeDescription { Code = "EXS", Description = "Exit Summary Declaration" };
				movementReferenceNumber.MRN = mrn;
				movementReferenceNumber.CustomsOfficeCode = customsOfficeCode;

				movementReferenceNumbers.Add(movementReferenceNumber);
			}

			packingLine.MovementReferenceNumbers = movementReferenceNumbers;

			return packingLine;
		}

		const string expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>C20201025</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>

      <Workflow>
        <Company>
          <Code>EDI</Code>
          <Country Name=""Australia"">AU</Country>
          <Name>Eagle Datamation International</Name>
        </Company>
        <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
        <EventDepartment Name=""Department"">BRN</EventDepartment>
        <EventUser Name=""CargoWise Support"">E</EventUser>
      </Workflow>
    </DataContext>

    <BookingConfirmationReference>BKG111</BookingConfirmationReference>
    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <PlaceOfReceipt Name=""Sydney"">AUSYD</PlaceOfReceipt>
    <PortOfLoading Name=""Antwerp"">BEANR</PortOfLoading>
    <ShipmentType Description=""Sea Freight"">SEA</ShipmentType>

    <AddInfoCollection>
      <AddInfo>
        <Key>TransportToTerminalType_Code</Key>
        <Value>ROA</Value>
      </AddInfo>
      <AddInfo>
        <Key>Terminal_Code</Key>
        <Value>Terminal111</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>BEANR</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Antwerp</Value>
      </AddInfo>
      <AddInfo>
        <Key>VesselType_Code</Key>
        <Value>BA</Value>
      </AddInfo>
      <AddInfo>
        <Key>Is_FerryTerminal</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>FormVersion</Key>
        <Value>1.0.0</Value>
      </AddInfo>
    </AddInfoCollection>

    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>C20201025</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>

    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>MRN111</Key>
            <Type>MRN</Type>
          </DataSource>
        </DataContext>


        <AddInfoCollection>
          <AddInfo>
            <Key>Entry_DocumentCode</Key>
            <Value>EXS</Value>
          </AddInfo>
          <AddInfo>
            <Key>Customs_OfficeCode</Key>
            <Value>CMS111</Value>
          </AddInfo>
          <AddInfo>
            <Key>FormVersion</Key>
            <Value>1.0.0</Value>
          </AddInfo>
        </AddInfoCollection>

        <PackingLineCollection>
          <PackingLine>
            <ContainerNumber>TBNU1111111</ContainerNumber>
            <ExportReferenceNumber>MRN111</ExportReferenceNumber>
            <ReferenceNumber>VIN111</ReferenceNumber>
          </PackingLine>
          <PackingLine>
            <ContainerNumber>TBNU1111112</ContainerNumber>
            <ExportReferenceNumber>MRN111</ExportReferenceNumber>
            <ReferenceNumber>VIN112</ReferenceNumber>
          </PackingLine>
          <PackingLine>
            <ContainerNumber>TBNU1111113</ContainerNumber>
            <ExportReferenceNumber>MRN111</ExportReferenceNumber>
            <ReferenceNumber>VIN113</ReferenceNumber>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>MRN456</Key>
            <Type>MRN</Type>
          </DataSource>
        </DataContext>


        <AddInfoCollection>
          <AddInfo>
            <Key>Entry_DocumentCode</Key>
            <Value>EXS</Value>
          </AddInfo>
          <AddInfo>
            <Key>Customs_OfficeCode</Key>
            <Value>CMS111</Value>
          </AddInfo>
          <AddInfo>
            <Key>FormVersion</Key>
            <Value>1.0.0</Value>
          </AddInfo>
        </AddInfoCollection>

        <PackingLineCollection>
          <PackingLine>
            <ContainerNumber>TBNU1111111</ContainerNumber>
            <ExportReferenceNumber>MRN456</ExportReferenceNumber>
            <ReferenceNumber>VIN111</ReferenceNumber>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>MRN789</Key>
            <Type>MRN</Type>
          </DataSource>
        </DataContext>


        <AddInfoCollection>
          <AddInfo>
            <Key>Entry_DocumentCode</Key>
            <Value>EXS</Value>
          </AddInfo>
          <AddInfo>
            <Key>Customs_OfficeCode</Key>
            <Value>CMS111</Value>
          </AddInfo>
          <AddInfo>
            <Key>FormVersion</Key>
            <Value>1.0.0</Value>
          </AddInfo>
        </AddInfoCollection>

        <PackingLineCollection>
          <PackingLine>
            <ContainerNumber>TBNU1111113</ContainerNumber>
            <ExportReferenceNumber>MRN789</ExportReferenceNumber>
            <ReferenceNumber>VIN113</ReferenceNumber>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>MRN222</Key>
            <Type>MRN</Type>
          </DataSource>
        </DataContext>


        <AddInfoCollection>
          <AddInfo>
            <Key>Entry_DocumentCode</Key>
            <Value>EXS</Value>
          </AddInfo>
          <AddInfo>
            <Key>Customs_OfficeCode</Key>
            <Value>CMS222</Value>
          </AddInfo>
          <AddInfo>
            <Key>FormVersion</Key>
            <Value>1.0.0</Value>
          </AddInfo>
        </AddInfoCollection>

        <PackingLineCollection>
          <PackingLine>
            <ContainerNumber>TBNU2222221</ContainerNumber>
            <ExportReferenceNumber>MRN222</ExportReferenceNumber>
            <ReferenceNumber>VIN211</ReferenceNumber>
          </PackingLine>
          <PackingLine>
            <ContainerNumber>TBNU2222222</ContainerNumber>
            <ExportReferenceNumber>MRN222</ExportReferenceNumber>
            <ReferenceNumber>VIN212</ReferenceNumber>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>";

		const string expectedXmlBlancoAddInfo = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>C20201025</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>

      <Workflow>
        <Company>
          <Code>EDI</Code>
          <Country Name=""Australia"">AU</Country>
          <Name>Eagle Datamation International</Name>
        </Company>
        <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
        <EventDepartment Name=""Department"">BRN</EventDepartment>
        <EventUser Name=""CargoWise Support"">E</EventUser>
      </Workflow>
    </DataContext>

    <BookingConfirmationReference>BKG111</BookingConfirmationReference>
    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <PlaceOfReceipt Name=""Sydney"">AUSYD</PlaceOfReceipt>
    <ShipmentType Description=""Sea Freight"">SEA</ShipmentType>

    <AddInfoCollection>
      <AddInfo>
        <Key>Terminal_Code</Key>
        <Value>Terminal111</Value>
      </AddInfo>
      <AddInfo>
        <Key>VesselType_Code</Key>
        <Value>BA</Value>
      </AddInfo>
      <AddInfo>
        <Key>Is_FerryTerminal</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>FormVersion</Key>
        <Value>1.0.0</Value>
      </AddInfo>
    </AddInfoCollection>

    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>C20201025</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>

    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>MRN111</Key>
            <Type>MRN</Type>
          </DataSource>
        </DataContext>


        <AddInfoCollection>
          <AddInfo>
            <Key>Entry_DocumentCode</Key>
            <Value>EXS</Value>
          </AddInfo>
          <AddInfo>
            <Key>Customs_OfficeCode</Key>
            <Value>CMS111</Value>
          </AddInfo>
          <AddInfo>
            <Key>FormVersion</Key>
            <Value>1.0.0</Value>
          </AddInfo>
        </AddInfoCollection>

        <PackingLineCollection>
          <PackingLine>
            <ContainerNumber>TBNU1111111</ContainerNumber>
            <ExportReferenceNumber>MRN111</ExportReferenceNumber>
            <ReferenceNumber>VIN111</ReferenceNumber>
          </PackingLine>
          <PackingLine>
            <ContainerNumber>TBNU1111112</ContainerNumber>
            <ExportReferenceNumber>MRN111</ExportReferenceNumber>
            <ReferenceNumber>VIN112</ReferenceNumber>
          </PackingLine>
          <PackingLine>
            <ContainerNumber>TBNU1111113</ContainerNumber>
            <ExportReferenceNumber>MRN111</ExportReferenceNumber>
            <ReferenceNumber>VIN113</ReferenceNumber>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>MRN456</Key>
            <Type>MRN</Type>
          </DataSource>
        </DataContext>


        <AddInfoCollection>
          <AddInfo>
            <Key>Entry_DocumentCode</Key>
            <Value>EXS</Value>
          </AddInfo>
          <AddInfo>
            <Key>Customs_OfficeCode</Key>
            <Value>CMS111</Value>
          </AddInfo>
          <AddInfo>
            <Key>FormVersion</Key>
            <Value>1.0.0</Value>
          </AddInfo>
        </AddInfoCollection>

        <PackingLineCollection>
          <PackingLine>
            <ContainerNumber>TBNU1111111</ContainerNumber>
            <ExportReferenceNumber>MRN456</ExportReferenceNumber>
            <ReferenceNumber>VIN111</ReferenceNumber>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>MRN789</Key>
            <Type>MRN</Type>
          </DataSource>
        </DataContext>


        <AddInfoCollection>
          <AddInfo>
            <Key>Entry_DocumentCode</Key>
            <Value>EXS</Value>
          </AddInfo>
          <AddInfo>
            <Key>Customs_OfficeCode</Key>
            <Value>CMS111</Value>
          </AddInfo>
          <AddInfo>
            <Key>FormVersion</Key>
            <Value>1.0.0</Value>
          </AddInfo>
        </AddInfoCollection>

        <PackingLineCollection>
          <PackingLine>
            <ContainerNumber>TBNU1111113</ContainerNumber>
            <ExportReferenceNumber>MRN789</ExportReferenceNumber>
            <ReferenceNumber>VIN113</ReferenceNumber>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>MRN222</Key>
            <Type>MRN</Type>
          </DataSource>
        </DataContext>


        <AddInfoCollection>
          <AddInfo>
            <Key>Entry_DocumentCode</Key>
            <Value>EXS</Value>
          </AddInfo>
          <AddInfo>
            <Key>Customs_OfficeCode</Key>
            <Value>CMS222</Value>
          </AddInfo>
          <AddInfo>
            <Key>FormVersion</Key>
            <Value>1.0.0</Value>
          </AddInfo>
        </AddInfoCollection>

        <PackingLineCollection>
          <PackingLine>
            <ContainerNumber>TBNU2222221</ContainerNumber>
            <ExportReferenceNumber>MRN222</ExportReferenceNumber>
            <ReferenceNumber>VIN211</ReferenceNumber>
          </PackingLine>
          <PackingLine>
            <ContainerNumber>TBNU2222222</ContainerNumber>
            <ExportReferenceNumber>MRN222</ExportReferenceNumber>
            <ReferenceNumber>VIN212</ReferenceNumber>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>";

		protected override void SetUp()
		{
			base.SetUp();
			context = new CommonContext(Factory);
		}
	}
}
