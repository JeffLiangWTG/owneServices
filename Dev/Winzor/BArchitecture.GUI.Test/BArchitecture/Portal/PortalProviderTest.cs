using System;
using System.Linq;
using Microsoft.AspNetCore.Components;
using NUnit.Framework;

namespace WinzorFramework;

internal sealed class PortalProviderTest
{
	[Test]
	public void AddPortalContent()
	{
		var portalProvider = new PortalProvider();
		var portalChangedCount = 0;
		portalProvider.PortalChanged += (_, _) => portalChangedCount++;

		portalProvider.AddOrUpdatePortalContent(Guid.NewGuid(), new RenderFragment(builder =>
		{
			builder.OpenElement(0, "p");
			builder.AddContent(1, "Hello");
			builder.CloseElement();
		}));

		Assert.That(portalProvider.PortalInstances.Count, Is.EqualTo(1));
		Assert.That(portalChangedCount, Is.EqualTo(1));

		portalProvider.AddOrUpdatePortalContent(Guid.NewGuid(), new RenderFragment(builder =>
		{
			builder.OpenElement(0, "p");
			builder.AddContent(1, "Goodbye");
			builder.CloseElement();
		}));

		Assert.That(portalProvider.PortalInstances.Count, Is.EqualTo(2));
		Assert.That(portalChangedCount, Is.EqualTo(2));
	}

	[Test]
	public void UpdatePortalContent()
	{
		var portalProvider = new PortalProvider();
		var portalId = Guid.NewGuid();

		portalProvider.AddOrUpdatePortalContent(portalId, new RenderFragment(builder =>
		{
			builder.OpenElement(0, "p");
			builder.AddContent(1, "Hello");
			builder.CloseElement();
		}));

		var portalChangedCount = 0;
		portalProvider.PortalChanged += (_, _) => portalChangedCount++;

		portalProvider.AddOrUpdatePortalContent(portalId, new RenderFragment(builder =>
		{
			builder.OpenElement(0, "p");
			builder.AddContent(1, "Goodbye");
			builder.CloseElement();
		}));

		Assert.That(portalProvider.PortalInstances.Count, Is.EqualTo(1));
		Assert.That(portalChangedCount, Is.EqualTo(1));
	}

	[Test]
	public void RemovePortalContent()
	{
		var portalProvider = new PortalProvider();
		var portalId = Guid.NewGuid();

		portalProvider.AddOrUpdatePortalContent(portalId, new RenderFragment(builder =>
		{
			builder.OpenElement(0, "p");
			builder.AddContent(1, "Hello");
			builder.CloseElement();
		}));

		var portalChangedCount = 0;
		portalProvider.PortalChanged += (_, _) => portalChangedCount++;

		portalProvider.RemovePortalContent(portalId);

		Assert.That(portalProvider.PortalInstances.Count, Is.EqualTo(0));
		Assert.That(portalChangedCount, Is.EqualTo(1));
	}

	[Test]
	public void PortalProviderRendersChildContent()
	{
		using var ctx = new Bunit.TestContext();
		var rendered = ctx.RenderComponent<PortalProvider>(parameters => parameters
			.AddChildContent("<p>Child Content</p>")
		);

		Assert.That(rendered.Markup, Does.Contain("<p>Child Content</p>"));
	}
}
