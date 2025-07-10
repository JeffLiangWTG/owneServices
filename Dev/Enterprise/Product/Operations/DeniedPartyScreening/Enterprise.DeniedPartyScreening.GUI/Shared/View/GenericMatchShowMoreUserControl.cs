using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public partial class GenericMatchShowMoreUserControl : ZUserControl
	{
		public GenericMatchShowMoreUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			using (var suspendChange = GetSuspendLayoutChanges())
			{
				AutoSize = true;
				OtherInfoPanel.AutoSize = true;

				if (dataSource is GenericMatchWinModel genericMatchWinModel)
				{
					GenericMatchWinModel = genericMatchWinModel;

					InitShowMoreControls();

					OtherItemsPanel.Controls.RemoveAndDisposeAll();

					var headerUserControl = new GenericMatchHeaderUserControl();
					headerUserControl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
					headerUserControl.ScreenedPartyHeaderLabel.Text = genericMatchWinModel.OtherScreenedPartyHeader;
					headerUserControl.DeniedPartyHeaderLabel.Text = genericMatchWinModel.OtherDeniedPartyHeader;
					OtherItemsPanel.Controls.Add(headerUserControl);

					foreach (var screenedDeniedItem in genericMatchWinModel.OtherScreenedDeniedItems)
					{
						var itemUserControl = new GenericMatchItemUserControl();
						itemUserControl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
						itemUserControl.SetDataBinding(screenedDeniedItem, "");
						OtherItemsPanel.Controls.Add(itemUserControl);
					}
				}
				else if (dataSource is SourceListNamesWinModel sourceListNamesWinModel)
				{
					SourceListNamesWinModel = sourceListNamesWinModel;

					InitShowMoreControls();

					OtherItemsPanel.Controls.RemoveAndDisposeAll();

					foreach (var excludedSourceListName in sourceListNamesWinModel.ExcludedSourceListNames)
					{
						var itemUserControl = new SourceListNameItemUserControl();
						itemUserControl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
						itemUserControl.SetDataBinding(excludedSourceListName, "");
						OtherItemsPanel.Controls.Add(itemUserControl);
					}
				}
			}
		}

		GenericMatchWinModel GenericMatchWinModel { get; set; }
		SourceListNamesWinModel SourceListNamesWinModel { get; set; }

		void ShowMoreLessPanel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (GenericMatchWinModel != null)
			{
				GenericMatchWinModel.IsInnerExpanderExpanded = !GenericMatchWinModel.IsInnerExpanderExpanded;
			}
			else if (SourceListNamesWinModel != null)
			{
				SourceListNamesWinModel.IsInnerExpanderExpanded = !SourceListNamesWinModel.IsInnerExpanderExpanded;
			}

			using (var suspendChange = GetSuspendLayoutChanges())
			{
				InitShowMoreControls();
			}
		}

		void InitShowMoreControls()
		{
			if (GenericMatchWinModel != null)
			{
				ShowMoreLessLabel.Text = (GenericMatchWinModel.IsInnerExpanderExpanded ? WinformConstants.ArrowUp : WinformConstants.ArrowDown) + GenericMatchWinModel.InnerExpanderTitle;
				OtherInfoBorderPanel.Visible = GenericMatchWinModel.IsInnerExpanderExpanded;
				GenericMatchWinModel.NotifySizeChangedAction?.Invoke();
			}
			else if (SourceListNamesWinModel != null)
			{
				ShowMoreLessLabel.Text = (SourceListNamesWinModel.IsInnerExpanderExpanded ? WinformConstants.ArrowUp : WinformConstants.ArrowDown) + SourceListNamesWinModel.InnerExpanderTitle;
				OtherInfoBorderPanel.Visible = SourceListNamesWinModel.IsInnerExpanderExpanded;
			}
		}

		IDisposable GetSuspendLayoutChanges()
		{
			return new SuspendLayoutChanges(() =>
			{
				OtherItemsPanel.SuspendLayout();
				OtherInfoBorderPanel.SuspendLayout();
				OtherInfoPanel.SuspendLayout();
				SuspendLayout();
			},
			() =>
			{
				OtherItemsPanel.ResumeLayout(false);
				OtherItemsPanel.PerformLayout();
				OtherInfoBorderPanel.ResumeLayout(false);
				OtherInfoBorderPanel.PerformLayout();
				OtherInfoPanel.ResumeLayout(false);
				OtherInfoPanel.PerformLayout();
				ResumeLayout(false);
				PerformLayout();
			});
		}
	}
}
