using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	sealed class PresetDatabaseScopedConfigurationTest : TestCase
	{
		public void TestAllowedSyntaxValues()
		{
			Test(new[] { "ON", "OFF" }, new[] { "ON", "OFF" }, Array.Empty<string>());
			Test(new[] { PresetDatabaseScopedConfiguration.CommonSyntaxValues.TInt }, Array.Empty<string>(), new[] { PresetDatabaseScopedConfiguration.CommonSyntaxValues.TInt });
			Test(new[] { PresetDatabaseScopedConfiguration.CommonSyntaxValues.TInt, "OFF" }, new[] { "OFF" }, new[] { PresetDatabaseScopedConfiguration.CommonSyntaxValues.TInt });

			void Test(string[] argument, string[] expectedKeywords, string[] expectedTypes)
			{
				// Arrange
				// Act
				var config = new PresetDatabaseScopedConfiguration("0", argument, PresetDatabaseScopedConfiguration.EqualDatabaseValueSyntaxValueConverter, null);

				// Assert
				AssertContainsExactElementsInAnyOrder(expectedKeywords, config.AllowedKeywordSyntaxValues);
				AssertContainsExactElementsInAnyOrder(expectedTypes, config.AllowedTypesOfSyntaxValues);
			}
		}

		public void TestCheckIfValueMatchesSyntax()
		{
			Test(new[] { "ON", "OFF" }, "ON", true);
			Test(new[] { "ON", "OFF" }, "On", true);
			Test(new[] { "ON", "OFF" }, "OFF", true);
			Test(new[] { "ON", "OFF" }, "off", true);
			Test(new[] { "ON", "OFF" }, "A", false);
			Test(new[] { "ON", "OFF" }, "abc", false);

			Test(new[] { PresetDatabaseScopedConfiguration.CommonSyntaxValues.TInt, "OFF" }, "OFF", true);
			Test(new[] { PresetDatabaseScopedConfiguration.CommonSyntaxValues.TInt, "OFF" }, "OfF", true);
			Test(new[] { PresetDatabaseScopedConfiguration.CommonSyntaxValues.TInt, "OFF" }, "100", true);
			Test(new[] { PresetDatabaseScopedConfiguration.CommonSyntaxValues.TInt, "OFF" }, "ON", false);
			Test(new[] { PresetDatabaseScopedConfiguration.CommonSyntaxValues.TInt, "OFF" }, "oN", false);
			Test(new[] { PresetDatabaseScopedConfiguration.CommonSyntaxValues.TInt, "OFF" }, "A", false);

			Test(new[] { PresetDatabaseScopedConfiguration.CommonSyntaxValues.TString }, "OFF", true);
			Test(new[] { PresetDatabaseScopedConfiguration.CommonSyntaxValues.TString }, "whatever", true);

			void Test(string[] allowedValues, string value, bool expectedResult)
			{
				// Arrange
				var config = new PresetDatabaseScopedConfiguration("0", allowedValues, PresetDatabaseScopedConfiguration.EqualDatabaseValueSyntaxValueConverter, null);

				// Act
				var result = config.CheckIfValueMatchesSyntax(value);

				// Assert
				AssertEquals(expectedResult, result);
			}
		}

		public static readonly (string Name, string Value)[] RecommendedItemSet = new[]
		{
			("ASYNC_STATS_UPDATE_WAIT_AT_LOW_PRIORITY", "ON"),
		};

		sealed class PresetConfigurationsTest : TestCase
		{
			public void TestName()
			{
				var names = new List<string>();
				var nameHashSet = new HashSet<string>(StringComparer.Ordinal);
				foreach (var config in PresetDatabaseScopedConfiguration.PresetConfigurations)
				{
					AssertNotNullOrEmpty(config.Name);
					AssertEquals("Upper case is required for consistency", config.Name.ToUpperInvariant(), config.Name);
					AssertEquals($"{config.Name} contains illegal character", true, config.Name.All(x => char.IsLetter(x) || x == '_'));

					names.Add(config.Name);
					nameHashSet.Add(config.Name);
				}

				AssertContainsExactElementsInAnyOrder(StringComparer.Ordinal, nameHashSet, names);
			}

			public void TestAllowedSyntaxValue()
			{
				foreach (var config in PresetDatabaseScopedConfiguration.PresetConfigurations)
				{
					AssertEquals(config.Name, true, config.AllowedKeywordSyntaxValues.Length > 0 || config.AllowedTypesOfSyntaxValues.Length > 0);

					var valueHashSet = new HashSet<string>(StringComparer.Ordinal);
					foreach (var value in config.AllowedKeywordSyntaxValues)
					{
						AssertNotNullOrEmpty(value);
						AssertEquals("Upper case is required for consistency", value.ToUpperInvariant(), value);
						AssertEquals($"{value} contains illegal character", true, int.TryParse(value, out _) || value.All(x => char.IsLetter(x) || x == '_'));

						valueHashSet.Add(value);
					}
					AssertContainsExactElementsInAnyOrder(StringComparer.Ordinal, valueHashSet, config.AllowedKeywordSyntaxValues);

					var typeHashSet = new HashSet<string>(StringComparer.Ordinal);
					foreach (var type in config.AllowedTypesOfSyntaxValues)
					{
						AssertNotNullOrEmpty(type);
						AssertEquals("Upper case is required for consistency", type.ToUpperInvariant(), type);
						AssertEquals($"{type} contains illegal character", true, type.Skip(2).All(char.IsLetter));

						typeHashSet.Add(type);
					}
					AssertContainsExactElementsInAnyOrder(StringComparer.Ordinal, typeHashSet, config.AllowedTypesOfSyntaxValues);

					AssertEquals(config.Name, true, typeHashSet.Count > 0 || valueHashSet.Count > 0);
				}
			}

			public void TestDatabaseValueToSyntaxValueConverter()
			{
				foreach (var config in PresetDatabaseScopedConfiguration.PresetConfigurations)
				{
					AssertNotNull(config.Name, config.DatabaseValueToSyntaxValueConverter);
				}
			}

			public void TestRecommendedValue()
			{
				foreach (var config in PresetDatabaseScopedConfiguration.PresetConfigurations)
				{
					AssertEquals(
						config.Name,
						true,
						string.IsNullOrEmpty(config.RecommendedSyntaxValue)
							|| config.RecommendedSyntaxValue.All(x => (char.IsLetter(x) && char.IsUpper(x)) || x == '_') // if not empty, it is allowed to be upper-case syntax keyword
							|| int.TryParse(config.RecommendedSyntaxValue, out _)); // or integer
				}
			}

			[DatCapabilityRequirementLatestAvailableSqlServer]
			public void TestRecommendedItemSet()
			{
				var configsRecommended = PresetDatabaseScopedConfiguration.PresetConfigurations
					.Where(x => !x.RecommendedSyntaxValue.IsNullOrEmpty())
					.Select(x => (x.Name, x.RecommendedSyntaxValue))
					.ToArray();
				AssertContainsExactElementsInAnyOrder(RecommendedItemSet, configsRecommended);
			}

			[DatCapabilityRequirementLatestAvailableSqlServer]
			public void TestAllKnownConfigurationsAreSyncedInOrder()
			{
				AssertContainsExactElementsInExactOrder(
					$"Database scoped configurations from database are supposed to be synchronized with {nameof(PresetDatabaseScopedConfiguration.PresetConfigurations)} in order",
					GetNamesFromDb(),
					PresetDatabaseScopedConfiguration.PresetConfigurations.Select(x => x.Name).ToArray());

				IEnumerable<string> GetNamesFromDb()
				{
					var names = new List<string>();
					using var connection = Db.NewAdminConnection();

					connection.ExecuteReader(
						"SELECT name FROM sys.database_scoped_configurations",
						record =>
						{
							names.Add((string)record["name"]);
						});

					return names;
				}
			}

			[DatCapabilityRequirementLatestAvailableSqlServer]
			[ExpectNoExceptions]
			public void TestAllAllowedValuesCanBeUsed()
			{
				var newDbName = nameof(TestAllAllowedValuesCanBeUsed);
				using var connection = Db.NewAdminConnection(Db.SqlMasterDb);
				using var dropDbDisposable = AdoTestUtils.CreateDbDropExistingDisposable(connection, newDbName);
				using var useMasterDisposable = ((ICurrentDbControl)connection).UseDatabase(newDbName);

				foreach (var config in PresetDatabaseScopedConfiguration.PresetConfigurations)
				{
					foreach (var allowedValue in config.AllowedKeywordSyntaxValues)
					{
						DatabaseScopedConfigurationExtensions.WriteConfiguration(connection, config.Name, allowedValue);
					}

					foreach (var allowedType in config.AllowedTypesOfSyntaxValues)
					{
						switch (allowedType)
						{
							case "T:STRING":
								break; // skip
							case "T:INT":
								DatabaseScopedConfigurationExtensions.WriteConfiguration(connection, config.Name, "1");
								DatabaseScopedConfigurationExtensions.WriteConfiguration(connection, config.Name, "10");
								break;
							default:
								throw new NotSupportedException($"Type {allowedType} is not supported");
						}
					}

					if (!config.RecommendedSyntaxValue.IsNullOrEmpty())
					{
						DatabaseScopedConfigurationExtensions.WriteConfiguration(connection, config.Name, config.RecommendedSyntaxValue);
					}
				}
			}
		}
	}
}
