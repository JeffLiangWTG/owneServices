using System.Threading.Tasks;
using Bunit;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.GUI;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class VisualBoardPreviewUserControlTest
{
	[Test]
	public async Task TestVisualBoardPreviewShouldNotBeEditable()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var system = VisualBoardsTestHelper.CreateSystem(factory, "XYZ");
			var bucket = VisualBoardsTestHelper.CreateBucket(system);
			var section = VisualBoardsTestHelper.CreateBoardSection(bucket);

			var visualBoardPreview = new VisualBoardPreviewUserControl();
			visualBoardPreview.UpdatePreview(new[] { section });

			var form = new ZForm(system);
			form.Controls.Add(visualBoardPreview);
			return form;
		});

		var visualBoardPreviewUserControlPanel = rendered.Find("div[data-type='Enterprise.BufferManagement.GUI.VisualBoardPreviewUserControl']");
		Assert.That(visualBoardPreviewUserControlPanel.GetAttribute("style"), Does.Contain("pointer-events:none;"));
	}
}
