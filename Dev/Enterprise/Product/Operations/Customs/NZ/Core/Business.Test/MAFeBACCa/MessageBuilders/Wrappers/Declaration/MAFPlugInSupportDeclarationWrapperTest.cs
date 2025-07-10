using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
using Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces;
using Enterprise.Customs.NZ.Business.MAFeBACCa.Testing;
using Enterprise.Customs.NZ.Business.MAFeBACCa.XMLSchemas;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.Registry;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders.Testing
{
	sealed class MAFPlugInSupportDeclarationWrapperTest : TestCaseWithFactory
	{
		public void TestImporterAndSupplierDetailsComeFromTheOrganisationNotTheJobDocAddress()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "BX");
			var declaration = Factory.New<JobDeclaration>();
			new TestDataBuilder(declaration).PopulateDeclarationThatPassesValidation();

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			TestDataBuilder.SetupAddressContactDetails(declaration.Importer.MainAddress, "ImpAddPhone", "ImpAddFax", "ImpAddEmail");
			TestDataBuilder.SetupAddressContactDetails(declaration.Supplier.MainAddress, "SupAddPhone", "SupAddFax", "SupAddEmail");

			declaration.DocAddresses.RemoveAndDeleteAll();
			var mafMessaging = TestDataBuilder.GetMAFMessaging(declaration);
			IMAFMessagingRequest messageData = mafMessaging;

			var importer = messageData.MetaData.Importer;
			CombineAssertions(delegate
			{
				AssertEquals(importer.OrganisationCode, "00112233F");
				AssertEquals("importer.OrganisationName", "IMPORTER INCORPORATED", importer.OrganisationName);
				AssertEquals("importer.AddressLine1", "77 IMPORTER AVENUE", importer.AddressLine1);
				AssertEquals("importer.AddressLine2", "IMPORTER SPIRE", importer.AddressLine2);
				AssertEquals("importer.City", "IMPORTERVILLE", importer.City);
				AssertEquals("importer.PostalCode", "0232", importer.PostalCode);
				AssertEquals("importer.Country", "NZ", importer.Country);
				AssertEquals("importer.ContactMethodValue[Phone]", "22222222", importer.ContactPhone);
				AssertEquals("importer.ContactMethodValue[Fax]", "11111111", importer.ContactFax);
				AssertEquals("importer.ContactMethodValue[Email]", "importer@importer.co.nz", importer.ContactEmail);
				AssertEquals("importer.ContactName", "Test ABDULLAH Importer", importer.ContactName);
			});

			var exporter = messageData.MetaData.Exporter;
			CombineAssertions(delegate
			{
				AssertEquals("exporter.OrganisationCode", "00998877Z", exporter.OrganisationCode);
				AssertEquals("exporter.OrganisationName", "SUPPLIER PTY LTD", exporter.OrganisationName);
				AssertEquals("exporter.AddressLine1", "98 SUPPLIER CIRCUIT", exporter.AddressLine1);
				AssertEquals("exporter.AddressLine2", "SUPPLIERHAVEN", exporter.AddressLine2);
				AssertEquals("exporter.City", "SUPPLIER HILL", exporter.City);
				AssertEquals("exporter.PostalCode", "2234", exporter.PostalCode);
				AssertEquals("exporter.Country", "AU", exporter.Country);
				AssertEquals("exporter.ContactMethodValue[Phone]", "876543211", exporter.ContactPhone);
				AssertEquals("exporter.ContactMethodValue[Fax]", "12345678", exporter.ContactFax);
				AssertEquals("exporter.ContactMethodValue[Email]", "major.exporter@exporter.com.au", exporter.ContactEmail);
				AssertEquals("exporter.ContactName", "Major ABDULLAH Exporter", exporter.ContactName);
			});

			var mafData = mafMessaging;
			mafData.ZX_ImporterContactName = "MPI ImpName";
			mafData.ZX_ImporterContactPhone = "MPIImpPhone";
			mafData.ZX_ImporterContactFax = "MPIImpFax";
			mafData.ZX_ImporterContactEmail = "MPIImpEmail";

			mafData.ZX_ExporterContactName = "MPI ExpName";
			mafData.ZX_ExporterContactPhone = "MPIExpPhone";
			mafData.ZX_ExporterContactFax = "MPIExpFax";
			mafData.ZX_ExporterContactEmail = "MPIExpEmail";

			importer = messageData.MetaData.Importer;
			CombineAssertions(delegate
			{
				AssertEquals("importer.ContactName", "MPI ImpName", importer.ContactName);
				AssertEquals("importer.ContactMethodValue[Phone]", "MPIImpPhone", importer.ContactPhone);
				AssertEquals("importer.ContactMethodValue[Fax]", "MPIImpFax", importer.ContactFax);
				AssertEquals("importer.ContactMethodValue[Email]", "MPIImpEmail", importer.ContactEmail);
			});

			exporter = messageData.MetaData.Exporter;
			CombineAssertions(delegate
			{
				AssertEquals("exporter.ContactName", "MPI ExpName", exporter.ContactName);
				AssertEquals("exporter.ContactMethodValue[Phone]", "MPIExpPhone", exporter.ContactPhone);
				AssertEquals("exporter.ContactMethodValue[Fax]", "MPIExpFax", exporter.ContactFax);
				AssertEquals("exporter.ContactMethodValue[Email]", "MPIExpEmail", exporter.ContactEmail);
			});
		}

		public void TestBuilderFullMessageForECI()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "BX");
			var declaration = Factory.New<JobDeclaration>();
			new TestDataBuilder(declaration).PopulateDeclarationThatPassesValidation();

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.CusEntryHeader.EntryNumber = "42098377";

			var messageData = TestDataBuilder.GetMAFMessaging(declaration);

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
					<ebacca:ContactMethods>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>Phone</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>876543211</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>Fax</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>12345678</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>EMail</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>major.exporter@exporter.com.au</ebacca:ContactMethodValue>
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
							</ebacca:SubBillOfLadings>
							<ebacca:Containers>
								<ebacca:Container>
									<ebacca:ContainerNumber>OOCL0000023</ebacca:ContainerNumber>
									<ebacca:ContainerType>MoveableCaseL615m</ebacca:ContainerType>
								</ebacca:Container>
							</ebacca:Containers>
						</ebacca:Identifiers>
					</ebacca:Shipment>
					<ebacca:ConsignmentDescription>UNQUALIFIED GROMMETS</ebacca:ConsignmentDescription>
					<ebacca:ConsignmentMeasurement>
						<ebacca:MeasurementUnitQualifer>box</ebacca:MeasurementUnitQualifer>
						<ebacca:MeasurementValue>12</ebacca:MeasurementValue>
					</ebacca:ConsignmentMeasurement>
					<ebacca:Commodities>
						<ebacca:Commodity>
							<ebacca:GoodsType>MSC</ebacca:GoodsType>
							<ebacca:GoodsDescription>UNQUALIFIED GROMMETS</ebacca:GoodsDescription>
							<ebacca:GoodsMeasurements>
								<ebacca:GoodsMeasurement>
									<ebacca:MeasurementUnitQualifer>kilograms</ebacca:MeasurementUnitQualifer>
									<ebacca:MeasurementValue>0</ebacca:MeasurementValue>
								</ebacca:GoodsMeasurement>
							</ebacca:GoodsMeasurements>
							<ebacca:IsNew>false</ebacca:IsNew>
						</ebacca:Commodity>
					</ebacca:Commodities>
					<ebacca:MAFProcessingOffice>Christchurch</ebacca:MAFProcessingOffice>
				</ebacca:Details>
				<ebacca:References>
					<ebacca:Customs>
						<ebacca:EntryNumber>42098377</ebacca:EntryNumber>
						<ebacca:EntryType>ECI</ebacca:EntryType>
					</ebacca:Customs>
					<ebacca:ClientReferenceNumber>||-SENDERS REFERENCE PLACE HOLDER-||</ebacca:ClientReferenceNumber>
				</ebacca:References>
				<ebacca:PaymentDetails>
					<ebacca:Account>
						<ebacca:AccountHolderName>IMPORTER INCORPORATED</ebacca:AccountHolderName>
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
				<ebacca:TreatmentProvider>
					<ebacca:OrganisationName>TREATMENT PROVIDER</ebacca:OrganisationName>
					<ebacca:Address>
						<ebacca:AddressLine1>15 TREATMENT ROAD</ebacca:AddressLine1>
						<ebacca:AddressLine2>BACK OF TREATMENT</ebacca:AddressLine2>
						<ebacca:City>TREATMENT</ebacca:City>
						<ebacca:PostalCode>2323</ebacca:PostalCode>
						<ebacca:Country>NZ</ebacca:Country>
					</ebacca:Address>
				</ebacca:TreatmentProvider>
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

		public void TestBuilderFullMessageForECIWithMultipleDeclarationsWithSameManifest()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "BX");
			var manifestingEntryHeader = Factory.New<Declaration.ECIWriteOff.Manifesting.CusEntryHeader>();
			manifestingEntryHeader.CH_BGMReference = "M00000001";
			manifestingEntryHeader.EntryNumber = "42098377";

			var declaration = Factory.New<JobDeclaration>();
			new TestDataBuilder(declaration).PopulateDeclarationThatPassesValidation();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.LinkToManifest(manifestingEntryHeader, manifestingEntryHeader.Declarations.Count);

			declaration = Factory.New<JobDeclaration>();
			new TestDataBuilder(declaration).PopulateDeclarationThatPassesValidation();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.LinkToManifest(manifestingEntryHeader, manifestingEntryHeader.Declarations.Count);

			var messageData = TestDataBuilder.GetMAFMessaging(declaration);

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
					<ebacca:ContactMethods>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>Phone</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>876543211</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>Fax</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>12345678</ebacca:ContactMethodValue>
						</ebacca:ContactMethod>
						<ebacca:ContactMethod>
							<ebacca:ContactMethodType>EMail</ebacca:ContactMethodType>
							<ebacca:ContactMethodValue>major.exporter@exporter.com.au</ebacca:ContactMethodValue>
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
							</ebacca:SubBillOfLadings>
							<ebacca:Containers>
								<ebacca:Container>
									<ebacca:ContainerNumber>OOCL0000023</ebacca:ContainerNumber>
									<ebacca:ContainerType>MoveableCaseL615m</ebacca:ContainerType>
								</ebacca:Container>
							</ebacca:Containers>
						</ebacca:Identifiers>
					</ebacca:Shipment>
					<ebacca:ConsignmentDescription>UNQUALIFIED GROMMETS</ebacca:ConsignmentDescription>
					<ebacca:ConsignmentMeasurement>
						<ebacca:MeasurementUnitQualifer>box</ebacca:MeasurementUnitQualifer>
						<ebacca:MeasurementValue>12</ebacca:MeasurementValue>
					</ebacca:ConsignmentMeasurement>
					<ebacca:Commodities>
						<ebacca:Commodity>
							<ebacca:GoodsType>MSC</ebacca:GoodsType>
							<ebacca:GoodsDescription>UNQUALIFIED GROMMETS</ebacca:GoodsDescription>
							<ebacca:GoodsMeasurements>
								<ebacca:GoodsMeasurement>
									<ebacca:MeasurementUnitQualifer>kilograms</ebacca:MeasurementUnitQualifer>
									<ebacca:MeasurementValue>0</ebacca:MeasurementValue>
								</ebacca:GoodsMeasurement>
							</ebacca:GoodsMeasurements>
							<ebacca:IsNew>false</ebacca:IsNew>
						</ebacca:Commodity>
					</ebacca:Commodities>
					<ebacca:MAFProcessingOffice>Christchurch</ebacca:MAFProcessingOffice>
				</ebacca:Details>
				<ebacca:References>
					<ebacca:Customs>
						<ebacca:EntryNumber>0</ebacca:EntryNumber>
						<ebacca:EntryType>ECI</ebacca:EntryType>
					</ebacca:Customs>
					<ebacca:ClientReferenceNumber>||-SENDERS REFERENCE PLACE HOLDER-||</ebacca:ClientReferenceNumber>
				</ebacca:References>
				<ebacca:PaymentDetails>
					<ebacca:Account>
						<ebacca:AccountHolderName>IMPORTER INCORPORATED</ebacca:AccountHolderName>
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
				<ebacca:TreatmentProvider>
					<ebacca:OrganisationName>TREATMENT PROVIDER</ebacca:OrganisationName>
					<ebacca:Address>
						<ebacca:AddressLine1>15 TREATMENT ROAD</ebacca:AddressLine1>
						<ebacca:AddressLine2>BACK OF TREATMENT</ebacca:AddressLine2>
						<ebacca:City>TREATMENT</ebacca:City>
						<ebacca:PostalCode>2323</ebacca:PostalCode>
						<ebacca:Country>NZ</ebacca:Country>
					</ebacca:Address>
				</ebacca:TreatmentProvider>
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

		public void TestInvalidImageAddedThrowsException()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "BX");
			var declaration = Factory.New<JobDeclaration>();

			new TestDataBuilder(declaration).PopulateDeclarationForAir();

			AddFileOrDocument(declaration, Encoding.ASCII.GetBytes("Invalid Image"), "Invalid Image.TIF", DocumentTypeList.Codes.Other);

			IMAFMessagingRequest data = TestDataBuilder.GetMAFMessaging(declaration);

			AssertExceptionThrown(typeof(MAFMessageException), "File [Invalid Image.TIF] appears to be corrupted. Could not load as Image to convert to PDF.", delegate
			{ var content = data.Files.Last().DataContent; });
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAllAddedFilesAreInPDF()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "BX");
			var declaration = Factory.New<JobDeclaration>();

			new TestDataBuilder(declaration).PopulateDeclarationForAir();

			AddFileOrDocument(declaration, BaseSourcePath + @"Enterprise\Product\Documents\DocumentEngine\FlexCelInterface\Testing\Sample2PageTIFF.TIF", DocumentTypeList.Codes.ExporterDeclaration);
			AddFileOrDocument(declaration, BaseSourcePath + @"Enterprise\Product\Documents\DocumentEngine\FlexCelInterface\Testing\SampleBitmap.bmp", DocumentTypeList.Codes.ExporterDeclaration);

			IMAFMessagingRequest messageData = TestDataBuilder.GetMAFMessaging(declaration);
			AssertNotNull("Pre-condition: dataBuilder.GetMessageData()", messageData);
			AssertEquals("Pre-condition: messageData.Files.Count", 3, messageData.Files.Take(4).Count());

			var file1 = messageData.Files.ElementAt(0);
			AssertEquals("file1.FileName", "FlatChat.pdf", file1.FileName);
			AssertEquals("file1.DocumentType", DocumentTypeList.Codes.ExporterDeclaration, file1.DocumentType);
			AssertEquals("file1.ContentType", "PDF", file1.ContentType);
			AssertEquals("file1.DataEncoding", MessagingRequestTypeBodyFileDataDataEncoding.Base64, file1.DataEncoding);
			AssertDataContentIsPDF(file1.DataContent);

			var file2 = messageData.Files.ElementAt(1);
			AssertEquals("file2.FileName", "Sample2PageTIFF.TIF", file2.FileName);
			AssertEquals("file2.DocumentType", DocumentTypeList.Codes.ExporterDeclaration, file2.DocumentType);
			AssertEquals("file2.ContentType", "PDF", file2.ContentType);
			AssertEquals("file2.DataEncoding", MessagingRequestTypeBodyFileDataDataEncoding.Base64, file2.DataEncoding);
			AssertDataContentIsPDF(file2.DataContent);

			var file3 = messageData.Files.ElementAt(2);
			AssertEquals("file3.FileName", "SampleBitmap.bmp", file3.FileName);
			AssertEquals("file3.DocumentType", DocumentTypeList.Codes.ExporterDeclaration, file3.DocumentType);
			AssertEquals("file3.ContentType", "PDF", file3.ContentType);
			AssertEquals("file3.DataEncoding", MessagingRequestTypeBodyFileDataDataEncoding.Base64, file3.DataEncoding);
			AssertDataContentIsPDF(file3.DataContent);
		}

		public void TestInvoiceLineFallbacks()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "BX");
			var declaration = Factory.New<JobDeclaration>();

			new TestDataBuilder(declaration).PopulateDeclarationForAir();
			var invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.JI_Volume = 0;
			invoiceLine.JI_VolumeUQ = Constants.Volume.CubicMetres;
			invoiceLine.JI_Weight = 0;
			invoiceLine.JI_WeightUQ = Constants.Weight.Kilograms;

			AssertMeasurement(declaration, 0, null);

			invoiceLine.JI_Volume = 5;
			invoiceLine.JI_VolumeUQ = Constants.Volume.CubicMetres;
			AssertMeasurement(declaration, 5m, MeasurementUQList.Codes.cubicMetres);

			invoiceLine.JI_Weight = 16;
			invoiceLine.JI_WeightUQ = Constants.Weight.Kilograms;
			AssertMeasurement(declaration, 16m, MeasurementUQList.Codes.kilograms);

			invoiceLine.JI_CustomsQuantity = 2m;
			invoiceLine.JI_CustomsUnitQty = StatisticalUQList.Codes.Tonnes;
			AssertMeasurement(declaration, 2m, MeasurementUQList.Codes.metricTonnes);

			invoiceLine.JI_CustomsUnitQty = StatisticalUQList.Codes.LitresOfPureAlcohol;
			AssertMeasurement(declaration, 16m, MeasurementUQList.Codes.kilograms);
		}

		[ExpectNoExceptions]
		public void TestEmptyDeclarationsDoNotBlowUp()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "BX");
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TST2021"))
			{
				var declaration = Factory.New<JobDeclaration>();

				var builder = new TestDataBuilder(declaration);
				builder.PopulateDeclarationForAir();

				var consol = Factory.New<ForwardingConsol>();
				builder.PopulateAirConsolThatPassesValidation(consol);

				declaration.JE_JS = consol.Shipments.First().PK;

				var messageData = TestDataBuilder.GetMAFMessaging(consol);

				var sender = new SendIPIFromConsol(messageData, new AdditionalMessageInformation(NZ.TradeSingleWindow.MessageBuilders.TSWTransactionTypes.Original, Factory), NZ.TradeSingleWindow.MessageBuilders.TSWTransactionTypes.Original);
				sender.SendMessage();

				new MAFMessageSerializer(messageData).Generate();

				declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;

				sender = new SendIPIFromConsol(messageData, new AdditionalMessageInformation(NZ.TradeSingleWindow.MessageBuilders.TSWTransactionTypes.Original, Factory), NZ.TradeSingleWindow.MessageBuilders.TSWTransactionTypes.Original);
				sender.SendMessage();

				new MAFMessageSerializer(messageData).Generate();
			}
		}

		public void TestConstructorWontAllowNullDeclaration()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{ new MAFPlugInSupportDeclarationWrapper(null); });
		}

		public void TestBuildFullMessage()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "BX");
			var declaration = Factory.New<JobDeclaration>();

			new TestDataBuilder(declaration).PopulateDeclarationForAir();

			IMAFMessagingRequest messageData = TestDataBuilder.GetMAFMessaging(declaration);
			AssertNotNull("dataBuilder.GetMessageData()", messageData);

			AssertEquals("messageData.ApplicationName", "EBACCA", messageData.ApplicationName);
			AssertEquals("messageData.ApplicationVersion", "1.0.0", messageData.ApplicationVersion);
			AssertEquals("messageData.DocumentType", "EBACCA", messageData.DocumentType);
			AssertEquals("messageData.SenderName", "CARGOWISE BROKERS", messageData.SenderName);
			AssertEquals("messageData.SenderEndPointType", EndPointTypeEndPointType.Email, messageData.SenderEndPointType);
			AssertEquals("messageData.SenderAddress", "cargowiseone@brokers.cargowise.com", messageData.SenderAddress);
			AssertEquals("messageData.CallerRefID", NZMMessage.MessageNumberPlaceHolder, messageData.CallerRefID);

			var ebaccaData = messageData.MetaData;
			AssertNotNull("messageData.MetaData", ebaccaData);
			AssertEquals("ebaccaData.OriginCountry", "AU", ebaccaData.Source.OriginCountry);
			AssertEquals("ebaccaData.DischargePorts", "NZCHC", new ZStringBuilder(ebaccaData.Source.DischargePorts).ToStringWithDelimiterBetweenAppends(","));
			AssertEquals("ebaccaData.Destinations", "NZNPE", new ZStringBuilder(ebaccaData.Source.Destinations).ToStringWithDelimiterBetweenAppends(","));
			AssertEquals("ebaccaData.FlightNumber", "QF253", ebaccaData.Source.FlightNumber);
			AssertEquals("ebaccaData.FlightArrivalDate", new DateTime(2009, 2, 2), ebaccaData.Source.FlightArrivalDate);
			AssertEquals("ebaccaData.BillOfLadingNumbers", "08111111111", new ZStringBuilder(ebaccaData.Source.BillOfLadingNumbers).ToStringWithDelimiterBetweenAppends(","));
			AssertEquals("ebaccaData.SubBillOfLadingNumbers", "HB01010101", new ZStringBuilder(ebaccaData.Source.SubBillOfLadingNumbers).ToStringWithDelimiterBetweenAppends(","));
			AssertEquals("ebaccaData.ConsignmentDescription", "UNQUALIFIED GROMMETS", ebaccaData.Source.ConsignmentDescription);
			AssertEquals("ebaccaData.CustomsEntryNumber", 72334537, ebaccaData.Source.CustomsEntryNumber);
			AssertEquals("ebaccaData.CustomsEntryType", false, ebaccaData.Source.IsECIWriteoff);
			AssertEquals("ebaccaData.ClientReferenceNumber", NZMMessage.SendersReferencePlaceHolder, ebaccaData.ClientReferenceNumber);

			AssertEquals("ebaccaData.ConsignmentType", ConsignmentTypeList.Codes.PrivateCargo, ebaccaData.ConsignmentType);
			AssertEquals("ebaccaData.MAFProcessingOffice", MAFProcessingOfficeList.Codes.Christchurch, ebaccaData.ProcessingOffice);

			AssertEquals("ebaccaData.IsMAFAuditRequiredByCustoms", null, ebaccaData.IsMAFAuditRequiredByCustoms);
			AssertEquals("ebaccaData.IsCustomsXRayRequired", null, ebaccaData.IsCustomsXRayRequired);
			AssertEquals("ebaccaData.IsCustomsCashClient", null, ebaccaData.IsCustomsCashClient);

			AssertEquals("ebaccaData.MeasurementValue", 12, ebaccaData.MeasurementValue);
			AssertEquals("ebaccaData.MeasurementUQ", MeasurementUQList.Codes.box, ebaccaData.MeasurementUQ);

			AssertEquals("ebaccaData.MAFConsignmentNumber", "", ebaccaData.MAFConsignmentNumber);
			AssertEquals("ebaccaData.MAFReceiptNumber", "", ebaccaData.MAFReceiptNumber);

			AssertEquals("ebaccaData.Comments", "", ebaccaData.Comments);

			AssertEquals("ebaccaData.AlternativePaymentMethod", ZString.Empty, ebaccaData.AlternativePaymentMethod);
			AssertEquals("ebaccaData.AccountHolderName", "IMPORTER INCORPORATED", ebaccaData.AccountDetails.AccountHolderName);
			AssertEquals("ebaccaData.AccountNumber", "AB123", ebaccaData.AccountDetails.AccountNumber);

			var broker = ebaccaData.Source.Broker;
			AssertNotNull("ebaccaData.Broker", broker);
			AssertEquals("broker.OrganisationCode", "00009915B", broker.OrganisationCode);
			AssertEquals("broker.OrganisationName", "CARGOWISE BROKERS", broker.OrganisationName);
			AssertEquals("broker.AddressLine1", "72 BROKER STREET", broker.AddressLine1);
			AssertEquals("broker.City", "BROKERVILLE", broker.City);
			AssertEquals("broker.Country", "NZ", broker.Country);
			AssertEquals("broker.PostalCode", "2015", broker.PostalCode);
			AssertEquals("broker.ContactName", "Sidney allaballah Broker", broker.ContactName);
			AssertEquals("broker.Email", "broker@brokers.cargowise.com", broker.ContactEmail);
			AssertEquals("broker.Fax", "99887766", broker.ContactFax);
			AssertEquals("broker.Phone", "44556677", broker.ContactPhone);

			var importer = ebaccaData.Importer;
			AssertNotNull("ebaccaData.Importer", importer);
			AssertEquals("importer.OrganisationCode", "00112233F", importer.OrganisationCode);
			AssertEquals("importer.OrganisationName", "IMPORTER INCORPORATED", importer.OrganisationName);
			AssertEquals("importer.AddressLine1", "77 IMPORTER AVENUE", importer.AddressLine1);
			AssertEquals("importer.AddressLine2", "IMPORTER SPIRE", importer.AddressLine2);
			AssertEquals("importer.City", "IMPORTERVILLE", importer.City);
			AssertEquals("importer.PostalCode", "0232", importer.PostalCode);
			AssertEquals("importer.Country", "NZ", importer.Country);
			AssertEquals("importer.ContactName", "Test ABDULLAH Importer", importer.ContactName);
			AssertEquals("importer.Email", "importer@importer.co.nz", importer.ContactEmail);
			AssertEquals("importer.Fax", "11111111", importer.ContactFax);
			AssertEquals("importer.Phone", "22222222", importer.ContactPhone);

			var exporter = ebaccaData.Exporter;
			AssertNotNull("ebaccaData.Exporter", exporter);
			AssertEquals("exporter.OrganisationCode", "00998877Z", exporter.OrganisationCode);
			AssertEquals("exporter.OrganisationName", "SUPPLIER PTY LTD", exporter.OrganisationName);
			AssertEquals("exporter.AddressLine1", "98 SUPPLIER CIRCUIT", exporter.AddressLine1);
			AssertEquals("exporter.AddressLine2", "SUPPLIERHAVEN", exporter.AddressLine2);
			AssertEquals("exporter.City", "SUPPLIER HILL", exporter.City);
			AssertEquals("exporter.PostalCode", "2234", exporter.PostalCode);
			AssertEquals("exporter.Country", "AU", exporter.Country);
			AssertEquals("exporter.ContactName", "Major ABDULLAH Exporter", exporter.ContactName);
			AssertEquals("exporter.Email", "major.exporter@exporter.com.au", exporter.ContactEmail);
			AssertEquals("exporter.Fax", "12345678", exporter.ContactFax);
			AssertEquals("exporter.Phone", "876543211", exporter.ContactPhone);

			var transitionalFacility = ebaccaData.Source.TransitionalFacility;
			AssertNotNull("ebaccaData.TransitionalFacility", transitionalFacility);
			AssertEquals("transitionalFacility.OrganisationCode", "TF1", transitionalFacility.OrganisationCode);
			AssertEquals("transitionalFacility.OrganisationName", "TRANSITIONAL FACILITY", transitionalFacility.OrganisationName);
			AssertEquals("transitionalFacility.AddressLine1", "45 TRANSITIONAL ROAD", transitionalFacility.AddressLine1);
			AssertEquals("transitionalFacility.AddressLine2", "FACILITY TOPS", transitionalFacility.AddressLine2);
			AssertEquals("transitionalFacility.PostalCode", "4389", transitionalFacility.PostalCode);
			AssertEquals("transitionalFacility.City", "TRANSITIONAL", transitionalFacility.City);
			AssertEquals("transitionalFacility.Country", "NZ", transitionalFacility.Country);

			var treatmentProvider = ebaccaData.Source.TreatmentProvider;
			AssertNotNull("ebaccaData.treatmentProvider", treatmentProvider);
			AssertEquals("treatmentProvider.OrganisationName", "TREATMENT PROVIDER", treatmentProvider.OrganisationName);
			AssertEquals("treatmentProvider.AddressLine1", "15 TREATMENT ROAD", treatmentProvider.AddressLine1);
			AssertEquals("treatmentProvider.AddressLine2", "BACK OF TREATMENT", treatmentProvider.AddressLine2);
			AssertEquals("treatmentProvider.City", "TREATMENT", treatmentProvider.City);
			AssertEquals("treatmentProvider.PostalCode", "2323", treatmentProvider.PostalCode);
			AssertEquals("treatmentProvider.Country", "NZ", treatmentProvider.Country);

			AssertEquals("ebaccaData.Commodities.Count", 1, ebaccaData.Source.Commodities.Take(2).Count());
			var commodity = ebaccaData.Source.Commodities.First();
			AssertEquals("commodity.GoodsType", GoodsTypeList.Codes.Tyres, commodity.GoodsType);
			AssertEquals("commodity.GoodsDescription", "STEEL BELTED RADIAL TYRES", commodity.GoodsDescription);
			AssertEquals("commodity.IsNew", true, commodity.IsNew);
			AssertEquals("commodity.TariffCodes", "4011100911E", new ZStringBuilder(commodity.TariffCodes).ToStringWithDelimiterBetweenAppends(","));
			AssertEquals("commodity.MergedLineNumber", 1, commodity.MergedLineNumber);

			AssertEquals("ebaccaData.Commodities[0].GoodsMeasurements.Count", 1, commodity.GoodsMeasurements.Take(2).Count());
			var measurement1 = commodity.GoodsMeasurements.First();
			AssertEquals("measurement1.MeasurementUQ", MeasurementUQList.Codes.kilograms, measurement1.MeasurementUQ);
			AssertEquals("measurement1.MeasurementValue", 120m, measurement1.MeasurementValue);

			AssertEquals("messageData.Files.Count", 1, messageData.Files.Take(2).Count());
			var file1 = messageData.Files.First();
			AssertEquals("file1.FileName", "FlatChat.pdf", file1.FileName);
			AssertEquals("file1.DocumentType", DocumentTypeList.Codes.ExporterDeclaration, file1.DocumentType);
			AssertEquals("file1.ContentType", "PDF", file1.ContentType);
			AssertEquals("file1.DataEncoding", MessagingRequestTypeBodyFileDataDataEncoding.Base64, file1.DataEncoding);
			AssertEquals("file1.DataContent", "JVBERiA8RmxhdENoYXQ+ICVFT0YK", file1.DataContent);

			var dataContentInBase8 = Convert.FromBase64String(file1.DataContent);
			AssertEquals("Encoding.ASCII.GetString(dataContentInBase8)", "%PDF <FlatChat> %EOF\n", Encoding.ASCII.GetString(dataContentInBase8));
			AssertEquals("dataContentStream.IsPDF()", true, ImageToPDFConverter.IsPDF(dataContentInBase8));
		}

		public void TestValuesSetInAddInfosOverrideDefaultValues()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "BX");
			var declaration = Factory.New<JobDeclaration>();
			new TestDataBuilder(declaration).PopulateDeclarationForSea();
			var mafMessaging = TestDataBuilder.GetMAFMessaging(declaration);
			IMAFMessagingRequest messageData = mafMessaging;
			AssertNotNull("Precondition: dataBuilder.GetMessageData()", messageData);
			var ebaccaData = messageData.MetaData;
			AssertNotNull("Precondition: messageData.MetaData", ebaccaData);

			AssertEquals("Precondition: ebaccaData.MAFProcessingOffice", MAFProcessingOfficeList.Codes.Christchurch, ebaccaData.ProcessingOffice);
			AssertEquals("Precondition: ebaccaData.ConsignmentType", ConsignmentTypeList.Codes.PrivateCargo, ebaccaData.ConsignmentType);

			AssertEquals("Precondition: ebaccaData.IsMAFAuditRequiredByCustoms", null, ebaccaData.IsMAFAuditRequiredByCustoms);
			AssertEquals("Precondition: ebaccaData.IsCustomsXRayRequired", null, ebaccaData.IsCustomsXRayRequired);
			AssertEquals("Precondition: ebaccaData.IsCustomsCashClient", null, ebaccaData.IsCustomsCashClient);

			AssertEquals("Precondition: ebaccaData.MeasurementValue", 12, ebaccaData.MeasurementValue);
			AssertEquals("Precondition: ebaccaData.MeasurementUQ", MeasurementUQList.Codes.box, ebaccaData.MeasurementUQ);

			AssertEquals("Precondition: ebaccaData.MAFConsignmentNumber", "", ebaccaData.MAFConsignmentNumber);
			AssertEquals("Precondition: ebaccaData.MAFReceiptNumber", "", ebaccaData.MAFReceiptNumber);

			AssertEquals("Precondition: ebaccaData.AlternativePaymentMethod", string.Empty, ebaccaData.AlternativePaymentMethod);
			AssertEquals("Precondition: ebaccaData.AccountHolderName", "IMPORTER INCORPORATED", ebaccaData.AccountDetails.AccountHolderName);
			AssertEquals("Precondition: ebaccaData.AccountNumber", "AB123", ebaccaData.AccountDetails.AccountNumber);

			AssertEquals("Precondition: ebaccaData.CargoType", CargoTypeList.Codes.Fcl, ebaccaData.CargoType);

			AssertEquals("Precondition: ebaccaData.Containers.Count", 1, ebaccaData.Source.Containers.Take(2).Count());
			var container = ebaccaData.Source.Containers.First();
			AssertEquals("Precondition: container.ContainerType", string.Empty, container.ContainerType);

			AssertEquals("Precondition: ebaccaData.Commodities.Count", 1, ebaccaData.Source.Commodities.Take(2).Count());
			var commodity = ebaccaData.Source.Commodities.First();
			AssertEquals("Precondition: commodity.GoodsType", GoodsTypeList.Codes.Tyres, commodity.GoodsType);
			AssertEquals("Precondition: commodity.IsNew", true, commodity.IsNew);
			AssertEquals("Precondition: commodity.GoodsMeasurements.Count", 1, commodity.GoodsMeasurements.Take(2).Count());
			var measurement = commodity.GoodsMeasurements.First();
			AssertEquals("Precondition: measurement.MeasurementValue", 120m, measurement.MeasurementValue);
			AssertEquals("Precondition: measurement.MeasurementUQ", MeasurementUQList.Codes.kilograms, measurement.MeasurementUQ);

			AssertEquals("Precondition: ebaccaData.Comments", "", ebaccaData.Comments);

			mafMessaging.ZX_ProcessingOffice = MAFProcessingOfficeList.Codes.Napier;
			mafMessaging.ZX_ConsignmentType = ConsignmentTypeList.Codes.AOPackage;
			mafMessaging.ZX_IsMAFAuditRequiredByCustoms = YesNoUnknownList.Codes.Yes;
			mafMessaging.ZX_IsCustomsXRayRequired = YesNoUnknownList.Codes.No;
			mafMessaging.ZX_IsCustomsCashClient = YesNoUnknownList.Codes.Yes;

			mafMessaging.ZX_MeasurementValue = 48;
			mafMessaging.ZX_MeasurementUQ = MeasurementUQList.Codes.bunch;

			mafMessaging.ZX_ConsignmentNumber = "MIFFY!";
			mafMessaging.ZX_ReceiptNumber = "MONGO!";

			mafMessaging.ZX_PaymentMethod = MAFPaymentMethodList.Codes.Account;
			mafMessaging.ZX_AccountHolder = "ANOTHER BUGGER";
			mafMessaging.ZX_AccountNumber = "DUNNIT";

			mafMessaging.ZX_CargoType = CargoTypeList.Codes.Bulk;

			var containerBO = declaration.CusContainers[0];
			containerBO.CO_MAF_ContainerType = ContainerTypeList.Codes.MoveableCaseL615m;

			var invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.JI_MAF_GoodsType = GoodsTypeList.Codes.Vehicles;
			invoiceLine.JI_MAF_NewGoods = YesNoUnknownList.Codes.No;
			invoiceLine.JI_MAF_MeasurementValue = 44;
			invoiceLine.JI_MAF_MeasurementUQ = MeasurementUQList.Codes.stem;

			mafMessaging.ZX_Comments = "NIK PHUT QUANDO FOO";

			ebaccaData = messageData.MetaData;
			AssertNotNull("messageData.MetaData", ebaccaData);

			AssertEquals("ebaccaData.MAFProcessingOffice", MAFProcessingOfficeList.Codes.Napier, ebaccaData.ProcessingOffice);
			AssertEquals("ebaccaData.ConsignmentType", ConsignmentTypeList.Codes.AOPackage, ebaccaData.ConsignmentType);

			AssertEquals("ebaccaData.IsMAFAuditRequiredByCustoms", true, ebaccaData.IsMAFAuditRequiredByCustoms);
			AssertEquals("ebaccaData.IsCustomsXRayRequired", false, ebaccaData.IsCustomsXRayRequired);
			AssertEquals("ebaccaData.IsCustomsCashClient", true, ebaccaData.IsCustomsCashClient);

			AssertEquals("ebaccaData.MeasurementValue", 48, ebaccaData.MeasurementValue);
			AssertEquals("ebaccaData.MeasurementUQ", MeasurementUQList.Codes.bunch, ebaccaData.MeasurementUQ);

			AssertEquals("ebaccaData.MAFConsignmentNumber", "MIFFY!", ebaccaData.MAFConsignmentNumber);
			AssertEquals("ebaccaData.MAFReceiptNumber", "MONGO!", ebaccaData.MAFReceiptNumber);

			AssertEquals("ebaccaData.AlternativePaymentMethod", string.Empty, ebaccaData.AlternativePaymentMethod);
			AssertEquals("ebaccaData.AccountHolderName", "ANOTHER BUGGER", ebaccaData.AccountDetails.AccountHolderName);
			AssertEquals("ebaccaData.AccountNumber", "DUNNIT", ebaccaData.AccountDetails.AccountNumber);

			AssertEquals("ebaccaData.CargoType", CargoTypeList.Codes.Bulk, ebaccaData.CargoType);

			AssertEquals("ebaccaData.Containers.Count", 1, ebaccaData.Source.Containers.Take(2).Count());
			container = ebaccaData.Source.Containers.First();
			AssertEquals("container.ContainerType", ContainerTypeList.Codes.MoveableCaseL615m, container.ContainerType);

			AssertEquals("ebaccaData.Commodities.Count", 1, ebaccaData.Source.Commodities.Take(2).Count());
			commodity = ebaccaData.Source.Commodities.First();
			AssertEquals("commodity.GoodsType", GoodsTypeList.Codes.Vehicles, commodity.GoodsType);
			AssertEquals("commodity.IsNew", false, commodity.IsNew);
			AssertEquals("commodity.GoodsMeasurements.Count", 1, commodity.GoodsMeasurements.Take(2).Count());
			measurement = commodity.GoodsMeasurements.First();
			AssertEquals("measurement.MeasurementValue", 44m, measurement.MeasurementValue);
			AssertEquals("measurement.MeasurementUQ", MeasurementUQList.Codes.stem, measurement.MeasurementUQ);

			AssertEquals("ebaccaData.Comments", "NIK PHUT QUANDO FOO", ebaccaData.Comments);

			mafMessaging.ZX_PaymentMethod = MAFPaymentMethodList.Codes.Cash;
			AssertNotNull("dataBuilder.GetMessageData()", messageData);
			ebaccaData = messageData.MetaData;
			AssertNotNull("messageData.MetaData", ebaccaData);
			AssertEquals("ebaccaData.AlternativePaymentMethod", MAFPaymentMethodList.Codes.Cash, ebaccaData.AlternativePaymentMethod);
			AssertEquals("ebaccaData.AccountHolderName", string.Empty, ebaccaData.AccountDetails.AccountHolderName);
			AssertEquals("ebaccaData.AccountNumber", string.Empty, ebaccaData.AccountDetails.AccountNumber);
		}

		public void TestTSWDeclarationsDoesNotUsePlugin()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			var plugin = new MAFMessagingBO(new MAFPlugInSupportDeclarationWrapper(declaration));
			IMAFPlugInSupport plugInSupport = plugin.PlugInSupport;
			AssertEquals("PlugInVisible - Not for TSW Declarations", false, plugInSupport.PlugInVisible);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			plugInSupport = plugin.PlugInSupport;
			AssertEquals("PlugInVisible", true, plugInSupport.PlugInVisible);
		}

		#region Implementation

		void AddFileOrDocument(JobDeclaration declaration, string path, string documentType)
		{
			using (var stream = File.OpenRead(path))
			{
				var contents = new byte[stream.Length];
				stream.Read(contents, 0, (int)stream.Length);
				var fileName = Path.GetFileName(path);

				AddFileOrDocument(declaration, contents, fileName, documentType);
			}
		}

		void AddFileOrDocument(JobDeclaration declaration, byte[] contents, string fileName, string documentType)
		{
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(contents, fileName, documentType);
			var file = TestDataBuilder.GetMAFMessaging(declaration).Files.AddNew().Data;
			file.ZF_DocumentType = eDoc.DocType;
			file.ZF_EDocsUniqueID = eDoc.UniqueKey;
			file.ZF_FileName = fileName;
		}

		void AssertDataContentIsPDF(string dataContent)
		{
			Assert("ImageToPDFConverter.IsPDF(Convert.FromBase64String(dataContent))", ImageToPDFConverter.IsPDF(Convert.FromBase64String(dataContent)));
		}

		static void AssertMeasurement(JobDeclaration declaration, decimal expectedValue, string expectedUnit)
		{
			IMAFMessagingRequest messageData = TestDataBuilder.GetMAFMessaging(declaration);
			var ebaccaData = messageData.MetaData;
			AssertEquals("ebaccaData.Commodities.Count", 1, ebaccaData.Source.Commodities.Take(2).Count());
			var commodity = ebaccaData.Source.Commodities.First();
			if (!string.IsNullOrEmpty(expectedUnit))
			{
				AssertEquals("ebaccaData.Commodities[0].GoodsMeasurements.Count", 1, commodity.GoodsMeasurements.Take(2).Count());
				var measurement1 = commodity.GoodsMeasurements.First();
				CombineAssertions(delegate
				{
					AssertEquals("measurement1.MeasurementValue", expectedValue, measurement1.MeasurementValue);
					AssertEquals("measurement1.MeasurementUQ", expectedUnit, measurement1.MeasurementUQ);
				});
			}
			else
			{
				AssertEquals("ebaccaData.Commodities[0].GoodsMeasurements.Count", false, commodity.GoodsMeasurements.Any());
			}
		}

		#endregion
	}
}
