using System;
using System.Xml.Serialization;

namespace CargoWise.Billing.CollectorService.Plugin
{
#nullable disable
	[Serializable]
	public sealed class PluginControllerSettings : IEquatable<PluginControllerSettings>
	{
		public PluginControllerSettings()
		{
			SendBillingTransaction = true;
			SendUsageTransaction = false;
		}

		public PluginControllerSettings(PluginControllerSettings settings)
		{
			Key = settings.Key;
			Active = settings.Active;
			TypeName = settings.TypeName;
			IntervalMinutes = settings.IntervalMinutes;
			RetryIntervalSeconds = settings.RetryIntervalSeconds;
			MaxRetryAttempts = settings.MaxRetryAttempts;
			SendBillingTransaction = settings.SendBillingTransaction;
			SendUsageTransaction = settings.SendUsageTransaction;
			PluginSettings = settings.PluginSettings != null ? new PluginSettings(settings.PluginSettings) : null;
			CronSettings = settings.CronSettings != null ? new CronSettings(settings.CronSettings) : null;
			SchedulerType = settings.SchedulerType;
		}

		public string Key { get; set; }

		public bool Active { get; set; }

		public string TypeName { get; set; }

		public string SchedulerType { get; set; }

		public int IntervalMinutes { get; set; }

		public int RetryIntervalSeconds { get; set; }

		public int MaxRetryAttempts { get; set; }

		public bool SendBillingTransaction { get; set; }

		public bool SendUsageTransaction { get; set; }

		[XmlElement("Cron")]
		public CronSettings CronSettings { get; set; }

		[XmlElement("Settings")]
		public PluginSettings PluginSettings { get; set; }

		[XmlIgnore]
		public TimeSpan Interval
		{
			get { return TimeSpan.FromMinutes(IntervalMinutes); }
		}

		[XmlIgnore]
		public TimeSpan RetryInterval
		{
			get { return TimeSpan.FromSeconds(RetryIntervalSeconds); }
		}

		#region Equality

		public bool Equals(PluginControllerSettings other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return
				string.Equals(Key, other.Key) &&
				Active == other.Active &&
				string.Equals(TypeName, other.TypeName) &&
				IntervalMinutes == other.IntervalMinutes &&
				RetryIntervalSeconds == other.RetryIntervalSeconds &&
				MaxRetryAttempts == other.MaxRetryAttempts &&
				SendBillingTransaction == other.SendBillingTransaction &&
				SendUsageTransaction == other.SendUsageTransaction &&
				string.Equals(SchedulerType, other.SchedulerType) &&
				CronSettings.Equals(CronSettings, other.CronSettings) &&
				PluginSettings.Equals(PluginSettings, other.PluginSettings);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is PluginControllerSettings && Equals((PluginControllerSettings) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (Key != null ? Key.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ Active.GetHashCode();
				hashCode = (hashCode * 397) ^ (TypeName != null ? TypeName.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (PluginSettings != null ? PluginSettings.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ IntervalMinutes;
				hashCode = (hashCode * 397) ^ RetryIntervalSeconds;
				hashCode = (hashCode * 397) ^ MaxRetryAttempts;
				hashCode = (hashCode * 397) ^ SendBillingTransaction.GetHashCode();
				hashCode = (hashCode * 397) ^ SendUsageTransaction.GetHashCode();
				hashCode = (hashCode * 397) ^ (SchedulerType != null ? SchedulerType.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (CronSettings != null ? CronSettings.GetHashCode() : 0);
				return hashCode;
			}
		}

		#endregion // Equality
	}
}
