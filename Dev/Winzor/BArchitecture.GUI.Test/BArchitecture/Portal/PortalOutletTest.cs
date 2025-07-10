using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using NUnit.Framework;

namespace WinzorFramework;

internal sealed class PortalOutletTest
{
	[Test]
	public void PortalOutletRendersPortalContent()
	{
		using var ctx = new Bunit.TestContext();
		var rendered = ctx.RenderComponent<ComponentWithPortal>();

		Assert.That(rendered.Find(".portalcontentwrapper").InnerHtml, Is.Empty);
		Assert.That(rendered.Find(".portaloutletwrapper > div").InnerHtml, Is.EqualTo("<p>this is the portal content</p>"));
	}

	[Test]
	public void PortalOutletRerendersWhenPortalContentChanged()
	{
		using var ctx = new Bunit.TestContext();
		var rendered = ctx.RenderComponent<ComponentWithPortal>();

		Assert.That(rendered.Find(".portalcontentwrapper").InnerHtml, Is.Empty);
		Assert.That(rendered.Find(".portaloutletwrapper > div").InnerHtml, Is.EqualTo("<p>this is the portal content</p>"));

		rendered.FindComponent<Portal>().SetParametersAndRender(parameters => parameters.AddChildContent((builder) =>
		{
			builder.OpenElement(7, "p");
			builder.AddContent(8, "this is the new portal content");
			builder.CloseElement();
		}));

		Assert.That(rendered.Find(".portalcontentwrapper").InnerHtml, Is.Empty);
		Assert.That(rendered.Find(".portaloutletwrapper > div").InnerHtml, Is.EqualTo("<p>this is the new portal content</p>"));
	}

	class ComponentWithPortal : ComponentBase
	{
		protected override void BuildRenderTree(RenderTreeBuilder builder)
		{
			base.BuildRenderTree(builder);

			builder.OpenElement(0, "div");
			builder.OpenComponent(1, typeof(PortalProvider));
			builder.AddAttribute(2, "ChildContent", (RenderFragment)((portalProviderContent) =>
			{
				portalProviderContent.OpenElement(3, "div");
				portalProviderContent.AddAttribute(4, "class", "portalcontentwrapper");
				portalProviderContent.OpenComponent(5, typeof(Portal));
				portalProviderContent.AddAttribute(6, "ChildContent", (RenderFragment)((portalContent) =>
				{
					portalContent.OpenElement(7, "p");
					portalContent.AddContent(8, "this is the portal content");
					portalContent.CloseElement();
				}));
				portalProviderContent.CloseComponent();
				portalProviderContent.CloseElement();
				portalProviderContent.OpenElement(9, "div");
				portalProviderContent.AddAttribute(10, "class", "portaloutletwrapper");
				portalProviderContent.OpenComponent(11, typeof(PortalOutlet));
				portalProviderContent.CloseComponent();
				portalProviderContent.CloseElement();
			}));
			builder.CloseComponent();
			builder.CloseElement();
		}
	}
}
