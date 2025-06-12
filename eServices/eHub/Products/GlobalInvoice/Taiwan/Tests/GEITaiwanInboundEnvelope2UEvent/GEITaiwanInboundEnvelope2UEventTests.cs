using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.GlobalInvoice.Taiwan.Transforms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
namespace CargoWise.eHub.Products.GlobalInvoice.Taiwan.Tests
{
	[TestClass]
	public class GEITaiwanInboundEnvelope2UEventTests
	{
		const string filePath = "GEITaiwanInboundEnvelope2UEvent.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGEITaiwanInboundEnvelope2UEvent()
		{
			var input = filePath + "Test_input.xml";
			var expectedOutput = filePath + "Test_output.xml";

			var mockDateMapper = MockRepository.GenerateMock<DateMapper>();
			var mockContextAccessor = MockRepository.GenerateMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			mockDateMapper.Stub(x => x.CurrentDateTime(Arg.Is("s"))).Return("2014-09-09T09:30:10");
			mockContextAccessor.Stub(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("GEI_Taiwan");
			mockContextAccessor.Stub(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")).Return("52889317-InvoiceMD-52889317-Paper-20230504-152858073-20230504-154003");
			mockContextAccessor.Stub(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("AAABBBCCC");

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", "GEI_Taiwan", "@ST_ID", "GEIMSG", "@value", "52889317-InvoiceMD-52889317-Paper-20230504-152858073")).Return("AAABBBCCC").Repeat.Once();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "GEI_Taiwan", "@recipientId", "AAABBBCCC", "@ST_ID", "GEIMSG", "@value", "52889317-InvoiceMD-52889317-Paper-20230504-152858073")).Return("5").Repeat.Once();

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockCodeMapper }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<GEITaiwanInboundEnvelope2UEvent>(input, expectedOutput);

			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDateMapper.VerifyAllExpectations();
		}
	}
}
