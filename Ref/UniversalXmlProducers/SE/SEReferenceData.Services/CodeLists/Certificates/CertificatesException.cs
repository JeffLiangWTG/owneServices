using System;
namespace CargoWise.RefDbRepo.SEReferenceData.Certificates.Services
{
	public class CertificatesException : Exception
	{
		public CertificatesException()
		{
		}

		public CertificatesException(string message) : base(message)
		{
		}

		public CertificatesException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
