using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconDeclarationValidationTest : TestCaseWithFactory
	{
		public void TestValidateMaximumNumberOfEntries()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			for (int i = 0; i < 9999; i++)
			{
				reconDec.OriginalEntries.AddNew();
			}

			reconDec.Validation.ValidateAll();
			AssertNoRowMessageError(reconDec, ReconDeclarationValidation.MaximumNumberOfEntriesExceeded);
			var entry = reconDec.OriginalEntries.AddNew();
			reconDec.Validation.ValidateAll();
			AssertHasRowMessageError(reconDec, ReconDeclarationValidation.MaximumNumberOfEntriesExceeded);
			reconDec.OriginalEntries.Remove(entry);
			entry.Delete();
			reconDec.Validation.ValidateAll();
			AssertNoRowMessageError(reconDec, ReconDeclarationValidation.MaximumNumberOfEntriesExceeded);
		}

		public void TestCheckReconEntryNumberWithEntryFilerCode()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_BondProducerAccNo = "10";
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 3000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "SG";
			invoice.US_UC_NKCountryOfOrigin = "SG";
			invoice.Charges.AddNew("OFT", 50m, "USD");
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3201.90.1000";
			invoiceLine.JI_CustomsQuantity = 50m;
			invoiceLine.JI_LinePrice = 3000m;
			invoiceLine.US_SPI = "";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entry1 = declaration.CustomsEntryHeaders[0];
			entry1.EntryNumber = "~7854378";
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconEntry;
			JobDeclaration declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration2.US_EnableENS = true;
			declaration2.US_EntryFilerCode = "XJ5";
			declaration2.US_BondProducerAccNo = "11";
			invoice = declaration2.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 3000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "SG";
			invoice.US_UC_NKCountryOfOrigin = "SG";
			invoice.Charges.AddNew("OFT", 50m, "USD");
			invoiceLine = declaration2.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3201.90.1000";
			invoiceLine.JI_CustomsQuantity = 50m;
			invoiceLine.JI_LinePrice = 3000m;
			invoiceLine.US_SPI = "";
			declaration2.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			CusEntryHeader entry2 = declaration2.CustomsEntryHeaders[0];
			entry2.EntryNumber = "~7854333";
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconEntry;
			ReconDeclaration reconDec = new ReconDeclaration(declaration2);
			ReconOriginalEntryHeader reconEntry = reconDec.OriginalEntries.AddNew();
			reconEntry.CH_OrigEntryReference = "XJ5~7854333";
			reconEntry.CH_CH_OriginalEntry = entry2.PK;
			reconEntry = reconDec.OriginalEntries.AddNew();
			reconEntry.CH_OrigEntryReference = "XJ5~7854378";
			reconEntry.CH_CH_OriginalEntry = entry1.PK;
			reconDec.Validation.ValidateReconEntryNumberWithEntryFilerCode();
			AssertHasMessageError(reconDec.ReconEntryNumberWithEntryFilerCodeInfo, ReconDeclarationValidation.BondNoMustBeTheSame);
			JobDeclaration declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration3.US_EnableENS = true;
			declaration3.US_EntryFilerCode = "XJ5";
			declaration3.US_BondProducerAccNo = "10";
			invoice = declaration3.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 3000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "SG";
			invoice.US_UC_NKCountryOfOrigin = "SG";
			invoice.Charges.AddNew("OFT", 50m, "USD");
			invoiceLine = declaration3.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3201.90.1000";
			invoiceLine.JI_CustomsQuantity = 50m;
			invoiceLine.JI_LinePrice = 3000m;
			invoiceLine.US_SPI = "";
			declaration3.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entry2 = declaration3.CustomsEntryHeaders[0];
			entry2.EntryNumber = "~7854333";
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconEntry;
			ReconDeclaration reconDec2 = new ReconDeclaration(declaration3);
			reconEntry = reconDec2.OriginalEntries.AddNew();
			reconEntry.CH_OrigEntryReference = "XJ5~7854333";
			reconEntry.CH_CH_OriginalEntry = entry2.PK;
			reconEntry = reconDec2.OriginalEntries.AddNew();
			reconEntry.CH_OrigEntryReference = "XJ5~7854378";
			reconEntry.CH_CH_OriginalEntry = entry1.PK;
			AssertNoMessageError(reconDec2.ReconEntryNumberWithEntryFilerCodeInfo, ReconDeclarationValidation.BondNoMustBeTheSame);
		}
	}
}
