namespace Enterprise.MasterFiles.GUI
{
	partial class ReactivateAddressesGrid
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.ReactivateGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ReactivateGrid)).BeginInit();
			this.ReactivateGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ReactivateBranchOrAddressCollection);
			// 
			// ReactivateGrid
			// 
			this.ReactivateGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ReactivateGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ReactivateBranchOrAddressItem)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ReactivateBranchOrAddressItem)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ReactivateBranchOrAddressItem)(null)).Address)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ReactivateBranchOrAddressItem)(null)).Selected)));
			this.ReactivateGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("bb1eedb2-a393-4de1-b380-fb5262c31d2c", "Code");
			zTextBoxColumnStyleInfo1.ColumnComparer = null;
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("354af240-f471-451d-84fb-bf9e94f8c677", "Address");
			zTextBoxColumnStyleInfo2.ColumnComparer = null;
			zTextBoxColumnStyleInfo2.ColumnName = "Address";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(340);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5015068e-e329-46ec-90c2-7e661ac6a8ef", "Selected");
			zCheckBoxColumnStyleInfo1.ColumnComparer = null;
			zCheckBoxColumnStyleInfo1.ColumnName = "Selected";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ReactivateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ReactivateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ReactivateGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ReactivateGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReactivateGrid.GridId = "4833952e-47e1-4c83-a980-711d800013fa";
			this.ReactivateGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReactivateGrid.LayoutKey = "ReactivateAddressesGrid";
			this.ReactivateGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReactivateGrid.Name = "ReactivateGrid";
			this.ReactivateGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(515, 226, true);
			this.ReactivateGrid.TabIndex = 1;
			// 
			// ReactivateAddressesGrid
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReactivateGrid);
			this.Name = "ReactivateAddressesGrid";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(515, 226, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ReactivateGrid)).EndInit();
			this.ReactivateGrid.ResumeLayout(false);
			this.ReactivateGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid ReactivateGrid;
	}
}
