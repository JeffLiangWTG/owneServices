using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconInvoiceLineFlatFileDataTransferProcessorTest : TestCaseWithFactory
	{
		public void TestThereIsNoChargesAndFeesWhenTheChargeValuesFromFileIsEmptyAfterImport()
		{
			ReconFlattenedDataLineCollection collection = new ReconFlattenedDataLineCollection(Factory);
			var dataLine1 = collection.AddNew();
			dataLine1.EntryNumber = "XJ51";
			dataLine1.LineNumber = 1;
			dataLine1.ImportationDate = new ZDateTime(2008, 10, 25);
			dataLine1.EntryDate = new ZDateTime(2008, 10, 23);
			dataLine1.EntryPort = "2506";
			dataLine1.ReconTariff = "0000000001";
			dataLine1.OrgLineNumber = "100";
			dataLine1.MessageMode = JobApplicationCodeList.Codes.ACS;
			ReconDeclaration recon = new ReconDeclaration(Factory.New<JobDeclaration>());
			new ReconInvoiceLineFlatFileDataTransferProcessor().ImportReconDataFromCollection(collection, recon);
			AssertEquals(1, recon.OriginalEntries.Count);
			var entry = recon.OriginalEntries[0];
			var invoice = recon.Invoices[0];
			AssertEquals(1, invoice.JobComInvoiceLines.Count);
			var invoiceLine = invoice.JobComInvoiceLines[0];
			AssertEquals(0, invoiceLine.ReconOriginalCharges.Count);
			AssertEquals(0, invoiceLine.FeeCusCodes.Count);
		}

		public void TestDataAreImportedAsProvided()
		{
			USCTariffRule tariffRule = AddSecondaryTariffRule("0000000001", "1111111111", "1111111112");
			ReconFlattenedDataLineCollection collection = new ReconFlattenedDataLineCollection(Factory);
			ReconFlattenedDataLine dataLine1 = collection.AddNew();
			dataLine1.EntryNumber = "XJ51";
			dataLine1.LineNumber = 1;
			dataLine1.ImportationDate = new ZDateTime(2008, 10, 25);
			dataLine1.EntryDate = new ZDateTime(2008, 10, 23);
			dataLine1.EntryPort = "2506";
			dataLine1.ReconTariff = "0000000001";
			dataLine1.OrgLineNumber = "100";
			dataLine1.MessageMode = JobApplicationCodeList.Codes.ACS;
			ReconFlattenedDataLine dataLine2 = collection.AddNew();
			dataLine2.EntryNumber = "XJ51";
			dataLine2.LineNumber = 2;
			dataLine2.ImportationDate = new ZDateTime(2008, 10, 25);
			dataLine2.EntryDate = new ZDateTime(2008, 10, 23);
			dataLine2.EntryPort = "2506";
			dataLine2.IsChildLine = true;
			dataLine2.ReconTariff = "1111111111";
			dataLine2.OrgLineNumber = "1";
			dataLine2.MessageMode = JobApplicationCodeList.Codes.ACS;
			ReconFlattenedDataLine dataLine3 = collection.AddNew();
			dataLine3.EntryNumber = "XJ51";
			dataLine3.LineNumber = 3;
			dataLine3.ImportationDate = new ZDateTime(2008, 10, 25);
			dataLine3.EntryDate = new ZDateTime(2008, 10, 23);
			dataLine3.EntryPort = "2506";
			dataLine3.IsChildLine = true;
			dataLine3.ReconTariff = "1111111112";
			dataLine3.OrgLineNumber = "50";
			dataLine3.MessageMode = JobApplicationCodeList.Codes.ACS;
			ReconDeclaration recon = new ReconDeclaration(Factory.New<JobDeclaration>());
			new ReconInvoiceLineFlatFileDataTransferProcessor().ImportReconDataFromCollection(collection, recon);
			AssertEquals(1, recon.OriginalEntries.Count);
			var entry = recon.OriginalEntries[0];
			AssertEquals("XJ51", entry.CH_OrigEntryReference);
			AssertEquals(new ZDateTime(2008, 10, 23), entry.US_R_ReleaseDate);
			AssertEquals(new ZDateTime(2008, 10, 25), entry.US_ImportDate);
			AssertEquals("2506", entry.US_SchDEntry);
			AssertEquals(JobApplicationCodeList.Codes.ACS, entry.US_R_MsgMode);
			AssertEquals(3, recon.InvoiceLines.Count);
			AssertEquals(1, recon.Invoices.Count);
			JobComInvoiceHeader invoice = recon.Invoices[0];
			AssertEquals(3, invoice.JobComInvoiceLines.Count);
			AssertEquals(invoice, entry.Invoice);
			JobComInvoiceLine line1 = recon.InvoiceLines[0];
			AssertEquals("0000000001", line1.JI_Tariff);
			AssertEquals(true, invoice.JobComInvoiceLines.Contains(line1));
			AssertEquals("100", line1.US_R_OrigEntryLineNo);
			JobComInvoiceLine line2 = recon.InvoiceLines[1];
			AssertEquals("1111111111", line2.JI_Tariff);
			AssertEquals(true, invoice.JobComInvoiceLines.Contains(line2));
			AssertEquals(line1.PK, line2.JI_ParentID);
			AssertEquals("1", line2.US_R_OrigEntryLineNo);
			JobComInvoiceLine line3 = recon.InvoiceLines[2];
			AssertEquals("1111111112", line3.JI_Tariff);
			AssertEquals(true, invoice.JobComInvoiceLines.Contains(line3));
			AssertEquals(line1.PK, line3.JI_ParentID);
			AssertEquals("50", line3.US_R_OrigEntryLineNo);
		}

		public void TestExportWhenThereAreManyChildren()
		{
			ReconDeclaration recon = new ReconDeclaration(Factory.New<JobDeclaration>());
			JobComInvoiceHeader invoice = recon.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.AddSecondaryInvoiceLine();
			invoiceLine1.AddSecondaryInvoiceLine();
			invoiceLine1.AddSecondaryInvoiceLine();
			invoiceLine1.US_R_OrigEntryLineNo = "599";
			JobComInvoiceLine invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.AddSecondaryInvoiceLine();
			invoiceLine4.AddSecondaryInvoiceLine();
			invoiceLine4.AddSecondaryInvoiceLine();
			invoiceLine4.US_R_OrigEntryLineNo = "1";
			ReconFlattenedDataLineCollection collection = new ReconFlattenedDataLineCollection(Factory);
			new ReconInvoiceLineFlatFileDataTransferProcessor().ExportReconDataToCollection(collection, recon);
			AssertEquals(8, collection.Count);
			AssertEquals((short)1, collection[0].LineNumber);
			AssertEquals((short)2, collection[1].LineNumber);
			AssertEquals((short)3, collection[2].LineNumber);
			AssertEquals((short)4, collection[3].LineNumber);
			AssertEquals((short)5, collection[4].LineNumber);
			AssertEquals((short)6, collection[5].LineNumber);
			AssertEquals((short)7, collection[6].LineNumber);
			AssertEquals((short)8, collection[7].LineNumber);
			AssertEquals("599", collection[0].OrgLineNumber);
			AssertEquals("1", collection[4].OrgLineNumber);
		}

		public void TestUpdateExistingJobFromDataFile()
		{
			DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("SV9");
			SetReconJobAndFlattenedDataLineCollection();
			new ReconInvoiceLineFlatFileDataTransferProcessor().ImportReconDataFromCollection(reconFlattenedDataLineCollection, reconDeclaration);
			AssertImportedReconDetails(reconDeclaration, "CN");
			reconFlattenedDataLineCollection[0].CountryOfOrigin = "AU"; //should be updated
			new ReconInvoiceLineFlatFileDataTransferProcessor().ImportReconDataFromCollection(reconFlattenedDataLineCollection, reconDeclaration);
			AssertImportedReconDetails(reconDeclaration, "AU");
		}

		[TestDate(2013, 01, 01)]
		public void TestExportImportTaxQty()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var reconOriginalEntryHeader = reconDec.OriginalEntries.AddNew();
			reconOriginalEntryHeader.US_R_DutyRateDate = ZDateTime.Today;
			var reconLine = reconOriginalEntryHeader.Invoice.JobComInvoiceLines.AddNew();
			reconLine.US_R_OrigTaxApply = TaxApplyList.Codes.Override;
			reconLine.US_R_OrigTaxCode = "016";
			reconLine.US_R_OrigTaxRateS = AppendixBTaxRateList.Codes.Wines_1;
			reconLine.US_R_OrigTaxQty = 100m;
			reconLine.US_TaxApply = TaxApplyList.Codes.Override;
			reconLine.US_TaxRateS = AppendixBTaxRateList.Codes.Wines_2;
			reconLine.US_TaxQty = 105m;
			reconFlattenedDataLineCollection = new ReconFlattenedDataLineCollection(Factory);
			new ReconInvoiceLineFlatFileDataTransferProcessor().ExportReconDataToCollection(reconFlattenedDataLineCollection, reconDec);
			AssertEquals(1, reconFlattenedDataLineCollection.Count);
			var reconDataLine = reconFlattenedDataLineCollection[0];
			AssertEquals("OriginalTaxQuantity", 100m, reconDataLine.OriginalTaxRateQuantity);
			AssertEquals("ReconTaxRateQuantity", 105m, reconDataLine.ReconTaxRateQuantity);
			reconDataLine.OriginalTaxRate = 99m;
			reconDataLine.ReconTaxRate = 104m;
			var reconDec2 = new ReconDeclaration(Factory.New<JobDeclaration>());
			new ReconInvoiceLineFlatFileDataTransferProcessor().ImportReconDataFromCollection(reconFlattenedDataLineCollection, reconDec2);
			AssertEquals(1, reconDec2.InvoiceLines.Count);
			var reconLine2 = reconDec2.InvoiceLines[0];
			AssertEquals("OriginalTaxQuantity", 100m, reconLine2.US_R_OrigTaxQty);
			AssertEquals("ReconTaxRateQuantity", 105m, reconLine2.US_TaxQty);
			AssertNotEquals("OriginalTaxRate", 99m, reconLine2.US_R_OrigTaxRate);
			AssertNotEquals("ReconTaxRate", 99m, reconLine2.US_TaxRate);
		}

		public void TestImportNewDataFromDataFiles()
		{
			reconFlattenedDataLineCollection = new ReconFlattenedDataLineCollection(Factory);
			var dataLine = reconFlattenedDataLineCollection.AddNew();
			dataLine.EntryNumber = "XJ51";
			dataLine.OwnerReferenceNumber = "8974578";
			dataLine.GoodsDescription = "45426 TELEPHONE PARTS";
			dataLine.LineNumber = 2;
			dataLine.ImportationDate = new ZDateTime(2008, 10, 25);
			dataLine.PaymentDate = new ZDateTime(2008, 10, 27);
			dataLine.EntryDate = new ZDateTime(2008, 10, 23);
			dataLine.EntryPort = "2506";
			dataLine.CountryOfOrigin = "CN";
			dataLine.OriginalTariff = "8517.11.0000";
			dataLine.ReconTariff = "8517.12.0000";
			dataLine.OriginalSPI = "A";
			dataLine.ReconSPI = "B";
			dataLine.OriginalCustomsValue = 12000m;
			dataLine.ReconCustomsValue = 13000m;
			dataLine.OriginalFirstQty = 250m;
			dataLine.OriginalFirstUQ = "KG";
			dataLine.ReconFirstQty = 260m;
			dataLine.OriginalMPF = 1.2m;
			dataLine.OriginalHMF = 5.4m;
			dataLine.OriginalTaxCode = "17";
			dataLine.OriginalTaxAmount = 7.1m;
			dataLine.OriginalOtherFeeCode = Core.Constants.USCustoms.FeeCodes.SoftwoodLumber;
			dataLine.OriginalOtherFee = 3.2m;
			dataLine.ReconMPF = 2.3m;
			dataLine.ReconHMF = 4.5m;
			dataLine.ReconOtherFeeCode = Core.Constants.USCustoms.FeeCodes.Coffee;
			dataLine.ReconOtherFee = 5.8m;
			dataLine.HTSChangedDueToValue = true;
			dataLine.IsTextile = true;
			dataLine.OriginalRateType = "A";
			dataLine.ReconRateType = "B";
			dataLine.ReconReason = "ABCDEFG";
			dataLine.OverrideOriginalMPF = true;
			dataLine.OverrideReconMPF = true;
			dataLine.OverrideOriginalHMF = true;
			dataLine.OverrideReconHMF = true;
			dataLine.OverrideOriginalOtherFeeAmount = true;
			dataLine.OverrideReconOtherFeeAmount = true;
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			new ReconInvoiceLineFlatFileDataTransferProcessor().ImportReconDataFromCollection(reconFlattenedDataLineCollection, reconDeclaration);
			AssertEquals("OriginalEntry", 1, reconDeclaration.OriginalEntries.Count);
			AssertEquals("invoices", 1, reconDeclaration.Invoices.Count);
			AssertEquals("invoice lines", 1, reconDeclaration.InvoiceLines.Count);
			var originalEntry = reconDeclaration.OriginalEntries[0];
			AssertEquals("XJ51", originalEntry.CH_OrigEntryReference);
			AssertEquals("8974578", originalEntry.US_R_OwnerRef);
			AssertEquals(new ZDateTime(2008, 10, 25), originalEntry.US_ImportDate);
			AssertEquals(new ZDateTime(2008, 10, 27), originalEntry.US_PaymentDate);
			AssertEquals(new ZDateTime(2008, 10, 23), originalEntry.US_R_ReleaseDate);
			AssertEquals("2506", originalEntry.US_SchDEntry);
			AssertEquals("Line has HMF amount", YesNoDefaultList.Codes.Yes, originalEntry.US_R_IsHMFApplicable);
			AssertEquals("XJ51", reconDeclaration.Invoices[0].JZ_InvoiceNumber);
			var invoiceLine = reconDeclaration.Invoices[0].JobComInvoiceLines[0];
			AssertEquals("Country of Origin", "CN", invoiceLine.US_UC_NKCountryOfOrigin);
			AssertEquals("Original tariff", "8517110000", invoiceLine.US_R_OrigTariff);
			AssertEquals("Recon tariff", "8517120000", invoiceLine.JI_Tariff);
			AssertEquals("Original SPI", "A", invoiceLine.US_R_OrigSPI);
			AssertEquals("Recon SPI", "B", invoiceLine.US_SPI);
			AssertEquals("Original CV", 12000m, invoiceLine.US_R_OrigCV);
			AssertEquals("Recon CV", 13000m, invoiceLine.JI_LinePrice);
			AssertEquals("First Qty", 250m, invoiceLine.US_R_OrigFirstQty);
			AssertEquals("First Qty", 260m, invoiceLine.JI_CustomsQuantity);
			AssertEquals("US_R_OrigMPFAmount", 1.2m, invoiceLine.US_R_OrigMPFAmount);
			AssertEquals("US_R_OrigHasMPF", true, invoiceLine.US_R_OrigHasMPF);
			AssertEquals("US_R_OrigHMFAmount", 5.4m, invoiceLine.US_R_OrigHMFAmount);
			AssertEquals("US_R_OrigOtherFeeCode", Core.Constants.USCustoms.FeeCodes.SoftwoodLumber, invoiceLine.US_R_OrigOtherFeeCode);
			AssertEquals("US_R_OrigOtherFeeAmount", 3.2m, invoiceLine.US_R_OrigOtherFeeAmount);
			AssertEquals("US_R_ReconOtherFeeCode", Core.Constants.USCustoms.FeeCodes.Coffee, invoiceLine.US_R_ReconOtherFeeCode);
			AssertEquals("US_R_ReconOtherFeeAmount", 5.8m, invoiceLine.US_R_ReconOtherFeeAmount);
			AssertEquals("US_R_OrigTaxCode", Core.Constants.USCustoms.FeeCodes.Wines, invoiceLine.US_R_OrigTaxCode);
			AssertEquals("US_R_OrigTaxAmount", 7.1m, invoiceLine.US_R_OrigTaxAmount);
			AssertEquals("US_R_ReconMPFAmount", 2.3m, invoiceLine.US_R_ReconMPFAmount);
			AssertEquals("US_R_ReconHMFAmount", 4.5m, invoiceLine.US_R_ReconHMFAmount);
			AssertEquals("US_R_HTSChanged4ValueInd", true, invoiceLine.US_R_HTSChanged4ValueInd);
			AssertEquals("US_R_Textile", true, invoiceLine.US_R_Textile);
			AssertEquals("US_R_OrigRateType", "A", invoiceLine.US_R_OrigRateType);
			AssertEquals("US_SelectedRateType", "B", invoiceLine.US_SelectedRateType);
			AssertEquals("US_R_ReconReasonText", "ABCDEFG", invoiceLine.US_R_ReconReasonText);
			AssertEquals("US_R_OverrideOriginMPF", true, invoiceLine.US_R_OverrideOriginMPF);
			AssertEquals("US_R_OverrideReconMPF", true, invoiceLine.US_R_OverrideReconMPF);
			AssertEquals("US_R_OverrideOriginHMF", true, invoiceLine.US_R_OverrideOriginHMF);
			AssertEquals("US_R_OverrideReconHMF", true, invoiceLine.US_R_OverrideReconHMF);
			AssertEquals("US_R_OverrideOrigOtherFeeAmount", true, invoiceLine.US_R_OverrideOrigOtherFeeAmount);
			AssertEquals("US_R_OverrideReconOtherFeeAmount", true, invoiceLine.US_R_OverrideReconOtherFeeAmount);
			dataLine.OriginalFormattedTariff = "8517.11.12345";
			dataLine.OriginalFormattedSupTariff = "8517.11.67890";
			dataLine.ReconFormattedTariff = "8517.12.12345";
			dataLine.ReconFormattedSupTariff = "8517.12.67890";
			reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			new ReconInvoiceLineFlatFileDataTransferProcessor().ImportReconDataFromCollection(reconFlattenedDataLineCollection, reconDeclaration);
			invoiceLine = reconDeclaration.Invoices[0].JobComInvoiceLines[0];
			AssertEquals("JI_Tariff", "8517121234", invoiceLine.JI_Tariff);
			AssertEquals("US_SupTariff", "8517126789", invoiceLine.US_SupTariff);
			AssertEquals("US_R_OrigTariff", "8517111234", invoiceLine.US_R_OrigTariff);
			AssertEquals("US_R_OrigSupTariff", "8517116789", invoiceLine.US_R_OrigSupTariff);
		}

		public void TestImportSupplementaryDetails()
		{
			reconFlattenedDataLineCollection = new ReconFlattenedDataLineCollection(Factory);
			var dataLine = reconFlattenedDataLineCollection.AddNew();
			dataLine.EntryNumber = "XJ51";
			dataLine.OriginalTariff = "8517.11.0000";
			dataLine.OriginalSupTariff = "98020050";
			dataLine.ReconTariff = "8517.12.0000";
			dataLine.ReconSupTariff = "98020051";
			dataLine.OriginalCustomsValue = 12000m;
			dataLine.ReconCustomsValue = 13000m;
			dataLine.Original98Value = 100m;
			dataLine.Recon98Value = 110m;
			dataLine.OriginalDuty = 45.61m;
			dataLine.OriginalSupDuty = 23.89m;
			dataLine.OriginalFirstQty = 250m;
			dataLine.OriginalFirstUQ = "KG";
			dataLine.OriginalSupFirstQty = 251m;
			dataLine.OriginalSupFirstUQ = "L";
			dataLine.ReconFirstQty = 260m;
			dataLine.ReconSupFirstQty = 261m;
			//Line2
			dataLine = reconFlattenedDataLineCollection.AddNew();
			dataLine.EntryNumber = "XJ51";
			dataLine.OriginalTariff = "8517.11.0000";
			dataLine.OriginalSupTariff = "98020050";
			dataLine.ReconTariff = "8517.12.0000";
			dataLine.ReconSupTariff = "98020051";
			dataLine.OriginalCustomsValue = 12000m;
			dataLine.ReconCustomsValue = 13000m;
			dataLine.Original98Value = 100m;
			dataLine.Recon98Value = 110m;
			dataLine.OriginalDuty = 40m;
			dataLine.OriginalSupDuty = 10m;
			dataLine.OriginalFirstQty = 250m;
			dataLine.OriginalFirstUQ = "KG";
			dataLine.OriginalSupFirstQty = 251m;
			dataLine.OriginalSupFirstUQ = "L";
			dataLine.ReconFirstQty = 260m;
			dataLine.ReconSupFirstQty = 261m;
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			new ReconInvoiceLineFlatFileDataTransferProcessor().ImportReconDataFromCollection(reconFlattenedDataLineCollection, reconDeclaration);
			AssertEquals("OriginalEntry", 1, reconDeclaration.OriginalEntries.Count);
			AssertEquals("invoices", 1, reconDeclaration.Invoices.Count);
			AssertEquals("invoice lines", 2, reconDeclaration.InvoiceLines.Count);
			var line = reconDeclaration.InvoiceLines[0];
			AssertEquals("98020051", line.US_SupTariff);
			AssertEquals("98020050", line.US_R_OrigSupTariff);
			AssertEquals(100m, line.US_R_Orig98Value);
			AssertEquals(110m, line.US_98GoodsValue);
			AssertEquals(261m, line.US_SupQty1);
			AssertEquals(251m, line.US_R_OrigSupQty1);
			AssertEquals(45.61m, line.US_R_OrigDuty);
			AssertEquals(23.89m, line.US_R_OrigSupDuty);
			var origEntry = reconDeclaration.OriginalEntries[0];
			AssertEquals(0m, origEntry.OriginalCharges.GetAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount));
			new ReconDutyFeeCalculationManager(reconDeclaration).CalculateAll();
			AssertEquals(119.50m, origEntry.OriginalCharges.GetAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount));
		}

		public void TestProgressChangedForImport()
		{
			reconFlattenedDataLineCollection = new ReconFlattenedDataLineCollection(Factory);
			ReconFlattenedDataLine dataLine = reconFlattenedDataLineCollection.AddNew();
			dataLine.EntryNumber = "XJ51";
			dataLine.LineNumber = 1;
			dataLine.OriginalTariff = "8517.11.0000";
			dataLine.ReconTariff = "8517.12.0000";
			reconFlattenedDataLineCollection.AddNew();
			dataLine.EntryNumber = "XJ51";
			dataLine.LineNumber = 2;
			dataLine.OriginalTariff = "8517.11.0000";
			dataLine.ReconTariff = "8517.12.0000";
			reconFlattenedDataLineCollection.AddNew();
			dataLine.EntryNumber = "XJ51";
			dataLine.LineNumber = 3;
			dataLine.OriginalTariff = "8517.11.0000";
			dataLine.ReconTariff = "8517.12.0000";
			int progressCount = 0;
			ReconInvoiceLineFlatFileDataTransferProcessor importer = new ReconInvoiceLineFlatFileDataTransferProcessor();
			importer.ProgressChanged += new ReconFlatFileDataTransferProcessor.ProgressChangedEventHandler(delegate
			{
				progressCount++;
				return progressCount != 2; //stop at 2 
			});
			reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			importer.ImportReconDataFromCollection(reconFlattenedDataLineCollection, reconDeclaration);
			AssertEquals("Progress should have been reported up to two", 2, progressCount);
			AssertEquals("Only the first element has been processed", 1, reconDeclaration.InvoiceLines.Count);
		}

		[TestDate(2009, 6, 1)]
		public void TestExportReconOriginalData()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var reconDeclaration = GetReconDeclaration();
			reconDeclaration.OriginalEntries[0].US_R_CalcOrigDuty = true;
			reconDeclaration.OriginalEntries[0].US_R_MsgMode = JobApplicationCodeList.Codes.ACS;
			reconDeclaration.CalculateDutyFeesForAllEntries();
			reconDeclaration.InvoiceLines[0].US_R_OrigDuty = 20m;
			reconDeclaration.InvoiceLines[0].US_Duty = 25m;
			reconDeclaration.InvoiceLines[0].US_R_OrigTaxAmount = 78.1m;
			reconDeclaration.InvoiceLines[0].US_R_ReconMPFAmount = 10m;
			reconDeclaration.InvoiceLines[0].US_R_ReconHMFAmount = 2.5m;
			reconDeclaration.InvoiceLines[0].US_R_OrigOtherFeeCode = Core.Constants.USCustoms.FeeCodes.Sugar;
			reconDeclaration.InvoiceLines[0].US_R_OrigOtherFeeAmount = 2.2m;
			reconDeclaration.InvoiceLines[0].US_R_ReconOtherFeeCode = Core.Constants.USCustoms.FeeCodes.Coffee;
			reconDeclaration.InvoiceLines[0].US_R_ReconOtherFeeAmount = 8.7m;
			reconDeclaration.InvoiceLines[0].US_R_HTSChanged4ValueInd = true;
			reconDeclaration.InvoiceLines[0].US_R_Textile = true;
			reconDeclaration.InvoiceLines[0].US_R_OrigRateType = "A";
			reconDeclaration.InvoiceLines[0].US_SelectedRateType = "B";
			reconDeclaration.InvoiceLines[0].US_R_ReconReasonText = "ABCDEFG";
			reconDeclaration.InvoiceLines[0].US_R_OverrideOriginMPF = true;
			reconDeclaration.InvoiceLines[0].US_R_OverrideReconMPF = true;
			reconDeclaration.InvoiceLines[0].US_R_OverrideOriginHMF = true;
			reconDeclaration.InvoiceLines[0].US_R_OverrideReconHMF = true;
			reconDeclaration.InvoiceLines[0].US_R_OverrideOrigOtherFeeAmount = true;
			reconDeclaration.InvoiceLines[0].US_R_OverrideReconOtherFeeAmount = true;
			var coll = new ReconFlattenedDataLineCollection(Factory);
			var processor = new ReconInvoiceLineFlatFileDataTransferProcessor();
			processor.ExportReconDataToCollection(coll, reconDeclaration);
			AssertEquals("1 data line is exported", 1, coll.Count);
			var dataLine = coll[0];
			AssertEquals("EntryNumber", "XJ5123456", dataLine.EntryNumber);
			AssertEquals("Owner Ref", "342623#", dataLine.OwnerReferenceNumber);
			AssertEquals("MessageMode", JobApplicationCodeList.Codes.ACS, dataLine.MessageMode);
			AssertEquals("Line number", (short)1, dataLine.LineNumber);
			AssertEquals("CountryOfOrigin", "AU", dataLine.CountryOfOrigin);
			AssertEquals("OriginalTariff", "1111111110", dataLine.OriginalTariff);
			AssertEquals("OriginalSupTariff", "5678125678", dataLine.OriginalSupTariff);
			AssertEquals("ReconTariff", "1111111111", dataLine.ReconTariff);
			AssertEquals("ReconSupTariff", "1212221212", dataLine.ReconSupTariff);
			AssertEquals("OriginalFormattedTariff", "1111.11.1110", dataLine.OriginalFormattedTariff);
			AssertEquals("OriginalFormattedSupTariff", "5678.12.5678", dataLine.OriginalFormattedSupTariff);
			AssertEquals("ReconFormattedTariff", "1111.11.1111", dataLine.ReconFormattedTariff);
			AssertEquals("ReconFormattedSupTariff", "1212.22.1212", dataLine.ReconFormattedSupTariff);
			AssertEquals("Goods Desc", "WHO IS THIS", dataLine.GoodsDescription);
			AssertEquals("Original FirstUQ", "KG", dataLine.OriginalFirstUQ);
			AssertEquals("Original SecondUQ", "M3", dataLine.OriginalSecondUQ);
			AssertEquals("Original ThirdUQ", "NO", dataLine.OriginalThirdUQ);
			AssertEquals("OriginalFirstQty", 30m, dataLine.OriginalFirstQty);
			AssertEquals("OriginalSecondQty", 40m, dataLine.OriginalSecondQty);
			AssertEquals("OriginalThirdQty", 50m, dataLine.OriginalThirdQty);
			AssertEquals("OriginalCV", 35000m, dataLine.OriginalCustomsValue);
			AssertEquals("OriginalDuty", 20m, dataLine.OriginalDuty);
			AssertEquals("Original MPF", 73.50m, dataLine.OriginalMPF);
			AssertEquals("Original HMF", 43.75m, dataLine.OriginalHMF);
			AssertEquals("Original SPI", "A", dataLine.OriginalSPI);
			AssertEquals("Recon SPI", "B", dataLine.ReconSPI);
			AssertEquals("Recon Secondary SPI", SecondarySpecProgIndicatorList.Codes.M, dataLine.ReconSecondarySPI);
			AssertEquals("Goods Description", "WHO IS THIS", dataLine.GoodsDescription);
			AssertEquals("Original Tax Rate Override", TaxApplyList.Codes.Override, dataLine.OriginalTaxApply);
			AssertEquals("Original Tax Code", Core.Constants.USCustoms.FeeCodes.DistilledSpirits, dataLine.OriginalTaxCode);
			AssertEquals("Original Tax Amount", 78.1m, dataLine.OriginalTaxAmount);
			AssertEquals("Original Tax Rate", AppendixBTaxRateList.Codes.Specify, dataLine.OriginalTaxRateS);
			AssertEquals("Original Tax Rate Value", 323.23m, dataLine.OriginalTaxRate);
			AssertEquals("Original Tax Rate Qty", 121.2m, dataLine.OriginalTaxRateQuantity);
			AssertEquals("Original Duty Override", ZBool.True, dataLine.OriginalDutyOverride);
			AssertEquals("Recon Duty Override", ZBool.True, dataLine.ReconDutyOverride);
			AssertEquals("Recon Duty", 25m, dataLine.ReconDuty);
			AssertEquals("Recon Tax Rate Override", TaxApplyList.Codes.Override, dataLine.ReconTaxApply);
			AssertEquals("Recon Tax Code", Core.Constants.USCustoms.FeeCodes.Wines, dataLine.ReconTaxCode);
			AssertEquals("Recon Tax Rate", AppendixBTaxRateList.Codes.Tobacco_7, dataLine.ReconTaxRateS);
			AssertEquals("Recon Tax Rate Value", 983.43m, dataLine.ReconTaxRate);
			AssertEquals("Recon Tax Rate Qty", 85.43m, dataLine.ReconTaxRateQuantity);
			AssertEquals("Product Number", "ZZ#23ZSD", dataLine.ProductNumber);
			AssertEquals("Lookup", "$#SDFSD3", dataLine.Lookup);
			AssertEquals("ReconMPF", 10m, dataLine.ReconMPF);
			AssertEquals("ReconHMF", 2.5m, dataLine.ReconHMF);
			AssertEquals("ReconMPF", Core.Constants.USCustoms.FeeCodes.Coffee, dataLine.ReconOtherFeeCode);
			AssertEquals("ReconHMF", 8.7m, dataLine.ReconOtherFee);
			AssertEquals("HTSChangedDueToValue", true, dataLine.HTSChangedDueToValue);
			AssertEquals("IsTextile", true, dataLine.IsTextile);
			AssertEquals("OriginalRateType", "A", dataLine.OriginalRateType);
			AssertEquals("ReconRateType", "B", dataLine.ReconRateType);
			AssertEquals("ReconReason", "ABCDEFG", dataLine.ReconReason);
			AssertEquals("OverrideOriginalMPF", true, dataLine.OverrideOriginalMPF);
			AssertEquals("OverrideReconMPF", true, dataLine.OverrideReconMPF);
			AssertEquals("OverrideOriginalHMF", true, dataLine.OverrideOriginalHMF);
			AssertEquals("OverrideReconHMF", true, dataLine.OverrideReconHMF);
			AssertEquals("OverrideOriginalOtherFeeAmount", true, dataLine.OverrideOriginalOtherFeeAmount);
			AssertEquals("OverrideReconOtherFeeAmount", true, dataLine.OverrideReconOtherFeeAmount);
			AssertEquals("Original Other Fee", 2.2m, dataLine.OriginalOtherFee);
			AssertEquals("Original Other Fee Code", Core.Constants.USCustoms.FeeCodes.Sugar, dataLine.OriginalOtherFeeCode);
		}

		public void TestExport98_99InvoiceLines()
		{
			ReconDeclaration reconDeclaration = GetReconDeclaration();
			reconDeclaration.OriginalEntries[0].US_R_DutyRateDate = new ZDateTime(2008, 12, 31);
			reconDeclaration.InvoiceLines[0].US_SupTariff = "99010050";
			reconDeclaration.InvoiceLines[0].US_SupQty1 = 5000m;
			reconDeclaration.CalculateDutyFeesForAllEntries();
			ReconFlattenedDataLineCollection coll = new ReconFlattenedDataLineCollection(Factory);
			ReconInvoiceLineFlatFileDataTransferProcessor processor = new ReconInvoiceLineFlatFileDataTransferProcessor();
			processor.ExportReconDataToCollection(coll, reconDeclaration);
			AssertEquals("1 data line is exported", 1, coll.Count);
			ReconFlattenedDataLine dataLine = coll[0];
			AssertEquals("99010050", dataLine.ReconSupTariff);
			AssertEquals(5000m, dataLine.ReconSupFirstQty);
		}

		public void TestProgressChangedForExport()
		{
			ReconDeclaration reconDeclaration = GetReconDeclaration();
			int progressCount = 0;
			ReconInvoiceLineFlatFileDataTransferProcessor importer = new ReconInvoiceLineFlatFileDataTransferProcessor();
			importer.ProgressChanged += new ReconFlatFileDataTransferProcessor.ProgressChangedEventHandler(delegate
			{
				progressCount++;
				return true;
			});
			ReconFlattenedDataLineCollection coll = new ReconFlattenedDataLineCollection(Factory);
			importer.ExportReconDataToCollection(coll, reconDeclaration);
			AssertEquals("Progress is notified", 1, progressCount);
			AssertEquals("Only the first element has been processed", 1, reconDeclaration.InvoiceLines.Count);
		}

		public void TestAggregateOriginalFeesForOriginalEntry()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			reconFlattenedDataLineCollection = new ReconFlattenedDataLineCollection(Factory);
			//Entry 1, line 1
			var dataLine = reconFlattenedDataLineCollection.AddNew();
			dataLine.EntryNumber = "XJ51";
			dataLine.OwnerReferenceNumber = "8974578";
			dataLine.GoodsDescription = "45426 TELEPHONE PARTS";
			dataLine.LineNumber = 2;
			dataLine.ImportationDate = new ZDateTime(2008, 10, 25);
			dataLine.PaymentDate = new ZDateTime(2008, 10, 27);
			dataLine.EntryDate = new ZDateTime(2008, 10, 23);
			dataLine.EntryPort = "2506";
			dataLine.CountryOfOrigin = "CN";
			dataLine.OriginalTariff = "8517.11.0000";
			dataLine.ReconTariff = "8517.12.0000";
			dataLine.OriginalMPF = 1.2m;
			dataLine.OriginalHMF = 5.4m;
			dataLine.OriginalTaxCode = Core.Constants.USCustoms.FeeCodes.Tobacco;
			dataLine.OriginalTaxAmount = 5.2m;
			dataLine.OriginalOtherFeeCode = Core.Constants.USCustoms.FeeCodes.Blueberry;
			dataLine.OriginalOtherFee = 2.5m;
			//Entry 1, line 2
			dataLine = reconFlattenedDataLineCollection.AddNew();
			dataLine.EntryNumber = "XJ51";
			dataLine.OwnerReferenceNumber = "8974578";
			dataLine.LineNumber = 3;
			dataLine.ImportationDate = new ZDateTime(2008, 10, 25);
			dataLine.PaymentDate = new ZDateTime(2008, 10, 27);
			dataLine.EntryDate = new ZDateTime(2008, 10, 23);
			dataLine.EntryPort = "2506";
			dataLine.CountryOfOrigin = "CN";
			dataLine.OriginalTariff = "3417.11.0000";
			dataLine.ReconTariff = "3417.12.0000";
			dataLine.OriginalMPF = 10m;
			dataLine.OriginalHMF = 9.1m;
			dataLine.OriginalTaxCode = Core.Constants.USCustoms.FeeCodes.Tobacco;
			dataLine.OriginalTaxAmount = 1.1m;
			dataLine.OriginalOtherFeeCode = Core.Constants.USCustoms.FeeCodes.Blueberry;
			dataLine.OriginalOtherFee = 4.8m;
			//Entry 2, line 1
			dataLine = reconFlattenedDataLineCollection.AddNew();
			dataLine.EntryNumber = "XJ52";
			dataLine.OwnerReferenceNumber = "8974580";
			dataLine.GoodsDescription = "45426 TELEPHONE PARTS";
			dataLine.LineNumber = 1;
			dataLine.ImportationDate = new ZDateTime(2008, 10, 25);
			dataLine.PaymentDate = new ZDateTime(2008, 10, 27);
			dataLine.EntryDate = new ZDateTime(2008, 10, 23);
			dataLine.EntryPort = "2506";
			dataLine.CountryOfOrigin = "CN";
			dataLine.OriginalTariff = "8517.11.0000";
			dataLine.ReconTariff = "8517.12.0000";
			dataLine.OriginalCustomsValue = 12000m;
			dataLine.ReconCustomsValue = 13000m;
			dataLine.OriginalFirstQty = 250m;
			dataLine.OriginalFirstUQ = "KG";
			dataLine.ReconFirstQty = 260m;
			dataLine.OriginalMPF = 1.2m;
			dataLine.OriginalHMF = 5.4m;
			dataLine.OriginalTaxCode = Core.Constants.USCustoms.FeeCodes.Tobacco;
			dataLine.OriginalTaxAmount = 5.2m;
			dataLine.OriginalOtherFeeCode = Core.Constants.USCustoms.FeeCodes.Blueberry;
			dataLine.OriginalOtherFee = 2.5m;
			//Entry 2, line 2
			dataLine = reconFlattenedDataLineCollection.AddNew();
			dataLine.EntryNumber = "XJ52";
			dataLine.OwnerReferenceNumber = "8974580";
			dataLine.LineNumber = 2;
			dataLine.ImportationDate = new ZDateTime(2008, 10, 25);
			dataLine.PaymentDate = new ZDateTime(2008, 10, 27);
			dataLine.EntryDate = new ZDateTime(2008, 10, 23);
			dataLine.EntryPort = "2506";
			dataLine.CountryOfOrigin = "CN";
			dataLine.OriginalTariff = "3417.11.0000";
			dataLine.ReconTariff = "3417.12.0000";
			dataLine.OriginalCustomsValue = 12480m;
			dataLine.ReconCustomsValue = 13090m;
			dataLine.OriginalFirstQty = 246m;
			dataLine.OriginalFirstUQ = "KG";
			dataLine.ReconFirstQty = 260m;
			dataLine.OriginalMPF = 10m;
			dataLine.OriginalHMF = 9.1m;
			dataLine.OriginalTaxCode = Core.Constants.USCustoms.FeeCodes.Tobacco;
			dataLine.OriginalTaxAmount = 1.1m;
			dataLine.OriginalOtherFeeCode = Core.Constants.USCustoms.FeeCodes.Blueberry;
			dataLine.OriginalOtherFee = 4.8m;
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var origEntry2 = reconDeclaration.OriginalEntries.AddNew();
			origEntry2.CH_OrigEntryReference = "XJ52";
			origEntry2.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			origEntry2.US_R_CalcOrigDuty = true;
			new ReconInvoiceLineFlatFileDataTransferProcessor().ImportReconDataFromCollection(reconFlattenedDataLineCollection, reconDeclaration);
			AssertEquals("OriginalEntry", 2, reconDeclaration.OriginalEntries.Count);
			AssertEquals("invoice lines", 4, reconDeclaration.InvoiceLines.Count);
			reconDeclaration.CalculateDutyFeesForAllEntries();
			var originalEntry1 = reconDeclaration.OriginalEntries.FindEntryBy("XJ51");
			AssertEquals("Line has HMF amount", YesNoDefaultList.Codes.Yes, originalEntry1.US_R_IsHMFApplicable);
			AssertEquals("Original HMF amount", 14.5m, originalEntry1.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.HMF));
			AssertEquals("Original MPF amount", 25m, originalEntry1.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals("Line US_R_OrigHasMPF", true, originalEntry1.Invoice.InvoiceLines[0].US_R_OrigHasMPF);
			AssertEquals("Original Tobacco amount", 6.3m, originalEntry1.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Tobacco));
			AssertEquals("Original Blueberry amount", 7.3m, originalEntry1.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Blueberry));
			var originalEntry2 = reconDeclaration.OriginalEntries.FindEntryBy("XJ52");
			AssertEquals("Line has HMF amount", YesNoDefaultList.Codes.Yes, originalEntry2.US_R_IsHMFApplicable);
			AssertEquals("Original HMF amount", 30.6m, originalEntry2.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.HMF));
		}

		USCTariffRule AddSecondaryTariffRule(ZString parentTariffNumber, params ZString[] secondaryTariffNumbers)
		{
			USCTariff tariff = CreateUSCTariffRecord(parentTariffNumber);
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_RuleCode = TariffRuleList.Codes.EligibleForSecondaryTariffNumbers;
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			tariffRule.U1_Tariff = parentTariffNumber;
			USCRuleSecondaryTariff secondaryTariff = tariffRule.SecondaryTariffs.AddNew();
			secondaryTariff.U3_DateFrom = ZDateTime.BrettsBirthday;
			if (secondaryTariffNumbers.Length > 0)
			{
				secondaryTariff.U3_TariffFrom = secondaryTariffNumbers[0];
				CreateUSCTariffRecord(secondaryTariffNumbers[0]);
			}

			if (secondaryTariffNumbers.Length > 1)
			{
				secondaryTariff.U3_Tariff2 = secondaryTariffNumbers[1];
				CreateUSCTariffRecord(secondaryTariffNumbers[1]);
			}

			if (secondaryTariffNumbers.Length > 2)
			{
				secondaryTariff.U3_Tariff3 = secondaryTariffNumbers[2];
				CreateUSCTariffRecord(secondaryTariffNumbers[2]);
			}

			return tariffRule;
		}

		USCTariff CreateUSCTariffRecord(ZString tariffNumber)
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = tariffNumber;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			return tariff;
		}

		ReconDeclaration GetReconDeclaration()
		{
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "ZZ#23ZSD";
			CusClassification classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "$#SDFSD3";
			classification.CC_TariffNum = "1111111111";
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_IssueCode = ReconIssueCodeList.Codes.ValueRecon;
			ReconOriginalEntryHeader entry = reconDeclaration.OriginalEntries.AddNew();
			entry.CH_OrigEntryReference = "XJ5123456";
			entry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			entry.US_R_DutyRateDate = ZDateTime.Today;
			entry.US_R_DateForMPFCalc = ZDateTime.Today;
			entry.US_R_OwnerRef = "342623#";
			entry.US_R_GoodsDescription = "Boys' suits";
			JobComInvoiceHeader invoice = reconDeclaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV123456";
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_CH_ReconEntry = entry.CH_PK;
			JobComInvoiceLine invoiceLine = reconDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.JI_InvoiceQuantity = 10m;
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_R_OrigSupTariff = "5678.12.5678";
			invoiceLine.US_R_OrigTariff = "1111.11.1110";
			invoiceLine.US_SupTariff = "1212.22.1212";
			invoiceLine.JI_CC = classification.PK;
			invoiceLine.US_R_OrigFirstUQ = "KG";
			invoiceLine.US_R_OrigFirstQty = 30m;
			invoiceLine.US_R_OrigSecondUQ = "M3";
			invoiceLine.US_R_OrigSecondQty = 40m;
			invoiceLine.US_R_OrigThirdUQ = "NO";
			invoiceLine.US_R_OrigThirdQty = 50m;
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = 35m;
			invoiceLine.JI_CustomsSecondUnitQty = "M3";
			invoiceLine.JI_CustomsSecondQuantity = 45m;
			invoiceLine.JI_CustomsThirdUnitQty = "NO";
			invoiceLine.JI_CustomsThirdQuantity = 55m;
			invoiceLine.JI_LinePrice = 30000m;
			invoiceLine.US_R_OrigCV = 35000m;
			invoiceLine.US_R_OrigSPI = "A";
			invoiceLine.US_SPI = "B";
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.M;
			invoiceLine.JI_Description = "WHO IS THIS";
			invoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_R_OrigTaxCode = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
			invoiceLine.US_R_OrigTaxRateS = AppendixBTaxRateList.Codes.Specify;
			invoiceLine.US_R_OrigTaxRate = 323.23m;
			invoiceLine.US_R_OrigTaxQty = 121.2m;
			invoiceLine.US_R_OrigOverrideDuty = ZBool.True;
			invoiceLine.US_R_OrigDuty = 532.54m;
			invoiceLine.US_OverrideDuty = ZBool.True;
			invoiceLine.US_Duty = 9823.50m;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.Wines;
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Tobacco_7;
			invoiceLine.US_TaxRate = 983.43m;
			invoiceLine.US_TaxQty = 85.43m;
			return reconDeclaration;
		}

		void AssertImportedReconDetails(ReconDeclaration reconDeclaration, ZString countryOfOrigin)
		{
			AssertEquals("OriginalEntry", 2, reconDeclaration.OriginalEntries.Count);
			AssertEquals("invoices", 2, reconDeclaration.Invoices.Count);
			AssertEquals("invoice lines", 4, reconDeclaration.InvoiceLines.Count);
			ReconOriginalEntryHeader oldOriginalEntry = reconDeclaration.OriginalEntries[0];
			ReconOriginalEntryHeader originalEntry = reconDeclaration.OriginalEntries[1];
			if (oldOriginalEntry.CH_OrigEntryReference == "XJ51")
			{
				oldOriginalEntry = reconDeclaration.OriginalEntries[1];
				originalEntry = reconDeclaration.OriginalEntries[0];
			}

			AssertEquals(true, oldOriginalEntry.IsInDatabase);
			AssertEquals("Entry Number", "XXX31025", oldOriginalEntry.CH_OrigEntryReference);
			AssertEquals("Owner Ref #", "8974578", oldOriginalEntry.US_R_OwnerRef);
			AssertEquals("Importation date is set", new ZDateTime(2008, 10, 25), oldOriginalEntry.US_ImportDate);
			AssertEquals("Payment date is set", new ZDateTime(2008, 10, 27), oldOriginalEntry.US_PaymentDate);
			AssertEquals("Entry date is set", new ZDateTime(2008, 10, 23), oldOriginalEntry.US_R_ReleaseDate);
			AssertEquals("Processing port", "2506", oldOriginalEntry.US_SchDEntry);
			JobComInvoiceHeader invoice1 = oldOriginalEntry.Invoice;
			AssertEquals(false, invoice1.IsInDatabase);
			AssertEquals("First Invoice Number", "XXX31025", invoice1.JZ_InvoiceNumber);
			AssertEquals("First Invoice Currency", Core.Constants.CurrencyCodes.UnitedStates, invoice1.JZ_RX_NKInvoice_Currency);
			AssertEquals("First invoice has one invoice line", 1, invoice1.JobComInvoiceLines.Count);
			JobComInvoiceLine invoiceLine = invoice1.JobComInvoiceLines[0];
			AssertEquals(false, invoiceLine.IsInDatabase);
			AssertEquals("Country of Origin", "CN", invoiceLine.US_UC_NKCountryOfOrigin);
			AssertEquals("Original tariff", "8517110000", invoiceLine.US_R_OrigTariff);
			AssertEquals("Recon tariff", "8517120000", invoiceLine.JI_Tariff);
			AssertEquals("Original SPI", "A", invoiceLine.US_R_OrigSPI);
			AssertEquals("Recon SPI", "A", invoiceLine.US_SPI);
			AssertEquals("Original CV", 12000m, invoiceLine.US_R_OrigCV);
			AssertEquals("Recon CV", 13000m, invoiceLine.JI_LinePrice);
			AssertEquals("Cotton Fee Exempt", "", invoiceLine.US_CottonFeeExempt);
			AssertEquals("Cotton Fee Exempt", "", invoiceLine.US_R_OrigCottonFeeExempt);
			AssertEquals("First Qty", 250m, invoiceLine.US_R_OrigFirstQty);
			AssertEquals("First Qty", 260m, invoiceLine.JI_CustomsQuantity);
			AssertEquals("Part No", "ZZ3234@#", invoiceLine.JI_PartNo);
			AssertEquals("Lookup", classification.PK, invoiceLine.JI_CC);
			AssertEquals("Goods Description", "45426 TELEPHONE PARTS", invoiceLine.JI_Description);
			AssertEquals("Secondary SPI", SecondarySpecProgIndicatorList.Codes.M, invoiceLine.US_SecondarySPI);
			AssertEquals("Orig. Duty Override", ZBool.True, invoiceLine.US_R_OrigOverrideDuty);
			AssertEquals("Orig. Duty", 423.23m, invoiceLine.US_R_OrigDuty);
			AssertEquals("Orig. Tax Rate Override", TaxApplyList.Codes.Override, invoiceLine.US_R_OrigTaxApply);
			AssertEquals("Orig. Tax Rate", AppendixBTaxRateList.Codes.Specify, invoiceLine.US_R_OrigTaxRateS);
			AssertEquals("Orig. Tax Rate Value", 984.97m, invoiceLine.US_R_OrigTaxRate);
			AssertEquals("Orig. Tax Rate Qty", 0m, invoiceLine.US_R_OrigTaxQty);
			AssertEquals("Recon Duty Override", ZBool.True, invoiceLine.US_OverrideDuty);
			AssertEquals("Recon Duty", 928.72m, invoiceLine.US_Duty);
			AssertEquals("Recon Tax Rate Override", TaxApplyList.Codes.Override, invoiceLine.US_TaxApply);
			AssertEquals("Recon Tax Rate", AppendixBTaxRateList.Codes.Specify, invoiceLine.US_TaxRateS);
			AssertEquals("Recon Tax Rate Value", 233.98m, invoiceLine.US_TaxRate);
			AssertEquals("Recon Tax Rate Qty", 0m, invoiceLine.US_TaxQty);
			AssertEquals("Parent ID", ZGuid.Empty, invoiceLine.JI_ParentID);
			AssertEquals("Line No", (short)1, invoiceLine.JI_LineNo);
			AssertEquals(false, originalEntry.IsInDatabase);
			AssertEquals("Entry Number", "XJ51", originalEntry.CH_OrigEntryReference);
			AssertEquals("Owner Ref #", "8974578", originalEntry.US_R_OwnerRef);
			AssertEquals("Importation date is set", new ZDateTime(2008, 10, 25), originalEntry.US_ImportDate);
			AssertEquals("Payment date is set", new ZDateTime(2008, 10, 27), originalEntry.US_PaymentDate);
			AssertEquals("Entry date is set", new ZDateTime(2008, 10, 23), originalEntry.US_R_ReleaseDate);
			AssertEquals("Processing port", "2506", originalEntry.US_SchDEntry);
			JobComInvoiceHeader invoice2 = originalEntry.Invoice;
			AssertEquals(false, invoice2.IsInDatabase);
			AssertEquals("Invoice Number", "XJ51", invoice2.JZ_InvoiceNumber);
			AssertEquals("Invoice Currency", Core.Constants.CurrencyCodes.UnitedStates, invoice2.JZ_RX_NKInvoice_Currency);
			AssertEquals("invoice has one invoice line", 3, invoice2.JobComInvoiceLines.Count);
			invoice2.JobComInvoiceLines.Sort(JobComInvoiceLine.Schema.JI_LineNo);
			JobComInvoiceLine invoiceLine2 = invoice2.JobComInvoiceLines[0];
			AssertEquals(false, invoiceLine2.IsInDatabase);
			AssertEquals("Line No", (short)1, invoiceLine2.JI_LineNo);
			AssertEquals("Cotton Fee Exempt", YesNoDefaultList.Codes.Yes, invoiceLine2.US_CottonFeeExempt);
			AssertEquals("Cotton Fee Exempt", YesNoDefaultList.Codes.Yes, invoiceLine2.US_R_OrigCottonFeeExempt);
			JobComInvoiceLine invoiceLine3 = invoice2.JobComInvoiceLines[1];
			AssertEquals(false, invoiceLine3.IsInDatabase);
			AssertEquals("Line No", (short)2, invoiceLine3.JI_LineNo);
			AssertEquals("Country of Origin", countryOfOrigin, invoiceLine3.US_UC_NKCountryOfOrigin);
			AssertEquals("Original tariff", "8517110000", invoiceLine3.US_R_OrigTariff);
			AssertEquals("Recon tariff", "8517120000", invoiceLine3.JI_Tariff);
			AssertEquals("Original SPI", "A", invoiceLine3.US_R_OrigSPI);
			AssertEquals("Recon SPI", "B", invoiceLine3.US_SPI);
			AssertEquals("Original CV", 12000m, invoiceLine3.US_R_OrigCV);
			AssertEquals("Recon CV", 13000m, invoiceLine3.JI_LinePrice);
			AssertEquals("Cotton Fee Exempt", YesNoDefaultList.Codes.No, invoiceLine3.US_CottonFeeExempt);
			AssertEquals("Cotton Fee Exempt", YesNoDefaultList.Codes.No, invoiceLine3.US_R_OrigCottonFeeExempt);
			AssertEquals("First Qty", 250m, invoiceLine3.US_R_OrigFirstQty);
			AssertEquals("First Qty", 260m, invoiceLine3.JI_CustomsQuantity);
			AssertEquals("Parent ID", ZGuid.Empty, invoiceLine3.JI_ParentID);
			JobComInvoiceLine invoiceLine4 = invoice2.JobComInvoiceLines[2];
			AssertEquals(false, invoiceLine4.IsInDatabase);
			AssertEquals("Line No", (short)3, invoiceLine4.JI_LineNo);
			AssertEquals("Cotton Fee Exempt", "", invoiceLine4.US_CottonFeeExempt);
			AssertEquals("Cotton Fee Exempt", "", invoiceLine4.US_R_OrigCottonFeeExempt);
			AssertEquals("Parent ID", ZGuid.Empty, invoiceLine2.JI_ParentID);
			AssertEquals("Parent ID", ZGuid.Empty, invoiceLine3.JI_ParentID);
			AssertEquals("Parent ID", invoiceLine2.PK, invoiceLine4.JI_ParentID);
		}

		void SetReconJobAndFlattenedDataLineCollection()
		{
			reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader reconOriginalEntryHeader = reconDeclaration.OriginalEntries.AddNew();
			reconOriginalEntryHeader.CH_OrigEntryReference = "XXX31025";
			reconOriginalEntryHeader.US_R_ReleaseDate = new ZDateTime(2008, 9, 13);
			reconOriginalEntryHeader.US_ImportDate = new ZDateTime(2008, 9, 11);
			reconOriginalEntryHeader.US_PaymentDate = new ZDateTime(2008, 9, 15);
			reconOriginalEntryHeader.US_SchDEntry = "2704";
			reconOriginalEntryHeader.US_R_OwnerRef = "OWNERREF";
			JobComInvoiceHeader invoice = reconOriginalEntryHeader.Invoice;
			JobComInvoiceLine invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "8517.12.0000";
			invoiceLine1.US_R_OrigTariff = "8517.11.0000";
			invoiceLine1.US_SPI = "B";
			invoiceLine1.US_R_OrigSPI = "A";
			invoiceLine1.US_R_OrigCottonFeeExempt = YesNoDefaultList.Codes.No;
			invoiceLine1.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
			invoiceLine1.US_R_OrigCV = 12000m;
			invoiceLine1.JI_LinePrice = 13000m;
			invoiceLine1.US_R_OrigFirstQty = 250m;
			invoiceLine1.US_R_OrigFirstUQ = "KG";
			invoiceLine1.JI_CustomsQuantity = 260m;
			JobComInvoiceLine invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine2.US_R_OrigTariff = "8517.11.0000";
			invoiceLine2.US_R_OrigSPI = "A";
			invoiceLine2.US_R_OrigCV = 12000m;
			invoiceLine2.JI_LinePrice = 13000m;
			invoiceLine2.US_R_OrigFirstQty = 250m;
			invoiceLine2.US_R_OrigFirstUQ = "KG";
			invoiceLine2.JI_CustomsQuantity = 260m;
			invoiceLine1.US_R_OrigCottonFeeExempt = YesNoDefaultList.Codes.Yes;
			invoiceLine1.US_CottonFeeExempt = YesNoDefaultList.Codes.Yes;
			JobComInvoiceLine invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine3.US_R_OrigTariff = "8517.11.0000";
			invoiceLine3.US_R_OrigSPI = "A";
			invoiceLine3.US_R_OrigCV = 12000m;
			invoiceLine3.JI_LinePrice = 13000m;
			invoiceLine3.US_R_OrigFirstQty = 250m;
			invoiceLine3.US_R_OrigFirstUQ = "KG";
			invoiceLine3.JI_CustomsQuantity = 260m;
			AssertEquals("pre-condition", 1, reconDeclaration.OriginalEntries.Count);
			AssertEquals("pre-condition", 1, reconDeclaration.Invoices.Count);
			AssertEquals("pre-condition", 3, reconDeclaration.InvoiceLines.Count);
			reconFlattenedDataLineCollection = new ReconFlattenedDataLineCollection(Factory);
			//Entry XJ51: Line number 2
			ReconFlattenedDataLine dataLine = reconFlattenedDataLineCollection.AddNew();
			dataLine.EntryNumber = "XJ51";
			dataLine.OwnerReferenceNumber = "8974578";
			dataLine.GoodsDescription = "45426 TELEPHONE PARTS";
			dataLine.LineNumber = 2;
			dataLine.ImportationDate = new ZDateTime(2008, 10, 25);
			dataLine.PaymentDate = new ZDateTime(2008, 10, 27);
			dataLine.EntryDate = new ZDateTime(2008, 10, 23);
			dataLine.EntryPort = "2506";
			dataLine.CountryOfOrigin = "CN";
			dataLine.OriginalTariff = "8517.11.0000";
			dataLine.ReconTariff = "8517.12.0000";
			dataLine.OriginalSPI = "A";
			dataLine.ReconSPI = "B";
			dataLine.OriginalCustomsValue = 12000m;
			dataLine.ReconCustomsValue = 13000m;
			dataLine.OriginalFirstQty = 250m;
			dataLine.OriginalFirstUQ = "KG";
			dataLine.OriginalCottonFeeExempt = YesNoDefaultList.Codes.No;
			dataLine.ReconCottonFeeExempt = YesNoDefaultList.Codes.No;
			dataLine.ReconFirstQty = 260m;
			//Entry XJ51: Line number 1
			dataLine = reconFlattenedDataLineCollection.AddNew();
			dataLine.OwnerReferenceNumber = "8974578";
			dataLine.EntryNumber = "XJ51";
			dataLine.GoodsDescription = "45426 TELEPHONE PARTS";
			dataLine.LineNumber = 1;
			dataLine.ImportationDate = new ZDateTime(2008, 10, 25);
			dataLine.PaymentDate = new ZDateTime(2008, 10, 27);
			dataLine.EntryDate = new ZDateTime(2008, 10, 23);
			dataLine.EntryPort = "2506";
			dataLine.CountryOfOrigin = "CN";
			dataLine.OriginalTariff = "8517.11.0000";
			dataLine.OriginalSPI = "A";
			dataLine.OriginalCottonFeeExempt = YesNoDefaultList.Codes.Yes;
			dataLine.ReconCottonFeeExempt = YesNoDefaultList.Codes.Yes;
			dataLine.OriginalCustomsValue = 12000m;
			dataLine.ReconCustomsValue = 13000m;
			dataLine.OriginalFirstQty = 250m;
			dataLine.OriginalFirstUQ = "KG";
			dataLine.ReconFirstQty = 260m;
			//Entry XJ51: Line number 3
			dataLine = reconFlattenedDataLineCollection.AddNew();
			dataLine.OwnerReferenceNumber = "8974578";
			dataLine.EntryNumber = "XJ51";
			dataLine.GoodsDescription = "45426 TELEPHONE PARTS";
			dataLine.LineNumber = 3;
			dataLine.IsChildLine = ZBool.True;
			dataLine.ImportationDate = new ZDateTime(2008, 10, 25);
			dataLine.PaymentDate = new ZDateTime(2008, 10, 27);
			dataLine.EntryDate = new ZDateTime(2008, 10, 23);
			dataLine.EntryPort = "2506";
			dataLine.CountryOfOrigin = "CN";
			dataLine.OriginalTariff = "8517.11.0000";
			dataLine.OriginalSPI = "A";
			dataLine.OriginalCustomsValue = 12000m;
			dataLine.ReconCustomsValue = 13000m;
			dataLine.OriginalFirstQty = 250m;
			dataLine.OriginalFirstUQ = "KG";
			dataLine.ReconFirstQty = 260m;
			//Entry XXX31025: Line number 1
			dataLine = reconFlattenedDataLineCollection.AddNew();
			dataLine.OwnerReferenceNumber = "8974578";
			dataLine.EntryNumber = "XXX31025";
			dataLine.GoodsDescription = "45426 TELEPHONE PARTS";
			dataLine.LineNumber = 1;
			dataLine.ImportationDate = new ZDateTime(2008, 10, 25);
			dataLine.PaymentDate = new ZDateTime(2008, 10, 27);
			dataLine.EntryDate = new ZDateTime(2008, 10, 23);
			dataLine.EntryPort = "2506";
			dataLine.CountryOfOrigin = "CN";
			dataLine.OriginalTariff = "8517.11.0000";
			dataLine.OriginalSPI = "A";
			dataLine.OriginalCustomsValue = 12000m;
			dataLine.ReconCustomsValue = 13000m;
			dataLine.OriginalFirstQty = 250m;
			dataLine.OriginalFirstUQ = "KG";
			classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_LookupCode = "LK23344@#";
			classification.CC_TariffNum = "8517.12.0000";
			dataLine.ProductNumber = "ZZ3234@#";
			dataLine.Lookup = "LK23344@#";
			dataLine.ReconSecondarySPI = SecondarySpecProgIndicatorList.Codes.M;
			dataLine.OriginalDutyOverride = ZBool.True;
			dataLine.OriginalDuty = 423.23m;
			dataLine.OriginalTaxApply = TaxApplyList.Codes.Override;
			dataLine.OriginalTaxCode = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
			dataLine.OriginalTaxRateS = AppendixBTaxRateList.Codes.Specify;
			dataLine.OriginalTaxRate = 984.97m;
			dataLine.OriginalTaxRateQuantity = 98.97m;
			dataLine.ReconDutyOverride = ZBool.True;
			dataLine.ReconDuty = 928.72m;
			dataLine.ReconTaxApply = TaxApplyList.Codes.Override;
			dataLine.ReconTaxCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dataLine.ReconTaxRateS = AppendixBTaxRateList.Codes.Specify;
			dataLine.ReconTaxRate = 233.98m;
			dataLine.ReconTaxRateQuantity = 23.98m;
			dataLine.ReconFirstQty = 260m;
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}

		ReconDeclaration reconDeclaration;
		ReconFlattenedDataLineCollection reconFlattenedDataLineCollection;
		CusClassification classification;
	}
}
