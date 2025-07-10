using System.Threading.Tasks;
using Bunit;
using Enterprise.ZArchitecture.GUI.Internal;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class ZGridFindBoxTest
{
	[Test]
	public async Task ZGridFindBoxText()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new ZGridFindBox());
		Assert.That(rendered.Find("div button div div").InnerHtml, Is.EqualTo("..."));
	}
}
