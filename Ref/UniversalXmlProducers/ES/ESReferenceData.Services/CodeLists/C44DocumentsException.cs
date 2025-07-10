using System;

namespace CargoWise.RefDbRepo.ESReferenceData.Services
{
	public class C44DocumentsException : Exception
	{
		public C44DocumentsException()
		{
		}

		public C44DocumentsException(string message) : base(message)
		{
		}

		public C44DocumentsException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
