using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.Products.Dakosy.BT.Transforms.DakosyAck2EDIUniversalEvent;
using CargoWise.eHub.Products.Dakosy.BT.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.EDIFACT.Tests
{
  [TestClass]
  public class DakosyAck2EDIUniversalEventTest
  {
    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestDakosyAck2EDIUniversalEvent()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml");
      AssertMapping("Test2_input.xml", "Test2_output.xml", "");
    }

    void AssertMapping(string inputFile, string outputFile, string subscribedDateTime = "2015-12-03 11:59:58.088")
    {
      InitialiseCodeMapsTestingContext();

      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

      mockSubscriptionHelper.Expect(x => x.SelectSubscriptions(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.GetSubscribedDateTime()).Return(subscribedDateTime).Repeat.Any();
      mockDateMapper.Stub(x => x.CurrentDateTimeUTC("O")).Return("2022-01-15T09:59:58.00000Z");

      var extensionObjects = new Dictionary<string, object>() {
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

      string sourceFile = "DakosyAck2EDIUniversalEvent.TestFiles." + inputFile;
      string expectedFile = "DakosyAck2EDIUniversalEvent.TestFiles." + outputFile;
      mapTester.Execute<DakosyAck2EDIUniversalEvent>(sourceFile, expectedFile);
    }

    private static void InitialiseCodeMapsTestingContext()
    {
      CodeMapsTestingContext ctx = new CodeMapsTestingContext();
      ctx.eHubClients.Add(new eHubClient { CC_ID = "DAKOSYHAM" });
      ctx.eHubClients.Add(new eHubClient { CC_ID = "DAKOSYHAM" });
      ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "Import Dakosy ACK", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

      ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Status Code", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
      ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "EventType" });
      ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 2, CR_Name = "Description" });
      ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 2, CR_Name = "Event Reference" });
      ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1, CK_Key1Value = "C001" });
      ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[0], CV_OutputCode = "MPP1" });
      ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[1], CV_OutputCode = "Message Pending Processing1" });
      ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[2], CV_OutputCode = "Message Delivered to Dakosy-Pending Quay Order Processing1|DEP=Dakosy|" });
      ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "%" });
      ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[0], CV_OutputCode = "IRJ2" });
      ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[1], CV_OutputCode = "Interchange Rejected2" });
      ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[2], CV_OutputCode = "Critical Failure Delivering Message to Dakosy - Contact Support2|DEP=Dakosy" });

      ctx.ActionProcedures.Add(new ActionProcedure
      {
        Procedure = "SelectSubscribedReference",
        OutputParm = "@reference",
        InputParms = new List<string> {
                    "@senderId", "DAKOSYHAM",
                    "@recipientId", "",
                    "@ST_ID", "DAKREF",
                    "@value", "SNAT_0000000018"
                },
        Result = "S12SHAM0000261S"
      });

      ctx.ActionProcedures.Add(new ActionProcedure
      {
        Procedure = "SelectSubscribedReference",
        OutputParm = "@reference",
        InputParms = new List<string> {
                    "@senderId", "DAKOSYHAM",
                    "@recipientId", "",
                    "@ST_ID", "DAKREF",
                    "@value", "SNAT_0000000018",
                    "@referenceType", "MessagePurpose"
                },
        Result = "HDS"
      });

      var ta = new TransformAccessor();
      ta.SetCodeMapsTestingContext(ctx);
    }
  }
}
