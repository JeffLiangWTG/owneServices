using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Moq;
using NUnit.Framework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;

namespace System.Windows.Forms;

internal sealed class SaveFileDialogTest
{
	[Test]
	[Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1090:DoNotUseSystemWindowsFormsDialogs", Justification = "Testing")]
	public async Task SaveFileDialogCallsFileServiceToShowDialog()
	{
		using var ctx = new WinzorTestContext();

		var fileServiceMock = new Mock<IFileService>();
		fileServiceMock.Setup(f => f.ShowSaveFileDialogAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>())).Returns(() => Task.FromResult(Mock.Of<IWritableBrowserFile>()));
		var cargowiseClientServices = MockCargoWiseClientServices.MakeMock(jsRuntime: ctx.JSInterop.JSRuntime, fileService: fileServiceMock.Object);

		Form form = null;
		await ctx.RenderFormAsync(() => form = new Form(), cargowiseClientServices);

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var saveFileDialog = new SaveFileDialog();
			var dialogResult = saveFileDialog.ShowDialog();
			Assert.That(dialogResult, Is.EqualTo(DialogResult.OK));
		});

		fileServiceMock.Verify(f => f.ShowSaveFileDialogAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>()), Times.Once());
	}

	[Test]
	[Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1090:DoNotUseSystemWindowsFormsDialogs", Justification = "Testing")]
	public async Task SaveFileDialogCancelDialog()
	{
		using var ctx = new WinzorTestContext();

		var fileServiceMock = new Mock<IFileService>();
		fileServiceMock.Setup(f => f.ShowSaveFileDialogAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>())).Returns(() => Task.FromResult<IWritableBrowserFile>(null));
		var cargowiseClientServices = MockCargoWiseClientServices.MakeMock(jsRuntime: ctx.JSInterop.JSRuntime, fileService: fileServiceMock.Object);

		Form form = null;
		await ctx.RenderFormAsync(() => form = new Form(), cargowiseClientServices);

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var saveFileDialog = new SaveFileDialog();
			var dialogResult = saveFileDialog.ShowDialog();
			Assert.That(dialogResult, Is.EqualTo(DialogResult.Cancel));
		});

		fileServiceMock.Verify(f => f.ShowSaveFileDialogAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>()), Times.Once());
	}

	[Test]
	[Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1090:DoNotUseSystemWindowsFormsDialogs", Justification = "Testing")]
	public async Task SaveFileDialogCallsFileServiceWithPreferredFileName()
	{
		using var ctx = new WinzorTestContext();

		var fileServiceMock = new Mock<IFileService>();
		fileServiceMock.Setup(f => f.ShowSaveFileDialogAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>())).Returns(() => Task.FromResult(Mock.Of<IWritableBrowserFile>()));
		var cargowiseClientServices = MockCargoWiseClientServices.MakeMock(jsRuntime: ctx.JSInterop.JSRuntime, fileService: fileServiceMock.Object);

		Form form = null;
		await ctx.RenderFormAsync(() => form = new Form(), cargowiseClientServices);

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var saveFileDialog = new SaveFileDialog();
			saveFileDialog.FileName = "TestFileName.txt";
			var dialogResult = saveFileDialog.ShowDialog();
			Assert.That(dialogResult, Is.EqualTo(DialogResult.OK));
		});

		fileServiceMock.Verify(f => f.ShowSaveFileDialogAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>()), Times.Once());
		Assert.That(fileServiceMock.Invocations[0].Arguments[0], Is.EqualTo("TestFileName.txt"));
	}

	[Test]
	[Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1090:DoNotUseSystemWindowsFormsDialogs", Justification = "Testing")]
	public async Task SaveFileDialogOpenFileWritesToBrowserFileOnDispose()
	{
		using var ctx = new WinzorTestContext();

		var fileServiceMock = new Mock<IFileService>();
		var writableBrowserFileMock = new Mock<IWritableBrowserFile>();
		fileServiceMock.Setup(f => f.ShowSaveFileDialogAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>())).Returns(() => Task.FromResult(writableBrowserFileMock.Object));
		var cargowiseClientServices = MockCargoWiseClientServices.MakeMock(jsRuntime: ctx.JSInterop.JSRuntime, fileService: fileServiceMock.Object);

		Form form = null;
		await ctx.RenderFormAsync(() => form = new Form(), cargowiseClientServices);
		Stream fileStream = null;

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var saveFileDialog = new SaveFileDialog();
			var dialogResult = saveFileDialog.ShowDialog();
			Assert.That(dialogResult, Is.EqualTo(DialogResult.OK));
			fileStream = saveFileDialog.OpenFile();
		});

		fileServiceMock.Verify(f => f.ShowSaveFileDialogAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>()), Times.Once());
		writableBrowserFileMock.Verify(f => f.WriteAsync(It.IsAny<Stream>()), Times.Never());
		Assert.That(fileStream, Is.Not.Null);

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			using (var writer = new StreamWriter(fileStream))
			{
				writer.WriteLine("Text Text");
			}
		});

		fileServiceMock.Verify(f => f.ShowSaveFileDialogAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>()), Times.Once());
		writableBrowserFileMock.Verify(f => f.WriteAsync(It.IsAny<Stream>()), Times.Once());
	}

	[Test]
	[Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1090:DoNotUseSystemWindowsFormsDialogs", Justification = "Testing")]
	public async Task SaveFileDialogOpenFileShowsBrowserPopupOnDisposeIfShowDialogWasNotCalled()
	{
		using var ctx = new WinzorTestContext();

		var fileServiceMock = new Mock<IFileService>();
		var writableBrowserFileMock = new Mock<IWritableBrowserFile>();
		fileServiceMock.Setup(f => f.SaveFileToDirectoryAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>())).Returns(() => Task.FromResult(writableBrowserFileMock.Object));
		var cargowiseClientServices = MockCargoWiseClientServices.MakeMock(jsRuntime: ctx.JSInterop.JSRuntime, fileService: fileServiceMock.Object);

		Form form = null;
		await ctx.RenderFormAsync(() => form = new Form(), cargowiseClientServices);
		Stream fileStream = null;

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var saveFileDialog = new SaveFileDialog();
			saveFileDialog.FileName = "TestFileName.txt";
			fileStream = saveFileDialog.OpenFile();
		});

		fileServiceMock.Verify(f => f.SaveFileToDirectoryAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>()), Times.Never());
		writableBrowserFileMock.Verify(f => f.WriteAsync(It.IsAny<Stream>()), Times.Never());
		Assert.That(fileStream, Is.Not.Null);

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			using (var writer = new StreamWriter(fileStream))
			{
				writer.WriteLine("Text Text");
			}
		});

		fileServiceMock.Verify(f => f.SaveFileToDirectoryAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>()), Times.Once());
		Assert.That(fileServiceMock.Invocations[0].Arguments[0], Is.EqualTo("TestFileName.txt"));
		writableBrowserFileMock.Verify(f => f.WriteAsync(It.IsAny<Stream>()), Times.Once());
	}

	[Test]
	[Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1090:DoNotUseSystemWindowsFormsDialogs", Justification = "Testing")]
	public async Task SaveFileDialogOpenFileThrowsIfShowDialogWasNotCalledAndNoFileNameProvided()
	{
		using var ctx = new WinzorTestContext();

		var fileServiceMock = new Mock<IFileService>();
		var writableBrowserFileMock = new Mock<IWritableBrowserFile>();
		fileServiceMock.Setup(f => f.ShowSaveFileDialogAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>())).Returns(() => Task.FromResult(writableBrowserFileMock.Object));
		var cargowiseClientServices = MockCargoWiseClientServices.MakeMock(jsRuntime: ctx.JSInterop.JSRuntime, fileService: fileServiceMock.Object);

		Form form = null;
		await ctx.RenderFormAsync(() => form = new Form(), cargowiseClientServices);

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var saveFileDialog = new SaveFileDialog();
			Assert.Throws<ArgumentNullException>(() =>
			{
				saveFileDialog.OpenFile();
			});
		});

		fileServiceMock.Verify(f => f.ShowSaveFileDialogAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>()), Times.Never());
		writableBrowserFileMock.Verify(f => f.WriteAsync(It.IsAny<Stream>()), Times.Never());
	}

	public static IEnumerable<TestCaseData> SaveFileDialogExtensionTestCases
	{
		get
		{
			yield return new TestCaseData(
				"Scalable Vector Graphic Files|*.svg",
				"SVG",
				new[] { new Dictionary<string, string[]> { ["Scalable Vector Graphic Files"] = new[] { ".svg" } } })
				.SetName("{m}(simple-description)");

			yield return new TestCaseData(
				"|*.svg",
				null,
				new[] { new Dictionary<string, string[]> { [""] = new[] { ".svg" } } })
				.SetName("{m}(no-description)");

			yield return new TestCaseData(
				"Excel Files (*.xlsx)|*.xlsx",
				null,
				new[] { new Dictionary<string, string[]> { ["Excel Files"] = new[] { ".xlsx" } } })
				.SetName("{m}(redundant-description)");

			yield return new TestCaseData(
				"Excel Files (Office 97)|*.xls",
				"xls",
				new[] { new Dictionary<string, string[]> { ["Excel Files (Office 97)"] = new[] { ".xls" } } })
				.SetName("{m}(non-redundant-description)");

			yield return new TestCaseData(
				"HTML files|*.html|HTM Files|*.htm",
				"htm",
				new[] { new Dictionary<string, string[]> { ["HTML files"] = new[] { ".html" } }, new Dictionary<string, string[]> { ["HTM Files"] = new[] { ".htm" } } })
				.SetName("{m}(multiple-types)");

			yield return new TestCaseData(
				"HTML files (*.html, *.htm)|*.html;*.htm",
				"htm",
				new[] { new Dictionary<string, string[]> { ["HTML files"] = new[] { ".html", ".htm" } } })
				.SetName("{m}(grouped-types)");

			yield return new TestCaseData(
				"HTML files|*.html;*.htm",
				"txt",
				new[] { new Dictionary<string, string[]> { [""] = new[] { ".txt" } }, new Dictionary<string, string[]> { ["HTML files"] = new[] { ".html", ".htm" } } })
				.SetName("{m}(out-of-range-default)");

			yield return new TestCaseData(null, null, null)
				.SetName("{m}(null-filter)");

			yield return new TestCaseData("", null, null)
				.SetName("{m}(empty-filter)");
		}
	}

	[TestCaseSource(nameof(SaveFileDialogExtensionTestCases))]
	public async Task SaveFileDialogShowsCorrectExtensions_UsingFilter(string filter, string defaultExt, Dictionary<string, string[]>[] expected)
	{
		using var ctx = new WinzorTestContext();

		var fileServiceMock = new Mock<IFileService>();
		var writableBrowserFileMock = new Mock<IWritableBrowserFile>();
		writableBrowserFileMock.Setup(x => x.FileName).Returns("TestFileName.txt");
		fileServiceMock.Setup(f => f.ShowSaveFileDialogAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>())).Returns(() => Task.FromResult(writableBrowserFileMock.Object));
		var cargowiseClientServices = MockCargoWiseClientServices.MakeMock(jsRuntime: ctx.JSInterop.JSRuntime, fileService: fileServiceMock.Object);

		Form form = null;
		await ctx.RenderFormAsync(() => form = new Form(), cargowiseClientServices);
		Stream fileStream = null;

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = filter;
			saveFileDialog.DefaultExt = defaultExt;
			var dialogResult = saveFileDialog.ShowDialog();
			Assert.That(dialogResult, Is.EqualTo(DialogResult.OK));
			fileStream = saveFileDialog.OpenFile();
		});

		fileServiceMock.Verify(f => f.ShowSaveFileDialogAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>()), Times.Once());
		writableBrowserFileMock.Verify(f => f.WriteAsync(It.IsAny<Stream>()), Times.Never());

		var actual = (IEnumerable<Dictionary<string, string[]>>)fileServiceMock.Invocations[0].Arguments[1];
		Assert.That(actual, expected == null ? Is.Null : Is.EquivalentTo(expected));
	}

	[TestCase("file.pdf", 1)]
	[TestCase("file.xls", 2)]
	[TestCase("file.xlsx", 3)]
	[TestCase("file.tif", 4)]
	public async Task SaveFileDialogUpdateFilterIndexCorrectly(string fileName, int expectedFilterIndex)
	{
		using var ctx = new WinzorTestContext();

		var fileServiceMock = new Mock<IFileService>();
		var writableBrowserFileMock = new Mock<IWritableBrowserFile>();
		writableBrowserFileMock.Setup(x => x.FileName).Returns(fileName);
		fileServiceMock.Setup(f => f.ShowSaveFileDialogAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>())).Returns(() => Task.FromResult(writableBrowserFileMock.Object));
		var cargowiseClientServices = MockCargoWiseClientServices.MakeMock(jsRuntime: ctx.JSInterop.JSRuntime, fileService: fileServiceMock.Object);

		Form form = null;
		await ctx.RenderFormAsync(() => form = new Form(), cargowiseClientServices);
		Stream fileStream = null;

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "PDF Files|*.pdf|Excel Files (*.xls)|*.xls|Excel Files (*.xlsx)|*.xlsx|Images|*.tif";
			var dialogResult = saveFileDialog.ShowDialog();
			Assert.That(dialogResult, Is.EqualTo(DialogResult.OK));
			fileStream = saveFileDialog.OpenFile();
			Assert.That(saveFileDialog.FilterIndex, Is.EqualTo(expectedFilterIndex));
		});

		fileServiceMock.Verify(f => f.ShowSaveFileDialogAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>()), Times.Once());
		writableBrowserFileMock.Verify(f => f.WriteAsync(It.IsAny<Stream>()), Times.Never());
	}

	[Test]
	public async Task SaveFileStreamDoesNotThrowExceptionWhenDisposedMultipleTimes()
	{
		using var ctx = new WinzorTestContext();
		var threadExceptionThrown = new TaskCompletionSource<Exception>();
		ctx.ThreadExceptionExceptionRaised += ex =>
		{
			threadExceptionThrown.SetResult(ex);
			return true;
		};

		var fileServiceMock = new Mock<IFileService>();
		var writableBrowserFileMock = new Mock<IWritableBrowserFile>();
		fileServiceMock.Setup(f => f.ShowSaveFileDialogAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>())).Returns(() => Task.FromResult(writableBrowserFileMock.Object));
		Stream fileStream = null;
		writableBrowserFileMock.Setup(x => x.WriteAsync(It.IsAny<Stream>())).Callback(() => fileStream.Seek(0, SeekOrigin.Begin));
		var cargowiseClientServices = MockCargoWiseClientServices.MakeMock(jsRuntime: ctx.JSInterop.JSRuntime, fileService: fileServiceMock.Object);

		Form form = null;
		await ctx.RenderFormAsync(() => form = new Form(), cargowiseClientServices);
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var saveFileDialog = new SaveFileDialog();
			var dialogResult = saveFileDialog.ShowDialog();
			Assert.That(dialogResult, Is.EqualTo(DialogResult.OK));
			fileStream = saveFileDialog.OpenFile();
		});

		Assert.That(fileStream, Is.Not.Null);

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			using (var writer = new StreamWriter(fileStream))
			{
				writer.WriteLine("Text Text");
			}

			fileStream.Dispose();
		});

		Assert.That(await threadExceptionThrown.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.False);
	}

	[TestCase(typeof(JSException), TestName = "{m}_JSException")]
	[TestCase(typeof(TimeoutException), TestName = "{m}_Timeout")]
	public async Task TestSaveFileDialogShowDialogThrows(Type exType)
	{
		using var ctx = new WinzorTestContext();
		Form form = null;
		var fileDialogOpened = new TaskCompletionSource<DialogResult>();

		_ = await ctx.RenderFormAsync(() => form = new Form());
		var fileServiceMock = new Mock<IFileService>();
		_ = fileServiceMock.Setup(i => i.ShowSaveFileDialogAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>())).Returns(() =>
		{
			var ex = (Exception)Activator.CreateInstance(exType, new string[] { "Error happened." });
			throw ex;
		});
		form.CargoWiseClientServices.FileService = fileServiceMock.Object;

		ctx.DeveloperExceptionRaised += DeveloperExceptionRaised;
		try
		{
			await form.InvokeWinzorDispatcherAsync(() =>
			{
				var dialog = new SaveFileDialog();
				fileDialogOpened.TrySetResult(dialog.ShowDialog());
			});

			Assert.That(async () => await fileDialogOpened.Task, Is.EqualTo(DialogResult.Cancel).After(3000, 200));
		}
		finally
		{
			ctx.DeveloperExceptionRaised -= DeveloperExceptionRaised;
		}

		bool DeveloperExceptionRaised(Exception ex)
		{
			return ex.GetType() == exType;
		}
	}
}
