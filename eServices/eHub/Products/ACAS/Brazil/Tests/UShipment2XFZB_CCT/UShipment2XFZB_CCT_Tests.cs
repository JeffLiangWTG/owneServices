using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.ACAS.BR.Helpers;
using CargoWise.eHub.Products.ACAS.BR.Transforms.UShipment2XFZB_CCT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ACAS.Tests
{
  [TestClass]
  public class UShipment2XFZB_CCT_Tests
  {
    const string filePath = "UShipment2XFZB_CCT.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUShipment2XFZB_CCT()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml", "FZB0000000021");
      AssertMapping("Test2_input.xml", "Test2_output.xml", "FZB0000000021");
      AssertMapping("Test3_input.xml", "Test3_output.xml", "FZB0000000021");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string previousConsolReference = "")
    {
      string input = filePath + inputFile;
      string expectedOutput = filePath + expectedOutputFile;

      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      mockDateMapper.Expect(x => x.CurrentDateTimeUTC("yyyyMMddHHmm")).Return("201910011459"); 
      mockDateMapper.Expect(x => x.CurrentDateTimeUTC("yyyy-MM-ddTHH:mm:ss")).Return("2019-10-01T14:59:00");
      var comparer = new ExcludingComparer(new List<string>
      {
        "/*[local-name()='HouseWaybill']/*[local-name()='BusinessHeaderDocument']/*[local-name()='SignatoryCarrierAuthentication']/*[local-name()='ActualDateTime']",
      });

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("HYETSTUAT").Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CCT_BR").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASBR", "ACAS_BR", "HYETSTUAT", "FZB0000000021", "S00001009_FZB0000000021", "InterchangeNum")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASBR", "ACAS_BR", "HYETSTUAT", "S00001009_FZB0000000021", "S00001009", "ShipmentId")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASBR", "ACAS_BR", "HYETSTUAT", "S00001009_FZB0000000021", "CCT Cargo And Transit Control", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASBR", "ACAS_BR", "HYETSTUAT", "S00001009_FZB0000000021", "ForwardingShipment", "ForwardingType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("ACASBR", "ACAS_BR", "HYETSTUAT", "S00001009_FZB0000000021", "ORG", "ActionPurpose")).Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ACAS.BR.Transforms.CCT", "@maxlength", "14")).Return("21");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_BR", "@recipientId", "HYETSTUAT", "@ST_ID", "ACASBR", "@value", "S00001009", "@referenceType", "ShipmentId")).Return(previousConsolReference).Repeat.Any();

      mockContextAccessor.Expect(x => x.SetContextProperty( Arg<string>.Is.Equal("OverrideEmailSubject"),  Arg<string>.Is.Equal("http://cargowise.com/ehub/processing/2010/06"), Arg<string>.Is.Anything));

      var extensionObjects = new Dictionary<string, object>() {
                { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor }
            };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer, extensionObjects);
      mapTester.Execute<UShipment2XFZB_CCT>(input, expectedOutput);

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
