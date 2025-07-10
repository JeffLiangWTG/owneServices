using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	internal class DragAndDropGridManagerTest : WhsTestCaseWithFactory
	{
		#region Constructor

		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructorThrowsExceptionIfGridNull()
		{
			DragAndDropGridManager manager = new DragAndDropGridManager(null);
		}

		#endregion

		#region Drag-n-Drop

		public void TestOnDragDrop()
		{
			var docket = Factory.New<WhsOrder>();
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			using (var form = new TestForm(docket))
			{
				form.Show();
				AssertEquals("Precondition no lines in docket", 0, docket.Lines.Count);

				docket.WD_WW_Whs = data.Whs1.PK;
				docket.WD_OH_Client = data.Org1.PK;
				AssertEquals("Precondition collection allows new", true, ((IBindingList)docket.Lines).AllowNew);

				form.LinesGridDragAndDropManager.OnDragDropCore(new BusinessObject[] { data.Line111 });
				AssertEquals("New line should be generated for docket", 1, docket.Lines.Count);
			}
		}

		#endregion

		#region Implementation

		class TestForm : ZForm
		{
			public TestForm(WhsDocket docket) : base(docket) { }
			public DocketLinesGridUserControl UserControl;
			public DragAndDropGridManager LinesGridDragAndDropManager;

			protected override void InitializeComponent()
			{
				this.UserControl = new DocketLinesGridUserControl();
				this.Controls.Add(this.UserControl);
				this.DataSourceAssemblyName = "Enterprise.Warehouse.Transactions.Business";
				this.DataSourceTypeName = "Enterprise.Warehouse.Transactions.Business.WhsDocket";

				this.LinesGridDragAndDropManager = new DragAndDropGridManager(UserControl.LinesGrid);
				this.LinesGridDragAndDropManager.ManageGrid();
			}

			protected override void Dispose(bool isNotFinalizing)
			{
				if (isNotFinalizing)
				{
					LinesGridDragAndDropManager.Dispose();
				}
				base.Dispose(isNotFinalizing);
			}
		}

		#endregion
	}
}
