using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.Portbase.Transforms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.EDIFACT.Tests
{
  [TestClass]
  public class EDIUniversalShipment2PortbaseM198Test
  {
    const string filePath = "UniShip2M198.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestEDIUniversalShipment2PortbaseM198()
    {
      var messageSingle = new Message();
      messageSingle.Documents.Add(new ContainerMRNNumber() { ContainerNumber = "PORT1234541", MrnNumber = "TESTMRN00001" });
      var messages = new List<Message>();
      messages.Add(messageSingle);

      TestMapping("Single MRN", "Test1_input_SingleNCTMsg.XML", "Test1_output_SingleNCTMsg.XML", "C00679067", messages);

      var messageMultiple1 = new Message();
      messageMultiple1.Documents.Add(new ContainerMRNNumber() { ContainerNumber = "CONT1111111", MrnNumber = "SHPMRN11111" });

      var messageMultiple2 = new Message();
      messageMultiple2.Documents.Add(new ContainerMRNNumber() { ContainerNumber = "CONT2222222", MrnNumber = "MRN333" });
      messageMultiple2.Documents.Add(new ContainerMRNNumber() { ContainerNumber = "CONT1111111", MrnNumber = "MRN444" });
      messageMultiple2.Documents.Add(new ContainerMRNNumber() { ContainerNumber = "CONT2222222", MrnNumber = "MRN222" });
      messageMultiple2.Documents.Add(new ContainerMRNNumber() { ContainerNumber = "CONT1111111", MrnNumber = "MRN111" });

      messages.Clear();
      messages.Add(messageMultiple1);
      messages.Add(messageMultiple2);

      TestMapping("Multiple MRN", "Test2_input_MultipleNCTMsg.XML", "Test2_output_MultipleNCTMsg.XML", "C00679068", messages);

      messageSingle = new Message();
      messageSingle.Documents.Add(new ContainerMRNNumber() { ContainerNumber = "CRLU0393995", MrnNumber = "0202994895" });
      messageSingle.Documents.Add(new ContainerMRNNumber() { ContainerNumber = "TGMU0039383", MrnNumber = "0202994895" });
      messageSingle.Documents.Add(new ContainerMRNNumber() { ContainerNumber = "CRLU0393995", MrnNumber = "R5009453U78" });
      messageSingle.Documents.Add(new ContainerMRNNumber() { ContainerNumber = "TGMU0039383", MrnNumber = "T039585TY67" });

      messages.Clear();
      messages.Add(messageSingle);

      TestMapping("Multiple MRN same TATG", "Test3_input_MultipleNCTSDoc.XML", "Test3_output_MultipleNCTSDoc.XML", "C00679042", messages);
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestEDIUniversalShipment2PortbaseM198_V2()
    {
      var messageSingle = new Message();
      var messages = new List<Message>();
      messages.Add(messageSingle);

      TestMapping("Single MRN (v2)", "Test4_input_v2_SingleNCTMsg.XML", "Test4_output_v2_SingleNCTMsg.XML", "C00679067", messages);
      TestMapping("Multiple MRN (v2)", "Test5_input_v2_MultipleNCTSDoc.XML", "Test5_output_v2_MultipleNCTSDoc.XML", "C00679067", messages);
    }

    void TestMapping(string message, string sourceFile, string expectedFile, string jobNumber, List<Message> messages, int portbaseEHubCounter = 1)
    {
      var input = filePath + sourceFile;
      var expectedOutput = filePath + expectedFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

      foreach (var msg in messages)
      {
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessaging.Portbase.MRN", "@maxlength", "14")).Return(portbaseEHubCounter.ToString());

        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("PBSMSG", "PORTBASE", "TESTSENDER", jobNumber, portbaseEHubCounter.ToString())).Repeat.Any();
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("PBSMSG", "PORTBASE", "TESTSENDER", portbaseEHubCounter.ToString(), jobNumber)).Repeat.Any();

        portbaseEHubCounter++;
      }

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER");

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<UniShip2M198Envelope>(input, expectedOutput);

      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
    }

    public class ContainerMRNNumber
    {
      public string ContainerNumber { get; set; }
      public string MrnNumber { get; set; }
    }

    public class Message
    {
      public List<ContainerMRNNumber> Documents = new List<ContainerMRNNumber>();
    }
  }
}
