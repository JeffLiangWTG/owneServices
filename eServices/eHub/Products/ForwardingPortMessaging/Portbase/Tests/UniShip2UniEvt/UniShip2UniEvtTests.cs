using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.Portbase.Transforms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.Portbase.Tests
{
	[TestClass]
	public class UniShip2UniEvtTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniShip2UniEvtTest()
		{
			const string filePath = "UniShip2UniEvt.TestFiles.";
			var input = filePath + "Test1_input.xml";
			var expectedOutput = filePath + "Test1_output.xml";

			var mockDateMapper = MockRepository.GenerateMock<DateMapper>();
			var mockContextAccessor = MockRepository.GenerateMock<ContextAccessor>();
			mockDateMapper.Stub(x => x.CurrentDateTime(Arg.Is("s"))).Return("2014-09-09T09:30:10");
			mockContextAccessor.Stub(x => x.GetContextProperty("ErrorCode", "http://cargowise.com/ehub/routing/2010/06")).Return("IRJ");
			mockContextAccessor.Stub(x => x.GetContextProperty("ErrorDescription", "http://cargowise.com/ehub/routing/2010/06")).Return("Department=WiseTechGlobal|Reason=You are not registered with this service. Contact WTG to register.");
			var extensionObjects = new Dictionary<string, object>() 
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<UniShip2UniEvt>(input, expectedOutput);
		}
	}
}
