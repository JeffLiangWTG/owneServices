using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class AccTaxRateFilterControl : ZFilterStripControl
	{
		public AccTaxRateFilterControl()
		{
			InitializeComponent();
		}

		public AccTaxRateFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject, bool isAuxiliaryRateGridColumnsVisible)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			if (!isAuxiliaryRateGridColumnsVisible)
			{
				RemoveColumnsForCompanyWithoutExtraTaxRateType();
			}

			RemoveColumnsForCountryWithoutPostingGroup();
		}

		void RemoveColumnsForCompanyWithoutExtraTaxRateType()
		{
			ZGridColumnInfo[] removeList = new[] { grid.GetColumnStyle("AT_ExtraTaxRateType"), grid.GetColumnStyle("ExtraRateForTodayForUIBinding") };
			foreach (var columnStyle in removeList)
			{
				grid.ColumnStyles.Remove(columnStyle);
			}
		}

		void RemoveColumnsForCountryWithoutPostingGroup()
		{
			if (!AccTaxRate.IsPostingGroupsEnabled(GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
			{
				grid.ColumnStyles.Remove(grid.GetColumnStyle("AT_PostingGroupId"));
			}
		}
	}
}
