using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders.Testing
{
	sealed class MAFMessageSerializerTest : RequestMessageTestCase
	{
		public void TestMaxLengthsAreEnforced()
		{
			var master = Factory.New<JobDeclaration>();
			plugInSupport = new Mock<IMAFPlugInSupport>();
			plugInSupport.Setup(m => m.Master).Returns(master);
			plugInSupport.Setup(m => m.AddInfo).Returns(new NZAddInfo(master));
			plugInSupport.Setup(m => m.DocManagerInfo).Returns(master.DocManagerInfo);

			plugInSupport.Setup(m => m.AccountDetails).Returns(GetAccountDetailsMock(GetLongString(12), GetLongString(50)).Object);
			plugInSupport.Setup(m => m.ConsignmentDescription).Returns(GetLongString(100));
			plugInSupport.Setup(m => m.OriginCountry).Returns(GetLongString(2));
			plugInSupport.Setup(m => m.DischargePorts).Returns(new[] { GetLongString(5) });
			plugInSupport.Setup(m => m.Destinations).Returns(new[] { GetLongString(50) });
			plugInSupport.Setup(m => m.ShipName).Returns(GetLongString(30));
			plugInSupport.Setup(m => m.VoyageNumber).Returns(GetLongString(8));
			plugInSupport.Setup(m => m.ShippingCompany).Returns(GetLongString(50));
			plugInSupport.Setup(m => m.BillOfLadingNumbers).Returns(new[] { GetLongString(17) });
			plugInSupport.Setup(m => m.SubBillOfLadingNumbers).Returns(new[] { GetLongString(17) });

			plugInSupport.Setup(m => m.Containers).Returns(new[] { GetContainerMock(GetLongString(17), ZString.Empty).Object });

			var importerMock = GetOrganisationMock(
				ZString.Empty,
				GetLongString(50),
				GetLongString(50),
				GetLongString(50),
				GetLongString(40),
				GetLongString(7),
				GetLongString(2),
				" " + GetLongString(50) + " " + GetLongString(50) + " ",
				GetLongString(255),
				GetLongString(255),
				GetLongString(255));
			plugInSupport.Setup(m => m.Importer).Returns(importerMock.Object);

			plugInSupport.Setup(m => m.ProcessingOffice).Returns(ZString.Empty);
			plugInSupport.Setup(m => m.VoyageArrivalDate).Returns(ZDateTime.Empty);
			plugInSupport.Setup(m => m.ConsignmentType).Returns(ZString.Empty);
			plugInSupport.Setup(m => m.CargoType).Returns(ZString.Empty);
			plugInSupport.Setup(m => m.MeasurementValue).Returns(ZInt.Zero);
			plugInSupport.Setup(m => m.FlightNumber).Returns(ZString.Empty);
			plugInSupport.Setup(m => m.CustomsEntryNumber).Returns(ZInt.Zero);
			plugInSupport.Setup(m => m.IsECIWriteoff).Returns(false);
			plugInSupport.Setup(m => m.Containers).Returns(System.Array.Empty<IMAFContainer>());
			plugInSupport.Setup(m => m.Commodities).Returns(System.Array.Empty<IMAFCommodity>());

			var serializer = new MAFMessageSerializer(new MAFMessagingBO(plugInSupport.Object));
			var messageContent = serializer.Generate();
			AssertNotContains("The 'Bad' character should be stripped off the end of all fields that are overlength.", BadCharacter, messageContent);
			AssertNotContains("<ebacca:FirstName />", messageContent);
			AssertNotContains("<ebacca:LastName />", messageContent);
			plugInSupport.VerifyAll();
		}

		const string BadCharacter = "*";

		ZString GetLongString(int length)
		{
			return BadCharacter.PadLeft(length + 1, 'A');
		}

		public void TestToArrayExtensionMethods()
		{
			var value = new List<ZString>();
			AssertEquals(null, value.ToArrayLeft(12));

			value.Add("1234567890");
			AssertArrayEqualsByElements(new[] { "1234567890" }, value.ToArrayLeft(12));
			AssertArrayEqualsByElements(new[] { "1234567890" }, value.AsEnumerable().ToArray());

			value.Add("123456789012");
			value.Add("12345678901234567890");
			AssertArrayEqualsByElements(new[] { "1234567890", "123456789012", "123456789012" }, value.ToArrayLeft(12));
		}

		public void TestLeftAndToStringExtensionMethods()
		{
			var value = ZString.Empty;
			AssertEquals(null, value.Left(12, nullIfEmpty: true));
			AssertEquals(ZString.Empty, value.Left(12, nullIfEmpty: false));
			AssertEquals(null, value.ToString(true));
			AssertEquals(ZString.Empty, value.ToString(false));
			value = "1234567890";
			AssertEquals("1234567890", value.Left(12));
			value = "123456789012";
			AssertEquals("123456789012", value.Left(12).ToString(false));
			value = "12345678901234567890";
			AssertEquals("123456789012", value.Left(12).ToString(true));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSeaMessage()
		{
			#region ExpectedSeaMessage

			const string expectedSeaMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<maf:MessagingRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:maf=""http://www.maf.govt.nz/Messaging/Request/2008/03"">
	<maf:Header>
		<maf:ApplicationName>EBACCA</maf:ApplicationName>
		<maf:ApplicationVersion>1.0.0</maf:ApplicationVersion>
		<maf:DocumentType>EBACCA</maf:DocumentType>
		<maf:Sender>
			<maf:Name>A Sender</maf:Name>
			<maf:EndPointType>Email</maf:EndPointType>
			<maf:Address>sender.email@domain.co.nz</maf:Address>
		</maf:Sender>
		<maf:CallerRefID>||-MESSAGE NUMBER PLACE HOLDER-||</maf:CallerRefID>
	</maf:Header>
	<maf:Body>
		<maf:MetaData>
			<ebacca:EBACCARequest xmlns:ebacca=""http://www.maf.govt.nz/EBACCA/Messaging/Request/2008/03/"">
				<ebacca:Broker>
					<ebacca:OrganisationCode>00231456A</ebacca:OrganisationCode>
					<ebacca:OrganisationName>A Broker New Zealand Ltd</ebacca:OrganisationName>
					<ebacca:Address>
						<ebacca:AddressLine1>11 Green Rd</ebacca:AddressLine1>
						<ebacca:City>NZ 1010</ebacca:City>
						<ebacca:Country>NZ</ebacca:Country>
					</ebacca:Address>
					<ebacca:ContactMethods>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>EMail</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>Broker@shipping.co.nz</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
					</ebacca:ContactMethods>
					<ebacca:ContactPerson>
						<ebacca:FirstName>Person</ebacca:FirstName>
						<ebacca:LastName>Broker</ebacca:LastName>
					</ebacca:ContactPerson>
				</ebacca:Broker>
				<ebacca:Importer>
					<ebacca:OrganisationCode>654321C</ebacca:OrganisationCode>
					<ebacca:OrganisationName>ACME Importers</ebacca:OrganisationName>
					<ebacca:Address>
						<ebacca:AddressLine1>12 Summer St</ebacca:AddressLine1>
						<ebacca:City>Auckland, New Zealand</ebacca:City>
						<ebacca:Country>NZ</ebacca:Country>
					</ebacca:Address>
					<ebacca:ContactMethods>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>Phone</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>09 123 1234</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>Fax</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>09 321 1234</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>EMail</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>a.importer@acme.co.nz</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
					</ebacca:ContactMethods>
					<ebacca:ContactPerson>
						<ebacca:FirstName>Person</ebacca:FirstName>
						<ebacca:LastName>Importer</ebacca:LastName>
					</ebacca:ContactPerson>
				</ebacca:Importer>
				<ebacca:Exporter>
					<ebacca:OrganisationCode>123456F</ebacca:OrganisationCode>
					<ebacca:OrganisationName>Fun Exports</ebacca:OrganisationName>
					<ebacca:Address>
						<ebacca:AddressLine1>1 Kangaroo Drive</ebacca:AddressLine1>
						<ebacca:City>Tauranga, Australia</ebacca:City>
						<ebacca:Country>AU</ebacca:Country>
					</ebacca:Address>
					<ebacca:ContactMethods>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>Phone</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>.</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
					</ebacca:ContactMethods>
					<ebacca:ContactPerson>
						<ebacca:FirstName>Person</ebacca:FirstName>
						<ebacca:LastName>Exporter</ebacca:LastName>
					</ebacca:ContactPerson>
				</ebacca:Exporter>
				<ebacca:Details>
					<ebacca:ConsignmentType>PrivateCargo</ebacca:ConsignmentType>
					<ebacca:Shipment>
						<ebacca:OriginCountry>AU</ebacca:OriginCountry>
						<ebacca:DischargePorts>
							<ebacca:DischargePort>NZAKL</ebacca:DischargePort>
						</ebacca:DischargePorts>
						<ebacca:Destinations>
							<ebacca:Destination>NZBLU</ebacca:Destination>
						</ebacca:Destinations>
						<ebacca:Voyage>
							<ebacca:ShipName>DIRECT BANANA</ebacca:ShipName>
							<ebacca:VoyageNumber>123</ebacca:VoyageNumber>
							<ebacca:ShippingCompany>Mersk Shipping Company</ebacca:ShippingCompany>
							<ebacca:CargoType>LCL</ebacca:CargoType>
						</ebacca:Voyage>
						<ebacca:Identifiers>
							<ebacca:BillOfLadings>
								<ebacca:BillOfLadingNumber>3463654</ebacca:BillOfLadingNumber>
							</ebacca:BillOfLadings>
							<ebacca:SubBillOfLadings>
								<ebacca:SubBillOfLadingNumber>11111111</ebacca:SubBillOfLadingNumber>
							</ebacca:SubBillOfLadings>
							<ebacca:Containers>
								<ebacca:Container>
									<ebacca:ContainerNumber>121212121222</ebacca:ContainerNumber>
									<ebacca:ContainerType>OpenTop40ft</ebacca:ContainerType>
								</ebacca:Container>
								<ebacca:Container>
									<ebacca:ContainerNumber>272727272722</ebacca:ContainerNumber>
									<ebacca:ContainerType>General20ft</ebacca:ContainerType>
								</ebacca:Container>
							</ebacca:Containers>
						</ebacca:Identifiers>
					</ebacca:Shipment>
					<ebacca:ConsignmentDescription>shoes</ebacca:ConsignmentDescription>
					<ebacca:ConsignmentMeasurement>
						<ebacca:MeasurementUnitQualifer>cartons</ebacca:MeasurementUnitQualifer>
						<ebacca:MeasurementValue>100</ebacca:MeasurementValue>
					</ebacca:ConsignmentMeasurement>
					<ebacca:Commodities>
						<ebacca:Commodity>
							<ebacca:GoodsType>VHC</ebacca:GoodsType>
							<ebacca:GoodsDescription>shoes</ebacca:GoodsDescription>
							<ebacca:GoodsMeasurements>
								<ebacca:GoodsMeasurement>
									<ebacca:MeasurementUnitQualifer>kilograms</ebacca:MeasurementUnitQualifer>
									<ebacca:MeasurementValue>1000</ebacca:MeasurementValue>
								</ebacca:GoodsMeasurement>
								<ebacca:GoodsMeasurement>
									<ebacca:MeasurementUnitQualifer>metricTonnes</ebacca:MeasurementUnitQualifer>
									<ebacca:MeasurementValue>3</ebacca:MeasurementValue>
								</ebacca:GoodsMeasurement>
							</ebacca:GoodsMeasurements>
							<ebacca:IsNew>true</ebacca:IsNew>
							<ebacca:TariffCodes>
								<ebacca:TariffCode>6403991949C</ebacca:TariffCode>
							</ebacca:TariffCodes>
						</ebacca:Commodity>
					</ebacca:Commodities>
					<ebacca:MAFProcessingOffice>Auckland</ebacca:MAFProcessingOffice>
				</ebacca:Details>
				<ebacca:References>
					<ebacca:MAF>
						<ebacca:ConsignmentNumber>11111111</ebacca:ConsignmentNumber>
					</ebacca:MAF>
					<ebacca:ClientReferenceNumber>||-SENDERS REFERENCE PLACE HOLDER-||</ebacca:ClientReferenceNumber>
				</ebacca:References>
				<ebacca:PaymentDetails>
					<ebacca:AlternativePaymentMethod>Other</ebacca:AlternativePaymentMethod>
				</ebacca:PaymentDetails>
				<ebacca:TransitionalFacility>
					<ebacca:OrganisationCode>.</ebacca:OrganisationCode>
					<ebacca:OrganisationName>Smiths Tranistional</ebacca:OrganisationName>
					<ebacca:Address>
						<ebacca:AddressLine1>23 Smith Street</ebacca:AddressLine1>
						<ebacca:City>Auckland, New Zealand</ebacca:City>
						<ebacca:Country>NZ</ebacca:Country>
					</ebacca:Address>
				</ebacca:TransitionalFacility>
			</ebacca:EBACCARequest>
		</maf:MetaData>
		<maf:Files>
			<maf:File>
				<maf:FileName>Sample.PDF</maf:FileName>
				<DocumentType>ExporterDeclaration</DocumentType>
				<maf:ContentType>PDF</maf:ContentType>
				<maf:Data>
					<maf:DataEncoding>Base64</maf:DataEncoding>
					<maf:DataContent>JVBERiBTZWEgRmlsZSBDb250ZW50ICVFT0YK</maf:DataContent>
				</maf:Data>
			</maf:File>
		</maf:Files>
	</maf:Body>
</maf:MessagingRequest>";
			#endregion

			var messageData = GetSeaValidData();
			var messageBuilder = new MAFMessageSerializer(messageData);
			AssertXMLEquals("Message one from ECN", expectedSeaMessage, messageBuilder.Generate());
			plugInSupport.VerifyAll();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAirMessage()
		{
			#region ExpectedAirMessage

			const string expectedAirMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<maf:MessagingRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:maf=""http://www.maf.govt.nz/Messaging/Request/2008/03"">
	<maf:Header>
		<maf:ApplicationName>EBACCA</maf:ApplicationName>
		<maf:ApplicationVersion>1.0.0</maf:ApplicationVersion>
		<maf:DocumentType>EBACCA</maf:DocumentType>
		<maf:Sender>
			<maf:Name>EDI DEMONSTRATION SYSTEM NZ</maf:Name>
			<maf:EndPointType>Email</maf:EndPointType>
			<maf:Address>enterprise_bjg@enterprisedevelopment.cargowise.com</maf:Address>
		</maf:Sender>
		<maf:CallerRefID>||-MESSAGE NUMBER PLACE HOLDER-||</maf:CallerRefID>
	</maf:Header>
	<maf:Body>
		<maf:MetaData>
			<ebacca:EBACCARequest xmlns:ebacca=""http://www.maf.govt.nz/EBACCA/Messaging/Request/2008/03/"">
				<ebacca:Broker>
					<ebacca:OrganisationCode>00009917B</ebacca:OrganisationCode>
					<ebacca:OrganisationName>EDI DEMONSTRATION SYSTEM NZ</ebacca:OrganisationName>
					<ebacca:Address>
						<ebacca:AddressLine1>Unit 3a 72 O'Riordan Street</ebacca:AddressLine1>
						<ebacca:City>Alexandria, NSW</ebacca:City>
						<ebacca:PostalCode>2015</ebacca:PostalCode>
						<ebacca:Country>AU</ebacca:Country>
					</ebacca:Address>
					<ebacca:ContactMethods>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>EMail</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>ben.govett@cargowise.com</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
					</ebacca:ContactMethods>
					<ebacca:ContactPerson>
						<ebacca:FirstName>Ben</ebacca:FirstName>
						<ebacca:LastName>Govett</ebacca:LastName>
					</ebacca:ContactPerson>
				</ebacca:Broker>
				<ebacca:Importer>
					<ebacca:OrganisationCode>00782903F</ebacca:OrganisationCode>
					<ebacca:OrganisationName>ADULT BOOKS LTD</ebacca:OrganisationName>
					<ebacca:Address>
						<ebacca:AddressLine1>TEST CODE FOR NZ CUSTOMS</ebacca:AddressLine1>
						<ebacca:AddressLine2>LOCATED IN NZAKL</ebacca:AddressLine2>
						<ebacca:City>AUCKLAND</ebacca:City>
						<ebacca:PostalCode>0000</ebacca:PostalCode>
						<ebacca:Country>NZ</ebacca:Country>
					</ebacca:Address>
					<ebacca:ContactMethods>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>EMail</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>test.importer@adultbooks.co.nz</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
					</ebacca:ContactMethods>
					<ebacca:ContactPerson>
						<ebacca:FirstName>Test</ebacca:FirstName>
						<ebacca:LastName>Importer</ebacca:LastName>
					</ebacca:ContactPerson>
				</ebacca:Importer>
				<ebacca:Exporter>
					<ebacca:OrganisationCode>00710841Y</ebacca:OrganisationCode>
					<ebacca:OrganisationName>TEST SUPPLIER AU</ebacca:OrganisationName>
					<ebacca:Address>
						<ebacca:AddressLine1>TEST CODE FOR NZ CUSTOMS</ebacca:AddressLine1>
						<ebacca:AddressLine2>LOCATED IN AUSYD</ebacca:AddressLine2>
						<ebacca:City>SYDNEY</ebacca:City>
						<ebacca:PostalCode>2015</ebacca:PostalCode>
						<ebacca:Country>AU</ebacca:Country>
					</ebacca:Address>
					<ebacca:ContactMethods>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>EMail</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>major.exporter@tessupsyd.com.au</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
					</ebacca:ContactMethods>
					<ebacca:ContactPerson>
						<ebacca:FirstName>Major</ebacca:FirstName>
						<ebacca:LastName>Exporter</ebacca:LastName>
					</ebacca:ContactPerson>
				</ebacca:Exporter>
				<ebacca:Details>
					<ebacca:ConsignmentType>PrivateCargo</ebacca:ConsignmentType>
					<ebacca:Shipment>
						<ebacca:OriginCountry>AU</ebacca:OriginCountry>
						<ebacca:DischargePorts>
							<ebacca:DischargePort>NZAKL</ebacca:DischargePort>
						</ebacca:DischargePorts>
						<ebacca:Destinations>
							<ebacca:Destination>NZAKL</ebacca:Destination>
						</ebacca:Destinations>
						<ebacca:Flight>
							<ebacca:FlightNumber>QF253</ebacca:FlightNumber>
							<ebacca:FlightArrivalDate>2009-02-02</ebacca:FlightArrivalDate>
						</ebacca:Flight>
						<ebacca:Identifiers>
							<ebacca:BillOfLadings>
								<ebacca:BillOfLadingNumber>081-11111111</ebacca:BillOfLadingNumber>
							</ebacca:BillOfLadings>
							<ebacca:SubBillOfLadings>
								<ebacca:SubBillOfLadingNumber>HB01010101</ebacca:SubBillOfLadingNumber>
							</ebacca:SubBillOfLadings>
						</ebacca:Identifiers>
					</ebacca:Shipment>
					<ebacca:ConsignmentDescription>UNQUALIFIED GROMMETS</ebacca:ConsignmentDescription>
					<ebacca:ConsignmentMeasurement>
						<ebacca:MeasurementUnitQualifer>bunch</ebacca:MeasurementUnitQualifer>
						<ebacca:MeasurementValue>12</ebacca:MeasurementValue>
					</ebacca:ConsignmentMeasurement>
					<ebacca:Commodities>
						<ebacca:Commodity>
							<ebacca:GoodsType>MSC</ebacca:GoodsType>
							<ebacca:GoodsDescription>GROMMETS THAT CAN'T SURF</ebacca:GoodsDescription>
							<ebacca:GoodsMeasurements>
								<ebacca:GoodsMeasurement>
									<ebacca:MeasurementUnitQualifer>cage</ebacca:MeasurementUnitQualifer>
									<ebacca:MeasurementValue>1</ebacca:MeasurementValue>
								</ebacca:GoodsMeasurement>
								<ebacca:GoodsMeasurement>
									<ebacca:MeasurementUnitQualifer>sacks</ebacca:MeasurementUnitQualifer>
									<ebacca:MeasurementValue>2</ebacca:MeasurementValue>
								</ebacca:GoodsMeasurement>
							</ebacca:GoodsMeasurements>
							<ebacca:IsNew>false</ebacca:IsNew>
							<ebacca:TariffCodes>
								<ebacca:TariffCode>4016930000F</ebacca:TariffCode>
							</ebacca:TariffCodes>
						</ebacca:Commodity>
					</ebacca:Commodities>
					<ebacca:MAFProcessingOffice>Auckland</ebacca:MAFProcessingOffice>
				</ebacca:Details>
				<ebacca:Comments>I HOPE THIS TEST WORKS.</ebacca:Comments>
				<ebacca:References>
					<ebacca:MAF>
						<ebacca:ReceiptNumber>A123B456C7</ebacca:ReceiptNumber>
					</ebacca:MAF>
					<ebacca:Customs>
						<ebacca:EntryNumber>72334537</ebacca:EntryNumber>
						<ebacca:EntryType>IE</ebacca:EntryType>
					</ebacca:Customs>
					<ebacca:ClientReferenceNumber>||-SENDERS REFERENCE PLACE HOLDER-||</ebacca:ClientReferenceNumber>
				</ebacca:References>
				<ebacca:PaymentDetails>
					<ebacca:Account>
						<ebacca:AccountHolderName>ADULT BOOKS LTD</ebacca:AccountHolderName>
						<ebacca:AccountNumber>AB123</ebacca:AccountNumber>
					</ebacca:Account>
				</ebacca:PaymentDetails>
				<ebacca:TransitionalFacility>
					<ebacca:OrganisationCode>698</ebacca:OrganisationCode>
					<ebacca:OrganisationName>A Hartrodt NZ Ltd</ebacca:OrganisationName>
					<ebacca:Address>
						<ebacca:AddressLine1>Unit 5 / 197 Montgomerie Road</ebacca:AddressLine1>
						<ebacca:City>Airport Oaks, Auckland</ebacca:City>
						<ebacca:Country>NZ</ebacca:Country>
					</ebacca:Address>
				</ebacca:TransitionalFacility>
			</ebacca:EBACCARequest>
		</maf:MetaData>
		<maf:Files>
			<maf:File>
				<maf:FileName>Sample.PDF</maf:FileName>
				<DocumentType>ExporterDeclaration</DocumentType>
				<maf:ContentType>PDF</maf:ContentType>
				<maf:Data>
					<maf:DataEncoding>Base64</maf:DataEncoding>
					<maf:DataContent>JVBERiBBaXIgRmlsZSBDb250ZW50ICVFT0YK</maf:DataContent>
				</maf:Data>
			</maf:File>
		</maf:Files>
	</maf:Body>
</maf:MessagingRequest>";

			#endregion

			var messageData = GetAirValidData();
			var messageBuilder = new MAFMessageSerializer(messageData);
			AssertXMLEquals("Message one from ECN", expectedAirMessage, messageBuilder.Generate());
			plugInSupport.VerifyAll();
		}
	}
}
