using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Utils.Test;

[TestFixture]
class CertificateInfoManagerFixture
{
	[Test]
	public async Task Test_RenewalCertificate_Failed()
	{
		await using var sw = new StringWriter();
		Console.SetOut(sw);
		var mockRSA = new Mock<RSA>().Object;
		var certificateRenewal = new Mock<ICertificateRenewal>();
		
		using var certificate = new X509Certificate2(testCertificatePath);
		using var certificateInfoManager = new CertificateInfoManager(mockRSA, certificate, testCertificatePath, certificateRenewal.Object);
		var clientId = "test_clientId";
		var accessToken = "test_accessToken";

		certificateRenewal.Setup(x => x.GetNewCertificateContentAsync()).ThrowsAsync(new InvalidOperationException());

		Assert.ThrowsAsync<InvalidOperationException>(async () =>
			await certificateInfoManager.RenewalCertificateAsync(clientId, accessToken));
	}

	[Test]
	public async Task Test_RenewalCertificate_Succeed()
	{
		await using var sw = new StringWriter();
		Console.SetOut(sw);
		var mockRSA = new Mock<RSA>().Object;
		var certificateRenewal = new Mock<ICertificateRenewal>();
		var testCertificatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestCertificate.cer");
		using var certificate = new X509Certificate2(testCertificatePath);
		using var certificateInfoManager = new CertificateInfoManager(mockRSA, certificate, testCertificatePath, certificateRenewal.Object);
		var clientId = "test_clientId";
		var accessToken = "test_accessToken";

		certificateRenewal.Setup(x => x.GetNewCertificateContentAsync()).Returns(Task.FromResult(new byte[]{0x1, 0x2, 0x3}));
		await certificateInfoManager.RenewalCertificateAsync(clientId, accessToken);

		Assert.That(sw.ToString(),
			Does.Contain("Get Certificate Content succeeded."));
		Assert.That(sw.ToString(),
			Does.Contain("Write to Certificate succeeded."));
	}

	[SetUp]
	public void SetUp()
	{
		defaultOutput = Console.Out;
		testCertificatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestCertificate.cer");
	}

	[TearDown]
	public void TearDown()
	{
		Console.SetOut(defaultOutput);

		var newCertificatePath = $"{testCertificatePath}_new";
		if (File.Exists(newCertificatePath))
		{
			File.Delete(newCertificatePath);
		}
		var oldCertificatePath = $"{testCertificatePath}_old";
		if (File.Exists(oldCertificatePath))
		{
			File.Delete(oldCertificatePath);
		}
	}

	TextWriter defaultOutput;

	string testCertificatePath;
}
