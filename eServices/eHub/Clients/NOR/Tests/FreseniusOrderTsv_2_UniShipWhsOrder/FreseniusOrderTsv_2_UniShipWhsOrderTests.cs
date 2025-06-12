using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.NOR.Transforms.FreseniusOrderTsv_2_UniShipWhsOrder;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.NOR.Tests
{
	[TestClass]
	public class FreseniusOrderTsv_2_UniShipWhsOrderTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFreseniusOrderTsv_2_UniShipWhsOrder()
		{
			InitialiseCodeMapsTestingContext();
			InitialiseBizTalkTestingContext("ftpex://Fresenius.web@northlineftp.northline.com.au:21/CW/Adelaide");

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "FreseniusOrderTsv_2_UniShipWhsOrder.TestFiles.Input.xml";
			string expectedFile = "FreseniusOrderTsv_2_UniShipWhsOrder.TestFiles.Output.xml";
			mapTester.Execute<FreseniusOrderTsv_2_UniShipWhsOrder>(sourceFile, expectedFile);
		}

		private static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "NORMELHST_FWO" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "NORMELHST" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "Fresenius - Receive Warehouse Orders", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Data Provider" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "FREAUS001" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Warehouse Code", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Warehouse Code" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "%Adelaide%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "15" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 99, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "666" });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}

		static void InitialiseBizTalkTestingContext(string location)
		{
			var ctx = new TestingMessageContext();
			ctx.Write("InboundTransportLocation", "http://schemas.microsoft.com/BizTalk/2003/system-properties", location);
			var ca = new ContextAccessor();
			ca.SetTestingMessageContext(ctx);
		}
	}
}
