using System.Threading.Tasks;
using Bunit;
using CargoWise.Windows.UI;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

internal class KUserControlTest
{
	[TestCase("pointer-events:none;")]
	[TestCase("margin-block:unset;")]
	public async Task CustomAdditionalControlStyle(string extraStyleString)
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new KUserControl() { ExtraStyleString = extraStyleString });
		Assert.That(rendered.Find("div[data-type='CargoWise.Windows.UI.KUserControl']").GetAttribute("style"), Does.Contain(extraStyleString));
	}
}
