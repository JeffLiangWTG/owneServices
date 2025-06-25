using System;

namespace CargoWise.Billing.CollectorService.Plugin
{
#nullable disable
	[Serializable]
	public sealed class PluginControllerState : IEquatable<PluginControllerState>
	{
		public PluginControllerState()
		{
		}

		public PluginControllerState(IClock clock)
		{
			LastSuccessfulRun = clock.UtcNow;
			LastTransactionTimestamp = clock.UtcNow;
		}

		public PluginControllerState(PluginControllerState state)
		{
			Key = state.Key;
			LastSuccessfulRun = state.LastSuccessfulRun;
			LastTransactionTimestamp = state.LastTransactionTimestamp;
		}

		public string Key { get; set; }

		public DateTime LastSuccessfulRun { get; set; }

		public DateTime LastTransactionTimestamp { get; set; }

		#region Equality

		public bool Equals(PluginControllerState other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return
				string.Equals(Key, other.Key) &&
				LastSuccessfulRun.Equals(other.LastSuccessfulRun) &&
				LastTransactionTimestamp.Equals(other.LastTransactionTimestamp);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is PluginControllerSettings && Equals((PluginControllerState)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (Key != null ? Key.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ LastSuccessfulRun.GetHashCode();
				hashCode = (hashCode * 397) ^ LastTransactionTimestamp.GetHashCode();
				return hashCode;
			}
		}

		#endregion // Equality
	}
}
