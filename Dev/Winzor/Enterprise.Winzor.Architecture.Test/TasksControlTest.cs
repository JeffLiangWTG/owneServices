using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;
class TasksControlTest
{
	[Test, WithPlaywrightPage]
	public async Task TasksControlShouldAutoCompleteTextWithCorrectSelection()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var tasksControl = new TasksControlForTest();
			var opportunity = factory.New<OrgOpportunity>();
			var task1 = opportunity.WorkflowItems.AddNew();
			tasksControl.TasksGridForTest.BindTo = "WorkflowItems";
			tasksControl.SetDataBinding(opportunity, "");
			var form = new Form() { Width = 800 };
			form.Controls.Add(tasksControl);
			return form;
		});
		var staffCell = page.Locator(".datagrid__row-star--new > td:nth-child(6)");
		await staffCell.ClickAsync();
		await Task.Delay(1000);

		await page.Keyboard.DownAsync("A");
		var textbox = page.Locator(".textbox");

		Assert.That(async () => await textbox.EvaluateAsync<string>("(e) => e.value"), Is.EqualTo("ASN").After(1000, 100));
		Assert.That(async () => await textbox.EvaluateAsync<int>("(e) => e.selectionStart"), Is.EqualTo(1).After(1000, 100));
		Assert.That(async () => await textbox.EvaluateAsync<int>("(e) => e.selectionEnd"), Is.EqualTo(3).After(1000, 100));

		await page.Keyboard.DownAsync("S");

		Assert.That(async () => await textbox.EvaluateAsync<string>("(e) => e.value"), Is.EqualTo("ASN").After(1000, 100));
		Assert.That(async () => await textbox.EvaluateAsync<int>("(e) => e.selectionStart"), Is.EqualTo(2).After(1000, 100));
		Assert.That(async () => await textbox.EvaluateAsync<int>("(e) => e.selectionEnd"), Is.EqualTo(3).After(1000, 100));

		await page.Keyboard.DownAsync("D");

		Assert.That(async () => await textbox.EvaluateAsync<string>("(e) => e.value"), Is.EqualTo("ASD").After(1000, 100));
		Assert.That(async () => await textbox.EvaluateAsync<int>("(e) => e.selectionStart"), Is.EqualTo(3).After(1000, 100));
		Assert.That(async () => await textbox.EvaluateAsync<int>("(e) => e.selectionEnd"), Is.EqualTo(3).After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task RapidInputToTextBoxInGridCheck()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var tasksControl = new TasksControlForTest();
			var opportunity = factory.New<OrgOpportunity>();
			var task1 = opportunity.WorkflowItems.AddNew();
			tasksControl.TasksGridForTest.BindTo = "WorkflowItems";
			tasksControl.SetDataBinding(opportunity, "");
			var form = new Form() { Width = 800 };
			form.Controls.Add(tasksControl);
			return form;
		});
		var staffCell = page.Locator(".datagrid__row-star--new > td:nth-child(7)");
		await staffCell.WaitForAsync();
		await staffCell.ClickAsync();

		await page.WaitForFunctionAsync("document.activeElement === document.querySelector('.textbox')");
		await page.Keyboard.TypeAsync("A");
		await page.Keyboard.TypeAsync("B");
		await page.Keyboard.TypeAsync("C");

		var textbox = page.Locator(".textbox");
		Assert.That(async () => await textbox.EvaluateAsync<string>("(e) => e.value"), Is.EqualTo("ABC").After(1000, 100));
		Assert.That(async () => await textbox.EvaluateAsync<int>("(e) => e.selectionStart"), Is.EqualTo(3).After(1000, 100));
		Assert.That(async () => await textbox.EvaluateAsync<int>("(e) => e.selectionEnd"), Is.EqualTo(3).After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task CodeBoxInGridShouldNotSelectAllAfterInputFirstCharacter()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var tasksControl = new TasksControlForTest();
			var opportunity = factory.New<OrgOpportunity>();
			var task1 = opportunity.WorkflowItems.AddNew();
			tasksControl.TasksGridForTest.BindTo = "WorkflowItems";
			tasksControl.SetDataBinding(opportunity, "");
			var form = new Form() { Width = 800 };
			form.Controls.Add(tasksControl);
			return form;
		});
		var staffCell = page.Locator(".datagrid__row-star--new > td:nth-child(7)");
		await staffCell.WaitForAsync();
		await staffCell.ClickAsync();
		await page.WaitForFunctionAsync("document.activeElement === document.querySelector('.textbox')");
		var textbox = page.Locator(".textbox");

		await textbox.PressSequentiallyAsync("A");
		await Task.Delay(100);
		await textbox.PressSequentiallyAsync("BC");
		Assert.That(async () => await textbox.EvaluateAsync<string>("(e) => e.value"), Is.EqualTo("ABC").After(1000, 100));
		Assert.That(async () => await textbox.EvaluateAsync<int>("(e) => e.selectionStart"), Is.EqualTo(3).After(1000, 100));
		Assert.That(async () => await textbox.EvaluateAsync<int>("(e) => e.selectionEnd"), Is.EqualTo(3).After(1000, 100));
	}

	public class TasksControlForTest : TasksControl
	{
		public ZArchitecture.ZGrid TasksGridForTest => TasksGrid;
	}
}
