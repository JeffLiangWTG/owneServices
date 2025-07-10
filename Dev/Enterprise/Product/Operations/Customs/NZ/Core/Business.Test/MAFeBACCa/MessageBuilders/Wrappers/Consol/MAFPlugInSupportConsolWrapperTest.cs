namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders.Testing
{
	using System;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.Testing;
	using Enterprise.Customs.NZ.Business.Testing;
	using Enterprise.Customs.NZ.Business.TradeSingleWindow;
	using Enterprise.Freight.Forwarding.Business;
	using NUnit.Framework;
	using Registry;

	class MAFPlugInSupportConsolWrapperTest : TestCaseWithFactory
	{
		public void TestFullMessage()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "BX");
			var consol = Factory.New<ForwardingConsol>();
			new TestDataBuilder(consol.Factory).PopulateConsolThatPassesValidation(consol);
			var messageData = TestDataBuilder.GetMAFMessaging(consol);
			var messageBuilder = new MAFMessageSerializer(messageData);

			AssertXMLEquals("messageBuilder.Generate()",
@"<?xml version=""1.0"" encoding=""utf-8""?>
<maf:MessagingRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:maf=""http://www.maf.govt.nz/Messaging/Request/2008/03"">
	<maf:Header>
		<maf:ApplicationName>EBACCA</maf:ApplicationName>
		<maf:ApplicationVersion>1.0.0</maf:ApplicationVersion>
		<maf:DocumentType>EBACCA</maf:DocumentType>
		<maf:Sender>
			<maf:Name>CARGOWISE BROKERS</maf:Name>
			<maf:EndPointType>Email</maf:EndPointType>
			<maf:Address>cargowiseone@brokers.cargowise.com</maf:Address>
		</maf:Sender>
		<maf:CallerRefID>||-MESSAGE NUMBER PLACE HOLDER-||</maf:CallerRefID>
	</maf:Header>
	<maf:Body>
		<maf:MetaData>
			<ebacca:EBACCARequest xmlns:ebacca=""http://www.maf.govt.nz/EBACCA/Messaging/Request/2008/03/"">
				<ebacca:Broker>
					<ebacca:OrganisationCode>00009915B</ebacca:OrganisationCode>
					<ebacca:OrganisationName>CARGOWISE BROKERS</ebacca:OrganisationName>
					<ebacca:Address>
						<ebacca:AddressLine1>72 BROKER STREET</ebacca:AddressLine1>
						<ebacca:AddressLine2>BROKERS TOWER</ebacca:AddressLine2>
						<ebacca:City>BROKERVILLE</ebacca:City>
						<ebacca:PostalCode>2015</ebacca:PostalCode>
						<ebacca:Country>NZ</ebacca:Country>
					</ebacca:Address>
					<ebacca:ContactMethods>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>Phone</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>44556677</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>Fax</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>99887766</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>EMail</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>broker@brokers.cargowise.com</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
					</ebacca:ContactMethods>
					<ebacca:ContactPerson>
						<ebacca:FirstName>Sidney</ebacca:FirstName>
						<ebacca:LastName>Broker</ebacca:LastName>
					</ebacca:ContactPerson>
				</ebacca:Broker>
				<ebacca:Importer>
					<ebacca:OrganisationCode>00112233F</ebacca:OrganisationCode>
					<ebacca:OrganisationName>IMPORTER INCORPORATED</ebacca:OrganisationName>
					<ebacca:Address>
						<ebacca:AddressLine1>77 IMPORTER AVENUE</ebacca:AddressLine1>
						<ebacca:AddressLine2>IMPORTER SPIRE</ebacca:AddressLine2>
						<ebacca:City>IMPORTERVILLE</ebacca:City>
						<ebacca:PostalCode>0232</ebacca:PostalCode>
						<ebacca:Country>NZ</ebacca:Country>
					</ebacca:Address>
					<ebacca:ContactMethods>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>Phone</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>22222222</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>Fax</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>11111111</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>EMail</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>importer@importer.co.nz</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
					</ebacca:ContactMethods>
					<ebacca:ContactPerson>
						<ebacca:FirstName>Test</ebacca:FirstName>
						<ebacca:LastName>Importer</ebacca:LastName>
					</ebacca:ContactPerson>
				</ebacca:Importer>
				<ebacca:Exporter>
					<ebacca:OrganisationCode>00998877Z</ebacca:OrganisationCode>
					<ebacca:OrganisationName>SUPPLIER PTY LTD</ebacca:OrganisationName>
					<ebacca:Address>
						<ebacca:AddressLine1>98 SUPPLIER CIRCUIT</ebacca:AddressLine1>
						<ebacca:AddressLine2>SUPPLIERHAVEN</ebacca:AddressLine2>
						<ebacca:City>SUPPLIER HILL</ebacca:City>
						<ebacca:PostalCode>2234</ebacca:PostalCode>
						<ebacca:Country>AU</ebacca:Country>
					</ebacca:Address>
				</ebacca:Exporter>
				<ebacca:Details>
					<ebacca:ConsignmentType>CommercialCargo</ebacca:ConsignmentType>
					<ebacca:Shipment>
						<ebacca:OriginCountry>AU</ebacca:OriginCountry>
						<ebacca:DischargePorts>
							<ebacca:DischargePort>NZCHC</ebacca:DischargePort>
						</ebacca:DischargePorts>
						<ebacca:Destinations>
							<ebacca:Destination>NZNPE</ebacca:Destination>
						</ebacca:Destinations>
						<ebacca:Voyage>
							<ebacca:ShipName>BUNGA DELIMA</ebacca:ShipName>
							<ebacca:VoyageNumber>3599</ebacca:VoyageNumber>
							<ebacca:ShippingCompany>IMPORTER INCORPORATED</ebacca:ShippingCompany>
							<ebacca:VoyageArrivalDate>2009-02-01</ebacca:VoyageArrivalDate>
							<ebacca:CargoType>FAK</ebacca:CargoType>
						</ebacca:Voyage>
						<ebacca:Identifiers>
							<ebacca:BillOfLadings>
								<ebacca:BillOfLadingNumber>BR298032</ebacca:BillOfLadingNumber>
							</ebacca:BillOfLadings>
							<ebacca:SubBillOfLadings>
								<ebacca:SubBillOfLadingNumber>BOL01010101</ebacca:SubBillOfLadingNumber>
								<ebacca:SubBillOfLadingNumber>BOL01010102</ebacca:SubBillOfLadingNumber>
							</ebacca:SubBillOfLadings>
							<ebacca:Containers>
								<ebacca:Container>
									<ebacca:ContainerNumber>OOCL0000023</ebacca:ContainerNumber>
									<ebacca:ContainerType>MixedContainerTypes</ebacca:ContainerType>
								</ebacca:Container>
							</ebacca:Containers>
						</ebacca:Identifiers>
					</ebacca:Shipment>
					<ebacca:ConsignmentDescription>FAK</ebacca:ConsignmentDescription>
					<ebacca:ConsignmentMeasurement>
						<ebacca:MeasurementUnitQualifer>unit</ebacca:MeasurementUnitQualifer>
						<ebacca:MeasurementValue>24</ebacca:MeasurementValue>
					</ebacca:ConsignmentMeasurement>
					<ebacca:Commodities>
						<ebacca:Commodity>
							<ebacca:GoodsType>MSC</ebacca:GoodsType>
							<ebacca:GoodsDescription>FAK</ebacca:GoodsDescription>
							<ebacca:GoodsMeasurements>
								<ebacca:GoodsMeasurement>
									<ebacca:MeasurementUnitQualifer>unit</ebacca:MeasurementUnitQualifer>
									<ebacca:MeasurementValue>24</ebacca:MeasurementValue>
								</ebacca:GoodsMeasurement>
							</ebacca:GoodsMeasurements>
							<ebacca:IsNew>false</ebacca:IsNew>
						</ebacca:Commodity>
					</ebacca:Commodities>
					<ebacca:MAFProcessingOffice>Christchurch</ebacca:MAFProcessingOffice>
				</ebacca:Details>
				<ebacca:References>
					<ebacca:ClientReferenceNumber>||-SENDERS REFERENCE PLACE HOLDER-||</ebacca:ClientReferenceNumber>
				</ebacca:References>
				<ebacca:PaymentDetails>
					<ebacca:Account>
						<ebacca:AccountHolderName>EDI CUSTOMS BROKERS</ebacca:AccountHolderName>
						<ebacca:AccountNumber>AB123</ebacca:AccountNumber>
					</ebacca:Account>
				</ebacca:PaymentDetails>
				<ebacca:TransitionalFacility>
					<ebacca:OrganisationCode>TF1</ebacca:OrganisationCode>
					<ebacca:OrganisationName>TRANSITIONAL FACILITY</ebacca:OrganisationName>
					<ebacca:Address>
						<ebacca:AddressLine1>45 TRANSITIONAL ROAD</ebacca:AddressLine1>
						<ebacca:AddressLine2>FACILITY TOPS</ebacca:AddressLine2>
						<ebacca:City>TRANSITIONAL</ebacca:City>
						<ebacca:PostalCode>4389</ebacca:PostalCode>
						<ebacca:Country>NZ</ebacca:Country>
					</ebacca:Address>
				</ebacca:TransitionalFacility>
			</ebacca:EBACCARequest>
		</maf:MetaData>
		<maf:Files>
			<maf:File>
				<maf:FileName>FlatChat.pdf</maf:FileName>
				<DocumentType>ExporterDeclaration</DocumentType>
				<maf:ContentType>PDF</maf:ContentType>
				<maf:Data>
					<maf:DataEncoding>Base64</maf:DataEncoding>
					<maf:DataContent>JVBERiA8RmxhdENoYXQ+ICVFT0YK</maf:DataContent>
				</maf:Data>
			</maf:File>
		</maf:Files>
	</maf:Body>
</maf:MessagingRequest>",
				messageBuilder.Generate());
		}

		[ExpectNoExceptions]
		public void TestEmptyConsolDoNotBlowUp()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TST2021"))
			{
				var consol = Factory.New<ForwardingConsol>();
				var messageData = TestDataBuilder.GetMAFMessaging(consol);
				new SendIPIFromConsol(messageData, new AdditionalMessageInformation(NZ.TradeSingleWindow.MessageBuilders.TSWTransactionTypes.Original, Factory), NZ.TradeSingleWindow.MessageBuilders.TSWTransactionTypes.Original).SendMessage();
				new MAFMessageSerializer(messageData).Generate();
			}
		}

		[ExpectNoExceptions]
		public void TestAirConsolDoesNotCrash()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "BX");
			var consol = Factory.New<ForwardingConsol>();
			new TestDataBuilder(consol.Factory).PopulateAirConsolThatPassesValidation(consol);
			var messageData = TestDataBuilder.GetMAFMessaging(consol);
			var messageBuilder = new MAFMessageSerializer(messageData);
			messageBuilder.Generate();
		}

		public void TestAirConsolMessage()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "BX");
			var consol = Factory.New<ForwardingConsol>();
			new TestDataBuilder(consol.Factory).PopulateAirConsolThatPassesValidation(consol);
			var messageData = TestDataBuilder.GetMAFMessaging(consol);
			var messageBuilder = new MAFMessageSerializer(messageData);
			var messageGenerated = messageBuilder.Generate();
			AssertXMLEquals("messageBuilder.Generate()",
@"<?xml version=""1.0"" encoding=""utf-8""?>
<maf:MessagingRequest xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:maf=""http://www.maf.govt.nz/Messaging/Request/2008/03"">
	<maf:Header>
		<maf:ApplicationName>EBACCA</maf:ApplicationName>
		<maf:ApplicationVersion>1.0.0</maf:ApplicationVersion>
		<maf:DocumentType>EBACCA</maf:DocumentType>
		<maf:Sender>
			<maf:Name>CARGOWISE BROKERS</maf:Name>
			<maf:EndPointType>Email</maf:EndPointType>
			<maf:Address>cargowiseone@brokers.cargowise.com</maf:Address>
		</maf:Sender>
		<maf:CallerRefID>||-MESSAGE NUMBER PLACE HOLDER-||</maf:CallerRefID>
	</maf:Header>
	<maf:Body>
		<maf:MetaData>
			<ebacca:EBACCARequest xmlns:ebacca=""http://www.maf.govt.nz/EBACCA/Messaging/Request/2008/03/"">
				<ebacca:Broker>
					<ebacca:OrganisationCode>00009915B</ebacca:OrganisationCode>
					<ebacca:OrganisationName>CARGOWISE BROKERS</ebacca:OrganisationName>
					<ebacca:Address>
						<ebacca:AddressLine1>72 BROKER STREET</ebacca:AddressLine1>
						<ebacca:AddressLine2>BROKERS TOWER</ebacca:AddressLine2>
						<ebacca:City>BROKERVILLE</ebacca:City>
						<ebacca:PostalCode>2015</ebacca:PostalCode>
						<ebacca:Country>NZ</ebacca:Country>
					</ebacca:Address>
					<ebacca:ContactMethods>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>Phone</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>44556677</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>Fax</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>99887766</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>EMail</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>broker@brokers.cargowise.com</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
					</ebacca:ContactMethods>
					<ebacca:ContactPerson>
						<ebacca:FirstName>Sidney</ebacca:FirstName>
						<ebacca:LastName>Broker</ebacca:LastName>
					</ebacca:ContactPerson>
				</ebacca:Broker>
				<ebacca:Importer>
					<ebacca:OrganisationCode>00112233F</ebacca:OrganisationCode>
					<ebacca:OrganisationName>IMPORTER INCORPORATED</ebacca:OrganisationName>
					<ebacca:Address>
						<ebacca:AddressLine1>77 IMPORTER AVENUE</ebacca:AddressLine1>
						<ebacca:AddressLine2>IMPORTER SPIRE</ebacca:AddressLine2>
						<ebacca:City>IMPORTERVILLE</ebacca:City>
						<ebacca:PostalCode>0232</ebacca:PostalCode>
						<ebacca:Country>NZ</ebacca:Country>
					</ebacca:Address>
					<ebacca:ContactMethods>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>Phone</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>22222222</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>Fax</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>11111111</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>EMail</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>importer@importer.co.nz</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
					</ebacca:ContactMethods>
					<ebacca:ContactPerson>
						<ebacca:FirstName>Test</ebacca:FirstName>
						<ebacca:LastName>Importer</ebacca:LastName>
					</ebacca:ContactPerson>
				</ebacca:Importer>
				<ebacca:Exporter>
					<ebacca:OrganisationCode>00998877Z</ebacca:OrganisationCode>
					<ebacca:OrganisationName>SUPPLIER PTY LTD</ebacca:OrganisationName>
					<ebacca:Address>
						<ebacca:AddressLine1>98 SUPPLIER CIRCUIT</ebacca:AddressLine1>
						<ebacca:AddressLine2>SUPPLIERHAVEN</ebacca:AddressLine2>
						<ebacca:City>SUPPLIER HILL</ebacca:City>
						<ebacca:PostalCode>2234</ebacca:PostalCode>
						<ebacca:Country>AU</ebacca:Country>
					</ebacca:Address>
				</ebacca:Exporter>
				<ebacca:Details>
					<ebacca:ConsignmentType>CommercialCargo</ebacca:ConsignmentType>
					<ebacca:Shipment>
						<ebacca:OriginCountry>AU</ebacca:OriginCountry>
						<ebacca:DischargePorts>
							<ebacca:DischargePort>NZCHC</ebacca:DischargePort>
						</ebacca:DischargePorts>
						<ebacca:Destinations>
							<ebacca:Destination>NZNPE</ebacca:Destination>
						</ebacca:Destinations>
						<ebacca:Flight>
							<ebacca:FlightNumber>QF18</ebacca:FlightNumber>
							<ebacca:FlightArrivalDate>2017-05-29</ebacca:FlightArrivalDate>
						</ebacca:Flight>
						<ebacca:Identifiers>
							<ebacca:BillOfLadings>
								<ebacca:BillOfLadingNumber>08100239487</ebacca:BillOfLadingNumber>
							</ebacca:BillOfLadings>
							<ebacca:SubBillOfLadings>
								<ebacca:SubBillOfLadingNumber>BOL01010101</ebacca:SubBillOfLadingNumber>
								<ebacca:SubBillOfLadingNumber>BOL01010102</ebacca:SubBillOfLadingNumber>
							</ebacca:SubBillOfLadings>
						</ebacca:Identifiers>
					</ebacca:Shipment>
					<ebacca:ConsignmentDescription>FAK</ebacca:ConsignmentDescription>
					<ebacca:ConsignmentMeasurement>
						<ebacca:MeasurementUnitQualifer>unit</ebacca:MeasurementUnitQualifer>
						<ebacca:MeasurementValue>24</ebacca:MeasurementValue>
					</ebacca:ConsignmentMeasurement>
					<ebacca:Commodities>
						<ebacca:Commodity>
							<ebacca:GoodsType>MSC</ebacca:GoodsType>
							<ebacca:GoodsDescription>FAK</ebacca:GoodsDescription>
							<ebacca:GoodsMeasurements>
								<ebacca:GoodsMeasurement>
									<ebacca:MeasurementUnitQualifer>unit</ebacca:MeasurementUnitQualifer>
									<ebacca:MeasurementValue>24</ebacca:MeasurementValue>
								</ebacca:GoodsMeasurement>
							</ebacca:GoodsMeasurements>
							<ebacca:IsNew>false</ebacca:IsNew>
						</ebacca:Commodity>
					</ebacca:Commodities>
					<ebacca:MAFProcessingOffice>Christchurch</ebacca:MAFProcessingOffice>
				</ebacca:Details>
				<ebacca:References>
					<ebacca:ClientReferenceNumber>||-SENDERS REFERENCE PLACE HOLDER-||</ebacca:ClientReferenceNumber>
				</ebacca:References>
				<ebacca:PaymentDetails>
					<ebacca:Account>
						<ebacca:AccountHolderName>EDI CUSTOMS BROKERS</ebacca:AccountHolderName>
						<ebacca:AccountNumber>AB123</ebacca:AccountNumber>
					</ebacca:Account>
				</ebacca:PaymentDetails>
				<ebacca:TransitionalFacility>
					<ebacca:OrganisationCode>TF1</ebacca:OrganisationCode>
					<ebacca:OrganisationName>TRANSITIONAL FACILITY</ebacca:OrganisationName>
					<ebacca:Address>
						<ebacca:AddressLine1>45 TRANSITIONAL ROAD</ebacca:AddressLine1>
						<ebacca:AddressLine2>FACILITY TOPS</ebacca:AddressLine2>
						<ebacca:City>TRANSITIONAL</ebacca:City>
						<ebacca:PostalCode>4389</ebacca:PostalCode>
						<ebacca:Country>NZ</ebacca:Country>
					</ebacca:Address>
				</ebacca:TransitionalFacility>
			</ebacca:EBACCARequest>
		</maf:MetaData>
		<maf:Files>
			<maf:File>
				<maf:FileName>FlatChat.pdf</maf:FileName>
				<DocumentType>ExporterDeclaration</DocumentType>
				<maf:ContentType>PDF</maf:ContentType>
				<maf:Data>
					<maf:DataEncoding>Base64</maf:DataEncoding>
					<maf:DataContent>JVBERiA8RmxhdENoYXQ+ICVFT0YK</maf:DataContent>
				</maf:Data>
			</maf:File>
		</maf:Files>
	</maf:Body>
</maf:MessagingRequest>",
				messageGenerated);
		}

		public void TestEmptyConsolDoesNotPassValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.Containers.AddNew();
			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_IsLinked = false;
			transport.JW_Vessel = "BUNGA DELIMA";
			transport.JW_RL_NKLoadPort = "AUMEL";
			transport.JW_RL_NKDiscPort = "NZCHC";

			TestHelper.SetupMessagingEnvironment();
			var messageData = TestDataBuilder.GetMAFMessaging(consol);
			var sender = new SendIPIFromConsol(messageData, new AdditionalMessageInformation(NZ.TradeSingleWindow.MessageBuilders.TSWTransactionTypes.Original, Factory), NZ.TradeSingleWindow.MessageBuilders.TSWTransactionTypes.Original);

			var expectedErrors = @"
Cannot send message due the following validation errors:
You must enter at least one shipment.
Master Bill 1: You cannot have a Blank Master Bill on a packing row.
You must have a Container Number on All Containers.
You must have a Goods Location, (entered in the Consol > Arrival tab > CFS Address).
The organisation/address entered must have a valid CCP or ATF code configured.
You must have a MPI Total Quantity Unit.
You must have a MPI Total Quantity.
You must have a Port of Destination.
You must have a Port of Origin.
You must have a Receiving Agent when sending an eBACCa/IPI.
You must have a Sending Agent when sending an eBACCa/IPI.
You must have a Shipping Line.
You must have a Voyage Number.
";
			AssertMultilineASCIIEquals("dataValidator.Errors", expectedErrors.Trim(), sender.Errors);
		}

		public void TestConstructorWontAllowNull()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{ new MAFPlugInSupportConsolWrapper(null); });
		}
	}
}
