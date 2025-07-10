using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Balloons;
using Enterprise.ZArchitecture.Modules;
using Microsoft.AspNetCore.Components;
using NUnit.Framework;
using WinzorFramework;

namespace Enterprise.Winzor.Architecture.Test;

class ControlValidationTest
{
	[TestCaseSource(nameof(TestCases))]
	public async Task SetValidationBackground(string field, string badValue, string goodValue, string expectedBackground)
	{
		using var env = EnvProxy.SetTemporaryEnvForTest(ObjectFactory.Get<IEnv>());
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(RefCommodityCode);
			var textbox = new ZTextBox();
			var button = new ZButton() { Text = "Validate", Top = 200 };
			form.Controls.Add(textbox);
			form.Controls.Add(button);
			form.BindingSource.SetBindingMember(textbox, field);
			var factory = new BusinessObjectFactory();
			var bo = factory.New<RefCommodityCode>();
			form.SetDataBinding(bo, "");
			return form;
		});

		var input = rendered.Find("input");
		var validateButton = rendered.Find("button:contains('Validate')");

		await input.TriggerEventAsync("onwinzorfocusin", new WinzorFocusInEventArgs());
		await input.TriggerEventAsync("oninput", new ChangeEventArgs { Value = badValue });
		await validateButton.TriggerEventAsync("onwinzorfocusin", new WinzorFocusInEventArgs());
		Assert.That(input.Attributes["style"].Value.Contains($"background-color:#{expectedBackground}", StringComparison.OrdinalIgnoreCase), Is.True);

		await input.TriggerEventAsync("onwinzorfocusin", new WinzorFocusInEventArgs());
		await input.TriggerEventAsync("oninput", new ChangeEventArgs { Value = goodValue });
		await validateButton.TriggerEventAsync("onwinzorfocusin", new WinzorFocusInEventArgs());
		Assert.That(input.Attributes["style"].Value, Does.Not.Contain($"background-color:#{expectedBackground}").IgnoreCase);
	}

	static object[] TestCases => new object[] {
		new object[] { "RH_Code", " ", "foo", "FFD7D7FF" },
		new object[] { "RH_IATACommodityItem", "aa", string.Empty, "FFD2A6FF" } };

	[Test]
	public async Task SetIconAndBalloon()
	{
		using var env = EnvProxy.SetTemporaryEnvForTest(ObjectFactory.Get<IEnv>());
		using var ctx = new EnterpriseTestContext();
		var errorSelector = "div.notification.notification--error";
		using var testIcon = new Bitmap(40, 40, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
		NotificationIconScheme.Instance.SetMiniImage(CargoWise.ComponentModel.NotificationType.Error, testIcon);
		NotificationIconScheme.Instance.SetMiniImage(CargoWise.ComponentModel.NotificationType.Warning, testIcon);
		try
		{
			var rendered = await ctx.RenderFormAsync(() =>
			{
				var form = new WinzorTestForm();
				form.BindingSource.DataSourceType = typeof(RefCommodityCode);
				var codeTextBox = new ZTextBox() { Width = 200, Height = 50, Top = 100, Left = 200 };
				var button = new ZButton() { Text = "Validate", Top = 200 };
				form.Controls.Add(button);
				form.Controls.Add(codeTextBox);
				form.BindingSource.SetBindingMember(codeTextBox, "RH_Code");
				var factory = new BusinessObjectFactory();
				var bo = factory.New<RefCommodityCode>();
				bo.RH_Code = "FOO";
				form.SetDataBinding(bo, "");
				Balloon.Instance.IsShownDuringTesting = true;
				return form;
			});

			var input = rendered.Find("input");
			var validateButton = rendered.Find("button:contains('Validate')");

			Assert.That(rendered.FindAll(errorSelector), Is.Empty);

			await input.TriggerEventAsync("onwinzorfocusin", new WinzorFocusInEventArgs());
			await input.TriggerEventAsync("oninput", new ChangeEventArgs { Value = string.Empty });
			await validateButton.TriggerEventAsync("onwinzorfocusin", new WinzorFocusInEventArgs());

			rendered.WaitForState(() => rendered.FindAll(errorSelector).Count == 1);

			await rendered.Find(errorSelector).TriggerEventAsync("onmouseover", null);

			rendered.WaitForState(() => rendered.FindAll(".balloon").Count == 1);

			var caption = rendered.Find(".balloon .balloon__caption");
			var items = rendered.FindAll(".balloon__items li");
			var icon = rendered.FindAll(errorSelector).Single().ParentElement;
			var balloonWrapper = rendered.FindAll(".popup").Single().ParentElement;
			Assert.That(icon.Attributes["style"].Value, Is.EqualTo("position:absolute;width:0px;height:0px;top:103px;left:357px;background-color:#DCE1E4FF;"));
			Assert.That(balloonWrapper.Attributes["style"].Value, Is.EqualTo("position:absolute;width:0px;height:0px;top:103px;left:357px;z-index:95;background-color:#DCE1E4FF;"));
			Assert.That(caption.InnerHtml, Is.EqualTo("Commodity Code"));
			Assert.That(items.Count, Is.EqualTo(2));
			Assert.That(items[0].Attributes["class"].Value, Is.EqualTo("balloon__item--error"));
			Assert.That(items[0].InnerHtml, Is.EqualTo("Please enter a Commodity Code."));
			Assert.That(items[1].Attributes["class"].Value, Is.EqualTo("balloon__item--error"));
			Assert.That(items[1].InnerHtml, Is.EqualTo("Commodity code must be at least 2 characters long."));

			await rendered.Find(errorSelector).TriggerEventAsync("onmouseout", null);
			await input.TriggerEventAsync("onwinzorfocusin", new WinzorFocusInEventArgs());
			await input.TriggerEventAsync("oninput", new ChangeEventArgs { Value = "aa" });
			await validateButton.TriggerEventAsync("onwinzorfocusin", new WinzorFocusInEventArgs());

			rendered.WaitForState(() => rendered.FindAll(errorSelector).Count == 0);

			await OnWarningSetWarningIcon();
		}
		finally
		{
			NotificationIconScheme.Instance.SetMiniImage(CargoWise.ComponentModel.NotificationType.Error, Icons.GetMiniImage(IconTypes.Error));
			NotificationIconScheme.Instance.SetMiniImage(CargoWise.ComponentModel.NotificationType.Warning, Icons.GetMiniImage(IconTypes.Warning));
		}
	}

	async Task OnWarningSetWarningIcon()
	{
		using var env = EnvProxy.SetTemporaryEnvForTest(ObjectFactory.Get<IEnv>());
		using var ctx = new EnterpriseTestContext();
		var iconSelector = "div.notification.notification--warning";
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(RefCommodityCode);
			var descriptionTextBox = new ZTextBox() { Width = 200, Height = 50, Top = 100, Left = 200 };
			var button = new ZButton() { Text = "Validate", Top = 200 };
			form.Controls.Add(button);
			form.Controls.Add(descriptionTextBox);
			form.BindingSource.SetBindingMember(descriptionTextBox, "RH_IATACommodityItem");
			form.DataSourceType = typeof(RefCommodityCode);
			var factory = new BusinessObjectFactory();
			var bo = factory.New<RefCommodityCode>();
			bo.RH_IATACommodityItem = "";
			form.SetDataBinding(bo, "");
			Balloon.Instance.IsShownDuringTesting = true;
			return form;
		});

		var input = rendered.Find("input");
		var validateButton = rendered.Find("button:contains('Validate')");

		Assert.That(rendered.FindAll(iconSelector), Is.Empty);

		await input.TriggerEventAsync("onwinzorfocusin", new WinzorFocusInEventArgs());
		await input.TriggerEventAsync("oninput", new ChangeEventArgs { Value = "aa" });
		await validateButton.TriggerEventAsync("onwinzorfocusin", new WinzorFocusInEventArgs());

		rendered.WaitForState(() => rendered.FindAll(iconSelector).Count == 1);

		await rendered.Find(iconSelector).TriggerEventAsync("onmouseover", null);

		rendered.WaitForState(() => rendered.FindAll(".balloon").Count == 1);

		var caption = rendered.Find(".balloon .balloon__caption");
		var items = rendered.FindAll(".balloon__items li");
		var icon = rendered.FindAll(iconSelector).Single().ParentElement;
		var balloonWrapper = rendered.FindAll(".popup").Single().ParentElement;

		Assert.That(icon.Attributes["style"].Value, Is.EqualTo("position:absolute;width:0px;height:0px;top:103px;left:357px;background-color:#DCE1E4FF;"));
		Assert.That(icon.Attributes["style"].Value, Is.EqualTo("position:absolute;width:0px;height:0px;top:103px;left:357px;background-color:#DCE1E4FF;"));
		Assert.That(balloonWrapper.Attributes["style"].Value, Is.EqualTo("position:absolute;width:0px;height:0px;top:103px;left:357px;z-index:95;background-color:#DCE1E4FF;"));
		Assert.That(caption.InnerHtml, Is.EqualTo(string.Empty));
		Assert.That(items.Count, Is.EqualTo(1));
		Assert.That(items[0].Attributes["class"].Value, Is.EqualTo("balloon__item--warning"));
		Assert.That(items[0].InnerHtml, Is.EqualTo("Enter an IATA Commodity between 4 and 7 digits long."));

		await input.TriggerEventAsync("onwinzorfocusin", new WinzorFocusInEventArgs());
		await input.TriggerEventAsync("oninput", new ChangeEventArgs { Value = string.Empty });
		await validateButton.TriggerEventAsync("onwinzorfocusin", new WinzorFocusInEventArgs());

		rendered.WaitForState(() => rendered.FindAll(iconSelector).Count == 0);
	}

	[Test]
	public async Task DisableValidationForNonVisibleControls()
	{
		var validated = false;
		var validating = false;
		ZTextBox textbox = null;
		using var env = EnvProxy.SetTemporaryEnvForTest(ObjectFactory.Get<IEnv>());
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(RefCommodityCode);
			textbox = new ZTextBox();
			form.Controls.Add(textbox);
			form.BindingSource.SetBindingMember(textbox, "RH_Code");
			var factory = new BusinessObjectFactory();
			var bo = factory.New<RefCommodityCode>();
			form.SetDataBinding(bo, "");
			textbox.Validated += (s, a) => validated = true;
			textbox.Validating += (s, a) => validating = true;
			textbox.Visible = false;
			textbox.Text = "novalidation";
			return form;
		});

		Assert.That(validating, Is.False);
		Assert.That(validated, Is.False);
	}
}
