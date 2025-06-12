using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.Products.TWCustoms.Transforms.UniversalShipment2X101;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CargoWise.eHub.Products.TWCustoms.BizTalk.Tests
{
	[TestClass]
	public class UniversalShipment2X101Tests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2X101Line_Manufacturer_VAT()
		{
			AssertMapping("Test_Line_Manufacturer_VAT_Input.xml", "Test_Line_Manufacturer_VAT_Output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2X101LocalProcessor_AddressOverride()
		{
			AssertMapping("Test_LocalProcessor_AddressOverride_Input.xml", "Test_LocalProcessor_AddressOverride_Output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2X101LocalProcessor_NotAddressOverride()
		{
			AssertMapping("Test_LocalProcessor_NotAddressOverride_Input.xml", "Test_LocalProcessor_NotAddressOverride_Output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2X101Export_AddressOverride_CertificateType15()
		{
			AssertMapping("Test_Export_AddressOverride_CertificateType15_Input.xml", "Test_Export_AddressOverride_CertificateType15_Output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2X101Import_AddressOverride_CertificateType15()
		{
			AssertMapping("Test_Import_AddressOverride_CertificateType15_Input.xml", "Test_Import_AddressOverride_CertificateType15_Output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2X101Export_AddressOverride()
		{
			AssertMapping("Test_Export_AddressOverride_Input.xml", "Test_Export_AddressOverride_Output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2X101Import_AddressOverride()
		{
			AssertMapping("Test_Import_AddressOverride_Input.xml", "Test_Import_AddressOverride_Output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2X101Import()
		{
			AssertMapping("Test_Import_input.xml", "Test_Import_output.xml");
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void Test2X101Export()
        {
            AssertMapping("Test_Export_input.xml", "Test_Export_output.xml");
        }

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2X101_Empty()
		{
			AssertMapping("Test_Empty_input.xml", "Test_Empty_output.xml");
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void Test2X101_MarksAndNumbers()
        {
            AssertMapping("Test_MarksAndNumbers_input.xml", "Test_MarksAndNumbers_output.xml");
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void Test2X101_SortTheGoodsItemNumber()
        {
            AssertMapping("Test_SortTheGoodsItemNumber_Input.xml", "Test_SortTheGoodsItemNumber_Output.xml");
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void Test2X101_WI00324918()
        {
            AssertMapping("Test_WI00324918_Input.xml", "Test_WI00324918_Output.xml");
        }

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2X101_WI00357265()
		{
			AssertMapping("Test_WI00357265_Input.xml", "Test_WI00357265_Output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2X101_WI00376438WithLocalProcessor()
		{
			AssertMapping("Test_WI00376438WithLocalProcessor_Input.xml", "Test_WI00376438WithLocalProcessor_Output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test2X101_WI00376438WithoutLocalProcessor()
		{
			AssertMapping("Test_WI00376438WithoutLocalProcessor_Input.xml", "Test_WI00376438WithoutLocalProcessor_Output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_CertificateType02()
		{
			AssertMapping("Test_CertificateType_02_Input.xml", "Test_CertificateType_02_Output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_CertificateType09()
		{
			AssertMapping("Test_CertificateType_09_Input.xml", "Test_CertificateType_09_Output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_CertificateType11()
		{
			AssertMapping("Test_CertificateType_11_Input.xml", "Test_CertificateType_11_Output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_CertificateType12()
		{
			AssertMapping("Test_CertificateType_12_Input.xml", "Test_CertificateType_12_Output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_CertificateType13()
		{
			AssertMapping("Test_CertificateType_13_Input.xml", "Test_CertificateType_13_Output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_CertificateType14()
		{
			AssertMapping("Test_CertificateType_14_Input.xml", "Test_CertificateType_14_Output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_CertificateType15()
		{
			AssertMapping("Test_CertificateType_15_Input.xml", "Test_CertificateType_15_Output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_LocalProcessor0806()
		{
			AssertMapping("Test_UXML_Local Processor_0806.xml", "Test_Local Processor_0806.xml");
		}

		void AssertMapping(string inputFile, string expectedOutputFile)
		{
			const string filePath = "UniversalShipment2X101.";
			var input = filePath + "Input." + inputFile;
			var expectedOutput = filePath + "Output." + expectedOutputFile;

			mapTester.ExecuteCompiled<UniversalShipment2X101>(input, expectedOutput);
		}

		static void InitialiseCodeMapsTestingContext()
		{
			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "TWCustoms" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "TWCustoms" });
			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "TW NCATK X101", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Measure Unit", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Measure Unit - X101" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "SET" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "台" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "UNT" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "輛" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 3, CK_Key1Value = "PCE" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "個" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 99, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Statistics Unit", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Statistics Measure - X101" });

			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "LTR" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "095" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 99, CK_Key1Value = "%" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Currency", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Chinese Description" });

			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "TWD" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "新台幣" });
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

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetCountryName",
				OutputParm = "@name",
				InputParms = new List<string> {
					"@code", "AU"
				},
				Result = "Australia"
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetCountryName",
				OutputParm = "@name",
				InputParms = new List<string> {
					"@code", "CN"
				},
				Result = "China"
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetCountryName",
				OutputParm = "@name",
				InputParms = new List<string> {
					"@code", "TW"
				},
				Result = "Taiwan"
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetCountryName",
				OutputParm = "@name",
				InputParms = new List<string> {
					"@code", "US"
				},
				Result = "United States"
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "GetCountryName",
				OutputParm = "@name",
				InputParms = new List<string> {
					"@code", ""
				},
				Result = ""
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
