using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ZA.Business.Business.FetchStrategies;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ZA.Business
{
	[SystemDefinedValues]
	public partial class JobComInvoiceHeader : AutoZAJobComInvoiceHeader, Integration.Customs.ZA.IJobComInvoiceHeader, IInvoiceInformation, IInvoiceHeaderInformation, IDA63ValueRecalculationParent, ICurrencyConverterDataProvider
	{
		public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		public override ZDecimal MinimumReapportionedLineWeight => 0.001;

		protected override ZString LocalCurrencyCodeCore
		{
			get { return JobDeclaration.LocalCurrencyConstantCode; }
		}

		protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation()
		{
			return new JobComInvoiceHeaderValidation(this);
		}

		public override ZGuid JZ_OH_Supplier
		{
			get { return base.JZ_OH_Supplier; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(JobComInvoiceHeader.Schema.JZ_OH_Supplier))
				{
					var oldValue = JZ_OH_Supplier;
					base.JZ_OH_Supplier = value;
					if (oldValue != JZ_OH_Supplier && !IsCopying)
					{
						PopulateValuesFromSupplierLink();
						DefaultRooCertWithApprovedExporter();
					}
				}
			}
		}

		void PopulateValuesFromSupplierLink()
		{
			if (SupplierBuyerLink != null && IsImport)
			{
				if (!SupplierBuyerLink.OL_ValuationBasisDeterminationNum.IsEmpty && JZ_VDN.IsEmpty)
				{
					JZ_VDN = SupplierBuyerLink.OL_ValuationBasisDeterminationNum;
				}

				if (!SupplierBuyerLink.OL_ValuationBasis.IsEmpty && JZ_ValuationCode.IsEmpty)
				{
					JZ_ValuationCode = SupplierBuyerLink.OL_ValuationBasis.Left(AutoJobComInvoiceHeader.Schema.JZ_ValuationCodeMaxLength);
				}

				if (!SupplierBuyerLink.OL_RelatedParty.IsEmpty && JZ_RelatedIndicator.IsEmpty)
				{
					JZ_RelatedIndicator = SupplierBuyerLink.OL_RelatedParty;
				}

				if (!SupplierBuyerLink.OL_ValuationBasisMarkupPercent.IsEmpty && JZ_ValuationMarkup.IsEmpty)
				{
					JZ_ValuationMarkup = SupplierBuyerLink.OL_ValuationBasisMarkupPercent;
				}
			}
		}

		public void DefaultRooCertWithApprovedExporter()
		{
			if (JZ_ROOType.IsEmpty)
			{
				var dec = JobDeclaration;

				if (dec != null)
				{
					var supplierCustomsApprovedExporterCode = dec.SupplierCustomsApprovedExporterCode;

					if (dec.ShouldDefaultRooCertForApprovedExporter(supplierCustomsApprovedExporterCode))
					{
						JZ_ROOType = OrgCusCode.SouthAfricaCodeTypes.CustomsApprovedExporter;
						JZ_ROOCert = supplierCustomsApprovedExporterCode;
					}
				}
			}
		}

		public override ZString JZ_RelatedIndicator
		{
			get { return base.JZ_RelatedIndicator; }
			set
			{
				var oldValue = JZ_RelatedIndicator;
				base.JZ_RelatedIndicator = value;
				if (oldValue != JZ_RelatedIndicator && !IsCopying)
				{
					if (RelatedIndicatorList.Codes.Exempt.Equals(value))
					{
						JZ_ValuationCode = ZString.Empty;
					}
				}
			}
		}

		public override ZGuid JZ_JE
		{
			get { return base.JZ_JE; }
			set
			{
				base.JZ_JE = value;
				if (!IsDataChangeSuspendedByFakeDeclaration)
				{
					JobDeclaration jobDeclaration = this.JobDeclaration;
					if (jobDeclaration != null)
					{
						jobDeclaration.CustomsEntryHeaders.MarkAsNeedingValidation();
					}
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.ZA.Business.JobComInvoiceHeader|JZ_RN_NKDefaultOrigin", Caption = "Goods Origin", ShortCaption = "Origin")]
		public override ZString JZ_RN_NKDefaultOrigin
		{
			get { return GetEffectiveValueToReturn(base.JZ_RN_NKDefaultOrigin, JobDeclaration.Schema.JE_GoodsOrigin, Schema.JZ_RN_NKDefaultOrigin); }
			set
			{
				var oldValue = base.JZ_RN_NKDefaultOrigin; // need base value instead of effective value
				base.JZ_RN_NKDefaultOrigin = GetEffectiveValueToSet(value, JobDeclaration.Schema.JE_GoodsOrigin);
				if (!IsCopying)
				{
					var newValue = JZ_RN_NKDefaultOrigin;
					if (oldValue != newValue)
					{
						ClearInvoiceLinesValuesIfSame(newValue, JobComInvoiceLine.Schema.JI_CountryOfOrigin);
						JobComInvoiceLines.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZDecimal JZ_Calc_ConversionFactor => Factory.GetValue(ref cachedJZ_Calc_ConversionFactor, () =>
		{
			var result = ZDecimal.Zero;
			var linesEntered = JZ_Calc_LinesEntered;
			var currencyConverter = CurrencyConverter;
			var invCurrency = Invoice_Currency;
			var factorDivisor = linesEntered - GroupCharges.GetTotal(currencyConverter, invCurrency, (charge) => charge.IsDiscount && !charge.J7_IsIncludedInITOT)
				- Charges.GetTotal(this.CurrencyConverter, invCurrency, (charge) => charge.IsDiscount && !charge.J7_IsIncludedInITOT);
			if (factorDivisor != ZDecimal.Zero)
			{
				var fobAmountInForeignCurrency = linesEntered + DutiableChargesNotIncludedInLinesInInvoiceCurrency - NonDutiableChargesIncludedInLinesInInvoiceCurrency;
				var factorDivident = currencyConverter?.ConvertExact(new Money(fobAmountInForeignCurrency, invCurrency), LocalCurrency)?.Amount;
				result = (factorDivident ?? 0m) / factorDivisor;
			}
			return result.Round(8);
		});

		CachedProperty<ZDecimal> cachedJZ_Calc_ConversionFactor;

		public override ZDecimal JZ_Calc_CIFAmount
		{
			get
			{
				var intellectualValue = InvoiceLines.OfType<JobComInvoiceLine>().Sum(x => x.JI_Calc_IntellectualValue_Core);
				return base.JZ_Calc_CIFAmount + intellectualValue;
			}
		}

		public override ZDecimal JZ_Calc_FOBAmount
		{
			get
			{
				var intellectualValue = InvoiceLines.OfType<JobComInvoiceLine>().Sum(x => x.JI_Calc_IntellectualValue_Core);
				return base.JZ_Calc_FOBAmount + intellectualValue;
			}
		}

		protected override void SetDefaultInvoiceDate()
		{
			//do not set invoice date for ZA.
		}

		protected override ZDateTime EffectiveValuationDateCore
		{
			get
			{
				ZDateTime result;
				if (IsExport)
				{
					var originalEntryInstruction = InvoiceLines.Cast<JobComInvoiceLine>().Where(x => x.EntryInstruction != null).Select(x => x.EntryInstruction)
						.Distinct().OrderBy(x => x.CEI_Style).ThenBy(x => x.CEI_Description).FirstOrDefault(x => x.CEI_ExchangeRateDate.IsValid);
					if (originalEntryInstruction != null)
					{
						result = originalEntryInstruction.CEI_ExchangeRateDate;
					}
					else
					{
						result = base.EffectiveValuationDateCore;
					}
				}
				else
				{
					result = base.EffectiveValuationDateCore;
				}
				return result;
			}
		}

		#endregion

		#region New Properties

		#region JZ_ROOCert

		[BusinessObjectTestExclude]
		[MaxLength(35)]
		[ResourceStringData("Enterprise.Customs.ZA.Business.JobComInvoiceHeader|UZ_ROOCert", Caption = "Rules Of Origin Certificate", ShortCaption = "ROO Certificate")]
		public override ZString JZ_ROOCert
		{
			get
			{
				var declarationRooType = JobDeclaration?.JE_ROOType ?? ZString.Empty;
				if (declarationRooType == JZ_ROOType)
				{
					return GetEffectiveValueToReturn(base.JZ_ROOCert, JobDeclaration.Schema.JE_ROOCert, Schema.JZ_ROOCert);
				}
				else
				{
					return base.JZ_ROOCert;
				}
			}
			set
			{
				var oldValue = base.JZ_ROOCert;
				value = GetEffectiveValueToSet(value, JobDeclaration.Schema.JE_ROOCert);
				base.JZ_ROOCert = value;

				if (!IsCopying && oldValue != JZ_ROOCert)
				{
					ClearInvoiceLinesValuesIfSame(JZ_ROOCert, JobComInvoiceLine.Schema.JI_ROOCert);
					InvoiceLines.MarkAsNeedingValidationIncludingChildren();
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateJZ_ROOType();
				}
			}
		}

		public bool JZ_RX_NKInvoice_Currency_ReadOnly => JobDeclaration != null && JobDeclaration.JE_MessageType == ZAJobMessageTypeList.Codes.ExBond;

		#endregion

		#region ROOType

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.ROOTypesList))]
		[MaxLength(3)]
		public override ZString JZ_ROOType
		{
			get { return GetEffectiveValueToReturn(base.JZ_ROOType, JobDeclaration.Schema.JE_ROOType, Schema.JZ_ROOType); }
			set
			{
				var oldValue = base.JZ_ROOType; // need base value instead of effective value
				base.JZ_ROOType = GetEffectiveValueToSet(value, JobDeclaration.Schema.JE_ROOType);
				if (!IsCopying)
				{
					var newValue = JZ_ROOType;
					if (oldValue != newValue)
					{
						ClearInvoiceLinesValuesIfSame(newValue, JobComInvoiceLine.Schema.JI_PrimaryPreference);
						InvoiceLines.MarkAsNeedingValidation();
					}
				}
			}
		}

		#endregion

		public override ZDecimal JZ_ValuationMarkup
		{
			get { return base.JZ_ValuationMarkup; }
			set
			{
				var oldValue = JZ_ValuationMarkup;
				base.JZ_ValuationMarkup = value;
				if (oldValue != JZ_ValuationMarkup)
				{
					foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
					{
						invoiceLine.UpdateValuationMarkupIfNeeded(oldValue);
					}
				}
			}
		}

		protected override OrgHeader BuyerForSuppplierLinkCalculation
		{
			get { return IsAttachedToPersistentDeclaration ? JobDeclaration.Importer : null; }
		}

		ZDecimal NonDutiableChargesIncludedInLinesInInvoiceCurrency
		{
			get
			{
				var invoiceCurrency = Invoice_Currency;
				var currencyConverter = CurrencyConverter;
				return this.Charges.GetTotal(currencyConverter, invoiceCurrency, (charge) => !charge.J7_IsDutiable && (charge.J7_IsIncludedInITOT != charge.IsDiscount) && charge.J7_ChargeType != InvoiceLineCustomsChargeTypeList.Codes.IntellectualValue)
						+ this.GroupCharges.GetTotal(currencyConverter, invoiceCurrency, (charge) => !charge.J7_IsDutiable && (charge.J7_IsIncludedInITOT != charge.IsDiscount) && charge.J7_ChargeType != InvoiceLineCustomsChargeTypeList.Codes.IntellectualValue);
			}
		}

		ZDecimal DutiableChargesNotIncludedInLinesInInvoiceCurrency
		{
			get
			{
				var invoiceCurrency = Invoice_Currency;
				var currencyConverter = CurrencyConverter;
				return this.Charges.GetTotal(currencyConverter, invoiceCurrency, (charge) => charge.J7_IsDutiable && (charge.J7_IsIncludedInITOT == charge.IsDiscount))
						+ this.GroupCharges.GetTotal(currencyConverter, invoiceCurrency, (charge) => charge.J7_IsDutiable && (charge.J7_IsIncludedInITOT == charge.IsDiscount));
			}
		}

		internal ZBool IsZAROnlyInvoice
		{
			get
			{
				var isImport = IsImport;
				var zarCurrCode = Core.Constants.CurrencyCodes.SouthAfrica;
				Predicate<JobComInvCharge> isChargeZAROrEmpty = (JobComInvCharge charge) =>
				{
					var curr = charge.J7_RX_NKCurrency;
					return curr.IsEmpty || curr == zarCurrCode;
				};
				var result = true;
				result = this.JZ_RX_NKInvoice_Currency == zarCurrCode;
				result = result && this.GroupCharges.OfType<JobComInvCharge>().All(groupCharge => isChargeZAROrEmpty(groupCharge));
				result = result && this.Charges.OfType<JobComInvCharge>().All(charge => isChargeZAROrEmpty(charge));
				result = result && this.InvoiceLines.OfType<JobComInvoiceLine>().All(invLine =>
				{
					var lineCVCurrencyOveride = invLine.JI_RX_NKCustomsValueCurrencyOverride;
					var lineResult = !isImport || lineCVCurrencyOveride == zarCurrCode || lineCVCurrencyOveride.IsEmpty;
					lineResult = lineResult && invLine.Charges.OfType<JobComInvCharge>().All(lineCharge => isChargeZAROrEmpty(lineCharge));
					return lineResult;
				});
				return result;
			}
		}

		#endregion

		#region Implementation

		#region Suspend Effective Value

		internal IDisposable SuspendEffectiveValue(string fieldName, IZType decValue)
		{
			EffectiveValueSuspender holder;
			if (!EffectiveValueSuspenders.TryGetValue(fieldName, out holder))
			{
				holder = new EffectiveValueSuspender(this, fieldName) { DeclarationValue = decValue };
				EffectiveValueSuspenders.Add(fieldName, holder);
			}
			return holder;
		}

		IZType GetEffectiveValue(string fieldName)
		{
			EffectiveValueSuspender holder;
			return EffectiveValueSuspenders.TryGetValue(fieldName, out holder) ? holder.DeclarationValue : null;
		}

		Dictionary<string, EffectiveValueSuspender> EffectiveValueSuspenders
		{
			get { return effectiveValueSuspenders ?? (effectiveValueSuspenders = new Dictionary<string, EffectiveValueSuspender>()); }
		}

		Dictionary<string, EffectiveValueSuspender> effectiveValueSuspenders;

		class EffectiveValueSuspender : IDisposable
		{
			public EffectiveValueSuspender(JobComInvoiceHeader invoice, string fieldName)
			{
				this.invoice = invoice;
				this.fieldName = fieldName;
			}

			public IZType DeclarationValue { get; set; }

			readonly JobComInvoiceHeader invoice;
			readonly string fieldName;

			#region IDisposable Members

			public void Dispose()
			{
				invoice.EffectiveValueSuspenders.Remove(fieldName);
			}

			#endregion
		}
		#endregion

		protected delegate bool ShouldClearInvoiceValue(JobComInvoiceLine invoiceLine, IZType invoiceValue);

		void ClearInvoiceLinesValuesIfSame(IZType invoiceHeaderValue, string invoiceLineFieldName)
		{
			ClearInvoiceLinesValuesIfSame(invoiceHeaderValue, invoiceLineFieldName, delegate (JobComInvoiceLine h, IZType invoiceValue)
			{ return invoiceValue.Equals(invoiceHeaderValue); });
		}

		void ClearInvoiceLinesValuesIfSame(IZType invoiceHeaderValue, string invoiceLineFieldName, ShouldClearInvoiceValue shouldClearInvoiceValue)
		{
			foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
			{
				IZType invoiceValue = (IZType)invoiceLine[invoiceLineFieldName];

				if (shouldClearInvoiceValue(invoiceLine, invoiceValue))
				{
					using (invoiceLine.SuspendEffectiveValue(invoiceLineFieldName, invoiceHeaderValue))
					{
						invoiceLine[invoiceLineFieldName] = invoiceValue.Default;
					}

					ZPropertyInfo infoToRefresh = invoiceLine.ZPropertyInfoHash[invoiceLineFieldName];
					if (infoToRefresh != null)
					{
						infoToRefresh.RefreshBinding();
					}
				}
			}
		}

		T GetEffectiveValueToReturn<T>(T baseValue, string fieldNameInJobDeclaration, string invoiceFieldName) where T : IZType
		{
			T result = baseValue;

			if (result.IsEmpty && IsAttachedToPersistentDeclaration)
			{
				IZType effectiveValue = GetEffectiveValue(invoiceFieldName) ?? (IZType)JobDeclaration[fieldNameInJobDeclaration];
				result = (T)effectiveValue;
			}

			return result;
		}

		T GetEffectiveValueToSet<T>(T valuePassed, string fieldNameInJobDeclaration) where T : IZType
		{
			T result = valuePassed;

			if (!valuePassed.IsDefault && IsAttachedToPersistentDeclaration && JobDeclaration[fieldNameInJobDeclaration].Equals(valuePassed))
			{
				result = (T)valuePassed.Default;
			}

			return result;
		}

		protected override string GetStandaloneIncoTermAndChargeFactoryCountryContext()
		{
			return base.GetStandaloneIncoTermAndChargeFactoryCountryContext() + this.GetCustomsChargeTypeListCacheKey();
		}
		#endregion

		#region IInvoiceInformation

		ZString IInvoiceInformation.InvoiceNumber => JZ_InvoiceNumber;

		ZDateTime IInvoiceInformation.InvoiceDate => JZ_InvoiceDate;

		#endregion

		#region IInvoiceHeaderInformation

		ZString IInvoiceHeaderInformation.NameOfIssuer => Supplier?.OH_FullName ?? ZString.Empty;

		ZString IInvoiceHeaderInformation.CountryOfIssuer => Supplier?.MainAddress?.Country?.Code ?? ZString.Empty;

		ZString IInvoiceHeaderInformation.Address1 => Supplier?.MainAddress?.Address1 ?? ZString.Empty;

		ZString IInvoiceHeaderInformation.Address2 => Supplier?.MainAddress?.Address2 ?? ZString.Empty;

		ZString IInvoiceHeaderInformation.Address3 => Supplier?.MainAddress?.City ?? ZString.Empty;

		ZString IInvoiceHeaderInformation.Address4 => Supplier?.MainAddress?.State ?? ZString.Empty;

		ZDecimal IInvoiceHeaderInformation.TotalInvoiceAmount => JZ_InvoiceAmount;

		ZString IInvoiceHeaderInformation.InvoiceCurrencyCoded => JZ_RX_NKInvoice_Currency;

		ZDecimal IInvoiceHeaderInformation.ExchangeRate => JZ_InvoiceCurrExRate;

		ZDecimal IInvoiceHeaderInformation.TotalChargesInLocalCurrency
		{
			get
			{
				var localCurrency = LocalCurrency;
				var currencyConverter = CurrencyConverter;
				return Charges.GetTotal(currencyConverter, localCurrency, c => true) + GroupCharges.GetTotal(currencyConverter, localCurrency, c => true);
			}
		}

		ZString IInvoiceHeaderInformation.PaymentTerms => JZ_PaymentTerms;

		ZDecimal IInvoiceHeaderInformation.CommonFactor => JZ_Calc_ConversionFactor;

		ZString IInvoiceHeaderInformation.TermsOfDelivery => JZ_IncoTerm;

		ZDecimal IInvoiceHeaderInformation.AdvancePaymentAmount => JZ_PaymentAmount;

		ZString IInvoiceHeaderInformation.AdvancePaymentCurrencyCode => JZ_RX_NKInvoice_Currency;

		ZString[] IInvoiceHeaderInformation.AdvancePaymentNotificationDetails
		{
			get
			{
				return InvoiceLines.Cast<JobComInvoiceLine>()
					.Where(line => line.CusEntryLine != null)
					.OrderBy(line => line.CusEntryLine.CL_LineNumber)
					.SelectMany(line => line.CusEntryLine.AdditionalInformationCodes
						.Where(addInfo => addInfo.CY_Code == UniversalReferenceConstants.AdditionalInformation.AdvancePaymentNo && !addInfo.CY_Data.IsEmpty)
						.Select(addInfo => addInfo.CY_Data))
					.Distinct()
					.Take(5)
					.ToArray();
			}
		}

		IEnumerable<IInvoiceLineInformation> IInvoiceHeaderInformation.InvoiceLineInformations => InvoiceLines.Cast<JobComInvoiceLine>().ToArray();

		IEnumerable<IInvoiceChargeInformation> IInvoiceHeaderInformation.InvoiceChargeInformations
		{
			get
			{
				var result = new List<IInvoiceChargeInformation>();
				result.AddRange(Charges.Select(c => new InvoiceChargeWrapper(c)));
				result.AddRange(GroupCharges.Select(c => new InvoiceChargeWrapper(c)));
				return result;
			}
		}

		#endregion

		#region ICurrencyConverterDataProvider

		int ICurrencyConverterDataProvider.MaximumDaysToFallback
		{
			get { return 0; }
		}

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new JobComInvoiceHeaderFetchStrategy(this);

		#region IDA63ValueRecalculationParent

		public ZBool DA63NeedsRecalculation
		{
			get { return InvoiceLines?.OfType<IDA63ValueRecalculationParent>()?.Any(x => x.DA63NeedsRecalculation) ?? false; }
		}

		public void RecalculateDA63Values()
		{
			foreach (var line in InvoiceLines.OfType<IDA63ValueRecalculationParent>().Where(x => x.DA63NeedsRecalculation))
			{
				line.RecalculateDA63Values();
			}
		}

		#endregion

		[ResourceStringData("Enterprise.Customs.ZA.Business.JobComInvoiceHeader|JZ_PaymentNo", Caption = "Advance Payment Notification Number", ShortCaption = "APN No")]
		public override ZString JZ_PaymentNo { get => base.JZ_PaymentNo; set => base.JZ_PaymentNo = value; }

		[ResourceStringData("5BAFFAF6-2B2C-4150-A53F-DEFFBC7D2B9E", Caption = "Payment Terms")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.PaymentTermsList))]
		public override ZString JZ_PaymentTerms { get => base.JZ_PaymentTerms; set => base.JZ_PaymentTerms = value; }

		#region Concurrency Handling

		protected override void OnConcurrencyExceptionCore(IEnumerable<IPropertyRecord> propertyRecords)
		{
			if (!IsDeleted)
			{
				base.OnConcurrencyExceptionCore(propertyRecords);
			}
		}

		protected override void OnConcurrencyExceptionAfterMergeCore(IEnumerable<IPropertyRecord> propertyRecords)
		{
			if (!IsDeleted)
			{
				base.OnConcurrencyExceptionAfterMergeCore(propertyRecords);
			}
		}

		#endregion
	}
}
