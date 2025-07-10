using Bunit;

namespace CargoWiseNext.Blazor.Components.Test;

public class CwnIconTest : BunitTestContext
{
	[TestCase(Icon.Search, "cwn-icon--search")]
	[TestCase(Icon.Settings, "cwn-icon--settings")]
	[TestCase(Icon.MenuMeatballs, "cwn-icon--menu-meatballs")]
	[TestCase(Icon.Help, "cwn-icon--help")]
	[TestCase(Icon.StarFilled, "cwn-icon--star-filled")]
	[TestCase(Icon.StarEmpty, "cwn-icon--star-empty")]
	[TestCase(Icon.History, "cwn-icon--history")]
	public void CwnIcon_RenderWithIcon(Icon icon, string expectedClass)
	{
		// Act
		var cut = RenderComponent<CwnIcon>(parameters => parameters
			.Add(p => p.Icon, icon)
		);

		// Assert
		cut.MarkupMatches(@$"
			<span
				class=""
					cwn-icon
					{expectedClass}
					cwn-icon--medium
				""
				aria-hidden=""true""
				role=""img""
			/>");
	}

	[TestCase(Icon.Search, "cwn-icon--search__hover")]
	[TestCase(Icon.Settings, "cwn-icon--settings__hover")]
	[TestCase(Icon.MenuMeatballs, "cwn-icon--menu-meatballs__hover")]
	[TestCase(Icon.Help, "cwn-icon--help__hover")]
	[TestCase(Icon.StarFilled, "cwn-icon--star-filled__hover")]
	[TestCase(Icon.StarEmpty, "cwn-icon--star-empty__hover")]
	[TestCase(Icon.History, "cwn-icon--history__hover")]
	public void CwnIcon_RenderWithHoverIcon(Icon hoverIcon, string expectedClass)
	{
		// Arrange
		var icon = Icon.Search;

		// Act
		var cut = RenderComponent<CwnIcon>(parameters => parameters
			.Add(p => p.Icon, icon)
			.Add(p => p.HoverIcon, hoverIcon)
		);

		// Assert
		cut.MarkupMatches(@$"
			<span
				class=""
					cwn-icon
					cwn-icon--search
					{expectedClass}
					cwn-icon--medium
				""
				aria-hidden=""true""
				role=""img""
			/>");
	}

	[TestCase(Size.Small, "cwn-icon--small")]
	[TestCase(Size.Medium, "cwn-icon--medium")]
	[TestCase(Size.Large, "cwn-icon--large")]
	[TestCase(Size.XLarge, "cwn-icon--xlarge")]
	[TestCase(Size.XXLarge, "cwn-icon--2xlarge")]
	[TestCase(Size.XXXLarge, "cwn-icon--3xlarge")]
	public void CwnIcon_RenderWithSize(Size size, string expectedClass)
	{
		// Arrange
		var icon = Icon.Search;

		// Act
		var cut = RenderComponent<CwnIcon>(parameters => parameters
			.Add(p => p.Icon, icon)
			.Add(p => p.Size, size)
		);

		// Assert
		cut.MarkupMatches(@$"
			<span
				class=""
					cwn-icon
					cwn-icon--search
					{expectedClass}
				""
				aria-hidden=""true""
				role=""img""
			/>");
	}
}
