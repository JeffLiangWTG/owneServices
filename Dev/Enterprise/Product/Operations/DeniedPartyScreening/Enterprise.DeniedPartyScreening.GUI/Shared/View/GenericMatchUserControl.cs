using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public partial class GenericMatchUserControl : ZUserControl
	{
		public GenericMatchUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource is GenericMatchWinModel genericMatchWinModel)
			{
				ContentBorderPanel.AutoSize = true;
				ShowMoreUserControl.AutoSize = true;

				base.SetDataBinding(dataSource, dataMember);

				var tableExpanderHelper = new TableExpanderHelper(GenericMatchTableLayoutPanel, ProfileContentExpander, GenericMatchStackPanel, ExpanderDescriptionLabel, collapsed => { genericMatchWinModel.IsExpanderExpanded = !collapsed; });

				tableExpanderHelper.InitBehaviour(!genericMatchWinModel.IsExpanderExpanded, genericMatchWinModel.IsExpanderEnabled);

				ExpanderTitleLabel.Text = genericMatchWinModel.ExpanderTitle;

				ExpanderDescriptionLabel.Text = genericMatchWinModel.ExpanderDescription;
				ExpanderDescriptionLabel.ForeColor = ColorConverter.MapExpanderDescriptionTextColor(genericMatchWinModel.IsExpanderEnabled);

				MatchedItemsListPanel.Controls.RemoveAndDisposeAll();

				if (genericMatchWinModel.ScreenedDeniedItemsCount > 0)
				{
					var headerUserControl = new GenericMatchHeaderUserControl();
					headerUserControl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
					headerUserControl.ScreenedPartyHeaderLabel.Text = genericMatchWinModel.ScreenedPartyHeader;
					headerUserControl.DeniedPartyHeaderLabel.Text = genericMatchWinModel.DeniedPartyHeader;
					MatchedItemsListPanel.Controls.Add(headerUserControl);

					foreach (var screenedDeniedItem in genericMatchWinModel.ScreenedDeniedItems)
					{
						var itemUserControl = new GenericMatchItemUserControl();
						itemUserControl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
						itemUserControl.SetDataBinding(screenedDeniedItem, "");
						MatchedItemsListPanel.Controls.Add(itemUserControl);
					}
				}

				if (genericMatchWinModel.OtherScreenedDeniedItemsCount > 0)
				{
					genericMatchWinModel.NotifySizeChangedAction = () =>
					{
						// These codes make sure that there will be no abnormal height when clicking the Show More button
						GenericMatchContentPanel.AutoSizeMode = AutoSizeMode.GrowOnly;
						GenericMatchContentPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
					};

					ShowMoreUserControl.Visible = true;
					ShowMoreUserControl.SetDataBinding(genericMatchWinModel, "");
				}
				else
				{
					ShowMoreUserControl.Visible = false;
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			GenericMatchContentPanel.Dispose();
			base.Dispose(disposing);
		}
	}
}
