using System.Collections.Specialized;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business.MessageProcessors
{
	public class N5111MessageHelper : TWMessageHelper
	{
		public N5111MessageHelper(TWMessage message) : base(message)
		{
			response = Message.IncomingMessageKeyInfomation.Result as CargoWise.Customs.TW.MessageDefinitions.N5111.Response;
		}

		readonly CargoWise.Customs.TW.MessageDefinitions.N5111.Response response;

		protected override void WriteTable(HtmlTableCreator table)
		{
			if (response != null)
			{
				WriteRow(table, Titles.IssueDateTime, response.IssueDateTime);
				WriteRow(table, Titles.BankAccountID, response.BankAccount?.Id?.Value ?? ZString.Empty);
				WriteRow(table, Titles.BankAccountReferenceID, response.BankAccount?.ReferenceId?.Value ?? ZString.Empty);
				WriteRow(table, Titles.StatusNameCode, response.Status?.NameCode?.Value ?? ZString.Empty);

				var declaration = response.Declaration;

				var declarationOfficeId = declaration?.DeclarationOfficeId?.Value ?? ZString.Empty;
				WriteRow(table, Titles.DeclarationOfficeID, declarationOfficeId);
				var declarationOfficeIdDecs = ZString.Empty;
				if (declarationOfficeId != ZString.Empty)
				{
					var declarationOffice = TWRefCusCodeListLoader.GetCustomsOffice(Factory, declarationOfficeId, ZDateTime.Now);
					if (declarationOffice != null)
					{
						declarationOfficeIdDecs = declarationOffice.ZZD_Description;
					}
				}
				WriteRow(table, Titles.DeclarationOfficeIDDesc, declarationOfficeIdDecs);

				WriteRow(table, Titles.DeclarationID, declaration?.Id?.Value ?? ZString.Empty);
				WriteRow(table, Titles.AdditionalDocumentID, declaration?.AdditionalDocument?.Id?.Value ?? ZString.Empty);
				var agent = declaration?.Agent;
				WriteRow(table, Titles.AgentID, agent?.Id?.Value ?? ZString.Empty);
				WriteRow(table, Titles.AgentSubBoxID, agent?.TwSubBoxId?.Value ?? ZString.Empty);

				var dutyTaxFeePayment = declaration?.DutyTaxFee?.Payment;

				WriteRow(table, Titles.PaymentAmount, dutyTaxFeePayment?.PaymentAmount?.Value);
				WriteRow(table, Titles.PaymentReferenceID, dutyTaxFeePayment?.ReferenceId?.Value ?? ZString.Empty);
				WriteRow(table, Titles.PaymentBelongDate, dutyTaxFeePayment?.TwBelongDate?.Value ?? ZString.Empty);

				var paymentCollectionTypeCode = dutyTaxFeePayment?.TwCollectionTypeCode?.Value ?? ZString.Empty;
				WriteRow(table, Titles.PaymentCollectionTypeCode, paymentCollectionTypeCode);
				var paymentCollectionTypeCodeDesc = ZString.Empty;
				if (paymentCollectionTypeCode == CodeUnderTwentyThousands)
				{
					paymentCollectionTypeCodeDesc = PaymentCollectionTypeCodeDesc_6AW;
				}
				else if (paymentCollectionTypeCode == CodeOverTwentyThousands)
				{
					paymentCollectionTypeCodeDesc = PaymentCollectionTypeCodeDesc_6AX;
				}
				WriteRow(table, Titles.PaymentCollectionTypeCode, paymentCollectionTypeCodeDesc);

				var paymentDepositTypeCode = dutyTaxFeePayment?.TwDepositTypeCode?.Value ?? ZString.Empty;
				WriteRow(table, Titles.PaymentDepositTypeCode, paymentDepositTypeCode);
				WriteRow(table, Titles.PaymentDepositTypeCodeDesc, Factory.GetCachedValue<DepositTypeCodeList>().GetDescriptionFromCode(paymentDepositTypeCode));

				var paymentIssueReasonCode = dutyTaxFeePayment?.TwIssueReasonCode?.Value ?? ZString.Empty;
				WriteRow(table, Titles.PaymentIssueReasonCode, paymentIssueReasonCode);
				WriteRow(table, Titles.PaymentIssueReasonCodeDesc, Factory.GetCachedValue<ReasonOfPaymentList>().GetDescriptionFromCode(paymentIssueReasonCode));

				WriteRow(table, Titles.ObligationGuaranteeReferenceID, dutyTaxFeePayment?.ObligationGuarantee?.ReferenceId?.Value ?? ZString.Empty);

				var obligationGuaranteeSecurityDetailsCode = dutyTaxFeePayment?.ObligationGuarantee?.SecurityDetailsCode?.Value ?? ZString.Empty;
				WriteRow(table, Titles.ObligationGuaranteeSecurityDetailsCode, obligationGuaranteeSecurityDetailsCode);
				WriteRow(table, Titles.ObligationGuaranteeSecurityDetailsCodeDesc, Factory.GetCachedValue<ReasonOfDepositList>().GetDescriptionFromCode(obligationGuaranteeSecurityDetailsCode));

				WriteRow(table, Titles.ObligationGuaranteeSuretyID, dutyTaxFeePayment?.ObligationGuarantee?.Surety?.Id?.Value ?? ZString.Empty);
				WriteRow(table, Titles.ObligationGuaranteeSuretyName, dutyTaxFeePayment?.ObligationGuarantee?.Surety?.Name?.Value ?? ZString.Empty);
				WriteRow(table, Titles.ObligationGuaranteeSuretyChineseName, dutyTaxFeePayment?.ObligationGuarantee?.Surety?.TwChineseName?.Value ?? ZString.Empty);

				var obligationGuaranteeSuretyTypeCode = dutyTaxFeePayment?.ObligationGuarantee?.Surety?.TwTypeCode?.Value ?? ZString.Empty;
				WriteRow(table, Titles.ObligationGuaranteeSuretyTypeCode, obligationGuaranteeSuretyTypeCode);
				WriteRow(table, Titles.ObligationGuaranteeSuretyTypeCodeDesc, Factory.GetCachedValue<PartyIdentifierCodeList>().GetDescriptionFromCode(obligationGuaranteeSuretyTypeCode));

				WriteRow(table, Titles.ResponsibleGovernmentAgencyID, declaration?.ResponsibleGovernmentAgency?.Id?.Value ?? ZString.Empty);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no need")]
		public override string ToHtml()
		{
			var table = new HtmlTableCreator(new NameValueCollection { { (NoResString)"border", "0" }, { (NoResString)"cellpadding", "0" }, { "cellspacing", "0" }, { "style", "margin: 5pt;" } }) { EnableHTMLEncoding = false };
			WriteTable(table);
			return table?.ToHtml();
		}

		const string CodeUnderTwentyThousands = "6AW";
		const string CodeOverTwentyThousands = "6AX";

		#region SuppressResourceStringsCheckRegion

		const string PaymentCollectionTypeCodeDesc_6AW = "金額 2 萬元(含)以下";
		const string PaymentCollectionTypeCodeDesc_6AX = "金額 2 萬元以上";

		sealed class Titles
		{
			internal const string IssueDateTime = "核發日期";

			internal const string BankAccountID = "銷帳編號";

			internal const string BankAccountReferenceID = "海關帳戶";

			internal const string StatusNameCode = "通關方式";

			internal const string DeclarationOfficeID = "核發關別代碼";

			internal const string DeclarationOfficeIDDesc = "核發關別代碼描述";

			internal const string DeclarationID = "報單號碼";

			internal const string AdditionalDocumentID = "文件號碼";

			internal const string AgentID = "報關業者箱號";

			internal const string AgentSubBoxID = "報關業者箱號附碼";

			internal const string PaymentAmount = "金額";

			internal const string PaymentReferenceID = "存款收款書號碼";

			internal const string PaymentBelongDate = "款項所屬年月";

			internal const string PaymentCollectionTypeCode = "代收項目代碼";

			internal const string PaymentCollectionTypeCodeDesc = "代收項目代碼描述";

			internal const string PaymentDepositTypeCode = "存款種類代碼";

			internal const string PaymentDepositTypeCodeDesc = "存款種類代碼描述";

			internal const string PaymentIssueReasonCode = "核發原因代碼";

			internal const string PaymentIssueReasonCodeDesc = "核發原因代碼描述";

			internal const string ObligationGuaranteeReferenceID = "案號";

			internal const string ObligationGuaranteeSecurityDetailsCode = "押款原因代碼";

			internal const string ObligationGuaranteeSecurityDetailsCodeDesc = "押款原因代碼描述";

			internal const string ObligationGuaranteeSuretyID = "繳款人統一編號";

			internal const string ObligationGuaranteeSuretyName = "繳款人英文名稱";

			internal const string ObligationGuaranteeSuretyChineseName = "繳款人中文名稱";

			internal const string ObligationGuaranteeSuretyTypeCode = "繳款人身分識別代碼";

			internal const string ObligationGuaranteeSuretyTypeCodeDesc = "繳款人身分識別代碼描述";

			internal const string ResponsibleGovernmentAgencyID = "條碼機關別代碼";
		}

		#endregion
	}
}
