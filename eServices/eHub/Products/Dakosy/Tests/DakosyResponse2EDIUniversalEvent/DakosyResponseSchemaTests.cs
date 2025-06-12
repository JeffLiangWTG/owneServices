using System.IO;
using System.Text;
using System.Xml;
using CargoWise.eHub.Products.Dakosy.BT.Schemas;
using CargoWise.eHub.Products.Dakosy.BT.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Winterdom.BizTalk.PipelineTesting.Simple;

namespace CargoWise.eHub.Products.EDIFACT.Tests
{
	[TestClass]
    public class DakosyResponseSchemaTests : BaseSchemaTests
    {
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestDakosyResponseResponse()
        {
            using (var inputMsg = GetEmbeddedResource("DakosyResponse2EDIUniversalEvent.TestFiles.Test2_success_input.txt"))
            using (var outputMsg = SchemaTester<DakosyResponse>.ParseFF(inputMsg))
            using (var sr = new StreamReader(outputMsg))
            {
                var expectedResult = GetResourceAsString("DakosyResponse2EDIUniversalEvent.TestFiles.Test2_success_input.xml");
                var actualResult = sr.ReadToEnd();
                XmlDocument xmlDocumentExpected = new XmlDocument();
                XmlDocument xmlDocumentActual = new XmlDocument();
                xmlDocumentExpected.LoadXml(expectedResult);
                xmlDocumentActual.LoadXml(actualResult);
                Assert.AreEqual(xmlDocumentExpected.InnerXml, xmlDocumentActual.InnerXml);
            }
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestDakosyResponseError()
        {
            using (var inputMsg = GetEmbeddedResource("DakosyResponse2EDIUniversalEvent.TestFiles.Test1_error_input.txt"))
            using (var outputMsg = SchemaTester<DakosyResponse>.ParseFF(inputMsg))
            using (var sr = new StreamReader(outputMsg))
            {
                var expectedResult = GetResourceAsString("DakosyResponse2EDIUniversalEvent.TestFiles.Test1_error_input.xml");
                var actualResult = sr.ReadToEnd();
                XmlDocument xmlDocumentExpected = new XmlDocument();
                XmlDocument xmlDocumentActual = new XmlDocument();
                xmlDocumentExpected.LoadXml(expectedResult);
                xmlDocumentActual.LoadXml(actualResult);
                Assert.AreEqual(xmlDocumentExpected.InnerXml, xmlDocumentActual.InnerXml);
            }
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestDakosyResponseWarning()
        {
            using (var inputMsg = GetEmbeddedResource("DakosyResponse2EDIUniversalEvent.TestFiles.Test3_warning_input.txt"))
            using (var outputMsg = SchemaTester<DakosyResponse>.ParseFF(inputMsg))
            using (var sr = new StreamReader(outputMsg))
            {
                var expectedResult = GetResourceAsString("DakosyResponse2EDIUniversalEvent.TestFiles.Test3_warning_input.xml");
                var actualResult = sr.ReadToEnd();
                XmlDocument xmlDocumentExpected = new XmlDocument();
                XmlDocument xmlDocumentActual = new XmlDocument();
                xmlDocumentExpected.LoadXml(expectedResult);
                xmlDocumentActual.LoadXml(actualResult);
                Assert.AreEqual(xmlDocumentExpected.InnerXml, xmlDocumentActual.InnerXml);
            }
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestDakosyResponseWarning2()
        {
            using (var inputMsg = GetEmbeddedResource("DakosyResponse2EDIUniversalEvent.TestFiles.Test4_error_input.txt"))
            using (var outputMsg = SchemaTester<DakosyResponse>.ParseFF(inputMsg))
            using (var sr = new StreamReader(outputMsg))
            {
                var expectedResult = GetResourceAsString("DakosyResponse2EDIUniversalEvent.TestFiles.Test4_error_input.xml");
                var actualResult = sr.ReadToEnd();
                XmlDocument xmlDocumentExpected = new XmlDocument();
                XmlDocument xmlDocumentActual = new XmlDocument();
                xmlDocumentExpected.LoadXml(expectedResult);
                xmlDocumentActual.LoadXml(actualResult);
                var actualOutputStream = new MemoryStream();
                var tempFile = Path.GetTempPath() + Path.GetRandomFileName() + ".xml";
                using (var fileStream = File.Create(tempFile))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(actualResult);
                    fileStream.Write(bytes, 0, bytes.Length);
                }
                Assert.AreEqual(xmlDocumentExpected.InnerXml, xmlDocumentActual.InnerXml, $"Processed content should match expected output. Actual Output: \r\n{tempFile}");
            }
        }
    }
}
