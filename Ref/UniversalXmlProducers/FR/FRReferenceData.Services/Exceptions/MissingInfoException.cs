using System;

namespace CargoWise.RefDbRepo.FRReferenceData.Services.Exceptions
{
	public class MissingInfoException : Exception
	{
		public MissingInfoException() : base()
		{
		}
		public MissingInfoException(string message) : base(message)
		{
		}

		public MissingInfoException(string message, Exception e) : base(message, e)
		{
		}
	}
}
