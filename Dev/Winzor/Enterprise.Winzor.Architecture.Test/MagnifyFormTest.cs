using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Imaging;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.GUI;
using Enterprise.DocumentScanning.GUI.JSInterop;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WTG.PlaywrightTesting;
using static WTG.PlaywrightTesting.PlaywrightTestContext;
namespace Enterprise.Winzor.Architecture.Test;

class MagnifyFormTest
{
	[Test]
	public async Task PreloadMagnifyFormJSInterop()
	{
		using var ctx = new EnterpriseTestContext();
		var services = ctx.MockCargoWiseClientServices;
		var interop = new Mock<IMagnifyFormJSInterop>();
		interop.Setup(i => i.PreloadInterop());
		ctx.Services.AddScoped(_ => interop.Object);

		var rendered = await ctx.RenderFormAsync(() => SetupMagnifyForm(testImage, "50"));

		interop.Verify(e => e.PreloadInterop(), Times.Once());
	}

	[Test, WithPlaywrightPage]
	public async Task MagnifyFormContentsDisplayCorrectly()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = SetupMagnifyForm(testImage, "50");
			return form;
		});

		var img = await page.WaitForSelectorAsync(".picturebox img");
		Assert.That(img, Is.Not.Null);
		Assert.That(await img.GetAttributeAsync("src"), Does.StartWith("data:image/png"));

		var bounds = await img.BoundingBoxAsync();
		Assert.That(bounds.X, Is.EqualTo(0));
		Assert.That(bounds.Y, Is.EqualTo(30));
		Assert.That(bounds.Width, Is.EqualTo(testImage.Width / 2)); //50% width
		Assert.That(bounds.Height, Is.EqualTo(testImage.Height / 2)); //50% height
	}

	[Test, WithPlaywrightPage]
	public async Task MagnifyFormContentsMoveUsingToolbarButtons()
	{
		ElementHandleBoundingBoxResult bounds = null;

		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = SetupMagnifyForm(largeImage, "300", new Size(100, 100));
			return form;
		});

		var pictureBox = await page.WaitForSelectorAsync(".picturebox");
		Assert.That(pictureBox, Is.Not.Null);
		var img = await pictureBox.WaitForSelectorAsync("img");
		Assert.That(img, Is.Not.Null);

		var oldBounds = await pictureBox.BoundingBoxAsync();

		await GetTaskbarMoveButton(page, "down").ClickAsync();
		Assert.That(async () => (bounds = await pictureBox.BoundingBoxAsync()).Y, Is.EqualTo(oldBounds.Y - 23).After(2000, 100), "Down arrow clicked.");
		Assert.That(bounds.X, Is.EqualTo(oldBounds.X));

		await GetTaskbarMoveButton(page, "up").ClickAsync();
		Assert.That(async () => (bounds = await pictureBox.BoundingBoxAsync()).Y, Is.EqualTo(oldBounds.Y).After(2000, 100), "Up arrow clicked.");
		Assert.That(bounds.X, Is.EqualTo(oldBounds.X));

		await GetTaskbarMoveButton(page, "right").ClickAsync();
		Assert.That(async () => (bounds = await pictureBox.BoundingBoxAsync()).X, Is.EqualTo(oldBounds.X - 23).After(2000, 100), "Right arrow clicked.");
		Assert.That(bounds.Y, Is.EqualTo(oldBounds.Y));

		await GetTaskbarMoveButton(page, "left").ClickAsync();
		Assert.That(async () => (bounds = await pictureBox.BoundingBoxAsync()).X, Is.EqualTo(oldBounds.X).After(2000, 100), "Left arrow clicked.");
		Assert.That(bounds.Y, Is.EqualTo(oldBounds.Y));
	}

	[Test, WithPlaywrightPage]
	public async Task MagnifyFormContentsMoveWhenDraggingWithMouse()
	{
		ElementHandleBoundingBoxResult bounds = null;

		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = SetupMagnifyForm(largeImage, "300");
			return form;
		});

		var pictureBox = await page.WaitForSelectorAsync(".picturebox");
		Assert.That(pictureBox, Is.Not.Null);
		var oldBounds = await pictureBox.BoundingBoxAsync();

		await page.Mouse.MoveAsync(140, 150);
		await page.Mouse.DownAsync();
		await page.Mouse.MoveAsync(130, 130, new MouseMoveOptions() { Steps = 30 });
		await page.Mouse.UpAsync();

		Assert.That(async () => (bounds = await pictureBox.BoundingBoxAsync()).X, Is.EqualTo(oldBounds.X - 10).Within(2).After(2000, 100));
		Assert.That(bounds.Y, Is.EqualTo(oldBounds.Y - 20).Within(2));
	}

	[Test, WithPlaywrightPage]
	public async Task MagnifyFormToolbar()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() => SetupMagnifyForm(largeImage, "300"));

		Assert.That(await (page.GetByTitle("Click to scroll left")).CountAsync(), Is.Not.Zero);
		Assert.That(await (page.GetByTitle("Click to scroll up")).CountAsync(), Is.Not.Zero);
		Assert.That(await (page.GetByTitle("Click to scroll down")).CountAsync(), Is.Not.Zero);
		Assert.That(await (page.GetByTitle("Click to scroll right")).CountAsync(), Is.Not.Zero);
		Assert.That(await (page.GetByTitle("Select Text")).CountAsync(), Is.Zero);
		Assert.That(await (page.GetByTitle("Erase Selection Box")).CountAsync(), Is.Zero);
		Assert.That(await (page.GetByTitle("Convert to Text")).CountAsync(), Is.Zero);
	}

	[Test, WithPlaywrightPage]
	public async Task MagnifyFormContextMenu()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() => SetupMagnifyForm(testImage, "300"));

		var pictureBox = page.Locator(".picturebox");
		await pictureBox.WaitForAsync();

		await pictureBox.ClickAsync(new LocatorClickOptions() {
			Button = MouseButton.Right,
			Position = new Position { X = 50, Y = 50 }
		});

		Assert.That(await (page.GetByTitle("Erase Selection Box")).CountAsync(), Is.Zero);
		Assert.That(await (page.GetByTitle("Convert to Text")).CountAsync(), Is.Zero);
	}

	[Test, WithPlaywrightPage]
	public async Task MagnifyFormContentsZoomCorrectly()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = SetupMagnifyForm();
			return form;
		});

		var img = await page.WaitForSelectorAsync(".picturebox img");
		var bounds = await img.BoundingBoxAsync();

		foreach (var size in new int[] { 50, 100, 50, 125, 75, 25 })
		{
			await SetZoomSize(page, size);
			var multiplier = (float)size / 100;
			Assert.That(async () => (bounds = await img.BoundingBoxAsync()).Width, Is.EqualTo(testImage.Width * multiplier).After(2000, 100));
			Assert.That(bounds.Height, Is.EqualTo(testImage.Height * multiplier));
		}
	}

	[Test, WithPlaywrightPage]
	public async Task MagnifyFormLoadsFitToPage()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = SetupMagnifyForm(testImage, "0");
			return form;
		});
		var img = await page.WaitForSelectorAsync(".picturebox img");
		Assert.That(img, Is.Not.Null);
		Assert.That(await img.GetAttributeAsync("src"), Does.StartWith("data:image/png"));

		var pictureBox = await page.QuerySelectorAsync(".picturebox");
		Assert.That(async () => await pictureBox.EvaluateAsync<int>("e => e.getBoundingClientRect().width"),
			Is.EqualTo(await pictureBox.EvaluateAsync<int>("e => e.parentElement.parentElement.getBoundingClientRect().width")));
	}

	[TestCase("bbb86a14-33c6-4f98-acc0-73aea94e97fe", "Magnifying Window")]
	public async Task ShowFormTitleWhenMagnifyFormLoad(string resourceKey,string englishText)
	{
		using var ctx = new EnterpriseTestContext();

		using var imageSource = new DummyIImagePageSelector(testImage);
		using var magnifyManager = new MagnifyManager(imageSource);
		MagnifyForm magnifyForm = null;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			magnifyForm = new MagnifyForm(magnifyManager);
			return magnifyForm;
		});
		var partTitle = Res.GetString(resourceKey,"{0}", englishText);

		Assert.That(magnifyForm.Text, Does.Contain(partTitle));
	}

	[Test, WithPlaywrightPage]
	public async Task MagnifyFormCanBeSetToFitToPage()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = SetupMagnifyForm(testImage, "100");
			return form;
		});

		var pictureBox = await page.WaitForSelectorAsync(".picturebox");
		Assert.That(pictureBox, Is.Not.Null);
		Assert.That(await pictureBox.EvaluateAsync<int>("e => e.getBoundingClientRect().width"), Is.EqualTo(testImage.Width));

		await SetZoomSize(page, 0);
		Assert.That(async () => await pictureBox.EvaluateAsync<int>("e => e.getBoundingClientRect().width"),
			Is.EqualTo(await pictureBox.EvaluateAsync<int>("e => e.parentElement.parentElement.getBoundingClientRect().width")).After(2000, 100));

		await SetZoomSize(page, 100);
		Assert.That(async () => await pictureBox.EvaluateAsync<int>("e => e.getBoundingClientRect().width"), Is.EqualTo(testImage.Width).After(2000, 100));
	}

	#region Helper Methods
	Form SetupMagnifyForm(Image specifiedImage = null, string zoom = "100", Size? formSize = null)
	{
		var image = specifiedImage ?? testImage;
#pragma warning disable CA2000 // Dispose objects before losing scope
		var imageSource = new DummyIImagePageSelector(image);
		var magnifyManager = new MagnifyManager(imageSource);
#pragma warning restore CA2000 // Dispose objects before losing scope
		magnifyManager.Zoom = zoom;

		var form = new MagnifyForm(magnifyManager);
		magnifyManager.MagForm = form;

		if (formSize != null)
		{
			form.Size = formSize.Value;
		}
				
		return form;
	}
	
	async Task SetZoomSize(IPage page, int size)
	{
		var dropEdit = await page.WaitForSelectorAsync("div[data-name=\"ZoomDropEdit\"]>div", new PageWaitForSelectorOptions { Timeout = 10000 });
		Assert.That(dropEdit, Is.Not.EqualTo(null));
		await (await dropEdit.WaitForSelectorAsync("button", new ElementHandleWaitForSelectorOptions { Timeout = 3000 })).ClickAsync();

		var text = size == 0 ? "'Fit to Width'" : size.ToString();
		await page.ClickAsync($"text={text}");
	}

	ILocator GetTaskbarMoveButton(IPage page, string direction) => page.GetByTitle("Click to scroll " + direction).First;
	#endregion

	#region DummyIImagePageSelector
	public sealed class DummyIImagePageSelector : IDisposable, IImagePageSelector
	{
		public DummyIImagePageSelector(Image image)
		{
			fImage = image;
		}

		public IImagePageSelector PageSelector
		{
			get { return this; }
		}

		public int TotalPages
		{
			get { return fImage.GetFrameCount(System.Drawing.Imaging.FrameDimension.Page); }
		}

		public int CurrentPageIndex
		{
			get
			{
				return fCurrentPageIndex;
			}
			set
			{
				if (fImage == null)
				{
					fCurrentPageIndex = -1;
				}
				else
				{
					if ((value >= 0) && (value < TotalPages))
					{
						fImage.SelectActiveFrame(System.Drawing.Imaging.FrameDimension.Page, value);
						fCurrentPageIndex = value;
					}
				}
			}
		}
		int fCurrentPageIndex;

		public Image CurrentImage
		{
			get { return fImage; }
		}

		public void Dispose()
		{
			fImage.Dispose();
			fImage = null;
		}

		Image fImage;
	}
	#endregion

	#region Test Data
	static Image testImage => Image.FromStream(new MemoryStream(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAABHNCSVQICAgIfAhkiAAAAAlwSFlzAAAA3QAAAN0BcFOiBwAAABl0RVh0U29mdHdhcmUAd3d3Lmlua3NjYXBlLm9yZ5vuPBoAAAKlSURBVFiFtZXPSxVRFMc/xwaJl8kDexC60EWrwAQRWoeiu1LCNiJS2EroX2jjoo0gtHi11FYtWmsQ5dJFiCbtAheSggU+QizBx2kxZ/R2m5k3c60Dhzdzfny/591z5lxUlbIKCDALfDSdBSQIK7CAMUA9HQvBaiNMHhe0tZTQAg4K2lpLYAsGjDA5/gNgIARLDLC0iEg38MVeb6jqXhBOaAFWRANAVauhGKEzgIjUgAiI7DlMAvrfDjwHjjmfgWOztZfFiwJqfgGMAO+BT2a7BdwDrgCPyoCVmgERiYAOVW1k+KvAkaqe/pcCjOQScBMYMhXOV/JnVW2WAizZ+2f82Xtfjy2m8CyUWTxbOcS+blFwMRUhnwROSpAnegJMXqgAoAc4DCBP9BDouUgBKxcgT3QlqABg+h+QJzqdxXO2ikWkYhdMIndJl58Z9jzfGZaIdItIxXXWgDrwA9h0TmDH+xdLxBuvAjwEdh3fLvEGrFjMkpe74+BuGlfduJl3AjctqMsD2CO+eNqBXouZc/xzZuu1mMhyXIwup4DENg+wn1LAsJe8YPZF4BS4BlSBpmnVbKfAosUueBjDKQXsR8D1v1vGVe/9tv2+Ar6p6ncRGeH8Oh9S1Xci8hRY9XKyMEm43SqTExj37AqMOn3sNKLEtwp0Ov7RlPzxlBPQMtfxWxHZALaB+0CH4xsDvorIG6AfGCwKGgEN4h4WkcEc8A5gpiixSSMCloEnJZJ+Aa+BNc9+B3gAXC6BtQzQR7xym+TPgBJ/v7Wc7VmzmLRcdwaaxtnnJvcST3NeAXXzTwEf7BTW7HnKfPUWBQxhuyTzLiAepDSQCfNvpPg2zDeRkdufexe4oqrbwHqay37T8tq8GFfWDTMzKU1mgJfAUU5MKzkyjMyv4zeJZBAjCmCS4wAAAABJRU5ErkJggg==")));

	static Image largeImage => Image.FromStream(new MemoryStream(Convert.FromBase64String("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")));
	#endregion
}
