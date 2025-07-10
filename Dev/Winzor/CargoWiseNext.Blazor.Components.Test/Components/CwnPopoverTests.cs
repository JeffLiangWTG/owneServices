using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CargoWiseNext.Blazor.Components.Test;

public class CwnPopoverTests : BunitTestContext
{
	[Test]
	public void CwnPopover_Renders()
	{
		Services.AddSingleton(new Mock<IPopoverService>().Object);
		// Act
		var cut = RenderComponent<CwnPopover>();

		// Assert
		cut.MarkupMatches(
						"<div id=\"00000000-0000-0000-0000-000000000000\" class=\"cwn-popover\" popover=\"manual\" >\r\n  "
						+ "<div class=\"cwn-popover__content\"></div>"
						+ "</div>");
	}

	[Test]
	public void CwnPopover_RendersWithChildContent()
	{
		Services.AddSingleton(new Mock<IPopoverService>().Object);
		// Act
		var cut = RenderComponent<CwnPopover>(parameters => parameters.AddChildContent("Child content"));

		// Assert
		cut.MarkupMatches(
						"<div id=\"00000000-0000-0000-0000-000000000000\" class=\"cwn-popover\" popover=\"manual\" >\r\n  "
						+ "<div class=\"cwn-popover__content\">Child content</div>"
						+ "</div>");
	}

	[Test]
	public void CwnPopover_PopoverAPITest()
	{
		var mockPopoverService = new Mock<IPopoverService>();
		Services.AddSingleton(mockPopoverService.Object);
		// Act
		var cut = RenderComponent<CwnPopover>(
			parameters => parameters.Add(p => p.Id, "custom-id")
									.AddChildContent("Child content"));

		// Assert
		cut.MarkupMatches(
			"<div id=\"custom-id\" class=\"cwn-popover\" popover=\"manual\" >\r\n  "
			+ "<div class=\"cwn-popover__content\">Child content</div>"
			+ "</div>");

		// Act
		cut.Instance.Show();
		cut.Instance.Hide();
		cut.Instance.Toggle();

		// Assert
		mockPopoverService.Verify(x => x.ShowAsync("custom-id"), Times.Once);
		mockPopoverService.Verify(x => x.HideAsync("custom-id"), Times.Once);
		mockPopoverService.Verify(x => x.ToggleAsync("custom-id"), Times.Once);
	}
}
