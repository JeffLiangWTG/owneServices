using System;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WinzorFramework.JSInterop;

namespace Enterprise.Winzor.Architecture.Test;

public class AcceptableBrowserFileValidatorTests
{
	[Test]
	public void ValidateFiles_WhenNoFiles_ShouldReturnEmptyFilesAndEmptyMessage()
	{
		var files = Array.Empty<BrowserFile>();

		var (validFiles, messages) = AcceptableBrowserFileValidator.ValidateFiles(files);
		Assert.That(validFiles, Is.Empty);
		Assert.That(messages, Is.Empty);
	}

	[Test]
	public void ValidateFiles_WhenNoDangerousOrLargeFiles_ShouldReturnAllFilesAndEmptyMessage()
	{
		var files = new BrowserFile[]
		{
				new BrowserFile { Name = "file1.txt", Size = 1024 },
				new BrowserFile { Name = "file2.txt", Size = 2048 }
		};

		var (validFiles, message) = AcceptableBrowserFileValidator.ValidateFiles(files);
		Assert.That(validFiles, Is.EquivalentTo(files));
		Assert.That(message, Is.Empty);
	}

	[Test]
	public void ValidateFiles_WhenDangerousFilesExist_ShouldReturnValidFilesAndDangerousFilesMessage()
	{
		var files = new BrowserFile[]
		{
				new BrowserFile { Name = "file1.exe", Size = 1024 },
				new BrowserFile { Name = "file2.txt", Size = 2048 },
				new BrowserFile { Name = "file3.msi", Size = 1024 }
		};

		var (validFiles, message) = AcceptableBrowserFileValidator.ValidateFiles(files);
		Assert.That(validFiles, Is.EquivalentTo(new [] { files[1] }));
		Assert.That(message, Is.EqualTo("The following files were not added because they are potentially dangerous file types:\r\nfile1.exe\r\nfile3.msi\r\n"));
	}

	[Test]
	public void ValidateFiles_WhenLargeFilesExist_ShouldReturnValidFilesAndLargeFilesMessage()
	{
		var files = new BrowserFile[]
		{
				new BrowserFile { Name = "file1.txt", Size = 1024 },
				new BrowserFile { Name = "file2.txt", Size = 12 * 1024 * 1024 },
				new BrowserFile { Name = "file3.png", Size = 11 * 1024 * 1024 }
		};

		var (validFiles, message) = AcceptableBrowserFileValidator.ValidateFiles(files);
		Assert.That(validFiles, Is.EquivalentTo(new[] { files[0] }));
		Assert.That(message, Is.EqualTo("The following files are larger than the maximum file size (10MB) specified in the registry 'System -> DocManager -> eDocs Maximum File Size':\r\nfile2.txt\r\nfile3.png\r\n"));
	}

	[Test]
	public void ValidateFiles_WhenLargeFilesAndDangerousFilesExist_ShouldReturnEmptyFilesAndMessageContainingBoth()
	{
		var files = new BrowserFile[]
		{
				new BrowserFile { Name = "file1.exe", Size = 1024 },
				new BrowserFile { Name = "file2.txt", Size = 12 * 1024 * 1024 }
		};

		var (validFiles, message) = AcceptableBrowserFileValidator.ValidateFiles(files);
		Assert.That(validFiles, Is.Empty);
		Assert.That(message, Is.EqualTo("The following files were not added because they are potentially dangerous file types:\r\nfile1.exe\r\n\r\nThe following files are larger than the maximum file size (10MB) specified in the registry 'System -> DocManager -> eDocs Maximum File Size':\r\nfile2.txt\r\n"));
	}
}
