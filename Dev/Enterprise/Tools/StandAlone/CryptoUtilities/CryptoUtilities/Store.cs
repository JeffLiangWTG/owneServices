using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace Enterprise.CryptoUtilities
{
	public sealed class Store : ICryptoContainer, IDisposable
	{
		#region ICryptoContainer

		/// <summary>
		/// Gets the serial number of the last certificate in the store
		/// </summary>
		public string SerialNumber
		{
			get { return Functions.GetNiceByteArrayOutput(Certificate.GetSerialNumber(), true); }
		}

		/// <summary>
		/// Gets the name of the last certificate in the store
		/// </summary>
		public string Name
		{
			get { return Certificate.GetNameInfo(X509NameType.SimpleName, false); }
		}

		public string IssuerName
		{
			get { return Certificate.GetNameInfo(X509NameType.SimpleName, true); }
		}

		/// <summary>
		/// Gets the email address of the last certificate in the store
		/// </summary>
		public string EmailAddress
		{
			get { return Certificate.GetNameInfo(X509NameType.EmailName, false); }
		}

		/// <summary>
		/// Gets the valid TO date of the last certificate in the store
		/// </summary>
		public DateTime ValidToDate
		{
			get { return Certificate.NotAfter; }
		}

		/// <summary>
		/// Gets the valid FROM date of the last certificate in the store
		/// </summary>
		public DateTime ValidFromDate
		{
			get { return Certificate.NotBefore; }
		}

		public CertificateState GetCertificateState(DateTime dateTime)
		{
			return new CertificateState(this, dateTime);
		}

		#endregion

		readonly byte[] certAsBytes;
		readonly string certPassword;

#if DEBUG
		internal
#endif
			X509Certificate2 lazyCertificate;

#if DEBUG
		internal
#endif
			X509Certificate2Collection lazyCertificateCollection;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "storeUnderCurrentUser")]
		public Store(string filename, string password, bool storeUnderCurrentUser) : this(filename, password)
		{
			// todo: This constructor was left for compatibility with existing code. It should be removed when all its usages are eliminated.
		}

		public Store(string filename, string password) : this(File.ReadAllBytes(filename), password)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "storeUnderCurrentUser")]
		public Store(byte[] keyFileBytes, string password, bool storeUnderCurrentUser) : this(keyFileBytes, password)
		{
			// todo: This constructor was left for compatibility with existing code. It should be removed when all its usages are eliminated.
		}

		public Store(byte[] keyFileBytes, string password)
		{
			if (keyFileBytes == null)
			{
				throw new ArgumentNullException(nameof(keyFileBytes));
			}

			if (keyFileBytes.Length == 0)
			{
				throw new ArgumentException("KeyFileByes length must be greater than 0", nameof(keyFileBytes));
			}

			certAsBytes = keyFileBytes;
			certPassword = password;
		}

		public void Dispose()
		{
			if (lazyCertificate != null)
			{
				lazyCertificate.Dispose();
			}

			if (lazyCertificateCollection != null)
			{
				foreach (var cert in lazyCertificateCollection)
				{
					cert.Dispose();
				}
			}
		}

		X509Certificate2 Certificate
		{
			get
			{
				if (lazyCertificate == null)
				{
					X509Certificate2 cert;
					try
					{
						cert = new X509Certificate2(certAsBytes, certPassword);
					}
					catch (CryptographicException ex)
					{
						throw CryptoUtilitiesException.InvalidPrivateKeyFile(ex.HResult);
					}

					if (!cert.HasPrivateKey)
					{
						throw CryptoUtilitiesException.InvalidPrivateKeyFile(0);
					}

					lazyCertificate = cert;
				}

				return lazyCertificate;
			}
		}

		X509Certificate2Collection CertificateCollection
		{
			get
			{
				if (lazyCertificateCollection == null)
				{
					X509Certificate2Collection collection;
					try
					{
						collection = new X509Certificate2Collection();
						collection.Import(certAsBytes, certPassword, X509KeyStorageFlags.DefaultKeySet);
					}
					catch (CryptographicException ex)
					{
						throw CryptoUtilitiesException.InvalidPrivateKeyFile(ex.HResult);
					}

					if (!collection.Cast<X509Certificate2>().Any(c => c.HasPrivateKey))
					{
						throw CryptoUtilitiesException.InvalidPrivateKeyFile(0);
					}

					lazyCertificateCollection = collection;
				}

				return lazyCertificateCollection;
			}
		}

		public string ClearSign(string stringToSign)
		{
			return Sign(stringToSign, true);
		}

		public string Sign(string stringToSign)
		{
			return Sign(stringToSign, false);
		}

		string Sign(string stringToSign, bool detachedSignature)
		{
			try
			{
				var signer = new CmsSigner(Certificate)
				{
					IncludeOption = X509IncludeOption.EndCertOnly
				};

				var message = Encoding.ASCII.GetBytes(stringToSign);
				var contentInfo = new ContentInfo(message);
				var signedCms = new SignedCms(contentInfo, detachedSignature);
				signedCms.ComputeSignature(signer);
				return Functions.ToBase64String(signedCms.Encode());
			}
			catch (CryptographicException ex)
			{
				throw new CryptoUtilitiesException(ex);
			}
		}

		public string Decrypt(string messageBase64, CancellationToken token = new CancellationToken()) 
		{
			try
			{
				token.ThrowIfCancellationRequested();
				EnvelopedCms envelopedCms = new EnvelopedCms();
				token.ThrowIfCancellationRequested();
				envelopedCms.Decode(Convert.FromBase64String(messageBase64));
				token.ThrowIfCancellationRequested();
				envelopedCms.Decrypt(CertificateCollection);
				token.ThrowIfCancellationRequested();
				return Encoding.ASCII.GetString(envelopedCms.ContentInfo.Content);
			}
			catch (CryptographicException ex)
			{
				throw new CryptoUtilitiesException(ex);
			}
			catch (OperationCanceledException)
			{
				throw;
			}
		}

		public string DecryptUsingWindowsCertificateStore(string message, CancellationToken token)
		{
			try
			{
				// todo: This method was left for compatibility with existing code. It should be removed when all its usages are eliminated.
				return Decrypt(message, token);
			}
			catch (OperationCanceledException)
			{

				throw;
			}
		}

		public string DecryptUsingEnterpriseCertificateStore(string message, CancellationToken token)
		{
			try
			{
				// todo: This method was left for compatibility with existing code. It should be removed when all its usages are eliminated.
				return Decrypt(message, token);

			}
			catch (OperationCanceledException)
			{

				throw;
			}
		}
	}
}
