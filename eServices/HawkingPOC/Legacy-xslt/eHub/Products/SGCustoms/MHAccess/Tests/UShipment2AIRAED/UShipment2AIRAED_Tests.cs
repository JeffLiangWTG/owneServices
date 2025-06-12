//using System.Collections.Generic;
//using System.Reflection;
//using Xunit;
//using Hawking.UnitTest.Tools.Xslt;


//using Hawking.Xslt.ExtensionMocks.Interfaces;
//using Hawking.Xslt.ExtensionObjects.Interfaces;
//using Moq;

//namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests
//{
//    public class UShipment2AIRAED_Tests
//    {
//        const string xsl = "UShipment2AIRAED.MapFiles.UShipment2AIRAED.xsl";
//        const string filePath = "UShipment2AIRAED.TestFiles.";

//        [Fact]
//        public void TestUShipment2AIRAED110HAWBWith11HavingMoreThan50PackLinesAndEndingOnOverCST()
//        {
//            AssertMappingMessageValues("Test1_input.xml", "Test1_output.xml", "HAWB1|HAWB2|HAWB3|HAWB4|HAWB5|HAWB6|HAWB7|HAWB8|HAWB9|HAWB10|HAWB1|HAWB2|HAWB3|HAWB4|HAWB5|HAWB6|HAWB7|HAWB8|HAWB9|HAWB10|HAWB1|HAWB2|HAWB3|HAWB4|HAWB5|HAWB6|HAWB7|HAWB8|HAWB9|HAWB10|HAWB1|HAWB2|HAWB3|HAWB4|HAWB5|HAWB6|HAWB7|HAWB8|HAWB9|HAWB10|HAWB1|HAWB2|HAWB3|HAWB4|HAWB5|HAWB6|HAWB7|HAWB8|HAWB9|HAWB10|HAWB1|HAWB2|HAWB3|HAWB4|HAWB5|HAWB6|HAWB7|HAWB8|HAWB9|HAWB10|HAWB1|HAWB2|HAWB3|HAWB4|HAWB5|HAWB6|HAWB7|HAWB8|HAWB9|HAWB10|HAWB1|HAWB2|HAWB3|HAWB4|HAWB5|HAWB6|HAWB7|HAWB8|HAWB9|HAWB10|HAWB1|HAWB2|HAWB3|HAWB4|HAWB5|HAWB6|HAWB7|HAWB8|HAWB9|HAWB10", "HAWB1|HAWB2|HAWB3|HAWB4|HAWB5|HAWB6|HAWB7|HAWB8|HAWB9|HAWB10|HAWB1|HAWB2|HAWB3|HAWB4|HAWB5|HAWB6|HAWB7|HAWB8|HAWB9|HAWB10");
//        }

//        [Fact]
//        public void TestUShipment2AIRAED66PackLinesAnd2HAWB()
//        {
//            AssertMappingAndSubscriptionValues("Test2_input.xml", "Test2_output.xml");
//        }

//        [Fact]
//        public void TestUShipment2AIRAEDBasic()
//        {
//            AssertMappingBasic("Test3_input.xml", "Test3_output.xml");
//        }

//        [Fact]
//        public void TestUShipment2AIRAED110HAWBWithLessThan50PackLines()
//        {
//            AssertMappingMessageValues("Test4_input.xml", "Test4_output.xml", "HAWB1|HAWB2|HAWB3|HAWB4|HAWB5|HAWB6|HAWB7|HAWB8|HAWB9|HAWB10|HAWB1|HAWB2|HAWB3|HAWB4|HAWB5|HAWB6|HAWB7|HAWB8|HAWB9|HAWB10|HAWB1|HAWB2|HAWB3|HAWB4|HAWB5|HAWB6|HAWB7|HAWB8|HAWB9|HAWB10|HAWB1|HAWB2|HAWB3|HAWB4|HAWB5|HAWB6|HAWB7|HAWB8|HAWB9|HAWB10|HAWB1|HAWB2|HAWB3|HAWB4|HAWB5|HAWB6|HAWB7|HAWB8|HAWB9|HAWB10|HAWB1|HAWB2|HAWB3|HAWB4|HAWB5|HAWB6|HAWB7|HAWB8|HAWB9|HAWB10|HAWB1|HAWB2|HAWB3|HAWB4|HAWB5|HAWB6|HAWB7|HAWB8|HAWB9|HAWB10|HAWB1|HAWB2|HAWB3|HAWB4|HAWB5|HAWB6|HAWB7|HAWB8|HAWB9|HAWB10|HAWB1|HAWB2|HAWB3|HAWB4|HAWB5|HAWB6|HAWB7|HAWB8|HAWB9|HAWB10|HAWB1|HAWB2|HAWB3|HAWB4|HAWB5|HAWB6|HAWB7|HAWB8|HAWB9|HAWB10", "HAWB1|HAWB2|HAWB3|HAWB4|HAWB5|HAWB6|HAWB7|HAWB8|HAWB9|HAWB10");
//        }

