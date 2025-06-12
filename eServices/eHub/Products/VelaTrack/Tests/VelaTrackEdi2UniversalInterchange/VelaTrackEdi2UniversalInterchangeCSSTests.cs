using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.VelaTrack.Transforms.VelaTrackEdi2UniversalInterchange;
using CargoWise.eHub.Products.VelaTrack.Transforms.VelaTrackEdi2UniversalInterchangeCSS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.VelaTrack.Tests
{
	[TestClass]
	public class VelaTrackEdi2UniversalInterchangeCSSTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestVelaTrackEdi2UniversalInterchangeCSS_Client()
		{
			var sourceFile = "VelaTrackEdi2UniversalInterchange.TestFiles.SEA_Client_Input.xml";
			var expectedFile = "VelaTrackEdi2UniversalInterchange.TestFiles.SEA_Client_Output.xml";
			InitilizeAndExecute("EDIDATDAU", sourceFile, expectedFile);
		}


		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestVelaTrackEdi2UniversalInterchangeCSS_Client_Consol()
		{
			var sourceFile = "VelaTrackEdi2UniversalInterchange.TestFiles.SEA_Client_Consol_Input.xml";
			var expectedFile = "VelaTrackEdi2UniversalInterchange.TestFiles.SEA_Client_Consol_Output.xml";
			InitilizeAndExecute("CEDIDATDAU", sourceFile, expectedFile);
		}


		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestVelaTrackEdi2UniversalInterchangeCSS_Reference_Client()
		{
			var sourceFile = "VelaTrackEdi2UniversalInterchange.TestFiles.SEA_Reference_Client_Input.xml";
			var expectedFile = "VelaTrackEdi2UniversalInterchange.TestFiles.SEA_Reference_Client_CSS_Output.xml";
			InitilizeAndExecute("CONTAINER_TRACKING", sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestVelaTrackEdi2UniversalInterchangeCSS_Reference_Client_Consol()
		{
			var sourceFile = "VelaTrackEdi2UniversalInterchange.TestFiles.SEA_Reference_Client_Consol_Input.xml";
			var expectedFile = "VelaTrackEdi2UniversalInterchange.TestFiles.SEA_Reference_Client_Consol_CSS_Output.xml";
			InitilizeAndExecute("CONTAINER_TRACKING", sourceFile, expectedFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestVelaTrackEdi2UniversalInterchangeCSS_EmptyReference()
		{
			var sourceFile = "VelaTrackEdi2UniversalInterchange.TestFiles.SEA_EmptyReference_Input.xml";
			var expectedFile = "VelaTrackEdi2UniversalInterchange.TestFiles.SEA_EmptyReference_Output.xml";
			InitilizeAndExecute("CONTAINER_TRACKING", sourceFile, expectedFile);
		}

		static void InitilizeAndExecute(string recipientId, string inputFile, string outputFile)
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", recipientId));

			mockCodeMapper.Expect(x => x.GetRecipientCode("VelaTrack_CSS", "VelaTrack_CSS", "VelaTrack to UniversalEvent", "Status Event", "CW1 Code", "EE")).Return("GOY").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("VelaTrack_CSS", "VelaTrack_CSS", "VelaTrack to UniversalEvent", "Status Event", "Reference", "EE")).Return("EEref").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("VelaTrack_CSS", "VelaTrack_CSS", "VelaTrack to UniversalEvent", "Status Event", "Parameters", "EE")).Return("Facility=CY").Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode("VelaTrack_CSS", "VelaTrack_CSS", "VelaTrack to UniversalEvent", "Status Event", "CW1 Code", "I")).Return("GIN").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("VelaTrack_CSS", "VelaTrack_CSS", "VelaTrack to UniversalEvent", "Status Event", "Reference", "I")).Return("Iref").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("VelaTrack_CSS", "VelaTrack_CSS", "VelaTrack to UniversalEvent", "Status Event", "Parameters", "I")).Return("Facility=CTO").Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode("VelaTrack_CSS", "VelaTrack_CSS", "VelaTrack to UniversalEvent", "Status Event", "CW1 Code", "L")).Return("FLO").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("VelaTrack_CSS", "VelaTrack_CSS", "VelaTrack to UniversalEvent", "Status Event", "Reference", "L")).Return("Lref").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("VelaTrack_CSS", "VelaTrack_CSS", "VelaTrack to UniversalEvent", "Status Event", "Parameters", "L")).Return("Facility=CTO").Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode("VelaTrack_CSS", "VelaTrack_CSS", "VelaTrack to UniversalEvent", "Status Event", "CW1 Code", "UV")).Return("FUL").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("VelaTrack_CSS", "VelaTrack_CSS", "VelaTrack to UniversalEvent", "Status Event", "Reference", "UV")).Return("").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("VelaTrack_CSS", "VelaTrack_CSS", "VelaTrack to UniversalEvent", "Status Event", "Parameters", "UV")).Return("Facility=CTO").Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode("VelaTrack_CSS", "VelaTrack_CSS", "VelaTrack to UniversalEvent", "Status Event", "CW1 Code", "CR")).Return("RLS").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("VelaTrack_CSS", "VelaTrack_CSS", "VelaTrack to UniversalEvent", "Status Event", "Reference", "CR")).Return("CRref").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("VelaTrack_CSS", "VelaTrack_CSS", "VelaTrack to UniversalEvent", "Status Event", "Parameters", "CR")).Return("Facility=CTO|Department=Carrier").Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode("VelaTrack_CSS", "VelaTrack_CSS", "VelaTrack to UniversalEvent", "Status Event", "CW1 Code", "CT")).Return("RLS").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("VelaTrack_CSS", "VelaTrack_CSS", "VelaTrack to UniversalEvent", "Status Event", "Reference", "CT")).Return("CTref").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("VelaTrack_CSS", "VelaTrack_CSS", "VelaTrack to UniversalEvent", "Status Event", "Parameters", "CT")).Return("Facility=CTO|Department=Customs").Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode("VelaTrack_CSS", "VelaTrack_CSS", "VelaTrack to UniversalEvent", "Status Event", "CW1 Code", "PA")).Return("SHL").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("VelaTrack_CSS", "VelaTrack_CSS", "VelaTrack to UniversalEvent", "Status Event", "Reference", "PA")).Return("PAref").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("VelaTrack_CSS", "VelaTrack_CSS", "VelaTrack to UniversalEvent", "Status Event", "Parameters", "PA")).Return("Department=Government").Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode("VelaTrack_CSS", "VelaTrack_CSS", "VelaTrack to UniversalEvent", "Status Event", "CW1 Code", "X9")).Return("SHL").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("VelaTrack_CSS", "VelaTrack_CSS", "VelaTrack to UniversalEvent", "Status Event", "Reference", "X9")).Return("").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("VelaTrack_CSS", "VelaTrack_CSS", "VelaTrack to UniversalEvent", "Status Event", "Parameters", "X9")).Return("Department=Carrier").Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode("VelaTrack_CSS", "VelaTrack_CSS", "VelaTrack to UniversalEvent", "Status Event", "CW1 Code", "OA")).Return("GOU").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("VelaTrack_CSS", "VelaTrack_CSS", "VelaTrack to UniversalEvent", "Status Event", "Reference", "OA")).Return("OAref").Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("VelaTrack_CSS", "VelaTrack_CSS", "VelaTrack to UniversalEvent", "Status Event", "Parameters", "OA")).Return("Facility=CTO").Repeat.Any();



			var extensionObjects = new Dictionary<string, object>()
			{
				{"http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper},
				{"http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor},
			};


			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<VelaTrackEdi2UniversalInterchangeCSS>(inputFile, outputFile);

			mockContextAccessor.VerifyAllExpectations();
		}
	}
}
