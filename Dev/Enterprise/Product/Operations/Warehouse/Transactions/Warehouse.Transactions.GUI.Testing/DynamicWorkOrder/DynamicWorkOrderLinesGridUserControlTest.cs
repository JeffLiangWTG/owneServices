using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	abstract class DynamicWorkOrderLinesGridUserControlTest<TUserControl> : DocketLinesGridUserControlTest<TUserControl>
		where TUserControl : DynamicWorkOrderLinesGridUserControl, new()
	{
		#region TestConstructor

		protected override void TestConstructorCore(TUserControl userControl)
		{
			base.TestConstructorCore(userControl);

			AssertEquals("ColumnLayoutContext.", ExpectedGridContext.ToString(), userControl.LinesGrid.ColumnLayoutContext);

			// Columns for the Qty's witout matching lines should *NOT* be in the list
			AssertEquals("Should not have the Is BOM column as BOM products are not valid on Dynamic Work Orders.", true, userControl.LinesGrid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>().Single(i => i.ColumnName == WhsDocketLine.Schema.IsBOMProduct).IsUnavailable);
		}

		protected abstract DocketLinesGridContext ExpectedGridContext { get; }

		#endregion

		#region Overrides

		protected override bool TestDocketSubTypeChanged => false; // Sub type doesn't change column visibility for dynamic work orders 

		protected override bool ExpectDuplicateMenuItemAdded => false; // It's not really valid to have lines that are similar on a DWO (e.g. they will have different products, flags etc.)

		#endregion

		#region Implementation

		protected override WhsDocket GetNewDocket() => Factory.New<WhsDynamicWorkOrder>();

		protected override TUserControl GetNewDocketLinesGridUserControl() => new TUserControl();

		protected override DocketLinesTestForm GetNewDocketLinesTestForm(WhsDocket docket) => new DynamicWorkOrderLinesTestForm((WhsDynamicWorkOrder)docket);

		class DynamicWorkOrderLinesTestForm : DocketLinesTestForm
		{
			public DynamicWorkOrderLinesTestForm(WhsDynamicWorkOrder workOrder)
				: base(workOrder)
			{
			}

			protected override TUserControl GetNewDocketLinesGridUserControl() => new TUserControl();
		}

		#endregion
	}
}
