using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.JSInterop;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace WinzorFramework;

using static PlaywrightTestContext;

public class DownloadFile
{
	public byte[] data;
	public string name;
}

internal sealed class FileServiceTest
{
	/*
	 * As playwright does not support handling window.showOpenFilePicker(), window.showSaveFilePicker(), and window.showDirectoryPicker();
	 * we will replace the JS function with a mock function to test the desired behaviour under different circumstances.
	 * When the playwright FileChooser api supports them, these can be replaced.
	 */

	#region Open File Tests

	[Test]
	public async Task FileServiceOpenFileDialogOpensBrowserFilePicker()
	{
		using var fileService = SetupFileServiceWithMockJS(out var js);
		await fileService.ShowOpenFilePickerAsync();
		AssertJsInvocationCount(js, "openFileDialog", 1);
	}

	public static IEnumerable<TestCaseData> MultipleUploadsTestCaseData
	{
		get
		{
			yield return new TestCaseData(true) { TestName = "{m}_True" };
			yield return new TestCaseData(false) { TestName = "{m}_False" };
		}
	}

	[TestCaseSource(nameof(MultipleUploadsTestCaseData))]
	public async Task FileServiceOpenFileDialogAllowsMultipleUploads(bool allowMultipleUploads)
	{
		using var fileService = SetupFileServiceWithMockJS(out var js);
		await fileService.ShowOpenFilePickerAsync(allowMultipleUploads);
		Assert.That((bool)GetJsInvocationArgument(js, 0, 0), Is.EqualTo(allowMultipleUploads));
	}

