using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.CACustoms.DocImg.BT.Transformations.UniversalEvent2LPCOImage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;

namespace CargoWise.eHub.Products.CACustoms.DocImg.BT.Tests
{
	[TestClass]
	public class UniversalEvent2LPCOImageTests
	{
		const string filePath = "TestFiles.";
		Random rand;

		[TestInitialize]
		public void Initialize()
		{
			rand = new Random();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalEvent2LPCOImage_ValidMessage()
		{
			InitilizeAndExecute("10207000013835", "EDIDATDAU", "Test1_input.xml", "Test1_output.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalEvent2LPCOImage_InvalidMessage()
		{
			InitilizeAndExecute("4321876509321", "EDIDATDAU", "Test2_input.xml", "Test2_output.xml", "CACustomsDoc");
		}

		static void InitilizeAndExecute(string requestNumber, string sourceDestination, string inputFile, string outputFile, string recipientID = "CACustomsDocTest")
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(sourceDestination);
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(recipientID);
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var trueRecipientID = recipientID == "CACustomsDocTest" ? "CACustomsTest" : "CACustoms";
			mockDataModelAccessor.Stub(x => x.InsertSubscriptionValue(
				Arg<string>.Is.Equal("CACDIF")
				, Arg<string>.Is.Equal(trueRecipientID)
				, Arg<string>.Is.Equal(sourceDestination)
				, Arg<string>.Is.Equal(requestNumber)
				, Arg<string>.Is.Anything));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CACDIF", trueRecipientID, sourceDestination, requestNumber));

			var extensionObjects = new Dictionary<string, object>
			{
				{"http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor},
				{"http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDataModelAccessor}
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<UniversalEvent2LPCOImage>(filePath + inputFile, filePath + outputFile);

			mockContextAccessor.VerifyAllExpectations();
		}
	}
}
