using System;
using System.IO;
using System.Reflection;
using System.Xml.XPath;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.Shared.ClientSpecificFTP.Maps;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.XLANGs.BaseTypes;

namespace CargoWise.eHub.Products.Shared.ClientSpecificFTP.Tests
{
    [TestClass]
    public class PollingResonse2ResponseEnvelopeTest
    {
        const string filePath = "PollingResonse2ResponseEnvelope.TestFiles.";

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestPollingResonse2ResponseEnvelope()
        {
            var input = filePath + "PollingResponse.xml";
            var expectedOutput = filePath + "ResponseEnvelope.xml";

            var mapTester = new MapTester(Assembly.GetExecutingAssembly());
            mapTester.ExecuteCompiled<PollingResonse2ResponseEnvelope>(input, expectedOutput);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestPollingResonse2ResponseEnvelopeNoFiles()
        {
	        var input = filePath + "PollingResponseNoFile.xml";
			var inputStream = ResourceHelper.GetEmbeddedResource(Assembly.GetExecutingAssembly(), input);
			TransformBase mapInstance = Activator.CreateInstance<PollingResonse2ResponseEnvelope>();

			using (MemoryStream mapResult = new MemoryStream())
			{
                mapInstance.Transform2.Transform(inputStream, null, mapResult);
				mapResult.Position = 0;
				Assert.AreEqual(string.Empty, new StreamReader(mapResult).ReadToEnd());
				Assert.IsTrue(mapResult.Length < 4);
			}
        }
    }
}
