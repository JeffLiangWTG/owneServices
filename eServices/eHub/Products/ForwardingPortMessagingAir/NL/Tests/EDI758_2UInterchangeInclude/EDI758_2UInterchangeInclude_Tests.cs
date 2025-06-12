using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessagingAir.NL.CARGONAUT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessagingAir.NL.Tests
{
	[TestClass]
	public class EDI758_2UInterchangeInclude_Tests
	{
		const string filePath = "EDI758_2UInterchangeInclude.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_XML758_2UInterchangeInclude()
		{
			AssertMapping("Test1_input.xml", "Test1_output.xml", "00003017361");
			AssertMapping("Test2_input.xml", "Test2_output.xml", "00003017361");
			AssertMapping("Test3_input.xml", "Test3_output.xml", "00003017361");
			AssertMapping("Test4_input.xml", "Test4_output.xml", "");
			AssertMapping("Test5_input.xml", "Test5_output.xml", "00003017362");
		}

		void AssertMapping(string inputFile, string expectedOutputFile, string acdReference)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var recipientID = "recipientId111";
			var senderId = "CARGONAUT";
			var serviceProviderMsgId = "CGNMSG";

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderId).Repeat.Once();

			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE_AIR", "FORWARDING_PORT_MESSAGE_AIR", "FPMA System Configuration", "Port Settings", "MSGID", senderId)).Return(serviceProviderMsgId).Repeat.Once();

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderId, "@recipientId", "", "@ST_ID", serviceProviderMsgId, "@value", acdReference, "@referenceType", "JobNumber"))
						  .Return(string.IsNullOrEmpty(acdReference) ? string.Empty : "C00010001").Repeat.Once();

			if (!string.IsNullOrEmpty(acdReference))
			{
				mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE_AIR", "FORWARDING_PORT_MESSAGE_AIR", "FPMA System Configuration", "Port Settings", "Name", senderId)).Return(senderId).Repeat.Once();

				if (acdReference == "00003017362")
				{
					mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", senderId, "@ST_ID", serviceProviderMsgId, "@value", "00003017362")).Return(string.Empty).Repeat.Once();
					mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", senderId, "@ST_ID", serviceProviderMsgId, "@value", "JAS")).Return(recipientID).Repeat.Once();
				}
				else
				{
					mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", senderId, "@ST_ID", serviceProviderMsgId, "@value", acdReference)).Return(recipientID).Repeat.Once();
				}

				mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGNTRC", recipientID, senderId, "C00010001_" + acdReference + "_0000099", "", "STU-758")).Repeat.Once();
				mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", recipientID)).Repeat.Once();
			}

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<EDI758_2UInterchangeInclude>(input, expectedOutput);

			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
		}
	}
}
