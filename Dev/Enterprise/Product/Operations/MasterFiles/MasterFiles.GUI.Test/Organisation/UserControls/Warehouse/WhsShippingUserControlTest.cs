using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.GUI.Test.Organisation.UserControls.Warehouse
{
	public class WhsShippingUserControlTest : TestCaseWithFactory
	{
		public void TestGridColumns()
		{
			var user = Factory.New<GlbStaff>();
			user.GS_Code = "TST";
			user.GS_LoginName = "tst";
			user.GS_FullName = "Test User";
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (var whsShippingUserControl = new WhsShippingUserControl())
			{
				var expectedColumns = new[]
				{
					"OWC_OAN_CarrierAccount",
					"CarrierCode",
					"CarrierName",
					"OWC_WW_Warehouse",
					"OWC_WSH_SalesChannel",
					"OWC_BillingType",
					"OWC_OAN_BillToCarrierAccount",
					"BillToCarrierCode",
					"BillToCarrierName",
					"OWC_OAN_DutyBillToCarrierAccount",
					"DutyBillToCarrierCode",
					"DutyBillToCarrierName"
				};

				var columns = new List<string>();
				foreach (var column in ((ZGrid)whsShippingUserControl.Controls[0]).ColumnStyles.ToArray().Cast<Core.Forms.ZGridColumnInfo>())
				{
					columns.Add(column.ColumnName);
				}

				AssertContainsExactElementsInAnyOrder(expectedColumns, columns);
			}
		}
	}
}
