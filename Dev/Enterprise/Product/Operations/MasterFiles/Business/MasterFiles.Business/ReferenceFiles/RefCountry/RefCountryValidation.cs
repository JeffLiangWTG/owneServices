using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class RefCountryValidation : AutoRefCountryValidation
	{
		public RefCountryValidation(AutoRefCountry parent) : base(parent)
		{
		}

		#region RN_Code

		protected override void CheckRN_Code()
		{
			base.CheckRN_Code();
			MandatoryValidation.CheckEntered(Parent.RN_CodeInfo);
			if (Parent.RN_Code.Length != 2)
			{
				Parent.RN_CodeInfo.AddError(Res.GetString("6fed6740-4da3-495a-8657-54af902629d5", "ISO code must be 2 characters."));
			}
		}

		#endregion

		#region RN_Desc

		protected override void CheckRN_Desc()
		{
			base.CheckRN_Desc();
			MandatoryValidation.CheckEntered(Parent.RN_DescInfo);
			if (Parent.RN_Desc.Length < 4)
			{
				Parent.RN_DescInfo.AddError(Res.GetString("8629eb87-8f27-4522-b1c5-70d883b6c52e", "Country/Region name must be at least 4 characters."));
			}

			if (Parent.RN_IsSystem && Parent.IsInDatabase && Parent.RN_Desc.ToUpper() != ((ZString)Parent.RN_DescInfo.OriginalValue).ToUpper())
			{
				Parent.RN_DescInfo.AddWarning(Res.GetString("cafc9d54-839c-4565-9325-aef83a270e8f", "Changes to the country/region name of a system defined country/region will be lost when an ISO update of the country/region list is received."));
			}

			TranslatableDataFieldAttribute.Validate(Parent.RN_DescInfo);
		}

		#endregion

		#region RN_RX_NKLocalCurrency

		protected override void CheckRN_RX_NKLocalCurrency()
		{
			base.CheckRN_RX_NKLocalCurrency();
			if (!Parent.RN_IsSystem)
			{
				MandatoryValidation.CheckEntered(Parent.RN_RX_NKLocalCurrencyInfo);
			}
		}

		#endregion

		#region RN_EconomicGrouping

		protected override void CheckRN_EconomicGrouping()
		{
			base.CheckRN_EconomicGrouping();
			ListValidation.ErrorIfInvalidCode(Parent.RN_EconomicGroupingInfo);
		}

		#endregion

		#region RN_AddressFormattingRule

		protected override void CheckRN_AddressFormattingRule()
		{
			base.CheckRN_AddressFormattingRule();
			MandatoryValidation.CheckEntered(Parent.RN_AddressFormattingRuleInfo);
			ListValidation.ErrorIfInvalidCode(Parent.RN_AddressFormattingRuleInfo);
		}

		#endregion

		#region RN_StateProvinceValidationRule

		protected override void CheckRN_StateProvinceValidationRule()
		{
			base.CheckRN_StateProvinceValidationRule();
			MandatoryValidation.CheckEntered(Parent.RN_StateProvinceValidationRuleInfo);
			ListValidation.ErrorIfInvalidCode(Parent.RN_StateProvinceValidationRuleInfo);
		}

		#endregion

		#region RN_PostcodeValidationRule

		protected override void CheckRN_PostcodeValidationRule()
		{
			base.CheckRN_PostcodeValidationRule();
			MandatoryValidation.CheckEntered(Parent.RN_PostcodeValidationRuleInfo);
			ListValidation.ErrorIfInvalidCode(Parent.RN_PostcodeValidationRuleInfo);
		}

		#endregion
	}
}
