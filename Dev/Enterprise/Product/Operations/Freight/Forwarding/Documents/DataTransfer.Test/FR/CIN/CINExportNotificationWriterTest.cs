using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.FR.Testing
{
	sealed class CINExportNotificationWriterTest : DataObjectWriterTest
	{
		public void TestPopulateDataObject()
		{
			var exportNotification = PrepareTestData();
			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CINExportNotificationWriter(manager);

			CodeDescriptionPairList pairs = new CodeDescriptionPairList();
			pairs.AddPair(ZGuid.NewZGuid(), Core.Constants.AgentType.Direct, Core.Constants.AgentType.Direct);
			pairs.AddPair(ZGuid.NewZGuid(), Core.Constants.AgentType.Agent, Core.Constants.AgentType.Agent);
			pairs.AddPair(ZGuid.NewZGuid(), "Code3", "Description 3");
			exportNotification.AgentTypes = new List<CodeDescription> { new CodeDescription(pairs) { Code = Core.Constants.AgentType.Agent, Description = Core.Constants.AgentType.Agent } };
			var dataObject = writer.GetDataObject(exportNotification);
			AssertUXml(dataObject, ExpectedXml(exportNotification.HouseBill, ShippingInstructionReleaseTypes.Descriptions.HouseBill, ShippingInstructionReleaseTypes.Codes.HouseBill));

			exportNotification.AgentTypes = new List<CodeDescription> { new CodeDescription(pairs) { Code = Core.Constants.AgentType.Agent, Description = Core.Constants.AgentType.Agent },
																		new CodeDescription(pairs) { Code = Core.Constants.AgentType.Direct, Description = Core.Constants.AgentType.Direct } };
			dataObject = writer.GetDataObject(exportNotification);
			AssertUXml(dataObject, ExpectedXml(exportNotification.MasterBill, ShippingInstructionReleaseTypes.Descriptions.AirWaybill, ShippingInstructionReleaseTypes.Codes.AirWaybill));
		}

		CINExportNotification PrepareTestData()
		{
			var exportNotification = new CINExportNotification("ForwardingShipment", "S00001001");

			exportNotification.ShipmentID = "S00001001";
			exportNotification.MasterBill = "11122222222";
			exportNotification.MasterBillWithPrefix = "111-22222222";
			exportNotification.HouseBill = "h001";
			exportNotification.MRNNumbers = new[]
			{
				new ReferenceNumber
				{
					Value = "MRNNumber1",
					Type = new DummyCodeDescription
					{
						Code = "MRN"
					},
					CountryOfIssue = new DummyCountry
					{
						Code = "FR",
						Name = "France"
					}
				},
				new ReferenceNumber
				{
					Value = "MRNNumber2",
					Type = new DummyCodeDescription
					{
						Code = "MRN"
					},
					CountryOfIssue = new DummyCountry
					{
						Code = "FR",
						Name = "France"
					}
				}
			};
			exportNotification.CustomsOfficeCode = "CON001";
			exportNotification.PackCount = 2;
			exportNotification.PackType = new DummyCodeDescription
			{
				Code = "PKG",
				Description = "Package"
			};
			exportNotification.GrossWeight = new Measurement()
			{
				Value = 52240.77,
				Unit = new DummyCodeDescription
				{
					Code = "KG",
					Description = "Kilograms"
				}
			};
			exportNotification.SendingParty = CreateAddress(nameof(exportNotification.SendingParty));
			exportNotification.SendingPartyCINNumber = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.CIN, "SendingPartyCINCode");
			exportNotification.SendingPartyCIN = exportNotification.SendingPartyCINNumber.Value;
			exportNotification.Carrier = CreateAddress(nameof(exportNotification.Carrier));
			exportNotification.CarrierCINNumber = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.CIN, "CarrierCINCode");
			exportNotification.CarrierCIN = exportNotification.CarrierCINNumber.Value;
			exportNotification.Warehouse = CreateAddress(nameof(exportNotification.Warehouse));
			exportNotification.WarehouseCINNumber = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.CIN, "WarehouseCINCode");
			exportNotification.WarehouseCIN = exportNotification.WarehouseCINNumber.Value;
			return exportNotification;
		}

		string ExpectedXml(string number, string description, string code)
		{
			return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>S00001001</Key>
        <Type>ForwardingShipment</Type>
      </DataSource>
    </DataContext>

    <TotalNoOfPacks>2</TotalNoOfPacks>
    <TotalNoOfPacksPackageType Description=""Package"">PKG</TotalNoOfPacksPackageType>
    <TotalWeight>52240.77</TotalWeight>
    <TotalWeightUnit Description=""Kilograms"">KG</TotalWeightUnit>
    <WayBillNumber>{number}</WayBillNumber>
    <WayBillType Description=""{description}"">{code}</WayBillType>


    <AddInfoCollection>
      <AddInfo>
        <Key>MAWB</Key>
        <Value>11122222222</Value>
      </AddInfo>
    </AddInfoCollection>

    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Movement Reference Number"">MRN</Type>
        <ReferenceNumber>MRNNumber1</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Movement Reference Number"">MRN</Type>
        <ReferenceNumber>MRNNumber2</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Customs Office Code"">COC</Type>
        <ReferenceNumber>CON001</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>CurrentUser</AddressType>
        <AdditionalAddressInformation>SendingParty additional info</AdditionalAddressInformation>
        <Address1>SendingParty address line 1</Address1>
        <Address2>SendingParty address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SendingParty city</City>
        <CompanyName>SendingParty</CompanyName>
        <Contact>SendingParty contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>SendingParty email</Email>
        <Fax>SendingParty fax</Fax>
        <GovRegNum>SendingParty tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>SendingParty phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>SendingPar</Postcode>
        <State>SendingParty state</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""CIN Registration Code"">CIN</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SendingPartyCINCode</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
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
            <Type Description=""CIN Registration Code"">CIN</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>CarrierCINCode</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>DepartureCFSAddress</AddressType>
        <AdditionalAddressInformation>Warehouse additional info</AdditionalAddressInformation>
        <Address1>Warehouse address line 1</Address1>
        <Address2>Warehouse address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Warehouse city</City>
        <CompanyName>Warehouse</CompanyName>
        <Contact>Warehouse contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Warehouse email</Email>
        <Fax>Warehouse fax</Fax>
        <GovRegNum>Warehouse tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Warehouse phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>Warehouse </Postcode>
        <State>Warehouse state</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""CIN Registration Code"">CIN</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>WarehouseCINCode</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";
		}
	}
}
