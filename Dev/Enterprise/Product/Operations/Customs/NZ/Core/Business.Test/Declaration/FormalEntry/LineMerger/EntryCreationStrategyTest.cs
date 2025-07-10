using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Declaration.FormalEntry.Testing
{
	public class EntryCreationStrategyTest : TestCaseWithFactory
	{
		public void TestSupplierGSTFieldsInMergeKey()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_SupplierGSTNumber = "TTG001";
			invoice1.JZ_IsGSTPrePaid = "Y";
			var invoice2 = testDec.Invoices.AddNew();
			invoice1.JZ_SupplierGSTNumber = "TTG001";
			invoice1.JZ_IsGSTPrePaid = "N";
			var invoice3 = testDec.Invoices.AddNew();
			invoice3.JZ_SupplierGSTNumber = "TTG001";
			invoice3.JZ_IsGSTPrePaid = "";
			var invoice4 = testDec.Invoices.AddNew();
			invoice4.JZ_SupplierGSTNumber = "TTG002";
			invoice4.JZ_IsGSTPrePaid = "Y";
			var invoice5 = testDec.Invoices.AddNew();
			invoice5.JZ_SupplierGSTNumber = "TTG002";
			invoice5.JZ_IsGSTPrePaid = "Y";

			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "0206.29.00 26";
			var line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "0206.29.00 26";
			var line3 = invoice3.JobComInvoiceLines.AddNew();
			line3.JI_Tariff = "0206.29.00 26";
			var line4 = invoice4.JobComInvoiceLines.AddNew();
			line4.JI_Tariff = "0206.29.00 26";
			var line5 = invoice5.JobComInvoiceLines.AddNew();
			line5.JI_Tariff = "0206.29.00 26";

			var entryStrategy = new EntryCreationStrategy(new LineMerger(testDec));
			var keyForLine1 = entryStrategy.GetKeyForLine(line1);
			var keyForLine2 = entryStrategy.GetKeyForLine(line2);
			var keyForLine3 = entryStrategy.GetKeyForLine(line3);
			var keyForLine4 = entryStrategy.GetKeyForLine(line4);
			var keyForLine5 = entryStrategy.GetKeyForLine(line5);

			Assert("Line1 and line2 cannot be merged into one - different JZ_IsGSTPrePaid", keyForLine1 != keyForLine2);
			Assert("Line1 and line3 cannot be merged into one - different JZ_IsGSTPrePaid", keyForLine1 != keyForLine3);
			Assert("Line1 and line4 cannot be merged into one - different JZ_SupplierGSTNumber", keyForLine1 != keyForLine4);
			Assert("Line4 and line5 can be merged into one", keyForLine4 == keyForLine5);
			Assert("Key1", keyForLine5.IndexOf(invoice5.JZ_SupplierGSTNumber) >= 0);
			Assert("Key2", keyForLine5.IndexOf(invoice5.JZ_IsGSTPrePaid) >= 0);

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;

			entryStrategy = new EntryCreationStrategy(new LineMerger(testDec));
			keyForLine1 = entryStrategy.GetKeyForLine(line1);
			keyForLine2 = entryStrategy.GetKeyForLine(line2);

			Assert("Line1 and line2 can be merged into one on EXP job", keyForLine1 == keyForLine2);
			Assert("Key1", keyForLine1.IndexOf(invoice1.JZ_SupplierGSTNumber) < 0);
			Assert("Key2", keyForLine1.IndexOf(invoice1.JZ_IsGSTPrePaid) < 0);
		}

		public void TestMergedLineRecyclingDoesntDoubleUpCharges()
		{
			DecCreator.MergeDeclaration();
			Declaration.DeclarationNumber = "88946253";
			var originalEntryHeader = (CusEntryHeader)Declaration.CusEntryHeader;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			DecCreator.MergeDeclaration();
			var completionEntryHeader = (CusEntryHeader)Declaration.CusEntryHeader;
			AssertEquals("Precondition: CompletionEntryHeader.MergedLines.Count", 1, completionEntryHeader.MergedLines.Count);
			var entryLine = completionEntryHeader.MergedLines[0];
			AssertEquals("EntryLine.VFDWholeNZD", 10000m, entryLine.VFDWholeNZD);
		}

		public void TestMergingWithAttachedInvoice_NoNullReference()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice2 = declaration.Invoices.AddNew();
			var invoice2line1 = invoice2.JobComInvoiceLines.AddNew();
			invoice2line1.JI_Tariff = "1020304050";
			var invoice2line2 = invoice2.JobComInvoiceLines.AddNew();
			invoice2line2.JI_Tariff = "2030405060";
			var decCreator = new TestFormalEntryCreator(declaration);
			var invoice1 = Factory.New<JobComInvoiceHeader>();
			var invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line1.JI_Tariff = "3040506070";
			var invoice1Line2 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line2.JI_Tariff = "4050607080";
			Factory.Save();
			decCreator.MergeDeclaration();
			declaration.ActiveEntryHeaders[0].EntryNumber = "SD324";
			invoice1.JZ_JE = declaration.PK;
			decCreator.MergeDeclaration();
			invoice1Line2.JI_LinePrice = 10m;
			AssertNoExceptionThrown(decCreator.MergeDeclaration);
		}

		public void TestMergeDoesntDeactivateTheOriginalCusEntryHeader()
		{
			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			Declaration.JE_OriginalEntryNumber = "12345687";
			DecCreator.MergeDeclaration();
			AssertEquals("Declaration.CustomsEntryHeaders.Count", 2, Declaration.CustomsEntryHeaders.Count);
			AssertEquals("Declaration.CustomsEntryHeaders[0].CH_IsActive", true, Declaration.CustomsEntryHeaders[0].CH_IsActive);
			AssertEquals("Declaration.CustomsEntryHeaders[1].CH_IsActive", true, Declaration.CustomsEntryHeaders[1].CH_IsActive);
		}

		public void TestMergeUsesEffectiveFieldsAndEndsUpWithOneEntryHeaderOnly()
		{
			var declaration = JobDeclaration.New(Factory);
			var decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m, "", "", "");
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "", "", "", 10000m);
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m, "AU", "", "");
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "", "", "", 10000m);
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m, "", "ZA", "");
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "", "", "", 10000m);
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m, "", "", "Q");
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "", "", "", 10000m);

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			decCreator.MergeDeclaration();

			AssertEquals("Declaration.MergedLines.Count", 4, declaration.CusEntryHeader.MergedLines.Count);
			AssertEquals("Declaration.CustomsEntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);
		}

		public void TestActiveDetachedEntryHeaderAndInactiveAttachedEntryHeader()
		{
			var declaration = JobDeclaration.New(Factory);
			var decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m, "", "", "");
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "", "", "", 10000m);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("One active entry is created", 1, declaration.CustomsEntryHeaders.Count);
			var entry1 = declaration.CustomsEntryHeaders[0];

			declaration.ResetToOriginal();
			AssertEquals("Deactivated", false, entry1.IsActive);

			declaration.JE_EDITransmitDate = ZDateTime.Today;
			AssertEquals("PreCondition", 2, declaration.CustomsEntryHeaders.Count);

			var entry2 = declaration.CusEntryHeader;
			AssertNotEquals(entry1, entry2);
			AssertEquals(true, entry2.IsActive);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Should reuse the active entry", 2, declaration.CustomsEntryHeaders.Count);
			AssertEquals(true, entry2.IsActive);
		}

		public void TestMergeDeclarationThatWasAttachedToAnECIManifestDoesntDeactivateTheECIManifestRecord()
		{
			var declaration1 = JobDeclaration.New(Factory);
			var decCreator1 = new TestECIWriteOffCreator(declaration1);
			decCreator1.SetupTestForECIWriteoffWithConsignmentDetails();
			decCreator1.SetupTestForAir();
			decCreator1.SetupTestForImportFromAU();
			decCreator1.SetupTestCommercialInvoiceHeaderFOB100NZD();
			declaration1.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ReadyForManifesting;

			var declaration2 = JobDeclaration.New(Factory);
			var decCreator2 = new TestECIWriteOffCreator(declaration2);
			decCreator2.SetupTestForECIWriteoffWithConsignmentDetails();
			decCreator2.SetupTestForAir();
			decCreator2.SetupTestForImportFromAU();
			decCreator2.SetupTestCommercialInvoiceHeaderFOB100NZD();
			declaration2.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ReadyForManifesting;
			declaration2.JE_OH_ShippingLine = declaration1.JE_OH_ShippingLine;
			Factory.Save();

			var manifestCreator = new ECIWriteOff.Manifesting.NewManifestCreator(Factory, GlbCompany.CurrentCompany.PK);
			var manifests = manifestCreator.PotentialManifests;
			manifests.Load();
			var manifest = manifests.Find(declaration1.JE_MasterBill, declaration1.JE_VoyageFlightNo, declaration1.JE_OH_ShippingLine, declaration1.JE_MessageType, declaration1.BarrierPort, declaration1.BarrierDate);
			var manifestEntryHeader = manifestCreator.CreateManifestFrom(manifest);

			var anotherFactory = new BusinessObjectFactory();
			manifestEntryHeader = anotherFactory.Load<ECIWriteOff.Manifesting.CusEntryHeader>(manifestEntryHeader.PK);
			declaration1 = anotherFactory.Load<JobDeclaration>(declaration1.PK);
			declaration2 = anotherFactory.Load<JobDeclaration>(declaration2.PK);

			AssertEquals("Declaration1.CustomsEntryHeaders.Count", 2, declaration1.CustomsEntryHeaders.Count);
			AssertEquals("Declaration1.CusEntryHeader", manifestEntryHeader, declaration1.CusEntryHeader);
			AssertEquals("Declaration2.CustomsEntryHeaders.Count", 2, declaration2.CustomsEntryHeaders.Count);
			AssertEquals("Declaration2.CusEntryHeader", manifestEntryHeader, declaration2.CusEntryHeader);

			declaration2.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration2.AutoCreatePackagesIfPossible();

			var invoiceHeader2 = declaration2.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0];
			var invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0000.00.00.00K";
			invoiceLine2.JI_Description = "INVOICE LINE 2";
			invoiceLine2.JI_LinePrice = invoiceHeader2.JZ_InvoiceAmount;
			invoiceLine2.JI_QualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.NonQualifying;
			invoiceLine2.JI_RN_NKCountryOfExport = "AU";
			invoiceLine2.JI_CountryOfOrigin = "AU";

			var mergeResultLog = new Customs.Business.SendsMessagesToCustomsReturningResultsAsProperties(true);
			declaration2.DoMerge(mergeResultLog);
			AssertEquals("Declaration2.CusEntryHeader.MergedLines.Count", 1, declaration2.CusEntryHeader.MergedLines.Count);
			AssertEquals("Declaration2.CustomsEntryHeaders.Count", 2, declaration2.CustomsEntryHeaders.Count);
			Assert("Declaration2.CusEntryHeader != ManifestEntryHeader", declaration2.CusEntryHeader != manifestEntryHeader);
			AssertEquals("ManifestEntryHeader.CH_IsActive", true, manifestEntryHeader.CH_IsActive);

			declaration1.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration1.AutoCreatePackagesIfPossible();

			var invoiceHeader1 = declaration1.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0];
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0000.00.00.00K";
			invoiceLine1.JI_Description = "INVOICE LINE 2";
			invoiceLine1.JI_LinePrice = invoiceHeader1.JZ_InvoiceAmount;
			invoiceLine1.JI_QualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.NonQualifying;
			invoiceLine1.JI_RN_NKCountryOfExport = "AU";
			invoiceLine1.JI_CountryOfOrigin = "AU";

			declaration1.DoMerge(mergeResultLog);
			AssertEquals("Declaration1.CusEntryHeader.MergedLines.Count", 1, declaration1.CusEntryHeader.MergedLines.Count);
			AssertEquals("Declaration1.CustomsEntryHeaders.Count", 2, declaration1.CustomsEntryHeaders.Count);
			Assert("Declaration1.CusEntryHeader != ManifestEntryHeader", declaration1.CusEntryHeader != manifestEntryHeader);
			AssertEquals("ManifestEntryHeader.CH_IsActive", true, manifestEntryHeader.CH_IsActive);
		}

		public void TestMergeDeclarationChangedToIPIEntry()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			declaration.JE_VoyageFlightNo = "QF117";
			declaration.JE_MasterBill = "081-11111111";
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV-001";
			invoiceHeader.JZ_InvoiceAmount = 5000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000.00.00.00K";
			invoiceLine.JI_Description = "INVOICE LINE";
			invoiceLine.JI_LinePrice = invoiceHeader.JZ_InvoiceAmount;
			invoiceLine.JI_QualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.NonQualifying;
			invoiceLine.JI_RN_NKCountryOfExport = "AU";
			invoiceLine.JI_CountryOfOrigin = "AU";
			Factory.Save();

			var mergeResultLog = new Customs.Business.SendsMessagesToCustomsReturningResultsAsProperties(true);
			declaration.DoMerge(mergeResultLog);
			AssertEquals("Declaration.CustomsEntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = "ABC123";
			entryHeader.EntryNumber = "ENTRYNUM";
			Factory.Save();

			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			Factory.Save();
			AssertEquals("Declaration.CustomsEntryHeaders.Count", 2, declaration.CustomsEntryHeaders.Count);

			declaration.DoMerge(mergeResultLog);
			AssertEquals("Merge should not generate a new CusEntryHeader - Declaration.CustomsEntryHeaders.Count", 2, declaration.CustomsEntryHeaders.Count);
		}

		public void TestDifferentProductNames()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 10600m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "0206.29.00 26";
			line1.JI_LinePrice = 10000m;
			line1.JI_BrandName = "BrandName";
			line1.JI_RegisteredName = "SameRegisteredName";

			var line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "0206.29.00 26";
			line2.JI_LinePrice = 10000m;
			line2.JI_BrandName = "DifferentBrand";
			line2.JI_RegisteredName = "SameRegisteredName";

			var line3 = invoice1.JobComInvoiceLines.AddNew();
			line3.JI_Tariff = "0206.29.00 26";
			line3.JI_LinePrice = 600m;
			line3.JI_BrandName = "BrandName";
			line3.JI_RegisteredName = "SameRegisteredName";

			var merger = new LineMerger(testDec);
			var entryStrategy = new EntryCreationStrategy(new LineMerger(testDec));
			var keyForLine1 = entryStrategy.GetKeyForLine(line1);
			var keyForLine2 = entryStrategy.GetKeyForLine(line2);
			var keyForLine3 = entryStrategy.GetKeyForLine(line3);

			Assert("Line1 and line2 cannot be merged into one", keyForLine1 != keyForLine2);
			Assert("Key1", keyForLine1.IndexOf(line1.JI_ConcessionOrder) >= 0);
			Assert("Line1 and line3 should be merged into one", keyForLine1 == keyForLine3);
		}

		public void TestZeroRateGSTStopsMerge()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 10600m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "0206.29.00 26";
			line1.JI_LinePrice = 10000m;

			var line2 = invoice1.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "0206.29.00 26";
			line2.JI_LinePrice = 600m;
			line2.JI_IsZeroRatedGST = "Y";

			var merger = new LineMerger(testDec);
			var entryStrategy = new EntryCreationStrategy(new LineMerger(testDec));
			var keyForLine1 = entryStrategy.GetKeyForLine(line1);
			var keyForLine2 = entryStrategy.GetKeyForLine(line2);

			Assert("Line1 and line2 cannot be merged into one - line2 has IsZeroRatedGST", keyForLine1 != keyForLine2);
			Assert("Key1", keyForLine1.IndexOf(line1.JI_ConcessionOrder) >= 0);
		}

		public void TestLineMergeKey()
		{
			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			InvoiceLine.JI_ConcessionCode = "CC";
			InvoiceLine.JI_RN_NKCountryOfExport = "AU";
			InvoiceLine.JI_CountryOfOrigin = "AU";
			InvoiceLine.JI_BrandName = "Brand";
			InvoiceLine.JI_CommonName = "Common";
			InvoiceLine.JI_RegisteredName = "Registered";
			InvoiceLine.JI_TradeName = "Trade";
			InvoiceLine.JI_UsedGoods = false;
			InvoiceLine.JI_GeneticallyModified = false;
			InvoiceLine.JI_IsZeroRatedDuty = "Y";
			InvoiceLine.JI_IsZeroRatedExcise = "N";
			InvoiceLine.JI_IsZeroRatedLevies = "N";
			InvoiceLine.JI_IsZeroRatedGST = "Y";
			InvoiceLine.JI_QualifiesForPreferentialDuty = "Q";
			InvoiceLine.JI_PreferentialCountryGroup = "PI";
			var mergeKey = EntryCreationStrategy.GetKeyForLine(InvoiceLine);

			AssertEquals(true, mergeKey.Contains(InvoiceLine.JI_ConcessionCode));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.JI_RN_NKCountryOfExport));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.JI_CountryOfOrigin));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.JI_BrandName));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.JI_CommonName));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.JI_RegisteredName));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.JI_TradeName));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.JI_UsedGoods));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.JI_GeneticallyModified));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.EffectiveIsZeroRatedDuty));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.EffectiveIsZeroRatedExcise));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.EffectiveIsZeroRatedLevies));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.EffectiveIsZeroRatedGST));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.JI_QualifiesForPreferentialDuty));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.JI_PreferentialCountryGroup));
		}

		public void TestNonTSWLineMergeKey()
		{
			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			InvoiceLine.JI_ConcessionCode = "CC";
			InvoiceLine.JI_RN_NKCountryOfExport = "AU";
			InvoiceLine.JI_CountryOfOrigin = "AU";
			InvoiceLine.JI_BrandName = "Brand";
			InvoiceLine.JI_CommonName = "Common";
			InvoiceLine.JI_RegisteredName = "Registered";
			InvoiceLine.JI_TradeName = "Trade";
			InvoiceLine.JI_UsedGoods = false;
			InvoiceLine.JI_GeneticallyModified = false;
			InvoiceLine.JI_IsZeroRatedDuty = "Y";
			InvoiceLine.JI_IsZeroRatedExcise = "Y";
			InvoiceLine.JI_IsZeroRatedLevies = "Y";
			InvoiceLine.JI_IsZeroRatedGST = "Y";
			var mergeKey = EntryCreationStrategy.GetKeyForLine(InvoiceLine);

			AssertEquals(true, mergeKey.Contains(InvoiceLine.JI_ConcessionCode));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.JI_RN_NKCountryOfExport));
			AssertEquals(true, mergeKey.Contains(InvoiceLine.JI_CountryOfOrigin));
			AssertEquals("TSW Fields should be excluded from merge key in non TSW declarations", false, mergeKey.Contains(InvoiceLine.JI_BrandName));
			AssertEquals("TSW Fields should be excluded from merge key in non TSW declarations", false, mergeKey.Contains(InvoiceLine.JI_CommonName));
			AssertEquals("TSW Fields should be excluded from merge key in non TSW declarations", false, mergeKey.Contains(InvoiceLine.JI_RegisteredName));
			AssertEquals("TSW Fields should be excluded from merge key in non TSW declarations", false, mergeKey.Contains(InvoiceLine.JI_TradeName));
			AssertEquals("TSW Fields should be excluded from merge key in non TSW declarations", false, mergeKey.Contains(InvoiceLine.JI_UsedGoods));
			AssertEquals("TSW Fields should be excluded from merge key in non TSW declarations", false, mergeKey.Contains(InvoiceLine.JI_GeneticallyModified));
		}

		#region Implementation

		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					SetupDeclarationAndDecCreator();
				}
				return fDeclaration;
			}
		}

		JobDeclaration fDeclaration;

		#region InvoiceHeader

		JobComInvoiceHeader InvoiceHeader
		{
			get { return invoiceHeader ?? (invoiceHeader = Declaration.Invoices.AddNew()); }
		}
		JobComInvoiceHeader invoiceHeader;

		#endregion

		#region InvoiceLine

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew()); }
		}
		JobComInvoiceLine invoiceLine;

		#endregion

		#region EntryCreationStrategy

		EntryCreationStrategy EntryCreationStrategy
		{
			get { return entryCreationStrategy ?? (entryCreationStrategy = new EntryCreationStrategy(new LineMerger(Declaration))); }
		}
		EntryCreationStrategy entryCreationStrategy;

		#endregion

		TestFormalEntryCreator DecCreator
		{
			get
			{
				if (fDecCreator == null)
				{
					SetupDeclarationAndDecCreator();
				}
				return fDecCreator;
			}
		}
		TestFormalEntryCreator fDecCreator;

		void SetupDeclarationAndDecCreator()
		{
			fDeclaration = JobDeclaration.New(Factory);
			fDecCreator = new TestFormalEntryCreator(fDeclaration);
			fDecCreator.SetupTestConsignmentDetails();
			fDecCreator.SetupTestForAir();
			fDecCreator.SetupTestForImportFromAU();
			fDecCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			fDecCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			fDecCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m, "", "", "");
			fDecCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "", "", "", 10000m);
		}

		#endregion
	}
}
