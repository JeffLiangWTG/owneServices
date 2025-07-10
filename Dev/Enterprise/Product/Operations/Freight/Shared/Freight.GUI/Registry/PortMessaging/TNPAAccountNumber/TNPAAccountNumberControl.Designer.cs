namespace Enterprise.Freight.GUI
{
	partial class TNPAAccountNumberControl
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
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.TNPAAccountNumberGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TNPAAccountNumberGrid)).BeginInit();
			this.TNPAAccountNumberGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.TNPAAccountNumberCollection);
			// 
			// TNPAAccountNumberGrid
			// 
			this.TNPAAccountNumberGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TNPAAccountNumberGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.TNPAAccountNumber)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.TNPAAccountNumber)(null)).Port)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.TNPAAccountNumber)(null)).ImportNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.TNPAAccountNumber)(null)).ExportNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.TNPAAccountNumber)(null)).CoastwiseNumber)));
			this.TNPAAccountNumberGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("9862D15D-C109-4155-9250-3AED0EBCEADC", "Port");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Port";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("d9c4c809-deba-4ece-a263-862e4c7499ee", "Import Number");
			zTextBoxColumnStyleInfo1.ColumnName = "ImportNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("1ac6c048-51c5-4539-8275-0448ad46abda", "Export Number");
			zTextBoxColumnStyleInfo2.ColumnName = "ExportNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("e6fa71d0-6bcf-47b2-b8ff-2fc000aed58c", "Coastwise Number");
			zTextBoxColumnStyleInfo3.ColumnName = "CoastwiseNumber";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.TNPAAccountNumberGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.TNPAAccountNumberGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TNPAAccountNumberGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TNPAAccountNumberGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.TNPAAccountNumberGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TNPAAccountNumberGrid.GridId = "e8ad6b7a-c5d5-45b6-ab2c-189fa87fdbb3";
			this.TNPAAccountNumberGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TNPAAccountNumberGrid.LayoutKey = "TNPAAccountNumberGrid";
			this.TNPAAccountNumberGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TNPAAccountNumberGrid.Name = "TNPAAccountNumberGrid";
			this.TNPAAccountNumberGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 210, true);
			this.TNPAAccountNumberGrid.TabIndex = 0;
			// 
			// TNPAAccountNumberControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TNPAAccountNumberGrid);
			this.Name = "TNPAAccountNumberControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 210, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TNPAAccountNumberGrid)).EndInit();
			this.TNPAAccountNumberGrid.ResumeLayout(false);
			this.TNPAAccountNumberGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid TNPAAccountNumberGrid;
	}
}
