namespace Enterprise.MasterFiles.GUI
{
	partial class AccSurchargeBasisUserControl
	{
		ZArchitecture.ZGrid zGrid1;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.zGrid1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccSurchargeBasis);
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccSurchargeBasis)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccSurchargeBasis)(null)).ASB_ChargeGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccSurchargeBasis)(null)).ASB_AC_ChargeCode)));
			this.zGrid1.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("cd294731-64ac-4e2a-be1d-b87303b4a5aa", "Charge Group");
			zDropEditColumnStyleInfo1.ColumnName = "ASB_ChargeGroup";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c8426d54-d06e-4af0-8d32-ecf55af02691", "Charge Code");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ASB_AC_ChargeCode";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.zGrid1.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid1.GridId = "46B84A1C-D724-41B9-A43C-75F7AEFC7691";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 283, true);
			this.zGrid1.TabIndex = 2;
			// 
			// SurchargeBasis
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zGrid1);
			this.Name = "SurchargeBasis";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 283, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.zGrid1.ResumeLayout(false);
			this.zGrid1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
