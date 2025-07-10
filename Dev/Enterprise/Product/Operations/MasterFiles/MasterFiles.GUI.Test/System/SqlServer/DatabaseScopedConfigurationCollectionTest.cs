using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(DatabaseScopedConfigurationCollection))]
	sealed class DatabaseScopedConfigurationCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DatabaseScopedConfigurationCollection>
	{
		[ExpectNoExceptions]
		public override void TestRemoveFromRelationship()
		{
		}

		protected override DatabaseScopedConfigurationCollection GetCollectionToTest()
		{
			return new DatabaseScopedConfigurationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DatabaseScopedConfiguration(1, new PresetDatabaseScopedConfiguration("2", PresetDatabaseScopedConfiguration.CommonSyntaxValues.OnOffOnly, PresetDatabaseScopedConfiguration.OnOffOnlyDatabaseValueToSyntaxValueConverter, null));
		}

		sealed class CanBeSavedPotentially : TestCase
		{
			public void TestTrue()
			{
				// Arrange
				var collection = new DatabaseScopedConfigurationCollection();
				collection.AddRange(config1, config2);

				// Act
				DatabaseScopedConfigurationExtensions.MakeChangeToProposedValueAsUIDoes(config2, "ON");

				// Assert
				AssertEquals(true, collection.CanBeSavedPotentially);
			}

			public void TestFalseIfEmpty()
			{
				// Arrange
				var collection = new DatabaseScopedConfigurationCollection();
				collection.AddRange(config1, config2);

				// Act
				config1.HasChanges = true;
				config2.HasChanges = true;

				// Assert
				AssertEquals(false, collection.CanBeSavedPotentially);
			}

			public void TestFalseIfNoChange()
			{
				// Arrange
				var collection = new DatabaseScopedConfigurationCollection();
				collection.AddRange(config1, config2);

				config1.ProposedValue = PresetDatabaseScopedConfiguration.CommonSyntaxValues.On;
				config2.ProposedValue = PresetDatabaseScopedConfiguration.CommonSyntaxValues.Off;

				// Act
				// Assert
				AssertEquals(false, collection.CanBeSavedPotentially);
			}

			public void TestFalseIfError()
			{
				// Arrange
				var collection = new DatabaseScopedConfigurationCollection();
				collection.AddRange(config1, config2);

				// Act
				DatabaseScopedConfigurationExtensions.MakeChangeToProposedValueAsUIDoes(config1, PresetDatabaseScopedConfiguration.CommonSyntaxValues.On);
				DatabaseScopedConfigurationExtensions.MakeChangeToProposedValueAsUIDoes(config2, "2'");

				// Assert
				AssertEquals(false, collection.CanBeSavedPotentially);
			}

			DatabaseScopedConfiguration config1;
			DatabaseScopedConfiguration config2;
			protected override void SetUp()
			{
				base.SetUp();
				config1 = new DatabaseScopedConfiguration(1, new PresetDatabaseScopedConfiguration("2", PresetDatabaseScopedConfiguration.CommonSyntaxValues.OnOffOnly, PresetDatabaseScopedConfiguration.OnOffOnlyDatabaseValueToSyntaxValueConverter, null));
				config2 = new DatabaseScopedConfiguration(2, new PresetDatabaseScopedConfiguration("3", PresetDatabaseScopedConfiguration.CommonSyntaxValues.OnOffOnly, PresetDatabaseScopedConfiguration.OnOffOnlyDatabaseValueToSyntaxValueConverter, null));
			}
		}
	}
}
