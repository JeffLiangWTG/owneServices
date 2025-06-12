//using System.Collections.Generic;
//using System.Reflection;
//using Xunit;
//using Hawking.UnitTest.Tools.Xslt;


//using Hawking.Xslt.ExtensionMocks.Interfaces;
//using Moq;
//using System;

//namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests
//{
//        public class UShipment2AIRAEU_Tests
//    {
//        const string filePath = "UShipment2AIRAEU.TestFiles.";

//         [Fact]
//        public void Test2AIRAEUSubscriptionNotExist()
//        {
//            bool isThrownException = false;
//            try
//            {
//                AssertMappingSubscriptionNotExist("Test1_input.xml", "Test1_output.xml");
//            }
//            catch (Exception ex)
//            {
//                isThrownException = true;
//                if (ex.Message != null && ex is ArgumentException)
//                {
//                    Assert.AreEqual("Cannot find matching subscription for '0813243534EXPHB1MAN0000017E'.", ex.Message.Trim());
//                }
//                else
//                {
//                    throw;
//                }   
//            }
//            Assert.IsTrue(isThrownException);
//        }


//        [Fact]
//        public void TestUShipment2AIRAEUAmendWith2OriginalIDT()
//        {
//            AssertMappingMultipleIDT("Test1_input.xml", "Test1_output.xml");
//        }

//        [Fact]
//        public void TestUShipment2AIRAEU66PackLineAmendSingleIDT()
//        {
//            AssertMappingMultipleOriginalCST("Test2_input.xml", "Test2_output.xml");
//        }

//        [Fact]
//        public void TestUShipment2AIRAEU66PackLine110HAWBSAmendSingleIDT()
//        {
//            AssertMappingMultipleHAWBOverflow("Test3_input.xml", "Test3_output.xml");
//        }

//        [Fact]
//        public void TestUShipment2AIRAEUCancel()
//        {
//            AssertMapping("Test4_input.xml", "Test4_output.xml");
//        }

//        [Fact]
//        public void TestUShipment2AIRAEUPartialDelete()
//        {
//            AssertMapping("Test5_input.xml", "Test5_output.xml");
//        }

//        [Fact]
//        public void TestUShipment2AIRAEUDefaultValues()
//        {
//            AssertMapping("Test6_input.xml", "Test6_output.xml");
//        }

//        [Fact]
//        public void TestUShipment2AIRAEUCustomsQty()
//        {
//            AssertMappingMultipleIDT("Test7_input.xml", "Test7_output.xml");
//            AssertMappingMultipleIDT("Test8_input.xml", "Test8_output.xml");
//            AssertMappingMultipleIDT("Test9_input.xml", "Test9_output.xml");
//            AssertMappingMultipleIDT("Test10_input.xml", "Test10_output.xml");
//        }

//	    [Fact]
//		public void TestSubshipments_AIRAEU_AreGroupedUnderSameIDT()
//	    {
//			const string inputFile = "Test11_input.xml";
//			const string expectedOutputFile = "Test11_output.xml";

//		    const string sender = "UPESINPRD";
//		    const string recipient = "SGCustoms";
//		    const string broker = "TKW";
//		    const string SGCustomsAccount = "UPS10P5";
//			const string originalIDTPiped = "198801949D|20180117|1975";

//		    var input = filePath + inputFile;
//		    var expectedOutput = filePath + expectedOutputFile;

//		    var mockIDateMapper= new Mock<IDateMapper>();
//		    var mockICodeMapper= new Mock<ICodeMapper>();
//		    var mockDataModelAccessor = new Mock<IDataModelAccessor>();
//		    var mockIContextAccessor = new Mock<IContextAccessor>();
//		    var mockSGCustomsDataModelAccessor = new Mock<SGCustomsDataModelAccessor>();

