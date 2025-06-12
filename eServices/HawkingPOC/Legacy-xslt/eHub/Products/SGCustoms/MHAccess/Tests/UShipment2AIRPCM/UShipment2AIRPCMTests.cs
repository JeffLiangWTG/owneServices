//using System.Collections.Generic;
//using System.Reflection;
//using Xunit;
//using Hawking.UnitTest.Tools.Xslt;


//using Hawking.Xslt.ExtensionMocks.Interfaces;
//using Moq;
//using System.IO;

//namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests
//{
//        public class UShipment2AIRPCMTests
//    {
//        const string filePath = "UShipment2AIRPCM.TestFiles.";

//        #region Old Tests still pass, but I'll remove them once we switch to new subscription system, the new tests will cover all scenarios

//        [Fact]
//        public void TestUShipment2AIRPCM110HAWBWith11HavingMoreThan50PackLinesAndEndingOnOverCST()
//        {
//            AssertMapping("Test1_input.xml", "Test1_output.xml");

//        }

//        [Fact]
//        public void TestUShipment2AIRPCM66PackLinesAnd2HAWB()
//        {
//            AssertMapping("Test2_input.xml", "Test2_output.xml");

//        }

//        [Fact]
//        public void TestUShipment2AIRPCMBasic()
//        {
//            AssertMapping("Test3_input.xml", "Test3_output.xml");

//        }

//        [Fact]
//        public void TestUShipment2AIRPCM110HAWBWithLessThan50PackLines()
//        {
//            AssertMapping("Test4_input.xml", "Test4_output.xml");

//        }

//        [Fact]
//        public void TestUShipment2AIRPCMMEDAddInfo()
//        {
//            AssertMapping("Test5_input.xml", "Test5_output.xml");
//            AssertMapping("Test6_input.xml", "Test6_output.xml");
//            AssertMapping("Test7_input.xml", "Test7_output.xml");
//            AssertMapping("Test8_input.xml", "Test8_output.xml");
//        }

//        private void AssertMapping(string inputFile, string expectedOutputFile)
//        {
//            var input = filePath + inputFile;
//            var expectedOutput = filePath + expectedOutputFile;

//            var IDTCounterStartingValue = 1;
//            var CSTCounterStartingValue = 1;

//            var mockIDateMapper= new Mock<IDateMapper>();
//            var mockICodeMapper= new Mock<ICodeMapper>();
//            var mockDataModelAccessor = new Mock<IDataModelAccessor>();
//            var mockIContextAccessor = new Mock<IContextAccessor>();
//            var mockSGCustomsDataModelAccessor = new Mock<SGCustomsDataModelAccessor>();

//            mockICodeMapper.Setup(x => x.GetRecipientCode("SGCustoms", "SGCustoms", "SGCustoms Configuration", "Subscription", "Config Value", "Support Backward Compatibility")).Returns("true");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustomsTest", "@recipientId", "UPEUPE001", "@ST_ID", "SGCMSG", "@value", "MAN0000091I")).Returns("");
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue(
//                It.Is<string>(v => v == "SGCMSG"),
//                It.Is<string>(v => v == "SGCustomsTest"),
//                It.Is<string>(v => v == "UPEUPE001"),
//                It.Is<string>(v => v == "MAN0000091I"),
//                It.IsAny<string>()));
//            mockIContextAccessor.Setup(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("SGCustomsTest");
//            mockIContextAccessor.Setup(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("UPEUPE001");
//            mockDataModelAccessor.Setup(x => x.GetClientRegistrationCode("UPEUPE001", "", "SGCustoms")).Returns("UPE1.UPE001");
//            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns("2017-09-06T12:00");
//            mockIDateMapper.Setup(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-09-06T12:00", "SGSIN")).Returns("2017-09-06T04:00");
//            mockSGCustomsDataModelAccessor.Setup(x => x.GetSGCustomsAccount("BKG", "UPEUPE001")).Returns("vwgt001");
//            mockSGCustomsDataModelAccessor.Setup(x => x.GetSGCustomsSenderID("vwgt001")).Returns("vwgt.vwgt001");
//            mockIContextAccessor.Setup(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "vwgt001"));
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT", "@maxlength", "4"))
//                .Return(IDTCounterStartingValue.ToString())
//                ;
//                .WhenCalled(x => x.ReturnValue = IDTCounterStartingValue++.ToString());
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "201612334A201709060001", "MAN0000091", "ManifestNumber"));
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper(
//                It.Is<string>(v => v == "SelectSubscribedReference"), 
//                It.Is<string>(v => v == "@reference"), 
//                It.Is<string>(v => v == "@senderId"), 
//                It.Is<string>(v => v == "SGCustomsTest"), 
//                It.Is<string>(v => v == "@recipientId"), 
//                It.Is<string>(v => v == "UPEUPE001"), 
//                It.Is<string>(v => v == "@ST_ID"), 
//                It.Is<string>(v => v == "SGCMSG"), 
//                It.Is<string>(v => v == "@value"), 
//                It.IsAny<string>(), 
//                It.Is<string>(v => v == "@referenceType"), 
//                It.Is<string>(v => v == "CST"))).Returns("");
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue(
//                It.Is<string>(v => v == "SGCMSG"),
//                It.Is<string>(v => v == "SGCustomsTest"),
//                It.Is<string>(v => v == "UPEUPE001"),
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<string>()));
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.Maps.AIRPCM.CST", "@maxlength", "5"))
//                .Return(CSTCounterStartingValue.ToString())
//                ;
//                .WhenCalled(x => x.ReturnValue = CSTCounterStartingValue++.ToString());

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

//        #endregion

//        #region Tests for new subscription system

