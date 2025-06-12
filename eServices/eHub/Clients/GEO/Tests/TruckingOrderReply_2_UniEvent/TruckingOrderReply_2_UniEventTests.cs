using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.GEO.Transforms.TruckingOrderReply_2_UniEvent;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;

namespace CargoWise.eHub.Clients.GEO.Tests
{
	[TestClass]
	public class TruckingOrderReply_2_UniEventTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestTruckingOrderReply_2_UniEvent()
		{
			InitialiseCodeMapsTestingContext();

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "TruckingOrderReply_2_UniEvent.TestFiles.Accepted_Input.xml";
			string expectedFile = "TruckingOrderReply_2_UniEvent.TestFiles.Accepted_Output.xml";
			mapTester.ExecuteCompiled<TruckingOrderReply_2_UniEvent>(sourceFile, expectedFile);

			sourceFile = "TruckingOrderReply_2_UniEvent.TestFiles.Rejected_Input.xml";
			expectedFile = "TruckingOrderReply_2_UniEvent.TestFiles.Rejected_Output.xml";
			mapTester.ExecuteCompiled<TruckingOrderReply_2_UniEvent>(sourceFile, expectedFile);
		}

		static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "GEOGFRPRO_EXW" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "GEOGFRPRO" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "ExWorks XML GEO - Receive Response Events", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Event Code", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "EventType" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "Accepted" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "ATH" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "Rejected" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "JED" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 99, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "" });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}