//			mockIContextAccessor.Setup(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns(sender);
//			mockIContextAccessor.Setup(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns(recipient);
//			mockSGCustomsDataModelAccessor.Setup(x => x.GetSGCustomsAccount(broker, sender)).Returns(SGCustomsAccount);
//			mockSGCustomsDataModelAccessor.Setup(x => x.GetSGCustomsSenderID(SGCustomsAccount)).Returns(string.Format("{0}.{1}", SGCustomsAccount.Substring(0, 4), SGCustomsAccount));
//			mockIContextAccessor.Setup(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", SGCustomsAccount));

//			mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustoms", "@recipientId", "UPESINPRD", "@ST_ID", "SGCMSG", "@value", "40692874003R1F008H4KNKMAN0003944E", "@referenceType", "IDT")).Returns(originalIDTPiped);
//			mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustoms", "@recipientId", "UPESINPRD", "@ST_ID", "SGCMSG", "@value", "40692874003R1F008GL3LWMAN0003944E", "@referenceType", "IDT")).Returns(originalIDTPiped);
//			mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustoms", "@recipientId", "UPESINPRD", "@ST_ID", "SGCMSG", "@value", "40692874003R1F008GL3LWMAN0003944E", "@referenceType", "CST")).Returns("26365");
//			mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustoms", "@recipientId", "UPESINPRD", "@ST_ID", "SGCMSG", "@value", "40692874003R1F008H4KNKMAN0003944E", "@referenceType", "CST")).Returns("26366");
//			mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "SGCustoms", "@recipientId", "UPESINPRD", "@ST_ID", "SGCMSG", "@value", "198801949D201801171975E", "@referenceType", "UpdateNumber")).Returns("1");

//			mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns("2017-08-23T11:30:01");
//		    mockIDateMapper.Setup(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-08-23T11:30:01", "SGSIN")).Returns("2017-08-23T18:30:01");
//		    mockICodeMapper.Setup(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT", "@maxlength", "4")).Returns("0001");

//			mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "SGCustoms", "UPESINPRD", "198801949D201708230001E", "198801949D201801171975", "OriginalIDT"));
//			mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "SGCustoms", "UPESINPRD", "198801949D201801171975E", "2", "UpdateNumber"));

//			mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "SGCustoms", "UPESINPRD", "198801949D201708230001E", "R1F008GL3LW|R1F008H4KNK", "HAWB"));
//			mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "SGCustoms", "UPESINPRD", "198801949D201708230001E", "26365|26366", "CST"));
//		    mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "SGCustoms", "UPESINPRD", "198801949D201708230001E", "40692874003", "MAWB"));
//		    mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "SGCustoms", "UPESINPRD", "198801949D201708230001E", "MAN0003944", "ManifestNumber"));

//		    var extensionObjects = new Dictionary<string, object>() {
//			    { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
//			    { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
//			    { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockIContextAccessor.Object },
//			    { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor.Object },
//			    { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS5", mockSGCustomsDataModelAccessor.Object }
//		    };

//		    Assert.True(BizTalkXsltTester.Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);

//	    }

//        void AssertMappingMultipleIDT(string inputFile, string expectedOutputFile)
//        {
//            var input = filePath + inputFile;
//            var expectedOutput = filePath + expectedOutputFile;

//            var mockIDateMapper= new Mock<IDateMapper>();
//            var mockICodeMapper= new Mock<ICodeMapper>();
//            var mockDataModelAccessor = new Mock<IDataModelAccessor>();
//            var mockIContextAccessor = new Mock<IContextAccessor>();
//            var mockSGCustomsDataModelAccessor = new Mock<SGCustomsDataModelAccessor>();

//            mockIContextAccessor.Setup(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("PRET1.PRET001");
//            mockIContextAccessor.Setup(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("VWGT.VWGT001");
//            mockSGCustomsDataModelAccessor.Setup(x => x.GetSGCustomsAccount("PWS", "PRET1.PRET001")).Returns("VWGT001");
//            mockSGCustomsDataModelAccessor.Setup(x => x.GetSGCustomsSenderID("VWGT001")).Returns("VWGT.VWGT001");
//            mockIContextAccessor.Setup(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "VWGT001"));


