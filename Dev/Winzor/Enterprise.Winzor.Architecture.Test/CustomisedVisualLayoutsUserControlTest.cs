using System.Drawing;
using System.Threading.Tasks;
using Bunit;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class CustomisedVisualLayoutsUserControlTest
{
	[Test]
	public async Task TestPreviewShouldNotEditable()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var system = new BusinessObjectFactory().NewWithValidTestData<BMSystem>();
			system.FS_Description = "Herp Derble.";
			system.FS_IsLive = true;

			var customisation1 = system.CustomisedControls.AddNew();
			customisation1.FM_Name = "customisation1";
			customisation1.Width = 150;
			customisation1.Height = 100;
			customisation1.BackgroundColor = Color.Azure.Name;

			var customisation2 = system.CustomisedControls.AddNew();
			customisation2.FM_Name = "customisation2";
			customisation2.Width = 150;
			customisation2.Height = 100;
			customisation2.BackgroundColor = Color.Beige.Name;

			var form = new ZForm(system);
			form.Controls.Add(new CustomisedVisualLayoutsUserControl());
			return form;
		});

		var customisedVisualLayoutsUserControl = rendered.Find("div[data-type='Enterprise.ZArchitecture.GUI.ZUserControl']");
		Assert.That(customisedVisualLayoutsUserControl.GetAttribute("style"), Does.Contain("pointer-events:none;"));
	}
}
