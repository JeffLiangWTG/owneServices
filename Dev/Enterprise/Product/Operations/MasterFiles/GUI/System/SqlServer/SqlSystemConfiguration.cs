using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using static System.FormattableString;

namespace Enterprise.MasterFiles.GUI
{
	public class SqlSystemConfiguration : NonPersistentBusinessObject, INotifyPropertyChanged
	{
		public SqlSystemConfiguration()
		{
		}

		public SqlSystemConfiguration(ZInt id)
		{
			ConfigurationId = id;
		}

		public ZInt ConfigurationId { get; set; }

		public override bool ReadOnly => ProposedValueIsReadOnly;

		public virtual ZInt ProposedValue
		{
			get => proposedValue;
			set
			{
				if (proposedValue != value)
				{
					proposedValue = value;
					ProposedValueInfo.RefreshBinding();
					OnPropertyChanged();
				}
			}
		}

		public virtual ZString Name { get; set; }

		public virtual ZString ItemDescription { get; set; }

		public virtual ZInt ValueInUse { get; set; }

		public virtual ZInt ConfiguredValue
		{
			get => configuredValue;
			set
			{
				configuredValue = value;
				ConfiguredValueInfo.RefreshBinding();

				IsDynamicInfo.RefreshBinding();
			}
		}

		public virtual ZInt MinValue { get; set; }

		public virtual ZInt MaxValue { get; set; }

		public virtual ZBool IsDynamic
		{
			get => isDynamic;
			set
			{
				isDynamic = value;

				IsDynamicInfo.RefreshBinding();
			}
		}

		public virtual ZString RequiresRestart => IsDynamic ? string.Empty : Res.GetString("90AA5CA7-2DEE-458B-AEB3-C6036AB66E5B", "yes");

		public virtual ZBool IsAdvanced { get; set; }

		public virtual ZBool HasRecommendedValue { get; set; }

		public virtual ZString RecommendedValue
		{
			get => recommendedValue;
			set
			{
				recommendedValue = value;
				HasRecommendedValue = true;
			}
		}

		[ReadOnlyMember(nameof(ProposedValueIsReadOnly))]
		public virtual ZString ProposedValueText
		{
			get => proposedValueText;
			set
			{
				proposedValueText = value;
				Validation.ValidateProposedStringValue();

				if (!ProposedValueTextInfo.HasNotifications() && int.TryParse(value, out var proposed))
				{
					ProposedValue = proposed;
				}
				else
				{
					ProposedValue = ConfiguredValue;
				}

				ProposedValueTextInfo.RefreshBinding();
				IsDynamicInfo.RefreshBinding();
			}
		}

		public virtual bool ProposedValueIsReadOnly
		{
			get
			{
				return
					SqlSystemConfigurationsHelper.Instance.BlockedConfigurationIds.Contains(ConfigurationId)
					|| SqlSystemConfigurationsHelper.DenyUserEdit;
			}
		}

		public virtual ZPropertyInfo ConfiguredValueInfo
		{
			get => GetZPropertyInfo(nameof(ConfiguredValue));
		}

		public virtual ZPropertyInfo IsDynamicInfo
		{
			get => GetZPropertyInfo(nameof(IsDynamic));
		}

		public virtual ZPropertyInfo ProposedValueInfo
		{
			get => GetZPropertyInfo(nameof(ProposedValue));
		}

		public virtual ZPropertyInfo ProposedValueTextInfo
		{
			get => GetZPropertyInfo(nameof(ProposedValueText));
		}

		public virtual ZBool HasProposedChange
		{
			get => !string.IsNullOrWhiteSpace(ProposedValueText) && ProposedValue != ConfiguredValue;
		}

		public PersistedSqlConfiguration PersistedConfig
		{
			get => new PersistedSqlConfiguration { ConfigurationId = ConfigurationId, ProposedValue = ProposedValue };
		}

		public SqlSystemConfigurationValidations Validation
		{
			get => new SqlSystemConfigurationValidations(this);
		}

		public void SetPersistedConfig(PersistedSqlConfiguration config)
		{
			if (ConfigurationId != config.ConfigurationId)
			{
				throw new ArgumentException(null, nameof(config));
			}

			if (config.ProposedValue != ConfiguredValue)
			{
				ProposedValueText = config.ProposedValue.ToString();
			}
		}

		void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public override string ToString()
		{
			return Invariant($"Set configuration id: {ConfigurationId}, {Name}, to new value=({ProposedValue}), current value=({ConfiguredValue}) -- {EffectiveHint}");
		}

		public ZString EffectiveHint => IsDynamic ? takeEffectImmediately : restartRequired;

		readonly ZString takeEffectImmediately = Res.GetString("C343D3F8-B384-4670-95C8-1634E3A94A70", "the new value will take effect immediately once applied");
		readonly ZString restartRequired = Res.GetString("1CED5308-B371-49FD-AEAE-C4E31CF8AA96", "changed configuration value will not take effect until applied and the server is restarted");

		ZString proposedValueText;
		ZInt proposedValue;
		ZInt configuredValue;
		ZString recommendedValue;
		ZBool isDynamic;
	}
}
