using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class InvoiceHeaderValidation : JobComInvoiceHeaderValidation, ILandedCostExchangeRateValidation
	{
		public enum TypeOfValidationForMissingMandatoryChargesForIncoterm { None, Warning, MessageError, Error }

		public InvoiceHeaderValidation(BaseJobComInvoiceHeader invoiceHeader) : base(invoiceHeader)
		{
		}

		public void ValidateBuyerOrgPK()
		{
			ValidateCalculatedProperty(Parent.BuyerOrgPKInfo);
		}

		protected virtual void CheckBuyerOrgPK()
		{
		}

		public void ValidateSellerOrgPK()
		{
			ValidateCalculatedProperty(Parent.SellerOrgPKInfo);
		}

		protected virtual void CheckSellerOrgPK()
		{
		}

		protected new BaseJobComInvoiceHeader Parent => (BaseJobComInvoiceHeader)base.Parent;
		// OK to cache as Validation is created each time JobComInvoiceHeader.Validation is touched
		public bool IsExport
		{
			get
			{
				if (!isExport.HasValue)
				{
					isExport = IsExportCore;
				}
				return isExport.Value;
			}
		}
		bool? isExport;

		protected virtual bool IsExportCore => Parent?.IsExport ?? false;

		// OK to cache as Validation is created each time JobComInvoiceHeader.Validation is touched
		public bool IsImport
		{
			get
			{
				if (!isImport.HasValue)
				{
					isImport = IsImportCore;
				}
				return isImport.Value;
			}
		}
		bool? isImport;

		protected virtual bool IsImportCore => Parent?.IsImport ?? false;

		ExternalMessageValidation fMessageValidation;
		public ExternalMessageValidation MessageValidation
		{
			get
			{
				if (fMessageValidation == null)
				{
					fMessageValidation = GetNewExternalMessageValidation();
				}
				return fMessageValidation;
			}
		}

		protected virtual ExternalMessageValidation GetNewExternalMessageValidation()
		{
			return new ExternalMessageValidation(Parent);
		}

		protected override void CheckJZ_JE()
		{
			base.CheckJZ_JE();
			ValidateJZ_OH_Supplier();
			ValidateJZ_InvoiceNumber();
		}

		protected virtual bool ShouldCheckRelatedHouseBillEntered => true;

		protected override void CheckJZ_CU_RelatedHouseBill()
		{
			base.CheckJZ_CU_RelatedHouseBill();
			if (ShouldCheckRelatedHouseBillEntered && Parent.JobDeclaration != null && Parent.JobDeclaration.ActiveEntryHeaders.Count > 1)
			{
				MessageValidation.CheckEntered(Parent.JZ_CU_RelatedHouseBillInfo, MultipleEntriesHouseBillMessage);
			}
		}

		public static string MultipleEntriesHouseBillMessage
		{
			get { return Res.GetString("19323d1b-6d77-4bbf-8ab3-c5035f7324b4", "A House Bill must be selected on every Invoice Header; Multiple Customs Entries Exist."); }
		}

		protected override void CheckJZ_InvoiceNumber()
		{
			base.CheckJZ_InvoiceNumber();

			var declaration = Parent.PersistentDeclaration;
			if (declaration == null)
			{
				MandatoryValidation.CheckEntered(Parent.JZ_InvoiceNumberInfo);
			}

			if (Parent.JZ_InvoiceNumber.Length > 2 && CustomsDataRegistry.Instance.WarnUserWhenCommercialInvoiceHasBeenUsedBefore.Value)
			{
				if (declaration == null)
				{
					var supplier = Parent.Supplier_Effective;
					var invoiceBranch = Parent.Branch;
					if (ShouldCheckDuplicate && supplier != null && invoiceBranch != null)
					{
						var retriever = new DuplicateInvoiceNumberRetriever(Parent.Factory, false);
						var duplicates = retriever.GetJobNumbersWithDuplicateInvoiceNumber(
							ZGuid.Empty,
							Parent.PK,
							new ZString[] { Parent.JZ_MessageType },
							invoiceBranch.Company,
							Parent.JZ_InvoiceNumber,
							supplier.PK
						);
						if (!duplicates.IsEmpty)
						{
							Parent.JZ_InvoiceNumberInfo.AddWarning(Res.GetString("bff5e545-32d6-42ff-8950-8c5d473ee1bd", "This invoice number already exists in job(s): {0}", duplicates));
						}
					}
				}
				else
				{
					var org = declaration.IsDrawback ? declaration.Importer : Parent.Supplier_Effective;
					var duplicates = ZString.Empty;

					if (org != null && ShouldCheckDuplicate)
					{
						var retriever = new DuplicateInvoiceNumberRetriever(Parent.Factory, declaration.IsDrawback);
						duplicates = retriever.RetrieveDuplicateJobNumbers(declaration, Parent, org.PK);

						if (!duplicates.IsEmpty)
						{
							Parent.JZ_InvoiceNumberInfo.AddWarning(Res.GetString("bff5e545-32d6-42ff-8950-8c5d473ee1bd", "This invoice number already exists in job(s): {0}", duplicates));
						}
					}

					if (duplicates.IsEmpty && WarnIfInvoiceNumberAlreadyExistsInThisDeclaration)
					{
						if (declaration.Invoices.Find(new ZQuery(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, Parent.JZ_InvoiceNumber)).Take(2).Count() > 1)
						{
							Parent.JZ_InvoiceNumberInfo.AddWarning(Res.GetString("f3033f1f-3b4c-48fc-9b21-fe5a275019c3", "This invoice number already exists in this job"));
						}
					}
				}
			}
		}

		protected virtual ZBool ShouldCheckDuplicate
		{
			get { return true; }
		}

		protected virtual bool WarnIfInvoiceNumberAlreadyExistsInThisDeclaration => true;

		protected override void CheckJZ_OH_Buyer()
		{
			base.CheckJZ_OH_Buyer();
			if (IsBuyerRequired)
			{
				MandatoryValidation.CheckEntered(Parent.JZ_OH_BuyerInfo);
			}

			if (Parent.IsABondedWarehousingInvoiceWithDifferentImporter)
			{
				Parent.JZ_OH_BuyerInfo.AddMessageError(BaseJobDeclaration.BondedWarehousingInvoiceCannotHaveDifferentImporterMessage(Parent.JobDeclaration.TermNameForBondedWarehouse));
			}
		}

		protected virtual bool IsBuyerRequired
		{
			get { return !Parent.IsAttachedToPersistentDeclaration; }
		}

		protected override void CheckJZ_GB()
		{
			base.CheckJZ_GB();
			if (!Parent.IsAttachedToPersistentDeclaration && Parent.Branch == null)
			{
				Parent.JZ_GBInfo.AddError(InvalidBranch);
			}
		}
		public static string InvalidBranch
		{
			get { return Res.GetString("aa57e28a-4d08-482c-bce0-71f8d20f06e2", "Please enter a valid Branch."); }
		}

		protected override void CheckJZ_OH_Supplier()
		{
			base.CheckJZ_OH_Supplier();
			if (Parent.IsAttachedToPersistentDeclaration
				&& !Parent.JZ_OH_Supplier.IsEmpty
				&& Parent.JZ_OH_Supplier == OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation
				&& !MiscSupplierIsAllowed)
			{
				Parent.JZ_OH_SupplierInfo.AddError(ErrorCannotUseMISCOnCommercialInvoiceHeader);
			}
			else if (IsSupplierRequired)
			{
				MandatoryValidation.CheckEntered(Parent.JZ_OH_SupplierInfo);
			}

			if (Parent.IsABondedWarehousingInvoiceWithDifferentSupplier)
			{
				Parent.JZ_OH_SupplierInfo.AddMessageError(BaseJobDeclaration.BondedWarehousingInvoiceCannotHaveDifferentSupplierMessage(Parent.JobDeclaration.TermNameForBondedWarehouse));
			}
		}

		public static string ErrorCannotUseMISCOnCommercialInvoiceHeader
		{
			get { return Res.GetString("fc22b707-88ff-45b3-9ff9-aef64f291d0f", "A 'MISC' Organization cannot be used on a Commercial Invoice. Please override the Declaration values and select a valid Supplier."); }
		}

		protected virtual bool MiscSupplierIsAllowed
		{
			get { return !Parent.JZ_SupplierMiscFields.IsEmpty; }
		}

		protected virtual bool IsSupplierRequired
		{
			get { return !Parent.IsAttachedToPersistentDeclaration; }
		}

		protected override void CheckJZ_RX_NKInvoice_Currency()
		{
			base.CheckJZ_RX_NKInvoice_Currency();
			ListValidation.ErrorIfInvalidCode(Parent.JZ_RX_NKInvoice_CurrencyInfo);
			ValidateJZ_InvoiceAmount();
			if (Parent.JobDeclaration != null)
			{
				CheckInvoice(Parent.JZ_InvoiceAmountInfo, Parent.JZ_RX_NKInvoice_CurrencyInfo);
			}

			if (Parent.GroupHeader != null)
			{
				Parent.GroupHeader.Charges.MarkAsNeedingValidation();
			}
			ValidateCurrenceyAreTheSame();
		}

		void ValidateCurrenceyAreTheSame()
		{
			var declaration = Parent.JobDeclaration;
			if (declaration != null && declaration.IsInvoicesRequiredToBeInSameCurrency)
			{
				var currency = ZString.Empty;
				foreach (var invoice in declaration.Invoices)
				{
					if (currency != ZString.Empty && invoice.JZ_RX_NKInvoice_Currency != ZString.Empty && currency != invoice.JZ_RX_NKInvoice_Currency)
					{
						Parent.JZ_RX_NKInvoice_CurrencyInfo.AddMessageError(Res.GetString("daaaef94-d407-4db3-87cc-e19f417dbba8", "Currency should be the same for all Invoices."));
						break;
					}
					else
					{
						currency = invoice.JZ_RX_NKInvoice_Currency;
					}
				}
			}
		}

		protected override void CheckJZ_InvoiceCurrExRate()
		{
			base.CheckJZ_InvoiceCurrExRate();
			if (!Parent.IsJZ_InvoiceCurrExRateUserEnterable && Parent.EffectiveExchangeRateForInvoiceCurr != Parent.JZ_InvoiceCurrExRate)
			{
				AddWarningOrMessageErrorToJZ_InvoiceCurrExRateInfoWhenExchangeRateStale(Parent.JZ_InvoiceCurrExRateInfo);
			}
		}

		protected virtual void AddWarningOrMessageErrorToJZ_InvoiceCurrExRateInfoWhenExchangeRateStale(ZPropertyInfo exchangeRateInfo)
		{
			exchangeRateInfo.AddWarning(ExchangeRateOutOfDateWarningMessage);
		}

		protected virtual string ExchangeRateOutOfDateWarningMessage
		{
			get
			{
				return Res.GetString("e1578c68-9585-4e72-a00f-692fdd8e88f4", "This exchange rate was set when the currency was set. But the current rate for the valuation date is {0}. This happens when latest exchange rates are imported after a currency is selected on the invoice. Please perform apportionment by clicking Brokerage > Perform Apportionment. This will update this exchange rate. Please also check if customs entries have correct figures.", Parent.EffectiveExchangeRateForInvoiceCurr.ToString(6));
			}
		}

		protected virtual void CheckInvoice(ZPropertyInfo amountInfo, ZPropertyInfo currencyInfo)
		{
			if (currencyInfo != null)
			{
				if (!amountInfo.Value.IsEmpty)
				{
					MessageValidation.CheckEntered(currencyInfo, Res.GetString("9aed46fa-0a99-44d9-a5da-ebcfbb7a40a2", "Please enter a Currency"));
				}
				else
				{
					MessageValidation.CheckInvalidValue(currencyInfo);
				}

				if (ShouldCheckExRates && !Parent.IsJZ_InvoiceCurrExRateUserEnterable)
				{
					MessageValidation.ValidateExchangeRateExist((RefCurrencyCurrencyConverter)Parent.CurrencyConverter, currencyInfo);
				}
			}
		}

		protected virtual bool ShouldCheckExRates
		{
			get { return true; }
		}

		protected override void CheckJZ_IncoTerm()
		{
			base.CheckJZ_IncoTerm();
			if (IncoTermRequired)
			{
				var info = Parent.JZ_IncoTermInfo;
				if (Parent.JZ_IncoTerm.IsEmpty)
				{
					info.AddMessageError(GetIncoTermIsRequiredMessage(info));
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(info, Parent.Lookups.JZ_IncoTerm_List);
				}
				ValidateMandatoryChargesForIncoTerm();
				ValidateGroupChargesJ7_IsIncludedInLinesRecursively(Parent.GroupHeader);

				ValidateIncoTermAreTheSame();
			}
		}

		void ValidateIncoTermAreTheSame()
		{
			var declaration = Parent.JobDeclaration;
			if (declaration != null && declaration.IsInvoicesRequiredToBeInSameIncoTerm)
			{
				var incoTerm = ZString.Empty;
				foreach (var invoice in declaration.Invoices)
				{
					if (incoTerm != ZString.Empty && invoice.IncoTerm != ZString.Empty && incoTerm != invoice.IncoTerm)
					{
						Parent.JZ_IncoTermInfo.AddMessageError(Res.GetString("f937b3f0-7e3a-fcb8-4dbf-f2e585ef8e03", "Incoterms should be the same for all Invoices."));
						break;
					}
					else
					{
						incoTerm = invoice.IncoTerm;
					}
				}
			}
		}

		protected virtual string GetIncoTermIsRequiredMessage(ZPropertyInfo info) => MandatoryValidation.MustBeEnteredMessage(info.Description);
		protected virtual bool IncoTermRequired => true;
		protected virtual TypeOfValidationForMissingMandatoryChargesForIncoterm ValidationForMissingMandatoryCharges => TypeOfValidationForMissingMandatoryChargesForIncoterm.None;

		protected void ValidateMandatoryChargesForIncoTerm()
		{
			string missingMandatoryCharges = GetMissingMandatoryCharges();
			if (missingMandatoryCharges.Length > 0)
			{
				string message = Res.GetString("301be84f-6ab2-7f98-43e4-b8f3a5da1142", "The following charges are missing for Incoterm {0}:\r\n{1}", Parent.JZ_IncoTerm, missingMandatoryCharges.Trim(','));
				if (ValidationForMissingMandatoryCharges == TypeOfValidationForMissingMandatoryChargesForIncoterm.MessageError)
				{
					Parent.JZ_IncoTermInfo.AddMessageError(message);
				}
				else if (ValidationForMissingMandatoryCharges == TypeOfValidationForMissingMandatoryChargesForIncoterm.Warning)
				{
					Parent.JZ_IncoTermInfo.AddWarning(message);
				}
				else if (ValidationForMissingMandatoryCharges == TypeOfValidationForMissingMandatoryChargesForIncoterm.Error)
				{
					Parent.JZ_IncoTermInfo.AddError(message);
				}
			}
		}

		void ValidateGroupChargesJ7_IsIncludedInLinesRecursively(BaseJobComInvoiceGroupHeader groupInvoice)
		{
			if (groupInvoice != null)
			{
				foreach (BaseGroupInvoiceCharge charge in groupInvoice.Charges)
				{
					charge.Validation.ValidateJ7_IsIncludedInITOT();
				}
				ValidateGroupChargesJ7_IsIncludedInLinesRecursively(groupInvoice.GroupHeader);
			}
		}

		protected string GetMissingMandatoryCharges()
		{
			string result = string.Empty;
			var incoTerm = Parent.IncoTerm;
			if (!incoTerm.IsEmpty)
			{
				var missingCharges = Parent.IncoTermAndChargeFactory.MissingMandatoryCharges(incoTerm, Parent);
				StringBuilder charges = new StringBuilder();
				foreach (ICustomsChargeCode chargeCode in missingCharges)
				{
					charges.Append(chargeCode.Description + ",");
				}
				result = charges.ToString();
			}
			return result;
		}

		#region Calculated Properties Public ValidateXXX() Methods

		public void ValidateJZ_Calc_Balance()
		{
			ValidateCalculatedProperty(Parent.JZ_Calc_BalanceInfo);
		}

		public void ValidateJZ_Calc_FOBAmount()
		{
			ValidateCalculatedProperty(Parent.JZ_Calc_FOBAmountInfo);
		}

		public void ValidateJZ_Calc_CIFAmount()
		{
			ValidateCalculatedProperty(Parent.JZ_Calc_CIFAmountInfo);
		}

		public void ValidateJZ_Calc_TNI()
		{
			ValidateCalculatedProperty(Parent.JZ_Calc_TNIInfo);
		}

		public void ValidateJZ_MessageType()
		{
			ValidateCalculatedProperty(Parent.JZ_MessageTypeInfo);
		}

		public void ValidateEffectiveValuationDate()
		{
			ValidateCalculatedProperty(Parent.EffectiveValuationDateInfo);
		}

		public void ValidateJZ_Calc_OFTInInvoiceCurrency()
		{
			ValidateCalculatedProperty(Parent.JZ_Calc_OFTInInvoiceCurrencyInfo);
		}

		public void ValidateJZ_Calc_ONSInInvoiceCurrency()
		{
			ValidateCalculatedProperty(Parent.JZ_Calc_ONSInInvoiceCurrencyInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateJZ_MessageType();
			ValidateJZ_Calc_TNI();
			ValidateJZ_Calc_CIFAmount();
			ValidateJZ_Calc_FOBAmount();
			ValidateJZ_Calc_Balance();
			ValidateEffectiveValuationDate();
			ValidateJZ_Calc_OFTInInvoiceCurrency();
			ValidateJZ_Calc_ONSInInvoiceCurrency();
			ValidateBuyerOrgPK();
			ValidateSellerOrgPK();
		}

		#endregion

		#region Calculated Properties CheckXXX() Methods

		protected virtual void CheckEffectiveValuationDate()
		{
		}

		protected virtual void CheckJZ_MessageType()
		{
			if (!Parent.IsAttachedToPersistentDeclaration)
			{
				MandatoryValidation.CheckEntered(Parent.JZ_MessageTypeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.JZ_MessageTypeInfo, Parent.Lookups.MessageTypes);
			}
		}

		protected void CheckJZ_Calc_Balance()
		{
			if (!Parent.IsAttachedToPersistentDeclaration || !Parent.JobDeclaration.ApportionmentDirty)
			{
				CheckJZ_Calc_BalanceCore();
			}
		}

		protected virtual void CheckJZ_Calc_BalanceCore()
		{
			if (Parent.JZ_Calc_Balance.Round(2) != 0.00m)
			{
				Parent.JZ_Calc_BalanceInfo.AddMessageError(UnbalancedInvoiceMessage);
			}
		}

		public static string UnbalancedInvoiceMessage
		{
			get { return Res.GetString("5cebd4d1-538d-4bfa-830e-b9649b09b106", "The total of all invoice lines must equal the invoice total before submitting this declaration to Customs"); }
		}

		protected virtual void CheckJZ_Calc_FOBAmount()
		{
			if (Parent.JZ_Calc_FOBAmount < 0)
			{
				Parent.JZ_Calc_FOBAmountInfo.AddMessageError(Res.GetString("2cf27bfe-d5b7-4e96-85da-88143b04170d", "The FOB amount should be greater than zero before submitting this declaration.\r\nThis usually happens when you have entered more invoice charge amounts than invoice total amount."));
			}
		}

		public static string ZeroFreightInsuranceWarning
		{
			get { return Res.GetString("56224ec5-187d-417a-98d0-3613706e0d5b", "The CIF amount is the same as the FOB amount and you have not entered any Overseas Freight or Insurance."); }
		}

		protected virtual void CheckJZ_Calc_CIFAmount()
		{
			if (Parent.JZ_Calc_CIFAmount < 0)
			{
				Parent.JZ_Calc_CIFAmountInfo.AddMessageError(Res.GetString("d6ee96e9-72ef-46d5-ada1-09410a80531e", "The CIF amount should be greater than zero before submitting this declaration.\r\nThis usually happens when you have entered more invoice charge amounts than invoice total amount."));
			}
			CheckJZ_Calc_CIFAmount_ZeroFreightInsurance();
		}

		protected virtual void CheckJZ_Calc_CIFAmount_ZeroFreightInsurance()
		{
			if (Parent.JZ_Calc_FOBAmount == Parent.JZ_Calc_CIFAmount)
			{
				Parent.JZ_Calc_CIFAmountInfo.AddNotification(NotificationTypeForZeroFreightAndInsuranceToCIF, ZeroFreightInsuranceWarning);
			}
		}

		protected virtual void CheckJZ_Calc_TNI()
		{
		}

		protected virtual void CheckJZ_Calc_OFTInInvoiceCurrency()
		{
		}

		protected virtual void CheckJZ_Calc_ONSInInvoiceCurrency()
		{
		}

		protected virtual INotificationType NotificationTypeForZeroFreightAndInsuranceToCIF
		{
			get { return CargoWise.EntityFramework.NotificationType.Warning; }
		}

		#endregion

		void ILandedCostExchangeRateValidation.ValidateExchangeRate()
		{
			ValidateJZ_InvoiceCurrLandedCostExRate();
		}

		protected virtual NotificationTypes ErrorTypeForNoOfPacks => NotificationTypes.Error;

		protected override void CheckJZ_NoOfPacks()
		{
			base.CheckJZ_NoOfPacks();

			if (ErrorTypeForNoOfPacks == NotificationTypes.Error)
			{
				CompareValidation.CheckNumberNotNegative(Parent.JZ_NoOfPacksInfo);
			}
			else if (ErrorTypeForNoOfPacks == NotificationTypes.MessageError)
			{
				CompareValidation.MessageErrorIfLessThan(Parent.JZ_NoOfPacksInfo, ZDecimal.Zero);
			}

			if ((Parent.JZ_NoOfPacks.IsDefault)
				&& (Parent?.JobDeclaration?.Importer?.MiscServ?.OM_IMBalanceInvoicePackage ?? false)
			)
			{
				Parent.JZ_NoOfPacksInfo.AddMessageError(Res.GetString("15F17655-5ADD-4550-BBDD-690EDF9F2ADF", "No. of Packages is mandatory."));
			}
		}

		#region Weight

		protected override void CheckJZ_Weight()
		{
			base.CheckJZ_Weight();
			ValidateJZ_NetWeight();
		}

		protected override void CheckJZ_WeightUQ()
		{
			base.CheckJZ_WeightUQ();
			ValidateJZ_NetWeight();
		}

		protected override void CheckJZ_NetWeight()
		{
			base.CheckJZ_NetWeight();

			if (!Parent.JZ_Weight.IsEmpty && !Parent.JZ_WeightUQ.IsEmpty && !Parent.JZ_NetWeight.IsEmpty && !Parent.JZ_NetWeightUQ.IsEmpty)
			{
				ZWeight weight = new ZWeight(Parent.JZ_Weight, Parent.JZ_WeightUQ);
				ZWeight netWeight = new ZWeight(Parent.JZ_NetWeight, Parent.JZ_NetWeightUQ);
				if (weight.IsValid && netWeight.IsValid && netWeight > weight)
				{
					Parent.JZ_NetWeightInfo.AddWarning(Res.GetString("527c964b-851d-4a80-b9e7-925db7ebc8e7", "The Net Weight should not be greater than the Gross Weight."));
				}
			}
		}

		protected override void CheckJZ_NetWeightUQ()
		{
			base.CheckJZ_NetWeightUQ();
			ValidateJZ_NetWeight();
		}

		#endregion

		protected override void CheckJZ_MarksAndNumbers()
		{
			base.CheckJZ_MarksAndNumbers();
			CheckJZ_MarksAndNumbersIsWesternEuropeanIfRequired();
		}

		protected virtual void CheckJZ_MarksAndNumbersIsWesternEuropeanIfRequired()
		{
			if (!Parent.AllowNonWesternEuropeanCharacterForMarksAndNumbers)
			{
				EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.JZ_MarksAndNumbersInfo);
			}
		}
	}
}
