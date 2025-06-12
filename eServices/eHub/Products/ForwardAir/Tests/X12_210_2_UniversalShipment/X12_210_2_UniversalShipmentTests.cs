using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardAir.Transforms.X12_210_2_UniversalShipment;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardAir.Tests
{
	[TestClass]
	public class X12_210_2_UniversalShipmentTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestX12_210_2_UniversalShipment()
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "FORAIRCMH", "@recipientId", "", "@ST_ID", "FORAWB", "@value", "03648060")).Return("C00024641");
			mockDateMapper.Stub(x => x.CurrentDateTimeUTC("u")).Return("2015-11-18 04:41:02Z");
			mockContextAccessor.Stub(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("AAABBBCCC");
			var extensionObjects = new Dictionary<string, object>() 
			{ 
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile = "X12_210_2_UniversalShipment.TestFiles.Test1_input.xml";
			string expectedFile = "X12_210_2_UniversalShipment.TestFiles.Test1_output.xml";
			mapTester.Execute<X12_210_2_UniversalShipment>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestX12_210_2_UniversalShipment_NoConsol()
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "FORAIRCMH", "@recipientId", "", "@ST_ID", "FORAWB", "@value", "03648060")).Return("");
			mockDateMapper.Stub(x => x.CurrentDateTimeUTC("u")).Return("2015-11-18 04:41:02Z");
			mockContextAccessor.Stub(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("AAABBBCCC");
			var extensionObjects = new Dictionary<string, object>() 
			{ 
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile = "X12_210_2_UniversalShipment.TestFiles.Test2_input.xml";
			string expectedFile = "X12_210_2_UniversalShipment.TestFiles.Test2_output.xml";
			mapTester.Execute<X12_210_2_UniversalShipment>(sourceFile, expectedFile);
		}
	}
}
