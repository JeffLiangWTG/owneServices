using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.Common.Utils;

public class CertificateRenewal(string clientIdentifier, string accessToken, IFileReader fileReader) : ICertificateRenewal
{
	public async Task<byte[]> GetNewCertificateContentAsync()
	{
		var certificatePk = await GetCertificatePkAsync();
		return await GetCertificateContentAsync(certificatePk);
	}

	static async Task<byte[]> GetCertificateContentAsync(string certificatePk)
	{
		using var client = new HttpClient();
		var retryAttempt = 0;

		while (retryAttempt < MaxRetryTimes)
		{
			var response = await client.GetAsync(new Uri(SystemToSystemTrustCertificateApiEndpoint + $"{certificatePk}"));
			response.EnsureSuccessStatusCode();
			var responseContent = await response.Content.ReadAsStringAsync();
			var certificateResponse = JsonSerializer.Deserialize<SystemToSystemTrustCertificateResponse>(responseContent);
			if (certificateResponse.IsCompleted)
			{
				return certificateResponse.CertificateData;
			}

			Console.WriteLine($"Certificate is not ready yet, retrying... {retryAttempt + 1}/{MaxRetryTimes}");
			await Task.Delay(TimeSpan.FromSeconds(30));

			retryAttempt++;
		}

		throw new InvalidOperationException("Get Certificate Content failed: No Certificate content.");
	}

	async Task<string> GetCertificatePkAsync()
	{
		using var client = new HttpClient();
		if (string.IsNullOrEmpty(accessToken))
		{
			throw new InvalidOperationException(
				"Access Token should not be empty when rollover certificate.");
		}
		client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

		var csrContent = await fileReader.ReadAllTextAsync();
		var requestBody = new SystemToSystemTrustCertificateRolloverRequest
		{
			ClientId = clientIdentifier, CertificateSignRequest = csrContent, CaRootType = "S2S"
		};
		var jsonContent = JsonSerializer.Serialize(requestBody);
		using var httpContent = new StringContent(jsonContent, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));
		var response = await client.PostAsync(new Uri(SystemToSystemTrustCertificateApiEndpoint + "rollover"), httpContent);
		response.EnsureSuccessStatusCode();

		Console.WriteLine("Trigger Rollover Certificate succeeded.");
		return await response.Content.ReadAsStringAsync();
	}

	const string SystemToSystemTrustCertificateApiEndpoint = "https://myaccount-portal.cargowise.com/myaccount/api/SystemTrust/certificate/";
	const int MaxRetryTimes = 3;

	class SystemToSystemTrustCertificateRolloverRequest
	{
		private static class CertificateRequestFieldSchema
		{
			public const string ClientId = "clientId";
			public const string CertificateSignRequest = "csr";
			public const string CaRootType = "caRootType";
		}

		[JsonPropertyName(CertificateRequestFieldSchema.ClientId)]
		public string ClientId { get; set; }

		[JsonPropertyName(CertificateRequestFieldSchema.CertificateSignRequest)]
		public string CertificateSignRequest { get; set; }

		[JsonPropertyName(CertificateRequestFieldSchema.CaRootType)]
		public string CaRootType { get; set; }
	}

	#pragma warning disable CA1819
	class SystemToSystemTrustCertificateResponse
	{
		[JsonPropertyName(CertificateResponseFieldSchema.CertificateData)]
		public byte[] CertificateData { get; init; }

		[JsonPropertyName(CertificateResponseFieldSchema.CertificateDataArray)]
		public IdentityCertificateData[] CertificateDataArray { get; init; }

		[JsonPropertyName(CertificateResponseFieldSchema.ClientId)]
		public string ClientId { get; init; }

		[JsonPropertyName(CertificateResponseFieldSchema.TenantId)]
		public string TenantId { get; init; }

		[JsonPropertyName(CertificateResponseFieldSchema.Status)]
		public string Status { get; init; }

		[JsonPropertyName(CertificateResponseFieldSchema.StatusCode)]
		public string StatusCode { get; init; }

		const string CompleteStatus = "COM";
		public bool IsCompleted => StatusCode.Equals(CompleteStatus, StringComparison.OrdinalIgnoreCase);
	}

	class IdentityCertificateData
	{
		[JsonPropertyName(CertificateResponseFieldSchema.CertificateData)]
		public byte[] CertificateData { get; set; }
	}

	static class CertificateResponseFieldSchema
	{
		public const string CertificateData = "certificate";
		public const string CertificateDataArray = "certificates";
		public const string ClientId = "clientId";
		public const string TenantId = "tenantId";
		public const string Status = "status";
		public const string StatusCode = "statusCode";
	}
	#pragma warning restore CA1819 // Properties should not return arrays
}
