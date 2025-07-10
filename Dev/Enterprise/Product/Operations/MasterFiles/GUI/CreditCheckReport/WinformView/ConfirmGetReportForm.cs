using System;
using System.Drawing;
using CargoWise.BrandManager;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ConfirmGetReportForm : KForm, ICaptionRenderingSupport
	{
		public ConfirmGetReportForm(ConfirmGetReportModel confirmGetReport)
		{
			InitializeComponent();
			SetSuitableFont();
			var confirmGetReportInfoModel = new ConfirmGetReportInfoModel(confirmGetReport);
			SetDataBinding(confirmGetReportInfoModel, string.Empty);
			Icon = BrandingFactory.Instance.ProductIcon;
			Text = confirmGetReportInfoModel.Title;
			foreach (var identifier in confirmGetReport.Identifiers)
			{
				AddIdentityItemControl(identifier.Type.ToString(), identifier.ID);
			}
		}

		public bool ConfirmButtonClicked { get; private set; }

		internal void GetReportButton_Click(object sender, EventArgs args)
		{
			ConfirmButtonClicked = true;
			Close();
		}

		internal void CancelButton_Click(object sender, EventArgs args)
		{
			Close();
		}

		void SetSuitableFont()
		{
			companyNameLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
			operationDetailLabel.Font = new Font("Segoe UI", 10, FontStyle.Regular);
		}

		void AddIdentityItemControl(string type, string id)
		{
			var identityDetailItemPanel = new ZPanel();
			var typeLabel = new ZLabel();
			var idLabel = new ZLabel();

			identityDetailItemPanel.AutoSize = true;
			identityDetailItemPanel.Margin = ControlDpiScalingHelper.NewScaledPadding(0);
			identityDetailItemPanel.Padding = ControlDpiScalingHelper.NewScaledPadding(3, 0, 0, 0);
			identityDetailItemPanel.MaximumSize = ControlDpiScalingHelper.NewScaledSize(190, 0);

			typeLabel.AutoSize = true;
			typeLabel.FontType = OFontTypes.Larger;
			typeLabel.ForeColor = Color.SteelBlue;
			typeLabel.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0);
			typeLabel.Name = "typeLabel";
			typeLabel.Margin = ControlDpiScalingHelper.NewScaledPadding(0);
			typeLabel.Size = ControlDpiScalingHelper.NewScaledSize(37, 20);
			typeLabel.TabIndex = 0;
			typeLabel.Text = type;
			typeLabel.UseMnemonic = false;

			idLabel.AutoSize = true;
			idLabel.FontType = OFontTypes.Larger;
			idLabel.ForeColor = Color.Gray;
			idLabel.Location = ControlDpiScalingHelper.NewScaledPoint(47, 0);
			idLabel.Name = "IDLabel";
			idLabel.Margin = ControlDpiScalingHelper.NewScaledPadding(0);
			idLabel.MaximumSize = ControlDpiScalingHelper.NewScaledSize(120, 0);
			idLabel.TabIndex = 0;
			idLabel.Text = id;
			idLabel.UseMnemonic = false;

			identityDetailItemPanel.Controls.Add(typeLabel);
			identityDetailItemPanel.Controls.Add(idLabel);

			identityItemsLayoutPanel.Controls.Add(identityDetailItemPanel);
		}

		#region ICaptionRenderingSupport

		bool? ICaptionRenderingSupport.CaptionRenderingEnabled
		{
			get { return true; }
		}

		event EventHandler ICaptionRenderingSupport.CaptionRenderingEnabledChanged
		{
			add { }
			remove { }
		}

		#endregion
	}
}
