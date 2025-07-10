using System.Linq;
using System.Threading.Tasks;
using Bunit;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.GUI.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class TaskPanelLayoutStrategyTest
{
	[Test]
	public async Task AssignCorrectZIndexToTaskCardControlsWhenLayoutCards()
	{
		const int numberOfTasks = 8;
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var group = VisualBoardsTestHelper.CreateGroup(factory, "AAA");
			var workflow = VisualBoardsTestHelper.CreateWorkflowAndParents<OrgHeader>(factory, "Container", releaseGroupPK: group.PK);
			var system = VisualBoardsTestHelper.CreateSystem(factory, "XYZ");
			var bucket = VisualBoardsTestHelper.CreateBucket(system);
			var section = VisualBoardsTestHelper.CreateBoardSection(bucket);
			var viewModel = VisualBoardsTestHelper.CreateViewModel(section, workflow);
			var taskPanel = BMSGUITestCase.CreateTaskPanel(new CellContent(0, 0, CellContentType.Cards), viewModel);

			taskPanel.Height = 500;

			for (var i = 0; i < numberOfTasks; i++)
			{
				var task = VisualBoardsTestCase.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: $"Task {i}");
				taskPanel.AddTask(task);
			}

			taskPanel.SetupTasksForTest(factory, Enumerable.Empty<IBoardFilter>());

			var form = new ZForm(system);
			form.Controls.Add(taskPanel);
			return form;
		});

		Assert.That(rendered.FindAll("[data-name='TaskCardControl']").Count, Is.EqualTo(numberOfTasks));

		for (var i = 1; i < numberOfTasks; i++)
		{
			Assert.That(rendered.Find($"[data-name='TaskCardControl']:nth-child({i})").GetAttribute("style"), Does.Contain($"z-index:{numberOfTasks - i};"));
		}

		Assert.That(rendered.Find($"[data-name='TaskCardControl']:nth-child({numberOfTasks})").GetAttribute("style"), Does.Not.Contain($"z-index"));
	}

	[Test]
	public async Task CheckTotalTasksButtonWidth()
	{
		const int numberOfTasks = 128;
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var group = VisualBoardsTestHelper.CreateGroup(factory, "AAA");
			var workflow = VisualBoardsTestHelper.CreateWorkflowAndParents<OrgHeader>(factory, "Container", releaseGroupPK: group.PK);
			var system = VisualBoardsTestHelper.CreateSystem(factory, "XYZ");
			var bucket = VisualBoardsTestHelper.CreateBucket(system);
			var section = VisualBoardsTestHelper.CreateBoardSection(bucket);
			var viewModel = VisualBoardsTestHelper.CreateViewModel(section, workflow);
			var taskPanel = BMSGUITestCase.CreateTaskPanel(new CellContent(0, 0, CellContentType.Cards), viewModel);

			taskPanel.Height = 500;

			for (var i = 0; i < numberOfTasks; i++)
			{
				var task = VisualBoardsTestCase.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: $"Task {i}");
				taskPanel.AddTask(task);
			}

			taskPanel.SetupTasksForTest(factory, Enumerable.Empty<IBoardFilter>());

			var form = new ZForm(system);
			form.Controls.Add(taskPanel);
			return form;
		});

		var totalTasksButton = rendered.Find("[data-name='TaskPanel'] button");

		Assert.That(totalTasksButton.TextContent, Is.EqualTo(numberOfTasks.ToString()));
		Assert.That(totalTasksButton.GetAttribute("style"), Does.Contain("width:31px"));
	}
}
