using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanTracing.Transforms.EDIUniversalShipment2COREOR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.EDIFACT.Tests
{
	[TestClass]
	public class EDIUniversalShipment2COREORTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIUniversalShipment2COREOR_Order()
		{
			const string sourceFile = "EDIUniversalShipment2COREOR.TestFiles.Universal_Order.xml";
			const string expectedFile = "EDIUniversalShipment2COREOR.TestFiles.COREOR_Order.xml";
			TestMapping(sourceFile, expectedFile, "NZNPE", "ShippingPort_PONL_COREOR", "PONL", "MRN123");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIUniversalShipment2COREOR_Withdrawal()
		{
			const string sourceFile = "EDIUniversalShipment2COREOR.TestFiles.Universal_Withdrawal.xml";
			const string expectedFile = "EDIUniversalShipment2COREOR.TestFiles.COREOR_Withdrawal.xml";
			TestMapping(sourceFile, expectedFile, "NZNPE", "ShippingPort_PONL_COREOR", "PONL", "MRN123", "BGM123");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIUniversalShipment2COREOR_Withdrawal_OperationalPort_Code()
		{
			const string sourceFile = "EDIUniversalShipment2COREOR.TestFiles.Universal_Withdrawal_OperationalPort_Code.xml";
			const string expectedFile = "EDIUniversalShipment2COREOR.TestFiles.COREOR_Withdrawal_OperationalPort_Code.xml";
			TestMapping(sourceFile, expectedFile, "NONPE", "ShippingPort_PONL_COREOR", "PONL", "MRN123", "BGM123");
		}

		void TestMapping(string sourceFile, string expectedFile, string port, string expectedEHubId, string expectedEdiId, params string[] nums)
		{
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("yyyyMMddHHmm"))).Return("201412190830");

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("HYEDNZIKB");
			mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", expectedEHubId));

			mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyName", "http://schemas.microsoft.com/Edi/PropertySchema", expectedEHubId));
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
			mockContextAccessor.Expect(x => x.SetContextProperty("UNB2_1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "HYE"));


			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			mockCodeMapper.Stub(x => x.GetRecipientCode("ShippingPortMessaging", "ShippingPortMessaging", "ShippingPortMessaging COREOR", "Ports", "DestinationParty", port)).Return(expectedEHubId);
			mockCodeMapper.Stub(x => x.GetRecipientCode("ShippingPortMessaging", "ShippingPortMessaging", "ShippingPortMessaging COREOR", "Ports", "EdiIdentifier", port)).Return(expectedEdiId);

			foreach (var num in nums)
			{
				mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanTracing.Transforms.COREOR", "@maxlength", "14")).Return(num);
			}

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			mapTester.Execute<EDIUniversalShipment2COREOR>(sourceFile, expectedFile);
			mockContextAccessor.VerifyAllExpectations();
		}
	}
}
