using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.Products.LastMileCarrier.Transforms.PreadviceResponse2UniversalEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.LastMileCarrier.Tests
{
	[TestClass]
	public class PreadviceResponse2UniversalEventTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestPreadviceResponse2UniversalEvent()
		{
			InitialiseCodeMapsTestingContext();
			var ctx = InitialiseTestingMessageContext();

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "PreadviceResponse2UniversalEvent.TestFiles.PreadviceResponse2UniversalEvent_input.xml";
			string expectedFile = "PreadviceResponse2UniversalEvent.TestFiles.PreadviceResponse2UniversalEvent_output.xml";
			mapTester.Execute<PreadviceResponse2UniversalEvent>(sourceFile, expectedFile);

			Assert.AreEqual("APOMELMEL", ctx.Read("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties"));
		}

		static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();

			ctx.eHubClients.Add(new eHubClient { CC_ID = "APOMELMEL_NPO" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "APOMELMEL_NPO" });

			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "PreAdvice Response - Import TB Event", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Event Code", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Event Type" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "AP" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "ACK" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "MSC" });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}

		static TestingMessageContext InitialiseTestingMessageContext()
		{
			var ctx = new TestingMessageContext();
			var ca = new ContextAccessor();
			ca.SetTestingMessageContext(ctx);
			return ctx;
		}
	}
}
