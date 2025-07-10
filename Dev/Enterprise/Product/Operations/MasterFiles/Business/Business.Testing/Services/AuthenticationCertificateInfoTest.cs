using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AuthenticationCertificateInfo))]
	public class AuthenticationCertificateInfoTest : TestCaseWithFactory
	{
		public void TestEmptyCertificateInfoAVS()
		{
			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemToSystemTrustInfo()))
			using (MDMSupportCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemToSystemTrustInfo()))
			{
				AssertEquals(true, new AuthenticationCertificateInfo(MDMProductCodes.AVS).IsEmpty);
			}
		}

		public void TestNotEmptyCertificateInfoAVS()
		{
			using (MDMSupportCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetDummyAuthenticationInfo()))
			{
				AssertEquals(false, new AuthenticationCertificateInfo(MDMProductCodes.AVS).IsEmpty);
			}
		}

		[ExpectNoExceptions]
		public void TestClientCertificateInfoAVS()
		{
			var systemToSystemTrustInfo = GetDummyAuthenticationInfo();
			systemToSystemTrustInfo.OperationId = null;
			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, systemToSystemTrustInfo))
			{
				var certificateInfo = new AuthenticationCertificateInfo(MDMProductCodes.AVS);

				AssertEquals(false, certificateInfo.IsEmpty);
				AssertEquals(systemToSystemTrustInfo.ClientId, certificateInfo.ClientId);
				AssertEquals(systemToSystemTrustInfo.TenantId, certificateInfo.TenantId);
				AssertEquals("c34e06bf-4d88-483b-adf7-d650fdbdc62b", certificateInfo.ServerClientId);
				AssertNotNull(certificateInfo.Certificate);
				AssertNotNull(certificateInfo.PrivateKey);
			}
		}

		[ExpectNoExceptions]
		public void TestMDMSupportCertificateInfoAVS()
		{
			var systemToSystemTrustInfo = GetDummyAuthenticationInfo();
			using (MDMSupportCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, systemToSystemTrustInfo))
			{
				var certificateInfo = new AuthenticationCertificateInfo(MDMProductCodes.AVS);

				AssertEquals(false, certificateInfo.IsEmpty);
				AssertEquals(systemToSystemTrustInfo.ClientId, certificateInfo.ClientId);
				AssertEquals(systemToSystemTrustInfo.TenantId, certificateInfo.TenantId);
				AssertEquals(systemToSystemTrustInfo.OperationId, certificateInfo.ServerClientId);
				AssertNotNull(certificateInfo.Certificate);
				AssertNotNull(certificateInfo.PrivateKey);
			}
		}

		public void TestInvalidCertificateAVS()
		{
			var systemToSystemTrustInfo = GetDummyAuthenticationInfo();
			systemToSystemTrustInfo.Certificate = ZBlob.FromUTF8("Invalid Value");

			using (MDMSupportCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, systemToSystemTrustInfo))
			{
				AssertExceptionThrown<CryptographicException>(() => new AuthenticationCertificateInfo(MDMProductCodes.AVS));
			}
		}

		public void TestAuthenticationCertificateInfoConstructorDPS_When_MDMSupportCertificateRegistryItemPopulated_Should_CreateAuthenticationCertificateUsingSupportCertificate()
		{
			var authenticationInfo = GetDummyAuthenticationInfo();
			var systemCertificate = GetDummyAuthenticationInfo();
			systemCertificate.OperationId = null;
			using (OrganisationsDataRegistry.Instance.MDMSupportCertificateForDPS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, authenticationInfo))
			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, systemCertificate))
			{
				var dpsCertificateInfo = new AuthenticationCertificateInfo(MDMProductCodes.DPS);
				AssertEquals(false, dpsCertificateInfo.IsEmpty);
				AssertEquals(authenticationInfo.TenantId, dpsCertificateInfo.TenantId);
				AssertEquals(authenticationInfo.ClientId, dpsCertificateInfo.ClientId);
				AssertEquals(authenticationInfo.OperationId, dpsCertificateInfo.ServerClientId);
				AssertEquals(new X509Certificate2(authenticationInfo.Certificate), dpsCertificateInfo.Certificate);
#pragma warning disable CS0618 // To be replaced with S2ST library once WI00771920 is implemented
				AssertEquals(authenticationInfo.PrivateKey, dpsCertificateInfo.AuthenticationInfo.PrivateKey);
#pragma warning restore CS0618 // To be replaced with S2ST library once WI00771920 is implemented
				AssertNotNull(dpsCertificateInfo.PrivateKey);
			}
		}

		public void TestAuthenticationCertificateInfoConstructorDPS_When_MDMSupportCertificateRegistryItemNotPopulated_Should_CreateAuthenticationCertificateUsingSystemCertificate()
		{
			var authenticationInfo = GetDummyAuthenticationInfo();
			using (OrganisationsDataRegistry.Instance.MDMSupportCertificateForDPS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemToSystemTrustInfo()))
			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, authenticationInfo))
			{
				var dpsCertificateInfo = new AuthenticationCertificateInfo(MDMProductCodes.DPS);
				AssertEquals(false, dpsCertificateInfo.IsEmpty);
				AssertEquals(authenticationInfo.TenantId, dpsCertificateInfo.TenantId);
				AssertEquals(authenticationInfo.ClientId, dpsCertificateInfo.ClientId);
				AssertEquals("1ae29a5d-8c05-4121-a540-90c9e75d5242", dpsCertificateInfo.ServerClientId);
				AssertEquals(new X509Certificate2(authenticationInfo.Certificate), dpsCertificateInfo.Certificate);
#pragma warning disable CS0618 // To be replaced with S2ST library once WI00771920 is implemented
				AssertEquals(authenticationInfo.PrivateKey, dpsCertificateInfo.AuthenticationInfo.PrivateKey);
#pragma warning restore CS0618 // To be replaced with S2ST library once WI00771920 is implemented
				AssertNotNull(dpsCertificateInfo.PrivateKey);
			}
		}

		public void TestAuthenticationCertificateInfoConstructorDPS_When_NeitherSupportNorSystemCertificateIsPopulated_Should_SetIsEmptyField()
		{
			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemToSystemTrustInfo()))
			using (OrganisationsDataRegistry.Instance.MDMSupportCertificateForDPS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemToSystemTrustInfo()))
			{
				AssertEquals(true, new AuthenticationCertificateInfo(MDMProductCodes.DPS).IsEmpty);
			}
		}

		public void TestBOLUsesSystemCertificateInfo()
		{
			var galileoTestAudience = Guid.NewGuid().ToString();
			var boleroEBLConfiguration = new BoleroEBLForOrganisationConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = galileoTestAudience
			};
			var authenticationInfo = GetDummyAuthenticationInfo();
			using (OrganisationRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, authenticationInfo))
			{
				var bolCertificateInfo = new AuthenticationCertificateInfo(MDMProductCodes.BOL);
				AssertEquals(false, bolCertificateInfo.IsEmpty);
				AssertEquals(authenticationInfo.TenantId, bolCertificateInfo.TenantId);
				AssertEquals(authenticationInfo.ClientId, bolCertificateInfo.ClientId);
				AssertEquals(galileoTestAudience, bolCertificateInfo.ServerClientId);
				AssertEquals(new X509Certificate2(authenticationInfo.Certificate), bolCertificateInfo.Certificate);
#pragma warning disable CS0618 // To be replaced with S2ST library once WI00771920 is implemented
				AssertEquals(authenticationInfo.PrivateKey, bolCertificateInfo.AuthenticationInfo.PrivateKey);
#pragma warning restore CS0618 // To be replaced with S2ST library once WI00771920 is implemented
				AssertNotNull(bolCertificateInfo.PrivateKey);
			}
		}

		public static SystemToSystemTrustInfo GetDummyAuthenticationInfo()
		{
			var fakePrivateKey = @"-----BEGIN PRIVATE KEY-----
				MIIEvAIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQCvhdGqhkWWTGr/
				KefcKSHeEfsCOTEmBw47LDMp2Xms8tV9CuKLnOltJU+rH9yYizdROS0J7A93GN6B
				+vAJxSLB3bRxT8m6nFP11awyXZPb5HcbU5dKJQNf3dcDus9jeyDMNbCuaA93vTQs
				s9PY7QfNPxcUSm0M5qHWEgBWeShVr3wg3RwwwZh+mLm6cYUmA+fD71QH/hZMipkq
				CUJcfPm1riVmzhaZ03lrwpJSd7KdFvJAT5xb668b9JCDW8MgTbIARYm9vijDdVbG
				WjW+y61uIb2wL0JLMj4FovPxQjhSwnikDRBkobUNRRsB1RUp4dLud4kZbBa6TfA5
				OFQ680M3AgMBAAECggEASsb/np8KqXAQC3o+cfDCIXpWjklwU2uhF/uKJpOkv1ZL
				NrT69BDa76l8KgLud7yjygJKWlZL9mjNbuHJ/teSKba656Ve45YzPOIVtPViB0Xr
				qmQv6aIgMGjx8ABX12F/BREAnyTtJg2g20SXhezhrILq4bWdhOgC3ZEYvL5sPShK
				0OnnGnKMFZ7CBjPuJk7++yZKdx0bjhB9eNeVynt1wvYJMta2p6PRAIq22sJq+KrL
				5ojUcLnutW8i8ZdHGysko+eb7QzZUwVgoIX2Rg5GbEnQTyQf5eW7f5pmXxTXwPhA
				wuiG/esvsHmMcKIFApLeiH5pcfumongFG7KAdLGAVQKBgQDsWjcEKhwMhMlEYth0
				eN4uGCBBhJxuGpqxFes1qyDOBrXEr6oE3QEAoF5/c80HLHICY+05qy/vwEkMMN/y
				syjNx6bW/QLuxT1UgcZgpLqHrwYxZBieJsKsmqN0SoLmBhGshCeswojPdZmz9vtl
				/HMZ8J+Y+ADAVprAEm2Y1RxCRQKBgQC+HRh+cswDuqpqK5VEF+K66YSBWP422i07
				evgIvYsO4CPn19L/fiqz6YbuIVqmkg/7rl49zH959JS7ukvfcN51NCzgkFJZbgW6
				2jClLjJjsztSahggTXqp0WD5LQ5NxsUNzMSrwZTjAotmNyxRTjZfzW183WPLgwKi
				xRP7GyaFSwKBgGpYbUjCabx4QtcyYpKFj/LNiDXypTAlaFUlt59+UFRjUIYfRDDM
				ABd4EQzn3ejMZsAMlkDMddU6f6Osmhdp5YIxwzAYx6kHtoC/o7L4a7WBWxf+IdWH
				OzDOo50/qYY2VN162R8yqLwv/eiryJIq9N9HFYiOjkf8r8SchhOuT/jBAoGAaYcJ
				A5eBO0iwM4LBtix0BEB+9rWJVrVAilW1vFRKDhXImHaqfntwBLHJ3gDRqshE6vVd
				Bnyu/ekPbiz41Kx4LyKpDnXN4Co8L/3RJr8/5Sul8BdIERYw0naQl3+1AuMkmoZh
				XN11YZUV/8T8ap05fXAwKDFTpbGxEtzGPIpTlYUCgYAWWqtgtXRtjQH70QWRfp0R
				6yU7l3PLEBVsq2X4AoN+zqTnAUeYL/YNrfrqBtJTbSSW0ZBsy75af5ou8y9x54qe
				Yeseunc2SZeKg7zS2xsPLCJ0deE8v9bbWmKXyRyGhKx4pxqFLbIj7s62IbntETp7
				+K4zZau/Ehh1ZoE+TPBRIg==
				-----END PRIVATE KEY-----
				";

			var fakeCertificate = "LS0tLS1CRUdJTiBDRVJUSUZJQ0FURS0tLS0tDQpNSUlEb3pDQ0FvdWdBd0lCQWdJVWNaS3M5UGszM1gydnlmUDNVeXBkNVRtRjQvRXdEUVlKS29aSWh2Y05BUUVMDQpCUUF3WVRFTE1Ba0dBMVVFQmhNQ01EQXhDekFKQmdOVkJBZ01BakV4TVFzd0NRWURWUVFIREFJeU1qRUxNQWtHDQpBMVVFQ2d3Q016TXhDekFKQmdOVkJBc01BalEwTVFzd0NRWURWUVFEREFJMU5URVJNQThHQ1NxR1NJYjNEUUVKDQpBUllDTmpZd0hoY05Nak14TWpBM01ETXdPVFExV2hjTk1qTXhNakE0TURNd09UUTFXakJoTVFzd0NRWURWUVFHDQpFd0l3TURFTE1Ba0dBMVVFQ0F3Q01URXhDekFKQmdOVkJBY01Bakl5TVFzd0NRWURWUVFLREFJek16RUxNQWtHDQpBMVVFQ3d3Q05EUXhDekFKQmdOVkJBTU1BalUxTVJFd0R3WUpLb1pJaHZjTkFRa0JGZ0kyTmpDQ0FTSXdEUVlKDQpLb1pJaHZjTkFRRUJCUUFEZ2dFUEFEQ0NBUW9DZ2dFQkFLK0YwYXFHUlpaTWF2OHA1OXdwSWQ0Uit3STVNU1lIDQpEanNzTXluWmVhenkxWDBLNG91YzZXMGxUNnNmM0ppTE4xRTVMUW5zRDNjWTNvSDY4QW5GSXNIZHRIRlB5YnFjDQpVL1hWckRKZGs5dmtkeHRUbDBvbEExL2Qxd082ejJON0lNdzFzSzVvRDNlOU5DeXowOWp0QjgwL0Z4UktiUXptDQpvZFlTQUZaNUtGV3ZmQ0RkSEREQm1INll1YnB4aFNZRDU4UHZWQWYrRmt5S21Tb0pRbHg4K2JXdUpXYk9GcG5UDQplV3ZDa2xKM3NwMFc4a0JQbkZ2cnJ4djBrSU5id3lCTnNnQkZpYjIrS01OMVZzWmFOYjdMclc0aHZiQXZRa3N5DQpQZ1dpOC9GQ09GTENlS1FORUdTaHRRMUZHd0hWRlNuaDB1NTNpUmxzRnJwTjhEazRWRHJ6UXpjQ0F3RUFBYU5UDQpNRkV3SFFZRFZSME9CQllFRkhjRENmeXdJZERmT3oyYlNmaUZWV3hMdDQzWk1COEdBMVVkSXdRWU1CYUFGSGNEDQpDZnl3SWREZk96MmJTZmlGVld4THQ0M1pNQThHQTFVZEV3RUIvd1FGTUFNQkFmOHdEUVlKS29aSWh2Y05BUUVMDQpCUUFEZ2dFQkFKbUwyUytOSWN3OU9KbG9RRnRldThCdHJwWHQra1ppOEZwbzE3L2xBWVgxaWhqTzRrNUJLZEJVDQpxNnlodDJzZ05CLy9xdXhZNEtJc1JsMnFaVytMOTFKNXV5UTNRTCtRSUtvWGMraDJIYlY2dEwwNjNRT25BSjIwDQpFVzY3NjhpcTRBU0dtNnVReTRPTCtnMjl0MGp6ZzdXaTdXa0RDdVhrdTN3eVN0emIwZVkxZk5JSU5LczYzcnhHDQpiUkQ2ekg3SUJja1dYRWFGcGptd0lpVkNzUVM2RFZlNGd0Ym1vZkZNcWlMUlkxQytVZnBtUjNWMEF0K0pYVlBaDQpqelZ3R1A5NUd6Y0dqMGpMTTVaWjlMb0tMNVB3U1RxMW0wYUN0SmpEdlZCN3VrTU5pem04V3MxckIzeDNzTEExDQpER1ROVG1ianJmUjZBU1Z2TDhGNkZEdmI0d2xVUE9nPQ0KLS0tLS1FTkQgQ0VSVElGSUNBVEUtLS0tLQ0K";

#pragma warning disable CS0618 // To be replaced with S2ST library once WI00771920 is implemented
			var systemToSystemTrustInfo = new SystemToSystemTrustInfo
			{
				ClientId = Guid.NewGuid().ToString(),
				TenantId = Guid.NewGuid().ToString(),
				PrivateKey = fakePrivateKey,
				OperationId = Guid.NewGuid().ToString(),
				Certificate = Convert.FromBase64String(fakeCertificate)
			};
#pragma warning restore CS0618 // To be replaced with S2ST library once WI00771920 is implemented

			return systemToSystemTrustInfo;
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
	}
}
