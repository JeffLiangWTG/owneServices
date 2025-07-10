using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.PL.Business.Declaration;

public class JobComInvoiceLine(BusinessObjectFactory factory, DataRow row) : AutoPLJobComInvoiceLine(factory, row), Integration.Customs.PL.IJobComInvoiceLine
{
	protected override List<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo> EffectiveAdditionalInfosCore()
	{
		var list = base.EffectiveAdditionalInfosCore();

		if (EntryInstruction is not null)
		{
			list.AddRange(EntryInstruction.AdditionalInfos.Where(ai => ai.IsLine));
		}

		return list;
	}

	public new class Schema : EU.Business.Declaration.JobComInvoiceLine.Schema
	{
		public const string JI_MarkModel = "JI_MarkModel";
		public new const int JI_BrandNameMaxLength = 50;
	}

	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	public new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;

	public new JobComInvoiceLineLookups Lookups => (JobComInvoiceLineLookups)base.Lookups;

	protected override Customs.Business.JobComInvoiceLineLookups GetNewLookups() => new JobComInvoiceLineLookups(this);

	public JobComInvoiceLineValidation PLValidationOrNull => Validation as JobComInvoiceLineValidation;

	protected override Customs.Business.JobComInvoiceLineValidation GetNewValidation() => this switch
	{
		{ IsCommercialInvoiceLine: true } => new CommercialInvoiceLineValidation(this),
		{ IsExport: true } => new ExportJobComInvoiceLineValidation(this),
		{ IsImport: true } => new ImportJobComInvoiceLineValidation(this),
		_ => new JobComInvoiceLineValidation(this),
	};

	public new AddInfoJobComInvoiceLine AddInfo => (AddInfoJobComInvoiceLine)base.AddInfo;

	public new AddInfoJobComInvoiceLineLookups AddInfoLookups => (AddInfoJobComInvoiceLineLookups)base.AddInfoLookups;

	public new AddInfoJobComInvoiceLineValidation AddInfoValidation => (AddInfoJobComInvoiceLineValidation)base.AddInfoValidation;

	protected override EU.Business.Declaration.AddInfoJobComInvoiceLine GetNewAddInfo() => new AddInfoJobComInvoiceLine(JI_AddInfoInfo);

	public ZBool JI_TariffQuotaQtyVisible => !JI_ConcessionOrder.IsEmpty;

	public ZBool PacksMeasure => PackagesPivot.Cast<InvoiceLinePackagePivot>().All(x => !x.CHC_NumberOfPacks.IsEmpty);

	public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

	public new ICusFiscalReferenceCollection<CusFiscalReference> FiscalReferences => (ICusFiscalReferenceCollection<CusFiscalReference>)base.FiscalReferences;

	protected override ICusFiscalReferenceCollection<EU.Business.Declaration.CusFiscalReference> GetNewFiscalReferenceCollection() => new CusFiscalReferenceCollection<CusFiscalReference>(this);
	protected override Type FiscalReferenceType => typeof(CusFiscalReference);

	public new ICusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine> CusAuthorizationUsages => (ICusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine>)base.CusAuthorizationUsages;

	protected override ICusAuthorizationUsageCollection<EU.Business.CusAuthorizationUsage, EU.Business.Declaration.JobComInvoiceLine> GetCusAuthorizationUsages() => new EU.Business.CusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine>(this, Factory);

	protected override BaseCusLinkPackageCollection PackagesForInvoiceLinesCore() => new InvoiceLineCusLinkPackageCollection(this);

	protected override Customs.Business.InvoiceLinePackageValidation GetNewLinkPackValidationCore(BaseCusLinkPackage linkPackage) =>
		IsImport ? new ImportInvoiceLinePackageValidation(linkPackage, this) :
		IsExport ? new ExportInvoiceLinePackageValidation(linkPackage, this)
		: new InvoiceLinePackageValidation(linkPackage, this);

	#region CusSupportingInfoTypes

	public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;
	protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

	public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;
	protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

