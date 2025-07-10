using CargoWise.Types;
using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
using Enterprise.Customs.NZ.Business.MAFeBACCa.XMLSchemas;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageProcessors
{
	class NotificationUpdaterRequestMoreInfo : NotificationUpdater
	{
		public NotificationUpdaterRequestMoreInfo(EBACCANotificationType notification, ZStringBuilder emailBody)
			: base(notification, emailBody)
		{
		}

		protected override string ResponseTypeCode
		{
			get { return NZMMessage.MessageTypes.Receive.MessageSubTypes.RequestMoreInfo; }
		}

		internal override void UpdateStatusAndReferences(MAFMessagingBO mafMessaging)
		{
			UpdateStatusAndReferences(mafMessaging, MessagingStatusList.Codes.MoreInformationRequired);
		}

		internal override string ResponseTypeDescription
		{
			get { return Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists.MessageSubTypeList.Descriptions.RequestMoreInfo; }
		}

		internal override string GetResponseInstruction()
		{
			return "MPI require more information / documentation to process this eBACCa.";
		}
	}
}
