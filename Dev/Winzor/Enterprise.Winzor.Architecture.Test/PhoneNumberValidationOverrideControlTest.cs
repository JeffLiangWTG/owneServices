using System.Threading.Tasks;
using Bunit;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.GUI;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class PhoneNumberValidationOverrideControlTest
{
	[Test]
	public async Task HeaderLabelShouldOverlapPanelByHavingZIndexEqualToOne()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var dummy = new BusinessObjectFactory().New<DummyBusinessObject>();
			form.Controls.Add(new PhoneNumberValidationOverrideControl(dummy.Z0_BoolInfo, "blah"));
			return form;
		});

		Assert.That(rendered.Find("[data-type='Enterprise.MasterFiles.GUI.PhoneNumberValidationOverrideControl'] > .label").GetAttribute("style"), Does.Contain("z-index:1"));
	}

	[Test]
	public async Task PhoneNumberValidationHasBorder()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var dummy = new BusinessObjectFactory().New<DummyBusinessObject>();
			form.Controls.Add(new PhoneNumberValidationOverrideControl(dummy.Z0_BoolInfo, "blah"));
			return form;
		});

		Assert.That(rendered.Find("[data-type='Enterprise.MasterFiles.GUI.PhoneNumberValidationOverrideControl']").GetAttribute("style"), Does.Contain("border: 1px solid #646464"));
	}
}
