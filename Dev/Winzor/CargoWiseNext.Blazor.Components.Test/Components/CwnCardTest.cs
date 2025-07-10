using Bunit;
using CargoWiseNext.Blazor.Components.Test.TestComponents.Card;

namespace CargoWiseNext.Blazor.Components.Test;

public class CwnCardTest : BunitTestContext
{
	[Test]
	public void CardChildContent()
	{
		var comp = RenderComponent<CardChildContentTest>();
		var card = comp.Find(".cwn-card");
		Assert.That(card.GetAttribute("class"), Is.EqualTo("cwn-paper cwn-card"));
		Assert.That(card.GetAttribute("style"), Is.EqualTo("height:200px;width:200px;margin-left:10px"));

		var header = comp.FindComponent<CwnCardHeader>();
		Assert.That(header.FindAll(".cwn-card__header"), Has.Count.EqualTo(1));
		Assert.That(header.FindAll(".cwn-card__header__content"), Has.Count.EqualTo(1));
		Assert.That(header.FindAll(".cwn-card__header__actions"), Has.Count.EqualTo(1));

		var content = comp.FindComponent<CwnCardContent>();
		var contentElement = content.Find(".cwn-card__content");
		Assert.That(contentElement.GetAttribute("class"), Does.Contain("cwn-scrollbar"));
		Assert.That(contentElement.TextContent, Does.Contain("This photo was taken in a small village in Istra Croatia."));

		var actions = comp.FindComponent<CwnCardActions>();
		Assert.That(actions.FindAll(".cwn-card__actions"), Has.Count.EqualTo(1));

		var buttons = actions.FindAll("button");
		Assert.That(buttons, Has.Count.EqualTo(2));
		Assert.That(buttons[0].TextContent, Is.EqualTo("ok"));
		Assert.That(buttons[1].TextContent, Is.EqualTo("cancel"));
	}

	[Test]
	public void CwnCardHeader_Render()
	{
		var cut = RenderComponent<CwnCardHeader>();
		cut.MarkupMatches("<div class=\"cwn-card__header\" />");
	}

	[Test]
	public void CwnCardHeader_WhenClass()
	{
		var cut = RenderComponent<CwnCardHeader>(parameter => parameter.Add(p => p.Class, "custom-class"));

		var uut = cut.Find(".cwn-card__header");
		Assert.That(uut.GetAttribute("class"), Does.Contain("custom-class"));
	}

	[Test]
	public void CwnCardHeader_WhenHeaderContent()
	{
		var cut = RenderComponent<CwnCardHeader>(parameter => parameter
			.Add(p => p.ClassHeaderContent, "custom-class")
			.Add(p => p.CardHeaderContent, "Header Content")
		);

		var uut = cut.Find(".cwn-card__header__content");
		Assert.That(uut.GetAttribute("class"), Does.Contain("custom-class"));
		Assert.That(uut.TextContent, Is.EqualTo("Header Content"));
	}

	[Test]
	public void CwnCardHeader_WhenHeaderActions()
	{
		var cut = RenderComponent<CwnCardHeader>(parameter => parameter
			.Add(p => p.ClassHeaderActions, "custom-class")
			.Add(p => p.CardHeaderActions, "Header Actions")
		);

		var uut = cut.Find(".cwn-card__header__actions");
		Assert.That(uut.GetAttribute("class"), Does.Contain("custom-class"));
		Assert.That(uut.TextContent, Is.EqualTo("Header Actions"));
	}
}

