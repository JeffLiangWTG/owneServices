namespace Enterprise.MasterFiles.GUI
{
	partial class ComplianceNumberSequenceConfigurationControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.ParentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ParentGrid = new Enterprise.ZArchitecture.ZGrid();
			this.GridSplitter = new CargoWise.Windows.UI.KSplitter();
			this.ChildGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ChildGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ParentGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ParentGrid)).BeginInit();
			this.ParentGrid.SuspendLayout();
			this.ChildGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChildGrid)).BeginInit();
			this.ChildGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.ComplianceNumberSequenceConfigurationCollection);
			// 
			// ParentGroupBox
			// 
			this.ParentGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("76084667-4415-49d6-914f-4f388a2806f8", "Code");
			this.ParentGroupBox.Controls.Add(this.ParentGrid);
			this.ParentGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ParentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ParentGroupBox.Name = "ParentGroupBox";
			this.ParentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(514, 128, true);
			this.ParentGroupBox.TabIndex = 2;
			this.ParentGroupBox.TabStop = false;
			// 
			// ParentGrid
			// 
			this.ParentGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ParentGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ComplianceNumberSequenceConfiguration)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceNumberSequenceConfiguration)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceNumberSequenceConfiguration)(null)).Description)));
			this.ParentGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("606b7828-8cdd-4a42-90d1-4ae5a7344bfd", "Code");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3f30a4f5-1f03-43dd-b78b-202a325fb08c", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(128);
			this.ParentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ParentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ParentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ParentGrid.GridId = "a203fcde-7802-4ceb-b315-5995d1030a8c";
			this.ParentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ParentGrid.LayoutKey = "zGrid1";
			this.ParentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ParentGrid.Name = "ParentGrid";
			this.ParentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(508, 109, true);
			this.ParentGrid.TabIndex = 0;
			// 
			// GridSplitter
			// 
			this.GridSplitter.Dock = System.Windows.Forms.DockStyle.Top;
			this.GridSplitter.DoNotSaveSplitterLayout = false;
			this.GridSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 128, true);
			this.GridSplitter.MinSize = 120;
			this.GridSplitter.Name = "GridSplitter";
			this.GridSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(514, 2, true);
			this.GridSplitter.TabIndex = 4;
			this.GridSplitter.TabStop = false;
			// 
			// ChildGroupBox
			// 
			this.ChildGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("bf306025-5dc7-40eb-9db1-52c77421dfc8", "Elements");
			this.ChildGroupBox.Controls.Add(this.ChildGrid);
			this.ChildGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChildGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 130, true);
			this.ChildGroupBox.Name = "ChildGroupBox";
			this.ChildGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(514, 244, true);
			this.ChildGroupBox.TabIndex = 5;
			this.ChildGroupBox.TabStop = false;
			// 
			// ChildGrid
			// 
			this.ChildGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ChildGrid, "Elements");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ComplianceNumberSequenceConfiguration)(null)).Elements)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.ComplianceNumberSequenceCustomisationElement)(((System.Collections.IList)(((Enterprise.Registry.Business.ComplianceNumberSequenceConfiguration)(null)).Elements)).SyncRoot)).Order)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceNumberSequenceCustomisationElement)(((System.Collections.IList)(((Enterprise.Registry.Business.ComplianceNumberSequenceConfiguration)(null)).Elements)).SyncRoot)).ElementName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceNumberSequenceCustomisationElement)(((System.Collections.IList)(((Enterprise.Registry.Business.ComplianceNumberSequenceConfiguration)(null)).Elements)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.ComplianceNumberSequenceCustomisationElement)(((System.Collections.IList)(((Enterprise.Registry.Business.ComplianceNumberSequenceConfiguration)(null)).Elements)).SyncRoot)).Include)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ComplianceNumberSequenceCustomisationElement)(((System.Collections.IList)(((Enterprise.Registry.Business.ComplianceNumberSequenceConfiguration)(null)).Elements)).SyncRoot)).DigitCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Registry.Business.ComplianceNumberSequenceCustomisationElement)(((System.Collections.IList)(((Enterprise.Registry.Business.ComplianceNumberSequenceConfiguration)(null)).Elements)).SyncRoot)).Length)));
			this.ChildGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ee0f079f-a320-437e-b98d-acd2266080f5", "Order");
			zCalcEditColumnStyleInfo1.ColumnName = "Order";
			zCalcEditColumnStyleInfo1.MaxValue = 100;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b33eb694-70a4-490d-956f-2a4c8aacb432", "Element Name");
			zTextBoxColumnStyleInfo3.ColumnName = "ElementName";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(190);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("35e11b3a-f2f4-4576-8a3a-e5d52b428833", "Description");
			zTextBoxColumnStyleInfo4.ColumnName = "Description";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c92d6648-0ce3-479a-872e-1ef12d7c673f", "Include");
			zCheckBoxColumnStyleInfo1.ColumnName = "Include";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3fa0087b-cd3a-44c3-a240-be0e435e3b23", "Digit/Code");
			zTextBoxColumnStyleInfo5.ColumnName = "DigitCode";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6b7a0176-08f8-455e-9dfb-b4a49471c3aa", "Length");
			zCalcEditColumnStyleInfo2.ColumnName = "Length";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			this.ChildGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ChildGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ChildGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ChildGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ChildGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ChildGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ChildGrid.DisableImportDataMenuItem = true;
			this.ChildGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChildGrid.GridId = "2fc832e9-d691-4b41-b288-797c4bb0a394";
			this.ChildGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChildGrid.LayoutKey = "zGrid1";
			this.ChildGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ChildGrid.Name = "ChildGrid";
			this.ChildGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(508, 225, true);
			this.ChildGrid.TabIndex = 1;
			// 
			// ComplianceNumberSequenceConfigurationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ChildGroupBox);
			this.Controls.Add(this.GridSplitter);
			this.Controls.Add(this.ParentGroupBox);
			this.Name = "ComplianceNumberSequenceConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(514, 374, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ParentGroupBox.ResumeLayout(false);
			this.ParentGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ParentGrid)).EndInit();
			this.ParentGrid.ResumeLayout(false);
			this.ParentGrid.PerformLayout();
			this.ChildGroupBox.ResumeLayout(false);
			this.ChildGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChildGrid)).EndInit();
			this.ChildGrid.ResumeLayout(false);
			this.ChildGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZGroupBox ParentGroupBox;
		internal ZArchitecture.ZGrid ParentGrid;
		CargoWise.Windows.UI.KSplitter GridSplitter;
		ZArchitecture.GUI.ZGroupBox ChildGroupBox;
		ZArchitecture.ZGrid ChildGrid;

	}
}
