using System;
using Bunit;
using Microsoft.AspNetCore.Components;
using Moq;
using NUnit.Framework;

namespace WinzorFramework;

internal sealed class PortalTest
{
	[Test]
	public void PortalContentAdded()
	{
		using var ctx = new Bunit.TestContext();
		var providerMock = new Mock<IPortalProvider>();
		providerMock.Setup(m => m.AddOrUpdatePortalContent(It.IsAny<Guid>(), It.IsAny<RenderFragment>()));
		var rendered = ctx.RenderComponent<Portal>(parameters => parameters
			.Add(p => p.Provider, providerMock.Object)
			.AddChildContent("<p>This should be rendered in a portal</p>")
		);

		providerMock.Verify(m => m.AddOrUpdatePortalContent(It.IsAny<Guid>(), It.IsAny<RenderFragment>()), Times.Once);
	}

	[Test]
	public void PortalContentUpdated()
	{
		using var ctx = new Bunit.TestContext();
		var providerMock = new Mock<IPortalProvider>();
		providerMock.Setup(m => m.AddOrUpdatePortalContent(It.IsAny<Guid>(), It.IsAny<RenderFragment>()));
		var rendered = ctx.RenderComponent<Portal>(parameters => parameters
			.Add(p => p.Provider, providerMock.Object)
			.AddChildContent("<p>This should be rendered in a portal</p>")
		);

		rendered.SetParametersAndRender(parameters => parameters
			.AddChildContent("<p>This updated content should be rendered in a portal</p>")
		);

		providerMock.Verify(m => m.AddOrUpdatePortalContent(It.IsAny<Guid>(), It.IsAny<RenderFragment>()), Times.Exactly(2));
	}

	[Test]
	public void PortalContentRemoved()
	{
		using var ctx = new Bunit.TestContext();
		var providerMock = new Mock<IPortalProvider>();
		providerMock.Setup(m => m.RemovePortalContent(It.IsAny<Guid>()));
		var rendered = ctx.RenderComponent<Portal>(parameters => parameters
			.Add(p => p.Provider, providerMock.Object)
			.AddChildContent("<p>This should be rendered in a portal</p>")
		);

		ctx.DisposeComponents();

		providerMock.Verify(m => m.RemovePortalContent(It.IsAny<Guid>()), Times.Once);
	}
}
