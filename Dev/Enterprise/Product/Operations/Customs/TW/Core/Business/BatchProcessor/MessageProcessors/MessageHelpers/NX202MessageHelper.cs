using CargoWise.Customs.TW.MessageDefinitions.NX202;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.TW.Business.MessageProcessors
{
	public class NX202MessageHelper : TWMessageHelper
	{
		public NX202MessageHelper(TWMessage message) : base(message)
		{
			response = Message.IncomingMessageKeyInfomation.Result as Response;
		}

		readonly Response response;

		protected override void WriteTable(HtmlTableCreator table)
		{
			if (response != null)
			{
				var declaration = response.Declaration;
				WriteRow(table, Titles.DocumentIssueDate, response.IssueDateTime);
				WriteRow(table, Titles.ImportOrExportPermitNo, response.AdditionalDocument?.Id?.Value ?? ZString.Empty);
				WriteRow(table, Titles.DocumentExpirationDateTime, response.AdditionalDocument?.LpcoExpirationDateTime ?? ZString.Empty);
				WriteRow(table, Titles.SpecialCondition, GetCPT_121_CodeOfSpecialConditionCodeList(response.AdditionalInformation?.StatementCode?.Value ?? ZString.Empty));
				WriteRow(table, Titles.ProcessingNumber, response.AdditionalInformation?.TwProcessNumber?.Value ?? ZString.Empty);
				WriteRow(table, Titles.AuditResultCoded, GetCPT_120_202_ResultCodeList(response.Status?.NameCode?.Value ?? ZString.Empty));
				WriteRow(table, Titles.MessageFunctionCoded, declaration?.FunctionCode?.Value ?? ZString.Empty);
				WriteRow(table, Titles.DateAndTimeOfApplication, declaration?.IssueDateTime ?? ZString.Empty);
				WriteRow(table, Titles.DeclarationDescription, declaration?.AdditionalInformation?.Content?.Value ?? ZString.Empty);
				WriteRow(table, Titles.DepartureDate, declaration?.GoodsShipment?.ExitDateTime ?? ZString.Empty);
				WriteRow(table, Titles.OriginalMessageIdentifier, declaration?.PreviousDocument?.TwFunctionalReferenceId?.Value ?? ZString.Empty);
				WriteRow(table, Titles.CustomsMessageIdentifier, declaration?.TwAdditionalDeclaration?.TwId?.Value ?? ZString.Empty);
				WriteRow(table, Titles.TypeOfApplicationCoded, GetNX202TypeOfApplicationCodeList(declaration?.TwApplication?.TwTypeCode?.Value ?? ZString.Empty));
				WriteRow(table, Titles.ApplicationAgentBAN_ID_PassportNo, declaration?.TwApplication?.Agent?.Id?.Value ?? ZString.Empty);
				WriteRow(table, Titles.ApplicantBAN_ID_PassportNo, declaration?.TwApplication?.TwApplicant?.TwId?.Value ?? ZString.Empty);
			}
		}

		ZString GetCPT_121_CodeOfSpecialConditionCodeList(ZString code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.CPT_121_CodeOfSpecialConditionCodeList.GetDescriptionFromCode(itemCode));
		}

		ZString GetCPT_120_202_ResultCodeList(ZString code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.CPT_120_202_ResultCodeList.GetDescriptionFromCode(itemCode));
		}

		ZString GetNX202TypeOfApplicationCodeList(ZString code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.NX202TypeOfApplicationCodeList.GetDescriptionFromCode(itemCode));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Title strings")]
		sealed class Titles
		{
			internal const string DocumentIssueDate = "文件核發日期Document issue date";
			internal const string ImportOrExportPermitNo = "輸出入許可文件號碼Import or export permit No.";
			internal const string DocumentExpirationDateTime = "文件有效日期Document expiration date";
			internal const string SpecialCondition = "加註有關規定Special condition";
			internal const string ProcessingNumber = "收件編號Processing number";
			internal const string AuditResultCoded = "審核結果Audit result, coded";
			internal const string MessageFunctionCoded = "訊息功能代碼Message function, coded";
			internal const string DateAndTimeOfApplication = "申請日期與時間Date and time of application";
			internal const string DeclarationDescription = "說明Description";
			internal const string DepartureDate = "出口日期Departure date";
			internal const string OriginalMessageIdentifier = "原訊息編號Original message identifier";
			internal const string CustomsMessageIdentifier = "會辦識別碼Customs message identifier";
			internal const string TypeOfApplicationCoded = "申請業務別Type of application, coded";
			internal const string ApplicationAgentBAN_ID_PassportNo = "報驗/申辦代理人統一編號Application agent BAN/ID/passport No.";
			internal const string ApplicantBAN_ID_PassportNo = "申請人統一編號Applicant BAN/ID/passport No.";
		}
	}
}
