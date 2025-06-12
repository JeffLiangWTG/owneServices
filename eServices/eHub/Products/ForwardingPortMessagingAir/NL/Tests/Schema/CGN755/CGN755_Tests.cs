using System.IO;
using System.Text;
using CargoWise.eHub.Products.ForwardingPortMessagingAir.NL.Schemas;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.ForwardingPortMessagingAir.NL.Tests.Schemas
{
	[TestClass]
	public class CGN755_Tests
	{
		const string filePath = "Schema.CGN755.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_CIN_EDI757_Schema()
		{
			AssertXML2FlatFileWithSchema("Test1_XML.xml", "Test1_FlatFile.txt");
			AssertXML2FlatFileWithSchema("Test2_XML.xml", "Test2_FlatFile.txt");
		}

		void AssertXML2FlatFileWithSchema(string inputFile, string expectedOutputFile)
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
					schemaFilePath = Path.Combine(Path.GetTempPath(), "CGN755.xsd");
					using (StreamWriter writer = new StreamWriter(schemaFilePath, false, Encoding.Unicode))
					{
						var xml = new CGN755();
						writer.Write(xml.XmlContent);
					}
				}

				return schemaFilePath;
			}
		}

		string schemaFilePath;
	}
}
