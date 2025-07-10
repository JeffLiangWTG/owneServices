namespace Enterprise.Customs.US.DataRegistry.GUI
{
	partial class EntryProcessingPortsMappingUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.PortMappingsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PortMappingsGrid)).BeginInit();
			this.PortMappingsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.DataRegistry.Business.EntryProcessingPortsMappingCollection);
			// 
			// PortMappingsGrid
			// 
			this.PortMappingsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PortMappingsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.DataRegistry.Business.EntryProcessingPortsMapping)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DataRegistry.Business.EntryProcessingPortsMapping)(null)).EntryPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.DataRegistry.Business.EntryProcessingPortsMapping)(null)).Ports)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DataRegistry.Business.EntryProcessingPortsMapping)(null)).ProcessingPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.DataRegistry.Business.EntryProcessingPortsMapping)(null)).Ports)));
			this.PortMappingsGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.BindToList = "Ports";
			zCodeFindBoxColumnStyleInfo1.Caption = "Entry Port";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "EntryPort";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.BindToList = "Ports";
			zCodeFindBoxColumnStyleInfo2.Caption = "Processing Port";
			zCodeFindBoxColumnStyleInfo2.ColumnName = "ProcessingPort";
			zCodeFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo2.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.PortMappingsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.PortMappingsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.PortMappingsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PortMappingsGrid.GridId = "a01e24a2-d10d-4f83-be2b-d3345a6369dd";
			this.PortMappingsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PortMappingsGrid.LayoutKey = "PortMappingsGrid";
			this.PortMappingsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PortMappingsGrid.Name = "PortMappingsGrid";
			this.PortMappingsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 234, true);
			this.PortMappingsGrid.TabIndex = 0;
			// 
			// EntryProcessingPortsMappingUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.PortMappingsGrid);
			this.Name = "EntryProcessingPortsMappingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 234, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PortMappingsGrid)).EndInit();
			this.PortMappingsGrid.ResumeLayout(false);
			this.PortMappingsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid PortMappingsGrid;
	}
}