	[Test, WithPlaywrightPage]
	public async Task FileServiceOpenFileDialogAllowsFilter()
	{
		await using var ctx = new InMemoryTestServerContext();
		OpenFileDialog dialog = null;
		Form form = null;
		IEnumerable<Dictionary<string, string[]>> fileTypes = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			dialog = new OpenFileDialog();
			dialog.Filter = "HTML files (*.htm, *.html)|*.htm;*.html";
			dialog.DefaultExt = "*.htm|*.html";
			fileTypes = dialog.ParseFilter();
			return form;
		});
		using var fileService = SetupFileServiceWithMockJS(out var js);
		await fileService.ShowOpenFilePickerAsync(false, fileTypes);

		Assert.That((IEnumerable<Dictionary<string, string[]>>)GetJsInvocationArgument(js, 0, 1), Is.EqualTo(fileTypes));
	}

	[Test]
	public async Task FileServiceOpenFileDialogUploadsFilesToTempDir()
	{
		using var fileService = SetupFileServiceWithMockJS(out var js);
		var files = Array.Empty<string>();
		using (var jsFileUploads = new JsFilesForTest(js))
		{
			jsFileUploads.AddFile();
			jsFileUploads.AddFile();
			var browseFiles = await fileService.ShowOpenFilePickerAsync();
			files = await fileService.UploadFilesToServerAsync(browseFiles, 52428800);
		}
		Assert.That(files, Is.Not.Empty);
		Assert.That(files.Count, Is.EqualTo(2));

		var expectedTempPath = Path.Combine(Path.GetTempPath(), "WiseTechGlobal", Process.GetCurrentProcess().Id.ToString(), "WinzorUploads");
		Assert.That(files.Where(f => f.Contains(expectedTempPath)).Count, Is.EqualTo(2));

		Assert.That(Path.GetFileName(files[0]), Is.EqualTo("File0.txt"));
		Assert.That(File.ReadAllText(files[0]), Is.EqualTo("This is file 0"));
		Assert.That(Path.GetFileName(files[1]), Is.EqualTo("File1.txt"));
		Assert.That(File.ReadAllText(files[1]), Is.EqualTo("This is file 1"));
	}

	[Test]
	public async Task FileServiceOpenFileDialogIteratesDuplicateFileNames()
	{
		using var fileService = SetupFileServiceWithMockJS(out var js);
		var files = Array.Empty<string>();
		using (var jsFileUploads = new JsFilesForTest(js))
		{
			jsFileUploads.AddDuplicateFiles();
			var browseFiles = await fileService.ShowOpenFilePickerAsync();
			files = await fileService.UploadFilesToServerAsync(browseFiles, 52428800);
		}
		Assert.That(files, Is.Not.Empty);
		Assert.That(files.Count, Is.EqualTo(3));

		Assert.That(Path.GetFileName(files[0]), Is.EqualTo("File0.txt"));
		Assert.That(File.ReadAllText(files[0]), Is.EqualTo("This is file 0"));
		Assert.That(Path.GetFileName(files[1]), Is.EqualTo("File0[0].txt"));
		Assert.That(File.ReadAllText(files[1]), Is.EqualTo("This is a duplicate of file 0"));
		Assert.That(Path.GetFileName(files[2]), Is.EqualTo("File0[1].txt"));
		Assert.That(File.ReadAllText(files[2]), Is.EqualTo("This is another duplicate of file 0"));
	}

	[Test]
	public async Task FileServiceRemovesTempFilesOnDispose()
	{
		var files = Array.Empty<string>();
		using (var fileService = SetupFileServiceWithMockJS(out var js))
		{
			using (var jsFileUploads = new JsFilesForTest(js))
			{
				jsFileUploads.AddFile();
				jsFileUploads.AddFile();
				var browseFiles = await fileService.ShowOpenFilePickerAsync();
				files = await fileService.UploadFilesToServerAsync(browseFiles, 52428800);
			}
			Assert.That(files, Is.Not.Empty);
			Assert.That(files.Count, Is.EqualTo(2));

			Assert.That(Directory.Exists(Path.GetDirectoryName(files[0])), Is.True);
			Assert.That(File.Exists(files[0]), Is.True);
			Assert.That(File.Exists(files[1]), Is.True);
		}

		Assert.That(Directory.Exists(Path.GetDirectoryName(files[0])), Is.False);
		Assert.That(File.Exists(files[0]), Is.False);
		Assert.That(File.Exists(files[1]), Is.False);
	}

	[Test, WithPlaywrightPage]
	public async Task FileServiceOpenFileDialogReturnsEmptyArrayIfCancelled()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			return form;
		});

		await page.WaitForSelectorAsync(".form");

		// This mock will throw the exception that the window.showOpenFilePicker() api throws then the dialog is cancelled.
		await page.EvaluateAsync(
@"() => window.showOpenFilePicker = () => {
var error = new Error();
error.name = 'AbortError';
throw error;
}");

		var files = await form.CargoWiseClientServices.FileService.ShowOpenFilePickerAsync();
		Assert.That(files, Is.Empty);
	}

	[Test, WithPlaywrightPage]
	public async Task FileServiceOpenFileDialogRethrowsUnexpectedErrors()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			return form;
		});

		await page.WaitForSelectorAsync(".form");

		// This mock will throw an unexpected error and ensure the JS rethrows it.
		await page.EvaluateAsync("() => window.showOpenFilePicker = () => {throw Error()}");

		Assert.ThrowsAsync<JSException>(async () =>
		{
			await form.CargoWiseClientServices.FileService.ShowOpenFilePickerAsync();
		});
	}

	[Test, WithPlaywrightPage]
	public async Task FileServiceOpenFileDialogThrowErrorForUnsupportedBrowser()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			return form;
		});

		await page.WaitForSelectorAsync(".form");

		// This mock will set showOpenFilePicker to null to mock an unsupported browser.
		await page.EvaluateAsync("() => delete window.showOpenFilePicker");

		var exception = Assert.ThrowsAsync<JSException>(async () => await form.CargoWiseClientServices.FileService.ShowOpenFilePickerAsync());
		Assert.That(exception.Message, Does.Contain("Uploading files is not supported in this browser."));
	}

	FileService SetupFileServiceWithMockJS(out Mock<IJSObjectReference> js)
	{
		var jsRuntime = new Mock<IJSRuntimeWithMonitor>();
		var downloadObjectManager = new Mock<IDownloadObjectManager>();
		js = new Mock<IJSObjectReference>();
		js.Setup(i => i.InvokeAsync<object>(It.IsAny<string>(), It.IsAny<object[]>()));
		var path = "/_content/WinzorFramework/js/module/fileService.js";
		jsRuntime.Setup(i => i.InvokeAsync<IJSObjectReference>("import", new object[] { path })).ReturnsAsync(js.Object);
#pragma warning disable CA2000 // Dispose objects before losing scope
		var fileServiceInterop = new FileServiceJSInterop(jsRuntime.Object, new DummyFileVersionHash());
#pragma warning restore CA2000 // Dispose objects before losing scope
		return new FileService(fileServiceInterop, downloadObjectManager.Object);
	}

	[Test, WithPlaywrightPage]
	public async Task FileServiceOpenFileDialogProcessesUploadedFiles()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			return form;
		});

		await page.WaitForSelectorAsync(".form");

		// This mock will return valid file mocks from window.showOpenFilePicker to be processed by the js.
		await page.EvaluateAsync(@"() => window.showOpenFilePicker = () => [
{
getFile: () => new Promise((resolve, reject) => {
var blob1 = new Blob(['This is file one.'], {type: 'text/plain'});
var file1 = new File([blob1], 'File1.txt', {type: 'text/plain', lastModified: 1654862400000});
file1.blob = blob1;
resolve(file1);
})
},
{
getFile: () => new Promise((resolve, reject) => {
var blob2 = new Blob(['This is file two.'], {type: 'text/plain'});
var file2 = new File([blob2], 'File1.txt', {type: 'text/plain', lastModified: 1654866000000});
file2.blob = blob2;
resolve(file2);
})
}]");
		var browseFiles = await form.CargoWiseClientServices.FileService.ShowOpenFilePickerAsync();
		var files = await form.CargoWiseClientServices.FileService.UploadFilesToServerAsync(browseFiles, 52428800);
		Assert.That(files.Count, Is.EqualTo(2));

		var file1 = files[0];
		Assert.That(File.ReadAllText(file1), Is.EqualTo("This is file one."));

		var file2 = files[1];
		Assert.That(File.ReadAllText(file2), Is.EqualTo("This is file two."));
	}

	[TestCaseSource(nameof(MultipleUploadsTestCaseData)), WithPlaywrightPage]
	public async Task FileServiceOpenFileDialogHasCorrectMultipleUploadsArgument(bool allowMultipleUploads)
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			return form;
		});

		await page.WaitForSelectorAsync(".form");

		// This mock will log the value of args.multiple to the console.
		await page.EvaluateAsync(@"() => window.showOpenFilePicker = (args) => {
console.log(args.multiple);
return [];
}");

		var consoleMessage = page.WaitForConsoleMessageAsync();
		var files = await form.CargoWiseClientServices.FileService.ShowOpenFilePickerAsync(allowMultipleUploads);
		Assert.That((await consoleMessage).Text, Is.EqualTo(allowMultipleUploads.ToString().ToLower()));
	}

	class JsFilesForTest : IDisposable
	{
		public JsFilesForTest(Mock<IJSObjectReference> js)
		{
			js.Setup(i => i.InvokeAsync<BrowserFile[]>("openFileDialog", It.IsAny<CancellationToken>(), It.IsAny<object[]>())).Returns(() => ValueTask.FromResult(files.ToArray()));
		}

		public void AddFile()
		{
			var fileNumber = files.Count;
			AddFile($"File{fileNumber}.txt", $"This is file {fileNumber}");
		}

		public void AddDuplicateFiles()
		{
			var fileNumber = files.Count;
			AddFile($"File{fileNumber}.txt", $"This is file {fileNumber}");
			AddFile($"File{fileNumber}.txt", $"This is a duplicate of file {fileNumber}");
			AddFile($"File{fileNumber}.txt", $"This is another duplicate of file {fileNumber}");
		}

		void AddFile(string name, string contents)
		{
			var stream = new MemoryStream(Encoding.ASCII.GetBytes(contents));
			var jsStreamReference = new Mock<IJSStreamReference>();
			jsStreamReference.Setup(i => i.OpenReadStreamAsync(It.IsAny<long>(), It.IsAny<CancellationToken>())).Returns(() => ValueTask.FromResult((Stream)stream));
			streams.Add(stream);
			files.Add(new BrowserFile() { Name = name, FileStream = jsStreamReference.Object });
		}

		public void Dispose()
		{
			foreach (var stream in streams)
			{
				stream.Dispose();
			}
		}

		readonly List<BrowserFile> files = new List<BrowserFile>();
		readonly List<Stream> streams = new List<Stream>();
	}

	void AssertJsInvocationCount(Mock<IJSObjectReference> js, string functionName, int count) => Assert.That(js.Invocations.Count(i => i.Arguments[0].ToString() == functionName), Is.EqualTo(count));

	object GetJsInvocationArgument(Mock<IJSObjectReference> js, int invocation, int argument) => ((object[])js.Invocations[invocation].Arguments[2])[argument];

	#endregion

	#region Save File Tests

	const string returnJSFileHandle = @"return {
