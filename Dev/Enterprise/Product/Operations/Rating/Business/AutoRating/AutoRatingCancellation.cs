namespace Enterprise.Rating.Business
{
	public class AutoRatingCancellation
	{
		public enum Reasons
		{
			RatesSecurity,
			AdditionalRatesPrompt,
			ContractNumbersPrompt,
			CompanyTariffRateSelectionPrompt,
			/// <summary>
			/// Undefined should not be used. It's here as a placeholder
			/// until we can migrate to all using these reasons.
			/// </summary>
			Undefined
		}

		public AutoRatingCancellation(Reasons reason, string message)
		{
			this.reason = reason;
			this.message = message;
		}

		/// <summary>
		/// Undefined should not be used. It's here as a placeholder
		/// until we can migrate to all using the other reasons on the Enum
		/// </summary>
		public static AutoRatingCancellation UndefinedReason
			=> new AutoRatingCancellation(Reasons.Undefined, string.Empty);

		public Reasons Reason
		{
			get { return reason; }
		}
		readonly Reasons reason;

		//TODO: Not sure if this is used. If not, it should be removed
		public string Message
		{
			get { return message; }
			set { message = value; }
		}
		string message;
	}
}
