using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class ControlTest
{
	[Test]
	public async Task AllowOverlapSetsZIndex()
	{
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			var control1 = new Control();
			var control2 = new Control();
			var control3 = new Control();
			control2.AllowOverlap(control1);
			control3.AllowOverlap(control2);
			Assert.That(control1.ZIndex, Is.EqualTo(0));
			Assert.That(control2.ZIndex, Is.EqualTo(1));
			Assert.That(control3.ZIndex, Is.EqualTo(2));
		});
	}

	[Test]
	public async Task AllowOverlapDoesNotReduceZIndex()
	{
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			var control1 = new Control();
			var control2 = new Control();
			var control3 = new Control();
			control2.AllowOverlap(control1);
			control3.AllowOverlap(control2);
			control3.AllowOverlap(control1);
			Assert.That(control1.ZIndex, Is.EqualTo(0));
			Assert.That(control2.ZIndex, Is.EqualTo(1));
			Assert.That(control3.ZIndex, Is.EqualTo(2));
		});
	}

	[Test]
	public async Task AllowOverlapWithSiblingChildControlHavingHigherZIndex()
	{
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			var parent = new Control();
			var control1 = new Control();
			var control2 = new Control();
			var control3 = new Control() { ZIndex = 1 };
			control2.Controls.Add(control3);
			parent.Controls.AddRange(new Control[] { control1, control2 });
			control1.AllowOverlap(control2);
			Assert.That(control1.ZIndex, Is.EqualTo(2));
		});
	}

	[Test]
	public async Task AllowOverlapWithSiblingDescendentControlHavingHigherZIndex()
	{
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			var parent = new Control();
			var control1 = new Control();
			var control2 = new Control();
			var control3 = new Control() { ZIndex = 1 };
			control2.Controls.Add(control3);
			var control4 = new Control() { ZIndex = 2 };
			control3.Controls.Add(control4);
			parent.Controls.AddRange(new Control[] { control1, control2 });
			control1.AllowOverlap(control2);
			Assert.That(control1.ZIndex, Is.EqualTo(3));
		});
	}

	[Test]
	public async Task UseDefaultCursorIfSet()
	{
		var cursor = Cursors.UpArrow;
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			Control testCursorControl = new TestDefaultCursorControl(cursor);
			Assert.That(testCursorControl.Cursor, Is.EqualTo(cursor));
		});
	}

	[Test]
	public async Task GetCursorByTheExpectedOrder()
	{
		var assignedCursor = Cursors.Cross;
		var defaultCursor = Cursors.UpArrow;
		var parentCursor = Cursors.Hand;
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			Control parentControl = new Control()
			{
				Cursor = parentCursor,
			};
			Control testCursorControl = new TestDefaultCursorControl(defaultCursor)
			{
				Cursor = assignedCursor,
			};
			Control testDefaultCursorControl = new TestDefaultCursorControl(Cursors.Default);
			parentControl.Controls.Add(testCursorControl);
			parentControl.Controls.Add(testDefaultCursorControl);
			Assert.That(testCursorControl.Cursor, Is.EqualTo(assignedCursor));

			testCursorControl.Cursor = null;
			Assert.That(testCursorControl.Cursor, Is.EqualTo(defaultCursor));
			Assert.That(testDefaultCursorControl.Cursor, Is.EqualTo(parentCursor));

			Control testDefaultCursorControlAlone = new TestDefaultCursorControl(Cursors.Default);
			Assert.That(testDefaultCursorControlAlone.Cursor, Is.EqualTo(Cursors.Default));
		});
	}

	[Test]
	public async Task CursorReflectingUseWaitCursorValue()
	{
		var cursor = Cursors.UpArrow;
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			Control control = new TestDefaultCursorControl(cursor);
			control.UseWaitCursor = true;
			Assert.That(control.Cursor, Is.EqualTo(Cursors.WaitCursor));
			control.UseWaitCursor = false;
			Assert.That(control.Cursor, Is.EqualTo(cursor));
		});
	}

	[Test]
	public async Task ChildUpdateUseWaitCursorAsParentDoes()
	{
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			Control parentControl = new Control();
			Control control = new Control();
			parentControl.Controls.Add(control);

			Assert.That(control.UseWaitCursor, Is.False);
			parentControl.UseWaitCursor = true;
			Assert.That(control.UseWaitCursor, Is.True);
		});
	}

	[Test]
	public async Task ControlFocusWhenParentSuspendAndResumeDrawing()
	{
		using var ctx = new EnterpriseTestContext();
		Control parentControl = null;
		Control childControl = null;
		await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			parentControl = new Control();
			childControl = new Control();
			parentControl.Controls.Add(childControl);
			form.Controls.Add(parentControl);
			return form;
		});

		Assert.That(parentControl.Focused, Is.True);
		Assert.That(childControl.Focused, Is.False);

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			parentControl.SuspendDrawing();
			childControl.Focus();
		});

		Assert.That(parentControl.Focused, Is.True);
		Assert.That(childControl.Focused, Is.False);

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			parentControl.ResumeDrawing();
			childControl.Focus();
		});

		Assert.That(parentControl.Focused, Is.False);
		Assert.That(childControl.Focused, Is.True);
	}

	[Test]
	public async Task ControlFocusWhenSuspendAndResumeDrawing()
	{
		using var ctx = new EnterpriseTestContext();
		Control parentControl = null;
		Control childControl = null;
		await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			parentControl = new Control();
			childControl = new Control();
			parentControl.Controls.Add(childControl);
			form.Controls.Add(parentControl);
			return form;
		});

		Assert.That(parentControl.Focused, Is.True);
		Assert.That(childControl.Focused, Is.False);

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			childControl.SuspendDrawing();
			childControl.Focus();
		});

		Assert.That(parentControl.Focused, Is.True);
		Assert.That(childControl.Focused, Is.False);

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			childControl.ResumeDrawing();
			childControl.Focus();
		});

		Assert.That(parentControl.Focused, Is.False);
		Assert.That(childControl.Focused, Is.True);
	}

	class TestDefaultCursorControl : Control
	{
		public TestDefaultCursorControl(Cursor cursor)
		{
			defaultCursor = cursor;
		}

		readonly Cursor defaultCursor;
		protected override Cursor DefaultCursor => defaultCursor;
	}
}
