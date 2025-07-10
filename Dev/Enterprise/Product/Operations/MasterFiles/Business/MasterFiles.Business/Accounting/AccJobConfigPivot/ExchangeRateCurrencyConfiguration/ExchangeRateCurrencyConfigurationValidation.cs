using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class ExchangeRateCurrencyConfigurationValidation : AccJobConfigPivotValidation
	{
		public ExchangeRateCurrencyConfigurationValidation(ExchangeRateCurrencyConfiguration parent) : base(parent)
		{
		}

		protected override void CheckJCT_Code()
		{
			base.CheckJCT_Code();
			ListValidation.ErrorIfInvalidCode(Parent.JCT_CodeInfo);
			CheckDuplicate();

			if (!((ExchangeRateCurrencyConfiguration)Parent).IsExRateConfigCurrencyTypeAll)
			{
				MandatoryValidation.CheckEntered(Parent.JCT_CodeInfo);
			}

			if (!isExpiryDateValidated)
			{
				ValidateJCT_ExpiryDate();
			}

			if (!isStartDateValidated)
			{
				ValidateJCT_StartDate();
			}
		}

		void CheckDuplicate()
		{
			var exRateConfig = ((ExchangeRateCurrencyConfiguration)Parent).AccExRateConfiguration;

			if (exRateConfig == null)
			{
				return;
			}

			var exRateConfigCollection = ((IBusinessObjectInternals)exRateConfig).ParentCollections.FirstOrDefault(pc => pc is AccExchangeRateConfigurationCollection) as AccExchangeRateConfigurationCollection;

			var parentCollection = exRateConfigCollection == null
				? exRateConfig.CurrencyConfigurations
					.Cast<ExchangeRateCurrencyConfiguration>()
				: exRateConfigCollection
					.Where(x => x.PK == exRateConfig.PK || x.IsTheSameConfigWithCURCurrencyType(exRateConfig))
					.SelectMany(x => x.CurrencyConfigurations.Cast<ExchangeRateCurrencyConfiguration>())
					.ToArray();

			if (parentCollection.Any(x => ((ExchangeRateCurrencyConfiguration)Parent).IsDuplicateOf(x)))
			{
				Parent.JCT_CodeInfo.AddError(AccountingMasterFilesConstants.ValidationErrorMessages.DuplicateCurrencyCodeForJobBillingExRateConfig);
			}
		}

		protected override void CheckJCT_ExRateType()
		{
			base.CheckJCT_ExRateType();
			MandatoryValidation.CheckEntered(Parent.JCT_ExRateTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JCT_ExRateTypeInfo);
		}

		bool isStartDateValidated;
		bool isExpiryDateValidated;

		protected override void CheckJCT_StartDate()
		{
			base.CheckJCT_StartDate();

			isStartDateValidated = true;

			ValidateDateRange(Parent.JCT_StartDateInfo, Parent.JCT_StartDate, Parent.JCT_ExpiryDate);

			if (!isExpiryDateValidated)
			{
				ValidateJCT_ExpiryDate();
			}

			ValidateJCT_Code();
		}

		protected override void CheckJCT_ExpiryDate()
		{
			base.CheckJCT_ExpiryDate();

			isExpiryDateValidated = true;

			ValidateDateRange(Parent.JCT_ExpiryDateInfo, Parent.JCT_ExpiryDate, Parent.JCT_StartDate);

			if (!isStartDateValidated)
			{
				ValidateJCT_StartDate();
			}

			ValidateJCT_Code();
		}

		void ValidateDateRange(ZPropertyInfo validatedDateInfo, ZDate validatedDateValue, ZDate refDateValue)
		{
			if (refDateValue.IsEmpty)
			{
				return;
			}

			MandatoryValidation.CheckEntered(validatedDateInfo);

			if (!validatedDateValue.IsEmpty)
			{
				if (Parent.JCT_StartDate > Parent.JCT_ExpiryDate)
				{
					validatedDateInfo.AddError(Res.GetString("8c18fa66-ab46-4a5a-80df-cb5f31a36be5", "Expiry Date must be after Start Date."));
				}

				if (!validatedDateInfo.HasErrors())
				{
					var currencyConfigs = ((ExchangeRateCurrencyConfiguration)Parent).AccExRateConfiguration.CurrencyConfigurations;

					foreach (ExchangeRateCurrencyConfiguration config in currencyConfigs)
					{
						if (config != Parent && !config.IsDeleted && config.JCT_Code == Parent.JCT_Code)
						{
							var isOverlappingDate = Parent.JCT_ExpiryDate >= config.JCT_StartDate && Parent.JCT_StartDate <= config.JCT_ExpiryDate;

							if (isOverlappingDate)
							{
								validatedDateInfo.AddError(Res.GetString("1d7b3a89-ea58-4d52-b8e5-fc9b91177b96", "This overlaps with an existing configuration with the same job parameters."));
								Parent.AddRowError(Res.GetString("1d7b3a89-ea58-4d52-b8e5-fc9c91177b96", "This overlaps with an existing configuration with the same job parameters."));
								break;
							}
						}
					}
				}
			}
		}
	}
}
