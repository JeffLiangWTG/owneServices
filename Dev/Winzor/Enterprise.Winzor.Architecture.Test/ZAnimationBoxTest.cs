using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Bunit;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class ZAnimationBoxTest
{
	[Test]
	public async Task AnimationBoxShouldHaveBackgroundImage()
	{
		using var context = new EnterpriseTestContext();
		var rendered = await context.RenderControlOnFormAsync((() =>
		{
			var animationBox = new ZAnimationBox();
			var resources = new System.ComponentModel.ComponentResourceManager(typeof(SaveProgressForm));
			animationBox.Image = ((System.Drawing.Image)(resources.GetObject("Animation.Image")));
			return animationBox;
		}));

		var zAnimationBox = rendered.Instance.Control.Controls.Single() as ZAnimationBox;
		Assert.That(zAnimationBox, Is.Not.Null);

		var controlId = zAnimationBox.WinzorControlId;
		var cssSelector = $"div[data-winzor-control-id='{controlId}']";
		IElement zAnimationElement = rendered.Find(cssSelector);

		Assert.That(zAnimationElement.GetAttribute("style"), Does.Contain("background-image"));
	}
}
