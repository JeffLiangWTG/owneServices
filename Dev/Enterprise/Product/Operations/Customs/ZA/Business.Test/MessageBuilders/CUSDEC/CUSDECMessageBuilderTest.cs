using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.ZA.Business.Business.Utilities;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	sealed class CUSDECMessageBuilderTest : TestCaseWithFactory
	{
		public void TestSupplier()
		{
			var testSupplier = Factory.NewWithValidTestData<OrgHeader>();
			testSupplier.MainAddress.Address1 = "SUPPLIERADDR1";
			testSupplier.CustomsCodes.AddNew("CSC", "CSC", "ZA");

			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			dec.JE_HouseBill = "12345678";
			dec.HouseBillIssuedDate = ZDate.BrettsBirthday;
			dec.JE_OH_Supplier = testSupplier.PK;

			var entry = dec.CustomsEntryHeaders.AddNew();
			var line = entry.MergedLines.AddNew();

			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = line.PK;

			CombineAssertions("With VDN", () =>
			{
				invoice.JZ_RelatedIndicator = RelatedIndicatorList.Codes.Yes;
				invoice.JZ_VDN = "112233";
				var testSendingObject = new MessageSendingObject(entry)
				{
					MessageType = MessageSubTypeCodes.Codes.Original
				};
				var builder = new CUSDECMessageBuilder(testSendingObject, MessageSubTypes.Create);
				var result = builder.PopulateMessages().GetBuilderResults().ToArray();
				AssertEquals(1, result.Length);
				AssertContains("NAD+SU+CSC++", result[0].Message.EM_MessageText);
			});

			CombineAssertions("Empty VDN", () =>
			{
				invoice.JZ_RelatedIndicator = RelatedIndicatorList.Codes.No;
				invoice.JZ_VDN = "";
				var testSendingObject = new MessageSendingObject(entry)
				{
					MessageType = MessageSubTypeCodes.Codes.Original
				};
				var builder = new CUSDECMessageBuilder(testSendingObject, MessageSubTypes.Create);
				var result = builder.PopulateMessages().GetBuilderResults().ToArray();
				AssertEquals(1, result.Length);
				AssertContains("NAD+SU+++", result[0].Message.EM_MessageText);
			});

			CombineAssertions("With VDN but no CSC code", () =>
			{
				invoice.JZ_RelatedIndicator = RelatedIndicatorList.Codes.Yes;
				invoice.JZ_VDN = "112233";
				testSupplier.CustomsCodes.RemoveAndDeleteAll();
				var testSendingObject = new MessageSendingObject(entry)
				{
					MessageType = MessageSubTypeCodes.Codes.Original
				};
				var builder = new CUSDECMessageBuilder(testSendingObject, MessageSubTypes.Create);
				var result = builder.PopulateMessages().GetBuilderResults().ToArray();
				AssertEquals(1, result.Length);
				AssertContains("NAD+SU+++", result[0].Message.EM_MessageText);
			});
		}

		public void TestGetMessageSubType()
		{
			var dummyHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var messagesendingobject = new MessageSendingObject(dummyHeader);

			CombineAssertions(() =>
			{
				var tester = new CUSDECMessageBuilderForTest(messagesendingobject, MessageSubTypes.Create);
				AssertEquals(MessageSubTypeCodes.Codes.Original, tester.GetMessageSubType_Exposed());
				tester = new CUSDECMessageBuilderForTest(messagesendingobject, MessageSubTypes.Replace);
				AssertEquals(MessageSubTypeCodes.Codes.Replace, tester.GetMessageSubType_Exposed());
				tester = new CUSDECMessageBuilderForTest(messagesendingobject, MessageSubTypes.Change);
				AssertEquals(MessageSubTypeCodes.Codes.Change, tester.GetMessageSubType_Exposed());
				tester = new CUSDECMessageBuilderForTest(messagesendingobject, MessageSubTypes.Withdraw);
				AssertEquals(MessageSubTypeCodes.Codes.Cancellation, tester.GetMessageSubType_Exposed());
			});
		}

		public void TestHouseBillAndDate()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			dec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			dec.JE_HouseBill = "12345678";
			dec.JE_CargoCarrier = "76543210";
			dec.HouseBillIssuedDate = ZDate.BrettsBirthday;

			var entry = dec.CustomsEntryHeaders.AddNew();
			var testSendingObject = new MessageSendingObject(entry);
			testSendingObject.MessageType = MessageSubTypeCodes.Codes.Original;

			var builder = new CUSDECMessageBuilder(testSendingObject, MessageSubTypes.Create);
			var result = builder.PopulateMessages().GetBuilderResults().ToArray();
			AssertEquals(1, result.Length);
			AssertContains("'RFF+BH:7654321012345678'", result[0].Message.EM_MessageText);
			AssertContains("'DTM+137:" + ZDate.BrettsBirthday.ToString("yyyyMMdd") + ":102'", result[0].Message.EM_MessageText);
		}

		public void TestVesselAgent()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			dec.JE_VesselAgent = "AMAL";

			var entry = dec.CustomsEntryHeaders.AddNew();
			var testSendingObject = new MessageSendingObject(entry);
			testSendingObject.MessageType = MessageSubTypeCodes.Codes.Original;

			using (TDTSegmentSplitHelper.TemporarilyEnableTDTSegmentSplit(true))
			{
				var builder = new CUSDECMessageBuilder(testSendingObject, MessageSubTypes.Create);
				var result = builder.PopulateMessages().GetBuilderResults().ToArray();
				AssertEquals(1, result.Length);
				AssertContains("NAD+CG+AMAL", result[0].Message.EM_MessageText);
			}

			using (TDTSegmentSplitHelper.TemporarilyEnableTDTSegmentSplit(false))
			{
				var builder = new CUSDECMessageBuilder(testSendingObject, MessageSubTypes.Create);
				var result = builder.PopulateMessages().GetBuilderResults().ToArray();
				AssertEquals(1, result.Length);
				AssertNotContains("NAD+CG+AMAL", result[0].Message.EM_MessageText);
			}
		}

		public void TestMasterCargoCarrier()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			dec.JE_CarrierCode = "MSC";

			var entry = dec.CustomsEntryHeaders.AddNew();
			var testSendingObject = new MessageSendingObject(entry);
			testSendingObject.MessageType = MessageSubTypeCodes.Codes.Original;

			using (TDTSegmentSplitHelper.TemporarilyEnableTDTSegmentSplit(true))
			{
				var builder = new CUSDECMessageBuilder(testSendingObject, MessageSubTypes.Create);
				var result = builder.PopulateMessages().GetBuilderResults().ToArray();
				AssertEquals(1, result.Length);
				AssertContains("NAD+RL+MSC", result[0].Message.EM_MessageText);
			}

			using (TDTSegmentSplitHelper.TemporarilyEnableTDTSegmentSplit(false))
			{
				var builder = new CUSDECMessageBuilder(testSendingObject, MessageSubTypes.Create);
				var result = builder.PopulateMessages().GetBuilderResults().ToArray();
				AssertEquals(1, result.Length);
				AssertNotContains("NAD+RL+MSC", result[0].Message.EM_MessageText);
			}
		}

		public void TestTransportDocumentNumber()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			dec.JE_MasterBill = "MSCU123450";
			dec.JE_CarrierCode = "MSC";

			var entry = dec.CustomsEntryHeaders.AddNew();
			var testSendingObject = new MessageSendingObject(entry);
			testSendingObject.MessageType = MessageSubTypeCodes.Codes.Original;

			using (TDTSegmentSplitHelper.TemporarilyEnableTDTSegmentSplit(true))
			{
				var builder = new CUSDECMessageBuilder(testSendingObject, MessageSubTypes.Create);
				var result = builder.PopulateMessages().GetBuilderResults().ToArray();
				AssertEquals(1, result.Length);
				AssertContains("RFF+AAS:MSCU123450", result[0].Message.EM_MessageText);
			}

			using (TDTSegmentSplitHelper.TemporarilyEnableTDTSegmentSplit(false))
			{
				var builder = new CUSDECMessageBuilder(testSendingObject, MessageSubTypes.Create);
				var result = builder.PopulateMessages().GetBuilderResults().ToArray();
				AssertEquals(1, result.Length);
				AssertContains("RFF+AAS:MSC MSCU123450", result[0].Message.EM_MessageText);
			}
		}

		public void TestVOCReason()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			var testSendingObject = new MessageSendingObject(entry);
			testSendingObject.MessageType = MessageSubTypeCodes.Codes.Change;
			testSendingObject.VOCReason = "TestReason";

			Messaging.MessageBuilders.IMessageBuilder builder = new CUSDECMessageBuilder(testSendingObject, MessageSubTypes.Change);
			var result = builder.PopulateMessages().GetBuilderResults();
			AssertEquals(1, result.Count());
			foreach (var item in result)
			{
				AssertEquals("TestReason", (item.Message as ZAMessage).VOCReason);
			}
		}

		public void TestCopyVPBAmounts()
		{
			CombineAssertions("TestOriginal", () =>
			{
				var dec = Factory.NewWithValidTestData<JobDeclaration>();
				var entry = dec.CustomsEntryHeaders.AddNew();
				var line1 = entry.MergedLines.AddNew();
				line1.CL_LineNumber = 2;
				line1.CL_VPBAmount = 100m;
				var line2 = entry.MergedLines.AddNew();
				line2.CL_LineNumber = 1;
				line2.CL_VPBAmount = 200m;
				var testSendingObject = new MessageSendingObject(entry);
				testSendingObject.MessageType = MessageSubTypeCodes.Codes.Original;

				Messaging.MessageBuilders.IMessageBuilder builder = new CUSDECMessageBuilder(testSendingObject, MessageSubTypes.Create);
				var result = builder.PopulateMessages().GetBuilderResults();
				AssertEquals(1, result.Count());
				var message = result.ElementAt(0).Message as CUSDECEDIMessage;
				AssertEquals("Count", 2, message.VPBAmounts.Count);
				AssertEquals("TestLine1", 200m, message.VPBAmounts.OfType<VPBAmountCodeData>().FirstOrDefault(x => x.CY_Code == "Line1").CY_Value);
				AssertEquals("TestLine2", 100m, message.VPBAmounts.OfType<VPBAmountCodeData>().FirstOrDefault(x => x.CY_Code == "Line2").CY_Value);
			});
			CombineAssertions("TestReplace", () =>
			{
				var dec = Factory.NewWithValidTestData<JobDeclaration>();
				var entry = dec.CustomsEntryHeaders.AddNew();
				var line1 = entry.MergedLines.AddNew();
				line1.CL_LineNumber = 2;
				line1.CL_VPBAmount = 100m;
				var line2 = entry.MergedLines.AddNew();
				line2.CL_LineNumber = 1;
				line2.CL_VPBAmount = 200m;
				var testSendingObject = new MessageSendingObject(entry);
				testSendingObject.MessageType = MessageSubTypeCodes.Codes.Replace;

				Messaging.MessageBuilders.IMessageBuilder builder = new CUSDECMessageBuilder(testSendingObject, MessageSubTypes.Replace);
				var result = builder.PopulateMessages().GetBuilderResults();
				AssertEquals(1, result.Count());
				var message = result.ElementAt(0).Message as CUSDECEDIMessage;
				AssertEquals("Count", 2, message.VPBAmounts.Count);
				AssertEquals("TestLine1", 200m, message.VPBAmounts.OfType<VPBAmountCodeData>().FirstOrDefault(x => x.CY_Code == "Line1").CY_Value);
				AssertEquals("TestLine2", 100m, message.VPBAmounts.OfType<VPBAmountCodeData>().FirstOrDefault(x => x.CY_Code == "Line2").CY_Value);
			});
			CombineAssertions("TestChange", () =>
			{
				var dec = Factory.NewWithValidTestData<JobDeclaration>();
				var entry = dec.CustomsEntryHeaders.AddNew();
				var line1 = entry.MergedLines.AddNew();
				line1.CL_LineNumber = 2;
				line1.CL_VPBAmount = 100m;
				var line2 = entry.MergedLines.AddNew();
				line2.CL_LineNumber = 1;
				line2.CL_VPBAmount = 200m;
				var testSendingObject = new MessageSendingObject(entry);
				testSendingObject.MessageType = MessageSubTypeCodes.Codes.Change;

				Messaging.MessageBuilders.IMessageBuilder builder = new CUSDECMessageBuilder(testSendingObject, MessageSubTypes.Change);
				var result = builder.PopulateMessages().GetBuilderResults();
				AssertEquals(1, result.Count());
				var message = result.ElementAt(0).Message as CUSDECEDIMessage;
				AssertEquals("Count", 0, message.VPBAmounts.Count);
			});
			CombineAssertions("TestCancel", () =>
			{
				var dec = Factory.NewWithValidTestData<JobDeclaration>();
				var entry = dec.CustomsEntryHeaders.AddNew();
				var line1 = entry.MergedLines.AddNew();
				line1.CL_LineNumber = 2;
				line1.CL_VPBAmount = 100m;
				var line2 = entry.MergedLines.AddNew();
				line2.CL_LineNumber = 1;
				line2.CL_VPBAmount = 200m;
				var testSendingObject = new MessageSendingObject(entry);
				testSendingObject.MessageType = MessageSubTypeCodes.Codes.Cancellation;

				Messaging.MessageBuilders.IMessageBuilder builder = new CUSDECMessageBuilder(testSendingObject, MessageSubTypes.Withdraw);
				var result = builder.PopulateMessages().GetBuilderResults();
				AssertEquals(1, result.Count());
				var message = result.ElementAt(0).Message as CUSDECEDIMessage;
				AssertEquals("Count", 0, message.VPBAmounts.Count);
			});
		}

		public void TestMessageContentEscapedWithCorrectCharacterSet()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			dec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			dec.JE_HouseBill = "123-+':?-789";
			dec.HouseBillIssuedDate = ZDate.BrettsBirthday;

			var entry = dec.CustomsEntryHeaders.AddNew();
			var testSendingObject = new MessageSendingObject(entry);
			testSendingObject.MessageType = MessageSubTypeCodes.Codes.Original;

			var builder = new CUSDECMessageBuilder(testSendingObject, MessageSubTypes.Create);
			var result = builder.PopulateMessages().GetBuilderResults().ToArray();
			AssertEquals(1, result.Length);
			AssertContains("'RFF+BH:123-?+?'?:??-789'", result[0].Message.EM_MessageText);
		}

		public void TestRelationshipIndicator_and_ValuationCode()
		{
			CreateReferenceData_for_TestingValuationCode_for_ZA();

			const string Import = ZAJobMessageTypeList.Codes.Import;
			const string Export = ZAJobMessageTypeList.Codes.Export;

			const string Related = RelatedIndicatorList.Codes.Yes;
			const string NotRelated = RelatedIndicatorList.Codes.No;
			const string Exempt = RelatedIndicatorList.Codes.Exempt;

			var list = new RelationshipIndicator_and_ValuationCode_TestCase_Collection(Factory);

			list.Add(Import, "14", Exempt, true);
			list.Add(Import, "11", NotRelated, true);
			list.Add(Import, "12", NotRelated, false);
			list.Add(Import, "20", NotRelated, false);
			list.Add(Import, "21", NotRelated, false);
			list.Add(Import, "22", NotRelated, false);
			list.Add(Import, "37", NotRelated, false);
			list.Add(Import, "78", NotRelated, false);
			list.Add(Export, "60", Related, false);

			list.TestAllTestCases();
		}

		public void TestInclude_MRN_to_be_Replaced()
		{
			const string Cancellation = MessageSubTypeCodes.Codes.Cancellation;   // "CNL";
			const string Change = MessageSubTypeCodes.Codes.Change;         // "CHG";
			const string Original = MessageSubTypeCodes.Codes.Original;       // "ORG";
			const string Replace = MessageSubTypeCodes.Codes.Replace;        // "REP";

			var scenarios = new MrnToBeReplaced_TestCase_Collection();

			scenarios.Add("01", Cancellation, mustOutputOriginalMRN: true, mustOutputMRNToBeReplaced: true);
			scenarios.Add("02", Change, mustOutputOriginalMRN: true, mustOutputMRNToBeReplaced: true);
			scenarios.Add("03", Original, mustOutputOriginalMRN: false, mustOutputMRNToBeReplaced: false);
			scenarios.Add("04", Replace, mustOutputOriginalMRN: false, mustOutputMRNToBeReplaced: true);

			scenarios.TestAll(Factory);
		}

		public void TestSupplierCode()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMP01";
			importer.OH_FullName = "Importer Name";
			importer.MainAddress.Address1 = "IMP_ADDR";
			importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "CUSTIMP", "ZA");

			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_OH_Importer = importer.PK;
			dec.JE_OA_ImporterAddress = importer.MainAddress.PK;
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			dec.JE_HouseBill = "12345678";
			dec.HouseBillIssuedDate = new ZDateTime(2020, 05, 05);

			dec.Invoices.RemoveAll();
			dec.Invoices.AddNew();
			dec.Invoices.AddNew();

			int counter = 0;

			foreach (JobComInvoiceHeader invoice in dec.Invoices)
			{
				var sCounter = string.Format(CultureInfo.InvariantCulture, "{0}", ++counter);

				var supplier = Factory.New<OrgHeader>();
				supplier.OH_Code = "SUP" + sCounter;
				supplier.OH_FullName = "Supplier Name " + sCounter;
				supplier.MainAddress.Address1 = "SUPP_ADDR_" + sCounter;

				invoice.JZ_InvoiceNumber = "INV-" + sCounter;
				invoice.JZ_InvoiceDate = new ZDateTime(2020, 05, counter);
				invoice.JZ_InvoiceAmount = 2000 * counter;
				invoice.JZ_RX_NKInvoice_Currency = "ZAR";
				invoice.JZ_OH_Supplier = supplier.PK;
				invoice.JZ_OA_SupplierAddress = supplier.MainAddress.PK;
				invoice.JZ_OH_Buyer = importer.PK;
				invoice.JZ_OA_BuyerAddress = importer.MainAddress.PK;
				invoice.JZ_RelatedIndicator = RelatedIndicatorList.Codes.Yes;

				var invLine1 = invoice.JobComInvoiceLines.AddNew();
				invLine1.JI_LinePrice = 1000 * counter;

				var invLine2 = invoice.JobComInvoiceLines.AddNew();
				invLine2.JI_LinePrice = 1000 * counter;
			}

			{
				var invoice01 = dec.Invoices[0];
				invoice01.JZ_VDN = "997788";

				var supplier01 = invoice01.Supplier;
				supplier01.CustomsCodes.AddNew("CSC", "MAINCSC", "ZA");

				dec.JE_OH_Supplier = supplier01.PK;
				dec.JE_OA_SupplierAddress = supplier01.MainAddress.PK;
			}

			{
				var invoice02 = dec.Invoices[1];
				var invLine01 = invoice02.JobComInvoiceLines[0];
				var invLine02 = invoice02.JobComInvoiceLines[1];

				var instr = dec.CustomsEntryInstructions.AddNew();
				instr.CEI_Style = "11";

				var entry = dec.CustomsEntryHeaders.AddNew();
				entry.CH_CEI_Instruction = instr.PK;

				var entryLine1 = entry.MergedLines.AddNew();
				invLine01.JI_CL = entryLine1.PK;

				var entryLine2 = entry.MergedLines.AddNew();
				invLine02.JI_CL = entryLine2.PK;
			}

			Create_SupplierCode_TestCases().TestAll(dec);
		}

		[TestDate(2022, 11, 14)]
		public void TestICustomsMessageGenerator()
		{
			var testSupplier = Factory.NewWithValidTestData<OrgHeader>();
			testSupplier.MainAddress.Address1 = "SUPPLIERADDR1";
			testSupplier.CustomsCodes.AddNew("CSC", "CSC", "ZA");

			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			dec.JE_HouseBill = "12345678";
			dec.HouseBillIssuedDate = ZDate.BrettsBirthday;
			dec.JE_OH_Supplier = testSupplier.PK;

			var entry = dec.CustomsEntryHeaders.AddNew();
			var line = entry.MergedLines.AddNew();

			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = line.PK;

			var testSendingObject = new MessageSendingObject(entry)
			{
				MessageType = MessageSubTypeCodes.Codes.Original,
				SubmissionDate = new ZDateTime(2022, 11, 14)
			};
			var builder = new CUSDECMessageBuilder(testSendingObject, MessageSubTypes.Create);

			var generator = builder as Customs.Business.MessagingProcess.ICustomsMessageGenerator;

			AssertNotNull("Builder could not be cast to ICustomsMessageGenerator", generator);

			var msg = generator.GenerateMessage();

			AssertNotNull("Message created", msg);
			AssertEquals("EM_HeldUntilDate Empty", true, msg.EM_HeldUntilDate.IsEmpty);
			AssertContains("UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B", msg.EM_MessageText);

			testSendingObject.SubmissionDate = new ZDateTime(2022, 11, 16);
			builder = new CUSDECMessageBuilder(testSendingObject, MessageSubTypes.Create);
			generator = builder;
			msg = generator.GenerateMessage();

			AssertEquals("EM_HeldUntilDate Filled", new ZDateTime(2022, 11, 16), msg.EM_HeldUntilDate);
		}

		public void TestTransportMode()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			dec.JE_VesselName = "TEST VESSEL WITH A VERY LONG NAME";
			dec.JE_RadioCallSign = "1234567";
			dec.JE_Carrier = "AR1";
			AssertTransportMode(dec, "'TDT+20++1+++++:::AR1 1234567  TEST VESSEL WITH A VER'");

			dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = Core.Constants.TransportModes.Road;
			dec.JE_VoyageFlightNo = "AC75PPGP00";
			dec.JE_Trailer1 = "BC75PPGP01";
			dec.JE_Trailer2 = "CC75PPGP02";
			AssertTransportMode(dec, "'TDT+20++3+++++:::AC75PPGP00BC75PPGP01CC75PPGP02'");

			dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			dec.JE_VoyageFlightNo = "AR14567890";
			AssertTransportMode(dec, "'TDT+20+AR14567890+4'");
		}

		void AssertTransportMode(JobDeclaration declaration, string expectedTDTSegment)
		{
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var testSendingObject = new MessageSendingObject(entry);
			testSendingObject.MessageType = MessageSubTypeCodes.Codes.Original;

			var builder = new CUSDECMessageBuilder(testSendingObject, MessageSubTypes.Create);
			var result = builder.PopulateMessages().GetBuilderResults().ToArray();
			AssertEquals(1, result.Length);
			AssertContains("TDT", expectedTDTSegment, result[0].Message.EM_MessageText);
		}

		void CreateReferenceData_for_TestingValuationCode_for_ZA()
		{
			var helper = new ZAUniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure("ZA", "X", "11", "00", "", "X1100", "IMP,EXP");
			helper.CreateRefCusProcedure("ZA", "X", "12", "00", "", "X1200", "IMP,EXP");
			helper.CreateRefCusProcedure("ZA", "X", "20", "00", "", "X2000", "IMP,EXP");
			helper.CreateRefCusProcedure("ZA", "X", "21", "00", "", "X2100", "IMP,EXP");
			helper.CreateRefCusProcedure("ZA", "X", "22", "00", "", "X2200", "IMP,EXP");
			helper.CreateRefCusProcedure("ZA", "X", "37", "00", "", "X3700", "IMP,EXP");
			helper.CreateRefCusProcedure("ZA", "X", "78", "00", "", "X7800", "IMP,EXP");
			helper.CreateRefCusProcedure("ZA", "X", "60", "00", "", "X6000", "IMP,EXP");

			var startDate = new ZDateTime("1900-01-01");
			var endDate = new ZDateTime("2079-06-06");
			var countryCodeZA = "ZA";

			helper.CreateCusMapType(mapType: "REL", direction: "BTH", description: "Related Party Indicator", isReadonly: true);

			helper.CreateCusMap(mapType: "REL", cW1orCommercialValue: "E", customsValue: "E", startDate, endDate, countryCodeZA);
			helper.CreateCusMap(mapType: "REL", cW1orCommercialValue: "N", customsValue: "N", startDate, endDate, countryCodeZA);
			helper.CreateCusMap(mapType: "REL", cW1orCommercialValue: "Y", customsValue: "R", startDate, endDate, countryCodeZA);

			Factory.Save();
		}

		SupplierCode_TestCase_Collection Create_SupplierCode_TestCases()
		{
			var boolRange = new List<bool>() { true, false };
			var scenarios = new SupplierCode_TestCase_Collection();
			int counter = 0;

			foreach (var a in boolRange)
			{
				foreach (var b in boolRange)
				{
					var sCounter = string.Format(CultureInfo.InvariantCulture, "{0}", ++counter);
					scenarios.Add(sCounter, a, b);
				}
			}
			return scenarios;
		}

		sealed class SupplierCode_TestCase
		{
			public SupplierCode_TestCase(string testId, bool supplierHasCustomsCode, bool hasVDN)
			{
				TestId = testId;
				SupplierHasCustomsCode = supplierHasCustomsCode;
				HasVDN = hasVDN;

				MustHave_NAD_SU_segment = hasVDN && supplierHasCustomsCode;
				Expected_NAD_SU_segment = MustHave_NAD_SU_segment ? "NAD+SU+SUPREGNO" : null;
			}

			public readonly string TestId;
			public readonly bool SupplierHasCustomsCode;
			public readonly bool HasVDN;

			public readonly bool MustHave_NAD_SU_segment;
			public readonly string Expected_NAD_SU_segment;

			public override string ToString()
			{
				StringBuilder sb = new StringBuilder();
				sb.Append("TestId=" + string.Format(CultureInfo.InvariantCulture, "{0}", TestId));
				sb.Append(", SupplierHasCustomsCode=" + SupplierHasCustomsCode);
				sb.Append(", HasVDN=" + HasVDN);
				return sb.ToString();
			}
		}

		sealed class SupplierCode_Tester
		{
			public static void TestScenario(SupplierCode_TestCase scenario, JobDeclaration dec)
			{
				var assertMsg = scenario.ToString();
				var theInvoice = dec.Invoices[1];
				var theSupplier = dec.Supplier;
				theSupplier.CustomsCodes.RemoveAll();

				if (scenario.SupplierHasCustomsCode)
				{
					theSupplier.CustomsCodes.AddNew("CSC", "SUPREGNO", "ZA");
				}

				if (scenario.HasVDN)
				{
					theInvoice.JZ_VDN = "334455";
				}
				else
				{
					theInvoice.JZ_VDN = "";
				}

				var entry = dec.CustomsEntryHeaders[0];
				var sendingObject = new MessageSendingObject(entry);
				sendingObject.MessageType = MessageSubTypeCodes.Codes.Original;

				var builder = new CUSDECMessageBuilder(sendingObject, MessageSubTypes.Create);
				var result = builder.PopulateMessages().GetBuilderResults().ToArray();

				AssertEquals(assertMsg + ": Exactly one EDI message is expected.", 1, result.Length);
				var ediMessageText = result[0].Message.EM_MessageText;

				if (scenario.MustHave_NAD_SU_segment)
				{
					Assert(assertMsg, ediMessageText.IndexOf(scenario.Expected_NAD_SU_segment) > -1);
				}
				else
				{
					Assert(assertMsg, ediMessageText.IndexOf("NAD+SU++") > -1);
				}
			}
		}

		sealed class SupplierCode_TestCase_Collection
		{
			public void Add(string testId, bool supplierHasCustomsCode, bool hasVDN)
			{
				list.Add(new SupplierCode_TestCase(testId, supplierHasCustomsCode, hasVDN));
			}

			public void TestAll(JobDeclaration dec)
			{
				list.ForEach(x => SupplierCode_Tester.TestScenario(x, dec));
			}

			readonly List<SupplierCode_TestCase> list = new List<SupplierCode_TestCase>();
		}

		sealed class RelationshipIndicator_and_ValuationCode_TestCase
		{
			public RelationshipIndicator_and_ValuationCode_TestCase(string shipmentType, string cpc, string relationshipIndicator, bool shouldOutput, BusinessObjectFactory factory)
			{
				MessageDataProviderInstructionTest.ApplySanityCheck_BeforeTesting_ShouldOutput_RelatedIndicator_and_ValuationCode_in_EdiMessage(shipmentType, cpc, shouldOutput);

				this.shipmentType = shipmentType;
				cPC = cpc;
				this.relationshipIndicator = relationshipIndicator;
				this.shouldOutput = shouldOutput;
				this.factory = factory;
			}

			public void Check_for_RelationshipIndicator_and_ValuationCode_Segment()
			{
				var negate = shouldOutput ? "" : " not";
				var direction = (shipmentType == ZAJobMessageTypeList.Codes.Export) ? "Export" : "Import";
				var related = "";
				var gisSegment = "GIS+";

				switch (relationshipIndicator)
				{
					case RelatedIndicatorList.Codes.Yes:
						related = ", Related";
						gisSegment = "GIS+Y1";
						break;
					case RelatedIndicatorList.Codes.No:
						related = ", Not Related";
						gisSegment = "GIS+N1";
						break;
					case RelatedIndicatorList.Codes.Exempt:
						related = ", Exempt";
						gisSegment = "GIS+E";
						break;
				}
				var assertMsg = "Must" + negate + " output Related Indicator and Valuation Code for " + direction + " (CPC=" + cPC + ")" + related;

				var dec = factory.NewWithValidTestData<JobDeclaration>();
				dec.JE_MessageType = shipmentType;
				dec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;

				var instr = dec.CustomsEntryInstructions.AddNew();
				instr.CEI_Style = cPC;

				var entry = dec.CustomsEntryHeaders.AddNew();
				var line = entry.MergedLines.AddNew();

				var invoice = dec.Invoices.AddNew();
				invoice.JZ_RelatedIndicator = relationshipIndicator;
				invoice.JZ_ValuationCode = "1";

				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CL = line.PK;

				var testSendingObject = new MessageSendingObject(entry);
				testSendingObject.MessageType = MessageSubTypeCodes.Codes.Original;

				var builder = new CUSDECMessageBuilder(testSendingObject, MessageSubTypes.Create);
				var result = builder.PopulateMessages().GetBuilderResults().ToArray();

				AssertEquals(1, result.Length);
				AssertEquals(assertMsg, shouldOutput, result[0].Message.EM_MessageText.IndexOf(gisSegment) > -1);
			}

			readonly string shipmentType;
			readonly string cPC;
			readonly string relationshipIndicator;
			readonly bool shouldOutput;
			readonly BusinessObjectFactory factory;
		}

		sealed class RelationshipIndicator_and_ValuationCode_TestCase_Collection
		{
			public RelationshipIndicator_and_ValuationCode_TestCase_Collection(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}

			public void Add(string shipmentType, string cpc, string relationshipIndicator, bool shouldOutput)
			{
				list.Add(new RelationshipIndicator_and_ValuationCode_TestCase(shipmentType, cpc, relationshipIndicator, shouldOutput, factory));
			}

			public void TestAllTestCases() => list.ForEach(x => x.Check_for_RelationshipIndicator_and_ValuationCode_Segment());

			readonly BusinessObjectFactory factory;
			readonly List<RelationshipIndicator_and_ValuationCode_TestCase> list = new List<RelationshipIndicator_and_ValuationCode_TestCase>();
		}

		sealed class MrnToBeReplaced_TestCase
		{
			public MrnToBeReplaced_TestCase(string testId, string messageSubType, bool mustOutputOriginalMRN, bool mustOutputMRNToBeReplaced)
			{
				TestId = testId;
				MessageSubType = messageSubType;
				MustOutputOriginalMRN = mustOutputOriginalMRN;
				MustOutputMRNToBeReplaced = mustOutputMRNToBeReplaced;
			}

			public readonly string TestId;
			public readonly string MessageSubType;
			public readonly bool MustOutputOriginalMRN;
			public readonly bool MustOutputMRNToBeReplaced;
		}

		sealed class MrnToBeReplaced_Tester
		{
			public static void TestScenario(MrnToBeReplaced_TestCase scenario, BusinessObjectFactory factory)
			{
				var currentMRN_Segment = "RFF+ABT:DBN202004061234570'";
				var mrnToBeReplaced_Segment = "RFF+IB:DBN202004061234567'";
				var assertMsg = $"{scenario.TestId} ({scenario.MessageSubType})";

				var dec = factory.NewWithValidTestData<JobDeclaration>();
				dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				dec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;

				var instr = dec.CustomsEntryInstructions.AddNew();
				instr.CEI_Style = "11";
				instr.CEI_MRNToBeReplaced = "DBN202004061234567";

				var entry = dec.CustomsEntryHeaders.AddNew();
				entry.CH_CEI_Instruction = instr.PK;

				if (scenario.MessageSubType != MessageSubTypeCodes.Codes.Original)
				{
					entry.MovementReferenceNumberSetter("DBN202004061234570");
				}

				var line = entry.MergedLines.AddNew();
				var invoice = dec.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CL = line.PK;

				var sendingObject = new MessageSendingObject(entry);
				sendingObject.MessageType = scenario.MessageSubType;
				var messageSubTypeIntegerCode = GetMessageSubTypeIntegerCode(scenario.MessageSubType);

				var builder = new CUSDECMessageBuilder(sendingObject, messageSubTypeIntegerCode);
				var result = builder.PopulateMessages().GetBuilderResults().ToArray();

				AssertEquals(assertMsg + ": Exactly one EDI message is expected.", 1, result.Length);
				var ediMessageText = result[0].Message.EM_MessageText;

				CheckIfEdiMessageContainsSegment(assertMsg, scenario.MustOutputOriginalMRN, ediMessageText, currentMRN_Segment);
				CheckIfEdiMessageContainsSegment(assertMsg, scenario.MustOutputMRNToBeReplaced, ediMessageText, mrnToBeReplaced_Segment);
			}

			static void CheckIfEdiMessageContainsSegment(string assertMsg, bool shouldExist, string ediMessageText, string ediSegment)
			{
				AssertEquals(assertMsg, shouldExist, ediMessageText.IndexOf(ediSegment) > -1);
			}

			static MessageSubTypes GetMessageSubTypeIntegerCode(string messageSubTypeCode)
			{
				var returnValue = MessageSubTypes.Undefined;

				if (MessageSubTypeLookup.ContainsKey(messageSubTypeCode))
				{
					returnValue = MessageSubTypeLookup[messageSubTypeCode];
				}
				return returnValue;
			}

			static readonly Dictionary<string, MessageSubTypes> MessageSubTypeLookup = new Dictionary<string, MessageSubTypes>()
			{
				{ MessageSubTypeCodes.Codes.Cancellation, MessageSubTypes.Withdraw },
				{ MessageSubTypeCodes.Codes.Change, MessageSubTypes.Change },
				{ MessageSubTypeCodes.Codes.Original, MessageSubTypes.Create },
				{ MessageSubTypeCodes.Codes.Replace, MessageSubTypes.Replace }
			};
		}

		sealed class MrnToBeReplaced_TestCase_Collection
		{
			public void Add(string testId, string messageSubType, bool mustOutputOriginalMRN, bool mustOutputMRNToBeReplaced)
			{
				list.Add(new MrnToBeReplaced_TestCase(testId, messageSubType, mustOutputOriginalMRN, mustOutputMRNToBeReplaced));
			}

			public void TestAll(BusinessObjectFactory factory)
			{
				list.ForEach(x => MrnToBeReplaced_Tester.TestScenario(x, factory));
			}

			readonly List<MrnToBeReplaced_TestCase> list = new List<MrnToBeReplaced_TestCase>();
		}

		sealed class CUSDECMessageBuilderForTest : CUSDECMessageBuilder
		{
			public CUSDECMessageBuilderForTest(MessageSendingObject source, MessageSubTypes messageSubType) : base(source, messageSubType)
			{
			}

			public ZString GetMessageSubType_Exposed()
			{
				return GetMessageSubType();
			}
		}
	}
}