//        [Fact]
//        public void TestUShipment2AIRAEDDefaultValues()
//        {
//            AssertMapping("Test5_input.xml", "Test5_output.xml");
//        }

//        [Fact]
//        public void TestUShipment2AIRAEDCustomsQty()
//        {
//            AssertMappingBasic("Test6_input.xml", "Test6_output.xml");
//            AssertMappingBasic("Test7_input.xml", "Test7_output.xml");
//            AssertMappingBasic("Test8_input.xml", "Test8_output.xml");
//            AssertMappingBasic("Test9_input.xml", "Test9_output.xml");
//        }

//        void AssertMapping(string inputFile, string expectedOutputFile)
//        {
//            var input = filePath + inputFile;
//            var expectedOutput = filePath + expectedOutputFile;

//            var mockIDateMapper = new Mock<IDateMapper>();
//            var mockICodeMapper = new Mock<ICodeMapper>();
//            var mockDataModelAccessor = new Mock<IDataModelAccessor>();
//            var mockIContextAccessor = new Mock<IContextAccessor>();
//            var mockSGCustomsDataModelAccessor = new Mock<ISGCustomsDataModelAccessor>();

//            mockIContextAccessor.Setup(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("PRET1.PRET001");
//            mockIContextAccessor.Setup(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("VWGT.VWGT001");
//            mockSGCustomsDataModelAccessor.Setup(x => x.GetSGCustomsAccount("BKG", "PRET1.PRET001")).Returns("VWGT001");
//            mockSGCustomsDataModelAccessor.Setup(x => x.GetSGCustomsSenderID("VWGT001")).Returns("VWGT.VWGT001");
//            mockIContextAccessor.Setup(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "VWGT001"));

//            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns("2017-08-23T11:30:01");
//            mockIDateMapper.Setup(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-08-23T11:30:01", "SGSIN")).Returns("2017-08-23T18:30:01");

//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT", "@maxlength", "4")).Returns("0001");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.Maps.AIRAED.CST", "@maxlength", "5")).Returns("00001");

//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "MAN0000092", "ManifestNumber")).Callback(() => { });
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "1", "UpdateNumber")).Callback(() => { });

//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "012-98765432", "MAWB")).Callback(() => { });
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "HAWB1", "HAWB")).Callback(() => { });

//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB1MAN000009200001E", "000012", "CNRF")).Callback(() => { });
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB1MAN0000092E", "201612334A|20170823|0001", "IDT")).Callback(() => { });
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB1MAN0000092E", "00001", "CST")).Callback(() => { });
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB1MAN000009200001E", "2", "CR")).Callback(() => { });


//            var extensionObjects = new Dictionary<string, object>() {
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object },
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object },
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockIContextAccessor.Object },
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor.Object },
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS5", mockSGCustomsDataModelAccessor.Object }
//            };

//            Assert.True(BizTalkXsltTester.Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);


//            mockIDateMapper.VerifyAll();
//            mockICodeMapper.VerifyAll();
//            mockIContextAccessor.VerifyAll();
//        }

//        void AssertMappingBasic(string inputFile, string expectedOutputFile)
//        {
//            var input = filePath + inputFile;
//            var expectedOutput = filePath + expectedOutputFile;

//            int CSTCount = 1;

//            var mockIDateMapper = new Mock<IDateMapper>();
//            var mockICodeMapper = new Mock<ICodeMapper>();
//            var mockDataModelAccessor = new Mock<IDataModelAccessor>();
//            var mockIContextAccessor = new Mock<IContextAccessor>();
//            var mockSGCustomsDataModelAccessor = new Mock<ISGCustomsDataModelAccessor>();

//            mockIContextAccessor.Setup(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("PRET1.PRET001");
//            mockIContextAccessor.Setup(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("VWGT.VWGT001");
//            mockSGCustomsDataModelAccessor.Setup(x => x.GetSGCustomsAccount("BKG", "PRET1.PRET001")).Returns("VWGT001");
//            mockSGCustomsDataModelAccessor.Setup(x => x.GetSGCustomsSenderID("VWGT001")).Returns("VWGT.VWGT001");
//            mockIContextAccessor.Setup(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "VWGT001"));

