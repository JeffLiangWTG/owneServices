using System;

namespace CargoWise.RefDbRepo.IHSReferenceData.Services
{
	public class DownloadException : Exception
	{
		public DownloadException()
		{
		}

		public DownloadException(Exception ex) : base(ex.Message, ex)
		{
		}

		public DownloadException(string message) : base(message)
		{ }

		public DownloadException(string message, Exception ex) : base(message, ex)
		{ }
	}
}
