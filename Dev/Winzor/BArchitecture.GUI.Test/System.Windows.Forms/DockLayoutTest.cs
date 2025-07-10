using System.Threading.Tasks;
using NUnit.Framework;
using WinzorTestFramework;

namespace System.Windows.Forms;

public class DockAnchorLayoutTest
{
	[TestCase(DockStyle.None)]
	[TestCase(DockStyle.Top)]
	[TestCase(DockStyle.Bottom)]
	[TestCase(DockStyle.Left)]
	[TestCase(DockStyle.Right)]
	[TestCase(DockStyle.Fill)]
	public async Task TestGetDockCorrectDockReturned(DockStyle dock)
	{
		var parent = await GetParentWithChildren<Control>(docks: dock);
		var child = parent.Controls[0];

		Assert.That(child.Dock, Is.EqualTo(dock));
	}

	[Test]
	public async Task TestDockLeftPositionLeft()
	{
		var parent = await GetParentWithChildren<Control>(docks: DockStyle.Left);
		var child = parent.Controls[0];

		Assert.That(child.Bounds.X, Is.EqualTo(0));
		Assert.That(child.Bounds.Y, Is.EqualTo(0));
	}

	[TestCase(DockStyle.Left)]
	[TestCase(DockStyle.Right)]
	public async Task TestDockHorizontalFullHeightOriginalWidth(DockStyle dockStyle)
	{
		var parent = await GetParentWithChildren<Control>(docks: dockStyle);
		var child = parent.Controls[0];

		Assert.That(child.Width, Is.EqualTo(10));
		Assert.That(child.Height, Is.EqualTo(1000));
	}

	[TestCase(DockStyle.Left)]
	[TestCase(DockStyle.Right)]
	public async Task TestDockHorizontalButtonAutoSizeRespected(DockStyle dockStyle)
	{
		var parent = await GetParentWithChildren<Button>(text: "I am a long string", autoSize: true, dockStyle);
		var child = parent.Controls[0];

		Assert.That(child.Width, Is.EqualTo(100));
		Assert.That(child.Height, Is.EqualTo(1000));
	}

	[Test]
	public async Task TestDockLeftTwiceZOrderRespected()
	{
		var parent = await GetParentWithChildren<Control>(docks: new[] { DockStyle.Left, DockStyle.Left });
		var child1 = parent.Controls[0];
		var child2 = parent.Controls[1];

		Assert.That(child1.Bounds.X, Is.EqualTo(10));
		Assert.That(child1.Bounds.Y, Is.EqualTo(0));
		Assert.That(child2.Bounds.X, Is.EqualTo(0));
		Assert.That(child2.Bounds.Y, Is.EqualTo(0));
	}

	[Test]
	public async Task TestDockTopTwiceZOrderRespected()
	{
		var parent = await GetParentWithChildren<Control>(docks: new[] { DockStyle.Top, DockStyle.Top });
		var child1 = parent.Controls[0];
		var child2 = parent.Controls[1];

		Assert.That(child1.Bounds.X, Is.EqualTo(0));
		Assert.That(child1.Bounds.Y, Is.EqualTo(15));
		Assert.That(child2.Bounds.X, Is.EqualTo(0));
		Assert.That(child2.Bounds.Y, Is.EqualTo(0));
	}

	[Test]
	public async Task TestDockBottomTwiceZOrderRespected()
	{
		var parent = await GetParentWithChildren<Control>(docks: new[] { DockStyle.Bottom, DockStyle.Bottom });
		var child1 = parent.Controls[0];
		var child2 = parent.Controls[1];

		Assert.That(child1.Bounds.X, Is.EqualTo(0));
		Assert.That(child1.Bounds.Y, Is.EqualTo(970));
		Assert.That(child2.Bounds.X, Is.EqualTo(0));
		Assert.That(child2.Bounds.Y, Is.EqualTo(985));
	}

	[Test]
	public async Task TestDockRightPositionRight()
	{
		var parent = await GetParentWithChildren<Control>(docks: DockStyle.Right);
		var child = parent.Controls[0];

		Assert.That(child.Bounds.X, Is.EqualTo(990));
		Assert.That(child.Bounds.Y, Is.EqualTo(0));
	}

	[Test]
	public async Task TestDockRightTwiceZOrderRespected()
	{
		var parent = await GetParentWithChildren<Control>(docks: new[] { DockStyle.Right, DockStyle.Right });
		var child1 = parent.Controls[0];
		var child2 = parent.Controls[1];

		Assert.That(child1.Bounds.X, Is.EqualTo(980));
		Assert.That(child1.Bounds.Y, Is.EqualTo(0));
		Assert.That(child2.Bounds.X, Is.EqualTo(990));
		Assert.That(child2.Bounds.Y, Is.EqualTo(0));
	}

