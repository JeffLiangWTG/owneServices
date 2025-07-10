using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business;

public sealed class CusClassPartPivot(BusinessObjectFactory factory, DataRow row) : AutoNOCusClassPartPivot(factory, row)
	, ISupplementaryCodeSupporter
{
	public new CusClassPartPivotLookups Lookups => (CusClassPartPivotLookups)base.Lookups;

	protected override Customs.Business.CusClassPartPivotLookups GetNewLookups()
	{
		return new CusClassPartPivotLookups(this);
	}

	[ResourceStringData("66679BC9-9C06-4F09-8521-1AAC8027F0E5", Caption = "Tariff")]
	public override ZString CI_FormattedTariffNum
	{
		get { return base.CI_FormattedTariffNum; }
		set { base.CI_FormattedTariffNum = value; }
	}

	[MaxLength(3)]
	[List(nameof(Lookups) + "+" + nameof(CusClassPartPivotLookups.PrimaryPreferenceList))]
	public ZString PreferenceCode
	{
		get { return CI_PrimaryPreference; }
		set { CI_PrimaryPreference = value; }
	}

	public ZPropertyInfo PreferenceCodeInfo => GetWrappedZPropertyInfo(nameof(PreferenceCode), x => CI_PrimaryPreferenceInfo);

	[List(nameof(Lookups) + "+" + nameof(CusClassPartPivotLookups.VATCodeList))]
	[ResourceStringData("4E12FC72-D71D-4208-BA7A-04049FAE1F67", Caption = "VAT Code")]
	public override ZString CI_ZZF_NKTaxType
	{
		get => base.CI_ZZF_NKTaxType;
		set => base.CI_ZZF_NKTaxType = value;
	}

	[List(nameof(Lookups) + "+" + nameof(CusClassPartPivotLookups.ReducedCustomsFlagList))]
	[ResourceStringData("5F4D2B82-D7E2-4E9E-94A1-34D085B03CD9", Caption = "Reduced custom")]
	public override ZString CI_ReducedCustomsFlag
	{
		get => base.CI_ReducedCustomsFlag;
		set => base.CI_ReducedCustomsFlag = value;
	}

	[List(nameof(Lookups) + "+" + nameof(CusClassPartPivotLookups.CountyOfOriginList))]
	[ResourceStringData("39D09972-85CE-4687-9E80-69B335CFA194", Caption = "County Of Origin")]
	public override ZString CI_RW_NKOriginState
	{
		get => base.CI_RW_NKOriginState;
		set => base.CI_RW_NKOriginState = value;
	}

	[ResourceStringData("3E5AD941-0051-45B3-8E24-408E980F99AD", Caption = "Excise codes")]
	[MaxLength(SupplementaryCode.Schema.CY_CodeMaxLength)]
	public ZString CI_Supplement1
	{
		get => SupplementaryCode1?.CY_Code ?? ZString.Empty;
		set => SupplementaryCodeHandler.LoadOrCreate(value, this, 1, CI_Supplement1Info);
	}

	public ZPropertyInfo CI_Supplement1Info
		=> SupplementaryCode1 != null
			? GetWrappedZPropertyInfo(nameof(CI_Supplement1), x => SupplementaryCode1.CY_CodeInfo)
			: GetZPropertyInfo(nameof(CI_Supplement1));

	SupplementaryCode SupplementaryCode1
	{
		get
		{
			if (supplementaryCode1 is null || supplementaryCode1.IsDeleted)
			{
				supplementaryCode1 = new BaseSupplementaryCode
						.Loader(Factory)
						.Load<SupplementaryCode, CusClassPartPivot>(this, 1);
				if (supplementaryCode1 is not null)
				{
					RegisterEditableChildObject(supplementaryCode1);
				}
			}

			return supplementaryCode1;
		}
	}
	SupplementaryCode supplementaryCode1;

	[MaxLength(SupplementaryCode.Schema.CY_CodeMaxLength)]
	public ZString CI_Supplement2
	{
		get => SupplementaryCode2?.CY_Code ?? ZString.Empty;
		set => SupplementaryCodeHandler.LoadOrCreate(value, this, 2, CI_Supplement2Info);
	}

	public ZPropertyInfo CI_Supplement2Info
		=> SupplementaryCode2 != null
			? GetWrappedZPropertyInfo(nameof(CI_Supplement2), x => SupplementaryCode2.CY_CodeInfo)
			: GetZPropertyInfo(nameof(CI_Supplement2));

	SupplementaryCode SupplementaryCode2
	{
		get
		{
			if (supplementaryCode2 is null || supplementaryCode2.IsDeleted)
			{
				supplementaryCode2 = new BaseSupplementaryCode
						.Loader(Factory)
						.Load<SupplementaryCode, CusClassPartPivot>(this, 2);
				if (supplementaryCode2 is not null)
				{
					RegisterEditableChildObject(supplementaryCode2);
				}
			}

			return supplementaryCode2;
		}
	}
	SupplementaryCode supplementaryCode2;

	[ReadOnly(true)]
	public ZString CI_AdditionalSupplements => AdditionalSupplementaryCodes.AsString;

	public ZPropertyInfo CI_AdditionalSupplementsInfo => GetZPropertyInfo(nameof(CI_AdditionalSupplements));

	public new CusClassPartPivotValidation Validation => (CusClassPartPivotValidation)base.Validation;

	protected override Customs.Business.CusClassPartPivotValidation GetNewValidation() => new CusClassPartPivotValidation(this);

	ICusCodeDataCollection<BaseSupplementaryCode> ISupplementaryCodeSupporter.AdditionalSupplementaryCodes => AdditionalSupplementaryCodes;

	[ChildEditable(true)]
	public SupplementaryCodeCollection AdditionalSupplementaryCodes => additionalSupplementaryCodes ??= CreateAdditionalSupplementaryCodesCollection();
	SupplementaryCodeCollection additionalSupplementaryCodes;

	SupplementaryCodeCollection CreateAdditionalSupplementaryCodesCollection()
	{
		var provider = BaseSupplementaryCodeProvider.GetBySupplementaryCodeSupporter(this);
		var codesCollection = new SupplementaryCodeCollection(CI_AdditionalSupplementsInfo, provider);
		RegisterEditableChildObject(codesCollection);
		codesCollection.Load();
		return codesCollection;
	}

	ZString ISupplementaryCodeSupporter.GetCountryCodeFromAdditionalCode(ZString additionalCode) => (string)GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

	IEnumerable<BaseSupplementaryCode> ISupplementaryCodeSupporter.SupplementaryCodes
		=> AdditionalSupplementaryCodes
			.Union([SupplementaryCode1, SupplementaryCode2])
			.WhereNotNull();

	ZString ISupplementaryCodeSupporter.SupplementaryCodesFieldType => nameof(FieldType.Text);

	ResourceStringData ISupplementaryCodeSupporter.SupplementaryCodeCaption => null;

	ZString ICusCodeDataWithOrderSupporter.GetCountryCodeForCodeProvider() => GetCountryCodeForSupplementaryCodeHelper((OrgSupplierPart)base.Part);

	string GetCountryCodeForSupplementaryCodeHelper(OrgSupplierPart part)
	{
		return part?.PivotsForBinding.countryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
	}

	void ICusCodeDataWithOrderSupporter.OnCodesChanged()
	{
	}

	TariffView ICusCodeDataWithOrderSupporter.Tariff => null;

	IZZRateSelectionCriteria ICusCodeDataWithOrderSupporter.RateSelectionCriteria => null;

	CodeDescriptionPairList ICusCodeDataWithOrderSupporter.CachedListOfAdditionalCodeDescriptions => null;

	ISupplementaryCodeHandler<SupplementaryCode> SupplementaryCodeHandler => supplementaryCodeHandler ??= new BaseSupplementaryCodeHandler<SupplementaryCode>();
	ISupplementaryCodeHandler<SupplementaryCode> supplementaryCodeHandler;
}
