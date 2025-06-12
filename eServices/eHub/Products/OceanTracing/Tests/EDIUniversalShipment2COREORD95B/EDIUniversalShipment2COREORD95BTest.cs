using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanTracing.Transforms.EDIUniversalShipment2COREORD95B;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.EDIFACT.Tests
{
	[TestClass]
	public class EDIUniversalShipment2COREORD95BTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIUniversalShipment2COREORD95B()
		{
			const string sourceFile = "EDIUniversalShipment2COREORD95B.TestFiles.Universal_Order.xml";
			const string expectedFile = "EDIUniversalShipment2COREORD95B.TestFiles.COREORD95B_Order.xml";
			TestMapping(sourceFile, expectedFile, "MRN123");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIUniversalShipment2COREORD95B_OperationalPort_Code()
		{
			const string sourceFile = "EDIUniversalShipment2COREORD95B.TestFiles.Universal_Order_OperationalPort_Code.xml";
			const string expectedFile = "EDIUniversalShipment2COREORD95B.TestFiles.COREORD95B_Order_OperationalPort_Code.xml";
			TestMapping(sourceFile, expectedFile, "MRN123");
		}

		void TestMapping(string sourceFile, string expectedFile, params string[] nums)
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("HYEDNZIKB");
			mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "ShippingPort_POAL_COREOR"));

			mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyName", "http://schemas.microsoft.com/Edi/PropertySchema", "ShippingPort_POAL_COREOR"));
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
			mockContextAccessor.Expect(x => x.SetContextProperty("UNB2_1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "HYE"));

			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			mockCodeMapper.Stub(x => x.GetRecipientCode("ShippingPortMessaging", "ShippingPortMessaging", "ShippingPortMessaging COREOR", "Ports", "DestinationParty", "NZAKL")).Return("ShippingPort_POAL_COREOR");
			mockCodeMapper.Stub(x => x.GetRecipientCode("ShippingPortMessaging", "ShippingPortMessaging", "ShippingPortMessaging COREOR", "Ports", "DestinationParty", "NOAKL")).Return("ShippingPort_POAL_COREOR");

			foreach (var num in nums)
			{
				mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanTracing.Transforms.COREOR", "@maxlength", "14")).Return(num);
			}

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			mapTester.Execute<EDIUniversalShipment2COREORD95B>(sourceFile, expectedFile);
		}
	}
}
