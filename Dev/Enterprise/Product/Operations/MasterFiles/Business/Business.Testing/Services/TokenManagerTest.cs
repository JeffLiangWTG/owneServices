using System;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Authentication;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class TokenManagerTest : TestCaseWithFactory
	{
		public void TestGetAuthorizationToken_When_CachedTokenIsValid_Should_ReturnAuthenticationHeaderValueWithCachedTokenOnly()
		{
			TestGetAuthorizationToken_When_CachedTokenIsValid_Should_ReturnAuthenticationHeaderValueWithCachedTokenOnly(MDMProductCodes.AVS);
			TestGetAuthorizationToken_When_CachedTokenIsValid_Should_ReturnAuthenticationHeaderValueWithCachedTokenOnly(MDMProductCodes.DPS);
		}

		public void TestGetAuthorizationToken_When_CachedTokenIsExpired_Should_ReturnAuthenticationHeaderValueWithUpdatedToken()
		{
			TestGetAuthorizationToken_When_CachedTokenIsExpired_Should_ReturnAuthenticationHeaderValueWithUpdatedToken(MDMProductCodes.AVS);
			TestGetAuthorizationToken_When_CachedTokenIsExpired_Should_ReturnAuthenticationHeaderValueWithUpdatedToken(MDMProductCodes.DPS);
		}

		public void TestGetAuthorizationToken_When_CertificateInfoDiffersFromCachedCertificateInfo_Should_ReturnAuthenticationHeaderValueWithUpdatedToken()
		{
			TestGetAuthorizationToken_When_CertificateInfoDiffersFromCachedCertificateInfo_Should_ReturnAuthenticationHeaderValueWithUpdatedToken(MDMProductCodes.AVS);
			TestGetAuthorizationToken_When_CertificateInfoDiffersFromCachedCertificateInfo_Should_ReturnAuthenticationHeaderValueWithUpdatedToken(MDMProductCodes.DPS);
		}

		public void TestGetAuthorizationToken_When_CertificateInfoIsEmpty_Should_ThrowCertificateNotFoundExceptionException()
		{
			TestGetAuthorizationToken_When_CertificateInfoIsEmpty_Should_ThrowAuthCertNotFoundException(MDMProductCodes.AVS);
			TestGetAuthorizationToken_When_CertificateInfoIsEmpty_Should_ThrowAuthCertNotFoundException(MDMProductCodes.DPS);
		}

		public void TestGetAuthorizationToken_When_AuthorizationTokenCoreThrowsException_Should_PropogateException()
		{
			TestGetAuthorizationToken_When_AuthorizationTokenCoreThrowsException_Should_PropogateException(MDMProductCodes.AVS);
			TestGetAuthorizationToken_When_AuthorizationTokenCoreThrowsException_Should_PropogateException(MDMProductCodes.DPS);
		}

		public void TestGetAuthorizationToken_When_AuthenticationCertificateInfoThrowException_Should_ThrowAuthenticationException()
		{
			TestGetAuthorizationToken_When_AuthenticationCertificateInfoThrowException_Should_ThrowAuthenticationException(MDMProductCodes.AVS);
			TestGetAuthorizationToken_When_AuthenticationCertificateInfoThrowException_Should_ThrowAuthenticationException(MDMProductCodes.DPS);
		}

		#region Implementation

		void TestGetAuthorizationToken_When_CachedTokenIsValid_Should_ReturnAuthenticationHeaderValueWithCachedTokenOnly(MDMProductCodes productCode)
		{
			var certificateInfo = AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo();

			using (ObjectFactory.Substitute<IAuthorizationTokenProvider>(new AuthorizationTokenProviderForTest()))
			using (productCode == MDMProductCodes.DPS
				? (OrganisationsDataRegistry.Instance.MDMSupportCertificateForDPS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, certificateInfo))
				: (MDMSupportCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, certificateInfo)))
			{
				var tokenManager = TokenManager.GetInstance(productCode);

				var initialHeaderToken = tokenManager.GetAuthorizationHeaderValue();
				AssertNotNull(initialHeaderToken);

				var headerToken = tokenManager.GetAuthorizationHeaderValue();
				AssertEquals(initialHeaderToken, headerToken);
			}
		}

		void TestGetAuthorizationToken_When_CachedTokenIsExpired_Should_ReturnAuthenticationHeaderValueWithUpdatedToken(MDMProductCodes productCode)
		{
			var certificateInfo = AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo();
			var authorizationTokenCoreForTest = new AuthorizationTokenProviderForTest();

			using (ObjectFactory.Substitute<IAuthorizationTokenProvider>(authorizationTokenCoreForTest))
			using (productCode == MDMProductCodes.DPS
				? (OrganisationsDataRegistry.Instance.MDMSupportCertificateForDPS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, certificateInfo))
				: (MDMSupportCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, certificateInfo)))
			{
				var tokenManager = TokenManager.GetInstance(productCode);

				authorizationTokenCoreForTest.ExpiryTime = DateTime.UtcNow.AddMinutes(3);
				var initialHeaderToken = tokenManager.GetAuthorizationHeaderValue();
				AssertNotNull(initialHeaderToken);

				var headerToken = tokenManager.GetAuthorizationHeaderValue();
				AssertNotEquals(initialHeaderToken, headerToken);
			}
		}

		void TestGetAuthorizationToken_When_CertificateInfoDiffersFromCachedCertificateInfo_Should_ReturnAuthenticationHeaderValueWithUpdatedToken(MDMProductCodes productCode)
		{
			var authorizationTokenCoreForTest = new AuthorizationTokenProviderForTest();
			var certificateInfo = AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo();
			var initialHeaderToken = default(AuthenticationHeaderValue);
			var tokenManager = TokenManager.GetInstance(productCode);

			using (ObjectFactory.Substitute<IAuthorizationTokenProvider>(authorizationTokenCoreForTest))
			{
				using (productCode == MDMProductCodes.DPS
				? (OrganisationsDataRegistry.Instance.MDMSupportCertificateForDPS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, certificateInfo))
				: (MDMSupportCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, certificateInfo)))
				{
					initialHeaderToken = tokenManager.GetAuthorizationHeaderValue();

					AssertNotNull(initialHeaderToken);
				}

				certificateInfo.ClientId = Guid.NewGuid().ToString();

				using (productCode == MDMProductCodes.DPS
					? (OrganisationsDataRegistry.Instance.MDMSupportCertificateForDPS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, certificateInfo))
					: (MDMSupportCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, certificateInfo)))
				{
					var headerToken = tokenManager.GetAuthorizationHeaderValue();

					AssertNotEquals(initialHeaderToken, headerToken);
				}
			}
		}

		void TestGetAuthorizationToken_When_CertificateInfoIsEmpty_Should_ThrowAuthCertNotFoundException(MDMProductCodes productCode)
		{
			var tokenManager = TokenManager.GetInstance(productCode);

			using (productCode == MDMProductCodes.DPS
				? (OrganisationsDataRegistry.Instance.MDMSupportCertificateForDPS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemToSystemTrustInfo()))
				: (MDMSupportCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemToSystemTrustInfo())))
			{
				AssertExceptionThrown<AuthCertNotFoundException>("Exception doesn't match", () => tokenManager.GetAuthorizationHeaderValue());
			}
		}

		void TestGetAuthorizationToken_When_AuthorizationTokenCoreThrowsException_Should_PropogateException(MDMProductCodes productCode)
		{
			var certificateInfo = AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo();
			var tokenManager = TokenManager.GetInstance(productCode);
			var authorizationTokenCoreForTest = new AuthorizationTokenProviderForTest();

			using (ObjectFactory.Substitute<IAuthorizationTokenProvider>(authorizationTokenCoreForTest))
			using (productCode == MDMProductCodes.DPS
				? (OrganisationsDataRegistry.Instance.MDMSupportCertificateForDPS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, certificateInfo))
				: (MDMSupportCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, certificateInfo)))
			{
				authorizationTokenCoreForTest.ShouldThrowException = true;
				authorizationTokenCoreForTest.Exception = new InvalidOperationException("Invalid client_id");

				AssertExceptionThrown<InvalidOperationException>("Exception message doesn't match", "Invalid client_id", () => tokenManager.GetAuthorizationHeaderValue());
			}
		}

		void TestGetAuthorizationToken_When_AuthenticationCertificateInfoThrowException_Should_ThrowAuthenticationException(MDMProductCodes productCode)
		{
			var certificateInfo = AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo();
			certificateInfo.Certificate = new Byte[] { 0x00 };

			var tokenManager = TokenManager.GetInstance(productCode);

			using (productCode == MDMProductCodes.DPS
				? (OrganisationsDataRegistry.Instance.MDMSupportCertificateForDPS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, certificateInfo))
				: (MDMSupportCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, certificateInfo)))
			{
				AssertExceptionThrown<AuthenticationException>("Exception message doesn't match", "System to system trust certificate is invalid.", () => tokenManager.GetAuthorizationHeaderValue());
			}
		}

		MDMSupportCertificateRegistryItem MDMSupportCertificate
		{
			get
			{
				mdmSupportCertificate ??= (MDMSupportCertificateRegistryItem)(typeof(OrganisationRegistry).GetProperty("MDMSupportCertificate", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(OrganisationRegistry.Instance));
				return mdmSupportCertificate;
			}
		}
		MDMSupportCertificateRegistryItem mdmSupportCertificate;

		#endregion
	}
}
