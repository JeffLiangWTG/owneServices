using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class AccAccountFeeValidationHelper
	{
		public static void ValidateAAF_AG_GLAccount(ZPropertyInfo propertyInfo)
		{
			TypeValidation.CheckValidGuid(propertyInfo);
			MandatoryValidation.CheckEntered(propertyInfo);
		}

		public static void ValidateAAF_FeeAmount(ZPropertyInfo propertyInfo, ZDecimal amount, ZString rule)
		{
			TypeValidation.CheckValidMoney(propertyInfo, 19, 4);

			if (!propertyInfo.HasErrors())
			{
				if (amount <= 0 && rule != AccAccountFee.AccountFeeCalculationRuleType.DoNotChargeAccountFee)
				{
					propertyInfo.AddError(Res.GetString("fe9684db-a748-4be1-aafa-46f0d7444399", "Account Fee Amount must be a positive number"));
				}
				if (amount != 0 && rule == AccAccountFee.AccountFeeCalculationRuleType.DoNotChargeAccountFee)
				{
					propertyInfo.AddError(Res.GetString("b1ad706a-1f63-4a85-ab87-3f4659745ec1", "As no Account Fee is charged, amount should be zero"));
				}
			}
		}

		public static void ValidateAAF_Rule(ZPropertyInfo propertyInfo, ICodeDescriptionPairList list)
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(propertyInfo);
			MandatoryValidation.CheckEntered(propertyInfo);
			ListValidation.ErrorIfInvalidCode(propertyInfo, list);
		}

		public static void ValidateAAF_RX_NKFeeCurrency(ZPropertyInfo propertyInfo, RefCurrencyCollection list)
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(propertyInfo);
			MandatoryValidation.CheckEntered(propertyInfo);
			ListValidation.ErrorIfInvalidCode(propertyInfo, list);
		}
	}
}
