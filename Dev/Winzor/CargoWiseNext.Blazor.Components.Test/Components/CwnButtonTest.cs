using Bunit;
using CargoWiseNext.Blazor.Components.Test.TestComponents.ButtonGroup;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CargoWiseNext.Blazor.Components.Test;

public class CwnButtonTest : BunitTestContext
{
	[Test]
	public void CwnButton_RenderWithoutIconsTest()
	{
		// Act
		var cut = RenderComponent<CwnButton>(p => p.AddChildContent("Click me"));

		// Assert
		cut.MarkupMatches(@"
			<button
				class=""
					cwn-button
					cwn-button--text
				"">
				<span class=""cwn-button__content"">
					<span class=""cwn-button__caption"">Click me</span>
				</span>
			</button>");
	}

	[TestCase(Icon.History, "cwn-icon--history")]
	[TestCase(Icon.Search, "cwn-icon--search")]
	[TestCase(Icon.Help, "cwn-icon--help")]
	public void CwnButton_RenderWithStartIconTest(Icon icon, string expectedClass)
	{
		// Act
		var cut = RenderComponent<CwnButton>(parameters => parameters
			.Add(p => p.StartIcon, icon)
			.AddChildContent("Click me")
		);

		var elements = cut.FindAll($".{expectedClass}");

		// Assert
		Assert.That(elements, Has.Count.EqualTo(1));
	}

	[TestCase(Icon.History, "cwn-icon--history")]
	[TestCase(Icon.Search, "cwn-icon--search")]
	[TestCase(Icon.Help, "cwn-icon--help")]
	public void CwnButton_RenderWithEndIconTest(Icon icon, string expectedClass)
	{
		// Act
		var cut = RenderComponent<CwnButton>(parameters => parameters
			.Add(p => p.EndIcon, icon)
			.AddChildContent("Click me")
		);

		var elements = cut.FindAll($".{expectedClass}");

		// Assert
		Assert.That(elements, Has.Count.EqualTo(1));
	}

	[TestCase(Icon.History, "cwn-icon--history")]
	[TestCase(Icon.Search, "cwn-icon--search")]
	[TestCase(Icon.Help, "cwn-icon--help")]
	public void CwnButton_RenderWithStartAndEndIconsTest(Icon icon, string expectedClass)
	{
		// Act
		var cut = RenderComponent<CwnButton>(parameters => parameters
			.Add(p => p.StartIcon, icon)
			.Add(p => p.EndIcon, icon)
			.AddChildContent("Click me")
		);

		var elements = cut.FindAll($".{expectedClass}");

		// Assert
		Assert.That(elements, Has.Count.EqualTo(2));
	}

	[TestCase(Size.Small, "cwn-icon--small")]
	[TestCase(Size.Medium, "cwn-icon--medium")]
	[TestCase(Size.Large, "cwn-icon--large")]
	public void CwnButton_RenderWithIconSizeTest(Size size, string expectedClass)
	{
		// Act
		var cut = RenderComponent<CwnButton>(parameters => parameters
			.Add(p => p.StartIcon, Icon.ArrowRight)
			.Add(p => p.EndIcon, Icon.Drag)
			.Add(p => p.IconSize, size)
			.AddChildContent("Click me")
		);

		var elements = cut.FindAll($".{expectedClass}");

		// Assert
		Assert.That(elements, Has.Count.EqualTo(2));
	}

	[TestCase(Variant.Filled, "cwn-button--filled")]
	[TestCase(Variant.Outlined, "cwn-button--outlined")]
	[TestCase(Variant.Text, "cwn-button--text")]
	public void CwnButton_RenderWithVariantTest(Variant variant, string expectedClass)
	{
		// Act
		var cut = RenderComponent<CwnButton>(parameters => parameters
			.Add(p => p.Variant, variant)
			.AddChildContent("Click me")
		);

		// Assert
		cut.MarkupMatches(@$"
			<button
				class=""
					cwn-button
					{expectedClass}
				"">
				<span class=""cwn-button__content"">
					<span class=""cwn-button__caption"">Click me</span>
				</span>
			</button>");
	}

	[Test]
	public void CwnButton_DefaultVariantTest()
	{
		// Act
		var cut = RenderComponent<CwnButton>();

		// Assert
		Assert.That(cut.Instance.Variant, Is.EqualTo(Variant.Text));
	}

	[Test]
	public void CwnButton_VariantClassTest()
	{
		// Act
		var cut = RenderComponent<CwnButton>(parameters => parameters
			.Add(p => p.Variant, Variant.Outlined)
		);

		// Assert
		Assert.That(cut.Markup, Does.Contain("cwn-button--outlined"));
	}

	[Test]
	public void CwnButton_DisableElevationClassTest()
	{
		// Act
		var cut = RenderComponent<CwnButton>(parameters => parameters
			.Add(p => p.DropShadow, false)
		);

		// Assert
		Assert.That(cut.Markup, Does.Contain("cwn-button--disable-elevation"));
	}

	[Test]
	public void CwnButton_HandleEventTest()
	{
		// Arrange
		var wasClicked = false;
		var onClickCallback = EventCallback.Factory.Create(this,
			(MouseEventArgs e) => { wasClicked = true; });
		var wasButtonClicked = false;
		var onButtonClickedHandler = new EventHandler<WebMouseEventArgs>((sender, args) => { wasButtonClicked = true; });

		// Act
		var cut = RenderComponent<CwnButton>(parameters => parameters
			.Add(p => p.OnClick, onClickCallback)
			.Add(p => p.OnButtonClicked, onButtonClickedHandler)
			.AddChildContent("Click me")
		);

		cut.Find("button").Click();

		// Assert
		Assert.That(wasClicked, Is.True);
		Assert.That(wasButtonClicked, Is.True);
	}

	[Test]
	public void CwnButton_ButtonGroupShouldBeGroupByCascadingParameter()
	{
		var comp = RenderComponent<ButtonGroupWithThreeButtons>(
			parameters => parameters
				.Add(c => c.ButtonGroupFullWidth, true)
				.Add(c => c.Button1FullWidth, true)
				.Add(c => c.Button2FullWidth, false)
				.Add(c => c.Button3FullWidth, false)
		);

		var group = comp.FindComponent<CwnButtonGroup>();
		var buttons = comp.FindComponents<CwnButton>();

		foreach (var button in buttons)
		{
			Assert.That(button.Instance.ButtonGroup, Is.EqualTo(group.Instance));
		}
	}
}

