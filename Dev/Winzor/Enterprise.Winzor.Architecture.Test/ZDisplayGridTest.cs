using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using static Enterprise.ZArchitecture.GUI.Testing.ZDisplayGridTest;

namespace Enterprise.Winzor.Architecture.Test;

internal class ZDisplayGridTest
{
	[Test, WithPlaywrightPage]
	public async Task TestScrollBarsEnabled()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		DummyBusinessObjectCollection dummies = null;
		ZDisplayGridForTest testGrid = null;
		ZChildForm testForm = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			dummies = new DummyBusinessObjectCollection(factory);

			testGrid = ctx.Using(new ZDisplayGridForTest());
			testForm = new ZChildForm();
			testForm.Size = new Size(500, 172);

			dummies.SetReadOnlyIncludingChildren(true);

			var columnInfo1 = new ZTextBoxColumnStyleInfo();
			columnInfo1.ColumnName = DummyBusinessObject.Schema.Z0_Code;
			columnInfo1.Width = 80;

			var columnInfo2 = new ZTextBoxColumnStyleInfo();
			columnInfo2.ColumnName = DummyBusinessObject.Schema.Z0_Description;
			columnInfo2.Width = 80;

			testGrid.Dock = DockStyle.Fill;
			testGrid.IsWholeRowSelectedOnClick = true;
			testGrid.Columns.Add(columnInfo1);
			testGrid.Columns.Add(columnInfo2);
			testForm.Controls.Add(testGrid);

			testGrid.SetDataBinding(dummies, "");
			return testForm;
		});

		Assert.That(testGrid.Columns[0].ColumnStyle.Width, Is.EqualTo(80));
		Assert.That(testGrid.Columns[1].ColumnStyle.Width, Is.EqualTo(80));
		Assert.That(() => testGrid.HorizScrollBar.Visible, Is.False.After(2000, 100));
		Assert.That(() => testGrid.VertScrollBar.Visible, Is.False.After(2000, 100));

		await testGrid.InvokeWinzorDispatcherAsync(() =>
		{
			for (var i = 0; i < 20; i++)
			{
				dummies.AddNew();
			}
		});

		Assert.That(() => testGrid.HorizScrollBar.Visible, Is.False.After(2000, 100));
		Assert.That(() => testGrid.VertScrollBar.Visible, Is.True.After(2000, 100));
		Assert.That(() => testGrid.VertScrollBar.Enabled, Is.True.After(2000, 100));

		ZChildForm modalForm = null;
		var mockContext = Mock.Of<IWinzorDispatcherContext>();

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			using (ctx.WinzorDispatcher.WithContext(mockContext))
			{
				modalForm = new ZChildForm();
				modalForm.Show(testGrid.FindForm());
				testGrid.RefreshTableStyles();
				modalForm.Close();
			}
		});

		Assert.That(() => testGrid.VertScrollBar.Enabled, Is.True.After(2000, 100));
	}
}
