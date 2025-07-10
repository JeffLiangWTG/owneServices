using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public class SendICRFromConsol : SendICR
	{
		public SendICRFromConsol(ForwardingConsol consol, IAdditionalInformation additionalMessageInformation, ICRManifestStatus manifestStatus, TSWTransactionTypes type)
			: base(consol, additionalMessageInformation, type)
		{
			this.manifestStatus = manifestStatus;
		}
		readonly ICRManifestStatus manifestStatus;

		ForwardingConsol Consol => (ForwardingConsol)HostEntity;

		public override ZString ApplicationReference => Consol.JK_UniqueConsignRef;

		protected override void SetStatusOnSuccess(StatusTransactionScope scope)
		{
			scope.Add(manifestStatus.E2_MessageStatusInfo);
			manifestStatus.E2_MessageStatus = LowValueManifestStatusList.Codes.SentToCustoms;
		}

		protected override void AddMessageToMessages(TSWMessage message) => Consol.Messages.Add(message);

		protected override ZString MessageSubType
		{
			get
			{
				var result = string.Empty;
				switch (TransactionType)
				{
					case TSWTransactionTypes.Original:
						result = NZCMessage.MessageTypes.OutwardReport.MessageSubTypes.Original;
						break;
					case TSWTransactionTypes.Cancel:
						result = NZCMessage.MessageTypes.OutwardReport.MessageSubTypes.Cancellation;
						break;
					case TSWTransactionTypes.Replace:
						result = NZCMessage.MessageTypes.OutwardReport.MessageSubTypes.Replacement;
						break;
				}

				return result;
			}
		}

		protected override void CheckErrorsBeforeGeneratingMessageCore()
		{
			errorList.AddRange(ICRValidation.CheckErrorsBeforeGeneratingMessage());
		}

		protected override ICRMessageBuilder GetICRMessageBuilder() => new ICRMessageBuilder(new ICRConsolWrapper(Consol, AdditionalMessageInformation), TransactionType, SubmitterCode);

		ICRValidation ICRValidation => icrValidation ?? (icrValidation = new ICRValidation(HostEntity, TransactionType, manifestStatus));
		ICRValidation icrValidation;
	}
}