//        [Fact]
//        public void Test_UShipment2AIRPCM_EdgeCasesWithoutBackwardCompatibility()
//        {
//            AssertMapping("Test9_input.xml", "Test9_output.xml", "Test9_subscription_Input.xml", "Test9_subscription_Output.xml", "false");
//        }

//        [Fact]
//        public void Test_UShipment2AIRPCM_SimpleWithoutBackwardCompatibility()
//        {
//            AssertMapping("Test10_input.xml", "Test10_output.xml", "Test10_subscription_Input.xml", "Test10_subscription_Output.xml", "false");
//        }

//        private void AssertMapping(string inputFile, string expectedOutputFile, string inputSubscriptionFile, string outputSubscriptionFile, string SupportBackwardCompatibility)
//        {
//            var input = filePath + inputFile;
//            var expectedOutput = filePath + expectedOutputFile;
//            var subscriptionInput = ReadResource(filePath + inputSubscriptionFile);
//            var subscriptionOutput = ReadResource(filePath + outputSubscriptionFile);

//            var IDTCounterStartingValue = 1;
//            var CSTCounterStartingValue = 1;

//            var mockIDateMapper= new Mock<IDateMapper>();
//            var mockICodeMapper= new Mock<ICodeMapper>();
//            var mockDataModelAccessor = new Mock<IDataModelAccessor>();
//            var mockIContextAccessor = new Mock<IContextAccessor>();
//            var mockSGCustomsDataModelAccessor = new Mock<SGCustomsDataModelAccessor>();
//            var mockSGCustomsSubscriptionHelper = new SGCustomsSubscriptionHelper();

//            mockICodeMapper.Setup(x => x.GetRecipientCode("SGCustoms", "SGCustoms", "SGCustoms Configuration", "Subscription", "Config Value", "Support Backward Compatibility")).Returns(SupportBackwardCompatibility);
//            if (SupportBackwardCompatibility == "true")
//            {
//                mockICodeMapper.Setup(x => x.CallActionProcedureHelper(
//                    It.Is<string>(v => v == "SelectSubscribedReference"),
//                    It.Is<string>(v => v == "@reference"),
//                    It.Is<string>(v => v == "@senderId"),
//                    It.Is<string>(v => v == "SGCustomsTest"),
//                    It.Is<string>(v => v == "@recipientId"),
//                    It.Is<string>(v => v == "UPEUPE001"),
//                    It.Is<string>(v => v == "@ST_ID"),
//                    It.Is<string>(v => v == "SGCMSG"),
//                    It.Is<string>(v => v == "@value"),
//                    It.IsAny<string>(),
//                    It.Is<string>(v => v == "@referenceType"),
//                    It.Is<string>(v => v == "CST"))).Returns("");
//            }
//            else
//            {
//                mockICodeMapper.Setup(x => x.CallActionProcedureHelper(
//                    It.Is<string>(v => v == "SelectSubscribedReference"),
//                    It.Is<string>(v => v == "@reference"),
//                    It.Is<string>(v => v == "@senderId"),
//                    It.Is<string>(v => v == "SGCustomsTest"),
//                    It.Is<string>(v => v == "@recipientId"),
//                    It.Is<string>(v => v == "UPEUPE001"),
//                    It.Is<string>(v => v == "@ST_ID"),
//                    It.Is<string>(v => v == "SGCMSG"),
//                    It.Is<string>(v => v == "@value"),
//                    It.IsAny<string>())).Returns(subscriptionInput);
//                mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "SGCustomsTest", "UPEUPE001", "MAN0000030I", subscriptionOutput));
//            }
//            mockIContextAccessor.Setup(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("SGCustomsTest");
//            mockIContextAccessor.Setup(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("UPEUPE001");
//            mockDataModelAccessor.Setup(x => x.GetClientRegistrationCode("UPEUPE001", "", "SGCustoms")).Returns("UPE1.UPE001");
//            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns("2017-09-06T12:00");
//            mockIDateMapper.Setup(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-09-06T12:00", "SGSIN")).Returns("2017-09-06T04:00");
//            mockSGCustomsDataModelAccessor.Setup(x => x.GetSGCustomsAccount("SW", "UPEUPE001")).Returns("vwgt001");
//            mockSGCustomsDataModelAccessor.Setup(x => x.GetSGCustomsSenderID("vwgt001")).Returns("vwgt.vwgt001");
//            mockIContextAccessor.Setup(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "vwgt001"));
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT", "@maxlength", "4"))
//                .Return(IDTCounterStartingValue.ToString())
//                ;
//                .WhenCalled(x => x.ReturnValue = IDTCounterStartingValue++.ToString());

            
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue(
//                It.Is<string>(v => v == "SGCMSG"),
//                It.Is<string>(v => v == "SGCustomsTest"),
//                It.Is<string>(v => v == "UPEUPE001"),
//                It.IsAny<string>(),
//                It.Is<string>(v => v == "MAN0000030"),
//                It.Is<string>(v => v == "ManifestNumber")));
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.Maps.AIRPCM.CST", "@maxlength", "5"))
//                .Return(CSTCounterStartingValue.ToString())
//                ;
//                .WhenCalled(x => x.ReturnValue = CSTCounterStartingValue++.ToString());

//            var extensionObjects = new Dictionary<string, object>()
//			{
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockIContextAccessor.Object },
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor.Object },
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS5", mockSGCustomsDataModelAccessor.Object },
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS6", mockSGCustomsSubscriptionHelper }
//			};

//            Assert.True(BizTalkXsltTester.Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);

//        }

//        public static string ReadResource(string resourceName)
//        {
//            var assembly = Assembly.GetExecutingAssembly();

//            using (Stream stream = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + resourceName))
//            using (StreamReader reader = new StreamReader(stream))
//            {
//                return reader.ReadToEnd();
//            }
//        }

//        #endregion
//    }
//}
