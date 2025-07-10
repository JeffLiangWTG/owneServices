using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.ComponentModel;
using Extensions;
using NUnit.Framework;
using WinzorFramework;

namespace Enterprise.Winzor.Architecture.Test;

class NotificationIconTest
{
	[Test]
	public async Task NotificationIconEmpty()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new NotificationIcon());

		Assert.That(rendered.FindAll("div.validation").Count, Is.EqualTo(0));
	}

	[Test]
	public async Task NotificationIconTypes()
	{
		using var ctx = new EnterpriseTestContext();
		NotificationIcon icon = null;
		TextBox textbox = null;
		bool mouseOver = false, mouseOut = false, iconClicked = false;
		var error = await ctx.RenderControlOnFormAsync(() => {
			textbox = new TextBox();
			icon = new NotificationIcon() { NotificationType = NotificationType.Error.EnumValueName.ToLowerHyphen(), NotificationAnchorControl = textbox, OnMouseOver = (s, a) => mouseOver = true, OnMouseOut = (s, a) => mouseOut = true, OnClickIcon = (s, a) => iconClicked = true };
			return icon;
		});

		var warning = await ctx.RenderControlOnFormAsync(() => new NotificationIcon() { NotificationType = NotificationType.Warning.EnumValueName.ToLowerHyphen(), NotificationAnchorControl = textbox });
		var information = await ctx.RenderControlOnFormAsync(() => new NotificationIcon() { NotificationType = NotificationType.Information.EnumValueName.ToLowerHyphen(), NotificationAnchorControl = textbox });
		var messageError = await ctx.RenderControlOnFormAsync(() => new NotificationIcon() { NotificationType = CargoWise.EntityFramework.NotificationType.MessageError.EnumValueName.ToLowerHyphen(), NotificationAnchorControl = textbox });

		await error.Find(".notification").TriggerEventAsync("onmouseover", null);
		await error.Find(".notification").TriggerEventAsync("onmouseout", null);
		await error.Find(".notification").TriggerEventAsync("onclick", null);
		Assert.That(error.FindAll("div.notification.notification--error").Count, Is.EqualTo(1));
		Assert.That(warning.FindAll("div.notification.notification--warning").Count, Is.EqualTo(1));
		Assert.That(information.FindAll("div.notification.notification--information").Count, Is.EqualTo(1));
		Assert.That(messageError.FindAll("div.notification.notification--message-error").Count, Is.EqualTo(1));
		Assert.That(mouseOver, Is.True);
		Assert.That(mouseOut, Is.True);
		Assert.That(iconClicked, Is.True);
		Assert.That(icon.NotificationAnchorControl, Is.EqualTo(textbox));
	}

	[Test]
	public async Task NotificationIconShouldBeHiddenOnFocusOnTextBox()
	{
		using var ctx = new EnterpriseTestContext();
		Form form = null;
		NotificationIcon icon = null;
		var error = await ctx.RenderFormAsync(() => {
			form = new Form();
			var textBox = new TextBox();
			form.Controls.Add(textBox);
			textBox.Focus();
			icon = new NotificationIcon() { NotificationType = NotificationType.Error.EnumValueName, NotificationAnchorControl = textBox };
			return form;
		});

		Assert.That(error.FindAll(".notification").Count, Is.EqualTo(0));
	}

	[Test]
	public async Task NotificationIconShouldReappearOnLostFocusOnTextBox()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();

			var textBox = new TextBox();
			var icon = new NotificationIcon() { NotificationType = NotificationType.Error.EnumValueName, NotificationAnchorControl = textBox };
			form.Controls.Add(textBox);
			form.Controls.Add(icon);

			var textBox2 = new TextBox();
			form.Controls.Add(textBox2);

			return form;
		});

		rendered.Find("input").Click();
		Assert.That(rendered.FindAll(".notification").Count, Is.EqualTo(1));

		rendered.FindAll("input").Last().Click();
		Assert.That(rendered.FindAll(".notification").Count, Is.EqualTo(1));
	}

	[Test]
	public async Task NotificationIconShouldBeDisplayedOnFocusOnAnchorControlWhichIsNotTextBox()
	{
		using var ctx = new EnterpriseTestContext();
		NotificationIcon icon = null;
		var error = await ctx.RenderControlOnFormAsync(() => {
			var checkBox = new CheckBox();
			checkBox.Focus();
			icon = new NotificationIcon() { NotificationType = NotificationType.Error.EnumValueName, NotificationAnchorControl = checkBox };
			return icon;
		});

		Assert.That(error.FindAll(".notification").Count, Is.EqualTo(1));
	}

	[Test]
	public async Task NotificationIconClickShouldNotThrowExceptions()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();

			var textBox = new TextBox();
			form.Controls.Add(textBox);

			var icon = new NotificationIcon() { NotificationType = NotificationType.Error.EnumValueName, NotificationAnchorControl = textBox };
			icon.OnClickIcon += (s, a) => icon.Dispose();
			form.Controls.Add(icon);

			return form;
		});

		rendered.Find("input").Click();
		Assert.That(rendered.FindAll(".notification").Count, Is.EqualTo(1));

		Assert.DoesNotThrowAsync(async () => await rendered.Find(".notification").TriggerEventAsync("onclick", null));
		Assert.That(rendered.FindAll(".notification").Count, Is.EqualTo(0));
	}
}
