using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanTracing.Transforms.EDIUniversalShipment2COPARN;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.EDIFACT.Tests
{
	[TestClass]
	public class EDIUniversalShipment2COPARNTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIUniversalShipment2COPARN()
		{
			const string sourceFile = "EDIUniversalShipment2COPARN.TestFiles.Input_1.xml";
			const string expectedFile = "EDIUniversalShipment2COPARN.TestFiles.Output_1.xml";
			TestMapping(sourceFile, expectedFile, "REFNUM123", "DOCNUM123");
		}

		#region Yard

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIUniversalShipment2COPARN_Authorization()
		{
			const string sourceFile = "EDIUniversalShipment2COPARN.TestFiles.Yard.Booking_Authorisation.xml";
			const string expectedFile = "EDIUniversalShipment2COPARN.TestFiles.Yard.COPARN_Authorisation.xml";
			TestMapping(sourceFile, expectedFile, "REFNUM123");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIUniversalShipment2COPARN_Cancellation()
		{
			const string sourceFile = "EDIUniversalShipment2COPARN.TestFiles.Yard.Booking_Cancellation.xml";
			const string expectedFile = "EDIUniversalShipment2COPARN.TestFiles.Yard.COPARN_Cancellation.xml";
			TestMapping(sourceFile, expectedFile, "REFNUM123", "DOCNUM456");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIUniversalShipment2COPARN_Revised()
		{
			const string sourceFile = "EDIUniversalShipment2COPARN.TestFiles.Yard.Booking_Revised.xml";
			const string expectedFile = "EDIUniversalShipment2COPARN.TestFiles.Yard.COPARN_Revised.xml";
			TestMapping(sourceFile, expectedFile, "REFNUM567", "DOCNUM678");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIUniversalShipment2COPARN__Input_1()
		{
			const string sourceFile = "EDIUniversalShipment2COPARN.TestFiles.Yard.Input_1.xml";
			const string expectedFile = "EDIUniversalShipment2COPARN.TestFiles.Yard.Output_1.xml";
			TestMapping(sourceFile, expectedFile, "REFNUM567", "DOCNUM678");
		}

		#endregion

		#region Port

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIUniversalShipment2COPARN_PortAuthorisation()
		{
			const string sourceFile = "EDIUniversalShipment2COPARN.TestFiles.Port.Booking_PortAuthorisation.xml";
			const string expectedFile = "EDIUniversalShipment2COPARN.TestFiles.Port.COPARN_PortAuthorisation.xml";
			TestMapping(sourceFile, expectedFile, "REFNUM123");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIUniversalShipment2COPARN_PortRevision()
		{
			const string sourceFile = "EDIUniversalShipment2COPARN.TestFiles.Port.Booking_PortRevision.xml";
			const string expectedFile = "EDIUniversalShipment2COPARN.TestFiles.Port.COPARN_PortRevision.xml";
			TestMapping(sourceFile, expectedFile, "REFNUM123", "DOCNUM123");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIUniversalShipment2COPARN_PortInstanceCancellation()
		{
			const string sourceFile = "EDIUniversalShipment2COPARN.TestFiles.Port.Booking_PortInstanceCancellation.xml";
			const string expectedFile = "EDIUniversalShipment2COPARN.TestFiles.Port.COPARN_PortInstanceCancellation.xml";
			TestMapping(sourceFile, expectedFile, "REFNUM123", "DOCNUM123");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEDIUniversalShipment2COPARN_PortFullCancellation()
		{
			const string sourceFile = "EDIUniversalShipment2COPARN.TestFiles.Port.Booking_PortFullCancellation.xml";
			const string expectedFile = "EDIUniversalShipment2COPARN.TestFiles.Port.COPARN_PortFullCancellation.xml";
			TestMapping(sourceFile, expectedFile, "REFNUM123", "DOCNUM123");
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestEDIUniversalShipment2COPARN_PortWithSpaceInReference()
        {
            const string sourceFile = "EDIUniversalShipment2COPARN.TestFiles.Port.Booking_PortWithSpace.xml";
            const string expectedFile = "EDIUniversalShipment2COPARN.TestFiles.Port.COPARN_PortWithSpace.xml";
            TestMapping(sourceFile, expectedFile, "REFNUM123", "DOCNUM123");
        }

		#endregion

		void TestMapping(string sourceFile, string expectedFile, params string[] nums)
		{
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("yyyyMMddHHmm"))).Return("201412190830");

			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			foreach (var num in nums)
			{
				mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanTracing.Transforms.COPARN", "@maxlength", "14")).Return(num);
			}

			mockCodeMapper.Stub(x => x.GetRecipientCode("ShippingPortMessaging", "ShippingPortMessaging", "ShippingPortMessaging COPARN", "Ports", "DestinationParty", "NZAKL")).Return("ShippingPort_POAL_COPARN");
			mockCodeMapper.Stub(x => x.GetRecipientCode("ShippingPortMessaging", "ShippingPortMessaging", "ShippingPortMessaging COPARN", "Ports", "EdiIdentifier", "NZAKL")).Return("POAL");

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("HYEDNZIKB");
			mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "ShippingPort_POAL_COPARN"));

			mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyName", "http://schemas.microsoft.com/Edi/PropertySchema", "ShippingPort_POAL_COPARN"));
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
			mockContextAccessor.Expect(x => x.SetContextProperty("UNB2_1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "HYE"));

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

            mapTester.Execute<EDIUniversalShipment2COPARN>(sourceFile,expectedFile);
            mockContextAccessor.VerifyAllExpectations();
		}
	}
}
