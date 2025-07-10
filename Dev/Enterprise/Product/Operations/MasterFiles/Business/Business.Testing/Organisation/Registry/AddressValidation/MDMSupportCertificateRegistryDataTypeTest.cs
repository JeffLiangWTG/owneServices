using System;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(MDMSupportCertificateRegistryDataType))]
	public class MDMSupportCertificateRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<MDMSupportCertificateRegistryDataType>
	{
		public void TestValidation()
		{
			var dataType = new MDMSupportCertificateRegistryDataType(MDMProductCodes.AVS);

			var certificateInfo = AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo();

			AssertNoExceptionThrown(() => dataType.Validate(null, certificateInfo, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestAllowEmptyValue()
		{
			var dataType = new MDMSupportCertificateRegistryDataType(MDMProductCodes.AVS);

			AssertNoExceptionThrown(() => dataType.Validate(null, new SystemToSystemTrustInfo(), Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestProductCode()
		{
			var dataType = new MDMSupportCertificateRegistryDataType(MDMProductCodes.AVS);
			AssertEquals(MDMProductCodes.AVS, dataType.ProductCode);

			dataType = new MDMSupportCertificateRegistryDataType(MDMProductCodes.DPS);
			AssertEquals(MDMProductCodes.DPS, dataType.ProductCode);
		}

		public void TestAllFieldsAreRequired()
		{
#pragma warning disable CS0618 // To be replaced with S2ST library once WI00771920 is implemented
			var actions = new[]
			{
				new Action<SystemToSystemTrustInfo>(info => info.ClientId = null),
				new Action<SystemToSystemTrustInfo>(info => info.TenantId = null),
				new Action<SystemToSystemTrustInfo>(info => info.OperationId = null),
				new Action<SystemToSystemTrustInfo>(info => info.PrivateKey = null),
				new Action<SystemToSystemTrustInfo>(info => info.ClientId = string.Empty),
				new Action<SystemToSystemTrustInfo>(info => info.TenantId = string.Empty),
				new Action<SystemToSystemTrustInfo>(info => info.OperationId = string.Empty),
				new Action<SystemToSystemTrustInfo>(info => info.PrivateKey = string.Empty),
				new Action<SystemToSystemTrustInfo>(info => info.Certificate = default)
			};
#pragma warning restore CS0618 // To be replaced with S2ST library once WI00771920 is implemented

			var dataType = new MDMSupportCertificateRegistryDataType(MDMProductCodes.AVS);
			actions.ForEach(x =>
			{
				var certificateInfo = AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo();
				x.Invoke(certificateInfo);

				AssertExceptionThrown<RegistryValidationException>("Missing field", "For a valid MDM Support Certificate, all fields are required, including three Guid values and two files.", () => dataType.Validate(null, certificateInfo, Guid.Empty, Guid.Empty, Guid.Empty));
			});
		}

		public void TestUniqueGuidValuesAreRequired()
		{
			var actions = new[]
			{
				new Action<SystemToSystemTrustInfo>(info => info.ClientId = "Invalid Guid"),
				new Action<SystemToSystemTrustInfo>(info => info.TenantId = "Invalid Guid"),
				new Action<SystemToSystemTrustInfo>(info => info.OperationId = "Invalid Guid"),
				new Action<SystemToSystemTrustInfo>(info => info.ClientId = info.TenantId),
				new Action<SystemToSystemTrustInfo>(info => info.ClientId = info.OperationId),
				new Action<SystemToSystemTrustInfo>(info => info.TenantId = info.OperationId)
			};

			var dataType = new MDMSupportCertificateRegistryDataType(MDMProductCodes.AVS);
			actions.ForEach(x =>
			{
				var certificateInfo = AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo();
				x.Invoke(certificateInfo);

				AssertExceptionThrown<RegistryValidationException>("Guid Error", "Client ID, Tenant ID and Target AVS ID are all required to be a unique Guid value.", () => dataType.Validate(null, certificateInfo, Guid.Empty, Guid.Empty, Guid.Empty));
			});
		}

		protected override MDMSupportCertificateRegistryDataType GetNewDataType()
		{
			return new MDMSupportCertificateRegistryDataType(MDMProductCodes.AVS);
		}

		protected override string ExpectedEditorName => "MDMSupportCertificateRegistryEditor";

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var certificateInfo1 = AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo();
			certificateInfo1.ClientId = "C3843230-0C84-4228-962D-349D8D8874F0";
			certificateInfo1.TenantId = "985F5153-C738-4C55-9469-CF0C45542C53";
			certificateInfo1.OperationId = "10992770-FC66-4B1E-BB21-5D435363430A";

			var certificateInfo2 = AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo();
			certificateInfo2.ClientId = "E6769F9D-573F-481C-956C-20FAA4184BFC";
			certificateInfo2.TenantId = "823FA03D-13F7-4E12-9C0D-04869F774688";
			certificateInfo2.OperationId = "CB2715AA-DC02-4BA9-835E-B9DDBE8F574F";

			var xml1 = Regex.Replace(@"<?xml version=""1.0"" encoding=""utf-16""?>
<SystemToSystemTrustInfo>
  <Certificate>LS0tLS1CRUdJTiBDRVJUSUZJQ0FURS0tLS0tDQpNSUlEb3pDQ0FvdWdBd0lCQWdJVWNaS3M5UGszM1gydnlmUDNVeXBkNVRtRjQvRXdEUVlKS29aSWh2Y05BUUVMDQpCUUF3WVRFTE1Ba0dBMVVFQmhNQ01EQXhDekFKQmdOVkJBZ01BakV4TVFzd0NRWURWUVFIREFJeU1qRUxNQWtHDQpBMVVFQ2d3Q016TXhDekFKQmdOVkJBc01BalEwTVFzd0NRWURWUVFEREFJMU5URVJNQThHQ1NxR1NJYjNEUUVKDQpBUllDTmpZd0hoY05Nak14TWpBM01ETXdPVFExV2hjTk1qTXhNakE0TURNd09UUTFXakJoTVFzd0NRWURWUVFHDQpFd0l3TURFTE1Ba0dBMVVFQ0F3Q01URXhDekFKQmdOVkJBY01Bakl5TVFzd0NRWURWUVFLREFJek16RUxNQWtHDQpBMVVFQ3d3Q05EUXhDekFKQmdOVkJBTU1BalUxTVJFd0R3WUpLb1pJaHZjTkFRa0JGZ0kyTmpDQ0FTSXdEUVlKDQpLb1pJaHZjTkFRRUJCUUFEZ2dFUEFEQ0NBUW9DZ2dFQkFLK0YwYXFHUlpaTWF2OHA1OXdwSWQ0Uit3STVNU1lIDQpEanNzTXluWmVhenkxWDBLNG91YzZXMGxUNnNmM0ppTE4xRTVMUW5zRDNjWTNvSDY4QW5GSXNIZHRIRlB5YnFjDQpVL1hWckRKZGs5dmtkeHRUbDBvbEExL2Qxd082ejJON0lNdzFzSzVvRDNlOU5DeXowOWp0QjgwL0Z4UktiUXptDQpvZFlTQUZaNUtGV3ZmQ0RkSEREQm1INll1YnB4aFNZRDU4UHZWQWYrRmt5S21Tb0pRbHg4K2JXdUpXYk9GcG5UDQplV3ZDa2xKM3NwMFc4a0JQbkZ2cnJ4djBrSU5id3lCTnNnQkZpYjIrS01OMVZzWmFOYjdMclc0aHZiQXZRa3N5DQpQZ1dpOC9GQ09GTENlS1FORUdTaHRRMUZHd0hWRlNuaDB1NTNpUmxzRnJwTjhEazRWRHJ6UXpjQ0F3RUFBYU5UDQpNRkV3SFFZRFZSME9CQllFRkhjRENmeXdJZERmT3oyYlNmaUZWV3hMdDQzWk1COEdBMVVkSXdRWU1CYUFGSGNEDQpDZnl3SWREZk96MmJTZmlGVld4THQ0M1pNQThHQTFVZEV3RUIvd1FGTUFNQkFmOHdEUVlKS29aSWh2Y05BUUVMDQpCUUFEZ2dFQkFKbUwyUytOSWN3OU9KbG9RRnRldThCdHJwWHQra1ppOEZwbzE3L2xBWVgxaWhqTzRrNUJLZEJVDQpxNnlodDJzZ05CLy9xdXhZNEtJc1JsMnFaVytMOTFKNXV5UTNRTCtRSUtvWGMraDJIYlY2dEwwNjNRT25BSjIwDQpFVzY3NjhpcTRBU0dtNnVReTRPTCtnMjl0MGp6ZzdXaTdXa0RDdVhrdTN3eVN0emIwZVkxZk5JSU5LczYzcnhHDQpiUkQ2ekg3SUJja1dYRWFGcGptd0lpVkNzUVM2RFZlNGd0Ym1vZkZNcWlMUlkxQytVZnBtUjNWMEF0K0pYVlBaDQpqelZ3R1A5NUd6Y0dqMGpMTTVaWjlMb0tMNVB3U1RxMW0wYUN0SmpEdlZCN3VrTU5pem04V3MxckIzeDNzTEExDQpER1ROVG1ianJmUjZBU1Z2TDhGNkZEdmI0d2xVUE9nPQ0KLS0tLS1FTkQgQ0VSVElGSUNBVEUtLS0tLQ0K</Certificate>
  <PrivateKey>-----BEGIN PRIVATE KEY-----
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
</PrivateKey>
  <LegacyPrivateKey />
  <RolloverPrivateKey />
  <LegacyCertificate />
  <CSR />
  <ClientId>C3843230-0C84-4228-962D-349D8D8874F0</ClientId>
  <TenantId>985F5153-C738-4C55-9469-CF0C45542C53</TenantId>
  <OperationId>10992770-FC66-4B1E-BB21-5D435363430A</OperationId>
</SystemToSystemTrustInfo>", @">\s*<", "><");

			var xml2 = Regex.Replace(@"<?xml version=""1.0"" encoding=""utf-16""?>
<SystemToSystemTrustInfo>
  <Certificate>LS0tLS1CRUdJTiBDRVJUSUZJQ0FURS0tLS0tDQpNSUlEb3pDQ0FvdWdBd0lCQWdJVWNaS3M5UGszM1gydnlmUDNVeXBkNVRtRjQvRXdEUVlKS29aSWh2Y05BUUVMDQpCUUF3WVRFTE1Ba0dBMVVFQmhNQ01EQXhDekFKQmdOVkJBZ01BakV4TVFzd0NRWURWUVFIREFJeU1qRUxNQWtHDQpBMVVFQ2d3Q016TXhDekFKQmdOVkJBc01BalEwTVFzd0NRWURWUVFEREFJMU5URVJNQThHQ1NxR1NJYjNEUUVKDQpBUllDTmpZd0hoY05Nak14TWpBM01ETXdPVFExV2hjTk1qTXhNakE0TURNd09UUTFXakJoTVFzd0NRWURWUVFHDQpFd0l3TURFTE1Ba0dBMVVFQ0F3Q01URXhDekFKQmdOVkJBY01Bakl5TVFzd0NRWURWUVFLREFJek16RUxNQWtHDQpBMVVFQ3d3Q05EUXhDekFKQmdOVkJBTU1BalUxTVJFd0R3WUpLb1pJaHZjTkFRa0JGZ0kyTmpDQ0FTSXdEUVlKDQpLb1pJaHZjTkFRRUJCUUFEZ2dFUEFEQ0NBUW9DZ2dFQkFLK0YwYXFHUlpaTWF2OHA1OXdwSWQ0Uit3STVNU1lIDQpEanNzTXluWmVhenkxWDBLNG91YzZXMGxUNnNmM0ppTE4xRTVMUW5zRDNjWTNvSDY4QW5GSXNIZHRIRlB5YnFjDQpVL1hWckRKZGs5dmtkeHRUbDBvbEExL2Qxd082ejJON0lNdzFzSzVvRDNlOU5DeXowOWp0QjgwL0Z4UktiUXptDQpvZFlTQUZaNUtGV3ZmQ0RkSEREQm1INll1YnB4aFNZRDU4UHZWQWYrRmt5S21Tb0pRbHg4K2JXdUpXYk9GcG5UDQplV3ZDa2xKM3NwMFc4a0JQbkZ2cnJ4djBrSU5id3lCTnNnQkZpYjIrS01OMVZzWmFOYjdMclc0aHZiQXZRa3N5DQpQZ1dpOC9GQ09GTENlS1FORUdTaHRRMUZHd0hWRlNuaDB1NTNpUmxzRnJwTjhEazRWRHJ6UXpjQ0F3RUFBYU5UDQpNRkV3SFFZRFZSME9CQllFRkhjRENmeXdJZERmT3oyYlNmaUZWV3hMdDQzWk1COEdBMVVkSXdRWU1CYUFGSGNEDQpDZnl3SWREZk96MmJTZmlGVld4THQ0M1pNQThHQTFVZEV3RUIvd1FGTUFNQkFmOHdEUVlKS29aSWh2Y05BUUVMDQpCUUFEZ2dFQkFKbUwyUytOSWN3OU9KbG9RRnRldThCdHJwWHQra1ppOEZwbzE3L2xBWVgxaWhqTzRrNUJLZEJVDQpxNnlodDJzZ05CLy9xdXhZNEtJc1JsMnFaVytMOTFKNXV5UTNRTCtRSUtvWGMraDJIYlY2dEwwNjNRT25BSjIwDQpFVzY3NjhpcTRBU0dtNnVReTRPTCtnMjl0MGp6ZzdXaTdXa0RDdVhrdTN3eVN0emIwZVkxZk5JSU5LczYzcnhHDQpiUkQ2ekg3SUJja1dYRWFGcGptd0lpVkNzUVM2RFZlNGd0Ym1vZkZNcWlMUlkxQytVZnBtUjNWMEF0K0pYVlBaDQpqelZ3R1A5NUd6Y0dqMGpMTTVaWjlMb0tMNVB3U1RxMW0wYUN0SmpEdlZCN3VrTU5pem04V3MxckIzeDNzTEExDQpER1ROVG1ianJmUjZBU1Z2TDhGNkZEdmI0d2xVUE9nPQ0KLS0tLS1FTkQgQ0VSVElGSUNBVEUtLS0tLQ0K</Certificate>
  <PrivateKey>-----BEGIN PRIVATE KEY-----
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
</PrivateKey>
  <LegacyPrivateKey />
  <RolloverPrivateKey />
  <LegacyCertificate />
  <CSR />
  <ClientId>E6769F9D-573F-481C-956C-20FAA4184BFC</ClientId>
  <TenantId>823FA03D-13F7-4E12-9C0D-04869F774688</TenantId>
  <OperationId>CB2715AA-DC02-4BA9-835E-B9DDBE8F574F</OperationId>
</SystemToSystemTrustInfo>", @">\s*<", "><");

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(certificateInfo1, Encoding.Unicode.GetBytes(xml1)),
				new ValidSampleAndBinaryValueInDB(certificateInfo2, Encoding.Unicode.GetBytes(xml2))
			};
		}
	}
}
