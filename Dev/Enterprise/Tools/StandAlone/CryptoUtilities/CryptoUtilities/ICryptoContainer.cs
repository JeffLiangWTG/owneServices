using System;

namespace Enterprise.CryptoUtilities
{
	public interface ICryptoContainer
	{
		string Name { get; }
		string IssuerName { get; }
		string EmailAddress { get; }
		string SerialNumber { get; }
		DateTime ValidToDate { get; }
		DateTime ValidFromDate { get; }
		CertificateState GetCertificateState(DateTime dateTime);
	}
}
