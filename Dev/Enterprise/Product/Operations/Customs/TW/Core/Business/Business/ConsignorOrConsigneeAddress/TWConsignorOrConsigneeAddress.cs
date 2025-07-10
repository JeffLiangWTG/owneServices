using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public abstract class TWConsignorOrConsigneeAddress : JobDocAddress
	{
		public TWConsignorOrConsigneeAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new JobDeclaration Parent => (JobDeclaration)base.Parent;

		public override bool IsEmpty => !E2_OA_Address.IsValid && E2_GovRegNum.IsEmpty && E2_GovRegNumType.IsEmpty;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			E2_GovRegNumType = ZString.Empty;
		}

		[ReadOnly(false)]
		public override ZString E2_GovRegNum
		{
			get => new ZString(GetValueFromRowSafely(JobDocAddressSchema.E2_GovRegNum));
			set
			{
				value = value.TrimEndSpaceTab();
				if (this.E2_GovRegNum != value)
				{
					var e2_GovRegNumInfo = E2_GovRegNumInfo;
					CheckMaximumLength(e2_GovRegNumInfo, value);
					SetPropertyValue(e2_GovRegNumInfo, value);
					if (!base.IsValidationSuspended)
					{
						Validation.ValidateE2_GovRegNum();
					}
				}
			}
		}

		[ReadOnly(false)]
		public override ZString E2_GovRegNumType
		{
			get => new ZString(GetValueFromRowSafely(JobDocAddressSchema.E2_GovRegNumType));
			set
			{
				value = value.TrimEndSpaceTab();
				if (this.E2_GovRegNumType != value)
				{
					var e2_GovRegNumTypeInfo = E2_GovRegNumTypeInfo;
					CheckMaximumLength(e2_GovRegNumTypeInfo, value);
					SetPropertyValue(e2_GovRegNumTypeInfo, value);
					DefaultValueWhenGovRegNumTypeChanged(value);
					if (!base.IsValidationSuspended)
					{
						Validation.ValidateE2_GovRegNumType();
					}
				}
			}
		}

		public override ZGuid E2_OA_Address
		{
			get => base.E2_OA_Address;
			set
			{
				if (E2_OA_Address != value)
				{
					base.E2_OA_Address = value;
					DefaultValueWhenAddressChanged();
				}
			}
		}

		protected void DefaultValueFromCompanyName(OrgAddress address)
		{
			E2_GovRegNumType = OrgCusCode.CodeTypes.VATCode;

			var countryCode = address.OA_RN_NKCountryCode;
			var companyName = address.CompanyName;
			if (countryCode == Core.Constants.CountryCodes.UnitedStates)
			{
				E2_GovRegNum = FormattableString.Invariant($"{SharedHelper.GetLetterFromEnglishName(companyName).PadRight(6, 'Z')}{address.StateCode.Left(2)}");
			}
			else if (countryCode != Core.Constants.CountryCodes.Taiwan)
			{
				E2_GovRegNum = SharedHelper.GetLetterFromEnglishName(companyName).PadRight(6, 'Z');
			}
		}

		protected abstract void DefaultValueWhenAddressChanged();

		protected abstract void DefaultValueWhenGovRegNumTypeChanged(ZString govRegNumType);

		protected override void ClearAddressOverrides() { }

		protected override JobDocAddressLookups GetNewLookups() => new TWConsignorOrConsigneeAddressLookups(this);

		protected override JobDocAddressValidation GetNewValidation() => new TWConsignorOrConsigneeAddressValidation(this);

		public new TWConsignorOrConsigneeAddressValidation Validation => (TWConsignorOrConsigneeAddressValidation)base.Validation;
	}
}
