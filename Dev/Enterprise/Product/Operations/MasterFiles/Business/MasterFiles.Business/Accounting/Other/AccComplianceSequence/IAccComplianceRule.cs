using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Registry.Business
{
	public interface IAccComplianceRule
	{
		ZString Country { get; }
		ZString SubType { get; }
		ZString LedgerType { get; }
		ZString InvoiceType { get; }
		ZString TaxInvoiceRule { get; }
		IReadOnlyCollection<ZString> SeparatedTaxIDCodes { get; }
		ZString DisbursementRule { get; }
		ZString OriginalRule { get; }
		ZString TaxRegistrationType { get; }
		ZString OrganisationLocation { get; }
		ZString OrganisationCategory { get; }
		ZString SelfBillingRule { get; }
		ZString ExporterExemption { get; }
		ZString TaxRegistrationLocationRule { get; }
		ZString VATGroupRule { get; }
		ZString ParentTransactionSubType { get; }
		ZString RequiredTaxSystem { get; }
		ZString ExcludedTaxSystem { get; }
		ZString RequiredRegistrationCode { get; }
		ZString ExcludedRegistrationCode { get; }
		ZBool ThresholdApplies { get; }
		ZString SubTypeThresholdNotMet { get; }
	}
}
