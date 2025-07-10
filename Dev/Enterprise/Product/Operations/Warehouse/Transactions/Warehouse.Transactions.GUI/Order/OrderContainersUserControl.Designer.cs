namespace Enterprise.Warehouse.Transactions.GUI
{
	partial class OrderContainersUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.GenerateLinesFromContainersButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ContainersGridControl = new Enterprise.Warehouse.Transactions.GUI.DocketContainerGridUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// GenerateLinesFromContainersButton
			// 
			this.GenerateLinesFromContainersButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.GenerateLinesFromContainersButton.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderContainersUserControl|d8bd1f96-478a-407d-8add-e9dacb718b25", "Generate Lines from Containers");
			this.GenerateLinesFromContainersButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(638, 539, true);
			this.GenerateLinesFromContainersButton.Name = "GenerateLinesFromContainersButton";
			this.GenerateLinesFromContainersButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 24, true);
			this.GenerateLinesFromContainersButton.TabIndex = 3;
			this.GenerateLinesFromContainersButton.Click += new System.EventHandler(this.GenerateLinesFromContainersButton_Click);
			// 
			// ContainersGridControl
			// 
			this.ContainersGridControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ContainersGridControl, ".");
			this.ContainersGridControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainersGridControl.Name = "ContainersGridControl";
			this.ContainersGridControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 537, true);
			this.ContainersGridControl.TabIndex = 2;
			// 
			// OrderContainersUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GenerateLinesFromContainersButton);
			this.Controls.Add(this.ContainersGridControl);
			this.Name = "OrderContainersUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 564, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton GenerateLinesFromContainersButton;
		private DocketContainerGridUserControl ContainersGridControl;
	}
}
