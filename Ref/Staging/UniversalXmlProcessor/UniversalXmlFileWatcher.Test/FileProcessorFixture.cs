using System.Runtime.Versioning;
using System.Security.AccessControl;
using CargoWise.RefDbRepo.UniversalXmlFileWatcher.Configuration;
using CargoWise.RefDbRepo.UniversalXmlFileWatcher.Interfaces;
using CargoWise.RefDbRepo.UniversalXmlFileWatcher.Utilities;
using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;
using Moq;
using NUnit.Framework;
using Unity;

namespace CargoWise.RefDbRepo.UniversalXmlFileWatcher.Tests;

[TestFixture]
class FileProcessorFixture
{
	[TestCase]
	public async Task ScanFolders()
	{
		var tmpPath = Path.GetTempPath();
		Application.ConfigCoreEnvironment(tmpPath);
		var container = Application.UnityContainer;
		var sourceDataWriter = new Mock<ISourceDataWriter>();
		var parser = new Mock<IUniversalXmlParser>();
		using var innerContainer = new UnityContainer();

		var factory = new Mock<IContainerFactory>();
		factory.Setup(x => x.Create(It.IsAny<string>())).Returns(innerContainer);
		innerContainer.RegisterInstance(sourceDataWriter.Object);
		innerContainer.RegisterInstance(parser.Object);
		container.RegisterInstance(factory.Object);
		var tmpFile = Path.GetTempFileName();
		var processor = new FileProcessor
		{
			FoldersToScan = [tmpPath],
			ArchiveFolder = tmpPath,
			ErrorFolder = tmpPath,
			FileExtensionToMonitor = Path.GetFileName(tmpFile),
			LastRunTimeFile = AppConfig.Configuration["LastRunTimeFile"] ?? "FileWatcher_LastRunTime.cfg"
		};
		await processor.ScanFolders();
		parser.Verify(x => x.ParseAsync(It.IsAny<Stream>(), false));
		File.Delete(tmpFile);
	}

	[Test]
	public async Task ScanFolders_OnlyOneFileIsParsed()
	{
		var tmpPath = Path.GetTempPath();
		Application.ConfigCoreEnvironment(tmpPath);
		var container = Application.UnityContainer;
		var sourceDataWriter = new Mock<ISourceDataWriter>();
		var parser = new Mock<IUniversalXmlParser>();
		using var innerContainer = new UnityContainer();

		var factory = new Mock<IContainerFactory>();
		factory.Setup(x => x.Create(It.IsAny<string>())).Returns(innerContainer);
		innerContainer.RegisterInstance(sourceDataWriter.Object);
		innerContainer.RegisterInstance(parser.Object);
		container.RegisterInstance(factory.Object);
		var forceParseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestForceParse");
		var parseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestParse");
		var processor = new FileProcessor
		{
			FoldersToScan = [forceParseFolder, parseFolder],
			ArchiveFolder = tmpPath,
			ErrorFolder = tmpPath,
			FileExtensionToMonitor = "*.xml",
			LastRunTimeFile = AppConfig.Configuration["LastRunTimeFile"] ?? "FileWatcher_LastRunTime.cfg"
		};
		await processor.ScanFolders();
		parser.Verify(x => x.ParseAsync(It.IsAny<Stream>(), It.IsAny<bool>()), Times.Once);
	}

