using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	partial class AccCFXUpliftCfg
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
			if (dataSourceFactory != null)
			{
				dataSourceFactory.RemoveContext(BusinessContext.PermittedToDeleteCFXUpliftConfig);
			}
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
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
            this.zGrid1.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccCFXUpliftConfigurationCollection);
            // 
            // zGrid1
            // 
            this.zGrid1.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.zGrid1, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccCFXUpliftConfiguration)(null)))));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCFXUpliftConfiguration)(null)).LevelName)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCFXUpliftConfiguration)(null)).JCF_JobType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCFXUpliftConfiguration)(null)).JCF_ServiceDirection)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCFXUpliftConfiguration)(null)).JCF_TransportMode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCFXUpliftConfiguration)(null)).JCF_RX_NKCurrency)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCFXUpliftConfiguration)(null)).JCF_CFXPercentage)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCFXUpliftConfiguration)(null)).JCF_CFXMinimum)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCFXUpliftConfiguration)(null)).JCF_RN_NKOriginCountry)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCFXUpliftConfiguration)(null)).JCF_RN_NKDestinationCountry)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MasterFiles.Business.AccCFXUpliftConfiguration)(null)).JCF_StartDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MasterFiles.Business.AccCFXUpliftConfiguration)(null)).JCF_ExpiryDate)));
            this.zGrid1.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b6d4707d-712f-45c9-8657-c4eae1b06e99", "Source");
            zTextBoxColumnStyleInfo1.ColumnName = "LevelName";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6770106f-5dbe-43be-97b5-462495611bf6", "Job Type");
            zDropEditColumnStyleInfo1.ColumnName = "JCF_JobType";
            zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2513c659-c9b8-4fd2-8e8b-0798929c4023", "Direction");
            zDropEditColumnStyleInfo2.ColumnName = "JCF_ServiceDirection";
            zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
            zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c8c8ab96-da3c-4e3f-995c-821b2c0be513", "Transport");
            zDropEditColumnStyleInfo3.ColumnName = "JCF_TransportMode";
            zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
            zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("78e84a85-b0f3-488d-9049-19335032917e", "Currency");
            zCodeFindBoxColumnStyleInfo1.ColumnName = "JCF_RX_NKCurrency";
            zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("01a990c9-dafa-4094-87de-c237f7c417bd", "CFX %");
            zCalcEditColumnStyleInfo1.ColumnName = "JCF_CFXPercentage";
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("16ac6659-971b-4744-8b2e-9e8527c22c92", "CFX Minimum");
            zCalcEditColumnStyleInfo2.ColumnName = "JCF_CFXMinimum";
            zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
            zCodeFindBoxColumnStyleInfo2.ColumnName = "JCF_RN_NKOriginCountry";
            zCodeFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCodeFindBoxColumnStyleInfo2.IsVisible = false;
            zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCodeFindBoxColumnStyleInfo3.ColumnName = "JCF_RN_NKDestinationCountry";
            zCodeFindBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zCodeFindBoxColumnStyleInfo3.IsVisible = false;
            zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3ba67539-7a5b-4f98-848c-80b2d69cd67f", "Start Date");
            zDateEditColumnStyleInfo1.ColumnName = "JCF_StartDate";
            zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
            zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
            zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e9a8fac4-b8d5-406b-b8c2-9029461b0b2b", "Expiry Date");
            zDateEditColumnStyleInfo2.ColumnName = "JCF_ExpiryDate";
            zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
            zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
            this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.zGrid1.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.zGrid1.ColumnStyles.Add(zDropEditColumnStyleInfo2);
            this.zGrid1.ColumnStyles.Add(zDropEditColumnStyleInfo3);
            this.zGrid1.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
            this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.zGrid1.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
            this.zGrid1.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
            this.zGrid1.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.zGrid1.ColumnStyles.Add(zDateEditColumnStyleInfo2);
            this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zGrid1.GridId = "9BD1783B-0508-40D0-9C9F-68ED2B5F126B";
            this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.zGrid1.LayoutKey = "zGrid1";
            this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.zGrid1.Name = "zGrid1";
            this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(699, 245, true);
            this.zGrid1.TabIndex = 1;
            // 
            // AccCFXUpliftCfg
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = this.Enabled;
            this.Controls.Add(this.zGrid1);
            this.Name = "AccCFXUpliftCfg";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(699, 245, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
            this.zGrid1.ResumeLayout(false);
            this.zGrid1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private ZArchitecture.ZGrid zGrid1;
	}
}
