using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;

namespace Enterprise.Customs.NZ.Manifest.Business
{
	public class SendICRFromManifestHeader : NZ.Business.TradeSingleWindow.SendICR
	{
		public SendICRFromManifestHeader(AsycudaManifestHeader manifestHeader, IAdditionalInformation additionalMessageInformation, TSWTransactionTypes transactionType)
			: base(manifestHeader, additionalMessageInformation, transactionType)
		{
		}

		AsycudaManifestHeader ManifestHeader => (AsycudaManifestHeader)HostEntity;

		public ZString TransactionTypeName => TransactionType.ToString();

		#region overrides
		public override ZString ApplicationReference => ManifestHeader.AMA_JobReference;

		protected override void AddMessageToMessages(TSWMessage message) => ManifestHeader.Messages.Add(message);

		protected override void CheckErrorsBeforeGeneratingMessageCore()
		{
			ManifestHeader.LoadChildEditableObjects();
			using (((IBusinessObjectInternals)ManifestHeader).ResumeValidationForAllDescendantsTemporarily())
			{
				ManifestHeader.RunPreSaveValidation();
			}

			if (ManifestHeader.HasErrors)
			{
				var errorCollector = new Customs.Business.CustomsNotificationCollector(ManifestHeader, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors();
				errorList.Add(errorCollector.ToUniqueMessageListString() + System.Environment.NewLine);
			}

			if (ManifestHeader.HasMessageErrors)
			{
				var messageErrorCollector = new Customs.Business.CustomsNotificationCollector(ManifestHeader, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors();
				errorList.Add(messageErrorCollector.ToUniqueMessageListString());
			}
		}

		protected override void SetStatusOnSuccess(StatusTransactionScope scope)
		{
			scope.Add(ManifestHeader.AMA_MessageStatusInfo);
			ManifestHeader.AMA_MessageStatus = NZMessageStatusList.Codes.Sent;
			ManifestHeader.Bills.Cast<AsycudaBill>().ForEach(x =>
			{
				scope.Add(x.ABL_MessageStatusInfo);
				x.ABL_MessageStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			});
		}

		protected override ICRMessageBuilder GetICRMessageBuilder() => new ICRMessageBuilder(new ICRManifestHeaderWrapper(ManifestHeader, AdditionalMessageInformation), TransactionType, SubmitterCode);

		#endregion
	}
}