//            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns("2017-08-23T11:30:01");
//            mockIDateMapper.Setup(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-08-23T11:30:01", "SGSIN")).Returns("2017-08-23T18:30:01");

//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT", "@maxlength", "4")).Returns("0001");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "201612334A201708230001E", "@referenceType", "UpdateNumber")).Returns("1");
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W201708230001E", "201612334A201708230001", "OriginalIDT"));

//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT", "@maxlength", "4")).Returns("0002");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "201612334A201708230002E", "@referenceType", "UpdateNumber")).Returns("2");
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W201708230002E", "201612334A201708230002", "OriginalIDT"));

//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB1MAN0000017E", "@referenceType", "IDT")).Returns("201612334A|20170823|0001");
//			mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB1MAN0000017E", "@referenceType", "CST")).Returns("00038");
//			mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "0813243534EXPHB1MAN000001700001E", "1", "CR"));
//			mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "0813243534EXPHB1MAN000001700038E", "000011", "CNRF"));

//			mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB2MAN0000017E", "@referenceType", "IDT")).Returns("201612334A|20170823|0001");
//			mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB2MAN0000017E", "@referenceType", "CST")).Returns("00056");
//			mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "0813243534EXPHB2MAN000001700001E", "1", "CR"));
//			mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "0813243534EXPHB2MAN000001700056E", "000011", "CNRF"));

//			mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB3MAN0000017E", "@referenceType", "IDT")).Returns("201612334A|20170823|0001");
//			mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB3MAN0000017E", "@referenceType", "CST")).Returns("00074");
//			mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "0813243534EXPHB3MAN000001700001E", "1", "CR"));
//			mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "0813243534EXPHB3MAN000001700074E", "000011", "CNRF"));

//			mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB4MAN0000017E", "@referenceType", "IDT")).Returns("201612334A|20170823|0002");
//			mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB4MAN0000017E", "@referenceType", "CST")).Returns("00087");
//			mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "0813243534EXPHB4MAN000001700001E", "1", "CR"));
//			mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "0813243534EXPHB4MAN000001700087E", "000011", "CNRF"));

//			mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB5MAN0000017E", "@referenceType", "IDT")).Returns("201612334A|20170823|0002");
//			mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB5MAN0000017E", "@referenceType", "CST")).Returns("00099");
//			mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "0813243534EXPHB5MAN000001700001E", "1", "CR"));
//			mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "0813243534EXPHB5MAN000001700099E", "000011", "CNRF"));

//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "2", "UpdateNumber"));
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230002E", "3", "UpdateNumber"));
//			mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W201708230001E", "EXPHB1|EXPHB2|EXPHB3", "HAWB"));
//			mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W201708230002E", "EXPHB4|EXPHB5", "HAWB"));

//	        mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W201708230001E", "0813243534", "MAWB"));
//	        mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W201708230001E", "MAN0000017", "ManifestNumber"));
//	        mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W201708230002E", "0813243534", "MAWB"));
//	        mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W201708230002E", "MAN0000017", "ManifestNumber"));

//            var extensionObjects = new Dictionary<string, object>() {
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockIContextAccessor.Object },
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor.Object },
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS5", mockSGCustomsDataModelAccessor.Object }
//			};

//            Assert.True(BizTalkXsltTester.Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);


//            mockIDateMapper.VerifyAll();
//            mockICodeMapper.VerifyAll();
//            mockIContextAccessor.VerifyAll();
//        }

//        void AssertMappingSubscriptionNotExist(string inputFile, string expectedOutputFile)
//        {
//            var input = filePath + inputFile;
//            var expectedOutput = filePath + expectedOutputFile;

