using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public class ICRMessage : TSWMessage
	{
		public ICRMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		public override void OnSaving()
		{
			base.OnSaving();

			if (EM_MessageSubType == MessageTypes.InwardCargoReport.MessageSubTypes.Original)
			{
				Logs.AddNew(Events.CargoReportSent);
			}
			else if (EM_MessageSubType == MessageTypes.InwardCargoReport.MessageSubTypes.Cancellation)
			{
				Logs.AddNew(Events.CargoReportWithdraw);
			}
		}

		public void SetMessageSubType(MessageBuilder.MessageTypes messageType)
		{
			switch (messageType)
			{
				case MessageBuilder.MessageTypes.Cancellation:
					EM_MessageSubType = ICRMessage.MessageTypes.InwardCargoReport.MessageSubTypes.Cancellation;
					break;
				case MessageBuilder.MessageTypes.Original:
					EM_MessageSubType = ICRMessage.MessageTypes.InwardCargoReport.MessageSubTypes.Original;
					break;
				case MessageBuilder.MessageTypes.Replacement:
					EM_MessageSubType = ICRMessage.MessageTypes.InwardCargoReport.MessageSubTypes.Replacement;
					break;
			}
		}

		#endregion

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypes.InwardCargoReport.MessageType;
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
