using System.Linq;
using System.Reflection;

using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.AGS.Transforms.WWAInvoiceAcknowledgement_2_UniEvent;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.AGS.Tests
{
	[TestClass]
	public class WWAInvoiceAcknowledgement_2_UniEventTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestWWAInvoiceAcknowledgment_2_UniEvent()
		{
			var ctx = new TestingMessageContext();
			var ca = new ContextAccessor();
			ctx.Write("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "AGSWORXXX");
			ca.SetTestingMessageContext(ctx);

			InitialiseCodeMapsTestingContext();
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());


			ctx.Write("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "AGSS_INVOICEACK_C_C_E_BNE00000050.20201022.092115.333.xml");
			string sourceFile = "WWAInvoiceAcknowledgement_2_UniEvent.TestFiles.Input_CRD.xml";
			string expectedFile = "WWAInvoiceAcknowledgement_2_UniEvent.TestFiles.Output_CRD.xml";
			mapTester.Execute<WWAInvoiceAcknowledgement_2_UniEvent>(sourceFile, expectedFile);


			ctx.Write("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "AGSS_INVOICEACK_S_C_E_BNE00000050.20201022.092115.333.xml");
			sourceFile = "WWAInvoiceAcknowledgement_2_UniEvent.TestFiles.Input_INV.xml";
			expectedFile = "WWAInvoiceAcknowledgement_2_UniEvent.TestFiles.Output_INV.xml";
			mapTester.Execute<WWAInvoiceAcknowledgement_2_UniEvent>(sourceFile, expectedFile);
		}

		private static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "AGSWORAGS_WTA" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "AGSWORAGS" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "WWA AR Acknowledgement xml-File - Receive Events", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = null });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Time Zone" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "China Standard Time" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Status Code", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "CW1 Event Code", });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "301" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "ATH" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "304" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "REJ" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Z01" });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}
