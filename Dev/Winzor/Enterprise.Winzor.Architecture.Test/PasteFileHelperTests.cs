using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Winzor;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorFramework.JSInterop;

namespace Enterprise.Winzor.Architecture.Test;

public class PasteFileHelperTests
{
	[Test]
	public async Task ClipboardDataAsync_WhenClipboardIsEmpty_ShouldReturnEmptyDataObject()
	{
		var fileServiceMock = new Mock<IFileService>();
		var helper = new PasteFileHelper(fileServiceMock.Object);
		var args = new WinzorPasteEventArgs();

		var result = await helper.GetClipboardDataAsync(args);

		Assert.That(result, Is.Not.Null);
		Assert.That(result.GetDataPresent(DataFormats.Text), Is.False);
		Assert.That(result.GetDataPresent(DataFormats.Html), Is.False);
		Assert.That(result.GetDataPresent(DataFormats.FileDrop), Is.False);
	}

	[Test]
	public async Task GetClipboardDataAsync_WhenClipboardContainsText_ShouldReturnDataObjectWithText()
	{
		var fileServiceMock = new Mock<IFileService>();
		var helper = new PasteFileHelper(fileServiceMock.Object);
		var args = new WinzorPasteEventArgs
		{
			Text = "Sample text"
		};

		var result = await helper.GetClipboardDataAsync(args);

		Assert.That(result, Is.Not.Null);
		Assert.That(result.GetDataPresent(DataFormats.Text), Is.True);
		Assert.That(result.GetDataPresent(DataFormats.Html), Is.False);
		Assert.That(result.GetDataPresent(DataFormats.FileDrop), Is.False);
	}

	[Test]
	public async Task GetClipboardDataAsync_WhenClipboardContainsHtml_ShouldReturnDataObjectWithHtml()
	{
		var fileServiceMock = new Mock<IFileService>();
		var helper = new PasteFileHelper(fileServiceMock.Object);
		var args = new WinzorPasteEventArgs
		{
			Html = "<p>Sample HTML</p>"
		};

		var result = await helper.GetClipboardDataAsync(args);

		Assert.That(result, Is.Not.Null);
		Assert.That(result.GetDataPresent(DataFormats.Html), Is.True);
		Assert.That(result.GetDataPresent(DataFormats.Text), Is.False);
		Assert.That(result.GetDataPresent(DataFormats.FileDrop), Is.False);
	}

	[Test]
	public async Task GetClipboardDataAsync_WhenClipboardContainsFiles_ShouldReturnDataObjectWithFileDrop()
	{
		var fileServiceMock = new Mock<IFileService>();

		fileServiceMock
			.Setup(fs => fs.UploadFilesToServerAsync(It.IsAny<BrowserFile[]>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((BrowserFile[] files, long _, CancellationToken _) =>
			{
				return files.Select(f => f.Name).ToArray();
			});

		var helper = new PasteFileHelper(fileServiceMock.Object);

		var args = new WinzorPasteEventArgs
		{
			Files = new BrowserFile[]
			{
					new BrowserFile { Name = "file1.txt", Size = 1024 },
					new BrowserFile { Name = "file2.txt", Size = 2048 }
			}
		};

		var result = await helper.GetClipboardDataAsync(args);

		Assert.That(result, Is.Not.Null);
		Assert.That(result.GetDataPresent(DataFormats.FileDrop), Is.True);
		Assert.That(result.GetDataPresent(DataFormats.Text), Is.False);
		Assert.That(result.GetDataPresent(DataFormats.Html), Is.False);
	}

	[Test]
	public async Task GetClipboardDataAsync_WhenFileServiceIsNull_ShouldReturnEmptyDataObject()
	{
		var helper = new PasteFileHelper(null);

		var args = new WinzorPasteEventArgs
		{
			Files = new BrowserFile[]
			{
					new BrowserFile { Name = "file1.txt", Size = 1024 },
					new BrowserFile { Name = "file2.txt", Size = 2048 }
			}
		};

		var result = await helper.GetClipboardDataAsync(args);

		Assert.That(result, Is.Not.Null);
		Assert.That(result.GetDataPresent(DataFormats.Text), Is.False);
		Assert.That(result.GetDataPresent(DataFormats.Html), Is.False);
		Assert.That(result.GetDataPresent(DataFormats.FileDrop), Is.False);
	}

	[Test]
	public async Task GetClipboardDataAsync_WhenClipboardContainsTextAndHtmlAndFiles_ShouldReturnDataObjectWithTextAndHtmlAndFileDrop()
	{
		var fileServiceMock = new Mock<IFileService>();

		fileServiceMock
			.Setup(fs => fs.UploadFilesToServerAsync(It.IsAny<BrowserFile[]>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((BrowserFile[] files, long _, CancellationToken _) =>
			{
				return files.Select(f => f.Name).ToArray();
			});

		var helper = new PasteFileHelper(fileServiceMock.Object);

		var args = new WinzorPasteEventArgs
		{
			Text = "Sample text",
			Html = "<p>Sample HTML</p>",
			Files = new BrowserFile[]
			{
					new BrowserFile { Name = "file1.txt", Size = 1024 },
					new BrowserFile { Name = "file2.txt", Size = 2048 }
			}
		};

		var result = await helper.GetClipboardDataAsync(args);

		Assert.That(result, Is.Not.Null);
		Assert.That(result.GetDataPresent(DataFormats.Text), Is.True);
		Assert.That(result.GetDataPresent(DataFormats.Html), Is.True);
		Assert.That(result.GetDataPresent(DataFormats.FileDrop), Is.True);
	}
}
