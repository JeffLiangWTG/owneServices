using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

using static PlaywrightTestContext;

public class FolderBrowserDialogTest
{
	[Test, WithPlaywrightPage]
	public async Task OpenFolderDialogOpensFolderPicker()
	{
		await using var ctx = new InMemoryTestServerContext();
		FolderBrowserDialog dialog = null;
		Form form = null;
		var dialogClosed = new TaskCompletionSource<bool>();

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			dialog = new FolderBrowserDialog();
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
		fileServiceMock.Setup(i => i.ShowDirectoryPickerAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(() =>
		{
			fileDialogOpened.SetResult(true);
			return Task.FromResult(string.Empty);
		});
		form.CargoWiseClientServices.FileService = fileServiceMock.Object;

		await (await page.WaitForSelectorAsync("button")).ClickAsync();
		Assert.That(await fileDialogOpened.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		Assert.That(await dialogClosed.Task, Is.True);
	}

	[TestCase(typeof(JSException), TestName = "{m}_JSException")]
	[TestCase(typeof(TimeoutException), TestName = "{m}_Timeout")]
	public async Task TestFolderBrowserDialogShowDialogThrows(Type exType)
	{
		using var ctx = new WinzorTestContext();
		Form form = null;
		var fileDialogOpened = new TaskCompletionSource<DialogResult>();

		_ = await ctx.RenderFormAsync(() => form = new Form());
		var fileServiceMock = new Mock<IFileService>();
		_ = fileServiceMock.Setup(i => i.ShowDirectoryPickerAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(() =>
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
				var dialog = new FolderBrowserDialog();
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
