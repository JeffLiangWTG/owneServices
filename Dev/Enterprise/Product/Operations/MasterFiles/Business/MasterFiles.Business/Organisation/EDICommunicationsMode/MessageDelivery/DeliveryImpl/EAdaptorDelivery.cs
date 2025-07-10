using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;

namespace Enterprise.MasterFiles.Business.MessageDelivery
{
	public sealed class EAdaptorDelivery : EServicesDelivery
	{
		protected override string GetRecipientID(IEDICommunicationsMode mode)
		{
			if (mode is EDICommunicationsMode modeBO)
			{
				var organisation = modeBO.Organisation;
				if (organisation != null)
				{
					return organisation.OH_Code; // Just to stop it blowing up for now.
				}
			}

			return mode.EK_Destination; // Fallback to make mocked testing easier.
		}

		protected override string InterchangeQueuedStatus
		{
			get { return EDIInterchangeStatusList.Codes.eAdaptorQueued; }
		}

		protected override string TransportType
		{
			get { return EDIInterchangeTransportTypeList.Codes.eAdaptor; }
		}

		public ZGuid RequestMessagePK { get; set; }

		protected override void ExecuteExtraInitializationForMessage(IEDIMessage message)
		{
			message.EM_EM_RequestMessage = RequestMessagePK;
		}
	}
}
