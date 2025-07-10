using System.Linq;
using CargoWise.Application;
using Enterprise.Core.Forms;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class DynamicWorkOrderParentLinesGridUserControlTest : DynamicWorkOrderLinesGridUserControlTest<DynamicWorkOrderParentLinesGridUserControl>
	{
		#region TestConstructor

		protected override void TestConstructorCore(DynamicWorkOrderParentLinesGridUserControl userControl)
		{
			base.TestConstructorCore(userControl);

			AssertEquals(true, userControl.LinesGrid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>().Any(i => i.ColumnName == nameof(WhsDynamicWorkOrderLine.IsMainInwardProcessedItem)));
			AssertEquals(true, userControl.LinesGrid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>().Any(i => i.ColumnName == nameof(WhsDynamicWorkOrderLine.IsSecondaryInwardProcessedItem)));
			AssertEquals(true, userControl.LinesGrid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>().Any(i => i.ColumnName == nameof(WhsDocketLine.WE_LineComment)));
		}

		#endregion

		#region TestAllocationKeyColumn

		public void TestAllocationKeyColumn_Assembly_InwardProcessingEnabled() => TestAllocationKeyColumn(DynamicWorkOrderType.Codes.Assemble, inwardProcessingEnabled: true);

		public void TestAllocationKeyColumn_Disassembly_InwardProcessingEnabled() => TestAllocationKeyColumn(DynamicWorkOrderType.Codes.Disassemble, inwardProcessingEnabled: true);

		public void TestAllocationKeyColumn_Assembly_InwardProcessingDisabled() => TestAllocationKeyColumn(DynamicWorkOrderType.Codes.Assemble, inwardProcessingEnabled: false);

		public void TestAllocationKeyColumn_Disassembly_InwardProcessingDisabled() => TestAllocationKeyColumn(DynamicWorkOrderType.Codes.Disassemble, inwardProcessingEnabled: false);

		void TestAllocationKeyColumn(string type, bool inwardProcessingEnabled)
		{
			var whs1 = Helper.CreateWarehouse("1");
			whs1.WarehouseAddress.OA_RN_NKCountryCode = "FR";

			var docket = Factory.New<WhsDynamicWorkOrder>();
			docket.WD_DocketSubType = type;
			docket.WD_WW_Whs = whs1.PK;

			var mock = new Mock<Enterprise.Integration.Customs.ISupportedForProcessing>();
			mock.Setup(m => m.IsSupportedForProcessing()).Returns(inwardProcessingEnabled);

			using (ObjectFactory.Substitute(mock.Object))
			{
				using (var form = GetNewDocketLinesTestForm(docket))
				{
					form.Show();
					var userControl = form.UserControl;
					AssertEquals("WE_AllocationKey availability.",
						type == DynamicWorkOrderType.Codes.Disassemble && inwardProcessingEnabled,
						userControl.LinesGrid.Columns.Contains(WhsDocketLineSchema.WE_AllocationKey.Name));
				}
			}
		}

		#endregion

		protected override DocketLinesGridContext ExpectedGridContext => DocketLinesGridContext.DynamicWorkOrderParent;
	}
}
