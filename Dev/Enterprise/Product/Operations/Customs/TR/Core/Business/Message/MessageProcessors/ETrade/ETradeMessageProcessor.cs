using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.Business;
using IAsycudaManifestHeader = Enterprise.Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader;

namespace Enterprise.Customs.TR.Business
{
	public abstract class ETradeMessageProcessor : TRBranchCustomsApplicationTypeMessageProcessor<ETradeEDIMessage>
	{
		protected ETradeMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.TRCustoms;

		protected override ZGuid GetCorrectBranchPK(BusinessObject linkedObject)
		{
			var header = linkedObject as IAsycudaManifestHeader;
			return header?.AMA_GB ?? ZGuid.Empty;
		}

		internal bool shouldUpdateMessageMode = true;
		protected override void UpdateStatusCore(ETradeEDIMessage message, bool isSuccess)
		{
			base.UpdateStatusCore(message, isSuccess);

			if (message.EM_LinkedObject != null)
			{
				var messageStatus = isSuccess ? TRMessageStatusCodeList.Codes.Accepted : TRMessageStatusCodeList.Codes.Error;
				if (message.EM_LinkedObject is IMessageAttachee headerAttachee)
				{
					headerAttachee.MessageStatus = messageStatus;
				}

				if (message.EM_LinkedObject is IAsycudaManifestHeader header)
				{
					var statusInfo = MessageStatusCalculatorHelper.CalculateMessageStatusAndMode(OriginalMessageType, messageStatus, header.IsImport);
					if (statusInfo != (null, null))
					{
						header.RegistrationStatus = statusInfo.RegistrationStatus;

						if (shouldUpdateMessageMode)
						{
							header.MessageMode = statusInfo.MessageMode;
						}
					}
				}
			}
		}
	}
}
