using System;

namespace CargoWise.RefDbRepo.ESReferenceData.Services
{
	public class LocationsProviderException : Exception
	{
		public LocationsProviderException()
		{
		}

		public LocationsProviderException(string message) : base(message)
		{
		}

		public LocationsProviderException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
