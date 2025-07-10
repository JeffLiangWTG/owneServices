using CargoWise.Customs.TW.MessageDefinitions.NX102;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.TW.Business.MessageProcessors
{
	public class NX102MessageHelper : TWMessageHelper
	{
		public NX102MessageHelper(TWMessage message) : base(message)
		{
			response = Message.IncomingMessageKeyInfomation.Result as Response;
		}

		readonly Response response;

		protected override void WriteTable(HtmlTableCreator table)
		{
			if (response != null)
			{
				var declaration = response.Declaration;
				WriteRow(table, Titles.ProcessingNumber, response.AdditionalInformation?.TwProcessNumber?.Value ?? ZString.Empty);
				WriteRow(table, Titles.ValidationCode, response.AdditionalInformation?.StatementCode?.Value ?? ZString.Empty);
				WriteRow(table, Titles.DateAndTimeOfApplication, declaration.IssueDateTime);
				WriteRow(table, Titles.CompetentAuthority, declaration.ContactOffice?.TwName.Value ?? ZString.Empty);
				WriteRow(table, Titles.CertificateOfOriginNo, declaration.GoodsShipment.AdditionalDocument.Id.Value ?? ZString.Empty);
				WriteRow(table, Titles.OriginalMessageIdentifier, declaration.PreviousDocument?.TwFunctionalReferenceId?.Value ?? ZString.Empty);
				WriteRow(table, Titles.TypeOfApplicationCoded, GetNX102_TypeOfApplicationCodedDescription(declaration.TwApplication?.TwTypeCode?.Value ?? ZString.Empty));
				var responseCode = response.Status?.NameCode?.Value ?? ZString.Empty;
				WriteRow(table, Titles.AuditResultCoded, GetResponseCode(responseCode));
				WriteRow(table, Titles.ErrorConditionOverrideCoded, GetNX102_CodeofErrorConditionOverrideCodeList(response.Error?.ValidationCode?.Value ?? ZString.Empty));
				WriteRow(table, Titles.Description, declaration.AdditionalInformation?.Content?.Value ?? ZString.Empty);
			}
		}

		ZString GetNX102_CodeofErrorConditionOverrideCodeList(ZString code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.CPT_118_NX102_ErrorConditionOverrideCodeList.GetDescriptionFromCode(itemCode));
		}

		ZString GetNX102_TypeOfApplicationCodedDescription(ZString code)
		{
			return GetDescriptionFromCode(code, (itemCode) => Lookups.TypeOfApplicationCode_FromGlobalCodes.GetDescriptionFromCode(itemCode));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Title strings")]
		sealed class Titles
		{
			internal const string ProcessingNumber = "收件編號Processing number";
			internal const string ValidationCode = "驗證碼Validation Code";
			internal const string OriginalMessageIdentifier = "原訊息編號Original message identifier";
			internal const string TypeOfApplicationCoded = "申請業務別Type of application, coded";
			internal const string CertificateOfOriginNo = "產地證明書號碼Certificate of origin No.";
			internal const string DateAndTimeOfApplication = "申請日期與時間Date and time of application";
			internal const string CompetentAuthority = "產證簽發單位Competent authority";
			internal const string AuditResultCoded = "審核結果Audit result, coded";
			internal const string ErrorConditionOverrideCoded = "不合格原因代碼Error condition override, coded";
			internal const string Description = "說明Description";
		}
	}
}
