using System;

namespace CargoWise.RefDbRepo.SourceDataBinaryScanner
{
	public class NotProcessUntilException : Exception
	{
		public NotProcessUntilException() : base("The data is being processed by another program") { }

		public NotProcessUntilException(string message) : base(message) { }

		public NotProcessUntilException(string message, Exception innerException) : base(message, innerException) { }
	}
}
