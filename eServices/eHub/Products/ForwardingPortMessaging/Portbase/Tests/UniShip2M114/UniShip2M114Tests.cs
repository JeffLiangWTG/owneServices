using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.Portbase.Transforms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.Portbase.Tests
{
  [TestClass]
  public class UniShip2M114Tests
  {
    const string filePath = "UniShip2M114.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void UniShip2M114()
    {
      var containerMrnNumbers = new List<ContainerMRNNumber>();
      containerMrnNumbers.Add(new ContainerMRNNumber() { ContainerNumber = "TEST1111111", MrnNumber = "SHPMRN0001" });
      containerMrnNumbers.Add(new ContainerMRNNumber() { ContainerNumber = "TEST2222222", MrnNumber = "SHPMRN0001" });

      TestMapping("Original Message", "Test1_Original_input.xml", "Test1_Original_output.xml", "C00001004", containerMrnNumbers);
      TestMapping("Replace Message", "Test2_Replace_input.xml", "Test2_Replace_output.xml", "C00001004", containerMrnNumbers, 3, 2);
      TestMapping("Cancelled Message", "Test3_Cancelled_input.xml", "Test3_Cancelled_output.xml", "C00001004", containerMrnNumbers, 4, 3, "WTH");
      TestMapping("Original Message", "Test15_input.xml", "Test15_output.xml", "C00001004", containerMrnNumbers);
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void UniShip2M114_VariousScenario()
    {
      var containerMrnNumbers = new List<ContainerMRNNumber>();
      containerMrnNumbers.Add(new ContainerMRNNumber() { ContainerNumber = "TEST1111111", MrnNumber = "SHPMRN0001" });
      TestMapping("1 Shipment, MRN Number entered in Shipment", "Test4_input.xml", "Test4_output.xml", "C00001004", containerMrnNumbers);

      containerMrnNumbers = new List<ContainerMRNNumber>();
      containerMrnNumbers.Add(new ContainerMRNNumber() { ContainerNumber = "TEST1111111", MrnNumber = "SHPMRN0001" });
      TestMapping("2 Shipments, same MRN Number entered in both shipments", "Test5_input.xml", "Test5_output.xml", "C00001004", containerMrnNumbers);

      containerMrnNumbers = new List<ContainerMRNNumber>();
      containerMrnNumbers.Add(new ContainerMRNNumber() { ContainerNumber = "TEST1111111", MrnNumber = "PACKMRN0001" });
      TestMapping("2 Shipments, Shipment and Packline MRN number are entered, it should take packing MRN number", "Test6_input.xml", "Test6_output.xml", "C00001004", containerMrnNumbers);

      containerMrnNumbers = new List<ContainerMRNNumber>();
      containerMrnNumbers.Add(new ContainerMRNNumber() { ContainerNumber = "TEST1111111", MrnNumber = "SHPMRN0001" });
      containerMrnNumbers.Add(new ContainerMRNNumber() { ContainerNumber = "TEST1111111", MrnNumber = "SHPMRN0002" });
      TestMapping("2 Shipments, Different MRN number entered in shipment", "Test7_input.xml", "Test7_output.xml", "C00001004", containerMrnNumbers);

      containerMrnNumbers = new List<ContainerMRNNumber>();
      containerMrnNumbers.Add(new ContainerMRNNumber() { ContainerNumber = "TEST1111111", MrnNumber = "SHPMRN0001" });
      containerMrnNumbers.Add(new ContainerMRNNumber() { ContainerNumber = "TEST1111111", MrnNumber = "SHPMRN0002" });
      containerMrnNumbers.Add(new ContainerMRNNumber() { ContainerNumber = "TEST2222222", MrnNumber = "SHPMRN0002" });
      TestMapping("2 Shipments, MRN number entered in shipment, shipment(2) has additional container", "Test8_input.xml", "Test8_output.xml", "C00001004", containerMrnNumbers);

      containerMrnNumbers = new List<ContainerMRNNumber>();
      containerMrnNumbers.Add(new ContainerMRNNumber() { ContainerNumber = "TEST1111111", MrnNumber = "PACKMRN0002" });
      containerMrnNumbers.Add(new ContainerMRNNumber() { ContainerNumber = "TEST1111111", MrnNumber = "PACKMRN0001" });
      TestMapping("2 Shipments, each shipment has different packline MRN number", "Test9_input.xml", "Test9_output.xml", "C00001004", containerMrnNumbers);

      containerMrnNumbers = new List<ContainerMRNNumber>();
      containerMrnNumbers.Add(new ContainerMRNNumber() { ContainerNumber = "TEST1111111", MrnNumber = "PACKMRN0003" });
      containerMrnNumbers.Add(new ContainerMRNNumber() { ContainerNumber = "TEST1111111", MrnNumber = "PACKMRN0004" });
      containerMrnNumbers.Add(new ContainerMRNNumber() { ContainerNumber = "TEST1111111", MrnNumber = "PACKMRN0001" });
      containerMrnNumbers.Add(new ContainerMRNNumber() { ContainerNumber = "TEST1111111", MrnNumber = "PACKMRN0002" });
      TestMapping("2 Shipments, each shipment has 2 packlines, all packlines MRN number are different", "Test10_input.xml", "Test10_output.xml", "C00001004", containerMrnNumbers);

      containerMrnNumbers = new List<ContainerMRNNumber>();
      containerMrnNumbers.Add(new ContainerMRNNumber() { ContainerNumber = "TEST1111111", MrnNumber = "PACKMRN0001" });
      containerMrnNumbers.Add(new ContainerMRNNumber() { ContainerNumber = "TEST1111111", MrnNumber = "PACKMRN0002" });
      containerMrnNumbers.Add(new ContainerMRNNumber() { ContainerNumber = "TEST2222222", MrnNumber = "PACKMRN0001" });
      containerMrnNumbers.Add(new ContainerMRNNumber() { ContainerNumber = "TEST2222222", MrnNumber = "PACKMRN0002" });
      TestMapping("2 Shipments, each shipment has 2 Packline with different MRN number", "Test11_input.xml", "Test11_output.xml", "C00001004", containerMrnNumbers);

      containerMrnNumbers = new List<ContainerMRNNumber>();
      containerMrnNumbers.Add(new ContainerMRNNumber() { ContainerNumber = "TEST1111111", MrnNumber = "PACKMRN0001" });
      containerMrnNumbers.Add(new ContainerMRNNumber() { ContainerNumber = "TEST1111111", MrnNumber = "PACKMRN0002" });
      containerMrnNumbers.Add(new ContainerMRNNumber() { ContainerNumber = "TEST2222222", MrnNumber = "SHPMRN0001" });
      TestMapping("2 Shipments, Shipment(1) has entered different packline MRN number , shipment(1) has entered shipment MRN number", "Test12_input.xml", "Test12_output.xml", "C00001004", containerMrnNumbers);

      containerMrnNumbers = new List<ContainerMRNNumber>();
      containerMrnNumbers.Add(new ContainerMRNNumber() { ContainerNumber = "TEST1111111", MrnNumber = "PACKMRN0001" });
      containerMrnNumbers.Add(new ContainerMRNNumber() { ContainerNumber = "TEST1111111", MrnNumber = "SHPMRN0001" });
      containerMrnNumbers.Add(new ContainerMRNNumber() { ContainerNumber = "TEST2222222", MrnNumber = "SHPMRN0001" });
      TestMapping("2 Shipments, shipment(1) has entered different MRN number in packline, one of the packline MRN number same as shipment(2) MRN number", "Test13_input.xml", "Test13_output.xml", "C00001004", containerMrnNumbers);

      containerMrnNumbers = new List<ContainerMRNNumber>();
      containerMrnNumbers.Add(new ContainerMRNNumber() { ContainerNumber = "TEST1111111", MrnNumber = "SHPMRN0001" });
      containerMrnNumbers.Add(new ContainerMRNNumber() { ContainerNumber = "TEST2222222", MrnNumber = "SHPMRN0001" });
      TestMapping("2 Shipments, each shipment with different MRN number, only shipment(1) is selected to send", "Test14_input.xml", "Test14_output.xml", "C00001004", containerMrnNumbers, 222, 111);
    }

    void TestMapping(string message, string sourceFile, string expectedFile, string jobNumber, List<ContainerMRNNumber> containerMrnNumbers, int portbaseEHubCounter = 1, int previousConsolMRNCounter = 0, string actionPurpose = "APP")
    {
      var input = filePath + sourceFile;
      var expectedOutput = filePath + expectedFile;

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessaging.Portbase.MRN", "@maxlength", "14")).Return(portbaseEHubCounter.ToString());
      var eHubCounter = portbaseEHubCounter + 1;

      foreach (var mrnNumber in containerMrnNumbers.Select(p => p.MrnNumber).Distinct().ToArray())
      {
        var consolMRNNumber = jobNumber + "_" + mrnNumber;
        var consolMRNCounter = actionPurpose != "WTH" ? eHubCounter.ToString() : String.Empty;

        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("PBSMSG", "PORTBASE", "TESTSENDER", consolMRNNumber, consolMRNCounter));
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("PBSMSG", "PORTBASE", "TESTSENDER", consolMRNCounter, consolMRNNumber));
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessaging.Portbase.MRN", "@maxlength", "14")).Return(eHubCounter.ToString());
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PORTBASE", "@recipientId", "TESTSENDER", "@ST_ID", "PBSMSG", "@value", consolMRNNumber)).Return("").Repeat.AtLeastOnce();

        if (previousConsolMRNCounter > 0)
        {
          mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PORTBASE", "@recipientId", "TESTSENDER", "@ST_ID", "PBSMSG", "@value", consolMRNNumber)).Return(previousConsolMRNCounter.ToString()).Repeat.Any();
        }
        eHubCounter += 1;
      }

      foreach (var container in containerMrnNumbers)
      {
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("PBSMSG", "PORTBASE", "TESTSENDER", container.ContainerNumber + container.MrnNumber, jobNumber)).Repeat.Any();
      }

      mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("s"))).Return("2015-07-09T09:30:10");
      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER");
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER", "ROT", "PORTBASE")).Return("FORWDA");
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("PBSID", "PORTBASE", "TESTSENDER", "FORWDA"));
      var extensionObjects = new Dictionary<string, object>()
            {
                { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
            };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<UniShip2M114>(input, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
    }

    public class ContainerMRNNumber
    {
      public string ContainerNumber { get; set; }
      public string MrnNumber { get; set; }
    }
  }
}
