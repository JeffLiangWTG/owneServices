using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using Rhino.Mocks;
using CargoWise.eHub.Products.ACAS.DeliveryNotification2UEvent;

namespace CargoWise.eHub.Products.ACAS.Common.Tests
{
    [TestClass]
    public class DeliveryNotification2UEventTests
    {
        const string FilePath = "DeliveryNotification2UEvent.TestFiles.";

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void DeliveryNotification2UEventTests_Test()
        {
            AssertMapping("Test1_Input.xml", "Test1_Output.xml");
            AssertMapping("Test2_Input.xml", "Test2_Output.xml");
            AssertMapping("Test3_Input.xml", "Test3_Output.xml");
            AssertMapping("Test4_Input.xml", "Test4_Output.xml");
            AssertMapping("Test5_Input.xml", "Test5_Output.xml");
        }

        private void AssertMapping(string inputFile, string expectedOutputFile)
        {
            var input = FilePath + inputFile;
            var expectedOutput = FilePath + expectedOutputFile;
            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
            var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("EDIEDIDAT");

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "ACAS_US", "@recipientId", "EDIEDIDAT", "@ST_ID", "ACASUS", "@value", "9e238311-2885-4cd5-82da-150af33e65a7")).Return("88").Repeat.Any();
            mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("s"))).Return("2016-03-27T09:30:10");

            var extensionObjects = new Dictionary<string, object>() 
            {
                { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper }
            };
            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            mapTester.Execute<DeliveryNotification2UEvent.DeliveryNotification2UEvent>(input, expectedOutput);

            mockDateMapper.VerifyAllExpectations();
            mockContextAccessor.VerifyAllExpectations();
            mockCodeMapper.VerifyAllExpectations();
        }
    }
}
