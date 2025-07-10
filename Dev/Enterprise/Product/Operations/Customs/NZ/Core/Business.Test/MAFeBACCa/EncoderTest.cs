namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders.Testing
{
	class EncoderTest : NUnit.Framework.TestCase
	{
		public void TestEncoder()
		{
			string encodedMessage = Base64Encoder.Base64Encode(TestXMLString);
			AssertNotEquals("The encoded message should be different from the un-encoded message.", TestXMLString, encodedMessage);
			string decodedMessage = Base64Encoder.Base64Decode(encodedMessage);
			AssertMultilineASCIIEquals("The encoded the un-encoded message should match the original.", TestXMLString, decodedMessage);
		}
		#region TestXMLString
		const string TestXMLString = @"<?xml version=""1.0"" encoding=""utf-8""?>
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
		<maf:CallerRefID>888898/48694321</maf:CallerRefID>
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
							<ebacca:CargoType>FCL</ebacca:CargoType>
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
					<ebacca:ClientReferenceNumber>123456A</ebacca:ClientReferenceNumber>
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
				<maf:FileName>Sample.pdf</maf:FileName>
				<DocumentType>ExporterDeclaration</DocumentType>
				<maf:ContentType>PDF</maf:ContentType>
				<maf:Data>
					<maf:DataEncoding>Base64</maf:DataEncoding>
					<maf:DataContent>Sea File Content</maf:DataContent>
				</maf:Data>
			</maf:File>
		</maf:Files>
	</maf:Body>
</maf:MessagingRequest>";
		#endregion
	}
}
