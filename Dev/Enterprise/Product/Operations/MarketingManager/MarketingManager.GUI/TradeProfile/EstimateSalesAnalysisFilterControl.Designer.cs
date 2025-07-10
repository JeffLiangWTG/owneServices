namespace Enterprise.MarketingManager.GUI
{
	partial class EstimateSalesAnalysisFilterControl
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
			this.statusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.shouldMatchOnBuyerSupplierCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.statusDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.EstimateSalesAnalysisFilter);
			// 
			// statusDropEdit
			// 
			this.statusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.statusDropEdit, "Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.EstimateSalesAnalysisFilter)(null)).Status)));
			this.statusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(57, 2, true);
			this.statusDropEdit.Name = "statusDropEdit";
			this.statusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			this.statusDropEdit.TabIndex = 1;
			// 
			// shouldMatchOnBuyerSupplierCheckBox
			// 
			this.shouldMatchOnBuyerSupplierCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.shouldMatchOnBuyerSupplierCheckBox, "ShouldMatchOnBuyerSupplier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.EstimateSalesAnalysisFilter)(null)).ShouldMatchOnBuyerSupplier)));
			this.shouldMatchOnBuyerSupplierCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.shouldMatchOnBuyerSupplierCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(309, 4, true);
			this.shouldMatchOnBuyerSupplierCheckBox.Name = "shouldMatchOnBuyerSupplierCheckBox";
			this.shouldMatchOnBuyerSupplierCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 17, true);
			this.shouldMatchOnBuyerSupplierCheckBox.TabIndex = 2;
			this.shouldMatchOnBuyerSupplierCheckBox.UseVisualStyleBackColor = true;
			// 
			// EstimateSalesAnalysisFilterControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.shouldMatchOnBuyerSupplierCheckBox);
			this.Controls.Add(this.statusDropEdit);
			this.Name = "EstimateSalesAnalysisFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(851, 24, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.statusDropEdit.ResumeLayout(true);
			this.statusDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit statusDropEdit;
		private ZArchitecture.GUI.ZCheckBox shouldMatchOnBuyerSupplierCheckBox;
	}
}
