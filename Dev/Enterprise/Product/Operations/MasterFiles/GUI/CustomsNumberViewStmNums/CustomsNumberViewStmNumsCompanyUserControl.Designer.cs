using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CustomsNumberViewStmNumsCompanyUserControl : CustomsNumberViewStmNumsUserControl
	{
		ResourceStringData AddButtomCaption => Res.GetData("{D8810F99-0F85-4676-8FF4-1E22837AB8AF}", "&Add");
		ResourceStringData AddCompanyButtonCaption => Res.GetData("{839FBDB6-B037-4D67-9446-5247FBAFFDCF}", "Add &Company");
		ResourceStringData AddBranchButtonCaption => Res.GetData("{E82D6D4C-AD25-4B83-A918-DB059AF8C43E}", "Add &Branch");
		protected new CustomsNumberViewStmNumsCompanyProvider Provider => base.Provider as CustomsNumberViewStmNumsCompanyProvider;

		void InitializeComponent()
		{
			if (Provider is CustomsNumberViewStmNumsCompanyProvider provider)
			{
				using (NumberRangesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
				{
					var enableBranchLevel = provider.EnableBranchLevel;
					var enableCompanyLevel = provider.EnableCompanyLevel;

					if (enableCompanyLevel && enableBranchLevel)
					{
						AddIsBranchLevelColumn();
					}

					if (enableBranchLevel)
					{
						var addBranchButton = CreateAddBranchButton();
						addBranchButton.CaptionResourceString = enableCompanyLevel ? AddBranchButtonCaption : AddButtomCaption;
					}

					NumberRangesGrid.SetAvailability(enableBranchLevel, CustomsNumberViewStmNumsWrapper.Schema.SN_Owner);

					NumberRangesAddButtonPanel.Visible = enableCompanyLevel;
					NumberRangesAdditionalButtonsPanel.Visible = enableBranchLevel;

					NumberRangesAddButton.CaptionResourceString = enableBranchLevel ? AddCompanyButtonCaption : AddButtomCaption;
				}
			}
		}
	}
}
