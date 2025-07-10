using System;

namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	public sealed class RefDataParseException : Exception
	{
		public RefDataParseException()
		{
		}

		public RefDataParseException(string message) : base(message)
		{
		}

		public RefDataParseException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
