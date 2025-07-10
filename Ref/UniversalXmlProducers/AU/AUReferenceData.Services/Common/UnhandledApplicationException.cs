using System;

namespace CargoWise.RefDbRepo.AUReferenceData.Services
{
	public class UnhandledApplicationException : Exception
	{
		public UnhandledApplicationException()
		{
		}

		public UnhandledApplicationException(Exception ex) : base(ex.Message, ex)
		{
		}

		public UnhandledApplicationException(string message) : base(message)
		{ }

		public UnhandledApplicationException(string message, Exception ex) : base(message, ex)
		{ }
	}
}
