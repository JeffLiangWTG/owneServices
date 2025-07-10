using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bunit;
using Microsoft.JSInterop;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

using static PlaywrightTestContext;

internal class OpenFileDialogTest
{
	[Test, WithPlaywrightPage]
	[Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1090:DoNotUseSystemWindowsFormsDialogs", Justification = "Testing")]
	public async Task OpenFileDialogOpensFilePicker()
	{
		await using var ctx = new InMemoryTestServerContext();
		OpenFileDialog dialog = null;
		Form form = null;
		var dialogClosed = new TaskCompletionSource<bool>();

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			dialog = new OpenFileDialog();
			var button = new Button() { Text = "Open Dialog" };
			button.Click += (sender, args) =>
			{
				dialog.ShowDialog();
				dialogClosed.SetResult(true);
			};
			form.Controls.Add(button);
			return form;
		});

		await page.WaitForSelectorAsync(".form");

		var fileDialogOpened = new TaskCompletionSource<bool>();
		var fileServiceMock = new Mock<IFileService>();
		fileServiceMock.Setup(i => i.ShowOpenFilePickerAsync(It.IsAny<bool>(), null)).Returns(() =>
		{
			fileDialogOpened.SetResult(true);
			return Task.FromResult(Array.Empty<BrowserFile>());
		});
		form.CargoWiseClientServices.FileService = fileServiceMock.Object;

		var button = await page.WaitForSelectorAsync("button");
		await button.ClickAsync();
		Assert.That(await fileDialogOpened.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		Assert.That(await dialogClosed.Task, Is.True);
	}

	[Test, WithPlaywrightPage]
	[Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1090:DoNotUseSystemWindowsFormsDialogs", Justification = "Testing")]
	public async Task OpenFileDialogFileListIsLocalFileLocations()
	{
		await using var ctx = new InMemoryTestServerContext();
		OpenFileDialog dialog = null;
		Form form = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			dialog = new OpenFileDialog();
			var button = new Button() { Text = "Open Dialog" };
			button.Click += (sender, args) => dialog.ShowDialog();
			form.Controls.Add(button);
			return form;
		});

		await page.WaitForSelectorAsync(".form");

		var fileDialogOpened = new TaskCompletionSource<bool>();
		var fileServiceMock = new Mock<IFileService>();
		var fileNames = new[] { "LocalFileName1", "LocalFileName2", "LocalFileName3" };
		fileServiceMock.Setup(i => i.ShowOpenFilePickerAsync(It.IsAny<bool>(), null)).Returns(() =>
		{
			fileDialogOpened.SetResult(true);
			return Task.FromResult(fileNames.Select(fileName => new BrowserFile() { Name = fileName }).ToArray());
		});
		fileServiceMock.Setup(i => i.UploadFilesToServerAsync(It.IsAny<BrowserFile[]>(), It.IsAny<long>(), It.IsAny<CancellationToken>())).Returns(() =>
		{
			return Task.FromResult(fileNames);
		});
		form.CargoWiseClientServices.FileService = fileServiceMock.Object;

		var button = await page.WaitForSelectorAsync("button");
		await button.ClickAsync();
		Assert.That(await fileDialogOpened.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);

