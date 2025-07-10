using System;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
using Enterprise.Customs.NZ.Business.MAFeBACCa.XMLSchemas;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageProcessors
{
	class NotificationUpdaterError : NotificationUpdater
	{
		public NotificationUpdaterError(EBACCANotificationType notification, ZStringBuilder emailBody)
			: base(notification, emailBody)
		{
		}

		protected override string ResponseTypeCode
		{
			get { return NZMMessage.MessageTypes.Receive.MessageSubTypes.ErrorMessage; }
		}

		internal override void UpdateStatusAndReferences(MAFMessagingBO mafMessaging)
		{
			UpdateStatusAndReferences(mafMessaging, IsDuplicateError ? mafMessaging.ZX_MessagingStatus : new ZString(MessagingStatusList.Codes.Error));
		}

		bool IsDuplicateError
		{
			get { return Notification.Description.StartsWith("Duplicate application detected. Original application receipt number:", StringComparison.InvariantCultureIgnoreCase); }
		}

		internal override string ResponseTypeDescription
		{
			get { return Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists.MessageSubTypeList.Descriptions.ErrorMessage; }
		}

		internal override string GetResponseInstruction()
		{
			return IsDuplicateError
				? "Duplicate eBACCa messages were received by MPI with the same Receipt #. The second copy sent has been ignored."
				: "Errors were found by MPI processing sent eBACCa message.";
		}
	}
}
