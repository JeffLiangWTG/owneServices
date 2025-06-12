//using System.Collections.Generic;
//using System.Reflection;
//using Xunit;
//using Hawking.UnitTest.Tools.Xslt;


//using Hawking.Xslt.ExtensionMocks.Interfaces;
//using Moq;

//namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests
//{
//        public class UShipment2AIRPCUTests
//    {
//        const string filePath = "UShipment2AIRPCU.TestFiles.";

//        [Fact]
//        public void TestUShipment2AIRPCU110HAWBWith11HavingMoreThan50PackLinesAndEndingOnOverCST()
//        {
//            AssertMapping("Test1_input.xml", "Test1_output.xml");

//        }

//        [Fact]
//        public void TestUShipment2AIRPCU66PackLinesAnd2HAWB()
//        {
//            AssertMapping("Test2_input.xml", "Test2_output.xml");

//        }

//        [Fact]
//        public void TestUShipment2AIRPCUBasic()
//        {
//            AssertMapping("Test3_input.xml", "Test3_output.xml");

//        }

//        [Fact]
//        public void TestUShipment2AIRPCU110HAWBWithLessThan50PackLines()
//        {
//            AssertMapping("Test4_input.xml", "Test4_output.xml");

//        }

//        private void AssertMapping(string inputFile, string expectedOutputFile)
//        {
//            var input = filePath + inputFile;
//            var expectedOutput = filePath + expectedOutputFile;

//            var IDTCounterStartingValue = 1;
//            var SubScribedIDTCounter = 0 ;
//            var SubScribedIDTSuffix = 0;

//            var mockIDateMapper= new Mock<IDateMapper>();
//            var mockICodeMapper= new Mock<ICodeMapper>();
//            var mockDataModelAccessor = new Mock<IDataModelAccessor>();
//            var mockIContextAccessor = new Mock<IContextAccessor>();
//            var mockSGCustomsDataModelAccessor = new Mock<SGCustomsDataModelAccessor>();

//            mockIContextAccessor.Setup(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("SGCustomsTest");
//            mockIContextAccessor.Setup(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("UPEUPE001");
//            mockDataModelAccessor.Setup(x => x.GetClientRegistrationCode("UPEUPE001", "", "SGCustoms")).Returns("UPE1.UPE001");
//            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns("2017-09-06T12:00");
//            mockIDateMapper.Setup(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-09-06T12:00", "SGSIN")).Returns("2017-09-06T04:00");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT", "@maxlength", "4"))
//                .Return(IDTCounterStartingValue.ToString())
//                ;
//                .WhenCalled(x => x.ReturnValue = IDTCounterStartingValue++.ToString());
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper(It.Is<string>(v => v == "SelectSubscribedReference"),
//                            It.Is<string>(v => v == "@reference"),
//                            It.Is<string>(v => v == "@senderId"),
//                            It.Is<string>(v => v == "SGCustomsTest"),
//                            It.Is<string>(v => v == "@recipientId"),
//                            It.Is<string>(v => v == "UPEUPE001"),
//                            It.Is<string>(v => v == "@ST_ID"),
//                            It.Is<string>(v => v == "SGCMSG"),
//                            It.Is<string>(v => v == "@value"),
//                            It.IsAny<string>(),
//                            It.Is<string>(v => v == "@referenceType"),
//                            It.Is<string>(v => v == "IDT"))).Returns("IDT1")
//                            .WhenCalled(x =>
//                            {
//                                if (SubScribedIDTCounter % 110 == 0) SubScribedIDTSuffix++;
//                                x.ReturnValue = "IDT" + SubScribedIDTSuffix.ToString() + "|IDT1.2|IDT1.3";
//                                SubScribedIDTCounter++;
//                            });
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper(It.Is<string>(v => v == "SelectSubscribedReference"),
//                            It.Is<string>(v => v == "@reference"),
//                            It.Is<string>(v => v == "@senderId"),
//                            It.Is<string>(v => v == "SGCustomsTest"),
//                            It.Is<string>(v => v == "@recipientId"),
//                            It.Is<string>(v => v == "UPEUPE001"),
//                            It.Is<string>(v => v == "@ST_ID"),
//                            It.Is<string>(v => v == "SGCMSG"),
//                            It.Is<string>(v => v == "@value"),
//                            It.IsAny<string>(),
//                            It.Is<string>(v => v == "@referenceType"), 
//                            It.Is<string>(v => v == "CycleDate"))).Returns("20170906");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper(It.Is<string>(v => v == "SelectSubscribedReference"),
//                            It.Is<string>(v => v == "@reference"),
//                            It.Is<string>(v => v == "@senderId"),
//                            It.Is<string>(v => v == "SGCustomsTest"),
//                            It.Is<string>(v => v == "@recipientId"),
//                            It.Is<string>(v => v == "UPEUPE001"),
//                            It.Is<string>(v => v == "@ST_ID"),
//                            It.Is<string>(v => v == "SGCMSG"),
//                            It.Is<string>(v => v == "@value"),
//                            It.IsAny<string>(),
//                            It.Is<string>(v => v == "@referenceType"),
//                            It.Is<string>(v => v == "CycleNumber"))).Returns("21");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper(It.Is<string>(v => v == "SelectSubscribedReference"),
//                            It.Is<string>(v => v == "@reference"),
//                            It.Is<string>(v => v == "@senderId"),
//                            It.Is<string>(v => v == "SGCustomsTest"),
//                            It.Is<string>(v => v == "@recipientId"),
//                            It.Is<string>(v => v == "UPEUPE001"),
//                            It.Is<string>(v => v == "@ST_ID"),
//                            It.Is<string>(v => v == "SGCMSG"),
//                            It.Is<string>(v => v == "@value"),
//                            It.IsAny<string>(),
//                            It.Is<string>(v => v == "@referenceType"),
//                            It.Is<string>(v => v == "CST"))).Returns("fakeCST");
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue(
//                            It.Is<string>(v => v == "SGCMSG"),
//                            It.Is<string>(v => v == "SGCustomsTest"),
//                            It.Is<string>(v => v == "UPEUPE001"),
//                            It.IsAny<string>(),
//                            It.IsAny<string>(),
//                            It.IsAny<string>()));
//            mockSGCustomsDataModelAccessor.Setup(x => x.GetSGCustomsAccount("BKG", "UPEUPE001")).Returns("vgwt001");
//            mockSGCustomsDataModelAccessor.Setup(x => x.GetSGCustomsSenderID("vgwt001")).Returns("vgwt.vgwt001");
//            mockIContextAccessor.Setup(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "vgwt001"));

//            var extensionObjects = new Dictionary<string, object>()
//			{
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockIContextAccessor.Object },
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor.Object },
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS5", mockSGCustomsDataModelAccessor.Object }
//			};

//            Assert.True(BizTalkXsltTester.Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);



//        }
//    }
//}