		await form.InvokeWinzorDispatcherAsync(() => dialog.ForceLocalFiles(52428800));
		Assert.That(() => dialog.FileNames, Is.Not.Empty.After(1000, 100));
		Assert.That(dialog.FileNames, Does.Contain("LocalFileName1"));
		Assert.That(dialog.FileNames, Does.Contain("LocalFileName2"));
		Assert.That(dialog.FileNames, Does.Contain("LocalFileName3"));
	}

	[Test, WithPlaywrightPage]
	[Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1090:DoNotUseSystemWindowsFormsDialogs", Justification = "Testing")]
	public async Task OpenFileDialogFileListIsLocalFileLocations_ReopenDialog()
	{
		await using var ctx = new InMemoryTestServerContext();
		OpenFileDialog dialog = null;
		Form form = null;

		await OpenFileDialog("LocalFileName1");
		await form.InvokeWinzorDispatcherAsync(() => dialog.ForceLocalFiles(52428800));
		Assert.That(() => dialog.FileNames, Is.Not.Empty.After(1000, 100));
		Assert.That(dialog.FileNames, Does.Contain("LocalFileName1"));
		Assert.That(dialog.FileNames, Does.Not.Contain("LocalFileName2"));
		Assert.That(dialog.FileNames, Does.Not.Contain("LocalFileName3"));

		await OpenFileDialog("LocalFileName2", "LocalFileName3");
		Assert.That(() => dialog.FileNames, Is.Empty.After(1000, 100));

		await form.InvokeWinzorDispatcherAsync(() => dialog.ForceLocalFiles(52428800));
		Assert.That(() => dialog.FileNames, Is.Not.Empty.After(1000, 100));
		Assert.That(dialog.FileNames, Does.Not.Contain("LocalFileName1"));
		Assert.That(dialog.FileNames, Does.Contain("LocalFileName2"));
		Assert.That(dialog.FileNames, Does.Contain("LocalFileName3"));

		async Task OpenFileDialog(params string[] fileNames)
		{
			var page = await ctx.LoadFormAsync(() =>
			{
				form = new Form();
				dialog ??= new OpenFileDialog();
				var button = new Button() { Text = "Open Dialog" };
				button.Click += (sender, args) => dialog.ShowDialog();
				form.Controls.Add(button);
				return form;
			});

			await page.WaitForSelectorAsync(".form");

			var fileDialogOpened = new TaskCompletionSource<bool>();
			var fileServiceMock = new Mock<IFileService>();
			fileServiceMock.Setup(i => i.ShowOpenFilePickerAsync(It.IsAny<bool>(), null)).Returns(() =>
			{
				fileDialogOpened.SetResult(true);
				return Task.FromResult(fileNames.Select(fileName => new BrowserFile() { Name = fileName }).ToArray());
			});

			fileServiceMock.Setup(i => i.UploadFilesToServerAsync(It.IsAny<BrowserFile[]>(), It.IsAny<long>(), It.IsAny<CancellationToken>())).Returns(() =>
			{
				return Task.FromResult(fileNames);
			});

			form.CargoWiseClientServices.FileService = fileServiceMock.Object;

			var button = await page.WaitForSelectorAsync("button");
			await button.ClickAsync();
			Assert.That(await fileDialogOpened.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		}
	}

	[Test, WithPlaywrightPage]
	[Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1090:DoNotUseSystemWindowsFormsDialogs", Justification = "Testing")]
	public async Task SetBrowserFilesAndForceLocalFiles()
	{
		await using var ctx = new InMemoryTestServerContext();
		OpenFileDialog dialog = null;
		Form form = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			dialog = new OpenFileDialog();
			var button = new Button() { Text = "Open Dialog" };
			button.Click += (sender, args) => dialog.ShowDialog();
			form.Controls.Add(button);
			return form;
		});

		await using (var file1 = new BrowserFile { Name = "LocalFileName1" })
		await using (var file2 = new BrowserFile { Name = "LocalFileName2" })
		await using (var file3 = new BrowserFile { Name = "LocalFileName3" })
		{
			var localFiles = new[] { file1, file2, file3 };

			var fileServiceMock = new Mock<IFileService>();
			fileServiceMock.Setup(i => i.UploadFilesToServerAsync(localFiles, It.IsAny<long>(), It.IsAny<CancellationToken>())).Returns(() =>
			{
				return Task.FromResult(new[] { "UploadedFileName1", "UploadedFileName2", "UploadedFileName3" });
			});

			form.CargoWiseClientServices.FileService = fileServiceMock.Object;

			await form.InvokeWinzorDispatcherAsync(() => dialog.SetBrowserFilesAndForceLocalFiles(localFiles, 52428800));
			Assert.That(() => dialog.FileNames, Is.Not.Empty.After(1000, 100));
			Assert.That(dialog.FileNames, Does.Contain("UploadedFileName1"));
			Assert.That(dialog.FileNames, Does.Contain("UploadedFileName2"));
			Assert.That(dialog.FileNames, Does.Contain("UploadedFileName3"));
		}
	}

	[WithPlaywrightPage]
	[TestCase("HTML files (*.htm, *.html)|*.htm;*.html", "*.htm|*.html", "LocalFileName1.html")]
	[TestCase("Images | *.png;*.jpg;*.gif", "*.png;*.jpg;*.gif", "LocalFileName1.png")]
	public async Task OpenFileDialogShowsCorrectFilter(string filter, string defaultExt, string exampleFileName)
	{
		await using var ctx = new InMemoryTestServerContext();
		OpenFileDialog dialog = null;
		Form form = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			dialog = new OpenFileDialog();
			dialog.Filter = filter;
			dialog.DefaultExt = defaultExt;
			var button = new Button() { Text = "Open Dialog" };
			button.Click += (sender, args) => dialog.ShowDialog();
			form.Controls.Add(button);
			return form;
		});

		await page.WaitForSelectorAsync(".form");

		var fileDialogOpened = new TaskCompletionSource<bool>();
		var fileServiceMock = new Mock<IFileService>();
		var fileTypes = dialog.ParseFilter();

		fileServiceMock.Setup(i => i.ShowOpenFilePickerAsync(It.IsAny<bool>(),
			It.Is<IEnumerable<Dictionary<string, string[]>>>(a => a.First().Keys.FirstOrDefault().Equals(fileTypes.First().Keys.FirstOrDefault())))).Returns(() =>
		{
			fileDialogOpened.SetResult(true);
			return Task.FromResult(new BrowserFile[]
			{
				new BrowserFile() { Name = exampleFileName },
			});
		});
		fileServiceMock.Setup(i => i.UploadFilesToServerAsync(It.IsAny<BrowserFile[]>(), It.IsAny<long>(), It.IsAny<CancellationToken>())).Returns(() =>
		{
			return Task.FromResult(new string[] { exampleFileName });
		});
		form.CargoWiseClientServices.FileService = fileServiceMock.Object;

		var button = await page.WaitForSelectorAsync("button");
		await button.ClickAsync();

		Assert.That(await fileDialogOpened.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		await form.InvokeWinzorDispatcherAsync(() => dialog.ForceLocalFiles(52428800));

		Assert.That(() => dialog.FileNames, Is.Not.Empty.After(1000, 100));
		Assert.That(dialog.FileNames, Does.Contain(exampleFileName));
	}

	[WithPlaywrightPage]
	[TestCase(typeof(JSException), true, TestName = "{m}_JSException")]
	[TestCase(typeof(TimeoutException), true, TestName = "{m}_Timeout")]
	[TestCase(typeof(PathTooLongException), false, TestName = "{m}_InvalidFileName")]
	[TestCase(typeof(IOException), false, TestName = "{m}_IOException")]
	public async Task TestOpenFileDialogShowDialogThrows(Type exType, bool shouldReport)
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var fileDialogOpened = new TaskCompletionSource<DialogResult>();
		Exception developerException = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			Application.ThreadException += Application_ThreadException;
			form = new Form();
			var dialog = new OpenFileDialog();
			var button = new Button() { Text = "Open Dialog" };
			button.Click += (sender, args) => fileDialogOpened.TrySetResult(dialog.ShowDialog());
			form.Controls.Add(button);
			return form;
		});

		var fileServiceMock = new Mock<IFileService>();
		_ = fileServiceMock.Setup(i => i.ShowOpenFilePickerAsync(It.IsAny<bool>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>())).Returns(() =>
		{
			var ex = (Exception)Activator.CreateInstance(exType, new string[] { "Error happened." });
			throw ex;
		});
		form.CargoWiseClientServices.FileService = fileServiceMock.Object;

		try
		{
			var button = await page.WaitForSelectorAsync("button");
			await button.ClickAsync();

			Assert.That(async () => await fileDialogOpened.Task, Is.EqualTo(DialogResult.Cancel).After(3000, 200));
			if (shouldReport)
			{
				Assert.That(developerException, Is.TypeOf(exType));
			}
		}
		finally
		{
			Application.ThreadException -= Application_ThreadException;
		}

		void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
		{
			developerException = e.Exception;
		}
	}

	[Test]
	public async Task FormDisposeShouldCloseOpenFileDialog()
	{
		using var ctx = new WinzorTestContext();
		Form form = null;
		var dialogClosed = new TaskCompletionSource();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			var button = new Button { Text = "Test" };
			form.Controls.Add(button);
			button.Click += (sender, e) =>
			{
				var dialog = new OpenFileDialog();
				dialog.ShowDialog();
				dialogClosed.SetResult();
			};
			return form;
		});
		var fileDialogOpened = new TaskCompletionSource<bool>();
		var fileServiceMock = new Mock<IFileService>();
		fileServiceMock.Setup(i => i.ShowOpenFilePickerAsync(It.IsAny<bool>(), null)).Returns(async () =>
		{
			fileDialogOpened.SetResult(true);
			await dialogClosed.Task;
			return Array.Empty<BrowserFile>();
		});
		form.CargoWiseClientServices.FileService = fileServiceMock.Object;
		rendered.Find("button").Click();
		Assert.That(await fileDialogOpened.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		await ctx.WinzorDispatcher.InvokeAsync(() => form.Dispose());
		Assert.That(await dialogClosed.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);
	}
}
