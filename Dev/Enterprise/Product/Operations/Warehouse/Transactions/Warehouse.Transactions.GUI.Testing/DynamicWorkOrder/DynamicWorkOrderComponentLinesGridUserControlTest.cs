using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class DynamicWorkOrderComponentLinesGridUserControlTest : DynamicWorkOrderLinesGridUserControlTest<DynamicWorkOrderComponentLinesGridUserControl>
	{
		#region TestConstructor

		protected override void TestConstructorCore(DynamicWorkOrderComponentLinesGridUserControl userControl)
		{
			base.TestConstructorCore(userControl);

			AssertEquals(true, userControl.LinesGrid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>().Any(i => i.ColumnName == WhsDocketLineSchema.Constants.WE_BondedEntryKey));
			AssertEquals(true, userControl.LinesGrid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>().Any(i => i.ColumnName == nameof(WhsPickableDocketLine.SumOfUnitsMet)));

			var productColumnStyle = userControl.LinesGrid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>().Single(i => i.ColumnName == nameof(WhsDocketLine.WE_OP));
			AssertEquals(typeof(ZMultiControlColumnStyle), productColumnStyle.ColumnStyleType);
			AssertEquals(nameof(WhsDynamicWorkOrderLine.ProductFieldType), ((ZMultiControlColumnStyleInfo)productColumnStyle).FieldTypeColumnName);
		}

		#endregion

		#region TestIsInwardProcessingJobChanged

		public void TestIsInwardProcessingJobChanged()
		{
			var whs1 = Helper.CreateWarehouse("1");
			whs1.WW_IsVirtualWarehouse = true;
			Helper.CreateArea(whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var docket = GetNewDocket();
			docket.WD_WW_Whs = whs1.PK;

			docket.WD_IsInwardsProcessingJob = false;
			using (var form = GetNewDocketLinesTestForm(docket))
			{
				form.Show();
				var userControl = form.UserControl;
				AssertEquals(false, userControl.LinesGrid.Columns.Contains(WhsDocketLineSchema.Constants.WE_BondedEntryKey));

				docket.WD_IsInwardsProcessingJob = true;
				AssertEquals(true, userControl.LinesGrid.Columns.Contains(WhsDocketLineSchema.Constants.WE_BondedEntryKey));

				docket.WD_IsInwardsProcessingJob = false;
				AssertEquals(false, userControl.LinesGrid.Columns.Contains(WhsDocketLineSchema.Constants.WE_BondedEntryKey));
			}
		}

		#endregion

		#region Implementation

		protected override string ControlBindTo => "Lines.ChildComponentLinesCollection";

		protected override DocketLinesGridContext ExpectedGridContext => DocketLinesGridContext.DynamicWorkOrderComponents;

		protected override WhsDocket GetNewDocket()
		{
			var workOrder = Factory.New<WhsDynamicWorkOrder>();
			var mainLine = workOrder.Lines.AddNew();
			mainLine.IsMainInwardProcessedItem = true;

			return workOrder;
		}

		#endregion
	}
}
