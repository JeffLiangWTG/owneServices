using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.Products.LastMileCarrier.Transforms.ICOSStatus2UniEventInterchange;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cargowise.eHub.Products.LastMileCarrier.Tests
{
	[TestClass]
	public class ICOSStatus2UniEventInterchangeTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestICOSStatus2ICOSStatusEnvelope()
		{
			var mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "ICOSStatus2UniEventInterchange.TestFiles.Grouping_Input.xml";
			string expectedFile = "ICOSStatus2UniEventInterchange.TestFiles.Grouping_Output.xml";
			mapTester.Execute<ICOSStatus2ICOSStatusEnvelope>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestICOSStatus2UniEventInterchange()
		{
			InitializeCodeMapsTestingContext();
			var ctx = InitialiseTestingMessageContext();

			var mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "ICOSStatus2UniEventInterchange.TestFiles.Input.xml";
			string expectedFile = "ICOSStatus2UniEventInterchange.TestFiles.Output.xml";
			mapTester.Execute<ICOSStatus2UniEventInterchange>(sourceFile, expectedFile);

			Assert.AreEqual("APOMELMEL", ctx.Read("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties"));
		}

		#region Implementation

		static void InitializeCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();

			ctx.eHubClients.Add(new eHubClient { CC_ID = "LMCSERVICES" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "LMCSERVICES" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "ICOS Messages - Import Status Update", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Status Events", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Output Code" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "D" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "BKC" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "" });

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

		#endregion
	}
}