	[Test]
	public async Task ScanFolders_OnlyOneFileIsParsed_EvenFailed()
	{
		var tmpPath = Path.GetTempPath();
		Application.ConfigCoreEnvironment(tmpPath);
		var container = Application.UnityContainer;
		var sourceDataWriter = new Mock<ISourceDataWriter>();
		var parser = new Mock<IUniversalXmlParser>();
		parser.Setup(x => x.ParseAsync(It.IsAny<Stream>(), It.IsAny<bool>()))
			.Throws(new UnauthorizedAccessException("Test exception"));
		using var innerContainer1 = new UnityContainer();
		using var innerContainer2 = new UnityContainer();

		var factory = new Mock<IContainerFactory>();
		factory.SetupSequence(x => x.Create(It.IsAny<string>())).Returns(innerContainer1).Returns(innerContainer2);
		innerContainer1.RegisterInstance(sourceDataWriter.Object);
		innerContainer1.RegisterInstance(parser.Object);
		innerContainer2.RegisterInstance(sourceDataWriter.Object);
		innerContainer2.RegisterInstance(parser.Object);
		container.RegisterInstance(factory.Object);
		var parseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestParse");
		var processor = new FileProcessor
		{
			FoldersToScan = [parseFolder],
			ArchiveFolder = tmpPath,
			ErrorFolder = tmpPath,
			FileExtensionToMonitor = "*.xml",
			LastRunTimeFile = AppConfig.Configuration["LastRunTimeFile"] ?? "FileWatcher_LastRunTime.cfg"
		};
		await processor.ScanFolders();
		parser.Verify(x => x.ParseAsync(It.IsAny<Stream>(), It.IsAny<bool>()), Times.Once);
	}

	[SupportedOSPlatform("windows")]
	[TestCase]
	public void ScanFolders_NoExceptionThrown()
	{
		var tmpPath = Path.GetTempPath();
		Application.ConfigCoreEnvironment(tmpPath);
		var container = Application.UnityContainer;
		var sourceDataWriter = new Mock<ISourceDataWriter>();
		var parser = new Mock<IUniversalXmlParser>();
		using var innerContainer = new UnityContainer();

		var factory = new Mock<IContainerFactory>();
		factory.Setup(x => x.Create(It.IsAny<string>())).Returns(innerContainer);
		innerContainer.RegisterInstance(sourceDataWriter.Object);
		innerContainer.RegisterInstance(parser.Object);
		container.RegisterInstance(factory.Object);
		var tmpFile = Path.GetTempFileName();
		File.WriteAllText(tmpFile, "123456789");
		var processor = new FileProcessor
		{
			FoldersToScan = [tmpPath],
			ArchiveFolder = tmpPath,
			ErrorFolder = tmpPath,
			FileExtensionToMonitor = Path.GetFileName(tmpFile),
			LastRunTimeFile = AppConfig.Configuration["LastRunTimeFile"] ?? "FileWatcher_LastRunTime.cfg"
		};
		var fileInfo = new FileInfo(tmpFile);
		var originFileSecurity = fileInfo.GetAccessControl();
		var fileSecurity = originFileSecurity;
		var accessRule = new FileSystemAccessRule(Environment.UserName, FileSystemRights.FullControl, AccessControlType.Deny);
		fileSecurity.AddAccessRule(accessRule);
		fileInfo.SetAccessControl(fileSecurity);
		Assert.DoesNotThrowAsync(processor.ScanFolders);
		fileInfo.SetAccessControl(originFileSecurity);
		if (File.Exists(tmpFile))
		{
			File.Delete(tmpFile);
		}
	}

