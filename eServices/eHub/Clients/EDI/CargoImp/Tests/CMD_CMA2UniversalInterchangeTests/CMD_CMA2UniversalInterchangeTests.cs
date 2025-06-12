using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.Transforms.CargoImp.CMD_CMA2UniversalInterchange;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.BizTalk.TestTools.Schema;
using Microsoft.BizTalk.TestTools.Mapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Clients.EDI.Schemas.CargoImp.Tests.CMD_CMA2UniversalInterchangeTests
{
	[TestClass]
	public class CMD_CMA2UniversalInterchangeTests
	{

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CMD_CMA2UniversalInterchange_Short()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CCN");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("WTLDSGSGC");
			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor }
			};
			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			var sourceFile = "CMD_CMA_XSDTests.TestFiles.CMDCMAShort.xml";
			var expectedFile = "CMD_CMA2UniversalInterchangeTests.TestFiles.CMDCMA2UniversalInterchangeShort_output.xml";

			mapTester.Execute<CMD_CMA2UniversalInterchange>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CMD_CMA2UniversalInterchange_Long()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CCN");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("WTLDSGSGC");
			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor }
			};
			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			var sourceFile = "CMD_CMA_XSDTests.TestFiles.CMDCMALong.xml";
			var expectedFile = "CMD_CMA2UniversalInterchangeTests.TestFiles.CMDCMA2UniversalInterchangeLong_output.xml";

			mapTester.Execute<CMD_CMA2UniversalInterchange>(sourceFile, expectedFile);
		}
	}
}
