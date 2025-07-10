using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WinzorFramework;
using DummyZTemplateForm = Enterprise.ZArchitecture.GUI.Testing.ZTemplateFormTest.DummyZTemplateForm;
using MouseEventArgs = Microsoft.AspNetCore.Components.Web.MouseEventArgs;

namespace Enterprise.Winzor.Architecture.Test;

class ZTemplateFormTest
{
	[Test]
	[WithTransaction]
	public async Task NotesGridLoadsWhenNotesTabClicked()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var bizo = factory.New<DummyEnterpriseBusinessObject>();
			factory.Save();
			var form = new DummyZTemplateForm(bizo);
			return form;
		}, OpenFormAction.BlockUntilShown);
		rendered.WaitForElement("button:contains('Notes')");
		await rendered.InvokeAsync(() => rendered.Find("button:contains('Notes')").ClickAsync(new MouseEventArgs()));
		Assert.That(rendered.Find(".datagrid"), Is.Not.Null);
	}

	[Test]
	public async Task ZTemplateFormEdocsTabInCorrectPosition()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() => new ZTemplateForm());

		var tabControlTabs = rendered.Find(".tabcontrol > .tabcontrol__navigation > .tabcontrol__tabs");
		Assert.That(tabControlTabs, Is.Not.Null);
		var buttons = tabControlTabs.Children.Where(c => c.ClassName.Contains("tabcontrol__button", StringComparison.Ordinal)).ToList();
		Assert.That(buttons.Count, Is.EqualTo(4));
		Assert.That(buttons.ElementAt(0).TextContent, Is.EqualTo("Details"));
		Assert.That(buttons.ElementAt(1).TextContent, Is.EqualTo("eDocs"));
		Assert.That(buttons.ElementAt(2).TextContent, Is.EqualTo("Notes"));
		Assert.That(buttons.ElementAt(3).TextContent, Is.EqualTo("Logs"));
	}

	[Test]
	public async Task TestMainStatusBarAlwaysDocksToTheBottom()
	{
		using var ctx = new EnterpriseTestContext();
		ZForm form = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new ZForm();

			var mainPanel = new Panel();
			mainPanel.Dock = DockStyle.Fill;
			mainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			mainPanel.Name = "MainPanel";
			mainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 200, true);
			mainPanel.TabIndex = 12;

			var bottomPanel = new Panel();
			bottomPanel.Dock = DockStyle.Bottom;
			bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 264, true);
			bottomPanel.Name = "BottomPanel";
			bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 36, true);
			bottomPanel.TabIndex = 0;

			form.Controls.Add(mainPanel);
			form.Controls.Add(bottomPanel);
			form.Controls.SetChildIndex(bottomPanel, 0);
			form.Controls.SetChildIndex(mainPanel, 0);
			form.Controls.SetChildIndex(form.MainStatusBar, 0);
			return form;
		});
		Assert.That(rendered.Find(".statusbar"), Is.Not.Null);
		var statusBarTop = form.Bounds.Height - form.MainStatusBar.Bounds.Height;
		Assert.That(rendered.Find(".statusbar").GetAttribute("style"), Does.Contain("top:" + statusBarTop + "px"));
	}
}
