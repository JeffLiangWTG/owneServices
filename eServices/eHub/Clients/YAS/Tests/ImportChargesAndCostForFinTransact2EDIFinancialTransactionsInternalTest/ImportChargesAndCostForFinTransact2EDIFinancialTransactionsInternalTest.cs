using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.YAS.Transforms.ImportChargesAndCost2EDIFinTransac;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.YAS.Tests
{
	[TestClass]
	public class ImportChargesAndCostForFinTransact2EDIFinancialTransactionsInternalTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestImportChargesAndCostForFinTransact2EDIFinancialTransactionsInternal()
		{
			string sourceFile = "ImportChargesAndCostForFinTransact2EDIFinancialTransactionsInternalTest.TestFiles.SampleFileImportChargesAndCosts.xml";
			string expectedFile = "ImportChargesAndCostForFinTransact2EDIFinancialTransactionsInternalTest.TestFiles.FinancialTransactionInternal.xml";

			var mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.ExecuteCompiled<ImportChargesAndCostForFinTransact2EDIFinancialTransactionsInternal>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestImportChargesAndCostForFinTransact2EDIFinancialTransactionsInternal_CurrencyCode()
		{
			string sourceFile = "ImportChargesAndCostForFinTransact2EDIFinancialTransactionsInternalTest.TestFiles.SampleFileImportChargesAndCosts_WithCurrencyCode.xml";
			string expectedFile = "ImportChargesAndCostForFinTransact2EDIFinancialTransactionsInternalTest.TestFiles.FinancialTransactionInternal_WithCurrencyCode.xml";

			var mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.ExecuteCompiled<ImportChargesAndCostForFinTransact2EDIFinancialTransactionsInternal>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestImportChargesAndCostForFinTransact2EDIFinancialTransactionsInternalGrouped()
		{
			string sourceFile = "ImportChargesAndCostForFinTransact2EDIFinancialTransactionsInternalTest.TestFiles.P1291update.xml";
			string expectedFile = "ImportChargesAndCostForFinTransact2EDIFinancialTransactionsInternalTest.TestFiles.FinancialTransactionInternal_P1291update.xml";

			var mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.ExecuteCompiled<ImportChargesAndCostForFinTransact2EDIFinancialTransactionsInternal>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestImportChargesAndCostForFinTransact2EDIFinancialTransactionsInternalWithNegativeAmountAndInvalidDate()
		{
			string sourceFile = "ImportChargesAndCostForFinTransact2EDIFinancialTransactionsInternalTest.TestFiles.input_NegativeAmount_InvalidDate.xml";
			string expectedFile = "ImportChargesAndCostForFinTransact2EDIFinancialTransactionsInternalTest.TestFiles.output_NegativeAmount_InvalidDate.xml";

			var mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.ExecuteCompiled<ImportChargesAndCostForFinTransact2EDIFinancialTransactionsInternal>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestImportChargesAndCostForFinTransact2EDIFinancialTransactionsInternalWithExRate()
		{
			InitialiseCodeMapsTestingContext();

			string sourceFile = "ImportChargesAndCostForFinTransact2EDIFinancialTransactionsInternalTest.TestFiles.input_CSL.xml";
			string expectedFile = "ImportChargesAndCostForFinTransact2EDIFinancialTransactionsInternalTest.TestFiles.output_CSL.xml";

			var mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.ExecuteCompiled<ImportChargesAndCostForFinTransact2EDIFinancialTransactionsInternal>(sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestImportChargesAndCostForFinTransact2EDIFinancialTransactionsInternalWithDepartmentCode()
		{
			InitialiseCodeMapsTestingContext();

			string sourceFile = "ImportChargesAndCostForFinTransact2EDIFinancialTransactionsInternalTest.TestFiles.input_CSL_DeptCode.xml";
			string expectedFile = "ImportChargesAndCostForFinTransact2EDIFinancialTransactionsInternalTest.TestFiles.output_CSL_DeptCode.xml";

			var mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.ExecuteCompiled<ImportChargesAndCostForFinTransact2EDIFinancialTransactionsInternal>(sourceFile, expectedFile);
		}

		private static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "YASYJPPRD_CHG" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "YASYJPPRD_CHG" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "Flat File- Import YJP Air Imp. Ship.Charges+Costs2", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Local Currency Code", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Currency Code" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "YJP" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "JPY" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "AUD" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Branch and Department", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "YJP" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Branch" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "TYO" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Department" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "FEA" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "%" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Branch" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "TVE" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Department" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "FES" });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}
