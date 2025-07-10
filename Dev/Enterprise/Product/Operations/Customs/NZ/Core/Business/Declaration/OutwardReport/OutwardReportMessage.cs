using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.MessageBuilders.OutwardReport;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NZ.Business.Declaration.OutwardReport
{
	public class OutwardReportMessage : NZCMessage
	{
		public OutwardReportMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		public override void OnSaving()
		{
			base.OnSaving();

			if (EM_MessageSubType == MessageTypes.OutwardReport.MessageSubTypes.Original)
			{
				Logs.AddNew(Events.DeclarationQueued);
			}
			else if (EM_MessageSubType == MessageTypes.OutwardReport.MessageSubTypes.Cancellation)
			{
				Logs.AddNew(Events.DeclarationCancellationQueued);
			}
			else
			{
				Logs.AddNew(Events.DeclarationAmendmentQueued);
			}
		}

		public void SetMessageSubType(MessageBuilder.MessageTypes messageType)
		{
			switch (messageType)
			{
				case MessageBuilder.MessageTypes.Cancellation:
					EM_MessageSubType = OutwardReportMessage.MessageTypes.OutwardReport.MessageSubTypes.Cancellation;
					break;
				case MessageBuilder.MessageTypes.Original:
					EM_MessageSubType = OutwardReportMessage.MessageTypes.OutwardReport.MessageSubTypes.Original;
					break;
				case MessageBuilder.MessageTypes.Replacement:
					EM_MessageSubType = OutwardReportMessage.MessageTypes.OutwardReport.MessageSubTypes.Replacement;
					break;
			}
		}

		#endregion

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypes.OutwardReport.MessageType;
			EM_ApplicationCode = EDIMessage.ApplicationCodes.NewZealandCustoms;
		}

		protected override string GetSendersReference()
		{
			ForwardingConsol consol = (ForwardingConsol)EM_LinkedObject;
			consol.PopulateJK_UniqueConsignRefIfNeeded();
			return consol.JK_UniqueConsignRef.Replace("/", "");
		}

		#endregion

	}
}
