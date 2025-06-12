using System;
using System.IO;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.CACustoms.Transformations.GOVCBR2UniversalEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.CACustoms.Tests.Transformations
{
	[TestClass]
	public class CACustomsGOVCBR2UniversalEventTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestTransformGOVCBR2UniversalEvent_CIG()
		{
			AssertExcuteResultEquals(false, "SENDER", "IIDP", "UNB+UNOC:3+INETCECPT+YUSAIRXPN+160901:0146+7995'###", "GOVCBR_Input.xml", "UniversalEvent_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestTransformGOVCBR2UniversalEvent_Direct()
		{
			AssertExcuteResultEquals(false, "RCCECECPP", "IIDP", "UNB+UNOC:3+RCCECECPP+YUSAIRXPN+160901:0146+7999'###", "GOVCBR_Direct_Input.xml", "UniversalEvent_Direct_output.xml", true);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestTransformGOVCBR2UniversalEvent_D4_Simple()
		{
			AssertExcuteResultEquals(true, "INETCECPP", "CCR", "UNB+UNOC:3+INETCECPT+YUSAIRXPN+160901:0146+7995", "GOVCBR_Input_D4_Simple.xml", "UniversalEvent_output_D4_Simple.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestTransformGOVCBR2UniversalEvent_D4_Full()
		{
			AssertExcuteResultEquals(true, "INETCECPP", "CCR", "UNB+UNOC:3+INETCECPT+YUSAIRXPN+160901:0146+7995+++A++1", "GOVCBR_Input_D4_Full.xml", "UniversalEvent_output_D4_Full.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestTransformGOVCBR2UniversalEvent_D4_AllSegs()
		{
			AssertExcuteResultEquals(true, "INETCECPP", "CCR", "UNB+UNOC:3+INETCECPT+YUSAIRXPN+160901:0146+7995+++A++1'", "GOVCBR_Input_D4_AllSegs.xml", "UniversalEvent_output_D4_AllSegs.xml");
		}

		void AssertExcuteResultEquals(bool testD4, string unb21, string ung21, string ung5, string sourceFile, string expectedFile, bool isDirect = false)
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<CargoWise.eHub.Core.Transforms.Helper.ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return(unb21);
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("RECIPIENT");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB_Segment", "http://schemas.microsoft.com/Edi/PropertySchema")).Return(ung5);
			mockContextAccessor.Expect(x => x.GetContextProperty("UNG2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return(ung21);
			mockContextAccessor.Expect(x => x.GetContextProperty("RawMessage", "http://cargowise.com/ehub/processing/CACustoms/2016/12")).Return("RawMessageEDIFACT").Repeat.Once();
			if (!testD4)
			{
				var sourceParty = isDirect ? "CACustomsMQ" : "CACustoms";
				mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(sourceParty);
				mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", sourceParty, "@recipientId", "", "@value", unb21 + "+RECIPIENT+12345XXXXXXXXXXX", "@ST_ID", "IIDMSG", "@referenceType", "IIDMSG")).Return("SentRawMessageEDIFACT").Repeat.Once();
			}
			var extensionObjects = new System.Collections.Generic.Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockCodeMapper }
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<CACustomsGOVCBR2UniversalEvent>("Transformations.TestFiles.CACustomsGOVCBR2UniversalEvent_Input." + sourceFile, "Transformations.TestFiles.CACustomsGOVCBR2UniversalEvent_Output." + expectedFile);
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestTransformGOVCBR2UniversalEvent_DIF_312()
		{
			AssertDIFExcuteResultEquals("INETCECPT", "WTCDIF01", "GOVCBR_Input_DIF_312.xml", "UniversalEvent_output_DIF_312.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestTransformGOVCBR2UniversalEvent_DIF_313()
		{
			AssertDIFExcuteResultEquals("INETCECPT", "WTCDIF01", "GOVCBR_Input_DIF_313.xml", "UniversalEvent_output_DIF_313.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestTransformGOVCBR2UniversalEvent_DIF_ERC()
		{
			AssertDIFExcuteResultEquals("INETCECPT", "WTCDIF01", "GOVCBR_Input_DIF_ERC.xml", "UniversalEvent_output_DIF_ERC.xml");
		}

		void AssertDIFExcuteResultEquals(string unb21, string unb31, string sourceFile, string expectedFile)
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<CargoWise.eHub.Core.Transforms.Helper.ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return(unb21).Repeat.Once();;
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return(unb31).Repeat.Once();;
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CACustomsTest").Repeat.Once();;
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", "CACustomsTest", "@value", "10207900000602", "@ST_ID", "CACDIF")).Return("4TSJEATST").Repeat.Once();;
			var extensionObjects = new System.Collections.Generic.Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockCodeMapper }
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<CACustomsGOVCBR2UniversalEvent>("Transformations.TestFiles.CACustomsGOVCBR2UniversalEvent_Input." + sourceFile, "Transformations.TestFiles.CACustomsGOVCBR2UniversalEvent_Output." + expectedFile);
			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
		}
	}
}
