using System;

namespace CargoWise.RefDbRepo.FRReferenceData.Services.Exceptions
{
	public class RITAWebServiceException : Exception
	{
		public RITAWebServiceException() : base()
		{
		}
		public RITAWebServiceException(string message) : base(message)
		{
		}

		public RITAWebServiceException(string message, Exception e) : base(message, e)
		{
		}
	}
}
