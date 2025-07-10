namespace Enterprise.Freight.Forwarding.Routing.S8.Business
{
	using System;
	using CargoWise.Common;

	public class MethodCallResult<T>
	{
		public MethodCallResult(T result)
			: this(result, string.Empty)
		{
		}

		public MethodCallResult(Exception ex)
			: this(ex.Message, true)
		{
			this.StackTrace = ex.ToString();
		}

		public MethodCallResult(string errorMessage, bool shouldBeReported = true)
			: this(default, errorMessage)
		{
			Argument.NotNullOrEmpty(ErrorMessage, nameof(ErrorMessage));
			ShouldBeReported = shouldBeReported;
		}

		public MethodCallResult(T result, string errorMessage)
		{
			this.result = result;
			this.errorMessage = errorMessage;
		}

		public T Result
		{
			get
			{
				if (!Succeeded)
				{
					throw new InvalidOperationException("Do not try to get Result if it failed."); // Developer Only
				}

				return result;
			}
		}
		readonly T result;

		public string ErrorMessage
		{
			get
			{
				if (Succeeded)
				{
					throw new InvalidOperationException("Do not try to get ErrorMessage if it succeeded."); // Developer Only
				}

				return errorMessage;
			}
		}
		readonly string errorMessage;

		public bool Succeeded => string.IsNullOrWhiteSpace(errorMessage);

		public bool ShouldBeReported { get; private set; }

		public bool WasOperationCancelledByUser { get; set; }

		public string StackTrace { get; set; } = string.Empty;
	}
}
