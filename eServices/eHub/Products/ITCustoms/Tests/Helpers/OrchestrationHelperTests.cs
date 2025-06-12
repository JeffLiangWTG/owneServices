using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.ITCustoms.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ITCustoms.Tests.Helpers
{
	[TestClass]
	public class OrchestrationHelperTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGetMessageBatchTimeLimit()
		{
			var mockeHubTransactionsContext = TestHelper.eHubTransactionsContextForTest();
			OrchestrationHelper.GetContext = () => mockeHubTransactionsContext;
			var customs = new eHubClient() { CC_ID = "ITCustoms", CC_FriendlyName = "ITCustoms Production" };
			var transformationSet = new eHubTransformationSet() { TS_Name = "ITCustoms Outbound Message Batch Time Limit", eHubClient_Recipient = customs };
			var codeSet = new eHubCodeSet() { CS_Name = "Default", eHubClient_Sender = customs, eHubClient_Recipient = customs, eHubTransformationSet = transformationSet };
			var codeSetResult = new eHubCodeSetResult() { eHubCodeSet = codeSet, CR_Name = "Batch Time(Seconds)", CR_Order = 1 };
			var codeMapKey = new eHubCodeMapKey() { eHubCodeSet = codeSet, CK_Order = 1 };
			var codeMapValue = new eHubCodeMapValue() { eHubCodeMapKey = codeMapKey, eHubCodeSetResult = codeSetResult, CV_OutputCode = "300" };

			var ehubClients = new TestDbSet<eHubClient>() { customs };
			var transfromationSets = new TestDbSet<eHubTransformationSet>() { transformationSet };
			var codeSets = new TestDbSet<eHubCodeSet>() { codeSet };
			var setResults = new TestDbSet<eHubCodeSetResult>() { codeSetResult };
			var codeMapKeys = new TestDbSet<eHubCodeMapKey>() { codeMapKey };
			var codeMapValues = new TestDbSet<eHubCodeMapValue>() { codeMapValue };

			mockeHubTransactionsContext.Stub(x => x.eHubClients).Return(ehubClients);
			mockeHubTransactionsContext.Stub(x => x.eHubTransformationSets).Return(transfromationSets);
			mockeHubTransactionsContext.Stub(x => x.eHubCodeSets).Return(codeSets);
			mockeHubTransactionsContext.Stub(x => x.eHubCodeSetResults).Return(setResults);
			mockeHubTransactionsContext.Stub(x => x.eHubCodeMapKeys).Return(codeMapKeys);
			mockeHubTransactionsContext.Stub(x => x.eHubCodeMapValues).Return(codeMapValues);

			Assert.AreEqual(new TimeSpan(0, 0, 300), OrchestrationHelper.GetSendMessageBatchTimeSpan("ITCustoms"));
			mockeHubTransactionsContext.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGetXpathValue()
		{
			var xmlString = @"
<ns0:DeclarationFile xmlns:ns0='http://cargowise.com/ehub/products/ITCustoms/2017/08/DeclarationFile'>
	<ShipmentID>S700049923</ShipmentID>
	<Node>BOB</Node>
	<UserID>ABCD</UserID>
	<FileType>P</FileType>
	<Declaration>IT Customs Message</Declaration>
</ns0:DeclarationFile>";

			var declaration = OrchestrationHelper.GetXpathValue(xmlString,
				"/*[local-name()='DeclarationFile']/*[local-name()='Declaration']");

			Assert.AreEqual("IT Customs Message", declaration);
		}


		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestInsertOrUpdateXmlElement()
		{
			var eHubITMessage = new System.Xml.XmlDocument();
			eHubITMessage.LoadXml(@"<ITMessage xmlns:ns=""http://cargowise.com/ehub/products/ITCustoms""><MessageType>SWR</MessageType><AccountNumber>123456789-123</AccountNumber><Body>&lt;richiesta_esito&gt;&lt;dichiarazione&gt;&lt;num_reg&gt;25781&lt;/num_reg&gt;&lt;cod_uff_dog&gt;279100&lt;/cod_uff_dog&gt;&lt;cod_reg&gt;1&lt;/cod_reg&gt;&lt;anno_reg&gt;2021&lt;/anno_reg&gt;&lt;/dichiarazione&gt;&lt;/richiesta_esito&gt;</Body></ITMessage>");

			OrchestrationHelper.InsertOrUpdateXmlElement(eHubITMessage, "StatusHash", "-123");

			var declaration = OrchestrationHelper.GetXpathValue(eHubITMessage.OuterXml,
				"/*[local-name()='ITMessage']/*[local-name()='StatusHash']");
			Assert.AreEqual("-123", declaration);

			OrchestrationHelper.InsertOrUpdateXmlElement(eHubITMessage, "StatusHash", "-999");
			declaration = OrchestrationHelper.GetXpathValue(eHubITMessage.OuterXml,
				"/*[local-name()='ITMessage']/*[local-name()='StatusHash']");
			Assert.AreEqual("-999", declaration);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestLAndQFilesAreExpectedFromRFile()
		{
			CheckLAndQFilesAreExpectedFromRFileRunAndAssert("TestFiles.SampleRFiles.BothLandQAreExpected", true, true);
			CheckLAndQFilesAreExpectedFromRFileRunAndAssert("TestFiles.SampleRFiles.OnlyLIsExpected", true, false);
			CheckLAndQFilesAreExpectedFromRFileRunAndAssert("TestFiles.SampleRFiles.OnlyQIsExpected", false, true);
		}

		private void CheckLAndQFilesAreExpectedFromRFileRunAndAssert(string RFileName, bool expectL, bool expectQ)
		{
			bool actualL, actualQ;
			var RFileContent = TestHelper.GetResourceText(RFileName);
			OrchestrationHelper.CheckForLAndQIndicators(RFileContent, out actualL, out actualQ);
			Assert.AreEqual(expectL, actualL);
			Assert.AreEqual(expectQ, actualQ);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestLAndQFilesSearchingRowCountsAreExpectedFromRFile()
		{
			CheckLAndQSearchingRowCountAreExpectedFromRFileRunAssert("TestFiles.SampleRFiles.BothLandQAreExpected", 1, 1);
			CheckLAndQSearchingRowCountAreExpectedFromRFileRunAssert("TestFiles.SampleRFiles.OnlyLIsExpected", 2, 0);
			CheckLAndQSearchingRowCountAreExpectedFromRFileRunAssert("TestFiles.SampleRFiles.OnlyQIsExpected", 0, 2);
		}

		private void CheckLAndQSearchingRowCountAreExpectedFromRFileRunAssert(string RFileName, int expectIRILDESCount, int expectIVISTOCount)
		{
			var declarationContent = TestHelper.GetResourceText(RFileName);
			var actualIRILDESCount = OrchestrationHelper.GetCountOfTETRowsFromIRILDES(declarationContent);
			var actualIVISTOCount = OrchestrationHelper.GetCountOfTETRowsFromIVISTO(declarationContent);
			Assert.AreEqual(expectIRILDESCount, actualIRILDESCount);
			Assert.AreEqual(expectIVISTOCount, actualIVISTOCount);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCheckIfFinalStatusOfLineForL()
		{
			var row = "TIE45    014581A21ITQ1W8T0000042T714IT01TR000003691Garanzia svincolata         050121LU715000";
			Assert.AreEqual(true, OrchestrationHelper.CheckIfFinalStatusOfLineForL(row));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCheckIfFinalStatusOfLineForQ()
		{
			var row = "TIVISTO  21ITQ1V1T0000286E2IT371100IT279100MALPENSA                           07012021Uscita conclusa                         ";
			Assert.AreEqual(true, OrchestrationHelper.CheckIfFinalStatusOfLineForQ(row));
		}
	}
}
