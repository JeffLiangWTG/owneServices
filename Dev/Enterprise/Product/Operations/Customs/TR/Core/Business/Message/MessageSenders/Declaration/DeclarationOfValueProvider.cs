using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.TR.MessageContracts.Interfaces.Declaration;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;

namespace Enterprise.Customs.TR.Business
{
	public class DeclarationOfValueProvider : IDeclarationOfValue
	{
		public DeclarationOfValueProvider(JobComInvoiceHeader invoiceHeader)
		{
			InvoiceHeader = Argument.NotNull(invoiceHeader, nameof(invoiceHeader));
			DV1Details = (CusDV1Detail)(InvoiceHeader?.JobDeclaration?.DV1Details.FirstOrDefault());
			EntryHeader = InvoiceHeader?.JobDeclaration?.CusEntryHeader;
		}
		JobComInvoiceHeader InvoiceHeader { get; }
		CusDV1Detail DV1Details { get; }
		CusEntryHeader EntryHeader { get; }

		public string TypeOfDelivery => InvoiceHeader.JZ_IncoTerm;
		public string InvoiceDateAndNumber => InvoiceHeader.JZ_InvoiceDate.ToString(CusEntryMessageConstants.DateFormat.DayMonthYear) + " " + InvoiceHeader.JZ_InvoiceNumber;
		public string AgreementDateAndNumber => DV1Details != null ? DV1Details.DV1_ContractDate.ToString(CusEntryMessageConstants.DateFormat.DayMonthYear) + " " + DV1Details.DV1_ContractNumber : string.Empty;
		public string DecisionOfCustomsOffice => DV1Details != null ? DV1Details.DV1_CustomsDecisionNumber + " " + DV1Details.DV1_CustomsDecisionDate.ToString(CusEntryMessageConstants.DateFormat.DayMonthYear) : string.Empty;
		public string ConsigneeAndSeller => DV1Details != null && DV1Details.DV1_Relationship == RelationCodeList.Codes.Y ? CusEntryMessageConstants.TurkishAnswers.ThereIsRelationShip : CusEntryMessageConstants.TurkishAnswers.NoRelationShip;
		public string Relationship => DV1Details != null && DV1Details.DV1_PriceInfluence == YesNoList.Codes.Yes ? CusEntryMessageConstants.TurkishAnswers.Yes : CusEntryMessageConstants.TurkishAnswers.No;
		public string Similars => DV1Details != null && DV1Details.DV1_CloseApproximation == YesNoList.Codes.Yes ? CusEntryMessageConstants.TurkishAnswers.Yes : CusEntryMessageConstants.TurkishAnswers.No;
		public string ConsigneeAndSellerDetails => DV1Details != null ? DV1Details.DV1_RelationDetails.ToString() : string.Empty;
		public string Restrictions => DV1Details != null && DV1Details.DV1_Restrictions == YesNoList.Codes.Yes ? CusEntryMessageConstants.TurkishAnswers.Yes : CusEntryMessageConstants.TurkishAnswers.No;
		public string Act => DV1Details != null && DV1Details.DV1_Consideration == YesNoList.Codes.Yes ? CusEntryMessageConstants.TurkishAnswers.Yes : CusEntryMessageConstants.TurkishAnswers.No;
		public string RestrictionsDetails => DV1Details != null ? DV1Details.DV1_RestrictionConsiderationDetails.ToString() : string.Empty;
		public string Royalty => DV1Details != null && DV1Details.DV1_RoyaltiesLicence == YesNoList.Codes.Yes ? CusEntryMessageConstants.TurkishAnswers.Yes : CusEntryMessageConstants.TurkishAnswers.No;
		public string RoyaltyConditions => DV1Details != null ? DV1Details.DV1_RoyaltiesLicenceDetails.ToString() : string.Empty;
		public string TransitionToSeller => DV1Details != null && DV1Details.DV1_Resale == YesNoList.Codes.Yes ? CusEntryMessageConstants.TurkishAnswers.Yes : CusEntryMessageConstants.TurkishAnswers.No;
		public string TransitionToSellerConditions => DV1Details != null ? DV1Details.DV1_ResaleDetails.ToString() : string.Empty;
		public string CityPlace => DV1Details != null ? DV1Details.DV1_Place.ToString() : string.Empty;
		public string WrittenContract => DV1Details != null ? CusEntryMessageConstants.TurkishAnswers.Yes : string.Empty;

		IReadOnlyCollection<IDeclarationOfValueLines> fDeclarationOfValueLinesCache;
		public IReadOnlyCollection<IDeclarationOfValueLines> DeclarationOfValueLines => fDeclarationOfValueLinesCache ??= EntryHeader.AllEntryLines.Select((entryline, index) => new CusDV1DetailProvider(entryline, index + 1)).ToArray();
	}
}
