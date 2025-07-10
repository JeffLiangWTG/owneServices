using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.GUI
{
	public partial class CustomsNumberViewStmNumsUserControl : MasterFiles.GUI.CustomsNumberViewStmNumsCompanyUserControl
	{
		public CustomsNumberViewStmNumsUserControl(CustomsNumberViewStmNumsWrapperCollection collection)
			: base(collection)
		{
			InitializeComponent();
			InitializeNumberRangesGrid();
		}

		void InitializeNumberRangesGrid()
		{
			using (NumberRangesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				NumberRangesGrid.SetColumnWidth(CustomsNumberViewStmNumsWrapper.Schema.SN_FountainName, 160);
			}
		}
	}
}
