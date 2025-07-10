using System;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Common
{
	public class ReferenceDataException : Exception
	{
		public ReferenceDataException()
		{
		}

		public ReferenceDataException(string message) : base(message)
		{
		}

		public ReferenceDataException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
