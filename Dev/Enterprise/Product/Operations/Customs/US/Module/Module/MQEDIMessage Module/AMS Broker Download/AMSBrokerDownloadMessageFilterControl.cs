using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.Module
{
	partial class AMSBrokerDownloadMessageFilterControl : MQEDIMessageFilterControl
	{
		public AMSBrokerDownloadMessageFilterControl(AMSBrokerDownloadMQEDIMessageCollection gridCollection, AMSBrokerDownloadFilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject, GetColumnsToHide())
		{
			InitializeComponent();
		}

		static string[] GetColumnsToHide()
		{
			return new string[]
			{
				MQEDIMessage.Schema.EM_SendOrReceiveHumanReadable,
				MQEDIMessage.Schema.EM_ApplicationCode,
				MQEDIMessage.Schema.EM_ApplicationReference,
				MQEDIMessage.Schema.EM_MessageNum,
				MQEDIMessage.Schema.EM_MessageType,
				MQEDIMessage.Schema.EM_MessageSubType,
				MQEDIMessage.Schema.EM_MessageSubTypeDescription,
				MQEDIMessage.Schema.EM_User,
				MQEDIMessage.Schema.EM_InterchangeSender,
				MQEDIMessage.Schema.EM_InterchangeReceiver,
				MQEDIMessage.Schema.EM_SendingUser,
				MQEDIMessage.Schema.EM_SystemCreateUser,
				MQEDIMessage.Schema.EM_DateTimeInterchangeSent,
				MQEDIMessage.Schema.EM_InterchangeNumber,
				MQEDIMessage.Schema.EM_InterchangeStatus,
				MQEDIMessage.Schema.EM_SystemLastEditTimeUtc,
				MQEDIMessage.Schema.EM_SystemLastEditUser,
				MQEDIMessage.Schema.EM_Status
			};
		}
	}
}
