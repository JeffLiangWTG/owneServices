using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Balloons;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.Modules;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorFramework;
using WinzorFramework.Extensions;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using static WTG.PlaywrightTesting.PlaywrightTestContext;
using Color = System.Drawing.Color;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.Winzor.Architecture.Test;

class BalloonWindowTest
{
	[Test]
	public async Task BalloonWindow()
	{
		BalloonWindow window = null;
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(RefCommodityCode);
			var descriptionTextBox = new ZTextBox() { Width = 200, Height = 50, Top = 100, Left = 200 };
			var descriptor = new BalloonDescriptor(descriptionTextBox, "caption", "", new INotification[] {
				new Notification(CargoWise.ComponentModel.NotificationType.Error, "error"),
				new Notification(CargoWise.ComponentModel.NotificationType.Warning, "warn"),
				new Notification(CargoWise.ComponentModel.NotificationType.Information, "info"),
				new Notification(CargoWise.EntityFramework.NotificationType.MessageError, "message error") });
			window = new BalloonWindow() { Descriptor = descriptor };
			form.Controls.Add(descriptionTextBox);
			form.BindingSource.SetBindingMember(descriptionTextBox, "RH_IATACommodityItem");
			form.DataSourceType = typeof(RefCommodityCode);
			var factory = new BusinessObjectFactory();
			var bo = factory.New<RefCommodityCode>();
			bo.RH_IATACommodityItem = "";
			form.SetDataBinding(bo, "");
			window.Show();
			return form;
		});

		rendered.WaitForState(() =>
		rendered.FindAll(".balloon").Count == 1);

		var input = rendered.Find("input");
		var caption = rendered.Find(".balloon .balloon__caption");
		var description = rendered.Find(".balloon .balloon__description");
		var items = rendered.FindAll(".balloon__items li");
		var popup = rendered.FindAll(".balloon").Single().ParentElement;
		var balloonWrapper = popup.ParentElement;

		Assert.That(window.ZIndex, Is.EqualTo(95));
		Assert.That(popup.Attributes["class"].Value, Does.Contain("popup"));
		Assert.That(balloonWrapper.Attributes["style"].Value.Contains("z-index:95;", StringComparison.OrdinalIgnoreCase), Is.True);
		Assert.That(balloonWrapper.Attributes["style"].Value.Contains("top:100px;", StringComparison.OrdinalIgnoreCase), Is.True);
		Assert.That(balloonWrapper.Attributes["style"].Value.Contains("left:200px;", StringComparison.OrdinalIgnoreCase), Is.True);
		Assert.That(caption.InnerHtml, Is.EqualTo("caption"));
		Assert.That(description.InnerHtml, Is.Empty);
		Assert.That(items.Count, Is.EqualTo(4));
		Assert.That(items[0].Attributes["class"].Value, Is.EqualTo("balloon__item--error"));
		Assert.That(items[0].InnerHtml, Is.EqualTo("error"));
		Assert.That(items[1].Attributes["class"].Value, Is.EqualTo("balloon__item--warning"));
		Assert.That(items[1].InnerHtml, Is.EqualTo("warn"));
		Assert.That(items[2].Attributes["class"].Value, Is.EqualTo("balloon__item--information"));
		Assert.That(items[2].InnerHtml, Is.EqualTo("info"));
		Assert.That(items[3].Attributes["class"].Value, Is.EqualTo("balloon__item--message-error"));
		Assert.That(items[3].InnerHtml, Is.EqualTo("message error"));
	}

	[Test, WithPlaywrightPage]
	public async Task BalloonNotificationIconHasCorrectSize()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var descriptionTextBox = new ZTextBox() { Width = 200, Height = 50, Top = 100, Left = 200 };
			var descriptor = new BalloonDescriptor(descriptionTextBox, "caption", "", new INotification[] {
				new Notification(CargoWise.ComponentModel.NotificationType.Error, "error"),
			});
			var window = new BalloonWindow() { Descriptor = descriptor };
			form.Controls.Add(descriptionTextBox);
			window.Show();
			return form;
		});

		var balloonItem = await page.WaitForSelectorAsync(".balloon li");
		var backgroundSize = (await balloonItem.EvaluateAsync($"e => window.getComputedStyle(e).getPropertyValue('background-size')")).Value.ToString();
		Assert.That(backgroundSize, Is.EqualTo("14px"));
	}

	[Test, WithPlaywrightPage]
	public async Task BalloonNotificationHasCorrectIcon()
	{
		try
		{
			var imageForWarning = SystemIcons.Application.ToBitmap();
			var imageForError = Icons.GetMiniImage(IconTypes.Warning);

			NotificationIconScheme.Instance.SetMiniImage(CargoWise.ComponentModel.NotificationType.Warning, imageForWarning);
			NotificationIconScheme.Instance.SetMiniImage(CargoWise.ComponentModel.NotificationType.Error, imageForError);

			await using var ctx = new InMemoryAppServerTestContext();
			var page = await ctx.LoadFormAsync(() =>
			{
				var form = new Form();
				var descriptionTextBox = new ZTextBox() { Width = 200, Height = 50, Top = 100, Left = 200 };
				var descriptor = new BalloonDescriptor(descriptionTextBox, "caption", "", new INotification[] {
					new Notification(CargoWise.ComponentModel.NotificationType.Warning, "Warning message here"),
					new Notification(CargoWise.ComponentModel.NotificationType.Error, "Error message here")
				});
				var window = new BalloonWindow() { Descriptor = descriptor };
				form.Controls.Add(descriptionTextBox);
				window.Show();
				return form;
			});

			var balloonItemWarn = await page.WaitForSelectorAsync(".balloon li:nth-child(1)");
			Assert.That(await balloonItemWarn.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-image')"), Does.Contain(imageForWarning.ToBase64()));

			var balloonItemError = await page.WaitForSelectorAsync(".balloon li:nth-child(2)");
			Assert.That(await balloonItemError.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-image')"), Does.Contain(imageForError.ToBase64()));
		}
		finally
		{
			NotificationIconScheme.Instance.SetMiniImage(CargoWise.ComponentModel.NotificationType.Error, Icons.GetMiniImage(IconTypes.Error));
			NotificationIconScheme.Instance.SetMiniImage(CargoWise.ComponentModel.NotificationType.Warning, Icons.GetMiniImage(IconTypes.Warning));
		}
	}

	[Test, WithPlaywrightPage]
	public async Task BalloonNotificationIconHasCorrectPadding()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var descriptionTextBox = new ZTextBox() { Width = 200, Height = 50, Top = 100, Left = 200 };
			var descriptor = new BalloonDescriptor(descriptionTextBox, "caption", "", new INotification[] {
				new Notification(CargoWise.ComponentModel.NotificationType.Error, "error"),
			});
			var window = new BalloonWindow() { Descriptor = descriptor };
			form.Controls.Add(descriptionTextBox);
			window.Show();
			return form;
		});

		var balloon = await page.WaitForSelectorAsync(".balloon");
		var paddingTop = (await balloon.EvaluateAsync($"e => window.getComputedStyle(e).getPropertyValue('padding-top')")).Value.ToString();
		var paddingRight = (await balloon.EvaluateAsync($"e => window.getComputedStyle(e).getPropertyValue('padding-right')")).Value.ToString();
		var paddingBottom = (await balloon.EvaluateAsync($"e => window.getComputedStyle(e).getPropertyValue('padding-bottom')")).Value.ToString();
		var paddingLeft = (await balloon.EvaluateAsync($"e => window.getComputedStyle(e).getPropertyValue('padding-left')")).Value.ToString();
		Assert.That(paddingTop, Is.EqualTo("8px"));
		Assert.That(paddingRight, Is.EqualTo("20px"));
		Assert.That(paddingBottom, Is.EqualTo("2px"));
		Assert.That(paddingLeft, Is.EqualTo("10px"));
	}

	[Test, WithPlaywrightPage]
	public async Task BalloonNotificationIconHasCorrectMargin()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var descriptionTextBox = new ZTextBox() { Width = 300, Height = 50, Top = 100, Left = 0 };
			var descriptor = new BalloonDescriptor(descriptionTextBox, "This is a balloon test caption", "This is a balloon test description", new INotification[] {
				new Notification(CargoWise.ComponentModel.NotificationType.Information, "Information Message"),
			});
			var window = new BalloonWindow() { Descriptor = descriptor };
			form.Controls.Add(descriptionTextBox);
			window.Show();
			return form;
		});

		var balloon = await page.WaitForSelectorAsync(".balloon");
		Assert.That(async () => await balloon.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('margin-top')"), Is.EqualTo("0px"));
		Assert.That(async () => await balloon.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('margin-right')"), Is.EqualTo("2px"));
		Assert.That(async () => await balloon.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('margin-bottom')"), Is.EqualTo("0px"));
		Assert.That(async () => await balloon.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('margin-left')"), Is.EqualTo("2px"));
	}

	[Test, WithPlaywrightPage]
	public async Task BalloonShouldShowOnInvisibleControl()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		Form form = null;
		TabPage tabPage2 = null;
		BalloonWindow window = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var tabControl = new ZTabControl();
			var tabPage1 = new ZTabPage();
			tabPage1.Text = "TabPage1";
			tabControl.Controls.Add(tabPage1);
			tabPage2 = new TabPage();
			tabPage2.Text = "TabPage2";
			tabControl.Controls.Add(tabPage2);

			var descriptor = new BalloonDescriptor(tabPage2, "This is a balloon test caption", "This is a balloon test description", new INotification[] {
				new Notification(CargoWise.ComponentModel.NotificationType.Information, "Information Message"),
			});
			window = new BalloonWindow() { Descriptor = descriptor };
			form.Controls.Add(tabControl);
			window.Show();
			return form;
		});

		var balloon = await page.WaitForSelectorAsync(".balloon");

		Assert.That(tabPage2.Visible, Is.False);
		Assert.That(window.Visible, Is.True);
		Assert.That(window.GetAnchorControlForTest(), Is.EqualTo(tabPage2));
		Assert.That(window.GetVisibleAnchorControlForTest, Is.EqualTo(form));

		Assert.That(async () => await balloon.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('margin-top')"), Is.EqualTo("0px"));
		Assert.That(async () => await balloon.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('margin-right')"), Is.EqualTo("2px"));
		Assert.That(async () => await balloon.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('margin-bottom')"), Is.EqualTo("0px"));
		Assert.That(async () => await balloon.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('margin-left')"), Is.EqualTo("2px"));
	}

	[Test, WithPlaywrightPage, WithTransaction]
	public async Task HelpBalloonWindowShouldShowOnRadioButton()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		const string testCaption = "Carrier MAWB";
		var page = await ctx.LoadFormAsync(() =>
		{
			EnvProxy.Instance.Registry.TraningModeEnabled = true;
			var radioButton1 = new ZRadioButton() { Width = 266, Height = 18, Top = 8, Left = 32, Text = testCaption };
			radioButton1.CaptionResourceString = Res.GetData("AddJobMawbRangeForm|93c6f5fe-45e2-4675-b8ea-15ee29bbb5d9", testCaption);
			radioButton1.AutoCheck = false;
			radioButton1.Name = "CarrierMAWBRadioButton";
			radioButton1.TabIndex = 1;
			var form = new ZForm();
			form.Controls.Add(radioButton1);
			Balloon.Instance.IsShownDuringTesting = true;
			return form;
		});

		await page.Locator(".radiobutton").ClickAsync();
		Assert.That(async () => await page.Locator(".balloon__caption").TextContentAsync(), Is.EqualTo(testCaption).After(3000, 100));
	}

	[Test]
	public async Task BalloonWindow_MouseOut()
	{
		BalloonWindow window = null;
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(RefCommodityCode);
			var descriptionTextBox = new ZTextBox() { Width = 200, Height = 50, Top = 100, Left = 200 };
			var descriptor = new BalloonDescriptor(descriptionTextBox, "caption", "", new INotification[] {
				new Notification(CargoWise.ComponentModel.NotificationType.Error, "error"),
				new Notification(CargoWise.ComponentModel.NotificationType.Warning, "warn"),
				new Notification(CargoWise.ComponentModel.NotificationType.Information, "info") });
			window = new BalloonWindow() { Descriptor = descriptor };
			form.Controls.Add(descriptionTextBox);
			form.BindingSource.SetBindingMember(descriptionTextBox, "RH_IATACommodityItem");
			form.DataSourceType = typeof(RefCommodityCode);
			var factory = new BusinessObjectFactory();
			var bo = factory.New<RefCommodityCode>();
			bo.RH_IATACommodityItem = "";
			form.SetDataBinding(bo, "");
			window.Show();
			return form;
		});

		var balloon = rendered.Find(".balloon");
		Assert.That(rendered.FindAll(".balloon").Count, Is.GreaterThan(0));

		await balloon.TriggerEventAsync("onmouseout", new EventArgs());
		Assert.That(rendered.FindAll(".balloon").Count, Is.EqualTo(0));
	}

	[Test]
	public async Task BalloonWindow_Click()
	{
		BalloonWindow window = null;
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(RefCommodityCode);
			var descriptionTextBox = new ZTextBox() { Width = 200, Height = 50, Top = 100, Left = 200 };
			var descriptor = new BalloonDescriptor(descriptionTextBox, "caption", "", new INotification[] {
				new Notification(CargoWise.ComponentModel.NotificationType.Error, "error"),
				new Notification(CargoWise.ComponentModel.NotificationType.Warning, "warn"),
				new Notification(CargoWise.ComponentModel.NotificationType.Information, "info") });
			window = new BalloonWindow() { Descriptor = descriptor };
			form.Controls.Add(descriptionTextBox);
			form.BindingSource.SetBindingMember(descriptionTextBox, "RH_IATACommodityItem");
			form.DataSourceType = typeof(RefCommodityCode);
			var factory = new BusinessObjectFactory();
			var bo = factory.New<RefCommodityCode>();
			bo.RH_IATACommodityItem = "";
			form.SetDataBinding(bo, "");
			window.Show();
			return form;
		});

		var balloon = rendered.Find(".balloon");
		Assert.That(rendered.FindAll(".balloon").Count, Is.GreaterThan(0));

		await balloon.TriggerEventAsync("onclick", new WebMouseEventArgs { Button = 1 });
		Assert.That(rendered.FindAll(".balloon").Count, Is.EqualTo(0));
	}

	[Test, WithPlaywrightPage]
	public async Task TestBalloonShouldNotFlickerWhenAlreadyOverAnchor()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var showCount = 0;
		var page = await ctx.LoadFormAsync(() =>
		{
			Balloon.Instance.IsShownDuringTesting = true;
			var form = new Form();
			var icon = new NotificationIcon()
			{
				NotificationType = CargoWise.ComponentModel.NotificationType.Error.EnumValueName,
				OnMouseOver = (sender, _) =>
				{
					var iconRef = (NotificationIcon)sender;
					var iconRectangle = ControlDpiScalingHelper.NewScaledRectangle(iconRef.Bounds.X, iconRef.Bounds.Y, 0, 60, false);
					var caption = "I am a long enough caption";
					var description = "I am a long enough description";
					var descriptor = new BalloonDescriptor(iconRef, iconRectangle, caption, description, new INotification[] { new Notification(CargoWise.EntityFramework.NotificationType.Error, "This is a test error") });
					descriptor.HideWhenMouseOverBalloon = false;

					Balloon.Instance.Show(descriptor);
					++showCount;
				},
				OnMouseOut = (_, _) =>
				{
					Balloon.Instance.Hide();
				},
			};

			form.Controls.Add(icon);
			return form;
		});

		await page.WaitForSelectorAsync(".notification");
		await page.Mouse.MoveAsync(0, 11);
		await Task.Delay(100);
		await page.Mouse.MoveAsync(0, 11);
		await Task.Delay(100);
		Assert.That(showCount, Is.EqualTo(1));
	}

	[Test, WithPlaywrightPage]
	public async Task ExistedBalloonWindowShouldHideAndShowNotificationWhenHoverOverControlWithNotificationIcon()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var textBoxWithOpenBalloonWindow = new ZTextBox() { Width = 200, Height = 50, Top = 300, Left = 200 };
			var descriptor = new BalloonDescriptor(textBoxWithOpenBalloonWindow, "caption", "", null);

			var dropEditWithNotificationIcon = new ZDropEdit();
			var notifications = new NotificationCollection();
			notifications.AddError("Error");
			dropEditWithNotificationIcon.GetExtension<NotificationExtension>().Notifications = notifications;

			var panel = new Panel { Top = 10, Left = 10, Width = 900, Height = 900 };
			panel.Controls.Add(dropEditWithNotificationIcon);
			panel.Controls.Add(textBoxWithOpenBalloonWindow);
			var form = new Form { Width = 1000, Height = 1000 };
			form.Controls.Add(panel);

			Balloon.Instance.IsShownDuringTesting = true;
			Balloon.Instance.Show(descriptor);

			return form;
		});

		var balloonItem = page.Locator(".balloon__caption");
		await Assertions.Expect(balloonItem).ToBeVisibleAsync();
		await page.Locator(".notification").HoverAsync();
		await Assertions.Expect(balloonItem).ToBeHiddenAsync();

		var notificationError = page.Locator(".notification");
		await Assertions.Expect(notificationError).ToBeVisibleAsync();
	}

	[Test, WithPlaywrightPage]
	public async Task TestBalloonTransitDoesNotCauseCrash()
	{
		// Quickly transiting through the error button until this work item triggers a crash.
		// That is, a hover event so quick that it didn't have time to render the balloon will crash the UI.

		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			Balloon.Instance.IsShownDuringTesting = true;
			var form = new Form();
			var icon = new NotificationIcon()
			{
				NotificationType = CargoWise.ComponentModel.NotificationType.Error.EnumValueName,
				OnMouseOver = (sender, _) =>
				{
					var iconRef = (NotificationIcon)sender;
					var iconRectangle = ControlDpiScalingHelper.NewScaledRectangle(iconRef.Bounds.X, iconRef.Bounds.Y, 0, 60, false);
					var caption = "I am a long enough caption";
					var description = "I am a long enough description";
					var descriptor = new BalloonDescriptor(iconRef, iconRectangle, caption, description, new INotification[] { new Notification(CargoWise.EntityFramework.NotificationType.Error, "This is a test error") });
					descriptor.HideWhenMouseOverBalloon = false;

					Balloon.Instance.Show(descriptor);
				},
				OnMouseOut = (_, _) =>
				{
					Balloon.Instance.Hide();
				},
			};

			form.Controls.Add(icon);
			return form;
		});

		await page.WaitForSelectorAsync(".notification");

		await page.Mouse.MoveAsync(0, 11);
		await Task.Delay(1);
		await page.Mouse.MoveAsync(0, 110);
		await Task.Delay(1);

		try
		{
			var consoleMessage = await page.WaitForConsoleMessageAsync(new PageWaitForConsoleMessageOptions { Timeout = 100 });
			Assert.That(consoleMessage.Text, Does.Not.Contain("Cannot read properties of null (reading 'querySelector')"));
		}
		catch (TimeoutException)
		{
		}
	}

	[Test, WithPlaywrightPage]
	public async Task TestBalloon_ShouldRenderTextAlignmentToLeft()
	{
		BalloonWindow window = null;
		await using var ctx = new InMemoryAppServerTestContext();
		WinzorTestForm form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(RefCommodityCode);
			var descriptionTextBox = new ZTextBox() { Width = 200, Height = 50, Top = 100, Left = 200 };
			var descriptor = new BalloonDescriptor(descriptionTextBox, "caption", "description", new INotification[] {
				new Notification(CargoWise.ComponentModel.NotificationType.Warning, "warn")
			});
			window = new BalloonWindow() { Descriptor = descriptor };
			form.Controls.Add(descriptionTextBox);
			form.BindingSource.SetBindingMember(descriptionTextBox, "RH_IATACommodityItem");
			form.DataSourceType = typeof(RefCommodityCode);
			var factory = new BusinessObjectFactory();
			var bo = factory.New<RefCommodityCode>();
			bo.RH_IATACommodityItem = "";
			form.SetDataBinding(bo, "");
			window.Show();
			return form;
		});

		var balloonOverlay = await page.WaitForSelectorAsync(".balloon__items");
		Assert.That(async () => await balloonOverlay.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('text-align')"), Is.EqualTo("left"));

		var balloonOverlayCaption = await page.WaitForSelectorAsync(".balloon__caption");
		Assert.That(async () => await balloonOverlayCaption.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('text-align')"), Is.EqualTo("left"));

		var balloonOverlayDescription = await page.WaitForSelectorAsync(".balloon__description");
		Assert.That(async () => await balloonOverlayDescription.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('text-align')"), Is.EqualTo("left"));

		var balloonOverlayNotification = await page.WaitForSelectorAsync(".balloon__items li");
		Assert.That(async () => await balloonOverlayNotification.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('text-align')"), Is.EqualTo("left"));
	}

	[Test, WithPlaywrightPage]
	public async Task TestBalloonWindowRendersInPortal()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var doesRenderInPortal = false;
		var page = await ctx.LoadFormAsync(() =>
		{
			Balloon.Instance.IsShownDuringTesting = true;
			var form = new Form();
			var icon = new NotificationIcon()
			{
				NotificationType = CargoWise.ComponentModel.NotificationType.Error.EnumValueName,
				OnMouseOver = (sender, _) =>
				{
					var iconRef = (NotificationIcon)sender;
					var iconRectangle = ControlDpiScalingHelper.NewScaledRectangle(iconRef.Bounds.X, iconRef.Bounds.Y, 0, 60, false);
					var caption = "I am a caption";
					var description = "I am a description";
					var descriptor = new BalloonDescriptor(iconRef, iconRectangle, caption, description, new INotification[] { new Notification(CargoWise.EntityFramework.NotificationType.Error, "This is a test error") });
					descriptor.HideWhenMouseOverBalloon = false;

					Balloon.Instance.Show(descriptor);
					doesRenderInPortal = ((BalloonWindow)Balloon.Instance.BalloonWindowExposedForTesting).RenderInPortal;
				},
				OnMouseOut = (_, _) =>
				{
					Balloon.Instance.Hide();
				},
			};

			form.Controls.Add(icon);
			return form;
		});

		await page.WaitForSelectorAsync(".notification");
		await page.Mouse.MoveAsync(0, 11);
		await Task.Delay(100);
		await page.Mouse.MoveAsync(0, 11);
		await Task.Delay(100);
		Assert.That(doesRenderInPortal, Is.True);
	}

	[Test]
	public async Task TestBalloonWindowRendersInPopup()
	{
		BalloonWindow window = null;
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(RefCommodityCode);
			var descriptionTextBox = new ZTextBox() { Width = 200, Height = 50, Top = 100, Left = 200 };
			var descriptor = new BalloonDescriptor(descriptionTextBox, "caption", "", new INotification[] {
				new Notification(CargoWise.ComponentModel.NotificationType.Error, "error"),
				new Notification(CargoWise.ComponentModel.NotificationType.Warning, "warn"),
				new Notification(CargoWise.ComponentModel.NotificationType.Information, "info") });
			window = new BalloonWindow() { Descriptor = descriptor };
			form.Controls.Add(descriptionTextBox);
			form.BindingSource.SetBindingMember(descriptionTextBox, "RH_IATACommodityItem");
			form.DataSourceType = typeof(RefCommodityCode);
			var factory = new BusinessObjectFactory();
			var bo = factory.New<RefCommodityCode>();
			bo.RH_IATACommodityItem = "";
			form.SetDataBinding(bo, "");
			window.Show();
			return form;
		});
		rendered.WaitForState(() => rendered.FindAll(".balloon").Count == 1);

		var popup = rendered.FindAll(".balloon").Single().ParentElement;
		var input = rendered.FindAll("input").Single();

		Assert.That(popup.Attributes["class"].Value, Does.Contain("popup"));
		ctx.JSInterop.VerifyInvoke("attachPopup").Arguments[1].ShouldBeElementReferenceTo(input);
	}

	[Test, WithPlaywrightPage]
	public async Task TestBalloonWindowRendersWithinScreen()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			Balloon.Instance.IsShownDuringTesting = true;
			var form = new Form();
			var icon = new NotificationIcon()
			{
				NotificationType = CargoWise.ComponentModel.NotificationType.Error.EnumValueName,
				OnMouseOver = (sender, _) =>
				{
					var iconRef = (NotificationIcon)sender;
					Assert.That(iconRef, Is.Not.Null);
					var iconRectangle = ControlDpiScalingHelper.NewScaledRectangle(iconRef.Bounds.X, iconRef.Bounds.Y, 0, 60, false);
					var caption = "I am a caption";
					var description = "I am a description";
					var descriptor = new BalloonDescriptor(iconRef, iconRectangle, caption, description, new INotification[] { new Notification(CargoWise.EntityFramework.NotificationType.Error, "This is a test error") });
					descriptor.HideWhenMouseOverBalloon = false;
					Balloon.Instance.Show(descriptor);
				},
				OnMouseOut = (_, _) =>
				{
					Balloon.Instance.Hide();
				},
			};

			form.Controls.Add(icon);
			return form;
		});

		await page.WaitForSelectorAsync(".notification");
		await page.Mouse.MoveAsync(0, 11);
		await Task.Delay(100);

		Assert.That(await page.EvaluateAsync<bool>(@"async () => {
					sleep = (ms) => {
					  return new Promise(resolve => setTimeout(resolve, ms));
					}
					const balloon = document.querySelector('.balloon');
					const rect = balloon.getBoundingClientRect();
					await sleep(100);
					const isInViewPort = rect.top >= 0 &&
							rect.left >= 0 &&
							rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
							rect.right <= (window.innerWidth || document.documentElement.clientWidth);
					return isInViewPort;
				}
			"), Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task TestBalloon_ShouldRenderTextColorBasedOnNotificationType()
	{
		BalloonWindow window = null;
		await using var ctx = new InMemoryAppServerTestContext();
		WinzorTestForm form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(RefCommodityCode);
			var descriptionTextBox = new ZTextBox() { Width = 200, Height = 50, Top = 100, Left = 200 };
			var descriptor = new BalloonDescriptor(descriptionTextBox, "caption", "", new INotification[] {
				new Notification(CargoWise.ComponentModel.NotificationType.Warning, "warn"),
				new Notification(CargoWise.ComponentModel.NotificationType.Error, "error"),
				new Notification(CargoWise.ComponentModel.NotificationType.Information, "info")
			});
			window = new BalloonWindow() { Descriptor = descriptor };
			form.Controls.Add(descriptionTextBox);
			form.BindingSource.SetBindingMember(descriptionTextBox, "RH_IATACommodityItem");
			form.DataSourceType = typeof(RefCommodityCode);
			var factory = new BusinessObjectFactory();
			var bo = factory.New<RefCommodityCode>();
			bo.RH_IATACommodityItem = "";
			form.SetDataBinding(bo, "");
			window.Show();
			return form;
		});

		var balloonOverlayWarning = await page.WaitForSelectorAsync(".balloon__items li:nth-child(1)");
		Assert.That(async () => await balloonOverlayWarning.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('color')"), Is.EqualTo("rgb(128, 128, 0)"));
		var balloonOverlayError = await page.WaitForSelectorAsync(".balloon__items li:nth-child(2)");
		Assert.That(async () => await balloonOverlayError.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('color')"), Is.EqualTo("rgb(255, 0, 0)"));
		var balloonOverlayInformation = await page.WaitForSelectorAsync(".balloon__items li:nth-child(3)");
		Assert.That(async () => await balloonOverlayInformation.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('color')"), Is.EqualTo("rgb(0, 0, 255)"));
	}

	[Test, WithPlaywrightPage]
	public async Task TestBalloonWindowHasLowerZIndexThanZDropForm()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			Balloon.Instance.IsShownDuringTesting = true;
			var form = new Form();
			var icon = new NotificationIcon()
			{
				NotificationType = CargoWise.ComponentModel.NotificationType.Error.EnumValueName,
				OnMouseOver = (sender, _) =>
				{
					var iconRef = (NotificationIcon)sender;
					var iconRectangle = ControlDpiScalingHelper.NewScaledRectangle(iconRef.Bounds.X, iconRef.Bounds.Y, 0, 60, false);
					var caption = "I am a caption";
					var description = "I am a description";
					var descriptor = new BalloonDescriptor(iconRef, iconRectangle, caption, description, new INotification[] { new Notification(CargoWise.EntityFramework.NotificationType.Error, "This is a test error") });
					descriptor.HideWhenMouseOverBalloon = false;

					Balloon.Instance.Show(descriptor);
				},
				OnMouseOut = (_, _) =>
				{
					Balloon.Instance.Hide();
				},
			};
			form.Controls.Add(icon);
			return form;
		});

		var zIndexOfBalloonWindow = 1.0;
		var zIndexOfDropForm = 0.0;

		await page.Locator(".notification").HoverAsync();
		await Task.Delay(100);
		var balloonWindowPopup = await page.WaitForSelectorAsync(".popup");
		await Task.Delay(100);
		zIndexOfBalloonWindow = await balloonWindowPopup.EvaluateAsync<float>("e => e.parentElement.style['zIndex']");
		await Task.Delay(100);

		zIndexOfDropForm = await page.EvaluateAsync<float>(@"() => {
				function getStyleSheetPropertyValue(selectorText, propertyName) {
					for (var s= document.styleSheets.length - 1; s >= 0; s--) {
						var cssRules = document.styleSheets[s].cssRules ||
								document.styleSheets[s].rules || []; // IE support
						for (var c=0; c < cssRules.length; c++) {
							if (cssRules[c].selectorText === selectorText)
								return cssRules[c].style[propertyName];
						}
					}
					return null;
				}
				return getStyleSheetPropertyValue('.zdropform', 'z-index');
			}");
		await Task.Delay(100);
		Assert.That(zIndexOfBalloonWindow, Is.LessThan(zIndexOfDropForm));
	}

	[Test, WithPlaywrightPage]
	[TestCase(300, 300, 100, false, 5)]
	[TestCase(300, 300, 1500, true, 5)]
	[TestCase(394, 80, 30, false, 5)]
	public async Task BalloonShouldHaveArrowPointingToAnchorElement(int formWidth, int formHeight, int notificationIconTopPosition, bool isBalloonExpectedToFlip, int yOffset)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			Balloon.Instance.IsShownDuringTesting = true;
			var form = new Form() { Width = formWidth, Height = formHeight };
			var icon = new NotificationIcon()
			{
				NotificationType = CargoWise.ComponentModel.NotificationType.Error.EnumValueName,
				OnMouseOver = (sender, _) =>
				{
					var iconRef = (NotificationIcon)sender;
					var iconRectangle = ControlDpiScalingHelper.NewScaledRectangle(iconRef.Bounds.X, iconRef.Bounds.Y, 0, 60, false);
					var caption = "I am a caption";
					var description = "I am a description";
					var descriptor = new BalloonDescriptor(iconRef, iconRectangle, caption, description, new INotification[] { new Notification(CargoWise.EntityFramework.NotificationType.Error, "This is a test error") });
					descriptor.HideWhenMouseOverBalloon = false;
					Balloon.Instance.Show(descriptor);
				},
				Top = notificationIconTopPosition,
				Left = 100,
				Width = 12,
				Height = 12,
				BackColor = Color.Red
			};
			form.Controls.Add(icon);
			return form;
		});

		await page.Locator(".notification").HoverAsync();
		await Task.Delay(100);

		var icon = await page.Locator(".notification").BoundingBoxAsync();
		var arrow = await page.Locator(".arrow").BoundingBoxAsync();

		if (isBalloonExpectedToFlip)
		{
			Assert.That(arrow.X + arrow.Width, Is.InRange(icon.X, icon.X + icon.Width));
			Assert.That(arrow.Y + arrow.Height, Is.InRange(icon.Y - yOffset, icon.Y + icon.Height));
		}
		else
		{
			Assert.That(arrow.X, Is.InRange(icon.X, icon.X + icon.Width));
			Assert.That(arrow.Y, Is.InRange(icon.Y, (icon.Y + icon.Height + yOffset)));
		}
	}

	[Test, WithPlaywrightPage]
	public async Task BalloonShouldAvoidOtherPopup()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			Balloon.Instance.IsShownDuringTesting = true;
			var form = new WinzorTestForm() { Width = 500, Height = 300 };
			var icon = new NotificationIcon()
			{
				NotificationType = NotificationType.Error.EnumValueName,
				OnMouseOver = (sender, _) =>
				{
					var iconRef = (NotificationIcon)sender;
					var iconRectangle = ControlDpiScalingHelper.NewScaledRectangle(iconRef.Bounds.X, iconRef.Bounds.Y, 0, 60, false);
					var caption = "I am a caption";
					var description = "I am a description";
					var descriptor = new BalloonDescriptor(iconRef, iconRectangle, caption, description, new INotification[] { new Notification(NotificationType.Error, "This is a test error") });
					descriptor.HideWhenMouseOverBalloon = false;
					Balloon.Instance.Show(descriptor);
				},
				Top = 100,
				Left = 100,
				Width = 12,
				Height = 12,
				BackColor = Color.Red
			};
			form.Controls.Add(icon);

			form.BindingSource.DataSourceType = typeof(DummyBusinessObjectWithList);
			var dropEdit = new ZDropEdit();
			dropEdit.BindToList = "List";
			form.Controls.Add(dropEdit);
			form.BindingSource.SetBindingMember(dropEdit, "Z0_Code");

			var dropEdit2 = new ZDropEdit() { Top = 120 };
			dropEdit2.BindToList = "List";
			form.Controls.Add(dropEdit2);
			form.BindingSource.SetBindingMember(dropEdit2, "Z0_Code");

			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObjectWithList>();
			for (var i = 0; i < 5; i++)
			{
				var a = dummyBizo.List.AddNew();
				a.Z0_Code = i.ToString();
				a.Z0_Description = "Description " + i;
			}
			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		var buttons = await page.QuerySelectorAllAsync(".zdropbutton button");
		await buttons[0].ClickAsync();
		await Task.Delay(100);
		await page.Locator(".notification").HoverAsync();
		await AssertBalloonFlip(page, false);

		await page.Locator(".balloon").ClickAsync();
		await buttons[1].ClickAsync();
		await Task.Delay(100);
		await page.Locator(".notification").HoverAsync();
		await AssertBalloonFlip(page, true);
	}

	async Task AssertBalloonFlip(IPage page, bool top)
	{
		var yOffset = 5;
		var icon = await page.Locator(".notification").BoundingBoxAsync();
		var arrow = await page.Locator(".arrow").BoundingBoxAsync();
		if (top)
		{
			Assert.That(arrow.X + arrow.Width, Is.InRange(icon.X, icon.X + icon.Width));
			Assert.That(arrow.Y + arrow.Height, Is.InRange(icon.Y - yOffset, icon.Y + icon.Height));
		}
		else
		{
			Assert.That(arrow.X, Is.InRange(icon.X, icon.X + icon.Width));
			Assert.That(arrow.Y, Is.InRange(icon.Y, (icon.Y + icon.Height + yOffset)));
		}
	}

	[Test, WithPlaywrightPage]
	public async Task BalloonWindowShouldPointToAssociatedTextBox()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		BalloonWindow window_1 = null;
		BalloonWindow window_2 = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var descriptionTextBox_1 = new ZTextBox() { Width = 200, Height = 50, Top = 50, Left = 200 };
			var descriptionTextBox_2 = new ZTextBox() { Width = 200, Height = 50, Top = 150, Left = 200 };
			var descriptor_1 = new BalloonDescriptor(descriptionTextBox_1, "caption", "", new INotification[] {
				new Notification(CargoWise.ComponentModel.NotificationType.Error, "error"),
				});
			var descriptor_2 = new BalloonDescriptor(descriptionTextBox_2, "caption", "", new INotification[] {
				new Notification(CargoWise.ComponentModel.NotificationType.Error, "error"),
				});
			window_1 = new BalloonWindow() { Descriptor = descriptor_1 };
			window_2 = new BalloonWindow() { Descriptor = descriptor_2 };
			form.Controls.Add(descriptionTextBox_1);
			form.Controls.Add(descriptionTextBox_2);

			descriptionTextBox_1.Click += (s, e) => { window_1.Show(); };
			descriptionTextBox_2.Click += (s, e) => { window_1.Hide(); window_2.Show(); };
			return form;
		});

		await (await page.WaitForSelectorAsync(".form > .textbox:nth-child(1)")).ClickAsync();
		var arrowRect_1 = await page.Locator(".arrow").BoundingBoxAsync();

		await (await page.WaitForSelectorAsync(".form > .textbox:nth-child(2)")).ClickAsync();
		var arrowRect_2 = await page.Locator(".arrow").BoundingBoxAsync();

		Assert.That(arrowRect_1, Is.Not.EqualTo(arrowRect_2));
	}

	[Test, WithPlaywrightPage]
	public async Task TestBalloon_ListItemShouldHaveWhiteSpacePreLine()
	{
		BalloonWindow window = null;
		await using var ctx = new InMemoryAppServerTestContext();
		WinzorTestForm form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(RefCommodityCode);
			var descriptionTextBox = new ZTextBox() { Width = 200, Height = 50, Top = 100, Left = 200 };
			var descriptor = new BalloonDescriptor(descriptionTextBox, "caption", "description", new INotification[] {
				new Notification(CargoWise.ComponentModel.NotificationType.Error, "ERROR: The phone number as entered has a high probability of being incorrect.\r\n \r\n No country/region identified to format the number. Please enter the number in an international format. See below example: \r\n \r\n +61 123 123 123")
			});
			window = new BalloonWindow() { Descriptor = descriptor };
			form.Controls.Add(descriptionTextBox);
			form.BindingSource.SetBindingMember(descriptionTextBox, "RH_IATACommodityItem");
			form.DataSourceType = typeof(RefCommodityCode);
			var factory = new BusinessObjectFactory();
			var bo = factory.New<RefCommodityCode>();
			bo.RH_IATACommodityItem = "";
			form.SetDataBinding(bo, "");
			window.Show();
			return form;
		});

		var balloonOverlay = await page.WaitForSelectorAsync(".balloon li");
		Assert.That(async () => await balloonOverlay.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('white-space')"), Is.EqualTo("pre-line"));
	}

	[Test, WithPlaywrightPage]
	public async Task TestBalloon_ListItemShouldHaveCorrectHeight()
	{
		BalloonWindow window = null;
		BalloonWindow windowDefault = null;
		await using var ctx = new InMemoryAppServerTestContext();
		WinzorTestForm form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(RefCommodityCode);

			var descriptionTextBoxDefault = new ZTextBox() { Width = 200, Height = 50, Top = 100, Left = 200 };
			var descriptorDefault = new BalloonDescriptor(descriptionTextBoxDefault, "caption", "description", new INotification[] {
				new Notification(CargoWise.ComponentModel.NotificationType.Error, "ERROR: single line")
			});
			windowDefault = new BalloonWindow() { Descriptor = descriptorDefault };

			var descriptionTextBox = new ZTextBox() { Width = 200, Height = 50, Top = 500, Left = 200 };
			var descriptor = new BalloonDescriptor(descriptionTextBox, "caption", "description", new INotification[] {
				new Notification(CargoWise.ComponentModel.NotificationType.Error, "ERROR: The phone number as entered has a high probability of being incorrect.\r\n \r\n No country/region identified to format the number. Please enter the number in an international format. See below example: \r\n \r\n +61 123 123 123")
			});
			window = new BalloonWindow() { Descriptor = descriptor };

			form.Controls.Add(descriptionTextBox);
			form.BindingSource.SetBindingMember(descriptionTextBox, "RH_IATACommodityItem");

			form.Controls.Add(descriptionTextBoxDefault);
			form.BindingSource.SetBindingMember(descriptionTextBoxDefault, "RH_IATACommodityItem");

			form.DataSourceType = typeof(RefCommodityCode);
			var factory = new BusinessObjectFactory();
			var bo = factory.New<RefCommodityCode>();
			bo.RH_IATACommodityItem = "";
			form.SetDataBinding(bo, "");
			window.Show();
			windowDefault.Show();
			return form;
		});

		var balloonOverlay = await page.WaitForSelectorAsync(".balloon li");

		var overlay = await page.EvaluateAsync<string>("document.querySelectorAll('.balloon')[0].offsetHeight");
		var overlayDefault = await page.EvaluateAsync<string>("document.querySelectorAll('.balloon')[1].offsetHeight");

		Assert.That(int.Parse(overlay), Is.GreaterThan(int.Parse(overlayDefault)));
	}

	[Test, WithPlaywrightPage]
	public async Task TestBalloon_NotificationIconIsCorrectlyPositioned()
	{
		BalloonWindow window = null;
		await using var ctx = new InMemoryAppServerTestContext();
		WinzorTestForm form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(RefCommodityCode);
			var descriptionTextBox = new ZTextBox() { Width = 200, Height = 50, Top = 100, Left = 200 };
			var descriptor = new BalloonDescriptor(descriptionTextBox, "caption", "description", new INotification[] {
				new Notification(CargoWise.ComponentModel.NotificationType.Error, "ERROR: The phone number as entered has a high probability of being incorrect.\r\n \r\n No country/region identified to format the number. Please enter the number in an international format. See below example: \r\n \r\n +61 123 123 123")
			});
			window = new BalloonWindow() { Descriptor = descriptor };
			form.Controls.Add(descriptionTextBox);
			form.BindingSource.SetBindingMember(descriptionTextBox, "RH_IATACommodityItem");
			form.DataSourceType = typeof(RefCommodityCode);
			var factory = new BusinessObjectFactory();
			var bo = factory.New<RefCommodityCode>();
			bo.RH_IATACommodityItem = "";
			form.SetDataBinding(bo, "");
			window.Show();
			return form;
		});

		var balloonOverlay = await page.WaitForSelectorAsync(".balloon .balloon__item--error");
		Assert.That(async () => await balloonOverlay.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-position')"), Is.EqualTo("0% 0%"));
	}

	[Test, WithPlaywrightPage]
	public async Task TestBalloon_RemoveNotificationIconShouldHidePopup()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZCheckBox checkBox = null;
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			checkBox = new ZCheckBox() { Width = 200, Height = 50, Top = 300, Left = 200 };
			checkBox.Text = "Balloon Should Close When Hover On This";

			var notifications = new NotificationCollection();
			notifications.AddError("Err");
			checkBox.GetExtension<NotificationExtension>().Notifications = notifications;

			var panel = new Panel { Top = 10, Left = 10, Width = 900, Height = 900 };
			panel.Controls.Add(checkBox);

			form = new Form { Width = 1000, Height = 1000 };
			form.Controls.Add(panel);

			Balloon.Instance.IsShownDuringTesting = true;
			return form;
		});

		await page.Locator(".notification").HoverAsync();

		var balloon = page.Locator(".balloon");
		await Assertions.Expect(balloon).ToHaveCountAsync(1);

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			checkBox.GetExtension<NotificationExtension>().Notifications = NotificationCollection.Empty;
		});

		await Assertions.Expect(balloon).ToHaveCountAsync(0);
	}

	[Test, WithSnapshotProtection, WithPlaywrightPage]
	public async Task BalloonShouldHideWhenHoverOverTextBoxWithNotificationIcon()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		DummyBusinessObject bo = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm() { Width = 1000, Height = 1000 };
			var factory = new BusinessObjectFactory();
			bo = factory.New<DummyBusinessObject>();
			form.SetDataBinding(bo, "");

			var dateEdit = new ZDateEdit { Location = new Point(200, 200), BindTo = DummyBusinessObject.Schema.Z0_AnotherDate };
			form.Controls.Add(dateEdit);

			var textBoxWithNotificationIcon = new ZTextBox() { Width = 200, Height = 50, Top = 300, Left = 200 };
			var icon = new NotificationIcon() { NotificationType = NotificationType.Error.EnumValueName, NotificationAnchorControl = textBoxWithNotificationIcon };
			form.Controls.Add(textBoxWithNotificationIcon);

			Balloon.Instance.IsShownDuringTesting = true;
			return form;
		});

		var oldTrainingModeEnabledSetting = EnvProxy.Instance.Registry.TraningModeEnabled;
		EnvProxy.Instance.Registry.TraningModeEnabled = true;

		var textbox = await page.WaitForSelectorAsync(".textbox");
		await textbox.FillAsync("1");
		await textbox.ClickAsync();
		await page.Mouse.ClickAsync(0, 0);

		var iconElement = await page.WaitForSelectorAsync(".notification");
		var iconBoundingBox = await iconElement.BoundingBoxAsync();

		await page.Locator(".notification").HoverAsync();
		var dateTimeBalloon = await page.WaitForSelectorAsync(".balloon");
		var balloonBoxOriginal = await dateTimeBalloon.BoundingBoxAsync();

		await page.Locator(".textbox").Nth(1).HoverAsync();
		await Task.Delay(100);
		Assert.That(await dateTimeBalloon.IsHiddenAsync(), Is.True);
		EnvProxy.Instance.Registry.TraningModeEnabled = oldTrainingModeEnabledSetting;
	}

	[WithPlaywrightPage]
	[TestCase(20)]
	[TestCase(25)]
	[TestCase(30)]
	public async Task BalloonWindowShouldRenderWithoutExceptionIfHiddenInAnotherThread(int delayPercentage)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		BalloonDescriptor descriptor1 = null;
		BalloonWindow window1 = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var textBox1 = new ZTextBox() { Width = 200, Height = 50, Top = 300, Left = 200 };
			descriptor1 = new BalloonDescriptor(textBox1, "caption1", "",
				new INotification[]
				{
					new Notification(CargoWise.ComponentModel.NotificationType.Error, "error1"),
				});
			var textBox2 = new ZTextBox() { Width = 200, Height = 50, Top = 400, Left = 200 };
			var descriptor2 = new BalloonDescriptor(textBox2, "caption2", "",
				new INotification[]
				{
					new Notification(CargoWise.ComponentModel.NotificationType.Information, "information2"),
				});
			window1 = new BalloonWindow() { Descriptor = descriptor1 };
			var window2 = new BalloonWindow() { Descriptor = descriptor2 };

			textBox1.Click += (sender, e) =>
			{
				window1.Show();
				window2.Hide();
			};
			textBox2.Click += (sender, e) =>
			{
				window1.Hide();
				window2.Show();
			};

			var form = new Form { Width = 1000, Height = 1000 };
			form.Controls.Add(textBox1);
			form.Controls.Add(textBox2);

			Balloon.Instance.IsShownDuringTesting = true;

			return form;
		});

		var visibleLocatorOption = new LocatorWaitForOptions
		{
			State = WaitForSelectorState.Visible,
			Timeout = 500
		};

		var textBox1 = page.Locator(".form > .textbox:nth-child(1)");
		var textBox2 = page.Locator(".form > .textbox:nth-child(2)");

		await textBox1.WaitForAsync(visibleLocatorOption);
		await textBox2.WaitForAsync(visibleLocatorOption);

		var balloonRenderTimer = new Stopwatch();
		balloonRenderTimer.Start();
		await textBox1.ClickAsync();
		await page.Locator(".balloon").WaitForAsync(visibleLocatorOption);
		balloonRenderTimer.Stop();
		var delayTime = balloonRenderTimer.Elapsed * delayPercentage / 100;

		const int iterations = 100;
		foreach (var index in Enumerable.Range(0, iterations))
		{
			await ctx.WinzorDispatcher.InvokeAsync(() => window1.Descriptor = descriptor1);
			_ = textBox1.ClickAsync();
			await Task.Delay(delayTime);
			await ctx.WinzorDispatcher.InvokeAsync(() => window1.Descriptor = null);

			await textBox2.ClickAsync();
			await page.Locator(".balloon").WaitForAsync(visibleLocatorOption);

			Assert.That(PageErrors.Count, Is.Zero);
		}
	}

	[Test, WithSnapshotProtection, WithPlaywrightPage]
	public async Task BalloonShouldHideWhenHoverOverCheckBoxLabelWithNotificationIcon()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		DummyBusinessObject bo = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm() { Width = 1000, Height = 1000 };
			var factory = new BusinessObjectFactory();
			bo = factory.New<DummyBusinessObject>();
			form.SetDataBinding(bo, "");

			var dateEdit = new ZDateEdit { Location = new Point(200, 200), BindTo = DummyBusinessObject.Schema.Z0_AnotherDate };
			form.Controls.Add(dateEdit);

			var checkBox = new ZCheckBox() { Width = 200, Height = 50, Top = 300, Left = 200 };
			checkBox.Text = "Balloon Should Close When Hover On This";

			var notifications = new NotificationCollection();
			notifications.AddError("Err");
			checkBox.GetExtension<NotificationExtension>().Notifications = notifications;
			form.Controls.Add(checkBox);

			Balloon.Instance.IsShownDuringTesting = true;
			return form;
		});

		var oldTrainingModeEnabledSetting = EnvProxy.Instance.Registry.TraningModeEnabled;
		EnvProxy.Instance.Registry.TraningModeEnabled = true;

		var textbox = page.GetByRole(AriaRole.Textbox);
		await textbox.FillAsync("1");
		await textbox.ClickAsync();
		await page.Mouse.ClickAsync(0, 0);

		var iconElement = await page.WaitForSelectorAsync(".notification");
		var iconBoundingBox = await iconElement.BoundingBoxAsync();

		await page.Locator(".notification").First.HoverAsync();

		var dateTimeBalloon = await page.WaitForSelectorAsync(".balloon");
		var balloonBoxOriginal = await dateTimeBalloon.BoundingBoxAsync();

		await page.Locator(".checkbox__label").First.HoverAsync();

		await Task.Delay(100);
		Assert.That(await dateTimeBalloon.IsHiddenAsync(), Is.True);

		EnvProxy.Instance.Registry.TraningModeEnabled = oldTrainingModeEnabledSetting;
	}

	[Test, WithPlaywrightPage]
	public async Task BalloonDescriptionRespectsNewLines()
	{
		// Arrange
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var descriptionTextBox = new ZTextBox() { Width = 200, Height = 50, Top = 100, Left = 200 };
			var descriptor = new BalloonDescriptor(descriptionTextBox, "", "description", Array.Empty<INotification>());
			var window = new BalloonWindow() { Descriptor = descriptor };
			descriptionTextBox.Click += (s, e) =>
			{
				window.Descriptor = new BalloonDescriptor(descriptionTextBox, "non-empty", "multi\nline\ndesc", Array.Empty<INotification>());
				window.Hide();
				window.Show();
			};
			form.Controls.Add(descriptionTextBox);
			window.Show();
			return form;
		});

		await page.WaitForSelectorAsync(".balloon");

		var initialHeight = (await page.Locator(".balloon__description").BoundingBoxAsync()).Height;

		// Act
		await (await page.WaitForSelectorAsync(".textbox")).ClickAsync();
		await page.WaitForSelectorAsync(".balloon__caption:not(:empty)");

		// Assert
		Assert.That(
			(await page.Locator(".balloon__description").BoundingBoxAsync()).Height,
			Is.GreaterThan(initialHeight),
			"Description containing new-lines should render on multiple lines and hence a greater height");
	}

	[Test, WithPlaywrightPage]
	[TestCase(394, 80, "4px", "5px", "2px", "5px")]
	[TestCase(400, 300, "8px", "20px", "2px", "10px")]
	public async Task BalloonPaddingShouldFactorFormSize(int formWidth, int formHeight, string expectedTopPadding, string expectedRightPadding, string expectedBottomPadding, string expectedLeftPadding)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var dateEdit = new ZDateEdit() { Width = 85, Height = 20, Top = 8, Left = 280, BindTo = DummyBusinessObject.Schema.Z0_AnotherDate };

			var icon = new NotificationIcon()
			{
				NotificationType = CargoWise.ComponentModel.NotificationType.Error.EnumValueName,
				OnMouseOver = (sender, _) =>
				{
					var iconRef = (NotificationIcon)sender;
					var iconRectangle = ControlDpiScalingHelper.NewScaledRectangle(12, 12, 0, 0, false);
					var caption = "Please enter the date the staff member was reviewed";

					var descriptor = new BalloonDescriptor(iconRef, iconRectangle, caption, string.Empty,
						new INotification[]
						{
							new Notification(CargoWise.EntityFramework.NotificationType.Error,
								"Please enter a Please enter the date the staff member was reviewed")
						});
					descriptor.HideWhenMouseOverBalloon = false;
					Balloon.Instance.Show(descriptor);
				},
				Top = 30,
				Left = 282,
				Height = 12,
				Width = 12,
				BackColor = Color.Brown,
			};

			dateEdit.Controls.Add(icon);
			var form = new Form { Width = formWidth, Height = formHeight };
			form.Controls.Add(dateEdit);
			Balloon.Instance.IsShownDuringTesting = true;
			return form;
		});

		await page.Locator(".notification").HoverAsync();
		await Task.Delay(500);
		var balloon = await page.WaitForSelectorAsync(".balloon");

		var paddingTop = (await balloon.EvaluateAsync($"e => window.getComputedStyle(e).getPropertyValue('padding-top')")).Value.ToString();
		var paddingRight = (await balloon.EvaluateAsync($"e => window.getComputedStyle(e).getPropertyValue('padding-right')")).Value.ToString();
		var paddingBottom = (await balloon.EvaluateAsync($"e => window.getComputedStyle(e).getPropertyValue('padding-bottom')")).Value.ToString();
		var paddingLeft = (await balloon.EvaluateAsync($"e => window.getComputedStyle(e).getPropertyValue('padding-left')")).Value.ToString();

		Assert.That(paddingTop, Is.EqualTo(expectedTopPadding));
		Assert.That(paddingRight, Is.EqualTo(expectedRightPadding));
		Assert.That(paddingBottom, Is.EqualTo(expectedBottomPadding));
		Assert.That(paddingLeft, Is.EqualTo(expectedLeftPadding));
	}

	[Test, WithPlaywrightPage]
	public async Task TestBalloonListItemShouldHaveBottomMarginForAllElementsExceptLast()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var descriptionTextBox = new ZTextBox() { Width = 200, Height = 50, Top = 100, Left = 200 };
			var descriptor = new BalloonDescriptor(descriptionTextBox, "caption", "", new INotification[] {
				new Notification(CargoWise.ComponentModel.NotificationType.Error, "error1"),
				new Notification(CargoWise.ComponentModel.NotificationType.Error, "error2"),
				new Notification(CargoWise.ComponentModel.NotificationType.Error, "error3")
			});
			var window = new BalloonWindow() { Descriptor = descriptor };
			form.Controls.Add(descriptionTextBox);
			window.Show();
			return form;
		});

		var balloonItem1 = await page.WaitForSelectorAsync(".balloon li:nth-child(1)");
		var balloonItem2 = await page.WaitForSelectorAsync(".balloon li:nth-child(2)");
		var balloonItem3 = await page.WaitForSelectorAsync(".balloon li:nth-child(3)");
		var balloonItem1MarginBottom = await balloonItem1.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('margin-bottom')");
		var balloonItem2MarginBottom = await balloonItem2.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('margin-bottom')");
		var balloonItem3MarginBottom = await balloonItem3.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('margin-bottom')");
		Assert.That(balloonItem1MarginBottom, Is.EqualTo("3px"));
		Assert.That(balloonItem2MarginBottom, Is.EqualTo("3px"));
		Assert.That(balloonItem3MarginBottom, Is.EqualTo("0px"));
	}

	[Test, WithPlaywrightPage]
	public async Task TestBalloonListItemShouldHaveTextAppropriatelySpacedFromIcons()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var descriptionTextBox = new ZTextBox() { Width = 200, Height = 50, Top = 100, Left = 200 };
			var descriptor = new BalloonDescriptor(descriptionTextBox, "caption", "", new INotification[] {
				new Notification(CargoWise.ComponentModel.NotificationType.Error, "error")
			});
			var window = new BalloonWindow() { Descriptor = descriptor };
			form.Controls.Add(descriptionTextBox);
			window.Show();
			return form;
		});

		var balloonItem = await page.WaitForSelectorAsync(".balloon li:nth-child(1)");
		var balloonItemMarginBottom = await balloonItem.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('padding-left')");
		Assert.That(balloonItemMarginBottom, Is.EqualTo("25px"));
	}

	[Test, WithPlaywrightPage, WithTransaction]
	public async Task BalloonsShouldDisplayInCorrectPositionOnEditControlInTasksGrid()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			EnvProxy.Instance.Registry.TraningModeEnabled = true;

			var factory = new BusinessObjectFactory();
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(factory, addDefaultProcessHeaderIfNone: false);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow");
			var otherTask = VisualBoardsTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Working, sequence: 1);
			var taskToChange = VisualBoardsTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Suspended, sequence: 2);
			factory.Save();

			var form = new ZForm() { Height = 600, Width = 1000 };
			var control = new TaskDetailsUserControl();
			control.SetDataBinding(jobHeader, string.Empty);
			form.Controls.Add(control);
			return form;
		});

		var balloonLocator = page.Locator(".balloon__caption");
		await balloonLocator.WaitForAsync();
		var arrow = page.Locator(".arrow");
		await arrow.WaitForAsync();

		// DataGridTextBox
		await page.Locator($"[data-name='TasksGrid'] tbody>tr:nth-of-type({1})>td:nth-child({3})").ClickAsync();
		await Assertions.Expect(balloonLocator).ToHaveTextAsync("Description");
		var arrowBoundBox = await arrow.BoundingBoxAsync();
		Assert.Multiple(() =>
		{
			Assert.That((int)arrowBoundBox.X, Is.EqualTo(186));
			Assert.That((int)arrowBoundBox.Y, Is.EqualTo(35));
		});

		// ZDropEdit
		await page.Locator($"[data-name='TasksGrid'] tbody>tr:nth-of-type({1})>td:nth-child({4})").ClickAsync();
		await Assertions.Expect(balloonLocator).ToHaveTextAsync("Task Type");
		arrowBoundBox = await arrow.BoundingBoxAsync();
		Assert.Multiple(() =>
		{
			Assert.That((int)arrowBoundBox.X, Is.EqualTo(316));
			Assert.That((int)arrowBoundBox.Y, Is.EqualTo(35));
		});

		// ZGridFindBox
		await page.Locator($"[data-name='TasksGrid'] tbody>tr:nth-of-type({1})>td:nth-child({6})").ClickAsync();
		await Assertions.Expect(balloonLocator).ToHaveTextAsync("Task Assigned To");
		arrowBoundBox = await arrow.BoundingBoxAsync();
		Assert.Multiple(() =>
		{
			Assert.That((int)arrowBoundBox.X, Is.EqualTo(407));
			Assert.That((int)arrowBoundBox.Y, Is.EqualTo(35));
		});
	}

	[Test, WithSnapshotProtection, WithPlaywrightPage]
	public async Task TestBalloonLocationFlipsWhenRepositioningDueToCalendarShowingWithTrainingMode()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		DummyBusinessObject bo = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm() { Width = 1000, Height = 1000 };
			var factory = new BusinessObjectFactory();
			bo = factory.New<DummyBusinessObject>();
			form.SetDataBinding(bo, "");

			var dateEdit = new ZDateEdit { Location = new Point(200, 200), BindTo = DummyBusinessObject.Schema.Z0_AnotherDate };
			form.Controls.Add(dateEdit);

			Balloon.Instance.IsShownDuringTesting = true;

			return form;
		});

		var oldTrainingModeEnabledSetting = EnvProxy.Instance.Registry.TraningModeEnabled;
		EnvProxy.Instance.Registry.TraningModeEnabled = true;

		var textbox = page.GetByRole(AriaRole.Textbox);
		await textbox.FillAsync("1");
		await textbox.ClickAsync();
		await page.Mouse.ClickAsync(0, 0);

		var iconElement = await page.WaitForSelectorAsync(".notification");
		var iconBoundingBox = await iconElement.BoundingBoxAsync();

		await page.Locator(".notification").HoverAsync();

		var balloon = await page.WaitForSelectorAsync(".balloon");
		var balloonBoxOriginal = await balloon.BoundingBoxAsync();

		// Clicks the ZDateEdit button which is hidden by the notification icon await page.WaitForSelectorAsync(".button").ClickAsync()
		// gives an playwright log saying that the click has been intercepted
		await page.Mouse.ClickAsync(iconBoundingBox.X, iconBoundingBox.Y);
		await Task.Delay(500);

		balloon = await page.WaitForSelectorAsync(".balloon");
		var balloonBoxFlipped = await balloon.BoundingBoxAsync();

		// The left should not change, the new top should be less than the original as it flips up
		Assert.That(balloonBoxFlipped.Y, Is.LessThan(balloonBoxOriginal.Y));

		EnvProxy.Instance.Registry.TraningModeEnabled = oldTrainingModeEnabledSetting;
	}

	[Test, WithSnapshotProtection, WithPlaywrightPage]
	public async Task TestBalloonTrainingModeShouldHideOnHoverOverDateEditButton()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		DummyBusinessObject bo = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm() { Width = 1000, Height = 1000 };
			var factory = new BusinessObjectFactory();
			bo = factory.New<DummyBusinessObject>();
			form.SetDataBinding(bo, "");

			var dateEdit = new ZDateEdit { Location = new Point(200, 200), BindTo = DummyBusinessObject.Schema.Z0_AnotherDate };
			form.Controls.Add(dateEdit);

			Balloon.Instance.IsShownDuringTesting = true;

			return form;
		});

		var oldTrainingModeEnabledSetting = EnvProxy.Instance.Registry.TraningModeEnabled;
		EnvProxy.Instance.Registry.TraningModeEnabled = true;

		var textbox = page.GetByRole(AriaRole.Textbox);
		await textbox.FillAsync("1");
		await textbox.ClickAsync();
		await page.Mouse.ClickAsync(0, 0);
		await textbox.ClickAsync();

		var iconElement = await page.WaitForSelectorAsync(".notification");
		var iconBoundingBox = await iconElement.BoundingBoxAsync();

		await page.Locator(".notification").HoverAsync();

		var balloon = await page.WaitForSelectorAsync(".balloon");

		Assert.That(await page.Locator(".balloon").CountAsync(), Is.EqualTo(1));

		// Moves to ZDateEdit button
		var buttonElement = await page.WaitForSelectorAsync(".button");
		var buttonBoundingBox = await buttonElement.BoundingBoxAsync();
		await page.Mouse.MoveAsync(buttonBoundingBox.X - buttonBoundingBox.Width, buttonBoundingBox.Y - buttonBoundingBox.Height);
		await Task.Delay(500);

		Assert.That(await page.Locator(".balloon").CountAsync(), Is.EqualTo(0));
		EnvProxy.Instance.Registry.TraningModeEnabled = oldTrainingModeEnabledSetting;
	}

	[Test, WithSnapshotProtection, WithPlaywrightPage]
	public async Task TestBalloonHidesWhenDateEditIsOpenedAndTrainingModeOff()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		DummyBusinessObject bo = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm() { Width = 1000, Height = 1000 };
			var factory = new BusinessObjectFactory();
			bo = factory.New<DummyBusinessObject>();
			form.SetDataBinding(bo, "");

			var dateEdit = new ZDateEdit { Location = new Point(200, 200), BindTo = DummyBusinessObject.Schema.Z0_AnotherDate };
			form.Controls.Add(dateEdit);

			Balloon.Instance.IsShownDuringTesting = true;
			return form;
		});

		var oldTrainingModeEnabledSetting = EnvProxy.Instance.Registry.TraningModeEnabled;
		EnvProxy.Instance.Registry.TraningModeEnabled = false;

		var textbox = page.GetByRole(AriaRole.Textbox);
		await textbox.WaitForAsync();
		await textbox.FillAsync("1");
		await textbox.ClickAsync();
		await page.Mouse.ClickAsync(0, 0);

		var iconElement = page.Locator(".notification");
		await iconElement.WaitForAsync();
		var iconBoundingBox = await iconElement.BoundingBoxAsync();

		await page.Mouse.MoveAsync(iconBoundingBox.X, iconBoundingBox.Y);
		var balloon = page.Locator(".balloon");
		await Assertions.Expect(balloon).ToHaveCountAsync(1);

		// Clicks the ZDateEdit button which is hidden by the notification icon
		// gives an playwright log saying that the click has been intercepted
		await page.Mouse.ClickAsync(iconBoundingBox.X, iconBoundingBox.Y);
		await Assertions.Expect(balloon).ToHaveCountAsync(0);

		EnvProxy.Instance.Registry.TraningModeEnabled = oldTrainingModeEnabledSetting;
	}

	[Test, WithSnapshotProtection, WithPlaywrightPage]
	public async Task TestBalloonRelocatesWhenDateEditIsOpenedAndTrainingModeOn()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		DummyBusinessObject bo = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm() { Width = 1000, Height = 1000 };
			var factory = new BusinessObjectFactory();
			bo = factory.New<DummyBusinessObject>();
			form.SetDataBinding(bo, "");

			var dateEdit = new ZDateEdit { Location = new Point(200, 200), BindTo = DummyBusinessObject.Schema.Z0_AnotherDate };
			form.Controls.Add(dateEdit);

			Balloon.Instance.IsShownDuringTesting = true;

			return form;
		});

		var oldTrainingModeEnabledSetting = EnvProxy.Instance.Registry.TraningModeEnabled;
		EnvProxy.Instance.Registry.TraningModeEnabled = true;

		var textbox = page.GetByRole(AriaRole.Textbox);
		await textbox.FillAsync("1");
		await textbox.ClickAsync();
		await page.Mouse.ClickAsync(0, 0);

		var iconElement = await page.WaitForSelectorAsync(".notification");
		var iconBoundingBox = await iconElement.BoundingBoxAsync();

		await page.Mouse.MoveAsync(iconBoundingBox.X, iconBoundingBox.Y);

		await Task.Delay(500);

		Assert.That(await page.Locator(".balloon").CountAsync(), Is.EqualTo(1));

		// Clicks the ZDateEdit button which is hidden by the notification icon await page.WaitForSelectorAsync(".button").ClickAsync()
		// gives an playwright log saying that the click has been intercepted
		await page.Mouse.ClickAsync(iconBoundingBox.X, iconBoundingBox.Y);
		await Task.Delay(500);

		Assert.That(await page.Locator(".balloon").CountAsync(), Is.EqualTo(1));

		EnvProxy.Instance.Registry.TraningModeEnabled = oldTrainingModeEnabledSetting;
	}

	[Test, WithPlaywrightPage]
	public async Task BalloonContentShouldNotBeHiddenByArrow()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		BalloonWindow balloonWindow = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 500, Height = 500 };
			var textBox = new ZTextBox() { Width = 200, Height = 20, Top = 50, Left = 200 };

			balloonWindow = new BalloonWindow()
			{
				Descriptor = new BalloonDescriptor(textBox, "caption", "",
					new INotification[]
					{
							new Notification(NotificationType.Error, "error"),
					})
			};

			form.Controls.Add(textBox);
			textBox.Click += (s, e) => { balloonWindow.Show(); };
			return form;
		});

		var textBoxStandard = await page.WaitForSelectorAsync(".form > .textbox:nth-child(1)");
		await textBoxStandard.ClickAsync();

		var arrowZIndex = balloonWindow.ZIndex;
		var balloonContent = page.Locator(".balloon__content");
		var contentZIndex = await balloonContent.GetComputedStyleAsync("z-index");
		Assert.That(int.Parse(contentZIndex.Raw), Is.GreaterThan(arrowZIndex));
	}

	[Test, WithPlaywrightPage]
	public async Task BalloonArrowShouldBeWithinRangeOfNotificationIcon()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			Balloon.Instance.IsShownDuringTesting = true;
			var form = new Form { Height = 400, Width = 400 };
			var icon = new NotificationIcon()
			{
				Left = 10,
				Top = 10,
				Height = 12,
				Width = 12,
				BackColor = Color.Brown,
				NotificationType = NotificationType.Error.EnumValueName,
				OnMouseOver = (sender, _) =>
				{
					var iconRef = (NotificationIcon)sender;
					var iconRectangle = ControlDpiScalingHelper.NewScaledRectangle(iconRef.Bounds.X, iconRef.Bounds.Y, 0, 260, false);
					var caption = "I am a caption";
					var description = "I am a description";
					var descriptor = new BalloonDescriptor(iconRef, iconRectangle, caption, description, new INotification[] { new Notification(CargoWise.EntityFramework.NotificationType.Error, "This is a test error.This is a test error.This is a test error.This is a test error.This is a test error.This is a test error.") });
					descriptor.HideWhenMouseOverBalloon = false;
					Balloon.Instance.Show(descriptor);
				},
			};
			form.Controls.Add(icon);
			return form;
		});

		await page.SetViewportSizeAsync(400, 400);
		await page.Locator(".notification").HoverAsync();
		await Task.Delay(100);

		var icon = await page.Locator(".notification").BoundingBoxAsync();
		var arrow = await page.Locator(".arrow").BoundingBoxAsync();

		Assert.That(arrow.X, Is.InRange(icon.X, (icon.X + icon.Width)));
		int acceptableOffsetYAxis = 10;
		Assert.That(arrow.Y, Is.InRange((icon.Y + icon.Height), (icon.Y + icon.Height + acceptableOffsetYAxis)));
	}

	[Test, WithPlaywrightPage]
	public async Task BalloonDoesNotOverlapAnchorElement()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 500, Height = 500 };
			var textBox = new ZTextBox() { Width = 200, Height = 20, Top = 50, Left = 200 };

			var balloonWindow = new BalloonWindow()
			{
				Descriptor = new BalloonDescriptor(textBox, "caption", "",
					new INotification[]
					{
							new Notification(CargoWise.ComponentModel.NotificationType.Error, "error"),
					})
			};

			form.Controls.Add(textBox);
			textBox.Click += (s, e) => { balloonWindow.Show(); };
			return form;
		});

		var textBox = await page.WaitForSelectorAsync(".form > .textbox:nth-child(1)");
		await textBox.ClickAsync();
		var arrow = await page.WaitForSelectorAsync(".arrow");
		var textBoxBottom = await textBox.EvaluateAsync<int>("e => e.getBoundingClientRect().bottom");
		var arrowTop = await arrow.EvaluateAsync<int>("e => e.getBoundingClientRect().top");

		Assert.That(arrowTop, Is.GreaterThanOrEqualTo(textBoxBottom).Within(5));
	}

	[Test, WithPlaywrightPage]
	public async Task BalloonWhenFlippedDoesNotOverlapAnchorElement()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 500, Height = 500 };
			var textBox = new ZTextBox() { Width = 200, Height = 20, Top = 480, Left = 200 };

			var balloonWindow = new BalloonWindow()
			{
				Descriptor = new BalloonDescriptor(textBox, "caption", "",
					new INotification[]
					{
							new Notification(CargoWise.ComponentModel.NotificationType.Error, "error"),
					})
			};

			form.Controls.Add(textBox);
			textBox.Click += (s, e) => { balloonWindow.Show(); };
			return form;
		});

		var textBox = await page.WaitForSelectorAsync(".form > .textbox:nth-child(1)");
		await textBox.ClickAsync();

		var arrow = await page.WaitForSelectorAsync(".arrow");
		var textBoxTop = (await textBox.BoundingBoxAsync()).Y;
		var arrowPosition = await arrow.BoundingBoxAsync();
		var arrowBottom = arrowPosition.Y + arrowPosition.Height;

		Assert.That(arrowBottom, Is.LessThanOrEqualTo(textBoxTop));
	}

	[Test, WithPlaywrightPage]
	[TestCase(700, 10, true, "skew(0deg, 45deg)")]
	[TestCase(700, 1200, true, "skew(0deg, -45deg)")]
	[TestCase(180, 100, true, "skew(0deg, 45deg)")]
	[TestCase(180, 1200, true, "skew(0deg, -45deg)")]
	[TestCase(700, 10, false, "skew(15deg, 45deg)")]
	[TestCase(700, 1200, false, "skew(-15deg, -45deg)")]
	[TestCase(180, 100, false, "skew(15deg, 45deg)")]
	[TestCase(180, 1200, false, "skew(-15deg, -45deg)")]
	public async Task BalloonArrowStyleShouldBeDeterminedByCaptionAndAnchorPosition(int textBoxTop, int textBoxLeft, bool isCaptionLong, string expectedTransformation)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 1280, Height = 720 };
			var textBox = new ZTextBox() { Width = 200, Height = 20, Top = textBoxTop, Left = textBoxLeft };
			var balloonWindow = new BalloonWindow()
			{
				Descriptor = new BalloonDescriptor(textBox, isCaptionLong ? "long caption - Wider than 80px changes skew on balloon arrow" : "short", "",
					new INotification[]
					{
							isCaptionLong ? new Notification(NotificationType.Error, "error") : new Notification(NotificationType.Information, "I"),
					})
			};

			form.Controls.Add(textBox);
			textBox.Click += (s, e) => { balloonWindow.Show(); };
			return form;
		});

		var textBox = await page.WaitForSelectorAsync(".form > .textbox:nth-child(1)");
		await textBox.ClickAsync();

		var arrow = await page.WaitForSelectorAsync(".arrow");
		var arrowTransform = await arrow.EvaluateAsync<string>("e => e.style['transform']");

		Assert.That(arrowTransform, Is.EqualTo(expectedTransformation));
	}

	[Test, WithPlaywrightPage]
	public async Task BalloonAvoidOtherPopupButShouldNotOverflow()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			Balloon.Instance.IsShownDuringTesting = true;
			var form = new WinzorTestForm() { Width = 500, Height = 300 };
			var icon = new NotificationIcon()
			{
				NotificationType = NotificationType.Error.EnumValueName,
				OnMouseOver = (sender, _) =>
				{
					var iconRef = (NotificationIcon)sender;
					var iconRectangle = ControlDpiScalingHelper.NewScaledRectangle(iconRef.Bounds.X, iconRef.Bounds.Y, 0, 60, false);
					var caption = "I am a caption";
					var description = "I am a description";
					var descriptor = new BalloonDescriptor(iconRef, iconRectangle, caption, description, new INotification[] { new Notification(NotificationType.Error, "This is a test error") });
					descriptor.HideWhenMouseOverBalloon = false;
					Balloon.Instance.Show(descriptor);
				},
				Top = 10,
				Left = 100,
				Width = 12,
				Height = 12,
				BackColor = Color.Red
			};
			form.Controls.Add(icon);

			form.BindingSource.DataSourceType = typeof(DummyBusinessObjectWithList);
			var dropEdit = new ZDropEdit()
			{
				Top = 20,
			};
			dropEdit.BindToList = "List";
			form.Controls.Add(dropEdit);
			form.BindingSource.SetBindingMember(dropEdit, "Z0_Code");

			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObjectWithList>();
			for (var i = 0; i < 5; i++)
			{
				var a = dummyBizo.List.AddNew();
				a.Z0_Code = i.ToString();
				a.Z0_Description = "Description " + i;
			}
			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		var buttons = await page.QuerySelectorAllAsync(".zdropbutton button");
		await buttons[0].ClickAsync();
		var notification = page.Locator(".notification");
		var notificationBox = await notification.BoundingBoxAsync();
		Assert.That(async () => await notification.IsVisibleAsync(), Is.True.After(2000, 100));
		await notification.HoverAsync();

		var balloon = page.Locator(".balloon");
		var balloonBox = await balloon.BoundingBoxAsync();
		Assert.That(balloonBox.Y, Is.GreaterThanOrEqualTo(notificationBox.Y + notificationBox.Height));
	}

	[Test]
	public async Task BalloonWindow_HooksParentFormEvents()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(RefCommodityCode);
			var descriptionTextBox = new ZTextBox();
			var descriptionTextBoxRectangle = ControlDpiScalingHelper.NewScaledRectangle(descriptionTextBox.Bounds.X, descriptionTextBox.Bounds.Y, 0, 60, false);
			descriptionTextBox.Parent = form;
			var descriptor = new BalloonDescriptor(descriptionTextBox,
				descriptionTextBoxRectangle,
				"I am a caption",
				"I am a description",
				new INotification[] { new Notification(CargoWise.EntityFramework.NotificationType.Error, "This is a test error") });
			var window = new BalloonWindow() { Descriptor = descriptor };
			form.Controls.Add(descriptionTextBox);
			form.BindingSource.SetBindingMember(descriptionTextBox, "RH_IATACommodityItem");
			form.DataSourceType = typeof(RefCommodityCode);
			var factory = new BusinessObjectFactory();
			var bo = factory.New<RefCommodityCode>();
			bo.RH_IATACommodityItem = "";
			form.SetDataBinding(bo, "");
			window.Show();

			Assert.That(window.hasHookedFormDispose, Is.True);
			return form;
		});
	}
}
