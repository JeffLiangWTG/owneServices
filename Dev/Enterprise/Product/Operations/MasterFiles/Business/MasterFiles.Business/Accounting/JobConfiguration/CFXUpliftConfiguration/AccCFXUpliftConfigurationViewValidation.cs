//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccCFXUpliftConfigurationViewValidation
//
//    This class should be used for overriding validation in AutoAccCFXUpliftConfigurationViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	using System.Linq;
	using CargoWise.EntityFramework;

	public class AccCFXUpliftConfigurationViewValidation : AutoAccCFXUpliftConfigurationViewValidation
	{
		public AccCFXUpliftConfigurationViewValidation(AutoAccCFXUpliftConfigurationView parent) : base(parent)
		{
		}

		protected override void CheckJCF_RX_NKCurrencyIsNotEmpty()
		{
		}

		protected override void CheckJCF_JobType()
		{
			base.CheckJCF_JobType();
			CheckForClash();
		}

		protected override void CheckJCF_RX_NKCurrency()
		{
			base.CheckJCF_RX_NKCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.JCF_RX_NKCurrencyInfo);
			CheckForClash();
			CheckItIsNotLocal();
			CheckItIsCFXAllowed();
		}

		protected override void CheckJCF_RN_NKOriginCountry()
		{
			base.CheckJCF_RN_NKOriginCountry();
			ListValidation.ErrorIfInvalidCode(Parent.JCF_RN_NKOriginCountryInfo);
			CheckForClash();
		}

		protected override void CheckJCF_RN_NKDestinationCountry()
		{
			base.CheckJCF_RN_NKDestinationCountry();
			ListValidation.ErrorIfInvalidCode(Parent.JCF_RN_NKDestinationCountryInfo);
			CheckForClash();
		}

		protected override void CheckJCF_ServiceDirection()
		{
			base.CheckJCF_ServiceDirection();
			MandatoryValidation.CheckEntered(Parent.JCF_ServiceDirectionInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JCF_ServiceDirectionInfo);
			CheckForClash();
		}

		protected override void CheckJCF_TransportMode()
		{
			base.CheckJCF_TransportMode();
			MandatoryValidation.CheckEntered(Parent.JCF_TransportModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JCF_TransportModeInfo);
			CheckForClash();
		}

		protected override void CheckJCF_StartDate()
		{
			base.CheckJCF_StartDate();
			if (Parent.JCF_ExpiryDate.IsValid)
			{
				MandatoryValidation.CheckEntered(Parent.JCF_StartDateInfo);
			}
			CheckForClash();
			ValidateJCF_ExpiryDate();
		}

		protected override void CheckJCF_ExpiryDate()
		{
			base.CheckJCF_ExpiryDate();
			if (Parent.JCF_StartDate.IsValid)
			{
				MandatoryValidation.CheckEntered(Parent.JCF_ExpiryDateInfo);
				if (Parent.JCF_ExpiryDate.IsValid && Parent.JCF_ExpiryDate < Parent.JCF_StartDate)
				{
					Parent.JCF_ExpiryDateInfo.AddError(ExpiryDateBeforeStartDateErrorString);
				}
			}
			CheckForClash();
			ValidateJCF_StartDate();
		}

		protected override void CheckJCF_CFXMinimumIsNotEmpty()
		{
		}

		protected override void CheckJCF_CFXMinimumIsValidMoney()
		{
			MandatoryValidation.CheckNotNegative(Parent.JCF_CFXMinimumInfo, Parent.JCF_CFXMinimumInfo.HumanReadableName);
			CompareValidation.CheckLessThanOrEqualTo(Parent.JCF_CFXMinimumInfo, 922337203685477.5807m);
		}

		protected override void CheckJCF_CFXPercentageIsNotEmpty()
		{
		}

		protected override void CheckJCF_CFXPercentageIsValidZDecimal()
		{
			MandatoryValidation.CheckNotNegative(Parent.JCF_CFXPercentageInfo, Parent.JCF_CFXPercentageInfo.HumanReadableName);
			CompareValidation.CheckLessThanOrEqualTo(Parent.JCF_CFXPercentageInfo, 100.000m);
		}

		void CheckForClash()
		{
			Parent.RemoveRowError(IsDuplicateErrorString);
			Parent.RemoveRowError(OverlappingDatesErrorString);

			var parentCollection = (AccCFXUpliftConfigurationCollection)((IBusinessObjectInternals)Parent).ParentCollections.FirstOrDefault(pc => pc is AccCFXUpliftConfigurationCollection);
			if (parentCollection == null)
			{
				return;
			}

			var config = (AccCFXUpliftConfiguration)Parent;
			var otherConfigurations = parentCollection.Cast<AccCFXUpliftConfiguration>().Where(c => c != config).ToList();
			if (otherConfigurations.Any(config.IsDuplicateOf))
			{
				Parent.AddRowError(IsDuplicateErrorString);
			}
			else if (otherConfigurations.Any(config.OverlapsWith))
			{
				Parent.AddRowError(OverlappingDatesErrorString);
			}
		}

		void CheckItIsNotLocal()
		{
			if (Parent.JCF_RX_NKCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
			{
				var isLocalCurrencyErrorString = Res.GetString("ea3238bf-0ce3-4023-a771-01233388cdab", "This list is for foreign currency only. Please remove local currency from the list.");
				Parent.JCF_RX_NKCurrencyInfo.AddError(isLocalCurrencyErrorString);
			}
		}

		void CheckItIsCFXAllowed()
		{
			if (Parent.Currency == null)
			{
				return;
			}

			if (Parent.Currency.RX_IsExcludedCFXCalculation)
			{
				var isExcludedFromCFXWarningString = Res.GetString("467cc876-cef2-40eb-88dd-7b67b954c5d1", "This currency is currently excluded from CFX Calculations and no uplift applies. Please check the currency configuration in Maintain > Reference Files > Currencies.");
				Parent.JCF_RX_NKCurrencyInfo.AddWarning(isExcludedFromCFXWarningString);
			}
		}

		internal static string IsDuplicateErrorString => Res.GetString("5fceff68-0a84-419b-aa75-195932d13b9d", "At least one more record already sets CFX Uplift for the same Job parameters.");
		internal static string OverlappingDatesErrorString => Res.GetString("96f6a423-6dda-4dd0-a9ad-0991d4975c37", "This overlaps with an existing configuration with the same job parameters.");
		internal static string ExpiryDateBeforeStartDateErrorString => Res.GetString("d5c5fed2-df9f-4d7c-9ae0-2fc1b24413dd", "The expiry date cannot be earlier than the start date.");
	}
}
