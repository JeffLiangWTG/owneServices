using System;
using System.ComponentModel;
using System.Drawing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ReportItemControl : ZUserControl
	{
		public ReportItemControl()
		{
			InitializeComponent();
#if !WINZOR
			creditEventToolTip.ForeColor = System.Drawing.Color.White;
			creditEventToolTip.OwnerDraw = true;
			creditEventToolTip.Draw += CreditEventToolTip_Draw;
#endif
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (DataSource != null)
			{
				DataSource.PropertyChanged -= ReportItemInfoModel_PropertyChanged;
			}

			base.SetDataBinding(dataSource, dataMember);

			if (DataSource != null)
			{
				reportPictureBox.Image = ImageBitmapHelper.GetReportTypeIcon(DataSource.CreditReportType);
				reportPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
				reportNameLabel.SetBindingMember("ReportCaption");
				moreInfoLinkLabel.SetBindingMember("LastReportDateOrMoreInfo");
				moreInfoLinkLabel.Click += MoreInfoLinkLabel_Click;
				SetToolTipAndCreditEventPicture();
				creditEventPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
				getReportButton.SetBindingMember("ButtonCaption");
				creditEventPictureBox.MouseEnter += (mouseEnterSender, mouseEnterE) =>
				{
					DataSource.FilterEvents();
				};
				DataSource.PropertyChanged += ReportItemInfoModel_PropertyChanged;
			}
		}

		protected new ReportItemInfoModel DataSource => BindingSource.Current as ReportItemInfoModel;

		async void GetReportButton_Click(object sender, EventArgs e)
		{
			var reportItemInfoModel = BindingSource.Current as ReportItemInfoModel;
			await reportItemInfoModel.CompanyLookupAndGetReportAsync();
		}

		async void MoreInfoLinkLabel_Click(object sender, EventArgs e)
		{
			var linkControl = sender as ZLinkLabel;
			var reportItemControl = linkControl.GetParent<ReportItemControl>();
			var reportItemModel = reportItemControl.BindingSource.Current as ReportItemInfoModel;

			await reportItemModel.OpenMoreInfoForm(ParentForm);
		}

#if !WINZOR
		void CreditEventToolTip_Draw(object sender, System.Windows.Forms.DrawToolTipEventArgs e)
		{
			e.DrawBackground();
			e.DrawBorder();
			e.DrawText();
		}
#endif

		void SetToolTipAndCreditEventPicture()
		{
			creditEventPictureBox.Image = DataSource.ToolTipIcon;
			creditEventPictureBox.Visible = DataSource.ToolTipIconVisible;
#if !WINZOR
			creditEventToolTip.BackColor = ColorTranslator.FromHtml(DataSource.ToolTipBackgroundColor);
#endif
			creditEventToolTip.SetToolTip(creditEventPictureBox, DataSource.ToolTipCaption);
		}

		void ReportItemInfoModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(ReportItemInfoModel.RelatedLatestEvents))
			{
				SetToolTipAndCreditEventPicture();
			}
		}
	}
}