//            var mockIDateMapper= new Mock<IDateMapper>();
//            var mockICodeMapper= new Mock<ICodeMapper>();
//            var mockDataModelAccessor = new Mock<IDataModelAccessor>();
//            var mockIContextAccessor = new Mock<IContextAccessor>();
//            var mockSGCustomsDataModelAccessor = new Mock<SGCustomsDataModelAccessor>();

//            mockIContextAccessor.Setup(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("PRET1.PRET001");
//            mockIContextAccessor.Setup(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("VWGT.VWGT001");
//            mockSGCustomsDataModelAccessor.Setup(x => x.GetSGCustomsAccount("PWS", "PRET1.PRET001")).Returns("VWGT001");
//            mockSGCustomsDataModelAccessor.Setup(x => x.GetSGCustomsSenderID("VWGT001")).Returns("VWGT.VWGT001");
//            mockIContextAccessor.Setup(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "VWGT001"));


//            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns("2017-08-23T11:30:01");
//            mockIDateMapper.Setup(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-08-23T11:30:01", "SGSIN")).Returns("2017-08-23T18:30:01");

//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT", "@maxlength", "4")).Returns("0001");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "201612334A201708230001E", "@referenceType", "UpdateNumber")).Returns("1");
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W201708230001E", "201612334A201708230001", "OriginalIDT"));

//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB1MAN0000017E", "@referenceType", "IDT")).Returns("");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB1MAN0000017E", "@referenceType", "CST")).Returns("");
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "0813243534EXPHB100001E", "1", "CR"));
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "0813243534EXPHB100038E", "000011", "CNRF"));


//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "2", "UpdateNumber"));
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230002E", "3", "UpdateNumber"));
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W201708230001E", "EXPHB1|EXPHB2|EXPHB3", "HAWB"));
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W201708230002E", "EXPHB4|EXPHB5", "HAWB"));

//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W201708230001E", "0813243534", "MAWB"));
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W201708230001E", "MAN0000017", "ManifestNumber"));
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W201708230002E", "0813243534", "MAWB"));
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W201708230002E", "MAN0000017", "ManifestNumber"));

//            var extensionObjects = new Dictionary<string, object>() {
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockIContextAccessor.Object },
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor.Object },
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS5", mockSGCustomsDataModelAccessor.Object }
//			};

//            Assert.True(BizTalkXsltTester.Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);


//            mockIDateMapper.VerifyAll();
//            mockICodeMapper.VerifyAll();
//            mockIContextAccessor.VerifyAll();
//        }

//        void AssertMappingMultipleOriginalCST(string inputFile, string expectedOutputFile)
//        {
//            var input = filePath + inputFile;
//            var expectedOutput = filePath + expectedOutputFile;

//            var mockIDateMapper= new Mock<IDateMapper>();
//            var mockICodeMapper= new Mock<ICodeMapper>();
//            var mockDataModelAccessor = new Mock<IDataModelAccessor>();
//            var mockIContextAccessor = new Mock<IContextAccessor>();
//            var mockSGCustomsDataModelAccessor = new Mock<SGCustomsDataModelAccessor>();

//            mockIContextAccessor.Setup(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("PRET1.PRET001");
//            mockIContextAccessor.Setup(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("VWGT.VWGT001");
//            mockSGCustomsDataModelAccessor.Setup(x => x.GetSGCustomsAccount("BKG", "PRET1.PRET001")).Returns("VWGT001");
//            mockSGCustomsDataModelAccessor.Setup(x => x.GetSGCustomsSenderID("VWGT001")).Returns("VWGT.VWGT001");
//            mockIContextAccessor.Setup(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "VWGT001"));


//            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns("2017-08-23T11:30:01");
//            mockIDateMapper.Setup(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-08-23T11:30:01", "SGSIN")).Returns("2017-08-23T18:30:01");
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "201612334A201708230001", "OriginalIDT"));

//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT", "@maxlength", "4")).Returns("0001");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "201612334A201708230001E", "@referenceType", "UpdateNumber")).Returns("1");

