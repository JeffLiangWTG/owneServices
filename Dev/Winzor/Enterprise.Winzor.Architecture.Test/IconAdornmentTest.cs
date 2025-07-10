using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.Notifications;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorFramework.Extensions;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;
internal class IconAdornmentTest
{
	[Test]
	public async Task IconAdornmentIsRenderedForControlNotification()
	{
		using var ctx = new EnterpriseTestContext();
		ZTextBox textBox = null;
		ZTextBox textBoxFocused = null;
		Form form = null;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			textBox = new ZTextBox();
			textBoxFocused = new ZTextBox();
			form = new Form();
			form.Controls.Add(textBoxFocused);
			form.Controls.Add(textBox);
			return form;
		});

		Assert.That(rendered.FindAll(".notification").Count, Is.EqualTo(0));

		await textBox.InvokeWinzorDispatcherAsync(() =>
		{
			var notifications = new NotificationCollection();
			notifications.AddError("Err");
			textBox.GetExtension<NotificationExtension>().Notifications = notifications;
		});
		rendered.WaitForState(() => rendered.FindAll(".notification").Count == 1);

		await textBox.InvokeWinzorDispatcherAsync(() =>
		{
			textBox.GetExtension<NotificationExtension>().Notifications = NotificationCollection.Empty;
		});
		rendered.WaitForState(() => rendered.FindAll(".notification").Count == 0);
	}

	[Test]
	public async Task NotificationWarningIconPosition()
	{
		using var ctx = new EnterpriseTestContext();
		ZButton button = null;
		IconAdornment iconAdornment = new IconAdornment();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			button = new ZButton();
			var notifications = new NotificationCollection();
			notifications.AddWarning("Warning");
			button.GetExtension<NotificationExtension>().Notifications = notifications;
			return button;
		});
		var notification = rendered.Find("div[data-type='WinzorFramework.NotificationIcon']");
		Assert.That(notification, Is.Not.Null);
		Assert.That(notification.GetAttribute("style"), Does.Contain("top:1px;left:62px;"));
	}

	[Test, WithPlaywrightPage]
	public async Task NotificationIconIsHiddenForHiddenControl()
	{
		ZTextBox hiddenTextBox = null;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			ZTextBox inputBoxFocused = new ZTextBox() { BorderStyle = BorderStyle.Fixed3D, Location = new Point(10, 10) };
			ZTextBox textBox = new ZTextBox() { Top = 40, Left = 10 };
			textBox.GetExtension<NotificationExtension>().Notifications = new NotificationCollection
			{
				{ NotificationType.Error, "Notification Error Message Here" }
			};

			hiddenTextBox = new ZTextBox() { Visible = false , Top = 70, Left = 10 };
			hiddenTextBox.GetExtension<NotificationExtension>().Notifications = new NotificationCollection
			{
				{ NotificationType.Warning, "Notification Warning Message Here" }
			};

			form.Controls.Add(inputBoxFocused);
			form.Controls.Add(hiddenTextBox);
			form.Controls.Add(textBox);
			return form;
		});
		await page.WaitForSelectorAsync(".notification--error");
		var notifications = await page.QuerySelectorAllAsync(".notification");
		Assert.That(notifications.Count, Is.EqualTo(1));

		await hiddenTextBox.InvokeWinzorDispatcherAsync(() => hiddenTextBox.Visible = true );

		await page.WaitForSelectorAsync(".notification--error");
		await page.WaitForSelectorAsync(".notification--warning");
		notifications = await page.QuerySelectorAllAsync(".notification");
		Assert.That(notifications.Count, Is.EqualTo(2));
	}

	[Test]
	public async Task IconAdornmentPassesThruClickEventIForControl()
	{
		using var ctx = new EnterpriseTestContext();
		ZButton button = null;
		ZButton buttonFocused = null;
		Form newForm = null;
		var wasHit = false;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			button = new ZButton();
			buttonFocused = new ZButton();
			button.Click += (o, e) => wasHit = true;

			newForm = new Form();
			newForm.Controls.Add(buttonFocused);
			newForm.Controls.Add(button);
			var notifications = new NotificationCollection();
			notifications.AddError("Err");
			button.GetExtension<NotificationExtension>().Notifications = notifications;
			return newForm;
		});

		var notification = rendered.Find(".notification");
		Assert.That(notification, Is.Not.Null);

		await notification.ClickAsync(new WebMouseEventArgs());
		Assert.That(wasHit, Is.True);
	}

	[Test]
	public async Task IconAdornmentPassesThruMouseUpEventForControl()
	{
		using var ctx = new EnterpriseTestContext();
		ZButton button = null;
		ZButton buttonFocused = null;
		Form newForm = null;
		var wasHit = false;
		var mouseUpHit = false;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			button = new ZButton();
			buttonFocused = new ZButton();
			button.Click += (o, e) => wasHit = true;
			button.MouseUp += (s, e) => mouseUpHit = true;

			newForm = new Form();
			newForm.Controls.Add(buttonFocused);
			newForm.Controls.Add(button);
			var notifications = new NotificationCollection();
			notifications.AddError("Err");
			button.GetExtension<NotificationExtension>().Notifications = notifications;
			return newForm;
		});

		var notification = rendered.Find(".notification");
		Assert.That(notification, Is.Not.Null);

		await notification.ClickAsync(new WebMouseEventArgs() { ClientX = 1, ClientY = 1 });
		Assert.That(wasHit, Is.True);
		Assert.That(mouseUpHit, Is.True);
	}

	[Test]
	public async Task ShowDialogFromClickEventForControlWithControlLayoutChanging()
	{
		using var ctx = new EnterpriseTestContext();
		var windowService = new Mock<IWindowService>();
		Form newForm = null;
		var loadRequestSent = new TaskCompletionSource();
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				loadRequestSent.SetResult();
				newForm.Dispose();
			});
		var clientServices = MockCargoWiseClientServices.MakeMock(windowService: windowService.Object);
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var button = new ZButton();
			var buttonFocused = new ZButton();
			button.Click += (o, e) =>
			{
				button.Width += 1;
				newForm = new Form();
				newForm.ShowDialog();
			};
			var form = new Form();
			form.Controls.Add(buttonFocused);
			form.Controls.Add(button);
			var notifications = new NotificationCollection();
			notifications.AddError("Err");
			button.GetExtension<NotificationExtension>().Notifications = notifications;
			return form;
		}, clientServices);

		var notification = rendered.Find(".notification");
		Assert.That(notification, Is.Not.Null);

		await notification.ClickAsync(new WebMouseEventArgs());
		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);
	}

	[Test]
	public async Task ShowDialogFromClickEventForControlWithNotificationClearing()
	{
		using var ctx = new EnterpriseTestContext();
		var windowService = new Mock<IWindowService>();
		Form newForm = null;
		var loadRequestSent = new TaskCompletionSource();
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				loadRequestSent.SetResult();
				newForm.Dispose();
			});
		var clientServices = MockCargoWiseClientServices.MakeMock(windowService: windowService.Object);
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var button = new ZButton();
			var buttonFocused = new ZButton();
			button.Click += (o, e) =>
			{
				button.GetExtension<NotificationExtension>().Notifications = new NotificationCollection();
				newForm = new Form();
				newForm.ShowDialog();
			};
			var form = new Form();
			form.Controls.Add(buttonFocused);
			form.Controls.Add(button);
			var notifications = new NotificationCollection();
			notifications.AddError("Err");
			button.GetExtension<NotificationExtension>().Notifications = notifications;
			return form;
		}, clientServices);

		var notification = rendered.Find(".notification");
		Assert.That(notification, Is.Not.Null);

		await notification.ClickAsync(new WebMouseEventArgs());
		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);
	}

	[Test]
	public async Task TestCheckboxIconAdornmentLocation()
	{
		using var ctx = new EnterpriseTestContext();
		ZCheckBox button = null;

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			button = new ZCheckBox();
			var notifications = new NotificationCollection();
			notifications.AddError("Err");
			button.GetExtension<NotificationExtension>().Notifications = notifications;
			return button;
		});

		var notification = rendered.Find("div[data-type='WinzorFramework.NotificationIcon']");

		Assert.That(notification, Is.Not.Null);
		Assert.That(notification.GetAttribute("style"), Does.Contain("top:6px;left:15px;"));
	}

	[Test]
	public async Task TestZDropEditIconAdornmentLocation()
	{
		using var ctx = new EnterpriseTestContext();
		ZDropEdit button = null;

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			button = new ZDropEdit();
			var notifications = new NotificationCollection();
			notifications.AddError("Err");
			button.GetExtension<NotificationExtension>().Notifications = notifications;
			return button;
		});

		var notification = rendered.Find("div[data-type='WinzorFramework.NotificationIcon']");

		Assert.That(notification, Is.Not.Null);
		Assert.That(notification.GetAttribute("style"), Does.Contain("top:5px;left:89px;"));
	}

	[Test, WithPlaywrightPage]
	public async Task TestNotificationIconAdornmentSize()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		Form form = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			ZTextBox inputBox = new ZTextBox();
			ZTextBox inputBoxFocused = new ZTextBox();
			var notifications = new NotificationCollection();
			notifications.AddError("Err");
			inputBox.GetExtension<NotificationExtension>().Notifications = notifications;
			form.Controls.Add(inputBoxFocused);
			form.Controls.Add(inputBox);
			return form;
		});

		var notificationErrorIcon = await page.WaitForSelectorAsync(".notification");

		Assert.That(async () => await notificationErrorIcon.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-size')"), Is.EqualTo("12px 12px"));
		Assert.That(async () => await notificationErrorIcon.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('height')"), Is.EqualTo("12px"));
		Assert.That(async () => await notificationErrorIcon.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("12px"));
	}

	[WithPlaywrightPage(Headless = true)]
	[TestCase(BorderStyle.Fixed3D, 2)]
	[TestCase(BorderStyle.FixedSingle, 1)]
	[TestCase(BorderStyle.None, 0)]
	public async Task TestNotificationIconTextBoxPlacement(BorderStyle bs, int borderWidth)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		Form form = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var inputBox = new ZTextBox();
			var inputBoxFocused = new ZTextBox();
			inputBox.BorderStyle = bs;
			inputBox.Location = new Point(10, 10);
			inputBoxFocused.BorderStyle = bs;
			inputBoxFocused.Location = new Point(10, 10);

			var notifications = new NotificationCollection();
			notifications.AddError("Err");
			var notif = inputBox.GetExtension<NotificationExtension>();
			notif.Notifications = notifications;
			form.Controls.Add(inputBoxFocused);
			form.Controls.Add(inputBox);
			return form;
		});

		await page.Mouse.ClickAsync(50, 50); // unfocus, trigger notification icon

		var textBox = await page.QuerySelectorAsync("input.textbox");
		var textBoxBounding = await textBox.BoundingBoxAsync();
		var textBoxBoundingX = textBoxBounding.X;
		var textBoxBoundingY = textBoxBounding.Y;
		var textBoxBoundingWidth = textBoxBounding.Width;

		var notificationErrorIcon = await page.QuerySelectorAsync("div[data-type='WinzorFramework.NotificationIcon'] > div");
		var notificationBounding = await notificationErrorIcon.BoundingBoxAsync();

		var x = notificationBounding.X;
		var y = notificationBounding.Y;
		var width = notificationBounding.Width;
		var height = notificationBounding.Height;

		var iconOffset = 1; // px

		Assert.That(y - iconOffset - borderWidth, Is.EqualTo(textBoxBoundingY));
		var xPos = textBoxBoundingX + textBoxBoundingWidth - iconOffset - borderWidth - width;
		Assert.That(x, Is.EqualTo(xPos));
	}

	[Test, WithPlaywrightPage]
	public async Task ValidateCorrectNotificationIconHasRendered()
	{
		try
		{
			var imageForError = Icons.GetMiniImage(IconTypes.Warning);
			using var imageForWarning = new Bitmap(12, 10, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

			NotificationIconScheme.Instance.SetMiniImage(CargoWise.ComponentModel.NotificationType.Error, imageForError);
			NotificationIconScheme.Instance.SetMiniImage(CargoWise.ComponentModel.NotificationType.Warning, imageForWarning);

			await using var ctx = new InMemoryAppServerTestContext();
			var page = await ctx.LoadFormAsync(() =>
			{
				Form form = new Form();

				ZTextBox inputBoxFocused = new ZTextBox() { BorderStyle = BorderStyle.Fixed3D , Location = new Point(10, 10) };

				ZTextBox inputBoxError = new ZTextBox() { BorderStyle = BorderStyle.Fixed3D, Location = new Point(10, 40) };
				inputBoxError.GetExtension<NotificationExtension>().Notifications = new NotificationCollection
				{
					{ CargoWise.ComponentModel.NotificationType.Error, "Notification Error Message Here" }
				};

				ZTextBox inputBoxWarning = new ZTextBox() { BorderStyle = BorderStyle.Fixed3D, Location = new Point(10, 70) };
				inputBoxWarning.GetExtension<NotificationExtension>().Notifications = new NotificationCollection
				{
					{ CargoWise.ComponentModel.NotificationType.Warning, "Notification Warning Message Here" }
				};

				form.Controls.Add(inputBoxFocused);
				form.Controls.Add(inputBoxError);
				form.Controls.Add(inputBoxWarning);
				return form;
			});

			await page.Locator(".form").ClickAsync(); // unfocus, trigger notification icon

			var notificationError = await page.WaitForSelectorAsync(".notification--error");
			Assert.That(await notificationError.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-image')"), Does.Contain(imageForError.ToBase64()));
			Assert.That(async () => await notificationError.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('height')"), Is.EqualTo("12px"));
			Assert.That(async () => await notificationError.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("12px"));
			Assert.That(async () => await notificationError.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-size')"), Is.EqualTo("12px 12px"));

			var notificationWarning = await page.WaitForSelectorAsync(".notification--warning");
			Assert.That(await notificationWarning.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-image')"), Does.Contain(imageForWarning.ToBase64()));
			Assert.That(async () => await notificationWarning.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('height')"), Is.EqualTo("10px"));
			Assert.That(async () => await notificationWarning.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("12px"));
			Assert.That(async () => await notificationWarning.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-size')"), Is.EqualTo("12px 10px"));
		}
		finally
		{
			NotificationIconScheme.Instance.SetMiniImage(CargoWise.ComponentModel.NotificationType.Error, Icons.GetMiniImage(IconTypes.Error));
			NotificationIconScheme.Instance.SetMiniImage(CargoWise.ComponentModel.NotificationType.Warning, Icons.GetMiniImage(IconTypes.Warning));
		}
	}

	[Test]
	public async Task DoNotStoreMultipleNotificationIconsForSingleControl()
	{
		using var ctx = new EnterpriseTestContext();
		Form form = null;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form();

			var notifications = new NotificationCollection();
			notifications.AddError("12345");
			notifications.AddWarning("ABCDE");

			var control = new ZTextBox();
			control.GetExtension<NotificationExtension>().Notifications1 = notifications;
			control.GetExtension<NotificationExtension>().Notifications2 = notifications;

			var activeTextBox = new ZTextBox();

			form.Controls.Add(activeTextBox);
			form.Controls.Add(control);

			return form;
		});

		Assert.That(form.WinzorSpecificControls.Count, Is.EqualTo(1));
		Assert.That(form.WinzorSpecificControls[0].GetType(), Is.EqualTo(typeof(NotificationIcon)));
		Assert.That(((NotificationIcon)form.WinzorSpecificControls[0]).NotificationType, Is.EqualTo("error"));
	}

	[TestCase(0, 0, true)]
	[TestCase(0, 0, false)]
	[TestCase(100, 100, true)]
	[TestCase(100, 100, false)]
	[WithPlaywrightPage]
	public async Task ZCheckBoxHasCorrectlyPositionedNotificationIcon(int left, int top, [Values] bool inPanel)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(500, 500) };
			var checkbox = new ZCheckBox();
			checkbox.Left = left;
			checkbox.Top = top;
			checkbox.Text = "ABCDE";

			var checkBoxFocused = new ZCheckBox();
			checkBoxFocused.Left = left;
			checkBoxFocused.Top = top;
			checkBoxFocused.Text = "TEST";

			if (inPanel)
			{
				var panel = new Panel() { Bounds = new Rectangle(200, 150, 300, 350) };
				panel.Controls.Add(checkBoxFocused);
				panel.Controls.Add(checkbox);
				form.Controls.Add(panel);
			}
			else
			{
				form.Controls.Add(checkBoxFocused);
				form.Controls.Add(checkbox);
			}

			var notifications = new NotificationCollection();
			notifications.AddError("12345");
			checkbox.GetExtension<NotificationExtension>().Notifications1 = notifications;

			return form;
		});

		await page.Mouse.ClickAsync(50, 50); // unfocus, trigger notification icon

		var icon = await page.WaitForSelectorAsync(".notification");
		var iconBoundBox = await icon.BoundingBoxAsync();

		var checkbox = await page.WaitForSelectorAsync(".checkbox");
		var checkboxBoundBox = await checkbox.BoundingBoxAsync();

		Assert.That(iconBoundBox.X, Is.EqualTo(checkboxBoundBox.X + 15), "Incorrect X value");
		Assert.That(iconBoundBox.Y, Is.EqualTo(checkboxBoundBox.Y + 6), "Incorrect Y value");
	}

	[Test]
	public async Task ZDropButtonClickNotBlockedByNotificationIcon()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var button1 = new ZDropEdit() { Top = 0 };
			var button2 = new ZDropEdit() { Top = 50 };
			var notifications1 = new NotificationCollection();
			var notifications2 = new NotificationCollection();
			notifications1.AddError("Err");
			notifications2.AddWarning("Warn");
			button1.GetExtension<NotificationExtension>().Notifications = notifications1;
			button2.GetExtension<NotificationExtension>().Notifications = notifications2;
			form.Controls.Add(button1);
			form.Controls.Add(button2);

			return form;
		});

		await rendered.Find(".notification--error").MouseDownAsync(new WebMouseEventArgs { ClientX = 1, ClientY = 1 });
		Assert.That(rendered.WaitForElement(".zdropform"), Is.Not.Null);

		await rendered.Find(".notification--error").MouseDownAsync(new WebMouseEventArgs { ClientX = 1, ClientY = 1 });
		Assert.Throws<ElementNotFoundException>(() => rendered.Find(".zdropform"));

		await rendered.Find(".notification--warning").MouseDownAsync(new WebMouseEventArgs { ClientX = 1, ClientY = 1 });
		Assert.That(rendered.WaitForElement(".zdropform"), Is.Not.Null);

		await rendered.Find(".notification--warning").MouseDownAsync(new WebMouseEventArgs { ClientX = 1, ClientY = 1 });
		Assert.Throws<ElementNotFoundException>(() => rendered.Find(".zdropform"));
	}

	[Test, WithPlaywrightPage]
	public async Task ZDropButtonHaveCorrectNotificationIcon()
	{
		try
		{
			using var imageForError = new Bitmap(10, 11, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
			NotificationIconScheme.Instance.SetMiniImage(NotificationType.Error, imageForError);

			await using var ctx = new InMemoryAppServerTestContext();
			var page = await ctx.LoadFormAsync(() =>
			{
				var form = new Form();
				ZTextBox inputBoxFocused = new ZTextBox() { BorderStyle = BorderStyle.Fixed3D, Location = new Point(10, 10) };
				var dropEditError = new ZDropEdit() { BorderStyle = BorderStyle.Fixed3D, Location = new Point(10, 30) };
				dropEditError.GetExtension<NotificationExtension>().Notifications = new NotificationCollection
				{
					{ NotificationType.Error, "Notification Warning Message Here" }
				};
				form.Controls.Add(inputBoxFocused);
				form.Controls.Add(dropEditError);
				return form;
			});

			await page.Mouse.ClickAsync(50, 50); // unfocus, trigger notification icon

			var notificationError = await page.WaitForSelectorAsync(".notification--error");
			Assert.That(await notificationError.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-image')"), Does.Contain(imageForError.ToBase64()));
			Assert.That(async () => await notificationError.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('height')"), Is.EqualTo("11px"));
			Assert.That(async () => await notificationError.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('width')"), Is.EqualTo("10px"));
			Assert.That(async () => await notificationError.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-size')"), Is.EqualTo("10px 11px"));
		}
		finally
		{
			NotificationIconScheme.Instance.SetMiniImage(NotificationType.Error, Icons.GetMiniImage(IconTypes.Error));
		}
	}

	[Test]
	public async Task IconAdornmentIsNotRenderedForAnchorControlLabelWithEmptyLabelText()
	{
		using var ctx = new EnterpriseTestContext();
		ZLabel label = null;
		Form form = null;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			label = new ZLabel() { Text = string.Empty };
			form = new Form();
			form.Controls.Add(label);
			return form;
		});

		Assert.That(rendered.FindAll(".notification").Count, Is.EqualTo(0));

		await label.InvokeWinzorDispatcherAsync(() =>
		{
			var notifications = new NotificationCollection();
			notifications.AddError("Error");
			label.GetExtension<NotificationExtension>().Notifications = notifications;
		});
		Assert.That(rendered.FindAll(".notification").Count, Is.EqualTo(0));
	}

	[Test]
	public async Task IconAdornmentIsRenderedForAnchorControlLabelWithText()
	{
		using var ctx = new EnterpriseTestContext();
		ZLabel label = null;
		Form form = null;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			label = new ZLabel() { Text = "Test Notification" };
			form = new Form();
			form.Controls.Add(label);
			return form;
		});

		Assert.That(rendered.FindAll(".notification").Count, Is.EqualTo(0));

		await label.InvokeWinzorDispatcherAsync(() =>
		{
			var notifications = new NotificationCollection();
			notifications.AddError("Error");
			label.GetExtension<NotificationExtension>().Notifications = notifications;
		});
		Assert.That(rendered.FindAll(".notification").Count, Is.EqualTo(1));
	}

	[Test]
	public async Task IconAdornmentIsRenderedForAnchorControlWhenLabelTextIsReset()
	{
		using var ctx = new EnterpriseTestContext();
		Label label = null;
		Form form = null;
		var notifications = new NotificationCollection();
		notifications.AddError("Error");

		var rendered = await ctx.RenderFormAsync(() =>
		{
			label = new ZLabel() { Text = string.Empty };
			form = new Form();
			form.Controls.Add(label);
			return form;
		});

		await label.InvokeWinzorDispatcherAsync(() =>
		{
			label.GetExtension<NotificationExtension>().Notifications = notifications;
		});

		Assert.That(rendered.FindAll(".notification").Count, Is.EqualTo(0));

		await label.InvokeWinzorDispatcherAsync(() =>
		{
			label.Text = "Test Notification";
			label.GetExtension<NotificationExtension>().Redraw();
		});

		Assert.That(rendered.FindAll(".notification").Count, Is.EqualTo(1));
	}
}
