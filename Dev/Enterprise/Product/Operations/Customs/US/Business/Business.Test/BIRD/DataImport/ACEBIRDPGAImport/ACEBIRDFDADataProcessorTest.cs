using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEBIRDFDADataProcessorTest : ACEBIRDCommonPGADataProcessorTest
	{
		public void TestProcessFoodSubmissionMessageBlocks()
		{
			invoiceLine.JI_Description = "NATURE’S FINEST REAL FRUIT JUICE, 12 OUNCE BOTTLES";
			invoiceLine.Declaration.US_FDAADTA = new ZDateTime(2015, 07, 24, 11, 05, 00);
			invoiceLine.Declaration.US_SchDEntry = "2704";
			invoiceLine.Declaration.US_FDAContactName = "PGA CONTACT FOR TEST";
			invoiceLine.Declaration.US_FDAContactEmail = "test@test.com";
			invoiceLine.Declaration.US_FDAContactPhoneNo = "091245022";

			var fdaSubmitter = Factory.New<OrgHeader>();
			fdaSubmitter.OH_Code = "ZXCVCXZV";
			fdaSubmitter.MainAddress.OA_Address1 = "HOLLAND VILLAGE";
			fdaSubmitter.MainAddress.OA_Address2 = "HOLLAND VILLAGE2";
			fdaSubmitter.MainAddress.OA_Phone = "+ 1 (234) 5678901";
			fdaSubmitter.MainAddress.OA_City = "CHICAGO";
			fdaSubmitter.MainAddress.OA_RL_NKRelatedPortCode = "USCHI";
			fdaSubmitter.MainAddress.OA_State = "IL";
			fdaSubmitter.MainAddress.OA_PostCode = "987654";
			fdaSubmitter.OH_FullName = "CARGOWISE";
			fdaSubmitter.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "DUNS132123", Core.Constants.CountryCodes.UnitedStates);

			DeclarationTestHelper.AddPGAContact(fdaSubmitter, "BRENDON", "PAINE", "+ 1 (234) 5678317", "b.paine@submitter.com", null);
			invoiceLine.Declaration.JE_OH_FDASubmitter = fdaSubmitter.PK;

			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_FEE;
			fda.US_ProductCode = "52DAA02";
			fda.US_ProdCountry = "FR";
			fda.US_Description = "FRUIT JUICE";
			fda.US_BrandName = "NATURAL";
			fda.US_ProducerType = ProducerFirmTypeList.Codes.M;
			fda.US_InvCurrValue = 10000m;
			fda.US_FME = "Y";
			fda.US_CanDim1 = 14.04m;
			fda.US_CanDim2 = 8m;
			fda.US_CanDim3 = 6.08m;
			fda.US_PackageTrackCode = "DHLX";
			fda.US_PackageTrackNumber = "1234567891";

			fda.AffirmationCodes.AddNew(ACE_AffirmationOfComplianceList.Codes.IFE, "N");
			fda.AffirmationCodes.AddNew(ACE_AffirmationOfComplianceList.Codes.PKC, "A00125");
			fda.AffirmationCodes.AddNew(ACE_AffirmationOfComplianceList.Codes.VOL, "12 OUNCE");

			var lot = fda.Lots.AddNew();
			lot.US_LotNumber = "142536";
			lot.US_StartDate = new ZDateTime(2014, 11, 01);
			lot.US_EndDate = new ZDateTime(2014, 12, 31);

			fda.US_Qty1 = 12m;
			fda.US_UQ1 = "FOZ";

			fda.US_Qty2 = 24m;
			fda.US_UQ2 = "BO";

			fda.US_Qty3 = 1000m;
			fda.US_UQ3 = "CS";
			fda.US_Remarks = "TESTING FOOD WITH REMARKS";

			var license1 = fda.Licenses.AddNew();
			license1.US_CountryCode = "MX";
			license1.US_Number = "KT0554";
			license1.US_StateCode = "MX";
			license1.US_StateDescription = "UNKNOWN STATE";

			Factory.Save();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);
			AssertEquals(fdaSubmitter.PK, DeclarationImported.JE_OH_FDASubmitter);
			AssertEquals("2704", DeclarationImported.US_SchDEntry);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLineImported.US_FDAIndicator);
			AssertEquals(1, invoiceLineImported.ACE_FDALines.Count);

			var fdaImported = invoiceLineImported.ACE_FDALines[0];
			AssertEquals(FDAProgramCodeList.Codes.FOO, fdaImported.US_ProgramCode);
			AssertEquals(FDAProcessingCodeList.Codes.FOO_FEE, fdaImported.US_ProcessingCode);
			AssertEquals("52DAA02", fdaImported.US_ProductCode);
			AssertEquals("Y", fdaImported.US_FME);
			AssertEquals("TESTING FOOD WITH REMARKS", fdaImported.US_Remarks);
			AssertEquals(14.04m, fdaImported.US_CanDim1);
			AssertEquals(8m, fdaImported.US_CanDim2);
			AssertEquals(6.08m, fdaImported.US_CanDim3);
			AssertEquals("DHLX", fdaImported.US_PackageTrackCode);
			AssertEquals("1234567891", fdaImported.US_PackageTrackNumber);
			AssertEquals(1, fdaImported.Licenses.Count);
			AssertEquals(3, fdaImported.AffirmationCodes.Count);
			AssertEquals(1, fdaImported.Lots.Count);

			var licenseImported = fdaImported.Licenses[0];
			AssertEquals("MX", licenseImported.US_CountryCode);
			AssertEquals("MX", licenseImported.US_StateCode);
			AssertEquals("UNKNOWN STATE", licenseImported.US_StateDescription);
			AssertEquals("KT0554", licenseImported.US_Number);

			var affirmationCode0 = fdaImported.AffirmationCodes[0];
			AssertEquals(ACE_AffirmationOfComplianceList.Codes.IFE, affirmationCode0.CY_Code);
			AssertEquals("N", affirmationCode0.CY_Data);
			var affirmationCode1 = fdaImported.AffirmationCodes[1];
			AssertEquals(ACE_AffirmationOfComplianceList.Codes.PKC, affirmationCode1.CY_Code);
			AssertEquals("A00125", affirmationCode1.CY_Data);
			var affirmationCode2 = fdaImported.AffirmationCodes[2];
			AssertEquals(ACE_AffirmationOfComplianceList.Codes.VOL, affirmationCode2.CY_Code);
			AssertEquals("12 OUNCE", affirmationCode2.CY_Data);

			var lotImported = fdaImported.Lots[0];
			AssertEquals("142536", lotImported.US_LotNumber);
			AssertEquals(new ZDateTime(2014, 11, 1), lotImported.US_StartDate);
			AssertEquals(new ZDateTime(2014, 12, 31), lotImported.US_EndDate);
			AssertEquals(0m, lotImported.US_Temperature);
		}

		public void TestProcessGoodsFromFTZ()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0407000028";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-2);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff.UE_PGACodes = "FD3";

			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_GoodsFromFTZ = "B815";

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.JI_Description = "IAN TEST GOODS FROM FTZ";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;

			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;
			fda.US_ProductCode = "40BAR01";
			fda.US_ProducerType = ProducerFirmTypeList.Codes.M;
			fda.US_Qty1 = 100m;
			fda.US_UQ1 = "KG";
			fda.US_Description = "WHOLE BARRY";
			Factory.Save();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(EntryTypeList.Codes.Warehouse, DeclarationImported.US_EntryType);
			AssertEquals("B815", DeclarationImported.US_GoodsFromFTZ);
		}

		public override void TestEndToEndProcessDisclaimedPGA()
		{
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_FDADisclaimReason = PGADisclaimReasonList.Codes.B;
			invoiceLine.JI_Description = "TEST FDA";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, invoiceLineImported.US_FDAIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.B, invoiceLineImported.US_FDADisclaimReason);
			AssertEquals("TEST FDA", invoiceLineImported.JI_Description);
			AssertEquals(0, invoiceLineImported.ACE_FDALines.Count);
		}

		protected override MQEDIMessage GetEDIMessageForEndToEndTest()
		{
			invoiceLine.JI_Description = "COSMETICS PRODUCT";
			declaration.US_FDAADTA = new ZDateTime(2015, 07, 14);
			declaration.US_FDAContactName = "Joo Youm";
			declaration.US_FDAContactPhoneNo = "5555555555";
			declaration.US_FDAContactEmail = "joo.youm@test.com";
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.COS;
			fda.US_ProductCode = "203AB05";
			fda.US_ProdCountry = "CA";
			fda.US_Description = "TEST";
			fda.US_Qty1 = 1000M;
			fda.US_UQ1 = "LB";
			fda.US_Qty2 = 100m;
			fda.US_UQ2 = "CT";
			fda.US_TotalValue = 2000m;
			fda.US_UnitValue = 100m;
			fda.US_ManufacturerAddress = manufacturerAddress.PK;
			fda.US_OA_ShipperAddress = manufacturerAddress.PK;
			fda.US_FDAImporterAddress = impAddress.PK;

			Factory.Save();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			return builder.PopulateMessage();
		}

		protected override void AssertEndToEndTestResult()
		{
			AssertEquals(new ZDateTime(2015, 7, 14), DeclarationImported.US_FDAADTA);
			AssertEquals("JOO YOUM", DeclarationImported.US_FDAContactName);
			AssertEquals("5555555555", DeclarationImported.US_FDAContactPhoneNo);
			AssertEquals("JOO.YOUM@TEST.COM", DeclarationImported.US_FDAContactEmail);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLineImported.US_FDAIndicator);
			AssertEquals("COSMETICS PRODUCT", invoiceLineImported.JI_Description);
			AssertEquals(1, invoiceLineImported.ACE_FDALines.Count);

			var fdaImported = invoiceLineImported.ACE_FDALines[0];
			CombineAssertions(() =>
			{
				AssertEquals(FDAProgramCodeList.Codes.COS, fdaImported.US_ProgramCode);
				AssertEquals("203AB05", fdaImported.US_ProductCode);
				AssertEquals(Core.Constants.CountryCodes.Canada, fdaImported.US_ProdCountry);
				AssertEquals("TEST", fdaImported.US_Description);
				AssertEquals(2000m, fdaImported.US_InvCurrValue);
				AssertEquals(100m, fdaImported.US_UnitValue);
				AssertEquals(manufacturerAddress.PK, fdaImported.US_ManufacturerAddress);
				AssertEquals(manufacturerAddress.PK, fdaImported.US_OA_ShipperAddress);
				AssertEquals(impAddress.PK, fdaImported.US_FDAImporterAddress);
				AssertEquals(0, fdaImported.Lots.Count);
				AssertEquals(1000m, fdaImported.US_Qty1);
				AssertEquals("LB", fdaImported.US_UQ1);
				AssertEquals(100m, fdaImported.US_Qty2);
				AssertEquals("CT", fdaImported.US_UQ2);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;

			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_Code = "TESTMANU";
			manufacturer.OH_RL_NKClosestPort = "DEBRE";
			DeclarationTestHelper.AddPGAContact(manufacturer, "Test", "Contact For FDA For Testing", null, "TestContact@test.com", null);
			manufacturerAddress = manufacturer.Addresses.AddNew();
			manufacturerAddress.OA_Address1 = "TEST Manufacturer Address";
			manufacturerAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "456378259");
			manufacturerAddress.OA_PostCode = "1234567890";

			var fdaImporter = Factory.New<OrgHeader>();
			fdaImporter.OH_Code = "TESTIMP";
			fdaImporter.OH_RL_NKClosestPort = "DEBRE";
			DeclarationTestHelper.AddPGAContact(fdaImporter, "John", "Smith", null, "JohnTestContact@test.com", null);
			impAddress = fdaImporter.Addresses.AddNew();
			impAddress.OA_Address1 = "TEST IMPORTER ADDRESS";
			impAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "4568214563");
		}
		OrgAddress manufacturerAddress;
		OrgAddress impAddress;
	}
}
