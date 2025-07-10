namespace Enterprise.Freight.Agency.GUI
{
	partial class CMMFlagsControl
	{
		private void InitializeComponent()
		{
			wharfGateInCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			depotGateInCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			yardGateInCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			depotGateOutCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			wharfGateOutCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			loadCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			yardGateOutCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			dischargeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			gateInLabel = new Enterprise.ZArchitecture.ZLabel();
			gateOutLabel = new Enterprise.ZArchitecture.ZLabel();
			loadLabel = new Enterprise.ZArchitecture.ZLabel();
			dischargeLabel = new Enterprise.ZArchitecture.ZLabel();
			wharfLabel = new Enterprise.ZArchitecture.ZLabel();
			depotLabel = new Enterprise.ZArchitecture.ZLabel();
			yardLabel = new Enterprise.ZArchitecture.ZLabel();
			layoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			layoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.CMMFlags);
			// 
			// wharfGateInCheckBox
			// 
			wharfGateInCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(wharfGateInCheckBox, "WharfGateIn");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.CMMFlags)(null)).WharfGateIn)));
			wharfGateInCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(wharfGateInCheckBox, false);
			wharfGateInCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 27, true);
			wharfGateInCheckBox.Name = "wharfGateInCheckBox";
			wharfGateInCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			wharfGateInCheckBox.TabIndex = 5;
			wharfGateInCheckBox.UseVisualStyleBackColor = true;
			// 
			// depotGateInCheckBox
			// 
			depotGateInCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(depotGateInCheckBox, "DepotGateIn");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.CMMFlags)(null)).DepotGateIn)));
			depotGateInCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(depotGateInCheckBox, false);
			depotGateInCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 51, true);
			depotGateInCheckBox.Name = "depotGateInCheckBox";
			depotGateInCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			depotGateInCheckBox.TabIndex = 10;
			depotGateInCheckBox.UseVisualStyleBackColor = true;
			// 
			// yardGateInCheckBox
			// 
			yardGateInCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(yardGateInCheckBox, "YardGateIn");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.CMMFlags)(null)).YardGateIn)));
			yardGateInCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(yardGateInCheckBox, false);
			yardGateInCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 75, true);
			yardGateInCheckBox.Name = "yardGateInCheckBox";
			yardGateInCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			yardGateInCheckBox.TabIndex = 13;
			yardGateInCheckBox.UseVisualStyleBackColor = true;
			// 
			// depotGateOutCheckBox
			// 
			depotGateOutCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(depotGateOutCheckBox, "DepotGateOut");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.CMMFlags)(null)).DepotGateOut)));
			depotGateOutCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(depotGateOutCheckBox, false);
			depotGateOutCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 51, true);
			depotGateOutCheckBox.Name = "depotGateOutCheckBox";
			depotGateOutCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			depotGateOutCheckBox.TabIndex = 11;
			depotGateOutCheckBox.UseVisualStyleBackColor = true;
			// 
			// wharfGateOutCheckBox
			// 
			wharfGateOutCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(wharfGateOutCheckBox, "WharfGateOut");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.CMMFlags)(null)).WharfGateOut)));
			wharfGateOutCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(wharfGateOutCheckBox, false);
			wharfGateOutCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 27, true);
			wharfGateOutCheckBox.Name = "wharfGateOutCheckBox";
			wharfGateOutCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			wharfGateOutCheckBox.TabIndex = 6;
			wharfGateOutCheckBox.UseVisualStyleBackColor = true;
			// 
			// loadCheckBox
			// 
			loadCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(loadCheckBox, "Load");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.CMMFlags)(null)).Load)));
			loadCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(loadCheckBox, false);
			loadCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 27, true);
			loadCheckBox.Name = "loadCheckBox";
			loadCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			loadCheckBox.TabIndex = 7;
			loadCheckBox.UseVisualStyleBackColor = true;
			// 
			// yardGateOutCheckBox
			// 
			yardGateOutCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(yardGateOutCheckBox, "YardGateOut");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.CMMFlags)(null)).YardGateOut)));
			yardGateOutCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(yardGateOutCheckBox, false);
			yardGateOutCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 75, true);
			yardGateOutCheckBox.Name = "yardGateOutCheckBox";
			yardGateOutCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			yardGateOutCheckBox.TabIndex = 14;
			yardGateOutCheckBox.UseVisualStyleBackColor = true;
			// 
			// dischargeCheckBox
			// 
			dischargeCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(dischargeCheckBox, "Discharge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.CMMFlags)(null)).Discharge)));
			dischargeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(dischargeCheckBox, false);
			dischargeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(259, 27, true);
			dischargeCheckBox.Name = "dischargeCheckBox";
			dischargeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			dischargeCheckBox.TabIndex = 8;
			dischargeCheckBox.UseVisualStyleBackColor = true;
			// 
			// gateInLabel
			// 
			gateInLabel.AutoSize = true;
			gateInLabel.CaptionResourceString = Res.GetData("cdc61797-50d9-4364-b7fa-169ccb29732e", "Gate In");
			gateInLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 0, true);
			gateInLabel.Name = "gateInLabel";
			gateInLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 13, true);
			gateInLabel.TabIndex = 0;
			// 
			// gateOutLabel
			// 
			gateOutLabel.AutoSize = true;
			gateOutLabel.CaptionResourceString = Res.GetData("83bd022a-862b-43d9-aa20-0c5df969bbc7", "Gate Out");
			gateOutLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 0, true);
			gateOutLabel.Name = "gateOutLabel";
			gateOutLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 13, true);
			gateOutLabel.TabIndex = 1;
			// 
			// loadLabel
			// 
			loadLabel.AutoSize = true;
			loadLabel.CaptionResourceString = Res.GetData("6b05e697-ec9e-4ceb-a6e1-b0495664e1c6", "Load");
			loadLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 0, true);
			loadLabel.Name = "loadLabel";
			loadLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 13, true);
			loadLabel.TabIndex = 2;
			// 
			// dischargeLabel
			// 
			dischargeLabel.AutoSize = true;
			dischargeLabel.CaptionResourceString = Res.GetData("059e5163-949c-499e-a332-f66367ac97e3", "Discharge");
			dischargeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(259, 0, true);
			dischargeLabel.Name = "dischargeLabel";
			dischargeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 13, true);
			dischargeLabel.TabIndex = 3;
			// 
			// wharfLabel
			// 
			wharfLabel.AutoSize = true;
			wharfLabel.CaptionResourceString = Res.GetData("a2716dea-8931-4339-9e3c-3d7b5af11db2", "Wharf");
			wharfLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 24, true);
			wharfLabel.Name = "wharfLabel";
			wharfLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 13, true);
			wharfLabel.TabIndex = 4;
			// 
			// depotLabel
			// 
			depotLabel.AutoSize = true;
			depotLabel.CaptionResourceString = Res.GetData("e3844041-fe1a-4079-bc13-4c04213ea44a", "Depot");
			depotLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 48, true);
			depotLabel.Name = "depotLabel";
			depotLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 13, true);
			depotLabel.TabIndex = 9;
			// 
			// yardLabel
			// 
			yardLabel.AutoSize = true;
			yardLabel.CaptionResourceString = Res.GetData("27798997-3d75-465c-80ce-95728cad6dd7", "Yard");
			yardLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 72, true);
			yardLabel.Name = "yardLabel";
			yardLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(29, 13, true);
			yardLabel.TabIndex = 12;
			// 
			// layoutPanel
			// 
			layoutPanel.ColumnCount = 5;
			layoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			layoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			layoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			layoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			layoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			layoutPanel.Controls.Add(gateInLabel, 1, 0);
			layoutPanel.Controls.Add(dischargeCheckBox, 4, 1);
			layoutPanel.Controls.Add(yardLabel, 0, 3);
			layoutPanel.Controls.Add(loadCheckBox, 3, 1);
			layoutPanel.Controls.Add(yardGateOutCheckBox, 2, 3);
			layoutPanel.Controls.Add(dischargeLabel, 4, 0);
			layoutPanel.Controls.Add(depotLabel, 0, 2);
			layoutPanel.Controls.Add(depotGateOutCheckBox, 2, 2);
			layoutPanel.Controls.Add(wharfGateOutCheckBox, 2, 1);
			layoutPanel.Controls.Add(gateOutLabel, 2, 0);
			layoutPanel.Controls.Add(wharfLabel, 0, 1);
			layoutPanel.Controls.Add(yardGateInCheckBox, 1, 3);
			layoutPanel.Controls.Add(loadLabel, 3, 0);
			layoutPanel.Controls.Add(depotGateInCheckBox, 1, 2);
			layoutPanel.Controls.Add(wharfGateInCheckBox, 1, 1);
			layoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			layoutPanel.Name = "layoutPanel";
			layoutPanel.RowCount = 4;
			layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(24)));
			layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(24)));
			layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(24)));
			layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(24)));
			layoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 104, true);
			layoutPanel.TabIndex = 0;
			// 
			// CMMFlagsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(layoutPanel);
			this.Name = "CMMFlagsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			layoutPanel.ResumeLayout(false);
			layoutPanel.PerformLayout();
			this.ResumeLayout(false);
		}

		Enterprise.ZArchitecture.GUI.ZCheckBox wharfGateInCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox depotGateInCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox yardGateInCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox depotGateOutCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox wharfGateOutCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox loadCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox yardGateOutCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox dischargeCheckBox;
		Enterprise.ZArchitecture.ZLabel gateInLabel;
		Enterprise.ZArchitecture.ZLabel gateOutLabel;
		Enterprise.ZArchitecture.ZLabel loadLabel;
		Enterprise.ZArchitecture.ZLabel dischargeLabel;
		Enterprise.ZArchitecture.ZLabel wharfLabel;
		Enterprise.ZArchitecture.ZLabel depotLabel;
		Enterprise.ZArchitecture.ZLabel yardLabel;
		CargoWise.Windows.UI.KTableLayoutPanel layoutPanel;
	}
}
