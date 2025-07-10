using System.Collections.Generic;
using System.Threading.Tasks;
using Bunit;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WTG.RtfConverter.Html;
using ResString = Enterprise.ZArchitecture.Core.ResString;

namespace Enterprise.Winzor.Architecture.Test;

class ZButtonTest
{
	[TestCaseSource(nameof(ZButtonToolTipSource))]
	public async Task ZButtonToolTip(MultilingualString caption, bool enable, string expectedTitle)
	{
		// Arrange
		using var context = new EnterpriseTestContext();
		var rendered = await context.RenderControlOnFormAsync((() =>
		{
			var button = new ZButton();
			// Act
			button.ToolTipCaption = caption;
			button.Enabled = enable;
			return button;
		}));
		// Assert
		Assert.That(rendered.Find("button").GetAttribute("title"), Is.EqualTo(expectedTitle));
	}

	[Test]
	public async Task TestButtonClickReEntrancy()
	{
		var invokeCount = 0;
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var button = new ZButton();
			button.Click += delegate
			{
				// if re-entrant, the second call to this method will cause an ObjectDisposedException when trying to show.
				invokeCount++;
				button.Parent.Show();
				button.Parent.Dispose();
			};
			return button;
		});

		var button = rendered.Find("button");
		await Task.WhenAll(
			button.ClickAsync(new WebMouseEventArgs()),
			button.ClickAsync(new WebMouseEventArgs())
		);
		Assert.That(invokeCount, Is.EqualTo(1));
	}

	static IEnumerable<TestCaseData> ZButtonToolTipSource()
	{
#pragma warning disable CW1178 // Do Not Invoke Old Res.GetString Methods; this is a test and the target project does not use ResString
		yield return new TestCaseData(ResString.GetMultilingualString(string.Empty, "test caption"), true, "test caption");
		yield return new TestCaseData(ResString.GetMultilingualString(string.Empty, string.Empty), true, null);
		yield return new TestCaseData(ResString.GetMultilingualString(string.Empty, "test caption"), false, null);
		yield return new TestCaseData(ResString.GetMultilingualString(string.Empty, string.Empty), false, null);
#pragma warning restore CW1178 // Do Not Invoke Old Res.GetString Methods
		yield return new TestCaseData(null, false, null);
		yield return new TestCaseData(null, true, null);
	}
}
