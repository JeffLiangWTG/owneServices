using System.Collections.Generic;
using System.Reflection;
using Xunit;
using Hawking.UnitTest.Tools.Xslt;

using Hawking.Xslt.ExtensionObjects.Interfaces;
using Moq;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests
{
    public class AIRPCU2UEventTests
    {
        const string xsl = "AIRPCU2UEvent.MapFiles.AIRPCU2UEvent.xsl";
        const string filePath = "AIRPCU2UEvent.TestFiles.";

        [Fact]
        public void TestAIRPCU2UEvent1_failure()
        {
            var input = filePath + "Test1_input.xml";
            var expectedOutput = filePath + "Test1_output_failure.xml";

            var mockIDateMapper = new Mock<IDateMapper>();
            var mockICodeMapper = new Mock<ICodeMapper>();
            var mockIContextAccessor = new Mock<IContextAccessor>();
            var mockDataModelAccessor = new Mock<IDataModelAccessor>();

            mockIContextAccessor.Setup(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("UPEUPE001");
            mockIContextAccessor.Setup(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("SGCustomsTest");
            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns("2017-09-06T12:00:00");
            mockIContextAccessor.Setup(x => x.GetContextProperty("UNB_Segment", "http://schemas.microsoft.com/Edi/PropertySchema")).Returns("UNB+++++115");

            mockICodeMapper.Setup(x => x.CallActionProcedureHelper(It.Is<string>(v => v == "SelectSubscribedReference"),
                            It.Is<string>(v => v == "@reference"),
                            It.Is<string>(v => v == "@senderId"),
                            It.Is<string>(v => v == "SGCustomsTest"),
                            It.Is<string>(v => v == "@recipientId"),
                            It.Is<string>(v => v == "UPEUPE001"),
                            It.Is<string>(v => v == "@ST_ID"),
                            It.Is<string>(v => v == "SGCMSG"),
                            It.Is<string>(v => v == "@value"),
                            It.IsAny<string>(),
                            It.Is<string>(v => v == "@referenceType"),
                            It.Is<string>(v => v == "CR"))).Returns("fakeConsignment");
            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue(
                            It.Is<string>(v => v == "SGCMSG"),
                            It.Is<string>(v => v == "SGCustomsTest"),
                            It.Is<string>(v => v == "UPEUPE001"),
                            It.IsAny<string>(),
                            It.IsAny<string>(),
                            It.IsAny<string>()));
            mockIContextAccessor.Setup(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")).Returns("ErrorCode: 99999999;LinkedOutboxMessageTrackingID: 3DF09E8A-C79A-4F6A-95BC-E790A38FA7FB. ErrorMessage: Account 'AccountID1' is invalid");
            mockIContextAccessor.Setup(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", ""));

            var extensionObjects = new Dictionary<string, object>()
            {
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockIContextAccessor.Object },
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor.Object }
            };

            Assert.True(BizTalkXsltTester.Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);

        }

        [Fact]
        public void TestAIRPCU2UEvent1_success()
        {
            var input = filePath + "Test1_input.xml";
            var expectedOutput = filePath + "Test1_output_success.xml";

            var mockIDateMapper = new Mock<IDateMapper>();
            var mockICodeMapper = new Mock<ICodeMapper>();
            var mockIContextAccessor = new Mock<IContextAccessor>();
            var mockDataModelAccessor = new Mock<IDataModelAccessor>();

            mockIContextAccessor.Setup(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("UPEUPE001");
            mockIContextAccessor.Setup(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("SGCustomsTest");
            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns("2017-09-06T12:00:00");
            mockIContextAccessor.Setup(x => x.GetContextProperty("UNB_Segment", "http://schemas.microsoft.com/Edi/PropertySchema")).Returns("UNB+++++115");

            mockICodeMapper.Setup(x => x.CallActionProcedureHelper(It.Is<string>(v => v == "SelectSubscribedReference"),
                            It.Is<string>(v => v == "@reference"),
                            It.Is<string>(v => v == "@senderId"),
                            It.Is<string>(v => v == "SGCustomsTest"),
                            It.Is<string>(v => v == "@recipientId"),
                            It.Is<string>(v => v == "UPEUPE001"),
                            It.Is<string>(v => v == "@ST_ID"),
                            It.Is<string>(v => v == "SGCMSG"),
                            It.Is<string>(v => v == "@value"),
                            It.IsAny<string>(),
                            It.Is<string>(v => v == "@referenceType"),
                            It.Is<string>(v => v == "CR"))).Returns("fakeConsignment");
            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue(
                            It.Is<string>(v => v == "SGCMSG"),
                            It.Is<string>(v => v == "SGCustomsTest"),
                            It.Is<string>(v => v == "UPEUPE001"),
                            It.IsAny<string>(),
                            It.IsAny<string>(),
                            It.IsAny<string>()));
            mockIContextAccessor.Setup(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")).Returns("ACKSuccess");
            mockIContextAccessor.Setup(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", ""));

            var extensionObjects = new Dictionary<string, object>()
            {
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockIContextAccessor.Object },
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor.Object }
            };

            Assert.True(BizTalkXsltTester.Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);

        }
    }
}
