using CargoWise.Types;
using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
using Enterprise.Customs.NZ.Business.MAFeBACCa.XMLSchemas;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageProcessors
{
	class NotificationUpdaterNotifyCRN : NotificationUpdater
	{
		public NotificationUpdaterNotifyCRN(EBACCANotificationType notification, ZStringBuilder emailBody)
			: base(notification, emailBody)
		{
		}

		protected override string ResponseTypeCode
		{
			get { return NZMMessage.MessageTypes.Receive.MessageSubTypes.NotifyCRN; }
		}

		internal override void UpdateStatusAndReferences(MAFMessagingBO mafMessaging)
		{
			UpdateStatusAndReferences(mafMessaging, MessagingStatusList.Codes.CrnReceived);
		}

		internal override string ResponseTypeDescription
		{
			get { return Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists.MessageSubTypeList.Descriptions.NotifyCRN; }
		}

		internal override string GetResponseInstruction()
		{
			return "CRN issued for eBACCa by MPI.";
		}
	}
}
