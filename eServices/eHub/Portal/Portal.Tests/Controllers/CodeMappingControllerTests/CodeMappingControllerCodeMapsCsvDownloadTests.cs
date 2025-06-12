using System;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Portal.Tests.CodeMappingControllerTests
{
    [TestClass]
    public class CodeMappingControllerCodeMapsCsvDownloadTests : CodeMappingController_TestBase
    {
        [TestMethod()]
        public void ShouldDownloadCsvFileForCodeSet()
        {
            string expectedData = "Key 1,Key 2,Key 3,Result 1,Result 2" + Environment.NewLine +
                "XXX,AA,111*,311,312" + Environment.NewLine +
                "XXX,AA,*222,321,322" + Environment.NewLine +
                "XXX,AA,*333*,331,332" + Environment.NewLine +
                "XXX,*,*,341,342" + Environment.NewLine +
                "*,*,*,#INPUTFIELD1#,#INPUTFIELD2#" + Environment.NewLine;
            string expectedName = "Test Client 1 - Test Client 2 - Test Transformation 1 - Code Set 3.csv";
            FileContentResult result = controller.DownloadCsv(new Guid("{00000000-CCCC-3333-0000-000000000000}"));
            Assert.AreEqual(expectedData, Encoding.Default.GetString(result.FileContents));
            Assert.AreEqual(expectedName, result.FileDownloadName);
        }
    }
}
