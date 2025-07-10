using System.Drawing;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.GUI.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;
public class TaskPanelTest
{
	[Test, WithPlaywrightPage]
	public async Task TestBackgroundFadeColorChanged()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var group = VisualBoardsTestHelper.CreateGroup(factory, "AAA");
			var workflow = VisualBoardsTestHelper.CreateWorkflowAndParents<OrgHeader>(factory, "Container", releaseGroupPK: group.PK);
			var system = VisualBoardsTestHelper.CreateSystem(factory, "XYZ");
			var bucket = VisualBoardsTestHelper.CreateBucket(system);
			var section = VisualBoardsTestHelper.CreateBoardSection(bucket);
			var viewModel = VisualBoardsTestHelper.CreateViewModel(section, workflow);

			var cell = viewModel.ComponentGrid[0, 0];
			cell.BackColor = Color.AliceBlue;
			cell.BackgroundFadeColor = cell.BackColor.Value.FadeTowardsWhite();

			var panel = BMSGUITestCase.CreateTaskPanel(cell, viewModel);
			var form = new ZForm();
			form.Controls.Add(panel);
			return form;
		});
		var taskPanel = page.Locator("[data-name='TaskPanel']");
		Assert.That(() => taskPanel.GetComputedStyleAsync("background-image"), Is.EqualTo("linear-gradient(rgb(240, 248, 255), rgb(247, 251, 255))"));
	}

	[Test]
	public void RenderingTaskPanel_DoesNotThrowException_WhenCellContentBackColorIsNull()
	{
		using var ctx = new EnterpriseTestContext();
		var formConstructor = () =>
		{
			var factory = new BusinessObjectFactory();
			var group = VisualBoardsTestHelper.CreateGroup(factory, "AAA");
			var workflow = VisualBoardsTestHelper.CreateWorkflowAndParents<OrgHeader>(factory, "Container", releaseGroupPK: group.PK);
			var system = VisualBoardsTestHelper.CreateSystem(factory, "XYZ");
			var bucket = VisualBoardsTestHelper.CreateBucket(system);
			var section = VisualBoardsTestHelper.CreateBoardSection(bucket);
			var viewModel = VisualBoardsTestHelper.CreateViewModel(section, workflow);

			var cell = viewModel.ComponentGrid[0, 0];
			cell.BackColor = null;
			cell.BackgroundFadeColor = Color.AliceBlue;

			var panel = BMSGUITestCase.CreateTaskPanel(cell, viewModel);
			var form = new ZForm();
			form.Controls.Add(panel);
			return form;
		};
		Assert.That(() => ctx.RenderFormAsync(formConstructor), Throws.Nothing);
	}
}
