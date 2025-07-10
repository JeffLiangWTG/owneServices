using System.Threading.Tasks;
using Bunit;
using CargoWise.EntityFramework;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class BoardControlsPanelTest
{
	[Test]
	public async Task TestAppliedFilter_HighlightButton()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var system = VisualBoardsTestHelper.CreateSystem(factory);
			var group = VisualBoardsTestHelper.CreateGroup(factory, "AAA");
			var bucket = VisualBoardsTestHelper.CreateBucket(system);
			var section = VisualBoardsTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			var bufferBoard = section.Board;
			var form = new VisualBoardForm(VisualBoardsTestHelper.CreateSlideshowViewModel(bufferBoard));

			var panel = form.controlsPanel;
			panel.Expand();

			var searchControl = panel.SearchBox;
			searchControl.SearchTerm = "test";
			searchControl.OnSearchPerformed(false);
			return form;
		});
		var filterButton = rendered.Find("button.button.button--standard");
		Assert.That(filterButton, Is.Not.Null);
		Assert.That(filterButton.GetAttribute("style"), Does.Contain("outline: 1px solid red;"));
	}
}
