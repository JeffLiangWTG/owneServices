using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Security;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.MasterFiles.Business
{
	public abstract class LevelAuthorizationSecurityHelper<LevelAuthorisationSettingsType, LevelAuthorisationSettingsCollectionType>
		where LevelAuthorisationSettingsType : AmountBasedMultiLevelAuthorisationRequirement
		where LevelAuthorisationSettingsCollectionType : AmountBasedAuthorisationRequirementCollection
	{
		public SecurityCheckpoint GetSecurityCheckPoint(ZDecimal localAmount)
		{
			LevelAuthorisationSettingsType authorisationSetting = null;
			if (localAmount > 0)
			{
				foreach (LevelAuthorisationSettingsType currentSetting in RegistryValue)
				{
					if (currentSetting.Range == RangeCodes.Above && localAmount > currentSetting.Amount)
					{
						authorisationSetting = currentSetting;
					}
				}

				if (authorisationSetting == null)
				{
					foreach (LevelAuthorisationSettingsType currentSetting in RegistryValue)
					{
						if (currentSetting.Range == RangeCodes.UpTo)
						{
							if (authorisationSetting == null || currentSetting.Amount < authorisationSetting.Amount)
							{
								if (currentSetting.Amount >= localAmount)
								{
									authorisationSetting = currentSetting;
								}
							}
						}
					}
				}
			}
			return GetCheckPointFromRegistrySetting(authorisationSetting);
		}

		#region Implementation

		//protected abstract PaymentThreeLevelAuthorisationSettingsCollection RegistryValue { get; }
		protected abstract LevelAuthorisationSettingsCollectionType RegistryValue { get; }

		protected abstract SecurityCheckpoint GetCheckPointFromRegistrySetting(LevelAuthorisationSettingsType setting);

		#endregion
	}
}
