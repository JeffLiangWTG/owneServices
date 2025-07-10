using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Utils.Test
{
	[TestFixture]
	class AccessTokenProviderFixture
	{
		[Test]
		public void GetAccessToken_WithValidParameters_ReturnsAccessToken()
		{
			var mockCertificateInfoManager = new Mock<ICertificateInfoManager>();
			var mockRSA = new Mock<RSA>().Object;
			mockCertificateInfoManager.Setup(x => x.GetPrivateKey()).Returns(mockRSA);
			var mockCertificate = new Mock<X509Certificate2>().Object;
			mockCertificateInfoManager.Setup(x => x.GetCertificate()).Returns(mockCertificate);
			var accessToken = AccessTokenGenerator.GenerateS2SAccessToken("8152BA52-8BCE-4255-8234-B8DF56E09E64", DateTime.Now);
			var mockOAuthClientWrapper = new Mock<IOAuthClientWrapper>();
			mockOAuthClientWrapper.Setup(x =>
					x.GetClientAccessTokenAsync("https://login.microsoftonline.com/valid_tenant_id/oauth2/v2.0/token", mockRSA, mockCertificate, "8152BA52-8BCE-4255-8234-B8DF56E09E64", "valid_service_id"))
				.ReturnsAsync(accessToken);
			using (var tokenProvider = new AccessTokenProvider("valid_tenant_id",
						"8152BA52-8BCE-4255-8234-B8DF56E09E64", "valid_service_id", mockCertificateInfoManager.Object,
						mockOAuthClientWrapper.Object))
			{
				var actualAccessToken = tokenProvider.GetAccessToken();
				Assert.That(actualAccessToken.Equals(accessToken, StringComparison.OrdinalIgnoreCase));
			}
		}

		[Test]
		public void GetAccessToken_WithExpiredToken_RequestNewAccessToken()
		{
			var mockCertificateInfoManager = new Mock<ICertificateInfoManager>();
			var mockRSA = new Mock<RSA>().Object;
			mockCertificateInfoManager.Setup(x => x.GetPrivateKey()).Returns(mockRSA);
			var mockCertificate = new Mock<X509Certificate2>().Object;
			mockCertificateInfoManager.Setup(x => x.GetCertificate()).Returns(mockCertificate);
			var mockOAuthClientWrapper = new Mock<IOAuthClientWrapper>();
			mockOAuthClientWrapper.Setup(x =>
					x.GetClientAccessTokenAsync("https://login.microsoftonline.com/valid_tenant_id/oauth2/v2.0/token", mockRSA, mockCertificate, "2033F807-558E-48FA-9814-83CD11310C8D", "valid_service_id"))
				.ReturnsAsync(AccessTokenGenerator.GenerateS2SAccessToken("2033F807-558E-48FA-9814-83CD11310C8D", DateTime.Now.AddMonths(-1)));
			using (var tokenProvider = new AccessTokenProvider("valid_tenant_id", "2033F807-558E-48FA-9814-83CD11310C8D", "valid_service_id",
						mockCertificateInfoManager.Object, mockOAuthClientWrapper.Object))
			{
				tokenProvider.GetAccessToken();
				tokenProvider.GetAccessToken();
				mockCertificateInfoManager.Verify(x => x.GetCertificate(), Times.Exactly(2));
				mockCertificateInfoManager.Verify(x => x.IsCertNearToExpire(), Times.Exactly(2));
				mockCertificateInfoManager.Verify(x => x.GetPrivateKey(), Times.Exactly(2));
			}
		}

		[Test]
		public void GetAccessToken_DoesNotThrowException_RequestAccessTokenMultipleTimes()
		{
			var mockRSA = new Mock<RSA>().Object;
			var testCertificatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestCertificate.cer");
			using (var certificate = new X509Certificate2(testCertificatePath))
			using (var certificateManager = new CertificateInfoManager(mockRSA, certificate))
			{
				var mockOAuthClientWrapper = new Mock<IOAuthClientWrapper>();
				mockOAuthClientWrapper.Setup(x =>
						x.GetClientAccessTokenAsync("https://login.microsoftonline.com/valid_tenant_id/oauth2/v2.0/token", mockRSA, certificate, "9ECEDA61-66D8-4C8E-BAEE-238659A25969", "valid_service_id"))
					.ReturnsAsync(AccessTokenGenerator.GenerateS2SAccessToken("9ECEDA61-66D8-4C8E-BAEE-238659A25969", DateTime.Now));
				using (var tokenProvider = new AccessTokenProvider("valid_tenant_id",
							"9ECEDA61-66D8-4C8E-BAEE-238659A25969", "valid_service_id", certificateManager,
							mockOAuthClientWrapper.Object))
				{
					Assert.DoesNotThrow(() => tokenProvider.GetAccessToken());
					Assert.DoesNotThrow(() => tokenProvider.GetAccessToken());
					Assert.DoesNotThrow(() => tokenProvider.GetAccessToken());
				}
			}
		}

		[Test]
		public void GetAccessToken_WithValidToken_NotToRequestNewAccessToken()
		{
			var mockCertificateInfoManager = new Mock<ICertificateInfoManager>();
			var mockRSA = new Mock<RSA>().Object;
			mockCertificateInfoManager.Setup(x => x.GetPrivateKey()).Returns(mockRSA);
			var mockCertificate = new Mock<X509Certificate2>().Object;
			mockCertificateInfoManager.Setup(x => x.GetCertificate()).Returns(mockCertificate);
			var mockOAuthClientWrapper = new Mock<IOAuthClientWrapper>();
			var accessToken = AccessTokenGenerator.GenerateS2SAccessToken("0732A448-9C7B-4FBF-873F-A10789A95E0C", DateTime.Now.AddMonths(1));
			mockOAuthClientWrapper.Setup(x =>
					x.GetClientAccessTokenAsync($"https://login.microsoftonline.com/valid_tenant_id/oauth2/v2.0/token", mockRSA, mockCertificate, "0732A448-9C7B-4FBF-873F-A10789A95E0C", "valid_service_id"))
				.ReturnsAsync(accessToken);
			using (var tokenProvider = new AccessTokenProvider("valid_tenant_id",
						"0732A448-9C7B-4FBF-873F-A10789A95E0C", "valid_service_id", mockCertificateInfoManager.Object,
						mockOAuthClientWrapper.Object))
			{
				tokenProvider.GetAccessToken();
				tokenProvider.GetAccessToken();
				mockCertificateInfoManager.Verify(x => x.GetCertificate(), Times.Exactly(1));
				mockCertificateInfoManager.Verify(x => x.IsCertNearToExpire(), Times.Exactly(1));
				mockCertificateInfoManager.Verify(x => x.GetPrivateKey(), Times.Exactly(1));
			}
		}

		[Test]
		public void GetAccessToken_WithNearToExpireCert_RenewalNewCertificate()
		{
			var mockCertificateInfoManager = new Mock<ICertificateInfoManager>();
			var mockRSA = new Mock<RSA>().Object;
			mockCertificateInfoManager.Setup(x => x.GetPrivateKey()).Returns(mockRSA);
			mockCertificateInfoManager.Setup(x => x.IsCertNearToExpire()).Returns(true);
			var mockCertificate = new Mock<X509Certificate2>().Object;
			mockCertificateInfoManager.Setup(x => x.GetCertificate()).Returns(mockCertificate);
			var mockOAuthClientWrapper = new Mock<IOAuthClientWrapper>();
			var accessToken = AccessTokenGenerator.GenerateS2SAccessToken("0732A448-9C7B-4FBF-873F-A10789A95E0C", DateTime.Now.AddMonths(1));
			mockOAuthClientWrapper.Setup(x =>
					x.GetClientAccessTokenAsync($"https://login.microsoftonline.com/valid_tenant_id/oauth2/v2.0/token", mockRSA, mockCertificate, "0732A448-9C7B-4FBF-873F-A10789A95E0C", It.IsAny<string>()))
				.ReturnsAsync(accessToken);
			using var tokenProvider = new AccessTokenProvider("valid_tenant_id", "0732A448-9C7B-4FBF-873F-A10789A95E0C", "valid_service_id", mockCertificateInfoManager.Object,
				mockOAuthClientWrapper.Object);
			tokenProvider.GetAccessToken();
			mockCertificateInfoManager.Verify(x => x.RenewalCertificateAsync(It.IsAny<string>(), accessToken), Times.Once);
		}
	}
}
