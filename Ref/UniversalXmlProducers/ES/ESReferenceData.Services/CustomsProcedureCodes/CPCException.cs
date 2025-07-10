using System;

namespace CargoWise.RefDbRepo.ESReferenceData.Services
{
	public class CPCException : Exception
	{
		public CPCException()
		{
		}

		public CPCException(string message) : base(message)
		{
		}

		public CPCException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
