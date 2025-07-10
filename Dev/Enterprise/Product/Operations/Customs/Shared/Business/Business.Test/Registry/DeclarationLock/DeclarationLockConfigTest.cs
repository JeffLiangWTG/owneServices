using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(DeclarationLockConfig))]
	sealed class DeclarationLockConfigTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestDeclarationTypeDescription()
		{
			var config = GetDeclarationLockConfig();

			config.DeclarationType = string.Empty;
			AssertEquals(string.Empty, config.DeclarationTypeDescription);

			config.DeclarationType = "XXX";
			AssertEquals(string.Empty, config.DeclarationTypeDescription);

			config.DeclarationType = "IMP";
			AssertEquals("Import", config.DeclarationTypeDescription);
		}

		#region Validation

		public void TestValidateDeclarationType()
		{
			var messageOfNotFound = "Please enter a value.";
			var messageOfInvalidSelection = "Enter a valid selection.";

			var config = GetDeclarationLockConfig();

			config.DeclarationType = string.Empty;
			config.ValidateDeclarationType();

			AssertHasError(config.DeclarationTypeInfo, messageOfNotFound);
			AssertNoError(config.DeclarationTypeInfo, messageOfInvalidSelection);

			config.DeclarationType = "XXX";

			AssertNoError(config.DeclarationTypeInfo, messageOfNotFound);
			AssertHasError(config.DeclarationTypeInfo, messageOfInvalidSelection);

			config.DeclarationType = "IMP";

			AssertNoError(config.DeclarationTypeInfo, messageOfNotFound);
			AssertNoError(config.DeclarationTypeInfo, messageOfInvalidSelection);
		}

		public void TestValidateDeclarationType_UniqueCheck()
		{
			var message = "There is an existing item with IMP.";

			var config1 = GetDeclarationLockConfig();

			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);

			var collection = new DeclarationLockConfigCollection(fallbackLevel, Factory);
			collection.Add(config1);

			config1.DeclarationType = "IMP";
			AssertNoError(config1.DeclarationTypeInfo, message);

			var config2 = GetDeclarationLockConfig();
			collection.Add(config2);

			void AssertHasOrNoError(string declarationType1, string declarationType2, bool hasError)
			{
				config1.DeclarationType = declarationType1;
				config2.DeclarationType = declarationType2;

				config1.ValidateDeclarationType();
				config2.ValidateDeclarationType();

				if (hasError)
				{
					AssertHasError(config1.DeclarationTypeInfo, message);
					AssertHasError(config2.DeclarationTypeInfo, message);
				}
				else
				{
					AssertNoError(config1.DeclarationTypeInfo, message);
					AssertNoError(config2.DeclarationTypeInfo, message);
				}
			}

			AssertHasOrNoError(string.Empty, string.Empty, false);
			AssertHasOrNoError("XXX", "XXX", false);
			AssertHasOrNoError("IMP", "IMP", true);
			AssertHasOrNoError("IMP", "EXP", false);
		}

		public void TestValidateDeclarationType_HasAtLeastOneTabInfo()
		{
			var message = @"Should have at least one row on ""Tabs that become locked""";

			var config = GetDeclarationLockConfig();
			config.TabInfos.RemoveAndDeleteAll();

			config.RunPreSaveValidation();
			AssertHasError(config.DeclarationTypeInfo, message);

			config.TabInfos.AddNew();
			config.RunPreSaveValidation();
			AssertNoError(config.DeclarationTypeInfo, message);
		}

		public void TestValidateLockMode()
		{
			var messageOfNotFound = "Please enter a value.";
			var messageOfInvalidSelection = "Enter a valid selection.";

			var lockConfig = GetDeclarationLockConfig();

			lockConfig.LockMode = "@#&";
			AssertHasError(lockConfig.LockModeInfo, messageOfInvalidSelection);

			lockConfig.LockMode = Core.Constants.Customs.DeclarationLockModes.Codes.All;
			AssertNoError(lockConfig.LockModeInfo, messageOfInvalidSelection);

			lockConfig.LockMode = ZString.Empty;
			AssertHasError(lockConfig.LockModeInfo, messageOfNotFound);
		}

		DeclarationLockConfig GetDeclarationLockConfig()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var config = new DeclarationLockConfig(fallbackLevel, Factory);
			config.TabInfos.AddNew();

			return config;
		}

		#endregion

		#region Implementation

		protected override void CheckAllPropertiesAreEqual(RegistryBusinessObjectTemplate originalBusinessObject, RegistryBusinessObjectTemplate newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);

			var originalConfig = (DeclarationLockConfig)originalBusinessObject;
			var newConfig = (DeclarationLockConfig)newBusinessObject;

			AssertEquals("newConfig.Events.Count", originalConfig.EventInfos.Count, newConfig.EventInfos.Count);
			AssertEquals("newConfig.Events.LockConfig", newConfig, newConfig.EventInfos.LockConfig);

			AssertEquals("newConfig.Tabs.Count", originalConfig.TabInfos.Count, newConfig.TabInfos.Count);
			AssertEquals("newConfig.Tabs.LockConfig", newConfig, newConfig.TabInfos.LockConfig);

			if (!isClone)
			{
				AssertEquals("newConfig.CurrentFallbackLevel", originalConfig.CurrentFallbackLevel, newConfig.CurrentFallbackLevel);
			}

			for (var i = 0; i < originalConfig.EventInfos.Count; ++i)
			{
				CheckAllPropertiesInZPropertyInfoHashAreEqual(originalConfig.EventInfos[i], newConfig.EventInfos[i]);
			}

			for (var i = 0; i < originalConfig.TabInfos.Count; ++i)
			{
				CheckAllPropertiesInZPropertyInfoHashAreEqual(originalConfig.TabInfos[i], newConfig.TabInfos[i]);
			}
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			var config = new DeclarationLockConfig(null, Factory) { DeclarationType = "IMP" };

			var eventLockInfo1 = config.EventInfos.AddNew();
			var eventLockInfo2 = config.EventInfos.AddNew();

			eventLockInfo1.EventType = "ARV";
			eventLockInfo1.EventReference = "REF=XXX";
			eventLockInfo1.EventSource = "DEC";

			eventLockInfo2.EventType = "DEP";
			eventLockInfo2.EventReference = "ORG=NJG";
			eventLockInfo2.EventSource = "CEN";

			var tabLockInfo1 = config.TabInfos.AddNew();
			var tabLockInfo2 = config.TabInfos.AddNew();

			tabLockInfo1.TabPage = "DEC";
			tabLockInfo2.TabPage = "SVC";

			return config;
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		#endregion
	}
}
