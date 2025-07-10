using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business.Business.JobComInvoiceHeader;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Customs.ZA;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ZA.Business
{
	public class JobComInvoiceHeaderValidation : AutoZAJobComInvoiceHeaderValidation
	{
		public JobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidateEntryInstructionDetails();
			ValidateSingleVDNPerEntryInstruction();
		}

		protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

		protected override ExternalMessageValidation GetNewExternalMessageValidation()
		{
			return new JobComInvoiceHeaderExternalMessageValidation(Parent);
		}

		protected override void CheckJZ_Calc_CIFAmount_ZeroFreightInsurance()
		{
			if (!Parent.JobDeclaration?.IsExWarehouse ?? false)
			{
				base.CheckJZ_Calc_CIFAmount_ZeroFreightInsurance();
			}
		}

		protected override void CheckJZ_RN_NKDefaultOrigin()
		{
			base.CheckJZ_RN_NKDefaultOrigin();
			ListValidation.MessageErrorIfInvalidCode(Parent.JZ_RN_NKDefaultOriginInfo);
		}

		/// <Incident>I00026553</Incident>
		protected override void CheckJZ_OH_Supplier()
		{
			base.CheckJZ_OH_Supplier();
			bool isExport = Parent.JobDeclaration != null && Parent.JobDeclaration.IsExport;
			bool isExWarehouse = Parent.JobDeclaration != null && Parent.JobDeclaration.IsExWarehouse;
			if (Parent.JobDeclaration == null || !isExWarehouse)
			{
				OrgHeader supplier = Parent.Supplier;
				if (supplier == null)
				{
					Parent.JZ_OH_SupplierInfo.AddMessageError("Please enter a supplier");
				}
				else
				{
					ZString supplierCode = supplier.LocalCustomsSupplierCode;
					if (supplierCode.IsEmpty && (isExport || !Parent.JZ_VDN.IsEmpty))
					{
						Parent.JZ_OH_SupplierInfo.AddMessageError("The currently selected supplier does not have a Customs Supplier Code");
					}
				}
			}

			if (isExWarehouse && JobComInvoiceLine.ZAAddInvoiceDetailsToCUSDECMessageEnabled && IsLinkedToEntryInstructionWith46Or47CPC())
			{
				if (Parent.JZ_OH_Supplier.IsEmpty)
				{
					Parent.JZ_OH_SupplierInfo.AddMessageError("A Supplier is required for this declaration.");
				}
				else
				{
					var supplier = Parent.Supplier;
					if (supplier == null)
					{
						ListValidation.ErrorIfInvalidPK(Parent.JZ_OH_SupplierInfo);
					}
					else if (supplier.MainAddress == null || supplier.MainAddress.Address1.IsEmpty || supplier.MainAddress.OA_RN_NKCountryCode.IsEmpty)
					{
						Parent.JZ_OH_SupplierInfo.AddMessageError("A Supplier's address is required for this declaration.");
					}
				}
			}
		}

		protected override void CheckJZ_PaymentTerms()
		{
			if (Parent.JZ_PaymentTerms.IsEmpty)
			{
				var declaration = Parent.JobDeclaration;
				if (declaration != null && PaymentTermsIsRequired(declaration.JE_MessageType))
				{
					Parent.JZ_PaymentTermsInfo.AddMessageError(ValidationConstants.InvoiceHeader.PaymentTermsIsRequired);
				}
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JZ_PaymentTermsInfo, ValidationConstants.InvoiceHeader.PaymentTermsInvalid);
			}

			bool PaymentTermsIsRequired(string messageType) => messageType switch
			{
				ZAJobMessageTypeList.Codes.Import or ZAJobMessageTypeList.Codes.Export => true,
				ZAJobMessageTypeList.Codes.ExBond => Parent.CusEntryInstructions.Any(x => x.CEI_Style == ProcedureCodes._46 || x.CEI_Style == ProcedureCodes._47),
				_ => false,
			};
		}

		protected override void CheckJZ_InvoiceNumber()
		{
			base.CheckJZ_InvoiceNumber();
			var jobDeclaration = Parent.JobDeclaration;
			var isImport = jobDeclaration != null && jobDeclaration.JE_MessageType == ZAJobMessageTypeList.Codes.Import;
			var invoiceNumber = Parent.JZ_InvoiceNumber;
			if (isImport && invoiceNumber.IsEmpty)
			{
				Parent.JZ_InvoiceNumberInfo.AddMessageError(ValidationConstants.InvoiceHeader.InvoiceNumberRequiredForImportShipments);
			}
		}

		protected override void CheckJZ_InvoiceDate()
		{
			base.CheckJZ_InvoiceDate();
			var jobDeclaration = Parent.JobDeclaration;
			var isImport = jobDeclaration != null && jobDeclaration.JE_MessageType == ZAJobMessageTypeList.Codes.Import;
			var isExport = jobDeclaration != null && jobDeclaration.JE_MessageType == ZAJobMessageTypeList.Codes.Export;
			var isExWarehouse = jobDeclaration != null && Parent.JobDeclaration.IsExWarehouse;
			var invoiceDate = Parent.JZ_InvoiceDate;
			if (isImport || isExport)
			{
				if (invoiceDate.IsEmpty)
				{
					Parent.JZ_InvoiceDateInfo.AddMessageError(ValidationConstants.InvoiceHeader.InvoiceDateRequiredForImportAndExportShipments);
				}

				if (Parent.JZ_InvoiceDate.IsInTheFutureDatePartOnly)
				{
					Parent.JZ_InvoiceDateInfo.AddMessageError(ValidationConstants.InvoiceHeader.InvoiceDateCannotBeInTheFuture);
				}
			}

			if (isExWarehouse && JobComInvoiceLine.ZAAddInvoiceDetailsToCUSDECMessageEnabled && IsLinkedToEntryInstructionWith46Or47CPC() && Parent.JZ_InvoiceDate.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_InvoiceDateInfo);
			}
		}

		protected override void CheckJZ_ValuationCode()
		{
			base.CheckJZ_ValuationCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.JZ_ValuationCodeInfo);

			var valuationCode = Parent.JZ_ValuationCode;
			var relatedIndicator = Parent.JZ_RelatedIndicator;
			ValuationCodeListValidation.ValidateIndicator(relatedIndicator, valuationCode, Parent.JZ_ValuationCodeInfo);
		}

		protected override void CheckJZ_RelatedIndicator()
		{
			base.CheckJZ_RelatedIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.JZ_RelatedIndicatorInfo);
			var jobDeclaration = Parent.JobDeclaration;
			var isImport = jobDeclaration != null && jobDeclaration.JE_MessageType == ZAJobMessageTypeList.Codes.Import;
			if (isImport && Parent.JZ_RelatedIndicator.IsEmpty)
			{
				Parent.JZ_RelatedIndicatorInfo.AddMessageError(ValidationConstants.InvoiceHeader.RelationshipIndicatorRequiredForImports);
			}
			ValidateJZ_ValuationCode();
		}

		protected override void CheckJZ_CU_RelatedHouseBill()
		{
			//dont need the validation. Hide it away.
		}

		protected override void CheckJZ_RX_NKInvoice_Currency()
		{
			base.CheckJZ_RX_NKInvoice_Currency();
			if (!Parent.JZ_RX_NKInvoice_Currency.IsEmpty && Parent.JZ_RX_NKInvoice_Currency != Core.Constants.CurrencyCodes.SouthAfrica &&
				(Parent.JobDeclaration?.IsImportByExternalBroker ?? false))
			{
				Parent.JZ_RX_NKInvoice_CurrencyInfo.AddMessageError(ValidationConstants.InvoiceHeader.MustBeZARCurrencyForImportByExternalBroker);
			}
		}

		protected override TypeOfValidationForMissingMandatoryChargesForIncoterm ValidationForMissingMandatoryCharges
		{
			get { return InvoiceHeaderValidation.TypeOfValidationForMissingMandatoryChargesForIncoterm.Warning; }
		}

		void ValidateEntryInstructionDetails()
		{
			var dec = Parent.JobDeclaration;
			if (dec != null && dec.IsPersistent && dec.IsExport)
			{
				if (Parent.IsZAROnlyInvoice)
				{
					if (Parent.CusEntryInstructions.Cast<CusEntryInstruction>().Where(x => x != null).Select(x => x.CEI_ExchangeRateDate).Distinct().Take(2).Count() > 1)
					{
						Parent.AddRowMessageError(ValidationConstants.InvoiceHeader.SingleInvoiceToEntryForExportsHasMultipleExchangeRateDate);
					}
				}
				else if (Parent.CusEntryInstructions.Take(2).Count() > 1)
				{
					Parent.AddRowMessageError(ValidationConstants.InvoiceHeader.SingleForeignCurrencyInvoiceToEntryForExports);
				}
			}
		}

		protected JobComInvoiceHeaderLookups Lookups
		{
			get { return Parent.Lookups; }
		}

		protected override void CheckJZ_ROOType()
		{
			base.CheckJZ_ROOType();
			ListValidation.MessageErrorIfInvalidCode(Parent.JZ_ROOTypeInfo, Lookups.ROOTypesList);

			if (IsExport)
			{
				if (Parent.JZ_ROOType.IsEmpty && !Parent.JZ_ROOCert.IsEmpty)
				{
					Parent.JZ_ROOTypeInfo.AddMessageError(ValidationConstants.InvoiceLine.NoROOTypeEnteredForCert);
				}
			}
		}

		protected override void CheckJZ_ValuationMarkup()
		{
			base.CheckJZ_ValuationMarkup();
			var jobDeclaration = Parent.JobDeclaration;
			var isImport = jobDeclaration != null && jobDeclaration.JE_MessageType == ZAJobMessageTypeList.Codes.Import;
			if (isImport)
			{
				if (Parent.JZ_VDN.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.JZ_ValuationMarkupInfo);
				}
				else
				{
					MandatoryValidation.CheckNotNegative(Parent.JZ_ValuationMarkupInfo);
				}
			}
		}

		protected override void CheckJZ_VDN()
		{
			if (Parent.JZ_VDN.IsEmpty)
			{
				var invoice = Parent;
				var declaration = invoice.JobDeclaration;
				if (declaration != null && declaration.JE_MessageType == ZAJobMessageTypeList.Codes.Import)
				{
					if ((invoice.JZ_RelatedIndicator == RelatedIndicatorList.Codes.Yes) && (!invoice.Supplier?.LocalCustomsSupplierCode.IsEmpty ?? false))
					{
						Parent.JZ_VDNInfo.AddMessageError(ValidationConstants.InvoiceHeader.VDNRequiredWhenSupplierHasCSCCode);
					}
				}
			}

			ValidateSingleVDNPerEntryInstruction();
		}

		void ValidateSingleVDNPerEntryInstruction()
		{
			foreach (CusEntryInstruction entryInstruction in Parent.CusEntryInstructions)
			{
				entryInstruction.ClearRowNotificationsContaining(ValidationConstants.EntryInstruction.InvoicesWithDifferentVDNNumberOnEntryInstruction);

				if (entryInstruction.InvoiceLines.Select(x => x.InvoiceHeader.JZ_VDN).Distinct().Take(2).Count() > 1)
				{
					entryInstruction.AddRowMessageError(ValidationConstants.EntryInstruction.InvoicesWithDifferentVDNNumberOnEntryInstruction);
				}
			}
		}

		bool IsLinkedToEntryInstructionWith46Or47CPC() => Parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => new ZString[] { ProcedureCodes._46, ProcedureCodes._47 }.Contains(x.EntryInstruction?.CEI_Style ?? ZString.Empty));
	}
}
