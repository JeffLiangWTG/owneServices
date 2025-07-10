using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Packing.Module
{
	public partial class PackingFilterControl
	{
		#region Component Designer generated code

		private System.ComponentModel.Container components = null;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.PkgPackageJob)(null)).KJ_JobID);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.PkgPackageJob)(null)).ParentJob.JobDescription);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.PkgPackageJob)(null)).ParentJob.JobNo);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.PkgPackageJob)(null)).Weight);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.PkgPackageJob)(null)).WeightUQ);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.PkgPackageJob)(null)).Volume);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.PkgPackageJob)(null)).VolumeUQ);
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = null;
			zTextBoxColumnStyleInfo1.ColumnName = "KJ_JobID";
			zTextBoxColumnStyleInfo1.GroupName = null;
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Packing.Module.Res.GetData("PackingFilterControl|94427f37-bbe5-4591-95f8-510ce06fdddf", "Job Desc.", "Parent Job Desc.", "Parent Job Description", "");
			zTextBoxColumnStyleInfo2.ColumnName = "ParentJob+JobDescription";
			zTextBoxColumnStyleInfo2.GroupName = null;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zTextBoxColumnStyleInfo3.Caption = null;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Packing.Module.Res.GetData("PackingFilterControl|b5c380d5-577f-4b2e-b6f4-e2d988b3c2f4", "Ref #", "Parent Main Ref #", "Parent Job Main Ref #", "");
			zTextBoxColumnStyleInfo3.ColumnName = "ParentJob+JobNo";
			zTextBoxColumnStyleInfo3.GroupName = null;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Packing.Module.Res.GetData("PackingFilterControl|27CBA268-6167-467C-BD98-C30EB1E28DA7", "Weight");
			zCalcEditColumnStyleInfo1.ColumnName = "Weight";
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Packing.Module.Res.GetData("9fe2f568-4c2e-40cc-bf85-a19ea0da7513", "Weight");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.Caption = null;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Packing.Module.Res.GetData("PackingFilterControl|BA23DA03-C3F8-4F3E-B33F-F4CC825361E3", "UQ");
			zTextBoxColumnStyleInfo4.ColumnName = "WeightUQ";
			zTextBoxColumnStyleInfo4.GroupName = Enterprise.Packing.Module.Res.GetData("9fe2f568-4c2e-40cc-bf85-a19ea0da7513", "Weight");
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Packing.Module.Res.GetData("PackingFilterControl|404A76B0-9E3F-43A6-915D-366CAA80297E", "Volume");
			zCalcEditColumnStyleInfo2.ColumnName = "Volume";
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Packing.Module.Res.GetData("9161b03b-264b-4283-b380-9bb412b07339", "Volume");
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo5.Caption = null;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Packing.Module.Res.GetData("PackingFilterControl|2577F0BF-0865-4F9F-B413-F6BE2E5EA6AC", "UQ");
			zTextBoxColumnStyleInfo5.ColumnName = "VolumeUQ";
			zTextBoxColumnStyleInfo5.GroupName = Enterprise.Packing.Module.Res.GetData("9161b03b-264b-4283-b380-9bb412b07339", "Volume");
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 173, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(732, 371, true);
			this.grid.TabIndex = 99;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.PkgPackageJob);
			// 
			// PackingFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "PackingFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(732, 544, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
