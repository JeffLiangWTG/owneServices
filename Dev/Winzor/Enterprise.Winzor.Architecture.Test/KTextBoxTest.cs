using System.Threading.Tasks;
using Bunit;
using CargoWise.Windows.UI;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class KTextBoxTest
{
	[Test]
	public async Task KTextBoxPlaceHolderText()
	{
		using var ctx = new EnterpriseTestContext();
		KTextBox kTextBox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => kTextBox = new KTextBox());
		Assert.That(rendered.RenderCount, Is.EqualTo(1));

		var input = rendered.Find("input");
		Assert.That(input, Is.Not.Null);
		Assert.That(input.GetAttribute("placeholder"), Is.Empty);

		await kTextBox.InvokeWinzorDispatcherAsync(() => kTextBox.PlaceHolderText = "test text info");
		Assert.That(input.GetAttribute("placeholder"), Is.EqualTo("test text info"));
	}
}
