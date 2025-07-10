using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class DeclarationMessageHelperTest : TestCaseWithDummy
	{
		public void TestGetEntryStatusShouldReturnExpectedStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			var branch = declaration.Company.Branches.AddNew();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_DeclarationReference = "JE001";
			declaration.JE_GB = branch.PK;
			cusEntryHeader.CH_BGMReference = "LRN9998889991";
			cusEntryHeader.Messages.AddNew();

			CombineAssertions(() =>
			{
				cusEntryHeader.CH_Status = "NOS";
				AssertState("TEST_TYPE", "NOS", "TEST_STATUS", "NOS", cusEntryHeader);
				cusEntryHeader.CH_Status = TRMessageStatusCodeList.Codes.Sent;
				AssertState("DKO", "SNT", "TEST_STATUS", "SCM", cusEntryHeader);
				cusEntryHeader.CH_Status = TRMessageStatusCodeList.Codes.Accepted;
				AssertState("DKO", "ACP", "TEST_STATUS", "ACM", cusEntryHeader);
				cusEntryHeader.CH_Status = TRMessageStatusCodeList.Codes.Sent;
				AssertState("DK1", "SNT", "TEST_STATUS", "QCM", cusEntryHeader);
				cusEntryHeader.CH_Status = TRMessageStatusCodeList.Codes.Accepted;
				AssertState("DK1", "ACP", "TEST_STATUS", "CMR", cusEntryHeader);
				cusEntryHeader.CH_Status = TRMessageStatusCodeList.Codes.Sent;
				AssertState("DTE", "SNT", "TEST_STATUS", "SRM", cusEntryHeader);
				cusEntryHeader.CH_Status = TRMessageStatusCodeList.Codes.Accepted;
				AssertState("DTE", "ACP", "TEST_STATUS", "RMA", cusEntryHeader);
				cusEntryHeader.CH_Status = TRMessageStatusCodeList.Codes.Sent;
				AssertState("DT1", "SNT", "TEST_STATUS", "QRM", cusEntryHeader);
				cusEntryHeader.CH_Status = TRMessageStatusCodeList.Codes.Accepted;
				AssertState("DT1", "ACP", "TEST_STATUS", "RMR", cusEntryHeader);
				cusEntryHeader.CH_Status = TRMessageStatusCodeList.Codes.Sent;
				AssertState("DT2", "SNT", "TEST_STATUS", "QUR", cusEntryHeader);
				cusEntryHeader.CH_Status = TRMessageStatusCodeList.Codes.Accepted;
				AssertState("DT2", "ACP", "TEST_STATUS", "QUA", cusEntryHeader);
				cusEntryHeader.CH_Status = TRMessageStatusCodeList.Codes.Sent;
				AssertState("DT3", "SNT", "TEST_STATUS", "GQR", cusEntryHeader);
				cusEntryHeader.CH_Status = TRMessageStatusCodeList.Codes.Accepted;
				AssertState("DT3", "ACP", "TEST_STATUS", "AGQ", cusEntryHeader);
				cusEntryHeader.CH_Status = TRMessageStatusCodeList.Codes.Accepted;
				AssertState("EUR", "ACP", "TEST_STATUS", "REG", cusEntryHeader);
				cusEntryHeader.CH_Status = TRMessageStatusCodeList.Codes.Error;
				cusEntryHeader.CH_EntryStatus = "ERR";
				AssertState("TEST_TYPE", "ERR", "TEST_STATUS", "ERR", cusEntryHeader);
				cusEntryHeader.CH_Status = TRMessageStatusCodeList.Codes.Accepted;
				AssertState("TEST_TYPE", "ACP", "TEST_STATUS", "REG", cusEntryHeader);
			});
		}

		public void TestUniqueQuestionAndDocument()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
			helper.CreateNewOrGetExistingCusCodeType("TRCUQ", "TurkeyCustomsQuestion", Core.Constants.CountryCodes.Turkey);
			helper.CreateNewOrGetExistingCusCodeType("TRCUW", "TurkeyCustomsWarning", Core.Constants.CountryCodes.Turkey);
			Factory.Save();

			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_GB = newBranch.PK;
			var cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			var cusEntryLine = jobDeclaration.CusEntryHeader.AllEntryLines.AddNew();
			cusEntryLine.CL_LineNumber = 1;

			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_CL = cusEntryLine.PK;

			var cpDecHeaders = cusEntryHeader.CPDecCollection;

			DeclarationMessageHelper.CreateEntryQuestionsContent(cusEntryHeader, 7780, SoapMessageTextHelper.Constants.DocumentCodes.Code7780Desc, 0, "Soru");
			cpDecHeaders = cusEntryHeader.CPDecCollection;
			CombineAssertions("Questions by Header", () =>
			{
				AssertNotNull("Not Null", cpDecHeaders);
				AssertEquals("Count", 1, cpDecHeaders.Count);
				AssertEquals("1.CPDecs | ON_CPDecNum", 7780, cpDecHeaders[0].ON_CPDecNum);
				AssertEquals("1.CPDecs | Description", SoapMessageTextHelper.Constants.DocumentCodes.Code7780Desc, cpDecHeaders[0].Description);
				AssertEquals("1.CPDecs | ON_QuestionType", "Q", cpDecHeaders[0].ON_QuestionType);
			});

			DeclarationMessageHelper.CreateEntryQuestionsContent(cusEntryHeader, 7780, SoapMessageTextHelper.Constants.DocumentCodes.Code7780Desc, 0, "Soru");
			cpDecHeaders = cusEntryHeader.CPDecCollection;
			AssertEquals("Count", 1, cpDecHeaders.Count);

			DeclarationMessageHelper.CreateEntryQuestionsContent(cusEntryHeader, 5178, SoapMessageTextHelper.Constants.DocumentCodes.Code5178Desc, 0, "Soru");
			cpDecHeaders = cusEntryHeader.CPDecCollection;
			CombineAssertions("Questions by Header", () =>
			{
				AssertEquals("Count", 2, cpDecHeaders.Count);
				AssertEquals("2.CPDecs | ON_CPDecNum", 5178, cpDecHeaders[1].ON_CPDecNum);
				AssertEquals("2.CPDecs | Description", SoapMessageTextHelper.Constants.DocumentCodes.Code5178Desc, cpDecHeaders[1].Description);
				AssertEquals("2.CPDecs | ON_QuestionType", "Q", cpDecHeaders[1].ON_QuestionType);
			});

			DeclarationMessageHelper.CreateDocumentContent(cusEntryHeader, 1, "0100");
			var docs = invoiceLine.SupportingDocuments;
			CombineAssertions("Supporting Documents", () =>
			{
				AssertNotNull("Not Null", docs);
				AssertEquals("Count", 1, docs.Count);
				AssertEquals("1.Document | CSI_Code", "0100", docs[0].CSI_Code);
			});

			DeclarationMessageHelper.CreateDocumentContent(cusEntryHeader, 1, "0100");
			docs = invoiceLine.SupportingDocuments;
			AssertEquals("Count", 1, docs.Count);

			DeclarationMessageHelper.CreateDocumentContent(cusEntryHeader, 1, "0200");
			docs = invoiceLine.SupportingDocuments;
			CombineAssertions("Supporting Documents", () =>
			{
				AssertEquals("Count", 2, docs.Count);
				AssertEquals("2.Document | CSI_Code", "0200", docs[1].CSI_Code);
			});
		}

		public void TestCreateOrUpdateTaxes()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
			helper.CreateNewOrGetExistingCusCodeType("TRCUQ", "TurkeyCustomsQuestion", Core.Constants.CountryCodes.Turkey);
			helper.CreateNewOrGetExistingCusCodeType("TRCUW", "TurkeyCustomsWarning", Core.Constants.CountryCodes.Turkey);
			Factory.Save();

			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_GB = newBranch.PK;
			var cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			var cusEntryLine = jobDeclaration.CusEntryHeader.AllEntryLines.AddNew();
			cusEntryLine.CL_LineNumber = 1;

			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_CL = cusEntryLine.PK;

			var cpDecHeaders = cusEntryHeader.CPDecCollection;
			var fees = cusEntryLine.Fees;

			DeclarationMessageHelper.CreateOrUpdateTaxesContent(cusEntryHeader, 1, "10", 100, 10, "P", 1000);
			fees = cusEntryLine.Fees;
			CombineAssertions("Taxes | Create", () =>
			{
				AssertNotNull("Not Null", fees);
				AssertEquals("Count", 1, fees.Count);
				AssertEquals("1.Tax | CF_ChargeType", "10", fees[0].CF_ChargeType);
				AssertEquals("1.Tax | NationalFeeTypeCode", "10", fees[0].NationalFeeTypeCode);
				AssertEquals("1.Tax | CF_ChargeAmount", 100m, fees[0].CF_ChargeAmount);
				AssertEquals("1.Tax | CF_Rate", 10m, fees[0].CF_Rate);
				AssertEquals("1.Tax | CF_MethodOfPayment", "P", fees[0].CF_MethodOfPayment);
				AssertEquals("1.Tax | CF_BaseValue", 1000m, fees[0].CF_BaseValue);
				AssertEquals("1.Tax | CF_Source", "CW1", fees[0].CF_Source);
				AssertEquals("1.Tax | CF_RateOverrideReasonCode", "ADD", fees[0].CF_RateOverrideReasonCode);
				AssertEquals("1.Tax | CF_MethodOfCalculation", "Gümrük", fees[0].CF_MethodOfCalculation);
			});

			DeclarationMessageHelper.CreateOrUpdateTaxesContent(cusEntryHeader, 1, "10", 200, 20, "C", 1000);
			fees = cusEntryLine.Fees;
			CombineAssertions("Taxes | Update", () =>
			{
				AssertEquals("Count", 1, fees.Count);
				AssertEquals("1.Tax | CF_ChargeType", "10", fees[0].CF_ChargeType);
				AssertEquals("1.Tax | NationalFeeTypeCode", "10", fees[0].NationalFeeTypeCode);
				AssertEquals("1.Tax | CF_ChargeAmount", 200m, fees[0].CF_ChargeAmount);
				AssertEquals("1.Tax | CF_Rate", 20m, fees[0].CF_Rate);
				AssertEquals("1.Tax | CF_MethodOfPayment", "C", fees[0].CF_MethodOfPayment);
				AssertEquals("1.Tax | CF_BaseValue", 1000m, fees[0].CF_BaseValue);
				AssertEquals("1.Tax | CF_Source", "CW1", fees[0].CF_Source);
				AssertEquals("1.Tax | CF_RateOverrideReasonCode", "OVR", fees[0].CF_RateOverrideReasonCode);
				AssertEquals("1.Tax | CF_MethodOfCalculation", "Gümrük", fees[0].CF_MethodOfCalculation);
			});

			DeclarationMessageHelper.CreateOrUpdateTaxesContent(cusEntryHeader, 1, "40", 100, 10, "P", 1000);
			fees = cusEntryLine.Fees;
			CombineAssertions("Taxes | Create Second Tax", () =>
			{
				AssertEquals("Count", 2, fees.Count);
				AssertEquals("2.Tax | CF_ChargeType", "B00", fees[1].CF_ChargeType);
				AssertEquals("2.Tax | NationalFeeTypeCode", "40", fees[1].NationalFeeTypeCode);
				AssertEquals("2.Tax | CF_ChargeAmount", 100m, fees[1].CF_ChargeAmount);
				AssertEquals("2.Tax | CF_Rate", 10m, fees[1].CF_Rate);
				AssertEquals("2.Tax | CF_MethodOfPayment", "P", fees[1].CF_MethodOfPayment);
				AssertEquals("2.Tax | CF_BaseValue", 1000m, fees[1].CF_BaseValue);
				AssertEquals("2.Tax | CF_Source", "CW1", fees[1].CF_Source);
				AssertEquals("2.Tax | CF_RateOverrideReasonCode", "ADD", fees[1].CF_RateOverrideReasonCode);
				AssertEquals("2.Tax | CF_MethodOfCalculation", "Gümrük", fees[1].CF_MethodOfCalculation);
			});

			DeclarationMessageHelper.CreateOrUpdateTaxesContent(cusEntryHeader, 1, "40", 150, 15, "P", 1500);
			fees = cusEntryLine.Fees;
			CombineAssertions("Taxes | Create Second Tax", () =>
			{
				AssertEquals("Count", 2, fees.Count);
				AssertEquals("2.Tax | CF_ChargeType", "B00", fees[1].CF_ChargeType);
				AssertEquals("2.Tax | NationalFeeTypeCode", "40", fees[1].NationalFeeTypeCode);
				AssertEquals("2.Tax | CF_ChargeAmount", 150m, fees[1].CF_ChargeAmount);
				AssertEquals("2.Tax | CF_Rate", 15m, fees[1].CF_Rate);
				AssertEquals("2.Tax | CF_MethodOfPayment", "P", fees[1].CF_MethodOfPayment);
				AssertEquals("2.Tax | CF_BaseValue", 1500m, fees[1].CF_BaseValue);
				AssertEquals("2.Tax | CF_Source", "CW1", fees[1].CF_Source);
				AssertEquals("2.Tax | CF_RateOverrideReasonCode", "OVR", fees[1].CF_RateOverrideReasonCode);
				AssertEquals("2.Tax | CF_MethodOfCalculation", "Gümrük", fees[1].CF_MethodOfCalculation);
			});
		}

		public void TestCreateOrUpdateLinkedInvoiceLines()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
			helper.CreateNewOrGetExistingCusCodeType("TRCUQ", "TurkeyCustomsQuestion", Core.Constants.CountryCodes.Turkey);
			helper.CreateNewOrGetExistingCusCodeType("TRCUW", "TurkeyCustomsWarning", Core.Constants.CountryCodes.Turkey);
			Factory.Save();

			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			jobDeclaration.JE_GB = newBranch.PK;
			var cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			invoiceHeader.JZ_IncoTerm = "CIF";
			invoiceHeader.JZ_InvoiceAmount = 9000;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "TRY";

			var cusEntryLine = jobDeclaration.CusEntryHeader.MergedLines.AddNew();
			cusEntryLine.CL_LineNumber = 1;
			var chargeTypeCode = "10";

			#region Invoice Lines

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = cusEntryLine.PK;
			invoiceLine1.JI_LineNo = 1;
			invoiceLine1.JI_LinePrice = 4000;

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = cusEntryLine.PK;
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.JI_LinePrice = 3000;

			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_CL = cusEntryLine.PK;
			invoiceLine3.JI_LineNo = 3;
			invoiceLine3.JI_LinePrice = 2000;

			#endregion

			#region Invoice Taxes

			var invoiceTax1 = invoiceLine1.Taxes.AddNew();
			invoiceTax1.JLT_Type = chargeTypeCode;
			invoiceTax1.NationalType = chargeTypeCode;
			invoiceTax1.JLT_BaseValue = 400;
			invoiceTax1.JLT_Rate = 10;
			invoiceTax1.JLT_Amount = 40;
			invoiceTax1.JLT_MethodOfPayment = "P";
			invoiceTax1.JLT_MethodOfCalculation = "CW1";
			invoiceTax1.JLT_RateOverrideReasonCode = "ADD";

			var invoiceTax2 = invoiceLine2.Taxes.AddNew();
			invoiceTax2.JLT_Type = chargeTypeCode;
			invoiceTax2.NationalType = chargeTypeCode;
			invoiceTax2.JLT_BaseValue = 300;
			invoiceTax2.JLT_Rate = 10;
			invoiceTax2.JLT_Amount = 30;
			invoiceTax2.JLT_MethodOfPayment = "P";
			invoiceTax2.JLT_MethodOfCalculation = "CW1";
			invoiceTax2.JLT_RateOverrideReasonCode = "ADD";

			var invoiceTax3 = invoiceLine3.Taxes.AddNew();
			invoiceTax3.JLT_Type = chargeTypeCode;
			invoiceTax3.NationalType = chargeTypeCode;
			invoiceTax3.JLT_BaseValue = 200;
			invoiceTax3.JLT_Rate = 10;
			invoiceTax3.JLT_Amount = 20;
			invoiceTax3.JLT_MethodOfPayment = "P";
			invoiceTax3.JLT_MethodOfCalculation = "CW1";
			invoiceTax3.JLT_RateOverrideReasonCode = "ADD";

			#endregion

			var merger = new LineMerger(jobDeclaration);
			merger.DoMerge();

			var entryLineFee = (CusEntryLineFee)cusEntryLine.Fees.AddNew();
			entryLineFee.CF_ChargeType = chargeTypeCode;
			entryLineFee.NationalFeeTypeCode = chargeTypeCode;
			entryLineFee.CF_BaseValue = 900m;
			entryLineFee.CF_Rate = 10;
			entryLineFee.CF_ChargeAmount = 90m;
			entryLineFee.CF_MethodOfPayment = "C";

			CombineAssertions("Entry Line Fee | First", () =>
			{
				AssertEquals("CF_BaseValue", 900m, entryLineFee.CF_BaseValue);
				AssertEquals("CF_Rate", 10m, entryLineFee.CF_Rate);
				AssertEquals("CF_ChargeAmount", 90m, entryLineFee.CF_ChargeAmount);
			});

			var updatedTax1 = invoiceLine1.Taxes.Cast<JobComInvoiceLineTax>().FirstOrDefault();
			var updatedTax2 = invoiceLine2.Taxes.Cast<JobComInvoiceLineTax>().FirstOrDefault();
			var updatedTax3 = invoiceLine3.Taxes.Cast<JobComInvoiceLineTax>().FirstOrDefault();

			CombineAssertions("Taxes Invoice Line | First", () =>
			{
				AssertEquals("Line 1 | JLT_BaseValue", 400m, updatedTax1.JLT_BaseValue);
				AssertEquals("Line 1 | JLT_Rate", 10m, updatedTax1.JLT_Rate);
				AssertEquals("Line 1 | JLT_Amount", 40m, updatedTax1.JLT_Amount);

				AssertEquals("Line 2 | JLT_BaseValue", 300m, updatedTax2.JLT_BaseValue);
				AssertEquals("Line 2 | JLT_Rate", 10m, updatedTax2.JLT_Rate);
				AssertEquals("Line 2 | JLT_Amount", 30m, updatedTax2.JLT_Amount);

				AssertEquals("Line 3 | JLT_BaseValue", 200m, updatedTax3.JLT_BaseValue);
				AssertEquals("Line 3 | JLT_Rate", 10m, updatedTax3.JLT_Rate);
				AssertEquals("Line 3 | JLT_Amount", 20m, updatedTax3.JLT_Amount);

				AssertEquals("Total | Value", entryLineFee.CF_BaseValue, updatedTax1.JLT_BaseValue + updatedTax2.JLT_BaseValue + updatedTax3.JLT_BaseValue);
				AssertEquals("Total | Amount", entryLineFee.CF_ChargeAmount, updatedTax1.JLT_Amount + updatedTax2.JLT_Amount + updatedTax3.JLT_Amount);
			});

			entryLineFee.CF_BaseValue = 1350m;
			entryLineFee.CF_Rate = 10;
			entryLineFee.CF_ChargeAmount = 135m;
			entryLineFee.CF_MethodOfPayment = "C";

			DeclarationMessageHelper.CreateOrUpdateLinkedInvoiceLines(entryLineFee);
			updatedTax1 = invoiceLine1.Taxes.Cast<JobComInvoiceLineTax>().FirstOrDefault();
			updatedTax2 = invoiceLine2.Taxes.Cast<JobComInvoiceLineTax>().FirstOrDefault();
			updatedTax3 = invoiceLine3.Taxes.Cast<JobComInvoiceLineTax>().FirstOrDefault();

			CombineAssertions("Taxes Invoice Line | Changed %50", () =>
			{
				AssertEquals("Line 1 | JLT_BaseValue", 600m, updatedTax1.JLT_BaseValue);
				AssertEquals("Line 1 | JLT_Rate", 10m, updatedTax1.JLT_Rate);
				AssertEquals("Line 1 | JLT_Amount", 60m, updatedTax1.JLT_Amount);

				AssertEquals("Line 2 | JLT_BaseValue", 450m, updatedTax2.JLT_BaseValue);
				AssertEquals("Line 2 | JLT_Rate", 10m, updatedTax2.JLT_Rate);
				AssertEquals("Line 2 | JLT_Amount", 45m, updatedTax2.JLT_Amount);

				AssertEquals("Line 3 | JLT_BaseValue", 300m, updatedTax3.JLT_BaseValue);
				AssertEquals("Line 3 | JLT_Rate", 10m, updatedTax3.JLT_Rate);
				AssertEquals("Line 3 | JLT_Amount", 30m, updatedTax3.JLT_Amount);

				AssertEquals("Total | Base Value", entryLineFee.CF_BaseValue, updatedTax1.JLT_BaseValue + updatedTax2.JLT_BaseValue + updatedTax3.JLT_BaseValue);
				AssertEquals("Total | Amount", entryLineFee.CF_ChargeAmount, updatedTax1.JLT_Amount + updatedTax2.JLT_Amount + updatedTax3.JLT_Amount);
			});

			entryLineFee.CF_BaseValue = ZDecimal.Zero;
			entryLineFee.CF_Rate = 10;
			entryLineFee.CF_ChargeAmount = 600m;

			DeclarationMessageHelper.CreateOrUpdateLinkedInvoiceLines(entryLineFee);
			updatedTax1 = invoiceLine1.Taxes.Cast<JobComInvoiceLineTax>().FirstOrDefault();
			updatedTax2 = invoiceLine2.Taxes.Cast<JobComInvoiceLineTax>().FirstOrDefault();
			updatedTax3 = invoiceLine3.Taxes.Cast<JobComInvoiceLineTax>().FirstOrDefault();

			CombineAssertions("Taxes Invoice Line | Base Amount is Zero", () =>
			{
				AssertEquals("Line 1 | JLT_BaseValue", 2666.67m, updatedTax1.JLT_BaseValue);
				AssertEquals("Line 1 | JLT_Rate", 10m, updatedTax1.JLT_Rate);
				AssertEquals("Line 1 | JLT_Amount", 266.67m, updatedTax1.JLT_Amount);

				AssertEquals("Line 2 | JLT_BaseValue", 2000m, updatedTax2.JLT_BaseValue);
				AssertEquals("Line 2 | JLT_Rate", 10m, updatedTax2.JLT_Rate);
				AssertEquals("Line 2 | JLT_Amount", 200m, updatedTax2.JLT_Amount);

				AssertEquals("Line 3 | JLT_BaseValue", 1333.33m, updatedTax3.JLT_BaseValue);
				AssertEquals("Line 3 | JLT_Rate", 10m, updatedTax3.JLT_Rate);
				AssertEquals("Line 3 | JLT_Amount", 133.33m, updatedTax3.JLT_Amount);

				AssertEquals("Total | Base Value", 6000m, updatedTax1.JLT_BaseValue + updatedTax2.JLT_BaseValue + updatedTax3.JLT_BaseValue);
				AssertEquals("Total | Amount", entryLineFee.CF_ChargeAmount, updatedTax1.JLT_Amount + updatedTax2.JLT_Amount + updatedTax3.JLT_Amount);
			});

			entryLineFee.CF_BaseValue = ZDecimal.Zero;
			entryLineFee.CF_Rate = ZDecimal.Zero;
			entryLineFee.CF_ChargeAmount = 600m;

			DeclarationMessageHelper.CreateOrUpdateLinkedInvoiceLines(entryLineFee);
			updatedTax1 = invoiceLine1.Taxes.Cast<JobComInvoiceLineTax>().FirstOrDefault();
			updatedTax2 = invoiceLine2.Taxes.Cast<JobComInvoiceLineTax>().FirstOrDefault();
			updatedTax3 = invoiceLine3.Taxes.Cast<JobComInvoiceLineTax>().FirstOrDefault();

			CombineAssertions("Taxes Invoice Line | Only charge Amount", () =>
			{
				AssertEquals("Line 1 | JLT_BaseValue", 0m, updatedTax1.JLT_BaseValue);
				AssertEquals("Line 1 | JLT_Rate", 0m, updatedTax1.JLT_Rate);
				AssertEquals("Line 1 | JLT_Amount", 266.67m, updatedTax1.JLT_Amount);

				AssertEquals("Line 2 | JLT_BaseValue", 0m, updatedTax2.JLT_BaseValue);
				AssertEquals("Line 2 | JLT_Rate", 0m, updatedTax2.JLT_Rate);
				AssertEquals("Line 2 | JLT_Amount", 200m, updatedTax2.JLT_Amount);

				AssertEquals("Line 3 | JLT_BaseValue", 0m, updatedTax3.JLT_BaseValue);
				AssertEquals("Line 3 | JLT_Rate", 0m, updatedTax3.JLT_Rate);
				AssertEquals("Line 3 | JLT_Amount", 133.33m, updatedTax3.JLT_Amount);

				AssertEquals("Total | Base Value", 0m, updatedTax1.JLT_BaseValue + updatedTax2.JLT_BaseValue + updatedTax3.JLT_BaseValue);
				AssertEquals("Total | Rate", 0m, updatedTax1.JLT_Rate + updatedTax2.JLT_Rate + updatedTax3.JLT_Rate);
				AssertEquals("Total | Amount", entryLineFee.CF_ChargeAmount, updatedTax1.JLT_Amount + updatedTax2.JLT_Amount + updatedTax3.JLT_Amount);
			});

			entryLineFee.CF_BaseValue = 3331m;
			entryLineFee.CF_Rate = 10;
			entryLineFee.CF_ChargeAmount = 333.1m;

			DeclarationMessageHelper.CreateOrUpdateLinkedInvoiceLines(entryLineFee);
			updatedTax1 = invoiceLine1.Taxes.Cast<JobComInvoiceLineTax>().FirstOrDefault();
			updatedTax2 = invoiceLine2.Taxes.Cast<JobComInvoiceLineTax>().FirstOrDefault();
			updatedTax3 = invoiceLine3.Taxes.Cast<JobComInvoiceLineTax>().FirstOrDefault();

			CombineAssertions("Taxes Invoice Line | Adding the remaining value at last record", () =>
			{
				AssertEquals("Line 1 | JLT_BaseValue", 1480.44m, updatedTax1.JLT_BaseValue);
				AssertEquals("Line 1 | JLT_Rate", 10m, updatedTax1.JLT_Rate);
				AssertEquals("Line 1 | JLT_Amount", 148.04m, updatedTax1.JLT_Amount);

				AssertEquals("Line 2 | JLT_BaseValue", 1480.44m, updatedTax1.JLT_BaseValue);
				AssertEquals("Line 2 | JLT_Rate", 10m, updatedTax2.JLT_Rate);
				AssertEquals("Line 2 | JLT_Amount", 111.03m, updatedTax2.JLT_Amount);

				AssertEquals("Line 3 | JLT_BaseValue", 740.23m, updatedTax3.JLT_BaseValue);
				AssertEquals("Line 3 | JLT_Rate", 10m, updatedTax3.JLT_Rate);
				AssertEquals("Line 3 | JLT_Amount", 74.03m, updatedTax3.JLT_Amount);

				AssertEquals("Total | Value", entryLineFee.CF_BaseValue, updatedTax1.JLT_BaseValue + updatedTax2.JLT_BaseValue + updatedTax3.JLT_BaseValue);
				AssertEquals("Total | Amount", entryLineFee.CF_ChargeAmount, updatedTax1.JLT_Amount + updatedTax2.JLT_Amount + updatedTax3.JLT_Amount);
			});
		}

		static void AssertState(string messageType, string messageStatus, string currentEntryStatus, string expectedEntryStatus, CusEntryHeader cusEntryHeader)
		{
			var entryStatus = DeclarationMessageHelper.GetEntryStatus(cusEntryHeader, messageType);
			AssertEquals($"Case: {messageType}.{messageStatus} => {currentEntryStatus}", expectedEntryStatus, entryStatus);
		}
	}
}
