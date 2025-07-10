using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEBIRDFSISDataProcessorTest : ACEBIRDCommonPGADataProcessorTest
	{
		public override void TestEndToEndProcessDisclaimedPGA()
		{
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_FSISDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.JI_Description = "TEST FSIS DISCLAIMED";
			Factory.Save();

			var action = GetAction(declaration);
			action.US_AcknowledgeAndSign = true;
			action.US_DateOfDeclaration = new ZDateTime(2016, 07, 20);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, invoiceLineImported.US_FSISInd);
			AssertEquals(PGADisclaimReasonList.Codes.A, invoiceLineImported.US_FSISDisclaimReason);
			AssertEquals("TEST FSIS DISCLAIMED", invoiceLineImported.JI_Description);
		}

		protected override MQEDIMessage GetEDIMessageForEndToEndTest()
		{
			declaration.US_EnableCRL = false;
			declaration.US_FDAContactName = "WENDY THE DESTROYER";
			declaration.US_FDAContactPhoneNo = "+164285648734";
			declaration.US_FDAContactEmail = "WENDY@WHERE.COM";

			var ior = Factory.New<OrgHeader>();
			declaration.IOROrgPK = ior.PK;
			ior.OH_Code = "TESTIOR";
			ior.OH_FullName = "IMPORTER OF RECORD";
			var iorAddress = ior.MainAddress;
			iorAddress.OA_Address1 = "IOR ADDRESS 1";
			iorAddress.OA_Address2 = "IOR ADDRESS 2";
			iorAddress.OA_City = "SYDNEY";
			iorAddress.OA_RL_NKRelatedPortCode = "US2CW";
			iorAddress.OA_State = "NSW";
			iorAddress.OA_PostCode = "2017";
			DeclarationTestHelper.AddPGAContact(ior, "IOR", "ALEXANDER", "04 123456", "IOR EMAIL", "IOR FAX");

			invoiceLine.JI_Description = "FSIS TEST";
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
			lot2.US_LotNumber = "LOT 2";
			lot2.US_NoOfUnit1 = 11;
			lot2.US_UQ1 = ShippingOrPackingingUnitList.Codes.Cup;
			lot2.US_ShippingMarks = "MARK 2";
			lot2.US_NetWeight = 77m;
			lot2.US_WeightUQ = Core.Constants.Weight.Ounces;

			lot2.US_Species = FSISProductSpeciesNameList.Codes.BeefMeat;
			lot2.US_ProductQualifierCode = FSISProductQualifierCodeList.Codes.FCNS;
			lot2.US_ProductCharacteristicQualifier = EEPCharacteristicList.Codes._1A;
			lot2.US_ProducingEstNo = "PRO EST 2";

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
			cer2.US_DateOfInspection = new ZDateTime(2013, 9, 25);

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
			lot3.US_ProducingEstNo = "PRO EST 3";

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
			lot4.US_ProductQualifierCode = FSISProductQualifierCodeList.Codes.NHTS;
			lot4.US_ProductCharacteristicQualifier = NHTSCharacteristicList.Codes._1H;
			lot4.US_ProducingEstNo = "PRO EST 4";

			Factory.Save();

			var action = GetAction(declaration);
			action.US_DateOfDeclaration = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			return builder.PopulateMessage();
		}

		protected override void AssertEndToEndTestResult()
		{
			AssertEquals("IOR ALEXANDER", DeclarationImported.US_FDAContactName);
			AssertEquals("04123456", DeclarationImported.US_FDAContactPhoneNo);
			AssertEquals("IOR EMAIL", DeclarationImported.US_FDAContactEmail);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLineImported.US_FSISInd);
			AssertEquals(importer.PK, invoiceLineImported.InvoiceHeader.JZ_OH_Buyer);
			AssertEquals("FSIS TEST", invoiceLineImported.JI_Description);
			AssertEquals(2, invoiceLineImported.FSISLines.Count);

			CombineAssertions(() =>
			{
				var fsisLine0Imported = invoiceLineImported.FSISLines[0];
				AssertEquals("CER1", fsisLine0Imported.US_HealthCertificateNumber);
				AssertEquals(Core.Constants.CountryCodes.Australia, fsisLine0Imported.US_UC_NKCertificateIssuerCountry);
				AssertEquals(Core.Constants.CountryCodes.VietNam, fsisLine0Imported.US_UC_NKCountryOfOrigin);
				AssertEquals("Value cleared because of IsElectronicallyCertificated", ZString.Empty, fsisLine0Imported.US_ProductID);
				AssertEquals("Value cleared because of IsElectronicallyCertificated", ZString.Empty, fsisLine0Imported.US_ProductIDQualifier);
				AssertEquals("Value cleared because of IsElectronicallyCertificated", ZString.Empty, fsisLine0Imported.US_IntendedUseCode);
				AssertEquals("Value cleared because of IsElectronicallyCertificated", ZString.Empty, fsisLine0Imported.US_ExportingEstNo);
				AssertEquals("USLAX", fsisLine0Imported.US_ImportingEstNo);
				AssertEquals("SEAL1,SEAL2,SEAL3", fsisLine0Imported.US_SealNumbers);
				AssertEquals(new ZDateTime(2013, 9, 24), fsisLine0Imported.US_DateOfInspection);
				AssertEquals("No lots sent in message because of IsElectronicallyCertificated", 0, fsisLine0Imported.Lots.Count);

				var fsisLine1Imported = invoiceLineImported.FSISLines[1];
				AssertEquals("CER2", fsisLine1Imported.US_HealthCertificateNumber);
				AssertEquals(Core.Constants.CountryCodes.VietNam, fsisLine1Imported.US_UC_NKCertificateIssuerCountry);
				AssertEquals(Core.Constants.CountryCodes.Yemen, fsisLine1Imported.US_UC_NKCountryOfOrigin);
				AssertEquals("203918503910256", fsisLine1Imported.US_ProductID);
				AssertEquals(GlobalUniqueProductCodeQualifierList.Codes.AI, fsisLine1Imported.US_ProductIDQualifier);
				AssertEquals(ACEIntendedUseBaseCodeList.Codes._210000, fsisLine1Imported.US_IntendedUseCode);
				AssertEquals("EXP EST 2", fsisLine1Imported.US_ExportingEstNo);
				AssertEquals("USCHI", fsisLine1Imported.US_ImportingEstNo);
				AssertEquals("SEALA,SEALB,SEALC", fsisLine1Imported.US_SealNumbers);
				AssertEquals(new ZDateTime(2013, 9, 25), fsisLine1Imported.US_DateOfInspection);
				AssertEquals(2, fsisLine1Imported.Lots.Count);

				var lotLine1_0Imported = fsisLine1Imported.Lots[0];
				AssertEquals("LOT 3", lotLine1_0Imported.US_LotNumber);
				AssertEquals(10, lotLine1_0Imported.US_NoOfUnit1);
				AssertEquals(ShippingOrPackingingUnitList.Codes.Bag, lotLine1_0Imported.US_UQ1);
				AssertEquals(20, lotLine1_0Imported.US_NoOfUnit2);
				AssertEquals(ShippingOrPackingingUnitList.Codes.Carton, lotLine1_0Imported.US_UQ2);
				AssertEquals(new ZDateTime(2014, 1, 4), lotLine1_0Imported.US_StartDate);
				AssertEquals(new ZDateTime(2014, 10, 5), lotLine1_0Imported.US_EndDate);
				AssertEquals("MARK 3", lotLine1_0Imported.US_ShippingMarks);
				AssertEquals("NetWeight in pound", 0.15m, lotLine1_0Imported.US_NetWeight);
				AssertEquals(Core.Constants.Weight.Pounds, lotLine1_0Imported.US_WeightUQ);
				AssertEquals(FSISProductSpeciesNameList.Codes.GoosePoultry, lotLine1_0Imported.US_Species);
				AssertEquals(FSISProductQualifierCodeList.Codes.HTSS, lotLine1_0Imported.US_ProductQualifierCode);
				AssertEquals(HTSSCharacteristicList.Codes._1F, lotLine1_0Imported.US_ProductCharacteristicQualifier);
				AssertEquals("PRO EST 3", lotLine1_0Imported.US_ProducingEstNo);
				AssertEquals("SRC EST 2", lotLine1_0Imported.US_SourceEstNo);
				AssertEquals("CA", lotLine1_0Imported.US_SourceCountry);

				var lotLine1_1Imported = fsisLine1Imported.Lots[1];
				AssertEquals("LOT 4", lotLine1_1Imported.US_LotNumber);
				AssertEquals(13, lotLine1_1Imported.US_NoOfUnit1);
				AssertEquals(ShippingOrPackingingUnitList.Codes.Case, lotLine1_1Imported.US_UQ1);
				AssertEquals("MARK 4", lotLine1_1Imported.US_ShippingMarks);
				AssertEquals("NetWeight in pound", 160937.45m, lotLine1_1Imported.US_NetWeight);
				AssertEquals(Core.Constants.Weight.Pounds, lotLine1_1Imported.US_WeightUQ);
				AssertEquals(FSISProductSpeciesNameList.Codes.GoosePoultry, lotLine1_1Imported.US_Species);
				AssertEquals(FSISProductQualifierCodeList.Codes.NHTS, lotLine1_1Imported.US_ProductQualifierCode);
				AssertEquals(NHTSCharacteristicList.Codes._1H, lotLine1_1Imported.US_ProductCharacteristicQualifier);
				AssertEquals("PRO EST 4", lotLine1_1Imported.US_ProducingEstNo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Declared;

			importer = Factory.New<OrgHeader>();
			invoiceLine.InvoiceHeader.JZ_OH_Buyer = importer.PK;
			importer.OH_Code = "TESTIMP";
			importer.OH_FullName = "BUYER";
			var imAddress = importer.MainAddress;
			imAddress.OA_Address1 = "BUYER ADDRESS 1";
			imAddress.OA_Address2 = "BUYER ADDRESS 2";
			imAddress.OA_City = "MELBOURN";
			imAddress.OA_RL_NKRelatedPortCode = "US2CW";
			imAddress.OA_State = "MEL";
			imAddress.OA_PostCode = "2011";
			DeclarationTestHelper.AddPGAContact(importer, "BUYER", "CONTACT", "04 654321", "BUYER EMAIL", "BUYER FAX");
		}
		OrgHeader importer;
	}
}
