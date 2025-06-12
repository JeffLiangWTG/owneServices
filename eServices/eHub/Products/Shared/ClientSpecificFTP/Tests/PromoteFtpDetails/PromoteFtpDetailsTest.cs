using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.Products.Shared.ClientSpecificFTP.PipelineComponents;
using CargoWise.eHub.Shared.BizTalk.PipelineHelpers;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.Shared.ClientSpecificFTP.Tests
{
    [TestClass]
    public class PromoteFtpDetailsTest
    {
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestFTPPromotion()
        {
            var messageFactory = new MessageFactory();
            var pipelineContext = new PipelineContext();
            var message = messageFactory.CreateMessage();
            message.AddPart("xml", messageFactory.CreateMessagePart(), true);
            message.Context = messageFactory.CreateMessageContext();
            message.Context.Write("SourceParty", "http://cargowise.com/ehub/system-properties/2010/06", "TestClient");
            message.Context.Write("DestinationParty", "http://cargowise.com/ehub/system-properties/2010/06", "TestProvider_MessageType");
            message.Context.Write("EventBranch", "http://cargowise.com/ehub/ocm-properties/2010/06", "TestBranch");
            message.Context.WriteProperty<BTS.ReceivePortName>("TestPort");

            var component = new PromoteFtpDetailsForTest();
            component.Enabled = true;

            var reader = new Dictionary<string, object>();
            reader.Add("CX_Qualifier", "ftp://user@host:21/path");
            reader.Add("CX_Password1", "123456E.K");
            reader.Add("CX_Code", "TestProvider_*");
            component.SetUpAccessor(accessor =>
            {
                accessor.Stub(x => x.ReadRegistrations("TestClient", "CSFTP", flag1: 1, flag2: 1)).Return(new List<Dictionary<string,object>> { reader });
            });
            var result = component.Execute(pipelineContext, message);
            Assert.AreEqual("host", message.Context.Read("Server", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties"));
            Assert.AreEqual("123456E.K", message.Context.Read("Password", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties"));
            Assert.AreEqual("user", message.Context.Read("User", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties"));
            Assert.AreEqual("user", message.Context.Read("UserName", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties"));
            Assert.AreEqual("/path", message.Context.Read("Folder", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties"));
            Assert.AreEqual(21, message.Context.Read("Port", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties"));
        }
    }

    class PromoteFtpDetailsForTest : PromoteFtpDetails
    {
        IClientRegistrationAccessor accessor = MockRepository.GenerateMock<IClientRegistrationAccessor>();
        public void SetUpAccessor(Action<IClientRegistrationAccessor> setup)
        {
            setup(accessor);
        }

        protected internal override IClientRegistrationAccessor GetClientRegistrationAccessor()
        {
            return accessor;
        }
    }
}
