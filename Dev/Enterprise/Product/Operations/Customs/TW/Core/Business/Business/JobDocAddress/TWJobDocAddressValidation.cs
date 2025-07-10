using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class TWJobDocAddressValidation : JobDocAddressValidation
	{
		public TWJobDocAddressValidation(AutoJobDocAddress parent) : base(parent)
		{
		}

		public new TWJobDocAddress Parent => (TWJobDocAddress)base.Parent;

		public static IEnumerable<ZString> BondIdCodeTypes
		{
			get
			{
				yield return OrgCusCode.TaiwanCodeTypes.EPZ;
				yield return OrgCusCode.TaiwanCodeTypes.CBF;
				yield return OrgCusCode.TaiwanCodeTypes.AgriculturalTechnologyPark;
				yield return OrgCusCode.TaiwanCodeTypes.SciencePark;
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateCodes();
			}
		}

		internal void ValidateCodes()
		{
			ValidateIDCodeType();
			ValidateCBPCodeType();
			ValidateFRICodeType();
			ValidateIDCode();
			ValidateAEOCode();
			ValidateTPCCode();
			ValidateCBPCode();
			ValidateFRICode();
		}

		public void ValidateIDCode()
		{
			((IValidationInternals)this).Validate(Parent.IDCodeInfo, () => { CheckIDCode(); });
		}

		public void ValidateAEOCode()
		{
			((IValidationInternals)this).Validate(Parent.AEOCodeInfo, () => { CheckAEOCode(); });
		}

		public void ValidateTPCCode()
		{
			((IValidationInternals)this).Validate(Parent.TPCCodeInfo, () => { CheckTPCCode(); });
		}

		public void ValidateCBPCode()
		{
			((IValidationInternals)this).Validate(Parent.CBPCodeInfo, () => { CheckCBPCode(); });
		}

		public void ValidateFRICode()
		{
			((IValidationInternals)this).Validate(Parent.FRICodeInfo, () => { CheckFRICode(); });
		}

		public void ValidateIDCodeType()
		{
			((IValidationInternals)this).Validate(Parent.IDCodeTypeInfo, () => { CheckIDCodeType(); });
		}

		public void ValidateCBPCodeType()
		{
			((IValidationInternals)this).Validate(Parent.CBPCodeTypeInfo, () => { CheckCBPCodeType(); });
		}

		public void ValidateFRICodeType()
		{
			((IValidationInternals)this).Validate(Parent.FRICodeTypeInfo, () => { CheckFRICodeType(); });
		}

		protected void CheckIDCodeType()
		{
			var parent = Parent;
			ListValidation.ErrorIfInvalidCode(parent.IDCodeTypeInfo);
			(parent.Requirement as TWJobDocAddressRequirement)?.ValidateIDCodeType?.Invoke(this);
		}

		protected void CheckCBPCodeType()
		{
			var parent = Parent;
			ListValidation.ErrorIfInvalidCode(Parent.CBPCodeTypeInfo);
			(parent.Requirement as TWJobDocAddressRequirement)?.ValidateCBPCodeType?.Invoke(this);
		}

		protected void CheckFRICodeType()
		{
			ListValidation.ErrorIfInvalidCode(Parent.FRICodeTypeInfo);
		}

		protected void CheckIDCode()
		{
			var parent = Parent;
			var propertyInfo = parent.IDCodeInfo;
			var idCode = parent.IDCode;
			var idCodeType = parent.IDCodeType;
			if (idCodeType == OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber)
			{
				var customsRegistrationNumberValidation = new DocAddressCustomsRegistrationNumberValidation(parent);
				customsRegistrationNumberValidation.CheckFRICustomsRegistrationNumber(propertyInfo, idCode, CargoWise.ComponentModel.NotificationType.Warning);
			}
			else
			{
				ValidateCustomsCode(parent, idCodeType, idCode, propertyInfo);
			}
			(parent.Requirement as TWJobDocAddressRequirement)?.ValidateIDCode?.Invoke(this);
		}

		protected void CheckAEOCode()
		{
			var parent = Parent;
			var code = parent.AEOCode;
			if (!code.IsEmpty)
			{
				OrgCusCodeValidation.ValidateCustomsCode(new DocAddressCustomsRegistrationNumberValidation(parent), parent.E2_RN_NKCountryCode, parent.AEOCodeType, code, parent.AEOCodeInfo);
			}
		}

		protected override void CheckE2_RN_NKCountryCode()
		{
			base.CheckE2_RN_NKCountryCode();
			var parent = Parent;
			if (parent.E2_AddressOverride && !parent.AEOCode.IsEmpty)
			{
				MandatoryValidation.CheckEntered(parent.E2_RN_NKCountryCodeInfo);
			}
		}

		protected void CheckTPCCode()
		{
			var parent = Parent;
			ValidateCustomsCode(parent, parent.TPCCodeType, parent.TPCCode, parent.TPCCodeInfo);
		}

		protected void CheckCBPCode()
		{
			var parent = Parent;
			ValidateCustomsCode(parent, parent.CBPCodeType, parent.CBPCode, parent.CBPCodeInfo);
			(parent.Requirement as TWJobDocAddressRequirement)?.ValidateCBPCode?.Invoke(this);
		}

		protected void CheckFRICode()
		{
			var parent = Parent;
			ValidateCustomsCode(parent, parent.FRICodeType, parent.FRICode, parent.FRICodeInfo);
			(parent.Requirement as TWJobDocAddressRequirement)?.ValidateFRICode?.Invoke(this);
		}

		void ValidateCustomsCode(TWJobDocAddress parent, ZString codeType, ZString customsRegNo, ZPropertyInfo customsRegNoInfo)
		{
			if (!customsRegNo.IsEmpty)
			{
				if (parent.E2_AddressOverride || codeType == OrgCusCode.CodeTypes.PassportID)
				{
					OrgCusCodeValidation.ValidateCustomsCode(new DocAddressCustomsRegistrationNumberValidation(parent), parent.E2_RN_NKCountryCode, codeType, customsRegNo, customsRegNoInfo);
				}
			}
		}

		protected override void CheckE2_OA_Address()
		{
			var requirement = Parent.Requirement;
			if (requirement != null && requirement.ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address != null)
			{
				requirement.ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address(this);
			}
		}

		public void ValidateLocalAddressE2_Address1Maxlength()
		{
			var parent = Parent;
			if (parent.E2_AddressOverride && parent.IsImport)
			{
				var chineseAddressData = new AddressData(parent, Core.SharedConstants.Languages.ChineseTraditional);
				if (chineseAddressData.ChineseTraditionalAddressFormat.Length > ValidationConstants.TWJobDocAddressValidationMessages.ChineseAddressMaxlength)
				{
					parent.E2_Address1Info.AddWarning(ValidationConstants.TWJobDocAddressValidationMessages.MaxChineseAddressCharactersLength(ValidationConstants.TWJobDocAddressValidationMessages.ChineseAddressMaxlength));
				}
			}
		}

		public static string ManufacturerLocalAddressResString => Res.GetString("F46B67A1-0C02-4597-8956-100259EAC5E7", "Manufacturer Local Address");

		protected override void CheckE2_ValidationStatus()
		{
		}

		protected override void CheckE2_Phone()
		{
			base.CheckE2_Phone();
			(Parent.Requirement as TWJobDocAddressRequirement)?.ValidatePhone?.Invoke(this);
		}

		protected override void CheckE2_CompanyName()
		{
			base.CheckE2_CompanyName();
			if (!IsAddressOverridenValidationEnabled)
			{
				Parent.Requirement?.ValidateCompanyName?.Invoke(this);
			}
		}

		protected override void CheckE2_Fax()
		{
			base.CheckE2_Fax();
			(Parent.Requirement as TWJobDocAddressRequirement)?.ValidateFax?.Invoke(this);
		}

		public void ValidateState()
		{
			var parent = Parent;
			if (parent.E2_AddressOverride && parent.Country == null && !parent.E2_State.IsEmpty)
			{
				parent.E2_StateInfo.AddWarning(Res.GetString("4A9c1329-742d-4d62-b7d4-e7b11af8b003", "This state code needs to be followed by valid country code."));
			}
		}
	}
}
