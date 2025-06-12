using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using AzureSign.Core;
using RSAKeyVaultProvider;

namespace XT.REST.Deployment.Tasks
{
  public class AzureKeyVaultBuildSigner : IDisposable
  {
    public AzureKeyVaultBuildSigner(string secret)
    {
      azureClientSecret = secret;
    }

    public async Task<int> SignFileAsync(string filePath)
    {
      var signers = await GetSignersAsync().ConfigureAwait(false);
      var description = Path.GetFileName(filePath);
      var result = 0;
      var attemptNumber = 1;
      var nextTimeServer = 0;
      while (attemptNumber < 30)
      {
        var delayTimeSpan = attemptNumber < signers.Count ? TimeSpan.FromSeconds(1) : TimeSpan.FromSeconds(10);
        var signer = signers[nextTimeServer];

        result = signer.SignFile(ToReadOnlySpan(filePath), ToReadOnlySpan(description), null, null);
        if (result == 0)
        {
          return 0;
        }

        await Task.Delay(delayTimeSpan).ConfigureAwait(false);

        attemptNumber++;
        nextTimeServer++;
        nextTimeServer %= signers.Count;
      }
      return result;
    }

    protected virtual Task<List<AuthenticodeKeyVaultSigner>> GetSignersAsync()
    {
      if (signerTasks == null)
      {
        lock (syncRoot)
        {
          signerTasks = signerTasks ?? CreateSignersAsync();
        }
      }

      return signerTasks;
    }

    async Task<List<AuthenticodeKeyVaultSigner>> CreateSignersAsync()
    {
      var credential = new ClientSecretCredential(AzureTenantId, AzureClientId, azureClientSecret);
      var certClient = new CertificateClient(new Uri(AzureKeyVaultUrl), credential);
      var azureCert = (await certClient.GetCertificateAsync(AzureKeyVaultCertificateName)).Value;
      publicCertificate = new X509Certificate2(azureCert.Cer);
      keyVault = RSAFactory.Create(credential, azureCert.KeyId, publicCertificate);
      var signers = new List<AuthenticodeKeyVaultSigner>(timestampServers.Length);
      foreach (var timeStampUrl in timestampServers)
      {
        var timeStampConfiguration = new TimeStampConfiguration(timeStampUrl, HashAlgorithmName.SHA256, TimeStampType.RFC3161);
        var signer = new AuthenticodeKeyVaultSigner(keyVault, publicCertificate, HashAlgorithmName.SHA256, timeStampConfiguration);
        signers.Add(signer);
      }
      return signers;
    }

    static ReadOnlySpan<char> ToReadOnlySpan(string value)
    {
      return value != null ? new ReadOnlySpan<char>(value.ToCharArray(), 0, value.Length) : default;
    }

    public void Dispose()
    {
      foreach (var signerTask in signerTasks?.Result ?? Array.Empty<AuthenticodeKeyVaultSigner>().ToList())
      {
        signerTask?.Dispose();
      }
      keyVault?.Dispose();
      publicCertificate?.Dispose();
    }

    Task<List<AuthenticodeKeyVaultSigner>> signerTasks;
    RSA keyVault;
    X509Certificate2 publicCertificate;

    readonly object syncRoot = new object();
    readonly string azureClientSecret;

    const string AzureKeyVaultUrl = "https://kv-wtg-code-signing-prod.vault.azure.net";
    const string AzureTenantId = "8b493985-e1b4-4b95-ade6-98acafdbdb01";
    const string AzureClientId = "38f2e48d-4345-4505-b1e7-37e8848532cd";
    const string AzureKeyVaultCertificateName = "dat-code-signing";
    static readonly ImmutableArray<string> timestampServers = ImmutableArray.Create(
      "http://timestamp.digicert.com",
      "http://timestamp.comodoca.com/rfc3161",
      "http://timestamp.sectigo.com");
  }
}