	public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;
	protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var result = base.GetCusSupportingInfoTypes();
		result[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
		result[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
		result[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
		return result;
	}

	#endregion

	protected override Dictionary<ZString, Type> GetCusCodeDataTypes()
	{
		var result = base.GetCusCodeDataTypes();
		result[EU.Business.CusCodeDataTypeList.Codes.AdditionalProcedureCode] = typeof(AdditionalProcedureCode);
		return result;
	}

	public override ZString JI_FormattedProcedure
	{
		get
		{
			return Declaration?.IsUCCCompliant ?? false ?
				DisplayFormatForProcedure(JI_Procedure) :
				JI_Procedure;
		}
		set => JI_Procedure = FormatForProcedure(value).Left(JI_ProcedureInfo.MaxLength);
	}

	static ZString FormatForProcedure(ZString unformattedProcedure)
	{
		return unformattedProcedure.KeepAlphanumericCharacters().Left(7);
	}

	static ZString DisplayFormatForProcedure(ZString unformattedProcedure)
	{
		var newProcedure = unformattedProcedure.KeepAlphanumericCharacters();
		ZString dottedProcedure = newProcedure.IsEmpty ? "" : newProcedure.SubstringSafe(0, 2) + " " + newProcedure.SubstringSafe(2, 2) + " " + newProcedure.SubstringSafe(4, 3).Trim();
		return dottedProcedure.Trim(' ');
	}

	public IZZRateSelectionCriteria ExciseRateSelectionCriteria => Factory.GetValue(ref exciseRateSelectionCriteria, GetExciseRateSelectionCriteriaCore);
	CachedProperty<IZZRateSelectionCriteria> exciseRateSelectionCriteria;

	IZZRateSelectionCriteria GetExciseRateSelectionCriteriaCore() => new RateSelectionCriteria<JobComInvoiceLine>(this, Universal.Constants.RateTypes.Excise, TaxTypeList.Codes.ExciseTax);

	#region AdditionalProcedureCodes

	public override ZInt MaxNumberOfAdditionalProcedureCode => 99;

	public new AdditionalProcedureCodeCollection AdditionalProcedureCodes => (AdditionalProcedureCodeCollection)base.AdditionalProcedureCodes;

	protected override EU.Business.AdditionalProcedureCodeCollection GetAdditionalProcedureCodeCollection() => new AdditionalProcedureCodeCollection(this);

	#endregion

	protected override ZString CustomsCountryCodeCore => Core.Constants.CountryCodes.Poland;
	protected override Type TypeOfPartUsedCore => typeof(OrgSupplierPart);

	protected override string ChargeCodeForOverseasFreight => PLCustomsChargeTypeList.Codes.AK;

	protected override string ChargeCodeForOverseasInsurance => PLCustomsChargeTypeList.Codes.BA;

	#region AddInfo

	#region Car Details

	public ZBool IsCarDetailsDataRequired => Factory.GetValue(ref isCarDetailsDataRequired, () =>
	{
		var tariffLeft8 = JI_Tariff.Left(8);
		return !tariffLeft8.IsEmpty
				&& ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory
					, tariffLeft8
					, GetDefaultDataGroupingCode()
					, UniversalReferenceConstants.RefCusCodeListType.Codes.CarDetailsRequiredCodes
					, ZDate.Today) != null;
	});
	CachedProperty<ZBool> isCarDetailsDataRequired;

	#region JI_MarkModel

	[BusinessObjectTestExclude]
	[ResourceStringData("PLJobComInvoiceLine|JI_MarkModel", Caption = "Make, Model")]
	public ZString JI_MarkModel
	{
		get => JI_BrandName.IsEmpty && FirstVehicle.CVH_ModelName.IsEmpty
			? ZString.Empty
			: (ZString)$"{JI_BrandName},{FirstVehicle.CVH_ModelName}"; //
		set
		{
			var oldValue = JI_MarkModel;
			var markModelAttemptedByCode = ZString.Empty;
			if (!value.IsEmpty && !value.Contains(","))
			{
				markModelAttemptedByCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, value, GetDefaultDataGroupingCode(), UniversalReferenceConstants.RefCusCodeListType.Codes.CarMarkModel, ZDateTime.Today)?.ZZD_Description ?? ZString.Empty;
			}
			SetCarMakeAndModel(markModelAttemptedByCode.IsEmpty ? value : markModelAttemptedByCode);
			if (!IsValidationSuspended)
			{
				PLValidationOrNull?.ValidateJI_MarkModel();
			}
			JI_MarkModelInfo.RefreshBinding(oldValue);
		}
	}

