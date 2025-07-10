using System.Threading.Tasks;
using Bunit;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WinzorFramework;
using static Enterprise.ZArchitecture.GUI.Testing.FilterStripControlTest;

namespace Enterprise.Winzor.Architecture.Test;

public class StripControlTest
{
	[Test]
	public async Task DragAndDropStripFilter()
	{
		DummyZFilterStripControl filterControl = null;
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var textFilter = new ModuleTextFilter("Text filter", DummyBizoSchema.GenericStringSchemaColumn);
			var textFilter2 = new ModuleTextFilter("more filter", DummyBizoSchema.GenericStringSchemaColumn);
			var filterBizO = new DummyFilterStripBusinessObject();

			filterBizO.AddModuleFilterForTest(textFilter);
			filterBizO.AddModuleFilterForTest(textFilter2);

			filterControl = new DummyZFilterStripControl(new DummyBusinessObjectCollection(new BusinessObjectFactory()), filterBizO);

			return filterControl;
		});

		var zFilterStripAddButton = rendered.Find("div[data-type='Enterprise.ZArchitecture.GUI.Internal.ZFilterStripAddButton'] button");
		await zFilterStripAddButton.ClickAsync(new WebMouseEventArgs());

		await filterControl.InvokeWinzorDispatcherAsync(() =>
		{
			filterControl.Strips[0].CurrentDataItem.FilterDescription = "Text filter";
			filterControl.Strips[1].CurrentDataItem.FilterDescription = "more filter";
		});

		var strips = rendered.FindAll("div[data-type='Enterprise.ZArchitecture.GUI.ZFilterStrip']");

		Assert.That(strips[0].GetAttribute("style"), Does.Contain("top:3px;left:3px;"));
		Assert.That(strips[0].GetElementsByTagName("input")[0].GetAttribute("value"), Is.EqualTo("Text filter"));

		Assert.That(strips[1].GetAttribute("style"), Does.Contain("top:26px;left:3px;"));
		Assert.That(strips[1].GetElementsByTagName("input")[0].GetAttribute("value"), Is.EqualTo("more filter"));

		await strips[0].GetElementsByClassName("panel")[0].DragStartAsync(new WebDragEventArgs());
		await strips[1].DragOverAsync(new WebDragEventArgs());
		strips = rendered.FindAll("div[data-type='Enterprise.ZArchitecture.GUI.ZFilterStrip']");

		Assert.That(strips[1].GetAttribute("style"), Does.Contain("background-color:#D3D3D3FF"));
		Assert.That(strips[0].GetAttribute("style"), Does.Contain("background-color:var(--color-control)"));

		await strips[1].DragLeaveAsync(new WebDragEventArgs());
		await strips[0].DragOverAsync(new WebDragEventArgs());
		strips = rendered.FindAll("div[data-type='Enterprise.ZArchitecture.GUI.ZFilterStrip']");

		Assert.That(strips[1].GetAttribute("style"), Does.Contain("background-color:var(--color-control)"));
		Assert.That(strips[0].GetAttribute("style"), Does.Contain("background-color:#D3D3D3FF"));

		await strips[1].GetElementsByClassName("panel")[0].TriggerEventAsync("onwinzordragend", new WinzorDragEndEventArgs());

		await strips[0].TriggerEventAsync("onwinzordrop", new WinzorDragEventArgs()
		{
			ClientX = 3,
			ClientY = 30,
			ControlID = filterControl.Strips[0].WinzorControlId
		});

		strips = rendered.FindAll("div[data-type='Enterprise.ZArchitecture.GUI.ZFilterStrip']");

		Assert.That(strips[0].GetAttribute("style"), Does.Contain("top:26px;left:3px;"));
		Assert.That(strips[0].GetElementsByTagName("input")[0].GetAttribute("value"), Is.EqualTo("Text filter"));

		Assert.That(strips[1].GetAttribute("style"), Does.Contain("top:3px;left:3px;"));
		Assert.That(strips[1].GetElementsByTagName("input")[0].GetAttribute("value"), Is.EqualTo("more filter"));
	}
}
