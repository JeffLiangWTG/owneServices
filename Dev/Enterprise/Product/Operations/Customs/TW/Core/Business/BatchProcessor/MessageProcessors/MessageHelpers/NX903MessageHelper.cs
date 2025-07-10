using CargoWise.Common;
using CargoWise.Customs.TW.MessageDefinitions.NX903;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.MessageProcessors;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.TW.Business
{
	public class NX903MessageHelper : TWMessageHelper
	{
		public NX903MessageHelper(TWMessage message) : base(message)
		{
			response = Message.IncomingMessageKeyInfomation.Result as Response;
		}

		readonly Response response;

		protected override void WriteTable(HtmlTableCreator table)
		{
			if (response != null)
			{
				var declaration = response.Declaration;
				var goodsShipmentPreviousDocument = declaration?.GoodsShipment?.GovernmentAgencyGoodsItem?.PreviousDocument;
				WriteRow(table, Titles.MessageIdentifier, response.FunctionalReferenceId?.Value ?? ZString.Empty);
				WriteRow(table, Titles.DateAndTimeOfNotification, response.IssueDateTime);
				WriteRow(table, Titles.NotifierIdentification, response.ContactOffice?.Id?.Value ?? ZString.Empty);
				response.Error.ForEach(x =>
				{
					WriteRow(table, Titles.ErrorTypeCoded, GetNX903_CodeOfErrorOrIrregularityNoticeCodeList(x.ValidationCode?.Value ?? ZString.Empty));
					WriteRow(table, Titles.Description, x.AdditionalInformation?.Content?.Value ?? ZString.Empty);
				});
				WriteRow(table, Titles.GoodsDeclarationNumber, declaration.Id?.Value ?? ZString.Empty);
				WriteRow(table, Titles.OriginalMessageIdentifier, goodsShipmentPreviousDocument?.TwFunctionalReferenceId?.Value ?? ZString.Empty);
				WriteRow(table, Titles.OriginalMessageUUID, goodsShipmentPreviousDocument?.TwUniversallyUniqueId?.Value ?? ZString.Empty);
				WriteRow(table, Titles.OriginalMessageType, goodsShipmentPreviousDocument?.TypeCode?.Value ?? ZString.Empty);
			}
		}

		ZString GetNX903_CodeOfErrorOrIrregularityNoticeCodeList(ZString code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.NX903_CodeOfErrorOrIrregularityNoticeCodeList.GetDescriptionFromCode(itemCode));
		}

		sealed class Titles
		{
			#region SuppressResourceStringsCheckRegion
			internal const string MessageIdentifier = "訊息編號Message identifier";
			internal const string DateAndTimeOfNotification = "通知日期與時間Date and time of notification";
			internal const string NotifierIdentification = "通知方代碼Notifier identification";
			internal const string ErrorTypeCoded = "錯誤種類代碼Error type, coded";
			internal const string Description = "說明Description";
			internal const string GoodsDeclarationNumber = "報單號碼Goods declaration number";
			internal const string OriginalMessageIdentifier = "原訊息編號Original message identifier";
			internal const string OriginalMessageUUID = "原訊息UUID Original message UUID";
			internal const string OriginalMessageType = "原訊息類別Original message type";
			#endregion
		}
	}
}
