namespace Enterprise.Customs.US.ACEManifest.GUI
{
	partial class AIMBillsSelectionDialog
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.ReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DescPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).BeginInit();
			this.ItemsGrid.SuspendLayout();
			this.ItemsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// SelectedItemCountLabel
			// 
			this.SelectedItemCountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 10, true);
			// 
			// DescPanel
			// 
			this.DescPanel.Controls.Add(this.ReasonDropEdit);
			this.DescPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 238, true);
			this.DescPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 32, true);
			this.DescPanel.Controls.SetChildIndex(this.ReasonDropEdit, 0);
			this.DescPanel.Controls.SetChildIndex(this.SelectedItemCountLabel, 0);
			// 
			// ItemsGrid
			// 
			this.ItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(546, 221, true);
			// 
			// ItemsGroupBox
			// 
			this.ItemsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 238, true);
			// 
			// ReasonDropEdit
			//
			this.BindingSource.SetBindingMember(this.ReasonDropEdit, "Reason");
			this.ReasonDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 4, true);
			this.ReasonDropEdit.Name = "ReasonDropEdit";
			this.ReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 21, true);
			this.ReasonDropEdit.TabIndex = 2;
			this.ReasonDropEdit.CaptionResourceString = Enterprise.Customs.US.ACEManifest.GUI.Res.GetData("CA282001-5DC7-4A41-BF56-88CAABEDD6AB", "Reason");
			// 
			// AirAMSBillsSelectionDialog
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 300, true);
			this.Name = "AirAMSBillsSelectionDialog";
			this.DescPanel.ResumeLayout(false);
			this.DescPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).EndInit();
			this.ItemsGrid.ResumeLayout(false);
			this.ItemsGrid.PerformLayout();
			this.ItemsGroupBox.ResumeLayout(false);
			this.ItemsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		protected Enterprise.ZArchitecture.GUI.ZDropEdit ReasonDropEdit;

		#endregion
	}
}
