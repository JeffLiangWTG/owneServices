using System;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCusEntryLine))]
	sealed class DocCusEntryLineTest : DocBaseCusEntryLineAbstractTest<CusEntryLine, DocCusEntryLine>
	{
		#region Additional Information

		public void TestAdditionalInformationOneProperties()
		{
			var addInfo1 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo1.CY_Code = "CC";
			addInfo1.CY_Data = "CC Data";
			var addInfo2 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo2.CY_Code = "AA";
			addInfo2.CY_Data = "AA Data";
			AssertEquals("AdditionalInfoCodeOne is not empty", "AA", EntryLineWrapper.AdditionalInfoCodeOne);
			AssertEquals("AdditionalInfoOne is not empty", "AA Data", EntryLineWrapper.AdditionalInfoOne);
		}

		public void TestAdditionalInformationTwoProperties()
		{
			var addInfo1 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo1.CY_Code = "CC";
			addInfo1.CY_Data = "CC Data";
			var addInfo2 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo2.CY_Code = "AA";
			addInfo2.CY_Data = "AA Data";
			AssertEquals("AdditionalInfoCodeTwo is not empty", "CC", EntryLineWrapper.AdditionalInfoCodeTwo);
			AssertEquals("AdditionalInfoTwo is not empty", "CC Data", EntryLineWrapper.AdditionalInfoTwo);
		}

		public void TestAdditionalInformationThreeProperties()
		{
			var addInfo1 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo1.CY_Code = "CC";
			addInfo1.CY_Data = "CC Data";
			var addInfo2 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo2.CY_Code = "ATV";
			addInfo2.CY_Data = "ATV Data";
			var addInfo3 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo3.CY_Code = "BBB";
			addInfo3.CY_Data = "BBB Data";
			AssertEquals("AdditionalInfoCodeThree is not empty", "CC", EntryLineWrapper.AdditionalInfoCodeThree);
			AssertEquals("AdditionalInfoThree is not empty", "CC Data", EntryLineWrapper.AdditionalInfoThree);
		}

		public void TestAdditionalInformationFourProperties()
		{
			var addInfo1 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo1.CY_Code = "CC";
			addInfo1.CY_Data = "CC Data";
			var addInfo2 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo2.CY_Code = "ATV";
			addInfo2.CY_Data = "ATV Data";
			var addInfo3 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo3.CY_Code = "DD";
			addInfo3.CY_Data = "DD Data";
			var addInfo4 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo4.CY_Code = "EE";
			addInfo4.CY_Data = "EE Data";
			AssertEquals("AdditionalInfoCodeFour is not empty", "EE", EntryLineWrapper.AdditionalInfoCodeFour);
			AssertEquals("AdditionalInfoFour is not empty", "EE Data", EntryLineWrapper.AdditionalInfoFour);
		}

		#endregion

		#region ZString Fields

		public override void TestLinePricesWithCurrency()
		{
			var message = "Should be formatted with South African Number Culture";
			AssertEquals(message, "200,05 ZAR", DocEntryLineMergeOfTwoInvoiceLines.LinePricesWithCurrency);
		}

		public void TestActualPrice()
		{
			AssertEquals("Actual Price is empty", ZString.Empty, EntryLineWrapper.ActualPrice);

			entryLine.CL_LineNumber = 1;
			invoiceLine.JI_LinePrice = 0M;
			AssertEquals("Actual Price is NC", "NC", EntryLineWrapper.ActualPrice);

			invoiceLine.JI_LinePrice = 123.45M;
			entryLine.CL_CustomsValue = 123.45m;
			AssertEquals("Actual Price is 123", "NC", EntryLineWrapper.ActualPrice);

			entryLine.CL_CustomsValue = 1.65M;
			invoiceLine.JI_LinePrice = 1.65M;
			AssertEquals("Actual Price is 2", "NC", EntryLineWrapper.ActualPrice);
		}

		public void TestDescription()
		{
			invoiceLine.JI_Description = "LINE DESCRIPTION";
			AssertEquals("Entry Line Description", "LINE DESCRIPTION", EntryLineWrapper.Description);
		}

		#endregion

		#region Wrapper Fields

		public void TestInvoiceLineWrapper()
		{
			AssertNotNull("InvoiceLine", EntryLineWrapper.InvoiceLine);
			AssertEquals("InvoiceLine is of type DocJobComInvoiceLIne", typeof(DocJobComInvoiceLine), EntryLineWrapper.InvoiceLine.GetType());
		}

		public void TestCusEntryHeader()
		{
			AssertNotNull("CusEntryHeader", EntryLineWrapper.CusEntryHeader);
			AssertEquals("CusEntryHeader is of type DocCusEntryHeader", typeof(DocCusEntryHeader), EntryLineWrapper.CusEntryHeader.GetType());
		}

		#endregion

		#region ZDecimal Fields
		public override void TestDutyAmountRounded()
		{
			EntryLineInternal.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 12.3453M);
			AssertEquals("DutyAmount", 12.35M, EntryLineWrapperInternal.DutyAmountRounded);
		}

		public override void TestGSTVATAmountRounded()
		{
			var entryLine = Factory.New<CusEntryLine>();
			entryLine.Fees.AddOrUpdate("VAT", 12.3453m);
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invLine = invoice.InvoiceLines.AddNew();
			invLine.JI_CL = entryLine.PK;
			invLine.JI_ZZF_NKTaxType = "VAT";
			var entryLineWrapper = DocCusEntryLine.New(entryLine, Factory);
			AssertEquals("DutyAmount", 12.35M, entryLineWrapper.GSTVATAmountRounded);
		}

		public void TestCustomsDuty()
		{
			MockCusEntryLine.Object.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 1200.50m);
			AssertEquals("CustomsDuty", 1200.50m, MockEntryWrapper.CustomsDuty);
		}

		public void TestImportDutyPaid()
		{
			MockCusEntryLine.Setup(m => m.ImportDutyPaid).Returns(new ZDecimal(120m));
			AssertEquals("Import Duty Paid", 120m, MockEntryWrapper.ImportDutyPaid);
		}

		public void TestImportDutySch1P2BPaid()
		{
			MockCusEntryLine.Setup(m => m.ImportDutySch1P2BPaid).Returns(new ZDecimal(150m));
			AssertEquals("Import Duty Sch1P2B Paid", 150m, MockEntryWrapper.ImportDutySch1P2BPaid);
		}

		public void TestImportVATPaid()
		{
			MockCusEntryLine.Setup(m => m.ImportVATPaid).Returns(new ZDecimal(150m));
			AssertEquals("Import VAT Paid", 150m, MockEntryWrapper.ImportVATPaid);
		}

		public void TestCustomsValueFromRelatedImportEntry()
		{
			MockCusEntryLine.Setup(m => m.ImportCustomsValue).Returns(new ZDecimal(500));
			AssertEquals("Import Customs Value", 500.0m, MockEntryWrapper.CustomsValueFromRelatedImportEntry);
		}

		#endregion

		#region Implementation

		CusEntryLine entryLine;
		DocCusEntryLine EntryLineWrapper
		{
			get { return EntryLineWrapperInternal; }
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		CusEntryHeader entryHeader;

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.SouthAfrica; }
		}

		DocCusEntryLine MockEntryWrapper
		{
			get
			{
				if (fMockEntryWrapper == null)
				{
					fMockEntryWrapper = DocCusEntryLine.New(MockCusEntryLine.Object, Factory);
				}
				return fMockEntryWrapper;
			}
		}
		DocCusEntryLine fMockEntryWrapper;

		Mock<CusEntryLine> MockCusEntryLine
		{
			get
			{
				if (fMockCusEntryLine == null)
				{
					fMockCusEntryLine = Factory.NewMoq<CusEntryLine>();
				}
				return fMockCusEntryLine;
			}
		}
		Mock<CusEntryLine> fMockCusEntryLine;

		protected override CusEntryLine GetNewEntryLine()
		{
			if (entryLine == null)
			{
				declaration = (JobDeclaration)GetNewDeclaration();
				JobComInvoiceGroupHeader invoiceGroupHeader = declaration.JobComInvoiceGroupHeaders[0];
				invoiceHeader = invoiceGroupHeader.JobComInvoiceHeaders.AddNew();
				invoiceHeader.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
			}
			return entryLine;
		}

		protected override DocCusEntryLine CreateEntryLineWrapper(Enterprise.Customs.Business.ICusEntryLine entryLineInternal)
		{
			return DocCusEntryLine.New((CusEntryLine)entryLineInternal, Factory);
		}

		Enterprise.Customs.Business.ICusEntryLine fEntryLineMergeOfTwoInvoiceLines;
		protected override Enterprise.Customs.Business.ICusEntryLine EntryLineMergeOfTwoInvoiceLines
		{
			get
			{
				if (fEntryLineMergeOfTwoInvoiceLines == null)
				{
					JobDeclaration declaration = (JobDeclaration)GetNewDeclaration();
					JobComInvoiceHeader header = declaration.Invoices.AddNew();
					header.JZ_InvoiceNumber = "1";
					header.JZ_RX_NKInvoice_Currency = "ZAR";

					JobComInvoiceLine invoiceLine = header.JobComInvoiceLines.AddNew();
					invoiceLine.JI_LinePrice = 200.05m;
					invoiceLine.JI_Tariff = "123";
					invoiceLine.JI_LineNo = 1;

					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = Enterprise.Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
					var testInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
					invoiceLine.JI_CEI = testInstruction.PK;
					new LineMerger(declaration).DoMerge();
					Assert("Must have at least one CustomsEntryHeader", declaration.CustomsEntryHeaders.Count > 0);
					Assert("Must have at least one CustomsEntryHeader", declaration.CustomsEntryHeaders[0].MergedLines.Count > 0);
					fEntryLineMergeOfTwoInvoiceLines = declaration.CustomsEntryHeaders[0].MergedLines[0];
				}
				return fEntryLineMergeOfTwoInvoiceLines;
			}
		}

		protected override DocCusEntryLine DocEntryLineMergeOfTwoInvoiceLines
		{
			get { return CreateEntryLineWrapper(EntryLineMergeOfTwoInvoiceLines); }
		}

		#endregion

		protected override void SetUp()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Interfaced;
			CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface);
			base.SetUp();
		}
	}
}
