using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.AGC.Transforms.BillOfLading2UniversalShipment;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.AGC.Tests
{
	[TestClass]
	public class BillOfLading2UniversalShipmentTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestBillOfLading2UniversalShipment()
		{
			InitialiseCodeMapsTestingContext();

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "BillOfLading2UniversalShipment.TestFiles.BillOfLading2UniversalShipment_input.xml";
			string expectedFile = "BillOfLading2UniversalShipment.TestFiles.BillOfLading2UniversalShipment_output.xml";

			mapTester.Execute<BillOfLading2UniversalShipment>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestBillOfLading2UniversalShipment_ContainerType()
		{
			InitialiseCodeMapsTestingContext();

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "BillOfLading2UniversalShipment.TestFiles.BillOfLading2UniversalShipment_ContainerType_input.xml";
			string expectedFile = "BillOfLading2UniversalShipment.TestFiles.BillOfLading2UniversalShipment_ContainerType_output.xml";

			mapTester.Execute<BillOfLading2UniversalShipment>(sourceFile, expectedFile);
		}

		private static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "AGCUJVUJV_HLS" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "AGCUJVUJV" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "HLS Bill of Lading xml - Import Consols&Shipments", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Transport Mode" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "SEA" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Unit of Measurement", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "CargoWiseOne Code" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "KGM" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "KG" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "MTQ" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "M3" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Payment Method", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "CargoWiseOne Code" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "prepaid" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "PPD" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "collect" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "CCK" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Container Mode", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Consol Container Mode" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 2, CR_Name = "Container Container Mode" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 3, CR_Name = "Shipment Container Mode" });

			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "CY/CY", CK_Key2Value = "" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[ctx.eHubCodeSetResults.Count - 3], CV_OutputCode = "CFCL" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[ctx.eHubCodeSetResults.Count - 2], CV_OutputCode = "TFCL" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[ctx.eHubCodeSetResults.Count - 1], CV_OutputCode = "SFCL" });

			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "CFS/CFS", CK_Key2Value = "GRP" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[ctx.eHubCodeSetResults.Count - 3], CV_OutputCode = "GRP" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[ctx.eHubCodeSetResults.Count - 2], CV_OutputCode = "FCL" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[ctx.eHubCodeSetResults.Count - 1], CV_OutputCode = "LCL" });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}