	[Test]
	public async Task TestDockTopPositionTop()
	{
		var parent = await GetParentWithChildren<Control>(docks: DockStyle.Top);
		var child = parent.Controls[0];

		Assert.That(child.Bounds.X, Is.EqualTo(0));
		Assert.That(child.Bounds.Y, Is.EqualTo(0));
	}

	[TestCase(DockStyle.Top)]
	[TestCase(DockStyle.Bottom)]
	public async Task TestDockVerticalFullWidthOriginalHeight(DockStyle dockStyle)
	{
		var parent = await GetParentWithChildren<Control>(docks: dockStyle);
		var child = parent.Controls[0];

		Assert.That(child.Width, Is.EqualTo(1000));
		Assert.That(child.Height, Is.EqualTo(15));
	}

	[Test]
	public async Task TestDockBottomPositionBottom()
	{
		var parent = await GetParentWithChildren<Control>(docks: DockStyle.Bottom);
		var child = parent.Controls[0];

		Assert.That(child.Bounds.X, Is.EqualTo(0));
		Assert.That(child.Bounds.Y, Is.EqualTo(985));
	}

	[Test]
	public async Task TestDockLeftThenTopZOrderRespected()
	{
		var parent = await GetParentWithChildren<Control>(docks: new[] { DockStyle.Top, DockStyle.Top });
		var child1 = parent.Controls[0];
		var child2 = parent.Controls[1];

		Assert.That(child1.Bounds.X, Is.EqualTo(0));
		Assert.That(child1.Bounds.Y, Is.EqualTo(15));
		Assert.That(child2.Bounds.X, Is.EqualTo(0));
		Assert.That(child2.Bounds.Y, Is.EqualTo(0));
	}

	[Test]
	public async Task TestDockTopThenLeftZOrderRespected()
	{
		var parent = await GetParentWithChildren<Control>(docks: new[] { DockStyle.Top, DockStyle.Left });
		var child1 = parent.Controls[0];
		var child2 = parent.Controls[1];

		Assert.That(child1.Bounds.X, Is.EqualTo(10));
		Assert.That(child1.Bounds.Y, Is.EqualTo(0));
		Assert.That(child2.Bounds.X, Is.EqualTo(0));
		Assert.That(child2.Bounds.Y, Is.EqualTo(0));
	}

	[Test]
	public async Task TestDockBottomThenRightZOrderRespected()
	{
		var parent = await GetParentWithChildren<Control>(docks: new[] { DockStyle.Bottom, DockStyle.Right });
		var child1 = parent.Controls[0];
		var child2 = parent.Controls[1];

		Assert.That(child1.Bounds.X, Is.EqualTo(0));
		Assert.That(child1.Bounds.Y, Is.EqualTo(985));
		Assert.That(child2.Bounds.X, Is.EqualTo(990));
		Assert.That(child2.Bounds.Y, Is.EqualTo(0));
	}

	[Test]
	public async Task TestDockRightThenTopZOrderRespected()
	{
		var parent = await GetParentWithChildren<Control>(docks: new[] { DockStyle.Right, DockStyle.Top });
		var child1 = parent.Controls[0];
		var child2 = parent.Controls[1];

		Assert.That(child1.Bounds.X, Is.EqualTo(990));
		Assert.That(child1.Bounds.Y, Is.EqualTo(15));
		Assert.That(child2.Bounds.X, Is.EqualTo(0));
		Assert.That(child2.Bounds.Y, Is.EqualTo(0));
	}

	[Test]
	public async Task TestDockFillFullWidthAndHeight()
	{
		var parent = await GetParentWithChildren<Control>(docks: DockStyle.Fill);
		var child = parent.Controls[0];

		Assert.That(child.Width, Is.EqualTo(1000));
		Assert.That(child.Height, Is.EqualTo(1000));
	}

	[Test]
	public async Task TestDockLeftDockFillZOrderRespected()
	{
		var parent = await GetParentWithChildren<Control>(docks: new[] { DockStyle.Left, DockStyle.Fill });
		var child1 = parent.Controls[0];
		var child2 = parent.Controls[1];

		Assert.That(child1.Bounds.X, Is.EqualTo(0));
		Assert.That(child1.Bounds.Y, Is.EqualTo(0));
		Assert.That(child2.Bounds.X, Is.EqualTo(0)); // You'd think this would be 10 but based on testing in WinForms minimal repo the controls overlap??
		Assert.That(child2.Bounds.Y, Is.EqualTo(0));
	}

	static async Task<Control> GetParentWithChildren<T>(string text = "", bool autoSize = false, params DockStyle[] docks) where T : Control, new()
	{
		using var ctx = new WinzorTestContext();

		Control parent = null;

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			parent = new Control();
			parent.Height = 1000;
			parent.Width = 1000;

			foreach (var dock in docks)
			{
				var child = new T();
				child.Height = 15;
				child.Width = 10;
				child.Text = text;
				child.AutoSize = autoSize;
				child.Dock = dock;
				parent.Controls.Add(child);
			}
		});

		return parent;
	}
}
