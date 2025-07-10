using System;

namespace CargoWise.RefDbRepo.IEReferenceData.ExchangeRates.Services
{
	public class ExchangeRatesException : Exception
	{
		public ExchangeRatesException(string message) : base(message)
		{
		}

		public ExchangeRatesException()
		{
		}

		public ExchangeRatesException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
