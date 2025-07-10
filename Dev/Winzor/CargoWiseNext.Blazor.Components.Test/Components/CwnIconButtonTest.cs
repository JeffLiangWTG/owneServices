using Bunit;
using CargoWiseNext.Blazor.Components.Test.TestComponents.IconButton;

namespace CargoWiseNext.Blazor.Components.Test;

public class CwnIconButtonTest : BunitTestContext
{
	[Test]
	public void CwnIconButton_RenderTest()
	{
		var comp = RenderComponent<IconButtonTest>();
		var markupButton = @"
			<button
				accesskey=""f""
				blazor:onclick=""1""
				class=""
					cwn-button
					cwn-icon-button
					cwn-icon-button--medium
					cwn-icon-button--text""
				blazor:elementReference="""">
				<span diff:ignore />
			</button>
		";

		var iconButton = comp.FindComponent<CwnIconButton>();
		iconButton.MarkupMatches(markupButton);

		var markupIcon = @"
			<span
				class=""
					cwn-icon
					cwn-icon--star-empty
					cwn-icon--star-filled__hover
					cwn-icon--fill-parent""
				aria-hidden=""true""
				role=""img"" />
		";

		var icon = comp.FindComponent<CwnIcon>();
		icon.MarkupMatches(markupIcon);
	}

	[Test]
	public void CwnIconButton_RenderDisabledTest()
	{
		var comp = RenderComponent<DisabledIconButtonTest>();
		var markupButton = @"
			<button
				accesskey=""h""
				class=""
					cwn-button
					cwn-icon-button
					cwn-icon-button--large
					cwn-icon-button--text""
				disabled="""">
				<span diff:ignore />
			</button>";

		var iconButton = comp.FindComponent<CwnIconButton>();
		iconButton.MarkupMatches(markupButton);

		var markupIcon = @"
			<span
				class=""
					cwn-icon
					cwn-icon--history
					cwn-icon--fill-parent""
				aria-hidden=""true""
				role=""img""
				disabled=""""/>
		";

		var icon = comp.FindComponent<CwnIcon>();
		icon.MarkupMatches(markupIcon);
	}
}