//			mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "012-98765432HAWB1MAN0000092E", "@referenceType", "IDT")).Returns("201612334A|20170823|0001");
//			mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "012-98765432HAWB1MAN0000092E", "@referenceType", "CST")).Returns("00038|00039");
//			mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB1MAN000009200038E",
//                "000011|000022|000033|000044|000055|000066|000077|000088|000099|0001010|0001111|0001212|0001313|0001414|0001515|0001616|0001717|0001818|0001919|0002020|0002121|0002222|0002323|0002424|0002525|0002626|0002727|0002828|0002929|0003030|0003131|0003232|0003333|0003434|0003535|0003636|0003737|0003838|0003939|0004040|0004141|0004242|0004343|0004444|0004545|0004646|0004747|0004848|0004949|0005050", "CNRF"));
//			mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB1MAN000009200039E", "0000151|0000252|0000353|0000454|0000555|0000656|0000757|0000858|0000959|0001060|0001161|0001262|0001363|0001464|0001565|0001666", "CNRF"));
	        
//			mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "012-98765432HAWB2MAN0000092E", "@referenceType", "IDT")).Returns("201612334A|20170823|0001");
//			mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "012-98765432HAWB2MAN0000092E", "@referenceType", "CST")).Returns("00056");
//			mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB2MAN000009200056E", "0000167|0000268|0000369|0000470|0000571", "CNRF"));

//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue(It.Is<string>(v => v == "SGCMSG"), Arg<string>.Is.Equal("VWGT.VWGT001"), Arg<string>.Is.Equal("PRET1.PRET001"), It.IsAny<string>(), It.IsAny<string>(), Arg<string>.Is.Equal("CR")));
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "2", "UpdateNumber"));
//			mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "HAWB1|HAWB2", "HAWB"));
//	        mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "012-98765432HAWB1MAN000009200039E", "0005151|0005252|0005353|0005454|0005555|0005656|0005757|0005858|0005959|0006060|0006161|0006262|0006363|0006464|0006565|0006666", "CNRF"));
//	        mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "045-99736483", "MAWB"));
//	        mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "MAN0000092", "ManifestNumber"));

//            var extensionObjects = new Dictionary<string, object>() {
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockIContextAccessor.Object },
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor.Object },
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS5", mockSGCustomsDataModelAccessor.Object }
//			};

//            Assert.True(BizTalkXsltTester.Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);


//            mockIDateMapper.VerifyAll();
//            mockICodeMapper.VerifyAll();
//            mockIContextAccessor.VerifyAll();
//        }

//        void AssertMappingMultipleHAWBOverflow(string inputFile, string expectedOutputFile)
//        {
//            var input = filePath + inputFile;
//            var expectedOutput = filePath + expectedOutputFile;

//            var mockIDateMapper= new Mock<IDateMapper>();
//            var mockICodeMapper= new Mock<ICodeMapper>();
//            var mockDataModelAccessor = new Mock<IDataModelAccessor>();
//            var mockIContextAccessor = new Mock<IContextAccessor>();
//            var mockSGCustomsDataModelAccessor = new Mock<SGCustomsDataModelAccessor>();
            
//            int ArticleItemNumber = 1;
//            int SubscribedCSTCounter = 1;

//            //mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue(It.Is<string>(v => v == "SGCMSG"), Arg<string>.Is.Equal("VWGT.VWGT001"), Arg<string>.Is.Equal("PRET1.PRET001"), It.IsAny<string>(), It.IsAny<string>(), Arg<string>.Is.Equal("CNRF")));

//            mockIContextAccessor.Setup(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("PRET1.PRET001");
//            mockIContextAccessor.Setup(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("VWGT.VWGT001");
//            mockSGCustomsDataModelAccessor.Setup(x => x.GetSGCustomsAccount("BKG", "PRET1.PRET001")).Returns("VWGT001");
//            mockSGCustomsDataModelAccessor.Setup(x => x.GetSGCustomsSenderID("VWGT001")).Returns("VWGT.VWGT001");
//            mockIContextAccessor.Setup(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "VWGT001"));

