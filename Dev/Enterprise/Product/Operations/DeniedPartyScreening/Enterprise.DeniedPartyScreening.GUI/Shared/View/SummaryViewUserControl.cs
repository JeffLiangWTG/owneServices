using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.DeniedPartyScreening.GUI.Shared.View;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public partial class SummaryViewUserControl : UserControl
	{
		public SummaryViewUserControl()
		{
			InitializeComponent();

			var special19PtHeader = new Font(titleLabel.Font.FontFamily, 19, FontStyle.Bold, GraphicsUnit.Point);
			titleLabel.Font = special19PtHeader;
		}

		SummaryViewModel _viewModel;
		public SummaryViewModel ViewModel
		{
			get => _viewModel;
			set
			{
				_viewModel = value;
				ViewModelChanged();
			}
		}

		void ViewModelChanged()
		{
			titleLabel.Text = ViewModel.Title;
			closeButton.Text = ViewModel.CloseText;

			itemsLayoutPanel.Controls.Clear();

			foreach (var item in ViewModel.SummaryItemViewModels)
			{
				var c = new SummaryViewItemUserControl { ViewModel = item };
				c.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
				itemsLayoutPanel.Controls.Add(c);
			}
		}

		void closeButton_Click(object sender, EventArgs e)
		{
			ViewModel.CloseCommand.Execute(null);
		}
	}
}
