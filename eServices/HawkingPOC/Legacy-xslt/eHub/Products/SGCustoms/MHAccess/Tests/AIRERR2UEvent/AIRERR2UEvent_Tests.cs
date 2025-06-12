using Hawking.UnitTest.Tools.Xslt;
using Hawking.Xslt.ExtensionObjects.Interfaces;

using Moq;
using System.Collections.Generic;
using Xunit;
using System.Reflection;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests
{
    public class AIRERR2UEvent_Tests
    {
        const string filePath = "AIRERR2UEvent.TestFiles.";
        const string testDate = "2017-09-20T14:21:01";
        const string manifestNumber = "MAN0000057";
        const string xsl = "AIRERR2UEvent.MapFiles.AIRERR2UEvent.xsl";

        [Fact]
        public void TestAIRERR2UEvent_Import_SingleERRWithLineReference()
        {
            AssertMappingAIRPCM("Test1_input.xml", "Test1_output.xml");
        }

        [Fact]
        public void TestAIRERR2UEvent_Export_AIRAEDSingleERRWithLineReference()
        {
            AssertMappingExportAIRAED("Test2_input.xml", "Test2_output.xml");
        }

        [Fact]
        public void TestAIRERR2UEvent_Import_ManyERROneWithLineReferenceOneWithout()
        {
            AssertMappingAIRPCM("Test3_input.xml", "Test3_output.xml");
        }

        [Fact]
        public void TestAIRERR2UEvent_Import_UsingIDT()
        {
            AssertMappingAIRPCM("Test4_input.xml", "Test4_output.xml");
        }

        [Fact]
        public void TestAIRERR2UEvent_Export_AIRAEUUsingIDT()
        {
            AssertMappingExportAIRAEU("Test5_input.xml", "Test5_output.xml");
        }

        [Fact]
        public void TestAIRERR2UEvent_Export_AIRAEDManyERRUsingIDT()
        {
            AssertMappingExportAIRAED("Test6_input.xml", "Test6_output.xml");
        }

        [Fact]
        public void TestAIRERR2UEvent_Export_InvalidSequenceNumber()
        {
            AssertMappingExportSequenceNumber("Test7_input.xml", "Test7_output.xml");
        }

        [Fact]
        public void TestAIRERR2UEvent_Export_DecrementSequenceNumber()
        {
            AssertMappingExportSequenceNumber("Test8_input.xml", "Test8_output.xml");
        }

        [Fact]
        public void TestAIRERR2UEvent_Export_AIRAEUWithLineReference()
        {
            AssertMappingExportAIRAEU("Test9_input.xml", "Test9_output.xml");
        }

        [Fact]
        public void TestAIRERR2UEvent_Export_AIRAEDforSER()
        {
            AssertMappingExportAIRAED_SER("Test10_input.xml", "Test10_output.xml");
        }

        [Fact]
        public void TestAIRERR2UEvent_Export_UpdateNotInSequence()
        {
            AssertMappingExportAIRAEU("Test11_input.xml", "Test11_output.xml");
        }

        void AssertMappingAIRPCM(string inputFile, string expectedOutputFile)
        {
            var input = filePath + inputFile;
            var expectedOutput = filePath + expectedOutputFile;

            var mockIDateMapper= new Mock<IDateMapper>();
            var mockICodeMapper= new Mock<ICodeMapper>();
            var mockIContextAccessor = new Mock<IContextAccessor>();
            var mockDataModelAccessor = new Mock<IDataModelAccessor>();

            mockIContextAccessor.Setup(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("PRET1.PRET001");
            mockIContextAccessor.Setup(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("UPEUPE001");
            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns("2017-09-20T14:21:01");

            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239709I", "@referenceType", "ManifestNumber")).Returns(manifestNumber);
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D2017062397092V562FSZJBX00083I", "@referenceType", "CR")).Returns("37");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239709AE5168THCZS00020I", "@referenceType", "CR")).Returns("48");

            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239709I", "@referenceType", "MAWB")).Returns("MAWB1");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239709I", "@referenceType", "HAWB")).Returns("HAWB1");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239709HAWB1I", "@referenceType", "CNRF")).Returns("0000113|0000214");

            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue(
                            It.Is<string>(v => v == "SGCMSG"),
                            It.Is<string>(v => v == "UPEUPE001"),
                            It.Is<string>(v => v == "PRET1.PRET001"),
                            It.IsAny<string>(),
                            It.IsAny<string>(),
                            It.IsAny<string>()));

            var extensionObjects = new Dictionary<string, object>()
            {
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockIContextAccessor.Object },
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor.Object }

            };

            Assert.True(BizTalkXsltTester.Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);
        }

        void AssertMappingExportSequenceNumber(string inputFile, string expectedOutputFile)
        {
            var input = filePath + inputFile;
            var expectedOutput = filePath + expectedOutputFile;

            var mockIDateMapper= new Mock<IDateMapper>();
            var mockICodeMapper= new Mock<ICodeMapper>();
            var mockIContextAccessor = new Mock<IContextAccessor>();
            var mockDataModelAccessor = new Mock<IDataModelAccessor>();

            mockIContextAccessor.Setup(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("PRET1.PRET001");
            mockIContextAccessor.Setup(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("UPEUPE001");
            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns(testDate);

            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239709E", "@referenceType", "OriginalIDT")).Returns("198801949D201709280055");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201709280055E", "@referenceType", "UpdateNumber")).Returns("2");
            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "UPEUPE001", "PRET1.PRET001", "198801949D201709280055E", "1", "UpdateNumber"));
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201709280055E", "@referenceType", "ManifestNumber")).Returns(manifestNumber);
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239709E", "@referenceType", "ManifestNumber")).Returns(manifestNumber);
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "406 23HKG 06172V562FSZJBXMAN000005700083E", "@referenceType", "CNRF")).Returns("0000621");

            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "UPEUPE001", "PRET1.PRET001", "198801949D201709280055E", "001", "UpdateNumber"));
            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue(
                            It.Is<string>(v => v == "SGCMSG"),
                            It.Is<string>(v => v == "UPEUPE001"),
                            It.Is<string>(v => v == "PRET1.PRET001"),
                            It.IsAny<string>(),
                            It.IsAny<string>(),
                            It.IsAny<string>()));

            var extensionObjects = new Dictionary<string, object>()
            {
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockIContextAccessor.Object },
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor.Object }

            };

            Assert.True(BizTalkXsltTester.Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);
        }

        void AssertMappingExportAIRAED(string inputFile, string expectedOutputFile)
        {
            var input = filePath + inputFile;
            var expectedOutput = filePath + expectedOutputFile;

            var mockIDateMapper= new Mock<IDateMapper>();
            var mockICodeMapper= new Mock<ICodeMapper>();
            var mockIContextAccessor = new Mock<IContextAccessor>();
            var mockDataModelAccessor = new Mock<IDataModelAccessor>();

            mockIContextAccessor.Setup(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("PRET1.PRET001");
            mockIContextAccessor.Setup(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("UPEUPE001");
            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns(testDate);

            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201709290026E", "@referenceType", "MAWB")).Returns("MAWB1");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201709290026E", "@referenceType", "ManifestNumber")).Returns(manifestNumber);
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201709290026E", "@referenceType", "HAWB")).Returns("HAWB1|HAWB2");

            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAWB1HAWB1MAN0000057E", "@referenceType", "CST")).Returns("00038");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAWB1HAWB1MAN000005700038E", "@referenceType", "CNRF")).Returns("0000167|0000268|0000369|0000470|0000571");

            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAWB1HAWB2MAN0000057E", "@referenceType", "CST")).Returns("00087");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAWB1HAWB2MAN000005700087E", "@referenceType", "CNRF")).Returns("000012|000023");

            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "406 23HKG 06172V562FSZJBXMAN000005700083E", "@referenceType", "CNRF")).Returns("0000621");

            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue(
                            It.Is<string>(v => v == "SGCMSG"),
                            It.Is<string>(v => v == "UPEUPE001"),
                            It.Is<string>(v => v == "PRET1.PRET001"),
                            It.IsAny<string>(),
                            It.IsAny<string>(),
                            It.IsAny<string>()));

            var extensionObjects = new Dictionary<string, object>()
        {
            { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
            { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
            { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockIContextAccessor.Object },
            { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor.Object }

        };

            Assert.True(BizTalkXsltTester.Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);
        }

        void AssertMappingExportAIRAED_SER(string inputFile, string expectedOutputFile)
        {
            var input = filePath + inputFile;
            var expectedOutput = filePath + expectedOutputFile;

            var mockIDateMapper= new Mock<IDateMapper>();
            var mockICodeMapper= new Mock<ICodeMapper>();
            var mockIContextAccessor = new Mock<IContextAccessor>();
            var mockDataModelAccessor = new Mock<IDataModelAccessor>();

            mockIContextAccessor.Setup(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("WTLDSGSGC");
            mockIContextAccessor.Setup(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("SGCustomsTest");
            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns(testDate);

            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "WTLDSGSGC", "@ST_ID", "SGCMSG", "@value", "198801949D201711270152E", "@referenceType", "ManifestNumber")).Returns("MAN0000192");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "WTLDSGSGC", "@ST_ID", "SGCMSG", "@value", "363636773BILL20171127H1MAN0000192E", "@referenceType", "CST")).Returns("00716");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "WTLDSGSGC", "@ST_ID", "SGCMSG", "@value", "363636773BILL20171127H1MAN000019200001E", "@referenceType", "CNRF")).Returns("");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "WTLDSGSGC", "@ST_ID", "SGCMSG", "@value", "363636773BILL20171127H1MAN000019200716E", "@referenceType", "CNRF")).Returns("000013|000024|000035|000046");

            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue(
                            It.Is<string>(v => v == "SGCMSG"),
                            It.Is<string>(v => v == "SGCustomsTest"),
                            It.Is<string>(v => v == "WTLDSGSGC"),
                            It.IsAny<string>(),
                            It.IsAny<string>(),
                            It.IsAny<string>()));

            var extensionObjects = new Dictionary<string, object>()
        {
            { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
            { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
            { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockIContextAccessor.Object },
            { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor.Object }

        };

            Assert.True(BizTalkXsltTester.Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);


            mockICodeMapper.VerifyAll();
            mockIContextAccessor.VerifyAll();
        }

        void AssertMappingExportAIRAEU(string inputFile, string expectedOutputFile)
        {
            var input = filePath + inputFile;
            var expectedOutput = filePath + expectedOutputFile;

            var mockIDateMapper= new Mock<IDateMapper>();
            var mockICodeMapper= new Mock<ICodeMapper>();
            var mockIContextAccessor = new Mock<IContextAccessor>();
            var mockDataModelAccessor = new Mock<IDataModelAccessor>();

            mockIContextAccessor.Setup(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("PRET1.PRET001");
            mockIContextAccessor.Setup(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("UPEUPE001");
            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns(testDate);

            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201709290026E", "@referenceType", "OriginalIDT")).Returns("198801949D20170928006");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D20170928006E", "@referenceType", "MAWB")).Returns("MAWB1");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201709290026E", "@referenceType", "MAWB")).Returns("MAWB1");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D20170928006E", "@referenceType", "HAWB")).Returns("HAWB1|HAWB2");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201709290026E", "@referenceType", "HAWB")).Returns("HAWB1|HAWB2");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D20170928006E", "@referenceType", "UpdateNumber")).Returns("2");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201709290026E", "@referenceType", "UpdateNumber")).Returns("2");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201706239709E", "@referenceType", "UpdateNumber")).Returns("1");

            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "UPEUPE001", "PRET1.PRET001", "198801949D20170928006E", "1", "UpdateNumber"));
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D20170928006E", "@referenceType", "ManifestNumber")).Returns(manifestNumber);
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201709290026E", "@referenceType", "ManifestNumber")).Returns(manifestNumber);

            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAWB1HAWB1MAN0000057E", "@referenceType", "CST")).Returns("00034");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAWB1HAWB1MAN000005700034E", "@referenceType", "CNRF")).Returns("0000185|0000291|0000372");

            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAWB1HAWB2MAN0000057E", "@referenceType", "CST")).Returns("00021");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAWB1HAWB2MAN000005700021E", "@referenceType", "CNRF")).Returns("000012");

            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "406 23HKG 06172V562FSZJBXMAN000005700083E", "@referenceType", "CNRF")).Returns("0000621");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201801171956E", "@referenceType", "MAWB")).Returns("MAWB1");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201801170074E", "@referenceType", "OriginalIDT")).Returns("198801949D201801171956");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201801171956E", "@referenceType", "UpdateNumber")).Returns("001");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201801171956E", "@referenceType", "ManifestNumber")).Returns("MAN0003944");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201801170074E", "@referenceType", "MAWB")).Returns("MAWB1");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201801170074E", "@referenceType", "HAWB")).Returns("R1F008H4KNK");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAWB1R1F008H4KNKMAN0003944E", "@referenceType", "CST")).Returns("238");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "MAWB1R1F008H4KNKMAN0003944238E", "@referenceType", "CNRF")).Returns("0000185|0000291|0000372");
            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "UPEUPE001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "198801949D201801170074E", "@referenceType", "ManifestNumber")).Returns("MAN0003944");

            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "WTLDSGSGC", "@ST_ID", "SGCMSG", "@value", "363636773BILL20171127H1MAN000019200001E", "@referenceType", "CNRF")).Returns("0000621");

            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue(
                               It.Is<string>(v => v == "SGCMSG"),
                            It.IsAny<string>(),
                            It.IsAny<string>(),
                               It.IsAny<string>(),
                               It.IsAny<string>(),
                               It.IsAny<string>()));

            var extensionObjects = new Dictionary<string, object>()
            {
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockIContextAccessor.Object },
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor.Object }

            };

            Assert.True(BizTalkXsltTester.Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);
        }

    }
}
