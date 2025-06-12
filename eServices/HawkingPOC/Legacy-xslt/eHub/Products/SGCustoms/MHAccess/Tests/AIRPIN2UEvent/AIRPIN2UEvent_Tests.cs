//using System.Collections.Generic;
//using System.Reflection;
//using Xunit;
//using Hawking.UnitTest.Tools.Xslt;
//using Hawking.Xslt.ExtensionMocks.Interfaces;
//using Moq;

//namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests
//{
//    public class AIRPIN2UEvent_Tests
//    {
//        const string xsl = "AIRPIN2UEvent.MapFiles.AIRPIN2UEvent.xsl";
//        const string filePath = "AIRPIN2UEvent.TestFiles.";

//        [Fact]
//        public void TestAIRPIN2UEvent_DetailsProvided()
//        {
//            var input = filePath + "Test1_input.xml";
//            var expectedOutput = filePath + "Test1_output.xml";

//            var mockIDateMapper = new Mock<IDateMapper>();
//            var mockICodeMapper = new Mock<ICodeMapper>();
//            var mockIContextAccessor = new Mock<IContextAccessor>();

//            mockIContextAccessor.Setup(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("UPEUPE001");
//            mockIContextAccessor.Setup(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("PRET1.PRET001");

//            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("yyyy-MM-ddTHH:mm:ssK")).Returns("2017-09-11T04:00:00");

//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711I", "@referenceType", "ManifestNumber")).Returns("MANIFEST001");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D20170623971142236YTD74FI", "@referenceType", "CNRF")).Returns("002361");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711V0223557999I", "@referenceType", "CNRF")).Returns("002372");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711V0225454220I", "@referenceType", "CNRF")).Returns("002383");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711V0273622618I", "@referenceType", "CNRF")).Returns("002394");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711V910823JVKLI", "@referenceType", "CNRF")).Returns("002405");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711W9W462G8JXJI", "@referenceType", "CNRF")).Returns("002416");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711I", "@referenceType", "HAWB")).Returns("42236YTD74F|V0223557999|V0225454220|V0273622618|V910823JVKL|W9W462G8JXJ");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711MANIFEST00142236YTD74F1I", "@referenceType", "Status")).Returns("Declared");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711MANIFEST001V02235579992I", "@referenceType", "Status")).Returns("Declared");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711MANIFEST001V02254542203I", "@referenceType", "Status")).Returns("Declared");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711MANIFEST001V02736226184I", "@referenceType", "Status")).Returns("Declared");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711MANIFEST001V910823JVKL5I", "@referenceType", "Status")).Returns("Declared");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711MANIFEST001W9W462G8JXJ6I", "@referenceType", "Status")).Returns("Declared");

//            var extensionObjects = new Dictionary<string, object>()
//            {
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockIContextAccessor.Object },
//            };

//            Assert.True(BizTalkXsltTester.Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);

//        }

//        [Fact]
//        public void TestAIRPIN2UEvent_DetailsInferred()
//        {
//            var input = filePath + "Test2_input.xml";
//            var expectedOutput = filePath + "Test2_output.xml";

//            var mockIDateMapper = new Mock<IDateMapper>();
//            var mockICodeMapper = new Mock<ICodeMapper>();
//            var mockIContextAccessor = new Mock<IContextAccessor>();

//            mockIContextAccessor.Setup(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("UPEUPE001");
//            mockIContextAccessor.Setup(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("PRET1.PRET001");

//            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("yyyy-MM-ddTHH:mm:ssK")).Returns("2017-09-11T04:00:00");

//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711I", "@referenceType", "ManifestNumber")).Returns("MANIFEST001");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711I", "@referenceType", "HAWB")).Returns("W9W462G8JXJ|V0225454220|V910823JVKL|42236YTD74F|V0223557999|V0273622618");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D20170623971142236YTD74FI", "@referenceType", "CNRF")).Returns("00236ZZZZ1");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711V0223557999I", "@referenceType", "CNRF")).Returns("00237ZZZZ2");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711V0225454220I", "@referenceType", "CNRF")).Returns("00238ZZZZ3");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711V0273622618I", "@referenceType", "CNRF")).Returns("00239ZZZZ4");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711V910823JVKLI", "@referenceType", "CNRF")).Returns("00240ZZZZ5");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711W9W462G8JXJI", "@referenceType", "CNRF")).Returns("00241ZZZZ6|00242ZZZZ7");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711MANIFEST001W9W462G8JXJZZZZ6I", "@referenceType", "Status")).Returns("Declared");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711MANIFEST001V0225454220ZZZZ3I", "@referenceType", "Status")).Returns("Declared");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711MANIFEST001V910823JVKLZZZZ5I", "@referenceType", "Status")).Returns("Declared");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711MANIFEST00142236YTD74FZZZZ1I", "@referenceType", "Status")).Returns("Declared");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711MANIFEST001V0223557999ZZZZ2I", "@referenceType", "Status")).Returns("Declared");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711MANIFEST001V0273622618ZZZZ4I", "@referenceType", "Status")).Returns("Declared");

