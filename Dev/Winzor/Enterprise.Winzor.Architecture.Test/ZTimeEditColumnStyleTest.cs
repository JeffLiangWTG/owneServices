using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class ZTimeEditColumnStyleTest
{
	[Test]
	public async Task CheckTimeEditColumnTextAlignmentStyleAndWhiteSpaceStyleTest()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			var dummy1 = collection.AddNew();
			dummy1.Z0_SmallDateTime = new ZDateTime(2022, 1, 1, 11, 59, 59);
			var form = new WinzorTestForm();
			var grid = new ZGrid { Dock = DockStyle.Fill };
			grid.ColumnStyles.Add(new ZTimeEditExColumnStyleInfo(DummyBizoSchema.Constants.Z0_SmallDateTime, 80));
			form.Controls.Add(grid);
			grid.SetDataBinding(collection, "");
			return form;
		});
		var grid = rendered.FindAll("tbody>tr>td");
		Assert.That(grid[1].ClassList.ToString(), Does.Contain("datagrid__cell--edit"));
		Assert.That(grid[1].GetAttribute("style"), Does.Contain("text-align: left;"));
		Assert.That(grid[1].GetAttribute("style"), Does.Contain("white-space: pre;"));
	}

	[TestCase("2022-1-1T00:00:59", "000:00", TestName = "CheckWithSecond")]
	[TestCase("2022-1-1T00:59:00", "000:59", TestName = "CheckWithMinute")]
	[TestCase("2022-1-1T00:59:59", "000:59", TestName = "CheckWithMinuteSecond")]
	[TestCase("2022-1-1T11:00:00", "011:00", TestName = "CheckWithHour")]
	[TestCase("2022-1-1T11:59:00", "011:59", TestName = "CheckWithHourMinute")]
	[TestCase("2022-1-1T11:59:59", "011:59", TestName = "CheckWithHourMinuteSecond")]
	[TestCase("2022-3-1T11:59:59", "1427:59", TestName = "CheckExceed24HourDisplay")]
	public async Task CheckTimeEditColumnMarkupTest(string inputDateTime, string expectedDisplayDateTime)
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			var dummy1 = collection.AddNew();
			dummy1.Z0_SmallDateTime = new ZDateTime(inputDateTime);
			var form = new WinzorTestForm();
			var grid = new ZGrid { Dock = DockStyle.Fill };
			grid.ColumnStyles.Add(new ZTimeEditExColumnStyleInfo(DummyBizoSchema.Constants.Z0_SmallDateTime, 80));
			form.Controls.Add(grid);
			grid.SetDataBinding(collection, "");
			return form;
		});
		var textbox = rendered.Find(".textbox");
		Assert.That(textbox.GetAttribute("value"), Is.EqualTo(expectedDisplayDateTime));
	}

	[TestCase(638396663999999999, "000:40")]
	[TestCase(638396664000000000, "000:40")]
	public async Task PreventTimeEditLoses1Minute(long inputDateTime, string expectedDisplayDateTime)
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			var dummy1 = collection.AddNew();
			dummy1.Z0_SmallDateTime = new ZDateTime(inputDateTime);
			var form = new WinzorTestForm();
			var grid = new ZGrid { Dock = DockStyle.Fill };
			grid.ColumnStyles.Add(new ZTimeEditExColumnStyleInfo(DummyBizoSchema.Constants.Z0_SmallDateTime, 80));
			form.Controls.Add(grid);
			grid.SetDataBinding(collection, "");
			return form;
		});
		var textbox = rendered.Find(".textbox");
		Assert.That(textbox.GetAttribute("value"), Is.EqualTo(expectedDisplayDateTime));
	}
}
