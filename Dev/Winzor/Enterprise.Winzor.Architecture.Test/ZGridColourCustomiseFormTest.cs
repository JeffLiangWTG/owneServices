using System;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Testing;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;
class ZGridColourCustomiseFormTest
{
	[Test, WithPlaywrightPage, WithTransaction]
	public async Task TestDragFunctionalityForZGridColourCustomiseForm()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZGridColourCustomiseFormForTest form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var scheme = factory.New<GridColourScheme>();
			var strip = new GridColourSchemeTest.FilterStripBusinessObjectForTest();
			var control = new FilterStripControlForTest(null, strip);

			var colourStripBO1 = new GridColourStripBusinessObject(strip, scheme, null);
			colourStripBO1.FilterStrips.AddNew();
			colourStripBO1.RuleName = "Rule1";
			((IFilterStripBusinessObjectInternals)colourStripBO1).LayoutContext = "LC1" + GridColourFactory.ColorStripCode;

			var colourStripBO2 = new GridColourStripBusinessObject(strip, scheme, null);
			colourStripBO2.FilterStrips.AddNew();
			colourStripBO2.RuleName = "Rule2";
			((IFilterStripBusinessObjectInternals)colourStripBO2).LayoutContext = "LC2" + GridColourFactory.ColorStripCode;

			var filter1 = factory.New<StmModuleFilter>();
			filter1.S9_FilterName = "Scheme1";
			filter1.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
			filter1.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			filter1.S9_ModuleID = "LC1" + GridColourFactory.ColorStripCode;

			var filter2 = factory.New<StmModuleFilter>();
			filter2.S9_FilterName = "Scheme2";
			filter2.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
			filter2.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			filter2.S9_ModuleID = "LC2" + GridColourFactory.ColorStripCode;

			colourStripBO1.StmModuleFilter = filter1;
			colourStripBO2.StmModuleFilter = filter2;

			scheme.ColourStrips.Add(colourStripBO1);
			scheme.ColourStrips.Add(colourStripBO2);
			scheme.S9_ModuleID = "test_module";

			factory.Save();
			form = new ZGridColourCustomiseFormForTest(scheme, strip, control, typeof(BusinessObject));
			return form;
		});

		Assert.That(() => form.RulesTabControl.TabPages[0].Text, Is.EqualTo("Rule1").After(3000,100));
		Assert.That(() => form.RulesTabControl.TabPages[1].Text, Is.EqualTo("Rule2").After(3000,100));

		var tabPageButton1 = page.Locator(".tabcontrol__button:nth-child(1)");
		var tabPageButton2 = page.Locator(".tabcontrol__button:nth-child(2)");
		var tabPageButton1Box = await tabPageButton1.BoundingBoxAsync();
		var tabPageButton2Box = await tabPageButton2.BoundingBoxAsync();

		Assert.That(tabPageButton1Box,Is.Not.Null);
		Assert.That(tabPageButton2Box, Is.Not.Null);

		await tabPageButton1.HoverAsync();
		await page.Mouse.DownAsync();
		await page.Mouse.MoveAsync(tabPageButton2Box.X + tabPageButton2Box.Width / 2, tabPageButton2Box.Y + tabPageButton2Box.Height / 2);
		await page.Mouse.UpAsync();

		Assert.That(() => form.RulesTabControl.TabPages[0].Text, Is.EqualTo("Rule2").After(3000, 100));
		Assert.That(() => form.RulesTabControl.TabPages[1].Text, Is.EqualTo("Rule1").After(3000, 100));
	}

	class ZGridColourCustomiseFormForTest : ZGridColourCustomiseForm
	{
		public ZGridColourCustomiseFormForTest(GridColourScheme scheme, FilterStripBusinessObject filterStripBusinessObject, ZFilterStripCommonControl stripControl, Type businessEntityType)
			: base(scheme, filterStripBusinessObject, stripControl, businessEntityType)
		{
		}

		public ZGridColourCustomiseFormForTest(GridColourScheme scheme, FilterStripBusinessObject filterStripBusinessObject, ZFilterStripCommonControl stripControl)
			: base(scheme, filterStripBusinessObject, stripControl, typeof(BusinessObject))
		{
		}

		public new ZTabControl RulesTabControl
		{
			get { return base.RulesTabControl; }
		}
	}
}
