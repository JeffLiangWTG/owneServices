using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.NO.Business;

public class CusEntryLineFee : Customs.Business.CusEntryLineFee, IDutyCategory
{
	public CusEntryLineFee(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	[ReadOnly(true)]
	[MaxLength(2)]
	[ResourceStringData("62734A9F-BAE1-426E-9E5C-C819478C736F", Caption = "Duty Code")]
	public ZString CF_DutyCode
	{
		get => DisplayDutyCode.SubstringSafe(0, 2).TrimEnd();
	}

	public ZPropertyInfo CF_DutyCodeInfo => GetZPropertyInfo(nameof(CF_DutyCode));

	[ReadOnly(true)]
	[ResourceStringData("541EAC5D-75CB-4172-9F6E-5B31E4880DB7", Caption = "Sequence")]
	public ZString CF_Sequence
	{
		get => DisplayDutyCode.SubstringSafe(2);
	}

	public ZPropertyInfo CF_SequenceInfo => GetZPropertyInfo(nameof(CF_Sequence));

	[ReadOnly(true)]
	[ResourceStringData("487B29CA-5A21-437D-9759-362E4F4A564B", Caption = "Rate type")]
	public ZString CF_RateType
	{
		get
		{
			return base.CF_MethodOfPayment;
		}
	}

	public ZPropertyInfo CF_RateTypeInfo => GetZPropertyInfo(nameof(CF_RateType));

	[ReadOnly(true)]
	[DecimalPlaces(4)]
	[ResourceStringData("0A30538A-BA60-453A-A417-538415F21A92", Caption = "Rate")]
	public override ZDecimal CF_Rate
	{
		get
		{
			return base.CF_Rate;
		}
	}

	[ReadOnly(true)]
	[DecimalPlaces(3)]
	[ResourceStringData("94D8E5EF-7E2C-4595-BB8A-2971E7344495", Caption = "Base value")]
	public override ZDecimal CF_BaseValue
	{
		get
		{
			return base.CF_BaseValue;
		}
	}

	[ReadOnly(true)]
	[DecimalPlaces(0)]
	[ResourceStringData("B08A52F5-D8D5-4830-9814-DE6BC2D42431", Caption = "Duty amt in NOK")]
	public override ZDecimal CF_ChargeAmount
	{
		get
		{
			return base.CF_ChargeAmount;
		}
	}

	[ResourceStringData("18A17D59-731C-4B84-4A15-A4D7C06201A9", Caption = "Payable to Customs/Tax authorities", ShortCaption = "Payable to")]
	public ZString IsLandedCostOnlyAsText => CF_IsLandedCostOnly ? ResString.GetMultilingualString("C7CAF36B-0C72-A9B9-48C6-01A22F768763", "Tax Auth.") : ResString.GetMultilingualString("807684BC-7C9F-A5AB-4C48-08A3332C454E", "Customs");

	public ZPropertyInfo IsLandedCostOnlyAsTextInfo => GetZPropertyInfo(nameof(IsLandedCostOnlyAsText));

	public bool IsCustomsDuty => Factory.GetValue(ref isCustomsDutyCached, () => DutyCode switch
	{
		Core.Constants.Customs.CusEntryFeeTypes.DutyAmount => true,
		NOCustomDutyCodeList.Codes.TL1 => true,
		_ => false,
	});
	CachedProperty<bool> isCustomsDutyCached;

	public bool IsAgriculturalDuty => Factory.GetValue(ref isAgriculturalDutyCached, () => DutyCode == NOCustomDutyCodeList.Codes.RT100);
	CachedProperty<bool> isAgriculturalDutyCached;

	public bool IsExciseDuty => Factory.GetValue(ref isExciseDutyCached, () => !IsCustomsDuty && !IsAgriculturalDuty && CF_DutyCode != ((ZString)NOCustomDutyCodeList.Codes.MV1).SubstringSafe(0, 2));
	CachedProperty<bool> isExciseDutyCached;

	public bool IsVAT => Factory.GetValue(ref isVATCached, () => DutyCode switch
	{
		NOCustomDutyCodeList.Codes.MV1 => !CF_IsLandedCostOnly,
		NOCustomDutyCodeList.Codes.MV2 => !CF_IsLandedCostOnly,
		_ => false,
	});
	CachedProperty<bool> isVATCached;

	public string DutyCode => CF_ChargeType.ToUpper();

	ZString DisplayDutyCode => base.CF_ChargeType == Constants.RateTypes.Duty ? NOCustomDutyCodeList.Codes.TL1 : base.CF_ChargeType;

	public new CusEntryLineFee Clone() => (CusEntryLineFee)base.Clone();

	public new CusEntryLineFeeLookups Lookups => (CusEntryLineFeeLookups)base.Lookups;

	protected override Customs.Business.CusEntryLineFeeLookups GetNewLookups() => new CusEntryLineFeeLookups(this);
}
