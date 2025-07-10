using System;
using System.Collections.Specialized;
using System.Linq;
using CargoWise.Customs.TW.MessageDefinitions.N5110;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.TW.Business.MessageConstants;

namespace Enterprise.Customs.TW.Business.MessageProcessors
{
	public class N5110MessageHelper : TWMessageHelper
	{
		public N5110MessageHelper(TWMessage message) : base(message)
		{
			response = Message.IncomingMessageKeyInfomation.Result as Response;
		}

		readonly Response response;
		readonly Func<ResponseDeclarationGoodsShipmentConsignmentTransportContractDocument, string[], ZString> getDocumentIdBySpecifiedCodes = (document, specifiedCodes) =>
		{
			var result = ZString.Empty;
			var typeCode = document.TypeCode?.Value ?? ZString.Empty;
			if (specifiedCodes.Contains(typeCode))
			{
				result = document.Id?.Value ?? ZString.Empty;
			}
			return result;
		};

		protected override void WriteTable(HtmlTableCreator table)
		{
			if (response != null)
			{
				var bankAccount = response.BankAccount;

				WriteRow(table, Titles.BankAccountID, bankAccount?.Id?.Value ?? ZString.Empty);
				WriteRow(table, Titles.BankAccountReferenceID, bankAccount?.ReferenceId?.Value ?? ZString.Empty);

				WriteRow(table, Titles.StatusNameCode, response.Status?.NameCode?.Value ?? ZString.Empty);

				var declaration = response.Declaration;

				WriteRow(table, Titles.DeclarationID, declaration?.Id?.Value ?? ZString.Empty);
				WriteRow(table, Titles.DeclarationTotalPackageQuantity, declaration?.TotalPackageQuantity?.Value, true);
				WriteRow(table, Titles.DeclarationTypeCode, declaration?.TypeCode?.Value ?? ZString.Empty);
				var agent = declaration?.Agent;
				WriteRow(table, Titles.DeclarationAgentID, agent?.Id?.Value ?? ZString.Empty);
				WriteRow(table, Titles.DeclarationAgentSubBoxID, agent?.TwSubBoxId?.Value ?? ZString.Empty);

				WriteRow(table, Titles.DeclarationBorderTransportMeansArrivalDateTime, declaration?.BorderTransportMeans?.ArrivalDateTime ?? ZString.Empty);

				var payment = declaration?.DutyTaxFee?.Payment;
				WriteRow(table, Titles.PaymentDueDateTime, payment?.DueDateTime ?? ZString.Empty);
				WriteRow(table, Titles.PaymentReferenceID, payment?.ReferenceId?.Value ?? ZString.Empty);

				WriteRow(table, Titles.PaymentCollectionTypeCode, payment?.TwCollectionTypeCode?.Value ?? ZString.Empty);
				WriteRow(table, Titles.PaymentIssueReasonCode, payment?.TwIssueReasonCode?.Value ?? ZString.Empty);
				WriteRow(table, Titles.PaymentObligationGuaranteeReferenceID, payment?.ObligationGuarantee?.ReferenceId?.Value ?? ZString.Empty);

				var goodsShipment = declaration?.GoodsShipment;
				var consignment = goodsShipment?.Consignment;
				WriteRow(table, Titles.ConsignmentBorderTransportMeansID, consignment?.BorderTransportMeans?.Id?.Value ?? ZString.Empty);
				WriteRow(table, Titles.ConsignmentBorderTransportMeansJourneyID, consignment?.BorderTransportMeans?.JourneyId?.Value ?? ZString.Empty);

				var transportContractDocument = consignment?.TransportContractDocument;
				var documentIDwithType_704_741 = transportContractDocument?.Select(x => getDocumentIdBySpecifiedCodes(x, new string[] { TransportContractDocumentTypeCodes._704, TransportContractDocumentTypeCodes._741 }))?.FirstOrDefault(x => !x.IsEmpty) ?? ZString.Empty;
				WriteRow(table, Titles.ConsignmentTransportContractDocumentID_704_741, documentIDwithType_704_741);

				var documentIDwithType_703_714 = transportContractDocument?.Select(x => getDocumentIdBySpecifiedCodes(x, new string[] { TransportContractDocumentTypeCodes._703, TransportContractDocumentTypeCodes._714 }))?.FirstOrDefault(x => !x.IsEmpty) ?? ZString.Empty;
				WriteRow(table, Titles.ConsignmentTransportContractDocumentID_703_714, documentIDwithType_703_714);

				WriteRow(table, Titles.OtherChargeDeductionAmount, goodsShipment?.CustomsValuation?.OtherChargeDeductionAmount?.Value, true);
				WriteRow(table, Titles.CommodityClassificationID, goodsShipment?.GovernmentAgencyGoodsItem?.Commodity?.Classification?.Id?.Value ?? ZString.Empty);

				var importer = declaration?.Importer;
				WriteRow(table, Titles.ImporterID, importer?.Id?.Value ?? ZString.Empty);
				WriteRow(table, Titles.ImporterName, importer?.Name?.Value ?? ZString.Empty);
				WriteRow(table, Titles.ImporterChineseName, importer?.TwChineseName?.Value ?? ZString.Empty);
				WriteRow(table, Titles.ImporterTypeCode, importer?.TwTypeCode?.Value ?? ZString.Empty);

				WriteRow(table, Titles.PackagingTypeCode, declaration?.Packaging?.TypeCode?.Value ?? ZString.Empty);
				WriteRow(table, Titles.ResponsibleGovernmentAgencyID, declaration?.ResponsibleGovernmentAgency?.Id?.Value ?? ZString.Empty);

				WriteRow(table, Titles.TotalDutyTaxFeeAmount, declaration?.DutyTaxFee?.TwTotalDutyTaxFeeAmount.Value, true);

				var dutyTaxFees = goodsShipment?.DutyTaxFee;
				if (dutyTaxFees != null)
				{
					foreach (var fee in dutyTaxFees)
					{
						var typeCode = fee?.TypeCode?.Value ?? ZString.Empty;
						WriteRow(table, Titles.DutyTaxFeeTypeCode, fee?.TypeCode?.Value ?? ZString.Empty);
						WriteRow(table, Titles.DutyTaxFeeTypeCodeDesc, Factory.GetCachedValue<DutyTaxFeeCodeList>().GetDescriptionFromCode(typeCode));
						WriteRow(table, Titles.DutyTaxFeeAdValoremTaxBaseAmount, fee?.AdValoremTaxBaseAmount?.Value, true);
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no need")]
		public override string ToHtml()
		{
			var table = new HtmlTableCreator(new NameValueCollection { { (NoResString)"border", "0" }, { (NoResString)"cellpadding", "0" }, { "cellspacing", "0" }, { "style", "margin: 5pt;" } }) { EnableHTMLEncoding = false };
			WriteTable(table);
			return table?.ToHtml();
		}

		sealed class Titles
		{
			#region SuppressResourceStringsCheckRegion

			internal const string BankAccountID = "銷帳編號";

			internal const string BankAccountReferenceID = "海關帳戶";

			internal const string StatusNameCode = "通關方式";

			internal const string DeclarationID = "報單號碼";

			internal const string DeclarationTotalPackageQuantity = "總件數";

			internal const string DeclarationTypeCode = "報單類別代碼";

			internal const string DeclarationAgentID = "報關業者箱號";

			internal const string DeclarationAgentSubBoxID = "報關業者箱號附碼";

			internal const string DeclarationBorderTransportMeansArrivalDateTime = "進口日期";

			internal const string PaymentDueDateTime = "繳款截止日";

			internal const string PaymentReferenceID = "稅費繳納證號碼";

			internal const string PaymentCollectionTypeCode = "代收項目";

			internal const string PaymentIssueReasonCode = "核發原因代碼";

			internal const string PaymentObligationGuaranteeReferenceID = "案號";

			internal const string ConsignmentBorderTransportMeansID = "船(機)代碼";

			internal const string ConsignmentBorderTransportMeansJourneyID = "船舶航次(海)/航機班次(空)";

			internal const string ConsignmentTransportContractDocumentID_704_741 = "主提單號碼";

			internal const string ConsignmentTransportContractDocumentID_703_714 = "分提單號碼";

			internal const string OtherChargeDeductionAmount = "營業稅稅基";

			internal const string CommodityClassificationID = "貨品分類號列";

			internal const string ImporterID = "進口人(納稅義務人)統一編號";

			internal const string ImporterName = "進口人(納稅義務人)英文名稱";

			internal const string ImporterChineseName = "進口人(納稅義務人)中文名稱";

			internal const string ImporterTypeCode = "進口人(納稅義務人)身分識別代碼";

			internal const string PackagingTypeCode = "件數單位";

			internal const string ResponsibleGovernmentAgencyID = "條碼機關別代碼";

			internal const string TotalDutyTaxFeeAmount = "稅費合計";

			internal const string DutyTaxFeeTypeCode = "稅費代碼";

			internal const string DutyTaxFeeTypeCodeDesc = "稅費代碼描述";

			internal const string DutyTaxFeeAdValoremTaxBaseAmount = "稅費金額";

			#endregion
		}
	}
}