//            var extensionObjects = new Dictionary<string, object>()
//            {
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockIContextAccessor.Object }
//            };

//            Assert.True(BizTalkXsltTester.Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);
//        }

//        [Fact]
//        public void TestAIRPIN2UEvent_DetailsInferred_WithSomeErrors()
//        {
//            var input = filePath + "Test4_input.xml";
//            var expectedOutput = filePath + "Test4_output.xml";

//            var mockIDateMapper = new Mock<IDateMapper>();
//            var mockICodeMapper = new Mock<ICodeMapper>();
//            var mockIContextAccessor = new Mock<IContextAccessor>();

//            mockIContextAccessor.Setup(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("UPEUPE001");
//            mockIContextAccessor.Setup(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("PRET1.PRET001");

//            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("yyyy-MM-ddTHH:mm:ssK")).Returns("2017-09-11T04:00:00");

//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711I", "@referenceType", "ManifestNumber")).Returns("MANIFEST001");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711I", "@referenceType", "HAWB")).Returns("W9W462G8JXJ|V0225454220|V910823JVKL|42236YTD74F|V0223557999|V0273622618");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D20170623971142236YTD74FI", "@referenceType", "CNRF")).Returns("00236ZZZZ1");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711V0223557999I", "@referenceType", "CNRF")).Returns("00237ZZZZ2");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711V0225454220I", "@referenceType", "CNRF")).Returns("00238ZZZZ3");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711V0273622618I", "@referenceType", "CNRF")).Returns("00239ZZZZ4");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711V910823JVKLI", "@referenceType", "CNRF")).Returns("00240ZZZZ5");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711W9W462G8JXJI", "@referenceType", "CNRF")).Returns("00241ZZZZ6|00242ZZZZ7");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711MANIFEST001W9W462G8JXJZZZZ6I", "@referenceType", "Status")).Returns("Declared");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711MANIFEST001V0225454220ZZZZ3I", "@referenceType", "Status")).Returns("Declared");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711MANIFEST001V910823JVKLZZZZ5I", "@referenceType", "Status")).Returns("Error");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711MANIFEST00142236YTD74FZZZZ1I", "@referenceType", "Status")).Returns("Declared");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711MANIFEST001V0223557999ZZZZ2I", "@referenceType", "Status")).Returns("Declared");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711MANIFEST001V0273622618ZZZZ4I", "@referenceType", "Status")).Returns("Error");

//            var extensionObjects = new Dictionary<string, object>()
//            {
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockIContextAccessor.Object }
//            };

//            Assert.True(BizTalkXsltTester.Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);

//        }

//        [Fact]
//        public void TestAIRPIN2UEvent_DetailsProvidedHouseBillHasMostRestrictiveStatus()
//        {
//            var input = filePath + "Test3_input.xml";
//            var expectedOutput = filePath + "Test3_output.xml";

//            var mockIDateMapper = new Mock<IDateMapper>();
//            var mockICodeMapper = new Mock<ICodeMapper>();
//            var mockIContextAccessor = new Mock<IContextAccessor>();

//            mockIContextAccessor.Setup(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("UPEUPE001");
//            mockIContextAccessor.Setup(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("PRET1.PRET001");

//            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("yyyy-MM-ddTHH:mm:ssK")).Returns("2017-09-11T04:00:00");

//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711I", "@referenceType", "ManifestNumber")).Returns("MANIFEST001");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711W9W462G8JXJI", "@referenceType", "CNRF")).Returns("003681|002412");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711V0225454220I", "@referenceType", "CNRF")).Returns("002383|009384");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711I", "@referenceType", "HAWB")).Returns("W9W462G8JXJ|V0225454220");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711MANIFEST001W9W462G8JXJ1I", "@referenceType", "Status")).Returns("Declared");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711MANIFEST001W9W462G8JXJ2I", "@referenceType", "Status")).Returns("Declared");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711MANIFEST001V02254542203I", "@referenceType", "Status")).Returns("Declared");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "PRET1.PRET001", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239711MANIFEST001V02254542204I", "@referenceType", "Status")).Returns("Declared");

//            var extensionObjects = new Dictionary<string, object>()
//            {
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockIContextAccessor.Object },
//            };

//            Assert.True(BizTalkXsltTester.Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);
//        }
//    }
//}
