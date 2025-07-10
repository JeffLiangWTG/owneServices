using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class JobComInvoiceHeaderValidationFormalEntry : JobComInvoiceHeaderValidation
	{
		public JobComInvoiceHeaderValidationFormalEntry(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		#region Declaration
		protected JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Parent.JobDeclaration;
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;
		#endregion

		protected override void CheckJZ_InvoiceNumber()
		{
			base.CheckJZ_InvoiceNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_InvoiceNumberInfo);
		}

		protected override void CheckJZ_OH_Buyer()
		{
			base.CheckJZ_OH_Buyer();
			if (Declaration != null
				&& Declaration.IsImport
				&& !Parent.JZ_OH_Buyer.IsEmpty
				&& !Declaration.JE_OH_Importer.IsEmpty
				&& Parent.JZ_OH_Buyer != Declaration.JE_OH_Importer)
			{
				Parent.JZ_OH_BuyerInfo.AddMessageError(InvoiceImporterAndDeclarationImporterMustMatch);
			}

			if (Declaration != null && Declaration.IsTSWDeclaration && Declaration.IsExport && !Declaration.IsECIWriteoff)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_OH_BuyerInfo);
			}
		}

		protected override void CheckJZ_OH_Supplier()
		{
			base.CheckJZ_OH_Supplier();
			if (Declaration != null)
			{
				if (Declaration.IsExport)
				{
					if (!Parent.JZ_OH_Supplier.IsEmpty
						&& !Declaration.JE_OH_Supplier.IsEmpty
						&& Parent.JZ_OH_Supplier != Declaration.JE_OH_Supplier)
					{
						Parent.JZ_OH_SupplierInfo.AddMessageError(InvoiceSupplierAndDeclarationSupplierMustMatch);
					}
				}
				else
				{
					if (Parent.Supplier == null)
					{
						Parent.JZ_OH_SupplierInfo.AddMessageError(MustHaveSupplierForImportJob);
					}
					else if (!Declaration.IsSimplified && !Declaration.IsPrimaryIndustriesImportDeclaration && !string.IsNullOrEmpty(new NZCustomsCodeValidator(OrgCusCode.CodeTypes.SupplierCode).GetCasperCodeValidationErrors(Parent.Supplier.LocalCustomsSupplierCode)))
					{
						Parent.JZ_OH_SupplierInfo.AddMessageError(MissingCustomsSupplierCode);
					}
				}
			}
		}

		protected override void CheckJZ_InvoiceDate()
		{
			base.CheckJZ_InvoiceDate();
			if (Declaration != null && Declaration.IsTSWDeclaration && Declaration.IsExport && !Declaration.IsECIWriteoff)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_InvoiceDateInfo);
			}
		}

		protected override void CheckJZ_InvoiceCurrExRate()
		{
			base.CheckJZ_InvoiceCurrExRate();
			if (Parent.JZ_InvoiceCurrExRate.IsEmpty)
			{
				Parent.JZ_InvoiceCurrExRateInfo.AddMessageError(MissingExchangeRate);
			}
		}

		protected override void CheckJZ_RX_NKInvoice_Currency()
		{
			base.CheckJZ_RX_NKInvoice_Currency();
			if (Declaration != null && Declaration.IsExport)
			{
				ValidateJZ_InvoiceCurrExRateType();
			}
		}

		protected override void AddWarningOrMessageErrorToJZ_InvoiceCurrExRateInfoWhenExchangeRateStale(ZPropertyInfo exchangeRateInfo)
		{
			if (!Declaration.IsTSW_IPI_Declaration)
			{
				base.AddWarningOrMessageErrorToJZ_InvoiceCurrExRateInfoWhenExchangeRateStale(exchangeRateInfo);
			}
		}

		protected override void CheckJZ_CU_RelatedHouseBill()
		{
		}

		protected override void CheckJZ_RN_NKDefaultOrigin()
		{
			base.CheckJZ_RN_NKDefaultOrigin();
			if (!Parent.JZ_RN_NKDefaultOrigin.IsEmpty && Parent.IsImport && Parent.DefaultOrigin == null)
			{
				Parent.JZ_RN_NKDefaultOriginInfo.AddMessageError(MessageErrorRemoveOrFixDefaultCountryOfOrigin);
			}
		}

		public const string MessageErrorRemoveOrFixDefaultCountryOfOrigin = "Please either remove or enter a valid Default Country/Region of Origin.";
		public const string InvoiceImporterAndDeclarationImporterMustMatch = "This is an import job and Importer in Declaration is different to Importer set in this invoice.";
		public const string MissingExchangeRate = "You must have an Exchange Rate for each Invoice.";
		public const string MustHaveSupplierForImportJob = "You must have a Supplier for each Invoice on an Import Declaration.";
		public const string InvoiceSupplierAndDeclarationSupplierMustMatch = "This is an export job and Supplier in Declaration is different to Supplier set in this Invoice.";
		public const string MissingCustomsSupplierCode = JobDeclarationValidationFormalEntry.OrganisationCSCCustomsCodeMissing;

		protected override void CheckJZ_InvoiceCurrExRateType()
		{
			if (Declaration != null && Declaration.IsExport)
			{
				RefCurrency invoiceCurrency = Parent.Invoice_Currency;
				if (invoiceCurrency == null)
				{
					Parent.JZ_InvoiceCurrExRateTypeInfo.AddMessageError(MessageErrorMustHaveCurrencyAndExchangeRateIndicator);
				}
				else if (invoiceCurrency.RX_Code == Core.Constants.CurrencyCodes.NewZealand)
				{
					if (Parent.JZ_InvoiceCurrExRateType != ExchangeRateIndicatorList.Codes.NZD)
					{
						Parent.JZ_InvoiceCurrExRateTypeInfo.AddMessageError(MessageErrorExchangeRateIndicatorMustBeNZD);
					}
				}
				else
				{
					if (Parent.JZ_InvoiceCurrExRateType != ExchangeRateIndicatorList.Codes.Floating && Parent.JZ_InvoiceCurrExRateType != ExchangeRateIndicatorList.Codes.ForwardCover)
					{
						Parent.JZ_InvoiceCurrExRateTypeInfo.AddMessageError(MessageErrorExchangeRateIndicatorMustNotBeNZD);
					}

					if (Parent.JZ_MessageType == JobMessageTypeList.Codes.Export && Parent.JobDeclaration != null)
					{
						var otherInvoices = Parent.JobDeclaration.GetOtherInvoices(Parent);
						if (otherInvoices != null && otherInvoices.Any())
						{
							foreach (var jobComInvoiceHeader in otherInvoices)
							{
								jobComInvoiceHeader.Validation.ValidateJZ_InvoiceCurrExRateType();
							}
						}

						var otherInvoicesHavingDifferentIndicator =
							Parent.JobDeclaration.GetAllInvoicesOfSameCurrencyHavingDifferentCurrencyIndicator(invoiceCurrency.RX_Code, Parent);

						if (otherInvoicesHavingDifferentIndicator != null && otherInvoicesHavingDifferentIndicator.Any())
						{
							Parent.JZ_InvoiceCurrExRateTypeInfo.AddError(
								ErrorExchangeRateIndicatorMustBeSameForSameCurrency + invoiceCurrency.RX_Code);

							foreach (var jobComInvoiceHeader in otherInvoicesHavingDifferentIndicator)
							{
								jobComInvoiceHeader.Validation.ValidateJZ_InvoiceCurrExRateType();
							}
						}
					}
				}
			}
		}
		public const string MessageErrorMustHaveCurrencyAndExchangeRateIndicator = "Must have a valid Currency and Exchange Rate Indicator on a Commercial Invoice.";
		public const string MessageErrorExchangeRateIndicatorMustBeNZD = "Must use 'NZD - NZ Dollars' on a local currency Commercial Invoice.";
		public const string MessageErrorExchangeRateIndicatorMustNotBeNZD = "Must use 'FLO - Floating' or 'FCV - Forward Cover' on a foreign currency Commercial Invoice.";
		public const string ErrorExchangeRateIndicatorMustBeSameForSameCurrency = "Only one Exchange Rate Indicator is allowed for one foreign currency: ";

		#region Implementation

		protected override TypeOfValidationForMissingMandatoryChargesForIncoterm ValidationForMissingMandatoryCharges
		{
			get { return InvoiceHeaderValidation.TypeOfValidationForMissingMandatoryChargesForIncoterm.Warning; }
		}

		#endregion
	}
}
