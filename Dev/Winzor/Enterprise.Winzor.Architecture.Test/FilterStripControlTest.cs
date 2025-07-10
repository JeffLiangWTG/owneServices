using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

using static Enterprise.ZArchitecture.GUI.Testing.FilterStripControlTest;

class FilterStripControlTest
{
	[Test, WithPlaywrightPage, WithTransaction]
	public async Task ZDropEditAutoCompleteShouldWorksInFilterStripControl()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var filterBizO = new DummyFilterStripBusinessObjectForDefaultsTesting();
			var defaultValue = new ZString("default value");

			var defaults = new FilterBusinessObjectDefaults();
			var filterDefault = new FilterBusinessObjectDefault("hasDefaultValueFilter", "Property", defaultValue);
			defaults.Add(filterDefault);
			filterBizO.SetExternalDefaults(defaults);

			var form = new Form() { Width = 800 };
			var filterControl = new DummyZFilterStripControl(new DummyBusinessObjectCollection(factory), filterBizO);
			form.Controls.Add(filterControl);
			return form;
		});

		var zdropcodebox = page.Locator("input.zdropcodebox").Nth(1);
		await zdropcodebox.WaitForAsync();
		await zdropcodebox.FocusAsync();
		await page.WaitForFunctionAsync("document.activeElement.selectionStart === 0 && document.activeElement.selectionEnd === document.activeElement.value.length");

		await page.Keyboard.DownAsync("E");
		Assert.That(async () => await zdropcodebox.InputValueAsync(), Is.EqualTo("exact").After(1000, 200));
		Assert.That(async () => await zdropcodebox.GetAttributeAsync("class"), Is.EqualTo("zdropcodebox textbox textbox--lower textbox--border-none"));
	}
}
