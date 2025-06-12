using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.Products.JPCustoms.Transforms.AHRResponseFlatFile2UniversalEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace CargoWise.eHub.Products.JPCustoms.Tests
{
	[TestClass]
	public class AHRResponseFlatFile2UniversalEventTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AHRResponseFlatFile2UniversalEventTest1()
		{
			InitialiseMessageTestingContext();
			InitialiseCodeMapsTestingContext();

			string sourceFile = "AHRResponseFlatFile2UniversalEvent_input.input_1.xml";
			string outputFile = "AHRResponseFlatFile2UniversalEvent_output.output_1.xml";

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			mapTester.Execute<AHRResponseFlatFile2UniversalEvent>(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AHRResponseFlatFile2UniversalEventTest2()
		{
			InitialiseMessageTestingContext();
			InitialiseCodeMapsTestingContext();

			string sourceFile = "AHRResponseFlatFile2UniversalEvent_input.input_2.xml";
			string outputFile = "AHRResponseFlatFile2UniversalEvent_output.output_2.xml";

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			mapTester.Execute<AHRResponseFlatFile2UniversalEvent>(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AHRResponseFlatFile2UniversalEventTest3()
		{
			InitialiseMessageTestingContext();
			InitialiseCodeMapsTestingContext();

			string sourceFile = "AHRResponseFlatFile2UniversalEvent_input.input_3.xml";
			string outputFile = "AHRResponseFlatFile2UniversalEvent_output.output_3.xml";

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			mapTester.Execute<AHRResponseFlatFile2UniversalEvent>(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AHRResponseFlatFile2UniversalEventTest4()
		{
			InitialiseMessageTestingContext();
			InitialiseCodeMapsTestingContext();

			string sourceFile = "AHRResponseFlatFile2UniversalEvent_input.input_4.xml";
			string outputFile = "AHRResponseFlatFile2UniversalEvent_output.output_4.xml";

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			mapTester.Execute<AHRResponseFlatFile2UniversalEvent>(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AHRResponseFlatFile2UniversalEventTest5()
		{
			InitialiseMessageTestingContext();
			InitialiseCodeMapsTestingContext();

			string sourceFile = "AHRResponseFlatFile2UniversalEvent_input.input_5.xml";
			string outputFile = "AHRResponseFlatFile2UniversalEvent_output.output_5.xml";

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			mapTester.Execute<AHRResponseFlatFile2UniversalEvent>(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AHRResponseFlatFile2UniversalEventTestCompletionResponse()
		{
			InitialiseMessageTestingContext();
			InitialiseCodeMapsTestingContext();

			string sourceFile = "AHRResponseFlatFile2UniversalEvent_input.input_CompletionResponse.xml";
			string outputFile = "AHRResponseFlatFile2UniversalEvent_output.output_CompletionResponse.xml";

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			mapTester.Execute<AHRResponseFlatFile2UniversalEvent>(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AHRResponseFlatFile2UniversalEventTestCompletionResponse2()
		{
			InitialiseMessageTestingContext();
			InitialiseCodeMapsTestingContext();

			string sourceFile = "AHRResponseFlatFile2UniversalEvent_input.input_CompletionResponse2.xml";
			string outputFile = "AHRResponseFlatFile2UniversalEvent_output.output_CompletionResponse2.xml";

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			mapTester.Execute<AHRResponseFlatFile2UniversalEvent>(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AHRResponseFlatFile2UniversalEventTestCompletionResponse3()
		{
			InitialiseMessageTestingContext();
			InitialiseCodeMapsTestingContext();

			string sourceFile = "AHRResponseFlatFile2UniversalEvent_input.input_CompletionResponse3.xml";
			string outputFile = "AHRResponseFlatFile2UniversalEvent_output.output_CompletionResponse3.xml";

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			mapTester.Execute<AHRResponseFlatFile2UniversalEvent>(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AHRResponseFlatFile2UniversalEventTestCompletionResponse4()
		{
			InitialiseMessageTestingContext();
			InitialiseCodeMapsTestingContext();

			string sourceFile = "AHRResponseFlatFile2UniversalEvent_input.input_CompletionResponse4.xml";
			string outputFile = "AHRResponseFlatFile2UniversalEvent_output.output_CompletionResponse4.xml";

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			mapTester.Execute<AHRResponseFlatFile2UniversalEvent>(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AHRResponseFlatFile2UniversalEventTestCompletionResponse5()
		{
			InitialiseMessageTestingContext();
			InitialiseCodeMapsTestingContext();

			string sourceFile = "AHRResponseFlatFile2UniversalEvent_input.input_CompletionResponse5.xml";
			string outputFile = "AHRResponseFlatFile2UniversalEvent_output.output_CompletionResponse5.xml";

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			mapTester.Execute<AHRResponseFlatFile2UniversalEvent>(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AHRResponseFlatFile2UniversalEventTestCompletionResponse6()
		{
			InitialiseMessageTestingContext();
			InitialiseCodeMapsTestingContext();

			string sourceFile = "AHRResponseFlatFile2UniversalEvent_input.input_CompletionResponse6.xml";
			string outputFile = "AHRResponseFlatFile2UniversalEvent_output.output_CompletionResponse6.xml";

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			mapTester.Execute<AHRResponseFlatFile2UniversalEvent>(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AHRResponseFlatFile2UniversalEventTestCompletionResponse7()
		{
			InitialiseMessageTestingContext();
			InitialiseCodeMapsTestingContext();

			string sourceFile = "AHRResponseFlatFile2UniversalEvent_input.input_CompletionResponse7.xml";
			string outputFile = "AHRResponseFlatFile2UniversalEvent_output.output_CompletionResponse7.xml";

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			mapTester.Execute<AHRResponseFlatFile2UniversalEvent>(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AHRResponseFlatFile2UniversalEventTestCompletionResponse8()
		{
			InitialiseMessageTestingContext();
			InitialiseCodeMapsTestingContext();

			string sourceFile = "AHRResponseFlatFile2UniversalEvent_input.input_CompletionResponse8.xml";
			string outputFile = "AHRResponseFlatFile2UniversalEvent_output.output_CompletionResponse8.xml";

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			mapTester.Execute<AHRResponseFlatFile2UniversalEvent>(sourceFile, outputFile);
		}

		static void InitialiseMessageTestingContext() { }

		static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "JPCustoms" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "JP AFR - Import SAHR", eHubClient_Sender = ctx.eHubClients[0] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Field", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[0], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "FieldName" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "SCA", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Carrier Code" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "SL6", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Seal Number" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "OR3", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Code of Other Relevant Laws and Ordinances" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 4, CK_Key1Value = "BLN", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "B/L Number" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 5, CK_Key1Value = "CYC", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Container Operator Code" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 6, CK_Key1Value = "HBE", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "House B/L Register Completion Identifier" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 7, CK_Key1Value = "%", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "ErrorDetail", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[0], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "ErrorDescription" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "S0011", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Carrier Code entry is required." });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "S0108", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Seal Number is required to be entered with left justification." });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "R0007", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Code of Other Relevant Laws and Ordinances entry is not allowed because entered Port of Discharge Code is port outside Japan." });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 4, CK_Key1Value = "E0614", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Change in Carrier Code is not allowed." });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 5, CK_Key1Value = "L0003", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Entered Container Operator Code is not available because corresponding Bonded Area Code is not registered in NACCS." });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 6, CK_Key1Value = "E0004", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "E0004 Detail" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 7, CK_Key1Value = "E0006", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "E0006 Detail" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 8, CK_Key1Value = "%", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "No detailed description for this error" });

			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 2, CR_Name = "ErrorSolutionPartA" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "S0011", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Enter Carrier Code." });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "S0108", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Enter the data with left justification." });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "R0007", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "1. Enter LOCODE of port in Japan in Port of Discharge Code.\r\n" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 4, CK_Key1Value = "E0614", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "1. Check entered Carrier Code and correct if it is wrong.\r\n" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 5, CK_Key1Value = "L0003", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "Contact with NACCS Center." });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 6, CK_Key1Value = "E0004", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "E0004 Solution" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 7, CK_Key1Value = "E0006", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "E0006 Solution" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 8, CK_Key1Value = "%", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "" });

			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 3, CR_Name = "ErrorSolutionPartB" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "R0007", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "2. Delete Code of Other Relevant Laws and Ordinances." });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "E0614", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "2. Implement Re-filing due to the change of Vessel information, use AHR or CHR." });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "%", CK_Key2Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "" });

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}
	}
}