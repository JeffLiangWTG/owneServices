using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.AGS.Transforms.UniTrans_2_WWACustomerInvoice;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Clients.AGS.Tests
{
	[TestClass]
	public class UniTrans_2_CustomerInvoiceTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniTrans_2_WWACustomerInvoice_EventSubscriptionNotFound()
		{
			string sourceFile = "UniTrans_2_WWACustomerInvoice.TestFiles.Input_CRD.xml";
			string invoiceSubscription = XDocument.Load(GetEmbeddedResource("UniTrans_2_WWACustomerInvoice.TestFiles.Output_CRD.xml")).ToString(SaveOptions.DisableFormatting);
			string invoiceSubscription_NoOrigTransNum = XDocument.Load(GetEmbeddedResource("UniTrans_2_WWACustomerInvoice.TestFiles.Output_CRD_NoOrigTransNum.xml")).ToString(SaveOptions.DisableFormatting);
			string expectedFile = "UniTrans_2_WWACustomerInvoice.TestFiles.Output_NoSubscription.xml";

			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("AGSWWA", "AGSNZGAGS", "AGSWORAGS_WTR", "FPS_AR_CRD_00001000", invoiceSubscription, "Invoice")).Repeat.Once();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("AGSWWA", "AGSNZGAGS", "AGSWORAGS_WTR", "FPS_AR_CRD_00002000", invoiceSubscription_NoOrigTransNum, "Invoice")).Repeat.Once();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "AGSNZGAGS", "@recipientId", "AGSWORAGS_WTR", "@ST_ID", "AGSWWA", "@value", "FPS_AR_CRD_00001000", "@referenceType", "Event")).Return("").Repeat.Once();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "AGSNZGAGS", "@recipientId", "AGSWORAGS_WTR", "@ST_ID", "AGSWWA", "@value", "FPS_AR_CRD_00002000", "@referenceType", "Event")).Return("").Repeat.Once();
			mockDateMapper.AssertWasNotCalled(x => x.CurrentDateTime("yyyyMMddHHmmss"));
			mockDateMapper.AssertWasNotCalled(x => x.CurrentDateTime("yyyyMMdd.HHmmss.fff"));
			mockContextAccessor.AssertWasNotCalled(x => x.SetContextProperty(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything));
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("AGSNZGAGS").Repeat.Twice();
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("AGSWORAGS_WTR").Repeat.Twice();

			InitaliseCodeMapper(mockCodeMapper);
			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor }
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<UniTrans_2_WWACustomerInvoice>(sourceFile, expectedFile);

			sourceFile = "UniTrans_2_WWACustomerInvoice.TestFiles.Input_CRD_NoOrigTransNum.xml";
			expectedFile = "UniTrans_2_WWACustomerInvoice.TestFiles.Output_NoSubscription.xml";
			mapTester.Execute<UniTrans_2_WWACustomerInvoice>(sourceFile, expectedFile);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniTrans_2_WWACustomerInvoice_EventSubscriptionExists()
		{
			string sourceFile = "UniTrans_2_WWACustomerInvoice.TestFiles.Input_INV.xml";
			string eventSubscription = XDocument.Load(GetEmbeddedResource("UniTrans_2_WWACustomerInvoice.TestFiles.Event_Subscription.xml")).ToString(SaveOptions.DisableFormatting);
			string expectedFile = "UniTrans_2_WWACustomerInvoice.TestFiles.Output_INV.xml";

			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockDataModelAccessor.AssertWasNotCalled(x => x.InsertSubscriptionValue("AGSWWA", "AGSWORAGS", "AGSWORAGS_WTR", "DAU_AR_INV_00001122", eventSubscription, "Event"));
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "AGSWORAGS", "@recipientId", "AGSWORAGS_WTR", "@ST_ID", "AGSWWA", "@value", "DAU_AR_INV_00001122", "@referenceType", "Event")).Return(eventSubscription).Repeat.Once();
			mockDateMapper.Expect(x => x.CurrentDateTime("yyyyMMdd.HHmmss.fff")).Return("20150203.221429.501");
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("AGSWORAGS");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("AGSWORAGS_WTR");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "AGSS_INVOICE_S_C_E_00001122.20150203.221429.501.XML"));

			InitaliseCodeMapper(mockCodeMapper);
			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor }
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<UniTrans_2_WWACustomerInvoice>(sourceFile, expectedFile);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}

		void InitaliseCodeMapper(CodeMapper codeMapper)
		{
			SetUnKeyedCodeMapping(codeMapper, "Defaults", "Charge Basis", "LS");

			SetCodeMapping(codeMapper, "Envelope Details", "Sender ID", "AGSWORAGS", "edi_ags_prod");
			SetCodeMapping(codeMapper, "Envelope Details", "Receiver ID", "AGSWORAGS", "WorldWideAlliance");
			SetCodeMapping(codeMapper, "Envelope Details", "Password", "AGSWORAGS", "123456");
			SetCodeMapping(codeMapper, "Envelope Details", "Version", "AGSWORAGS", "1.0.0");
			SetCodeMapping(codeMapper, "Envelope Details", "Type", "AGSWORAGS", "export_invoice_XML_1.0.0");

			SetCodeMapping(codeMapper, "Envelope Details", "Sender ID", "AGSNZGAGS", "edi_akl_prod");
			SetCodeMapping(codeMapper, "Envelope Details", "Receiver ID", "AGSNZGAGS", "NZWorldWideAlliance");
			SetCodeMapping(codeMapper, "Envelope Details", "Password", "AGSNZGAGS", "888888");
			SetCodeMapping(codeMapper, "Envelope Details", "Version", "AGSNZGAGS", "1.1.0");
			SetCodeMapping(codeMapper, "Envelope Details", "Type", "AGSNZGAGS", "export_invoice_XML_1.1.0");

			SetCodeMapping(codeMapper, "SCAC", "AGSNZGAGS", "ANZS");
			SetCodeMapping(codeMapper, "SCAC", "AGSWORAGS", "AGSS");

			SetCodeMapping(codeMapper, "Issuer Tax ID", "Country Code", "AGSWORAGS", "AU");
			SetCodeMapping(codeMapper, "Issuer Tax ID", "Registration Type", "AGSWORAGS", "ABN");
			
			SetCodeMapping(codeMapper, "Issuer Tax ID", "Country Code", "AGSNZGAGS", "NZ");
			SetCodeMapping(codeMapper, "Issuer Tax ID", "Registration Type", "AGSNZGAGS", "CNO");

			SetCodeMapping(codeMapper, "Charge Code", "LA", "LAA");
			SetPassThruCodeMapping(codeMapper, "Charge Code", "LA");

			SetCodeMapping(codeMapper, "Invoice Office Code", "AUADL", "AUADL01");
			SetDefaultCodeMapping(codeMapper, "Invoice Office Code", "AUADL", "DEHAM02");

			SetCodeMapping(codeMapper, "Customer Alias", "EXPORTUNC", "KN");
			SetDefaultCodeMapping(codeMapper, "Customer Alias", "EXPORTUNC", "DHL");

			SetDefaultCodeMapping(codeMapper, "eDocs Format", "", "application/pdf");
			SetPassThruCodeMapping(codeMapper, "Package Type", "");
			SetPassThruCodeMapping(codeMapper, "Tax Type", "");

			SetCodeMapping(codeMapper, "Transaction Type", "INV", "S");
			SetCodeMapping(codeMapper, "Transaction Type", "CRD", "C");

			codeMapper.Expect(x => x.GetRecipientCode("eHub", "eHub", "Common Code Mappings", "Leg Transport Mode", "Code", "Sea")).Return("SEA").Repeat.Any();
			codeMapper.Expect(x => x.GetRecipientCode("eHub", "eHub", "Common Code Mappings", "Leg Transport Mode", "Code", "Road")).Return("ROA").Repeat.Any();
		}

		void SetUnKeyedCodeMapping(CodeMapper codeMapper, string codeSet, string resultField, string outputValue)
		{
			codeMapper.Expect(x => x.GetRecipientCodeUnkeyed(SenderID, RecipientID, InterfaceName, codeSet, resultField)).Return(outputValue).Repeat.Any(); ;
		}
		void SetCodeMapping(CodeMapper codeMapper, string codeSet, string value, string outputValue)
		{
			codeMapper.Expect(x => x.GetRecipientCode(SenderID, RecipientID, InterfaceName, codeSet, value)).Return(outputValue).Repeat.Any(); ;
		}

		void SetCodeMapping(CodeMapper codeMapper, string codeSet, string resultField, string value, string outputValue)
		{
			codeMapper.Expect(x => x.GetRecipientCode(SenderID, RecipientID, InterfaceName, codeSet, resultField, value)).Return(outputValue).Repeat.Any(); ;
		}

		void SetPassThruCodeMapping(CodeMapper codeMapper, string codeSet, string excludeValue)
		{
			codeMapper.Expect(x => x.GetRecipientCode(Arg.Is(SenderID), Arg.Is(RecipientID), Arg.Is(InterfaceName), Arg.Is(codeSet), Arg<string>.Is.NotEqual(excludeValue))).Return("").Repeat.Any()
				.WhenCalled(y => y.ReturnValue = y.Arguments[4]);
		}

		void SetDefaultCodeMapping(CodeMapper codeMapper, string codeSet, string excludeValue, string defaultValue)
		{
			codeMapper.Expect(x => x.GetRecipientCode(Arg.Is(SenderID), Arg.Is(RecipientID), Arg.Is(InterfaceName), Arg.Is(codeSet), Arg<string>.Is.NotEqual(excludeValue))).Return(defaultValue).Repeat.Any(); ;
		}

		const string SenderID = "AGSWORAGS";
		const string RecipientID = "AGSWORAGS_WTR";
		const string InterfaceName = "WWA AR Transaction xml-File - Send A/R Invoices";

		public Stream GetEmbeddedResource(string resourceName)
		{
			var assembly = Assembly.GetExecutingAssembly();
			var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(assembly.GetName().Name + "." + resourceName);
			if (resource == null)
			{
				throw new Exception(string.Format("Could not locate embedded resource '{0}'", resourceName));
			}
			return resource;
		}

		public string ReadResource(string resourceName)
		{
			using (Stream stream = GetEmbeddedResource(resourceName))
			using (StreamReader reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}


	}
}
