using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.TR.Business
{
	public class TRBranchCustomsMessageProcessor : BranchCustomsMessageProcessor
	{
		public TRBranchCustomsMessageProcessor() : base(new ZString[] { EDIMessage.ApplicationCodes.TRCustoms }, null)
		{
		}

		public override ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(EDIMessage message)
		{
			if (message.EM_ApplicationCode == EDIMessage.ApplicationCodes.TRCustoms)
			{
				switch (message.EM_MessageType)
				{
					case TRMessageTypes.Codes.XER:
						return new CustomsServiceErrorMessageProcessor(Logger);
					case TRMessageTypes.Codes.TRO:
						return new TROMessageProcessor(Logger);
					case TRMessageTypes.Codes.T1O:
						return new T1OMessageProcessor(Logger);
					case TRMessageTypes.Codes.T3O:
						return new T3OMessageProcessor(Logger);
					case TRMessageTypes.Codes.TRM:
						return new TRMMessageProcessor(Logger);
					case TRMessageTypes.Codes.TRE:
					case TRMessageTypes.Codes.T1E:
						return new ETradeTemporaryRegistrationMessageProcessor(Logger);
					case TRMessageTypes.Codes.TRI:
						return new ETradeQueryInspectionClerkMessageProcessor(Logger);
					case TRMessageTypes.Codes.TRS:
					case TRMessageTypes.Codes.T1S:
						return new ETradeExportRegistrationNoMessageProcessor(Logger);
					case TRMessageTypes.Codes.TRQ:
						return new ETradeQueryRegistrationNoMessageProcessor(Logger);
					case TRMessageTypes.Codes.TRD:
					case TRMessageTypes.Codes.T1D:
						return new ETradeImportDischargeListMessageProcessor(Logger);
					case TRMessageTypes.Codes.TCD:
					case TRMessageTypes.Codes.T2D:
						return new ETradeImportComplementaryDeclarationProcessor(Logger);
					case TRMessageTypes.Codes.TRB:
						return new ETradeQueryRemainingBillsForImportMessageProcessor(Logger);
					case TRMessageTypes.Codes.TRL:
						return new ETradeQueryInspectionLineMessageProcessor(Logger);
					case TRMessageTypes.Codes.TSP:
					case TRMessageTypes.Codes.T1P:
						return new SPTSBranchCustomsApplicationTypeMessageProcessor(Logger);
					case TRMessageTypes.Codes.TRN:
						return new NCTSSubmitDeclarationResponseMessageProcessor(Logger);
					case TRMessageTypes.Codes.T1N:
						return new NCTSGetMessagesListByGuidResponseMessageProcessor(Logger);
					case TRMessageTypes.Codes.T2N:
						return new NCTSDownloadMessageByIndexResponseMessageProcessor(Logger);
					case TRMessageTypes.Codes.T2O:
						return new T2OMessageProcessor(Logger);
					case TRMessageTypes.Codes.DKO:
						return new DKOMessageProcessor(Logger);
					case TRMessageTypes.Codes.DK1:
						return new DK1MessageProcessor(Logger);
					case TRMessageTypes.Codes.DT1:
						return new DT1MessageProcessor(Logger);
					case TRMessageTypes.Codes.DT2:
						return new DT2MessageProcessor(Logger);
					case TRMessageTypes.Codes.DT3:
						return new DT3MessageProcessor(Logger);
					case TRMessageTypes.Codes.DTE:
						return new DTEMessageProcessor(Logger);
					case TRMessageTypes.Codes.EUR:
						return new EURMessageProcessor(Logger);
				}
			}
			return null;
		}
	}
}
