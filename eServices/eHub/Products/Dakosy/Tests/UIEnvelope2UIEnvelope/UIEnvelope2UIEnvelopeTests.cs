using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.Dakosy.BT.Transforms.DakosyResponse2EDIUniversalEvent;
using CargoWise.eHub.Products.Dakosy.BT.Transforms.UIEnvelope2UIEnvelope;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.EDIFACT.Tests
{
  [TestClass]
  public class UIEnvelope2UIEnvelopeTests
  {
    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUIEnvelope2UIEnvelope()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml");
      AssertMapping("Test2_input.xml", "Test2_output.xml");
      AssertMapping("Test3_input.xml", "Test3_output.xml");
    }

    public void AssertMapping(string inputFile, string outputFile)
    {
      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

      string sourceFile = "UIEnvelope2UIEnvelope.TestFiles." + inputFile;
      string expectedFile = "UIEnvelope2UIEnvelope.TestFiles." + outputFile;
      mapTester.Execute<UIEnvelope2UIEnvelope>(sourceFile, expectedFile);
    }
  }
}