createWritable: () => new WritableStream({
	write(chunk) {
		return new Promise((resolve, reject) => {
			window.saveFileStreamData = new TextDecoder('utf-8').decode(new Uint8Array(chunk));
			resolve();
		});
	},
	close() {
		window.saveFileStreamClosed = true;
	}
}).getWriter()}";

	[Test, WithPlaywrightPage]
	public async Task FileServiceShowSaveFileDialogOpensBrowserFileSaveDialog()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			return form;
		});

		await page.WaitForSelectorAsync(".form");

		// This mock will ensure window.showSaveFilePicker is called.
		await page.EvaluateAsync($"() => window.showSaveFilePicker = (options) => {{ window.showSaveFilePickerCalled = true; {returnJSFileHandle}}}");

		await using (var writableBrowserFile = await form.CargoWiseClientServices.FileService.ShowSaveFileDialogAsync("TestFileName.txt"))
		{
			Assert.That(async () => await page.EvaluateAsync<bool>("() => Boolean(window?.showSaveFilePickerCalled)"), Is.True.After(1000, 100));
		}
	}

	[Test, WithPlaywrightPage]
	public async Task FileServiceShowSaveFileDialogWritesDataToFileAndClosesStream()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			return form;
		});

		await page.WaitForSelectorAsync(".form");

		// This mock will return a writable stream, and set window variables for the data written to to stream.
		await page.EvaluateAsync($"() => window.showSaveFilePicker = (options) => {{{returnJSFileHandle}}}");

		await using (var writableBrowserFile = await form.CargoWiseClientServices.FileService.ShowSaveFileDialogAsync("TestFileName.txt"))
		using (var testStream = new MemoryStream())
		{
			using var writer = new StreamWriter(testStream);
			await writer.WriteAsync("Test File Data!");
			await writer.FlushAsync();
			await writableBrowserFile.WriteAsync(testStream);
		}

		Assert.That(async () => await page.EvaluateAsync<bool>("() => Boolean(window?.saveFileStreamClosed)"), Is.True.After(1000, 100));
		Assert.That(async () => await page.EvaluateAsync<string>("() => window?.saveFileStreamData || ''"), Is.EqualTo("Test File Data!").After(1000, 100));
	}

	public static IEnumerable<TestCaseData> SaveFileDialogTestCases
	{
		get
		{
			yield return new TestCaseData(
				"no-description.svg",
				new[] { new Dictionary<string, string[]> { [""] = new[] { ".svg" } } },
				"{'suggestedName':'no-description.svg','types':[{'accept':{'application/x-wtg-svg':['.svg']}}],'excludeAcceptAllOption':true}")
				.SetName("{m}(no-description)");

			yield return new TestCaseData(
				"with-description.svg",
				new[] { new Dictionary<string, string[]> { ["Scalable Vector Graphic Files"] = new[] { ".svg" } } },
				"{'suggestedName':'with-description.svg','types':[{'accept':{'application/x-wtg-svg':['.svg']},'description':'Scalable Vector Graphic Files'}],'excludeAcceptAllOption':true}")
				.SetName("{m}(with-description)");

			yield return new TestCaseData(
				"multiple-types.html",
				new[] { new Dictionary<string, string[]> { [""] = new[] { ".html" } }, new Dictionary<string, string[]> { [""] = new[] { ".htm" } } },
				"{'suggestedName':'multiple-types.html','types':[{'accept':{'application/x-wtg-html':['.html']}},{'accept':{'application/x-wtg-htm':['.htm']}}],'excludeAcceptAllOption':true}")
				.SetName("{m}(multiple-types)");

			yield return new TestCaseData(
				"no-suggested-extension",
				new[] { new Dictionary<string, string[]> { [""] = new[] { ".html", ".htm" } } },
				"{'suggestedName':'no-suggested-extension.html','types':[{'accept':{'application/x-wtg-html':['.html'],'application/x-wtg-htm':['.htm']}}],'excludeAcceptAllOption':true}")
				.SetName("{m}(no-suggested-extension)");

			yield return new TestCaseData(
				"null-file-types.txt",
				null,
				"{'suggestedName':'null-file-types.txt','excludeAcceptAllOption':false}")
				.SetName("{m}(null-file-types)");

			yield return new TestCaseData(
				"empty-file-types.txt",
				Array.Empty<Dictionary<string, string[]>>(),
				"{'suggestedName':'empty-file-types.txt','types':[],'excludeAcceptAllOption':false}")
				.SetName("{m}(empty-file-types)");
		}
	}

	[TestCaseSource(nameof(SaveFileDialogTestCases)), WithPlaywrightPage]
	public async Task FileServiceShowSaveFileDialogContainCorrectOptions(string suggectedName, Dictionary<string, string[]>[] fileTypes, string expected)
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			return form;
		});

		await page.WaitForSelectorAsync(".form");

		// This mock will log the suggested file name passed into window.showSaveFilePicker.
		await page.EvaluateAsync($"() => window.showSaveFilePicker = (options) => {{ console.log(JSON.stringify(options)); {returnJSFileHandle}}}");

		var consoleMessage = page.WaitForConsoleMessageAsync();

		await using (await form.CargoWiseClientServices.FileService.ShowSaveFileDialogAsync(suggectedName, fileTypes))
		{
			Assert.That((await consoleMessage).Text, Is.EqualTo(expected.Replace("'", "\"")));
		}
	}

	[Test, WithPlaywrightPage]
	public async Task FileServiceShowSaveFileDialogReturnsNullIfFilePickerClosed()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			return form;
		});

		await page.WaitForSelectorAsync(".form");

		// This mock will throw an exception and ensure it is caught.
		await page.EvaluateAsync($"() => window.showSaveFilePicker = () => {{const e = new Error(); e.name = 'AbortError'; throw e;}}");

		await using (var writableBrowserFile = await form.CargoWiseClientServices.FileService.ShowSaveFileDialogAsync("TestFileName.txt"))
		{
			Assert.That(writableBrowserFile, Is.Null);
		}
	}

	public static IEnumerable<TestCaseData> DataForDownload
	{
		get
		{
			yield return new TestCaseData(new List<DownloadFile>()
				{
					new DownloadFile() { data = Encoding.UTF8.GetBytes("Hello world!"), name = "file1.txt" }
				})
			{ TestName = "{m}_SingleTextFile" };
			yield return new TestCaseData(new List<DownloadFile>()
				{
					new DownloadFile() { data = Encoding.UTF8.GetBytes("Hello world!"), name = "file1.txt" },
					new DownloadFile() { data = Encoding.UTF8.GetBytes("Hello world! Second file"), name = "file2.txt" }
				})
			{ TestName = "{m}_TwoTextFile" };
			yield return new TestCaseData(new List<DownloadFile>()
				{
					new DownloadFile() { data = Encoding.UTF8.GetBytes("Hello world!link"), name = "file1.lnk" },
					new DownloadFile() { data = Encoding.UTF8.GetBytes("Hello world!url"), name = "file2.url" }
				})
			{ TestName = "{m}_SpecialSuffixFile" };
		}
	}

	class TestDownloadObject : AbsDownloadObject
	{
		readonly string _name;
		readonly byte[] _content;

		public TestDownloadObject(string name, byte[] content)
		{
			_name = name;
			_content = content;
		}

		public override string Name => _name;

		public override async Task DownloadAsync(Stream body, CancellationToken cts)
		{
			await using var writer = new BinaryWriter(body);
			writer.Write(_content);
			await Task.CompletedTask;
		}

		protected override Task<Stream> OpenInStreamAsync()
		{
			throw new NotImplementedException();
		}
	}

	[TestCaseSource(nameof(DataForDownload)), WithPlaywrightPage]
	public async Task FileServiceDownloadFile(List<DownloadFile> fileList)
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			return form;
		});

		await page.WaitForSelectorAsync(".form");

		foreach (var downloadFile in fileList)
		{
			var waitForDownloadTask = page.WaitForDownloadAsync(new PageWaitForDownloadOptions());
			using var downloadObject = new TestDownloadObject(downloadFile.name, downloadFile.data);
			var cwcService = form.CargoWiseClientServices;
			Assert.That(cwcService, Is.Not.EqualTo(null));
			var objectId = cwcService!.FileService.AddDownloadObject(downloadObject);
			var callOk = await cwcService!.FileService.DownloadFileAsync(objectId);

			Assert.That(callOk);

			var download = await waitForDownloadTask;
			var downloadedPath = await download.PathAsync();
			var actualDownloadedData = await File.ReadAllBytesAsync(downloadedPath!);

			Assert.That(actualDownloadedData, Is.EqualTo(downloadFile.data));
			var expectedFileName = downloadFile.name;
			if (downloadFile.name.EndsWith("lnk"))
			{
				// ReSharper disable once StringIndexOfIsCultureSpecific.1
				expectedFileName = expectedFileName.Remove(expectedFileName.IndexOf(".")) + ".download";
			}
			Assert.That(download.SuggestedFilename, Is.EqualTo(expectedFileName));
		}
	}

	[Test]
	public async Task DownloadObjectManagerResourceReleaseTest()
	{
		// add a download object, on time up, verify object has been removed and disposed
		using var downloadObjectManager = new DownloadObjectManager(50);
		var moqDownloadObject = new Mock<IDownloadObject>();
		var objectId = downloadObjectManager.AddDownloadObject(moqDownloadObject.Object);
		Assert.That(downloadObjectManager.IsWaitingObjectsEmpty, Is.EqualTo(false));
		Assert.That(downloadObjectManager.IsDeferTimerEnabled, Is.EqualTo(true));
		await Task.Delay(100);
		Assert.That(downloadObjectManager.IsWaitingObjectsEmpty, Is.EqualTo(true));
		Assert.That(downloadObjectManager.IsDeferTimerEnabled, Is.EqualTo(false));
		moqDownloadObject.Verify(d => d.Dispose(), Times.Once);
	}

	public static IEnumerable<TestCaseData> SaveFileDialogJSExceptions
	{
		get
		{
			yield return new TestCaseData("NotAllowedError") { TestName = "{m}_NotAllowedError" };
			yield return new TestCaseData("TypeError") { TestName = "{m}_TypeError" };
			yield return new TestCaseData("InvalidStateError") { TestName = "{m}_InvalidStateError" };
		}
	}

	[TestCaseSource(nameof(SaveFileDialogJSExceptions)), WithPlaywrightPage]
	public async Task FileServiceShowSaveFileDialogRethrowUnexpectedError(string error)
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			return form;
		});

		// This mock will throw an exception and ensure it is caught.
		await page.EvaluateAsync($"() => window.showSaveFilePicker = () => {{const e = new Error(); e.name = '{error}'; throw e;}}");

		Assert.ThrowsAsync<JSException>(async () => await form.CargoWiseClientServices.FileService.ShowSaveFileDialogAsync("TestFileName.txt"));
	}

	[Test, WithPlaywrightPage]
	public async Task FileServiceShowSaveFileDialogThrowErrorForUnsupportedBrowser()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			return form;
		});
		await page.WaitForSelectorAsync(".form");

		// This mock will set showSaveFilePicker to null to mock an unsupported browser.
		await page.EvaluateAsync("() => delete window.showSaveFilePicker");

		var exception = Assert.ThrowsAsync<JSException>(async () => await form.CargoWiseClientServices.FileService.ShowSaveFileDialogAsync("TestFileName.txt"));
		Assert.That(exception.Message, Does.Contain("Saving files is not supported in this browser."));
	}

	[Test, WithPlaywrightPage]
	public async Task TestFileServiceSaveFileToDirectoryAsync()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;

		var page = await ctx.LoadFormAsync(() => { form = new Form(); return form; });
		Assert.That(form, Is.Not.Null);

		// This mock will return a writable stream, and set window variables for the data written to to stream.
		await page.EvaluateAsync($"() => window.showSaveFilePicker = (options) => {{ window.showSaveFilePickerCalled = true; {returnJSFileHandle};}}");

		await using (var writableBrowserFile = await form.CargoWiseClientServices.FileService.SaveFileToDirectoryAsync("TestFileName.txt"))
		{
			var actual = await page.EvaluateAsync<bool>("window.showSaveFilePickerCalled === true");
			Assert.That(actual, Is.True);
		}

		// mock the dirHandle to have the same directory as the file's name
		await page.EvaluateAsync<bool>($"window.showSaveFilePickerCalled = false; let dirHandle = {{ name:'abc', getFileHandle: (fileName, options) => {{{{ window.getFileHandleCalled = true; window.getFileHandleFileName = fileName; {returnJSFileHandle} }}}}}}; window.showDirectoryPicker = (options) => {{return dirHandle}};");
		await form.CargoWiseClientServices.FileService.ShowDirectoryPickerAsync();

		await using (var writableBrowserFile = await form.CargoWiseClientServices.FileService.SaveFileToDirectoryAsync("abc\\TestFileName.txt"))
		{
			var expected = new string[] { "False", "True", "TestFileName.txt" };
			var actual = await page.EvaluateAsync<string[]>("[window.showSaveFilePickerCalled, window.getFileHandleCalled, window.getFileHandleFileName]");
			Assert.That(actual, Is.EqualTo(expected));
		}
	}

	[Test, WithPlaywrightPage]
	public async Task TestFileServiceSaveFileToDirectoryCallsSaveFileDialogWhenFileNameIsNull()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;

		var page = await ctx.LoadFormAsync(() => { form = new Form(); return form; });
		Assert.That(form, Is.Not.Null);

		// This mock will return a writable stream, and set window variables for the data written to to stream.
		await page.EvaluateAsync($"() => window.showSaveFilePicker = (options) => {{ window.showSaveFilePickerCalled = true; window.suggestedName = options.suggestedName; {returnJSFileHandle};}}");

		await using (var writableBrowserFile = await form.CargoWiseClientServices.FileService.SaveFileToDirectoryAsync(null))
		{
			var actual = await page.EvaluateAsync<string[]>("[window.showSaveFilePickerCalled, window.suggestedName]");
			var expected = new string[] { "True", null };
			Assert.That(actual, Is.EqualTo(expected));
		}
	}

	#endregion

	#region Open Folder Tests

	[Test]
	public async Task FileServiceOpenDirectoryDialogOpensBrowserFolderPicker()
	{
		using var fileService = SetupFileServiceWithMockJS(out var js);
		await fileService.ShowDirectoryPickerAsync();
		AssertJsInvocationCount(js, "OpenDirectoryDialog", 1);
	}

	[Test, WithPlaywrightPage]
	public async Task FileServiceOpenFolderDialogReturnEmptyStringIfCancelled()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			return form;
		});

		await page.WaitForSelectorAsync(".form");

		await page.EvaluateAsync(
			@"() => window.showDirectoryPicker = () => {
var error = new Error();
error.name = 'AbortError';
throw error;
}");

		var fileName = await form.CargoWiseClientServices.FileService.ShowDirectoryPickerAsync();
		Assert.That(fileName, Is.Empty);
	}

	[Test, WithPlaywrightPage]
	public async Task FileServiceOpenFolderDialogRethrowsUnexpectedErrors()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			return form;
		});

		await page.WaitForSelectorAsync(".form");

		// This mock will throw an unexpected error and ensure the JS rethrows it.
		await page.EvaluateAsync("() => window.showDirectoryPicker = () => {throw Error()}");

		Assert.ThrowsAsync<JSException>(async () =>
		{
			await form.CargoWiseClientServices.FileService.ShowDirectoryPickerAsync();
		});
	}

	[Test, WithPlaywrightPage]
	public async Task FileServiceOpenFolderDialogThrowErrorForUnsupportedBrowser()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			return form;
		});

		await page.WaitForSelectorAsync(".form");

		// This mock will set showOpenFilePicker to null to mock an unsupported browser.
		await page.EvaluateAsync("() => delete window.showDirectoryPicker");

		var exception = Assert.ThrowsAsync<JSException>(async () => await form.CargoWiseClientServices.FileService.ShowDirectoryPickerAsync());
		Assert.That(exception.Message, Does.Contain("Uploading directory is not supported in this browser."));
	}

	#endregion
}
