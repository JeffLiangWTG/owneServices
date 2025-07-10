//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccOrgTaxConfigurationValidation
//
//    This class should be used for overriding validation in AutoAccOrgTaxConfigurationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Types;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.MasterFiles.Business
{
	public class AccOrgTaxConfigurationValidation : AutoAccOrgTaxConfigurationValidation
	{
		public AccOrgTaxConfigurationValidation(AutoAccOrgTaxConfiguration parent) : base(parent)
		{
		}

		protected override void CheckOTC_ETC()
		{
			base.CheckOTC_ETC();

			if (Parent.IsInDatabase && Parent.OTC_ETCInfo.HasChanges)
			{
				Parent.OTC_ETCInfo.AddError(Res.GetString("f48d597e-9f14-4fd3-9fd7-90b3fb471de7", "This value cannot be changed. Changing a Tax Code already saved against an organization is not permitted."));
			}

			if (Parent.TaxRates.Count > 0 && !Parent.lastValidOTC_ETC.IsEmpty && Parent.lastValidOTC_ETC != Parent.OTC_ETC)
			{
				Parent.OTC_ETCInfo.AddError(Res.GetString("3D050AA0-27FC-4C6D-9725-E3FDFDD5D907", "Value '{0}' can't be changed when Tax Rates are entered.", Parent.Factory.Load<AccTaxConfiguration>(Parent.lastValidOTC_ETC)?.ETC_Code ?? ZString.Empty));
			}
		}

		protected override void CheckOTC_RecoverTax()
		{
			base.CheckOTC_RecoverTax();

			if (Parent.OTC_RecoverTax && Parent.OTC_IsActive)
			{
				var taxConfig = Parent.TaxConfiguration;
				var invalidRecoveryMethod = TaxRecoveryMethods.NoRecovery.Code;
				if (taxConfig != null && taxConfig.ETC_RecoveryMethod == invalidRecoveryMethod)
				{
					Parent.OTC_RecoverTaxInfo.AddWarning(Res.GetString("D6D7DBD6-C75B-486B-9558-527A33F17183", "Tax Recovery will not be activated for Tax Configurations with recovery method '{0}'.", invalidRecoveryMethod));
				}
			}
		}

		protected override void CheckOTC_IsThresholdUsed()
		{
			if (Parent.OTC_IsThresholdUsed && Parent.OTC_IsActive)
			{
				var taxConfig = Parent.TaxConfiguration;
				var invalidThresholdMethod = ETC_ThresholdMethods.NoThreshold.Code;
				if (taxConfig != null && taxConfig.ETC_ThresholdMethod == invalidThresholdMethod)
				{
					Parent.OTC_IsThresholdUsedInfo.AddWarning(Res.GetString("0BFC46CC-DE9A-4289-A728-2ADF9DA73BEA", "Threshold will not be activated for Tax Configurations with threshold method '{0}'.", invalidThresholdMethod));
				}
			}
		}

		protected new AccOrgTaxConfiguration Parent
		{
			get { return (AccOrgTaxConfiguration)base.Parent; }
		}
	}
}
