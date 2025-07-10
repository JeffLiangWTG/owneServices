using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NO.Business;

public class CusEntryHeader : AutoNOCusEntryHeader
	, IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider
	, Integration.Customs.NO.ICusEntryHeader
	, Integration.Customs.NO.IEmmaMessageGenerationProcessorProvider
{
	// ReSharper disable once ConvertToPrimaryConstructor Reason: this breaks DEBUG build on DAT at AspectExtractor
	public CusEntryHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : AutoNOCusEntryHeader.Schema
	{
		public const string Style = nameof(Style);
		public const string Description = nameof(Description);
		public const string Procedure = nameof(Procedure);
		public const string ExciseDutyAmount = nameof(ExciseDutyAmount);
		public const string CIFAmount = nameof(CIFAmount);
		public const string TotalAmount = nameof(TotalAmount);
		public const string VatAmount = nameof(VatAmount);
		public const string PhaseStatusDescription = nameof(PhaseStatusDescription);
		public const int NO_PaymentMethodMaxLength = 1;
	}

	public new class Loader : Customs.Business.CusEntryHeader.Loader
	{
		public Loader(BusinessObjectFactory factory) : base(factory)
		{
		}

		public CusEntryHeader GetByBGMReferenceNumber(string referenceNumber)
		{
			if (string.IsNullOrEmpty(referenceNumber) || referenceNumber.Length != ReferenceNumberWrapper.ReferenceNumberWithoutVersionLength)
			{
				return null;
			}

			var query = new ZQuery();
			query.AddToFilter(CusEntryHeaderSchema.CH_BGMReference, SQLComparisonOperator.Like, FormattableString.Invariant($"{referenceNumber}%"));
			return Factory.LoadTop1<CusEntryHeader>(query);
		}
	}

	public new CusEntryHeaderLookups Lookups => (CusEntryHeaderLookups)base.Lookups;

	protected override Customs.Business.CusEntryHeaderLookups GetNewLookups() => new CusEntryHeaderLookups(this);

	public new CusEntryLine RandomEntryLine => (CusEntryLine)base.RandomEntryLine;

	internal JobDeclaration GetOriginalDeclaration()
	{
		var declaration = Declaration;
		while (declaration?.JE_CopyStatus == (ZString?)NODeclarationCopyStatus.Codes.Recalculation)
		{
			if (declaration.ParentRelatedDeclaration is not JobDeclaration declarationParent)
			{
				return declaration;
			}
			declaration = declarationParent;
		}
		return declaration;
	}

	public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

	public new JobComInvoiceHeader[] InvoiceHeaders => (JobComInvoiceHeader[])base.InvoiceHeaders;

	public override ZInt CH_ClusterKey
	{
		get => base.CH_ClusterKey;
		set
		{
			var oldValue = CH_ClusterKey;
			base.CH_ClusterKey = value;
			if (!IsCopying && oldValue != CH_ClusterKey)
			{
				Declaration?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString CH_EntryStatus
	{
		get => base.CH_EntryStatus;
		set
		{
			var oldValue = CH_EntryStatus;
			base.CH_EntryStatus = value;
			if (!IsCopying && oldValue != CH_EntryStatus)
			{
				Declaration?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZGuid CH_JE
	{
		get => base.CH_JE;
		set
		{
			var oldValue = CH_JE;
			base.CH_JE = value;
			if (!IsCopying && oldValue != CH_JE)
			{
				Declaration?.MarkAsNeedingValidation();
			}
		}
	}

	[ResourceStringData("C9030ABC-C5D3-389D-49D6-7688D9C72CCB", Caption = "Message from customs", FullDescription = "Message from customs. Basis for decision.")]
	public override ZString CH_ReCalcReplyMessage
	{
		get => base.CH_ReCalcReplyMessage;
		set => base.CH_ReCalcReplyMessage = value;
	}

	[ResourceStringData("F4CACB4D-2973-558C-4CAD-7BEC0B4DD9CB", Caption = "Reason", FullDescription = "Free text description of the reason. Extra references if needed.")]
	public override ZString CH_ReCalcReason
	{
		get => base.CH_ReCalcReason;
		set => base.CH_ReCalcReason = value;
	}

	[List(nameof(Lookups) + "." + nameof(CusEntryHeaderLookups.CH_ReCalcOrigDecl_List))]
	[ResourceStringData("22B8DFBA-822A-4B90-479D-4D854B162209", Caption = "Original Id", FullDescription = "Original declaration id")]
	public override ZString CH_ReCalcOrigDecl
	{
		get { return base.CH_ReCalcOrigDecl; }
		set
		{
			var oldValue = base.CH_ReCalcOrigDecl;
			base.CH_ReCalcOrigDecl = value;
			if (value != oldValue && !IsValidationSuspended && !IsCopying)
			{
				if (Declaration is JobDeclaration jobDeclaration)
				{
					foreach (var cusEntryHeader in jobDeclaration.CustomsEntryHeaders)
					{
						cusEntryHeader.Validation.ValidateCH_ReCalcOrigDecl();
					}
				}
				else
				{
					Validation.ValidateCH_ReCalcOrigDecl();
				}
			}
			CH_ReCalcOrigDeclInfo.RefreshBinding();
		}
	}

	[List(nameof(Lookups) + "." + nameof(CusEntryHeaderLookups.CH_ReCalcCaseCode_List))]
	[ResourceStringData("E0225723-FFC1-85B9-4003-0B0A9FB484D3", Caption = "Case code", FullDescription = "Case code for re-calculation of existing customs declaration. The case code denotes the nature and reason for the re-calculation.")]
	public override ZString CH_ReCalcCaseCode
	{
		get { return base.CH_ReCalcCaseCode; }
		set { base.CH_ReCalcCaseCode = value; }
	}

	[List(nameof(Lookups) + "." + nameof(CusEntryHeaderLookups.CH_ReCalcDeclType_List))]
	[ResourceStringData("1058C75E-546D-02AE-4842-343E8BBEDB7F", Caption = "Select type", FullDescription = "The type of declaration selected for re-calculation or statistics.")]
	public override ZString CH_ReCalcDeclType
	{
		get { return base.CH_ReCalcDeclType; }
		set { base.CH_ReCalcDeclType = value; }
	}

	[List(nameof(Lookups) + "." + nameof(CusEntryHeaderLookups.PaymentMethodCodeList))]
	public override ZString CH_PaymentMethod
	{
		get { return base.CH_PaymentMethod; }
		set { base.CH_PaymentMethod = value; }
	}

	public new JobComInvoiceHeader RandomHeader => (JobComInvoiceHeader)base.RandomHeader;

	public new CusEntryHeader Clone() => (CusEntryHeader)base.Clone();

	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	public new ICusEntryLineCollection<CusEntryLine> MergedLines => (ICusEntryLineCollection<CusEntryLine>)base.MergedLines;

	public CusEntryHeaderFeeCollection Fees
	{
		get
		{
			var fees = new CusEntryHeaderFeeCollection();
			fees.AddRange(
				MergedLines
					.SelectMany(entry => entry.Fees.Cast<CusEntryLineFee>())
					.Where(fee => !fee.CF_IsLandedCostOnly)
					.GroupBy(fee => fee.CF_DutyCode)
					.Select(grouping => new CusEntryHeaderFee { Duty = grouping.Key, Amount = grouping.Sum(fee => fee.CF_ChargeAmount) })
			);
			fees.Sort<CusEntryHeaderFee>(DutyComparer.Comparison);
			return fees;
		}
	}

	[ChildEditable(true)]
	public new IAllCusEntryLineCollection<CusEntryLine> AllEntryLines => (IAllCusEntryLineCollection<CusEntryLine>)base.AllEntryLines;

	[ChildEditable(true)]
	public new ICusEntryHeaderChargesCollection<CusEntryHeaderCharges> Charges => (CusEntryHeaderChargesCollection<CusEntryHeaderCharges>)base.Charges;

	public new CusEntryHeaderValidation Validation => (CusEntryHeaderValidation)base.Validation;

	protected override ICusEntryLineCollection<Customs.Business.CusEntryLine> GetMergedLineCollection() => new CusEntryLineCollection<CusEntryLine>(this);

	protected override IAllCusEntryLineCollection<Customs.Business.CusEntryLine> GetAllEntryLinesCollection() => new AllCusEntryLineCollection<CusEntryLine>(this);

	protected override ICusEntryHeaderChargesCollection<Customs.Business.CusEntryHeaderCharges> CreateNewCusEntryHeaderChargesCollection() => new CusEntryHeaderChargesCollection<CusEntryHeaderCharges>(this);

	protected override Customs.Business.CusEntryHeaderValidation GetNewValidation() => new CusEntryHeaderValidation(this);

	public void DefaultPaymentMethod()
	{
		if (!HasPayments)
		{
			CH_PaymentMethod = NOPaymentMethodCodeList.Codes.NoDutiesOrVatPayable;
		}
		else if (Declaration.HasImporterWithDeferredCustomsPaymentAccount)
		{
			CH_PaymentMethod = NOPaymentMethodCodeList.Codes.ImportersDeferred;
		}
		else if(Style == TemporaryImportEntryType)
		{
			CH_PaymentMethod = NOPaymentMethodCodeList.Codes.Cash;
		}
		else
		{
			CH_PaymentMethod = NOPaymentMethodCodeList.Codes.ForwardersDayCredit;
		}
	}

	public virtual bool HasPayments => !TotalPaymentAmount.IsEmpty;

	const string TemporaryImportEntryType = "5";

	public ZString Style => EntryInstruction?.CEI_Style ?? ZString.Empty;

	public ZString Description => EntryInstruction?.CEI_Description ?? ZString.Empty;

	public ZString Procedure => EntryInstruction?.CEI_Procedure ?? ZString.Empty;

	public ZDecimal CIFAmount => CIF.Amount;

	[DecimalPlaces(0)]
	[DecimalPrecision(16)]
	[ReadOnly(true)]
	public ZDecimal CustomsDutyAmount => Factory.GetValue(ref customsDutyAmountCached, () => Fees.GetAmount(x => x.IsCustomsDuty || x.IsAgriculturalDuty));
	CachedProperty<ZDecimal> customsDutyAmountCached;

	[DecimalPlaces(0)]
	[DecimalPrecision(16)]
	[ReadOnly(true)]
	public ZDecimal ExciseDutyAmount => Factory.GetValue(ref exciseDutyAmountCached, () => Fees.GetAmount(x => x.IsExciseDuty));
	CachedProperty<ZDecimal> exciseDutyAmountCached;

	[DecimalPlaces(0)]
	[DecimalPrecision(16)]
	[ReadOnly(true)]
	public ZDecimal VatAmount => Factory.GetValue(ref vatAmountCached, () => Fees.GetAmount(x => x.IsVAT));
	CachedProperty<ZDecimal> vatAmountCached;

	[DecimalPlaces(0)]
	[DecimalPrecision(16)]
	[ReadOnly(true)]
	public ZDecimal TotalAmount => Factory.GetValue(ref totalAmountCached, () => CustomsDutyAmount + ExciseDutyAmount + VatAmount);
	CachedProperty<ZDecimal> totalAmountCached;

	public ZDecimal TotalPaymentAmount => Factory.GetValue(ref totalPaymentAmount, GetTotalPaymentAmount);
	CachedProperty<ZDecimal> totalPaymentAmount;

	protected virtual ZDecimal GetTotalPaymentAmount() => Declaration?.HasImporterWithMVARegistration switch
	{
		true => CustomsDutyAmount + ExciseDutyAmount,
		_ => VatAmount + CustomsDutyAmount + ExciseDutyAmount,
	};

	[DecimalPlaces(0)]
	[DecimalPrecision(16)]
	[ReadOnly(true)]
	public override ZDecimal TotalDutyAmount => base.TotalDutyAmount;

	public override bool HasBeenWithdrawn => false;

	protected override ZString EntryNumberType => CusEntryNumberTypes.Norway.CustomsEntryReleaseNumber;

	public ZString EntryReleaseNumber => EntryReleaseCusEntryNumber?.CE_EntryNum ?? ZString.Empty;

	public ZDateTime EntryReleaseNumberIssueDate => EntryReleaseCusEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;

	public ZDateTime EntryReleaseNumberExpiryDate => EntryReleaseCusEntryNumber?.CE_ExpiryDate ?? ZDateTime.Empty;

	public ZString EntryReferenceNumberEntryStatus => EntryReleaseCusEntryNumber?.CE_EntryStatus ?? ZString.Empty;

	public void SetEntryReleaseNumber(ZString entryReleaseNumber, ZDateTime? issueDate = null, ZString? entryStatus = null, ZDateTime? expiryDate = null)
	{
		var cusEntryNumber = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.Norway.CustomsEntryReleaseNumber, CountryCode);
		cusEntryNumber.CE_EntryIsSystemGenerated = true;
		cusEntryNumber.CE_EntryNum = entryReleaseNumber;

		if (issueDate.HasValue)
		{
			cusEntryNumber.CE_IssueDate = issueDate.Value;
		}

		if (entryStatus.HasValue)
		{
			cusEntryNumber.CE_EntryStatus = entryStatus.Value;
		}

		if (expiryDate.HasValue)
		{
			cusEntryNumber.CE_ExpiryDate = expiryDate.Value;
		}
	}

	public void SetEntryReleaseDate(ZDateTime releaseDate)
	{
		if(releaseDate.IsValid)
		{
			CH_EntryReleaseDate = releaseDate;
		}
	}

	[ResourceStringData("E614F641-91BD-6EB9-439D-AE6AC202D8CE", Caption = "Phase Status Description")]
	public ZString PhaseStatusDescription => Lookups.PhaseStatusList.GetDescriptionFromCode(CH_PhaseStatus);

	public ZPropertyInfo PhaseStatusDescriptionInfo => GetZPropertyInfo(nameof(PhaseStatusDescription));

	CusEntryNumber EntryReleaseCusEntryNumber => entryReleaseCusEntryNumber ??= CusEntryNumber.Load(this, CusEntryNumberTypes.Norway.CustomsEntryReleaseNumber, CountryCode);
	CusEntryNumber entryReleaseCusEntryNumber;

	#region IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider

	public ZString MessageStatus
	{
		get { return CH_Status; }
		set { CH_Status = value; }
	}
	public ZString JobStatus
	{
		get { return CH_EntryStatus; }
		set { CH_EntryStatus = value; }
	}

	public ZString JobIdentification => CH_BGMReference;

	public bool RefreshValidationBeforeSendMessage => true;

	public BusinessObject TopLevelBusinessObject => this;

	public void AddMessage(EDIMessage message)
	{
		Messages.Add(message);
	}

	[ChildEditable(true)]
	public new NOEDIMessageCollection Messages => (NOEDIMessageCollection)base.Messages;

	protected override EDIMessageCollection GetNewMessageCollection() => new NOEDIMessageCollection(this);

	public EDIFACTMessageStatusCalculator GetCalculator(string country)
	{
		return country == Constants.CountryCodes.Norway ? MessageStatusCalculator : null;
	}

	internal EDIFACTMessageStatusCalculator MessageStatusCalculator => messageStatusCalculator ??= new EDIFACTStatusCalculator(EDIMessageConstants.MessageTypeNames.CUSDEC);
	EDIFACTMessageStatusCalculator messageStatusCalculator;

	#endregion

	internal ZDecimal TotalNetWeight => InvoiceLines.Sum(i => i.JI_NetWeight);

	internal ZDecimal TotalGrossWeight => InvoiceLines.Sum(i => i.JI_Weight);

	internal ZDecimal TotalFreightInLocalCurrency => InvoiceLines.Cast<JobComInvoiceLine>().Sum(i => i.JI_Calc_FreightInLocalCurrency);

	public override bool IsFeePaidByBroker(string feeCode, ZString methodOfPaymentCode, ILogger logger)
	{
		return this.ShouldBrokerPayThisFee(feeCode, methodOfPaymentCode, logger);
	}

	IProcessor Integration.Customs.NO.IEmmaMessageGenerationProcessorProvider.GetEmmaMessageGenerationActionProcessor(Integration.Customs.NO.ICusEntryHeader entryHeader)
		=> new EmmaMessageGeneratorProcessor(this);

	IProcessor Integration.Customs.IBaseAutoSendingMessageSupporter.CreateStmProcessQueueProcessor(BusinessObject parent, ZString triggerActionCode)
		=> new CustomsStmProcessQueueCreatorProcessor(parent, CustomsStmProcessQueueLoader.Constants.AutoSendCustomsMessaging, triggerActionCode);

	protected override ProcessHandlingInfo GetProcessHandingInfoCore() => new CusEntryHeaderProcessHandlingInfo(this);
}