//            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns("2017-08-23T11:30:01");
//            mockIDateMapper.Setup(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-08-23T11:30:01", "SGSIN")).Returns("2017-08-23T18:30:01");

//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT", "@maxlength", "4")).
//                Return(ArticleItemNumber.ToString());
//                WhenCalled(x => x.ReturnValue = ArticleItemNumber++.ToString());

//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "201612334A201708230001E", "@referenceType", "UpdateNumber")).Returns("1");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "201612334A201708230001E", "@referenceType", "UpdateNumber")).Returns("2");
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "201612334A201708230001", "OriginalIDT"));
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230002E", "201612334A201708230001", "OriginalIDT"));
//	        mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "012-12334123", "MAWB"));
//	        mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230002E", "012-12334123", "MAWB"));
//	        mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "MAN0000091", "ManifestNumber"));
//	        mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230002E", "MAN0000091", "ManifestNumber"));

//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper(It.Is<string>(v => v == "SelectSubscribedReference"),
//                It.Is<string>(v => v == "@reference"),
//                It.Is<string>(v => v == "@senderId"),
//                It.Is<string>(v => v == "VWGT.VWGT001"),
//                It.Is<string>(v => v == "@recipientId"),
//                It.Is<string>(v => v == "PRET1.PRET001"),
//                It.Is<string>(v => v == "@ST_ID"),
//                It.Is<string>(v => v == "SGCMSG"),
//                It.Is<string>(v => v == "@value"),
//                It.IsAny<string>(),
//                It.Is<string>(v => v == "@referenceType"),
//                It.Is<string>(v => v == "IDT"))).Returns("201612334A|20170823|0001");

//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper(It.Is<string>(v => v == "SelectSubscribedReference"),
//                It.Is<string>(v => v == "@reference"),
//                It.Is<string>(v => v == "@senderId"),
//                It.Is<string>(v => v == "VWGT.VWGT001"),
//                It.Is<string>(v => v == "@recipientId"),
//                It.Is<string>(v => v == "PRET1.PRET001"),
//                It.Is<string>(v => v == "@ST_ID"),
//                It.Is<string>(v => v == "SGCMSG"),
//                It.Is<string>(v => v == "@value"),
//                It.IsAny<string>(),
//                It.Is<string>(v => v == "@referenceType"),
//                It.Is<string>(v => v == "CST"))).Returns("00001");
//                {
//                    if (SubscribedCSTCounter % 10 == 0)
//                    {
//                        x.ReturnValue = "00001|00002";
//                    }
//                    SubscribedCSTCounter++;
//                });

//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "2", "UpdateNumber"));
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "3", "UpdateNumber"));
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue(It.Is<string>(v => v == "SGCMSG"), Arg<string>.Is.Equal("VWGT.VWGT001"), Arg<string>.Is.Equal("PRET1.PRET001"), It.IsAny<string>(), It.IsAny<string>(), Arg<string>.Is.Equal("CNRF")));
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue(It.Is<string>(v => v == "SGCMSG"), Arg<string>.Is.Equal("VWGT.VWGT001"), Arg<string>.Is.Equal("PRET1.PRET001"), It.IsAny<string>(), It.IsAny<string>(), Arg<string>.Is.Equal("CR")));

//			mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue(It.Is<string>(v => v == "SGCMSG"), Arg<string>.Is.Equal("VWGT.VWGT001"), Arg<string>.Is.Equal("PRET1.PRET001"), Arg<string>.Is.Equal("201612334A201708230001E"), It.IsAny<string>(), Arg<string>.Is.Equal("HAWB")));
//			mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue(It.Is<string>(v => v == "SGCMSG"), Arg<string>.Is.Equal("VWGT.VWGT001"), Arg<string>.Is.Equal("PRET1.PRET001"), Arg<string>.Is.Equal("201612334A201708230002E"), It.IsAny<string>(), Arg<string>.Is.Equal("HAWB")));

