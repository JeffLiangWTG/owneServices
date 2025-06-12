using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.DFD.Transforms.LeCreusetPurchaseOrderARRADV2WhsDocketsInternal;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.DFD.Tests
{
	[TestClass]
	public class LeCreusetPurchaseOrderARRADV2WhsDocketsInternalTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestLeCreusetPurchaseOrderARRADV2WhsDocketsInternal()
		{
			InitialiseCodeMapsTestingContext();

			var ctx = new TestingMessageContext();
			var ca = new ContextAccessor();

			List<string> exclusionXpaths = new List<string>();
			exclusionXpaths.Add("/*[local-name()='WhsDocketsInternal']/*[local-name()='InterchangeInfo']/*[local-name()='Date']");
			exclusionXpaths.Add("/*[local-name()='WhsDocketsInternal']/*[local-name()='Payload']/*[local-name()='WhsDockets']/*[local-name()='WhsDocket']/*[local-name()='DocketDetail']/*[local-name()='CustomerInwardsDetail']/*[local-name()='BookingDate' or local-name()='ArrivalDate']");
			ICompare comparer = new ExcludingComparer(exclusionXpaths);

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

			ctx.Write("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "DFDSYDPRD_CWR");
			ca.SetTestingMessageContext(ctx);
			string sourceFile = "LeCreusetPurchaseOrderARRADV2WhsDocketsInternal.TestFiles.LeCreusetPurchaseOrderARRADV_Input.xml";
			string expectedFile = "LeCreusetPurchaseOrderARRADV2WhsDocketsInternal.TestFiles.WhsDocketsInternal(Receive)_Output.xml";
			mapTester.ExecuteCompiled<LeCreusetPurchaseOrderARRADV2WhsDocketsInternal>(sourceFile, expectedFile);

			ctx.Write("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "DFDAU2PRD_CWR");
			ca.SetTestingMessageContext(ctx);
			sourceFile = "LeCreusetPurchaseOrderARRADV2WhsDocketsInternal.TestFiles.LeCreusetPurchaseOrderARRADV_Input.xml";
			expectedFile = "LeCreusetPurchaseOrderARRADV2WhsDocketsInternal.TestFiles.WhsDocketsInternal(Receive)_Output_AU2.xml";
			mapTester.ExecuteCompiled<LeCreusetPurchaseOrderARRADV2WhsDocketsInternal>(sourceFile, expectedFile);
		}

		private static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "DFDSYDPRD_CWR" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "DFDSYDPRD" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "Le Creuset SAP - Import of Warehouse Receipts", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Whs. Code", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "ediEnterprise Code" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "DFDSYDPRD_CWR", CK_Key2Value = "LIT" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "SYM" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "DFDAU2PRD_CWR", CK_Key2Value = "LIT" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "HPK" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 99, CK_Key1Value = "%", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_PassThroughKey = 2 });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Unit of Qantity", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "ediEnterprise Code" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "STK" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "PCE" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_PassThroughKey = 1 });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}

	}
}
