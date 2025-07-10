using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;
using WinzorFramework.JSInterop;
using WTG.PlaywrightTesting;
using static Enterprise.ZArchitecture.GUI.Testing.ZFilterStripControlWithDummyTest;

namespace Enterprise.Winzor.Architecture.Test;
class ZFilterStripControlTest
{
	[Test, WithPlaywrightPage]
	public async Task DragFilesOntoFilteredGridShouldTriggerOnDragDrop()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var dragDropEventFired = new TaskCompletionSource<DragEventArgs>();
		var form = default(ZForm);
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new ZForm();
			var module = new WinzorOverridenFilterGridModule();
			var filter = (WinzorOverridenFilterControl)module.EmbeddedControl;
			filter.dragDropEventFired = dragDropEventFired;
			form.Controls.Add(filter);
			return form;
		});

		var fileServiceMock = new Mock<IFileService>();
		fileServiceMock
			.Setup(i => i.UploadFilesToServerAsync(It.IsAny<BrowserFile[]>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
			.Returns<BrowserFile[], long, CancellationToken>((files, _, _) => Task.FromResult(files.Select(f => f.Name).ToArray()));
		form.CargoWiseClientServices.FileService = fileServiceMock.Object;

		var filteredGrid = page.Locator(".datagrid");
		var dataTransfer = await page.EvaluateHandleAsync("() => new DataTransfer()");
		await dataTransfer.EvaluateAsync("data => data.items.add(new File(['This is file one.'], 'File1.txt', {type: 'text/plain', lastModified: 1654862400000}))");
		await dataTransfer.EvaluateAsync("data => data.items.add(new File(['This is file two.'], 'File2.txt', {type: 'text/plain', lastModified: 1654866000000}))");

		await filteredGrid.DispatchEventAsync("drop", new { dataTransfer, clientX = 50, clientY = 200 });

		var receivedDragEventArgs = await dragDropEventFired.Task;
		var values = (string[])receivedDragEventArgs.Data.GetData(DataFormats.FileDrop);
		Assert.That(values.Length, Is.EqualTo(2));
		Assert.That(values[0], Does.Contain("File1.txt"));
		Assert.That(values[1], Does.Contain("File2.txt"));
		Assert.That(receivedDragEventArgs.X, Is.EqualTo(50));
		Assert.That(receivedDragEventArgs.Y, Is.EqualTo(200));
	}

	class WinzorOverridenFilterGridModule : OverridenFilterGridModule
	{
		protected override IFilterControl GetNewFilterControl()
		{
			return new WinzorOverridenFilterControl(GridCollection, FilterBusinessObject);
		}
	}

	class WinzorOverridenFilterControl : OverridenFilterControl
	{
		public TaskCompletionSource<DragEventArgs> dragDropEventFired;

		public WinzorOverridenFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
				: base(gridCollection, filterBusinessObject)
		{
		}

		protected override void OnDragDrop(DragEventArgs e)
		{
			dragDropEventFired.SetResult(e);
		}
	}
}
