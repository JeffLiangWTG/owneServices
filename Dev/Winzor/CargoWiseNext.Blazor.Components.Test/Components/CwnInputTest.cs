using Bunit;
using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components.Test;

public class CwnInputTest : BunitTestContext
{
	[Test]
	public void CwnInput_RenderTest()
	{
		var cut = RenderComponent<CwnInput>();

		cut.MarkupMatches(@"
			<div class=""cwn-input"">
				<input class=""cwn-input__input"" type=""text"" maxlength:ignore value:ignore>
			</div>
		");
	}

	[Test]
	public void CwnInput_WhenStartIcon()
	{
		var cut = RenderComponent<CwnInput>(parameter => parameter
			.Add(c => c.StartIcon, Icon.Search));

		cut.MarkupMatches(@"
			<div class=""cwn-input"">
				<span class:ignore aria-hidden:ignore role=""img""></span>
				<input diff:ignoreAttributes>
			</div>
		");
	}

	[Test]
	public void CwnInput_WhenEndIcon()
	{
		var cut = RenderComponent<CwnInput>(parameter => parameter
			.Add(c => c.EndIcon, Icon.Search));

		cut.MarkupMatches(@"
			<div class=""cwn-input"">
				<input diff:ignoreAttributes>
				<span class:ignore aria-hidden:ignore role=""img""></span>
			</div>
		");

		var classAttr = cut.Find("span").GetAttribute("class");

		Assert.That(classAttr, Contains.Substring("cwn-icon--small"));
		Assert.That(classAttr, Contains.Substring("cwn-neutral"));
	}

	[Test]
	public void CwnInput_WhenIconSize()
	{
		var cut = RenderComponent<CwnInput>(parameter => parameter
			.Add(c => c.EndIcon, Icon.Search)
			.Add(c => c.IconSize, Size.XXLarge));

		var classAttr = cut.Find("span").GetAttribute("class");

		Assert.That(classAttr, Contains.Substring("cwn-icon--2xlarge"));
	}

	[Test]
	public void CwnInput_WhenIconColor()
	{
		var cut = RenderComponent<CwnInput>(parameter => parameter
			.Add(c => c.EndIcon, Icon.Search)
			.Add(c => c.IconColor, Color.Error));

		var classAttr = cut.Find("span").GetAttribute("class");

		Assert.That(classAttr, Contains.Substring("cwn-error"));
	}

	[Test]
	public void CwnInput_WhenPlaceholder()
	{
		var cut = RenderComponent<CwnInput>(parameter => parameter
			.Add(c => c.Placeholder, "Place holder..."));

		var uut = cut.Find("input");

		Assert.That(uut.GetAttribute("placeholder"), Is.EqualTo("Place holder..."));
	}

	[Test]
	public void CwnInput_WhenInputType()
	{
		var cut = RenderComponent<CwnInput>(parameter => parameter
			.Add(c => c.InputType, InputType.Url));

		var uut = cut.Find("input");

		Assert.That(uut.GetAttribute("type"), Is.EqualTo("url"));
	}

	[Test]
	public void CwnInput_WhenMaxLength()
	{
		var cut = RenderComponent<CwnInput>(parameter => parameter
			.Add(c => c.MaxLength, 123));

		var uut = cut.Find("input");

		Assert.That(uut.GetAttribute("maxlength"), Is.EqualTo("123"));
	}

	[Test]
	public void CwnInput_WhenValue()
	{
		var value = "Initial value";

		var cut = RenderComponent<CwnInput>(parameters => parameters
			.Bind(p => p.Value, value, newValue => value = newValue)
		);

		var input = cut.Find("input");

		Assert.That(input.GetAttribute("value"), Is.EqualTo("Initial value"));

		input.Input("Hello World");

		Assert.That(value, Is.EqualTo("Hello World"));
	}

	[Test]
	public void CwnInput_OnFocus()
	{
		var focused = false;
		var cut = RenderComponent<CwnInput>(parameters => parameters
			.Add(p => p.OnFocus, EventCallback.Factory.Create(this, () => focused = true))
		);

		var input = cut.Find("input");

		Assert.Multiple(() =>
		{
			Assert.That(cut.Instance.IsFocused, Is.False);
			Assert.That(focused, Is.False);
		});

		input.Focus();

		Assert.Multiple(() =>
		{
			Assert.That(cut.Instance.IsFocused, Is.True);
			Assert.That(focused, Is.True);
		});
	}

	[Test]
	public void CwnInput_OnBlur()
	{
		var blurred = false;
		var cut = RenderComponent<CwnInput>(parameters => parameters
			.Add(p => p.OnBlur, EventCallback.Factory.Create(this, () => blurred = true))
		);

		var input = cut.Find("input");

		input.Blur();

		Assert.That(blurred, Is.True);
	}
}
