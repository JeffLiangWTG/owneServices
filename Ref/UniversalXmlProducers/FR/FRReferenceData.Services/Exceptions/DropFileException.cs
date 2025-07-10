using System;

namespace CargoWise.RefDbRepo.FRReferenceData.Services.Exceptions
{
	public class DropFileException : Exception
	{
		public DropFileException() : base()
		{
		}
		public DropFileException(string message) : base(message)
		{
		}

		public DropFileException(string message, Exception e) : base(message, e)
		{
		}
	}
}
