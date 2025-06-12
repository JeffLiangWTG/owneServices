using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.CET.Transforms.IFTMCS_2_UniversalShipment;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Clients.CET.Tests
{
	[TestClass]
	public class IFTMCS_2_UniversalShipmentTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestIFTMCS_2_UniversalShipment()
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("CETKOPKOP_KLB", "CETKOPKOP", "K-Line IFTMCS - Receive Shipping Manager BOLs", "Defaults", "Data Provider")).Return("CET");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("CETKOPKOP_KLB", "CETKOPKOP", "K-Line IFTMCS - Receive Shipping Manager BOLs", "Defaults", "Transport Mode")).Return("SEA");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("CETKOPKOP_KLB", "CETKOPKOP", "K-Line IFTMCS - Receive Shipping Manager BOLs", "Defaults", "Freight Charge Code")).Return("DOC");
			mockCodeMapper.Expect(x => x.GetRecipientCode("CETKOPKOP_KLB", "CETKOPKOP", "K-Line IFTMCS - Receive Shipping Manager BOLs", "Payment Method", "P")).Return("PPD");
			mockCodeMapper.Expect(x => x.GetRecipientCode("CETKOPKOP_KLB", "CETKOPKOP", "K-Line IFTMCS - Receive Shipping Manager BOLs", "Container Type", "4510")).Return("45HC");
			mockCodeMapper.Expect(x => x.GetRecipientCode("CETKOPKOP_KLB", "CETKOPKOP", "K-Line IFTMCS - Receive Shipping Manager BOLs", "Container Type", "2210")).Return("20HC");
			mockCodeMapper.Expect(x => x.GetRecipientCode("CETKOPKOP_KLB", "CETKOPKOP", "K-Line IFTMCS - Receive Shipping Manager BOLs", "Unit of Measurement", "KGM")).Return("KG").Repeat.Twice();
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CETKOPKOP");

			var extensionObjects = new Dictionary<string, object>() { 
			     { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
					 { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile = "IFTMCS_2_UniversalShipment.TestFiles.IFTMCS_2_UniversalShipment_input.xml";
			string expectedFile = "IFTMCS_2_UniversalShipment.TestFiles.IFTMCS_2_UniversalShipment_output.xml";
			mapTester.Execute<IFTMCS_2_UniversalShipment>(sourceFile, expectedFile);

			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestIFTMCS_2_UniversalShipment_2()
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("CETKOPKOP_KLB", "CETKOPKOP", "K-Line IFTMCS - Receive Shipping Manager BOLs", "Defaults", "Data Provider")).Return("CET");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("CETKOPKOP_KLB", "CETKOPKOP", "K-Line IFTMCS - Receive Shipping Manager BOLs", "Defaults", "Transport Mode")).Return("SEA");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("CETKOPKOP_KLB", "CETKOPKOP", "K-Line IFTMCS - Receive Shipping Manager BOLs", "Defaults", "Freight Charge Code")).Return("DOC");
			mockCodeMapper.Expect(x => x.GetRecipientCode("CETKOPKOP_KLB", "CETKOPKOP", "K-Line IFTMCS - Receive Shipping Manager BOLs", "Payment Method", "C")).Return("CLT");
			mockCodeMapper.Expect(x => x.GetRecipientCode("CETKOPKOP_KLB", "CETKOPKOP", "K-Line IFTMCS - Receive Shipping Manager BOLs", "Container Type", "4510")).Return("45HC");
			mockCodeMapper.Expect(x => x.GetRecipientCode("CETKOPKOP_KLB", "CETKOPKOP", "K-Line IFTMCS - Receive Shipping Manager BOLs", "Unit of Measurement", "KGM")).Return("KG").Repeat.Twice();
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CETKOPKOP");

			var extensionObjects = new Dictionary<string, object>() { 
			     { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
					 { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile = "IFTMCS_2_UniversalShipment.TestFiles.IFTMCS_2_UniversalShipment_input2.xml";
			string expectedFile = "IFTMCS_2_UniversalShipment.TestFiles.IFTMCS_2_UniversalShipment_output2.xml";
			mapTester.Execute<IFTMCS_2_UniversalShipment>(sourceFile, expectedFile);

			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}
	}
}
