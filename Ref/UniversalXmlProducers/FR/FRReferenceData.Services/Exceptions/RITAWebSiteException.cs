using System;

namespace CargoWise.RefDbRepo.FRReferenceData.Services.Exceptions
{
	public class RITAWebSiteException : Exception
	{
		public RITAWebSiteException() : base()
		{
		}
		public RITAWebSiteException(string message) : base(message)
		{
		}

		public RITAWebSiteException(string message, Exception e) : base(message, e)
		{
		}
	}
}
