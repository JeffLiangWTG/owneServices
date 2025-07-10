using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	class DraftBillOfLadingDocumentTest : StandardDocumentContentTest
	{
		public override void TestDocumentContent()
		{
			var consol = CreateConsol();
			ImportDraftBillOfLadingMessage(xmlMessageRegularScenario);

			var pivotPK = new ZGuid("b53f0d42-5056-4b98-b455-3cfd49f5c07e");

			using (FreightDataRegistry.Instance.EnableDraftBillOfLadingForm.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertContents(consol, pivotPK, Content);
			}
		}

		const string Content =
@"[2,4] HAPAG-LLOYD
[2,28] DRAFT
[3,28] HLCUSZX2209BHYC4
[5,4] Shipper
[5,28] Booking No.   
[5,34] 23380355
[5,40] B/L No.
[5,46] HLCUSZX2209BHYC4
[6,4] QWERTY
[6,28] Export References
[6,40] C09140756
[7,4] 123 QWERTY STREET
[7,28] Shipper's Reference
[7,40] CON0000001084
[8,4] Building X
[8,28] Forwarding Agent References
[8,40] CYYY00692899
[9,4] ANDORRA LA VELLA
[9,16] 07
[9,24] 1234
[9,28] Shipment Method
[9,38] FCL
[9,40] Delivery Mode
[9,46] CY/CY
[10,4] Andorra
[10,28] Service Contract No.
[10,40] 64645802
[11,4] Contact: Jos Vermeulen
Tel: 987654610
Fax: 12345678900
Email: Jos.Vermeulen@qwerty.com
[13,4] Consignee
[13,28] Notify Party
[14,4] AZERTY
[14,28] HollekeBolleke
[15,4] 123 AZERTY STREET
[15,28] 123 HollekeBolleke STREET
[16,4] Building X
[16,28] Building X
[17,4] ANDORRA LA VELLA
[17,16] 07
[17,24] 1234
[17,28] ANDORRA LA VELLA
[17,40] 07
[17,48] 1234
[18,4] Andorra
[18,28] Andorra
[19,4] Contact: Jos Vermeulen
Tel: 987654610
Fax: 12345678900
Email: Jos.Vermeulen@AZERTY.com
[19,28] Contact: Jos Vermeulen
Tel: 987654610
Fax: 12345678900
Email: Jos.Vermeulen@HollekeBolleke.com
[21,4] Pre-Carriage By
[21,28] Place of Receipt
[21,40] Place of Delivery
[22,4] Sea
[22,28] BEANR-Antwerpen
[22,40] AUSYD-Sydney
[23,4] Vessel & Voyage No.
[23,18] Lloyd's No.
[23,28] Port of Loading
[23,40] Port of Discharge
[24,4] DACHAN BAY EXPRESS 004W
[24,18] XX456
[24,28] DEHAM-Hamburg
[24,40] FRPAR-Paris
[25,3] PARTICULARS FURNISHED BY SHIPPER
[27,4] Marks & Nos.
Container / Seal No.
[27,14] Kind of Packages
[27,20] Description of Goods
[27,40] Gross Weight
[27,46] Measurement
[29,14] 0 
[29,40] 272.00 KG
[29,46] 456.00 M
[32,4] AAA/SEAL1/SEAL2/SEAL3
[34,14] 0 
[34,40] 785.00 KG
[34,46] 13.00 M
[37,4] BBB/S1/S2/S3
[39,4] Freight & Charges
[39,28] Freight Payable At
[39,40] BEANR-Antwerpen
[40,4] Freight Charges: Prepaid
Destination Haulage: Prepaid
Destination Port Charge: Prepaid
Origin Haulage: Collect
Origin Port Charge: Collect

[40,28] Declared Value
[40,40] 452
[40,46] EUR
[41,28] Place And Date Of Issue
[41,40] AUSYD-Sydney 2024-05-11
[42,28] Shipped On Board Date
[42,40] 2024-05-11
[43,4] Bill Of Lading Clauses
[44,4] NoteText123
Something Else
HollekeBollekeRiebesolleke
[49,42] Created By";

		void ImportDraftBillOfLadingMessage(string xmlMessage)
		{
			var message = Factory.CreateDraftBillOfLadingMessage(xmlMessage);

			var logger = new UniversalXmlImportLogger();
			var universalFactory = new UniversalObjectFactory(Factory);

			message.ProcessUniversalMessage(universalFactory, logger);
			AssertContains("<DocumentName>Draft Bill Of Lading</DocumentName>", message.EM_MessageText, true);

			var logs = string.Join(" ", logger.Logs.Select(l => l.Message));
			AssertContains("Universal Shipment data was linked to Consol C01329220", logs, true);
		}

		sealed class UniversalXmlImportLogger : ISimpleLogger
		{
			public IEnumerable<ISimpleLog> Logs => logs;

			readonly List<ISimpleLog> logs = new List<ISimpleLog>();

			public void Clear() => logs.Clear();

			public void Log(LogType type, string message)
			{
				if (!string.IsNullOrWhiteSpace(message))
				{
					logs.Add(new SimpleLog(type, message));
				}
			}
		}

		ForwardingConsol CreateConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C01329220";
			consol.JK_MasterBillNum = "BL091042970";
			consol.JK_AgentsReference = "AR001";
			consol.JK_BookingReference = "BR001";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_CarrierContractNumber = "11111";

			Factory.Save();

			return consol;
		}

		const string xmlMessageRegularScenario = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Shipment>
		<DataContext>
			<Action>LinkOnly</Action>
			<DocumentaryOverride>
				<DocumentName>Draft Bill Of Lading</DocumentName>
			</DocumentaryOverride>
			<DataTargetCollection>
				<DataTarget>
					<Key>C01329220</Key>
					<Type>ForwardingConsol</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<BookingConfirmationReference>23380355</BookingConfirmationReference>
		<WayBillNumber>HLCUSZX2209BHYC4</WayBillNumber>
		<ContainerMode>FCL</ContainerMode>
		<DeliveryMode>PTP</DeliveryMode>
		<VesselName>DACHAN BAY EXPRESS</VesselName>
		<VoyageFlightNo>004W</VoyageFlightNo>
		<LloydsIMO>XX456</LloydsIMO>
		<PortOfDischarge>FRPAR</PortOfDischarge>
		<PortOfLoading>DEHAM</PortOfLoading>
		<PlaceOfReceipt>BEANR</PlaceOfReceipt>
		<PlaceOfDelivery>AUSYD</PlaceOfDelivery>
		<PlaceOfIssue>AUSYD</PlaceOfIssue>
		<GoodsValue>452</GoodsValue>
		<GoodsValueCurrency>EUR</GoodsValueCurrency>
		<AdditionalReferenceCollection>
			<AdditionalReference>
				<Type Description=""Shipper's Reference"">SHP</Type>
				<ReferenceNumber>CON0000001084</ReferenceNumber>
			</AdditionalReference>
			<AdditionalReference>
				<Type Description=""Freight Forwarder Reference"">FFW</Type>
				<ReferenceNumber>CYYY00692899</ReferenceNumber>
			</AdditionalReference>
			<AdditionalReference>
				<Type Description=""eHub Interchange ID"">HID</Type>
				<ReferenceNumber>C09140756</ReferenceNumber>
			</AdditionalReference>
			<AdditionalReference>
				<Type Description=""Carrier Contract Number"">CON</Type>
				<ReferenceNumber>64645802</ReferenceNumber>
			</AdditionalReference>
		</AdditionalReferenceCollection>
		<AddInfoCollection>
			<AddInfo>
				<Key>FreightPayableAt_Code</Key>
				<Value>BEANR</Value>
			</AddInfo>
			<AddInfo>
				<Key>FreightPayableAt_Name</Key>
				<Value>Antwerpen</Value>
			</AddInfo>
		</AddInfoCollection>
		<DateCollection>
			<Date>
				<Type>BillIssued</Type>
				<IsEstimate>false</IsEstimate>
				<Value>2024-05-11T00:00:00</Value>
			</Date>
			<Date>
				<Type>ShippedOnBoard</Type>
				<IsEstimate>false</IsEstimate>
				<Value>2024-05-11T00:00:00</Value>
			</Date>
		</DateCollection>
		<NoteCollection Content=""Partial"">
			<Note>
				<Description>Bill Clause Notes</Description>
				<IsCustomDescription>false</IsCustomDescription>
				<NoteText>NoteText123
