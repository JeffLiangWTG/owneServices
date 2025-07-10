using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(DatabaseScopedConfiguration))]
	sealed class DatabaseScopedConfigurationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			// Arrange
			// Act
			var databaseScopedConfiguration = new DatabaseScopedConfiguration(
				1,
				new PresetDatabaseScopedConfiguration("2", PresetDatabaseScopedConfiguration.CommonSyntaxValues.OnOffOnly, PresetDatabaseScopedConfiguration.OnOffOnlyDatabaseValueToSyntaxValueConverter, "ON"));

			// Assert
			AssertEquals(1, databaseScopedConfiguration.ConfigurationId);
			AssertEquals("2", databaseScopedConfiguration.Name);
			AssertEquals("ON", databaseScopedConfiguration.RecommendedValue);
		}

		public void TestRefreshWithLatestDataFromDb_Int()
		{
			// Arrange
			var databaseScopedConfiguration = new DatabaseScopedConfiguration(
				1,
				new PresetDatabaseScopedConfiguration("2", new[] { PresetDatabaseScopedConfiguration.CommonSyntaxValues.TInt }, PresetDatabaseScopedConfiguration.EqualDatabaseValueSyntaxValueConverter, null));

			// Act
			databaseScopedConfiguration.RefreshWithLatestDataFromDb(3, 5, true);

			// Assert
			AssertEquals(3, databaseScopedConfiguration.CurrentValue);
			AssertEquals("3", databaseScopedConfiguration.CurrentValueText);
			AssertEquals(5, databaseScopedConfiguration.CurrentValueForSecondary);
			AssertEquals("5", databaseScopedConfiguration.CurrentValueForSecondaryText);
			AssertEquals(true, databaseScopedConfiguration.IsValueDefault);
		}

		public void TestRefreshWithLatestDataFromDb_OnOff()
		{
			// Arrange
			var databaseScopedConfiguration = new DatabaseScopedConfiguration(
				1,
				new PresetDatabaseScopedConfiguration("2", PresetDatabaseScopedConfiguration.CommonSyntaxValues.OnOffOnly, PresetDatabaseScopedConfiguration.OnOffOnlyDatabaseValueToSyntaxValueConverter, null));

			// Act
			databaseScopedConfiguration.RefreshWithLatestDataFromDb(true, false, true);

			// Assert
			AssertEquals(true, databaseScopedConfiguration.CurrentValue);
			AssertEquals("ON", databaseScopedConfiguration.CurrentValueText);
			AssertEquals(false, databaseScopedConfiguration.CurrentValueForSecondary);
			AssertEquals("OFF", databaseScopedConfiguration.CurrentValueForSecondaryText);
			AssertEquals(true, databaseScopedConfiguration.IsValueDefault);
		}

		public void TestRefreshWithLatestDataFromDb_NullSecondaryValue()
		{
			// Arrange
			var databaseScopedConfiguration = new DatabaseScopedConfiguration(
				1,
				new PresetDatabaseScopedConfiguration("2", PresetDatabaseScopedConfiguration.CommonSyntaxValues.OnOffOnly, PresetDatabaseScopedConfiguration.OnOffOnlyDatabaseValueToSyntaxValueConverter, null));

			// Act
			databaseScopedConfiguration.RefreshWithLatestDataFromDb(false, null, false);

			// Assert
			AssertEquals(false, databaseScopedConfiguration.CurrentValue);
			AssertEquals("OFF", databaseScopedConfiguration.CurrentValueText);
			AssertEquals(null, databaseScopedConfiguration.CurrentValueForSecondary);
			AssertEquals(string.Empty, databaseScopedConfiguration.CurrentValueForSecondaryText);
			AssertEquals(false, databaseScopedConfiguration.IsValueDefault);
		}

		public void TestRefreshWithLatestDataFromDb_NoEventFiredWhenNoDataRefreshed()
		{
			// Arrange
			var databaseScopedConfiguration = new DatabaseScopedConfiguration(
				1,
				new PresetDatabaseScopedConfiguration("2", new[] { PresetDatabaseScopedConfiguration.CommonSyntaxValues.TInt }, PresetDatabaseScopedConfiguration.EqualDatabaseValueSyntaxValueConverter, null));
			databaseScopedConfiguration.RefreshWithLatestDataFromDb(10, 20, true);

			databaseScopedConfiguration.CurrentValueTextInfo.ValueChanged += ValueChanged;
			databaseScopedConfiguration.CurrentValueForSecondaryTextInfo.ValueChanged += ValueChanged;
			databaseScopedConfiguration.IsValueDefaultInfo.ValueChanged += ValueChanged;

			// Act
			databaseScopedConfiguration.RefreshWithLatestDataFromDb(10, 20, true);

			// Assert
			AssertEquals(10, databaseScopedConfiguration.CurrentValue);
			AssertEquals("10", databaseScopedConfiguration.CurrentValueText);
			AssertEquals(20, databaseScopedConfiguration.CurrentValueForSecondary);
			AssertEquals("20", databaseScopedConfiguration.CurrentValueForSecondaryText);
			AssertEquals(true, databaseScopedConfiguration.IsValueDefault);

			void ValueChanged(object sender, EventArgs e)
			{
				Assert("ValueChanged event should not be fired", false);
			}
		}

		public void TestRefreshWithLatestDataFromDb_EventFiredWhenDataIsRefreshed()
		{
			// Arrange
			var databaseScopedConfiguration = new DatabaseScopedConfiguration(
				1,
				new PresetDatabaseScopedConfiguration("2", new[] { PresetDatabaseScopedConfiguration.CommonSyntaxValues.TInt }, PresetDatabaseScopedConfiguration.EqualDatabaseValueSyntaxValueConverter, null));
			databaseScopedConfiguration.RefreshWithLatestDataFromDb(10, 20, true);

			var propertyInfos = new List<ZPropertyInfo>();
			databaseScopedConfiguration.CurrentValueTextInfo.ValueChanged += ValueChanged;
			databaseScopedConfiguration.CurrentValueForSecondaryTextInfo.ValueChanged += ValueChanged;
			databaseScopedConfiguration.IsValueDefaultInfo.ValueChanged += ValueChanged;

			// Act
			databaseScopedConfiguration.RefreshWithLatestDataFromDb(100, 200, false);

			// Assert
			AssertEquals(100, databaseScopedConfiguration.CurrentValue);
			AssertEquals("100", databaseScopedConfiguration.CurrentValueText);
			AssertEquals(200, databaseScopedConfiguration.CurrentValueForSecondary);
			AssertEquals("200", databaseScopedConfiguration.CurrentValueForSecondaryText);
			AssertEquals(false, databaseScopedConfiguration.IsValueDefault);
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					databaseScopedConfiguration.CurrentValueTextInfo,
					databaseScopedConfiguration.CurrentValueForSecondaryTextInfo,
					databaseScopedConfiguration.IsValueDefaultInfo,
				},
				propertyInfos);

			void ValueChanged(object sender, EventArgs e)
			{
				if (e is InfoEventArgs infoEventArgs)
				{
					propertyInfos.Add(infoEventArgs.Info);
				}
			}
		}

		public void TestProposedValue()
		{
			Test(new PresetDatabaseScopedConfiguration("2", PresetDatabaseScopedConfiguration.CommonSyntaxValues.OnOffOnly, PresetDatabaseScopedConfiguration.OnOffOnlyDatabaseValueToSyntaxValueConverter, null), "ON");
			Test(new PresetDatabaseScopedConfiguration("2", new[] { PresetDatabaseScopedConfiguration.CommonSyntaxValues.TInt }, PresetDatabaseScopedConfiguration.EqualDatabaseValueSyntaxValueConverter, null), "100");

			void Test(PresetDatabaseScopedConfiguration presetConfig, string value)
			{
				// Arrange
				var databaseScopedConfiguration = new DatabaseScopedConfiguration(1, presetConfig);
				databaseScopedConfiguration.ProposedValueInfo.ValueChanged += ValueChanged;

				// Act
				databaseScopedConfiguration.ProposedValue = value;

				// Assert
				AssertEquals(value, databaseScopedConfiguration.ProposedValue);
				AssertEquals(false, databaseScopedConfiguration.HasErrors);

				void ValueChanged(object sender, EventArgs e)
				{
					AssertEquals(databaseScopedConfiguration.ProposedValueInfo, ((InfoEventArgs)e).Info);
				}
			}
		}

		public void TestProposedValue_ValueChangedEventNotFired()
		{
			// Arrange
			var databaseScopedConfiguration = new DatabaseScopedConfiguration(
				1,
				new PresetDatabaseScopedConfiguration("2", new[] { PresetDatabaseScopedConfiguration.CommonSyntaxValues.TInt }, PresetDatabaseScopedConfiguration.EqualDatabaseValueSyntaxValueConverter, null));
			databaseScopedConfiguration.ProposedValue = "10";
			databaseScopedConfiguration.ProposedValueInfo.ValueChanged += ValueChanged;

			// Act
			databaseScopedConfiguration.ProposedValue = "10  ";

			// Assert
			AssertEquals("10", databaseScopedConfiguration.ProposedValue);
			AssertEquals(false, databaseScopedConfiguration.ProposedValueInfo.HasErrors());

			void ValueChanged(object sender, EventArgs e)
			{
				Assert(false);
			}
		}

		public void TestProposedValue_Error()
		{
			Test("'A", "Single quotes in entered value ({0}) should be in pair.");
			Test(" 'A", "Single quotes in entered value ({0}) should be in pair.");
			Test("B'", "Single quotes in entered value ({0}) should be in pair.");
			Test("B' ", "Single quotes in entered value ({0}) should be in pair.");

			Test("''AB'", "Single quotes is disallowed in the middle of entered value ({0}).");
			Test("''AB' ", "Single quotes is disallowed in the middle of entered value ({0}).");

			Test("'A'B'", "Single quotes is disallowed in the middle of entered value ({0}).");
			Test("'A'B' ", "Single quotes is disallowed in the middle of entered value ({0}).");

			Test("'AB''", "Single quotes is disallowed in the middle of entered value ({0}).");
			Test("'AB'' ", "Single quotes is disallowed in the middle of entered value ({0}).");

			Test("A'B", "Single quotes is disallowed in the middle of entered value ({0}).");
			Test(" A'B", "Single quotes is disallowed in the middle of entered value ({0}).");

			Test("A\rB", "Control character is not supported.");
			Test("A\nB", "Control character is not supported.");
			Test("A\r\tB", "Control character is not supported.");

			Test("AB", "Entered value ({0}) does not match syntax requirement.");

			void Test(string proposedValue, string errorFormat)
			{
				// Arrange
				var config = new DatabaseScopedConfiguration(
					1,
					new PresetDatabaseScopedConfiguration("MAXDOP", new[] { PresetDatabaseScopedConfiguration.CommonSyntaxValues.TInt }, PresetDatabaseScopedConfiguration.EqualDatabaseValueSyntaxValueConverter, null));

				// Act
				config.ProposedValue = proposedValue;

				// Assert
				AssertEquals($"Case [{proposedValue}]", true, config.ProposedValueInfo.HasError(string.Format(CultureInfo.InvariantCulture, errorFormat, config.ProposedValue)));
			}
		}

		public void TestAllErrorsAreClearedAfterFix_PropertyError()
		{
			// Arrange
			var config = new DatabaseScopedConfiguration(
				1,
				new PresetDatabaseScopedConfiguration("MAXDOP", new[] { PresetDatabaseScopedConfiguration.CommonSyntaxValues.TInt }, PresetDatabaseScopedConfiguration.EqualDatabaseValueSyntaxValueConverter, null));
			config.ProposedValue = "1'2";

			// Act
			config.ProposedValue = "12";

			// Assert
			AssertEquals(false, config.HasErrors);
		}

		public void TestAllErrorsAreClearedAfterFix_RowError()
		{
			// Arrange
			var config = new DatabaseScopedConfiguration(
				1,
				new PresetDatabaseScopedConfiguration("MAXDOP", new[] { PresetDatabaseScopedConfiguration.CommonSyntaxValues.TInt }, PresetDatabaseScopedConfiguration.EqualDatabaseValueSyntaxValueConverter, null));
			config.AddRowError("whatever");

			// Act
			config.ProposedValue = "12";

			// Assert
			AssertEquals(false, config.HasErrors);
		}

		public void TestProposedValueInfoRefreshBindingAfterValidation()
		{
			Test("10", false);
			Test("'10", true);

			void Test(string proposedValue, bool hasErrors)
			{
				// Arrange
				var config = new DatabaseScopedConfiguration(
				1,
				new PresetDatabaseScopedConfiguration("MAXDOP", new[] { PresetDatabaseScopedConfiguration.CommonSyntaxValues.TInt }, PresetDatabaseScopedConfiguration.EqualDatabaseValueSyntaxValueConverter, null));
				config.ProposedValueInfo.ValueChanged += ValueChanged;

				// Act
				config.ProposedValue = proposedValue;

				// Assert
				void ValueChanged(object sender, EventArgs e)
				{
					AssertEquals(hasErrors, config.HasErrors);
				}
			}
		}

		public void TestCanBeSavedPotentiallyReturnsTrueAfterChange()
		{
			// Arrange
			var config = new DatabaseScopedConfiguration(
				1,
				new PresetDatabaseScopedConfiguration("MAXDOP", new[] { PresetDatabaseScopedConfiguration.CommonSyntaxValues.TInt }, PresetDatabaseScopedConfiguration.EqualDatabaseValueSyntaxValueConverter, null));

			// Act
			config.ProposedValue = "1";
			config.HasChanges = true; // UI framework set HasChanges

			// Assert
			AssertEquals(true, config.CanBeSavedPotentially);
		}

		public void TestCanBeSavedPotentiallyReturnsFalseWhenProposedValueIsEmpty()
		{
			// Arrange
			var config = new DatabaseScopedConfiguration(
				1,
				new PresetDatabaseScopedConfiguration("MAXDOP", new[] { PresetDatabaseScopedConfiguration.CommonSyntaxValues.TInt }, PresetDatabaseScopedConfiguration.EqualDatabaseValueSyntaxValueConverter, null));

			// Act
			config.HasChanges = true; // UI framework set HasChanges

			// Assert
			AssertEquals(false, config.CanBeSavedPotentially);
		}

		public void TestCanBeSavedPotentiallyReturnsFalseWhenNoChange()
		{
			// Arrange
			var config = new DatabaseScopedConfiguration(
				1,
				new PresetDatabaseScopedConfiguration("MAXDOP", new[] { PresetDatabaseScopedConfiguration.CommonSyntaxValues.TInt }, PresetDatabaseScopedConfiguration.EqualDatabaseValueSyntaxValueConverter, null));
			config.ProposedValue = "1";

			// Act
			// Assert
			AssertEquals(false, config.CanBeSavedPotentially);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DatabaseScopedConfiguration(
				1,
				new PresetDatabaseScopedConfiguration("MAXDOP", new[] { PresetDatabaseScopedConfiguration.CommonSyntaxValues.TInt }, PresetDatabaseScopedConfiguration.EqualDatabaseValueSyntaxValueConverter, null));
		}
	}
}
