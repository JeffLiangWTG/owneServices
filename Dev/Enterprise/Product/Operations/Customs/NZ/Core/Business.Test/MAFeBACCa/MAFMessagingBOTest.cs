using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
using Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.Testing
{
	using System;
	using Enterprise.Customs.NZ.Business.Declaration;
	using Enterprise.Customs.NZ.Business.Testing;
	using Enterprise.Customs.NZ.Business.TradeSingleWindow;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.MasterFiles.Business;
	using NUnit.Framework;

	[TestedType(typeof(MAFMessagingBO))]
	public class MAFMessagingBOTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPointers2Bills1ContainerInIPIMessage()
		{
			//<!--This example represents the association between 1 Consol/Masterbill, 2 Shipments/Housebills, 1 Container and 2 Package Types-->
			var expectedBillDetails = @"<TransportContractDocument>
        <ID>BKG200727OBL1</ID>
        <TypeCode>MB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>3</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>HBILL1</ID>
        <TypeCode>BM</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>31B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>HBILL2</ID>
        <TypeCode>BM</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>31B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>";

			var alternativeSeqBillDetails = @"<TransportContractDocument>
        <ID>BKG200727OBL1</ID>
        <TypeCode>MB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>3</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>HBILL1</ID>
        <TypeCode>BM</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>31B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>HBILL2</ID>
        <TypeCode>BM</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>31B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>";

			var expectedContainerElements = @"<TransportEquipment>
        <SequenceNumeric>1</SequenceNumeric>
        <CharacteristicCode>23</CharacteristicCode>
        <FullnessCode>7</FullnessCode>
        <ID>BKGU1111110</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportEquipment>
      <TransportEquipment>
        <SequenceNumeric>2</SequenceNumeric>
        <CharacteristicCode>23</CharacteristicCode>
        <FullnessCode>7</FullnessCode>
        <ID>BKGU1111110</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportEquipment>";

			var expectedPackagingElements = @"<Packaging>
    <SequenceNumeric>1</SequenceNumeric>
    <QuantityQuantity>1</QuantityQuantity>
    <TypeCode>PX</TypeCode>
  </Packaging>
  <Packaging>
    <SequenceNumeric>2</SequenceNumeric>
    <QuantityQuantity>2</QuantityQuantity>
    <TypeCode>PX</TypeCode>
  </Packaging>";

			Create2Bills1ContainerImportConsol();
			MAFMessagingBO mafMessaging = new MAFMessagingBO(new MAFPlugInSupportConsolWrapper(Consol));
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, mafMessaging.Factory, "IPI");
			var ipiBuilder = new IPIMessageBuilder(mafMessaging, TSWTransactionTypes.Original, additionalMessageInformation);
			var ipiMessageString = ipiBuilder.GetXMLMessage();

			AssertEquals("expected BillDetails: NZ Customs advise that each bill must point to its own Container elements, even though they have the same container" + "\r\n\r\n" + expectedBillDetails + "\r\n\r\nmessage generated:\r\n\r\n" + ipiMessageString, true, (ipiMessageString.Contains(expectedBillDetails) || ipiMessageString.Contains(alternativeSeqBillDetails)));
			AssertEquals("expected ContainerElements: Same container details get repeated for each bill" + "\r\n\r\n" + expectedContainerElements + "\r\n\r\nmessage generated:\r\n\r\n" + ipiMessageString, true, ipiMessageString.Contains(expectedContainerElements));
			AssertEquals("expected PackagingElements:" + "\r\n\r\n" + expectedPackagingElements + "\r\n\r\nmessage generated:\r\n\r\n" + ipiMessageString, true, ipiMessageString.Contains(expectedPackagingElements));
		}

		[ExpectNoExceptions]
		public void TestStoreEquipmentPointers()
		{
			Create1ContainerMultiPacksConsol();
			var shipment = Consol.Shipments[0];
			AssertEquals("Pre-condition - shipment outer pack lines", 4, shipment.OuterPackLines.Count);
			MAFMessagingBO mafMessaging = new MAFMessagingBO(new MAFPlugInSupportConsolWrapper(Consol));
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, mafMessaging.Factory, "IPI");
			var ipiBuilder = new IPIMessageBuilder(mafMessaging, TSWTransactionTypes.Original, additionalMessageInformation);
			ipiBuilder.GetXMLMessage();
		}

		public void TestContainerEquipmentElementsForColoadShipments()
		{
			//<!--This example represents the association between 1 Consol, 1 Co-Load Shipment with 2 child shipments both using the same container-->
			// The system was sending the bill details for the Co-load shipment, but not sending any associated container elements.
			// The Message should be built with a seperate Transport Equipment element for each shipment, (with the same container details repeated except for incrementing sequence number)
			var expectedEquipmentElements = @"<TransportEquipment>
        <SequenceNumeric>1</SequenceNumeric>
        <CharacteristicCode>23</CharacteristicCode>
        <FullnessCode>7</FullnessCode>
        <ID>CHSU0039483</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>3</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportEquipment>
      <TransportEquipment>
        <SequenceNumeric>2</SequenceNumeric>
        <CharacteristicCode>23</CharacteristicCode>
        <FullnessCode>7</FullnessCode>
        <ID>CHSU0039483</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>3</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportEquipment>
      <TransportEquipment>
        <SequenceNumeric>3</SequenceNumeric>
        <CharacteristicCode>23</CharacteristicCode>
        <FullnessCode>7</FullnessCode>
        <ID>CHSU0039483</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>3</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportEquipment>";
			CreateConsolWithCoLoadShipments();
			MAFMessagingBO mafMessaging = new MAFMessagingBO(new MAFPlugInSupportConsolWrapper(Consol));
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, mafMessaging.Factory, "IPI");
			var ipiBuilder = new IPIMessageBuilder(mafMessaging, TSWTransactionTypes.Original, additionalMessageInformation);
			var ipiMessageString = ipiBuilder.GetXMLMessage();
			AssertEquals("expected 3 sequences of EquipmentElements:" + "\r\n\r\n" + expectedEquipmentElements + "\r\n\r\nmessage generated:\r\n\r\n" + ipiMessageString, true, ipiMessageString.Contains(expectedEquipmentElements));
		}

		public void TestBillPackagingPointersForAirConsol()
		{
			//<!--This example represents the association between 1 Air Consol, 3 Shipments and 3 Package Types-->
			var expectedBillDetails = @"<TransportContractDocument>
        <ID>08600544272</ID>
        <TypeCode>MB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>3</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>4</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>G85928</ID>
        <TypeCode>HWB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>H69238</ID>
        <TypeCode>HWB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>G05827-17E</ID>
        <TypeCode>HWB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>3</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>";

			var containerElements = @"<TransportEquipment>";

			var expectedPackagingElements = @"<Packaging>
    <SequenceNumeric>1</SequenceNumeric>
    <QuantityQuantity>50</QuantityQuantity>
    <TypeCode>CT</TypeCode>
  </Packaging>
  <Packaging>
    <SequenceNumeric>2</SequenceNumeric>
    <QuantityQuantity>10</QuantityQuantity>
    <TypeCode>BX</TypeCode>
  </Packaging>
  <Packaging>
    <SequenceNumeric>3</SequenceNumeric>
    <QuantityQuantity>1</QuantityQuantity>
    <TypeCode>PX</TypeCode>
  </Packaging>";

			CreateAirImportConsol();
			MAFMessagingBO mafMessaging = new MAFMessagingBO(new MAFPlugInSupportConsolWrapper(Consol));
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, mafMessaging.Factory, "IPI");
			var ipiBuilder = new IPIMessageBuilder(mafMessaging, TSWTransactionTypes.Original, additionalMessageInformation);
			var ipiMessageString = ipiBuilder.GetXMLMessage();

			AssertEquals("expectedBillDetails: \r\n\r\n" + expectedBillDetails + "\r\n\r\nmessage generated:\r\n\r\n" + ipiMessageString, true, ipiMessageString.Contains(expectedBillDetails));
			AssertEquals("containerElements: should not be generated for this entry", false, ipiMessageString.Contains(containerElements));
			AssertEquals("expectedPackagingElements:" + "\r\n\r\n" + expectedPackagingElements + "\r\n\r\nmessage generated:\r\n\r\n" + ipiMessageString, true, ipiMessageString.Contains(expectedPackagingElements));
		}

		public void TestEffectiveImporterContactDetails()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "BX");
			var declaration = Factory.New<JobDeclaration>();
			var testDataBuilder = new TestDataBuilder(declaration);
			testDataBuilder.PopulateDeclarationThatPassesValidation();

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			TestDataBuilder.SetupAddressContactDetails(declaration.Importer.MainAddress, "ImpAddPhone", "ImpAddFax", "ImpAddEmail");

			var mafData = TestDataBuilder.GetMAFMessaging(declaration);

			mafData.ZX_ImporterContactName = "MPI ImpName";
			mafData.ZX_ImporterContactPhone = "MPIImpPhone";
			mafData.ZX_ImporterContactFax = "MPIImpFax";
			mafData.ZX_ImporterContactEmail = "MPIImpEmail";

			CombineAssertions(delegate
			{
				AssertEquals("mafData.EffectiveImporterContactName", "MPI ImpName", mafData.EffectiveImporterContactName);
				AssertEquals("mafData.EffectiveImporterContactPhone", "MPIImpPhone", mafData.EffectiveImporterContactPhone);
				AssertEquals("mafData.EffectiveImporterContactFax", "MPIImpFax", mafData.EffectiveImporterContactFax);
				AssertEquals("mafData.EffectiveImporterContactEmail", "MPIImpEmail", mafData.EffectiveImporterContactEmail);
			});

			mafData.ZX_ImporterContactPhone = "";
			mafData.ZX_ImporterContactFax = "";
			mafData.ZX_ImporterContactEmail = "";

			CombineAssertions(delegate
			{
				AssertEquals("mafData.EffectiveImporterContactName", "MPI ImpName", mafData.EffectiveImporterContactName);
				AssertEquals("mafData.EffectiveImporterContactPhone", "ImpAddPhone", mafData.EffectiveImporterContactPhone);
				AssertEquals("mafData.EffectiveImporterContactFax", "ImpAddFax", mafData.EffectiveImporterContactFax);
				AssertEquals("mafData.EffectiveImporterContactEmail", "ImpAddEmail", mafData.EffectiveImporterContactEmail);
			});

			mafData.ZX_ImporterContactName = "";

			CombineAssertions(delegate
			{
				AssertEquals("mafData.EffectiveImporterContactName", "Test ABDULLAH Importer", mafData.EffectiveImporterContactName);
				AssertEquals("mafData.EffectiveImporterContactPhone", "22222222", mafData.EffectiveImporterContactPhone);
				AssertEquals("mafData.EffectiveImporterContactFax", "11111111", mafData.EffectiveImporterContactFax);
				AssertEquals("mafData.EffectiveImporterContactEmail", "importer@importer.co.nz", mafData.EffectiveImporterContactEmail);
			});

			mafData.ZX_ImporterContactEmail = "Fred@kelloggs.com";

			CombineAssertions(delegate
			{
				AssertEquals("mafData.EffectiveImporterContactName", "", mafData.EffectiveImporterContactName);
				AssertEquals("mafData.EffectiveImporterContactPhone", "", mafData.EffectiveImporterContactPhone);
				AssertEquals("mafData.EffectiveImporterContactFax", "", mafData.EffectiveImporterContactFax);
				AssertEquals("mafData.EffectiveImporterContactEmail", "Fred@kelloggs.com", mafData.EffectiveImporterContactEmail);
			});

			mafData.ZX_ImporterContactEmail = "";

			TestDataBuilder.AddContact(declaration.Importer, ContactType.NotifyParty, "Importer Nofify", "111", "222", "111@222.com");
			TestDataBuilder.AddContact(declaration.Importer, ContactType.Consignee, "Importer Consignee", "333", "444", "333@444.com");

			CombineAssertions(delegate
			{
				AssertEquals("mafData.EffectiveImporterContactName", "Importer Consignee", mafData.EffectiveImporterContactName);
				AssertEquals("mafData.EffectiveImporterContactPhone", "333", mafData.EffectiveImporterContactPhone);
				AssertEquals("mafData.EffectiveImporterContactFax", "444", mafData.EffectiveImporterContactFax);
				AssertEquals("mafData.EffectiveImporterContactEmail", "333@444.com", mafData.EffectiveImporterContactEmail);
			});

			declaration.Importer.Contacts.RemoveAndDeleteAll();

			CombineAssertions(delegate
			{
				AssertEquals("mafData.EffectiveImporterContactName", "", mafData.EffectiveImporterContactName);
				AssertEquals("mafData.EffectiveImporterContactPhone", "ImpAddPhone", mafData.EffectiveImporterContactPhone);
				AssertEquals("mafData.EffectiveImporterContactFax", "ImpAddFax", mafData.EffectiveImporterContactFax);
				AssertEquals("mafData.EffectiveImporterContactEmail", "ImpAddEmail", mafData.EffectiveImporterContactEmail);
			});

			var otherImporter = testDataBuilder.GetNewOrganisation(
	"OTHER ORG",
	"72 OTHER AVENUE",
	"OTHER TREADLE",
	"OTHERVIAL",
	"1234", "NZTXT",
	OrgCusCode.CodeTypes.CustomsClientCode, "00112233F",
	"Bad", "Acid", "badacid@other.co.nz", "45454545", "67676767");
			declaration.JE_OH_Importer = otherImporter.PK;

			CombineAssertions(delegate
			{
				AssertEquals("mafData.EffectiveImporterContactName", "Bad ABDULLAH Acid", mafData.EffectiveImporterContactName);
				AssertEquals("mafData.EffectiveImporterContactPhone", "67676767", mafData.EffectiveImporterContactPhone);
				AssertEquals("mafData.EffectiveImporterContactFax", "45454545", mafData.EffectiveImporterContactFax);
				AssertEquals("mafData.EffectiveImporterContactEmail", "badacid@other.co.nz", mafData.EffectiveImporterContactEmail);
			});
		}

		public void TestEffectiveExporterContactDetails()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "BX");
			var declaration = Factory.New<JobDeclaration>();
			var testDataBuilder = new TestDataBuilder(declaration);
			testDataBuilder.PopulateDeclarationThatPassesValidation(); // Already has "ALL" contact.

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			TestDataBuilder.SetupAddressContactDetails(declaration.Supplier.MainAddress, "ExpAddPhone", "ExpAddFax", "ExpAddEmail");

			var mafData = TestDataBuilder.GetMAFMessaging(declaration);

			mafData.ZX_ExporterContactName = "MPI ExpName";
			mafData.ZX_ExporterContactPhone = "MPIExpPhone";
			mafData.ZX_ExporterContactFax = "MPIExpFax";
			mafData.ZX_ExporterContactEmail = "MPIExpEmail";

			CombineAssertions(delegate
			{
				AssertEquals("mafData.EffectiveExporterContactName", "MPI ExpName", mafData.EffectiveExporterContactName);
				AssertEquals("mafData.EffectiveExporterContactPhone", "MPIExpPhone", mafData.EffectiveExporterContactPhone);
				AssertEquals("mafData.EffectiveExporterContactFax", "MPIExpFax", mafData.EffectiveExporterContactFax);
				AssertEquals("mafData.EffectiveExporterContactEmail", "MPIExpEmail", mafData.EffectiveExporterContactEmail);
			});

			mafData.ZX_ExporterContactPhone = "";
			mafData.ZX_ExporterContactFax = "";
			mafData.ZX_ExporterContactEmail = "";

			CombineAssertions(delegate
			{
				AssertEquals("mafData.EffectiveExporterContactName", "MPI ExpName", mafData.EffectiveExporterContactName);
				AssertEquals("mafData.EffectiveExporterContactPhone", "ExpAddPhone", mafData.EffectiveExporterContactPhone);
				AssertEquals("mafData.EffectiveExporterContactFax", "ExpAddFax", mafData.EffectiveExporterContactFax);
				AssertEquals("mafData.EffectiveExporterContactEmail", "ExpAddEmail", mafData.EffectiveExporterContactEmail);
			});

			mafData.ZX_ExporterContactName = "";

			CombineAssertions(delegate
			{
				AssertEquals("mafData.EffectiveExporterContactName", "Major ABDULLAH Exporter", mafData.EffectiveExporterContactName);
				AssertEquals("mafData.EffectiveExporterContactPhone", "876543211", mafData.EffectiveExporterContactPhone);
				AssertEquals("mafData.EffectiveExporterContactFax", "12345678", mafData.EffectiveExporterContactFax);
				AssertEquals("mafData.EffectiveExporterContactEmail", "major.exporter@exporter.com.au", mafData.EffectiveExporterContactEmail);
			});

			TestDataBuilder.AddContact(declaration.Supplier, ContactType.NotifyParty, "Exporter Nofify", "111", "222", "111@222.com");
			TestDataBuilder.AddContact(declaration.Supplier, ContactType.Consignor, "Exporter Consignor", "333", "444", "333@444.com");

			CombineAssertions(delegate
			{
				AssertEquals("mafData.EffectiveExporterContactName", "Exporter Consignor", mafData.EffectiveExporterContactName);
				AssertEquals("mafData.EffectiveExporterContactPhone", "333", mafData.EffectiveExporterContactPhone);
				AssertEquals("mafData.EffectiveExporterContactFax", "444", mafData.EffectiveExporterContactFax);
				AssertEquals("mafData.EffectiveExporterContactEmail", "333@444.com", mafData.EffectiveExporterContactEmail);
			});

			declaration.Supplier.Contacts.RemoveAndDeleteAll();

			CombineAssertions(delegate
			{
				AssertEquals("mafData.EffectiveExporterContactName", "", mafData.EffectiveExporterContactName);
				AssertEquals("mafData.EffectiveExporterContactPhone", "ExpAddPhone", mafData.EffectiveExporterContactPhone);
				AssertEquals("mafData.EffectiveExporterContactFax", "ExpAddFax", mafData.EffectiveExporterContactFax);
				AssertEquals("mafData.EffectiveExporterContactEmail", "ExpAddEmail", mafData.EffectiveExporterContactEmail);
			});

			var otherExporter = testDataBuilder.GetNewOrganisation(
"OTHER ORG",
"72 OTHER AVENUE",
"OTHER TREADLE",
"OTHERVIAL",
"1234", "NZTXT",
OrgCusCode.CodeTypes.CustomsClientCode, "00112233F",
"Bad", "Acid", "badacid@other.co.nz", "45454545", "67676767");
			declaration.JE_OH_Supplier = otherExporter.PK;

			CombineAssertions(delegate
			{
				AssertEquals("mafData.EffectiveExporterContactName", "Bad ABDULLAH Acid", mafData.EffectiveExporterContactName);
				AssertEquals("mafData.EffectiveExporterContactPhone", "67676767", mafData.EffectiveExporterContactPhone);
				AssertEquals("mafData.EffectiveExporterContactFax", "45454545", mafData.EffectiveExporterContactFax);
				AssertEquals("mafData.EffectiveExporterContactEmail", "badacid@other.co.nz", mafData.EffectiveExporterContactEmail);
			});
		}

		public void TestSettingAccountDetailsDefaultToAccount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var mafMessagingBo = TestDataBuilder.GetMAFMessaging(declaration);

			mafMessagingBo.ZX_PaymentMethod = MAFPaymentMethodList.Codes.Account;
			mafMessagingBo.ZX_AccountNumber = "12345";
			mafMessagingBo.ZX_AccountHolder = "SOME OTHER BUGGER";

			AssertEquals("mafMessagingBo.ZX_PaymentMethod", MAFPaymentMethodList.Codes.Account, mafMessagingBo.ZX_PaymentMethod);
			AssertEquals("mafMessagingBo.ZX_AccountNumber", "12345", mafMessagingBo.ZX_AccountNumber);
			AssertEquals("mafMessagingBo.ZX_AccountHolder", "SOME OTHER BUGGER", mafMessagingBo.ZX_AccountHolder);

			mafMessagingBo.ZX_PaymentMethod = MAFPaymentMethodList.Codes.Cash;

			AssertEquals("mafMessagingBo.ZX_PaymentMethod", MAFPaymentMethodList.Codes.Cash, mafMessagingBo.ZX_PaymentMethod);
			AssertEquals("mafMessagingBo.ZX_AccountNumber", "", mafMessagingBo.ZX_AccountNumber);
			AssertEquals("mafMessagingBo.ZX_AccountHolder", "", mafMessagingBo.ZX_AccountHolder);

			mafMessagingBo.ZX_AccountNumber = "12345";

			AssertEquals("mafMessagingBo.ZX_PaymentMethod", MAFPaymentMethodList.Codes.Account, mafMessagingBo.ZX_PaymentMethod);
			AssertEquals("mafMessagingBo.ZX_AccountNumber", "12345", mafMessagingBo.ZX_AccountNumber);
			AssertEquals("mafMessagingBo.ZX_AccountHolder", "", mafMessagingBo.ZX_AccountHolder);

			mafMessagingBo.ZX_PaymentMethod = MAFPaymentMethodList.Codes.Cash;

			AssertEquals("mafMessagingBo.ZX_PaymentMethod", MAFPaymentMethodList.Codes.Cash, mafMessagingBo.ZX_PaymentMethod);
			AssertEquals("mafMessagingBo.ZX_AccountNumber", "", mafMessagingBo.ZX_AccountNumber);
			AssertEquals("mafMessagingBo.ZX_AccountHolder", "", mafMessagingBo.ZX_AccountHolder);

			mafMessagingBo.ZX_AccountHolder = "SOME OTHER BUGGER";

			AssertEquals("mafMessagingBo.ZX_PaymentMethod", MAFPaymentMethodList.Codes.Account, mafMessagingBo.ZX_PaymentMethod);
			AssertEquals("mafMessagingBo.ZX_AccountNumber", "", mafMessagingBo.ZX_AccountNumber);
			AssertEquals("mafMessagingBo.ZX_AccountHolder", "SOME OTHER BUGGER", mafMessagingBo.ZX_AccountHolder);

			mafMessagingBo.ZX_PaymentMethod = ZString.Empty;

			AssertEquals("mafMessagingBo.ZX_PaymentMethod", ZString.Empty, mafMessagingBo.ZX_PaymentMethod);
			AssertEquals("mafMessagingBo.ZX_AccountNumber", "", mafMessagingBo.ZX_AccountNumber);
			AssertEquals("mafMessagingBo.ZX_AccountHolder", "", mafMessagingBo.ZX_AccountHolder);
		}

		public void TestEBACCAPaymentDetailsSent()
		{
			const string expectedImporterAccountHolderName = "DA MAIN MAN";
			const string expectedImporterAccountNumber = "234987324";

			const string expectedBranchAccountHolderName = "BRANCH FIDDY";
			const string expectedBranchAccountNumber = "239087422";

			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = expectedImporterAccountHolderName;

			var declaration = Factory.New<JobDeclaration>();
			var mafMessagingBo = TestDataBuilder.GetMAFMessaging(declaration);
			declaration.JE_OH_Importer = importer.PK;

			AssertMultilineASCIIEquals("mafMessagingBo.PaymentDetailsSent", @"
CSH - Cash
NB: You can setup a default MPI QE # against Importer/Branch Organization.
".Trim(), mafMessagingBo.PaymentDetailsSent);

			var branchOrganisation = declaration.Branch.OrgProxy;
			AssertNotNull("Precondition: declaration.Branch.OrgProxy", branchOrganisation);
			branchOrganisation.OH_FullName = expectedBranchAccountHolderName;
			branchOrganisation.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.MAFCoverSheetQE, expectedBranchAccountNumber);

			AssertMultilineASCIIEquals("mafMessagingBo.PaymentDetailsSent", @"
ACC - MPI Account
239087422 / BRANCH FIDDY
NB: Defaulted from Branch Organization.
".Trim(), mafMessagingBo.PaymentDetailsSent);

			importer.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.MAFCoverSheetQE, expectedImporterAccountNumber);

			AssertMultilineASCIIEquals("mafMessagingBo.PaymentDetailsSent", @"
ACC - MPI Account
234987324 / DA MAIN MAN
NB: Defaulted from Importer.
".Trim(), mafMessagingBo.PaymentDetailsSent);

			mafMessagingBo.ZX_PaymentMethod = MAFPaymentMethodList.Codes.Other;

			AssertMultilineASCIIEquals("mafMessagingBo.PaymentDetailsSent", @"
OTH - Other
".Trim(), mafMessagingBo.PaymentDetailsSent);

			mafMessagingBo.ZX_PaymentMethod = MAFPaymentMethodList.Codes.Account;

			AssertMultilineASCIIEquals("mafMessagingBo.PaymentDetailsSent", @"
ACC - MPI Account
234987324 / DA MAIN MAN
NB: Defaulted from Importer.
".Trim(), mafMessagingBo.PaymentDetailsSent);

			mafMessagingBo.ZX_PaymentMethod = ZString.Empty;
			mafMessagingBo.ZX_AccountNumber = "12345";
			mafMessagingBo.ZX_AccountHolder = "SOME OTHER BUGGER";

			AssertMultilineASCIIEquals("mafMessagingBo.PaymentDetailsSent", @"
ACC - MPI Account
12345 / SOME OTHER BUGGER
".Trim(), mafMessagingBo.PaymentDetailsSent);
		}

		public void TestEBACCAPaymentDetailsSentIndividualFields()
		{
			const string expectedAccoungHolderName = "Test Importer";
			const string expectedAccountNumber = "888";
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = expectedAccoungHolderName;

			var declaration = Factory.New<JobDeclaration>();
			var mafMessagingBo = TestDataBuilder.GetMAFMessaging(declaration);
			declaration.JE_OH_Importer = importer.PK;

			AssertEquals("mafMessagingBo.PaymentTypeToBeSent", MAFPaymentMethodList.Codes.Cash, mafMessagingBo.PaymentTypeToBeSent);
			AssertEquals("mafMessagingBo.AccountHolderNameToBeSent", "", mafMessagingBo.AccountHolderNameToBeSent);
			AssertEquals("mafMessagingBo.AccountNumberToBeSent", "", mafMessagingBo.AccountNumberToBeSent);

			importer.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.MAFCoverSheetQE, expectedAccountNumber);

			AssertEquals("mafMessagingBo.PaymentTypeToBeSent", MAFPaymentMethodList.Codes.Account, mafMessagingBo.PaymentTypeToBeSent);
			AssertEquals("mafMessagingBo.AccountHolderNameToBeSent", expectedAccoungHolderName, mafMessagingBo.AccountHolderNameToBeSent);
			AssertEquals("mafMessagingBo.AccountNumberToBeSent", expectedAccountNumber, mafMessagingBo.AccountNumberToBeSent);

			mafMessagingBo.ZX_PaymentMethod = MAFPaymentMethodList.Codes.Other;

			AssertEquals("mafMessagingBo.PaymentTypeToBeSent", MAFPaymentMethodList.Codes.Other, mafMessagingBo.PaymentTypeToBeSent);
			AssertEquals("mafMessagingBo.AccountHolderNameToBeSent", "", mafMessagingBo.AccountHolderNameToBeSent);
			AssertEquals("mafMessagingBo.AccountNumberToBeSent", "", mafMessagingBo.AccountNumberToBeSent);
		}

		public void TestEffectiveMeasurement()
		{
			var declaration = Factory.New<JobDeclaration>();
			var mafMessaging = TestDataBuilder.GetMAFMessaging(declaration);
			AssertEquals("EffectiveMeasurementValue", ZInt.Zero, mafMessaging.EffectiveMeasurementValue);
			AssertEquals("EffectiveMeasurementUQ", ZString.Empty, mafMessaging.EffectiveMeasurementUQ);

			declaration.JE_TotalNoOfPacksPackType = PackageTypeList.Codes.Bag;
			declaration.JE_TotalNoOfPacks = 15;
			AssertEquals("EffectiveMeasurementValue", 15, mafMessaging.EffectiveMeasurementValue);
			AssertEquals("EffectiveMeasurementUQ", MeasurementUQList.Codes.bag, mafMessaging.EffectiveMeasurementUQ);

			mafMessaging.ZX_MeasurementValue = 43;
			mafMessaging.ZX_MeasurementUQ = MeasurementUQList.Codes.egg;
			AssertEquals("EffectiveMeasurementValue", 43, mafMessaging.EffectiveMeasurementValue);
			AssertEquals("EffectiveMeasurementUQ", MeasurementUQList.Codes.egg, mafMessaging.EffectiveMeasurementUQ);
		}

		public void TestEffectiveProcessingOfficeDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			var mafMessaging = TestDataBuilder.GetMAFMessaging(declaration);
			declaration.JE_RL_NKPortOfArrival = "NZAKL";
			AssertEquals("EffectiveProcessingOfficeDescription", MAFProcessingOfficeList.Descriptions.Auckland, mafMessaging.EffectiveProcessingOfficeDescription);
			declaration.JE_RL_NKPortOfArrival = "NZCHC";
			AssertEquals("EffectiveProcessingOfficeDescription", MAFProcessingOfficeList.Descriptions.Christchurch, mafMessaging.EffectiveProcessingOfficeDescription);
			mafMessaging.ZX_ProcessingOffice = MAFProcessingOfficeList.Codes.Dunedin;
			AssertEquals("EffectiveProcessingOfficeDescription", MAFProcessingOfficeList.Descriptions.Dunedin, mafMessaging.EffectiveProcessingOfficeDescription);
		}

		public void TestEffectiveConsignmentTypeDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			var mafMessaging = TestDataBuilder.GetMAFMessaging(declaration);
			AssertEquals("EffectiveConsignmentTypeDescription", ConsignmentTypeList.Descriptions.CommercialCargo, mafMessaging.EffectiveConsignmentTypeDescription);
			declaration.OtherInfos.AddNew(HeaderOtherInfoList.Codes.PrivateImportTransactionFee, "");
			AssertEquals("EffectiveConsignmentTypeDescription", ConsignmentTypeList.Descriptions.PrivateCargo, mafMessaging.EffectiveConsignmentTypeDescription);
			mafMessaging.ZX_ConsignmentType = ConsignmentTypeList.Codes.Magazine;
			AssertEquals("EffectiveConsignmentTypeDescription", ConsignmentTypeList.Descriptions.Magazine, mafMessaging.EffectiveConsignmentTypeDescription);
		}

		public void TestEffectiveCargoTypeDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			var mafMessaging = TestDataBuilder.GetMAFMessaging(declaration);
			AssertEquals("EffectiveCargoTypeDescription", "", mafMessaging.EffectiveCargoTypeDescription);
			declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			var container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			AssertEquals("EffectiveCargoTypeDescription", "", mafMessaging.EffectiveCargoTypeDescription);
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			AssertEquals("EffectiveCargoTypeDescription", CodedLists.CargoTypeList.Descriptions.Fcl, mafMessaging.EffectiveCargoTypeDescription);
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_FCL_LCL_AIR = ContainerModeList.Codes.LCL;
			AssertEquals("EffectiveCargoTypeDescription", CodedLists.CargoTypeList.Descriptions.Fak, mafMessaging.EffectiveCargoTypeDescription);
			mafMessaging.ZX_CargoType = CodedLists.CargoTypeList.Codes.Bulk;
			AssertEquals("EffectiveCargoTypeDescription", CodedLists.CargoTypeList.Descriptions.Bulk, mafMessaging.EffectiveCargoTypeDescription);
		}

		public void TestEffectiveIsMAFAuditRequiredByCustomsDescription()
		{
			var mafMessaging = TestDataBuilder.GetMAFMessaging(Factory.New<JobDeclaration>());
			AssertEquals("EffectiveIsMAFAuditRequiredByCustomsDescription", YesNoUnknownList.Descriptions.Unknown, mafMessaging.EffectiveIsMAFAuditRequiredByCustomsDescription);
			mafMessaging.ZX_IsMAFAuditRequiredByCustoms = YesNoUnknownList.Codes.Yes;
			AssertEquals("EffectiveIsMAFAuditRequiredByCustomsDescription", YesNoUnknownList.Descriptions.Yes, mafMessaging.EffectiveIsMAFAuditRequiredByCustomsDescription);
		}

		public void TestEffectiveIsCustomsXRayRequiredDescription()
		{
			var mafMessaging = TestDataBuilder.GetMAFMessaging(Factory.New<JobDeclaration>());
			AssertEquals("EffectiveIsCustomsXRayRequiredDescription", YesNoUnknownList.Descriptions.Unknown, mafMessaging.EffectiveIsCustomsXRayRequiredDescription);
			mafMessaging.ZX_IsCustomsXRayRequired = YesNoUnknownList.Codes.Yes;
			AssertEquals("EffectiveIsMAFAuditRequiredByCustomsDescription", YesNoUnknownList.Descriptions.Yes, mafMessaging.EffectiveIsCustomsXRayRequiredDescription);
		}

		public void TestEffectiveIsCustomsCashClientDescription()
		{
			var mafMessaging = TestDataBuilder.GetMAFMessaging(Factory.New<JobDeclaration>());
			AssertEquals("EffectiveIsCustomsCashClientDescription", YesNoUnknownList.Descriptions.Unknown, mafMessaging.EffectiveIsCustomsCashClientDescription);
			mafMessaging.ZX_IsCustomsCashClient = YesNoUnknownList.Codes.Yes;
			AssertEquals("EffectiveIsMAFAuditRequiredByCustomsDescription", YesNoUnknownList.Descriptions.Yes, mafMessaging.EffectiveIsCustomsCashClientDescription);
		}

		public void TestZX_MessagingStatusDescription()
		{
			var mafMessaging = TestDataBuilder.GetMAFMessaging(Factory.New<JobDeclaration>());
			AssertEquals("mafMessaging.ZX_MessagingStatusDescription", MessagingStatusList.Descriptions.NotSentToMpi, mafMessaging.ZX_MessagingStatusDescription);
			mafMessaging.ZX_MessagingStatus = MessagingStatusList.Codes.MpiCleared;
			AssertEquals("mafMessaging.ZX_MessagingStatusDescription", MessagingStatusList.Descriptions.MpiCleared, mafMessaging.ZX_MessagingStatusDescription);
			mafMessaging.ZX_MessagingStatus = MessagingStatusList.Codes.SentAndAcknowledged;
			AssertEquals("mafMessaging.ZX_MessagingStatusDescription", MessagingStatusList.Descriptions.SentAndAcknowledged, mafMessaging.ZX_MessagingStatusDescription);

			mafMessaging.ZX_MessagingStatus = ConsolIPIStatusList.Codes.SentToCustoms;
			AssertEquals("Lookup should now be using TSW statuses", "Sent to Customs", mafMessaging.ZX_MessagingStatusDescription);
			mafMessaging.ZX_MessagingStatus = ConsolIPIStatusList.Codes.PP;
			AssertEquals("Lookup should now be using TSW statuses", "Response Pending", mafMessaging.ZX_MessagingStatusDescription);
			mafMessaging.ZX_MessagingStatus = ConsolIPIStatusList.Codes.HC;
			AssertEquals("Lookup should now be using TSW statuses", "MPI Biosecurity - Entry Held / MPI Food - Cleared", mafMessaging.ZX_MessagingStatusDescription);

			// test old eBACCa jobs will still show the legacy status when Consol TSW / IPI messages are activated.
			mafMessaging.ZX_MessagingStatus = MessagingStatusList.Codes.MpiCleared;
			AssertEquals("Messaging Status Description should fall back to eBACCa status.", MessagingStatusList.Descriptions.MpiCleared, mafMessaging.ZX_MessagingStatusDescription);
		}

		public void TestMessageCollection()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			Factory.Save(); // Save first so I make sure the damn thing is registered editable.
			var message = TestDataBuilder.GetMAFMessaging(declaration).Messages.AddNew();
			message.EM_MessageType = NZMMessage.MessageTypes.Transmit.MessagingRequest;
			message.EM_MessageSubType = NZMMessage.MessageTypes.Transmit.MessageSubTypes.Replacement;
			message.EM_MessageText = "Back In Black" + NZMMessage.SendersReferencePlaceHolder + ":" + NZMMessage.MessageNumberPlaceHolder;
			AssertEquals("declaration.HasChanges", true, declaration.HasChanges);
			Factory.Save();

			var reloadFactory = new BusinessObjectFactory();
			var reloadedDeclaration = reloadFactory.Load<JobDeclaration>(declaration.PK);

			var mafMessaging = TestDataBuilder.GetMAFMessaging(reloadedDeclaration);
			AssertEquals("reloadedDeclaration.MAFMessaging.Messages.Count", 1, mafMessaging.Messages.Count);
			var reloadedMessage = mafMessaging.Messages[0];

			AssertEquals("reloadedMessage.EM_MessageType", NZMMessage.MessageTypes.Transmit.MessagingRequest, reloadedMessage.EM_MessageType);
			AssertEquals("reloadedMessage.EM_MessageSubType", NZMMessage.MessageTypes.Transmit.MessageSubTypes.Replacement, reloadedMessage.EM_MessageSubType);
			AssertEquals("reloadedMessage.EM_MessageText", "Back In Black", reloadedMessage.EM_MessageText.Left(13));
		}

		public void TestAllPropertiesPersistProperly()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			Factory.Save();
			AssertEquals("Precondition: declaration.HasChanges", false, declaration.HasChanges);
			var mafMessaging = TestDataBuilder.GetMAFMessaging(declaration);
			var file = mafMessaging.Files.AddNew().Data;
			file.ZF_DocumentType = DocumentTypeList.Codes.AQISExportDocument;
			AssertEquals("declaration.HasChanges", true, declaration.HasChanges);

			Factory.Save();
			AssertEquals("Precondition: declaration.HasChanges", false, declaration.HasChanges);
			mafMessaging.ZX_MeasurementValue = 1;
			AssertEquals("declaration.HasChanges", true, declaration.HasChanges);
			Factory.Save();

			declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			mafMessaging = TestDataBuilder.GetMAFMessaging(declaration);
			AssertEquals("MAFMessaging.Files.Count", 1, mafMessaging.Files.Count);
			AssertEquals("MAFMessaging.Files[0].Data.ZF_DocumentType", DocumentTypeList.Codes.AQISExportDocument, mafMessaging.Files[0].Data.ZF_DocumentType);
			AssertEquals("MAFMessaging.ZX_MeasurementValue", 1, mafMessaging.ZX_MeasurementValue);
		}

		public void TestLocationOfGoods()
		{
			CreateAirImportConsol();

			var goodsLocationATF = Factory.New<OrgHeader>();
			var goodsLocationATFAddr = goodsLocationATF.Addresses.AddNew();
			var goodsLocationATFAddrCusCode = goodsLocationATFAddr.CustomsCodes.AddNew();
			goodsLocationATFAddrCusCode.OK_CodeType = OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility;
			goodsLocationATFAddrCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.NewZealand;
			goodsLocationATFAddrCusCode.OK_CustomsRegNo = "1234Z";
			goodsLocationATFAddrCusCode.OK_OA_PremisesAddress = goodsLocationATFAddr.PK;

			Consol.JK_OA_UnpackDepotAddress = goodsLocationATFAddr.PK;
			var expectedATFGoodsLocationElement = @"<GoodsLocation>
        <ID>1234Z</ID>
      </GoodsLocation>";

			var mafMessaging = new MAFMessagingBO(new MAFPlugInSupportConsolWrapper(Consol));
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, mafMessaging.Factory, "IPI");
			var ipiBuilder = new IPIMessageBuilder(mafMessaging, TSWTransactionTypes.Original, additionalMessageInformation);
			var ipiMessageString = ipiBuilder.GetXMLMessage();
			AssertEquals("mafMessagingBo.LocationOfGoods - should fall back to ATF value from Consol Unpack Depot", true, ipiMessageString.Contains(expectedATFGoodsLocationElement));

			var goodsLocationCCP = Factory.New<OrgHeader>();
			var goodsLocationCCPCusCode = goodsLocationCCP.CustomsCodes.AddNew();
			goodsLocationCCPCusCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			goodsLocationCCPCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.NewZealand;
			goodsLocationCCPCusCode.OK_CustomsRegNo = "9876A";

			Consol.JK_OA_UnpackDepotAddress = goodsLocationCCP.MainAddress.PK;
			var expectedCCPGoodsLocationElement = @"<GoodsLocation>
        <ID>9876A</ID>
      </GoodsLocation>";

			mafMessaging = new MAFMessagingBO(new MAFPlugInSupportConsolWrapper(Consol));
			ipiBuilder = new IPIMessageBuilder(mafMessaging, TSWTransactionTypes.Original, additionalMessageInformation);
			ipiMessageString = ipiBuilder.GetXMLMessage();
			AssertEquals("mafMessagingBo.LocationOfGoods - should get location code from CCP value first if it exists, from the Consol Unpack Depot", true, ipiMessageString.Contains(expectedCCPGoodsLocationElement));
		}

		public void TestConsolWithASMShipment()
		{
			CreateConsolWithASMShipment();
			var expectedBillsElement = @"<TransportContractDocument>
        <ID>08600544272</ID>
        <TypeCode>MB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>3</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>H69238</ID>
        <TypeCode>HWB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>G05827-17E</ID>
        <TypeCode>HWB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>";

			var mafMessaging = new MAFMessagingBO(new MAFPlugInSupportConsolWrapper(Consol));
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, mafMessaging.Factory, "IPI");
			var ipiBuilder = new IPIMessageBuilder(mafMessaging, TSWTransactionTypes.Original, additionalMessageInformation);
			var ipiMessageString = ipiBuilder.GetXMLMessage();
			AssertEquals("mafMessagingBo.AllBills should not include ASM Shipment", true, ipiMessageString.Contains(expectedBillsElement));
		}

		public void TestHasContainers()
		{
			CreateAirImportConsol();
			var transportEquipmentElement = @"<TransportEquipment>";
			var mafMessaging = new MAFMessagingBO(new MAFPlugInSupportConsolWrapper(Consol));
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, mafMessaging.Factory, "IPI");
			var ipiBuilder = new IPIMessageBuilder(mafMessaging, TSWTransactionTypes.Original, additionalMessageInformation);
			var ipiMessageString = ipiBuilder.GetXMLMessage();
			AssertEquals("HasContainersOrPallets - should be false for Air Consol", false, ipiMessageString.Contains(transportEquipmentElement));

			Create2Bills1ContainerImportConsol();
			mafMessaging = new MAFMessagingBO(new MAFPlugInSupportConsolWrapper(Consol));
			ipiBuilder = new IPIMessageBuilder(mafMessaging, TSWTransactionTypes.Original, additionalMessageInformation);
			ipiMessageString = ipiBuilder.GetXMLMessage();
			AssertEquals("HasContainersOrPallets - Sea Consol with container", true, ipiMessageString.Contains(transportEquipmentElement));
		}

		public void TestCountryOrigin()
		{
			CreateAirImportConsol();
			var shipmentExportCountryElement = @"<ExportationCountryCode>AU</ExportationCountryCode";
			var itemExportCountryElement = @"<Source>
          <CountryCode>AU</CountryCode>
        </Source>";
			var mafMessaging = new MAFMessagingBO(new MAFPlugInSupportConsolWrapper(Consol));
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, mafMessaging.Factory, "IPI");
			var ipiBuilder = new IPIMessageBuilder(mafMessaging, TSWTransactionTypes.Original, additionalMessageInformation);
			var ipiMessageStringAir = ipiBuilder.GetXMLMessage();
			AssertEquals("Has shipment export country", true, ipiMessageStringAir.Contains(shipmentExportCountryElement));
			AssertEquals("Has item export country", true, ipiMessageStringAir.Contains(itemExportCountryElement));

			Create2Bills1ContainerImportConsol();
			mafMessaging = new MAFMessagingBO(new MAFPlugInSupportConsolWrapper(Consol));
			ipiBuilder = new IPIMessageBuilder(mafMessaging, TSWTransactionTypes.Original, additionalMessageInformation);
			var ipiMessageStringSea = ipiBuilder.GetXMLMessage();
			AssertEquals("Has shipment export country", true, ipiMessageStringSea.Contains(shipmentExportCountryElement));
			AssertEquals("Has item export country", true, ipiMessageStringSea.Contains(itemExportCountryElement));
		}

		public void TestConsolWithNonRelatedShipmentsContainers()
		{
			CreateImportConsolWithNonRelevantShipments();
			var expectedBillDetails = @"<TransportContractDocument>
        <ID>GAZ0023</ID>
        <TypeCode>MB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>3</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>HBILL1</ID>
        <TypeCode>BM</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>31B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>HBILL2</ID>
        <TypeCode>BM</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>31B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>";
			var expectedBillDetailsAlternative = @"<TransportContractDocument>
        <ID>GAZ0023</ID>
        <TypeCode>MB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>3</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>HBILL1</ID>
        <TypeCode>BM</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>31B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>HBILL2</ID>
        <TypeCode>BM</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>31B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>";

			var containerElements = @"<TransportEquipment>
        <SequenceNumeric>1</SequenceNumeric>
        <CharacteristicCode>23</CharacteristicCode>
        <FullnessCode>7</FullnessCode>
        <ID>BKGU1111110</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportEquipment>
      <TransportEquipment>
        <SequenceNumeric>2</SequenceNumeric>
        <CharacteristicCode>23</CharacteristicCode>
        <FullnessCode>7</FullnessCode>
        <ID>BKGU1111110</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportEquipment>";

			var expectedPackagingElements = @"<Packaging>
    <SequenceNumeric>1</SequenceNumeric>
    <QuantityQuantity>1</QuantityQuantity>
    <TypeCode>PX</TypeCode>
  </Packaging>
  <Packaging>
    <SequenceNumeric>2</SequenceNumeric>
    <QuantityQuantity>2</QuantityQuantity>
    <TypeCode>PX</TypeCode>
  </Packaging>";

			var mafMessaging = new MAFMessagingBO(new MAFPlugInSupportConsolWrapper(Consol));
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, mafMessaging.Factory, "IPI");
			var ipiBuilder = new IPIMessageBuilder(mafMessaging, TSWTransactionTypes.Original, additionalMessageInformation);
			var ipiMessageString = ipiBuilder.GetXMLMessage();

			AssertEquals("expectedBillDetails: \r\n\r\n" + expectedBillDetails + "\r\n\r\nmessage generated:\r\n\r\n" + ipiMessageString, true, ipiMessageString.Contains(expectedBillDetails) || ipiMessageString.Contains(expectedBillDetailsAlternative));
			AssertEquals("containerElements: ", true, ipiMessageString.Contains(containerElements));
			AssertEquals("expectedPackagingElements:" + "\r\n\r\n" + expectedPackagingElements + "\r\n\r\nmessage generated:\r\n\r\n" + ipiMessageString, true, ipiMessageString.Contains(expectedPackagingElements));
		}

		public void TestConsolPackagingMapsToNZCPackagingUnits()
		{
			CreateConsolWithShipmentPackaging();
			var expectedPackagingElements = @"<Packaging>
    <SequenceNumeric>1</SequenceNumeric>
    <QuantityQuantity>50</QuantityQuantity>
    <TypeCode>CT</TypeCode>
  </Packaging>
  <Packaging>
    <SequenceNumeric>2</SequenceNumeric>
    <QuantityQuantity>10</QuantityQuantity>
    <TypeCode>BX</TypeCode>
  </Packaging>
  <Packaging>
    <SequenceNumeric>3</SequenceNumeric>
    <QuantityQuantity>30</QuantityQuantity>
    <TypeCode>UN</TypeCode>
  </Packaging>";

			var mafMessaging = new MAFMessagingBO(new MAFPlugInSupportConsolWrapper(Consol));
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, mafMessaging.Factory, "IPI");
			var ipiBuilder = new IPIMessageBuilder(mafMessaging, TSWTransactionTypes.Original, additionalMessageInformation);
			var ipiMessageString = ipiBuilder.GetXMLMessage();

			AssertEquals("expectedPackagingElements:" + "\r\n\r\n" + expectedPackagingElements + "\r\n\r\nmessage generated:\r\n\r\n" + ipiMessageString, true, ipiMessageString.Contains(expectedPackagingElements));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return TestDataBuilder.GetMAFMessaging(Factory.New<JobDeclaration>());
		}

		ForwardingConsol Consol
		{
			get
			{
				if (consol == null)
				{
					consol = Factory.NewWithValidTestData<ForwardingConsol>();
				}

				return consol;
			}
		}
		ForwardingConsol consol;

		void Create2Bills1ContainerImportConsol()
		{
			Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			Consol.JK_ConsolMode = Core.Constants.ContainerModes.Groupage;
			Consol.JK_MasterBillNum = "BKG200727OBL1";
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Consol.JK_RL_NKDischargePort = "NZAKL";

			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_IsLinked = false;
			transport.JW_Vessel = "AAL FREMANTLE";
			transport.JW_VoyageFlight = "8765432";
			transport.JW_ATA = new DateTime(2020, 07, 27);
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NZAKL";

			var shipment1 = Consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HBILL1";
			shipment1.JS_OuterPacks = 1;
			shipment1.JS_F3_NKPackType = "PLT";
			shipment1.JS_RL_NKDestination = "NZAKL";
			shipment1.JS_ActualWeight = 1;
			shipment1.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var shipment2 = Consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "HBILL2";
			shipment2.JS_OuterPacks = 2;
			shipment2.JS_F3_NKPackType = "PLT";
			shipment2.JS_RL_NKDestination = "NZAKL";
			shipment2.JS_ActualWeight = 2;
			shipment2.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var container1 = Consol.Containers.AddNew();
			container1.JC_ContainerNum = "BKGU1111110";
			container1.JC_ContainerMode = Core.Constants.ContainerModes.Groupage;

			var packLine1 = Consol.RelatedPackLines.AddNew();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_Description = "STUFF";
			packLine1.JL_F3_NKPackType = "CTN";
			packLine1.JL_JS = shipment1.PK;

			var packLine2 = Consol.RelatedPackLines.AddNew();
			packLine2.JL_PackageCount = 2;
			packLine2.JL_Description = "THINGS";
			packLine2.JL_F3_NKPackType = "CTN";
			packLine2.JL_JS = shipment2.PK;

			Factory.Save();
		}

		void Create1ContainerMultiPacksConsol()
		{
			Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			Consol.JK_ConsolMode = Core.Constants.ContainerModes.Groupage;
			Consol.JK_MasterBillNum = "BKG200727OBL1";
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_RL_NKDischargePort = "NZAKL";

			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_IsLinked = false;
			transport.JW_Vessel = "AAL FREMANTLE";
			transport.JW_VoyageFlight = "8765432";
			transport.JW_ATA = new DateTime(2020, 07, 27);
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NZAKL";

			var shipment1 = Consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HBILL1";
			shipment1.JS_OuterPacks = 1;
			shipment1.JS_F3_NKPackType = "PLT";
			shipment1.JS_RL_NKDestination = "NZAKL";
			shipment1.JS_ActualWeight = 1;
			shipment1.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment1.OuterPackLines.RemoveAndDeleteAll();

			var container1 = Consol.Containers.AddNew();
			container1.JC_ContainerNum = "BKGU1111110";
			container1.JC_ContainerMode = Core.Constants.ContainerModes.Groupage;

			var outerPackLine1 = shipment1.OuterPackLines.AddNew();
			outerPackLine1.JL_PackageCount = 1;
			outerPackLine1.JL_Description = "STUFF";
			outerPackLine1.JL_F3_NKPackType = "CTN";

			var outerPackLine2 = shipment1.OuterPackLines.AddNew();
			outerPackLine2.JL_PackageCount = 2;
			outerPackLine2.JL_Description = "THINGS";
			outerPackLine2.JL_F3_NKPackType = "CTN";

			var outerPackLine3 = shipment1.OuterPackLines.AddNew();
			outerPackLine3.JL_PackageCount = 3;
			outerPackLine3.JL_Description = "THINGS";
			outerPackLine3.JL_F3_NKPackType = "CTN";

			var outerPackLine4 = shipment1.OuterPackLines.AddNew();
			outerPackLine4.JL_PackageCount = 2;
			outerPackLine4.JL_Description = "THINGS";
			outerPackLine4.JL_F3_NKPackType = "CTN";

			Factory.Save();
		}

		void CreateAirImportConsol()
		{
			Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			Consol.JK_ConsolMode = Core.Constants.ContainerModes.Loose;
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Consol.JK_MasterBillNum = "08600544272";
			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_RL_NKDischargePort = "NZAKL";

			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_IsLinked = false;
			transport.JW_VoyageFlight = "NZ17";
			transport.JW_ATA = new DateTime(2020, 08, 21);
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NZAKL";

			var shipment1 = Consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "G85928";
			shipment1.JS_OuterPacks = 50;
			shipment1.JS_F3_NKPackType = "CTN";
			shipment1.JS_RL_NKDestination = "NZAKL";
			shipment1.JS_ActualWeight = 780;
			shipment1.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var shipment2 = Consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "H69238";
			shipment2.JS_OuterPacks = 10;
			shipment2.JS_F3_NKPackType = "BOX";
			shipment2.JS_RL_NKDestination = "NZAKL";
			shipment2.JS_ActualWeight = 500;
			shipment2.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var shipment3 = Consol.Shipments.AddNew();
			shipment3.JS_HouseBill = "G05827-17E";
			shipment3.JS_OuterPacks = 1;
			shipment3.JS_F3_NKPackType = "PLT";
			shipment3.JS_RL_NKDestination = "NZAKL";
			shipment3.JS_ActualWeight = 210;
			shipment3.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var packLine1 = Consol.RelatedPackLines.AddNew();
			packLine1.JL_PackageCount = 50;
			packLine1.JL_Description = "Magazines";
			packLine1.JL_F3_NKPackType = "CTN";
			packLine1.JL_JS = shipment1.PK;

			var packLine2 = Consol.RelatedPackLines.AddNew();
			packLine2.JL_PackageCount = 10;
			packLine1.JL_Description = "Books";
			packLine2.JL_F3_NKPackType = "BOX";
			packLine2.JL_JS = shipment2.PK;

			var packLine3 = Consol.RelatedPackLines.AddNew();
			packLine3.JL_PackageCount = 1;
			packLine3.JL_Description = "Newsprint";
			packLine3.JL_F3_NKPackType = "PLT";
			packLine3.JL_JS = shipment3.PK;

			Factory.Save();
		}

		void CreateConsolWithASMShipment()
		{
			Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			Consol.JK_ConsolMode = Core.Constants.ContainerModes.Loose;
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Consol.JK_MasterBillNum = "08600544272";
			Consol.JK_RL_NKDischargePort = "NZAKL";

			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_IsLinked = false;
			transport.JW_VoyageFlight = "NZ17";
			transport.JW_ATA = new DateTime(2020, 08, 21);
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NZAKL";

			var asmShipment = Consol.Shipments.AddNew();
			asmShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			asmShipment.JS_HouseBill = "G85928";
			asmShipment.JS_OuterPacks = 50;
			asmShipment.JS_F3_NKPackType = "CTN";
			asmShipment.JS_RL_NKDestination = "NZAKL";
			asmShipment.JS_ActualWeight = 780;
			asmShipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var shipment2 = Consol.Shipments.AddNew();
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment2.JS_HouseBill = "H69238";
			shipment2.JS_OuterPacks = 10;
			shipment2.JS_F3_NKPackType = "BOX";
			shipment2.JS_RL_NKDestination = "NZAKL";
			shipment2.JS_ActualWeight = 500;
			shipment2.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var shipment3 = Consol.Shipments.AddNew();
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment3.JS_HouseBill = "G05827-17E";
			shipment3.JS_OuterPacks = 1;
			shipment3.JS_F3_NKPackType = "PLT";
			shipment3.JS_RL_NKDestination = "NZAKL";
			shipment3.JS_ActualWeight = 210;
			shipment3.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var packLine1 = Consol.RelatedPackLines.AddNew();
			packLine1.JL_PackageCount = 50;
			packLine1.JL_Description = "Magazines";
			packLine1.JL_F3_NKPackType = "CTN";
			packLine1.JL_JS = shipment2.PK;

			var packLine2 = Consol.RelatedPackLines.AddNew();
			packLine2.JL_PackageCount = 10;
			packLine2.JL_Description = "Books";
			packLine2.JL_F3_NKPackType = "BOX";
			packLine2.JL_JS = shipment3.PK;

			Factory.Save();
		}

		void CreateImportConsolWithNonRelevantShipments()
		{
			Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			Consol.JK_ConsolMode = Core.Constants.ContainerModes.Groupage;
			Consol.JK_MasterBillNum = "GAZ0023";
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Consol.JK_RL_NKLoadPort = "CNYTN";
			Consol.JK_RL_NKDischargePort = "NZAKL";

			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_IsLinked = false;
			transport.JW_Vessel = "AAL FREMANTLE";
			transport.JW_VoyageFlight = "8765432";
			transport.JW_ATA = new DateTime(2020, 07, 27);
			transport.JW_RL_NKLoadPort = "CNYTN";
			transport.JW_RL_NKDiscPort = "NZAKL";

			var shipment1 = Consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HBILL1";
			shipment1.JS_OuterPacks = 1;
			shipment1.JS_F3_NKPackType = "PLT";
			shipment1.JS_RL_NKDestination = "NZAKL";
			shipment1.JS_ActualWeight = 1;
			shipment1.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var shipment2 = Consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "HBILL2";
			shipment2.JS_OuterPacks = 2;
			shipment2.JS_F3_NKPackType = "PLT";
			shipment2.JS_RL_NKDestination = "NZAKL";
			shipment2.JS_ActualWeight = 2;
			shipment2.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var container1 = Consol.Containers.AddNew();
			container1.JC_ContainerNum = "BKGU1111110";
			container1.JC_ContainerMode = Core.Constants.ContainerModes.Groupage;

			var packLine1 = Consol.RelatedPackLines.AddNew();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_Description = "STUFF";
			packLine1.JL_F3_NKPackType = "CTN";
			packLine1.JL_JS = shipment1.PK;

			var packLine2 = Consol.RelatedPackLines.AddNew();
			packLine2.JL_PackageCount = 2;
			packLine2.JL_Description = "THINGS";
			packLine2.JL_F3_NKPackType = "CTN";
			packLine2.JL_JS = shipment2.PK;

			var unrelatedConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			var unrelatedContainer1 = Consol.Containers.AddNew();
			unrelatedContainer1.JC_ContainerNum = "CHSU0428472";
			unrelatedContainer1.JC_ContainerMode = Core.Constants.ContainerModes.Groupage;
			unrelatedContainer1.JC_JK = unrelatedConsol.PK;
			var packLine3 = unrelatedConsol.RelatedPackLines.AddNew();
			packLine3.JL_PackageCount = 3;
			packLine3.JL_Description = "THINGS";
			packLine3.JL_F3_NKPackType = "CTN";
			packLine3.JL_JS = shipment2.PK;

			var unrelatedContainer2 = Consol.Containers.AddNew();
			unrelatedContainer2.JC_ContainerMode = Core.Constants.ContainerModes.Groupage;
			var packLine4 = unrelatedConsol.RelatedPackLines.AddNew();
			packLine4.JL_PackageCount = 3;
			packLine4.JL_Description = "THINGS";
			packLine4.JL_F3_NKPackType = "CTN";
			packLine4.JL_JS = shipment2.PK;

			Factory.Save();
		}

		void CreateConsolWithCoLoadShipments()
		{
			Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			Consol.JK_ConsolMode = Core.Constants.ContainerModes.Loose;
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Consol.JK_MasterBillNum = "OB0402998";
			Consol.JK_RL_NKDischargePort = "NZAKL";

			var transport = Consol.Transports.MostInterestingTransport;
			transport.JW_IsLinked = false;
			transport.JW_VoyageFlight = "273E";
			transport.JW_ATA = new DateTime(2021, 05, 03);
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NZAKL";

			var container1 = Consol.Containers.AddNew();
			container1.JC_ContainerNum = "CHSU0039483";
			container1.JC_ContainerMode = Core.Constants.ContainerModes.Groupage;

			var coloadShipment = Consol.Shipments.AddNew();
			coloadShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			coloadShipment.JS_HouseBill = "G85928";
			coloadShipment.JS_OuterPacks = 50;
			coloadShipment.JS_F3_NKPackType = "CTN";
			coloadShipment.JS_RL_NKDestination = "NZAKL";
			coloadShipment.JS_ActualWeight = 780;
			coloadShipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var childShipment1 = coloadShipment.CoLoadShipments.AddNew();
			childShipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			childShipment1.JS_HouseBill = "H69238";
			childShipment1.JS_OuterPacks = 10;
			childShipment1.JS_F3_NKPackType = "BOX";
			childShipment1.JS_RL_NKDestination = "NZAKL";
			childShipment1.JS_ActualWeight = 500;
			childShipment1.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var childShipment2 = coloadShipment.CoLoadShipments.AddNew();
			childShipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			childShipment2.JS_HouseBill = "T4289";
			childShipment2.JS_OuterPacks = 20;
			childShipment2.JS_F3_NKPackType = "CTN";
			childShipment2.JS_RL_NKDestination = "NZAKL";
			childShipment2.JS_ActualWeight = 200;
			childShipment2.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			Factory.Save();
		}

		void CreateConsolWithShipmentPackaging()
		{
			Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			Consol.JK_ConsolMode = Core.Constants.ContainerModes.Loose;
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Consol.JK_MasterBillNum = "08600544272";
			Consol.JK_RL_NKDischargePort = "NZAKL";

			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_IsLinked = false;
			transport.JW_VoyageFlight = "NZ17";
			transport.JW_ATA = new DateTime(2020, 08, 21);
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NZAKL";

			var shipment1 = Consol.Shipments.AddNew();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment1.JS_HouseBill = "G85928";
			shipment1.JS_OuterPacks = 50;
			shipment1.JS_F3_NKPackType = "CTN";
			shipment1.JS_RL_NKDestination = "NZAKL";
			shipment1.JS_ActualWeight = 780;
			shipment1.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var shipment2 = Consol.Shipments.AddNew();
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment2.JS_HouseBill = "H69238";
			shipment2.JS_OuterPacks = 10;
			shipment2.JS_F3_NKPackType = "BOX";
			shipment2.JS_RL_NKDestination = "NZAKL";
			shipment2.JS_ActualWeight = 500;
			shipment2.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var shipment3 = Consol.Shipments.AddNew();
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment3.JS_HouseBill = "G05827-17E";
			shipment3.JS_OuterPacks = 30;
			shipment3.JS_F3_NKPackType = "UNT";
			shipment3.JS_RL_NKDestination = "NZAKL";
			shipment3.JS_ActualWeight = 210;
			shipment3.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var packLine1 = Consol.RelatedPackLines.AddNew();
			packLine1.JL_PackageCount = 50;
			packLine1.JL_Description = "Magazines";
			packLine1.JL_F3_NKPackType = "CTN";
			packLine1.JL_JS = shipment1.PK;

			var packLine2 = Consol.RelatedPackLines.AddNew();
			packLine2.JL_PackageCount = 10;
			packLine2.JL_Description = "Books";
			packLine2.JL_F3_NKPackType = "BOX";
			packLine2.JL_JS = shipment2.PK;

			var packLine3 = Consol.RelatedPackLines.AddNew();
			packLine3.JL_PackageCount = 30;
			packLine3.JL_Description = "Clothes";
			packLine3.JL_F3_NKPackType = "UNT";
			packLine3.JL_JS = shipment3.PK;

			Factory.Save();
		}

		#endregion
	}
}
