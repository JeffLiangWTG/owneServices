using Enterprise.Customs.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class BaseCusDecHouseBillColumnProvider : GridColumnProvider
	{
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();

			if (ShouldDisplayHBLIssueDate)
			{
				AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("9F99A66B-D8E1-46F3-BD19-1A5D389BAEFA", "HBL Issue Date"), nameof(Bill.CU_IssueDate)) { ColumnKey = WebTracker.Grids.CusDecHouseBills.HBLIssueDate });
			}

			AddToDictionary(new ZCalcEditColumn(Res.GetString("7C3A8CCA-0C24-47ED-A369-FB205B2A58B4", "Manifest Qty"), nameof(Bill.CU_NoOfPacks)) { ColumnKey = WebTracker.Grids.CusDecHouseBills.ManifestQty });
			AddToDictionary(new ZTextEditColumn(Res.GetString("117E7787-8412-428D-A484-9533A9C3096A", "UQ"), nameof(Bill.CU_PackType)) { ColumnKey = WebTracker.Grids.CusDecHouseBills.UQ });
			AddCountrySpecificColumns();
		}

		protected virtual bool ShouldDisplayHBLIssueDate => true;

		protected virtual void AddCountrySpecificColumns()
		{
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("84FFE589-96A6-4188-B704-BFB751D66F82", "Bill Number"), nameof(Bill.CU_BillNum)) { ColumnKey = WebTracker.Grids.CusDecHouseBills.BillNum });
			AddToDictionaryAsDefault(new ZCodeFindBoxColumn(Res.GetString("546756AD-82F9-423D-9002-4777D2745A36", "Bill Type"), nameof(Bill.CU_BillType), "Lookups+CU_BillTypeList")
			{
				ColumnKey = WebTracker.Grids.CusDecHouseBills.BillType,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("1A4ECCC3-C918-4FEF-8258-9D2A06DDB669", "Parent Bill"), nameof(Bill.CU_ParentBillUniqueCode)) { ColumnKey = WebTracker.Grids.CusDecHouseBills.ParentBill });
		}
	}
}
