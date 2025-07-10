using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class CreditTemporaryIncreaseAuthorisationSettingsHelper
	{
		public static CreditTemporaryIncreaseAuthorisationSettings CreateCreditTemporaryIncreaseRequirement(this CreditTemporaryIncreaseAuthorisationSettingsCollection collection, ZDecimal amount, ZDecimal percentage, ZString range, ZString auth, ZInt daysToExpiry)
		{
			var settings = collection.AddNew();
			settings.Amount = amount;
			settings.Percentage = percentage;
			settings.Range = range;
			settings.AuthorisationRequirement = auth;
			settings.DaysToExpiry = daysToExpiry;
			return settings;
		}

		public static void QuickSetupTemporaryCreditLimitOnOrg(OrgCompanyData data, ZDecimal creditLimit, ZDecimal temporaryIncrease)
		{
			data.OB_ARCreditLimit = creditLimit;

			if (temporaryIncrease > 0)
			{
				var registry = ObjectFactory.Get<IAccounting>().Registry.CreditLimitCheckTemporaryCreditLimitIncreaseThreshold as CreditTemporaryIncreaseAuthorisationSettingsRegistryItem;
				if (registry.Value.Count == 0)
				{
					var collection = new CreditTemporaryIncreaseAuthorisationSettingsCollection();
					collection.CreateCreditTemporaryIncreaseRequirement(20000, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired, 5);
					collection.CreateCreditTemporaryIncreaseRequirement(20000, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly, 365);
					collection.RunPreSaveValidation();
					if (collection.HasNotifications())
					{
						throw new ApplicationException("Expected no notifications on collection");
					}
					registry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
				}
				data.OB_ARTemporaryCreditLimitIncrease = temporaryIncrease;

				if (data.OB_ARTemporaryCreditLimitIncreaseExpiry.IsEmpty)
				{
					throw new ApplicationException("Expected expirty date to be set");
				}
			}
			else
			{
				data.OB_ARTemporaryCreditLimitIncrease = temporaryIncrease;
			}
		}
	}
}
