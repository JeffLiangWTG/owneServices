using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.Products.TWCustoms.Transforms.NCATK.UniversalShipment2NX101;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CargoWise.eHub.Products.TWCustoms.BizTalk.Tests
{
	[TestClass]
	public class UniversalShipment2NX101Tests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestManufacturerChineseNameAndAddress()
		{
			AssertMapping("Test_ManufacturerChineseNameAndAddress.xml", "Test_ManufacturerChineseNameAndAddress_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2NX101()
		{
			AssertMapping("Test_Simple.xml", "Test_Simple_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2NX101_CertificateType_01()
		{
			AssertMapping("Test_CertificateType_01.xml", "Test_CertificateType_01_Output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2NX101_CertificateType_07()
		{
			AssertMapping("Test_CertificateType_07.xml", "Test_CertificateType_07_Output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2NX101_CertificateType_09()
		{
			AssertMapping("Test_CertificateType_09.xml", "Test_CertificateType_09_Output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2NX101_CertificateType_10()
		{
			AssertMapping("Test_CertificateType_10.xml", "Test_CertificateType_10_Output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2NX101_CertificateType_11()
		{
			AssertMapping("Test_CertificateType_11.xml", "Test_CertificateType_11_Output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2NX101_CertificateType_12()
		{
			AssertMapping("Test_CertificateType_12.xml", "Test_CertificateType_12_Output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2NX101_CertificateType_13()
		{
			AssertMapping("Test_CertificateType_13.xml", "Test_CertificateType_13_Output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2NX101_CertificateType_14()
		{
			AssertMapping("Test_CertificateType_14.xml", "Test_CertificateType_14_Output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2NX101_CertificateType_15()
		{
			AssertMapping("Test_CertificateType_15.xml", "Test_CertificateType_15_Output.xml");
		}
		
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2NX101_CertificateType_17() 
		{
			AssertMapping("Test_CertificateType_17.xml", "Test_CertificateType_17_Output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2NX101_SayTotal()
		{
			AssertMapping("Test_SayTotal_1.xml", "Test_SayTotal_1_Output.xml");
			AssertMapping("Test_SayTotal_2.xml", "Test_SayTotal_2_Output.xml");
			AssertMapping("Test_SayTotal_3.xml", "Test_SayTotal_3_Output.xml");
			AssertMapping("Test_SayTotal_4.xml", "Test_SayTotal_4_Output.xml");
			AssertMapping("Test_SayTotal_5.xml", "Test_SayTotal_5_Output.xml");
			AssertMapping("Test_SayTotal_6.xml", "Test_SayTotal_6_Output.xml");
			AssertMapping("Test_SayTotal_7.xml", "Test_SayTotal_7_Output.xml");
			AssertMapping("Test_SayTotal_8.xml", "Test_SayTotal_8_Output.xml");
			AssertMapping("Test_SayTotal_9.xml", "Test_SayTotal_9_Output.xml");
			AssertMapping("Test_SayTotal_10.xml", "Test_SayTotal_10_Output.xml");
			AssertMapping("Test_SayTotal_11.xml", "Test_SayTotal_11_Output.xml");
			AssertMapping("Test_SayTotal_12.xml", "Test_SayTotal_12_Output.xml");
			AssertMapping("Test_SayTotal_13.xml", "Test_SayTotal_13_Output.xml");
			AssertMapping("Test_SayTotal_14.xml", "Test_SayTotal_14_Output.xml");
			AssertMapping("Test_SayTotal_15.xml", "Test_SayTotal_15_Output.xml");
			AssertMapping("Test_SayTotal_16.xml", "Test_SayTotal_16_Output.xml");
			AssertMapping("Test_SayTotal_17.xml", "Test_SayTotal_17_Output.xml");
			AssertMapping("Test_SayTotal_18.xml", "Test_SayTotal_18_Output.xml");
			AssertMapping("Test_SayTotal_19.xml", "Test_SayTotal_19_Output.xml");
			AssertMapping("Test_SayTotal_20.xml", "Test_SayTotal_20_Output.xml");
			AssertMapping("Test_SayTotal_30.xml", "Test_SayTotal_30_Output.xml");
			AssertMapping("Test_SayTotal_40.xml", "Test_SayTotal_40_Output.xml");
			AssertMapping("Test_SayTotal_50.xml", "Test_SayTotal_50_Output.xml");
			AssertMapping("Test_SayTotal_60.xml", "Test_SayTotal_60_Output.xml");
			AssertMapping("Test_SayTotal_70.xml", "Test_SayTotal_70_Output.xml");
			AssertMapping("Test_SayTotal_80.xml", "Test_SayTotal_80_Output.xml");
			AssertMapping("Test_SayTotal_90.xml", "Test_SayTotal_90_Output.xml");
			AssertMapping("Test_SayTotal_99.xml", "Test_SayTotal_99_Output.xml");
		}

		void AssertMapping(string inputFile, string expectedOutputFile)
		{
			const string filePath = "UniversalInterchange2NX101.";
			var input = filePath + "Input." + inputFile;
			var expectedOutput = filePath + "Output." + expectedOutputFile;
			mapTester.ExecuteCompiled<UniversalShipment2NX101>(input, expectedOutput);
		}

		static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "TWCustoms" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "TWCustoms" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "TW NCATK X101", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Measure Unit", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Measure Unit - X101" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "KGM" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "公斤(千克)" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "PCE" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "個" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 99, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Currency", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Chinese Description" });

			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "TWD" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "新台幣" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "USD" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "美元" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 99, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_PassThroughKey = 1 });

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "SelectClientExists",
				OutputParm = "",
				InputParms = new List<string> {
					"@ID", "Test11223_TCA"
				},
				Result = "True"
			});

			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);
		}

		[TestInitialize]
		public void Setup()
		{
			var ctx = new TestingMessageContext();
			ctx.Write("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "Test11223");
			var ca = new ContextAccessor();
			ca.SetTestingMessageContext(ctx);

			InitialiseCodeMapsTestingContext();
			mapTester = new MapTester(Assembly.GetExecutingAssembly());
		}
		MapTester mapTester;
	}
}
