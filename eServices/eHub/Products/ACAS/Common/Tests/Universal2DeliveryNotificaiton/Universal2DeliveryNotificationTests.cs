using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Products.ACAS.Universal2DeliveryNotification;

namespace CargoWise.eHub.Products.ACAS.Common.Tests
{
    [TestClass]
    public class Universal2DeliveryNotificationTests
    {
        const string FilePath = "Universal2DeliveryNotificaiton.TestFiles.";

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void Universal2DeliveryNotification()
        {
            AssertMapping("Test1_Input.xml", "Test1_Output.xml");
            AssertMapping("Test2_Input.xml", "Test2_Output.xml");
        }

        void AssertMapping(string inputFile, string expectedOutputFile)
        {
            var input = FilePath + inputFile;
            var expectedOutput = FilePath + expectedOutputFile;
            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
            mockContextAccessor.Expect(x => x.GetContextProperty("ErrorCode", "http://cargowise.com/ehub/routing/2010/06")).Return("IRJ");
            mockContextAccessor.Expect(x => x.GetContextProperty("ErrorDescription", "http://cargowise.com/ehub/routing/2010/06")).Return("Department=WiseTechGlobal|Reason=You are not registered with this Carrier. Contact WTG to register.");
            mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("000");
            mockContextAccessor.Expect(x => x.GetContextProperty("MessageTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("111");

            var extensionObjects = new Dictionary<string, object>() 
            {
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor },
            };

            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            mapTester.Execute<UniversalInterchange2DeliveryNotification>(input, expectedOutput);

            mockDateMapper.VerifyAllExpectations();
            mockContextAccessor.VerifyAllExpectations();
        }
    }
}

