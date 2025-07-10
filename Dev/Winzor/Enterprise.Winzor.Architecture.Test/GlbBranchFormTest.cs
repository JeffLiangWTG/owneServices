using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

class GlbBranchFormTest
{
	[Test]
	public async Task ZTabControlAfterTabPageWithZGridSelectedCanFocusFalse()
	{
		using var ctx = new EnterpriseTestContext();
		GlbBranchForm form = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var branch = factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			form = new GlbBranchForm(branch);
			return form;
		});

		var tabControl = form.Controls.OfType<ZTabControl>().FirstOrDefault();
		var tabPage = tabControl.TabPages[2] as ZTabPage;
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			tabControl.SelectedTab = tabPage;
		});
		var zgrid = tabPage.Controls.OfType<ZGrid>().FirstOrDefault();
		Assert.That(zgrid.GetState(ZGrid.GridState.CanFocus), Is.False);
	}
}
