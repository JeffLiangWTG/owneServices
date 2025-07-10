using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class AccTaxOverrideGroupTaxConfigurationPivot : AutoAccTaxOverrideGroupTaxConfigurationPivot
	{
		public AccTaxOverrideGroupTaxConfigurationPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Tax Rate

		[DecimalPlaces(9)]
		public ZDecimal Rate => AXP_RateDenominator != 0 ? (new ZDecimal(AXP_RateNumerator) / AXP_RateDenominator) : 0;

		public ZPropertyInfo RateInfo => GetZPropertyInfo(nameof(Rate));

		#endregion

		public ZString Ledger => TaxConfiguration?.ETC_Ledger ?? ZString.Empty;

		public ZString TaxConfigurationDescription => TaxConfiguration?.ETC_Description ?? ZString.Empty;

		public ZString TaxConfigurationBranch => TaxConfiguration?.ParentBranch?.GB_Code ?? ZString.Empty;

		public override ZString AXP_TaxAuthorityServiceCode
		{
			get => base.AXP_TaxAuthorityServiceCode;
			set
			{
				base.AXP_TaxAuthorityServiceCode = value;
				Validation.ValidateAXP_TaxAuthorityServiceCodeDescription();
			}
		}

		public override ZString AXP_TaxAuthorityServiceCodeDescription
		{
			get => base.AXP_TaxAuthorityServiceCodeDescription;
			set
			{
				base.AXP_TaxAuthorityServiceCodeDescription = value;
				Validation.ValidateAXP_TaxAuthorityServiceCode();
			}
		}

		public override ZGuid AXP_AT_TaxID
		{
			get => base.AXP_AT_TaxID;
			set
			{
				base.AXP_AT_TaxID = value;
				Validation.ValidateAXP_A9_DefaultVATClass();
			}
		}

		public override ZGuid AXP_ETC_TaxConfiguration
		{
			get => base.AXP_ETC_TaxConfiguration;
			set
			{
				base.AXP_ETC_TaxConfiguration = value;
				Validation.ValidateAXP_AT_TaxID();
			}
		}

		public override ZInt AXP_RateDenominator
		{
			get => base.AXP_RateDenominator;
			set
			{
				bool hasChanged = AXP_RateDenominator != value;
				base.AXP_RateDenominator = value;

				if (hasChanged)
				{
					Validation.Validate_Rate();
				}
			}
		}

		public override ZInt AXP_RateNumerator
		{
			get => base.AXP_RateNumerator;
			set
			{
				bool hasChanged = AXP_RateNumerator != value;
				base.AXP_RateNumerator = value;

				if (hasChanged)
				{
					Validation.Validate_Rate();
				}
			}
		}

		public ZString TaxRateSource => TaxID?.AT_RateSource ?? ZString.Empty;
	}
}
