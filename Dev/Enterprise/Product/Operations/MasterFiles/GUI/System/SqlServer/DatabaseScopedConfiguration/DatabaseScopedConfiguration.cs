using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.GUI
{
	[DebuggerDisplay("{Name}, {CurrentValue}, {ProposedValue}")]
	sealed class DatabaseScopedConfiguration : NonPersistentBusinessObject
	{
		[ReadOnly(true)]
		public ZInt ConfigurationId { get; }

		[ReadOnly(true)]
		public ZString Name => PresetValue.Name;

		[ReadOnly(true)]
		public ZString RecommendedValue { get; }

		public PresetDatabaseScopedConfiguration PresetValue { get; }
		public DatabaseScopedConfiguration(int configurationId, PresetDatabaseScopedConfiguration presetValue)
		{
			ConfigurationId = configurationId;
			PresetValue = presetValue;
			RecommendedValue = presetValue.RecommendedSyntaxValue;
		}

		object currentValue;
		public object CurrentValue
		{
			get => currentValue;
			private set
			{
				currentValue = value;
				CurrentValueText = currentValue != null ? PresetValue.DatabaseValueToSyntaxValueConverter(PresetValue, currentValue.ToString().ToUpperInvariant()) : string.Empty;
			}
		}

		ZString currentValueText;
		public ZString CurrentValueText
		{
			get => currentValueText;
			private set
			{
				if (value == currentValueText)
				{
					return;
				}

				currentValueText = value;
				CurrentValueTextInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CurrentValueTextInfo
		{
			get => GetZPropertyInfo(nameof(CurrentValueText));
		}

		object currentValueForSecondary;
		public object CurrentValueForSecondary
		{
			get => currentValueForSecondary;
			private set
			{
				currentValueForSecondary = value;
				CurrentValueForSecondaryText = currentValueForSecondary != null ? PresetValue.DatabaseValueToSyntaxValueConverter(PresetValue, currentValueForSecondary.ToString().ToUpperInvariant()) : string.Empty;
			}
		}

		ZString currentValueForSecondaryText;
		public ZString CurrentValueForSecondaryText
		{
			get => currentValueForSecondaryText;
			private set
			{
				if (value == currentValueForSecondaryText)
				{
					return;
				}

				currentValueForSecondaryText = value;
				CurrentValueForSecondaryTextInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CurrentValueForSecondaryTextInfo
		{
			get => GetZPropertyInfo(nameof(CurrentValueForSecondaryText));
		}

		ZBool isValueDefault;
		public ZBool IsValueDefault
		{
			get => isValueDefault;
			private set
			{
				if (value == isValueDefault)
				{
					return;
				}

				isValueDefault = value;
				IsValueDefaultInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsValueDefaultInfo
		{
			get => GetZPropertyInfo(nameof(IsValueDefault));
		}

		public void RefreshWithLatestDataFromDb(object currentValueFromDatabase, object currentValueForSecondaryFromDatabase, bool isValueDefault)
		{
			CurrentValue = currentValueFromDatabase;
			CurrentValueForSecondary = currentValueForSecondaryFromDatabase;
			IsValueDefault = isValueDefault;
		}

		ZBool isDbReadOnly;
		public ZBool IsDbReadOnly
		{
			get => isDbReadOnly;
			set
			{
				if (value == isDbReadOnly)
				{
					return;
				}

				isDbReadOnly = value;
				IsDbReadOnlyInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsDbReadOnlyInfo
		{
			get => GetZPropertyInfo(nameof(IsDbReadOnly));
		}

		ZString proposedValue;
		[MaxLength(500)]
		[ReadOnlyMember(nameof(IsDbReadOnly))]
		public ZString ProposedValue
		{
			get => proposedValue;
			set
			{
				value = value.Trim();
				if (value == proposedValue)
				{
					return;
				}

				CheckMaximumLength(ProposedValueInfo, value);
				proposedValue = value;

				if (!IsValidationSuspended)
				{
					new DatabaseScopedConfigurationValidation(this).ValidateProposedValue();
				}

				// ValueChanged event might be subscribed, then it needs to validate property before raising this event so that subscriber can get right info.
				ProposedValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ProposedValueInfo
		{
			get => GetZPropertyInfo(nameof(ProposedValue));
		}

		public bool CanBeSavedPotentially => !ProposedValue.IsEmpty && HasChanges;

		public bool HasRecommendedValue => !RecommendedValue.IsEmpty;

		sealed class DatabaseScopedConfigurationValidation : ZValidation
		{
			readonly DatabaseScopedConfiguration configuration;

			public DatabaseScopedConfigurationValidation(DatabaseScopedConfiguration configuration) : base(configuration)
			{
				this.configuration = configuration;
			}

			public override void ValidateAll()
			{
				ValidateCalculatedProperty(configuration.ProposedValueInfo);
			}

			public void ValidateProposedValue()
			{
				configuration.ClearRowNotifications();
				((IValidationInternals)this).Validate(configuration.ProposedValueInfo, CheckProposedValue);
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by ZValidation")]
			void CheckProposedValue()
			{
				if (configuration.ProposedValue.IsEmpty)
				{
					return;
				}

				if (((string)configuration.ProposedValue).Any(char.IsControl))
				{
					configuration.ProposedValueInfo.AddError(Res.GetString(
						"DF50860A-02EF-4D3B-98EF-C9D193974F04",
						"Control character is not supported.",
						configuration.ProposedValue));
					return;
				}

				if (configuration.ProposedValue.StartsWith("'") ^ configuration.ProposedValue.EndsWith("'"))
				{
					configuration.ProposedValueInfo.AddError(Res.GetString(
						"4BBD3FE5-72F1-4779-9D1B-9143FB1D0B60",
						"Single quotes in entered value ({0}) should be in pair.",
						configuration.ProposedValue));
					return;
				}

				// to avoid sql injection like "ALTER DATABASE SCOPED CONFIGURATION SET XXX = 'YYY'; SELECT * FROM StmData WHERE SD_Name = 'ZZZ'",
				var secondIndexOfSingleQuotes = configuration.ProposedValue.IndexOf("'", 1, StringComparison.Ordinal);
				if (secondIndexOfSingleQuotes > 0 && secondIndexOfSingleQuotes != configuration.ProposedValue.Length - 1)
				{
					configuration.ProposedValueInfo.AddError(Res.GetString(
						"3C0265AE-E3CB-4A5C-A56E-1E836F30F4B4",
						"Single quotes is disallowed in the middle of entered value ({0}).",
						configuration.ProposedValue));
					return;
				}

				if (!configuration.PresetValue.CheckIfValueMatchesSyntax(configuration.ProposedValue))
				{
					configuration.ProposedValueInfo.AddError(Res.GetString(
						"44D0B9BF-2D7A-4B52-92CC-20A64A01C60D",
						"Entered value ({0}) does not match syntax requirement.",
						configuration.ProposedValue));
					return;
				}
			}

			public override Type AutoValidationType => typeof(DatabaseScopedConfiguration);
		}
	}
}
