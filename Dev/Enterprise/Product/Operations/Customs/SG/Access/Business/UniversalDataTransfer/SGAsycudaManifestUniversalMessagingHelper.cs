using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;

namespace Enterprise.Customs.SG.Access.Business.UniversalDataTransfer
{
	public class SGAsycudaManifestUniversalMessagingHelper : AsycudaManifestUniversalMessagingHelper
	{
		public SGAsycudaManifestUniversalMessagingHelper(INotifications notifications, ICycleDetailSupporter cycleDetailSupporter)
			: base(notifications)
		{
			this.cycleDetailSupporter = cycleDetailSupporter;
		}
		readonly ICycleDetailSupporter cycleDetailSupporter;
		ZString headerOldMessagingStatus;
		ZString headerOldCustomsStatus;

		protected override void SetMessageStatusOnSend(ASYCUDA.Business.AsycudaManifestHeader header, IList<IMessageParent> messageParents, ZString messageSubType)
		{
			base.SetMessageStatusOnSend(header, messageParents, messageSubType);
			if (messageSubType == MessageSubTypeCodes.Codes.Cancellation || messageSubType == MessageSubTypeCodes.Codes.CancelManifest)
			{
				foreach (var messageParent in messageParents)
				{
					messageParent.SetNewCountryCustomsStatus(Constants.CustomsStatusCode.Cancelled);
				}
			}
			var messagingProvider = header.ApplicationBusinessProvider.MessagingProvider;
			headerOldMessagingStatus = header.SetNewCountryMessagingStatus(messagingProvider.GetMostSevereValueMessageStatus(header));
			headerOldCustomsStatus = header.SetNewCountryCustomsStatus(messagingProvider.GetMostSevereValueCustomsStatus(header));
		}

		protected override void UndoSetMessageStatusOnSend(ASYCUDA.Business.AsycudaManifestHeader header, IList<IMessageParent> messageParents, ZString messageSubType)
		{
			base.UndoSetMessageStatusOnSend(header, messageParents, messageSubType);
			header.SetNewCountryMessagingStatus(headerOldMessagingStatus);
			header.SetNewCountryCustomsStatus(headerOldCustomsStatus);
		}

		protected override string CalculateMessageStatus(IMessageParent messageParent) => MessageStatusCodeList.Codes.Sent;

		protected override void AddAdditionalDetails(Shipment headerData)
		{
			base.AddAdditionalDetails(headerData);
			headerData.SetAddInfoCollection(() =>
			{
				var list = headerData.AddInfoCollection ?? new List<AddInfo>();
				if (cycleDetailSupporter?.RequiresCycleFields ?? ZBool.False)
				{
					list.Add(new AddInfo { Key = AddInfoConstants.Header.CycleDate, Value = cycleDetailSupporter?.CycleDate.ToISO8601String() });
					list.Add(new AddInfo { Key = AddInfoConstants.Header.CycleNumber, Value = cycleDetailSupporter?.CycleNumber.ToString() });
				}
				return list;
			});
		}

		protected override ICodeDescriptionDataObject GetActionPurpose(string countryCode, string messageType, string messageSubType)
		{
			return GetSGAccessActionPurpose(messageType, messageSubType);
		}

		protected override IEnumerable<RecipientRoleDetail> GetRecipientRoles()
		{
			return new[] { new RecipientRoleDetail { Type = RecipientRoleType.SGA } };
		}

		internal static ICodeDescriptionDataObject GetSGAccessActionPurpose(string messageType, string messageSubType)
		{
			string code;
			string description;
			if (messageType == Constants.ManifestType.Export)
			{
				description = Constants.ActionPurpose.ACCESSExport.Description;
				switch (messageSubType)
				{
					case MessageSubTypeCodes.Codes.Original:
						code = Constants.ActionPurpose.ACCESSExport.Original;
						break;
					case MessageSubTypeCodes.Codes.Change:
						code = Constants.ActionPurpose.ACCESSExport.Change;
						break;
					case MessageSubTypeCodes.Codes.Cancellation:
						code = Constants.ActionPurpose.ACCESSExport.Cancellation;
						break;
					case MessageSubTypeCodes.Codes.CancelManifest:
						code = Constants.ActionPurpose.ACCESSExport.CancelManifest;
						break;
					default:
						code = MessageSubTypeCodes.Codes.Undefined;
						break;
				}
			}
			else
			{
				description = Constants.ActionPurpose.ACCESSImport.Description;
				switch (messageSubType)
				{
					case MessageSubTypeCodes.Codes.Original:
						code = Constants.ActionPurpose.ACCESSImport.Original;
						break;
					case MessageSubTypeCodes.Codes.Cancellation:
						code = Constants.ActionPurpose.ACCESSImport.Cancellation;
						break;
					case MessageSubTypeCodes.Codes.CancelManifest:
						code = Constants.ActionPurpose.ACCESSImport.CancelManifest;
						break;
					default:
						code = MessageSubTypeCodes.Codes.Undefined;
						break;
				}
			}
			return new CodeDescriptionPair { Code = code, Description = description };
		}
	}
}
