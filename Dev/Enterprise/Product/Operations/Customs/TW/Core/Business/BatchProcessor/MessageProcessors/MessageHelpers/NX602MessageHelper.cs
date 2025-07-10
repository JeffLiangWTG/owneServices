using CargoWise.Common;
using CargoWise.Customs.TW.MessageDefinitions.NX602;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.TW.Business.MessageProcessors
{
	public class NX602MessageHelper : TWMessageHelper
	{
		public NX602MessageHelper(TWMessage message) : base(message)
		{
			response = Message.IncomingMessageKeyInfomation.Result as Response;
		}

		readonly Response response;

		protected override void WriteTable(HtmlTableCreator table)
		{
			if (response != null)
			{
				var declaration = response.Declaration;
				WriteRow(table, Titles.MessageFunctionCoded, response.FunctionCode?.Value ?? ZString.Empty);
				WriteRow(table, Titles.DateAndTimeOfNotification, response.IssueDateTime ?? ZString.Empty);
				WriteRow(table, Titles.AuditResultCoded, GetNX302AuditResultCoded(response.Status?.NameCode?.Value ?? ZString.Empty));
				WriteRow(table, Titles.GoodsDeclarationNumber, declaration?.Id?.Value ?? ZString.Empty);
				WriteRow(table, Titles.CustomsBrokerBoxNo, declaration?.Agent?.Id?.Value ?? ZString.Empty);
				WriteRow(table, Titles.CustomsBrokerSubboxNo, declaration?.Agent?.TwSubBoxId?.Value ?? ZString.Empty);
				WriteRow(table, Titles.ImportOrExportPermitNo, declaration?.GoodsShipment?.AdditionalDocument?.Id?.Value ?? ZString.Empty);
				var governmentAgencyGoodsItem = declaration?.GoodsShipment?.GovernmentAgencyGoodsItem;
				if (governmentAgencyGoodsItem != null)
				{
					foreach (var item in governmentAgencyGoodsItem)
					{
						WriteRow(table, Titles.GoodsItemNumber, item.SequenceNumeric);
						WriteRow(table, Titles.Description, item.AdditionalInformation?.Content?.Value ?? ZString.Empty);
						item.Error.ForEach(e => WriteRow(table, Titles.ErrorConditionOverrideCoded, GetCPT_118_602_CodeOfErrorConditionOverrideCodeList(e.ValidationCode?.Value ?? ZString.Empty)));
					}
				}
				WriteRow(table, Titles.OriginalMessageIdentifier, declaration?.PreviousDocument?.TwFunctionalReferenceId?.Value ?? ZString.Empty);
				WriteRow(table, Titles.TypeOfApplicationCoded, GetNX401TypeOfApplicationCodeList(declaration?.TwApplication?.TwTypeCode?.Value ?? ZString.Empty));
				WriteRow(table, Titles.ApplicationAgentBAN_ID_PassportNo, declaration?.TwApplication?.Agent?.Id?.Value ?? ZString.Empty);
				WriteRow(table, Titles.PartyIdentifierCoded, declaration?.TwApplication?.Agent?.TwTypeCode?.Value ?? ZString.Empty);
			}
		}

		ZString GetNX302AuditResultCoded(ZString code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.NX302AuditResultCoded.GetDescriptionFromCode(itemCode));
		}

		ZString GetCPT_118_602_CodeOfErrorConditionOverrideCodeList(ZString code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.CPT_118_602_CodeOfErrorConditionOverrideCodeList.GetDescriptionFromCode(itemCode));
		}

		ZString GetNX401TypeOfApplicationCodeList(ZString code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.GetTypeOfApplicationCodeList(MessageTypeList.Codes._602).GetDescriptionFromCode(itemCode));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Title strings")]
		sealed class Titles
		{
			internal const string MessageFunctionCoded = "訊息功能代碼Message function, coded";
			internal const string DateAndTimeOfNotification = "通知日期與時間Date and time of notification";
			internal const string AuditResultCoded = "審核結果Audit result, coded";
			internal const string GoodsDeclarationNumber = "報單號碼Goods declaration number";
			internal const string CustomsBrokerBoxNo = "報關業者箱號Customs broker box No.";
			internal const string CustomsBrokerSubboxNo = "報關業者箱號附碼Customs broker sub-box No.";
			internal const string ImportOrExportPermitNo = "輸出入許可文件號碼Import or export permit No.";
			internal const string GoodsItemNumber = "項次Goods item number";
			internal const string Description = "說明Description";
			internal const string ErrorConditionOverrideCoded = "不合格原因代碼Error condition override, coded";
			internal const string OriginalMessageIdentifier = "原訊息編號Original message identifier";
			internal const string TypeOfApplicationCoded = "申請業務別Type of application, coded";
			internal const string ApplicationAgentBAN_ID_PassportNo = "報驗/申辦代理人統一編號Application agent BAN/ID/passport No.";
			internal const string PartyIdentifierCoded = "身分識別代碼Party identifier, coded";
		}
	}
}
