using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.CryptoUtilities;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Business.BatchProcessor
{
	public abstract class CertificateManager : Disposable
	{
		protected CertificateManager()
		{
			if (Registry.Business.SystemDataRegistry.Instance.RemoveCertificateFromUserStore.Value)
			{
				RemoveExistingCertificateFromStores();
			}
		}

		public string EncryptionCertificateName => CustomsCertificate.Name;

		public Certificate CustomsCertificate => customsCertificate ?? (customsCertificate = LoadCustomsCertificateFromDatabase());
		Certificate customsCertificate;

		public Store CompanyCertificate => companyCertificate ?? (companyCertificate = LoadCompanyCertificateFromDatabase());
		Store companyCertificate;

		public Certificate TrustPointCertificate => trustPointCertificate ?? (trustPointCertificate = LoadTrustPointCertificateFromDatabase());
		Certificate trustPointCertificate;

		public bool IsType3Certificate(string issuerName) => ValidType3CertificateIssuerNames.Contains(issuerName);

		public readonly IEnumerable<string> ValidType3CertificateIssuerNames = ImmutableArray.Create("Gatekeeper TYPE 3 CA", "Gatekeeper General Supplementary Device CA-G3", "DigiCert Gatekeeper Device Issuing CA");

		void RemoveExistingCertificateFromStores()
		{
			var store = new X509Store(StoreLocation.CurrentUser);
			store.Open(OpenFlags.ReadWrite);
			var otherCertificates = string.Empty;
			var certRemoved = false;
			foreach (var cert in store.Certificates)
			{
				var issuerName = GetIssuerName(cert);
				if (IsType3Certificate(issuerName))
				{
					store.Remove(cert);
					store.Close();
					certRemoved = true;
					break;
				}
				otherCertificates += "/" + issuerName;
			}
			store.Close();
			store = new X509Store(StoreLocation.LocalMachine);
			store.Open(OpenFlags.ReadWrite);
			foreach (var cert in store.Certificates)
			{
				var issuerName = GetIssuerName(cert);
				if (IsType3Certificate(issuerName))
				{
					store.Remove(cert);
					store.Close();
					certRemoved = true;
					break;
				}
				otherCertificates += "/" + issuerName;
			}
			store.Close();
			throw new System.Security.Cryptography.CryptographicException(
				(certRemoved ? "****Certificate has been removed" : "****No Certificate was found to remove. Other certificates: " + otherCertificates) + ". Reset registry now.");
		}

		protected virtual byte[] GetCompanyCertificateData() => null;

		protected virtual IRegistryItem GetRawCompanyCertificateData() => null;

		protected virtual string GetCompanyCertificatePassword() => null;

		protected virtual ZString CompanyCertificateCaption => RawCompanyCertificate?.Caption ?? "Company Certificate";

		protected virtual ZString CompanyCertificateLocation => ((IRegistryItemInternals)RawCompanyCertificate)?.Location ?? ZString.Empty;

		protected virtual byte[] GetCustomsCertificateData() => null;

		protected virtual ZString CustomsCertificateCaption => "Customs Certificate";

		protected virtual byte[] GetTrustPointCertificateData() => null;

		protected virtual ZString TrustPointCertificateCaption => "Trust Point Certificate";

		string GetIssuerName(X509Certificate2 cert) => cert.GetNameInfo(X509NameType.SimpleName, true);

		Certificate LoadCustomsCertificateFromDatabase()
		{
			Certificate customsCertificate = null;
			var customsCertificateData = GetCustomsCertificateData();
			if (customsCertificateData != null && customsCertificateData.Length > 0)
			{
				try
				{
					customsCertificate = new Certificate(customsCertificateData);
				}
				catch (CryptoUtilitiesException e)
				{
					NotifyError(CustomsCertificateCaption, ZString.Empty, e.Message);
				}
			}
			return customsCertificate;
		}

		Store LoadCompanyCertificateFromDatabase()
		{
			Store companyCertificate = null;
			var companyCertificatePassword = GetCompanyCertificatePassword();
			var companyCertificateData = GetCompanyCertificateData();
			if (companyCertificateData != null && companyCertificateData.Length > 0 && companyCertificatePassword != null && companyCertificatePassword.Length > 0)
			{
				try
				{
					companyCertificate = new Store(companyCertificateData, companyCertificatePassword, Registry.Business.SystemDataRegistry.Instance.StoreCertificatesUnderCurrentUser.Value);
				}
				catch (CryptoUtilitiesException e)
				{
					NotifyError(CompanyCertificateCaption, CompanyCertificateLocation, e.Message);
				}
			}
			return companyCertificate;
		}

		Certificate LoadTrustPointCertificateFromDatabase()
		{
			Certificate trustPointCertificate = null;
			var trustPointCertificateData = GetTrustPointCertificateData();
			if (trustPointCertificateData != null && trustPointCertificateData.Length > 0)
			{
				try
				{
					trustPointCertificate = new Certificate(trustPointCertificateData);
				}
				catch (CryptoUtilitiesException e)
				{
					NotifyError(TrustPointCertificateCaption, ZString.Empty, e.Message);
				}
			}
			return trustPointCertificate;
		}

		void NotifyError(ZString caption, ZString location, ZString error)
		{
			var message = $"There was an error loading the {caption}";
			if (!location.IsEmpty)
			{
				message += $" which can be found in the Registry at: {location}";
			}

			Globals.Message.ShowError($"{message}.  The error was: {error}");
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				customsCertificate?.Dispose();
				companyCertificate?.Dispose();
				trustPointCertificate?.Dispose();
				DecryptionCertificateStore?.Dispose();
			}
		}

		IRegistryItem RawCompanyCertificate => rawCompanyCertificate ?? (rawCompanyCertificate = GetRawCompanyCertificateData());
		IRegistryItem rawCompanyCertificate;

		Store DecryptionCertificateStore => CompanyCertificate;
	}
}
