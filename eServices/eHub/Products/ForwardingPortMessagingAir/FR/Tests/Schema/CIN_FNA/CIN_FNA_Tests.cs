using System.IO;
using System.Text;
using CargoWise.eHub.Products.ForwardingPortMessagingAir.FR.Schemas;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.ForwardingPortMessagingAir.FR.Tests.Schemas
{
	[TestClass]
	public class CIN_FNA_Tests
	{
		const string filePath = "Schema.CIN_FNA.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_CIN_FNA_Schema()
		{
			AssertSchema("Test1_input.txt", "Test1_output.xml");
			AssertSchema("Test2_input.txt", "Test2_output.xml");
		}

		void AssertSchema(string inputFile, string expectedOutputFile)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			string inpuFileInString = TestHelper.GetFileWithEmbeddedResource(input);

			TestHelper.VerifyFlatFile2XMLWithSchema(inpuFileInString, expectedOutput, SchemaFilePath);
		}

		string SchemaFilePath
		{
			get
			{
				if (schemaFilePath == null || !File.Exists(schemaFilePath))
				{
					schemaFilePath = Path.Combine(Path.GetTempPath(), "CIN_FNA.xsd");
					using (StreamWriter writer = new StreamWriter(schemaFilePath, false, Encoding.Unicode))
					{
						var xml = new CIN_FNA();
						writer.Write(xml.XmlContent);
					}
				}

				return schemaFilePath;
			}
		}

		string schemaFilePath;
	}
}
