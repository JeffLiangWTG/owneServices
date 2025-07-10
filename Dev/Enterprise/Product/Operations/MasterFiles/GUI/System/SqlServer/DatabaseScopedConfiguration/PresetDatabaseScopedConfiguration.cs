using System;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.MasterFiles.GUI
{
	sealed class PresetDatabaseScopedConfiguration
	{
		public PresetDatabaseScopedConfiguration(
			string name,
			string[] allowedSyntaxValues,
			Func<PresetDatabaseScopedConfiguration, string, string> databaseValueToSyntaxValueConverter,
			string recommendedSyntaxValue)
		{
			Name = name;
			this.DatabaseValueToSyntaxValueConverter = databaseValueToSyntaxValueConverter;
			RecommendedSyntaxValue = recommendedSyntaxValue;

			var syntaxValues = GetSyntaxValues();
			AllowedKeywordSyntaxValues = syntaxValues.keywords;
			AllowedTypesOfSyntaxValues = syntaxValues.types;

			(string[] keywords, string[] types) GetSyntaxValues()
			{
				var keywords = new List<string>();
				var types = new List<string>();

				foreach (var value in allowedSyntaxValues)
				{
					if (value.StartsWith(CommonSyntaxValues.OfTypePrefix, StringComparison.Ordinal))
					{
						types.Add(value);
					}
					else
					{
						keywords.Add(value);
					}
				}
				return (keywords.ToArray(), types.ToArray());
			}
		}

		public string Name { get; }
		public string[] AllowedKeywordSyntaxValues { get; }
		public string[] AllowedTypesOfSyntaxValues { get; }

		public bool CheckIfValueMatchesSyntax(string value)
		{
			if (AllowedKeywordSyntaxValues.Contains(value, StringComparer.OrdinalIgnoreCase))
			{
				return true;
			}

			foreach (var type in AllowedTypesOfSyntaxValues)
			{
				if (type == CommonSyntaxValues.TString)
				{
					return true;
				}

				if (type == CommonSyntaxValues.TInt && int.TryParse(value, out _))
				{
					return true;
				}
			}

			return false;
		}

		public Func<PresetDatabaseScopedConfiguration, string, string> DatabaseValueToSyntaxValueConverter { get; }
		public static readonly Func<PresetDatabaseScopedConfiguration, string, string> OnOffOnlyDatabaseValueToSyntaxValueConverter = (_, dbValue)
			=> dbValue switch {
				CommonSyntaxValues.OnFromDb => CommonSyntaxValues.On,
				CommonSyntaxValues.OffFromDb => CommonSyntaxValues.Off,
				_ => throw new SyntaxValueNotMatchedException(dbValue)
			};
		public static readonly Func<PresetDatabaseScopedConfiguration, string, string> EqualDatabaseValueSyntaxValueConverter = (_, x) => x;

		public string RecommendedSyntaxValue { get; }

		internal static class CommonSyntaxValues
		{
			public const string OnFromDb = "TRUE";
			public const string On = "ON";

			public const string OffFromDb = "FALSE";
			public const string Off = "OFF";

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "NO UPDATE AT RUNTIME")]
			public static readonly string[] OnOffOnly = { On, Off };

			public const string OfTypePrefix = "T:";
			public static readonly string TInt = $"{OfTypePrefix}INT";
			public static readonly string TString = $"{OfTypePrefix}STRING";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "NO UPDATE AT RUNTIME")]
		public static readonly PresetDatabaseScopedConfiguration[] PresetConfigurations = new PresetDatabaseScopedConfiguration[]
		{
			new PresetDatabaseScopedConfiguration(
				"MAXDOP",
				new[] { CommonSyntaxValues.TInt },
				EqualDatabaseValueSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"LEGACY_CARDINALITY_ESTIMATION",
				CommonSyntaxValues.OnOffOnly,
				OnOffOnlyDatabaseValueToSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"PARAMETER_SNIFFING",
				CommonSyntaxValues.OnOffOnly,
				OnOffOnlyDatabaseValueToSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"QUERY_OPTIMIZER_HOTFIXES",
				CommonSyntaxValues.OnOffOnly,
				OnOffOnlyDatabaseValueToSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"IDENTITY_CACHE",
				CommonSyntaxValues.OnOffOnly,
				OnOffOnlyDatabaseValueToSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"INTERLEAVED_EXECUTION_TVF",
				CommonSyntaxValues.OnOffOnly,
				OnOffOnlyDatabaseValueToSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"BATCH_MODE_MEMORY_GRANT_FEEDBACK",
				CommonSyntaxValues.OnOffOnly,
				OnOffOnlyDatabaseValueToSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"BATCH_MODE_ADAPTIVE_JOINS",
				CommonSyntaxValues.OnOffOnly,
				OnOffOnlyDatabaseValueToSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"TSQL_SCALAR_UDF_INLINING",
				CommonSyntaxValues.OnOffOnly,
				OnOffOnlyDatabaseValueToSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"ELEVATE_ONLINE",
				new[] { CommonSyntaxValues.Off, "WHEN_SUPPORTED", "FAIL_UNSUPPORTED" },
				EqualDatabaseValueSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"ELEVATE_RESUMABLE",
				new[] { CommonSyntaxValues.Off, "WHEN_SUPPORTED", "FAIL_UNSUPPORTED" },
				EqualDatabaseValueSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"OPTIMIZE_FOR_AD_HOC_WORKLOADS",
				CommonSyntaxValues.OnOffOnly,
				OnOffOnlyDatabaseValueToSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"XTP_PROCEDURE_EXECUTION_STATISTICS",
				CommonSyntaxValues.OnOffOnly,
				OnOffOnlyDatabaseValueToSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"XTP_QUERY_EXECUTION_STATISTICS",
				CommonSyntaxValues.OnOffOnly,
				OnOffOnlyDatabaseValueToSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"ROW_MODE_MEMORY_GRANT_FEEDBACK",
				CommonSyntaxValues.OnOffOnly,
				OnOffOnlyDatabaseValueToSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"ISOLATE_SECURITY_POLICY_CARDINALITY",
				CommonSyntaxValues.OnOffOnly,
				OnOffOnlyDatabaseValueToSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"BATCH_MODE_ON_ROWSTORE",
				CommonSyntaxValues.OnOffOnly,
				OnOffOnlyDatabaseValueToSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"DEFERRED_COMPILATION_TV",
				CommonSyntaxValues.OnOffOnly,
				OnOffOnlyDatabaseValueToSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"ACCELERATED_PLAN_FORCING",
				CommonSyntaxValues.OnOffOnly,
				OnOffOnlyDatabaseValueToSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"GLOBAL_TEMPORARY_TABLE_AUTO_DROP",
				CommonSyntaxValues.OnOffOnly,
				OnOffOnlyDatabaseValueToSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"LIGHTWEIGHT_QUERY_PROFILING",
				CommonSyntaxValues.OnOffOnly,
				OnOffOnlyDatabaseValueToSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"VERBOSE_TRUNCATION_WARNINGS",
				CommonSyntaxValues.OnOffOnly,
				OnOffOnlyDatabaseValueToSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"LAST_QUERY_PLAN_STATS",
				CommonSyntaxValues.OnOffOnly,
				OnOffOnlyDatabaseValueToSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"PAUSED_RESUMABLE_INDEX_ABORT_DURATION_MINUTES",
				new[] { CommonSyntaxValues.TInt },
				EqualDatabaseValueSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"DW_COMPATIBILITY_LEVEL",
				new[] { "AUTO", "10", "20", "30", "40", "50", "9000" },
				(config, dbValue)
					=> dbValue switch
					{
						"0" => "AUTO",
						_ => config.AllowedKeywordSyntaxValues.Any(x => StringComparer.Ordinal.Equals(x, dbValue)) ? dbValue : throw new SyntaxValueNotMatchedException(dbValue),
					},
				null),
			new PresetDatabaseScopedConfiguration(
				"EXEC_QUERY_STATS_FOR_SCALAR_FUNCTIONS",
				CommonSyntaxValues.OnOffOnly,
				OnOffOnlyDatabaseValueToSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"PARAMETER_SENSITIVE_PLAN_OPTIMIZATION",
				CommonSyntaxValues.OnOffOnly,
				OnOffOnlyDatabaseValueToSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"ASYNC_STATS_UPDATE_WAIT_AT_LOW_PRIORITY",
				CommonSyntaxValues.OnOffOnly,
				OnOffOnlyDatabaseValueToSyntaxValueConverter,
				"ON"),
			new PresetDatabaseScopedConfiguration(
				"CE_FEEDBACK",
				CommonSyntaxValues.OnOffOnly,
				OnOffOnlyDatabaseValueToSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"MEMORY_GRANT_FEEDBACK_PERSISTENCE",
				CommonSyntaxValues.OnOffOnly,
				OnOffOnlyDatabaseValueToSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"MEMORY_GRANT_FEEDBACK_PERCENTILE_GRANT",
				CommonSyntaxValues.OnOffOnly,
				OnOffOnlyDatabaseValueToSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"OPTIMIZED_PLAN_FORCING",
				CommonSyntaxValues.OnOffOnly,
				OnOffOnlyDatabaseValueToSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"DOP_FEEDBACK",
				CommonSyntaxValues.OnOffOnly,
				OnOffOnlyDatabaseValueToSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"LEDGER_DIGEST_STORAGE_ENDPOINT",
				new[] { CommonSyntaxValues.TString },
				EqualDatabaseValueSyntaxValueConverter,
				null),
			new PresetDatabaseScopedConfiguration(
				"FORCE_SHOWPLAN_RUNTIME_PARAMETER_COLLECTION",
				CommonSyntaxValues.OnOffOnly,
				OnOffOnlyDatabaseValueToSyntaxValueConverter,
				null),
		};
	}
}
