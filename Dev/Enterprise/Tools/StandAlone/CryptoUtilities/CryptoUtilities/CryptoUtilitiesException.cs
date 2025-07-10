using System;
using System.Security.Cryptography;

namespace Enterprise.CryptoUtilities
{
	[Serializable]
	public class CryptoUtilitiesException : ApplicationException
	{
		const int STATUS_INVALID_SIGNATURE = unchecked((int)0xC000A000);
		const int ERROR_INVALID_PASSWORD = unchecked((int)0x80070056);
		const int NTE_TEMPORARY_PROFILE = unchecked((int)0x80090024);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "exception message")]
		public CryptoUtilitiesException(string message, int win32ErrorNumber) : base(message + " Error Number: " + win32ErrorNumber)
		{
			Win32ErrorNumber = win32ErrorNumber;
		}

		public CryptoUtilitiesException(CryptographicException ex) : base(FormatMessage(ex), ex)
		{
			Win32ErrorNumber = ex.HResult;
		}

#if NETFRAMEWORK
		protected CryptoUtilitiesException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public int Win32ErrorNumber { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "exception message")]
		static string FormatMessage(CryptographicException ex)
		{
			if (TryGetMessageFromHResult(ex.HResult, out var message))
			{
				return message;
			}

			return "CryptoUtility Exception. " + ex.Message;
		}

		public static CryptoUtilitiesException InvalidPrivateKeyFile(int hResult)
		{
			if (TryGetMessageFromHResult(hResult, out var message))
			{
				return new CryptoUtilitiesException(message, hResult);
			}

			return new CryptoUtilitiesException("The supplied file is not a valid private key file.", hResult);
		}

		public static CryptoUtilitiesException InvalidCertificateFile(int hResult)
		{
			return new CryptoUtilitiesException
			(
				"The supplied file does not appear to be a valid certificate. The certificate must be in 'DER encoded binary X.509 (.CER)' format.\r\n" +
				"Try importing it into Internet Explorer and then exporting as 'DER encoded binary X.509 (.CER)'.",
				hResult
			);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "exception message")]
		static bool TryGetMessageFromHResult(int hResult, out string message)
		{
			if (hResult == STATUS_INVALID_SIGNATURE)
			{
				message = "The cryptographic signature is invalid.";
				return true;
			}

			if (hResult == ERROR_INVALID_PASSWORD)
			{
				message = $@"The supplied password for your private key file is incorrect or the certificate is not compatible with the current operating system - ({Environment.OSVersion}).";
				return true;
			}

			if (hResult == NTE_TEMPORARY_PROFILE)
			{
				message = "The profile for the user is a temporary profile. Please check service task account profile.";
				return true;
			}

			message = null;
			return false;
		}
	}
}
