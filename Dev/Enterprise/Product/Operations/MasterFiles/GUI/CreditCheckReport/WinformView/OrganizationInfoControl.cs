using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OrganizationInfoControl : ZUserControl
	{
		Timer timer;

		public OrganizationInfoControl()
		{
			InitializeComponent();
			InitializeTimer();

			companyInfoContentPanel.AllowOverlap(companyInfoHighlightPanel);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (DataSource != null)
			{
				DataSource.PropertyChanged -= CompanyLookupItemModel_PropertyChanged;
			}

			base.SetDataBinding(dataSource, dataMember);
			if (DataSource != null)
			{
				var companyItem = DataSource.CompanyItem;
				var companyItemScore = companyItem.Score;

				companyInfoPanel.BackColor = ImageBitmapHelper.GetScoreLabelColor(companyItemScore);
				companyScoreToolTip = new KToolTip();
				companyScoreToolTip.SetToolTip(companyInfoPanel, companyItemScore.ToString());

				companyCityStatePostLabel.Text = FormattableString.Invariant($"{companyItem.City} {companyItem.StateCode} {companyItem.PostCode}");

				companyDunsLabel.Visible = !string.IsNullOrEmpty(DataSource.DUNS);
				companyACNLabel.Visible = !string.IsNullOrEmpty(DataSource.ACN);
				companyABNLabel.Visible = !string.IsNullOrEmpty(DataSource.ABN);

				DataSource.PropertyChanged += CompanyLookupItemModel_PropertyChanged;
			}
		}

		protected new CompanyLookupItemModel DataSource => base.DataSource as CompanyLookupItemModel;

		void CompanyLookupItemModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(CompanyLookupItemModel.Selected))
			{
				var isSelected = DataSource.Selected;

				companyInfoHighlightPanel.BackColor = isSelected ? ImageBitmapHelper.CompanyInfoPanelClickColor : ImageBitmapHelper.CompanyInfoPanelBackColor;
				BackColor = isSelected ? ImageBitmapHelper.CompanyInfoPanelInnerClickColor : ImageBitmapHelper.CompanyInfoPanelBackColor;
			}
		}

		void InitializeTimer()
		{
			timer = new Timer() { Interval = 1000 };
			timer.Tick += TimerShowToolTip;
		}

		protected void CompanyInfoContentPanel_MouseLeave(object sender, EventArgs e)
		{
			if (companyScorePanel.Visible || timer.Enabled)
			{
				timer.Stop();
				companyScorePanel.Visible = false;
			}

			if (!cityStatePostInfoRadioButton.Checked)
			{
				companyInfoHighlightPanel.BackColor = ImageBitmapHelper.CompanyInfoPanelBackColor;
				BackColor = ImageBitmapHelper.CompanyInfoPanelBackColor;
			}
		}

		protected void CompanyInfoContentPanel_MouseEnter(object sender, EventArgs e)
		{
			timer.Start();

			companyInfoHighlightPanel.BackColor = ImageBitmapHelper.CompanyInfoPanelHoverColor;
			BackColor = ImageBitmapHelper.CompanyInfoPanelInnerHoverColor;
		}

		protected void CompanyInfoContentPanel_Click(object sender, EventArgs e)
		{
			if (!cityStatePostInfoRadioButton.Checked)
			{
				cityStatePostInfoRadioButton.PerformClick();
			}
		}

		void TimerShowToolTip(object sender, EventArgs e)
		{
			companyScorePanel.Visible = true;
			timer.Stop();
		}

		protected void CityStatePostInfoRadioButton_Click(object sender, EventArgs e)
		{
			var parentForm = ParentForm as ConfirmOrganizationForm;

			foreach (var companyLookupItemModel in parentForm.CompanyLookupModel.CompanyLookupItemModels)
			{
				companyLookupItemModel.Selected = false;
			}

			if (!cityStatePostInfoRadioButton.Checked)
			{
				cityStatePostInfoRadioButton.Checked = true;
			}
		}
	}
}
