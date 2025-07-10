using CargoWise.Types;
using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
using Enterprise.Customs.NZ.Business.MAFeBACCa.XMLSchemas;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageProcessors
{
	class NotificationUpdaterCancellation : NotificationUpdater
	{
		public NotificationUpdaterCancellation(EBACCANotificationType notification, ZStringBuilder emailBody)
			: base(notification, emailBody)
		{
		}

		protected override string ResponseTypeCode
		{
			get { return NZMMessage.MessageTypes.Receive.MessageSubTypes.Cancellation; }
		}

		internal override string ResponseTypeDescription
		{
			get { return Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists.MessageSubTypeList.Descriptions.Cancellation; }
		}

		internal override void UpdateStatusAndReferences(MAFMessagingBO mafMessaging)
		{
			UpdateStatusAndReferences(mafMessaging, MessagingStatusList.Codes.CancelledByMpi);
			mafMessaging.ZX_ReceiptNumber = "";
			mafMessaging.ZX_ConsignmentNumber = "";
		}

		internal override string GetResponseInstruction()
		{
			return "eBACCa was Cancelled by MPI.";
		}
	}
}
