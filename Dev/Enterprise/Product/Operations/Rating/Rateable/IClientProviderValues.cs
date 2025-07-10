namespace Enterprise.Rating.Rateable
{
	/// <summary>
	/// Interface for the value of a MeasureType quantity that can have different numbers for client (revenue) and provider (cost).
	/// For example, a Weight quantity, where the client value is the documented value and the provider value is the manifested value.
	/// </summary>
	public interface IClientProviderValues
	{
		/// <summary>
		/// Actual value, as opposed to the specific values for client and provider.
		/// </summary>
		decimal Actual { get; }

		/// <summary>
		/// Client value is used when rating revenue and only for weight, volume, chargeable and loading meters.
		/// On the job it is called the "documented" value.
		/// </summary>
		decimal ForClient { get; }

		/// <summary>
		/// Provider value is used when rating costs and only for weight, volume, chargeable and loading meters
		/// On the job it is called the "manifested" value.
		/// </summary>
		decimal ForProvider { get; }
	}
}
