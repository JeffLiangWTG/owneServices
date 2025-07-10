using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Enterprise.CryptoUtilities
{
	public sealed class Certificate : ICryptoContainer, IDisposable
	{
#if DEBUG
		internal
#endif
			readonly X509Certificate2 certificate;

		public Certificate(string filename) : this(File.ReadAllBytes(filename))
		{
		}

		public Certificate(byte[] data)
		{
			try
			{
				certificate = new X509Certificate2(data);
			}
			catch (CryptographicException ex)
			{
				throw CryptoUtilitiesException.InvalidCertificateFile(ex.HResult);
			}
		}

		public void Dispose()
		{
			certificate.Dispose();
		}

		public static void VerifyCertificateContextFromBytes(byte[] data)
		{
			try
			{
				new X509Certificate2(data).Dispose();
			}
			catch (CryptographicException ex)
			{
				throw CryptoUtilitiesException.InvalidCertificateFile(ex.HResult);
			}
		}

		#region ICryptoContainer

		string fName;

		public string Name
		{
			get
			{
				if (fName == null)
				{
					fName = certificate.GetNameInfo(X509NameType.SimpleName, false);
				}

				return fName;
			}
		}

		string fIssuerName;

		public string IssuerName
		{
			get
			{
				if (fIssuerName == null)
				{
					fIssuerName = certificate.GetNameInfo(X509NameType.SimpleName, true);
				}

				return fIssuerName;
			}
		}

		string fSerialNumber;

		public string SerialNumber
		{
			get
			{
				if (fSerialNumber == null)
				{
					fSerialNumber = Functions.GetNiceByteArrayOutput(certificate.GetSerialNumber(), true);
				}

				return fSerialNumber;
			}
		}

		DateTime fValidFromDate = DateTime.MinValue;

		public DateTime ValidFromDate
		{
			get
			{
				if (fValidFromDate == DateTime.MinValue)
				{
					fValidFromDate = certificate.NotBefore;
				}

				return fValidFromDate;
			}
		}

		DateTime fValidToDate = DateTime.MinValue;

		public DateTime ValidToDate
		{
			get
			{
				if (fValidToDate == DateTime.MinValue)
				{
					fValidToDate = certificate.NotAfter;
				}

				return fValidToDate;
			}
		}

		string fEmailAddress;

		public string EmailAddress
		{
			get
			{
				if (fEmailAddress == null)
				{
					fEmailAddress = certificate.GetNameInfo(X509NameType.EmailName, false);
				}

				return fEmailAddress;
			}
		}

		public CertificateState GetCertificateState(DateTime dateTime)
		{
			return new CertificateState(this, dateTime);
		}

		#endregion

		public VerifiedMessage VerifyBytes(string text, string expectedSigner)
		{
			SignedCms signedCms = new SignedCms();

			try
			{
				signedCms.Decode(Convert.FromBase64String(text));
				signedCms.CheckSignature(true);
			}
			catch (CryptographicException ex)
			{
				throw new CryptoUtilitiesException(ex);
			}

			if (signedCms.SignerInfos.Count != 1)
			{
				throw new CryptoUtilitiesException(FormattableString.Invariant($"Unexpected number of signers: {signedCms.SignerInfos.Count}."), 0);
			}

			var signerCertificate = signedCms.SignerInfos[0].Certificate;
			if (!signerCertificate.IsIssuedBy(certificate))
			{
				throw new CryptoUtilitiesException("The documents signing certificate is not trusted by the supplied trust point certificate.", 0);
			}

			string signerName = signerCertificate.GetNameInfo(X509NameType.SimpleName, false);
			if (expectedSigner != null && signerName != expectedSigner)
			{
				throw new CryptoUtilitiesException(FormattableString.Invariant($"The documents signer was not the expected signer. Expected Signer = '{expectedSigner}', Actual Signer = '{signerName}'."), 0);
			}

			return new VerifiedMessage(Encoding.ASCII.GetString(signedCms.ContentInfo.Content), signerCertificate);
		}

		public string Encrypt(string message)
		{
			try
			{
				byte[] messageBytes = Encoding.ASCII.GetBytes(message);
				EnvelopedCms envelopedCms = new EnvelopedCms(new ContentInfo(messageBytes));
				envelopedCms.Encrypt(new CmsRecipient(certificate));
				return Functions.ToBase64String(envelopedCms.Encode());
			}
			catch (CryptographicException ex)
			{
				throw new CryptoUtilitiesException(ex);
			}
		}

		public class VerifiedMessage
		{
			public VerifiedMessage(string content, X509Certificate2 signingCertificate)
			{
				Content = content;
				SigningCertificate = signingCertificate;
			}

			public string Content { get; }

			public X509Certificate2 SigningCertificate { get; }
		}
	}
}
