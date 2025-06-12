using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using Xunit;
using Hawking.UnitTest.Tools.Xslt;

using Hawking.Xslt.ExtensionObjects.Interfaces;
using Moq;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests
{
    public class AIRAEU2UEvent_Tests
    {
        const string filePath = "AIRAEU2UEvent.TestFiles.";
        const string xsl = "AIRAEU2UEvent.MapFiles.AIRAEU2UEvent.xsl";

        [Fact]
        public void TestAIRAEU2UEvent_AmendBasic_failure()
        {
            var input = filePath + "Test1_input.xml";
            var expectedOutput = filePath + "Test1_output_failure.xml";

            var mockIDateMapper= new Mock<IDateMapper>();
            var mockICodeMapper= new Mock<ICodeMapper>();
            var mockIContextAccessor = new Mock<IContextAccessor>();

            mockIContextAccessor
                .Setup(x => x.GetContextProperty("SourceParty",
                    "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("VWGT.VWGT001");
            mockIContextAccessor
                .Setup(x => x.GetContextProperty("DestinationParty",
                    "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("PRET1.PRET001");
            mockIContextAccessor
                .Setup(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"))
                .Returns(
                    "ErrorCode: 1325;LinkedOutboxMessageTrackingID: 3DF09E8A-C79A-4F6A-95BC-E790A38FA7FB. ErrorMessage: Invalid User ID / Password");
            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns("2017-09-20T14:21:01");
            mockIContextAccessor.Setup(x =>
                x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", ""));

            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value",
                "0813243534EXPHB1MAN000001700038E", "@referenceType", "CNRF")).Returns("000011|000022");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value",
                "0813243534EXPHB2MAN000001700056E", "@referenceType", "CNRF")).Returns("000013");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value",
                "0813243534EXPHB3MAN000001700074E", "@referenceType", "CNRF")).Returns("000014|000025");

            var extensionObjects = new Dictionary<string, object>()
            {
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockIContextAccessor.Object},
            };

            Assert.True(BizTalkXsltTester
                .Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);

            mockIDateMapper.VerifyAll();
            mockICodeMapper.VerifyAll();
            mockIContextAccessor.VerifyAll();
        }

        [Fact]
        public void TestAIRAEU2UEvent_AmendBasic_success()
        {
            var input = filePath + "Test1_input.xml";
            var expectedOutput = filePath + "Test1_output_success.xml";

            var mockIDateMapper= new Mock<IDateMapper>();
            var mockICodeMapper= new Mock<ICodeMapper>();
            var mockIContextAccessor = new Mock<IContextAccessor>();

            mockIContextAccessor
                .Setup(x => x.GetContextProperty("SourceParty",
                    "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("VWGT.VWGT001");
            mockIContextAccessor
                .Setup(x => x.GetContextProperty("DestinationParty",
                    "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("PRET1.PRET001");
            mockIContextAccessor
                .Setup(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"))
                .Returns("ACKSuccess");
            mockIContextAccessor.Setup(x =>
                x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", ""));
            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns("2017-09-20T14:21:01");

            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value",
                "0813243534EXPHB1MAN000001700038E", "@referenceType", "CNRF")).Returns("000011|000022");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value",
                "0813243534EXPHB2MAN000001700056E", "@referenceType", "CNRF")).Returns("000013");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value",
                "0813243534EXPHB3MAN000001700074E", "@referenceType", "CNRF")).Returns("000014|000025");

            var extensionObjects = new Dictionary<string, object>()
            {
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockIContextAccessor.Object},
            };

            Assert.True(BizTalkXsltTester
                .Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);


            mockIDateMapper.VerifyAll();
            mockICodeMapper.VerifyAll();
            mockIContextAccessor.VerifyAll();
        }

        [Fact]
        public void TestAIRAEU2UEvent_Amend1IDT100HAWB_MAWB()
        {
            var input = filePath + "Test2_input.xml";
            var expectedOutput = filePath + "Test2_output.xml";

            int SubscribedCSTCounter = 1;

            var mockIDateMapper= new Mock<IDateMapper>();
            var mockICodeMapper= new Mock<ICodeMapper>();
            var mockIContextAccessor = new Mock<IContextAccessor>();

            mockIContextAccessor
                .Setup(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("VWGT.VWGT001");
            mockIContextAccessor
                .Setup(x => x.GetContextProperty("DestinationParty","http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("PRET1.PRET001");
            mockIContextAccessor
                .Setup(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"))
                .Returns(
                    "ErrorCode: 1325;LinkedOutboxMessageTrackingID: 3DF09E8A-C79A-4F6A-95BC-E790A38FA7FB. ErrorMessage: Invalid User ID / Password");
            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns("2017-09-20T14:21:01");
            mockIContextAccessor.Setup(x =>
                x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", ""));

            var returns = new Queue<string>();
            returns.Enqueue("000011|000022");
            returns.Enqueue("000011");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper(It.Is<string>(v => v == "SelectSubscribedReference"),
                It.Is<string>(v => v == "@reference"),
                It.Is<string>(v => v == "@senderId"),
                It.Is<string>(v => v == "VWGT.VWGT001"),
                It.Is<string>(v => v == "@recipientId"),
                It.Is<string>(v => v == "PRET1.PRET001"),
                It.Is<string>(v => v == "@ST_ID"),
                It.Is<string>(v => v == "SGCMSG"),
                It.Is<string>(v => v == "@value"),
                It.IsAny<string>(),
                It.Is<string>(v => v == "@referenceType"),
                It.Is<string>(v => v == "CNRF"))).Returns(() =>
            {
                var returnValue = returns.Count > 1 ? returns.Dequeue() : returns.Peek();
                if (SubscribedCSTCounter % 10 == 0)
                {
                    return "000011";
                }

                SubscribedCSTCounter++;

                return returnValue;
            });

            var extensionObjects = new Dictionary<string, object>()
            {
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockIContextAccessor.Object},
            };

            var result = BizTalkXsltTester.Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects);
            Assert.True(!string.IsNullOrEmpty(result.Actual));
        }

        [Fact]
        public void TestAIRAEU2UEvent_Amend1IDT1HAWBOver2CST()
        {
            var input = filePath + "Test3_input.xml";
            var expectedOutput = filePath + "Test3_output.xml";

            var mockIDateMapper= new Mock<IDateMapper>();
            var mockICodeMapper= new Mock<ICodeMapper>();
            var mockIContextAccessor = new Mock<IContextAccessor>();

            mockIContextAccessor
                .Setup(x => x.GetContextProperty("SourceParty",
                    "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("VWGT.VWGT001");
            mockIContextAccessor
                .Setup(x => x.GetContextProperty("DestinationParty",
                    "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("PRET1.PRET001");
            mockIContextAccessor
                .Setup(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"))
                .Returns(
                    "ErrorCode: 1325;LinkedOutboxMessageTrackingID: 3DF09E8A-C79A-4F6A-95BC-E790A38FA7FB. ErrorMessage: Invalid User ID / Password");
            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns("2017-09-20T14:21:01");
            mockIContextAccessor.Setup(x =>
                x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", ""));

            mockICodeMapper
                .Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId",
                    "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value",
                    "012-98765432HAWB1MAN000009200038E", "@referenceType", "CNRF")).Returns(
                    "000011|000022|000033|000044|000055|000066|000077|000088|000099|0001010|0001111|0001212|0001313|0001414|0001515|0001616|0001717|0001818|0001919|0002020|0002121|0002222|0002323|0002424|0002525|0002626|0002727|0002828|0002929|0003030|0003131|0003232|0003333|0003434|0003535|0003636|0003737|0003838|0003939|0004040|0004141|0004242|0004343|0004444|0004545|0004646|0004747|0004848|0004949|0005050");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value",
                "012-98765432HAWB1MAN000009200039E", "@referenceType", "CNRF")).Returns(
                "0005151|0005252|0005353|0005454|0005555|0005656|0005757|0005858|0005959|0006060|0006161|0006262|0006363|0006464|0006565|0006666");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                    "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value",
                    "012-98765432HAWB2MAN000009200056E", "@referenceType", "CNRF"))
                .Returns("000011|000022|000033|000044|000055");

            var extensionObjects = new Dictionary<string, object>()
            {
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockIContextAccessor.Object},
            };

            Assert.True(BizTalkXsltTester
                .Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);


            mockIDateMapper.VerifyAll();
            mockICodeMapper.VerifyAll();
            mockIContextAccessor.VerifyAll();
        }


        [Fact]
        public void TestAIRAEU2UEvent_Cancel()
        {
            var input = filePath + "Test4_input.xml";
            var expectedOutput = filePath + "Test4_output.xml";

            var mockIDateMapper= new Mock<IDateMapper>();
            var mockICodeMapper= new Mock<ICodeMapper>();
            var mockIContextAccessor = new Mock<IContextAccessor>();

            mockIContextAccessor
                .Setup(x => x.GetContextProperty("SourceParty",
                    "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("VWGT.VWGT001");
            mockIContextAccessor
                .Setup(x => x.GetContextProperty("DestinationParty",
                    "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("PRET1.PRET001");
            mockIContextAccessor
                .Setup(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"))
                .Returns(
                    "ErrorCode: 1325;LinkedOutboxMessageTrackingID: 3DF09E8A-C79A-4F6A-95BC-E790A38FA7FB. ErrorMessage: Invalid User ID / Password");
            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns("2017-09-20T14:21:01");
            mockIContextAccessor.Setup(x =>
                x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", ""));

            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value",
                "199702247W201708230001E", "@referenceType", "MAWB")).Returns("012-98765432");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value",
                "199702247W201708230001E", "@referenceType", "HAWB")).Returns("HAWB1|HAWB2");

            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value",
                "012-98765432HAWB1MAN0000017E", "@referenceType", "CST")).Returns("00001|00002");
            mockICodeMapper
                .Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId",
                    "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value",
                    "012-98765432HAWB1MAN000001700001E", "@referenceType", "CNRF")).Returns(
                    "000011|000022|000033|000044|000055|000066|000077|000088|000099|0001010|0001111|0001212|0001313|0001414|0001515|0001616|0001717|0001818|0001919|0002020|0002121|0002222|0002323|0002424|0002525|0002626|0002727|0002828|0002929|0003030|0003131|0003232|0003333|0003434|0003535|0003636|0003737|0003838|0003939|0004040|0004141|0004242|0004343|0004444|0004545|0004646|0004747|0004848|0004949|0005050");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value",
                "012-98765432HAWB1MAN000001700002E", "@referenceType", "CNRF")).Returns(
                "0005151|0005252|0005353|0005454|0005555|0005656|0005757|0005858|0005959|0006060|0006161|0006262|0006363|0006464|0006565|0006666");

            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value",
                "012-98765432HAWB2MAN0000017E", "@referenceType", "CST")).Returns("00003");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                    "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value",
                    "012-98765432HAWB2MAN000001700003E", "@referenceType", "CNRF"))
                .Returns("000011|000022|000033|000044|000055||000066");

            var extensionObjects = new Dictionary<string, object>()
            {
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockIContextAccessor.Object},
            };

            Assert.True(BizTalkXsltTester
                .Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);
        }

        [Fact]
        public void TestAIRAEU2UEvent_PartialDelete()
        {
            var input = filePath + "Test5_input.xml";
            var expectedOutput = filePath + "Test5_output.xml";

            var mockIDateMapper= new Mock<IDateMapper>();
            var mockICodeMapper= new Mock<ICodeMapper>();
            var mockIContextAccessor = new Mock<IContextAccessor>();

            mockIContextAccessor
                .Setup(x => x.GetContextProperty("SourceParty",
                    "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("VWGT.VWGT001");
            mockIContextAccessor
                .Setup(x => x.GetContextProperty("DestinationParty",
                    "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("PRET1.PRET001");
            mockIContextAccessor
                .Setup(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"))
                .Returns(
                    "ErrorCode: 1325;LinkedOutboxMessageTrackingID: 3DF09E8A-C79A-4F6A-95BC-E790A38FA7FB. ErrorMessage: Invalid User ID / Password");
            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns("2017-09-20T14:21:01");
            mockIContextAccessor.Setup(x =>
                x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", ""));

            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value",
                "199702247W201708230001E", "@referenceType", "MAWB")).Returns("012-98765432");

            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value",
                "012-98765432EXPHB1MAN000001700038E", "@referenceType", "CNRF")).Returns("000011|000022|000033");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                    "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value",
                    "012-98765432EXPHB2MAN000001700038E", "@referenceType", "CNRF"))
                .Returns("000011|000022|000033|000044|000055");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference",
                    "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value",
                    "012-98765432EXPHB2MAN000001700057E", "@referenceType", "CNRF"))
                .Returns("000011|000022|000033|000044|000055");

            var extensionObjects = new Dictionary<string, object>()
            {
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockIContextAccessor.Object},
            };

            Assert.True(BizTalkXsltTester.Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);
        }
    }
}