	[TestCase]
	public void DeleteOverdueFiles()
	{
		var tmpArchivePath = Path.GetTempPath();
		tmpArchivePath = Path.Combine(tmpArchivePath, "Archive_DeleteFolder");
		if (!Directory.Exists(tmpArchivePath))
		{
			Directory.CreateDirectory(tmpArchivePath);
		}

		var tmpErrorPath = Path.GetTempPath();
		tmpErrorPath = Path.Combine(tmpErrorPath, "Error_DeleteFolder");
		if (!Directory.Exists(tmpErrorPath))
		{
			Directory.CreateDirectory(tmpErrorPath);
		}

		Application.ConfigCoreEnvironment(tmpArchivePath);

		var now = DateTime.UtcNow;
		var tmpXmlFile = Path.Combine(tmpArchivePath, "TestFile_Delete.xml");
		var tmpTxtFile = Path.Combine(tmpArchivePath, "TestFile_NotDelete.txt");
		var lastRunTimeFile = Path.Combine(tmpArchivePath, "LastRunTime.txt");
		File.WriteAllText(tmpXmlFile, "1234567890");
		File.SetLastWriteTimeUtc(tmpXmlFile, now.AddMonths(-13));
		File.WriteAllText(tmpTxtFile, "1234567890");
		File.SetLastWriteTimeUtc(tmpTxtFile, now.AddMonths(-11));
		File.WriteAllText(lastRunTimeFile, "1234567890");
		File.SetLastWriteTimeUtc(lastRunTimeFile, now.AddDays(-32));

		var tmpErrorXmlFile = Path.Combine(tmpErrorPath, "TestFile_Delete.xml");
		var tmpErrorTxtFile = Path.Combine(tmpErrorPath, "TestFile_NotDelete.txt");
		var lastRunTimeErrorFile = Path.Combine(tmpErrorPath, "LastRunTime.txt");
		File.WriteAllText(tmpErrorXmlFile, "1234567890");
		File.SetLastWriteTimeUtc(tmpErrorXmlFile, now.AddMonths(-13));
		File.WriteAllText(tmpErrorTxtFile, "1234567890");
		File.SetLastWriteTimeUtc(tmpErrorTxtFile, now.AddMonths(-11));
		File.WriteAllText(lastRunTimeErrorFile, "1234567890");
		File.SetLastWriteTimeUtc(lastRunTimeErrorFile, now.AddDays(-32));

		var processor = new FileProcessor
		{
			FoldersToScan = [],
			FileExtensionToMonitor = "*.xml",
			ArchiveFolder = tmpArchivePath,
			ErrorFolder = tmpErrorPath,
			OverdueMonths = 12,
			LastRunTimeFile = lastRunTimeFile,
		};

		processor.DeleteOverdueFiles();
		GC.Collect();
		Assert.AreEqual(false, File.Exists(tmpXmlFile));
		Assert.AreEqual(true, File.Exists(tmpTxtFile));
		var lastRunTime = File.GetLastWriteTimeUtc(lastRunTimeFile);
		Assert.That(now.Subtract(lastRunTime).TotalHours < 1);

		if (Directory.Exists(tmpArchivePath))
		{
			Directory.Delete(tmpArchivePath, true);
		}

		Assert.AreEqual(false, File.Exists(tmpErrorXmlFile));
		Assert.AreEqual(true, File.Exists(tmpErrorTxtFile));

		if (Directory.Exists(tmpErrorPath))
		{
			Directory.Delete(tmpErrorPath, true);
		}
	}

	[Test]
	public void DeleteOverdueFiles_NoExceptionThrown()
	{
		var tmpPath = Path.GetTempPath();
		var lastRunTimeFile = Path.GetTempFileName();
		Application.ConfigCoreEnvironment(tmpPath);

		var processor = new FileProcessor
		{
			FoldersToScan = [],
			FileExtensionToMonitor = "*.xml",
			ArchiveFolder = tmpPath,
			ErrorFolder = tmpPath,
			OverdueMonths = 12,
			LastRunTimeFile = lastRunTimeFile
		};
		using var stream = new FileStream(lastRunTimeFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

		Assert.DoesNotThrow(processor.DeleteOverdueFiles);
	}

	[Test]
	public void ShouldDeleteFiles()
	{
		var tmpFile = Path.GetTempFileName();
		var now = DateTime.UtcNow;
		File.SetLastWriteTimeUtc(tmpFile, now.AddDays(-32));
		Assert.AreEqual(true, FileProcessor.ShouldDeleteFiles(tmpFile));

		File.SetLastWriteTimeUtc(tmpFile, now.AddDays(-28));
		Assert.AreEqual(false, FileProcessor.ShouldDeleteFiles(tmpFile));

		if (File.Exists(tmpFile))
		{
			File.Delete(tmpFile);
		}
	}
}
