using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.Products.JPCustoms.Transforms.SystemCommonErrorResponse2UniversalEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace CargoWise.eHub.Products.JPCustoms.Tests
{
	[TestClass]
	public class SystemCommonErrorResponse2UniversalEventTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SystemCommonErrorResponse2UniversalEventTest1()
		{
			InitialiseMessageTestingContext();
			InitialiseCodeMapsTestingContext();

			string sourceFile = "SystemCommonErrorResponse2UniversalEvent_input.input_1.xml";
			string outputFile = "SystemCommonErrorResponse2UniversalEvent_output.output_1.xml";

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			mapTester.Execute<SystemCommonErrorResponse2UniversalEvent>(sourceFile, outputFile);
		}

		static void InitialiseMessageTestingContext() { }

		static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "JPCustoms" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "JP AFR - Import System Common Error", eHubClient_Sender = ctx.eHubClients[0] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Field", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[0], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "ErrorDetail", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[0], eHubTransformationSet = ctx.eHubTransformationSets[0] });

			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 0, CR_Name = "ErrorDescription" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "A0004" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "A0004 Description" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "No detailed description for this error" });

			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 2, CR_Name = "ErrorSolution" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "A0004" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "A0004 Solution" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "A0008" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "NoSolution" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "" });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}