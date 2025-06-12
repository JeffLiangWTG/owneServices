//using System.Collections.Generic;
//using System.Reflection;
//using Xunit;
//using Hawking.UnitTest.Tools.Xslt;


//using Hawking.Xslt.ExtensionMocks.Interfaces;
//using Moq;

//namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests
//{
//        public class UShipment2UEventTests
//    {
//        const string filePath = "UShipment2UEvent.TestFiles.";

//        [Fact]
//        public void TestUShipment2UEventAIRAED()
//        {
//            AssertMapping("Test1AIRAED_input.xml", "Test1AIRAED_output.xml");
//        }

//        [Fact]
//        public void TestUShipment2UEventAIRAEU()
//        {
//            AssertMapping("Test1AIRAEU_input.xml", "Test1AIRAEU_output.xml");
//        }


//        [Fact]
//        public void TestUShipment2UEventAIRPCM()
//        {
//            AssertMapping("Test1AIRPCM_input.xml", "Test1AIRPCM_output.xml");
//        }


//        [Fact]
//        public void TestUShipment2UEventAIRPCU()
//        {
//            AssertMapping("Test1AIRPCU_input.xml", "Test1AIRPCU_output.xml");
//        }

//        private void AssertMapping(string inputFile, string expectedOutputFile)
//        {
//            var input = filePath + inputFile;
//            var expectedOutput = filePath + expectedOutputFile;

//            var mockIDateMapper= new Mock<IDateMapper>();
//            var mockICodeMapper= new Mock<ICodeMapper>();
//            var mockDataModelAccessor = new Mock<IDataModelAccessor>();
//            var mockIContextAccessor = new Mock<IContextAccessor>();
//            var mockSGCustomsDataModelAccessor = new Mock<IDataModelAccessor>();


//            mockIContextAccessor.Setup(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("PRET1.PRET001");
//            mockIContextAccessor.Setup(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("VWGT.VWGT001");
//            mockSGCustomsDataModelAccessor.Setup(x => x.GetSGCustomsAccount("BKG", "PRET1.PRET001")).Returns("VWGT001");
//            mockSGCustomsDataModelAccessor.Setup(x => x.GetSGCustomsSenderID("VWGT001")).Returns("VWGT.VWGT001");
//            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns("2017-08-23T11:30:01");
//            mockIDateMapper.Setup(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-08-23T11:30:01", "SGSIN")).Returns("2017-08-23T18:30:01");

//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "MAN0000092", "ManifestNumber"));
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "1", "UpdateNumber"));
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "012-98765432", "MAWB"));
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "HAWB1", "HAWB"));
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB100001E", "000012", "CNRF"));
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB1E", "201612334A|20170823|0001", "IDT"));
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB1E", "00001", "CST"));
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB100001E", "2", "CR"));

//            var extensionObjects = new Dictionary<string, object>() {
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockIContextAccessor.Object },
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor.Object },
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS5", mockSGCustomsDataModelAccessor.Object }
//			};

//            Assert.True(BizTalkXsltTester.Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);

//         //   mapTester.ExecuteCompiledWithXslDebug<UShipment2UEvent>(input, expectedOutput, @"C:\eServices4\eHub\Products\SGCustoms\MHAccess\BizTalk\Send\Maps\UShipment2UEvent\UShipment2UEvent.xsl");

//            mockIDateMapper.VerifyAll();
//            mockICodeMapper.VerifyAll();
//            mockIContextAccessor.VerifyAll();
//        }
//    }
//}