Something Else
HollekeBollekeRiebesolleke</NoteText>
			</Note>
		</NoteCollection>
		<OrganizationAddressCollection>
			<OrganizationAddress>
				<AddressType>ShippingLineAddress</AddressType>
				<CompanyName>HAPAG-LLOYD</CompanyName>
				<Address1>Ballindamm 25</Address1>
				<Address2>Building X</Address2>
				<AddressOverride>false</AddressOverride>
				<City>Frankfurt</City>
				<Postcode>D-20095</Postcode>
				<State>Westfalen</State>
				<Country>DE</Country>
				<Contact>Jos Vermeulen</Contact>
				<Email>Jos.Vermeulen@hapag-lloyd.com</Email>
				<Fax>12345678900</Fax>
				<Phone>987654610</Phone>
				<RegistrationNumberCollection>
					<RegistrationNumber>
						<Type Description=""Standard Carrier Alpha Code (Sea)"">CCC</Type>
						<CountryOfIssue Name=""United States"">US</CountryOfIssue>
						<Value>HLCU</Value>
					</RegistrationNumber>
				</RegistrationNumberCollection>
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>ConsignorDocumentaryAddress</AddressType>
				<CompanyName>QWERTY</CompanyName>
				<Address1>123 QWERTY STREET</Address1>
				<Address2>Building X</Address2>
				<AddressOverride>false</AddressOverride>
				<City>ANDORRA LA VELLA</City>
				<Postcode>1234</Postcode>
				<State>07</State>
				<Country>AD</Country>
				<Contact>Jos Vermeulen</Contact>
				<Email>Jos.Vermeulen@qwerty.com</Email>
				<Fax>12345678900</Fax>
				<Phone>987654610</Phone>
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>ConsigneeDocumentaryAddress</AddressType>
				<CompanyName>AZERTY</CompanyName>
				<Address1>123 AZERTY STREET</Address1>
				<Address2>Building X</Address2>
				<AddressOverride>false</AddressOverride>
				<City>ANDORRA LA VELLA</City>
				<Postcode>1234</Postcode>
				<State>07</State>
				<Country>AD</Country>
				<Contact>Jos Vermeulen</Contact>
				<Email>Jos.Vermeulen@AZERTY.com</Email>
				<Fax>12345678900</Fax>
				<Phone>987654610</Phone>
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>NotifyParty</AddressType>
				<CompanyName>HollekeBolleke</CompanyName>
				<Address1>123 HollekeBolleke STREET</Address1>
				<Address2>Building X</Address2>
				<AddressOverride>false</AddressOverride>
				<City>ANDORRA LA VELLA</City>
				<Postcode>1234</Postcode>
				<State>07</State>
				<Country>AD</Country>
				<Contact>Jos Vermeulen</Contact>
				<Email>Jos.Vermeulen@HollekeBolleke.com</Email>
				<Fax>12345678900</Fax>
				<Phone>987654610</Phone>
			</OrganizationAddress>
		</OrganizationAddressCollection>
		<PaymentHandlingInstructionCollection>
			<PaymentHandlingInstruction>
				<Category Description=""Freight"">FRT</Category>
				<PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
			</PaymentHandlingInstruction>
			<PaymentHandlingInstruction>
				<Category Description=""Destination Haulage"">DHC</Category>
				<PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
			</PaymentHandlingInstruction>
			<PaymentHandlingInstruction>
				<Category Description=""Destination Port"">DPC</Category>
				<PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
			</PaymentHandlingInstruction>
			<PaymentHandlingInstruction>
				<Category Description=""Origin Haulage"">OHC</Category>
				<PaymentMethod Description=""Collect"">CCX</PaymentMethod>
			</PaymentHandlingInstruction>
			<PaymentHandlingInstruction>
				<Category Description=""Origin Port"">OPC</Category>
				<PaymentMethod Description=""Collect"">CCX</PaymentMethod>
			</PaymentHandlingInstruction>
		</PaymentHandlingInstructionCollection>
		<ContainerCollection>
			<Container>
				<Link>1</Link>
				<ContainerNumber>AAA</ContainerNumber>
				<ContainerCount>1</ContainerCount>
				<ContainerType>
					<Code>20FR</Code>
					<Category Description=""Flat Rack"">FLT</Category>
					<Description>Twenty foot flatrack</Description>
					<ISOCode>22P1</ISOCode>
				</ContainerType>
				<GrossWeight>272</GrossWeight>
				<WeightUnit>KG</WeightUnit>
				<VolumeCapacity>456</VolumeCapacity>
				<VolumeUnit>M</VolumeUnit>
				<Seal>SEAL1</Seal>
				<SealPartyType Description=""Carrier"">CAR</SealPartyType>
				<SecondSeal>SEAL2</SecondSeal>
				<SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
				<ThirdSeal>SEAL3</ThirdSeal>
				<ThirdSealPartyType Description=""Terminal"">CTP</ThirdSealPartyType>
			</Container>
			<Container>
				<Link>2</Link>
				<ContainerNumber>BBB</ContainerNumber>
				<ContainerCount>1</ContainerCount>
				<ContainerType>
					<Code>20FR</Code>
					<Category Description=""Flat Rack"">FLT</Category>
					<Description>Twenty foot flatrack</Description>
					<ISOCode>22P1</ISOCode>
				</ContainerType>
				<GrossWeight>785</GrossWeight>
				<WeightUnit>KG</WeightUnit>
				<VolumeCapacity>13</VolumeCapacity>
				<VolumeUnit>M</VolumeUnit>
				<Seal>S1</Seal>
				<SealPartyType Description=""Carrier"">CAR</SealPartyType>
				<SecondSeal>S2</SecondSeal>
				<SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
				<ThirdSeal>S3</ThirdSeal>
				<ThirdSealPartyType Description=""Terminal"">CTP</ThirdSealPartyType>
			</Container>
		</ContainerCollection>
		<PackingLineCollection>
			<PackingLine>
				<ContainerLink>2</ContainerLink>
				<ContainerNumber>BBB</ContainerNumber>
				<PackQty>4</PackQty>
				<PackType Description=""Pallet"">PLT</PackType>
				<DetailedDescription>BBB packline 2</DetailedDescription>
				<MarksAndNos>Marks 785</MarksAndNos>
				<UNDGCollection>
					<UNDG>
						<PackQty>11</PackQty>
						<PackType>BAG</PackType>
						<UNDGCode>0001C</UNDGCode>
						<ProperShippingName>Danger</ProperShippingName>
						<TechicalName>Technicals</TechicalName>
						<IMOClass>A</IMOClass>
						<SubLabel1>SUB1</SubLabel1>
						<SubLabel2>SUB2</SubLabel2>
						<FlashPoint>10</FlashPoint>
						<PackingGroup>III</PackingGroup>
						<MarinePollutant>Y</MarinePollutant>
						<PackedInLimitedQuantity>true</PackedInLimitedQuantity>
						<EmergencyScheduleFire>F-B</EmergencyScheduleFire>
						<EmergencyScheduleSpillage>S-X</EmergencyScheduleSpillage>
						<Weight>142.01</Weight>
						<WeightUQ Description=""Kilograms"">KG</WeightUQ>
						<Contact>
							<FullName>Jos Vermeulen</FullName>
							<Phone>+33456784512</Phone>
						</Contact>
						<Standard>IAT</Standard>
					</UNDG>
				</UNDGCollection>
			</PackingLine>
			<PackingLine>
				<ContainerLink>1</ContainerLink>
				<ContainerNumber>AAA</ContainerNumber>
				<PackQty>2</PackQty>
				<PackType Description=""Pallet"">PLT</PackType>
				<DetailedDescription>AAA packline 2</DetailedDescription>
				<MarksAndNos>Marks 785</MarksAndNos>
				<UNDGCollection>
					<UNDG>
						<PackQty>11</PackQty>
						<PackType>BAG</PackType>
						<UNDGCode>0001C</UNDGCode>
						<ProperShippingName>Danger</ProperShippingName>
						<TechicalName>Technicals</TechicalName>
						<IMOClass>A</IMOClass>
						<SubLabel1>SUB1</SubLabel1>
						<SubLabel2>SUB2</SubLabel2>
						<FlashPoint>10</FlashPoint>
						<PackingGroup>III</PackingGroup>
						<MarinePollutant>Y</MarinePollutant>
						<PackedInLimitedQuantity>true</PackedInLimitedQuantity>
						<EmergencyScheduleFire>F-B</EmergencyScheduleFire>
						<EmergencyScheduleSpillage>S-X</EmergencyScheduleSpillage>
						<Weight>142.01</Weight>
						<WeightUQ Description=""Kilograms"">KG</WeightUQ>
						<Contact>
							<FullName>Jos Vermeulen</FullName>
							<Phone>+33456784512</Phone>
						</Contact>
						<Standard>IAT</Standard>
					</UNDG>
				</UNDGCollection>
			</PackingLine>
			<PackingLine>
				<ContainerLink>2</ContainerLink>
				<ContainerNumber>BBB</ContainerNumber>
				<PackQty>3</PackQty>
				<PackType Description=""Pallet"">PLT</PackType>
				<DetailedDescription>BBB packline 1</DetailedDescription>
				<MarksAndNos>Marks 785</MarksAndNos>
				<UNDGCollection>
					<UNDG>
						<PackQty>11</PackQty>
						<PackType>BAG</PackType>
						<UNDGCode>0001C</UNDGCode>
						<ProperShippingName>Danger</ProperShippingName>
						<TechicalName>Technicals</TechicalName>
						<IMOClass>A</IMOClass>
						<SubLabel1>SUB1</SubLabel1>
						<SubLabel2>SUB2</SubLabel2>
						<FlashPoint>10</FlashPoint>
						<PackingGroup>III</PackingGroup>
						<MarinePollutant>Y</MarinePollutant>
						<PackedInLimitedQuantity>true</PackedInLimitedQuantity>
						<EmergencyScheduleFire>F-B</EmergencyScheduleFire>
						<EmergencyScheduleSpillage>S-X</EmergencyScheduleSpillage>
						<Weight>142.01</Weight>
						<WeightUQ Description=""Kilograms"">KG</WeightUQ>
						<Contact>
							<FullName>Jos Vermeulen</FullName>
							<Phone>+33456784512</Phone>
						</Contact>
						<Standard>IAT</Standard>
					</UNDG>
				</UNDGCollection>
			</PackingLine>
		</PackingLineCollection>
		<TransportLegCollection>
			<TransportLeg>
				<PortOfDischarge>DEWVN</PortOfDischarge>
				<PortOfLoading>CNDCB</PortOfLoading>
				<LegOrder>1</LegOrder>
				<EstimatedArrival>2022-11-11T03:00:00</EstimatedArrival>
				<EstimatedDeparture>2022-10-15T20:00:00</EstimatedDeparture>
				<LegType>PreCarriage</LegType>
				<TransportMode>Sea</TransportMode>
				<VGMCutOff>2022-10-13T17:00:00</VGMCutOff>
				<DocumentCutOff>2022-10-12T16:00:00</DocumentCutOff>
				<FCLCutOff>2022-10-13T17:00:00</FCLCutOff>
				<VesselLloydsIMO>9539664</VesselLloydsIMO>
				<VesselName>DACHAN BAY EXPRESS</VesselName>
				<VoyageFlightNo>004W</VoyageFlightNo>
				<Carrier>
					<AddressType>Carrier</AddressType>
					<RegistrationNumberCollection>
						<RegistrationNumber>
							<Type>CCC</Type>
							<CountryOfIssue>US</CountryOfIssue>
							<Value>HLCU</Value>
						</RegistrationNumber>
					</RegistrationNumberCollection>
				</Carrier>
			</TransportLeg>
		</TransportLegCollection>
	</Shipment>
</UniversalShipment>";
	}
}
