using Bunit;
using CargoWiseNext.Blazor.Components.Test.TestComponents.List;
using Microsoft.AspNetCore.Components.Web;

namespace CargoWiseNext.Blazor.Components.Test;

public class CwnListTest : BunitTestContext
{
	[Test]
	public void CwnList_PrimaryTextRenderTest()
	{
		var comp = RenderComponent<ListComponentTest>();
		var list = comp.FindComponent<CwnList>();
		Assert.That(list.Markup, Does.StartWith("<ul class=\"cwn-list\">"));
		Assert.That(list.Instance.ChildContent, Is.Not.Null);

		var listItems = comp.FindComponents<CwnListItem>();
		Assert.That(listItems, Has.Count.EqualTo(5));
		listItems[0].MarkupMatches("<li class=\"cwn-list-item\">Container Yard</li>");
		listItems[1].MarkupMatches("<li class=\"cwn-list-item\">Handling Unit</li>");
		listItems[2].MarkupMatches("<li class=\"cwn-list-item\">Receive ASN</li>");
		listItems[3].MarkupMatches("<li class=\"cwn-list-item\">Transit Warehouse Management Portal</li>");
		listItems[4].MarkupMatches("<li class=\"cwn-list-item\">Dispatch Load List</li>");
	}

	[Test]
	public async Task CwnList_ItemClickTestAsync()
	{
		var comp = RenderComponent<ListComponentTest>();
		comp.FindAll(".cwn-list-item")[0].MarkupMatches("<li class=\"cwn-list-item\">Container Yard</li>");
		comp.FindAll(".cwn-list-item")[1].MarkupMatches("<li class=\"cwn-list-item\">Handling Unit</li>");
		comp.FindAll(".cwn-list-item")[2].MarkupMatches("<li class=\"cwn-list-item\">Receive ASN</li>");
		comp.FindAll(".cwn-list-item")[3].MarkupMatches("<li class=\"cwn-list-item\">Transit Warehouse Management Portal</li>");
		comp.FindAll(".cwn-list-item")[4].MarkupMatches("<li class=\"cwn-list-item\">Dispatch Load List</li>");

		await comp.FindAll(".cwn-list-item")[0].ClickAsync(new MouseEventArgs() { Detail = 1 });
		comp.FindAll(".cwn-list-item")[0].MarkupMatches("<li class=\"cwn-list-item cwn-list-item--selected\">Container Yard</li>");
		comp.FindAll(".cwn-list-item")[1].MarkupMatches("<li class=\"cwn-list-item\">Handling Unit</li>");
		comp.FindAll(".cwn-list-item")[2].MarkupMatches("<li class=\"cwn-list-item\">Receive ASN</li>");
		comp.FindAll(".cwn-list-item")[3].MarkupMatches("<li class=\"cwn-list-item\">Transit Warehouse Management Portal</li>");
		comp.FindAll(".cwn-list-item")[4].MarkupMatches("<li class=\"cwn-list-item\">Dispatch Load List</li>");

		await comp.FindAll(".cwn-list-item")[1].ClickAsync(new MouseEventArgs() { Detail = 1 });
		comp.FindAll(".cwn-list-item")[0].MarkupMatches("<li class=\"cwn-list-item\">Container Yard</li>");
		comp.FindAll(".cwn-list-item")[1].MarkupMatches("<li class=\"cwn-list-item cwn-list-item--selected\">Handling Unit</li>");
		comp.FindAll(".cwn-list-item")[2].MarkupMatches("<li class=\"cwn-list-item\">Receive ASN</li>");
		comp.FindAll(".cwn-list-item")[3].MarkupMatches("<li class=\"cwn-list-item\">Transit Warehouse Management Portal</li>");
		comp.FindAll(".cwn-list-item")[4].MarkupMatches("<li class=\"cwn-list-item\">Dispatch Load List</li>");
	}
}
