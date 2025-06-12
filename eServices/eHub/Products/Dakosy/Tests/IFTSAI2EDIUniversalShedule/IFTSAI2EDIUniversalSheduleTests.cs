using System.CodeDom;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.Dakosy.BT.Transforms.IFTSAI2EDIUniversalSchedule;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using CargoWise.eHub.Core.Transforms.Helper;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.EDIFACT.Tests
{
  [TestClass]
  public class IFTSAI2EDIUniversalSheduleTests
  {
    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestIFTSAI2EDIUniversalShedule()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml");
      AssertMapping("Test2_input.xml", "Test2_output.xml");
      AssertMapping("Test3_input.xml", "Test3_output.xml");
      AssertMapping("Test4_CMDU_input.xml", "Test4_CMDU_output.xml");
      AssertMapping("Test5_YMLU_input.xml", "Test5_YMLU_output.xml");
      AssertMapping("Test6_multiple_IFTSAI_input.xml", "Test6_multiple_IFTSAI_output.xml");
      AssertMapping("Test7_DEHAM_LOC153_input.xml", "Test7_DEHAM_LOC153_output.xml");
      AssertMapping("Test8_input.xml", "Test8_output.xml");
    }

    void AssertMapping(string inputFile, string expectedOutputFile)
    {
      const string filePath = "IFTSAI2EDIUniversalShedule.TestFiles.";

      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<IFTSAI2EDIUniversalSchedule>(input, expectedOutput);

    }
  }
}
