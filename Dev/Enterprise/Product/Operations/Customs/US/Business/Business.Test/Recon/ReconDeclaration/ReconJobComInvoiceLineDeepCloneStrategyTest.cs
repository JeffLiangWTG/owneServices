using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconEntryLineDeepCloneStrategyTest : TestCaseWithFactory
	{
		public void TestCopyReconEntryLine()
		{
			var srcDec = Factory.New<JobDeclaration>();
			srcDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			srcDec.JE_MessageType = Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Recon;
			srcDec.JE_EntryAuthorisationDate = ZDateTime.BrettsBirthday;
			srcDec.US_Comment = "COMMENTS";
			srcDec.US_PreliminaryStatementPrintDate = ZDateTime.BrettsBirthday;
			srcDec.US_EntryFilerCode = "XJ5";
			srcDec.US_ClientBranchDesignation = "DP";
			CusEntryHeader entryRCI = Factory.New<CusEntryHeader>();
			entryRCI.CH_JE = srcDec.PK;
			entryRCI.CH_BGMReference = "XJ5123456";
			entryRCI.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconOriginalEntry;
			entryRCI.Charges.AddNew("DDD", 45.5);
			entryRCI.Charges.AddNew("PPP", 46.6);
			CusEntryHeader entryREC = Factory.New<CusEntryHeader>();
			entryREC.CH_JE = srcDec.PK;
			entryREC.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconEntry;
			var broker = Factory.New<MasterFiles.Business.GlbStaff>();
			broker.GS_Code = "AGT";
			srcDec.JE_GS_NKCusAgent = broker.GS_Code;
			srcDec.ReconDeclaration = srcDec.ReconDeclaration ?? new ReconDeclaration(srcDec);
			var reconDec = srcDec.ReconDeclaration;
			reconDec.US_PreliminaryStatementPrintDate = ZDateTime.BrettsBirthday;
			reconDec.US_Comment = "Recon COMMENTS";
			reconDec.US_EstimatedEntryDate = ZDateTime.BrettsBirthday;
			JobComInvoiceHeader invoice = reconDec.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "XJ5123456";
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_CH_ReconEntry = entryRCI.PK;
			JobComInvoiceLine invoiceLine = reconDec.InvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 10m;
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_R_OrigSupTariff = "5678.12.5678";
			invoiceLine.JI_FormattedTariff = "1111.11.1110";
			invoiceLine.US_SupTariff = "1212.22.1212";
			invoiceLine.JI_Calc_Invoice = "XJ5123456";
			invoiceLine.US_R_OrigFirstUQ = "KG";
			invoiceLine.US_R_OrigFirstQty = 30m;
			invoiceLine.US_R_ReconHMFAmount = 11.1;
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
			invoiceLine.US_CottonFeeExempt = "C";
			invoiceLine.US_R_OrigSPI = "A";
			invoiceLine.US_SPI = "B";
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.M;
			invoiceLine.JI_Description = "WHO IS THIS";
			invoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_R_OrigTaxCode = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
			invoiceLine.US_R_OrigTaxRate = 323.23m;
			invoiceLine.US_R_OrigOverrideDuty = ZBool.True;
			invoiceLine.US_R_OrigDuty = 532.54m;
			invoiceLine.US_OverrideDuty = ZBool.True;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.Wines;
			invoiceLine.US_TaxRate = 983.43m;
			invoiceLine.US_TaxQty = 85.43m;
			Factory.Save();
			var newReconDec = (JobDeclaration)srcDec.ReconDeclaration.TemplateReconDeclarationCopyCore(Customs.Business.CloneType.TemplateCopy);
			AssertEquals(newReconDec.JE_MessageType, Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Recon);
			AssertEntryLines(newReconDec.ReconDeclaration.InvoiceLines[0], srcDec.ReconDeclaration.InvoiceLines[0]);
		}

		void AssertEntryLines(JobComInvoiceLine reconLine, JobComInvoiceLine srcLine)
		{
			AssertEquals(reconLine.US_R_OrigHMFAmount, srcLine.US_R_ReconHMFAmount);
			AssertEquals(reconLine.US_R_OrigMPFAmount, srcLine.US_R_ReconMPFAmount);
			AssertEquals(reconLine.US_R_OrigOverrideSupDuty, srcLine.US_OverrideSupDuty);
			AssertEquals(reconLine.US_R_OrigCV, srcLine.JI_LinePrice);
			AssertEquals(reconLine.US_R_OrigDuty, reconLine.US_Duty);
			AssertEquals(reconLine.US_R_OrigFirstQty, srcLine.JI_CustomsQuantity);
			AssertEquals(reconLine.US_R_OrigFirstUQ, srcLine.JI_CustomsUnitQty);
			AssertEquals(reconLine.US_R_OrigOverrideDuty, srcLine.US_OverrideDuty);
			AssertEquals(reconLine.US_R_OrigSPI, srcLine.US_SPI);
			AssertEquals(reconLine.US_R_OrigSecondQty, srcLine.JI_CustomsSecondQuantity);
			AssertEquals(reconLine.US_R_OrigCottonFeeExempt, srcLine.US_CottonFeeExempt);
			AssertEquals(reconLine.US_R_OrigSecondUQ, srcLine.JI_CustomsSecondUnitQty);
			AssertEquals(reconLine.OriginalTariffFormatted, srcLine.JI_FormattedTariff);
			AssertEquals(reconLine.OriginalSupTariffFormatted, srcLine.SupTariffFormatted);
		}
	}
}
