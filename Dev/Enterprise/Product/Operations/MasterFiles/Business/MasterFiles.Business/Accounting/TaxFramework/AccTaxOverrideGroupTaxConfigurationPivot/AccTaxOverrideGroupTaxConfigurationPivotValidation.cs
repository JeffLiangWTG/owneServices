//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccTaxOverrideGroupTaxConfigurationPivotValidation
//
//    This class should be used for overriding validation in AutoAccTaxOverrideGroupTaxConfigurationPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccTaxOverrideGroupTaxConfigurationPivotValidation : AutoAccTaxOverrideGroupTaxConfigurationPivotValidation
	{
		public AccTaxOverrideGroupTaxConfigurationPivotValidation(AutoAccTaxOverrideGroupTaxConfigurationPivot parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			Validate_Rate();
		}

		new AccTaxOverrideGroupTaxConfigurationPivot Parent => (AccTaxOverrideGroupTaxConfigurationPivot)base.Parent;

		public void Validate_Rate() => ValidateCalculatedProperty(Parent.RateInfo);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Method is used via reflection (see GetValidationMethod() in ZValidation.cs)")]
		void CheckRate()
		{
			if (Parent.Rate > 100)
			{
				Parent.RateInfo.AddError(Res.GetString("13c97182-c2c2-4b7f-91ea-4f9d8dff3640", "Rate must be less than 100%. Please correct the Numerator and Denominator entered."));
			}
		}
		protected override void CheckAXP_ETC_TaxConfiguration()
		{
			base.CheckAXP_ETC_TaxConfiguration();

			MandatoryValidation.CheckEntered(Parent.AXP_ETC_TaxConfigurationInfo);
			ListValidation.ErrorIfInvalidPK(Parent.AXP_ETC_TaxConfigurationInfo);
		}

		protected override void CheckAXP_RateNumerator()
		{
			base.CheckAXP_RateNumerator();

			CompareValidation.CheckGreaterThanOrEqualTo(Parent.AXP_RateNumeratorInfo, 0);
		}

		protected override void CheckAXP_RateDenominator()
		{
			base.CheckAXP_RateDenominator();

			CompareValidation.CheckGreaterThanOrEqualTo(Parent.AXP_RateDenominatorInfo, 1);
		}

		protected override void CheckAXP_TaxAuthorityServiceCode()
		{
			base.CheckAXP_TaxAuthorityServiceCode();

			if (!Parent.AXP_TaxAuthorityServiceCodeDescription.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.AXP_TaxAuthorityServiceCodeInfo);
			}

			if (!Parent.AXP_TaxAuthorityServiceCodeInfo.HasErrors())
			{
				if (Parent.TaxConfiguration != null && Parent.TaxConfiguration.TaxSystem != null)
				{
					var serviceCodeSeparatorAndWarning = Parent.TaxConfiguration.TaxSystem.GetServiceCodeSeparatorAndWarning(Parent.TaxConfiguration.Company.GC_RN_NKCountryCode);
					if (serviceCodeSeparatorAndWarning.HasValue && Parent.AXP_TaxAuthorityServiceCode.Contains(serviceCodeSeparatorAndWarning.Value.Separator))
					{
						Parent.AXP_TaxAuthorityServiceCodeInfo.AddWarning(serviceCodeSeparatorAndWarning.Value.WarningMessage);
					}
				}
			}
		}

		protected override void CheckAXP_TaxAuthorityServiceCodeDescription()
		{
			base.CheckAXP_TaxAuthorityServiceCodeDescription();

			if (!Parent.AXP_TaxAuthorityServiceCode.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.AXP_TaxAuthorityServiceCodeDescriptionInfo);
			}

			if (!Parent.AXP_TaxAuthorityServiceCodeDescription.IsEmpty)
			{
				ZQuery filterQuery = new ZDBOnlyQuery(typeof(AccTaxOverrideGroupTaxConfigurationPivot))
					.AddToFilter(AccTaxOverrideGroupTaxConfigurationPivotSchema.AXP_ETC_TaxConfiguration, Parent.AXP_ETC_TaxConfiguration)
					.AddToFilter(AccTaxOverrideGroupTaxConfigurationPivotSchema.AXP_TaxAuthorityServiceCode, Parent.AXP_TaxAuthorityServiceCode)
					.AddToFilter(AccTaxOverrideGroupTaxConfigurationPivotSchema.AXP_TaxAuthorityServiceCodeDescription, SQLComparisonOperator.NotEqual, Parent.AXP_TaxAuthorityServiceCodeDescription)
					.AddToFilter(AccTaxOverrideGroupTaxConfigurationPivotSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				var matchedRecords = Parent.Factory.Load(typeof(AccTaxOverrideGroupTaxConfigurationPivot), filterQuery);
				string serviceCodeDescriptionWarning = Res.GetString("318c8e83-c32e-4d91-b282-95a2b87173ba", "Please note: The Service Code Description recorded here differs to the description used on other Tax Configuration Override Groups with the same Service Code");

				if (matchedRecords.Length > 0)
				{
					if (!Parent.AXP_TaxAuthorityServiceCodeDescriptionInfo.HasWarning(serviceCodeDescriptionWarning))
					{
						Parent.AXP_TaxAuthorityServiceCodeDescriptionInfo.AddWarning(serviceCodeDescriptionWarning);
					}
				}
			}
		}

		protected override void CheckAXP_AT_TaxID()
		{
			base.CheckAXP_AT_TaxID();

			var pivots = Parent.TaxOverrideGroup?.TaxOverrideGroupTaxConfigurationPivots;
			if (pivots?.Count > 1)
			{
				MandatoryValidation.CheckEntered(Parent.AXP_AT_TaxIDInfo);
			}
			else
			{
				var taxOverride = (AccChargeTaxOverride)Parent.TaxOverrideGroup?.TaxOverrides.FirstOrDefault();
				if (taxOverride != null && taxOverride.AO_AT.IsEmpty)
				{
					MandatoryValidation.CheckEntered(Parent.AXP_AT_TaxIDInfo);
				}
			}
			ListValidation.ErrorIfInvalidPK(Parent.AXP_AT_TaxIDInfo);
		}

		protected override void CheckAXP_A9_DefaultVATClass()
		{
			base.CheckAXP_A9_DefaultVATClass();
			ListValidation.ErrorIfInvalidPK(Parent.AXP_A9_DefaultVATClassInfo);

			if (!Parent.AXP_A9_DefaultVATClass.IsEmpty && Parent.AXP_AT_TaxID.IsEmpty)
			{
				Parent.AXP_A9_DefaultVATClassInfo.AddError(Res.GetString("3145721c-a179-415b-9b06-b6ea15b7434c", "You must have a Tax ID before you can choose a Tax Message"));
			}
		}
	}
}
