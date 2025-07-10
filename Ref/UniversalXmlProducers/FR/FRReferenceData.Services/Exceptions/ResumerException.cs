using System;

namespace CargoWise.RefDbRepo.FRReferenceData.Services.Exceptions
{
	public class ResumerException : Exception
	{
		public ResumerException() : base()
		{
		}
		public ResumerException(string message) : base(message)
		{
		}

		public ResumerException(string message, Exception e) : base(message, e)
		{
		}
	}
}