//            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns("2017-08-23T11:30:01");
//            mockIDateMapper.Setup(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-08-23T11:30:01", "SGSIN")).Returns("2017-08-23T18:30:01");

//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT", "@maxlength", "4")).Returns("0001");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.Maps.AIRAED.CST", "@maxlength", "5")).
//                Returns(CSTCount++.ToString());

//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "MAN0000092", "ManifestNumber")).Callback(() => { });
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "1", "UpdateNumber")).Callback(() => { });

//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "012-98765432", "MAWB")).Callback(() => { });
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "HAWB1|HAWB2", "HAWB")).Callback(() => { });

//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB1MAN000009200001E", "000012", "CNRF")).Callback(() => { });
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB1MAN0000092E", "201612334A|20170823|0001", "IDT")).Callback(() => { });
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB1MAN0000092E", "00001", "CST")).Callback(() => { });
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB1MAN000009200001E", "2", "CR")).Callback(() => { });

//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB2MAN000009200002E", "000011", "CNRF")).Callback(() => { });
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB2MAN0000092E", "201612334A|20170823|0001", "IDT")).Callback(() => { });
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB2MAN0000092E", "00002", "CST")).Callback(() => { });
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB2MAN000009200001E", "1", "CR")).Callback(() => { });

//            var extensionObjects = new Dictionary<string, object>() {
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object },
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object },
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockIContextAccessor.Object },
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor.Object },
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS5", mockSGCustomsDataModelAccessor.Object }
//            };

//            Assert.True(BizTalkXsltTester.Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);
//        }


//        void AssertMappingAndSubscriptionValues(string inputFile, string expectedOutputFile)
//        {
//            var input = filePath + inputFile;
//            var expectedOutput = filePath + expectedOutputFile;

//            int CSTCount = 1;

//            var mockIDateMapper = new Mock<IDateMapper>();
//            var mockICodeMapper = new Mock<ICodeMapper>();
//            var mockDataModelAccessor = new Mock<IDataModelAccessor>();
//            var mockIContextAccessor = new Mock<IContextAccessor>();
//            var mockSGCustomsDataModelAccessor = new Mock<ISGCustomsDataModelAccessor>();

//            mockIContextAccessor.Setup(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("PRET1.PRET001");
//            mockIContextAccessor.Setup(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("VWGT.VWGT001");
//            mockSGCustomsDataModelAccessor.Setup(x => x.GetSGCustomsAccount("BKG", "PRET1.PRET001")).Returns("VWGT001");
//            mockSGCustomsDataModelAccessor.Setup(x => x.GetSGCustomsSenderID("VWGT001")).Returns("VWGT.VWGT001");
//            mockIContextAccessor.Setup(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "VWGT001"));

//            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns("2017-08-23T11:30:01");
//            mockIDateMapper.Setup(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-08-23T11:30:01", "SGSIN")).Returns("2017-08-23T18:30:01");

//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT", "@maxlength", "4")).Returns("0001");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.Maps.AIRAED.CST", "@maxlength", "5")).
//                Returns(CSTCount++.ToString());

//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "MAN0000092", "ManifestNumber")).Callback(() => { });
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "1", "UpdateNumber")).Callback(() => { });

//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "012-98765432", "MAWB")).Callback(() => { });
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "HAWB1|HAWB2", "HAWB")).Callback(() => { });

//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB1MAN000009200001E",
//                "000011|000022|000033|000044|000055|000066|000077|000088|000099|0001010|0001111|0001212|0001313|0001414|0001515|0001616|0001717|0001818|0001919|0002020|0002121|0002222|0002323|0002424|0002525|0002626|0002727|0002828|0002929|0003030|0003131|0003232|0003333|0003434|0003535|0003636|0003737|0003838|0003939|0004040|0004141|0004242|0004343|0004444|0004545|0004646|0004747|0004848|0004949|0005050", "CNRF")).Callback(() => { });
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB1MAN0000092E", "201612334A|20170823|0001", "IDT")).Callback(() => { });
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB1MAN0000092E", "00001|00002", "CST")).Callback(() => { });

//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB1MAN000009200002E", "0005151|0005252|0005353|0005454|0005555|0005656|0005757|0005858|0005959|0006060|0006161|0006262|0006363|0006464|0006565|0006666", "CNRF")).Callback(() => { });
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB2MAN0000092E", "201612334A|20170823|0001", "IDT")).Callback(() => { });
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB2MAN0000092E", "00003", "CST")).Callback(() => { });

