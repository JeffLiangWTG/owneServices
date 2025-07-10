using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.DevTools;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Playwright;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

class ZQueryAnalyzerTest
{
	[Test]
	public async Task TestErrorMessageShown()
	{
		ZQueryAnalyzerForm queryForm = null;
		ErrorReporter.SuppressReportingOfErrors = true;
		UnitTestUserNotification.Instance.ClearMessages();

		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() => queryForm = new ZQueryAnalyzerForm(""));

		await TestCommand(queryForm, "cast varchar varbinary select");
		Assert.That(UnitTestUserNotification.Instance.LastMessage.Text, Does.Contain("Incorrect syntax"));

		UnitTestUserNotification.Instance.ClearMessages();
		await TestCommand(queryForm, string.Empty);
		Assert.That(UnitTestUserNotification.Instance.LastMessage.Text, Is.Null);
	}

	[Test]
	public async Task TestSingleSQLThreadOwnershipProtect()
	{
		ZQueryAnalyzerForm queryForm = null;
		ErrorReporter.Clear();

		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() => queryForm = new ZQueryAnalyzerForm(""));

		await TestCommand(queryForm, "SELECT TOP 1 * FROM dbo.GlbBranch");
		Assert.That(ErrorReporter.LastMessageReported, Is.EqualTo(string.Empty));
	}

	[Test]
	public async Task TestTransactionCommitThreadOwnershipProtect()
	{
		ZQueryAnalyzerForm queryForm = null;
		ErrorReporter.Clear();

		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() => queryForm = new ZQueryAnalyzerForm(""));

		await TestCommand(queryForm, "begin tran");
		await TestCommand(queryForm, "SELECT TOP 1 * FROM dbo.GlbStaff");
		await TestCommand(queryForm, "SELECT TOP 1 * FROM sys.time_zone_info");
		await TestCommand(queryForm, "commit");

		Assert.That(ErrorReporter.LastMessageReported, Is.EqualTo(string.Empty));
	}

	[Test]
	public async Task TestTransactionRollBackThreadOwnershipProtect()
	{
		ZQueryAnalyzerForm queryForm = null;
		ErrorReporter.Clear();

		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() => queryForm = new ZQueryAnalyzerForm(""));

		await TestCommand(queryForm, "begin tran");
		await TestCommand(queryForm, "SELECT TOP 1 * FROM dbo.GlbStaff");
		await TestCommand(queryForm, "SELECT TOP 1 * FROM sys.time_zone_info");
		await TestCommand(queryForm, "rollback");

		Assert.That(ErrorReporter.LastMessageReported, Is.EqualTo(string.Empty));
	}

	[Test]
	public async Task TestCombinedTransactionCommitThreadOwnershipProtect()
	{
		ZQueryAnalyzerForm queryForm = null;
		ErrorReporter.Clear();

		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() => queryForm = new ZQueryAnalyzerForm(""));

		await TestCommand(queryForm, "SELECT TOP 1 * FROM dbo.GlbBranch");
		await TestCommand(queryForm, "begin tran");
		await TestCommand(queryForm, "SELECT TOP 1 * FROM dbo.GlbStaff");
		await TestCommand(queryForm, "SELECT TOP 1 * FROM sys.time_zone_info");
		await TestCommand(queryForm, "commit");
		Assert.That(ErrorReporter.LastMessageReported, Is.EqualTo(string.Empty));
	}

	[Test]
	public async Task TestCombinedTransactionRollBackThreadOwnershipProtect()
	{
		ZQueryAnalyzerForm queryForm = null;
		ErrorReporter.Clear();

		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() => queryForm = new ZQueryAnalyzerForm(""));

		await TestCommand(queryForm, "SELECT TOP 1 * FROM dbo.GlbBranch");
		await TestCommand(queryForm, "begin tran");
		await TestCommand(queryForm, "SELECT TOP 1 * FROM dbo.GlbStaff");
		await TestCommand(queryForm, "SELECT TOP 1 * FROM sys.time_zone_info");
		await TestCommand(queryForm, "rollback");
		Assert.That(ErrorReporter.LastMessageReported, Is.EqualTo(string.Empty));
	}

	[Test]
	public async Task TestIgnoreSqlCommentsWhenDecidingIsCommandNonQuery()
	{
		ZQueryAnalyzerForm queryForm = null;
		ErrorReporter.Clear();

		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() => queryForm = new ZQueryAnalyzerForm(""));

		await TestCommand(queryForm, "--update\r\nSELECT TOP 1 * FROM dbo.GlbBranch");
		var resultTextBox = queryForm.Controls.Find("ResultsTextBox", true)[0] as KTextBox;
		Assert.That(ErrorReporter.LastMessageReported, Is.EqualTo(string.Empty));
	}

	[Test, WithPlaywrightPage]
	public async Task ZQueryAnalyzerShouldRenderDataGridTableOnTypingSQLQuery()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var queryForm = new ZQueryAnalyzerForm("");
			queryForm.RunCommand("SELECT TOP 1 * FROM dbo.GlbStaff;");
			return queryForm;
		});

		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
		var thead = page.Locator(".datagrid table > thead");
		var tbody = page.Locator(".datagrid table > tbody");

		Assert.That(await thead.IsVisibleAsync(), Is.True);
		Assert.That(await tbody.IsVisibleAsync(), Is.True);
	}

	static async Task TestCommand(ZQueryAnalyzerForm queryForm, string sql)
	{
		var taskCompletionSource = new TaskCompletionSource();
		await queryForm.InvokeWinzorDispatcherAsync(() => queryForm.RunCommand(sql));
		// Wait for the command to complete
		await Task.Delay(3000);
	}
}
