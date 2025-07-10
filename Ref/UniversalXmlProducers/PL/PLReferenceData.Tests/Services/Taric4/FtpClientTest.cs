using System;
using System.IO;
using System.Net;
using System.Text;
using CargoWise.RefDbRepo.PLReferenceData.Services.Taric4;
using CargoWise.RefDbRepo.PLReferenceData.Services.Taric4.Interfaces;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Services.Taric4;

[TestFixture]
sealed class FtpClientTest
{
	const string TestFtpAddress = "ftp://example.com:22";

	[Test]
	public void TestCreateDirectory()
	{
		const string testDirectory = "testDirectory";

		testFtpClient.CreateDirectory(testDirectory);

		var expectedUri = new Uri($"{TestFtpAddress}/{testDirectory}");
		ftpRequestFactoryMock.Verify(f => f.CreateAndRun(
			It.Is<Uri>(uri => uri == expectedUri),
			It.Is<string>(ftpMethod => ftpMethod == WebRequestMethods.Ftp.MakeDirectory)));
	}

	[Test]
	public void TestGetDirectoryContent()
	{
		const string testDirectory = "testDirectory";
		const string testDirectoryContent = "testFile.txt\r\n\r\nanotherFile.txt\r\n";
		ftpWebResponseMock.Setup(f => f.GetResponseStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes(testDirectoryContent)));

		var result = testFtpClient.GetDirectoryContent(testDirectory);

		var expectedUri = new Uri($"{TestFtpAddress}/{testDirectory}");
		ftpRequestFactoryMock.Verify(f => f.CreateAndRun(
			It.Is<Uri>(uri => uri == expectedUri),
			It.Is<string>(ftpMethod => ftpMethod == WebRequestMethods.Ftp.ListDirectory)));
		Assert.Multiple(() =>
		{
			Assert.That(result, Is.Not.Null, "Result is not null.");
			Assert.That(result.Length, Is.EqualTo(2), "Result contains only two files. Empty entries are removed.");
			Assert.That(result[0], Is.EqualTo("testFile.txt"), "First file name is correct.");
			Assert.That(result[1], Is.EqualTo("anotherFile.txt"), "Second file name is correct.");
		});
	}

	[Test]
	public void TestDownloadFile()
	{
		const string testFtpDirectory = "testDirectory";
		const string testFileName = "testFile.txt";
		const string testFileContent = "testContent";
		var tempDirectory = Path.GetTempPath();
		var testFilePath = Path.Combine(tempDirectory, testFileName);
		ftpWebResponseMock.Setup(f => f.GetResponseStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes(testFileContent)));
		try
		{
			testFtpClient.DownloadFile(testFileName, testFtpDirectory, tempDirectory);

			var expectedUri = new Uri($"{TestFtpAddress}/{testFtpDirectory}/{testFileName}");
			ftpRequestFactoryMock.Verify(f => f.CreateAndRun(
				It.Is<Uri>(uri => uri == expectedUri),
				It.Is<string>(ftpMethod => ftpMethod == WebRequestMethods.Ftp.DownloadFile)));
			Assert.Multiple(() =>
			{
				Assert.That(File.Exists(testFilePath), Is.True, "File downloaded successfully.");
				Assert.That(File.ReadAllText(testFilePath), Is.EqualTo(testFileContent), "File contains expected content.");
			});
		}
		finally
		{
			if (File.Exists(testFilePath))
			{
				File.Delete(testFilePath);
			}
		}
	}

	[Test]
	public void TestDeleteFile()
	{
		const string testDirectory = "testDirectory";
		const string testFileName = "testFile.txt";

		testFtpClient.DeleteFile(testFileName, testDirectory);

		var expectedUri = new Uri($"{TestFtpAddress}/{testDirectory}/{testFileName}");
		ftpRequestFactoryMock.Verify(f => f.CreateAndRun(
			It.Is<Uri>(uri => uri == expectedUri),
			It.Is<string>(ftpMethod => ftpMethod == WebRequestMethods.Ftp.DeleteFile)));
	}

	[SetUp]
	public void SetUp()
	{
		ftpRequestFactoryMock = new Mock<IFtpRequestFactory>();
		ftpWebResponseMock = new Mock<IFtpWebResponse>();
		ftpRequestFactoryMock.Setup(f => f.CreateAndRun(It.IsAny<Uri>(), It.IsAny<string>())).Returns(ftpWebResponseMock.Object);
		testFtpClient = new FtpClient(TestFtpAddress, ftpRequestFactoryMock.Object);
	}

	Mock<IFtpRequestFactory> ftpRequestFactoryMock;
	Mock<IFtpWebResponse> ftpWebResponseMock;
	FtpClient testFtpClient;
}
