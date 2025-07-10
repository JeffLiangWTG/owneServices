using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.SG.Registry;
using FlexCel.Pdf;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	sealed class SingatureRepositoryTest : TestCaseWithFactory
	{
		[TestDate(2021, 04, 01)]
		public void TestGetGlobalPdfSignature_EmptyData()
		{
			var signature = GetGlobalPdfSignature(Array.Empty<byte>());
			AssertNull("Should be null when there is no data in PermitPrintingCertificate.", signature);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2079, 06, 06)]
		public void TestGetGlobalPdfSignature_ExpiredData()
		{
			var data = GetTestSGPrintPermitSignData();
			var signature = GetGlobalPdfSignature(data);
			AssertNull("Should be null when the certificate is expired.", signature);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2021, 04, 10)]
		public void TestGetGlobalPdfSignature_NormalData()
		{
			var expectedReason = $@"{BrandingFactory.Instance.CompanyBrandingName} (WTG) has provided the feature to generate and sign this PDF document using the data verified by the application users and available in the {Core.Constants.ProductName} at the time of signing. WTG or any of its employees shall not be held responsible for any discrepancies observed in the data contained in this document.";
			var data = GetTestSGPrintPermitSignData();
			var signature = GetGlobalPdfSignature(data);
			CombineAssertions(() =>
			{
				AssertEquals("Name", "CN=SG Customs CargoWise, O=WiseTech Global Ltd, L=Singapore, S=Singapore, C=SG", signature.Name);
				AssertEquals("Reason", expectedReason, signature.Reason);
				AssertEquals("Location", BrandingFactory.Instance.CompanyBrandingName, signature.Location);
				AssertEquals("ContactInfo", BrandingFactory.Instance.CompanyBrandingName, signature.ContactInfo);
				AssertEquals("Should not allow any changes.", TPdfAllowedChanges.None, signature.AllowedChanges);
			}

			);
		}

		public void TestIsDefaultCertificateExpired()
		{
			var certificate = new X509Certificate2(SingatureRepository.Instance.DefaultPermitPrintCertificateData, SingatureRepository.Instance.DefaultPermitPrintCertificatePassword, X509KeyStorageFlags.DefaultKeySet);
			var now = DateTime.Now.AddDays(60);
			CombineAssertions(() =>
			{
				var message = $@"The current date range of DefaultPermitPrintCertificateData is from {certificate.NotBefore} to {certificate.NotAfter}.
Please update SGPrintPermitSignData for make sure the certificate is not expired.";
				AssertGreaterThanOrEqualTo(message, now, certificate.NotBefore);
				AssertLessThanOrEqualTo(message, now, certificate.NotAfter);
			}

			);
		}

		byte[] GetTestSGPrintPermitSignData()
		{
			return File.ReadAllBytes(BaseSourcePath + @"Enterprise\Product\Operations\Customs\SG\Core\Business\PermitPrinting\TestFiles\TestSGPrintPermitSignData");
		}

		TPdfSignature GetGlobalPdfSignature(byte[] data)
		{
			using (SGCustomsDataRegistry.Instance.PermitPrintingCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, data))
			using (SGCustomsDataRegistry.Instance.PermitPrintingCertificatePassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "123"))
			{
				return SingatureRepository.Instance.GetGlobalPdfSignature(new BusinessObjectFactory());
			}
		}
	}
}
