using CargoWise.eHub.Products.JPCustoms.Schemas;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SchemaTestType = CargoWise.eHub.Products.JPCustoms.Tests.TestHelper.SchemaTestType;


namespace CargoWise.eHub.Products.JPCustoms.Tests
{
	/// <summary>
	/// Summary description for UnitTest1
	/// </summary>
	[TestClass]
	public class SchemaTest
	{

		#region AHRFlatFile

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AHRFlatFileSchema_TestEmpty()
		{
			string inputXMLResourceName = "SchemaTests_Input.AHRFlatFile.input_Empty.xml";
			string expectedOutputFlatFileResourceName = "SchemaTests_Output.AHRFlatFile.output_Empty.txt";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new InputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.InputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new AHRFlatFileSchema(), "AHRFlatFile.xsd");
			TestHelper.TestSchema(SchemaTestType.Assemble, inputXMLResourceName, expectedOutputFlatFileResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AHRFlatFileSchema_Test1()
		{
			string inputXMLResourceName = "SchemaTests_Input.AHRFlatFile.input_1.xml";
			string expectedOutputFlatFileResourceName = "SchemaTests_Output.AHRFlatFile.output_1.txt";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new InputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.InputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new AHRFlatFileSchema(), "AHRFlatFile.xsd");
			TestHelper.TestSchema(SchemaTestType.Assemble, inputXMLResourceName, expectedOutputFlatFileResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AHRFlatFileSchema_Test2()
		{
			string inputXMLResourceName = "SchemaTests_Input.AHRFlatFile.input_2.xml";
			string expectedOutputFlatFileResourceName = "SchemaTests_Output.AHRFlatFile.output_2.txt";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new InputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.InputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new AHRFlatFileSchema(), "AHRFlatFile.xsd");
			TestHelper.TestSchema(SchemaTestType.Assemble, inputXMLResourceName, expectedOutputFlatFileResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AHRFlatFileSchema_TestCompletion()
		{
			string inputXMLResourceName = "SchemaTests_Input.AHRFlatFile.input_CompletionFlag.xml";
			string expectedOutputFlatFileResourceName = "SchemaTests_Output.AHRFlatFile.output_CompletionFlag.txt";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new InputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.InputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new AHRFlatFileSchema(), "AHRFlatFile.xsd");
			TestHelper.TestSchema(SchemaTestType.Assemble, inputXMLResourceName, expectedOutputFlatFileResourceName, schemaPath, refSchemaPath);
		}

		#endregion

		#region AHRResponseFlatFile

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AHRResponseFlatFileSchema_Test1()
		{
			string inputResourceName = "SchemaTests_Input.AHRResponseFlatFile.input_1.txt";
			string expectedOutputResourceName = "SchemaTests_Output.AHRResponseFlatFile.output_1.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new AHRResponseFlatFileSchema(), "AHRResponseFlatFile.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AHRResponseFlatFileSchema_Test2()
		{
			string inputResourceName = "SchemaTests_Input.AHRResponseFlatFile.input_2.txt";
			string expectedOutputResourceName = "SchemaTests_Output.AHRResponseFlatFile.output_2.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new AHRResponseFlatFileSchema(), "AHRResponseFlatFile.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AHRResponseFlatFileSchema_Test3()
		{
			string inputResourceName = "SchemaTests_Input.AHRResponseFlatFile.input_3.txt";
			string expectedOutputResourceName = "SchemaTests_Output.AHRResponseFlatFile.output_3.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new AHRResponseFlatFileSchema(), "AHRResponseFlatFile.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AHRResponseFlatFileSchema_Test4()
		{
			string inputResourceName = "SchemaTests_Input.AHRResponseFlatFile.input_4.txt";
			string expectedOutputResourceName = "SchemaTests_Output.AHRResponseFlatFile.output_4.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new AHRResponseFlatFileSchema(), "AHRResponseFlatFile.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AHRResponseFlatFileSchema_TestCompletionResponse1()
		{
			string inputResourceName = "SchemaTests_Input.AHRResponseFlatFile.input_CompletionResponse1.txt";
			string expectedOutputResourceName = "SchemaTests_Output.AHRResponseFlatFile.output_CompletionResponse1.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new AHRResponseFlatFileSchema(), "AHRResponseFlatFile.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AHRResponseFlatFileSchema_TestCompletionResponse2()
		{
			string inputResourceName = "SchemaTests_Input.AHRResponseFlatFile.input_CompletionResponse2.txt";
			string expectedOutputResourceName = "SchemaTests_Output.AHRResponseFlatFile.output_CompletionResponse2.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new AHRResponseFlatFileSchema(), "AHRResponseFlatFile.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AHRResponseFlatFileSchema_TestCompletionResponse3()
		{
			string inputResourceName = "SchemaTests_Input.AHRResponseFlatFile.input_CompletionResponse3.txt";
			string expectedOutputResourceName = "SchemaTests_Output.AHRResponseFlatFile.output_CompletionResponse3.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new AHRResponseFlatFileSchema(), "AHRResponseFlatFile.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		#endregion

		#region AMRFlatFile

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AMRFlatFileSchema_Test1()
		{
			string inputXMLResourceName = "SchemaTests_Input.AMRFlatFile.AMR_Sample_1.xml";
			string expectedOutputFlatFileResourceName = "SchemaTests_Output.AMRFlatFile.AMR_Sample_1.txt";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new InputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.InputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new AMRFlatFileSchema(), "AMRFlatFile.xsd");
			TestHelper.TestSchema(SchemaTestType.Assemble, inputXMLResourceName, expectedOutputFlatFileResourceName, schemaPath, refSchemaPath);
		}

		#endregion 

		#region AMRResponseFlatFile

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AMRResponseFlatFileSchema_Test1()
		{
			string inputResourceName = "SchemaTests_Input.AMRResponseFlatFile.input_1.txt";
			string expectedOutputResourceName = "SchemaTests_Output.AMRResponseFlatFile.output_1.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new AMRResponseFlatFileSchema(), "AMRResponseFlatFile.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		#endregion

		#region ATDFlatFile

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ATDFlatFileSchema_Test1()
		{
			string inputXMLResourceName = "SchemaTests_Input.ATDFlatFile.ATD_Sample_1.xml";
			string expectedOutputFlatFileResourceName = "SchemaTests_Output.ATDFlatFile.ATD_Sample_1.txt";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new InputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.InputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new ATDFlatFileSchema(), "ATDFlatFile.xsd");
			TestHelper.TestSchema(SchemaTestType.Assemble, inputXMLResourceName, expectedOutputFlatFileResourceName, schemaPath, refSchemaPath);
		}

		#endregion

		#region ATDResponseFlatFile

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ATDResponseFlatFileSchema_Test1()
		{
			string inputResourceName = "SchemaTests_Input.ATDResponseFlatFile.input_1.txt";
			string expectedOutputResourceName = "SchemaTests_Output.ATDResponseFlatFile.output_1.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new ATDResponseFlatFileSchema(), "ATDResponseFlatFile.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		#endregion

		#region SAS111

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS111FlatFileSchema_Test_SAS111()
		{
			string inputResourceName = "SchemaTests_Input.SAS111.sample_sas111.txt";
			string expectedOutputResourceName = "SchemaTests_Output.SAS111.sample_sas111xml.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new SAS111FlatFileSchema(), "SAS111FlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS111FlatFileSchema_Test_SAS112()
		{
			string inputResourceName = "SchemaTests_Input.SAS111.sample_sas112.txt";
			string expectedOutputResourceName = "SchemaTests_Output.SAS111.sample_sas112xml.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new SAS111FlatFileSchema(), "SAS111FlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS111FlatFileSchema_Test_SAS111_DNL()
		{
			string inputResourceName = "SchemaTests_Input.SAS111.SAS111_DNL.txt";
			string expectedOutputResourceName = "SchemaTests_Output.SAS111.SAS111_DNLxml.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new SAS111FlatFileSchema(), "SAS111FlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS111FlatFileSchema_Test_SAS111_DNU()
		{
			string inputResourceName = "SchemaTests_Input.SAS111.SAS111_DNU.txt";
			string expectedOutputResourceName = "SchemaTests_Output.SAS111.SAS111_DNUxml.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new SAS111FlatFileSchema(), "SAS111FlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS111FlatFileSchema_Test_SAS111_HLD()
		{
			string inputResourceName = "SchemaTests_Input.SAS111.SAS111_HLD.txt";
			string expectedOutputResourceName = "SchemaTests_Output.SAS111.SAS111_HLDxml.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new SAS111FlatFileSchema(), "SAS111FlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS111FlatFileSchema_Test_SAS112_DNL()
		{
			string inputResourceName = "SchemaTests_Input.SAS111.SAS112_DNL.txt";
			string expectedOutputResourceName = "SchemaTests_Output.SAS111.SAS112_DNLxml.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new SAS111FlatFileSchema(), "SAS111FlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS111FlatFileSchema_Test_SAS112_DNU()
		{
			string inputResourceName = "SchemaTests_Input.SAS111.SAS112_DNU.txt";
			string expectedOutputResourceName = "SchemaTests_Output.SAS111.SAS112_DNUxml.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new SAS111FlatFileSchema(), "SAS111FlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS111FlatFileSchema_Test_SAS112_HLD()
		{
			string inputResourceName = "SchemaTests_Input.SAS111.SAS112_HLD.txt";
			string expectedOutputResourceName = "SchemaTests_Output.SAS111.SAS112_HLDxml.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new SAS111FlatFileSchema(), "SAS111FlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}
		#endregion

		#region SAS108

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS108FlatFileSchema_Test_SAS108()
		{
			string inputResourceName = "SchemaTests_Input.SAS108.sample_sas108.txt";
			string expectedOutputResourceName = "SchemaTests_Output.SAS108.sample_sas108.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new SAS108FlatFileSchema(), "SAS108FlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS108FlatFileSchema_Test_SAS108_1()
		{
			string inputResourceName = "SchemaTests_Input.SAS108.sas108_1.txt";
			string expectedOutputResourceName = "SchemaTests_Output.SAS108.sas108_1.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new SAS108FlatFileSchema(), "SAS108FlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		#endregion

		#region SAS135

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS108FlatFileSchema_Test_SAS135()
		{
			string inputResourceName = "SchemaTests_Input.SAS135.mock_sas135.txt";
			string expectedOutputResourceName = "SchemaTests_Output.SAS135.mock_sas135.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new SAS135FlatFileSchema(), "SAS135FlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS108FlatFileSchema_Test_SAS135_2()
		{
			string inputResourceName = "SchemaTests_Input.SAS135.mock_sas135_2.txt";
			string expectedOutputResourceName = "SchemaTests_Output.SAS135.mock_sas135_2.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new SAS135FlatFileSchema(), "SAS135FlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		#endregion

		#region TCCInput

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TCCInputFlatFileSchema_Test_TCCInput()
		{
			string inputResourceName = "SchemaTests_Input.TCC.TCCInput.txt";
			string expectedOutputResourceName = "SchemaTests_Output.TCC.TCCInput_XML.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new InputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.InputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new TCCInputFlatFileSchema(), "TCCInputFlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TCCInputFlatFileSchema_Test_TCCInput2()
		{
			string inputXMLResourceName = "SchemaTests_Output.TCC.TCCInput_XML.xml";
			string expectedOutputFlatFileResourceName = "SchemaTests_Input.TCC.TCCInput.txt";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new InputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.InputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new TCCInputFlatFileSchema(), "TCCInputFlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Assemble, inputXMLResourceName, expectedOutputFlatFileResourceName, schemaPath, refSchemaPath);
		}

		#endregion

		#region TCCOutput

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TCCOutputFlatFileSchema_Test()
		{
			string inputResourceName = "SchemaTests_Input.TCC.TCCOutput.txt";
			string expectedOutputResourceName = "SchemaTests_Output.TCC.TCCOutput_XML.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new TCCOutputFlatFileSchema(), "TCCOutputFlatFile.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		#endregion

		#region SysteomCommonErrorResponseFlatFile

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SystemCommonErrorResponseFlatFileSchema_Test1()
		{
			string inputResourceName = "SchemaTests_Input.SystemCommonErrorResponseFlatFile.input_1.txt";
			string expectedOutputResourceName = "SchemaTests_Output.SystemCommonErrorResponseFlatFile.output_1.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new SystemCommonErrorResponseFlatFileSchema(), "SystemCommonErrorResponseFlatFile.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		#endregion

	}

}