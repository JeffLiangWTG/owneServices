namespace Enterprise.MasterFiles.GUI
{
	public partial class ImageSelectionForm
	{

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.ImageSelectionControl = new Enterprise.ZArchitecture.GUI.ImageSelectionControl();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 188, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(System.Drawing.Image);
			// 
			// ImageSelectionControl
			// 
			this.BindingSource.SetBindingMember(this.ImageSelectionControl, ".");
			this.ImageSelectionControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.ImageSelectionControl.Name = "ImageSelectionControl";
			this.ImageSelectionControl.ReadOnly = false;
			this.ImageSelectionControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(345, 144, true);
			this.ImageSelectionControl.TabIndex = 0;
			this.ImageSelectionControl.Load += new System.EventHandler(this.ImageSelectionForm_Load);
			// 
			// CancelButton
			// 
			this.CancelButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImageSelectionForm|af3297e6-4444-4d67-bfec-0e0da261c942", "&Cancel");
			this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 160, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 2;
			// 
			// OKButton
			// 
			this.OKButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImageSelectionForm|39223ccc-5b2b-432e-907f-033118825611", "&OK");
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 160, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 1;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// ImageSelectionForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 212, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImageSelectionForm|4800127c-321a-4140-8c0c-102d515537f2", "Select Document Logo");
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.ImageSelectionControl);
			this.DataSourceType = typeof(System.Drawing.Image);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.Name = "ImageSelectionForm";
			this.Load += new System.EventHandler(this.ImageSelectionForm_Load);
			this.Controls.SetChildIndex(this.ImageSelectionControl, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		#region Auto-generated code

		protected Enterprise.ZArchitecture.GUI.ImageSelectionControl ImageSelectionControl;
		protected new Enterprise.ZArchitecture.GUI.ZButton CancelButton;
		protected Enterprise.ZArchitecture.GUI.ZButton OKButton;

		#endregion

	}
}
