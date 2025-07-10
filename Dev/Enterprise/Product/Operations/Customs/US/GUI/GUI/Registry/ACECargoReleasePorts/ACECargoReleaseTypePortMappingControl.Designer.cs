namespace Enterprise.Customs.US.DataRegistry.GUI
{
	partial class ACECargoReleaseTypePortMappingControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.MainGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AllPortsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MainGrid)).BeginInit();
			this.MainGrid.SuspendLayout();
			this.AllPortsDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.DataRegistry.Business.ACECargoReleaseTypePortMapping);
			// 
			// MainGrid
			// 
			this.MainGrid.AllowNavigation = false;
			this.MainGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MainGrid, "PortsAndModes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.DataRegistry.Business.ACECargoReleaseTypePortMapping)(null)).PortsAndModes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DataRegistry.Business.PortsAndModes)(((System.Collections.IList)(((Enterprise.Customs.US.DataRegistry.Business.ACECargoReleaseTypePortMapping)(null)).PortsAndModes)).SyncRoot)).CertificationMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DataRegistry.Business.PortsAndModes)(((System.Collections.IList)(((Enterprise.Customs.US.DataRegistry.Business.ACECargoReleaseTypePortMapping)(null)).PortsAndModes)).SyncRoot)).Port)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DataRegistry.Business.PortsAndModes)(((System.Collections.IList)(((Enterprise.Customs.US.DataRegistry.Business.ACECargoReleaseTypePortMapping)(null)).PortsAndModes)).SyncRoot)).TransportMode)));
			this.MainGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("45fe703d-ef76-43cc-8303-cdc49ad5efa4", "Cert. Method");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CertificationMethod";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("62af4a3b-8f6b-450a-ade5-20a85e126acd", "Port");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Port";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2669389a-1e9c-481f-9b62-2424add1cda7", "Transport Mode");
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "TransportMode";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.MainGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.MainGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.MainGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.MainGrid.GridId = "0aa5b771-bee9-43f0-8946-4944ad8a8ab6";
			this.MainGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MainGrid.LayoutKey = "MainGrid";
			this.MainGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 26, true);
			this.MainGrid.Name = "MainGrid";
			this.MainGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 331, true);
			this.MainGrid.TabIndex = 1;
			// 
			// AllPortsDropEdit
			// 
			this.AllPortsDropEdit.AllowDrop = true;
			this.AllPortsDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AllPortsDropEdit, "CertificationOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.DataRegistry.Business.ACECargoReleaseTypePortMapping)(null)).CertificationOption)));
			this.AllPortsDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("9a1866e9-3546-4ccb-8627-354501666ec0", "All Ports and Modes");
			this.AllPortsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 3, true);
			this.AllPortsDropEdit.Name = "AllPortsDropEdit";
			this.AllPortsDropEdit.PreBoundMaxLength = 1;
			this.AllPortsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 20, true);
			this.AllPortsDropEdit.TabIndex = 0;
			// 
			// ACECargoReleaseTypePortMappingControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.AllPortsDropEdit);
			this.Controls.Add(this.MainGrid);
			this.Name = "ACECargoReleaseTypePortMappingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 357, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MainGrid)).EndInit();
			this.MainGrid.ResumeLayout(false);
			this.MainGrid.PerformLayout();
			this.AllPortsDropEdit.ResumeLayout(true);
			this.AllPortsDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid MainGrid;
		private ZArchitecture.GUI.ZDropEdit AllPortsDropEdit;
	}
}
