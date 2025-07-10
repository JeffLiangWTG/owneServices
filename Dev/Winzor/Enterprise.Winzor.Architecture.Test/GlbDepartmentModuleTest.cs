using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Internal;
using Microsoft.Playwright;
using NUnit.Framework;
using WTG.PlaywrightTesting;
namespace Enterprise.Winzor.Architecture.Test;
using static PlaywrightTestContext;

class GlbDepartmentModuleTest
{
	[Test, WithPlaywrightPage]
	public async Task GlbDepartmentModuleDoubleClickClose()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var isGridDisposed = false;
		var isDoubleClickProcessed = false;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new ZForm() { Width = 800, Height = 600 };
			var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbDepartment);
			var popup = new EmbeddedModulePopup(module);
			module.OverrideModuleDecisionProvider(new PopupModuleDecisionProviderForTest(new DummyFindBoxForTest(popup)));
			module.DisplayGrid.Disposed += (s, e) => isGridDisposed = true;
			module.DisplayGrid.DoubleClick += (s, e) => isDoubleClickProcessed = true;
			form.Controls.Add(module.EmbeddedControl);
			return form;
		});
		await page.ClickAsync("button[title='Find']");
		var dataGridRow = page.Locator("tr:nth-child(5)");
		await dataGridRow.WaitForAsync();
		ErrorReporter.Clear();
		Assert.DoesNotThrowAsync(async () => await dataGridRow.DblClickAsync());
		await Assertions.Expect(dataGridRow).Not.ToBeVisibleAsync();
		Assert.That(() => isDoubleClickProcessed, Is.True.After(2000, 100));
		Assert.That(() => isGridDisposed, Is.True.After(2000, 100));
		Assert.That(ErrorReporter.LastExceptionReported, Is.Null);
		PageErrors.Clear();
	}

	public class PopupModuleDecisionProviderForTest : ModuleDecisionProvider
	{
		public PopupModuleDecisionProviderForTest(IFindBox findBox)
			: base(findBox, new PopupControllerLink(findBox))
		{
		}
		public override bool ShouldIgnoreAdditionalFilter
		{
			get { return true; }
		}
		public override bool AllowExcelExport
		{
			get { return false; }
		}
		public override bool EnablePreviousNextSupport
		{
			get { return false; }
		}
		public override bool ShouldDisplayNotifications
		{
			get { return true; }
		}
		public override bool AllowMultiSelect
		{
			get { return false; }
		}
		public override IBusinessObjectCollection List => null;
	}
	class DummyFindBoxForTest : IFindBox
	{
		public DummyFindBoxForTest(EmbeddedModulePopup popup)
			: this("Dummy", "It's dummy")
		{
			PopupForm = popup;
		}
		public DummyFindBoxForTest(string code, string description)
		{
			this.Code = Code;
			this.Description = description;
		}
		public string Code { get; set; }
		public string Description { get; set; }
		public IFindBoxListProvider ListProvider { get; private set; }
		public IFindBoxPopup PopupForm { get; private set; }
	}
}
