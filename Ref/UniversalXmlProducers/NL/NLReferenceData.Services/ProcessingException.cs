using System;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public class ProcessingException : Exception
	{
		public ProcessingException()
		{
		}

		public ProcessingException(string message) : base(message)
		{
		}

		public ProcessingException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
