using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class ZListBoxTest
{
	[Test]
	public async Task ZListBoxRenderredNotDraggable()
	{
		using var ctx = new EnterpriseTestContext();
		ZListBox newListbox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => {
			newListbox = new ZListBox() { SelectionMode = SelectionMode.MultiExtended };
			InitListBox(newListbox);
			return newListbox;
		});
		Assert.That(rendered.FindAll(".listbox").Count, Is.EqualTo(1));
		Assert.That(rendered.FindAll(".listbox__item:not(:last-child)").Select(tr => tr.InnerHtml), Is.EqualTo(new string[] { "item1", "item2", "item3", "item4", "item5" }));
		Assert.That(rendered.FindAll(".listbox__item[draggable='']").Count, Is.EqualTo(5));
		Assert.That(rendered.FindAll(".listbox__item[ondragover='']").Count, Is.EqualTo(6));
		Assert.That(rendered.FindAll(".listbox__item[ondragleave='']").Count, Is.EqualTo(6));
	}

	[Test]
	public async Task ZListBoxRenderDraggable()
	{
		using var ctx = new EnterpriseTestContext();
		ZListBox newListbox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => {
			newListbox = new ZListBox() { SelectionMode = SelectionMode.MultiExtended };
			InitListBox(newListbox);
			newListbox.EnableDragAndDrop(true);
			return newListbox;
		});

		Assert.That(rendered.FindAll(".listbox").Count, Is.EqualTo(1));
		Assert.That(rendered.FindAll(".listbox__item:not(:last-child)").Select(tr => tr.InnerHtml), Is.EqualTo(new string[] { "item1", "item2", "item3", "item4", "item5" }));
		Assert.That(rendered.FindAll(".listbox__item[draggable='true']").Count, Is.EqualTo(5));
		Assert.That(rendered.FindAll(".listbox__item:last-child:not([draggable])").Count, Is.EqualTo(1));
		Assert.That(rendered.FindAll(".listbox__item:not([ondragover=''])").Count, Is.EqualTo(6));
		Assert.That(rendered.FindAll(".listbox__item:not([ondragleave=''])").Count, Is.EqualTo(6));
	}

	[Test]
	public async Task ZListBoxDragOneItemUp()
	{
		using var ctx = new EnterpriseTestContext();
		ZListBox newListbox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => {
			newListbox = new ZListBox() { SelectionMode = SelectionMode.MultiExtended };
			InitListBox(newListbox);
			newListbox.EnableDragAndDrop(true);
			return newListbox;
		});

		await rendered.Find(".listbox__item:nth-child(3)").TriggerEventAsync("onmousedown", new WebMouseEventArgs());
		await rendered.Find(".listbox__item:nth-child(1)").TriggerEventAsync("ondragStart", new WebDragEventArgs());
		await rendered.Find(".listbox__item:nth-child(1)").TriggerEventAsync("ondragenter", new WebDragEventArgs());
		await rendered.Find(".listbox__item:nth-child(1)").TriggerEventAsync("ondrop", new WebDragEventArgs());

		AssertOrder(rendered, new int[] { 3, 1, 2, 4, 5 }, new int[] { 1 });
	}

	[Test]
	public async Task ZListBoxDragOneItemDown()
	{
		using var ctx = new EnterpriseTestContext();
		ZListBox newListbox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => {
			newListbox = new ZListBox() { SelectionMode = SelectionMode.MultiExtended };
			InitListBox(newListbox);
			newListbox.EnableDragAndDrop(true);

			return newListbox;
		});

		await rendered.Find(".listbox__item:nth-child(1)").TriggerEventAsync("onmousedown", new WebMouseEventArgs());
		await rendered.Find(".listbox__item:nth-child(1)").TriggerEventAsync("ondragStart", new WebDragEventArgs());
		await rendered.Find(".listbox__item:nth-child(4)").TriggerEventAsync("ondragenter", new WebDragEventArgs());
		await rendered.Find(".listbox__item:nth-child(4)").TriggerEventAsync("ondrop", new WebDragEventArgs());

		AssertOrder(rendered, new int[] { 2, 3, 1, 4, 5 }, new int[] { 3 });
	}

	[Test]
	public async Task ZListBoxDragMultipleItems()
	{
		using var ctx = new EnterpriseTestContext();
		ZListBox newListbox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => {
			newListbox = new ZListBox() { SelectionMode = SelectionMode.MultiExtended };
			InitListBox(newListbox);
			newListbox.EnableDragAndDrop(true);
			return newListbox;
		});

		await rendered.Find(".listbox__item:nth-child(1)").TriggerEventAsync("onmousedown", new WebMouseEventArgs() { Buttons = 1, CtrlKey = true });
		await rendered.Find(".listbox__item:nth-child(2)").TriggerEventAsync("onmousedown", new WebMouseEventArgs() { Buttons = 1, CtrlKey = true });
		await rendered.Find(".listbox__item:nth-child(1)").TriggerEventAsync("ondragStart", new WebDragEventArgs() { Buttons = 1 });
		await rendered.Find(".listbox__item:nth-child(4)").TriggerEventAsync("ondragenter", new WebDragEventArgs() { Buttons = 1 });
		await rendered.Find(".listbox__item:nth-child(4)").TriggerEventAsync("ondrop", new WebDragEventArgs() { Buttons = 1, CtrlKey = true, ShiftKey = true });

		AssertOrder(rendered, new int[] { 3, 1, 2, 4, 5 }, new int[] { 2, 3 });
	}

	[Test]
	public async Task ZListBoxDragItemsToLast()
	{
		using var ctx = new EnterpriseTestContext();
		ZListBox newListbox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => {
			newListbox = new ZListBox() { SelectionMode = SelectionMode.MultiExtended };
			InitListBox(newListbox);
			newListbox.EnableDragAndDrop(true);

			return newListbox;
		});

		await rendered.Find(".listbox__item:nth-child(1)").TriggerEventAsync("onmousedown", new WebMouseEventArgs() { CtrlKey = true });
		await rendered.Find(".listbox__item:nth-child(2)").TriggerEventAsync("onmousedown", new WebMouseEventArgs() { CtrlKey = true });
		await rendered.Find(".listbox__item:nth-child(1)").TriggerEventAsync("ondragStart", new WebDragEventArgs() { CtrlKey = true });
		await rendered.Find(".listbox__item:last-child").TriggerEventAsync("ondragenter", new WebDragEventArgs() { CtrlKey = true });
		await rendered.Find(".listbox__item:last-child").TriggerEventAsync("ondrop", new WebDragEventArgs() { CtrlKey = true });

		AssertOrder(rendered, new int[] { 3, 4, 5, 1, 2 }, new int[] { 4, 5 });
	}

	void AssertOrder(IRenderedFragment rendered, int[] arr, int[] selectedRow = null)
	{
		var items = rendered.FindAll(".listbox__item");
		for (var i = 0; i < arr.Length; i++)
		{
			Assert.That(items[i].TextContent, Is.EqualTo($"item{arr[i]}"));
		}

		if (selectedRow != null)
		{
			foreach (var i in selectedRow)
			{
				var item = rendered.Find($".listbox__item:nth-child({i})");
				Assert.That(item.ClassList, Does.Contain("listbox__item--selected"));
			}
		}
	}

	void InitListBox(ZListBox listbox)
	{
		var list = new string[] { "item1", "item2", "item3", "item4", "item5" };
		Array.ForEach(list, (item) => listbox.Items.Add(item));
	}
}
