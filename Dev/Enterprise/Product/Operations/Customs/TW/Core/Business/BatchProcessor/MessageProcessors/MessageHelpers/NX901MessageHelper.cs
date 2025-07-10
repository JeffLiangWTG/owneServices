using CargoWise.Common;
using CargoWise.Customs.TW.MessageDefinitions.NX901;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.TW.Business.MessageProcessors
{
	public class NX901MessageHelper : TWMessageHelper
	{
		public NX901MessageHelper(TWMessage message) : base(message)
		{
			response = Message.IncomingMessageKeyInfomation.Result as Response;
		}

		readonly Response response;

		protected override void WriteTable(HtmlTableCreator table)
		{
			if (response != null)
			{
				var declaration = response.Declaration;
				var goodsShipment = declaration?.GoodsShipment;
				var agencyCode = declaration?.ResponsibleGovernmentAgency?.Id?.Value ?? ZString.Empty;
				WriteRow(table, Titles.IssueDateTime, response.IssueDateTime);
				WriteRow(table, Titles.ProcessingUnit, response.ContactOffice?.Id?.Value ?? ZString.Empty);
				response.Status?.ForEach(x => WriteRow(table, Titles.ResponseCode, GetGovernmentAgencyResponseCodeList(agencyCode, x.NameCode?.Value ?? ZString.Empty)));
				WriteRow(table, Titles.AcceptanceDateTime, declaration?.AcceptanceDateTime ?? ZString.Empty);
				WriteRow(table, Titles.FunctionCode, declaration?.FunctionCode?.Value ?? ZString.Empty);
				WriteRow(table, Titles.ReferenceCode, declaration?.Id?.Value ?? ZString.Empty);
				WriteRow(table, Titles.ProcessNumber, declaration?.AdditionalInformation?.TwProcessNumber?.Value ?? ZString.Empty);
				WriteRow(table, Titles.Content, declaration?.AdditionalInformation?.Content?.Value ?? ZString.Empty);
				WriteRow(table, Titles.AgentBoxNumber, declaration?.Agent?.Id?.Value ?? ZString.Empty);
				WriteRow(table, Titles.AgentSubBoxNumber, declaration?.Agent?.TwSubBoxId?.Value ?? ZString.Empty);
				WriteRow(table, Titles.PermitNumber, goodsShipment?.AdditionalDocument?.Id?.Value ?? ZString.Empty);
				WriteRow(table, Titles.Remark, goodsShipment?.AdditionalInformation?.StatementDescription?.Value ?? ZString.Empty);
				goodsShipment?.GovernmentAgencyGoodsItem?.ForEach(item =>
				{
					WriteRow(table, Titles.GoodsItemNumber, item.SequenceNumeric.GetValueOrDefault());
					WriteRow(table, Titles.ItemNumberInPermit, item.AdditionalDocument?.TwSequenceNumeric ?? ZDecimal.Zero);
					WriteRow(table, Titles.Content, item.AdditionalInformation?.Content?.Value ?? ZString.Empty);
					item.Status?.ForEach(x => WriteRow(table, Titles.ResponseCode, GetGovernmentAgencyResponseCodeList(agencyCode, x.NameCode?.Value ?? ZString.Empty)));
				});
				WriteRow(table, Titles.OriginReferenceCode, declaration?.PreviousDocument?.TwFunctionalReferenceId?.Value ?? ZString.Empty);
				WriteRow(table, Titles.ControllingAgency, agencyCode);
				WriteRow(table, Titles.BusinessType, declaration?.TwApplication?.TwTypeCode?.Value ?? ZString.Empty);
				WriteRow(table, Titles.AgentID, declaration?.TwApplication?.Agent?.Id?.Value ?? ZString.Empty);
				WriteRow(table, Titles.ApplicantID, declaration?.TwApplication?.TwApplicant?.TwId?.Value ?? ZString.Empty);
			}
		}

		ZString GetGovernmentAgencyResponseCodeList(ZString agencyCode, ZString responseCode)
		{
			return GetDescriptionFromCode(responseCode, (itemCode) => Lookups.GetGovernmentAgencyResponseCodeList(agencyCode).GetDescriptionFromCode(itemCode));
		}

		sealed class Titles
		{
			#region SuppressResourceStringsCheckRegion
			internal const string IssueDateTime = "通知日期與時間Issue date time";
			internal const string ProcessingUnit = "受理單位Processing unit";
			internal const string ResponseCode = "回應狀況代碼Response code";
			internal const string AcceptanceDateTime = "收件日期與實踐Acceptance date time";
			internal const string FunctionCode = "訊息功能代碼Function code";
			internal const string ReferenceCode = "報單號碼Reference code";
			internal const string ProcessNumber = "收件編號Process number";
			internal const string Content = "說明Content";
			internal const string AgentBoxNumber = "報關業者箱號Agent box number";
			internal const string AgentSubBoxNumber = "報關業者箱號附碼Agent sub-box number";
			internal const string PermitNumber = "輸出入許可文件號碼Permit number";
			internal const string Remark = "備註Remark";
			internal const string GoodsItemNumber = "項次Goods item number";
			internal const string ItemNumberInPermit = "輸出入許可文文件項次Item Number in permit";
			internal const string OriginReferenceCode = "原訊息編號Origin reference code";
			internal const string ControllingAgency = "機關別代碼Controlling agency";
			internal const string BusinessType = "申請業務別Business type";
			internal const string AgentID = "報驗/申辦代理人統一編號Agent ID";
			internal const string ApplicantID = "申請人統一編號Applicant ID";
			#endregion
		}
	}
}
