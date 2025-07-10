using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DeclarationMessageDataPrint))]
	sealed class DeclarationMessageDataPrintTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIVisualizerNoteSupporterMembers()
		{
			var dataPrint = GetNewBusinessObject();

			var supporter = dataPrint as IVisualizerNoteSupporter;
			AssertNotNull("DeclarationDataPrint should implement IVisualizerNoteSupporter", supporter);
			AssertEquals("supporter.PK", Declaration.PK, supporter.PK);
			AssertEquals("supporter.TableCode", JobDeclarationSchema.Constants.Prefix, supporter.TableCode);
			AssertEquals("supporter.ChildBusinessObjectPK", ZGuid.Empty, supporter.ChildBusinessObjectPK);
		}

		public void TestPGARecapLines()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "TEST PRODUCT";

			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "TV";
			broker.GS_FullName = "BROKER";
			broker.GS_WorkPhone = "04 010101";
			broker.GS_EmailAddress = "BROKER EMAIL";
			broker.GS_FaxNum = "BROKER FAX";

			var ior = Factory.New<OrgHeader>();
			ior.OH_FullName = "IMPORTER OF RECORD";
			var iorAddress = ior.MainAddress;
			iorAddress.OA_Address1 = "IOR ADDRESS 1";
			iorAddress.OA_Address2 = "IOR ADDRESS 2";
			iorAddress.OA_City = "SYDNEY";
			iorAddress.OA_State = "NSW";
			iorAddress.OA_PostCode = "2017";

			DeclarationTestHelper.AddPGAContact(ior, "IOR", "ALEXANDER THE GREATEST OF ALL", "04 123456", "IOR EMAIL", "IOR FAX");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_DateOfArrival = new ZDateTime(2016, 1, 1);
			declaration.US_CertifyCargoRelease = true;
			declaration.JE_GS_NKCusAgent = broker.GS_Code;
			declaration.US_FDAContactEmail = "BOB@EMAIL.COM";
			declaration.US_FDAContactName = "CONTACT BOB";
			declaration.US_FDAContactPhoneNo = "6013942234";
			declaration.IOROrgPK = ior.PK;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_Tariff = "0106199120";
			invoiceLine.JI_Description = "LIVE DOGS";
			invoiceLine.JI_OP = product.PK;

			#region FSIS

			invoiceLine.JI_Description = "FSIS TEST";
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Declared;
			var cer1 = invoiceLine.FSISLines.AddNew();
			cer1.US_HealthCertificateNumber = "CER1";
			cer1.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.Australia;
			cer1.US_CommercialDescription = "PRODUCT ONE";
			cer1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.VietNam;
			cer1.US_ProductID = "100578620002680";
			cer1.US_ProductIDQualifier = GlobalUniqueProductCodeQualifierList.Codes.SRV;
			cer1.US_IntendedUseCode = ACEIntendedUseBaseCodeList.Codes._260000;
			cer1.US_ExportingEstNo = "EXP EST 1";
			cer1.US_ImportingEstNo = "USLAX";
			cer1.US_SealNumbers = "SEAL1,SEAL2,SEAL3";
			cer1.US_DateOfInspection = new ZDateTime(2013, 9, 24);

			var lot1 = cer1.Lots.AddNew();
			lot1.US_LotNumber = "LOT 1";
			lot1.US_NoOfUnit1 = 1;
			lot1.US_UQ1 = ShippingOrPackingingUnitList.Codes.Case;
			lot1.US_NoOfUnit2 = 2;
			lot1.US_UQ2 = ShippingOrPackingingUnitList.Codes.Cylinder;
			lot1.US_ShippingMarks = "MARK 1";
			lot1.US_NetWeight = 7m;
			lot1.US_WeightUQ = Core.Constants.Weight.Pounds;

			lot1.US_Species = FSISProductSpeciesNameList.Codes.GoatMeat;
			lot1.US_ProductQualifierCode = FSISProductQualifierCodeList.Codes.EEP;
			lot1.US_ProductCharacteristicQualifier = EEPCharacteristicList.Codes._3B;
			lot1.US_ProducingEstNo = "PRO EST 1";

			var lot2 = cer1.Lots.AddNew();
			lot2.US_LotNumber = "LOT 1";
			lot2.US_NoOfUnit1 = 11;
			lot2.US_UQ1 = ShippingOrPackingingUnitList.Codes.Cup;
			lot2.US_ShippingMarks = "MARK 2";
			lot2.US_NetWeight = 77;
			lot2.US_WeightUQ = Core.Constants.Weight.Ounces;

			lot2.US_Species = FSISProductSpeciesNameList.Codes.GoatMeat;
			lot2.US_ProductQualifierCode = FSISProductQualifierCodeList.Codes.EEP;
			lot2.US_ProductCharacteristicQualifier = EEPCharacteristicList.Codes._3B;
			lot2.US_ProducingEstNo = "PRO EST 1";

			var cer2 = invoiceLine.FSISLines.AddNew();
			cer2.US_HealthCertificateNumber = "CER2";
			cer2.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.VietNam;
			cer2.US_CommercialDescription = "PRODUCT TWO";
			cer2.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Yemen;
			cer2.US_ProductID = "203918503910256";
			cer2.US_ProductIDQualifier = GlobalUniqueProductCodeQualifierList.Codes.AI;
			cer2.US_IntendedUseCode = ACEIntendedUseBaseCodeList.Codes._210000;
			cer2.US_ExportingEstNo = "EXP EST 2";
			cer2.US_ImportingEstNo = "USCHI";
			cer2.US_SealNumbers = "SEALA,SEALB,SEALC";
			cer2.US_DateOfInspection = new ZDateTime(2013, 9, 24);

			var lot3 = cer2.Lots[0];
			lot3.US_LotNumber = "LOT 3";
			lot3.US_NoOfUnit1 = 10;
			lot3.US_UQ1 = ShippingOrPackingingUnitList.Codes.Bag;
			lot3.US_NoOfUnit2 = 20;
			lot3.US_UQ2 = ShippingOrPackingingUnitList.Codes.Carton;
			lot3.US_StartDate = new ZDateTime(2014, 1, 4);
			lot3.US_EndDate = new ZDateTime(2014, 10, 5);

			lot3.US_ShippingMarks = "MARK 3";
			lot3.US_NetWeight = 70m;
			lot3.US_WeightUQ = Core.Constants.Weight.Grams;

			lot3.US_Species = FSISProductSpeciesNameList.Codes.GoosePoultry;
			lot3.US_ProductQualifierCode = FSISProductQualifierCodeList.Codes.HTSS;
			lot3.US_ProductCharacteristicQualifier = HTSSCharacteristicList.Codes._1F;
			lot3.US_ProducingEstNo = "PRO EST 2";

			lot3.US_SourceEstNo = "SRC EST 2";
			lot3.US_SourceCountry = "CA";

			var lot4 = cer2.Lots[1];
			lot4.US_LotNumber = "LOT 4";
			lot4.US_NoOfUnit1 = 13;
			lot4.US_UQ1 = ShippingOrPackingingUnitList.Codes.Case;
			lot4.US_ShippingMarks = "MARK 4";
			lot4.US_NetWeight = 73m;
			lot4.US_WeightUQ = Core.Constants.Weight.Tonnes;

			lot4.US_Species = FSISProductSpeciesNameList.Codes.GoosePoultry;
			lot4.US_ProductQualifierCode = FSISProductQualifierCodeList.Codes.HTSS;
			lot4.US_ProductCharacteristicQualifier = HTSSCharacteristicList.Codes._1F;
			lot4.US_ProducingEstNo = "PRO EST 2";

			#endregion

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var action = new EntryHeaderMessageSendingAction(declaration.ActiveEntryHeaders.EntrySummaryEntry, ImportMessageStatusList.MessageType.EntrySummary, collection);
			action.US_CertifyCargoRelease = true;
			action.US_AcknowledgeAndSign = true;
			action.US_DateOfDeclaration = new ZDateTime(2016, 1, 2);
			cer1.InvoiceHeader.US_FSISSignDate = new ZDateTime(2016, 1, 2);
			var entryLineMessageBlockCollection = new JobDeclarationDocumentSupporter(declaration).EntryLineMessageBlockCollection;
			var lines = new DeclarationMessageDataPrint(declaration, entryLineMessageBlockCollection).PGARecapLines;
			AssertMultilineASCIIEquals("Data", string.Format(
@"Entry Line: 1   Product: TEST PRODUCT
        PGA Line: 1   Description: FSIS TEST   Intended Use Code: 260.000 For Research Use as Human Food
        PGA: FSI   Program: FSI (Applicable to all USDA/FSIS programs)   GS1 Global Trade Item Number: 100578620002680
        Country of Production (39): VN Viet Nam
        ISO Country Code (ISO): AU Australia   FSIS Meat, Poultry or Egg Products Foreign Inspection Certificate (FS7): CER1
        Importer: IMPORTER OF RECORD
                IOR ADDRESS 1 IOR ADDRESS 2 SYDNEY NSW 2017 US
                IOR ALEXANDER THE GREATEST OF ALL   Phone: 04123456   Email or Fax: IOR EMAIL
        Additional Roles: CI - Certifying Individual
        Customs broker: EDI CUSTOMS BROKERS
                10 HUTCHESON STREET ALBION  QLD 4010 AU
                CONTACT BOB   Phone: 6013942234   Email or Fax: BOB@EMAIL.COM
        Type: FSIS 9540-1 - 956   Entity Role: Certifying Individual   Declaration: FS3 - Agreement to hold goods intact (Form 9540-1)   Certification Y   Date Of Signature: 02-Jan-16
        General Remarks (GEN): SEAL1, SEAL2, SEAL3
        Product location for regulatory authority inspection (Date: 24-Sep-13 Location: Inspection Establishment Number Qualifier (8) - USLAX)
{0}Entry Line: 1   Product: TEST PRODUCT
        PGA Line: 2   Description: FSIS TEST   Intended Use Code: 210.000 For Personal Use as Human Food
        PGA: FSI   Program: FSI (Applicable to all USDA/FSIS programs)   UPC (Universal product code): 203918503910256
        Country of Production (39): YE Yemen
        ISO Country Code (ISO): VN Viet Nam   FSIS Meat, Poultry or Egg Products Foreign Inspection Certificate (FS7): CER2
        FSIS - Product Name Category (FS1)   Category: 12   Commodity: HTSS   Characteristic: 1F
        ID: EXP EST 2
        ID: PRO EST 2
        ID: SRC EST 2
        Country of Source (30): CA Canada
        Lot NumberLOT 3   Production Date: Start - 04-Jan-14 End - 05-Oct-14
        Packaging: Qty1: 10 BG (ID: MARK 3),  Qty2: 20 CT
        Line Detail:  Net - 0.15 LB
        FSIS - Product Name Category (FS1)   Category: 12   Commodity: HTSS   Characteristic: 1F
        ID: EXP EST 2
        ID: PRO EST 2
        Lot NumberLOT 4
        Packaging: Qty1: 13 CS (ID: MARK 4)
        Line Detail:  Net - 160937.45 LB
        Importer: IMPORTER OF RECORD
                IOR ADDRESS 1 IOR ADDRESS 2 SYDNEY NSW 2017 US
                IOR ALEXANDER THE GREATEST OF ALL   Phone: 04123456   Email or Fax: IOR EMAIL
        Additional Roles: CI - Certifying Individual
        Customs broker: EDI CUSTOMS BROKERS
                10 HUTCHESON STREET ALBION  QLD 4010 AU
                CONTACT BOB   Phone: 6013942234   Email or Fax: BOB@EMAIL.COM
        Type: FSIS 9540-1 - 956   Entity Role: Certifying Individual   Declaration: FS3 - Agreement to hold goods intact (Form 9540-1)   Certification Y   Date Of Signature: 02-Jan-16
        General Remarks (GEN): SEALA, SEALB, SEALC
        Product location for regulatory authority inspection (Date: 24-Sep-13 Location: Inspection Establishment Number Qualifier (8) - USCHI)", System.Environment.NewLine), new ZStringBuilder(lines).ToStringWithNewLineBetweenAppends());
		}

		protected override BusinessObject GetNewBusinessObject() => new DeclarationMessageDataPrint(Declaration, EntryLineMessageBlockCollection);

		IEnumerable<(ICusEntryLine entryLine, List<MessageBlock> messages)> EntryLineMessageBlockCollection => new JobDeclarationDocumentSupporter(Declaration).EntryLineMessageBlockCollection;

		JobDeclaration declaration;
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				}
				return declaration;
			}
		}
	}
}
