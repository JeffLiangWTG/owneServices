using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	sealed class ICUSDECMessageDataProviderExtensionsTestCase : TestCaseWithFactory
	{
		public void TestGetDeclarationTypeForDocumentWrapper()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var cusdecMessage = Factory.New<CUSDECEDIMessage>();
			cusdecMessage.EM_ReceiveTransmit = "TRX";
			cusdecMessage.EM_ApplicationCode = "ZAC";
			cusdecMessage.EM_MessageType = SARSEDIMessage.MessageTypes.CUSDEC;
			cusdecMessage.EM_Status = "SNT";
			cusdecMessage.EM_MessageText = CUSRESMessageProcessorTest.TestOutGoingMessage.Replace("\r\n", "");
			cusdecMessage.EM_MessageNum = "202";
			entryHeader.Messages.Add(cusdecMessage);
			var cusresMessage = Factory.New<CUSRESEDIMessage>();
			cusresMessage.EM_ReceiveTransmit = "RCV";
			cusresMessage.EM_ApplicationCode = "ZAC";
			cusresMessage.EM_MessageType = SARSEDIMessage.MessageTypes.CUSRES;
			cusresMessage.EM_Status = "PRS";
			cusresMessage.EM_MessageText = ZAMessageTest.CUSRESTestMessage.Replace("\r\n", "");
			entryHeader.Messages.Add(cusresMessage);
			foreach (CodeDescriptionPair declarationTypePair in new DeclarationTypeList())
			{
				cusdecMessage.EM_MessageText = CUSRESMessageProcessorTest.TestOutGoingMessage.Replace("\r\n", "").Replace("BGM+929+00626166CLP20160426000199::00002+9'", $"BGM+929:::{declarationTypePair.Code}+00626166CLP20160426000199::00002+9'");
				cusdecMessage.ResetDeclarationType();
				cusdecMessage.ResetCUSDECHelper();
				switch (declarationTypePair.Code)
				{
					case DeclarationTypeList.Codes.RegularCompleteDeclarationDefault:
						{
							var provider = new MessageSendingObject(entryHeader);
							AssertEquals(declarationTypePair.Code, "RCD", provider.GetDeclarationTypeForDocumentWrapper());
						}

						break;
					case DeclarationTypeList.Codes.RegularIncompleteDeclaration:
					case DeclarationTypeList.Codes.RegularProvisionalDeclaration:
					case DeclarationTypeList.Codes.RegularSupplementaryDeclaration:
						{
							var provider = new MessageSendingObject(entryHeader);
							AssertEquals(declarationTypePair.Code, "RSD", provider.GetDeclarationTypeForDocumentWrapper());
						}

						break;
				}
			}

			foreach (CodeDescriptionPair declarationTypePair in new DeclarationTypeList())
			{
				cusdecMessage.EM_MessageText = CUSRESMessageProcessorTest.TestOutGoingMessage.Replace("\r\n", "").Replace("BGM+929+00626166CLP20160426000199::00002+9'", $"BGM+929:::{declarationTypePair.Code}+00626166CLP20160426000199::00002+9'");
				cusdecMessage.ResetDeclarationType();
				cusdecMessage.ResetCUSDECHelper();
				AssertEquals(declarationTypePair.Code, CUSDECMessageHelper.New(cusdecMessage).GetDeclarationTypeForDocumentWrapper());
			}

			cusdecMessage.EM_MessageText = CUSRESMessageProcessorTest.TestOutGoingMessage.Replace("\r\n", "");
			AssertEquals("RCD", CUSDECMessageHelper.New(cusdecMessage).GetDeclarationTypeForDocumentWrapper());
			AssertEquals("RCD", new CUSDECMessageDataProviderForTest().GetDeclarationTypeForDocumentWrapper());
		}

		public void TestGetTransportDocumentNumberInBusinessLogic()
		{
			var provider = new CUSDECMessageDataProviderForTest { TransportDocumentNumber = "bill", MasterCargoCarrier = "MSC", TransportMode = TransportModeCodeList.Codes.Sea };
			AssertEquals("MSC bill", provider.GetTransportDocumentNumberInBusinessLogic());
			provider.TransportMode = TransportModeCodeList.Codes.Air;
			AssertEquals("bill", provider.GetTransportDocumentNumberInBusinessLogic());
			provider.TransportMode = TransportModeCodeList.Codes.Road;
			AssertEquals("bill", provider.GetTransportDocumentNumberInBusinessLogic());
			provider = new CUSDECMessageDataProviderForTest { TransportDocumentNumber = "MSC bill", MasterCargoCarrier = "", TransportMode = TransportModeCodeList.Codes.Sea };
			AssertEquals("MSC bill", provider.GetTransportDocumentNumberInBusinessLogic());
		}

		public void TestGetPartofPackagesValue()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			testDeclaration.JE_TransportMode = "SEA";
			var testInstruction1 = testDeclaration.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = "11";
			var testInstruction2 = testDeclaration.CustomsEntryInstructions.AddNew();
			testInstruction2.CEI_Style = "11";
			var testHeader1 = testDeclaration.CustomsEntryHeaders.AddNew();
			testHeader1.CH_CEI_Instruction = testInstruction1.PK;
			testHeader1.CH_BGMReference = "LRN001";
			var testHeader2 = testDeclaration.CustomsEntryHeaders.AddNew();
			testHeader2.CH_CEI_Instruction = testInstruction2.PK;
			testHeader2.CH_BGMReference = "LRN002";
			testDeclaration.JE_TotalNoOfPacks = 1;
			Factory.Save();
			new LineMerger(testDeclaration).DoMerge();
			var provider = new CUSDECMessageDataProviderForTest { PartClearanceQuantity = 2, TotalNoOfPacks = "1" };
			AssertEquals("Part 1 of 2 - Part of 1 Package", provider.GetPartofPackagesValue(testDeclaration, 1));
			testDeclaration.JE_TotalNoOfPacks = 2;
			testHeader1.CH_Packages = 1;
			testHeader2.CH_Packages = 1;
			Factory.Save();
			AssertEquals("Part 1 of 2 - 1 Packages of 2", provider.GetPartofPackagesValue(testDeclaration, 1));
		}
	}
}
