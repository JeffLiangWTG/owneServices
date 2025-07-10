using System.Drawing;
using System.Threading.Tasks;
using Bunit;
using NUnit.Framework;
using WinzorTestFramework;

namespace System.Windows.Forms.Adaptive;

public class PreviewBoxTest
{
	[Test]
	[TestCase(PreviewBoxSizeMode.Normal, "width:20px;height:10px;top:0px;left:0px;", TestName = "{m}_Normal")]
	[TestCase(PreviewBoxSizeMode.Stretch, "width:40px;height:40px;top:0px;left:0px;", TestName = "{m}_Stretch")]
	[TestCase(PreviewBoxSizeMode.AutoSize, "width:20px;height:10px;top:0px;left:0px;", TestName = "{m}_AutoSize")]
	[TestCase(PreviewBoxSizeMode.Center, "width:20px;height:10px;top:15px;left:10px;", TestName = "{m}_Center")]
	[TestCase(PreviewBoxSizeMode.Zoom, "width:40px;height:20px;top:10px;left:0px;", TestName = "{m}_Zoom")]
	public async Task PreviewBoxSizeModeTest(PreviewBoxSizeMode sizeMode, String result )
	{
		Button button;
		PreviewBox previewBox;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			button = new Button { Text = "buttontext", Size = new Size(20, 10) };
			previewBox = new PreviewBox()
			{
				Size = new Size(40, 40),
				PreviewControl = button,
				SizeMode = sizeMode
			};

			return previewBox;
		});

		var markup = rendered.Markup;
		Assert.That(rendered.Find(".button").GetAttribute("style"), Does.Contain(result));
	}

	[Test]
	public async Task SetPreviewControlTest()
	{
		PreviewBox previewBox = null;
		Button renderButton = null;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			renderButton = new Button() { Text = "test button" };
			previewBox = new PreviewBox() { PreviewControl = renderButton, Dock = DockStyle.Fill };

			return previewBox;
		});

		Assert.That(rendered.Find("body > div.form > div > button"), Is.Not.Null);
		Assert.That(previewBox.WinzorSpecificControls, Does.Contain(renderButton));
	}
}
