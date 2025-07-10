using System.Threading.Tasks;
using Bunit;
using Enterprise.ZArchitecture.GUI.SearchBox;
using NUnit.Framework;
using WinzorTestFramework;

namespace Enterprise.Winzor.Architecture.Test;

[TestFixture]
public class ZSearchListBoxTest
{
	[Test]
	public async Task ErrorItemsAreRenderedCorrectly()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var searchListBox = new ZSearchListBox();
			searchListBox.Items.AddRange(new[] { DisplayItemFactory.CreateErrorItem("Hello", "World") });
			return searchListBox;
		});

		rendered.Find("div > *").MarkupMatches(@"<ul class=""zsearchbox zsearchbox--striped"">
        <li class=""zsearchbox-item zsearchbox-item--non-interactive"" style=""color:#FF0000FF; font-weight:bold;"">
          <span>Hello</span>
          <br>
          <span>World</span>
          <br>
        </li>
      </ul>");
	}

	[Test]
	public async Task HeaderItemsAreRenderedCorrectly()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var searchListBox = new ZSearchListBox();
			searchListBox.Items.AddRange(new[] { DisplayItemFactory.CreateHeadingItem("Hello", "World") });
			return searchListBox;
		});

		rendered.Find("div > *").MarkupMatches(@"<ul class=""zsearchbox zsearchbox--striped"">
        <li class=""zsearchbox-item zsearchbox-item--non-interactive"" style=""color:#000000FF; font-weight:bold;background-color: #CCCCCCFF;"">
          <span>Hello</span>
          <br>
          <span>World</span>
          <br>
        </li>
      </ul>");
	}

	[Test]
	public async Task SearchItemsAreRenderedCorrectly()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var searchListBox = new ZSearchListBox();
			searchListBox.Items.AddRange(new[] { DisplayItemFactory.CreateSearchItem(() => { }, "Hello", "World") });
			return searchListBox;
		});

		rendered.Find("div > *").MarkupMatches(@"<ul class=""zsearchbox zsearchbox--striped"">
        <li class=""zsearchbox-item zsearchbox-item--selected"" style=""color:#000000FF; "">
          <a  >
            <span class=""text-decoration-underline"">Hello</span>
            <br>
            <span class=""text-decoration-underline"">World</span>
            <br>
          </a>
        </li>
      </ul>");
	}

	[Test]
	public async Task MixedItemListIsRenderedCorrectly()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var searchListBox = new ZSearchListBox();
			searchListBox.Items.AddRange(new[] {
				DisplayItemFactory.CreateHeadingItem("1"),
				DisplayItemFactory.CreateSearchItem(() => { }, "2"),
				DisplayItemFactory.CreateSearchItem(() => { }, "3"),
			});
			return searchListBox;
		});

		rendered.Find("div > *").MarkupMatches($@"<ul class=""zsearchbox zsearchbox--striped"">
        <li class=""zsearchbox-item zsearchbox-item--non-interactive"" style=""color:#000000FF; font-weight:bold;background-color: #CCCCCCFF;"">
          <span>1</span>
          <br>
        </li>
        <li class=""zsearchbox-item zsearchbox-item--selected"" style=""color:#000000FF; "">
          <a  >
            <span class=""text-decoration-underline"">2</span>
            <br>
          </a>
        </li>
        <li class=""zsearchbox-item"" style=""color:#000000FF; "">
          <a >
            <span>3</span>
            <br>
          </a>
        </li>
      </ul>");
	}

	[Test]
	public async Task MouseEnterShouldNotBeImplementedOnResultsAsItIsUnreliableInHighLatencyEnvironments()
	{
		using var ctx = new EnterpriseTestContext();

		ZSearchListBox searchListBox = null;

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			searchListBox = new ZSearchListBox();
			searchListBox.Items.AddRange(new[] {
				DisplayItemFactory.CreateHeadingItem("1"),
				DisplayItemFactory.CreateSearchItem(() => { }, "2"),
				DisplayItemFactory.CreateSearchItem(() => { }, "3"),
			});
			return searchListBox;
		});

		var anchorTags = rendered.FindAll("a");
		var listItems = rendered.FindAll("li");

		Assert.Multiple(() =>
		{
			foreach (var tag in anchorTags)
			{
				Assert.Throws<MissingEventHandlerException>(() => tag.MouseEnter());
			}

			foreach (var tag in listItems)
			{
				Assert.Throws<MissingEventHandlerException>(() => tag.MouseEnter());
			}
		});
	}

	[Test]
	public async Task TheFirstItemInTheListShouldBeSelectedByDefault()
	{
		using var ctx = new EnterpriseTestContext();

		ZSearchListBox searchListBox = null;

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			searchListBox = new ZSearchListBox();
			searchListBox.Items.AddRange(new[] {
				DisplayItemFactory.CreateHeadingItem("1"),
				DisplayItemFactory.CreateSearchItem(() => { }, "2"),
				DisplayItemFactory.CreateSearchItem(() => { }, "3"),
			});
			return searchListBox;
		});

		Assert.That(searchListBox.SelectedIndex, Is.EqualTo(1), "The search box should set the selected index to the first selectable item when it's made visible");
	}

	[Test]
	public async Task ClickedItemExecutesAction()
	{
		using var ctx = new EnterpriseTestContext();
		var tsc1 = new TaskCompletionSource();
		var tsc2 = new TaskCompletionSource();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var searchListBox = new ZSearchListBox();
			searchListBox.Items.AddRange(new[] {
				DisplayItemFactory.CreateHeadingItem("1"),
				DisplayItemFactory.CreateSearchItem(() => { tsc1.SetResult(); }, "2"),
				DisplayItemFactory.CreateSearchItem(() => { tsc2.SetResult(); }, "3"),
			});
			return searchListBox;
		});

		var listItem = rendered.FindAll("a");
		listItem[0].Click();
		listItem[1].Click();
		Assert.That(() => tsc1.Task.IsCompletedSuccessfully, Is.True.After(1000, 10));
		Assert.That(() => tsc2.Task.IsCompletedSuccessfully, Is.True.After(1000, 10));
	}
}
