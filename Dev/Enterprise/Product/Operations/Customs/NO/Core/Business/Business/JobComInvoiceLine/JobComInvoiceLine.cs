using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.NO.Business;

public class JobComInvoiceLine : AutoNOJobComInvoiceLine,
	ICusSupportingInfoTypeSupporter,
	ISupplementaryCodeSupporter,
	ICusCodeDataTypeSupporter
{
	public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new class Schema : AutoNOJobComInvoiceLine.Schema
	{
		public const string EntryInstructionDescription = "EntryInstructionDescription";
		public const string JI_Calc_FreightInLocalCurrency = "JI_Calc_FreightInLocalCurrency";
		public const string JI_Calc_InsuranceInLocalCurrency = "JI_Calc_InsuranceInLocalCurrency";
		public const string JI_SupplementaryCode1 = "JI_SupplementaryCode1";
		public const string JI_SupplementaryCode2 = "JI_SupplementaryCode2";

		public const int CustomsRateDecimalPlaces = 2;
	}

	public new JobComInvoiceLine Clone() => (JobComInvoiceLine)base.Clone();

	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

	public new JobComInvoiceLineLookups Lookups => (JobComInvoiceLineLookups)base.Lookups;

	public new JobComInvoiceLineValidation Validation => (JobComInvoiceLineValidation)base.Validation;

	public new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;

	[ChildEditable(true)]
	public new JobComInvChargeCollection<InvoiceLineCharge> Charges => (JobComInvChargeCollection<InvoiceLineCharge>)base.Charges;

	public new CusEntryLine CusEntryLine => (CusEntryLine)base.CusEntryLine;

	[ChildEditable(true)]
	public new JobComInvApportionedChargeCollection<InvoiceLineApportionCharge> ApportionedCharges => (JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>)base.ApportionedCharges;

	public override void OnLoaded()
	{
		base.OnLoaded();
		InitialiseCustomsOverride();
	}

	protected override void ResetValuesAfterCloneCore()
	{
		base.ResetValuesAfterCloneCore();
		InitialiseCustomsOverride();
	}

	protected override Customs.Business.JobComInvoiceLineLookups GetNewLookups() => this switch
	{
		{ IsImport: true } => new ImportJobComInvoiceLineLookups(this),
		{ IsExport: true } => new ExportJobComInvoiceLineLookups(this),
		_ => new JobComInvoiceLineLookups(this),
	};

	protected override Customs.Business.JobComInvoiceLineValidation GetNewValidation() => IsImport ? new ImportJobComInvoiceLineValidation(this) : new ExportJobComInvoiceLineValidation(this);

	protected override IJobComInvApportionedChargeCollection<BaseInvoiceLineApportionedCharge> CreateNewInvoiceLineApportionedChargeCollection() => new JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>(this);

	protected override IJobComInvChargeCollection<BaseInvoiceLineCharge> CreateNewInvoiceLineChargeCollection() => new JobComInvChargeCollection<InvoiceLineCharge>(this);

	public bool HasImporterWithMVARegistration => Factory.GetValue(ref hasImporterWithMVARegistrationCached, () => !Importer.GetMVACodeOrEmpty().IsEmpty);
	CachedProperty<bool> hasImporterWithMVARegistrationCached;

	protected override bool ShouldSetDescriptionWhenTariffChanges => false;

	protected override ZString CustomsCountryCodeCore => Core.Constants.CountryCodes.Norway;

	protected override Type TypeOfPartUsedCore => typeof(OrgSupplierPart);

	protected override ZDecimal ComponentPrice => JI_ZZF_NKTaxType == UniversalReferenceConstants.RefCusTaxOrFee.MVK
		? base.ComponentPrice * DutyCalculatorStrategy.ArtsVATBase
		: base.ComponentPrice;

	public override ZDecimal JI_Weight
	{
		get => base.JI_Weight;
		set
		{
			if (JI_Weight != value)
			{
				base.JI_Weight = value;
				if (JI_WeightUQ == JI_NetWeightUQ && JI_NetWeight.IsEmpty)
				{
					var factor = CalculateBetweenNetWeightAndGrossWeightFactor;
					JI_NetWeight = base.JI_Weight * factor;
				}
			}
		}
	}

	public override ZDecimal JI_NetWeight
	{
		get => base.JI_NetWeight;
		set
		{
			if (JI_NetWeight != value)
			{
				base.JI_NetWeight = value;
				if (JI_NetWeightUQ == JI_WeightUQ && JI_Weight.IsEmpty)
				{
					var factor = 1 / CalculateBetweenNetWeightAndGrossWeightFactor;
					JI_Weight = base.JI_NetWeight * factor;
				}
			}
		}
	}

	public const decimal CalculateBetweenNetWeightAndGrossWeightFactor = 0.9m;

	[ResourceStringData("F4F33FCB-0D43-2894-4B9F-7E944E00750B", Caption = "Procedure Code")]
	public override ZString JI_Procedure
	{
		get => base.JI_Procedure;
		set
		{
			base.JI_Procedure = value;
			Declaration?.MarkAsNeedingValidation();
			Declaration?.Invoices?.MarkAsNeedingValidation();
		}
	}

	public ZDecimal JI_Calc_TotalAmount => CusEntryLine switch
	{
		{ } x => x.DutyAmount + x.ExciseDutyAmount + x.VatAmount,
		_ => 0m,
	};

	public override ZDecimal JI_Calc_GSTVATAmountIncludingWHEstimate
	{
		get
		{
			return Declaration?.JE_MessageType.ToString() switch
			{
				JobMessageTypeList.Codes.Import => JI_Calc_GSTVATAmountIncludingWHEstimate_IMPORT(),
				_ => base.JI_Calc_GSTVATAmountIncludingWHEstimate,
			};
		}
	}

	ZDecimal JI_Calc_GSTVATAmountIncludingWHEstimate_IMPORT()
	{
		if(!CanCalculateVAT())
		{
			return ZDecimal.Zero;
		}
		var basis = JI_Calc_CIF_InLocalCurrency + JI_Calc_DutyAmountIncludingWHEstimate + TotalExciseDuties;
		return FeeRounder.Round(basis * AppliedTaxAndFeeRate);
	}

	public virtual ZDecimal AppliedTaxAndFeeRate => AppliedTaxAndFee?.ZZF_Value ?? 0;

	[ResourceStringData("03FF5DC8-9BDA-4240-8B33-BF0EB5FE904B", Caption = "Entry Instruction Description", MediumCaption = "Entry Inst. Desc.", ShortCaption = "CEI Desc.")]
	public ZString EntryInstructionDescription => EntryInstruction?.CEI_Description ?? ZString.Empty;

	[ChildEditable(true)]
	public SupportingDocumentCollection SupportingDocuments => supportingDocuments ??= GetSupportingDocuments();
	SupportingDocumentCollection supportingDocuments;

	SupportingDocumentCollection GetSupportingDocuments()
	{
		var result = new SupportingDocumentCollection(this);
		result.Load();
		RegisterEditableChildObject(result);

		return result;
	}

	public IEnumerable<IBusinessObjectFetchStrategy> GetFetchStrategies()
	{
		yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
	}

	public IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		return new Dictionary<ZString, Type>()
		{
			{ Common.NO.CusSupportingInfoTypeList.Codes.SupportingDocument, typeof(SupportingDocument) },
		};
	}

	[ResourceStringData("A990E5E7-994C-426B-A195-5DD0DF09412D", Caption = "Goods Marks", MediumCaption = "Goods Item Marks", FullDescription = "Specific goods item marks. This is to help customs identify the goods item in case of a physical control. If left blank “ADRESSE” will be sent to customs/printed on SAD.")]
	[MaxLength(28)]
	public override ZString JI_GoodsMarks
	{
		get => base.JI_GoodsMarks;
		set => base.JI_GoodsMarks = value;
	}

	[ResourceStringData("B7EF1348-FC4A-43BA-8097-09AD02170835", Caption = "Reduced custom", FullDescription = "Special exemption/duty reduction")]
	[MaxLength(1)]
	[List($"{nameof(Lookups)}.{nameof(JobComInvoiceLineLookups.ReducedCustomsFlagList)}")]
	public override ZString JI_ReducedCustomsFlag
	{
		get => base.JI_ReducedCustomsFlag;
		set
		{
			var oldValue = JI_ReducedCustomsFlag;
			base.JI_ReducedCustomsFlag = value;

			if (!IsCopying && oldValue != JI_ReducedCustomsFlag)
			{
				UpdateJI_PrimaryPreferenceIfNeeded();
				CustomsRateIsOverridden = !value.IsEmpty;
			}
		}
	}

	public override ZString JI_CountryOfOrigin
	{
		get => base.JI_CountryOfOrigin;
		set
		{
			var oldValue = JI_CountryOfOrigin;
			base.JI_CountryOfOrigin = value;

			if (!IsCopying && oldValue != value)
			{
				UpdateJI_PrimaryPreferenceIfNeeded();
				UpdateJI_StateOrRegionOfOriginIfNeeded();
			}
		}
	}

	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.NOStateOrRegionOfOrigin))]
	[ResourceStringData("E9EF77A7-187A-467A-B4DA-989AAC6858DD", Caption = "County Origin")]
	public override ZString JI_StateOrRegionOfOrigin
	{
		get => base.JI_StateOrRegionOfOrigin;
		set => base.JI_StateOrRegionOfOrigin = value;
	}

	void UpdateJI_StateOrRegionOfOriginIfNeeded()
	{
		var result = Lookups.NOStateOrRegionOfOrigin.DefaultCode;
		if (JI_CountryOfOrigin == Core.Constants.CountryCodes.Norway)
		{
			result = GetStateOrRegionFromCurrentSupplierIfExists();
		}
		if (JI_StateOrRegionOfOrigin != result)
		{
			JI_StateOrRegionOfOrigin = result;
		}
	}

	ZString GetStateOrRegionFromCurrentSupplierIfExists()
	{
		var result = ZString.Empty;

		var supplier = Declaration?.Supplier;
		if (supplier != null && (supplier.MainAddress?.OA_RN_NKCountryCode ?? ZString.Empty) == Core.Constants.CountryCodes.Norway)
		{
			result = supplier.MainAddress?.OA_State ?? ZString.Empty;
		}
		return result;
	}

	public void SetDefaultValuesForOrigin()
	{
		if (Declaration != null && Declaration.IsExport)
		{
			JI_CountryOfOrigin = Core.Constants.CountryCodes.Norway;
		}
	}

	public override ZString JI_Tariff
	{
		get => base.JI_Tariff;
		set
		{
			var oldValue = JI_Tariff;
			base.JI_Tariff = value;

			if (!IsCopying && oldValue != JI_Tariff)
			{
				UpdateJI_PrimaryPreferenceIfNeeded();
				CustomsRateIsOverridden = false;
			}
		}
	}

	public override ZString JI_PrimaryPreference
	{
		get => base.JI_PrimaryPreference;
		set
		{
			var oldValue = JI_PrimaryPreference;
			base.JI_PrimaryPreference = value;

			if (!IsCopying && oldValue != JI_PrimaryPreference)
			{
				if (IsImport && !PreferenceCodesExcludedFromCreatingDefaultSupportingDocument && !JI_PrimaryPreference.IsEmpty)
				{
					AddSupportingDocumentIfNeeded(SupportingDocumentCodeList.CertificateForOrigin, SupportingDocumentCodeList.CertificateForOrigin_DefaultText);
				}
				else if (PreferenceCodesExcludedFromCreatingDefaultSupportingDocument)
				{
					DeleteSupportingDocumentIfExists(SupportingDocumentCodeList.CertificateForOrigin);
				}
			}
		}
	}

	void AddSupportingDocumentIfNeeded(ZString docType, ZString reference)
	{
		if (!docType.IsEmpty)
		{
			var document = FindSupportingDocumentWithDocType(docType);
			if (document == null)
			{
				document = SupportingDocuments.AddNew();
				document.CSI_Code = docType;
				document.CSI_ReferenceNumber = reference;
			}
		}
	}

	void DeleteSupportingDocumentIfExists(ZString docType)
	{
		if (!docType.IsEmpty)
		{
			var document = FindSupportingDocumentWithDocType(docType);
			if (document != null)
			{
				SupportingDocuments.RemoveAndDelete(document);
			}
		}
	}

	public SupportingDocument FindSupportingDocumentWithDocType(ZString requestedDocType) => SupportingDocuments.Cast<SupportingDocument>().FirstOrDefault(x => x.CSI_Code == requestedDocType);

	void UpdateJI_PrimaryPreferenceIfNeeded()
	{
		if (IsImport && JI_PrimaryPreference.IsEmpty)
		{
			var isAutoSuggestOfPreferenceApplicable = !JI_Tariff.IsEmpty && !JI_CountryOfOrigin.IsEmpty;
			if (!isAutoSuggestOfPreferenceApplicable)
			{
				return;
			}

			var generalRateFormula = GetRateFormulaWithFallbackToNormalTariff(PrimaryPreferenceCodeList.Codes.N);
			if (JI_ReducedCustomsFlag == ReducedCustomsFlagList.Codes.S || generalRateFormula == "0")
			{
				JI_PrimaryPreference = PrimaryPreferenceCodeList.Codes.N;
			}
			else if (JI_ReducedCustomsFlag.IsEmpty)
			{
				var codesToLookFor = new string[] {
					PrimaryPreferenceCodeList.Codes.B, PrimaryPreferenceCodeList.Codes.A,
					PrimaryPreferenceCodeList.Codes.C, PrimaryPreferenceCodeList.Codes.G,
					PrimaryPreferenceCodeList.Codes.P, PrimaryPreferenceCodeList.Codes.N };
				JI_PrimaryPreference = codesToLookFor
									.Where(Lookups.PrimaryPreferenceList.ContainsCode)
									.MinBySafe(code => GetRateFormulaWithFallbackToNormalTariff(code), new RateFormulaComparer(CalcDataForConditionFormula))
									?? PrimaryPreferenceCodeList.Codes.N;
			}
		}
	}

	public ZString NormalTariffDutyRateFormula => UniversalTariff?.GetApplicableRate(NormalTariffDutyRateSelectionCriteria)?.ZZ2_RateFormula ?? ZString.Empty;

	public IZZRateSelectionCriteria NormalTariffDutyRateSelectionCriteria => Factory.GetValue(ref normalTariffDutyRateSelectionCriteria, GetNormalTariffDutyRateSelectionCriteria);
	CachedProperty<IZZRateSelectionCriteria> normalTariffDutyRateSelectionCriteria;

	IZZRateSelectionCriteria GetNormalTariffDutyRateSelectionCriteria()
		=> new SpecificRateSelectionCriteria(JI_CountryOfOrigin, CustomsCountryCode, PrimaryPreferenceCodeList.Codes.N, ZString.Empty, new HashSet<ZString>(), EffectiveAssessmentDate, Constants.RateTypes.Duty, ZString.Empty);

	ZString GetRateFormulaWithFallbackToNormalTariff(ZString prefCode)
	{
		if (JI_CountryOfOrigin.IsEmpty || UniversalTariff == null)
		{
			return ZString.Empty;
		}

		var rateFormula = ZString.Empty;
		var criteria = new SpecificRateSelectionCriteria(JI_CountryOfOrigin, CustomsCountryCode, prefCode, ZString.Empty, new HashSet<ZString>(), EffectiveAssessmentDate, ZString.Empty, ZString.Empty);
		if (!JI_PrimaryPreference.IsEmpty)
		{
			rateFormula = UniversalDutyRate?.ZZ2_RateFormula ?? ZString.Empty;
		}
		if ((rateFormula == ZString.Empty || rateFormula == "0") && JI_PrimaryPreference != PrimaryPreferenceCodeList.Codes.N)
		{
			rateFormula = UniversalTariff?.GetApplicableRate(criteria)?.ZZ2_RateFormula ?? rateFormula;
		}
		return rateFormula;
	}

	protected override ICustomsUnitDefaultingStrategy GetCustomsUnitDefaultingStrategy()
		=> new UniversalRateCustomsUnitDefaultingStrategy<JobComInvoiceLine>((invoiceLine) => new[] { invoiceLine.UniversalDutyRate }, Enumerable.Empty<ZPropertyInfo>());

	public ZBool IsRateGivenInFractionsOfKroner(ZString rateCode)
	{
		var criteria = GetExciseRateSelectionCriteria(rateCode);
		return (UniversalTariff?.GetApplicableRate(criteria)?.ZZ2_RateFormulaDerivedFrom ?? ZString.Empty) == UniversalReferenceConstants.RateGivenInFractionsOfKroner;
	}

	public bool PreferenceCodesExcludedFromCreatingDefaultSupportingDocument => new ZString[] { PrimaryPreferenceCodeList.Codes.N, PrimaryPreferenceCodeList.Codes.J }.Contains(JI_PrimaryPreference);

	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.TaxOrFeeCodeList))]
	[ResourceStringData("e3e131a3-0616-4cd1-9b4f-f790086b1983", Caption = "VAT Code")]
	public override ZString JI_ZZF_NKTaxType
	{
		get => base.JI_ZZF_NKTaxType;
		set => base.JI_ZZF_NKTaxType = value;
	}

	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsSecondUnitQtyList))]
	public override ZString JI_CustomsSecondUnitQty
	{
		get
		{
			var result = base.JI_CustomsSecondUnitQty;
			if (result.IsEmpty)
			{
				result = Lookups.CustomsSecondUnitQtyList.DefaultCode;
			}
			return result;
		}
		set => base.JI_CustomsSecondUnitQty = value;
	}

	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsUnitQtyList))]
	public override ZString JI_CustomsUnitQty
	{
		get
		{
			var result = base.JI_CustomsUnitQty;
			if (result.IsEmpty)
			{
				result = Lookups.CustomsUnitQtyList.DefaultCode;
			}
			return result;
		}
		set => base.JI_CustomsUnitQty = value;
	}

	public override ZDecimal JI_CustomsQuantity
	{
		get
		{
			var result = base.JI_CustomsQuantity;
			if (result.IsEmpty && CanConvertFromNetWeightToCustomsUnit(JI_CustomsUnitQty))
			{
				result = JI_NetWeight;
			}

			return result;
		}
		set => base.JI_CustomsQuantity = value;
	}

	public override ZDecimal JI_Volume
	{
		get => base.JI_Volume;
		set
		{
			base.JI_Volume = value;
			JI_CustomsSecondQuantityAutoPopulatedFromVolumeIfNeeded();
		}
	}

	public override ZString JI_VolumeUQ
	{
		get => base.JI_VolumeUQ;
		set
		{
			base.JI_VolumeUQ = value;
			JI_CustomsSecondQuantityAutoPopulatedFromVolumeIfNeeded();
		}
	}

	public override ZDecimal JI_InvoiceQuantity
	{
		get => base.JI_InvoiceQuantity;
		set
		{
			base.JI_InvoiceQuantity = value;
			JI_CustomsSecondQuantityAutoPopulatedFromInvoiceQuantityIfNeeded();
		}
	}

	public override ZString JI_InvoiceUQ
	{
		get => base.JI_InvoiceUQ;
		set
		{
			base.JI_InvoiceUQ = value;
			JI_CustomsSecondQuantityAutoPopulatedFromInvoiceQuantityIfNeeded();
		}
	}
	public bool IfTariffRequiresCustomsSecondUnitQuantityofUnit(string unit)
	{
		return UniversalTariff != null && UniversalTariff.UnitsOfMeasure
			.Any(u => u.ZZ8_UOM == unit && u.ZZ8_Type == UOMTypeList.Codes.CU2);
	}

	public void JI_CustomsSecondQuantityAutoPopulatedFromVolumeIfNeeded()
	{
		if (JI_CustomsSecondQuantity.IsEmpty && JI_VolumeUQ == Core.Constants.Volume.Litre && !JI_Volume.IsEmpty && IfTariffRequiresCustomsSecondUnitQuantityofUnit(NOCustomsFormulaUnitCodeList.Codes.LTR))
		{
			JI_CustomsSecondQuantity = JI_Volume;
		}
	}

	public void JI_CustomsSecondQuantityAutoPopulatedFromInvoiceQuantityIfNeeded()
	{
		if (JI_CustomsSecondQuantity.IsEmpty && JI_InvoiceUQ == "UNT" && !JI_InvoiceQuantity.IsEmpty && IfTariffRequiresCustomsSecondUnitQuantityofUnit(NOCustomsFormulaUnitCodeList.Codes.NMB))
		{
			JI_CustomsSecondQuantity = JI_InvoiceQuantity;
		}
	}

	public override bool CanConvertFromNetWeightToCustomsUnit(ZString customsUnit)
	{
		return JI_NetWeight > 0m &&
				CustomsQuantityConverter.CanConvertUnitOfQuantity(JI_NetWeightUQ) &&
				CustomsQuantityConverter.CanConvertUnitOfQuantity(customsUnit);
	}

	public new CustomsQuantityConverter CustomsQuantityConverter => (CustomsQuantityConverter)base.CustomsQuantityConverter;

	protected override BaseCustomsQuantityConverter GetCustomsQuantityConverter()
		=> new CustomsQuantityConverter(this, (ZPropertyInfoDecimal)JI_CustomsQuantityInfo, (ZPropertyInfoString)JI_CustomsUnitQtyInfo);

	Money TotalOtherChargesMoney
	{
		get
		{
			var result = Money.Empty;
			if (InvoiceHeader != null)
			{
				result = GetCharge(IncoTermAndChargeFactory.GetCharge(Common.CustomsChargeTypeList.Codes.OtherCharges));
			}
			return result;
		}
	}

	[ResourceStringData("8CDD5BA5-B5AA-2097-4923-9D8F9C670366", Caption = "Other Charges", FullDescription = "Other charges for current line item.")]
	public ZDecimal TotalOtherChargesInNOK
	{
		get
		{
			var result = ZDecimal.Zero;

			var localCurrency = LocalCurrency;
			if (localCurrency != null)
			{
				result = CurrencyConverter.ConvertExact(TotalOtherChargesMoney, localCurrency).Amount;
			}

			return result;
		}
	}

	public ZDecimal JI_Calc_FreightInLocalCurrency => ConvertAmountToLocalAmountExact(JI_Calc_FreightInInvoiceCurr);

	public ZPropertyInfo JI_Calc_FreightInLocalCurrencyInfo => GetZPropertyInfo(Schema.JI_Calc_InsuranceInLocalCurrency);

	public ZDecimal JI_Calc_InsuranceInLocalCurrency => ConvertAmountToLocalAmountExact(JI_Calc_InsuranceInInvoiceCurr);

	public ZPropertyInfo JI_Calc_InsuranceInLocalCurrencyInfo => GetZPropertyInfo(Schema.JI_Calc_InsuranceInLocalCurrency);

	ZDecimal ConvertAmountToLocalAmountExact(ZDecimal amount)
	{
		return ConvertToLocalAmountExact(GetEffectiveMoney(new Money(amount, InvoiceHeader.Invoice_Currency))).Amount;
	}

	[ResourceStringData("4C379F9B-8F47-998F-4A17-B430D9818ECF", Caption = "Deductions", FullDescription = "Total deductions for current line item.")]
	public ZDecimal TotalDeductionsInNOK
	{
		get
		{
			var result = ZDecimal.Zero;

			var localCurrency = LocalCurrency;
			if (localCurrency != null)
			{
				result = CurrencyConverter.ConvertExact(JI_NonDutiableDeductions, localCurrency).Amount;
			}

			return result;
		}
	}

	[ResourceStringData("E395DB97-51D8-F694-48C6-079030DC4DA6", ShortCaption = "Packing type", Caption = "Beverage packing type", FullDescription = "Filter the beverage packing excise duty codes by selecting pack type.")]
	[MaxLength(1)]
	[List($"{nameof(Lookups)}.{nameof(JobComInvoiceLineLookups.PackageTypeList)}")]
	public override ZString JI_PackageType
	{
		get => base.JI_PackageType;
		set => base.JI_PackageType = value;
	}

	[ResourceStringData("92023945-2C19-AE81-4517-29C7FAEE9DBB", Caption = "Inv. Line Value", FullDescription = "Invoice line value for current line item.")]
	public new ZDecimal JI_LinePriceInLocalCurrency
	{
		get { return JI_LinePriceInLocalCurrencyMoney.Amount; }
	}

	[ResourceStringData("650EB841-889B-60A0-4DAD-FD2B95C2DE04", Caption = "Customs Duty", FullDescription = "Customs duties for current line item.")]
	public override ZDecimal JI_Calc_DutyAmountIncludingWHEstimate
	{
		get
		{
			return GetOverridenCustomsAmount();
		}
	}

	[ResourceStringData("DFBC9620-0DC5-9E8A-480E-D605C4304EB5", Caption = "Merge override", FullDescription = "Add value (of own choice) here to avoid merging (when all other merge-fields are equal). If the same value is added to multiple lines, they will be merged (when all other merge-fields are equal).")]
	[MaxLength(10)]
	public override ZString JI_MergeOverride
	{
		get => base.JI_MergeOverride;
		set => base.JI_MergeOverride = value;
	}

	public override void OnSaving()
	{
		if (JI_PrimaryPreference == ZString.Empty && IsExport)
		{
			JI_PrimaryPreference = PrimaryPreferenceCodeList.Codes.N;
		}
		base.OnSaving();
	}

	public ZDecimal GetOverridenCustomsAmount()
	{
		var percentFactor = CustomsRateType == RateTypeCodeList.Codes.Percent || CustomsRateType == RateTypeCodeList.Codes.PercentSign ? 100 : 1;
		ZDecimal amount = CustomsRate / percentFactor * GetBaseValue();
		return amount.Round(0).Normalize();
	}

	public ZDecimal GetBaseValue() => GetBaseValue(CustomsRateType);

	public ZDecimal GetBaseValue(string rateType)
	{
		var result = ZDecimal.Zero;

		switch (rateType)
		{
			case RateTypeCodeList.Codes.Percent:
			case RateTypeCodeList.Codes.PercentSign:
				result = JI_Calc_CIF_InLocalCurrency;
				break;

			case RateTypeCodeList.Codes.Kilogram:
				if (CanConvertUnitOfQuantity(JI_CustomsUnitQty) && JI_CustomsQuantity > 0)
				{
					result = ConvertUnit(JI_CustomsQuantity, JI_CustomsUnitQty, Core.Constants.Weight.Kilograms);
				}
				else
				{
					result = ConvertUnit(JI_NetWeight, JI_NetWeightUQ, Core.Constants.Weight.Kilograms);
				}
				break;

			case RateTypeCodeList.Codes.Gram:
				if (CanConvertUnitOfQuantity(JI_CustomsUnitQty) && JI_CustomsQuantity > 0)
				{
					result = ConvertUnit(JI_CustomsQuantity, JI_CustomsUnitQty, Core.Constants.Weight.Grams);
				}
				else
				{
					result = ConvertUnit(JI_NetWeight, JI_NetWeightUQ, Core.Constants.Weight.Grams);
				}
				break;

			case RateTypeCodeList.Codes.Piece:
				if (JI_CustomsUnitQty == UnitQuantityCodeList.Codes.Piece && JI_CustomsQuantity > 0)
				{
					result = JI_CustomsQuantity;
				}
				else if (JI_InvoiceUQ == UnitQuantityCodeList.Codes.Piece)
				{
					result = JI_InvoiceQuantity;
				}
				break;

			case RateTypeCodeList.Codes.Liter:
				if (Core.Constants.Volume.ContainsCode(JI_CustomsUnitQty) && JI_CustomsQuantity > 0)
				{
					result = ConvertUnit(JI_CustomsQuantity, JI_CustomsUnitQty, Core.Constants.Volume.Litre);
				}
				else
				{
					result = ConvertUnit(JI_Volume, JI_VolumeUQ, Core.Constants.Volume.Litre);
				}
				break;

			case RateTypeCodeList.Codes.CubicMeter:
				if (Core.Constants.Volume.ContainsCode(JI_CustomsUnitQty) && JI_CustomsQuantity > 0)
				{
					result = ConvertUnit(JI_CustomsQuantity, JI_CustomsUnitQty, Core.Constants.Volume.CubicMetres);
				}
				else
				{
					result = ConvertUnit(JI_Volume, JI_VolumeUQ, Core.Constants.Volume.CubicMetres);
				}
				break;

			case RateTypeCodeList.Codes.Milliliter:
				if (Core.Constants.Volume.ContainsCode(JI_CustomsUnitQty) && JI_CustomsQuantity > 0)
				{
					result = ConvertUnit(JI_CustomsQuantity, JI_CustomsUnitQty, Core.Constants.Volume.CubicCentimeters);
				}
				else
				{
					result = ConvertUnit(JI_Volume, JI_VolumeUQ, Core.Constants.Volume.CubicCentimeters);
				}
				break;
		}
		return result;
	}

	static bool CanConvertUnitOfQuantity(ZString unitOfQuantity)
	{
		return unitOfQuantity == "KGM" || Core.Constants.Weight.ContainsCode(unitOfQuantity);
	}

	static ZDecimal ConvertUnit(ZDecimal value, ZString fromUnit, ZString toUnit)
	{
		fromUnit = fromUnit == "KGM" ? Core.Constants.Weight.Kilograms : fromUnit;
		toUnit = toUnit == "KGM" ? Core.Constants.Weight.Kilograms : toUnit;

		if (Core.Constants.Weight.ContainsCode(fromUnit) && Core.Constants.Weight.ContainsCode(toUnit))
		{
			return Core.Constants.Weight.Convert(value, fromUnit, toUnit);
		}
		if (Core.Constants.Volume.ContainsCode(fromUnit) && Core.Constants.Volume.ContainsCode(toUnit))
		{
			return Core.Constants.Volume.Convert(value, fromUnit, toUnit);
		}

		return 0m;
	}

	public bool NO_PackageTypeVisible => Factory.GetValue(ref no_PackageTypeVisible, GetNO_PackageTypeVisibility);
	CachedProperty<bool> no_PackageTypeVisible;

	protected virtual bool GetNO_PackageTypeVisibility()
	{
		bool result = false;
		var tariff = UniversalTariff;
		if (tariff != null)
		{
			result = tariff.FilteredRates.Any(x => tariffCodesNeedingPackageTypeInformation.Contains(x.RateCode.Left(2)));
		}

		return result;
	}

	readonly ZString[] tariffCodesNeedingPackageTypeInformation = { "MA", "MB", "MG", "MP", "GP", "GG", "GA", "GB" };

	[ReadOnlyMember(nameof(CustomsRateReadOnly))]
	[ResourceStringData("Enterprise.Customs.NO.Business.JobComInvoiceLine|CustomsRateWrapper", Caption = "Customs rate", ShortCaption = "Rate")]
	public ZDecimal CustomsRate
	{
		get => CustomsRateIsOverridden ? JI_CustomsRateOverrideValue : DefaultCustomsRate;
		set
		{
			var oldValue = CustomsRate;
			if (oldValue != value && !IsCopying)
			{
				CustomsRateIsOverridden = true;
				JI_CustomsRateOverrideValue = value;
				CustomsRateInfo.RefreshBinding();
			}
		}
	}
	public ZPropertyInfo CustomsRateInfo => GetZPropertyInfo(nameof(CustomsRate));

	[ReadOnlyMember(nameof(CustomsRateReadOnly))]
	[ResourceStringData("Enterprise.Customs.NO.Business.JobComInvoiceLine|CustomsRateTypeWrapper", Caption = "Type")]
	[MaxLength(JobComInvoiceLineTax.Schema.JLT_MethodOfPaymentMaxLength)]
	[List($"{nameof(Lookups)}.{nameof(JobComInvoiceLineLookups.CustomsOverrideTypeList)}")]
	public ZString CustomsRateType
	{
		get => CustomsRateIsOverridden ? JI_CustomsRateOverrideType : DefaultCustomsType;
		set
		{
			var oldValue = CustomsRateType;
			if (oldValue != value && !IsCopying)
			{
				CheckMaximumLength(CustomsRateTypeInfo, value);
				CustomsRateIsOverridden = true;
				JI_CustomsRateOverrideType = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCustomsRateType();
				}
				CustomsRateTypeInfo.RefreshBinding();
				SetDefaultCustomsRate(value);
			}
		}
	}

	public ZPropertyInfo CustomsRateTypeInfo => GetZPropertyInfo(nameof(CustomsRateType));

	void SetDefaultCustomsRate(ZString rateType)
	{
		var customsRate = AllApplicableRates
			?.FirstOrDefault(x => x.GetFirstUoMCodePair()?.Code == rateType)
			?.RateFormulaNumber ?? default;

		CustomsRate = rateType == RateTypeCodeList.Codes.PercentSign
			? customsRate * 100
			: customsRate;
	}

	[ReadOnly(true)]
	public ZDecimal DefaultCustomsRate
	{
		get
		{
			ZDecimal rateNumber = 0;
			if (UniversalDutyRate != null)
			{
				rateNumber = UniversalDutyRate.RateFormulaNumber ?? default;
				// When the formula is for VFD, we prefer to show the rate as percentage, to be clearer on what the unit is.
				if (UniversalDutyRate.GetUnitsOfMeasure().Contains(NOCustomsFormulaUnitCodeList.Codes.VFD))
				{
					rateNumber *= 100;
				}
			}
			return rateNumber;
		}
	}

	public ZString DefaultCustomsType =>
		UniversalDutyRate != null
			? UniversalDutyRate.GetFirstUoMCodePair()?.Code
			: ZString.Empty;

	public ZBool CustomsRateReadOnly => !CustomsRateIsOverridden;

	void InitialiseCustomsOverride()
	{
		customsRateIsOverridden = !JI_CustomsRateOverrideType.IsEmpty || !JI_CustomsRateOverrideValue.IsEmpty;
	}

	bool customsRateIsOverridden;

	[ResourceStringData("Enterprise.Customs.NO.Business.JobComInvoiceLine|CustomsRateIsOverridden", Caption = "Override customs rate", ShortCaption = "Override")]
	public ZBool CustomsRateIsOverridden
	{
		get => customsRateIsOverridden;
		set
		{
			var oldValue = customsRateIsOverridden;
			if (!IsCopying && value != oldValue)
			{
				if (value)
				{
					JI_CustomsRateOverrideType = DefaultCustomsType;
					JI_CustomsRateOverrideValue = DefaultCustomsRate;
				}
				else
				{
					JI_CustomsRateOverrideType = ZString.Empty;
					JI_CustomsRateOverrideValue = ZDecimal.Zero;
				}
				customsRateIsOverridden = value;
				CustomsRateIsOverriddenInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateJI_Tariff();
				}
			}
		}
	}

	public ZPropertyInfo CustomsRateIsOverriddenInfo => GetZPropertyInfo(nameof(CustomsRateIsOverridden));

	[ReadOnlyMember(nameof(CustomsRateReadOnly))]
	[ResourceStringData("Enterprise.Customs.NO.Business.JobComInvoiceLine|CustomsType", Caption = "Type")]
	[List($"{nameof(Lookups)}.{nameof(JobComInvoiceLineLookups.CustomsOverrideTypeList)}")]
	public override ZString JI_CustomsRateOverrideType
	{
		get => base.JI_CustomsRateOverrideType;
		set => base.JI_CustomsRateOverrideType = value;
	}

	[DecimalPlaces(Schema.CustomsRateDecimalPlaces)]
	[ReadOnlyMember(nameof(CustomsRateReadOnly))]
	[ResourceStringData("Enterprise.Customs.NO.Business.JobComInvoiceLine|CustomsRate", Caption = "Customs rate", ShortCaption = "Rate")]
	public override ZDecimal JI_CustomsRateOverrideValue
	{
		get => base.JI_CustomsRateOverrideValue;
		set => base.JI_CustomsRateOverrideValue = value;
	}

	[ResourceStringData("98BBB2F7-F025-4B21-AC5D-56BEAF9511BB", Caption = "Excise Duties", FullDescription = "Excise duties for current line item.")]
	public ZDecimal TotalExciseDuties => Factory.GetValue(ref totalExciseDutiesCached, CalculateTotalExciseDuties);
	CachedProperty<ZDecimal> totalExciseDutiesCached;

	ZDecimal CalculateTotalExciseDuties() => new InvoiceLineDutyCalculationStrategy(this).Calculate();

	[ReadOnlyMember(nameof(JI_RTOValueReadOnly))]
	[ResourceStringData("9E9AE7EB-4D2B-2B9A-42C0-86F44D823C39", Caption = "RT rate override")]
	public override ZDecimal JI_RTOValue
	{
		get => base.JI_RTOValue;
		set => base.JI_RTOValue = value;
	}

	public ZBool JI_RTOValueReadOnly => !HasRT100SupplementaryCode;

	ZBool HasRT100SupplementaryCode => Factory.GetValue(ref hasRT100SupplementaryCodeCached, () =>
	{
		return this is ISupplementaryCodeSupporter { SupplementaryCodes: { } codes }
		&& codes.Any(p => p.CY_Code == NOCustomDutyCodeList.Codes.RT100);
	});
	CachedProperty<bool> hasRT100SupplementaryCodeCached;

	public ZDecimal JI_Calc_StatisticalValue => Factory.GetValue(ref ji_Calc_StatisticalValue, GetJI_Calc_StatisticalValue);
	CachedProperty<ZDecimal> ji_Calc_StatisticalValue;

	ZDecimal GetJI_Calc_StatisticalValue() => JI_Calc_CIF_InLocalCurrency + ValuationCalculator.GetAmountToAddToITOTForStatistical(LocalCurrency);

	protected override ICustomsValuationCalculator GetValuationCalculatorCore() => new CustomsValuationCalculator(this);

	#region JI_SupplementaryCode1

	[List($"{nameof(Lookups)}.{nameof(JobComInvoiceLineLookups.AdditionalCodesList)}")]
	[ResourceStringData("A5E6830F-1E1A-41B1-9D6D-46D6940D7E40", Caption = "Excise Code 1", FullDescription = "Excise Code")]
	[MaxLength(SupplementaryCode.Schema.CY_CodeMaxLength)]
	public ZString JI_SupplementaryCode1
	{
		get => SupplementaryCode1?.CY_Code ?? ZString.Empty;
		set => SupplementaryCodeHandler.LoadOrCreate(value, this, 1, JI_SupplementaryCode1Info);
	}

	public ZPropertyInfo JI_SupplementaryCode1Info
		=> SupplementaryCode1 != null
			? GetWrappedZPropertyInfo(nameof(JI_SupplementaryCode1), x => SupplementaryCode1.CY_CodeInfo)
			: GetZPropertyInfo(nameof(JI_SupplementaryCode1));

	SupplementaryCode SupplementaryCode1
	{
		get
		{
			if (supplementaryCode1 is null || supplementaryCode1.IsDeleted)
			{
				supplementaryCode1 = new BaseSupplementaryCode
						.Loader(Factory)
						.Load<SupplementaryCode, JobComInvoiceLine>(this, 1);
				if (supplementaryCode1 is not null)
				{
					RegisterEditableChildObject(supplementaryCode1);
				}
			}

			return supplementaryCode1;
		}
	}
	SupplementaryCode supplementaryCode1;

	#endregion

	#region JI_SupplementaryCode2

	[List($"{nameof(Lookups)}.{nameof(JobComInvoiceLineLookups.AdditionalCodesList)}")]
	[ResourceStringData("1DC5DD61-DA4C-4806-9798-6881183D164E", Caption = "Excise Code 2", FullDescription = "Excise Code")]
	[MaxLength(SupplementaryCode.Schema.CY_CodeMaxLength)]
	public ZString JI_SupplementaryCode2
	{
		get => SupplementaryCode2?.CY_Code ?? ZString.Empty;
		set => SupplementaryCodeHandler.LoadOrCreate(value, this, 2, JI_SupplementaryCode2Info);
	}

	public ZPropertyInfo JI_SupplementaryCode2Info
		=> SupplementaryCode2 != null
			? GetWrappedZPropertyInfo(nameof(JI_SupplementaryCode2), x => SupplementaryCode2.CY_CodeInfo)
			: GetZPropertyInfo(nameof(JI_SupplementaryCode2));

	SupplementaryCode SupplementaryCode2
	{
		get
		{
			if (supplementaryCode2 is null || supplementaryCode2.IsDeleted)
			{
				supplementaryCode2 = new BaseSupplementaryCode
						.Loader(Factory)
						.Load<SupplementaryCode, JobComInvoiceLine>(this, 2);
				if (supplementaryCode2 is not null)
				{
					RegisterEditableChildObject(supplementaryCode2);
				}
			}

			return supplementaryCode2;
		}
	}
	SupplementaryCode supplementaryCode2;

	#endregion

	ICusCodeDataCollection<BaseSupplementaryCode> ISupplementaryCodeSupporter.AdditionalSupplementaryCodes => AdditionalSupplementaryCodes;

	[ChildEditable(true)]
	[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentID, CusCodeDataSchema.Constants.CY_ParentTableCode)]
	public SupplementaryCodeCollection AdditionalSupplementaryCodes => additionalSupplementaryCodes ??= CreateAdditionalSupplementaryCodesCollection();
	SupplementaryCodeCollection additionalSupplementaryCodes;

	SupplementaryCodeCollection CreateAdditionalSupplementaryCodesCollection()
	{
		var provider = BaseSupplementaryCodeProvider.GetBySupplementaryCodeSupporter(this);
		var codesCollection = new SupplementaryCodeCollection(JI_AdditionalSupplementsInfo, provider);
		RegisterEditableChildObject(codesCollection);
		codesCollection.Load();
		return codesCollection;
	}

	[ReadOnly(true)]
	[ResourceStringData("176F63D5-D5B6-439E-A394-FB5A0DD88F43", Caption = "Add. Exc. Codes", FullDescription = "Additional excise codes")]
	public ZString JI_AdditionalSupplements => AdditionalSupplementaryCodes.AsString;

	public ZPropertyInfo JI_AdditionalSupplementsInfo => GetZPropertyInfo(nameof(JI_AdditionalSupplements));

	ISupplementaryCodeHandler<SupplementaryCode> SupplementaryCodeHandler => supplementaryCodeHandler ??= new BaseSupplementaryCodeHandler<SupplementaryCode>();
	ISupplementaryCodeHandler<SupplementaryCode> supplementaryCodeHandler;

	#region CurrencyConverter

	public new RefCurrencyCurrencyConverter CurrencyConverter => (RefCurrencyCurrencyConverter)base.CurrencyConverter;

	protected override CurrencyConverter GetCurrencyConverter()
	{
		if (InvoiceHeader is { } header && header.IsJZ_InvoiceCurrExRateUserEnterable)
		{
			return header.CurrencyConverter;
		}

		return new RefCurrencyCurrencyConverter(Factory)
		{
			DateForRate = EffectiveDateForDutyAndRate,
			RateType = ExchangeRateType.Customs
		};
	}
	public ZDateTime EffectiveDateForDutyAndRate => NullIfNotValid(EntryInstruction?.CEI_DateForDuty ?? ZDateTime.Empty) ?? ZDateTime.Today;

	static ZDateTime? NullIfNotValid(ZDateTime dateTime) => dateTime.IsValid ? dateTime : null;

	public ZDecimal InvoiceLineCurrencyExchangeRateForCustoms => CurrencyConverter.GetExchangeRate(LinePriceRefCurrency) * CustomsExchangeRateMultiplier;

	ZInt CustomsExchangeRateMultiplier => CurrenciesWhereRateIsMultipliedBy100.Contains(JI_RX_NKLinePriceCurr) ? 100 : 1;

	protected ImmutableHashSet<string> CurrenciesWhereRateIsMultipliedBy100 => currenciesWhereRateIsMultipliedBy100 ??= CreateCurrenciesWhereRateIsMultipliedBy100();
	ImmutableHashSet<string> currenciesWhereRateIsMultipliedBy100;

	ImmutableHashSet<string> CreateCurrenciesWhereRateIsMultipliedBy100() => ImmutableHashSet.Create(
		Core.Constants.CurrencyCodes.Switzerland,
		Core.Constants.CurrencyCodes.CzechRepublic,
		Core.Constants.CurrencyCodes.Denmark,
		Core.Constants.CurrencyCodes.Hungary,
		Core.Constants.CurrencyCodes.India,
		Core.Constants.CurrencyCodes.Japan,
		Core.Constants.CurrencyCodes.RomaniaNew,
		Core.Constants.CurrencyCodes.Sweden,
		Core.Constants.CurrencyCodes.Thailand,
		Core.Constants.CurrencyCodes.SouthAfrica);

	#endregion

	#region ISupplementaryCodeSupporter

	ZString ISupplementaryCodeSupporter.GetCountryCodeFromAdditionalCode(ZString additionalCode) => CountryCode;

	IEnumerable<BaseSupplementaryCode> ISupplementaryCodeSupporter.SupplementaryCodes
		=> AdditionalSupplementaryCodes
			.Union(new[] { SupplementaryCode1, SupplementaryCode2 })
			.WhereNotNull();

	ZString ISupplementaryCodeSupporter.SupplementaryCodesFieldType => nameof(FieldType.TextDropEdit);

	ResourceStringData ISupplementaryCodeSupporter.SupplementaryCodeCaption => Res.GetData("8A367E3C-8815-40EE-8A14-B7814865ECC2", "Excise Code");

	ZString ICusCodeDataWithOrderSupporter.GetCountryCodeForCodeProvider()
		=> Declaration?.Country?.RN_Code
			?? InvoiceHeader?.InvoiceCountry?.RN_Code
			?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

	void ICusCodeDataWithOrderSupporter.OnCodesChanged()
	{
		if (!HasRT100SupplementaryCode)
		{
			JI_RTOValue = ZDecimal.Zero;
		}
	}

	TariffView ICusCodeDataWithOrderSupporter.Tariff => UniversalTariff;

	IZZRateSelectionCriteria ICusCodeDataWithOrderSupporter.RateSelectionCriteria => AllApplicableRatesSelectionCriteria;

	CodeDescriptionPairList ICusCodeDataWithOrderSupporter.CachedListOfAdditionalCodeDescriptions => Lookups.CachedListOfAdditionalCodeDescriptions;

	#endregion

	#region ICusCodeDataTypeSupporter

	IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes() => GetCusCodeDataTypes();

	static Dictionary<ZString, Type> GetCusCodeDataTypes() => new() { { BaseCusCodeDataTypeList.Codes.SupplementaryCode, typeof(SupplementaryCode) }, };

	IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
	{
		yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
	}

	#endregion

	internal ZBool CanCalculateVAT() => EntryInstruction?.CusProcedure?.ZZ6_CalculateVAT ?? true;

	internal ZBool CanCalculateDuty() => EntryInstruction?.CusProcedure?.ZZ6_CalculateDuty ?? true;

	protected override IZZRateSelectionCriteria GetAllApplicableRatesSelectionCriteriaCore() => new RateSelectionCriteria(this, ZString.Empty, ZString.Empty);

	protected override IZZRateSelectionCriteria GetDutyRateSelectionCriteriaCore() => new RateSelectionCriteria(this, Constants.RateTypes.Duty, ZString.Empty);

	internal IZZRateSelectionCriteria ExciseRateSelectionCriteria => Factory.GetValue(ref exciseRateSelectionCriteria, () => GetExciseRateSelectionCriteria(ZString.Empty));
	CachedProperty<IZZRateSelectionCriteria> exciseRateSelectionCriteria;

	IZZRateSelectionCriteria GetExciseRateSelectionCriteria(ZString rateCode) => new RateSelectionCriteria(this, ExciseRateType, rateCode);

	internal string ExciseRateType => IsExport ? Constants.RateTypes.ExportDuty : Constants.RateTypes.Excise;

	internal IFeeRounder FeeRounder => feeRounder ??= new IntegerFeeRounder();
	IFeeRounder feeRounder;

	protected override ZString EffectivePrimaryPreferenceCore => JI_PrimaryPreference == PrimaryPreferenceCodeList.Codes.J ? PrimaryPreferenceCodeList.Codes.N : JI_PrimaryPreference;

	public class RateSelectionCriteria : RateSelectionCriteria<JobComInvoiceLine>
	{
		public RateSelectionCriteria(JobComInvoiceLine invoiceLine, ZString rateType, ZString rateCode) : base(invoiceLine, rateType, rateCode)
		{
		}

		protected override ZString GetTradeGroupCountry(JobComInvoiceLine invoiceLine)
			=> invoiceLine.IsExport
				? (invoiceLine.Declaration?.JE_GoodsDestination ?? ZString.Empty)
				: base.GetTradeGroupCountry(invoiceLine);

		protected override ISet<ZString> GetAdditionalCodes(JobComInvoiceLine invoiceLine)
			=> invoiceLine is ISupplementaryCodeSupporter supplementaryCodeSupporter
				? supplementaryCodeSupporter.SupplementaryCodes.WhereNotNull().Select(c => c.CY_Code).ToHashSet()
				: base.GetAdditionalCodes(invoiceLine);
	}

	public override ITariffViewFilterData TariffViewFilterData => new TariffViewFilterData(GetRatesApplyToCountry(), EffectiveDateForDutyAndRate.Date);
}
