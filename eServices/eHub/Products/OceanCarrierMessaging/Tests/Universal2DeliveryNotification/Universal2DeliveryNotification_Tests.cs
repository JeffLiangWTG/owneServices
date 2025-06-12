using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Universal2DeliveryNotification;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
	[TestClass]
	public class Universal2DeliveryNotification_Tests
	{
		const string filePath = "Universal2DeliveryNotification.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Universal2DeliveryNotification_INTTRA_IRJ()
		{
			AssertMapping("Test1_input.xml", "Test1_output.xml");
			AssertMapping("Test2_input.xml", "Test2_output.xml");
		}

		void AssertMapping(string inputFile, string expectedOutputFile)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("ErrorCode", "http://cargowise.com/ehub/routing/2010/06")).Return("IRJ");
			mockContextAccessor.Expect(x => x.GetContextProperty("ErrorDescription", "http://cargowise.com/ehub/routing/2010/06")).Return("Department=WiseTechGlobal|Reason=You are not registered with this Carrier. Contact WTG to register.");
			mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("000");
			mockContextAccessor.Expect(x => x.GetContextProperty("MessageTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("111");
			var extensionObjects = new Dictionary<string, object>() 
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor }
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<Universal2DeliveryNotification>(input, expectedOutput);

			mockDateMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}
	}
}

