using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(BaseCusDecHouseBillColumnProvider))]
	class BaseCusDecHouseBillColumnProviderTest : GridColumnProviderTest
	{
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();

			if (ExpectHBLIssueDateColumn)
			{
				AddDefaultsColumn(new ZDateTimeColumn("HBL Issue Date", nameof(Bill.CU_IssueDate)) { ColumnKey = WebTracker.Grids.CusDecHouseBills.HBLIssueDate });
			}

			AddColumn(new ZCalcEditColumn("Manifest Qty", nameof(Bill.CU_NoOfPacks)) { ColumnKey = WebTracker.Grids.CusDecHouseBills.ManifestQty });
			AddColumn(new ZTextEditColumn("UQ", nameof(Bill.CU_PackType)) { ColumnKey = WebTracker.Grids.CusDecHouseBills.UQ });
			SetupCountrySpecificColumns();
		}

		protected virtual void SetupCountrySpecificColumns()
		{
			AddDefaultsColumn(new ZTextEditColumn("Bill Number", nameof(Bill.CU_BillNum)) { ColumnKey = WebTracker.Grids.CusDecHouseBills.BillNum });
			AddDefaultsColumn(new ZCodeFindBoxColumn("Bill Type", nameof(Bill.CU_BillType), "Lookups+CU_BillTypeList") { ColumnKey = WebTracker.Grids.CusDecHouseBills.BillType });
			AddDefaultsColumn(new ZTextEditColumn("Parent Bill", nameof(Bill.CU_ParentBillUniqueCode)) { ColumnKey = WebTracker.Grids.CusDecHouseBills.ParentBill });
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new BaseCusDecHouseBillColumnProvider();
		}

		protected override bool SupportsOldLayoutFix => false;

		protected virtual bool ExpectHBLIssueDateColumn => true;

		protected override List<object> GetUnsortableColumnKeys() => new List<object> { WebTracker.Grids.CusDecHouseBills.BillType };
	}
}
