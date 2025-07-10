using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TW.Business
{
	public partial class CusEntryHeader : AutoTWCusEntryHeader, ITWMessageInfoProvider,
		ICusEntryNumberParent,
		ICusEntryNumEntryStatusListProvider,
		ICusDispositionParent,
		IEntryNumberGeneratorProvider
	{
		public CusEntryHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Schema

		public new abstract class Schema : AutoCusEntryHeader.Schema
		{
			public const string BusinessTaxBaseAmount = nameof(CusEntryHeader.BusinessTaxBaseAmount);
			public const string TotalTaxAmount = nameof(CusEntryHeader.TotalTaxAmount);
			public const string TotalCashTaxAmount = nameof(CusEntryHeader.TotalCashTaxAmount);
			public const string TotalNonCashTaxAmount = nameof(CusEntryHeader.TotalNonCashTaxAmount);
			public const string UCRNumber = nameof(CusEntryHeader.UCRNumber);
			public const string DeclarationType = nameof(CusEntryHeader.DeclarationType);
			public const string DeclarationEntryNumber = nameof(CusEntryHeader.DeclarationEntryNumber);
			public const string CH_TotalIMPFOBAmountInInvoiceCurrency = nameof(CusEntryHeader.CH_TotalIMPFOBAmountInInvoiceCurrency);
			public const string CH_TotalNetWeightInKilograms = nameof(CusEntryHeader.CH_TotalNetWeightInKilograms);
			public const int NetWeightDecimalPlaces = 3;
		}

		#endregion
		public ZString DeclarationType => EntryInstruction?.CEI_Style ?? ZString.Empty;

		public ZPropertyInfo DeclarationTypeInfo => GetZPropertyInfo(Schema.DeclarationType);

		public ZString UCRNumber => EntryInstruction?.UCRNumber ?? ZString.Empty;

		public ZPropertyInfo UCRNumberInfo => GetZPropertyInfo(Schema.UCRNumber);

		[ResourceStringData("415766AD-EC01-9482-4B15-9D32A0F7228D", Caption = "Business Tax Base", ShortCaption = "Business Tax Base", FullDescription = "The base amount for business tax calculation.")]
		[ReadOnly(true)]
		public ZDecimal BusinessTaxBaseAmount
		{
			get
			{
				ZDecimal result = MergedLines.Cast<CusEntryLine>().Where(x => x.RandomLine.JI_VatPymntMthd == DutyTaxPaymentMethodList.Codes.CashPayment || x.RandomLine.JI_VatPymntMthd == DutyTaxPaymentMethodList.Codes.RorPayment).Sum(x => x.CL_ValueForVAT);
				return result.Truncate();
			}
		}

		public ZPropertyInfo BusinessTaxBaseAmountInfo => GetZPropertyInfo(Schema.BusinessTaxBaseAmount);

		[DecimalPlaces(0)]
		[ResourceStringData("9481B2A8-BDEA-B195-42E9-3D557EF7F599", Caption = "Total Tax Amount", ShortCaption = "Total Tax Amount")]
		[ReadOnly(true)]
		public ZDecimal TotalTaxAmount { get { return DutyTaxFeeCharges.Cast<DutyTaxFeeCharge>().Sum(x => x.ChargeAmount); } }

		public ZPropertyInfo TotalTaxAmountInfo => GetZPropertyInfo(Schema.TotalTaxAmount);

		[DecimalPlaces(0)]
		[ResourceStringData("1A4B304A-9A27-4BF9-B6F7-4BEB0ACAE7C6", Caption = "Total Tax Amount (Cash)", ShortCaption = "Total Tax Amount (Cash)", FullDescription = "The total amount (Cash) of the duties, taxes, and fees.")]
		[ReadOnly(true)]
		public ZDecimal TotalCashTaxAmount => DutyTaxFeeCharges.Cast<DutyTaxFeeCharge>().Where(x => x.MethodOfPayment == EntryChargePaymentMethod.Codes.CAS).Sum(x => x.ChargeAmount);

		public ZPropertyInfo TotalCashTaxAmountInfo => GetZPropertyInfo(Schema.TotalCashTaxAmount);

		[DecimalPlaces(0)]
		[ResourceStringData("2AE95015-A57F-46AA-8021-C0387DF85B3A", Caption = "Total Tax Amount (Non-Cash)", ShortCaption = "Total Tax Amount (Non-Cash)", FullDescription = "The total amount (Non-Cash) of the duties, taxes, and fees.")]
		[ReadOnly(true)]
		public ZDecimal TotalNonCashTaxAmount => DutyTaxFeeCharges.Cast<DutyTaxFeeCharge>().Where(x => x.MethodOfPayment == EntryChargePaymentMethod.Codes.DEF).Sum(x => x.ChargeAmount);

		public ZPropertyInfo TotalNonCashTaxAmountInfo => GetZPropertyInfo(Schema.TotalNonCashTaxAmount);

		public ZDecimal CH_ConfirmedBusinessTaxBase => Factory.GetValue(ref confirmedBusinessTaxBase, () =>
		{
			var dutyTaxFeeCodes = new DutyTaxFeeCodeList();
			return EntryPayInfos.Where(x => dutyTaxFeeCodes.ContainsCode(x.C9_TransactionType)).GroupBy(x => x.C9_IncomingPayResponseNo).Sum(g => g.Max(x => x.OtherChargeDeductionAmount));
		});

		CachedProperty<ZDecimal> confirmedBusinessTaxBase;

		public ZDecimal CH_ConfirmedTotalDutyTaxFee => Factory.GetValue(ref confirmedTotalDutyTaxFee, () =>
		{
			var dutyTaxFeeCodes = new DutyTaxFeeCodeList();
			return EntryPayInfos.Where(x => dutyTaxFeeCodes.ContainsCode(x.C9_TransactionType)).Sum(s => s.C9_PaymentAmount);
		});

		CachedProperty<ZDecimal> confirmedTotalDutyTaxFee;

		public ZDecimal CH_ConfirmedTotalDutyTaxFeeDeferred => Factory.GetValue(ref confirmedTotalDutyTaxFeeDeferred, () =>
		{
			var transactionTypes = new DepositTypeCodeList();
			return EntryPayInfos.Where(x => transactionTypes.ContainsCode(x.C9_TransactionType)).Sum(x => x.C9_PaymentAmount);
		});
		CachedProperty<ZDecimal> confirmedTotalDutyTaxFeeDeferred;

		[ReadOnly(true)]
		public override ZDateTime CH_EntryReleaseDate { get => base.CH_EntryReleaseDate; set => base.CH_EntryReleaseDate = value; }

		#region ITWMessageInfoProvider
		ZString ITWMessageInfoProvider.StaffCode => Declaration?.JE_GS_NKCusAgent ?? ZString.Empty;

		ZString ITWMessageInfoProvider.EntryNumberType => EntryNumberType;

		ZString ITWMessageInfoProvider.EntryNumber => EntryNumber;

		ZString ITWMessageInfoProvider.CompanyID => GlbCompany.CurrentCompany.GC_Code;

		ZString ITWMessageInfoProvider.PasswordType => Declaration?.GetCredential()?.GP_PasswordType ?? ZString.Empty;
		#endregion

		#region Overrides

		protected override ZString EntryNumberType
		{
			get
			{
				var result = Declaration?.EntryNumberType ?? ZString.Empty;
				if (result.IsEmpty)
				{
					result = base.EntryNumberType;
				}
				return result;
			}
		}

		public override ZString EntryNumber
		{
			get => base.EntryNumber;
			set
			{
				base.EntryNumber = value;
				if (!IsValidationSuspended)
				{
					Declaration?.MarkAsNeedingValidation();
				}
			}
		}

		public override bool HasBeenLodgedAtCustoms => !CH_EntryStatus.IsEmpty && !IsRejectedByCustoms;

		public bool IsRejectedByCustoms => CH_EntryStatus == EntryStatusCodeList.Codes.ARM && CusDispositions.Cast<CusDisposition>().Any(c => c.CDI_StatusKey == EntryStatusCodeList.Codes.ARM && c.CDI_Type == Common.CusEntryNumber.Categories.CustomsPermitClearanceNumber && !c.CDI_Status.StartsWith(Constants.CDIStatus.B) && !c.CDI_Status.StartsWith(Constants.CDIStatus.F));

		public override bool HasBeenWithdrawn => false;

		public override bool IsWaitingForResponse => JobDeclarationMessageStatusList.IsAwaiting(CH_Status);

		public bool IsWaitingForResponseOrHasBeenLodgedAtCustoms => IsWaitingForResponse || HasBeenLodgedAtCustoms;

		protected override DocumentSupporter CreateNewDocumentSupporter() => new CusEntryHeaderDocumentSupporter(this);

		public override ZString GoodsTypeForDocumentFilter => ChassisNumbers.Any() ? "VHC" : string.Empty;

		internal List<ZString> ChassisNumbers
		{
			get
			{
				var chassisnumbers = new List<ZString>();
				if (EntryInstruction != null && Declaration != null && InvoiceLines != null)
				{
					chassisnumbers = MergedLines.Cast<CusEntryLine>().SelectMany(x => x.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(invoiceLine => invoiceLine.ChassisNumbers)).ToList();
				}
				return chassisnumbers;
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryHeader|CH_DeclarationIncoterm", Caption = "Declaration Incoterm", FullDescription = "The code of the commercial terms published by the International Chamber of Commerce.")]
		[List(nameof(Lookups) + "." + nameof(CusEntryHeaderLookups.IncoTermList))]
		public override ZString CH_DeclarationIncoterm { get => base.CH_DeclarationIncoterm; set => base.CH_DeclarationIncoterm = value; }

		public override void PopulateEntrySubmittedDateIfRequired(ZDateTime? submittedDate = null)
		{
			var date = submittedDate ?? ZDateTime.Now;
			CH_EntrySubmittedDate = date;
			if (Declaration != null)
			{
				Declaration.JE_EntrySubmittedDate = CH_EntrySubmittedDate;
			}
		}
		#endregion

		public DutyTaxFeeChargeCollection DutyTaxFeeCharges
		{
			get
			{
				if (dutyTaxFeeCharges == null)
				{
					dutyTaxFeeCharges = new DutyTaxFeeChargeCollection(this);
				}
				dutyTaxFeeCharges.ShouldRebuildElements();
				return dutyTaxFeeCharges;
			}
		}
		DutyTaxFeeChargeCollection dutyTaxFeeCharges;

		public CusDispositionCollection CusDispositions
		{
			get
			{
				if (cusDisposition == null)
				{
					cusDisposition = new CusDispositionCollection(this);
					cusDisposition.Load();
					cusDisposition.Sort(CusDispositionSchema.CDI_StatusDate.Name, ListSortDirection.Descending);
				}
				return cusDisposition;
			}
		}
		CusDispositionCollection cusDisposition;

		internal bool HasSpecifiedDutyTaxFeeCharges(HashSet<ZString> chargeCodes)
		{
			return DutyTaxFeeCharges.Cast<DutyTaxFeeCharge>().Any(x => chargeCodes.Contains(x.ChargeType));
		}

		internal bool HasB10OrB19Charges => HasSpecifiedDutyTaxFeeCharges(new HashSet<ZString> { DutyTaxFeeCodeList.Codes.B10, DutyTaxFeeCodeList.Codes.B19 });

		internal bool HasB31OrB69Charges => HasSpecifiedDutyTaxFeeCharges(new HashSet<ZString> { DutyTaxFeeCodeList.Codes.B31, DutyTaxFeeCodeList.Codes.B69 });

		internal bool HasB60OrB89Charges => HasSpecifiedDutyTaxFeeCharges(new HashSet<ZString> { DutyTaxFeeCodeList.Codes.B60, DutyTaxFeeCodeList.Codes.B89 });

		public ZDecimal FirstInvoiceCurrencyExRate => FirstInvoiceCurrencyCode == Core.Constants.CurrencyCodes.Taiwan ? new ZDecimal(1) : FirstInvoice?.JZ_InvoiceCurrExRate ?? ZDecimal.Zero;

		public ZString FirstInvoiceCurrencyCode => FirstInvoiceCurrency?.Code ?? ZString.Empty;

		#region DocumentWrappers
		public List<DocumentWrapper> ApplicationAndCertificateDocumentWrappers
		{
			get
			{
				var result = new List<DocumentWrapper>();
				foreach (var entryLine in MergedLines)
				{
					var invoiceLine = entryLine.RandomLine;
					var originalGoodsDescription = invoiceLine.JI_DeclarationGoodsDescription;
					var netWeightInKG = entryLine.CL_Calc_NetWeightInKG;
					var unitCommodityTax = entryLine.UnitCommodityTax;
					var additionalDocuments = entryLine.AdditionalDocuments;
					foreach (var chassisNumber in invoiceLine.ChassisNumbers)
					{
						result.Add(new ApplicationAndCertificateDocumentWrapper(this, chassisNumber, originalGoodsDescription, netWeightInKG, unitCommodityTax, additionalDocuments));
					}
				}
				return result;
			}
		}
		#endregion

		public void CalculateCH_DeclarationIncoterm()
		{
			if (InvoiceIncotermIsBelowOrEqualToFOB)
			{
				CH_DeclarationIncoterm = FirstInvoiceIncoTerm;
			}
			else
			{
				var hasIncludedInLineFreight = HasIncludedInLineCharges("OFT");
				var hasIncludedInLineInsurance = HasIncludedInLineCharges("ONS");
				if (hasIncludedInLineFreight && hasIncludedInLineInsurance)
				{
					CH_DeclarationIncoterm = IncoTerms.CostInsuranceAndFreight;
				}
				else if (hasIncludedInLineFreight && !hasIncludedInLineInsurance)
				{
					CH_DeclarationIncoterm = IncoTerms.CostAndFreight;
				}
				else if (!hasIncludedInLineFreight && hasIncludedInLineInsurance)
				{
					CH_DeclarationIncoterm = IncoTerms.CostAndInsurance;
				}
				else
				{
					CH_DeclarationIncoterm = IncoTerms.FreeOnBoard;
				}
			}
		}

		bool InvoiceIncotermIsBelowOrEqualToFOB => FirstInvoiceIncoTerm == IncoTerms.ExWorks || FirstInvoiceIncoTerm == IncoTerms.FreeAlongsideShip || FirstInvoiceIncoTerm == IncoTerms.FreeOnBoard;

		bool HasIncludedInLineCharges(ZString chargeCode) => InvoiceLines.Any(line => line.ApportionedCharges.Any(charge => charge.J7_ChargeType == chargeCode && charge.J7_IsIncludedInITOT && charge.J7_Amount > 0));

		[DecimalPlaces(0)]
		public override ZDecimal CustomsValue => base.CustomsValue;

		public ZString GetLastSentOutgoingInterchangeNumberByMessageType(ZString messageType) => Messages.GetLastMessage(EDIMessage.ApplicationCodes.TaiwanCustoms, messageType, EDIMessage.Direction.Transmit)?.EM_InterchangeNumber ?? ZString.Empty;

		public ZString MarksAndNumbers
		{
			get
			{
				var marksAndNumbersNote = Declaration?.Shipment?.Notes?.FindByDescription(PredefinedNoteTypes.Instance.MarksAndNumbers.Description);
				var result = marksAndNumbersNote?.FirstOrDefault(c => c.ST_NoteContextModule == nameof(StmNoteContextModule.D))?.ST_NoteText ?? ZString.Empty;

				if (result.IsEmpty)
				{
					var stringBuilder = new ZStringBuilder();
					InvoiceHeaders().OrderBy(x => x.JZ_InvoiceNumber).Select(x => x.TW_MarksAndNumbers).Where(y => !y.IsEmpty).Distinct().ToList().ForEach(z => stringBuilder.Append(z));
					result = stringBuilder.ToStringWithNewLineBetweenAppends();
				}

				if (result.IsEmpty)
				{
					result = marksAndNumbersNote?.FirstOrDefault(c => c.ST_NoteContextModule == nameof(StmNoteContextModule.A))?.ST_NoteText ?? ZString.Empty;
				}

				if (result.IsEmpty)
				{
					result = Core.Constants.ContainerMarking.NoMarks;
				}
				return result;
			}
		}

		public ZPropertyInfo MarksAndNumbersInfo => GetZPropertyInfo(nameof(MarksAndNumbers));

		public override ZString DefaultStatusDescription => EntryStatusCodeList.Descriptions.NotReceive;

		public ZString DeclarationNumber
		{
			get
			{
				var result = Declaration?.EntryNumber ?? ZString.Empty;
				if (result.IsEmpty)
				{
					result = EntryNumber;
					var entryNumberGeneratorProvider = Declaration ?? this as IEntryNumberGeneratorProvider;
					if (!CommonHelper.IsMatchEntryNumber(entryNumberGeneratorProvider, result) && !CommonHelper.IsWaitingForResponseOrHasBeenLodgedAtCustoms(this))
					{
						result = ZString.Empty;
					}
				}
				return result;
			}
		}

		public ZString DeclarationEntryNumber => Factory.GetValue(ref fDeclarationEntryNumberCached, delegate
		{
			var result = Declaration?.EntryNumber ?? ZString.Empty;
			if (result.IsEmpty)
			{
				result = EntryNumber;
			}
			return CommonHelper.FormattedEntryNumber(result);
		});

		CachedProperty<ZString> fDeclarationEntryNumberCached;

		public ZPropertyInfo DeclarationEntryNumberInfo => GetZPropertyInfo(Schema.DeclarationEntryNumber);

		#region Entry Number PlaceHolder

		public ZString EntryNumberForSendingObject
		{
			get
			{
				var result = DeclarationNumber;
				if (result.IsEmpty)
				{
					result = MessageConstants.EntryNumberPlaceHolder;
				}
				return result;
			}
		}

		public void AllocateEntryNumber()
		{
			var entryNumber = Declaration?.EntryNumber ?? ZString.Empty;
			if (!entryNumber.IsEmpty)
			{
				DeleteAllCusEntryNumbers();
				Declaration.EntryNumber = ZString.Empty;
				EntryNumber = entryNumber;
			}
			else
			{
				var entryNumberGeneratorProvider = Declaration ?? this as IEntryNumberGeneratorProvider;
				if (!CommonHelper.IsMatchEntryNumber(entryNumberGeneratorProvider, EntryNumber) && !CommonHelper.IsWaitingForResponseOrHasBeenLodgedAtCustoms(this, true))
				{
					EntryNumber = ZString.Empty;
				}
				if (EntryNumber.IsEmpty)
				{
					DeleteAllCusEntryNumbers();
					AllocateNextEntryNumber();
					if (EntryNumber.IsEmpty && ShouldThrowGenerateEntryNumberException)
					{
						throw new GenerateEntryNumberException(GenerateEntryNumberException.GenerateError);
					}
				}
			}
			Declaration?.RefreshEntryNumberRrelevantInfo();
		}

		bool entryNumberAllocated;

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			if (!saveSucceeded)
			{
				RecoverFromFactoryUnsuccessfulSave();
			}

			base.OnFactorySaved(saveSucceeded);
			entryNumberAllocated = false;
		}

		void RecoverFromFactoryUnsuccessfulSave()
		{
			if (entryNumberAllocated)
			{
				EntryNumber = ZString.Empty;
			}
		}

		void AllocateNextEntryNumber()
		{
			var entryNumber = EntryNumberGenerator.New(this)?.GenerateEntryNumber() ?? ZString.Empty;
			if (!entryNumber.IsEmpty)
			{
				entryNumberAllocated = true;
				EntryNumber = entryNumber;
			}
		}

		class ThrowGenerateEntryNumberExceptionSupporter : IDisposable
		{
			internal ThrowGenerateEntryNumberExceptionSupporter(CusEntryHeader parent)
			{
				this.parent = parent;
				this.parent.shouldThrowGenerateEntryNumberExceptionCount++;
			}

			void IDisposable.Dispose()
			{
				parent.shouldThrowGenerateEntryNumberExceptionCount--;
			}

			readonly CusEntryHeader parent;
		}

		public IDisposable GetGenerateEntryNumberExceptionSupporter()
		{
			return new ThrowGenerateEntryNumberExceptionSupporter(this);
		}

		bool ShouldThrowGenerateEntryNumberException => shouldThrowGenerateEntryNumberExceptionCount > 0;
		int shouldThrowGenerateEntryNumberExceptionCount;
		#endregion

		protected override bool ShouldLogCustomsClearedToEntryHeader => false;

		protected override bool IsStatusChangedToClearedForLoggingCLREvent => IsInDatabase && IsEntryStatusCleared && ((ZString)(CusEntryNumber?.CE_EntryStatusInfo?.OriginalValue ?? ZString.Empty)).IsEmpty;

		public override bool ShouldLogCustomsClearedToDeclarationOrShipment => Declaration?.ActiveEntryHeaders.AreAllEntriesCleared ?? false;

		public override bool IsEntryStatusCleared => !ClearanceStatus.IsEmpty;

		ZString ClearanceStatus => CusEntryNumber?.CE_EntryStatus ?? ZString.Empty;

		public override bool ShouldLogEntryStatus => true;

		#region ICusEntryNumberParent
		bool ICusEntryNumberParent.CanBeChangedOrDeleted(Common.CusEntryNumber entryNumber, out string errMsg)
		{
			errMsg = string.Empty;
			return string.IsNullOrEmpty(errMsg);
		}

		void ICusEntryNumberParent.EntryNumberChanged(ZString oldValue, ZString newValue)
		{
			Logs.AddNew(Events.CustomsNumberEntered, EntryNumber);
		}

		string ICusEntryNumberParent.EntryNumberChangedCallStack => ZString.Empty;
		#endregion

		protected override Common.CusEntryNumber CreateCusEntryNumber()
		{
			var cusEntryNumber = base.CreateCusEntryNumber();
			cusEntryNumber.Parent = this;
			return cusEntryNumber;
		}

		protected override void ResetCachedValues()
		{
			base.ResetCachedValues();
			firstInvoice = null;
		}

		JobComInvoiceHeader FirstInvoice
		{
			get
			{
				if (firstInvoice == null || firstInvoice.IsDeleted)
				{
					firstInvoice = InvoiceHeaders().OrderBy(x => x.JZ_InvoiceNumber).FirstOrDefault();
				}
				return firstInvoice;
			}
		}
		JobComInvoiceHeader firstInvoice;

		public RefCurrency FirstInvoiceCurrency => FirstInvoice?.Invoice_Currency;

		public ZBool IsCurrencySameAsFirstInvoiceCurrency(ZString currencyCode)
		{
			return !currencyCode.IsEmpty && FirstInvoiceCurrencyCode == currencyCode;
		}

		internal ZString FirstInvoiceIncoTerm => FirstInvoice?.IncoTerm ?? ZString.Empty;

		ZBool IsExportAndTheFirstInvoiceIncoTermIsExWorks => IsExport && FirstInvoiceIncoTerm == IncoTerms.ExWorks;

		ZBool IsExportAndTheFirstInvoiceIncoTermIsNotExWorks => IsExport && FirstInvoiceIncoTerm != IncoTerms.ExWorks;

		JobComInvoiceGroupHeader GroupHeader => FirstInvoice?.GroupHeader;

		JobComInvChargeCollection<GroupInvoiceCharge> GroupCharges => GroupHeader?.Charges;

		ChargeCodeChargeKey GetChargeCodeChargeKey(ZString chargeCode)
		{
			return Declaration?.IncoTermAndChargeFactory?.GetCharge(chargeCode)?.ChargeCodeChargeKey;
		}

		JobComInvCharge[] AllInvoiceCharges
		{
			get
			{
				var invoiceCharges = InvoiceHeaders().SelectMany(x => x.Charges.Cast<JobComInvCharge>());
				var allInvoiceCharges = GroupCharges == null ? invoiceCharges : invoiceCharges.Concat(GroupCharges.Cast<JobComInvCharge>());
				return allInvoiceCharges.Where(x => !x.IsDeleted && !x.IsEmpty).ToArray();
			}
		}

		Money ConvertToLocalAmountExact(Money moneyAmount)
		{
			return CurrencyConverter == null ? Money.Empty : CurrencyConverter.ConvertExact(moneyAmount, LocalCurrency);
		}

		Money ConvertToFirstInvoiceCurrencyExact(Money moneyAmount)
		{
			return CurrencyConverter == null ? Money.Empty : CurrencyConverter.ConvertExact(moneyAmount, FirstInvoiceCurrency);
		}

		public Money CH_InvoiceLineTotal
		{
			get
			{
				var result = new Money(ZDecimal.Zero, FirstInvoiceCurrency);
				foreach (var invoice in InvoiceHeaders())
				{
					var invoiceLineTotal = new Money(invoice.InvoiceLineTotal, invoice.Invoice_Currency);
					var invoiceLineTotalInFirstInvoiceCurrency = ConvertToFirstInvoiceCurrencyExact(invoiceLineTotal);
					result = CurrencyConverter.Add(result, invoiceLineTotalInFirstInvoiceCurrency);
				}
				return result;
			}
		}

		public ZDecimal CH_TotalInternationalInsuranceAmountInInvoiceCurrency => Factory.GetValue(ref totalInternationalInsuranceAmountInInvoiceCurrencyCached, () => OverseasInsurance.Amount.Round(2));

		CachedProperty<ZDecimal> totalInternationalInsuranceAmountInInvoiceCurrencyCached;

		public override Money OverseasInsurance
		{
			get
			{
				var chargeCodeChargeKey = GetChargeCodeChargeKey(Common.CustomsChargeTypeList.Codes.OverseasInsurance);
				var result = GetOverseasFreightAndInsuranceAmount(chargeCodeChargeKey);
				return result;
			}
		}

		public ZDecimal CH_TotalInternationalFreightAmountInInvoiceCurrency => Factory.GetValue(ref totalInternationalFreightAmountInInvoiceCurrencyCached, () => OverseasFreight.Amount.Round(2));

		CachedProperty<ZDecimal> totalInternationalFreightAmountInInvoiceCurrencyCached;

		public override Money OverseasFreight
		{
			get
			{
				var chargeCodeChargeKey = GetChargeCodeChargeKey(Common.CustomsChargeTypeList.Codes.OverseasFreight);
				var result = GetOverseasFreightAndInsuranceAmount(chargeCodeChargeKey);
				return result;
			}
		}

		public ZDecimal CH_TotalAdditionsInInvoiceCurrency => Factory.GetValue(ref totalAdditionsInInvoiceCurrencyCached, () => CH_TotalAdditionsMoney.Amount.Round(2));

		CachedProperty<ZDecimal> totalAdditionsInInvoiceCurrencyCached;

		Money CH_TotalAdditionsMoney
		{
			get
			{
				var result = Money.Empty;
				if (IsExportAndTheFirstInvoiceIncoTermIsNotExWorks)
				{
					result = GetChargesAmount(x => x.J7_ChargeType != Common.CustomsChargeTypeList.Codes.OverseasFreight
					&& x.J7_ChargeType != Common.CustomsChargeTypeList.Codes.OverseasInsurance
					&& !x.J7_IsDutiable
					&& x.J7_IsGSTApplicable
					&& !x.J7_IsIncludedInITOT);
				}
				else if (IsExportAndTheFirstInvoiceIncoTermIsExWorks)
				{
					var additions = GetChargesAmount(x => x.J7_ChargeType != Common.CustomsChargeTypeList.Codes.OverseasFreight
					&& x.J7_ChargeType != Common.CustomsChargeTypeList.Codes.OverseasInsurance
					&& x.J7_IsDutiable
					&& !x.J7_IsGSTApplicable
					&& !x.J7_IsIncludedInITOT);
					var deductions = GetChargesAmount(x => x.J7_ChargeType != Common.CustomsChargeTypeList.Codes.OverseasFreight
					&& x.J7_ChargeType != Common.CustomsChargeTypeList.Codes.OverseasInsurance
					&& !x.J7_IsDutiable
					&& x.J7_IsGSTApplicable
					&& x.J7_IsIncludedInITOT);
					result = CurrencyConverter.Subtract(additions, deductions);
				}
				else if (IsImport)
				{
					result = GetChargesAmount(x => x.J7_ChargeType != Common.CustomsChargeTypeList.Codes.OverseasFreight
					&& x.J7_ChargeType != Common.CustomsChargeTypeList.Codes.OverseasInsurance
					&& x.J7_IsDutiable
					&& !x.J7_IsIncludedInITOT);
				}
				return result;
			}
		}

		public ZDecimal CH_TotalDeductionsInInvoiceCurrency => Factory.GetValue(ref totalDeductionsInInvoiceCurrencyCached, () => CH_TotalDeductionsMoney.Amount.Round(2));

		CachedProperty<ZDecimal> totalDeductionsInInvoiceCurrencyCached;

		Money CH_TotalDeductionsMoney
		{
			get
			{
				var result = new Money(0, FirstInvoiceCurrency);
				if (IsExportAndTheFirstInvoiceIncoTermIsNotExWorks)
				{
					result = GetChargesAmount(x => x.J7_ChargeType != Common.CustomsChargeTypeList.Codes.OverseasFreight
					&& x.J7_ChargeType != Common.CustomsChargeTypeList.Codes.OverseasInsurance
					&& x.J7_IsDutiable
					&& !x.J7_IsGSTApplicable
					&& x.J7_IsIncludedInITOT);
				}
				else if (IsImport)
				{
					result = GetChargesAmount(x => x.J7_ChargeType != Common.CustomsChargeTypeList.Codes.OverseasFreight
					&& x.J7_ChargeType != Common.CustomsChargeTypeList.Codes.OverseasInsurance
					&& !x.J7_IsDutiable
					&& x.J7_IsIncludedInITOT);
				}
				return result;
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryHeader|CH_TotalIMPFOBAmountInInvoiceCurrency", Caption = "FOB (17)", FullDescription = "The total FOB value of this entry.")]
		public ZDecimal CH_TotalIMPFOBAmountInInvoiceCurrency => Factory.GetValue(ref totalIMPFOBAmountInInvoiceCurrencyCached, () => CH_TotalIMPFOBAmountMoney.Amount.Round(2));

		CachedProperty<ZDecimal> totalIMPFOBAmountInInvoiceCurrencyCached;

		public ZPropertyInfo CH_TotalIMPFOBAmountInInvoiceCurrencyInfo => GetZPropertyInfo(Schema.CH_TotalIMPFOBAmountInInvoiceCurrency);

		Money CH_TotalIMPFOBAmountMoney
		{
			get
			{
				var additions = GetChargesAmount(x => x.J7_IsStatisticalValueApplicable && !x.J7_IsIncludedInITOT);
				var deductions = GetChargesAmount(x => !x.J7_IsStatisticalValueApplicable && x.J7_IsIncludedInITOT);
				var result = CH_InvoiceLineTotal;
				result = CurrencyConverter.Add(result, additions);
				result = CurrencyConverter.Subtract(result, deductions);
				return result;
			}
		}

		[ResourceStringData("f10503db-ad06-4046-91ba-01971d425ddd", Caption = "Total Inv. Amt. (16)", FullDescription = "The total amount of the commercial invoice.")]
		public ZDecimal CH_TotalEXPDisbursedAmountInInvoiceCurrency => IsInProcessOfMerging
			? ZDecimal.Zero
			: Factory.GetValue(ref totalEXPDisbursedAmountInInvoiceCurrencyCached,
				() => CH_TotalEXPDisbursedAmountMoney.Amount.Round(2));

		CachedProperty<ZDecimal> totalEXPDisbursedAmountInInvoiceCurrencyCached;

		public ZPropertyInfo CH_TotalEXPDisbursedAmountInInvoiceCurrencyInfo => GetZPropertyInfo(nameof(CH_TotalEXPDisbursedAmountInInvoiceCurrency));

		Money CH_TotalEXPDisbursedAmountMoney
		{
			get
			{
				var additions = GetChargesAmount(x => x.J7_IsGSTApplicable && !x.J7_IsIncludedInITOT);
				var deductions = GetChargesAmount(x => !x.J7_IsGSTApplicable && x.J7_IsIncludedInITOT);
				var result = CH_InvoiceLineTotal;
				result = CurrencyConverter.Add(result, additions);
				result = CurrencyConverter.Subtract(result, deductions);
				return result;
			}
		}

		internal ZDecimal CH_TotalInvoiceAmountInInvoiceCurrency => Factory.GetValue(ref totalInvoiceAmountInInvoiceCurrencyCached, () => CH_TotalInvoiceAmountMoney.Amount.Round(2));

		CachedProperty<ZDecimal> totalInvoiceAmountInInvoiceCurrencyCached;

		Money CH_TotalInvoiceAmountMoney
		{
			get
			{
				var result = new Money(ZDecimal.Zero, FirstInvoiceCurrency);
				foreach (var invoice in InvoiceHeaders())
				{
					var invoiceCurrencyCode = invoice.JZ_RX_NKInvoice_Currency;
					if (IsCurrencySameAsFirstInvoiceCurrency(invoiceCurrencyCode))
					{
						result = CurrencyConverter.Add(result, invoice.InvoiceAmount);
					}
					else
					{
						var invoiceAmountInFirstInvoiceHeaderCurrency = ConvertToFirstInvoiceCurrencyExact(invoice.JZ_InvoiceAmountInLocalCurrencyMoney);
						result = CurrencyConverter.Add(result, invoiceAmountInFirstInvoiceHeaderCurrency);
					}
				}
				return result;
			}
		}

		public ZDecimal CH_TotalRorCustomsValueInInvoiceCurrency => Factory.GetValue(ref totalRorCustomsValueInInvoiceCurrencyCached, () => CH_TotalRorCustomsValueMoney.Amount.Round(2));

		CachedProperty<ZDecimal> totalRorCustomsValueInInvoiceCurrencyCached;

		Money CH_TotalRorCustomsValueMoney
		{
			get
			{
				var overseasFreightAndInsurance = GetChargesAmount(x => (x.J7_ChargeType == Common.CustomsChargeTypeList.Codes.OverseasFreight ||
					x.J7_ChargeType == Common.CustomsChargeTypeList.Codes.OverseasInsurance)
					&& !x.J7_IsIncludedInITOT);
				return CurrencyConverter.Add(CH_InvoiceLineTotal, overseasFreightAndInsurance);
			}
		}

		public ZDecimal CH_TotalCustomsValueInInvoiceCurrency => Factory.GetValue(ref totalCustomsValueInInvoiceCurrencyCached, () => CH_TotalCustomsValueMoney.Amount.Round(2));

		CachedProperty<ZDecimal> totalCustomsValueInInvoiceCurrencyCached;

		Money CH_TotalCustomsValueMoney
		{
			get
			{
				var result = Money.Empty;
				if (IsImport)
				{
					result = CurrencyConverter.Add(CH_TotalIMPFOBAmountMoney, OverseasFreight);
					result = CurrencyConverter.Add(result, OverseasInsurance);
					result = CurrencyConverter.Add(result, CH_TotalAdditionsMoney);
					result = CurrencyConverter.Subtract(result, CH_TotalDeductionsMoney);
				}
				else if (IsExport)
				{
					if (FirstInvoiceIncoTerm == IncoTerms.ExWorks)
					{
						result = CurrencyConverter.Add(CH_TotalEXPDisbursedAmountMoney, CH_TotalAdditionsMoney);
					}
					else
					{
						result = CurrencyConverter.Subtract(CH_TotalEXPDisbursedAmountMoney, OverseasFreight);
						result = CurrencyConverter.Subtract(result, OverseasInsurance);
						result = CurrencyConverter.Subtract(result, CH_TotalAdditionsMoney);
						result = CurrencyConverter.Add(result, CH_TotalDeductionsMoney);
					}
				}
				return result;
			}
		}

		public ZDecimal CH_TotalCustomsValueInLocalCurrency => Factory.GetValue(ref totalCustomsValueInInLocalCurrencyCached, () => ConvertToLocalAmountExact(CH_TotalCustomsValueMoney).Amount.Round(0));

		CachedProperty<ZDecimal> totalCustomsValueInInLocalCurrencyCached;

		public ZDecimal CH_CustomsFactor => CH_TotalInvoiceAmountInInvoiceCurrency.IsDefault ? 0m : CH_TotalCustomsValueInInvoiceCurrency / CH_TotalInvoiceAmountInInvoiceCurrency;

		public ZDecimal CH_RorCustomsFactor => CH_TotalInvoiceAmountInInvoiceCurrency.IsDefault ? 0m : CH_TotalRorCustomsValueInInvoiceCurrency / CH_TotalInvoiceAmountInInvoiceCurrency;

		Money GetChargesAmount(Func<JobComInvCharge, bool> predicate)
		{
			var result = Money.Empty;
			var charges = AllInvoiceCharges.Where(predicate);
			foreach (var charge in charges)
			{
				var chargeMoney = IsCurrencySameAsFirstInvoiceCurrency(charge.J7_RX_NKCurrency) ? charge.Money : ConvertToFirstInvoiceCurrencyExact(charge.MoneyInLocalCurrency).Round(2);
				result = CurrencyConverter.Add(result, chargeMoney);
			}
			return result;
		}

		Money GetOverseasFreightAndInsuranceAmount(ChargeCodeChargeKey chargeKey, Func<JobComInvCharge, bool> predicate = null)
		{
			var result = Money.Empty;
			if (chargeKey != null)
			{
				var effectiveOverseasFreightAndInsuranceCharges = predicate == null ? AllInvoiceCharges : AllInvoiceCharges.Where(predicate);
				foreach (var charge in effectiveOverseasFreightAndInsuranceCharges)
				{
					if (charge.WithKey(chargeKey))
					{
						var chargeMoney = IsCurrencySameAsFirstInvoiceCurrency(charge.J7_RX_NKCurrency) ? charge.Money : ConvertToFirstInvoiceCurrencyExact(charge.MoneyInLocalCurrency).Round(2);
						result = CurrencyConverter.Add(result, chargeMoney);
					}
				}
			}
			return result;
		}

		public ZDecimal RorTPFCashAmountCalculation => Factory.GetValue(ref rorTPFCashAmountCalculationCached, () =>
		{
			var taxOrFeeValue = new RefCusTaxOrFee.Loader(Factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Taiwan, UniversalReferenceConstants.RefCusTaxOrFeeCodes.TPF, Declaration.DeclarationDate)?.ZZF_Value ?? ZDecimal.Zero;
			var result = ZDecimal.Zero;
			if (taxOrFeeValue != ZDecimal.Zero)
			{
				result = MergedLines.Where(line => line.IsROR && !line.Fees.HasOverriddenFeeOfGivenCode(UniversalReferenceConstants.RefCusTaxOrFeeCodes.TPF)).Sum(line =>
				{
					var baseValue = line.CL_Calc_RAPRORPriceLocalAmount.Round(0);
					return Utilities.Round(baseValue * taxOrFeeValue, 3);
				});
			}
			return result;
		});

		CachedProperty<ZDecimal> rorTPFCashAmountCalculationCached;

		public override ZGuid CH_JE
		{
			get { return base.CH_JE; }
			set
			{
				var hasChanged = CH_JE != value;
				base.CH_JE = value;
				if (hasChanged && !IsCopying)
				{
					if (Declaration != null)
					{
						AllEntryLines.MarkAsNeedingValidationIncludingChildren();
					}
				}
			}
		}

		CodeDescriptionPairList ICusEntryNumEntryStatusListProvider.EntryStatusList => Factory.GetCachedValue<ClearanceStatusCodeList>();
		void ICusEntryNumEntryStatusListProvider.OnEntryStatusSet(ZString entryType, ZString newValue) { }
		#region ICustomsCharges members
		protected override ICustomsCharges GetCustomsChargesProvider()
		{
			return new InterfaceImplementations.CusEntryHeaderCustomsCharges(this);
		}
		#endregion

		#region CH_TotalNetWeightInKilograms
		[ResourceStringData("4AA70E9C-FDC2-4149-8EC5-ED051A5D9D41", Caption = "Total Net Weight (KG)")]
		[DecimalPlaces(Schema.NetWeightDecimalPlaces)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1068:DoNotUseMathRound", Justification = "Baseline")]
		public ZDecimal CH_TotalNetWeightInKilograms => Factory.GetValue(ref netWeightKilogramsCached, () => Math.Round((ZDecimal)AllEntryLines.Cast<CusEntryLine>().Sum(x => x.EffectiveNetWeight.InKilogramsSafe), Schema.NetWeightDecimalPlaces));

		CachedProperty<ZDecimal> netWeightKilogramsCached;

		public ZPropertyInfo CH_TotalNetWeightInKilogramsInfo => GetZPropertyInfo(nameof(CH_TotalNetWeightInKilograms));

		internal bool IsTotalNetWeightGreaterThanTotalGrossWeight => Factory.GetValue(ref isTotalNetWeightGreaterThanTotalGrossWeightCached, () => Declaration is JobDeclaration declaration && CH_TotalNetWeightInKilograms > declaration.GrossWeight.InKilogramsSafe);
		CachedProperty<bool> isTotalNetWeightGreaterThanTotalGrossWeightCached;
		#endregion

		#region IEntryNumberGeneratorProvider members
		ZDateTime IEntryNumberGeneratorProvider.EntryNumberDate => EntryInstruction?.CEI_DateForDuty ?? ZDateTime.Empty;

		ZPropertyInfo IEntryNumberGeneratorProvider.EntryNumberPart1Info => EntryInstruction?.CEI_CustomsOfficeInfo;

		ZPropertyInfo IEntryNumberGeneratorProvider.CustomsBrokerageBoxNumberInfo => EntryInstruction?.CEI_BoxNumberInfo;

		ZString IEntryNumberGeneratorProvider.SequenceNumber => ZString.Empty;

		ZString IEntryNumberGeneratorProvider.ShipmentType => Declaration?.JE_MessageType ?? ZString.Empty;

		ZString IEntryNumberGeneratorProvider.EntryNumberType => EntryNumberType;

		GlbCompany IEntryNumberGeneratorProvider.Company => Declaration?.Company ?? GlbCompany.CurrentCompany;

		EntryNumberGeneratorCategory IEntryNumberGeneratorProvider.GetEntryNumberGeneratorCategory() => EntryInstruction?.GetEntryNumberGeneratorCategory() ?? EntryNumberGeneratorCategory.None;

		ZPropertyInfo IEntryNumberGeneratorProvider.EntryNumberPart2Info => EntryInstruction?.CEI_StyleInfo;

		EnterpriseBusinessObject IEntryNumberGeneratorProvider.EntryNumberGeneratorProviderBusinessObject => this;
		#endregion

		#region ICusDispositionParent
		ZString ICusDispositionParent.Type => ZString.Empty;
		ZString ICusDispositionParent.ParentTableCode => CusEntryHeaderSchema.Constants.Prefix;
		BusinessObject ICusDispositionParent.CollectionMaster => this;
		ZString ICusDispositionParent.GetStatusDescription(ZString status) => ZString.Empty;
		#endregion

		IEnumerable<IStorageDocsBaseCollection> GetAllEDocsView()
		{
			if (this is IDocManagerSupport docManagerSupport && docManagerSupport.DocManagerInfo is DocManagerInfo docManagerInfo)
			{
				yield return docManagerInfo.EDocsView;
			}

			if (Declaration is JobDeclaration declaration)
			{
				foreach (var doc in declaration.GetAllEDocs())
				{
					yield return doc;
				}
			}
		}

		internal HashSet<IStorageDocsBaseCollection> GetAllEDocs() => Factory.GetValue(ref allEDocsCached, () => GetAllEDocsView().ToHashSet());
		CachedProperty<HashSet<IStorageDocsBaseCollection>> allEDocsCached;
	}
}
