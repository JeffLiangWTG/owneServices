using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Utils.Test;

[TestFixture]
class FileReaderFixture
{
	[Test]
	public async Task ReadCsrFileAsync_WhenCsrFileExists_ReturnsContent()
	{
		var tempDir = Path.GetTempPath();
		var certificateName = Guid.NewGuid().ToString();
		var certificatePath = Path.Combine(tempDir, $"{certificateName}.cer");
		var csrFilePath = Path.ChangeExtension(certificatePath, "csr");
		var expectedContent = "CSR_CONTENT";

		await File.WriteAllTextAsync(csrFilePath, expectedContent);
		var reader = new FileReader(csrFilePath);

		try
		{
			var result = await reader.ReadAllTextAsync();
			Assert.That(result, Is.EqualTo(expectedContent));
		}
		finally
		{
			if (File.Exists(csrFilePath))
			{
				File.Delete(csrFilePath);
			}
		}
	}

	[Test]
	public void ReadCsrFileAsync_WhenCsrFileDoesNotExist_ThrowsException()
	{
		var certificateName = Guid.NewGuid().ToString();
		var nonExistentPath = Path.Combine(Path.GetTempPath(), $"{certificateName}.csr");
		var reader = new FileReader(nonExistentPath);

		var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
			await reader.ReadAllTextAsync());

		var expectedCsrFileName = Path.GetFileName(nonExistentPath);
		Assert.That(ex!.Message, Does.Contain(expectedCsrFileName));
	}

	[Test]
	public async Task ReadCsrFileAsync_WhenCertificatePathHasNoExtension_HandlesCorrectly()
	{
		var tempDir = Path.GetTempPath();
		var certificateName = Guid.NewGuid().ToString();
		var certificatePath = Path.Combine(tempDir, certificateName);
		var csrFilePath = certificatePath + ".csr";
		var expectedContent = "CSR_CONTENT";

		await File.WriteAllTextAsync(csrFilePath, expectedContent);
		var reader = new FileReader(csrFilePath);

		try
		{
			Assert.DoesNotThrowAsync(async () => await reader.ReadAllTextAsync());
			var result = await reader.ReadAllTextAsync();
			Assert.That(result, Is.EqualTo(expectedContent));
		}
		finally
		{
			if (File.Exists(csrFilePath))
			{
				File.Delete(csrFilePath);
			}
		}
	}
}
