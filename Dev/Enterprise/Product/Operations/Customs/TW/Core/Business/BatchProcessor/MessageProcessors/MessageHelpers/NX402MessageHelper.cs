using CargoWise.Common;
using CargoWise.Customs.TW.MessageDefinitions.NX402;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.TW.Business.MessageProcessors
{
	public class NX402MessageHelper : TWMessageHelper
	{
		public NX402MessageHelper(TWMessage message) : base(message)
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
				WriteRow(table, Titles.ProcessingNumber, response.AdditionalInformation?.TwProcessNumber?.Value ?? ZString.Empty);
				WriteRow(table, Titles.OfficeOfDeclaration, GetPROURefCusCodeList(response.ContactOffice?.Id?.Value ?? ZString.Empty));
				WriteRow(table, Titles.AuditResultCoded, GetCPT_104_QuarantineResultCodeList(response.Status?.NameCode?.Value ?? ZString.Empty));
				WriteRow(table, Titles.GoodsDeclarationAcceptanceDate, declaration?.AcceptanceDateTime ?? ZString.Empty);
				WriteRow(table, Titles.GoodsDeclarationNumber, declaration?.Id?.Value ?? ZString.Empty);
				WriteRow(table, Titles.CustomsBrokerBoxNo, declaration?.Agent?.Id?.Value ?? ZString.Empty);
				WriteRow(table, Titles.CustomsBrokerSubboxNo, declaration?.Agent?.TwSubBoxId?.Value ?? ZString.Empty);
				WriteRow(table, Titles.ImportOrExportPermitNo, declaration?.GoodsShipment?.AdditionalDocument?.Id?.Value ?? ZString.Empty);
				WriteRow(table, Titles.ImportOrExportPermitSegmentNo, declaration?.GoodsShipment?.AdditionalDocument?.TwSplitId?.Value ?? ZString.Empty);
				WriteRow(table, Titles.ModeOfMeansOfTransportAtArrivalCoded, GetTransportCodeList(declaration?.GoodsShipment?.Consignment?.ArrivalTransportMeans?.TypeCode?.Value ?? ZString.Empty));
				var governmentAgencyGoodsItem = declaration?.GoodsShipment?.GovernmentAgencyGoodsItem;
				if (governmentAgencyGoodsItem != null)
				{
					foreach (var item in governmentAgencyGoodsItem)
					{
						WriteRow(table, Titles.GoodsItemNumber, item.SequenceNumeric);
						item.Status.ForEach(s =>
						{
							WriteRow(table, Titles.TheResultOfInspectionCoded, GetCPT_120_402_ResultCodeList(s.NameCode?.Value ?? ZString.Empty));
							WriteRow(table, Titles.Description, s.AdditionalInformation?.Content?.Value ?? ZString.Empty);
						});
					}
				}
				WriteRow(table, Titles.OriginalMessageIdentifier, declaration?.PreviousDocument?.TwFunctionalReferenceId?.Value ?? ZString.Empty);
				WriteRow(table, Titles.TypeOfApplicationCoded, GetNX401TypeOfApplicationCodeList(declaration?.TwApplication?.TwTypeCode?.Value ?? ZString.Empty));
				WriteRow(table, Titles.ApplicationAgentBAN_ID_PassportNo, declaration?.TwApplication?.Agent?.Id?.Value ?? ZString.Empty);
				WriteRow(table, Titles.PartyIdentifierCoded, declaration?.TwApplication?.Agent?.TwTypeCode?.Value ?? ZString.Empty);
			}
		}

		ZString GetCPT_104_QuarantineResultCodeList(ZString code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.CPT_104_QuarantineResultCodeList.GetDescriptionFromCode(itemCode));
		}

		ZString GetCPT_120_402_ResultCodeList(ZString code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.CPT_120_402_ResultCodeList.GetDescriptionFromCode(itemCode));
		}

		ZString GetNX401TypeOfApplicationCodeList(ZString code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.GetTypeOfApplicationCodeList(ControllingMessageTypeList.Codes.NX401).GetDescriptionFromCode(itemCode));
		}

		ZString GetTransportCodeList(ZString code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.TransportCodeList.GetDescriptionFromCode(itemCode));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Title strings")]
		sealed class Titles
		{
			internal const string MessageFunctionCoded = "訊息功能代碼Message function, coded";
			internal const string DateAndTimeOfNotification = "通知日期與時間Date and time of notification";
			internal const string ProcessingNumber = "收件編號Processing number";
			internal const string OfficeOfDeclaration = "受理單位Office of declaration";
			internal const string AuditResultCoded = "審核結果Audit result, coded";
			internal const string GoodsDeclarationAcceptanceDate = "報關日期Goods declaration acceptance date";
			internal const string GoodsDeclarationNumber = "報單號碼Goods declaration number";
			internal const string CustomsBrokerBoxNo = "報關業者箱號Customs broker box No.";
			internal const string CustomsBrokerSubboxNo = "報關業者箱號附碼Customs broker sub-box No.";
			internal const string ImportOrExportPermitNo = "輸出入許可文件號碼Import or export permit No.";
			internal const string ImportOrExportPermitSegmentNo = "輸出入許可文件分割號碼Import or export permit segment No.";
			internal const string ModeOfMeansOfTransportAtArrivalCoded = "進口運輸方式代碼Mode of means of transport at arrival, coded";
			internal const string GoodsItemNumber = "項次Goods item number";
			internal const string TheResultOfInspectionCoded = "簽審回覆代碼The result of inspection, coded";
			internal const string Description = "說明Description";
			internal const string OriginalMessageIdentifier = "原訊息編號Original message identifier";
			internal const string TypeOfApplicationCoded = "申請業務別Type of application, coded";
			internal const string ApplicationAgentBAN_ID_PassportNo = "報驗/申辦代理人統一編號Application agent BAN/ID/passport No.";
			internal const string PartyIdentifierCoded = "身分識別代碼Party identifier, coded";
		}
	}
}
