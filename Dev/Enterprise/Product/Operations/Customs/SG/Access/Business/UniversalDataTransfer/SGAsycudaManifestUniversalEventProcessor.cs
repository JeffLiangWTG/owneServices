using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.SG.Access.Business.UniversalDataTransfer
{
	public class SGAsycudaManifestUniversalEventProcessor : ASYCUDA.Business.UniversalDataTransfer.AsycudaManifestUniversalEventProcessor
	{
		public SGAsycudaManifestUniversalEventProcessor(BusinessObjectFactory factory, IXmlImportLogger logger, ZString countryCode)
			: base(factory, logger, countryCode)
		{ }

		protected override bool CouldUpdateMessageAndCustomsStatusForPackItem(ZString actionPurpose, ASYCUDA.Business.AsycudaPackedItem packedItem)
		{
			var result = true;
			if (actionPurpose == AsycudaEventMessageConstants.ActionPurpose.ACK)
			{
				result = packedItem.API_MessageStatus != MessageStatusCodeList.Codes.Error && packedItem.API_MessageStatus != MessageStatusCodeList.Codes.Accepted;
			}
			return result;
		}

		protected override string GetMessageStatusMappedCode(ZString code, ZString manifestType, ZString messageType, ZString actionPurpose)
		{
			if (actionPurpose == AsycudaEventMessageConstants.ActionPurpose.ACK)
			{
				return HandleActionPurposeIsACK(code, manifestType, messageType);
			}
			else
			{
				return base.GetMessageStatusMappedCode(code, manifestType, messageType, actionPurpose);
			}
		}

		string HandleActionPurposeIsACK(ZString code, ZString manifestType, ZString messageType)
		{
			switch (messageType)
			{
				case Constants.MessageType.AIRPCM:
				case Constants.MessageType.AIRAED:
					return ASYCUDA.Business.MessageStatusCodeList.Codes.Awaiting;
				case Constants.MessageType.AIRPCU:
				case Constants.MessageType.AIRAEU:
					return ASYCUDA.Business.MessageStatusCodeList.Codes.Accepted;
				default:
					return ASYCUDA.Business.MessageStatusCodeList.GetMappedCode(factory, countryCode, code, manifestType);
			}
		}
	}
}
