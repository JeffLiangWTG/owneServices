using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class LevelAuthorizationSecurityHelperTest<LevelAuthorisationSettingsRegistryItemType, LevelAuthorisationSettingsCollectionType, LevelAuthorisationRequirementType> : TestCaseWithFactory
			where LevelAuthorisationSettingsRegistryItemType : StronglyTypedRegistryItem<LevelAuthorisationSettingsCollectionType>
			where LevelAuthorisationSettingsCollectionType : AmountBasedAuthorisationRequirementCollection
			where LevelAuthorisationRequirementType : AmountBasedMultiLevelAuthorisationRequirement
	{
		#region Overriden

		protected override void SetUp()
		{
			base.SetUp();
			SetUpRegistryForTest();
		}

		#endregion

		public abstract void TestGetSecurityCheckPoint();
		public abstract LevelAuthorisationSettingsRegistryItemType GetRegistryItem();
		public abstract LevelAuthorisationSettingsCollectionType PrepareAuthorisationSettingsCollection();

		void SetUpRegistryForTest()
		{
			var registry = GetRegistryItem();
			if (registry != null)
			{
				registry.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PrepareAuthorisationSettingsCollection());
			}
		}

		protected AmountBasedMultiLevelAuthorisationRequirement GetNewAuthorisationSetting(LevelAuthorisationSettingsCollectionType collection,
			ZString range, ZInt amount, ZString requirement)
		{
			var newSetting = collection.AddNew();
			newSetting.Amount = (ZDecimal)amount;
			newSetting.AuthorisationRequirement = requirement;
			newSetting.Range = range;

			return newSetting;
		}
	}
}
