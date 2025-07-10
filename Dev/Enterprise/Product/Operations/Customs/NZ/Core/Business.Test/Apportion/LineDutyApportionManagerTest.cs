using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Registry;

namespace Enterprise.Customs.NZ.Business.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Customs.NZ.Business.Declaration.FormalEntry;
	using Enterprise.Customs.NZ.Business.MasterFiles;
	using Enterprise.MasterFiles.Business;

	class LineDutyApportionManagerTest : TestCaseWithFactory
	{
		public void TestMergesTwice()
		{
			CusEntryLine cline = CreateInvoiceLine();
			CusEntryHeaderCharge charge1 = declaration.CusEntryHeader.Charges.AddNew();
			charge1.C1_ChargeType = "ENF";
			charge1.C1_ChargeAmount = 113m;

			CusEntryHeaderCharge charge4 = declaration.CusEntryHeader.Charges.AddNew();
			charge4.C1_ChargeType = "EFG";
			charge4.C1_ChargeAmount = 117m;

			CusEntryLineFee lineFee3 = cline.Fees.AddNew();
			lineFee3.CF_ChargeType = "GST";
			lineFee3.CF_ChargeAmount = 140m;

			CusEntryLineFee lineFee6 = cline.Fees.AddNew();
			lineFee6.CF_ChargeType = "ACC";
			lineFee6.CF_ChargeAmount = 151m;

			CusEntryLineFee lineFee7 = cline.Fees.AddNew();
			lineFee7.CF_ChargeType = "ALC";
			lineFee7.CF_ChargeAmount = 164m;
			Factory.Save();

			LineDutyApportionManager lineTest = new LineDutyApportionManager();
			lineTest.Apportion(declaration);

			InvoiceLineCompleteCollection jobComInvoiceLines = declaration.InvoiceLines;
			AssertEquals(3, jobComInvoiceLines.Count);
			AssertEquals(true, jobComInvoiceLines[0].JI_IsApportioned);
			AssertEquals(true, jobComInvoiceLines[1].JI_IsApportioned);
			AssertEquals(true, jobComInvoiceLines[2].JI_IsApportioned);

			AssertEquals(113m, jobComInvoiceLines[0].JI_EntryFeeAmount + jobComInvoiceLines[1].JI_EntryFeeAmount + jobComInvoiceLines[2].JI_EntryFeeAmount);
			AssertEquals(117m, jobComInvoiceLines[0].JI_EntryFeeGSTAmount + jobComInvoiceLines[1].JI_EntryFeeGSTAmount + jobComInvoiceLines[2].JI_EntryFeeGSTAmount);
			AssertEquals(140m, jobComInvoiceLines[0].JI_GSTAmount + jobComInvoiceLines[1].JI_GSTAmount + jobComInvoiceLines[2].JI_GSTAmount);
			AssertEquals(151m + 164m, jobComInvoiceLines[0].JI_LevyAmount + jobComInvoiceLines[1].JI_LevyAmount + jobComInvoiceLines[2].JI_LevyAmount);

			CusEntryLineFee lineFee11 = cline.Fees.AddNew();
			lineFee11.CF_ChargeType = "HER";
			lineFee11.CF_ChargeAmount = 2179m;

			CusEntryLineFee lineFee12 = cline.Fees.AddNew();
			lineFee12.CF_ChargeType = "PML";
			lineFee12.CF_ChargeAmount = 2111m;
			Factory.Save();

			lineTest.Apportion(declaration);
			AssertEquals(113m, jobComInvoiceLines[0].JI_EntryFeeAmount + jobComInvoiceLines[1].JI_EntryFeeAmount + jobComInvoiceLines[2].JI_EntryFeeAmount);
			AssertEquals(117m, jobComInvoiceLines[0].JI_EntryFeeGSTAmount + jobComInvoiceLines[1].JI_EntryFeeGSTAmount + jobComInvoiceLines[2].JI_EntryFeeGSTAmount);
			AssertEquals(140m, jobComInvoiceLines[0].JI_GSTAmount + jobComInvoiceLines[1].JI_GSTAmount + jobComInvoiceLines[2].JI_GSTAmount);
			AssertEquals(151m + 164m + 2179m + 2111m, jobComInvoiceLines[0].JI_LevyAmount + jobComInvoiceLines[1].JI_LevyAmount + jobComInvoiceLines[2].JI_LevyAmount);
		}

		public void TestHeaderCharges()
		{
			CreateInvoiceLine();
			CusEntryHeaderCharge charge1 = declaration.CusEntryHeader.Charges.AddNew();
			charge1.C1_ChargeType = "ENF";
			charge1.C1_ChargeAmount = 520m;

			CusEntryHeaderCharge charge4 = declaration.CusEntryHeader.Charges.AddNew();
			charge4.C1_ChargeType = "EFG";
			charge4.C1_ChargeAmount = 750m;

			Factory.Save();

			LineDutyApportionManager lineTest = new LineDutyApportionManager();
			lineTest.Apportion(declaration);
			InvoiceLineCompleteCollection jobComInvoiceLines = declaration.InvoiceLines;
			AssertEquals(true, jobComInvoiceLines[0].JI_IsApportioned);
			AssertEquals(true, jobComInvoiceLines[1].JI_IsApportioned);
			AssertEquals(true, jobComInvoiceLines[2].JI_IsApportioned);
			AssertEquals(520m, jobComInvoiceLines[0].JI_EntryFeeAmount + jobComInvoiceLines[1].JI_EntryFeeAmount + jobComInvoiceLines[2].JI_EntryFeeAmount);
			AssertEquals(750m, jobComInvoiceLines[0].JI_EntryFeeGSTAmount + jobComInvoiceLines[1].JI_EntryFeeGSTAmount + jobComInvoiceLines[2].JI_EntryFeeGSTAmount);
		}

		public void TestApportionDuty()
		{
			CreateInvoiceLine();
			LineDutyApportionManager lineTest = new LineDutyApportionManager();
			lineTest.Apportion(declaration);
			AssertEquals(1, declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Count);
			AssertEquals(3, declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.Count);
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0];
			AssertEquals(true, invoiceHeader.JobComInvoiceLines[0].JI_IsApportioned);
			AssertEquals(true, invoiceHeader.JobComInvoiceLines[1].JI_IsApportioned);
			AssertEquals(true, invoiceHeader.JobComInvoiceLines[2].JI_IsApportioned);
			AssertEquals(500m, invoiceHeader.JobComInvoiceLines[0].JI_DutyAmount + invoiceHeader.JobComInvoiceLines[1].JI_DutyAmount + invoiceHeader.JobComInvoiceLines[2].JI_DutyAmount);
		}

		public void TestApportionLevyFee()
		{
			CusEntryLine cline = CreateInvoiceLine();

			var feeCodelist = cline.Factory.GetCachedValue<EntryChargeTypeList>();
			AssertNotNull(feeCodelist);

			CusEntryLineFee lineFee3 = cline.Fees.AddNew();
			lineFee3.CF_ChargeType = "GST";
			lineFee3.CF_ChargeAmount = 140m;

			CusEntryLineFee lineFee6 = cline.Fees.AddNew();
			lineFee6.CF_ChargeType = "ACC";
			lineFee6.CF_ChargeAmount = 150m;

			CusEntryLineFee lineFee7 = cline.Fees.AddNew();
			lineFee7.CF_ChargeType = "ALC";
			lineFee7.CF_ChargeAmount = 160m;

			CusEntryLineFee lineFee11 = cline.Fees.AddNew();
			lineFee11.CF_ChargeType = "HER";
			lineFee11.CF_ChargeAmount = 200m;

			CusEntryLineFee lineFee12 = cline.Fees.AddNew();
			lineFee12.CF_ChargeType = "PML";
			lineFee12.CF_ChargeAmount = 210m;

			CusEntryLineFee lineFee13 = cline.Fees.AddNew();
			lineFee13.CF_ChargeType = "SGG";
			lineFee13.CF_ChargeAmount = 220m;

			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0];
			AssertEquals(3, declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.Count);

			invoiceHeader.JobComInvoiceLines[0].JI_LevyAmount = 100m;

			LineDutyApportionManager lineTest = new LineDutyApportionManager();
			lineTest.Apportion(declaration);

			AssertEquals(true, invoiceHeader.JobComInvoiceLines[0].JI_IsApportioned);
			AssertEquals(true, invoiceHeader.JobComInvoiceLines[1].JI_IsApportioned);
			AssertEquals(true, invoiceHeader.JobComInvoiceLines[2].JI_IsApportioned);
			AssertEquals(140m, invoiceHeader.JobComInvoiceLines[0].JI_GSTAmount + invoiceHeader.JobComInvoiceLines[1].JI_GSTAmount + invoiceHeader.JobComInvoiceLines[2].JI_GSTAmount);
			AssertEquals(150m + 160m + 200m + 210m + 220m, invoiceHeader.JobComInvoiceLines[0].JI_LevyAmount + invoiceHeader.JobComInvoiceLines[1].JI_LevyAmount + invoiceHeader.JobComInvoiceLines[2].JI_LevyAmount);
		}

		CusEntryLine CreateInvoiceLine()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_DateOfArrival = new ZDateTime(2005, 1, 1);
			declaration.JE_EDITransmitDate = new ZDateTime(2005, 1, 1);

			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "1234";
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			invoiceHeader.JZ_InvoiceAmount = 400m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_IncoTerm = "FOB";

			CusClassification lookupCode = Factory.New<CusClassification>();
			lookupCode.CC_ClassificationType = CusClassification.ClassificationType.Both;
			lookupCode.CC_LookupCode = "WOMENSJACKETS";
			lookupCode.CC_Description = "WOMEN COTTON JACKETS";
			lookupCode.CC_TariffNum = "6202.92.01.00A";

			CusEntryHeader originalEntryHeader = (CusEntryHeader)declaration.CusEntryHeader;
			CusEntryLine entryLine = originalEntryHeader.MergedLines.AddNew();

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CC = lookupCode.PK;
			invoiceLine.JI_LinePrice = 300m;
			invoiceLine.JI_CustomsQuantity = 15m;
			invoiceLine.JI_CustomsUnitQty = "NMB";
			invoiceLine.JI_CountryOfOrigin = "CN";
			invoiceLine.JI_RN_NKCountryOfExport = "AU";
			invoiceLine.JI_QualifiesForPreferentialDuty = "N";
			entryLine.InvoiceLines.Add(invoiceLine);

			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CC = lookupCode.PK;
			invoiceLine2.JI_LinePrice = 100m;
			invoiceLine2.JI_CustomsQuantity = 25m;
			invoiceLine2.JI_CustomsUnitQty = "NMB";
			invoiceLine2.JI_CountryOfOrigin = "CN";
			invoiceLine2.JI_RN_NKCountryOfExport = "AU";
			invoiceLine2.JI_QualifiesForPreferentialDuty = "N";
			entryLine.InvoiceLines.Add(invoiceLine2);

			JobComInvoiceLine invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CC = lookupCode.PK;
			invoiceLine3.JI_LinePrice = 100m;
			invoiceLine3.JI_CustomsQuantity = 25m;
			invoiceLine3.JI_CustomsUnitQty = "NMB";
			invoiceLine3.JI_CountryOfOrigin = "CN";
			invoiceLine3.JI_RN_NKCountryOfExport = "AU";
			invoiceLine3.JI_QualifiesForPreferentialDuty = "N";
			entryLine.InvoiceLines.Add(invoiceLine3);

			entryLine.DutyAmount = 500m;
			return entryLine;
		}

		JobDeclaration declaration;
		CusClassification lookupCode1;
		CusClassification lookupCode2;
		OrgHeader supplier;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			lookupCode1 = Factory.New<CusClassification>();
			lookupCode1.CC_ClassificationType = CusClassification.ClassificationType.Both;
			lookupCode1.CC_LookupCode = "Clothes";
			lookupCode1.CC_TariffNum = "6103.41.00.11K";

			lookupCode2 = Factory.New<CusClassification>();
			lookupCode2.CC_ClassificationType = CusClassification.ClassificationType.Both;
			lookupCode2.CC_LookupCode = "Plate";
			lookupCode2.CC_TariffNum = "3919.10.09.51A";

			supplier = Factory.LoadTop1<OrgHeader>(new ZQuery());
		}
	}
}
