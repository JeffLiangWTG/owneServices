using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ImportJobDocAddressValidation : JobDocAddressValidation
{
	public ImportJobDocAddressValidation(AutoJobDocAddress parent, JobDeclaration declaration) : base(parent)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
	}
	readonly JobDeclaration declaration;

	protected override void CheckOrganisationPK()
	{
		base.CheckOrganisationPK();

		if (Parent.Organisation != null)
		{
			switch (Parent.E2_AddressType)
			{
				case DocAddressTypes.Codes.ImporterDocumentaryAddress:
					CheckRuleR257();
					CheckOrganisationName();
					break;
			}
		}
	}

	void CheckRuleR257()
	{
		if (declaration.CustomsEntryHeaders.SelectMany(header => header.AllEntryLines).Cast<CusEntryLine>().Any(entryLine => EntryLineHaveR257Charge(entryLine)))
		{
			Parent.OrganisationPKInfo.AddMessageError(Res.GetString("841F5A19-60D0-4BF8-B16B-998CB346CB17",
				"(R257) Fiscal role code FR7 with a valid VAT number is missing."));
		}

		bool EntryLineHaveR257Charge(CusEntryLine entryLine)
		{
			return entryLine.Fees.Cast<CusEntryLineFee>().Any(fee =>
				fee.CF_ChargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat
				&& fee.CF_MethodOfPayment == PLMethodOfPaymentList.Codes.G
				&& entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Any(invoiceLine => InvoiceLineHaveR257Charge(invoiceLine)));
		}

		bool InvoiceLineHaveR257Charge(JobComInvoiceLine invoiceLine)
		{
			return !invoiceLine.FiscalReferences.Cast<CusFiscalReference>().Any(fiscalReference =>
				fiscalReference.CFR_Code == FiscalReferenceCodeList.Codes.FR7_Taxpayer && !fiscalReference.CFR_Reference.IsEmpty);
		}
	}

	void CheckOrganisationName()
	{
		if (declaration.Importer is OrgHeader orgHeader)
		{
			PLOrgHeaderValidationHelper.ValidateOrganizationName(orgHeader, declaration.ImporterDocumentaryAddress.OrganisationPKInfo);
		}
	}
}
