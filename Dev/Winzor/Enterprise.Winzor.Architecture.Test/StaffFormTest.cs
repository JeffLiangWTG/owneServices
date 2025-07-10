using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class StaffFormTest
{
	[Test]
	public async Task TextBoxShouldHaveRightWidth()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(new TaskWithDetailsControl());
			return form;
		});
		await rendered.Find(".tabcontrol .tabcontrol__navigation .tabcontrol__button:nth-child(2)").ClickAsync(new WebMouseEventArgs());
		var textBox = rendered.Find("fieldset.groupbox input:nth-child(4)").ToMarkup();
		Assert.That(textBox, Contains.Substring("width:303px"));
	}

	[Test]
	public async Task WorkingHoursShouldBeReadOnlyInViweMode()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			var form = new GlbStaffForm(staff);
			form.DisplayMode = ODisplayMode.ReadOnly;
			return form;
		});

		await rendered.InvokeAsync(() => rendered.Find("div.tabcontrol__navigation > .tabcontrol__tabs > .tabcontrol__button:nth-child(6)").ClickAsync(new WebMouseEventArgs()));
		await rendered.InvokeAsync(() => rendered.Find("div.tabcontrol__content > .tabcontrol__page .tabcontrol__button:nth-child(2)").ClickAsync(new WebMouseEventArgs()));

		var textBox = rendered.Find("fieldset.groupbox input:nth-child(4)").ToMarkup();
		Assert.That(textBox, Contains.Substring("readonly"));
	}

	[Test]
	public async Task CheckBoxShouldHaveSameBackColorAsItsParent()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			var form = new GlbStaffForm(staff);
			return form;
		});

		var markup = rendered.Find("div.panel label.checkbox").ToMarkup();
		Assert.That(markup, Contains.Substring("background-color:#DCE1E4FF"));
	}
}
