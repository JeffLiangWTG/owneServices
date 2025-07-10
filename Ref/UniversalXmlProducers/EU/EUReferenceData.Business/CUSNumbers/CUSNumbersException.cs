using System;

namespace CargoWise.RefDbRepo.EUReferenceData.CUSNumbers.Business
{
	public class CUSNumbersException : Exception
	{
		public CUSNumbersException(string message) : base(message)
		{
		}

		public CUSNumbersException() : base()
		{
		}

		public CUSNumbersException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
