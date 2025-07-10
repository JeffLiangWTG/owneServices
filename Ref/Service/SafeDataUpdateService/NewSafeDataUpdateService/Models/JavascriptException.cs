using System;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public class JavascriptException : Exception
	{
		public JavascriptException() { }

		public JavascriptException(string message) : base(message)
		{
		}

		public JavascriptException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public JavascriptException(string message, string stackTrace, Exception innerException = null)
			: base(message, innerException)
		{
			StackTrace = stackTrace;
		}

		public override string StackTrace { get; }
	}
}
