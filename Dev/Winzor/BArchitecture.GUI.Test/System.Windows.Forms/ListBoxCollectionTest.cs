using System.Collections;
using System.Threading.Tasks;
using Bunit;
using NUnit.Framework;
using WinzorTestFramework;
using static System.Windows.Forms.ListBox;

namespace System.Windows.Forms;

public class ListBoxCollectionTest
{
	[Test]
	public async Task SelectedObjectCollectionAddRemove()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listBox) = await TestListBoxAsync(ctx);
		var collection = new SelectedObjectCollection(listBox);

		await listBox.InvokeWinzorDispatcherAsync(() => collection.Add("item1"));
		AssertItem(rendered, listBox, collection, 1, 0, "item1", 0);

		await listBox.InvokeWinzorDispatcherAsync(() => collection.Add("item2"));
		AssertItem(rendered, listBox, collection, 2, 1, "item2", 1);

		await listBox.InvokeWinzorDispatcherAsync(() => collection.Remove("item1"));
		AssertItem(rendered, listBox, collection, 1, 0, "item2", 1);

		await listBox.InvokeWinzorDispatcherAsync(() => collection.Remove("item2"));

		Assert.That(collection.Count, Is.EqualTo(0));
		Assert.That(listBox.SelectedItems.Count, Is.EqualTo(0));
		Assert.That(listBox.SelectedIndices.Count, Is.EqualTo(0));
	}

	[Test]
	public async Task SelectedIndexCollectionAddRemove()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listBox) = await TestListBoxAsync(ctx);
		var collection = new SelectedIndexCollection(listBox);

		await listBox.InvokeWinzorDispatcherAsync(() => collection.Add(0));
		AssertItem(rendered, listBox, collection, 1, 0, "item1", 0);

		await listBox.InvokeWinzorDispatcherAsync(() => collection.Add(1));
		AssertItem(rendered, listBox, collection, 2, 1, "item2", 1);

		await listBox.InvokeWinzorDispatcherAsync(() => collection.Remove(0));
		AssertItem(rendered, listBox, collection, 1, 0, "item2", 1);

		await listBox.InvokeWinzorDispatcherAsync(() => collection.Remove(1));

		Assert.That(collection.Count, Is.EqualTo(0));
		Assert.That(listBox.SelectedItems.Count, Is.EqualTo(0));
		Assert.That(listBox.SelectedIndices.Count, Is.EqualTo(0));
	}

	[Test]
	public async Task SelectedObjectCollectionAddDuplicate()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listBox) = await TestListBoxAsync(ctx);
		var collection = new SelectedObjectCollection(listBox);

		await listBox.InvokeWinzorDispatcherAsync(() => collection.Add("item1"));
		AssertItem(rendered, listBox, collection, 1, 0, "item1", 0);

		await listBox.InvokeWinzorDispatcherAsync(() => collection.Add("item1"));
		AssertItem(rendered, listBox, collection, 1, 0, "item1", 0);
	}

	[Test]
	public async Task SelectedIndexCollectionAddDuplicate()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, listBox) = await TestListBoxAsync(ctx);
		var collection = new SelectedIndexCollection(listBox);

		await listBox.InvokeWinzorDispatcherAsync(() => collection.Add(0));
		AssertItem(rendered, listBox, collection, 1, 0, "item1", 0);

		await listBox.InvokeWinzorDispatcherAsync(() => collection.Add(0));
		AssertItem(rendered, listBox, collection, 1, 0, "item1", 0);
	}

	void AssertItem(IRenderedFragment rendered, ListBox listBox, ListBoxSelectedCollection collection, int expectedCount, int selectedIndex, string expectedItem, int expectedIndex)
	{
		rendered.Find($".listbox__item:nth-child({expectedIndex + 1}).listbox__item--selected");
		Assert.That(collection.Count, Is.EqualTo(expectedCount));
		Assert.That(listBox.SelectedItems.Count, Is.EqualTo(expectedCount));
		Assert.That(listBox.SelectedIndices.Count, Is.EqualTo(expectedCount));
		if (collection is SelectedObjectCollection)
		{
			Assert.That(((IList)collection)[selectedIndex], Is.EqualTo(expectedItem));
		}
		else
		{
			Assert.That(((IList)collection)[selectedIndex], Is.EqualTo(expectedIndex));
		}
		Assert.That(listBox.SelectedItems[selectedIndex], Is.EqualTo(expectedItem));
		Assert.That(listBox.SelectedIndices[selectedIndex], Is.EqualTo(expectedIndex));
	}

	async Task<(IRenderedFragment, ListBox)> TestListBoxAsync(WinzorTestContext ctx)
	{
		ListBox newListbox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => {
			newListbox = new ListBox() { SelectionMode = SelectionMode.MultiSimple };
			var list = new string[] { "item1", "item2", "item3" };
			foreach (var item in list)
			{
				newListbox.Items.Add(item);
			}

			return newListbox;
		});

		return (rendered, newListbox);
	}
}
