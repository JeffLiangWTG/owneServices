namespace Enterprise.MasterFiles.GUI
{
	partial class CashAdvanceChargeCodeMultiSelectUserControl
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
				ChargeCodeModuleButtonGrid.Attached += ChargeCodeModuleButtonGrid_Attached;
				ChargeCodeModuleButtonGrid.Detached += ChargeCodeModuleButtonGrid_Detached;
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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ChargeCodeModuleButtonGrid = new Enterprise.MasterFiles.GUI.ZModuleButtonGridForDesigner();
			this.zBindingSource1 = new Enterprise.ZArchitecture.GUI.ZBindingSource(this.components);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargeCodeModuleButtonGrid.InnerGrid)).BeginInit();
			this.ChargeCodeModuleButtonGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zBindingSource1)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccCashAdvanceDefaultingConfiguration);
			// 
			// ChargeCodeModuleButtonGrid
			// 
			this.ChargeCodeModuleButtonGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChargeCodeModuleButtonGrid, "LinkedChargeCodes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccCashAdvanceDefaultingConfiguration)(null)).LinkedChargeCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccCashAdvanceDefaultingConfiguration)(null)).Lookups.ChargeCodeFindBoxCollection)));
			this.ChargeCodeModuleButtonGrid.BindToFindBoxList = "Lookups.ChargeCodeFindBoxCollection";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ad382c7c-cbcd-4194-a927-eee6cef9cb2c", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "AC_Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("38af87b4-f7e0-46df-8359-d4645dd5aa16", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "AC_Desc";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ChargeCodeModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ChargeCodeModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ChargeCodeModuleButtonGrid.GridId = null;
			// 
			// 
			// 
			this.ChargeCodeModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.ChargeCodeModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.ChargeCodeModuleButtonGrid.InnerGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChargeCodeModuleButtonGrid.InnerGrid.GridId = null;
			this.ChargeCodeModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChargeCodeModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.ChargeCodeModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.ChargeCodeModuleButtonGrid.InnerGrid.Name = "Grid";
			this.ChargeCodeModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 208, true);
			this.ChargeCodeModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.ChargeCodeModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ChargeCodeModuleButtonGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.AccChargeCode;
			this.ChargeCodeModuleButtonGrid.Name = "ChargeCodeModuleButtonGrid";
			this.ChargeCodeModuleButtonGrid.ReadOnly = false;
			this.ChargeCodeModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(494, 243, true);
			this.ChargeCodeModuleButtonGrid.TabIndex = 0;
			// 
			// zBindingSource1
			// 
			this.zBindingSource1.ContainerControl = this;
			this.zBindingSource1.DataSourceType = null;
			// 
			// ChargeCodeMultiSelectUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ChargeCodeModuleButtonGrid);
			this.Name = "ChargeCodeMultiSelectUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(494, 243, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargeCodeModuleButtonGrid.InnerGrid)).EndInit();
			this.ChargeCodeModuleButtonGrid.ResumeLayout(true);
			this.ChargeCodeModuleButtonGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zBindingSource1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZModuleButtonGridForDesigner ChargeCodeModuleButtonGrid;
		private ZArchitecture.GUI.ZBindingSource zBindingSource1;
	}
}
