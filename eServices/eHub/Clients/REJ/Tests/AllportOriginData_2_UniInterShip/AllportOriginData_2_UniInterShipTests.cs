using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.REJ.Transforms.AllportOriginData_2_UniInterShip;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.REJ.Tests
{
	[TestClass]
	public class AllportOriginData_2_UniInterShipTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAllportOriginData_2_UniInterShip()
		{
			InitialiseCodeMapsTestingContext();

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "AllportOriginData_2_UniInterShip.TestFiles.Input.xml";
			string expectedFile = "AllportOriginData_2_UniInterShip.TestFiles.Output.xml";
			mapTester.Execute<AllportOriginData_2_UniInterShip>(sourceFile, expectedFile);

			sourceFile = "AllportOriginData_2_UniInterShip.TestFiles.MultipleConsols_Input.xml";
			expectedFile = "AllportOriginData_2_UniInterShip.TestFiles.MultipleConsols_Output.xml";
			mapTester.Execute<AllportOriginData_2_UniInterShip>(sourceFile, expectedFile);

			sourceFile = "AllportOriginData_2_UniInterShip.TestFiles.Input_container.xml";
			expectedFile = "AllportOriginData_2_UniInterShip.TestFiles.Output_container.xml";
			mapTester.Execute<AllportOriginData_2_UniInterShip>(sourceFile, expectedFile);

			sourceFile = "AllportOriginData_2_UniInterShip.TestFiles.Orders_Input.xml";
			expectedFile = "AllportOriginData_2_UniInterShip.TestFiles.Orders_Output.xml";
			mapTester.Execute<AllportOriginData_2_UniInterShip>(sourceFile, expectedFile);

			sourceFile = "AllportOriginData_2_UniInterShip.TestFiles.Large_Input.xml";
			expectedFile = "AllportOriginData_2_UniInterShip.TestFiles.Large_Output.xml";
			mapTester.Execute<AllportOriginData_2_UniInterShip>(sourceFile, expectedFile);
		}

		static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "REJMELMEL_CSF" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "REJMELMEL" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "CSFE Manifest xml - Import Consols & Shipments", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Transport Mode" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "SEA" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 2, CR_Name = "Container Mode" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "FCL" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 3, CR_Name = "Inner Pack Type" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "PCE" });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}
