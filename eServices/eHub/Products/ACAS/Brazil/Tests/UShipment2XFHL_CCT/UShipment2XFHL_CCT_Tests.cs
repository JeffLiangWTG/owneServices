using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.ACAS.BR.Helpers;
using CargoWise.eHub.Products.ACAS.BR.Transforms.UShipment2XFHL_CCT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ACAS.Tests
{
  [TestClass]
  public class UShipment2XFHL_CCT_Tests
  {
    const string filePath = "UShipment2XFHL_CCT.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUShipment2XFHL_CCT()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml");
      AssertMapping("Test2_input.xml", "Test2_output.xml");
      AssertMapping("Test3_input.xml", "Test3_output.xml");
    }

    void AssertMapping(string inputFile, string expectedOutputFile)
    {
      string input = filePath + inputFile;
      string expectedOutput = filePath + expectedOutputFile;

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("HYETSTUAT").Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CCT_BR").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ACAS.BR.Transforms.CCT", "@maxlength", "14")).Return("21");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_BR", "@recipientId", "HYETSTUAT", "@ST_ID", "ACASBR", "@value", "C0000100099", "@referenceType", "ShipmentId")).Return("C0000100099_FHL0000000021").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASBR", "ACAS_BR", "HYETSTUAT", "FHL0000000021", "C0000100099_FHL0000000021", "InterchangeNum")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASBR", "ACAS_BR", "HYETSTUAT", "C0000100099_FHL0000000021", "081-78046780", "FHL-HWB")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASBR", "ACAS_BR", "HYETSTUAT", "C0000100099_FHL0000000021", "C0000100099", "ShipmentId")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASBR", "ACAS_BR", "HYETSTUAT", "C0000100099_FHL0000000021", "CCT House Checklist", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASBR", "ACAS_BR", "HYETSTUAT", "C0000100099_FHL0000000021", "ForwardingConsol", "ForwardingType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASBR", "ACAS_BR", "HYETSTUAT", "C0000100099_FHL0000000021", "ORG", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASBR", "ACAS_BR", "HYETSTUAT", "C0000100099_FHL0000000021", "AMD", "ActionPurpose")).Repeat.Any();

      mockContextAccessor.Expect(x => x.SetContextProperty(Arg<string>.Is.Equal("OverrideEmailSubject"), Arg<string>.Is.Equal("http://cargowise.com/ehub/processing/2010/06"), Arg<string>.Is.Anything));

      mockDateMapper.Expect(x => x.CurrentDateTimeUTC("yyyyMMddHHmm")).Return("201910011459");
      mockDateMapper.Expect(x => x.CurrentDateTimeUTC("yyyy-MM-ddTHH:mm:ss")).Return("2019-10-01T14:59:00");

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor }
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<UShipment2XFHL_CCT>(input, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
    }

    [TestInitialize]
    public void TestInitialize()
    {
      pkFirstCharacter = 0;

      var eHubClients = new TestDbSet<eHubClient>() { new eHubClient { CC_PK = getNewGuid(pkFirstCharacter++), CC_ID = "CCT_BR" }, new eHubClient { CC_PK = getNewGuid(pkFirstCharacter++), CC_ID = "HYETSTUAT" } };
      var eHubRegistrationTypes = new TestDbSet<eHubRegistrationType>() { new eHubRegistrationType { RT_PK = getNewGuid(pkFirstCharacter++), RT_ID = "ACAS_BRProtocol", RT_RegistrantType = "AsyncPolling" } };
      var eHubClientSystems = new TestDbSet<eHubClientSystem>() { new eHubClientSystem() { EH_PK = getNewGuid(pkFirstCharacter++), EH_ID = "HYEUAT" } };
      var eHubAsyncPollingRegistrations = new TestDbSet<eHubAsyncPollingRegistration>();

      var stubContext = MockRepository.GenerateStub<eHubTransactionsContext>();
      stubContext.eHubClients = eHubClients;
      stubContext.eHubRegistrationTypes = eHubRegistrationTypes;
      stubContext.eHubClientSystems = eHubClientSystems;
      stubContext.eHubAsyncPollingRegistrations = eHubAsyncPollingRegistrations;
      eHubAsyncPollingRegistrationHelper.InternalContextFactory = () => stubContext;
      eHubAsyncPollingRegistrationHelper.InternalGuidFactory = () => getNewGuid(pkFirstCharacter++);

    }

    private int pkFirstCharacter;
    readonly Func<int, Guid> getNewGuid = (int i) => new Guid(Enumerable.Repeat((byte)((i % 16) * 0x11), 16).ToArray());
  }
}
