using System;

namespace CargoWise.RefDbRepo.ESReferenceData.Services
{
	public class ExchangeRatesException : Exception
	{
		public ExchangeRatesException()
		{
		}

		public ExchangeRatesException(string message) : base(message)
		{
		}

		public ExchangeRatesException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
