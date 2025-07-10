using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Winzor;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using static WTG.PlaywrightTesting.PlaywrightTestContext;

namespace Enterprise.Winzor.Architecture.Test;

class DocumentsZGridTests
{
	[Test, WithPlaywrightPage]
	public async Task DocumentsZGridCutPasteCopyEvent()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		DocumentsZGrid grid = null;
		var copyTask = new TaskCompletionSource<bool>();
		var cutTask = new TaskCompletionSource<bool>();
		var pasteTask = new TaskCompletionSource<bool>();

		await ctx.LoadFormAsync(() =>
		{
			var data = "data";
			var dataArray = Encoding.UTF32.GetBytes(data);
			var form = new WinzorTestForm();
			var factory = new BusinessObjectFactory();
			var docFactory = new DbBackendDocumentFactory(factory);
			var numFactory = new NumberedBusinessObjectFactory(0, docFactory);
			var parent = factory.New<DummyBusinessObject>();
			var storage = docFactory.New<StorageMain>();
			storage.SM_DB = 0;
			storage.SM_ParentFK = parent.PK;
			var file = StorageFile.NewWithParent_DEBUG(numFactory);
			file.SC_ImageData = new ZBlob(dataArray);
			file.SC_FileName = "test";
			file.SC_DataType = "txt";
			storage.eDocs.Add(file);

			grid = new DocumentsZGrid();
			var column = new ZTextBoxColumnStyleInfo() { ColumnName = "SC_FileNameWithExtension" };
			grid.ReadOnly = true;
			grid.Columns.Add(column);
			grid.ShowCopyMenuItem = true;
			grid.ShowCutMenuItem = true;
			grid.ShowPasteMenuItem = true;
			grid.RebuildContextMenu();
			grid.Copy = (sender, args) => copyTask.SetResult(true);
			grid.Cut = (sender, args) => cutTask.SetResult(true);
			grid.Paste = (sender, args) => pasteTask.SetResult(true);
			form.BindingSource.DataSourceType = typeof(StorageMain);
			form.BindingSource.SetBindingMember(grid, "eDocsView");
			form.SetDataBinding(storage, "");
			form.Controls.Add(grid);
			return form;
		});

		var td = Page.Locator("table > tbody > tr > td").First;

		await MouseMoveOfElementAsync(td, 2, 2);
		await MouseDownAsync();
		await MouseUpAsync();
		await BrowserContext.GrantPermissionsAsync(new[] { "clipboard-write", "clipboard-read" });
		
		Assert.That(await copyTask.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.False);
		await Page.Keyboard.PressAsync("Control+C");
		Assert.That(await copyTask.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);

		Assert.That(await pasteTask.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.False);
		await Page.Keyboard.PressAsync("Control+V");
		Assert.That(await pasteTask.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);

		Assert.That(await cutTask.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.False);
		await Page.Keyboard.PressAsync("Control+X");
		Assert.That(await cutTask.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
	}

	async Task MouseDownAsync(MouseDownOptions options = default)
	{
		await Page.Mouse.DownAsync(options);
		await Task.Delay(500);
	}

	async Task MouseMoveOfElementAsync(ILocator elementHandle, float x, float y)
	{
		var elementRect = await elementHandle.BoundingBoxAsync();
		await Page.Mouse.MoveAsync(elementRect.X + x, elementRect.Y + y);
		await Task.Delay(500);
	}

	async Task MouseUpAsync(MouseUpOptions options = default)
	{
		await Page.Mouse.UpAsync(options);
		await Task.Delay(500);
	}

	[Test, WithPlaywrightPage]
	public async Task Paste_WhenIsInsertAllowedIsFalse_ShouldNotAllowPaste()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		DocumentsZGridForTesting documentsZGrid = null;

		var page = await ctx.LoadControlOnFormAsync(() => documentsZGrid = new DocumentsZGridForTesting(10, false));
		var form = await page.WaitForSelectorAsync(".form");
		Assert.That(form, Is.Not.Null);

		var files = new List<JSClipboardData> { new JSClipboardData("text/plain", "VGhpcyBpcyBhIG5ldyB0ZXh0IGZpbGUgZm9yIHRlc3RpbmcgdGhlIERvY3VtZW50c1pHcmlkLg==", "testTextFile.txt") };
		await form.PasteFiles(files);

		Assert.That(await documentsZGrid.OnWinzorPasteAsyncCompletion.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		Assert.That(documentsZGrid.IsInsertFromDataCalled, Is.False);
		Assert.That(documentsZGrid.IsPasteDocumentsCalled, Is.False);
	}

	[Test, WithPlaywrightPage]
	public async Task Paste_WhenClipboardContainsAValidFile_ShouldInsertFile()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		DocumentsZGridForTesting documentsZGrid = null;

		var page = await ctx.LoadControlOnFormAsync(() => documentsZGrid = new DocumentsZGridForTesting());
		var form = await page.WaitForSelectorAsync(".form");
		Assert.That(form, Is.Not.Null);

		var files = new List<JSClipboardData> { new JSClipboardData("text/plain", "VGhpcyBpcyBhIG5ldyB0ZXh0IGZpbGUgZm9yIHRlc3RpbmcgdGhlIERvY3VtZW50c1pHcmlkLg==", "testTextFile.txt") };
		await form.PasteFiles(files);

