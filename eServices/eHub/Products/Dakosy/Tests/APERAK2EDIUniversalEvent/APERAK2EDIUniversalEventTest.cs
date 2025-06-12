using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.Products.Dakosy.BT.Transforms.APERAK2EDIUniversalEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.EDIFACT.Tests
{
	[TestClass]
	public class APERAK2EDIUniversalEventTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAPERAK2EDIUniversalEvent()
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

			mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM_APERAK", "DAKOSYHAM_APERAK", "Import EDIFACT Responses from Dakosy", "Event Type", "Event Type", "AAG")).Return("MAA");
			mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM_APERAK", "DAKOSYHAM_APERAK", "Import EDIFACT Responses from Dakosy", "Event Type", "Description", "AAG")).Return("Message Accepted");
			mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM_APERAK", "DAKOSYHAM_APERAK", "Import EDIFACT Responses from Dakosy", "Event Type", "EventReference", "AAG")).Return("Completion exit#SZB#|DEP=Dakosy");

			mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM_APERAK", "DAKOSYHAM_APERAK", "Import EDIFACT Responses from Dakosy", "Event Type", "Event Type", "STO")).Return("MRJ");
			mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM_APERAK", "DAKOSYHAM_APERAK", "Import EDIFACT Responses from Dakosy", "Event Type", "Description", "STO")).Return("Message Rejected");
			mockCodeMapper.Stub(x => x.GetRecipientCode("DAKOSYHAM_APERAK", "DAKOSYHAM_APERAK", "Import EDIFACT Responses from Dakosy", "Event Type", "EventReference", "STO")).Return("Examination/Loading Stop#SZB#|DEP=Dakosy");

			mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "DAKOSYHAM", "@recipientId", "", "@ST_ID", "DAKSZB", "@value", "Z12000006721")).Return("S12SHAM0000263C");

			mockDateMapper.Stub(x => x.CurrentDateTimeUTC("O")).Return("2015-12-03T05:48:58.6464725Z");

			var extensionObjects = new Dictionary<string, object>() { 
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile = "APERAK2EDIUniversalEvent.TestFiles.GM21.Test1_input.xml";
			string expectedFile = "APERAK2EDIUniversalEvent.TestFiles.GM21.Test1_output.xml";
			mapTester.Execute<APERAK2EDIUniversalEvent>(sourceFile, expectedFile);

			sourceFile = "APERAK2EDIUniversalEvent.TestFiles.APERAK_AAG.xml";
			expectedFile = "APERAK2EDIUniversalEvent.TestFiles.UniversalEvent_output_AAG.xml";
			mapTester.Execute<APERAK2EDIUniversalEvent>(sourceFile, expectedFile);

			sourceFile = "APERAK2EDIUniversalEvent.TestFiles.APERAK_STO.xml";
			expectedFile = "APERAK2EDIUniversalEvent.TestFiles.UniversalEvent_output_STO.xml";
			mapTester.Execute<APERAK2EDIUniversalEvent>(sourceFile, expectedFile);

			sourceFile = "APERAK2EDIUniversalEvent.TestFiles.SZBRef_Input.xml";
			expectedFile = "APERAK2EDIUniversalEvent.TestFiles.SZBRef_Output.xml";
			mapTester.Execute<APERAK2EDIUniversalEvent>(sourceFile, expectedFile);
		}
	}
}
