using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Bunit;
using NUnit.Framework;
using WinzorTestFramework;

namespace System.Windows.Forms;

public class UserControlTest
{
	[Test]
	public async Task UserControlDefaultSizeIs150By150()
	{
		using var ctx = new WinzorTestContext();

		Control control = null;

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			control = new UserControl();
		});

		Assert.That(control.Width, Is.EqualTo(150));
		Assert.That(control.Height, Is.EqualTo(150));
	}

	[Test]
	public async Task UserControlOnCreateControlInvokesOnLoad()
	{
		using var ctx = new WinzorTestContext();

		var onLoadInvoked = false;

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var control = new UserControl();
			control.Load += (sender, e) => onLoadInvoked = true;
			control.CreateControl();
		});

		Assert.That(onLoadInvoked, Is.EqualTo(true));
	}

	[TestCase(ImageLayout.Center, "backgroundimage--center")]
	[TestCase(ImageLayout.Stretch, "backgroundimage--stretch")]
	[TestCase(ImageLayout.Tile, "backgroundimage--tile")]
	[TestCase(ImageLayout.Zoom, "backgroundimage--zoom")]
	[TestCase(ImageLayout.None, null)]
	public async Task UserControlWithSetBackgroundImageLayout(ImageLayout layout, string expectedClass)
	{
		using var ctx = new WinzorTestContext();
		using var bitmap = new Bitmap(10, 10);
		(UserControl userControlInstance, IElement userControlElement) = await GetUserControlAndElementAsync(ctx, layout, bitmap);

		Assert.That(userControlInstance, Is.Not.Null, "userControlInstance should not be null");
		Assert.That(userControlElement, Is.Not.Null, "userControlElement should not be null");

		if (expectedClass == null)
		{
			Assert.That(userControlElement.GetAttribute("class"), Is.Null);
		}
		else
		{
			Assert.That(userControlElement.GetAttribute("class"), Does.Contain(expectedClass));
		}
	}

	[Test]
	public async Task UserControlWithOutSetBackgroundImageLayout()
	{
		using var ctx = new WinzorTestContext();
		using var bitmap = new Bitmap(10, 10);
		(UserControl userControlInstance, IElement userControlElement) = await GetUserControlAndElementAsync(ctx, null, bitmap);

		Assert.That(userControlInstance, Is.Not.Null, "userControlInstance should not be null");
		Assert.That(userControlElement, Is.Not.Null, "userControlElement should not be null");

		Assert.That(userControlElement.GetAttribute("class"), Is.Null);
	}

	[TestCase(BorderStyle.None, null)]
	[TestCase(BorderStyle.FixedSingle, "usercontrol--border-fixedsingle")]
	[TestCase(BorderStyle.Fixed3D, "usercontrol--border-fixed3d")]
	public async Task TestUserControlBorderStyle(BorderStyle borderStyle, string expectStyleClass)
	{
		using var ctx = new WinzorTestContext();
		using var bitmap = new Bitmap(10, 10);
		(UserControl userControlInstance, IElement userControlElement) = await GetUserControlAndElementAsync(ctx, null, bitmap, borderStyle);

		if (expectStyleClass == null)
		{
			Assert.That(userControlElement.GetAttribute("class"), Is.Null);
		}
		else
		{
			Assert.That(userControlElement.GetAttribute("class"), Does.Contain(expectStyleClass));
		}
	}

	[TestCase(BorderStyle.None, 0)]
	[TestCase(BorderStyle.FixedSingle, 1)]
	[TestCase(BorderStyle.Fixed3D, 2)]
	public async Task TestBorderStyleAdjustForClientSize(BorderStyle borderStyle, int borderSize)
	{
		using var ctx = new WinzorTestContext();
		using var bitmap = new Bitmap(10, 10);
		(UserControl userControlInstance, IElement userControlElement) = await GetUserControlAndElementAsync(ctx, null, bitmap, borderStyle: borderStyle);

		var bounds = userControlInstance.Bounds;
		Assert.That(userControlInstance.ClientSize.Width + 2 * borderSize, Is.EqualTo(bounds.Width));
		Assert.That(userControlInstance.ClientSize.Height + 2 * borderSize, Is.EqualTo(bounds.Height));
		Assert.That(userControlInstance.ClientAreaBounds.X, Is.EqualTo(bounds.X + borderSize));
		Assert.That(userControlInstance.ClientAreaBounds.Y, Is.EqualTo(bounds.Y + borderSize));
	}

	async Task<IRenderedComponent<ControlProxyComponent>> RenderUserControlAsync(WinzorTestContext ctx, ImageLayout? layout, Bitmap backgroundImage, BorderStyle? borderStyle = null)
	{
		return await ctx.RenderControlOnFormAsync(() =>
		{
			return new UserControl()
			{
				BackgroundImage = backgroundImage,
				BackgroundImageLayout = layout ?? ImageLayout.None,
				BorderStyle = borderStyle ?? BorderStyle.None
			};
		});
	}

	async Task<(UserControl, IElement)> GetUserControlAndElementAsync(WinzorTestContext ctx, ImageLayout? layout, Bitmap backgroundImage, BorderStyle? borderStyle = null)
	{
		IRenderedComponent<ControlProxyComponent> rendered = await RenderUserControlAsync(ctx, layout, backgroundImage, borderStyle);
		UserControl userControlInstance = rendered.Instance.Control.Controls.Single() as UserControl;
		string controlId = userControlInstance.WinzorControlId;
		string cssSelector = $"div[data-winzor-control-id='{controlId}']";
		IElement userControlElement = rendered.Find(cssSelector);

		return (userControlInstance, userControlElement);
	}
}
