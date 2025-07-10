namespace Enterprise.MasterFiles.GUI
{
	partial class ReactivateBranchesGrid
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ReactivateBranchOrAddressItem)(null)).Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ReactivateBranchOrAddressItem)(null)).Address)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ReactivateBranchOrAddressItem)(null)).Selected)));
			this.ReactivateGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f29f10d2-dd15-4951-b764-0ef4a7ba9b12", "Code");
			zTextBoxColumnStyleInfo1.ColumnComparer = null;
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c02b1197-74d0-453d-b3d4-e3447a997ec0", "Name");
			zTextBoxColumnStyleInfo2.ColumnComparer = null;
			zTextBoxColumnStyleInfo2.ColumnName = "Name";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("825707f7-dc43-4acd-88ce-c6585f1646bd", "Address");
			zTextBoxColumnStyleInfo3.ColumnComparer = null;
			zTextBoxColumnStyleInfo3.ColumnName = "Address";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(260);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7864ff58-eb51-4f8f-825a-94085dc9fc68", "Selected");
			zCheckBoxColumnStyleInfo1.ColumnComparer = null;
			zCheckBoxColumnStyleInfo1.ColumnName = "Selected";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ReactivateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ReactivateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ReactivateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ReactivateGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ReactivateGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReactivateGrid.GridId = "69be9dbe-179b-44e7-a79d-7ae76c75c09b";
			this.ReactivateGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReactivateGrid.LayoutKey = "ReactivateBranchesGrid";
			this.ReactivateGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReactivateGrid.Name = "ReactivateGrid";
			this.ReactivateGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(515, 226, true);
			this.ReactivateGrid.TabIndex = 1;
			// 
			// ReactivateBranchesGrid
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReactivateGrid);
			this.Name = "ReactivateBranchesGrid";
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
