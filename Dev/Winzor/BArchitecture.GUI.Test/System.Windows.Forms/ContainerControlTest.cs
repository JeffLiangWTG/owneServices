using System.Threading.Tasks;
using Bunit;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

class ContainerControlTest
{
	[Test]
	public async Task ContainerControl_ActiveContainerControl_Set_GetReturnsExpected()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var control = new ContainerControl();
			var child = new Control();
			var grandchild = new Control();
			control.Controls.Add(child);
			child.Controls.Add(grandchild);

			control.ActiveControl = child;
			Assert.That(child, Is.EqualTo(control.ActiveControl));

			// Set same.
			control.ActiveControl = child;
			Assert.That(child, Is.EqualTo(control.ActiveControl));

			// Set grandchild.
			control.ActiveControl = grandchild;
			Assert.That(grandchild, Is.EqualTo(control.ActiveControl));

			// Set null.
			control.ActiveControl = null;
			Assert.That(control.ActiveControl, Is.Null);
		});
	}

	[Test]
	public async Task ContainerControl_ActiveContainerControl_SetInvalid_ThrowsArgumentException()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var control = new ContainerControl();
			Assert.That(() => control.ActiveControl = control, Throws.ArgumentException);
			Assert.That(() => control.ActiveControl = new Control(), Throws.ArgumentException);
		});
	}

	[Test]
	public async Task ContainerControl_Dispose_Invoke_ResetsActiveControl()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var control = new ContainerControl();
			var child = new Control();
			control.Controls.Add(child);
			control.ActiveControl = child;

			control.Dispose();
			Assert.That(control.ActiveControl, Is.Null);
		});
	}

	[Test]
	public async Task FocusFirstFocusableChild()
	{
		using var ctx = new WinzorTestContext();

		ContainerControl parent = null;
		Button focusableChild = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			parent = new ContainerControl { Width = 100, Height = 100 };
			parent.Controls.Add(focusableChild = new Button());
			return parent;
		});

		Assert.That(focusableChild.Focused, Is.True);
		await parent.InvokeWinzorDispatcherAsync(() => parent.Focus());
		Assert.That(focusableChild.Focused, Is.True);
		Assert.That(parent.Focused, Is.False);
	}

	[Test, WithPlaywrightPage]
	public async Task CannotFocusUnfocusableChild()
	{
		await using var ctx = new InMemoryTestServerContext();

		ContainerControl parent = null;
		Label unfocusableChild = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();

			parent = new ContainerControl { Width = 100, Height = 100 };
			unfocusableChild = new Label { Text = "child" };

			parent.Controls.Add(unfocusableChild);
			form.Controls.Add(parent);

			return form;
		});

		Assert.That(unfocusableChild.Focused, Is.False);

		await parent.InvokeWinzorDispatcherAsync(() => parent.Focus());

		Assert.That(() => unfocusableChild.Focused, Is.False.After(2000, 100));
	}

	[Test]
	public async Task ShouldHaveCorrectSizeWhenAutoSize()
	{
		using var ctx = new WinzorTestContext();
		ContainerControl containerControl = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			containerControl = new ContainerControl { Height = 80, Width = 50, AutoSize = true };
			containerControl.Controls.Add(new Button { Height = 50, Width = 100, Top = 0, Left = 0, Margin = Padding.Empty });
			return containerControl;
		});

		var containerControlEl = rendered.Find("div[data-type='System.Windows.Forms.ContainerControl']");
		Assert.That(containerControlEl.GetAttribute("style"), Does.Contain("width:100px;height:80px"));

		await containerControl.InvokeWinzorDispatcherAsync(() => containerControl.Controls.Add(new Button { Height = 50, Width = 200, Top = 50, Left = 0, Margin = Padding.Empty }));
		Assert.That(containerControlEl.GetAttribute("style"), Does.Contain("width:200px;height:100px"));
	}

	[Test, WithPlaywrightPage]
	public async Task ActiveControlIsNullAfterAnotherContainerControlGetsFocus()
	{
		await using var ctx = new InMemoryTestServerContext();

		ContainerControl container1 = null;
		ContainerControl container2 = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();

			container1 = new ContainerControl { Width = 50, Height = 50, Top = 0 };
			container1.Controls.Add(new TextBox());
			form.Controls.Add(container1);

			container2 = new ContainerControl { Width = 50, Height = 50, Top = 50 };
			container2.Controls.Add(new TextBox());
			form.Controls.Add(container2);

			return form;
		});

		Assert.That(() => container2.ActiveControl, Is.Null);

		var textBoxElement2 = await page.WaitForSelectorAsync("div:nth-child(2) input");
		await textBoxElement2.ClickAsync();
		Assert.That(() => container2.ActiveControl, Is.Not.Null.After(3000, 100));

		var textBoxElement1 = await page.WaitForSelectorAsync("div:nth-child(1) input");
		await textBoxElement1.ClickAsync();
		Assert.That(() => container2.ActiveControl, Is.Null.After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task ValidatingEventNotRaisedWhenClickBlankAreaOfContainer()
	{
		await using var ctx = new InMemoryTestServerContext();

		ContainerControl container = null;
		var validatingEventRaised = false;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();

			container = new ContainerControl { Width = 50, Height = 50, Top = 0 };
			var textBox = new TextBox();
			textBox.Validating += (s, e) => validatingEventRaised = true;
			container.Controls.Add(textBox);
			form.Controls.Add(container);

			return form;
		});

		var textBoxElement = await page.WaitForSelectorAsync("div input");
		await textBoxElement.ClickAsync();
		Assert.That(() => container.ActiveControl, Is.Not.Null.After(3000, 100));
		Assert.That(validatingEventRaised, Is.False);

		await page.Mouse.ClickAsync(40, 40);
		Assert.That(() => container.ActiveControl, Is.Not.Null.After(3000, 100));
		Assert.That(validatingEventRaised, Is.False);
	}

	[Test]
	public async Task UpdateFocusedControlShouldTerminateWhenPathControlRemainsUnchangedAfterFocusedControlChanges()
	{
		using var ctx = new WinzorTestContext();
		ContainerControl containerControl1 = null;
		ContainerControl containerControl2 = null;
		ContainerControl containerControl3 = null;
		TextBox textBox1 = null;
		TextBox textBox2 = null;
		TextBox textBox3 = null;

		await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			containerControl1 = new ContainerControl();
			containerControl2 = new ContainerControl();
			containerControl3 = new ContainerControl();
			textBox1 = new TextBox();
			textBox2 = new TextBox();
			textBox3 = new TextBox();

			textBox2.Leave += (s, e) =>
			{
				textBox3.Focus();
			};

			containerControl1.Controls.Add(textBox1);
			containerControl2.Controls.Add(textBox2);
			containerControl3.Controls.Add(textBox3);

			form.Controls.Add(containerControl1);
			form.Controls.Add(containerControl2);
			form.Controls.Add(containerControl3);

			return form;
		});

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			Assert.That(containerControl1.ActiveControl, Is.EqualTo(textBox1));
			Assert.That(containerControl2.ActiveControl, Is.Null);
			Assert.That(containerControl3.ActiveControl, Is.Null);

			textBox2.Focus();
			Assert.That(containerControl1.ActiveControl, Is.Null);
			Assert.That(containerControl2.ActiveControl, Is.EqualTo(textBox2));
			Assert.That(containerControl3.ActiveControl, Is.Null);

			// Trigger textBox2.Leave
			textBox1.Focus();
			Assert.That(containerControl1.ActiveControl, Is.Null);
			Assert.That(containerControl2.ActiveControl, Is.Null);
			Assert.That(containerControl3.ActiveControl, Is.EqualTo(textBox3));
		});
	}
}