//            var extensionObjects = new Dictionary<string, object>() {
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockIContextAccessor.Object },
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor.Object },
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS5", mockSGCustomsDataModelAccessor.Object }
//			};

//            Assert.True(BizTalkXsltTester.Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);


//            mockIDateMapper.VerifyAll();
//            mockICodeMapper.VerifyAll();
//            mockIContextAccessor.VerifyAll();
//        }

//        void AssertMapping(string inputFile, string expectedOutputFile)
//        {
//            var input = filePath + inputFile;
//            var expectedOutput = filePath + expectedOutputFile;

//            var mockIDateMapper= new Mock<IDateMapper>();
//            var mockICodeMapper= new Mock<ICodeMapper>();
//            var mockDataModelAccessor = new Mock<IDataModelAccessor>();
//            var mockIContextAccessor = new Mock<IContextAccessor>();
//            var mockSGCustomsDataModelAccessor = new Mock<SGCustomsDataModelAccessor>();

//            mockIContextAccessor.Setup(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("PRET1.PRET001");
//            mockIContextAccessor.Setup(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Returns("VWGT.VWGT001");
//            mockSGCustomsDataModelAccessor.Setup(x => x.GetSGCustomsAccount("PWS", "PRET1.PRET001")).Returns("VWGT001");
//            mockSGCustomsDataModelAccessor.Setup(x => x.GetSGCustomsSenderID("VWGT001")).Returns("VWGT.VWGT001");
//            mockIContextAccessor.Setup(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "VWGT001"));


//            mockIDateMapper.Setup(x => x.CurrentDateTimeUTC("s")).Returns("2017-08-23T11:30:01");
//            mockIDateMapper.Setup(x => x.ConvertUTCToLocalTimeByUNLOCO("2017-08-23T11:30:01", "SGSIN")).Returns("2017-08-23T18:30:01");

//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.IDT", "@maxlength", "4")).Returns("0001");
//			mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "201612334A201708230001E", "@referenceType", "UpdateNumber")).Returns("1");
//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W201708230001E", "201612334A201708230001", "OriginalIDT"));

//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB1MAN0000017E", "@referenceType", "IDT")).Returns("201612334A|20170823|0001");
//            mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "0813243534EXPHB1MAN0000017E", "@referenceType", "CST")).Returns("00038");
//			mockICodeMapper.Setup(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "VWGT.VWGT001", "@recipientId", "PRET1.PRET001", "@ST_ID", "SGCMSG", "@value", "012-98765432HAWB1MAN0000092E", "@referenceType", "IDT")).Returns("201612334A|20170823|0001");

//			mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "0813243534EXPHB1MAN000001700001E", "1", "CR"));
//			mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "0813243534EXPHB1MAN000001700038E", "000011", "CNRF"));

//            mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "201612334A201708230001E", "2", "UpdateNumber"));
//			mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W201708230001E", "EXPHB1", "HAWB"));
//	        mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W201708230001E", "0813243534", "MAWB"));
//	        mockDataModelAccessor.Setup(x => x.InsertSubscriptionValue("SGCMSG", "VWGT.VWGT001", "PRET1.PRET001", "199702247W201708230001E", "MAN0000017", "ManifestNumber"));

//            var extensionObjects = new Dictionary<string, object>() {
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockICodeMapper.Object},
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockIDateMapper.Object},
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockIContextAccessor.Object },
//				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor.Object },
//                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS5", mockSGCustomsDataModelAccessor.Object }
//			};

//            Assert.True(BizTalkXsltTester.Execute(Assembly.GetExecutingAssembly(), xsl, input, expectedOutput, extensionObjects).Success);


//            mockIDateMapper.VerifyAll();
//            mockICodeMapper.VerifyAll();
//            mockIContextAccessor.VerifyAll();
//        }


//    }
//}
