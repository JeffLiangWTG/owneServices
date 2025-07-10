using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.GUI;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

class TaskCardControlTest
{
	[Test]
	public async Task TaskCardControlLayoutTest()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var group = VisualBoardsTestHelper.CreateGroup(factory, "AAA");
			var workflow = VisualBoardsTestHelper.CreateWorkflowAndParents<OrgHeader>(factory, "Container", releaseGroupPK: group.PK);
			var task1 = VisualBoardsTestCase.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 1");
			var system = VisualBoardsTestHelper.CreateSystem(factory, "XYZ");
			var bucket = VisualBoardsTestHelper.CreateBucket(system);
			var section = VisualBoardsTestHelper.CreateBoardSection(bucket);
			var viewModel = VisualBoardsTestHelper.CreateViewModel(section, workflow);
			var task1Card = new TaskCardControl(task1, viewModel);

			var form = new ZForm(system);
			form.Controls.Add(task1Card);
			return form;
		});

		Assert.That(rendered.Find("div[data-type='Enterprise.BufferManagement.GUI.TaskCardControl']").GetAttribute("style"), Does.Contain("contain:layout;"));
	}

	[Test]
	public async Task TaskCardControlDarkerShadingAppliedCorrectlyAccordingToOrder()
	{
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

			var task1 = VisualBoardsTestCase.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 1");
			var task1Card = new TaskCardControl(task1, viewModel);

			var task2 = VisualBoardsTestCase.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 2");
			var task2Card = new TaskCardControl(task2, viewModel);

			var task3 = VisualBoardsTestCase.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 3");
			var task3Card = new TaskCardControl(task3, viewModel);

			var cell = new CellContent(0, 0, CellContentType.Cards);
			var menuStrip = TaskCardControl.CreateMenuStrip();

			var taskPanel = new TaskPanel(cell, viewModel, menuStrip);
			taskPanel.Controls.Add(task1Card);
			taskPanel.Controls.Add(task2Card);
			taskPanel.Controls.Add(task3Card);

			var form = new ZForm(system);
			form.Controls.Add(taskPanel);
			return form;
		});

		Assert.That(rendered.Find("div[data-type='Enterprise.BufferManagement.GUI.TaskCardControl']:nth-child(1)").GetAttribute("style"), Does.Not.Contain("filter:brightness(75%);"));
		Assert.That(rendered.Find("div[data-type='Enterprise.BufferManagement.GUI.TaskCardControl']:nth-child(2)").GetAttribute("style"), Does.Contain("filter:brightness(75%);"));
		Assert.That(rendered.Find("div[data-type='Enterprise.BufferManagement.GUI.TaskCardControl']:nth-child(3)").GetAttribute("style"), Does.Contain("filter:brightness(75%);"));
	}

	[Test]
	public async Task TaskCardControlDarkerShadingAppliedCorrectlyAfterClickingTaskCard()
	{
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

			var task1 = VisualBoardsTestCase.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 1");
			var task1Card = new TaskCardControl(task1, viewModel);

			var task2 = VisualBoardsTestCase.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 2");
			var task2Card = new TaskCardControl(task2, viewModel);

			var task3 = VisualBoardsTestCase.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 3");
			var task3Card = new TaskCardControl(task3, viewModel);

			var cell = new CellContent(0, 0, CellContentType.Cards);
			var menuStrip = TaskCardControl.CreateMenuStrip();

			var taskPanel = new TaskPanel(cell, viewModel, menuStrip);
			taskPanel.Controls.Add(task1Card);
			taskPanel.Controls.Add(task2Card);
			taskPanel.Controls.Add(task3Card);

			task2Card.OnTaskCardControlClicked();

			var form = new ZForm(system);
			form.Controls.Add(taskPanel);
			return form;
		});

		Assert.That(rendered.Find("div[data-type='Enterprise.BufferManagement.GUI.TaskCardControl']:nth-child(1)").GetAttribute("style"), Does.Not.Contain("filter:brightness(75%);"));
		Assert.That(rendered.Find("div[data-type='Enterprise.BufferManagement.GUI.TaskCardControl']:nth-child(2)").GetAttribute("style"), Does.Contain("filter:brightness(75%);"));
		Assert.That(rendered.Find("div[data-type='Enterprise.BufferManagement.GUI.TaskCardControl']:nth-child(3)").GetAttribute("style"), Does.Contain("filter:brightness(75%);"));
	}

	[Test]
	public async Task TaskCardWithOneTag()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();

			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(factory);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(jobHeader, completionStatement: "Container");

			var tagDef = VisualBoardsTestHelper.CreateTagDefinition(factory, "red");
			var tagMagnitude = VisualBoardsTestHelper.CreateTagMagnitude(tagDef, "dem", color: Color.Red);

			workflow.AddTag(tagMagnitude);

			return GenerateTestForm(factory, workflow);
		});

		var actualStyle = rendered.Find("div[data-type='Enterprise.BufferManagement.GUI.AttachedTagsIndicator']").GetAttribute("style");
		var expectStyle = "background:linear-gradient(to right,Red 100%,Red 0.1%);";
		Assert.That(actualStyle, Contains.Substring(expectStyle));
	}

	[Test]
	public async Task TaskCardOverOneTag()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();

			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(factory);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(jobHeader, completionStatement: "Container");

			var tagDef1 = VisualBoardsTestHelper.CreateTagDefinition(factory, "red");
			var tagMagnitude1 = VisualBoardsTestHelper.CreateTagMagnitude(tagDef1, "de1", color: Color.Red);

			var tagDef2 = VisualBoardsTestHelper.CreateTagDefinition(factory, "blu");
			var tagMagnitude2 = VisualBoardsTestHelper.CreateTagMagnitude(tagDef2, "de2", color: Color.Blue);

			var tagDef3 = VisualBoardsTestHelper.CreateTagDefinition(factory, "gre");
			var tagMagnitude3 = VisualBoardsTestHelper.CreateTagMagnitude(tagDef3, "de3", color: Color.Green);

			var tagDef4 = VisualBoardsTestHelper.CreateTagDefinition(factory, "bla");
			var tagMagnitude4 = VisualBoardsTestHelper.CreateTagMagnitude(tagDef4, "de4", color: Color.Black);

			workflow.AddTag(tagMagnitude1);
			workflow.AddTag(tagMagnitude2);
			workflow.AddTag(tagMagnitude3);
			workflow.AddTag(tagMagnitude4);

			return GenerateTestForm(factory, workflow);
		});

		var actualStyle = rendered.Find("div[data-type='Enterprise.BufferManagement.GUI.AttachedTagsIndicator']").GetAttribute("style");
		var expectStyle = "background:linear-gradient(to right,Red 25%,Blue 0.1%,Blue 50%,Green 0.1%,Green 75%,Black 0.1%,Black 100%);";
		Assert.That(actualStyle, Contains.Substring(expectStyle));
	}

	[TestCase(VisualBoardButtonBorderStyle.Solid, "border: 1px Solid #000000FF;")]
	[TestCase(VisualBoardButtonBorderStyle.Dotted, "border: 1px Dotted #000000FF;")]
	public async Task TaskCardBordersSolid(VisualBoardButtonBorderStyle borderStyle, string expectStyle)
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();

			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(factory);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(jobHeader, completionStatement: "Container");

			var tagDef = VisualBoardsTestHelper.CreateTagDefinition(factory, "bla");
			var tagMagnitude = VisualBoardsTestHelper.CreateTagMagnitude(tagDef, "dem", borderStyle: borderStyle);

			workflow.AddTag(tagMagnitude);

			return GenerateTestForm(factory, workflow);
		});

		var actualStyle = rendered.FindAll("div[data-type='Enterprise.ZArchitecture.GUI.ZPanel']")[1].GetAttribute("style");

		Assert.That(actualStyle, Contains.Substring(expectStyle));
	}

	[Test]
	public async Task TaskCardTextBoxDisabled()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var system = BMSTestHelper.CreateSystem(factory, "XYZ");
			var bucket = BMSTestHelper.CreateBucket(system);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(factory);

			var customisation = factory.New<BMControlCustomisation>();
			customisation.FM_ControlType = CustomisedControlTypeList.Codes.TaskCard;
			customisation.FM_Name = "Mai Task Card";

			var line = customisation.CustomisationLines.AddNew();
			line.PropertySource = PropertySourceList.Codes.ProcessTask;
			line.PropertyName = "EstimatedTimeToCompleteHours";
			line.ControlType = PropertyTypeList.Codes.Number;

			var line2 = customisation.CustomisationLines.AddNew();
			line2.PropertyName = "EstimatedHandoverDate";
			line2.ControlType = PropertyTypeList.Codes.DateTime;

			var group = BMSTestHelper.CreateGroup(factory, "AAA");

			var workflow = jobHeader.ProcessHeaders.AddNew();
			workflow.FH_GG_ReleaseGroup = group.PK;
			var task = workflow.Parent.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;

			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			BMSTestHelper.CreateControlCustomisationLink(factory, section, customisation);

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			var taskCard = new TaskCardControl(task, viewModel);

			var form = new ZForm();
			form.Controls.Add(taskCard);

			return form;
		});

		var isDisabled = rendered.Find("div[data-type='Enterprise.BufferManagement.GUI.TaskCardControl'] .textbox").GetAttribute("disabled");
		Assert.That(isDisabled, Is.EqualTo(String.Empty));

		var isDisabledDateEdit = rendered.Find("div[data-type='Enterprise.ZArchitecture.GUI.ZDateEdit'] .textbox").GetAttribute("disabled");
		Assert.That(isDisabledDateEdit, Is.EqualTo(String.Empty));
	}

	ZForm GenerateTestForm(BusinessObjectFactory factory, ProcessHeader workflow)
	{
		var system = VisualBoardsTestHelper.CreateSystem(factory, "XYZ");
		var bucket = VisualBoardsTestHelper.CreateBucket(system);
		var section = VisualBoardsTestHelper.CreateBoardSection(bucket);
		var viewModel = VisualBoardsTestHelper.CreateViewModel(section, workflow);
		var task1 = VisualBoardsTestCase.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 1");
		var taskCard = new TaskCardControl(task1, viewModel);

		var cell = new CellContent(0, 0, CellContentType.Cards);
		using var menuStrip = TaskCardControl.CreateMenuStrip();

		var taskPanel = new TaskPanel(cell, viewModel, menuStrip);
		taskPanel.Controls.Add(taskCard);

		var form = new ZForm(system);
		form.Controls.Add(taskPanel);
		return form;
	}

	[Test, WithPlaywrightPage]
	public async Task TaskCardShouldHaveCorrectZIndex()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();

			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(factory);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(jobHeader, completionStatement: "Container");

			var tagDef = VisualBoardsTestHelper.CreateTagDefinition(factory, "bla");
			var tagMagnitude = VisualBoardsTestHelper.CreateTagMagnitude(tagDef, "dem", borderStyle: VisualBoardButtonBorderStyle.Solid);

			workflow.AddTag(tagMagnitude);

			return GenerateTestForm(factory, workflow);
		});

		  var borderPanelStyle = await page.WaitForSelectorAsync("div[data-type='Enterprise.BufferManagement.GUI.TaskCardControl'] > div:nth-child(2)");
		var noteTextPanelStyle = await page.WaitForSelectorAsync("div[data-type='Enterprise.BufferManagement.GUI.TaskCardControl'] > div:nth-child(1)");
		var attachedTagsIndicatorStyle = await page.WaitForSelectorAsync("div[data-type='Enterprise.BufferManagement.GUI.AttachedTagsIndicator']");

		Assert.That(await noteTextPanelStyle.GetComputedStyleAsync("z-index"), Is.EqualTo("7"));
		Assert.That(await attachedTagsIndicatorStyle.GetComputedStyleAsync("z-index"), Is.EqualTo("7"));
	}

	[Test]
	public async Task TestControlRightClickPosition()
	{
		TaskPanel taskPanel = null;
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

			var task1 = VisualBoardsTestCase.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 1");
			var task1Card = new TaskCardControl(task1, viewModel);
			var cell = new CellContent(0, 0, CellContentType.Cards);

			var menuStrip = TaskCardControl.CreateMenuStrip();
			menuStrip.Items.Add("Menu1");
			menuStrip.Items.Add("Menu2");

			taskPanel = new TaskPanel(cell, viewModel, menuStrip);
			taskPanel.ContextMenu = new ContextMenu();
			taskPanel.ContextMenu.MenuItems.Add(new MenuItem("Static Item"));
			taskPanel.Controls.Add(task1Card);

			var form = new ZForm(system);
			form.Controls.Add(taskPanel);
			return form;
		});

		var panel = rendered.Find(".panel");
		Assert.That(panel, Is.Not.Null);

		await panel.ClickAsync(new WebMouseEventArgs());

		var leftClickPosition = new Point(50, 50); 
		var rightClickPosition = new Point(2, 2);

		await panel.ClickAsync(new WebMouseEventArgs
		{
			ClientX = leftClickPosition.X,
			ClientY = leftClickPosition.Y
		});
		Assert.That(Control.MousePosition, Is.EqualTo(leftClickPosition));

		await panel.TriggerEventAsync("oncontextmenu", new WebMouseEventArgs
		{
			Button = 2,
			Type = "contextmenu",
			ClientX = rightClickPosition.X,
			ClientY = rightClickPosition.Y
		});
		Assert.That(Control.MousePosition, Is.EqualTo(rightClickPosition));
	}

	[Test, WithPlaywrightPage]
	public async Task TaskCardControlBorderPanelShouldHaveZIndexNotGreaterThanAllStaticSubControlsWithStayOnTopAttribute()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var system = BMSTestHelper.CreateSystem(factory, "XYZ");
			var bucket = BMSTestHelper.CreateBucket(system);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(factory);

			var customisation = factory.New<BMControlCustomisation>();
			customisation.FM_ControlType = CustomisedControlTypeList.Codes.TaskCard;
			customisation.FM_Name = "Main Task Card";

			var line1 = customisation.CustomisationLines.AddNew();
			line1.PropertyName = "line1";
			line1.ControlType = PropertyTypeList.Codes.Text;

			var line2 = customisation.CustomisationLines.AddNew();
			line2.PropertyName = "line2";
			line2.ControlType = PropertyTypeList.Codes.Text;

			var line3 = customisation.CustomisationLines.AddNew();
			line3.PropertyName = "line3";
			line3.ControlType = PropertyTypeList.Codes.Text;
			line3.BringToFront = true;

			var line4 = customisation.CustomisationLines.AddNew();
			line4.PropertyName = "line4";
			line4.ControlType = PropertyTypeList.Codes.Text;
			line4.BringToFront = true;

			var group = BMSTestHelper.CreateGroup(factory, "AAA");

			var workflow = jobHeader.ProcessHeaders.AddNew();
			workflow.FH_GG_ReleaseGroup = group.PK;
			var task = workflow.Parent.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;

			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			BMSTestHelper.CreateControlCustomisationLink(factory, section, customisation);

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);
			var taskCard = new TaskCardControl(task, viewModel);

			var form = new ZForm();
			form.Controls.Add(taskCard);
			return form;
		});

		var borderPanel = await page.WaitForSelectorAsync("div[data-type='Enterprise.BufferManagement.GUI.TaskCardControl'] > div:nth-child(2)");
		var zUserControl = await page.WaitForSelectorAsync("div[data-type='Enterprise.ZArchitecture.GUI.ZUserControl']");

		var children = await zUserControl.GetChildrenAsync();
		Assert.That((await children[0].GetComputedStyleAsync("z-index")).Raw, Is.EqualTo("auto"));
		Assert.That(await children[1].GetComputedStyleAsync("z-index"), Is.EqualTo("1"));
		Assert.That(await children[2].GetComputedStyleAsync("z-index"), Is.EqualTo("4"));
		Assert.That(await children[3].GetComputedStyleAsync("z-index"), Is.EqualTo("3"));
	}

	[Test]
	public async Task TaskCardEstimateDurationShouldNotLoseOneMinute()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var group = VisualBoardsTestHelper.CreateGroup(factory, "AAA");
			var workflow = VisualBoardsTestHelper.CreateWorkflowAndParents<OrgHeader>(factory, "Container", releaseGroupPK: group.PK);
			var task1 = VisualBoardsTestCase.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 1");
			task1.P9_EstDuration = new ZInt(20).GetDateTimeFromMinutes();
			task1.P9_EstimateVariationFactor = 2;

			var system = VisualBoardsTestHelper.CreateSystem(factory, "XYZ");
			var bucket = VisualBoardsTestHelper.CreateBucket(system);
			var section = VisualBoardsTestHelper.CreateBoardSection(bucket);
			var viewModel = VisualBoardsTestHelper.CreateViewModel(section, workflow);
			var task1Card = new TaskCardControl(task1, viewModel);

			var form = new ZForm();
			form.Controls.Add(task1Card);
			return form;
		});

		Assert.That(rendered.FindAll(".label")[1].TextContent, Is.EqualTo("0:30"));
	}

	[Test]
	public async Task TestTaskControlClickPropagationToOtherControls()
	{
		TaskPanel taskPanel = null;
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

			var task1 = VisualBoardsTestCase.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 1");
			var task1Card = new TaskCardControl(task1, viewModel);
			var cell = new CellContent(0, 0, CellContentType.Cards);

			var menuStrip = TaskCardControl.CreateMenuStrip();

			taskPanel = new TaskPanel(cell, viewModel, menuStrip);
			taskPanel.Height = 30;
			taskPanel.Width = 70;
			taskPanel.Controls.Add(task1Card);

			var form = new ZForm(system);
			form.Controls.Add(taskPanel);
			return form;
		});

		var taskCardControl = rendered.Find("[data-type='Enterprise.BufferManagement.GUI.TaskCardControl']");
		Assert.That(taskCardControl, Is.Not.Null);
		Assert.That(taskCardControl.GetAttribute("style"), Does.Contain("top:0px"));
		Assert.That(taskCardControl.GetAttribute("style"), Does.Contain("left:0px"));

		await taskCardControl.ClickAsync(new WebMouseEventArgs());
		taskCardControl = rendered.Find("[data-type='Enterprise.BufferManagement.GUI.TaskCardControl']");

		Assert.That(taskCardControl.GetAttribute("style"), Does.Contain("top:0px"));
		Assert.That(taskCardControl.GetAttribute("style"), Does.Contain("left:0px"));
	}
}
