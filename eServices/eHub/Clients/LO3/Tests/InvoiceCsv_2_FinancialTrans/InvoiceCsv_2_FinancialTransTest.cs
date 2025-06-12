using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.LO3.Transforms.InvoiceCsv_2_FinancialTrans;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.LO3.Tests
{
	[TestClass]
	public class InvoiceCsv_2_FinancialTransTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestInvoiceCsv_2_FinancialTrans_AR()
		{
			var ctx = new TestingMessageContext();
			ctx.Write("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "V3-Cargowise-AR.csv");
			var ca = new ContextAccessor();
			ca.SetTestingMessageContext(ctx);

			InitialiseCodeMapsTestingContext();

			List<string> exclusionXpaths = new List<string>();
			exclusionXpaths.Add("//*[local-name()='Date']");

			ICompare comparer = new ExcludingComparer(exclusionXpaths);

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

			string sourceFile = "InvoiceCsv_2_FinancialTrans.TestFiles.Input.xml";
			string expectedFile = "InvoiceCsv_2_FinancialTrans.TestFiles.AR_Output.xml";
			mapTester.ExecuteCompiled<InvoiceCsv_2_FinancialTrans>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestInvoiceCsv_2_FinancialTrans_AP()
		{
			var ctx = new TestingMessageContext();
			ctx.Write("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "AP-V3-Cargowise.csv");
			var ca = new ContextAccessor();
			ca.SetTestingMessageContext(ctx);

			InitialiseCodeMapsTestingContext();

			List<string> exclusionXpaths = new List<string>();
			exclusionXpaths.Add("//*[local-name()='Date']");

			ICompare comparer = new ExcludingComparer(exclusionXpaths);

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

			string sourceFile = "InvoiceCsv_2_FinancialTrans.TestFiles.Input.xml";
			string expectedFile = "InvoiceCsv_2_FinancialTrans.TestFiles.AP_Output.xml";
			mapTester.ExecuteCompiled<InvoiceCsv_2_FinancialTrans>(sourceFile, expectedFile);
		}

		private static void InitialiseCodeMapsTestingContext()
		{
			var ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "LO3BHXNBN" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "LO3BHXNBN" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "Previous OS v3 csv file - Receive A/* Invoice data", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Defaults", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Data Provider" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "PREVIOUS_SOFTWARE" });

			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 2, CR_Name = "Revenue Charge Code" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "ABC" });

			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 3, CR_Name = "Cost Charge Code" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "EFG" });

			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 4, CR_Name = "Currency" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "GBP" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Tax ID", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "TaxCode" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 99, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_PassThroughKey = 1 });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}
