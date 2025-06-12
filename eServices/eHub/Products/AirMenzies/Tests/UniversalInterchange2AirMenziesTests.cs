using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.AirMenzies.Transforms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.AirMenzies.Tests
{
	[TestClass]
	public class UniversalInterchange2AirMenziesTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalInterchange2AirMenziesTests()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			string sourceFile = "TestFiles.UniversalInterchange2AirMenziesTests_input.xml";
			string expectedFile = "TestFiles.UniversalInterchange2AirMenziesTests_output.xml";

			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "HYEUATBNE_CM00000067_2011-07-06T18.44.00"));

			var extensionObjects = new Dictionary<string, object>() { 
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor }
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<UniversalInterchange2AirMenzies>(sourceFile, expectedFile);

			mockContextAccessor.VerifyAllExpectations();
		}
	}
}
