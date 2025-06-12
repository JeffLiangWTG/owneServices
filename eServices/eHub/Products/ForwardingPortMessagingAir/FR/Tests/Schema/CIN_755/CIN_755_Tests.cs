using System.IO;
using System.Text;
using CargoWise.eHub.Products.ForwardingPortMessagingAir.FR.Schemas;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.ForwardingPortMessagingAir.FR.Tests.Schemas
{
	[TestClass]
	public class CIN_755_Tests
	{
		const string filePath = "Schema.CIN_755.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test1_Schema_XML2FlatFile()
		{
			AssertSchema_FlatOutput("Test1_XML.xml", "Test1_FlatFile.txt");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test1_Schema_XML2FlatFile_ExceptionCase()
		{
			AssertSchema_FlatOutput("Test2_XML_BlankValue.xml", "Test2_FlatFile_BlankValue.txt");
		}

		void AssertSchema_XmlOutput(string inputFile, string expectedOutputFile)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			string inpuFileInString = TestHelper.GetFileWithEmbeddedResource(input);

			TestHelper.VerifyFlatFile2XMLWithSchema(inpuFileInString, expectedOutput, SchemaFilePath);
		}

		void AssertSchema_FlatOutput(string inputFile, string expectedOutputFile)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			string inpuFileInString = TestHelper.GetFileWithEmbeddedResource(input);

			TestHelper.VerifyXML2FlatFileWithSchema(inpuFileInString, expectedOutput, SchemaFilePath);
		}

		string SchemaFilePath
		{
			get
			{
				if (schemaFilePath == null || !File.Exists(schemaFilePath))
				{
					schemaFilePath = Path.Combine(Path.GetTempPath(), "CIN_755.xsd");
					using (StreamWriter writer = new StreamWriter(schemaFilePath, false, Encoding.Unicode))
					{
						var xml = new CIN_755();
						writer.Write(xml.XmlContent);
					}
				}

				return schemaFilePath;
			}
		}

		string schemaFilePath;
	}
}
