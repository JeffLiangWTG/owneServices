using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using CusEntryHeader = Enterprise.Customs.NL.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.NL.Business;

public static class WrapperHelper
{
	public static string ConvertTransportMode(string transportMode) => new ModeOfTransportCodeList().GetDescriptionFromCode(transportMode);

	public static string ConvertAuthorizationUsageCode(string authorizationUsageCode) => new AuthorizationUsageCodeList().GetDescriptionFromCode(authorizationUsageCode) ?? authorizationUsageCode;

	public static ZBool IsDestinationSpecified(ZString style) => destinationSpecifiedStyles.Contains(style);

	static readonly ImmutableHashSet<string> destinationSpecifiedStyles = new HashSet<string>
	{
		NLConstants.EntryStyles.ExportReExport,
		NLConstants.EntryStyles.SpecialProcessing, NLConstants.EntryStyles.UnionGoods,
		NLConstants.EntryStyles.SpecialFiscalTerritory, NLConstants.EntryStyles.ExportDeclarationC1,

		NLConstants.EntryStyles.DeclarationForEndUse, NLConstants.EntryStyles.DeclarationForCustWarehouse,
		NLConstants.EntryStyles.DeclarationTemporaryAdmission, NLConstants.EntryStyles.DeclarationInwardProcessing,
		NLConstants.EntryStyles.ImportSpecialFiscalTerritoriesDeclaration, NLConstants.EntryStyles.ImportSimplifiedDeclaration
	}.ToImmutableHashSet();

	public static bool AllLineDestinationsEqualToDeclaration(this CusEntryHeader entryHeader) => entryHeader.InvoiceLines.Cast<JobComInvoiceLine>().All(x => x.ZG_CountryOfDestination.EqualsIgnoringCase(entryHeader.Declaration.JE_RL_NKFinalDestination.Left(2)));

	public static bool AllLineSellersAreEmptyOrEqualToDeclaration(this CusEntryHeader entryHeader) => entryHeader.InvoiceLines.Cast<JobComInvoiceLine>().All(x => x.SellerDocAddress.Organisation == null || x.SellerDocAddress.OrganisationPK == entryHeader.Declaration.Seller?.PK);

	public static bool AllLineBuyersAreEmptyOrEqualToDeclaration(this CusEntryHeader entryHeader) => entryHeader.InvoiceLines.Cast<JobComInvoiceLine>().All(x => x.BuyerDocAddress.Organisation == null || x.BuyerDocAddress.OrganisationPK == entryHeader.Declaration.ConsigneeOrgAddress?.PK);

	public static bool AllLineConsigneesAreEmptyOrEqualToDeclaration(this CusEntryHeader entryHeader) => entryHeader.InvoiceLines.Cast<JobComInvoiceLine>().All(x => x.ConsigneeAddress == null || x.ConsigneeAddressOrgPK == entryHeader.Declaration.ImporterDocumentaryAddress.OrganisationPK);

	public static bool AllLineConsigneesAreEmpty(this CusEntryHeader entryHeader) => entryHeader.InvoiceLines.Cast<JobComInvoiceLine>().All(x => x.ConsigneeAddress == null);

	public static bool AllLineConsignorsAreEmpty(this CusEntryHeader entryHeader) => entryHeader.InvoiceLines.Cast<JobComInvoiceLine>().All(x => x.ExporterAddress == null);
}
