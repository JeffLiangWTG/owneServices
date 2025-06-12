using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.Shared.ClientSpecificFTP.Maps;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.Shared.ClientSpecificFTP.Tests
{
    [TestClass]
    public class FTPDetails2FtpExPollingRequestTest
    {
        const string filePath = "FTPDetails2FtpExPollingRequest.TestFiles.";

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestFTPDetails2FtpExPollingRequest()
        {
            var input = filePath + "FTPDetails.xml";
            var expectedOutput = filePath + "FtpExPollingRequest.xml";

            var mapTester = new MapTester(Assembly.GetExecutingAssembly());
            mapTester.ExecuteCompiled<FTPDetails2FtpExPollingRequest>(input, expectedOutput);
        }
    }
}
