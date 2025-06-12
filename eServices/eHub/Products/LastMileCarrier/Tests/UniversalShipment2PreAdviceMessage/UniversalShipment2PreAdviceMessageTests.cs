using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.Products.LastMileCarrier.Transforms.UniversalShipment2PreAdviceMessage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.LastMileCarrier.Tests
{
	[TestClass]
	public class UniversalShipment2PreAdviceMessageTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2PreAdviceMessage()
		{
			InitialiseCodeMapsTestingContext();
			var ctx = InitialiseTestingMessageContext("APOMELMEL");

			List<string> exclusionXpaths = new List<string>();
			exclusionXpaths.Add("//*[local-name()='MessageDatetime']");
			ICompare comparer = new ExcludingComparer(exclusionXpaths);
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

			string sourceFile = "UniversalShipment2PreAdviceMessage.TestFiles.UniversalShipment2PreAdviceMessage_input.xml";
			string expectedFile = "UniversalShipment2PreAdviceMessage.TestFiles.UniversalShipment2PreAdviceMessage_output.xml";
			mapTester.Execute<UniversalShipment2PreAdviceMessage>(sourceFile, expectedFile);
		}

		static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();

			ctx.eHubClients.Add(new eHubClient { CC_ID = "APOMELMEL_NPO" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "APOMELMEL_NPO" });

			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "PreAdvice Messsage - Export Transport Bookings", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Account Number", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "NZCourier Code" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "APOMELMEL" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "ACCNo" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Service Level", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "NZCourier Code" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "APOMELMEL", CK_Key2Value = "STD" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "CPOLE" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Line Limit" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "3" });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}

		static TestingMessageContext InitialiseTestingMessageContext(string sourceParty)
		{
			var ctx = new TestingMessageContext();
			ctx.Write("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", sourceParty);
			var ca = new ContextAccessor();
			ca.SetTestingMessageContext(ctx);
			return ctx;
		}
	}
}
