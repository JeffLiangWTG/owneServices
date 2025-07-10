using System;
using CargoWise.Windows.UI;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class CartageTypeDropModeDialog : ZChildForm
	{
		public CartageTypeDropModeDialog(CommonCartage cartage, string message, string addressButtonText, string jobTypeButtonText)
			: base(cartage)
		{
			InitializeComponent();
			MessageLabel.Text = message;
			this.addressButtonText = addressButtonText;
			this.jobTypeButtonText = jobTypeButtonText;
			SetupButtons(cartage);
		}

		readonly string addressButtonText;
		readonly string jobTypeButtonText;

		const int PanelHeight = 42;
		const int HalfButtonHeight = 16;

		void SetupButtons(CommonCartage cartage)
		{
			SetupButton(KeepExistingButton, Res.GetString("033f2ea9-98ca-42a2-aa95-7f34e305961b", "Keep Existing Drop Mode '{0}'", cartage.JJ_DropMode));
			SetupButtonAndPanel(AddressButtonPanel, UseAddressDropModeButton, addressButtonText);
			SetupButtonAndPanel(JobTypeButtonPanel, UseJobTypeDropModeButton, jobTypeButtonText);
		}

		void SetupButtonAndPanel(ZPanel panel, ZButton button, string buttonText)
		{
			if (buttonText != null)
			{
				if (!buttonText.Contains("\r\n"))
				{
					ControlDpiScalingHelper.SetHeight(panel, panel.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(HalfButtonHeight), false);
					SetFormSize(ControlDpiScalingHelper.ScaleToCurrentDpiY(HalfButtonHeight));
					ControlDpiScalingHelper.SetHeight(ButtonPanel, ButtonPanel.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(HalfButtonHeight), false);
				}

				SetupButton(button, buttonText);
				panel.Visible = true;
			}
			else
			{
				SetFormSize(ControlDpiScalingHelper.ScaleToCurrentDpiY(PanelHeight));
				ControlDpiScalingHelper.SetHeight(ButtonPanel, ButtonPanel.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(PanelHeight), false);
			}
		}

		void SetFormSize(int height)
		{
			var heightToChangeTo = Height - height;
			MaximumSize = ControlDpiScalingHelper.NewScaledSize(MaximumSize.Width, MaximumSize.Height - height, false);
			MinimumSize = ControlDpiScalingHelper.NewScaledSize(MinimumSize.Width, MinimumSize.Height - height, false);
			ControlDpiScalingHelper.SetHeight(this, heightToChangeTo, false);
		}

		void SetupButton(ZButton button, string text)
		{
			button.Text = text;
			button.Visible = true;
			button.Enabled = true;
		}

		public override string FormHeading
		{
			get { return DialogDescription; }
		}

		public static string DialogDescription
		{
			get { return Res.GetString("4305fa93-2404-4fd9-8b96-1beb61764f58", "Populate Cartage Drop Mode"); }
		}

		void KeepExistingButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void UseAddressDropModeButton_Click(object sender, EventArgs e)
		{
			Result = DropMode.Address;
			Close();
		}

		void UseJobTypeDropModeButton_Click(object sender, EventArgs e)
		{
			Result = DropMode.CartageType;
			Close();
		}

		public DropMode Result = DropMode.None;
	}
}
