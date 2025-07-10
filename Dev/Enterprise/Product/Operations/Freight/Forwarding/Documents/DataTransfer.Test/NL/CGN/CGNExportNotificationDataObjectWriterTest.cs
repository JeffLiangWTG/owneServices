using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.NL;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Core;
using Address = Enterprise.DocumentVisualizer.DocDataObjects.Address;
using Country = Enterprise.DocumentVisualizer.DocDataObjects.Country;
using OrgCusCode = Enterprise.MasterFiles.Business.OrgCusCode;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;
using Shipment = Enterprise.Freight.Forwarding.Documents.DocDataObjects.Shipment;
using Unloco = Enterprise.DocumentVisualizer.DocDataObjects.Unloco;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.NL.Testing
{
	sealed class CGNExportNotificationDataObjectWriterTest : DataObjectWriterTest
	{
		public void TestPopulateDataObject()
		{
			var cgnExportNotification = PrepareTestData();

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CGNExportNotificationDataObjectWriter(manager);

			var dataObject = writer.GetDataObject(cgnExportNotification);

			AssertUXml(dataObject, expectedXml);
		}

		CGNExportNotification PrepareTestData()
		{
			var cgnExportNotification = new CGNExportNotification("ForwardingConsol", "C00001001");
			cgnExportNotification.MasterAirWaybill = "MAW-223504";

			cgnExportNotification.OperationalPort = new Unloco(Context.Factory, Context.Unlocos, Context.Countries)
			{
				Code = "NLAMS",
				Name = "Amsterdam"
			};

			var shipments = new List<Shipment>();

			var shipment1 = CreateShipment("S00001001", "BL001", "MRN001", 1939, "KG", "Kilograms", 20, "PLT", "Pallet");
			shipments.Add(shipment1);

			var shipment2 = CreateShipment("S00001002", "BL002", "MRN002", 1941, "KG", "Kilograms", 13, "PKG", "Package");
			shipments.Add(shipment2);
			cgnExportNotification.Shipments = shipments;

			var address1 = new Address(Factory);
			address1.CompanyName = "SENDING PARTY TEST NAME";
			address1.AddressLine1 = "SENDING PARTY ADDRESS 1";
			address1.AddressLine2 = "SENDING PARTY ADDRESS 2";
			address1.AdditionalAddressInformation = "SENDING PARTY ADDITIONAL ADDRESS";
			address1.City = "AMSTERDAM";
			address1.State = "NH";
			address1.Postcode = "2024";
			address1.Country = new Country(Context.Factory, Context.Countries) { Code = "NL", Name = "Netherlands" };
			address1.Unloco = new Unloco(Context.Factory, Context.Unlocos, Context.Countries) { Code = "NLAMS", Name = "Amsterdam" };
			address1.Fax = "11111111";
			address1.Phone = "22222222";
			address1.Email = "sendingparty@email.com";

			var address2 = new Address(Factory);
			address2.CompanyName = "CARRIER TEST NAME";
			address2.AddressLine1 = "CARRIER ADDRESS 1";
			address2.AddressLine2 = "CARRIER ADDRESS 2";
			address2.AdditionalAddressInformation = "CARRIER ADDITIONAL ADDRESS";
			address2.City = "AMSTERDAM";
			address2.State = "NH";
			address2.Postcode = "0408";
			address2.Country = new Country(Context.Factory, Context.Countries) { Code = "NL", Name = "Netherlands" };
			address2.Unloco = new Unloco(Context.Factory, Context.Unlocos, Context.Countries) { Code = "NLAMS", Name = "Amsterdam" };
			address2.Fax = "44444444";
			address2.Phone = "55555555";
			address2.Email = "carrier@testemail.com";

			cgnExportNotification.SendingParty = address1;
			cgnExportNotification.SendingPartyCGNNumber = CreateRegistrationNumber("CGNTEST1");
			cgnExportNotification.SendingPartyCGN = cgnExportNotification.SendingPartyCGNNumber.Value;

			cgnExportNotification.Carrier = address2;
			cgnExportNotification.CarrierCGNNumber = CreateRegistrationNumber("CGNTEST2");
			cgnExportNotification.CarrierCGN = cgnExportNotification.CarrierCGNNumber.Value;

			return cgnExportNotification;
		}

		Shipment CreateShipment(ZString shipmentId, ZString houseBillNumber, ZString mrnNumber, ZDecimal weight, ZString weightUnit, ZString weightUnitDescription, ZInt packCount, ZString packTypeCode, ZString packTypeDescription)
		{
			var shipment = new Shipment(ZGuid.NewZGuid());

			shipment.ShipmentID = shipmentId;
			shipment.HouseBillNumber = houseBillNumber;

			var grossWeight = new Measurement()
			{
				Value = weight,
				Unit = new CodeDescription(Context.WeightUnits) { Code = weightUnit, Description = weightUnitDescription }
			};

			shipment.GrossWeight = grossWeight;

			shipment.PackCount = packCount;
			shipment.PackType = new CodeDescription(Context.WeightUnits) { Code = packTypeCode, Description = packTypeDescription };

			shipment.MRNNumbers = CreateMrnNumber(mrnNumber);
			return shipment;
		}

		List<ReferenceNumber> CreateMrnNumber(ZString mrnNumber)
		{
			var referenceNumbers = new List<ReferenceNumber>();

			var types = new CodeDescriptionPairList();
			types.AddPair(CusEntryNumberTypes.Standard.MovementReferenceNumber, "Movement Reference Number");

			referenceNumbers.Add(new ReferenceNumber()
			{
				Value = mrnNumber,
				Type = new CodeDescription(types)
				{
					Code = CusEntryNumberTypes.Standard.MovementReferenceNumber,
					Description = "Movement Reference Number"
				}
			});

			return referenceNumbers;
		}

		RegistrationNumber CreateRegistrationNumber(ZString cgnNumber)
		{
			return new RegistrationNumber()
			{
				CountryOfIssue = new Country(Context.Factory, Context.Countries)
				{
					Code = Core.Constants.CountryCodes.Netherlands
				},
				Type = new CodeDescription(new OrgCodeLists().CustomsCodes_List(Core.Constants.CountryCodes.Netherlands))
				{
					Code = OrgCusCode.NetherlandsCodeTypes.CargonautRegistationCode,
					Description = "Cargonaut Registration Code",
				},
				Value = cgnNumber
			};
		}

		const string expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>C00001001</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
    </DataContext>


    <AddInfoCollection>
      <AddInfo>
        <Key>MAWB</Key>
        <Value>MAW-223504</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>NLAMS</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Amsterdam</Value>
      </AddInfo>
    </AddInfoCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>CurrentUser</AddressType>
        <AdditionalAddressInformation>SENDING PARTY ADDITIONAL ADDRESS</AdditionalAddressInformation>
        <Address1>SENDING PARTY ADDRESS 1</Address1>
        <Address2>SENDING PARTY ADDRESS 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>AMSTERDAM</City>
        <CompanyName>SENDING PARTY TEST NAME</CompanyName>
        <Contact></Contact>
        <Country Name=""Netherlands"">NL</Country>
        <Email>sendingparty@email.com</Email>
        <Fax>11111111</Fax>
        <GovRegNum></GovRegNum>
        <Phone>22222222</Phone>
        <Port Name=""Amsterdam"">NLAMS</Port>
        <Postcode>2024</Postcode>
        <State>NH</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Cargonaut Registration Code"">CGN</Type>
            <CountryOfIssue Name=""Netherlands"">NL</CountryOfIssue>
            <Value>CGNTEST1</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>Carrier</AddressType>
        <AdditionalAddressInformation>CARRIER ADDITIONAL ADDRESS</AdditionalAddressInformation>
        <Address1>CARRIER ADDRESS 1</Address1>
        <Address2>CARRIER ADDRESS 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>AMSTERDAM</City>
        <CompanyName>CARRIER TEST NAME</CompanyName>
        <Contact></Contact>
        <Country Name=""Netherlands"">NL</Country>
        <Email>carrier@testemail.com</Email>
        <Fax>44444444</Fax>
        <GovRegNum></GovRegNum>
        <Phone>55555555</Phone>
        <Port Name=""Amsterdam"">NLAMS</Port>
        <Postcode>0408</Postcode>
        <State>NH</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Cargonaut Registration Code"">CGN</Type>
            <CountryOfIssue Name=""Netherlands"">NL</CountryOfIssue>
            <Value>CGNTEST2</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S00001001</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>

        <TotalNoOfPacks>20</TotalNoOfPacks>
        <TotalNoOfPacksPackageType Description=""Pallet"">PLT</TotalNoOfPacksPackageType>
        <TotalWeight>1939</TotalWeight>
        <TotalWeightUnit Description=""Kilograms"">KG</TotalWeightUnit>
        <WayBillNumber>BL001</WayBillNumber>
        <WayBillType>HBL</WayBillType>
        <AdditionalReferenceCollection>
          <AdditionalReference>
            <Type Description=""Movement Reference Number"">MRN</Type>
            <ReferenceNumber>MRN001</ReferenceNumber>
          </AdditionalReference>
        </AdditionalReferenceCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S00001002</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>

        <TotalNoOfPacks>13</TotalNoOfPacks>
        <TotalNoOfPacksPackageType Description=""Package"">PKG</TotalNoOfPacksPackageType>
        <TotalWeight>1941</TotalWeight>
        <TotalWeightUnit Description=""Kilograms"">KG</TotalWeightUnit>
        <WayBillNumber>BL002</WayBillNumber>
        <WayBillType>HBL</WayBillType>
        <AdditionalReferenceCollection>
          <AdditionalReference>
            <Type Description=""Movement Reference Number"">MRN</Type>
            <ReferenceNumber>MRN002</ReferenceNumber>
          </AdditionalReference>
        </AdditionalReferenceCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>";
	}
}
