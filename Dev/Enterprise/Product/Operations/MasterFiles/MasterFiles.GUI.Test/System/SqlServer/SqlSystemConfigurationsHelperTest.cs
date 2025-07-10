using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	sealed class SqlSystemConfigurationsHelperTest : TestCase
	{
		public void TestDisallowResultsFromTriggersConfigurationIsRecommended()
		{
			// Arrange
			using (var connection = Db.NewAdminConnection())
			{
				var collection = SqlSystemConfigurationsHelper.Instance.LoadSqlServerConfigurations(connection);

				// Act
				SqlSystemConfigurationsHelper.Instance.ComputeOptimalValues(collection.Configurations);

				// Assert
				var config = collection.Configurations.First(x => x.ConfigurationId == 114);
				AssertEquals("disallow results from triggers", config.Name);
				AssertEquals("1", config.RecommendedValue);
			}
		}

		sealed class ComputedValueTest : TestCase
		{
			public void TestClrEnabledValue()
			{
				// Arrange
				const string expectedClrEnabledValue = "2020";
				var testScripts = new Dictionary<int, string>
				{
					{ SqlSystemConfigurationsHelper.ClrEnabledConfigId, $"{expectedClrEnabledValue}" },
				};
				var testBlockIds = new int[] { SqlSystemConfigurationsHelper.ClrEnabledConfigId };

				var configClrEnable = new SqlSystemConfiguration(SqlSystemConfigurationsHelper.ClrEnabledConfigId)
				{
					Name = "clr enable",
					MinValue = 0,
					MaxValue = 1,
					ConfiguredValue = 0,
				};
				var helper = new SqlSystemConfigurationsHelper(testScripts, testBlockIds);
				var isReadonlyBeforeCompute = configClrEnable.ProposedValueIsReadOnly;

				// Act
				helper.ComputeOptimalValues(new[] { configClrEnable });

				// Assert
				Assert(isReadonlyBeforeCompute);
				AssertEquals(expectedClrEnabledValue, configClrEnable.ProposedValueText);
				AssertEquals(expectedClrEnabledValue, configClrEnable.RecommendedValue);
			}

			public void TestTwoDigitYearCutoff()
			{
				// Arrange
				const string expectedTwoDigitYearCutoff = "2099";
				var testScripts = new Dictionary<int, string>
				{
					{ 1127, $"{expectedTwoDigitYearCutoff}" },
				};

				var configTwoDigitYearCutoff = new SqlSystemConfiguration(1127)
				{
					Name = "two digit year cutoff",
					MinValue = 1753,
					MaxValue = 9999,
					ConfiguredValue = 2000
				};
				var helper = new SqlSystemConfigurationsHelper(testScripts, Array.Empty<int>());
				var isReadonlyBeforeCompute = configTwoDigitYearCutoff.ProposedValueIsReadOnly;

				// Act
				helper.ComputeOptimalValues(new[] { configTwoDigitYearCutoff });

				// Assert
				Assert(!isReadonlyBeforeCompute);
				Assert(!configTwoDigitYearCutoff.ProposedValueIsReadOnly);
				AssertEquals(expectedTwoDigitYearCutoff, configTwoDigitYearCutoff.RecommendedValue);
				Assert(string.IsNullOrEmpty(configTwoDigitYearCutoff.ProposedValueText));
			}

			public void TestMaximunRecoverInterval()
			{
				// Arrange
				const string expectedMaximunRecoverInterval = "1";
				var testScripts = new Dictionary<int, string>
				{
					{ SqlSystemConfigurationsHelper.RecoveryIntervalConfigId, $"{expectedMaximunRecoverInterval}" },
				};

				var configMaximunRecoverInterval = new SqlSystemConfiguration(SqlSystemConfigurationsHelper.RecoveryIntervalConfigId)
				{
					Name = "recovery interval (min)",
					MinValue = 0,
					MaxValue = 32767,
					ConfiguredValue = 0
				};

				var helper = new SqlSystemConfigurationsHelper(testScripts, Array.Empty<int>());
				var isReadonlyBeforeCompute = configMaximunRecoverInterval.ProposedValueIsReadOnly;

				// Act
				helper.ComputeOptimalValues(new[] { configMaximunRecoverInterval });

				// Assert
				Assert(isReadonlyBeforeCompute);
				AssertEquals(expectedMaximunRecoverInterval, configMaximunRecoverInterval.ProposedValueText);
				AssertEquals(expectedMaximunRecoverInterval, configMaximunRecoverInterval.RecommendedValue);
			}

			public void TestInstanceRecommendedValues()
			{
				// Arrange
				const int recoveryIntervalConfigId = 101;
				const int allowUpdatesConfigId = 102;
				const int clrEnabledConfigId = 1562;
				const int textSizeConfigId = 1536;
				const int disallowResultsFromTriggersId = 114;
				var expectedValue = new Dictionary<int, string>()
				{
					{ recoveryIntervalConfigId, "1" },
					{ allowUpdatesConfigId, "0" },
					{ clrEnabledConfigId, "1" },
					{ textSizeConfigId, "-1" },
					{ disallowResultsFromTriggersId, "1" },
				}.ToArray();

				// Act
				var realValues = SqlSystemConfigurationsHelper
					.Instance
					.RecommendedValues
					.ToArray();

				// Assert
				AssertSequencesEqual(expectedValue, realValues);
			}

			public void TestInstanceBlockedConfigurationIds()
			{
				// Arrange
				const int recoveryIntervalConfigId = 101;
				const int allowUpdatesConfigId = 102;
				const int clrEnabledConfigId = 1562;
				var expectedValue = new int[]
				{
					recoveryIntervalConfigId,
					allowUpdatesConfigId,
					clrEnabledConfigId
				};

				// Act
				var realValues = SqlSystemConfigurationsHelper
					.Instance
					.BlockedConfigurationIds;

				// Assert
				AssertSequencesEqual(expectedValue, realValues);
			}
		}
	}
}
