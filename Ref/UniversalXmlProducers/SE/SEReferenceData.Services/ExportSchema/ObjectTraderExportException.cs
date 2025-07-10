using System;

namespace CargoWise.RefDbRepo.SEReferenceData.Services
{
	public class ObjectTraderExportException : Exception
	{
		public ObjectTraderExportException()
		{
		}

		public ObjectTraderExportException(string message) : base(message)
		{
		}

		public ObjectTraderExportException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
