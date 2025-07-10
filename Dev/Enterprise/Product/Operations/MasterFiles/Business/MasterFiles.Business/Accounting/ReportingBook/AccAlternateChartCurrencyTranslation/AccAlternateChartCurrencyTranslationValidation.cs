using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccAlternateChartCurrencyTranslationValidation : AutoAccAlternateChartCurrencyTranslationValidation
	{
		public AccAlternateChartCurrencyTranslationValidation(AutoAccAlternateChartCurrencyTranslation parent) : base(parent)
		{
		}

		protected override void CheckART_Type()
		{
			base.CheckART_Type();

			if (Parent.ART_AGA_AlternateAccount.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.ART_TypeInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.ART_TypeInfo);

			if (!string.IsNullOrEmpty(Parent.ART_Type))
			{
				if (Parent.ART_AGA_AlternateAccount.IsValid)
				{
					Parent.ART_TypeInfo.AddError(Res.GetString("BA8E06AD-6283-4667-9FA8-A8D4E6D1CF7E", "Either 'Account Type' (i.e. P&L or BSH) or a specific 'Alternate Account' can be selected."));
				}

				var accAlternateChartCurrencyTranslations = Parent.AlternateChart.AccAlternateChartCurrencyTranslations.Cast<AccAlternateChartCurrencyTranslation>();
				if (accAlternateChartCurrencyTranslations.Count(x => x.ART_Type == Core.Constants.AccountType.BalanceSheetAccount) != 1
					|| accAlternateChartCurrencyTranslations.Count(x => x.ART_Type == Core.Constants.AccountType.ProfitAndLossAccount) != 1)
				{
					Parent.ART_TypeInfo.AddError(Res.GetString("8A5415F7-A2FC-4BE9-B690-AE036F909D12", "There must be one row each for P&L and BSH Account Type."));
				}
			}
		}

		protected override void CheckART_AGA_AlternateAccount()
		{
			base.CheckART_AGA_AlternateAccount();

			if (string.IsNullOrEmpty(Parent.ART_Type))
			{
				MandatoryValidation.CheckEntered(Parent.ART_AGA_AlternateAccountInfo);
			}

			ListValidation.ErrorIfInvalidPK(Parent.ART_AGA_AlternateAccountInfo);

			if (Parent.ART_AGA_AlternateAccount.IsValid)
			{
				if (!Parent.ART_AGA_AlternateAccountInfo.HasErrors() && !string.IsNullOrEmpty(Parent.ART_Type))
				{
					Parent.ART_AGA_AlternateAccountInfo.AddError(Res.GetString("BA8E06AD-6283-4667-9FA8-A8D4E6D1CF7E", "Either 'Account Type' (i.e. P&L or BSH) or a specific 'Alternate Account' can be selected."));
				}

				var accAlternateChartCurrencyTranslations = Parent.AlternateChart.AccAlternateChartCurrencyTranslations.Cast<AccAlternateChartCurrencyTranslation>();
				if (!Parent.ART_AGA_AlternateAccountInfo.HasErrors() && accAlternateChartCurrencyTranslations.Any(x => x.ART_AGA_AlternateAccount == Parent.ART_AGA_AlternateAccount && x.PK != Parent.PK))
				{
					Parent.ART_AGA_AlternateAccountInfo.AddError(Res.GetString("9750FCAC-5588-46B4-8391-84CDD0FB3C2A", "None, one or more rows can be added for specify Alternate Account. No duplication allowed."));
				}

				if (!Parent.ART_AGA_AlternateAccountInfo.HasErrors() && Parent.AlternateAccount.AGA_AccountType != Core.Constants.AccountType.BalanceSheetAccount && Parent.AlternateAccount.AGA_AccountType != Core.Constants.AccountType.ProfitAndLossAccount)
				{	
					Parent.ART_AGA_AlternateAccountInfo.AddError(Res.GetString("667D30FA-3739-4251-EB26-ABD0773C2A8C", "Only Alternate Account with BSH or P&L Account Type can be set in Currency Translation."));
				}
			}
		}

		protected override void CheckART_CurrencyTranslationLevel()
		{
			base.CheckART_CurrencyTranslationLevel();

			MandatoryValidation.CheckEntered(Parent.ART_CurrencyTranslationLevelInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ART_CurrencyTranslationLevelInfo);
		}

		protected override void CheckART_ExRateType()
		{
			base.CheckART_ExRateType();

			MandatoryValidation.CheckEntered(Parent.ART_ExRateTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ART_ExRateTypeInfo);
		}
	}
}
