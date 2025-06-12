using System.IO;
using System.Text;
using CargoWise.eHub.Products.ForwardingPortMessagingAir.NL.Schemas;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.ForwardingPortMessagingAir.NL.Tests.Schemas
{
	[TestClass]
	public class EDI756_Tests
	{
		const string filePath = "Schema.EDI756.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_CIN_EDI756_Schema()
		{
			AssertSchema("Test1_FlatFile.txt", "Test1_XML.xml");
			AssertSchema("Test2_FlatFile.txt", "Test2_XML.xml");
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
					schemaFilePath = Path.Combine(Path.GetTempPath(), "EDI756.xsd");
					using (StreamWriter writer = new StreamWriter(schemaFilePath, false, Encoding.Unicode))
					{
						var xml = new CGN_EDI756();
						writer.Write(xml.XmlContent);
					}
				}

				return schemaFilePath;
			}
		}

		string schemaFilePath;
	}
}
