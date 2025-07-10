using CargoWise.Customs.TW.MessageDefinitions.NX902;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.MessageProcessors;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.TW.Business
{
	public class NX902MessageHelper : TWMessageHelper
	{
		public NX902MessageHelper(TWMessage message, BusinessObject bizObj) : base(message)
		{
			response = Message.IncomingMessageKeyInfomation.Result as Response;
			header = bizObj as CusTWControllingMessageHeader;
		}

		readonly Response response;
		readonly CusTWControllingMessageHeader header;

		protected override void WriteTable(HtmlTableCreator table)
		{
			if (response != null)
			{
				var declaration = response.Declaration;
				WriteRow(table, Titles.IssueDateTime, response.IssueDateTime);
				WriteRow(table, Titles.OfficeOfDeclaration, GetPROURefCusCodeList(response.ContactOffice?.Id?.Value ?? ZString.Empty));
				WriteRow(table, Titles.NotificationCode, GetNX902_NotificationCodeList(response.Status?.NameCode?.Value ?? ZString.Empty));
				WriteRow(table, Titles.SamplingPeriod, GetAppointmentPeriodList(response.TwSampling?.TwPeriodCode?.Value ?? ZString.Empty));
				WriteRow(table, Titles.SamplingDate, response.TwSampling?.TwSamplingDate?.ToString("yyyy-MM-dd") ?? ZString.Empty);
				WriteRow(table, Titles.ProcessingDateAndTime, declaration?.AcceptanceDateTime ?? ZString.Empty);
				WriteRow(table, Titles.MessageFunctionCoded, declaration?.FunctionCode?.Value ?? ZString.Empty);
				WriteRow(table, Titles.GoodsDeclarationNumber, declaration?.Id?.Value ?? ZString.Empty);
				WriteRow(table, Titles.Description, declaration?.AdditionalInformation?.Content?.Value ?? ZString.Empty);
				WriteRow(table, Titles.ProcessingNumber, declaration?.AdditionalInformation?.TwProcessNumber?.Value ?? ZString.Empty);
				WriteRow(table, Titles.CustomsBrokerBoxNo, declaration?.Agent?.Id?.Value ?? ZString.Empty);
				WriteRow(table, Titles.CustomsBrokerSubBoxNo, declaration?.Agent?.TwSubBoxId?.Value ?? ZString.Empty);
				WriteRow(table, Titles.PaymentFee, declaration?.DutyTaxFee?.Payment?.PaymentAmount?.Value ?? 0m);
				WriteRow(table, Titles.PaymentReferenceNumber, declaration?.DutyTaxFee?.Payment?.ReferenceId?.Value ?? ZString.Empty);
				WriteRow(table, Titles.ImportOrExportPermitNo, declaration?.GoodsShipment?.AdditionalDocument?.Id?.Value ?? ZString.Empty);
				WriteRow(table, Titles.ApplicationStatusCoded, GetCPT_111_201_ApplicationType(declaration?.GovernmentProcedure?.CurrentCode?.Value ?? ZString.Empty));
				WriteRow(table, Titles.OriginalMessageIdentifier, declaration?.PreviousDocument?.TwFunctionalReferenceId?.Value ?? ZString.Empty);
				WriteRow(table, Titles.AgencyCoded, GetTWCARefCusCodeList(declaration?.ResponsibleGovernmentAgency?.Id?.Value ?? ZString.Empty));
				WriteRow(table, Titles.TypeOfApplicationCoded, GetNX902_TypeOfApplicationCodeList(declaration?.TwApplication?.TwTypeCode?.Value ?? ZString.Empty));
				WriteRow(table, Titles.ApplicationAgent_BAN_ID_PassportNo, declaration?.TwApplication?.Agent?.Id?.Value ?? ZString.Empty);
				WriteRow(table, Titles.Applicant_BAN_ID_PassportNo, declaration?.TwApplication?.TwApplicant?.TwId?.Value ?? ZString.Empty);
			}
		}

		ZString GetTWCARefCusCodeList(ZString code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.TWCARefCusCodeList.GetDescriptionFromCode(itemCode));
		}

		ZString GetCPT_111_201_ApplicationType(ZString code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.CPT_111_201_ApplicationType.GetDescriptionFromCode(itemCode));
		}

		ZString GetNX902_NotificationCodeList(ZString code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.CPT_119_902_NotificationCodeList.GetDescriptionFromCode(itemCode));
		}

		ZString GetAppointmentPeriodList(ZString code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.AppointmentPeriodList.GetDescriptionFromCode(itemCode));
		}

		ZString GetNX902_TypeOfApplicationCodeList(ZString code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.GetTypeOfApplicationCodeList(header.TW1_ControllingMessageType).GetDescriptionFromCode(itemCode));
		}

		sealed class Titles
		{
			#region SuppressResourceStringsCheckRegion
			internal const string IssueDateTime = "通知日期與時間Issue date time";
			internal const string OfficeOfDeclaration = "受理單位Office of declaration";
			internal const string NotificationCode = "通知代碼Notification code";
			internal const string SamplingPeriod = "取樣時段Sampling period";
			internal const string SamplingDate = "取樣日期Sampling date";
			internal const string ProcessingDateAndTime = "收件日期與時間Processing date and time";
			internal const string MessageFunctionCoded = "訊息功能代碼Message function, coded";
			internal const string GoodsDeclarationNumber = "報單號碼Goods declaration number";
			internal const string Description = "說明Description";
			internal const string ProcessingNumber = "收件編號Processing number";
			internal const string CustomsBrokerBoxNo = "報關業者箱號Customs broker box No.";
			internal const string CustomsBrokerSubBoxNo = "報關業者箱號附碼Customs broker sub-box No.";
			internal const string PaymentFee = "應繳費用Payment fee";
			internal const string PaymentReferenceNumber = "繳費號碼Payment reference number";
			internal const string ImportOrExportPermitNo = "輸出入許可文件號碼Import or export permit No.";
			internal const string ApplicationStatusCoded = "申辦現況代碼Application status, coded";
			internal const string OriginalMessageIdentifier = "原訊息編號Original message identifier";
			internal const string AgencyCoded = "機關別代碼Agency, coded";
			internal const string TypeOfApplicationCoded = "申請業務別Type of application, coded";
			internal const string ApplicationAgent_BAN_ID_PassportNo = "報驗/申辦代理人統一編號Application agent BAN/ID/passport No.";
			internal const string Applicant_BAN_ID_PassportNo = "申請人統一編號Applicant BAN/ID/passport No.";
			#endregion
		}
	}
}
