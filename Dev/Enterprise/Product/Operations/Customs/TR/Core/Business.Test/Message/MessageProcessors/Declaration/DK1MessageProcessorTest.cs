using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using CusEntryHeader = Enterprise.Customs.TR.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class DK1MessageProcessorTest : DeclarationMessageProcessorBaseTest<DK1MessageProcessor>
	{
		public void TestGetCorrectBranchPK()
		{
			var messageObjects = CreateMessageWithOrigins(GetDefaultMessageText(), "DK1001", TRMessageTypes.Codes.DK1);

			var incomingMessage = messageObjects.incomingMessage;
			Processor.PreProcessMessage(incomingMessage);

			AssertEquals("GetCorrectBranchPK should have returned the correct BranchPK", messageObjects.messageAttachee.JE_GB, incomingMessage.EM_GB);
		}

		public void TestProcessMessageByErrors()
		{
			var messageObjects = CreateMessageWithOrigins(GetDefaultMessageText(), "DK1001", TRMessageTypes.Codes.DK1);
			var incomingMessage = messageObjects.incomingMessage;
			incomingMessage.EM_Status = EDIMessage.Status.PreProcessedOK;
			var messageText = incomingMessage.EM_MessageText;

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			var actualMessage = incomingMessage.EM_MessageInterpretation;
			CombineAssertions(() =>
			{
				AssertNotNullOrEmpty(actualMessage);
				Assert("EM_MessageInterpretation should contains 'Column Titles'", actualMessage.Contains(@"<tr class=""tableheadings""><th>Error Code</th><th>Error Description</th></tr>"));
				Assert("EM_MessageInterpretation should contains 'row 1'", actualMessage.Contains("<tr><td>1</td><td>A&#231;malarda esya ambar i&#231;inde se&#231;ilmis. Tasima senedi hen&#252;z ambara alinmamis ya da ambar &#231;ikis islemi yapilmistir. (226362740)</td></tr>"));
				Assert("EM_MessageInterpretation should contains 'row 2'", actualMessage.Contains("<tr><td>2</td><td>A&#231;malarda esya ambar i&#231;inde se&#231;ilmis. Tasima senedi hen&#252;z ambara alinmamis ya da ambar &#231;ikis islemi yapilmistir. (226252486)</td></tr>"));
			});

			AssertStatusesMessage(incomingMessage, TRMessageStatusCodeList.Codes.Error, EntryStatusTypeList.Codes.ERR, EDIMessage.Status.ProcessedOK);

			var messageObjects2 = CreateMessageWithOrigins(TRMessageTestHelper.GetFileText("DK1Error2.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Declaration.Incoming."), "DK1002", TRMessageTypes.Codes.DK1);
			var incomingMessage2 = messageObjects2.incomingMessage;
			incomingMessage2.EM_Status = EDIMessage.Status.PreProcessedOK;
			var messageText2 = incomingMessage2.EM_MessageText;

			Processor.PreProcessMessage(incomingMessage2);
			Processor.ProcessMessage(incomingMessage2);

			var actualMessage2 = incomingMessage2.EM_MessageInterpretation;
			CombineAssertions(() =>
			{
				AssertNotNullOrEmpty(actualMessage2);
				Assert("EM_MessageInterpretation should contains 'Column Titles'", actualMessage2.Contains(@"<tr class=""tableheadings""><th>Error Code</th><th>Error Description</th></tr>"));
				Assert("EM_MessageInterpretation should contains 'row 1'", actualMessage2.Contains("tr><td>1</td><td>066666 --&gt; G&#252;mr&#252;k Veri Tabanı Bağlantı Hatası</td></tr>"));
			});

			AssertStatusesMessage(incomingMessage2, TRMessageStatusCodeList.Codes.Error, EntryStatusTypeList.Codes.ERR, EDIMessage.Status.ProcessedOK);

			var messageObjects3 = CreateMessageWithOrigins(TRMessageTestHelper.GetFileText("DK1Error3.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Declaration.Incoming."), "DK1003", TRMessageTypes.Codes.DK1);
			var incomingMessage3 = messageObjects3.incomingMessage;
			incomingMessage3.EM_Status = EDIMessage.Status.PreProcessedOK;
			var messageText3 = incomingMessage3.EM_MessageText;

			Processor.PreProcessMessage(incomingMessage3);
			Processor.ProcessMessage(incomingMessage3);

			var actualMessage3 = incomingMessage3.EM_MessageInterpretation;
			CombineAssertions(() =>
			{
				AssertNotNullOrEmpty(actualMessage3);
				Assert("EM_MessageInterpretation should contains 'Declaration Message Type DK1 received successfully.'", actualMessage3.Contains("Declaration Message Type DK1 received successfully."));
				Assert("EM_MessageInterpretation should contains 'soapenv:Server'", actualMessage3.Contains("<tr><td>soapenv:Server</td><td>05-Servisimiz şu an hizmet verememektedir. L&#252;tfen entegrasyondestek@gtb.gov.tr ile irtibata ge&#231;iniz. G&#252;mr&#252;k ve Ticaret Bakanlığı</td></tr>"));
			});

			AssertStatusesMessage(incomingMessage3, TRMessageStatusCodeList.Codes.Error, EntryStatusTypeList.Codes.ERR, EDIMessage.Status.ProcessedOK);
		}

		public void TestProcessMessageByEmpty()
		{
			var messageObjects = CreateMessageWithOrigins(TRMessageTestHelper.GetFileText("DK1Empty.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Declaration.Incoming."), "DK1001", TRMessageTypes.Codes.DK1);
			var incomingMessage = messageObjects.incomingMessage;
			incomingMessage.EM_Status = EDIMessage.Status.PreProcessedOK;
			var messageText = incomingMessage.EM_MessageText;

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			var actualMessage = incomingMessage.EM_MessageInterpretation;
			Assert("EM_MessageInterpretation should contains 'Empty'", actualMessage.Contains(@"The message text sent back from Customs is empty."));
		}

		public void TestCreateEmailBodyWithImage()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";

			var messageObjects = CreateMessageWithOrigins(GetDefaultMessageText(), "DK1001", TRMessageTypes.Codes.DK1);
			var originalMessage = messageObjects.originalMessage;
			originalMessage.EM_SystemCreateUser = user.GS_Code;

			var incomingMessage = messageObjects.incomingMessage;

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			var cusEntryHeader = incomingMessage.EM_LinkedObject as CusEntryHeader;
			var declarationReference = cusEntryHeader.Declaration.JE_DeclarationReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(declarationReference));

			var imagePath = $"Enterprise.Customs.TR.Business.Message.EDIMessage.HtmlTemplates.Failure.jpg";
			string imageBase64 = default;
			imageBase64 = getImageValue(imagePath);

			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("Subject", "Customs Declaration Response (Failure) for 3-B00001000", email.Subject);
				Assert("Contains imageBase64", email.Body.Contains(imageBase64));
				Assert("Email body should contains 'Message Information'", email.Body.Contains("Customs Declaration Control Message Response for Job 3-B00001000 has Error. For details please follow the Link to the Customs Declaration"));
			});

			AssertStatusesMessage(incomingMessage, TRMessageStatusCodeList.Codes.Error, EntryStatusTypeList.Codes.ERR, EDIMessage.Status.ProcessedOK);
		}

		public void TestProcessMessageByEntryQuestions()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
			helper.CreateNewOrGetExistingCusCodeType("TRCUQ", "TurkeyCustomsQuestion", Core.Constants.CountryCodes.Turkey);
			helper.CreateNewOrGetExistingCusCodeType("TRCUW", "TurkeyCustomsWarning", Core.Constants.CountryCodes.Turkey);
			Factory.Save();

			var messageObjects = CreateMessageWithOrigins(TRMessageTestHelper.GetFileText("DK1Success.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Declaration.Incoming."), "DK1001", TRMessageTypes.Codes.DK1);
			var incomingMessage = messageObjects.incomingMessage;
			incomingMessage.EM_Status = EDIMessage.Status.PreProcessedOK;
			var messageText = incomingMessage.EM_MessageText;
			var jobDeclaration = messageObjects.messageAttachee;

			var cusEntryLine = jobDeclaration.CusEntryHeader.AllEntryLines.AddNew();
			cusEntryLine.CL_LineNumber = 1;
			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_CL = cusEntryLine.PK;

			var cusEntryLine2 = jobDeclaration.CusEntryHeader.AllEntryLines.AddNew();
			cusEntryLine2.CL_LineNumber = 2;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.JI_CL = cusEntryLine2.PK;

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			var actualMessage = incomingMessage.EM_MessageInterpretation.GetBodyText().RemoveLineBreakingsAndIndents();
			var cusEntryHeader = jobDeclaration.CusEntryHeader;

			CombineAssertions("Message Process Result", () =>
			{
				AssertNotNullOrEmpty("Actual Message", actualMessage);
				Assert("EM_MessageInterpretation should contains 'Column Labels'", actualMessage.Contains(@"<th>Code</th><th>Description</th><th>Line Number</th>"));
				Assert("EM_MessageInterpretation should contains '1.1 row'", actualMessage.Contains("<tr><td>7780</td><td>DIKKAT!!! Beyanname tescil edilecek ve otomatik olarak hat bildirimi (onay) islemi yapilacaktir. Bu asamadan sonra beyannamede d&#252;zeltme yapilamaz! D&#252;zeltme yapmak istiyormusunuz? (E/H)</td><td>0</td><td>Soru</td></tr>"));
				Assert("EM_MessageInterpretation should contains '1.2 row'", actualMessage.Contains("<tr><td>5178</td><td>Bu ithalat/ihracat elektronik ticaret (eticaret) midir ?</td><td>1</td><td>Soru</td></tr>"));
				Assert("EM_MessageInterpretation should contains '1.3 row'", actualMessage.Contains("<tr><td>5063</td><td>Beyan ettiginiz esyaniz Bazi Tarim &#220;r&#252;nlerinin Ihracatinda ve Ithalatinda Ticari Kalite Denetimi Tebligi kapsaminda ya da bu Teblig&#39;de yer alan sabit TAREKS referans numarasi beyanina tabi midir?</td><td>1</td><td>Soru</td></tr>"));
				Assert("EM_MessageInterpretation should contains '2.1 row'", actualMessage.Contains("<tr><td>5178</td><td>Bu ithalat/ihracat elektronik ticaret (eticaret) midir ?</td><td>2</td><td>Soru</td></tr>"));
				Assert("EM_MessageInterpretation should contains '2.2 row'", actualMessage.Contains("<tr><td>5063</td><td>Beyan ettiginiz esyaniz Bazi Tarim &#220;r&#252;nlerinin Ihracatinda ve Ithalatinda Ticari Kalite Denetimi Tebligi kapsaminda ya da bu Teblig&#39;de yer alan sabit TAREKS referans numarasi beyanina tabi midir?</td><td>2</td><td>Soru</td></tr>"));
				AssertNotNull("Entry Header", cusEntryHeader);
			});

			var cpDecHeaders = cusEntryHeader.CPDecCollection;
			CombineAssertions("Questions by Header", () =>
			{
				AssertNotNull("Not Null", cpDecHeaders);
				AssertEquals("Count", 1, cpDecHeaders.Count);
				AssertEquals("1.CPDecs | ON_CPDecNum", 7780, cpDecHeaders[0].ON_CPDecNum);
				AssertEquals("1.CPDecs | Description", SoapMessageTextHelper.Constants.DocumentCodes.Code7780Desc, cpDecHeaders[0].Description);
				AssertEquals("1.CPDecs | ON_QuestionType", "Q", cpDecHeaders[0].ON_QuestionType);
			});

			var cpDecLines = cusEntryHeader?.AllEntryLines[0]?.CPDecCollection;
			CombineAssertions("Line 1 Questions", () =>
			{
				AssertNotNull("Not Null", cpDecLines);
				AssertEquals("Count", 2, cpDecLines.Count);

				AssertEquals("1.CPDecs | ON_CPDecNum", 5178, cpDecLines[0].ON_CPDecNum);
				AssertEquals("1.CPDecs | Description", SoapMessageTextHelper.Constants.DocumentCodes.Code5178Desc, cpDecLines[0].Description);
				AssertEquals("1.CPDecs | ON_QuestionType", "Q", cpDecLines[0].ON_QuestionType);

				AssertEquals("2.CPDecs | ON_CPDecNum", 5063, cpDecLines[1].ON_CPDecNum);
				AssertEquals("2.CPDecs | Description", SoapMessageTextHelper.Constants.DocumentCodes.Code5063Desc, cpDecLines[1].Description);
				AssertEquals("2.CPDecs | ON_QuestionType", "Q", cpDecLines[1].ON_QuestionType);
			});

			var cpDecLines2 = cusEntryHeader?.AllEntryLines[1]?.CPDecCollection;
			CombineAssertions("Line 2 Questions", () =>
			{
				AssertNotNull("Not Null", cpDecLines2);
				AssertEquals("Count", 2, cpDecLines2.Count);

				AssertEquals("1.CPDecs | ON_CPDecNum", 5178, cpDecLines2[0].ON_CPDecNum);
				AssertEquals("1.CPDecs | Description", SoapMessageTextHelper.Constants.DocumentCodes.Code5178Desc, cpDecLines2[0].Description);
				AssertEquals("1.CPDecs | ON_QuestionType", "Q", cpDecLines2[0].ON_QuestionType);

				AssertEquals("2.CPDecs | ON_CPDecNum", 5063, cpDecLines2[1].ON_CPDecNum);
				AssertEquals("2.CPDecs | Description", SoapMessageTextHelper.Constants.DocumentCodes.Code5063Desc, cpDecLines2[1].Description);
				AssertEquals("2.CPDecs | ON_QuestionType", "Q", cpDecLines2[1].ON_QuestionType);
			});

			AssertStatusesMessage(incomingMessage, TRMessageStatusCodeList.Codes.Accepted, EntryStatusTypeList.Codes.QUE, EDIMessage.Status.ProcessedOK);
		}

		public void TestProcessMessageByDocuments()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
			helper.CreateNewOrGetExistingCusCodeType("TRCUQ", "TurkeyCustomsQuestion", Core.Constants.CountryCodes.Turkey);
			helper.CreateNewOrGetExistingCusCodeType("TRCUW", "TurkeyCustomsWarning", Core.Constants.CountryCodes.Turkey);
			Factory.Save();

			var messageObjects = CreateMessageWithOrigins(TRMessageTestHelper.GetFileText("DK1Success.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Declaration.Incoming."), "DK1001", TRMessageTypes.Codes.DK1);
			var incomingMessage = messageObjects.incomingMessage;
			incomingMessage.EM_Status = EDIMessage.Status.PreProcessedOK;
			var messageText = incomingMessage.EM_MessageText;
			var jobDeclaration = messageObjects.messageAttachee;
			var cusEntryLine = jobDeclaration.CusEntryHeader.AllEntryLines.AddNew();
			cusEntryLine.CL_LineNumber = 1;

			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_CL = cusEntryLine.PK;

			var cusEntryLine2 = jobDeclaration.CusEntryHeader.AllEntryLines.AddNew();
			cusEntryLine2.CL_LineNumber = 2;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.JI_CL = cusEntryLine2.PK;

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			var actualMessage = incomingMessage.EM_MessageInterpretation;
			var cusEntryHeader = jobDeclaration.CusEntryHeader;

			CombineAssertions("Message Process Result", () =>
			{
				AssertNotNullOrEmpty("Actual Message", actualMessage);
				Assert("EM_MessageInterpretation should contains 'Column Labels'", actualMessage.Contains(@"<th>Line Number</th><th>Code</th><th>Description</th></tr>"));
				Assert("EM_MessageInterpretation should contains '1.1 row'", actualMessage.Contains("<tr><td>1</td><td>0887</td><td>Transfer Bildirim Formu</td></tr><tr><td>1</td><td>0100</td><td>Fatura</td></tr>"));
				Assert("EM_MessageInterpretation should contains '1.2 row'", actualMessage.Contains("<tr><td>1</td><td>0100</td><td>Fatura</td></tr>"));
				Assert("EM_MessageInterpretation should contains '1.3 row'", actualMessage.Contains("<tr><td>1</td><td>0903</td><td>TPS-GTHB Uygunluk Yazisi (Bitki Karantinasi)  </td></tr>"));
				Assert("EM_MessageInterpretation should contains '1.5 row'", actualMessage.Contains("<tr><td>1</td><td>0902</td><td>TPS-GTHB Uygunluk Yazisi (Bitkisel Gida ve Yem)  </td></tr>"));
				Assert("EM_MessageInterpretation should contains '2.1 row'", actualMessage.Contains("<tr><td>2</td><td>0887</td><td>Transfer Bildirim Formu</td></tr>"));
				Assert("EM_MessageInterpretation should contains '2.2 row'", actualMessage.Contains("<tr><td>2</td><td>0100</td><td>Fatura</td></tr>"));
				Assert("EM_MessageInterpretation should contains '2.3 row'", actualMessage.Contains("<tr><td>2</td><td>0903</td><td>TPS-GTHB Uygunluk Yazisi (Bitki Karantinasi)  </td></tr>"));
				Assert("EM_MessageInterpretation should contains '2.4 row'", actualMessage.Contains("<tr><td>2</td><td>0902</td><td>TPS-GTHB Uygunluk Yazisi (Bitkisel Gida ve Yem)  </td></tr>"));
				AssertNotNull("Entry Header", cusEntryHeader);
			});

			var docs = invoiceLine.SupportingDocuments;
			CombineAssertions("Line 1Documents", () =>
			{
				AssertNotNull("Not Null", docs);
				AssertEquals("Count", 4, docs.Count);
				AssertEquals("1.Document | CSI_Code", "0887", docs[0].CSI_Code);
				AssertEquals("2.Document | CSI_Code", "0100", docs[1].CSI_Code);
				AssertEquals("3.Document | CSI_Code", "0903", docs[2].CSI_Code);
				AssertEquals("5.Document | CSI_Code", "0902", docs[3].CSI_Code);
			});

			var docs2 = invoiceLine2.SupportingDocuments;
			CombineAssertions("Line 2 Documents", () =>
			{
				AssertNotNull("Not Null", docs2);
				AssertEquals("Count", 4, docs2.Count);
				AssertEquals("1.Document | CSI_Code", "0887", docs2[0].CSI_Code);
				AssertEquals("2.Document | CSI_Code", "0100", docs2[1].CSI_Code);
				AssertEquals("3.Document | CSI_Code", "0903", docs2[2].CSI_Code);
				AssertEquals("4.Document | CSI_Code", "0902", docs2[3].CSI_Code);
			});

			AssertStatusesMessage(incomingMessage, TRMessageStatusCodeList.Codes.Accepted, EntryStatusTypeList.Codes.QUE, EDIMessage.Status.ProcessedOK);
		}

		public void TestProcessMessageByTaxes()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
			helper.CreateNewOrGetExistingCusCodeType("TRCUQ", "TurkeyCustomsQuestion", Core.Constants.CountryCodes.Turkey);
			helper.CreateNewOrGetExistingCusCodeType("TRCUW", "TurkeyCustomsWarning", Core.Constants.CountryCodes.Turkey);
			Factory.Save();

			var messageObjects = CreateMessageWithOrigins(TRMessageTestHelper.GetFileText("DK1Success.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Declaration.Incoming."), "DK1001", TRMessageTypes.Codes.DK1);
			var incomingMessage = messageObjects.incomingMessage;
			incomingMessage.EM_Status = EDIMessage.Status.PreProcessedOK;
			var messageText = incomingMessage.EM_MessageText;
			var jobDeclaration = messageObjects.messageAttachee;
			var cusEntryLine = jobDeclaration.CusEntryHeader.AllEntryLines.AddNew();
			cusEntryLine.CL_LineNumber = 1;

			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_CL = cusEntryLine.PK;

			var cusEntryLine2 = jobDeclaration.CusEntryHeader.AllEntryLines.AddNew();
			cusEntryLine2.CL_LineNumber = 2;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.JI_CL = cusEntryLine2.PK;

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			var actualMessage = incomingMessage.EM_MessageInterpretation.GetBodyText().RemoveLineBreakingsAndIndents();
			var cusEntryHeader = jobDeclaration.CusEntryHeader;

			CombineAssertions("Message Process Result", () =>
			{
				AssertNotNullOrEmpty("Actual Message", actualMessage);
				Assert("EM_MessageInterpretation should contains 'Column Labels'", actualMessage.Contains(@"<th>Line Number</th><th>Code</th><th>Description</th><th>Amount</th><th>Rate</th><th>Payment Type</th><th>Tax Assessment</th>"));
				Assert("EM_MessageInterpretation should contains '1.1 row'", actualMessage.Contains("<tr><td>1</td><td>10</td><td>G&#252;mr&#252;k Vergisi</td><td>131320.72</td><td>45</td><td>P</td><td>291823.83</td></tr>"));
				Assert("EM_MessageInterpretation should contains '1.2 row'", actualMessage.Contains("<tr><td>1</td><td>40</td><td>Katma Deger Vergisi</td><td>4265.38</td><td>1</td><td>P</td><td>426538.45</td></tr>"));
				Assert("EM_MessageInterpretation should contains '1.3 row'", actualMessage.Contains("<tr><td>1</td><td>89</td><td>Damga Vergisi</td><td>393.90</td><td>0.0</td><td>P</td><td>0</td></tr>"));
				Assert("EM_MessageInterpretation should contains '2.1 row'", actualMessage.Contains("<tr><td>2</td><td>10</td><td>G&#252;mr&#252;k Vergisi</td><td>131320.72</td><td>45</td><td>P</td><td>291823.83</td></tr>"));
				Assert("EM_MessageInterpretation should contains '2.2 row'", actualMessage.Contains("<tr><td>2</td><td>40</td><td>Katma Deger Vergisi</td><td>4261.45</td><td>1</td><td>P</td><td>426144.55</td></tr>"));
				AssertNotNull("Entry Header", cusEntryHeader);
			});

			var fees = cusEntryLine.Fees;
			CombineAssertions("Line 1 Taxes", () =>
			{
				AssertNotNull("Not Null", fees);
				AssertEquals("Count", 3, fees.Count);

				AssertEquals("1.Tax | CF_ChargeType", "10", fees[0].CF_ChargeType);
				AssertEquals("1.Tax | CF_ChargeAmount", 131320.72m, fees[0].CF_ChargeAmount);
				AssertEquals("1.Tax | CF_Rate", 45m, fees[0].CF_Rate);
				AssertEquals("1.Tax | CF_MethodOfPayment", "P", fees[0].CF_MethodOfPayment);
				AssertEquals("1.Tax | CF_BaseValue", 291823.83m, fees[0].CF_BaseValue);
				AssertEquals("1.Tax | CF_Source", "CW1", fees[0].CF_Source);
				AssertEquals("1.Tax | CF_RateOverrideReasonCode", "ADD", fees[0].CF_RateOverrideReasonCode);
				AssertEquals("1.Tax | CF_MethodOfCalculation", "Gümrük", fees[0].CF_MethodOfCalculation);

				AssertEquals("2.Tax | CF_ChargeType", "B00", fees[1].CF_ChargeType);
				AssertEquals("2.Tax | NationalFeeTypeCode", "40", fees[1].NationalFeeTypeCode);
				AssertEquals("2.Tax | CF_ChargeAmount", 4265.38m, fees[1].CF_ChargeAmount);
				AssertEquals("2.Tax | CF_Rate", 1m, fees[1].CF_Rate);
				AssertEquals("2.Tax | CF_MethodOfPayment", "P", fees[1].CF_MethodOfPayment);
				AssertEquals("2.Tax | CF_BaseValue", 426538.45m, fees[1].CF_BaseValue);
				AssertEquals("2.Tax | CF_Source", "CW1", fees[1].CF_Source);
				AssertEquals("2.Tax | CF_RateOverrideReasonCode", "ADD", fees[1].CF_RateOverrideReasonCode);
				AssertEquals("2.Tax | CF_MethodOfCalculation", "Gümrük", fees[1].CF_MethodOfCalculation);

				AssertEquals("3.Tax | CF_ChargeType", "89", fees[2].CF_ChargeType);
				AssertEquals("3.Tax | CF_ChargeAmount", 393.90m, fees[2].CF_ChargeAmount);
				AssertEquals("3.Tax | CF_Rate", 0m, fees[2].CF_Rate);
				AssertEquals("3.Tax | CF_MethodOfPayment", "P", fees[2].CF_MethodOfPayment);
				AssertEquals("3.Tax | CF_BaseValue", 0m, fees[2].CF_BaseValue);
				AssertEquals("3.Tax | CF_Source", "CW1", fees[2].CF_Source);
				AssertEquals("3.Tax | CF_RateOverrideReasonCode", "ADD", fees[2].CF_RateOverrideReasonCode);
				AssertEquals("3.Tax | CF_MethodOfCalculation", "Gümrük", fees[2].CF_MethodOfCalculation);
			});

			var fees2 = cusEntryLine2.Fees;
			CombineAssertions("Line 2 Taxes", () =>
			{
				AssertNotNull("Not Null", fees2);
				AssertEquals("Count", 2, fees2.Count);

				AssertEquals("1.Tax | CF_ChargeType", "10", fees2[0].CF_ChargeType);
				AssertEquals("1.Tax | CF_ChargeAmount", 131320.72m, fees2[0].CF_ChargeAmount);
				AssertEquals("1.Tax | CF_Rate", 45m, fees2[0].CF_Rate);
				AssertEquals("1.Tax | CF_MethodOfPayment", "P", fees2[0].CF_MethodOfPayment);
				AssertEquals("1.Tax | CF_BaseValue", 291823.83m, fees2[0].CF_BaseValue);
				AssertEquals("1.Tax | CF_Source", "CW1", fees2[0].CF_Source);
				AssertEquals("1.Tax | CF_RateOverrideReasonCode", "ADD", fees2[0].CF_RateOverrideReasonCode);
				AssertEquals("1.Tax | CF_MethodOfCalculation", "Gümrük", fees2[0].CF_MethodOfCalculation);

				AssertEquals("2.Tax | CF_ChargeType", "B00", fees2[1].CF_ChargeType);
				AssertEquals("2.Tax | NationalFeeTypeCode", "40", fees2[1].NationalFeeTypeCode);
				AssertEquals("2.Tax | CF_ChargeAmount", 4261.45m, fees2[1].CF_ChargeAmount);
				AssertEquals("2.Tax | CF_Rate", 1m, fees2[1].CF_Rate);
				AssertEquals("2.Tax | CF_MethodOfPayment", "P", fees2[1].CF_MethodOfPayment);
				AssertEquals("2.Tax | CF_BaseValue", 426144.55m, fees2[1].CF_BaseValue);
				AssertEquals("2.Tax | CF_Source", "CW1", fees2[1].CF_Source);
				AssertEquals("2.Tax | CF_RateOverrideReasonCode", "ADD", fees2[1].CF_RateOverrideReasonCode);
				AssertEquals("2.Tax | CF_MethodOfCalculation", "Gümrük", fees2[1].CF_MethodOfCalculation);
			});

			AssertStatusesMessage(incomingMessage, TRMessageStatusCodeList.Codes.Accepted, EntryStatusTypeList.Codes.QUE, EDIMessage.Status.ProcessedOK);
		}

		public void TestProcessMessageByTaxes2()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
			helper.CreateNewOrGetExistingCusCodeType("TRCUQ", "TurkeyCustomsQuestion", Core.Constants.CountryCodes.Turkey);
			helper.CreateNewOrGetExistingCusCodeType("TRCUW", "TurkeyCustomsWarning", Core.Constants.CountryCodes.Turkey);
			Factory.Save();

			var messageObjects = CreateMessageWithOrigins(TRMessageTestHelper.GetFileText("DK1Success2.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Declaration.Incoming."), "DK1001", TRMessageTypes.Codes.DK1);
			var incomingMessage = messageObjects.incomingMessage;
			incomingMessage.EM_Status = EDIMessage.Status.PreProcessedOK;
			var messageText = incomingMessage.EM_MessageText;
			var jobDeclaration = messageObjects.messageAttachee;
			var cusEntryLine = jobDeclaration.CusEntryHeader.AllEntryLines.AddNew();
			cusEntryLine.CL_LineNumber = 1;

			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_CL = cusEntryLine.PK;

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			var actualMessage = incomingMessage.EM_MessageInterpretation.GetBodyText().RemoveLineBreakingsAndIndents();
			var cusEntryHeader = jobDeclaration.CusEntryHeader;

			CombineAssertions("Message Process Result", () =>
			{
				AssertNotNullOrEmpty("Actual Message", actualMessage);
				Assert("EM_MessageInterpretation should contains 'Column Labels'", actualMessage.Contains(@"<th>Line Number</th><th>Code</th><th>Description</th><th>Amount</th><th>Rate</th><th>Payment Type</th><th>Tax Assessment</th>"));
				Assert("EM_MessageInterpretation should contains '1.1 row'", actualMessage.Contains("<tr><td>1</td><td>10</td><td>G&#252;mr&#252;k Vergisi</td><td>30095.82</td><td>8</td><td>P</td><td>376197.69</td></tr>"));
				Assert("EM_MessageInterpretation should contains '1.2 row'", actualMessage.Contains("<tr><td>1</td><td>40</td><td>Katma Deger Vergisi</td><td>0</td><td>0.0</td><td>P</td><td>406917.61</td></tr>"));
				Assert("EM_MessageInterpretation should contains '1.3 row'", actualMessage.Contains("<tr><td>1</td><td>89</td><td>Damga Vergisi</td><td>624.10</td><td>0.0</td><td>P</td><td>0</td></tr>"));
				AssertNotNull("Entry Header", cusEntryHeader);
			});

			var fees = cusEntryLine.Fees;
			CombineAssertions("Line 1 Taxes", () =>
			{
				AssertNotNull("Not Null", fees);
				AssertEquals("Count", 3, fees.Count);

				AssertEquals("1.Tax | CF_ChargeType", "10", fees[0].CF_ChargeType);
				AssertEquals("1.Tax | CF_ChargeAmount", 30095.82m, fees[0].CF_ChargeAmount);
				AssertEquals("1.Tax | CF_Rate", 8m, fees[0].CF_Rate);
				AssertEquals("1.Tax | CF_MethodOfPayment", "P", fees[0].CF_MethodOfPayment);
				AssertEquals("1.Tax | CF_BaseValue", 376197.69m, fees[0].CF_BaseValue);
				AssertEquals("1.Tax | CF_Source", "CW1", fees[0].CF_Source);
				AssertEquals("1.Tax | CF_RateOverrideReasonCode", "ADD", fees[0].CF_RateOverrideReasonCode);
				AssertEquals("1.Tax | CF_MethodOfCalculation", "Gümrük", fees[0].CF_MethodOfCalculation);

				AssertEquals("2.Tax | CF_ChargeType", "B00", fees[1].CF_ChargeType);
				AssertEquals("2.Tax | NationalFeeTypeCode", "40", fees[1].NationalFeeTypeCode);
				AssertEquals("2.Tax | CF_ChargeAmount", 0m, fees[1].CF_ChargeAmount);
				AssertEquals("2.Tax | CF_Rate", 0m, fees[1].CF_Rate);
				AssertEquals("2.Tax | CF_MethodOfPayment", "P", fees[1].CF_MethodOfPayment);
				AssertEquals("2.Tax | CF_BaseValue", 406917.61m, fees[1].CF_BaseValue);
				AssertEquals("2.Tax | CF_Source", "CW1", fees[1].CF_Source);
				AssertEquals("2.Tax | CF_RateOverrideReasonCode", "ADD", fees[1].CF_RateOverrideReasonCode);
				AssertEquals("2.Tax | CF_MethodOfCalculation", "Gümrük", fees[1].CF_MethodOfCalculation);

				AssertEquals("3.Tax | CF_ChargeType", "89", fees[2].CF_ChargeType);
				AssertEquals("3.Tax | CF_ChargeAmount", 624.10m, fees[2].CF_ChargeAmount);
				AssertEquals("3.Tax | CF_Rate", 0m, fees[2].CF_Rate);
				AssertEquals("3.Tax | CF_MethodOfPayment", "P", fees[2].CF_MethodOfPayment);
				AssertEquals("3.Tax | CF_BaseValue", 0m, fees[2].CF_BaseValue);
				AssertEquals("3.Tax | CF_Source", "CW1", fees[2].CF_Source);
				AssertEquals("3.Tax | CF_RateOverrideReasonCode", "ADD", fees[2].CF_RateOverrideReasonCode);
				AssertEquals("3.Tax | CF_MethodOfCalculation", "Gümrük", fees[2].CF_MethodOfCalculation);
			});

			AssertStatusesMessage(incomingMessage, TRMessageStatusCodeList.Codes.Accepted, EntryStatusTypeList.Codes.QUE, EDIMessage.Status.ProcessedOK);
		}

		public void TestProcessMessageByNoDublicateTaxes()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
			helper.CreateNewOrGetExistingCusCodeType("TRCUQ", "TurkeyCustomsQuestion", Core.Constants.CountryCodes.Turkey);
			helper.CreateNewOrGetExistingCusCodeType("TRCUW", "TurkeyCustomsWarning", Core.Constants.CountryCodes.Turkey);
			Factory.Save();

			var messageObjects = CreateMessageWithOrigins(TRMessageTestHelper.GetFileText("DK1Success.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Declaration.Incoming."), "DK1005", TRMessageTypes.Codes.DK1);
			var incomingMessage = messageObjects.incomingMessage;
			incomingMessage.EM_Status = EDIMessage.Status.PreProcessedOK;
			var messageText = incomingMessage.EM_MessageText;
			var jobDeclaration = messageObjects.messageAttachee;
			var entryHeader = jobDeclaration.CusEntryHeader;
			var cusEntryLine = jobDeclaration.CusEntryHeader.AllEntryLines.AddNew();
			cusEntryLine.CL_LineNumber = 1;

			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_CL = cusEntryLine.PK;

			var cusEntryLine2 = jobDeclaration.CusEntryHeader.AllEntryLines.AddNew();
			cusEntryLine2.CL_LineNumber = 2;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.JI_CL = cusEntryLine2.PK;

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			var fees = cusEntryLine.Fees;
			CombineAssertions("Line 1 Taxes", () =>
			{
				AssertNotNull("Not Null", fees);
				AssertEquals("Count", 3, fees.Count);

				AssertEquals("1.Tax | CF_ChargeType", "10", fees[0].CF_ChargeType);
				AssertEquals("1.Tax | CF_RateOverrideReasonCode", "ADD", fees[0].CF_RateOverrideReasonCode);

				AssertEquals("2.Tax | CF_ChargeType", "B00", fees[1].CF_ChargeType);
				AssertEquals("2.Tax | NationalFeeTypeCode", "40", fees[1].NationalFeeTypeCode);
				AssertEquals("2.Tax | CF_RateOverrideReasonCode", "ADD", fees[1].CF_RateOverrideReasonCode);

				AssertEquals("3.Tax | CF_ChargeType", "89", fees[2].CF_ChargeType);
				AssertEquals("3.Tax | CF_RateOverrideReasonCode", "ADD", fees[2].CF_RateOverrideReasonCode);
			});

			var originMessageType = TRMessageTypes.Codes.DKO;
			var sessionID = ZGuid.NewZGuid();
			var originalInterchange = TRMessageTestHelper.CreateInterchange(Factory, originMessageType, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, string.Empty, sessionID);
			var originalMessage = TRMessageTestHelper.CreateMessage<TRImportExportMessage>(Factory, originMessageType, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, string.Empty, "DKOTRX001", string.Empty);
			originalMessage.EM_EI = originalInterchange.PK;
			originalMessage.EM_LinkedObject = entryHeader;

			Factory.Save();

			var incomingInterchange = TRMessageTestHelper.CreateInterchange(Factory, TRMessageTypes.Codes.DK1, EDIInterchange.Direction.Receive, EDIInterchange.Status.Sent, string.Empty, sessionID);
			var incomingMessage2 = TRMessageTestHelper.CreateMessage<TRImportExportMessage>(Factory, TRMessageTypes.Codes.DK1, EDIInterchange.Direction.Receive, EDIInterchange.Status.Sent, string.Empty, "DKORCV001", string.Empty);
			incomingMessage2.EM_EI = incomingInterchange.PK;
			incomingMessage2.EM_MessageText = messageText;
			incomingMessage2.EM_MessageNum = "DK1006";
			incomingMessage2.EM_SystemCreateTimeUtc = ZDateTime.Now;
			incomingMessage2.EM_LinkedObject = entryHeader;

			Processor.PreProcessMessage(incomingMessage2);
			Processor.ProcessMessage(incomingMessage2);

			var fees2 = cusEntryLine.Fees;
			CombineAssertions("Line 1 Taxes", () =>
			{
				AssertNotNull("Not Null", fees2);
				AssertEquals("Count", 3, fees.Count);

				AssertEquals("1.Tax | CF_ChargeType", "10", fees2[0].CF_ChargeType);
				AssertEquals("1.Tax | CF_RateOverrideReasonCode", "OVR", fees2[0].CF_RateOverrideReasonCode);

				AssertEquals("2.Tax | CF_ChargeType", "B00", fees2[1].CF_ChargeType);
				AssertEquals("2.Tax | NationalFeeTypeCode", "40", fees2[1].NationalFeeTypeCode);
				AssertEquals("2.Tax | CF_RateOverrideReasonCode", "OVR", fees2[1].CF_RateOverrideReasonCode);

				AssertEquals("3.Tax | CF_ChargeType", "89", fees2[2].CF_ChargeType);
				AssertEquals("3.Tax | CF_RateOverrideReasonCode", "OVR", fees2[2].CF_RateOverrideReasonCode);
			});
		}

		public void TestMessageStatusesForQuestions()
		{
			var msgContext = SuccessWithQueWarnDocMessageContext;
			var incomingMessage = msgContext.IncomingMessage;
			incomingMessage.EM_Status = EDIMessage.Status.PreProcessedOK;

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			AssertStatusesMessage(incomingMessage, TRMessageStatusCodeList.Codes.Accepted, EntryStatusTypeList.Codes.QUE, EDIMessage.Status.ProcessedOK);
		}

		public void TestMessageStatusesForSupportingDocuments()
		{
			var msgContext = SuccessWithWarnDocMessageContext;
			var incomingMessage = msgContext.IncomingMessage;
			incomingMessage.EM_Status = EDIMessage.Status.PreProcessedOK;

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			AssertStatusesMessage(incomingMessage, TRMessageStatusCodeList.Codes.Accepted, EntryStatusTypeList.Codes.SDO, EDIMessage.Status.ProcessedOK);
		}

		public void TestMessageStatusesForWarnings()
		{
			var msgContext = SuccessWithWarnMessageContext;
			var incomingMessage = msgContext.IncomingMessage;
			incomingMessage.EM_Status = EDIMessage.Status.PreProcessedOK;

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			AssertStatusesMessage(incomingMessage, TRMessageStatusCodeList.Codes.Accepted, EntryStatusTypeList.Codes.WRN, EDIMessage.Status.ProcessedOK);
		}

		public void TestMessageResponseShouldIncludeErrorDescription()
		{
			var msgContext = ErrorMessageContext;
			var incomingMessage = msgContext.IncomingMessage;
			incomingMessage.EM_Status = EDIMessage.Status.PreProcessedOK;

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			AssertStatusesMessage(incomingMessage, TRMessageStatusCodeList.Codes.Error, EntryStatusTypeList.Codes.ERR, EDIMessage.Status.ProcessedOK);
		}

		public void TestProcessIncorrectSupportingCode()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
			helper.CreateNewOrGetExistingCusCodeType("TRCUQ", "TurkeyCustomsQuestion", Core.Constants.CountryCodes.Turkey);
			helper.CreateNewOrGetExistingCusCodeType("TRCUW", "TurkeyCustomsWarning", Core.Constants.CountryCodes.Turkey);
			Factory.Save();

			var messageObjects = CreateMessageWithOrigins(TRMessageTestHelper.GetFileText("DK1SuccessIncludeIncorrectSupportingCode.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Declaration.Incoming."), "DK1001", TRMessageTypes.Codes.DK1);
			var incomingMessage = messageObjects.incomingMessage;
			incomingMessage.EM_Status = EDIMessage.Status.PreProcessedOK;
			var messageText = incomingMessage.EM_MessageText;
			var jobDeclaration = messageObjects.messageAttachee;
			var cusEntryLine = jobDeclaration.CusEntryHeader.AllEntryLines.AddNew();
			cusEntryLine.CL_LineNumber = 1;

			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_CL = cusEntryLine.PK;

			Processor.ProcessMessage(incomingMessage);

			var supportingDocuments = invoiceLine.SupportingDocuments.Cast<SupportingDocument>().ToList();

			CombineAssertions("supportingDocuments should not contains 0103", () =>
			{
				AssertNotNull("Not Null", supportingDocuments);
				AssertEquals("Count", 3, supportingDocuments.Count);
				Assert(!supportingDocuments.Any(x => x.CSI_Code == TRMessageConstants.IncorrectSupportingCodeReceivedByCustoms0103));
			});

			AssertStatusesMessage(incomingMessage, TRMessageStatusCodeList.Codes.Accepted, EntryStatusTypeList.Codes.QUE, EDIMessage.Status.ProcessedOK);
		}

		static string getImageValue(string imagePath)
		{
			string imageBase64;
			using (var imageStream = typeof(TRManifestMessage).Assembly.GetManifestResourceStream(imagePath))
			{
				var imageBytes = new byte[imageStream.Length];
				_ = imageStream.Read(imageBytes, 0, imageBytes.Length);
				imageBase64 = Convert.ToBase64String(imageBytes);
			}

			return imageBase64;
		}

		protected override DK1MessageProcessor Processor => new DK1MessageProcessor(logger);
		protected override ZString DefaultMessageType => TRMessageTypes.Codes.DK1;

		protected TestMessageContext SuccessWithQueWarnDocMessageContext
		{
			get
			{
				var (originalInterchagne, originalMessage, incomingInterchagne, incomingMessage, messageAttachee, sessionId) = CreateMessageWithOrigins(TRMessageTestHelper.GetFileText("DK1SuccessWithQueWarnDoc.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Declaration.Incoming."), "DK1001", TRMessageTypes.Codes.DK1);

				var msgContext = new TestMessageContext
				{
					OriginalInterchange = originalInterchagne,
					OriginalMessage = originalMessage,
					IncomingInterchange = incomingInterchagne,
					IncomingMessage = incomingMessage,
					MessageAttachee = messageAttachee,
					SessionID = sessionId,
				};

				return msgContext;
			}
		}

		protected TestMessageContext SuccessWithWarnDocMessageContext
		{
			get
			{
				var (originalInterchagne, originalMessage, incomingInterchagne, incomingMessage, messageAttachee, sessionId) = CreateMessageWithOrigins(TRMessageTestHelper.GetFileText("DK1SuccessWithWarnDoc.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Declaration.Incoming."), "DK1001", TRMessageTypes.Codes.DK1);

				var msgContext = new TestMessageContext
				{
					OriginalInterchange = originalInterchagne,
					OriginalMessage = originalMessage,
					IncomingInterchange = incomingInterchagne,
					IncomingMessage = incomingMessage,
					MessageAttachee = messageAttachee,
					SessionID = sessionId,
				};

				return msgContext;
			}
		}
		protected TestMessageContext SuccessWithWarnMessageContext
		{
			get
			{
				var (originalInterchagne, originalMessage, incomingInterchagne, incomingMessage, messageAttachee, sessionId) = CreateMessageWithOrigins(TRMessageTestHelper.GetFileText("DK1SuccessWithWarn.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Declaration.Incoming."), "DK1001", TRMessageTypes.Codes.DK1);

				var msgContext = new TestMessageContext
				{
					OriginalInterchange = originalInterchagne,
					OriginalMessage = originalMessage,
					IncomingInterchange = incomingInterchagne,
					IncomingMessage = incomingMessage,
					MessageAttachee = messageAttachee,
					SessionID = sessionId,
				};

				return msgContext;
			}
		}
		protected TestMessageContext ErrorMessageContext
		{
			get
			{
				var (originalInterchagne, originalMessage, incomingInterchagne, incomingMessage, messageAttachee, sessionId) = CreateMessageWithOrigins(TRMessageTestHelper.GetFileText("DK1Error.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Declaration.Incoming."), "DK1001", TRMessageTypes.Codes.DK1);

				var msgContext = new TestMessageContext
				{
					OriginalInterchange = originalInterchagne,
					OriginalMessage = originalMessage,
					IncomingInterchange = incomingInterchagne,
					IncomingMessage = incomingMessage,
					MessageAttachee = messageAttachee,
					SessionID = sessionId,
				};

				return msgContext;
			}
		}

		ZString GetDefaultMessageText() => TRMessageTestHelper.GetFileText("DK1Error.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Declaration.Incoming.");
	}
}
