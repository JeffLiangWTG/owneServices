using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;
internal sealed class ZOpenFileDialogTest
{
	[Test, WithPlaywrightPage]
	public async Task ZOpenFileDialogCallsInvokesFileOk()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var fileOk = new TaskCompletionSource();

		Form form = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			return form;
		});

		var fileServiceMock = GenerateMockFileService();
		form.CargoWiseClientServices.FileService = fileServiceMock.Object;

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var dialog = new ZOpenFileDialog();
			dialog.FileOk += (_, _) => fileOk.SetResult();
			dialog.ShowDialog();
		});

		Assert.That(await fileOk.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
	}

	[Test, WithPlaywrightPage]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1090:DoNotUseSystemWindowsFormsDialogs", Justification = "Testing")]
	public async Task ZOpenFileDialogFileListIsLocalFileLocations()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZOpenFileDialog dialog = null;
		Form form = null;
		var fileDialogOpened = new TaskCompletionSource<bool>();

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			dialog = new ZOpenFileDialog();
			var button = new Button() { Text = "Open Dialog" };
			button.Click += (sender, args) =>
			{
				dialog.ShowDialog();
				fileDialogOpened.SetResult(true);
			};
			form.Controls.Add(button);
			return form;
		});

		await page.WaitForSelectorAsync(".form");

		var fileServiceMock = GenerateMockFileService();
		form.CargoWiseClientServices.FileService = fileServiceMock.Object;

		await (await page.WaitForSelectorAsync("button")).ClickAsync();
		Assert.That(await fileDialogOpened.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);

		await form.InvokeWinzorDispatcherAsync(() => dialog.ForceLocalFiles());
		var filesSelected = dialog.SelectedFiles.Select(filename => filename.UnmappedFileName).ToArray();
		Assert.That(filesSelected, Is.Not.Empty);
		Assert.That(filesSelected, Does.Contain("LocalFileName1"));
		Assert.That(filesSelected, Does.Contain("LocalFileName2"));
		Assert.That(filesSelected, Does.Contain("LocalFileName3"));
	}

	[Test, WithPlaywrightPage]
	public async Task ZOpenFileDialogShouldValidateLargeFiles()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZOpenFileDialog dialog = null;
		Form form = null;
		var fileDialogOpened = new TaskCompletionSource<bool>();

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			dialog = new ZOpenFileDialog();
			var button = new Button() { Text = "Open Dialog" };
			button.Click += (sender, args) =>
			{
				var result = dialog.ShowDialog();
				if (result == DialogResult.OK)
				{
					dialog.ForceLocalFiles();
				}
				fileDialogOpened.SetResult(true);
			};
			form.Controls.Add(button);
			return form;
		});

		var fileServiceMock = GenerateMockFileService(1, long.MaxValue);
		form.CargoWiseClientServices.FileService = fileServiceMock.Object;

		await (await page.WaitForSelectorAsync("button")).ClickAsync();
		var isFileDialogOpened = await fileDialogOpened.Task;
		Assert.That(isFileDialogOpened, Is.True);
		Assert.That(UnitTestUserNotification.Instance.LastMessage.Text, Is.EqualTo($"The following files are larger than the maximum file size ({SystemDataRegistry.Instance.eDocsMaximumFilesize.Value}MB) specified in the registry 'System -> DocManager -> eDocs Maximum File Size':\r\nLocalFileName1\r\n"));
	}

	[Test]
	public async Task ZOpenFileDialogOpenFileShouldEnsureFilesExisted()
	{
		Stream stream = null;
		using var tempFile = TempFile.New();
		var fileServiceMock = GenerateMockFileServiceWithTempFile(tempFile);

		using var ctx = new EnterpriseTestContext();

		var (dialog, form) = await ShowZOpenFileDialog(ctx, fileServiceMock);

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			stream = dialog.OpenFile();
		});
		Assert.That(stream, Is.Not.Null);
		await stream.DisposeAsync();
	}

	[Test]
	public async Task ZOpenFileDialogUnmappedFileNameShouldEnsureFilesExisted()
	{
		var fileServiceMock = GenerateMockFileService();

		using var ctx = new EnterpriseTestContext();

		var (dialog, form) = await ShowZOpenFileDialog(ctx, fileServiceMock);

		var selectFileName = string.Empty;
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			selectFileName = dialog.UnmappedFileName;
		});
		Assert.That(selectFileName, Is.EqualTo("LocalFileName1"));
	}

	[Test]
	public async Task ZOpenFileDialogUnmappedFileNameShouldEnsureFilesExisted_ReopenDialog()
	{
		using var ctx = new EnterpriseTestContext();

		var fileServiceMock1 = GenerateMockFileService(fileCount: 1);
		var fileServiceMock2 = GenerateMockFileService();

		var (dialog1, form1) = await ShowZOpenFileDialog(ctx, fileServiceMock1);
		ZOpenFileDialog.FileInfo[] selectedFiles1 = null;
		await form1.InvokeWinzorDispatcherAsync(() =>
		{
			selectedFiles1 = dialog1.SelectedFiles;
		});
		Assert.That(selectedFiles1.Count, Is.EqualTo(1));

		var (dialog2, form2) = await ShowZOpenFileDialog(ctx, fileServiceMock2, dialog1);
		ZOpenFileDialog.FileInfo[] selectedFiles = null;
		await form2.InvokeWinzorDispatcherAsync(() =>
		{
			selectedFiles = dialog2.SelectedFiles;
		});
		Assert.That(selectedFiles.Count, Is.EqualTo(3));
	}

	[Test]
	public async Task ZOpenFileDialogSelectedFilesShouldEnsureFilesExisted()
	{
		var fileServiceMock = GenerateMockFileService();

		using var ctx = new EnterpriseTestContext();

		var (dialog, form) = await ShowZOpenFileDialog(ctx, fileServiceMock);

		ZOpenFileDialog.FileInfo[] selectedFiles = null;
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			selectedFiles = dialog.SelectedFiles;
		});
		Assert.That(selectedFiles.Count, Is.EqualTo(3));
	}

	async Task<(ZOpenFileDialog dialog, Form form)> ShowZOpenFileDialog(EnterpriseTestContext ctx, Mock<IFileService> fileServiceMock, ZOpenFileDialog dialog = null)
	{
		Form form = null;
		var fileDialogOpened = new TaskCompletionSource<bool>();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			dialog ??= new ZOpenFileDialog();
			var button = new Button() { Text = "Open Dialog" };
			button.Click += (sender, args) =>
			{
				dialog.ShowDialog();
				fileDialogOpened.SetResult(true);
			};
			form.Controls.Add(button);
			return form;
		});
		form.CargoWiseClientServices.FileService = fileServiceMock.Object;

		await rendered.Find("button").ClickAsync(new WebMouseEventArgs());
		Assert.That(await fileDialogOpened.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);
		return (dialog, form);
	}

	Mock<IFileService> GenerateMockFileService(int fileCount = 3, long size = 0)
	{
		var fileServiceMock = new Mock<IFileService>();

		var fileNames = Enumerable.Range(1, fileCount).Select(i => $"LocalFileName{i}").ToArray();
		fileServiceMock.Setup(i => i.ShowOpenFilePickerAsync(It.IsAny<bool>(), null)).Returns(() =>
		{
			return Task.FromResult(fileNames.Select(fileName => new BrowserFile() { Name = fileName, Size = size }).ToArray());
		});
		fileServiceMock.Setup(i => i.UploadFilesToServerAsync(It.IsAny<BrowserFile[]>(), It.IsAny<long>(), It.IsAny<CancellationToken>())).Returns(() =>
		{
			return Task.FromResult(fileNames);
		});
		return fileServiceMock;
	}

	Mock<IFileService> GenerateMockFileServiceWithTempFile(TempFile tempFile)
	{
		var fileServiceMock = new Mock<IFileService>();
		fileServiceMock.Setup(i => i.ShowOpenFilePickerAsync(It.IsAny<bool>(), null)).Returns(() =>
		{
			return Task.FromResult<BrowserFile[]>([new BrowserFile() { Name = "LocalFileName1" }]);
		});
		fileServiceMock.Setup(i => i.UploadFilesToServerAsync(It.IsAny<BrowserFile[]>(), It.IsAny<long>(), It.IsAny<CancellationToken>())).Returns(() =>
		{
			return Task.FromResult<string[]>([tempFile.Filename]);
		});
		return fileServiceMock;
	}
}
