using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration.Accounting;

namespace Enterprise.MasterFiles.Business.Testing
{
	public sealed class FakeEligibilityLiteTransaction : IEInvoicingEligibilityLiteTransaction
	{
		public ZString CountryCode { get; set; }

		public ZString Ledger { get; set; }

		public ZString TransactionType { get; set; }

		public ZString ComplianceSubType { get; set; }

		public ZString ComplianceNumber { get; set; }

		public ZString TransactionCategory { get; set; }

		public ZString TransactionNumber { get; set; }

		public ZString PlaceOfSupply { get; set; }

		public IEInvoicingEligibilityLiteOrgHeader OrgHeader { get; set; } = new FakeEligibilityLiteOrgHeader();

		public IEInvoicingEligibilityLiteOrgHeader BranchOrgProxy { get; set; } = new FakeEligibilityLiteOrgHeader();

		public IEInvoicingEligibilityLiteOrgHeader CompanyOrgProxy { get; set; } = new FakeEligibilityLiteOrgHeader();

		public ZGuid CompanyPK { get; set; }

		public ZGuid BranchPK { get; set; }

		public ZDateTime InvoiceDate { get; set; }

		public ZDecimal InvoiceAmount { get; set; }

		public ZBool IsCancelled { get; }

		public IReadOnlyCollection<IEInvoicingEligibilityLiteTransactionLine> Lines { get; set; } = Array.Empty<IEInvoicingEligibilityLiteTransactionLine>();

		public ZString GovernmentAllocatedID { get; set; }

		public IEInvoicingEligibilityLiteTransaction OriginalTransactionIfExists { get; set; }

		public IEInvoicingEligibilityLiteOrgAddress InvoiceOrgAddressOverride { get; set; } = new FakeEligibilityLiteOrgAddress();
	}

	public sealed class FakeEligibilityLiteOrgAddress : IEInvoicingEligibilityLiteOrgAddress
	{
		public ZString CountryCode { get; set; }
	}

	public sealed class FakeEligibilityLiteOrgHeader : IEInvoicingEligibilityLiteOrgHeader
	{
		public ZString Category { get; set; }

		public IReadOnlyCollection<IEInvoicingEligibilityLiteRegistrationCode> RegistrationCodes { get; set; } = Array.Empty<IEInvoicingEligibilityLiteRegistrationCode>();

		public IEInvoicingEligibilityLiteOrgAddress MainAddress { get; set; } = new FakeEligibilityLiteOrgAddress();

		public FakeEligibilityLiteOrgHeader WithCategory(ZString category)
		{
			Category = category;
			return this;
		}

		public FakeEligibilityLiteOrgHeader WithRegistrationCode(string countryCode = null, string codeType = null, string registrationNumber = null)
		{
			RegistrationCodes = RegistrationCodes.Concat(new IEInvoicingEligibilityLiteRegistrationCode[]
								{
									new FakeEligibilityLiteRegistrationCode()
									{
										CountryCode = countryCode,
										CodeType = codeType,
										RegistrationNumber = registrationNumber,
									}
								}).ToArray();
			return this;
		}
	}

	public sealed class FakeEligibilityLiteRegistrationCode : IEInvoicingEligibilityLiteRegistrationCode
	{
		public ZString CountryCode { get; set; }

		public ZString CodeType { get; set; }

		public ZString RegistrationNumber { get; set; }
	}

	public sealed class FakeEligibilityLiteTransactionLine : IEInvoicingEligibilityLiteTransactionLine
	{
		public ZGuid ChargePK { get; set; }

		public ZString ChargeCode { get; set; }

		public ZString ChargeType { get; set; }

		public ZString TaxType { get; set; }

		public ZDecimal TaxRate { get; set; }

		public ZDecimal LineAmount { get; set; }

		public ZDecimal OSAmount { get; set; }

		public ZDecimal GSTVATAmount { get; set; }
	}
}
