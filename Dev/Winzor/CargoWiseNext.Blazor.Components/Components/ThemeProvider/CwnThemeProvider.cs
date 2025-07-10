using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CargoWiseNext.Blazor.Components;

[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard coded property names")]
public class CwnThemeProvider : ComponentBase
{
	[Parameter]
	public Theme? Theme { get; set; }

	protected override void BuildRenderTree(RenderTreeBuilder builder)
	{
		if (Theme is null)
		{
			return;
		}

		base.BuildRenderTree(builder);

		// Open element.
		builder.OpenElement(0, "style");

		// Add child content.
		builder.AddMarkupContent(1, BuildTheme());

		// Close element.
		builder.CloseElement();
	}

	string BuildTheme()
	{
		if (Theme == null)
		{
			return string.Empty;
		}

		var palette = Theme.Palette;

		var themeContent = new StyleBuilder()
			.AddStyle("--s-brand-bg-default", palette.AppBarBackgroundColor?.ToRgba(), palette.AppBarBackgroundColor.HasValue)
			.AddStyle("--s-brand-txt-inv-hover", palette.AppBarBackgroundColor?.ToRgba(), palette.AppBarBackgroundColor.HasValue)
			.AddStyle("--s-brand-txt-inv-active", palette.AppBarBackgroundColor?.ToRgba(), palette.AppBarBackgroundColor.HasValue)
			.AddStyle("--s-brand-txt-inv-default", palette.AppBarTextColor?.ToRgba(), palette.AppBarTextColor.HasValue)
			.AddStyle("--s-neutral-bg-active", palette.NavBarGroupSelected?.ToRgba(), palette.NavBarGroupSelected.HasValue)
			.AddStyle("--s-theme-bg-default", palette.BackgroundColor?.ToRgba(), palette.BackgroundColor.HasValue)
			.AddStyle("--s-neutral-txt-inv-active", palette.NavBarTextColor?.ToRgba(), palette.NavBarTextColor.HasValue)
			.AddStyle("--s-theme-recent-fav-bg-default", palette.RecentFavBackgroundColor?.ToRgba(), palette.RecentFavBackgroundColor.HasValue)
			.Build();

		if (string.IsNullOrEmpty(themeContent))
		{
			return string.Empty;
		}

		return $".cwn-theme--custom {{ {themeContent} }}";
	}
}
