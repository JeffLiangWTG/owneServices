using System.Threading.Tasks;
using Bunit;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class KButtonTest
{
	[TestCase("text", "toolTipCaption", "caption", "text")]
	[TestCase(null, "toolTipCaption", "caption", "toolTipCaption")]
	[TestCase(null, null, "caption", "caption")]
	[TestCase(null, null, null, null)]

	public async Task KButtonToolTip(string text, string toolTipCaption, string caption, string expectedTitle)
	{
		using var context = new EnterpriseTestContext();
		var rendered = await context.RenderControlOnFormAsync(() =>
		{
			var button = new KButton { Text = text };
#pragma warning disable CW1178 // ResString is not used in Enterprise.Winzor.Architecture so is not available here.  This suffices for the test's needs.
			button.CaptionRendererExposedForTest.ToolTipCaption = toolTipCaption is null ? null : ZArchitecture.Core.ResString.GetMultilingualString(string.Empty, toolTipCaption);
#pragma warning restore CW1178 // Do Not Invoke Old Res.GetString Methods
			button.CaptionRendererExposedForTest.Captions = caption is null ? null : new string[] { caption };
			return button;
		});
		Assert.That(rendered.Find("button").GetAttribute("title"), Is.EqualTo(expectedTitle));
	}

	[TestCase(null, false, "text")]
	[TestCase("toolTipText", true, "text")]
	[TestCase("toolTipText", false, "toolTipText")]
	public async Task KButtonToolTipOnlyGetsOverridenWhenIsNullOrRendererHasAutoSetToolTip(string toolTipText, bool hasAutoSetToolTip, string expectedTitle)
	{
		using var context = new EnterpriseTestContext();
		var rendered = await context.RenderControlOnFormAsync(() =>
		{
			var button = new KButton { Text = "text", ToolTipText = toolTipText };
			button.CaptionRendererExposedForTest.HasAutoSetToolTip = hasAutoSetToolTip;
			return button;
		});
		Assert.That(rendered.Find("button").GetAttribute("title"), Is.EqualTo(expectedTitle));
	}
}
