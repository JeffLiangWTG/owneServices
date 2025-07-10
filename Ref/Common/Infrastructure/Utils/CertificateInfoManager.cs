using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using WTG.IdentitySecurity;

namespace CargoWise.RefDbRepo.Common.Utils;

public class CertificateInfoManager : ICertificateInfoManager
{
	internal CertificateInfoManager(string privateKeyName, string certificateFileName)
	{
		LoadPrivateKey(privateKeyName);
		LoadCertificate(certificateFileName);
	}

	void LoadPrivateKey(string privateKeyName)
	{
		var privateKeyPath = Path.Combine(DefaultSecretFolder, privateKeyName);
		if (File.Exists(privateKeyPath))
		{
			privateKey = RSAKeyProvider.ImportPrivateKey(File.ReadAllText(privateKeyPath));
		}
	}

	void LoadCertificate(string certificateFileName)
	{
		certificatePath = Path.Combine(DefaultSecretFolder, certificateFileName);
		if (File.Exists(certificatePath))
		{
			certificate = new X509Certificate2(certificatePath);
		}

		newCertificatePath = $"{certificatePath}_new";
	}

#if DEBUG
	public CertificateInfoManager(RSA privateKey, X509Certificate2 certificate, string certificatePath = "", ICertificateRenewal certificateRenewal = null)
	{
		this.privateKey = privateKey;
		this.certificate = certificate;
		this.certificatePath = certificatePath;
		this.certificateRenewal = certificateRenewal;

		newCertificatePath = $"{certificatePath}_new";
	}
#endif

	public RSA GetPrivateKey() => privateKey;

	public X509Certificate2 GetCertificate() => certificate;

	public bool IsCertNearToExpire()
	{
		if (certificate == null)
		{
			throw new InvalidOperationException("The Certificate does not exist, please check it.");
		}

		var certExpirationTime = certificate.NotAfter.ToUniversalTime();
		if (certExpirationTime < DateTime.UtcNow)
		{
			throw new InvalidOperationException("The Certificate expired, please check it.");
		}

		var daysToExpiration = (int)(certExpirationTime - DateTime.UtcNow).TotalDays;
		if (daysToExpiration <= CertExpirationWarningDays)
		{
			PrintErrorMessageOfCertificateExpiration();
			return true;
		}

		return false;

		void PrintErrorMessageOfCertificateExpiration()
		{
			if (daysToExpiration <= CertExpirationWarningDays / 3)
			{
				Console.Error.WriteLine($"The Certificate will expire in {daysToExpiration} days.");
			}
		}
	}

	public async Task RenewalCertificateAsync(string clientIdentifier, string accessToken)
	{
		var fileLockerName = $"{clientIdentifier}.lock";
		var lockFilePath = Path.Combine(Path.GetTempPath(), fileLockerName);
		await FileLocker.RunWithLockAsync(lockFilePath, async () =>
		{
			if (IsNewCertExist())
			{
				ActivateNewCertificate();
			}
			else
			{
				await GenerateNewCertificate(clientIdentifier, accessToken);
			}
		});
	}

	bool IsNewCertExist()
	{
		return File.Exists(newCertificatePath);
	}

	void ActivateNewCertificate()
	{
		if (!IsNewCertificateValid())
		{
			return;
		}
		try
		{
			var oldCertificatePath = $"{certificatePath}_old_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
			File.Move(certificatePath, oldCertificatePath);
			File.Move(newCertificatePath, certificatePath);

			Console.WriteLine("Activate New Certificate succeeded.");
		}
		catch (IOException ex)
		{
			Console.WriteLine($"Activate New Certificate failed: {ex.Message}");
		}

		return;

		bool IsNewCertificateValid()
		{
			return IsNewCertExist() && File.GetLastWriteTimeUtc(newCertificatePath).AddDays(1) < DateTime.UtcNow;
		}
	}

	async Task GenerateNewCertificate(string clientIdentifier, string accessToken)
	{
		var csrFilePath = Path.ChangeExtension(certificatePath, "csr");
		var fileReader = new FileReader(csrFilePath);
		certificateRenewal ??= new CertificateRenewal(clientIdentifier, accessToken, fileReader);

		var content = await certificateRenewal.GetNewCertificateContentAsync();
		Console.WriteLine("Get Certificate Content succeeded.");
		await WriteToCertificateWithRetry(content);

		Console.WriteLine("Generate New Certificate succeeded.");
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
	async Task WriteToCertificateWithRetry(byte[] content)
	{
		var retries = 0;
		while (retries < 3)
		{
			try
			{
				await File.WriteAllBytesAsync(newCertificatePath, content);
				Console.WriteLine("Write to Certificate succeeded.");
				break;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Write to Certificate failed: {ex.Message}");
				retries++;
				await Task.Delay(TimeSpan.FromSeconds(1));
			}
		}
	}

	string certificatePath;
	string newCertificatePath;
	RSA privateKey;
	X509Certificate2 certificate;
	ICertificateRenewal certificateRenewal;

	const int CertExpirationWarningDays = 90;

	static readonly string DefaultSecretFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..\Secret");

	public void Dispose()
	{
		privateKey?.Dispose();
		certificate?.Dispose();
	}
}