	public ZString MarkModelCode => MarkModel?.ZZD_Code ?? ZString.Empty;

	public ZZRefCusCodeListCombined MarkModel
	{
		get
		{
			var markModel = JI_MarkModel;
			return !markModel.IsEmpty
				? ZZRefCusCodeListCombined.Loader.Load(Factory, GetDefaultDataGroupingCode(), UniversalReferenceConstants.RefCusCodeListType.Codes.CarMarkModel, ZDateTime.Today, new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Description, markModel)).SingleOrDefault()
				: null;
		}
	}

	void SetCarMakeAndModel(ZString sourceString)
	{
		var make = ZString.Empty;
		var model = ZString.Empty;
		if (!sourceString.IsEmpty)
		{
			var splitString = sourceString.Split(new[] { ',' }, 2);

			if (splitString.Length >= 2)
			{
				make = splitString[0];
				model = splitString[1];
			}
			else
			{
				make = splitString[0];
				model = ZString.Empty;
			}
		}

		JI_BrandName = make.Left(Schema.JI_BrandNameMaxLength);
		FirstVehicle.CVH_ModelName = model.Left(AutoCusVehicle.Schema.CVH_ModelNameMaxLength);
	}

	public ZPropertyInfo JI_MarkModelInfo => GetZPropertyInfo(Schema.JI_MarkModel);

	#endregion

	protected override ICusVehicleCollection<Customs.Business.CusVehicle, BaseJobComInvoiceLine> GetNewCusVehicleCollection() => new CusVehicleCollection<CusVehicle, JobComInvoiceLine>(this);

	public override VehicleRelationshipType VehicleRelationship => VehicleRelationshipType.One;

	public new CusVehicle FirstVehicle => (CusVehicle)base.FirstVehicle;

	#endregion

	#endregion

	#region JI_Description

	[ResourceStringData("PLJobComInvoiceLine|JI_Description", Caption = "Goods Description EN")]
	[MaxLength(nameof(DescriptionMaxSize))]
	public override ZString JI_Description { get => base.JI_Description; set => base.JI_Description = value; }

	[ResourceStringData("PLJobComInvoiceLine|JI_NDescription", Caption = "Goods Description PL")]
	[MaxLength(nameof(DescriptionMaxSize))]
	public override ZString JI_NDescription { get => base.JI_NDescription; set => base.JI_NDescription = value; }

	public int DescriptionMaxSize => Declaration?.JE_MessageType.ToString() switch
	{
		Common.Shared.SharedJobMessageTypeList.Codes.Export when EntryInstruction?.IsAESTransitionPeriod() == true => 280,
		_ => 512,
	};

	#endregion

	#region ICusLinkPackageSupporter

	protected override ZBool IsSupportEmptyPackType(BasePackage package)
	{
		var pack = package as EU.Business.Declaration.Package;
		return pack != null && (PackageHelper.IsBulkCode(pack.CW_PackType, Factory) || PackageHelper.HasThePacksBeenDeclaredOnOtherInvoiceLines(pack));
	}

	#endregion

	public override ZGuid JI_JZ
	{
		get => base.JI_JZ;
		set
		{
			base.JI_JZ = value;
			InvoiceHeader?.MarkAsNeedingValidation();
		}
	}

	public override ZString JI_Procedure
	{
		get => base.JI_Procedure;
		set
		{
			base.JI_Procedure = value;
			InvoiceHeader?.MarkAsNeedingValidation();
		}
	}

	public override ZString JI_RN_NKCountryOfExport
	{
		get => base.JI_RN_NKCountryOfExport;
		set
		{
			base.JI_RN_NKCountryOfExport = value;
			Declaration?.MarkAsNeedingValidation();
		}
	}

	public IEnumerable<ZString> ConcessionCodes
	{
		get
		{
			return AdditionalProcedureCodes.Cast<AdditionalProcedureCode>().Select(x => x.CY_Code);
		}
	}

	[ResourceStringData("PLImportJobComInvoiceLine|GoodsOriginPref", Caption = "[34] Goods Origin (pref.)", MediumCaption = "[34] Origin (pref.)", ShortCaption = "Origin (p.)", FullDescription = "Goods Origin/Preferences country.")]
	public override ZString JI_CountryOfOrigin
	{
		get => base.JI_CountryOfOrigin;
		set => base.JI_CountryOfOrigin = value;
	}

	[ResourceStringData("PLImportJobComInvoiceLine|GoodsOriginCountry", Caption = "Goods Origin", MediumCaption = "Country/Region of Origin of the goods being moved.", ShortCaption = "Origin")]
	public override ZString ZG_CountryOfSupply
	{
		get => base.ZG_CountryOfSupply;
		set => base.ZG_CountryOfSupply = value;
	}

	[ReadOnlyMember(nameof(ProcedureIsEmpty))]
	[MaxLength(2)]
	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.PreviousCustomsProcedureCodes))]
	[ResourceStringData("PLJobComInvoiceLine|PreviousProcedureCode", Caption = "Previous Procedure")]
	public ZString PreviousProcedureCode
	{
		get
		{
			return JI_Procedure.SubstringSafe(2, 2);
		}
		set
		{
			var oldValue = PreviousProcedureCode;

			if (JI_Procedure.IsEmpty || JI_Procedure.Length < 2)
			{
				JI_Procedure = "00" + value;
			}
			else
			{
				JI_Procedure = JI_Procedure.SubstringSafe(0, 2) + value;
			}

			if (!IsValidationSuspended)
			{
				PLValidationOrNull?.ValidatePreviousProcedureCode();
				PLValidationOrNull?.ValidateProcedureCodeBase();
			}

			PreviousProcedureCodeInfo.RefreshBinding(oldValue);
		}
	}

	public ZPropertyInfo PreviousProcedureCodeInfo => GetZPropertyInfo(nameof(PreviousProcedureCode));

	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.RequestedCustomsProcedureCodes))]
	[MaxLength(2)]
	[ResourceStringData("E1AEFE0B-C13E-414C-B852-840EAA8FD804", Caption = "[37] CPC")]
	public ZString ProcedureCodeBase
	{
		get
		{
			return JI_Procedure.Left(2);
		}
		set
		{
			var oldValue = ProcedureCodeBase;

			if (value.IsEmpty)
			{
				JI_Procedure = string.Empty;
			}
			else
			{
				JI_Procedure = value + JI_Procedure.SubstringSafe(2);
			}

			if (!IsValidationSuspended)
			{
				PLValidationOrNull?.ValidateProcedureCodeBase();
			}

			ProcedureCodeBaseInfo.RefreshBinding(oldValue);
		}
	}

	public ZPropertyInfo ProcedureCodeBaseInfo => GetZPropertyInfo(nameof(ProcedureCodeBase));

	ZBool ProcedureIsEmpty => ProcedureCodeBase.IsEmpty;

	[ResourceStringData("PLJobComInvoiceLine|AdditionalProcedureCodesAsString", Caption = "Additional Procedure Code", MediumCaption = "Add. Procedure Code", ShortCaption = "Add. CPC")]
	public override ZString AdditionalProcedureCodesAsString => base.AdditionalProcedureCodesAsString;

	[ResourceStringData("PLJobComInvoiceLine|JI_DateForDuty", Caption = "Date for Duty")]
	public override ZDateTime JI_DateForDutyOverride { get => base.JI_DateForDutyOverride; set => base.JI_DateForDutyOverride = value; }

	[ResourceStringData("PLJobComInvoiceLine|JI_ValuationDate", Caption = "Valuation Date")]
	public override ZDateTime JI_ValuationDateOverride { get => base.JI_ValuationDateOverride; set => base.JI_ValuationDateOverride = value; }

	[ResourceStringData("97aaa402-e4a0-4f5a-8cb4-c22e746bc7dd", Caption = "Additional Qty 2")]
	public override ZDecimal JI_CustomsThirdQuantity { get => base.JI_CustomsThirdQuantity; set => base.JI_CustomsThirdQuantity = value; }

	[ResourceStringData("522e622f-f4b7-4185-9e04-f083b0477562", Caption = "Additional Qty 4")]
	public override ZDecimal JI_CustomsFifthQuantity { get => base.JI_CustomsFifthQuantity; set => base.JI_CustomsFifthQuantity = value; }

	[ResourceStringData("8BD9A011-4908-449B-875E-F5948BCD3BCF", Caption = "Target Entry Line #", FullDescription = "Target Entry Line Number")]
	public override ZShort JI_TargetEntryLineNumber
	{
		get { return base.JI_TargetEntryLineNumber; }
		set { base.JI_TargetEntryLineNumber = value; }
	}

	public new RefCurrencyCurrencyConverter CurrencyConverter => (RefCurrencyCurrencyConverter)base.CurrencyConverter;

	protected override CurrencyConverter GetCurrencyConverter()
	{
		return new RefCurrencyCurrencyConverter(Factory)
		{
			DateForRate = GetCurrencyConverterDateForRate,
			RateType = ExchangeRateType.Customs
		};
	}

	internal ZDateTime GetCurrencyConverterDateForRate => IsImport
		? NullIfNotValid(JI_ValuationDateOverride) ?? NullIfNotValid(EntryInstruction?.CEI_DateForDuty ?? ZDateTime.Empty) ?? ZDateTime.Today
		: NullIfNotValid(EntryInstruction?.CEI_DateForDuty ?? ZDateTime.Empty) ?? ZDateTime.Today;

	public override ZDateTime EffectiveAssessmentDate => IsImport
		? NullIfNotValid(JI_DateForDutyOverride) ?? NullIfNotValid(EntryInstruction?.CEI_DateForDuty ?? ZDateTime.Empty) ?? ZDateTime.Today
		: NullIfNotValid(EntryInstruction?.CEI_DateForDuty ?? ZDateTime.Empty) ?? ZDateTime.Today;

	static ZDateTime? NullIfNotValid(ZDateTime dateTime) => dateTime.IsValid ? dateTime : null;

	[MaxLength(Schema.JI_BrandNameMaxLength)]
	public override ZString JI_BrandName { get => base.JI_BrandName; set => base.JI_BrandName = value; }

	protected override bool GetJI_CustomsUnitQtyInfoReadOnly() => CustomsUnitQtyIsReadOnly(JI_CustomsUnitQty, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType);

	protected override ZBool JI_CustomsSecondUnitQty_ReadOnly => CustomsUnitQtyIsReadOnly(JI_CustomsSecondUnitQty, Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType);

	protected override ZBool JI_CustomsThirdUnitQty_ReadOnly => CustomsUnitQtyIsReadOnly(JI_CustomsThirdUnitQty, Universal.Constants.UnitOfMeasureTypes.CustomsUOM3Type);

	protected override ZBool JI_CustomsFourthUnitQty_ReadOnly => CustomsUnitQtyIsReadOnly(JI_CustomsFourthUnitQty, Universal.Constants.UnitOfMeasureTypes.CustomsUOM4Type);

	protected override ZBool JI_CustomsFifthUnitQty_ReadOnly => CustomsUnitQtyIsReadOnly(JI_CustomsFifthUnitQty, Universal.Constants.UnitOfMeasureTypes.CustomsUOM5Type);

	ZBool CustomsUnitQtyIsReadOnly(ZString unitQty, ZString uomType)
	{
		ZString[] uomTypes =
		[
			Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType,
			Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType,
			Universal.Constants.UnitOfMeasureTypes.CustomsUOM3Type,
			Universal.Constants.UnitOfMeasureTypes.CustomsUOM4Type,
			Universal.Constants.UnitOfMeasureTypes.CustomsUOM5Type,
		];

		var uomTypeIndex = uomTypes.IndexOf(x => x == uomType);

		var result = !unitQty.IsEmpty && uomTypeIndex >= 0 && (UniversalTariffIsUsed(uomTypes.Skip(uomTypeIndex)) || UniversalRateIsUsed(UniversalDutyRate) || UniversalRateIsUsed(UniversalExciseRate) || QuotaIsUsed());
		return result;

		ZBool UniversalTariffIsUsed(IEnumerable<ZString> applicableUOMTypes)
			=> UniversalTariff is TariffView tariff
				&& tariff.UnitsOfMeasure.Any(uom => uom.ZZ8_UOM == unitQty && applicableUOMTypes.Contains(uom.ZZ8_Type) && CountryIsNullOrEqualCountryOfOrigin(uom));

		ZBool UniversalRateIsUsed(BusinessObject universalRate) => universalRate is RateView rate && rate.UnitsOfMeasure.Any(uom => uom.ZXG_UOM == unitQty);

		bool CountryIsNullOrEqualCountryOfOrigin(TariffUOMView uom)
		{
			var originCountry = JI_CountryOfOrigin;
			return uom.CusTradeGroup?.TradeGroupCountries.Any(country => country.ZZB_RN_NKTradeGroupCountryCode == originCountry) ?? true;
		}

		ZBool QuotaIsUsed()
		{
			var quotaIsUsed = false;
			var quotaUom = GetRefCusQuota(this)?.ZXQ_UnitOfMeasure ?? ZString.Empty;
			if (!quotaUom.IsEmpty && unitQty == quotaUom)
			{
				IReadOnlyCollection<ZPropertyInfo> unitQtyInfos =
				[
					JI_CustomsUnitQtyInfo,
					JI_CustomsSecondUnitQtyInfo,
					JI_CustomsThirdUnitQtyInfo,
					JI_CustomsFourthUnitQtyInfo,
					JI_CustomsFifthUnitQtyInfo,
				];
				var unitQtyValuesBeforeCurrent = unitQtyInfos.Take(uomTypeIndex).Select(x => (ZString)x.Value).Where(x => !x.IsEmpty).ToHashSet();
				quotaIsUsed = !unitQtyValuesBeforeCurrent.Contains(quotaUom) && !isConvertibleFrom(quotaUom, unitQtyValuesBeforeCurrent, CustomsCountryCode, Factory);
			}
			return quotaIsUsed;
		}
	}

	[ResourceStringData("PLJobComInvoiceLine|ZG_CountryOfDestination", Caption = "Destination")]
	public override ZString ZG_CountryOfDestination { get => base.ZG_CountryOfDestination; set => base.ZG_CountryOfDestination = value; }

	protected override ICustomsUnitDefaultingStrategy GetCustomsUnitDefaultingStrategy()
		=> new CompositeCustomsUnitDefaultingStrategy(GetUniversalRateCustomsUnitDefaultingStrategy(), GetQuotaCustomsUnitDefaultingStrategy());

	ICustomsUnitDefaultingStrategy GetUniversalRateCustomsUnitDefaultingStrategy()
		=> new UniversalRateCustomsUnitDefaultingStrategy<JobComInvoiceLine>((Func<JobComInvoiceLine, RateView[]>)GetRatesForUniversalRateUnitStrategy, [], RateCalcUnitOfMeasureAggregator.IsConvertableFrom);

	RateView[] GetRatesForUniversalRateUnitStrategy(JobComInvoiceLine invoiceLine) => [invoiceLine.UniversalDutyRate, UniversalExciseRate];

	public RateView UniversalExciseRate => UniversalTariff?.GetApplicableRates(ExciseRateSelectionCriteria).FirstOrDefault(x => UniversalTariffDutyRateCode.IsEmpty || x.RateCode == UniversalTariffDutyRateCode);

	ICustomsUnitDefaultingStrategy GetQuotaCustomsUnitDefaultingStrategy() =>
		new QuotaCustomsUnitDefaultingStrategy<JobComInvoiceLine>(GetRefCusQuota, isConvertibleFrom, additionalCustomsUnitQtyInfos: [JI_CustomsFourthUnitQtyInfo, JI_CustomsFifthUnitQtyInfo]);

	internal RefCusQuota GetRefCusQuota() => GetRefCusQuota(this);

	RefCusQuota GetRefCusQuota(JobComInvoiceLine invoiceLine) => IsImport
		? new RefCusQuota.Loader(Factory).GetFirst(GetDefaultDataGroupingCode(), EffectiveAssessmentDate, invoiceLine.JI_ConcessionOrder)
		: null;

	readonly IsConvertibleFrom isConvertibleFrom = RateCalcUnitOfMeasureAggregator.IsConvertableFrom;

	protected override IEnumerable<IZZRateSelectionCriteria> GetNationalRateSelectionCriteriaCore()
		=> base.GetNationalRateSelectionCriteriaCore().Append(ExciseRateSelectionCriteria);

	bool IsCommercialInvoiceLine => !Declaration?.IsPersistent ?? true;

	protected override IRefCountry CountryOfOriginFallbackCore
	{
		get
		{
			if (JI_CountryOfOrigin.IsEmpty)
			{
				return null;
			}

			var country = Lookups.CountryOfOrigins.Cast<ICountry>().FirstOrDefault(x => x.Code == JI_CountryOfOrigin);
			return new DataTransferCountryInfo(JI_CountryOfOrigin, country?.Description);
		}
	}
}
