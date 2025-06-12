using CargoWise.eHub.Products.JPCustoms.Schemas._2017;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SchemaTestType = CargoWise.eHub.Products.JPCustoms.Tests.TestHelper.SchemaTestType;

namespace CargoWise.eHub.Products.JPCustoms.Tests
{
	/// <summary>
	/// Summary description for UnitTest1
	/// </summary>
	[TestClass]
	public class Schema2017Test
	{
		#region AHRFlatFile

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AHRFlatFileSchema_TestEmpty()
		{
			string inputXMLResourceName = "SchemaTests_Input.AHRFlatFile.input2017_Empty.xml";
			string expectedOutputFlatFileResourceName = "SchemaTests_Output.AHRFlatFile.output2017_Empty.txt";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new InputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas._2017.InputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new AHRFlatFileSchema(), "AHRFlatFile.xsd");
			TestHelper.TestSchema(SchemaTestType.Assemble, inputXMLResourceName, expectedOutputFlatFileResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AHRFlatFileSchema_Test1()
		{
			string inputXMLResourceName = "SchemaTests_Input.AHRFlatFile.input2017_1.xml";
			string expectedOutputFlatFileResourceName = "SchemaTests_Output.AHRFlatFile.output2017_1.txt";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new InputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas._2017.InputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new AHRFlatFileSchema(), "AHRFlatFile.xsd");
			TestHelper.TestSchema(SchemaTestType.Assemble, inputXMLResourceName, expectedOutputFlatFileResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AHRFlatFileSchema_Test2()
		{
			string inputXMLResourceName = "SchemaTests_Input.AHRFlatFile.input2017_2.xml";
			string expectedOutputFlatFileResourceName = "SchemaTests_Output.AHRFlatFile.output2017_2.txt";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new InputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas._2017.InputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new AHRFlatFileSchema(), "AHRFlatFile.xsd");
			TestHelper.TestSchema(SchemaTestType.Assemble, inputXMLResourceName, expectedOutputFlatFileResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AHRFlatFileSchema_TestCompletion()
		{
			string inputXMLResourceName = "SchemaTests_Input.AHRFlatFile.input2017_CompletionFlag.xml";
			string expectedOutputFlatFileResourceName = "SchemaTests_Output.AHRFlatFile.output2017_CompletionFlag.txt";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new InputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas._2017.InputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new AHRFlatFileSchema(), "AHRFlatFile.xsd");
			TestHelper.TestSchema(SchemaTestType.Assemble, inputXMLResourceName, expectedOutputFlatFileResourceName, schemaPath, refSchemaPath);
		}

		#endregion

		#region AMRFlatFile

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AMRFlatFileSchema_Test1()
		{
			string inputXMLResourceName = "SchemaTests_Input.AMRFlatFile.AMR_Sample2017_1.xml";
			string expectedOutputFlatFileResourceName = "SchemaTests_Output.AMRFlatFile.AMR_Sample2017_1.txt";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new InputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas._2017.InputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new AMRFlatFileSchema(), "AMRFlatFile.xsd");
			TestHelper.TestSchema(SchemaTestType.Assemble, inputXMLResourceName, expectedOutputFlatFileResourceName, schemaPath, refSchemaPath);
		}

		#endregion

		#region CMVFlatFile

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CMVFlatFileSchema_TestEmpty()
		{
			string inputXMLResourceName = "SchemaTests_Input.CMVFlatFile.input2017_Empty.xml";
			string expectedOutputFlatFileResourceName = "SchemaTests_Output.CMVFlatFile.output2017_Empty.txt";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new InputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas._2017.InputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new CMVFlatFileSchema(), "CMVFlatFile.xsd");
			TestHelper.TestSchema(SchemaTestType.Assemble, inputXMLResourceName, expectedOutputFlatFileResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CMVFlatFileSchema_Test1()
		{
			string inputXMLResourceName = "SchemaTests_Input.CMVFlatFile.input2017_1.xml";
			string expectedOutputFlatFileResourceName = "SchemaTests_Output.CMVFlatFile.output2017_1.txt";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new InputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas._2017.InputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new CMVFlatFileSchema(), "CMVFlatFile.xsd");
			TestHelper.TestSchema(SchemaTestType.Assemble, inputXMLResourceName, expectedOutputFlatFileResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CMVFlatFileSchema_Test2()
		{
			string inputXMLResourceName = "SchemaTests_Input.CMVFlatFile.input2017_2.xml";
			string expectedOutputFlatFileResourceName = "SchemaTests_Output.CMVFlatFile.output2017_2.txt";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new InputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas._2017.InputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new CMVFlatFileSchema(), "CMVFlatFile.xsd");
			TestHelper.TestSchema(SchemaTestType.Assemble, inputXMLResourceName, expectedOutputFlatFileResourceName, schemaPath, refSchemaPath);
		}

		#endregion

		#region SAS144

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS144FlatFileSchema_Test_SAS144()
		{
			var inputResourceName = "SchemaTests_Input.SAS144.sample_sas144.txt";
			var expectedOutputResourceName = "SchemaTests_Output.SAS144.sample_sas144.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas._2017.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new SAS144FlatFileSchema(), "SAS144FlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS144FlatFileSchema_Test_SAS144_1()
		{
			var inputResourceName = "SchemaTests_Input.SAS144.sas144_1.txt";
			var expectedOutputResourceName = "SchemaTests_Output.SAS144.sas144_1.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas._2017.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new SAS144FlatFileSchema(), "SAS144FlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS144FlatFileSchema_Test_SAS144_2()
		{
			var inputResourceName = "SchemaTests_Input.SAS144.sas144_2.txt";
			var expectedOutputResourceName = "SchemaTests_Output.SAS144.sas144_2.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas._2017.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new SAS144FlatFileSchema(), "SAS144FlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		#endregion

		#region SAS148

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS148FlatFileSchema_Test_SAS148()
		{
			var inputResourceName = "SchemaTests_Input.SAS148.sample_sas148.txt";
			var expectedOutputResourceName = "SchemaTests_Output.SAS148.sample_sas148.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas._2017.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new SAS148FlatFileSchema(), "SAS148FlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS148FlatFileSchema_Test_SAS148_1()
		{
			var inputResourceName = "SchemaTests_Input.SAS148.sas148_1.txt";
			var expectedOutputResourceName = "SchemaTests_Output.SAS148.sas148_1.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas._2017.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new SAS148FlatFileSchema(), "SAS148FlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS148FlatFileSchema_Test_SAS148_2()
		{
			var inputResourceName = "SchemaTests_Input.SAS148.sas148_2.txt";
			var expectedOutputResourceName = "SchemaTests_Output.SAS148.sas148_2.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas._2017.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new SAS148FlatFileSchema(), "SAS148FlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		#endregion

		#region SAS157

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS157FlatFileSchema_Test_SAS157()
		{
			var inputResourceName = "SchemaTests_Input.SAS157.sample_sas157.txt";
			var expectedOutputResourceName = "SchemaTests_Output.SAS157.sample_sas157.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas._2017.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new SAS157FlatFileSchema(), "SAS157FlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS157FlatFileSchema_Test_SAS157_1()
		{
			var inputResourceName = "SchemaTests_Input.SAS157.sas157_1.txt";
			var expectedOutputResourceName = "SchemaTests_Output.SAS157.sas157_1.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas._2017.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new SAS157FlatFileSchema(), "SAS157FlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS157FlatFileSchema_Test_SAS157_2()
		{
			var inputResourceName = "SchemaTests_Input.SAS157.sas157_2.txt";
			var expectedOutputResourceName = "SchemaTests_Output.SAS157.sas157_2.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas._2017.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new SAS157FlatFileSchema(), "SAS157FlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		#endregion


		#region CMVResponseFlatFile

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CMVResponseFlatFileSchema_Test1()
		{
			var inputResourceName = "SchemaTests_Input.CMVResponseFlatFile.input_1.txt";
			var expectedOutputResourceName = "SchemaTests_Output.CMVResponseFlatFile.output_1.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas._2017.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new CMVResponseFlatFileSchema(), "CMVResponseFlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CMVResponseFlatFileSchema_Test2()
		{
			var inputResourceName = "SchemaTests_Input.CMVResponseFlatFile.input_2.txt";
			var expectedOutputResourceName = "SchemaTests_Output.CMVResponseFlatFile.output_2.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas._2017.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new CMVResponseFlatFileSchema(), "CMVResponseFlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CMVResponseFlatFileSchema_Test3()
		{
			var inputResourceName = "SchemaTests_Input.CMVResponseFlatFile.input_3.txt";
			var expectedOutputResourceName = "SchemaTests_Output.CMVResponseFlatFile.output_3.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas._2017.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new CMVResponseFlatFileSchema(), "CMVResponseFlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		#endregion

		#region SAS155FlatFile

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS155FlatFileSchema_Test1()
		{
			var inputResourceName = "SchemaTests_Input.SAS155.input_1.txt";
			var expectedOutputResourceName = "SchemaTests_Output.SAS155.output_1.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas._2017.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new SAS155FlatFileSchema(), "SAS155FlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SAS155FlatFileSchema_Test2()
		{
			var inputResourceName = "SchemaTests_Input.SAS155.input_2.txt";
			var expectedOutputResourceName = "SchemaTests_Output.SAS155.output_2.xml";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new OutputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas._2017.OutputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new SAS155FlatFileSchema(), "SAS155FlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Disassemble, inputResourceName, expectedOutputResourceName, schemaPath, refSchemaPath);
		}

		#endregion

		#region BLLFlatFile

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void BLLFlatFileSchema_TestEmpty()
		{
			string inputXMLResourceName = "SchemaTests_Input.BLLFlatFile.input2017_Empty.xml";
			string expectedOutputFlatFileResourceName = "SchemaTests_Output.BLLFlatFile.output2017_Empty.txt";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new InputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas._2017.InputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new BLLFlatFileSchema(), "BLLFlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Assemble, inputXMLResourceName, expectedOutputFlatFileResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void BLLFlatFileSchema_Test1()
		{
			string inputXMLResourceName = "SchemaTests_Input.BLLFlatFile.input2017_1.xml";
			string expectedOutputFlatFileResourceName = "SchemaTests_Output.BLLFlatFile.output2017_1.txt";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new InputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas._2017.InputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new BLLFlatFileSchema(), "BLLFlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Assemble, inputXMLResourceName, expectedOutputFlatFileResourceName, schemaPath, refSchemaPath);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void BLLFlatFileSchema_Test2()
		{
			string inputXMLResourceName = "SchemaTests_Input.BLLFlatFile.input2017_2.xml";
			string expectedOutputFlatFileResourceName = "SchemaTests_Output.BLLFlatFile.output2017_2.txt";

			var refSchemaPath = TestHelper.GetSchemaInTempPath(new InputCommonFieldFlatFileSchema(), "CargoWise.eHub.Products.JPCustoms.Schemas._2017.InputCommonFieldFlatFileSchema");
			var schemaPath = TestHelper.GetSchemaInTempPath(new BLLFlatFileSchema(), "BLLFlatFileSchema.xsd");
			TestHelper.TestSchema(SchemaTestType.Assemble, inputXMLResourceName, expectedOutputFlatFileResourceName, schemaPath, refSchemaPath);
		}

		#endregion
	}
}