using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GetSpecifiedReportForm : KForm, ICaptionRenderingSupport
	{
		public GetSpecifiedReportForm(GetSpecifiedReportModel getSpecifiedReportModel)
		{
			InitializeComponent();
			SetDataBinding(getSpecifiedReportModel, string.Empty);
			Text = getSpecifiedReportModel.FormTitle;
			TopImage.Image = getSpecifiedReportModel.TopImage;
			Icon = BrandingFactory.Instance.ProductIcon;
			SetSuitableSizeAndFont(getSpecifiedReportModel.ReportType);
			foreach (var item in getSpecifiedReportModel.RiskDecisionDescriptions)
			{
				var riskDecisionLabel = new ZLabel();
				riskDecisionLabel.Anchor = AnchorStyles.Left;
				riskDecisionLabel.AutoSize = true;
				riskDecisionLabel.FontType = OFontTypes.Larger;
				riskDecisionLabel.Padding = new Padding(ControlDpiScalingHelper.ScaleToCurrentDpiY(2));
				riskDecisionLabel.Font = segoeUIFont;
				riskDecisionLabel.Text = (NoResString)"●  " + item;
				RiskDecisionsDescriptionTable.Controls.Add(riskDecisionLabel);
			}
		}

		void GetReportButton_Click(object sender, EventArgs e)
		{
			NeedToGetReport = true;
			Close();
		}

		void ViewSimpleReportLinkLabel_Click(object sender, EventArgs e)
		{
			var specifiedReportFormModel = BindingSource.Current as GetSpecifiedReportModel;
			specifiedReportFormModel.ShowSampleReport();
		}

		void SetSuitableSizeAndFont(CreditReportType creditReportType)
		{
			switch (creditReportType)
			{
				case CreditReportType.ComprehensiveReport:
					Size = ControlDpiScalingHelper.NewScaledSize(390, 538);
					break;
				case CreditReportType.FailureRisk:
					Size = ControlDpiScalingHelper.NewScaledSize(390, 550);
					break;
				case CreditReportType.LatePaymentRisk:
					Size = ControlDpiScalingHelper.NewScaledSize(390, 510);
					break;
				case CreditReportType.CommercialBureauEnquiry:
					Size = ControlDpiScalingHelper.NewScaledSize(390, 475);
					break;
			}

			AddEdocDescriptionLabel.Font = segoeUIFont;
			ReportDescriptionLabel.Font = segoeUIFont;
			ViewSimpleReportLinkLabel.Font = segoeUIFont;
		}

		readonly Font segoeUIFont = new Font("Segoe UI", 10, FontStyle.Regular);

		public bool NeedToGetReport { get; private set; }

		#region ICaptionRenderingSupport

		public bool? CaptionRenderingEnabled
		{
			get { return true; }
		}

		public event EventHandler CaptionRenderingEnabledChanged
		{
			add { }
			remove { }
		}

		#endregion
	}
}
