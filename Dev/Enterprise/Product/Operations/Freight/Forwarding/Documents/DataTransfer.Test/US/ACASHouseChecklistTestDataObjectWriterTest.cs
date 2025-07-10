using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DataTransfer;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.US;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.US;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.US.Testing
{
	sealed class ACASHouseChecklistDataObjectWriterTest : DataObjectWriterTest
	{
		[TestDate(2018, 6, 6)]
		public void TestPopulateDataObject()
		{
			var acas = new ACASHouseChecklist("ForwardingConsol", "zzz", "ACASHouseChecklist");
			PopulateAcas(acas);
			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new ACASHouseChecklistDataObjectWriter(manager);

			var dataObject = writer.GetDataObject(acas);

			AssertUXml(dataObject, expectedXml);
		}

		void PopulateAcas(ACASHouseChecklist acas)
		{
			acas.ConsolNumber = "CONSOL";
			acas.WayBillNumber = "MAWB123";

			acas.PortOfOrigin = new DummyUnloco
			{
				Code = "AUSYD",
				Name = "Sydney",
				IATACode = "SYD"
			};

			acas.PortOfFirstArrival = new DummyUnloco
			{
				Code = "USJFK",
				Name = "New York",
				IATACode = "JFK"
			};

			acas.PortOfDestination = new DummyUnloco
			{
				Code = "USLAX",
				Name = "Los Angeles",
				IATACode = "LAX"
			};

			acas.TotalNoOfPacks = 12;
			acas.Weight = new Measurement
			{
				Value = 100,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};

			acas.Carrier = CreateAddress(nameof(acas.Carrier));
			acas.BookingParty = CreateAddress(nameof(acas.BookingParty));

			PopulateShipments(acas);
		}

		void PopulateShipments(ACASHouseChecklist acas)
		{
			acas.Shipments = new List<ACASHouseChecklistShipment>(new[]
			{
				GetACASShipment("S001", "0001", new ZDateTime(2019, 3, 10, 10, 0, 0), "Goods1", "AUSYD", "USLAX", 24, 33M),
				GetACASShipment("S002", "0002", new ZDateTime(2019, 3, 11, 8, 0, 0), "Goods2", "AUSYD", "USLAX", 44, 48M)
			});
		}

		ACASHouseChecklistShipment GetACASShipment(ZString shipmentID, ZString hawb, ZDateTime acasEventDate, ZString goodsDescription, ZString portOfOrigin, ZString portOfDestination, ZInt packs, ZDecimal weight)
		{
			var shipment = new ACASHouseChecklistShipment("ForwardingShipment", shipmentID, "ACASHouseChecklist");

			shipment.ShipmentNumber = shipmentID;
			shipment.WayBillNumber = hawb;
			shipment.ACASEventDate = acasEventDate;
			shipment.GoodsDescription = goodsDescription;
			shipment.TotalNoOfPacks = packs;

			shipment.Weight = new Measurement
			{
				Value = weight,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};

			shipment.PortOfOrigin = new DummyUnloco
			{
				Code = portOfOrigin,
				Name = portOfOrigin
			};

			shipment.PortOfDestination = new DummyUnloco
			{
				Code = portOfDestination,
				Name = portOfDestination
			};

			return shipment;
		}

		#region Expected Xml

		const string expectedXml =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>zzz</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>

    </DataContext>
    <PortOfDestination Name=""Los Angeles"">USLAX</PortOfDestination>
    <PortOfFirstArrival Name=""New York"">USJFK</PortOfFirstArrival>
    <PortOfOrigin Name=""Sydney"">AUSYD</PortOfOrigin>
    <TotalNoOfPacks>12</TotalNoOfPacks>
    <TotalWeight>100</TotalWeight>
    <TotalWeightUnit>KG</TotalWeightUnit>
    <WayBillNumber>MAWB123</WayBillNumber>
    <AddInfoCollection>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>USJFK</Value>
      </AddInfo>
    </AddInfoCollection>
    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>CONSOL</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>Carrier</AddressType>
        <AdditionalAddressInformation>Carrier additional info</AdditionalAddressInformation>
        <Address1>Carrier address line 1</Address1>
        <Address2>Carrier address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Carrier city</City>
        <CompanyName>Carrier</CompanyName>
        <Contact>Carrier contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Carrier email</Email>
        <Fax>Carrier fax</Fax>
        <GovRegNum>Carrier tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Carrier phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>Carrier po</Postcode>
        <State>Carrier state</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>BookingPartyDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>BookingParty additional info</AdditionalAddressInformation>
        <Address1>BookingParty address line 1</Address1>
        <Address2>BookingParty address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>BookingParty city</City>
        <CompanyName>BookingParty</CompanyName>
        <Contact>BookingParty contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>BookingParty email</Email>
        <Fax>BookingParty fax</Fax>
        <GovRegNum>BookingParty tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>BookingParty phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>BookingPar</Postcode>
        <State>BookingParty state</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S001</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>
        <GoodsDescription>Goods1</GoodsDescription>
        <OuterPacks>24</OuterPacks>
        <PortOfDestination Name=""USLAX"">USLAX</PortOfDestination>
        <PortOfOrigin Name=""AUSYD"">AUSYD</PortOfOrigin>
        <TotalWeight>33</TotalWeight>
        <TotalWeightUnit>KG</TotalWeightUnit>
        <WayBillNumber>0001</WayBillNumber>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S002</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>
        <GoodsDescription>Goods2</GoodsDescription>
        <OuterPacks>44</OuterPacks>
        <PortOfDestination Name=""USLAX"">USLAX</PortOfDestination>
        <PortOfOrigin Name=""AUSYD"">AUSYD</PortOfOrigin>
        <TotalWeight>48</TotalWeight>
        <TotalWeightUnit>KG</TotalWeightUnit>
        <WayBillNumber>0002</WayBillNumber>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>
";

		#endregion
	}
}
