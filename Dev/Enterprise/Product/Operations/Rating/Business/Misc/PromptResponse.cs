namespace Enterprise.Rating.Business
{
	/// <summary>
	/// I think it is okay to use multiple return values if you're doing it within
	/// a limited scope. But if you start to cross classes and interfaces then
	/// I feel this is neater to use instead.
	/// </summary>
	public class PromptResponse<TValue, TValidity>
	{
		/// <summary>
		/// Creates a VALID response with the given value.
		/// </summary>
		public PromptResponse(TValue value, TValidity validity)
		{
			Response = value;
			Validity = validity;
		}

		protected PromptResponse()
		{
			Validity = default;
		}

		/// <summary>
		/// Returns an invalid response
		/// </summary>
		public static PromptResponse<TValue, TValidity> Invalid => new PromptResponse<TValue, TValidity>();

		/// <summary>
		/// The response, if it is valid
		/// </summary>
		public TValue Response { get; }

		/// <summary>
		/// A non-valid response would be, for example, when a form asked
		/// Yes, No, Cancel.
		/// Cancel would be invalid
		/// </summary>
		public TValidity Validity { get; }
	}

	public class PromptResponse<TValue> : PromptResponse<TValue, bool>
	{
		public PromptResponse(TValue value) : base(value, true)
		{
		}

		protected PromptResponse() : base()
		{
		}

		public new static PromptResponse<TValue> Invalid => new PromptResponse<TValue>();
	}
}
