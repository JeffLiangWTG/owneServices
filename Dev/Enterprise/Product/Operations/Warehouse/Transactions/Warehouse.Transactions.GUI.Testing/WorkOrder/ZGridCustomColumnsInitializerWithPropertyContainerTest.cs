using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class ZGridCustomColumnsInitializerWithPropertyContainerTest : WhsGuiTestCaseWithFactory
	{
		public void TestConstructor()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2, 2m, "UNT");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.Part1, 5m);
			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			using (var form = new TestForm(workOrder))
			{
				form.Show();

				AssertExceptionThrown<ArgumentNullException>(() => new ZGridCustomColumnsInitializerWithPropertyContainer(form.SomeGrid, workOrder.Lines, null, null));
			}
		}

		public void TestPropertyContainer()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2, 2m, "UNT");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.Part1, 5m);
			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			using (var form = new TestForm(workOrder))
			{
				form.Show();

				var propertyContainer = new CustomPropertyContainer<WhsReceiveLine>();
				propertyContainer.AddCustomProperty("LineNo", typeof(ZShort), l => l.WE_LineNo);
				var columnHelper = new ZGridCustomColumnsInitializerWithPropertyContainer(form.SomeGrid, workOrder.Lines, propertyContainer, null);
				columnHelper.AddCustomColumns(propertyContainer.CustomProperties);

				var columnStyles = form.SomeGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				var column = columnStyles.Single(i => i.ColumnName == "LineNo");
				var descriptor = ((IOverridablePropertyDescriptor)column).PropertyDescriptor;

				var receiveLine = workOrder.Receive.Lines[0];
				AssertEquals((ZShort)1, descriptor.GetValue(receiveLine));

				receiveLine.WE_LineNo = 5;
				AssertEquals((ZShort)5, descriptor.GetValue(receiveLine));
			}
		}

		class TestForm : ZForm
		{
			public TestForm(WhsWorkOrder order)
				: base(order.Receive)
			{
			}

			public ZGrid SomeGrid;

			protected override void InitializeComponent()
			{
				SomeGrid = new ZGrid();
				SomeGrid.BindTo = "Lines";
				var column = new ZTextBoxColumnStyleInfo { ColumnName = "WE_LineComment" };
				SomeGrid.Columns.Add(column);

				Controls.Add(SomeGrid);
				DataSourceAssemblyName = "Enterprise.Warehouse.Transactions.Business";
				DataSourceTypeName = "Enterprise.Warehouse.Transactions.Business.WhsReceive";
			}
		}
	}
}
