using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class TRBranchCustomsMessageProcessorTest : TestCaseWithFactory
	{
		public void TestGetApplicationTypeProcessorCore()
		{
			var processor = new TRBranchCustomsMessageProcessor();
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;

			message.EM_MessageType = TRMessageTypes.Codes.T1S;
			AssertType<ETradeExportRegistrationNoMessageProcessor>("T1S", processor.GetApplicationTypeProcessorCore(message));

			message.EM_MessageType = TRMessageTypes.Codes.T1D;
			AssertType<ETradeImportDischargeListMessageProcessor>("T1D", processor.GetApplicationTypeProcessorCore(message));

			message.EM_MessageType = TRMessageTypes.Codes.T2D;
			AssertType<ETradeImportComplementaryDeclarationProcessor>("T2D", processor.GetApplicationTypeProcessorCore(message));

			message.EM_MessageType = TRMessageTypes.Codes.T1E;
			AssertType<ETradeTemporaryRegistrationMessageProcessor>("T1E", processor.GetApplicationTypeProcessorCore(message));

			message.EM_MessageType = TRMessageTypes.Codes.T1P;
			AssertType<SPTSBranchCustomsApplicationTypeMessageProcessor>("T1P", processor.GetApplicationTypeProcessorCore(message));

			message.EM_MessageType = TRMessageTypes.Codes.TRN;
			AssertType<NCTSSubmitDeclarationResponseMessageProcessor>("TRN", processor.GetApplicationTypeProcessorCore(message));

			message.EM_MessageType = TRMessageTypes.Codes.T1N;
			AssertType<NCTSGetMessagesListByGuidResponseMessageProcessor>("T1N", processor.GetApplicationTypeProcessorCore(message));

			message.EM_MessageType = TRMessageTypes.Codes.T2N;
			AssertType<NCTSDownloadMessageByIndexResponseMessageProcessor>("T2N", processor.GetApplicationTypeProcessorCore(message));

			message.EM_MessageType = TRMessageTypes.Codes.XER;
			AssertType<CustomsServiceErrorMessageProcessor>("XER", processor.GetApplicationTypeProcessorCore(message));

			message.EM_MessageType = TRMessageTypes.Codes.DKO;
			AssertType<DKOMessageProcessor>("DKO", processor.GetApplicationTypeProcessorCore(message));

			message.EM_MessageType = TRMessageTypes.Codes.DK1;
			AssertType<DK1MessageProcessor>("DK1", processor.GetApplicationTypeProcessorCore(message));

			message.EM_MessageType = TRMessageTypes.Codes.DT1;
			AssertType<DT1MessageProcessor>("DT1", processor.GetApplicationTypeProcessorCore(message));

			message.EM_MessageType = TRMessageTypes.Codes.DT3;
			AssertType<DT3MessageProcessor>("DT3", processor.GetApplicationTypeProcessorCore(message));

			message.EM_MessageType = TRMessageTypes.Codes.DTE;
			AssertType<DTEMessageProcessor>("DTE", processor.GetApplicationTypeProcessorCore(message));

			message.EM_MessageType = TRMessageTypes.Codes.TRM;
			AssertType<TRMMessageProcessor>("TRM", processor.GetApplicationTypeProcessorCore(message));
			message.EM_MessageType = TRMessageTypes.Codes.EUR;
			AssertType<EURMessageProcessor>("EUR", processor.GetApplicationTypeProcessorCore(message));
		}
	}
}
