using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.XXX.Transforms.UniShip_2_UniforceHST_TxStatusNotif_XML;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.XXX.Tests
{	
	[TestClass]
	public class UniShip_2_UniforceHST_TxStatusNotif_XMLTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniShip_2_UniforceHST_TxStatusNotif_XML()
		{
			InitialiseCodeMapsTestingContext();

			List<string> exclusionXpaths = new List<string>();
			exclusionXpaths.Add("//*[local-name()='CreationDateAndTime']");
			exclusionXpaths.Add("//*[local-name()='creationDateTime']");
			ICompare comparer = new ExcludingComparer(exclusionXpaths);

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

			string sourceFile = "UniShip_2_UniforceHST_TxStatusNotif_XML.TestFiles.01_Consol_Sea_Input.xml";
			string expectedFile = "UniShip_2_UniforceHST_TxStatusNotif_XML.TestFiles.01_Consol_Sea_Output.xml";
			mapTester.ExecuteCompiled<UniShip_2_UniforceHST_TxStatusNotif_XML>(sourceFile, expectedFile);

			sourceFile = "UniShip_2_UniforceHST_TxStatusNotif_XML.TestFiles.02_Consol_Air_Input.xml";
			expectedFile = "UniShip_2_UniforceHST_TxStatusNotif_XML.TestFiles.02_Consol_Air_Output.xml";
			mapTester.ExecuteCompiled<UniShip_2_UniforceHST_TxStatusNotif_XML>(sourceFile, expectedFile);

			sourceFile = "UniShip_2_UniforceHST_TxStatusNotif_XML.TestFiles.03_NoID_Input.xml";
			expectedFile = "UniShip_2_UniforceHST_TxStatusNotif_XML.TestFiles.03_NoID_Output.xml";
			mapTester.ExecuteCompiled<UniShip_2_UniforceHST_TxStatusNotif_XML>(sourceFile, expectedFile);
		}

		static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "XXXUSAOR1" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "XXXUSAOR1_HTS" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "Send HST Status XML files from Consols & Shipments", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = null });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Sender" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "SSS123" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 2, CR_Name = "Receiver" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "RRR456" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Status Code", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Status Code" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "023" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "U023" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "040" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "U040" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 99, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_PassThroughKey = 1 });
			
			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}
