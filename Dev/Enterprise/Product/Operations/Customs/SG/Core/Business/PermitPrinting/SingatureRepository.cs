using System;
using System.IO;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.Registry;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;
using FlexCel.Pdf;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.SG.V4.Business
{
	public class SingatureRepository
	{
		SingatureRepository()
		{
		}

		public static SingatureRepository Instance => instance ?? (instance = new SingatureRepository());

		[ThreadSafe]
		static SingatureRepository instance;

		public TPdfSignature GetGlobalPdfSignature(BusinessObjectFactory factory)
		{
			var now = ZDateTime.Today;

			return factory.GetCachedValue(GetCachedKey(now), () =>
			{
				TPdfSignature result = null;

				var data = SGCustomsDataRegistry.Instance.PermitPrintingCertificate.Value;

				if (data != null && data.Length > 0)
				{
					var password = SGCustomsDataRegistry.Instance.PermitPrintingCertificatePassword.Value;
					var certificate = new X509Certificate2(data, password, X509KeyStorageFlags.DefaultKeySet);

					if (now >= new ZDate(certificate.NotBefore) && new ZDate(certificate.NotAfter) >= now)
					{
						var signer = new CmsSigner(certificate);
						signer.IncludeOption = X509IncludeOption.EndCertOnly;

						result = new TPdfSignature(new TBuiltInSignerFactory(signer), certificate.Subject.Replace('.', '_'), DigitalSignatureField, BrandingFactory.Instance.CompanyBrandingName, BrandingFactory.Instance.CompanyBrandingName);
						result.AllowedChanges = TPdfAllowedChanges.None;
					}
					else
					{
						certificate.Dispose();
					}
				}

				return result;
			});
		}

		#region Default Values

		internal byte[] DefaultPermitPrintCertificateData
		{
			get
			{
				if (defaultPermitPrintCertificateData == null)
				{
					GetDefaultPermitPrintCertificateData();
				}

				return defaultPermitPrintCertificateData;
			}
		}
		byte[] defaultPermitPrintCertificateData;

		internal string DefaultPermitPrintCertificatePassword
		{
			get
			{
				if (string.IsNullOrWhiteSpace(defaultPermitPrintCertificatePassword))
				{
					GetDefaultPermitPrintCertificateData();
				}

				return defaultPermitPrintCertificatePassword;
			}
		}
		string defaultPermitPrintCertificatePassword;

		void GetDefaultPermitPrintCertificateData()
		{
			using (var stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.SG.V4.Business.PermitPrinting.SGPrintPermitSignCertData"))
			{
				using (var reader = new StreamReader(stream))
				{
					var innerData = reader.ReadToEnd().Split(new[] { '\r', '\n' }, 2, StringSplitOptions.RemoveEmptyEntries);

					var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();

					defaultPermitPrintCertificatePassword = encoder.Decrypt(innerData[0]);
					defaultPermitPrintCertificateData = Convert.FromBase64String(innerData[1]);
				}
			}
		}

		#endregion

		string GetCachedKey(ZDateTime date) => nameof(GetGlobalPdfSignature) + date;

		MultilingualString DigitalSignatureField => ResString.GetMultilingualString
		(
			"4E79C3CB-A61E-47EE-BF91-48B2AB3266D5",
			@"{0} (WTG) has provided the feature to generate and sign this PDF document using the data verified by the application users and available in the {1} at the time of signing. WTG or any of its employees shall not be held responsible for any discrepancies observed in the data contained in this document.",
			BrandingFactory.Instance.CompanyBrandingName,
			Core.Constants.ProductName
		);
	}
}
