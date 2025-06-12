using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.AGS.Transforms.UniTrans_2_WWACustomerInvoice;
using CargoWise.eHub.Core.Transforms.Helper;

namespace CargoWise.eHub.Clients.AGS.Tests
{
	[TestClass]
	public class UniEvent_2_CustomerInvoiceTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniEvent_2_WWACustomerInvoice_NoInvoiceSubscription()
		{
			string sourceFile = "UniEvent_2_WWACustomerInvoice.TestFiles.Input_UniEvent.xml";
			string eventSubscription = XDocument.Load(GetEmbeddedResource("UniEvent_2_WWACustomerInvoice.TestFiles.Event_Subscription.xml")).ToString(SaveOptions.DisableFormatting);
			string expectedFile = "UniEvent_2_WWACustomerInvoice.TestFiles.Output_NoSubscription.xml";

			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("AGSWWA", "AGSWORAGS", "AGSWORAGS_WTR", "DAU_AR_INV_SYD00000057", eventSubscription, "Event")).Repeat.Once();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "AGSWORAGS", "@recipientId", "AGSWORAGS_WTR", "@ST_ID", "AGSWWA", "@value", "DAU_AR_INV_SYD00000057", "@referenceType", "Invoice")).Return("").Repeat.Once();
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("AGSWORAGS");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("AGSWORAGS_WTR");

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor }
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<UniEvent_2_WWACustomerInvoice>(sourceFile, expectedFile);

			mockCodeMapper.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniEvent_2_WWACustomerInvoice_InvoiceSubscriptionExists()
		{
			string sourceFile = "UniEvent_2_WWACustomerInvoice.TestFiles.Input_UniEvent.xml";
			string invoiceSubscription = ReadResource("UniEvent_2_WWACustomerInvoice.TestFiles.Invoice_Subscription.xml");
			string eventSubscription = XDocument.Load(GetEmbeddedResource("UniEvent_2_WWACustomerInvoice.TestFiles.Event_Subscription.xml")).ToString(SaveOptions.DisableFormatting);
			string expectedFile = "UniEvent_2_WWACustomerInvoice.TestFiles.Output_SubscriptionExists.xml";

			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockDataModelAccessor.AssertWasNotCalled(x => x.InsertSubscriptionValue("AGSWWA", "AGSWORAGS", "AGSWORAGS_WTR", "DAU_AR_INV_SYD00000057", eventSubscription, "Event"));
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "AGSWORAGS", "@recipientId", "AGSWORAGS_WTR", "@ST_ID", "AGSWWA", "@value", "DAU_AR_INV_SYD00000057", "@referenceType", "Invoice")).Return(invoiceSubscription).Repeat.Once();
			mockCodeMapper.Expect(x => x.GetRecipientCode("AGSWORAGS", "AGSWORAGS_WTR", "WWA AR Transaction xml-File - Send A/R Invoices", "eDocs Format", "pdf")).Return("application/pdf").Repeat.Once();
			mockDateMapper.Expect(x => x.CurrentDateTime("yyyyMMdd.HHmmss.fff")).Return("20201222.045428.501");
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("AGSWORAGS");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("AGSWORAGS_WTR");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "AGSS_INVOICE_C_C_I_00001000.20201222.045428.501.XML"));

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor }
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<UniEvent_2_WWACustomerInvoice>(sourceFile, expectedFile);

			mockDateMapper.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();

		}

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
