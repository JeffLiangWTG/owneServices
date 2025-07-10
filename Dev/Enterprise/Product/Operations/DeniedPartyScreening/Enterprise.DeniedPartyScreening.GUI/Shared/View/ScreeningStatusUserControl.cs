using System;
using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public partial class ScreeningStatusUserControl : ZUserControl
	{
		ScreeningStatusWinModel screeningStatusWinModel;

		public ScreeningStatusUserControl()
		{
			InitializeComponent();

#if DEBUG
			TypeDescriptor.AddAttributes(MatchDecisionDropEdit, new SuppressControlRequiresTextBasherAttribute());
			TypeDescriptor.AddAttributes(ClearingReasonDropEdit, new SuppressControlRequiresTextBasherAttribute());
			TypeDescriptor.AddAttributes(ClearingReasonTextBox, new SuppressControlRequiresTextBasherAttribute());
			TypeDescriptor.AddAttributes(DpsLinkLabel, new SuppressControlRequiresTextBasherAttribute());
#endif
		}

		void DpsLinkLabel_Click(object sender, EventArgs e)
		{
			screeningStatusWinModel?.ExecuteOpenHyperLinkCommand();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var screeningStatus = dataSource as ScreeningStatusWinModel;
			if (screeningStatus != null)
			{
				screeningStatusWinModel = screeningStatus;
				base.SetDataBinding(screeningStatusWinModel, dataMember);

				DpsLinkLabel.Text = screeningStatusWinModel.LearnDeniedPartyScreeningText;

				MatchDecisionDropEdit.SetDataBinding(screeningStatusWinModel, nameof(ScreeningStatusWinModel.ScreeningStatus));
				MatchDecisionDropEdit.CaptionResourceString = screeningStatusWinModel.ChangeStatusWaterMark;

				ClearingReasonDropEdit.SetDataBinding(screeningStatusWinModel, nameof(ScreeningStatusWinModel.ClearingReason));
				ClearingReasonDropEdit.CaptionResourceString = screeningStatusWinModel.ClearingReasonWaterMark;

				ClearingReasonTextBox.SetDataBinding(screeningStatusWinModel, nameof(ScreeningStatusWinModel.ClearingReasonText));
				ClearingReasonTextBox.MaxLength = 2000;

				InitClearingReasonsVisibility();
				InitClearingReasonTextVisibility();
				InitClearingReasonTextEnabled();

				DropEditAndTextBoxPanel.Visible = !screeningStatusWinModel.HideScreeningStatusesComboBox;

				screeningStatusWinModel.RegisterNotifyPropertyChangeEvent(nameof(ScreeningStatusWinModel.ClearingReasonsVisibility), InitClearingReasonsVisibility, clearPreviousEvents: true);
				screeningStatusWinModel.RegisterNotifyPropertyChangeEvent(nameof(ScreeningStatusWinModel.ClearingReasonTextVisibility), InitClearingReasonTextVisibility, clearPreviousEvents: true);
				screeningStatusWinModel.RegisterNotifyPropertyChangeEvent(nameof(ScreeningStatusWinModel.ClearingReasonTextEnabled), InitClearingReasonTextEnabled, clearPreviousEvents: true);
			}
		}

		void InitClearingReasonsVisibility()
		{
			ClearingReasonDropEdit.Visible = screeningStatusWinModel?.ClearingReasonsVisibility ?? false;
		}

		void InitClearingReasonTextVisibility()
		{
			ClearingReasonTextBox.Visible = screeningStatusWinModel?.ClearingReasonTextVisibility ?? false;
		}

		void InitClearingReasonTextEnabled()
		{
			ClearingReasonTextBox.Enabled = screeningStatusWinModel?.ClearingReasonTextEnabled ?? false;
		}

		void ClearingReasonTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if (e.KeyChar == '\r' || e.KeyChar == '\n' || e.KeyChar == '\t')
			{
				e.Handled = true;
			}
		}

		bool isSettingClearingText;

		void ClearingReasonTextBox_TextChanged(object sender, EventArgs e)
		{
			if (screeningStatusWinModel != null && !isSettingClearingText && screeningStatusWinModel.ClearingReasonText != ClearingReasonTextBox.Text)
			{
				isSettingClearingText = true;
				screeningStatusWinModel.ClearingReasonText = ClearingReasonTextBox.Text;
				isSettingClearingText = false;
			}
		}
	}
}
