using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.Products.GlobalInvoice.Common.Transforms.GEI2DNotification;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GlobalInvoice.Common.Tests
{
    [TestClass]
    public class GlobalInvoice2DeliveryNotificationTests
    {
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void GlobalInvoice2DeliveryNotificationTest()
        {
            var sourceFile = "GlobalInvoice2DeliveryNotification.TestFiles.Input_1.xml";
            var expectedFile = "GlobalInvoice2DeliveryNotification.TestFiles.Output_1.xml";

            var extensionObjects = InitialiseCodeMapsTestingContext();

            var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            mapTester.ExecuteCompiled<GEI2DNotification>(sourceFile, expectedFile);
        }

        static Dictionary<string, object> InitialiseCodeMapsTestingContext()
        {
            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

            mockContextAccessor.Stub(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("E881011F-BDCF-403C-97ED-6C0201E03084");
            mockContextAccessor.Stub(x => x.GetContextProperty("MessageTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("8139A96B-E059-4633-9925-6F4E070CF335");
            mockContextAccessor.Stub(x => x.GetContextProperty("ErrorCode", "http://cargowise.com/ehub/routing/2010/06")).Return("NACK");
            mockContextAccessor.Stub(x => x.GetContextProperty("ErrorDescription", "http://cargowise.com/ehub/routing/2010/06")).Return("Test Error Description");

            var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor }
			};

            return extensionObjects;
        }
    }
}