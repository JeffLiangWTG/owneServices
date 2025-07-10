namespace Enterprise.MasterFiles.GUI
{
	public partial class EmailContactForm
	{
		#region Windows Form Designer generated code

		protected override void InitializeComponent()
		{
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EmailToContactUserControl1 = new Enterprise.MasterFiles.GUI.EmailToContactUserControl();
			this.PreviewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 565, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.EmailToContactBusinessObject);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EmailContactForm|c10cec95-6c78-44a5-8ced-0dceb22445b1", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 534, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 21, true);
			this.CloseButton.TabIndex = 2;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EmailContactForm|0dcd59d3-6764-4167-b456-3196ad8b22f5", "Send");
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(574, 534, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 21, true);
			this.SendButton.TabIndex = 1;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// EmailToContactUserControl1
			// 
			this.EmailToContactUserControl1.AllowDrop = true;
			this.EmailToContactUserControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.EmailToContactUserControl1, ".");
			this.EmailToContactUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EmailToContactUserControl1.Name = "EmailToContactUserControl1";
			this.EmailToContactUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 529, true);
			this.EmailToContactUserControl1.TabIndex = 0;
			// 
			// PreviewButton
			// 
			this.PreviewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PreviewButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EmailContactForm|90fd646e-ab5c-4d9d-beee-d2a7a607a350", "Preview");
			this.PreviewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(491, 534, true);
			this.PreviewButton.Name = "PreviewButton";
			this.PreviewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 21, true);
			this.PreviewButton.TabIndex = 1;
			this.PreviewButton.Click += new System.EventHandler(this.PreviewButton_Click);
			// 
			// EmailContactForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EmailContactForm|1739d904-ae92-403c-9341-e76bc163fd6a", "Email to Contact");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 589, true);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.PreviewButton);
			this.Controls.Add(this.SendButton);
			this.Controls.Add(this.EmailToContactUserControl1);
			this.DataSourceAssemblyName = "Enterprise.MasterFiles.Business";
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.EmailToContactBusinessObject);
			this.DataSourceTypeName = "Enterprise.MasterFiles.Business.EmailToContactBusinessObject";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(752, 572, true);
			this.Name = "EmailContactForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.EmailToContactUserControl1, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.PreviewButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		protected EmailToContactUserControl EmailToContactUserControl1;
		private Enterprise.ZArchitecture.GUI.ZButton PreviewButton;
		private Enterprise.ZArchitecture.GUI.ZButton SendButton;
	}
}
