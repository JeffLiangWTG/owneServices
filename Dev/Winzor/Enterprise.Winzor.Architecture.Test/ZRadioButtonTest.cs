using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.Application;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Core.Environment.Registry;
using Enterprise.ZArchitecture.GUI;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

using static PlaywrightTestContext;

class ZRadioButtonTest
{
	[Test, WithPlaywrightPage]
	public async Task RadioButtonBackgroundColorChangedAfterClick()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var radioButton1 = new ZRadioButton() { Dock = DockStyle.Top, Text = "Radio 1" };
			var radioButton2 = new ZRadioButton() { Dock = DockStyle.Top, Text = "Radio 2" };

			var panel = new ZPanel() { Dock = DockStyle.Fill };
			panel.Controls.Add(radioButton1);
			panel.Controls.Add(radioButton2);

			var form = new ZForm();
			form.Controls.Add(panel);

			return form;
		});

		await page.WaitForSelectorAsync(".form");
		Assert.That(async () => await (await page.QuerySelectorAllAsync(".radiobutton"))[1].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-color')"), Is.EqualTo("rgb(220, 225, 228)").After(2000, 100));

		await (await page.QuerySelectorAllAsync(".radiobutton"))[1].ClickAsync();
		Assert.That(async () => await (await page.QuerySelectorAllAsync(".radiobutton"))[1].EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-color')"), Is.EqualTo("rgb(255, 255, 225)").After(2000, 100));
	}

	[Test, WithTransaction]
	public async Task TestZRadioButtonText()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new RegistryFormForTest();
			var radioButtonControl = new RadioButtonControl("");
			form.Controls.Add(radioButtonControl);
			return form;
		});

		var radioButton = rendered.Find(".radiobutton__text");
		Assert.That(radioButton, Is.Not.Null, "Radio button text element not found.");
		Assert.That(radioButton.TextContent, Is.EqualTo("Yes"));
	}

	class RegistryFormForTest : RegistryForm
	{
		readonly IDisposable disposable;
		public RegistryFormForTest() : base()
		{
			var registryProvider = CreateProvider(CreateEmptyRegistry());
			disposable = ObjectFactory.Substitute(registryProvider);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			disposable.Dispose();
		}

		static IRegistry CreateEmptyRegistry()
		{
			var registryMock = new Mock<IRegistry>(MockBehavior.Strict);
			registryMock.Setup(r => r.GetSortedTopLevelCategories()).Returns(Array.Empty<RegistryCategoryRef>());
			return registryMock.Object;
		}

		static IRegistryProvider CreateProvider(IRegistry registry)
		{
			var registryFactoryMock = new Mock<IRegistryProvider>(MockBehavior.Strict);
			registryFactoryMock.Setup(f => f.CreateRegistry(It.IsAny<IRegistryItemVisibility>())).Returns(registry);
			return registryFactoryMock.Object;
		}
	}

	[Test, WithPlaywrightPage]
	public async Task RadioButtonShouldBeFocusedOnlyWhenActive()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var checkBox1 = new ZCheckBox() { Text = "Check1", Checked = true };

			var radioButton1 = new ZRadioButton() { Dock = DockStyle.Top, Text = "Radio 1", Checked = true };
			var radioButton2 = new ZRadioButton() { Dock = DockStyle.Top, Text = "Radio 2" };
			var radioButton3 = new ZRadioButton() { Dock = DockStyle.Top, Text = "Radio 3" };

			var form = new ZForm();
			form.Controls.Add(checkBox1);
			form.Controls.Add(radioButton1);
			form.Controls.Add(radioButton2);
			form.Controls.Add(radioButton3);

			return form;
		});

		var form = await page.WaitForSelectorAsync(".form");
		Assert.That(form, Is.Not.Null);

		var tabNumber = 20;
		for (int i = 0; i < tabNumber; i++)
		{
			await form.PressAsync("Tab");

			var radioButton2 = await page.WaitForSelectorAsync(".form > label:nth-child(4) > input[type=radio]");
			var radioButton2IsFocused = await page.EvaluateAsync<bool>("document.activeElement === document.querySelector('.form > label:nth-child(4) > input[type=radio]')");
			Assert.That(!radioButton2IsFocused, Is.True);

			var radioButton3 = await page.WaitForSelectorAsync(".form > label:nth-child(5) > input[type=radio]");
			var radioButton3IsFocused = await page.EvaluateAsync<bool>("document.activeElement === document.querySelector('.form > label:nth-child(5) > input[type=radio]')");
			Assert.That(!radioButton3IsFocused, Is.True);
		}
	}
}
