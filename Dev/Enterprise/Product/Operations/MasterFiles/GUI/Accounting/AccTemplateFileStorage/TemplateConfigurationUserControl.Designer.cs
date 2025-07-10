using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.GUI
{
	partial class TemplateConfigurationUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.TemplateConfigurationGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TemplateConfigurationGrid)).BeginInit();
			this.TemplateConfigurationGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccEInvoicingTemplateFileViewCollection);
			// 
			// TemplateConfigurationGrid
			// 
			this.TemplateConfigurationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TemplateConfigurationGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccEInvoicingTemplateFileView)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccEInvoicingTemplateFileView)(null)).LevelName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccEInvoicingTemplateFileView)(null)).ETF_JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccEInvoicingTemplateFileView)(null)).Lookups.JobTypesList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccEInvoicingTemplateFileView)(null)).JobTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccEInvoicingTemplateFileView)(null)).ETF_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccEInvoicingTemplateFileView)(null)).Lookups.TransportModesList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccEInvoicingTemplateFileView)(null)).ETF_TemplateCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccEInvoicingTemplateFileView)(null)).Lookups.TemplateCodesList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccEInvoicingTemplateFileView)(null)).TemplateFileDesc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccEInvoicingTemplateFileView)(null)).TemplateFileName)));
			this.TemplateConfigurationGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ce67dee4-a2a1-44a6-af67-0ed9df1293ef", "Source");
			zTextBoxColumnStyleInfo1.ColumnName = "LevelName";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.BindToList = "Lookups.JobTypesList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("03982833-23a3-41b7-a13f-9e30d494f716", "Job Type");
			zDropEditColumnStyleInfo1.ColumnName = "ETF_JobType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2b05307f-7e21-4922-8e46-e521a03ebd94", "Job Type Description");
			zTextBoxColumnStyleInfo2.ColumnName = "JobTypeDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.BindToList = "Lookups.TransportModesList";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e93e1fb1-ce52-49e4-907f-641b3299b89f", "Transport Mode");
			zDropEditColumnStyleInfo2.ColumnName = "ETF_TransportMode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.BindToList = "Lookups.TemplateCodesList";
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b1dbf54f-fbff-40e4-85c7-0d5f3583e1f3", "Template Code");
			zDropEditColumnStyleInfo3.ColumnName = "ETF_TemplateCode";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e52cf916-bed6-470b-bbf3-dcb8627838ff", "Template Description");
			zTextBoxColumnStyleInfo3.ColumnName = "TemplateFileDesc";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8886b7d8-7717-4a17-9552-cd12fc40be94", "XSLT File Name");
			zTextBoxColumnStyleInfo4.ColumnName = "TemplateFileName";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.TemplateConfigurationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TemplateConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.TemplateConfigurationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TemplateConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.TemplateConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.TemplateConfigurationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.TemplateConfigurationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.TemplateConfigurationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TemplateConfigurationGrid.GridId = "D5A74FDE-CB2A-4E9D-8006-506A0533ED04";
			this.TemplateConfigurationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TemplateConfigurationGrid.LayoutKey = "TemplateConfigurationGrid";
			this.TemplateConfigurationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TemplateConfigurationGrid.Name = "TemplateConfigurationGrid";
			this.TemplateConfigurationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 185, true);
			this.TemplateConfigurationGrid.TabIndex = 4;
			// 
			// TemplateConfigurationUserControl
			// 
			this.Controls.Add(this.TemplateConfigurationGrid);
			this.Name = "TemplateConfigurationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 185, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TemplateConfigurationGrid)).EndInit();
			this.TemplateConfigurationGrid.ResumeLayout(false);
			this.TemplateConfigurationGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZGrid TemplateConfigurationGrid;
	}
}
