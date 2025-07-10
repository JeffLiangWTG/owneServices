namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow
{
	partial class TSWOriginalForm
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Cancel_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AdditionalInformationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 311, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(523, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NZ.Business.TradeSingleWindow.AdditionalMessageInformation);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("A1129843-74F7-42A2-9F97-8E3C349952EB", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 277, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 1;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel_Button.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("9933048E-C561-4993-97BF-38F135CD97DB", "Cancel");
			this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(441, 277, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 23, true);
			this.Cancel_Button.TabIndex = 2;
			this.Cancel_Button.UseVisualStyleBackColor = true;
			this.Cancel_Button.Click += new System.EventHandler(this.Cancel_Button_Click);
			// 
			// AdditionalInformationGroupBox
			// 
			this.AdditionalInformationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.AdditionalInformationGroupBox.Name = "AdditionalInformationGroupBox";
			this.AdditionalInformationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 263, true);
			this.AdditionalInformationGroupBox.TabIndex = 0;
			this.AdditionalInformationGroupBox.TabStop = false;
			// 
			// TSWOriginalForm
			//
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("60EBBA17-0D9F-4473-BD9B-D5E6D4BB458C", "Send");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(523, 335, true);
			this.Controls.Add(this.Cancel_Button);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.AdditionalInformationGroupBox);
			this.DataSourceAssemblyName = "Enterprise.Customs.NZ.Business.TradeSingleWindow";
			this.DataSourceType = typeof(Enterprise.Customs.NZ.Business.TradeSingleWindow.AdditionalMessageInformation);
			this.DataSourceTypeName = "Enterprise.Customs.NZ.Business.TradeSingleWindow.AdditionalMessageInformation";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "TSWOriginalForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.AdditionalInformationGroupBox, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.Cancel_Button, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public Enterprise.ZArchitecture.GUI.ZButton OKButton;
		public Enterprise.ZArchitecture.GUI.ZButton Cancel_Button;
		internal ZArchitecture.GUI.ZGroupBox AdditionalInformationGroupBox;
	}
}
