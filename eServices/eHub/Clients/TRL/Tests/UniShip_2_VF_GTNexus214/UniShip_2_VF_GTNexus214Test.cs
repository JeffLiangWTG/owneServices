using System.Linq;
using System.Reflection;

using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.TRL.Transforms.UniShip_2_VF_GTNexus214;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace CargoWise.eHub.Clients.TRL.Tests
{
	[TestClass]
	public class UniShip_2_VF_GTNexus214Test
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniShip_2_VF_GTNexus214()
		{
            var ctx = new TestingMessageContext();
            var ca = new ContextAccessor();
            ca.SetTestingMessageContext(ctx);

			InitialiseCodeMapsTestingContext();
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "UniShip_2_VF_GTNexus214.TestFiles.AF_Input.xml";
			string expectedFile = "UniShip_2_VF_GTNexus214.TestFiles.AF_Output.xml";
            ctx.Write("GS03", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "VFCORP");

			mapTester.Execute<UniShip_2_VF_GTNexus214>(sourceFile, expectedFile);

			sourceFile = "UniShip_2_VF_GTNexus214.TestFiles.J1_Input.xml";
			expectedFile = "UniShip_2_VF_GTNexus214.TestFiles.J1_Output.xml";
			mapTester.Execute<UniShip_2_VF_GTNexus214>(sourceFile, expectedFile);
            Assert.AreEqual(ctx.Read("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties"), "true");
            Assert.AreEqual(ctx.Read("GS03", "http://schemas.microsoft.com/BizTalk/2006/edi-properties"), "KONTR");

			sourceFile = "UniShip_2_VF_GTNexus214.TestFiles.X6_Input.xml";
			expectedFile = "UniShip_2_VF_GTNexus214.TestFiles.X6_Output.xml";
            ctx.Write("GS03", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "VFCORP");

			mapTester.Execute<UniShip_2_VF_GTNexus214>(sourceFile, expectedFile);
            Assert.AreEqual(ctx.Read("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties"), "true");
            Assert.AreEqual(ctx.Read("GS03", "http://schemas.microsoft.com/BizTalk/2006/edi-properties"), "VFCORP");

			sourceFile = "UniShip_2_VF_GTNexus214.TestFiles.B6_Input1.xml";
			expectedFile = "UniShip_2_VF_GTNexus214.TestFiles.B6_Output1.xml";
			mapTester.Execute<UniShip_2_VF_GTNexus214>(sourceFile, expectedFile);

			sourceFile = "UniShip_2_VF_GTNexus214.TestFiles.B6_Input2.xml";
			expectedFile = "UniShip_2_VF_GTNexus214.TestFiles.B6_Output2.xml";
			mapTester.Execute<UniShip_2_VF_GTNexus214>(sourceFile, expectedFile);

			sourceFile = "UniShip_2_VF_GTNexus214.TestFiles.D1_Input.xml";
			expectedFile = "UniShip_2_VF_GTNexus214.TestFiles.D1_Output.xml";
			mapTester.Execute<UniShip_2_VF_GTNexus214>(sourceFile, expectedFile);
            Assert.AreEqual(ctx.Read("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties"), "true");
            Assert.AreEqual(ctx.Read("GS03", "http://schemas.microsoft.com/BizTalk/2006/edi-properties"), "86308990");

		}

		static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "TRLVSRTRI_V14" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "TRLVSRTRI_V14" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "VF Corporation 214 - Export Shipment Status", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = null });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "SCAC" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "JSJS" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Shipper Code N104" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "123" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Shipment Status", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "X12 Code" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_PassThroughKey = 1 });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Transport Mode", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "X12 Code" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "AIR" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "A" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_PassThroughKey = 1 });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Unit of Measurement", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "X12 Code" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "KG" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "K" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "LB" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "L" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "CF" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "E" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 4, CK_Key1Value = "M3" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "X" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 5, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_PassThroughKey = 1 });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}