		Assert.That(await documentsZGrid.OnWinzorPasteAsyncCompletion.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		Assert.That(documentsZGrid.IsInsertFromDataCalled, Is.True);
		Assert.That(documentsZGrid.IsPasteDocumentsCalled, Is.False);
	}

	[Test, WithPlaywrightPage]
	public async Task Paste_WhenClipboardDoesNotContainAnyFile_ShouldInvokeNormalPaste()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		DocumentsZGridForTesting documentsZGrid = null;

		var page = await ctx.LoadControlOnFormAsync(() => documentsZGrid = new DocumentsZGridForTesting());
		var form = await page.WaitForSelectorAsync(".form");
		Assert.That(form, Is.Not.Null);

		await form.PasteText("This is a text");

		Assert.That(await documentsZGrid.OnWinzorPasteAsyncCompletion.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		Assert.That(documentsZGrid.IsPasteDocumentsCalled, Is.True);
		Assert.That(documentsZGrid.IsInsertFromDataCalled, Is.False);
	}

	[Test, WithPlaywrightPage]
	public async Task Paste_WhenClipboardContainsADangerousFile_ShouldNotInsertFile()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		DocumentsZGridForTesting documentsZGrid = null;

		var page = await ctx.LoadControlOnFormAsync(() => documentsZGrid = new DocumentsZGridForTesting());
		var form = await page.WaitForSelectorAsync(".form");
		Assert.That(form, Is.Not.Null);

		var files = new List<JSClipboardData> { new JSClipboardData("application/application/octet-stream", "VGhpcyBpcyBhIG5ldyB0ZXh0IGZpbGUgZm9yIHRlc3RpbmcgdGhlIERvY3VtZW50c1pHcmlkLg==", "dangerousFile.exe") };
		await form.PasteFiles(files);

		Assert.That(await documentsZGrid.OnWinzorPasteAsyncCompletion.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		Assert.That(UnitTestUserNotification.Instance.LastMessage.Text, Is.EqualTo("The following files were not added because they are potentially dangerous file types:\r\ndangerousFile.exe\r\n"));
		Assert.That(documentsZGrid.IsInsertFromDataCalled, Is.False);
		Assert.That(documentsZGrid.IsPasteDocumentsCalled, Is.False);
	}

	[Test, WithPlaywrightPage, WithTransaction]
	public async Task Paste_WhenClipboardContainsALargeFile_ShouldNotInsertFile()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		DocumentsZGridForTesting documentsZGrid = null;

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			SystemDataRegistry.Instance.eDocsMaximumFilesize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			return documentsZGrid = new DocumentsZGridForTesting(-1);
		});
		var form = await page.WaitForSelectorAsync(".form");
		Assert.That(form, Is.Not.Null);

		var files = new List<JSClipboardData> { new JSClipboardData("text/plain", "VGhpcyBpcyBhIG5ldyB0ZXh0IGZpbGUgZm9yIHRlc3RpbmcgdGhlIERvY3VtZW50c1pHcmlkLg==", "testTextFile.txt") };
		await form.PasteFiles(files);

		Assert.That(await documentsZGrid.OnWinzorPasteAsyncCompletion.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		Assert.That(UnitTestUserNotification.Instance.LastMessage.Text, Is.EqualTo("The following files are larger than the maximum file size (0MB) specified in the registry 'System -> DocManager -> eDocs Maximum File Size':\r\ntestTextFile.txt\r\n"));
		Assert.That(documentsZGrid.IsInsertFromDataCalled, Is.False);
		Assert.That(documentsZGrid.IsPasteDocumentsCalled, Is.False);
	}
}

class DocumentsZGridForTesting : DocumentsZGrid
{
	protected PasteFileHelper PasteFileHelper { get; private set; }

	public DocumentsZGridForTesting(int? maxFileSize = 10, bool isInsertAllowed = true)
	{
		IsInsertAllowed = isInsertAllowed;

		var fileServiceMock = new Mock<IFileService>();

		fileServiceMock
			.Setup(fs => fs.UploadFilesToServerAsync(It.IsAny<BrowserFile[]>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((BrowserFile[] files, long _, CancellationToken _) =>
			{
				return files.Select(f => f.Name).ToArray();
			});

		PasteFileHelper = new PasteFileHelper(fileServiceMock.Object);
	}

	public bool IsInsertFromDataCalled { get; private set; }

	public bool IsPasteDocumentsCalled { get; private set; }

	public WinzorPasteEventArgs LastWinzorPasteEventArgs { get; private set; }

	public TaskCompletionSource<bool> OnWinzorPasteAsyncCompletion { get; } = new TaskCompletionSource<bool>();

	protected override async Task OnWinzorPasteAsync(WinzorPasteEventArgs args)
	{
		await base.OnWinzorPasteAsync(args);
		OnWinzorPasteAsyncCompletion.SetResult(true);
	}

	protected override void InsertFromData(IDataObject dataToInsert)
	{
		IsInsertFromDataCalled = true;
	}

	protected override void Grid_PasteDocuments(object sender, EventArgs e)
	{
		IsPasteDocumentsCalled = true;
	}

	protected override PasteFileHelper CreatePasteFileHelper()
	{
		return PasteFileHelper;
	}
}