//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB2MAN000009200003E", "0000167|0000268|0000369|0000470|0000571", "CNRF")).Callback(() => { });

//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue(It.Is<string>(v => v == "SGCMSG"), It.Is<string>(v => v == "VWGT.VWGT001"), It.Is<string>(v => v == "PRET1.PRET001"), It.IsAny<string>(), It.IsAny<string>(), It.Is<string>(v => v == "CR"))).Callback(() => { });

//            var extensionObjects = new Dictionary<string, object>() {
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object },
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object },
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockIContextAccessor.Object },
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor.Object },
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS5", mockSGCustomsDataModelAccessor.Object }
//            };

//            Assert.True(BizTalkXsltTester.Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);
//        }

//        void AssertMappingMessageValues(string inputFile, string expectedOutputFile, string FirstHAWB, string SecondHAWB)
//        {
//            var input = filePath + inputFile;
//            var expectedOutput = filePath + expectedOutputFile;

//            int ArticleItemNumber = 1;
//            int CSTCount = 1;

//            var mockIDateMapper = new Mock<IDateMapper>();
//            var mockICodeMapper = new Mock<ICodeMapper>();
//            var mockDataModelAccessor = new Mock<IDataModelAccessor>();
//            var mockIContextAccessor = new Mock<IContextAccessor>();
//            var mockSGCustomsDataModelAccessor = new Mock<ISGCustomsDataModelAccessor>();

//            mockIContextAccessor.Setup(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("PRET1.PRET001");
//            mockIContextAccessor.Setup(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("VWGT.VWGT001");
//            mockSGCustomsDataModelAccessor.Setup(x => x.GetSGCustomsAccount("BKG", "PRET1.PRET001")).Returns("VWGT001");
//            mockSGCustomsDataModelAccessor.Setup(x => x.GetSGCustomsSenderID("VWGT001")).Returns("VWGT.VWGT001");
//            mockIContextAccessor.Setup(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "VWGT001"));

//            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns("2017-08-23T11:30:01");
//            mockIDateMapper.Setup(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-08-23T11:30:01", "SGSIN")).Returns("2017-08-23T18:30:01");

//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "MAN0000091", "ManifestNumber")).Callback(() => { });
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "1", "UpdateNumber")).Callback(() => { });
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001",
//                "201612334A201708230002E", "MAN0000091", "ManifestNumber")).Callback(() => { });
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230002E", "1", "UpdateNumber")).Callback(() => { });

//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "012-12334123", "MAWB")).Callback(() => { });
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", FirstHAWB, "HAWB")).Callback(() => { });

//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230002E", "012-12334123", "MAWB")).Callback(() => { });
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230002E", SecondHAWB, "HAWB")).Callback(() => { });

//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT", "@maxlength", "4")).
//                Returns(() =>
//                {
//                    return ArticleItemNumber++.ToString();
//                });

//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.Maps.AIRAED.CST", "@maxlength", "5")).
//                Returns(() =>
//                {
//                    return CSTCount++.ToString();
//                });

//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue(It.Is<string>(v => v == "SGCMSG"), It.Is<string>(v => v == "VWGT.VWGT001"), It.Is<string>(v => v == "PRET1.PRET001"), It.IsAny<string>(), It.IsAny<string>(), It.Is<string>(v => v == "CNRF"))).Callback(() => { });
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue(It.Is<string>(v => v == "SGCMSG"), It.Is<string>(v => v == "VWGT.VWGT001"), It.Is<string>(v => v == "PRET1.PRET001"), It.IsAny<string>(), It.IsAny<string>(), It.Is<string>(v => v == "IDT"))).Callback(() => { });
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue(It.Is<string>(v => v == "SGCMSG"), It.Is<string>(v => v == "VWGT.VWGT001"), It.Is<string>(v => v == "PRET1.PRET001"), It.IsAny<string>(), It.IsAny<string>(), It.Is<string>(v => v == "CST"))).Callback(() => { });
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue(It.Is<string>(v => v == "SGCMSG"), It.Is<string>(v => v == "VWGT.VWGT001"), It.Is<string>(v => v == "PRET1.PRET001"), It.IsAny<string>(), It.IsAny<string>(), It.Is<string>(v => v == "CR"))).Callback(() => { });

//            var extensionObjects = new Dictionary<string, object>() {
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockIContextAccessor.Object },
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor.Object },
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS5", mockSGCustomsDataModelAccessor.Object }
//            };

//            Assert.True(BizTalkXsltTester.Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);
//        }
//    }
//}