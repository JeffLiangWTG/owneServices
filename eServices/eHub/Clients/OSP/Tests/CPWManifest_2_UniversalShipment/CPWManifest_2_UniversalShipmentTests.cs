using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.OSP.Transforms.CPWManifest_2_UniversalShipment;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Clients.OSP.Tests
{
	[TestClass]
	public class CPWManifest_2_UniversalShipmentTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCPWManifest_2_UniversalShipment()
		{
			var mockCodeMapper = GetMockCodeMapper();

			var extensionObjects = new Dictionary<string, object>() { 
			     { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile = "CPWManifest_2_UniversalShipment.TestFiles.CPWManifest_2_UniversalShipment_input.xml";
			string expectedFile = "CPWManifest_2_UniversalShipment.TestFiles.CPWManifest_2_UniversalShipment_output.xml";

			mapTester.Execute<CPWManifest_2_UniversalShipment>(sourceFile, expectedFile);
		}

		private static CodeMapper GetMockCodeMapper()
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("OSPAKLAKL_CPW", "OSPAKLAKL", "C P World Flat File-Import Consols&Shipments", "Defaults", "Transport Mode")).Return("SEA");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("OSPAKLAKL_CPW", "OSPAKLAKL", "C P World Flat File-Import Consols&Shipments", "Defaults", "Weight Unit")).Return("KG");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("OSPAKLAKL_CPW", "OSPAKLAKL", "C P World Flat File-Import Consols&Shipments", "Defaults", "Volume Unit")).Return("M3");
			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("OSPAKLAKL_CPW", "OSPAKLAKL", "C P World Flat File-Import Consols&Shipments", "Defaults", "Commodity")).Return("GEN");

			mockCodeMapper.Expect(x => x.GetRecipientCode("OSPAKLAKL_CPW", "OSPAKLAKL", "C P World Flat File-Import Consols&Shipments", "Container Mode", "Container Delivery Mode", "1")).Return("CY/CY");
			mockCodeMapper.Expect(x => x.GetRecipientCode("OSPAKLAKL_CPW", "OSPAKLAKL", "C P World Flat File-Import Consols&Shipments", "Container Mode", "Container Container Mode", "1")).Return("FCL");
			mockCodeMapper.Expect(x => x.GetRecipientCode("OSPAKLAKL_CPW", "OSPAKLAKL", "C P World Flat File-Import Consols&Shipments", "Container Mode", "Consol Container Mode", "1")).Return("FCL");
			mockCodeMapper.Expect(x => x.GetRecipientCode("OSPAKLAKL_CPW", "OSPAKLAKL", "C P World Flat File-Import Consols&Shipments", "Container Mode", "Shipment Container Mode", "1")).Return("FCL");

			mockCodeMapper.Expect(x => x.GetRecipientCode("OSPAKLAKL_CPW", "OSPAKLAKL", "C P World Flat File-Import Consols&Shipments", "Container Mode", "Container Delivery Mode", "2")).Return("CY/CFS");
			mockCodeMapper.Expect(x => x.GetRecipientCode("OSPAKLAKL_CPW", "OSPAKLAKL", "C P World Flat File-Import Consols&Shipments", "Container Mode", "Container Container Mode", "2")).Return("FCL");
			mockCodeMapper.Expect(x => x.GetRecipientCode("OSPAKLAKL_CPW", "OSPAKLAKL", "C P World Flat File-Import Consols&Shipments", "Container Mode", "Consol Container Mode", "2")).Return("GRP");
			mockCodeMapper.Expect(x => x.GetRecipientCode("OSPAKLAKL_CPW", "OSPAKLAKL", "C P World Flat File-Import Consols&Shipments", "Container Mode", "Shipment Container Mode", "2")).Return("LCL");

			mockCodeMapper.Expect(x => x.GetRecipientCode("OSPAKLAKL_CPW", "OSPAKLAKL", "C P World Flat File-Import Consols&Shipments", "Container Mode", "Container Delivery Mode", "3")).Return("CFS/CFS").Repeat.Times(6);
			mockCodeMapper.Expect(x => x.GetRecipientCode("OSPAKLAKL_CPW", "OSPAKLAKL", "C P World Flat File-Import Consols&Shipments", "Container Mode", "Container Container Mode", "3")).Return("LCL").Repeat.Times(6);
			mockCodeMapper.Expect(x => x.GetRecipientCode("OSPAKLAKL_CPW", "OSPAKLAKL", "C P World Flat File-Import Consols&Shipments", "Container Mode", "Consol Container Mode", "3")).Return("LCL").Repeat.Times(6);
			mockCodeMapper.Expect(x => x.GetRecipientCode("OSPAKLAKL_CPW", "OSPAKLAKL", "C P World Flat File-Import Consols&Shipments", "Container Mode", "Shipment Container Mode", "3")).Return("LCL").Repeat.Times(6);

			mockCodeMapper.Expect(x => x.GetRecipientCode("OSPAKLAKL_CPW", "OSPAKLAKL", "C P World Flat File-Import Consols&Shipments", "Payment Method", "Output Code", "P")).Return("PPD").Repeat.Twice();
			mockCodeMapper.Expect(x => x.GetRecipientCode("OSPAKLAKL_CPW", "OSPAKLAKL", "C P World Flat File-Import Consols&Shipments", "Payment Method", "Service Level", "P")).Return("CPR").Repeat.Times(6);
			mockCodeMapper.Expect(x => x.GetRecipientCode("OSPAKLAKL_CPW", "OSPAKLAKL", "C P World Flat File-Import Consols&Shipments", "Payment Method", "Output Code", "C")).Return("CCX");
			mockCodeMapper.Expect(x => x.GetRecipientCode("OSPAKLAKL_CPW", "OSPAKLAKL", "C P World Flat File-Import Consols&Shipments", "Payment Method", "Service Level", "C")).Return("");

			mockCodeMapper.Expect(x => x.GetRecipientCode("OSPAKLAKL_CPW", "OSPAKLAKL", "C P World Flat File-Import Consols&Shipments", "Release Type", "Output Code", "1")).Return("TLX").Repeat.Times(4);
			mockCodeMapper.Expect(x => x.GetRecipientCode("OSPAKLAKL_CPW", "OSPAKLAKL", "C P World Flat File-Import Consols&Shipments", "Release Type", "Output Code", "0")).Return("NTX").Repeat.Times(3);

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetUNLOCOFromLocation", "@result", "@city", "SYDNEY", "@country", "AU")).Return("AUSYD");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetUNLOCOFromLocation", "@result", "@city", "TTT", "@country", "AU")).Return("");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetUNLOCOFromLocation", "@result", "@city", "ADELAIDE", "@country", "AU")).Return("AUADL").Repeat.Any();

			return mockCodeMapper;
		}
	}
}
