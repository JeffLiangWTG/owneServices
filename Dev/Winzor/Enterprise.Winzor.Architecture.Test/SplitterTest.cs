using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WinzorFramework;

namespace Enterprise.Winzor.Architecture.Test;

class SplitterTest
{
	[Test]
	[WithTransaction]
	public async Task SplitterPositionSaved()
	{
		Splitter splitter = null;
		using var ctx = new EnterpriseTestContext();
		ZForm form = null;
		ZForm form2 = null;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new ZForm { Size = new Size(1000, 1000) };
			form.Load += (s, e) => { SplitterLayoutStrategy.RestoreSplittersLayoutForTest(form); };
			splitter = new KSplitter { Dock = DockStyle.Top };
			var fillPanel = new Panel { Dock = DockStyle.Fill };
			var dockedPanel = new Panel { Size = new Size(1000, 600), Dock = DockStyle.Top };
			form.Controls.AddRange(new Control[] { fillPanel, splitter, dockedPanel });
			return form;
		});

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			((IWinFormsEnvironment)EnvProxy.Instance).SplitterLayoutRegistry.ClearSplitterLayout(SplitterLayoutStrategy.ComputeUniqueKeyForSplitter(splitter));
		});

		var splitterObject1 = rendered.Find(".splitter:not(.splitter--horizontal)");
		var firstStyleBeforeMove = splitterObject1.GetAttribute("style");
		Assert.That(firstStyleBeforeMove, Does.Contain("top:600px;"), "first check that the top is 600px");

		await rendered.Find(".splitter").TriggerEventAsync("onsplittermoved", new SplitterMovedEventArgs()
		{
			xOffset = 0,
			yOffset = -100
		});
		var firstStyleAfterMove = splitterObject1.GetAttribute("style");

		Assert.That(firstStyleAfterMove, Does.Contain("top:500px;"), "second check that the top is now 500px after moving it by 100px");
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			form.Close(); //force close the form is required
		});
		rendered.Dispose();

		var rendered2 = await ctx.RenderFormAsync(() =>
		{
			form2 = new ZForm { Size = new Size(1000, 1000) };
			form2.Load += (s, e) => { SplitterLayoutStrategy.RestoreSplittersLayoutForTest(form2); };
			splitter = new KSplitter { Dock = DockStyle.Top };
			var fillPanel = new Panel { Dock = DockStyle.Fill };
			var dockedPanel = new Panel { Size = new Size(1000, 600), Dock = DockStyle.Top };
			form2.Controls.AddRange(new Control[] { fillPanel, splitter, dockedPanel });
			return form2;
		});
		var splitterObject2 = rendered2.Find(".splitter:not(.splitter--horizontal)");
		var secondStyleAfterNewLoad = splitterObject2.GetAttribute("style");

		Assert.That(secondStyleAfterNewLoad, Does.Contain("top:500px;"), "third check that the new form that is being loaded respects the cached splitter position and shows top 500px (not default 600px)");
	}
}
