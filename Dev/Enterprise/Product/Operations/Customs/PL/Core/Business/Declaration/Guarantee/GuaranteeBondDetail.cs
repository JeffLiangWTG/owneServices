using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.Business.Declaration;

public class GuaranteeBondDetail : GuaranteeForEntryInstruction
{
	public GuaranteeBondDetail(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
		PW_BondTypeInfo.ValueChanged += PW_BondTypeInfo_ValueChanged;
	}

	void PW_BondTypeInfo_ValueChanged(object sender, EventArgs e)
	{
		Validation.ValidateAll();
	}

	public new CusEntryInstruction EntryInstruction => Parent as CusEntryInstruction;

	#region Schema
	public new class Schema : AutoCusBondDetail.Schema
	{
		public const string ApplicationCode = "GUA";
		public const string ActivityCode = "CON";
	}
	#endregion

	[MaxLength(3)]
	[List(nameof(Lookups) + "." + nameof(GuaranteeBondDetailLookups.BondTypeList))]
	[ResourceStringData("PLGuaranteeBondDetail|PW_BondType", Caption = "Type")]
	public override ZString PW_BondType
	{
		get => base.PW_BondType;
		set
		{
			if (base.PW_BondType != value)
			{
				base.PW_BondType = value;
				PW_CPH_Guarantee = ZGuid.Empty;
				PW_BondNumber = ZString.Empty;
				PW_HolderIdentification = ZString.Empty;
				PW_Password = ZString.Empty;
				correspondingCusGuaranteeHeader = null;
			}
		}
	}

	[MaxLength(3)]
	[ReadOnlyMember(nameof(IsPW_CPH_GuaranteeReadOnly))]
	[List(nameof(Lookups) + "." + nameof(GuaranteeBondDetailLookups.GuaranteeCollection))]
	[ResourceStringData("PLGuaranteeBondDetail|PW_CPH_Guarantee", Caption = "Guarantee")]
	public override ZGuid PW_CPH_Guarantee
	{
		get => base.PW_CPH_Guarantee;
		set
		{
			if (PW_CPH_Guarantee != value)
			{
				base.PW_CPH_Guarantee = value;
				if (IsBondTypeAsCON)
				{
					OnBondTypeCONCPH_GuaranteeChanged(value);
				}
			}
		}
	}

	void OnBondTypeCONCPH_GuaranteeChanged(ZGuid newGuid)
	{
		if (newGuid.IsValid)
		{
			var cusGuaranteeHeader = CorrespondingCusGuaranteeHeader;
			if (cusGuaranteeHeader != null)
			{
				PW_BondNumber = cusGuaranteeHeader.CPH_Number;
				PW_Password = cusGuaranteeHeader.MainAccessCode.SubstringSafe(0, 4);

				OrgCusCode holder = cusGuaranteeHeader.PermitHolder?.CustomsCodes?.Where(x => IsExpectedOrgCusCode(x))?.FirstOrDefault();
				PW_HolderIdentification = holder?.OK_CustomsRegNo ?? ZString.Empty;
			}
		}
		else
		{
			PW_BondNumber = ZString.Empty;
			PW_HolderIdentification = ZString.Empty;
			PW_Password = ZString.Empty;
			correspondingCusGuaranteeHeader = null;
		}
	}

	bool IsExpectedOrgCusCode(OrgCusCode cusCode) => cusCode.OK_CodeType == OrgCusCode.PolandCodeTypes.TIN && cusCode.OK_RN_NKCodeCountry == CountryCodes.Poland;

	[ReadOnlyMember(nameof(IsBondTypeAsCON))]
	[ResourceStringData("PLGuaranteeBondDetail|PW_BondNumber", Caption = "Guarantee Number")]
	public override ZString PW_BondNumber
	{
		get => base.PW_BondNumber.ToUpper();
		set => base.PW_BondNumber = value.ToUpper();
	}

	protected override ZBool ShouldSetupHolderIdentificationOnBondNumberChange => false;

	[MaxLength(17)]
	[ReadOnly(true)]
	[ResourceStringData("PLGuaranteeBondDetail|PW_HolderIdentification", Caption = "Holder ID")]
	public override ZString PW_HolderIdentification
	{
		get => base.PW_HolderIdentification.ToUpper();
		set => base.PW_HolderIdentification = value.ToUpper();
	}

	[ReadOnlyMember(nameof(IsPasswordReadOnly))]
	[ResourceStringData("PLGuaranteeBondDetail|PW_Password", Caption = "Access Code")]
	[Password]
	public override ZString PW_Password
	{
		get => base.PW_Password;
		set => base.PW_Password = value;
	}

	[ReadOnlyMember(nameof(IsAmountReadOnly))]
	[ResourceStringData("PLGuaranteeBondDetail|PW_BondAmount", Caption = "Amount")]
	public override ZDecimal PW_BondAmount
	{
		get => base.PW_BondAmount;
		set => base.PW_BondAmount = value;
	}

	[ReadOnly(true)]
	[ResourceStringData("PLGuaranteeBondDetail|PW_RX_NKCurrency", Caption = "Currency", ShortCaption = "Cur.")]
	public override ZString PW_RX_NKCurrency
	{
		get => base.PW_RX_NKCurrency;
		set => base.PW_RX_NKCurrency = value;
	}

	BaseCusGuaranteeHeader CorrespondingCusGuaranteeHeader
	{
		get
		{
			if (correspondingCusGuaranteeHeader == null || correspondingCusGuaranteeHeader.IsDeleted || correspondingCusGuaranteeHeader.PK != PW_CPH_Guarantee)
			{
				correspondingCusGuaranteeHeader = Factory.Load<BaseCusGuaranteeHeader>(PW_CPH_Guarantee);
			}
			return correspondingCusGuaranteeHeader;
		}
	}
	BaseCusGuaranteeHeader correspondingCusGuaranteeHeader;

	public new GuaranteeBondDetailLookups Lookups => (GuaranteeBondDetailLookups)base.Lookups;

	public new GuaranteeBondDetailValidation Validation => (GuaranteeBondDetailValidation)base.Validation;

	protected override CusBondDetailLookups GetNewLookups() => new GuaranteeBondDetailLookups(this);

	protected override CusBondDetailValidation GetNewValidation() => new GuaranteeBondDetailValidation(this);

	bool GuaranteeHeaderHasAdditionalAccessCodes => CorrespondingCusGuaranteeHeader?.AdditionalAccessCodes.Any() ?? false;

	bool GuaranteeHeaderMainAccessCodeIsEmpty => CorrespondingCusGuaranteeHeader?.MainAccessCode.IsEmpty ?? true;

	protected bool IsAmountReadOnly => EntryInstruction.HasOnlyOneGuarantee();

	public bool IsBondTypeAsCON => PW_BondType == GuaranteeBondTypeList.Codes.Comprehensive;

	protected bool IsPW_CPH_GuaranteeReadOnly => !IsBondTypeAsCON;

	protected bool IsPasswordReadOnly => Factory.GetValue(ref isPasswordReadOnly, () => !IsBondTypeAsCON || CorrespondingCusGuaranteeHeader == null || !(GuaranteeHeaderMainAccessCodeIsEmpty || GuaranteeHeaderHasAdditionalAccessCodes));
	CachedProperty<bool> isPasswordReadOnly;
}
