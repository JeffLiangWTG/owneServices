namespace Enterprise.Freight.Agency.GUI
{
	partial class ContainerReleaseControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			this.sendMessageCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			midPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			splitPanel = new CargoWise.Windows.UI.KSplitContainer();
			noteTextBox = new Enterprise.ZArchitecture.ZTextBox();
			detailGrid = new Enterprise.ZArchitecture.ZGrid();
			topPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			releaseNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			midPanel.SuspendLayout();
			splitPanel.Panel1.SuspendLayout();
			splitPanel.Panel2.SuspendLayout();
			splitPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(detailGrid)).BeginInit();
			topPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.ReleaseHeader);
			// 
			// midPanel
			// 
			midPanel.Controls.Add(splitPanel);
			midPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			midPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 32, true);
			midPanel.Name = "midPanel";
			midPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 5, true);
			midPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 301, true);
			midPanel.TabIndex = 1;
			// 
			// splitPanel
			// 
			splitPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			splitPanel.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			splitPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 0, true);
			splitPanel.Name = "splitPanel";
			splitPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitPanel.Panel1
			// 
			splitPanel.Panel1.Controls.Add(noteTextBox);
			// 
			// splitPanel.Panel2
			// 
			splitPanel.Panel2.Controls.Add(detailGrid);
			splitPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 296, true);
			splitPanel.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(87);
			splitPanel.TabIndex = 0;
			// 
			// noteTextBox
			// 
			noteTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(noteTextBox, "ContainerReleaseNote");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Freight.Agency.Business.ReleaseHeader)(null)).ContainerReleaseNote)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(noteTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			noteTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 16, true);
			noteTextBox.Multiline = true;
			noteTextBox.Name = "noteTextBox";
			noteTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 71, true);
			noteTextBox.TabIndex = 0;
			// 
			// detailGrid
			// 
			detailGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(detailGrid, "Details");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.ReleaseHeader)(null)).Details)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.ReleaseDetail)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.ReleaseHeader)(null)).Details)).SyncRoot)).ContainerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.ReleaseDetail)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.ReleaseHeader)(null)).Details)).SyncRoot)).PreviouslyReleased)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.ReleaseDetail)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.ReleaseHeader)(null)).Details)).SyncRoot)).ContainerQuality)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.ReleaseDetail)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.ReleaseHeader)(null)).Details)).SyncRoot)).ContainerCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.ReleaseDetail)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.ReleaseHeader)(null)).Details)).SyncRoot)).ReleaseCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.ReleaseDetail)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.ReleaseHeader)(null)).Details)).SyncRoot)).ContainerYardOrg)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.ReleaseDetail)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.ReleaseHeader)(null)).Details)).SyncRoot)).ContainerYardAddress)));
			detailGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "ContainerType";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.ColumnName = "PreviouslyReleased";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.ColumnName = "ContainerQuality";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = "";
			zCalcEditColumnStyleInfo1.ColumnName = "ContainerCount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = "";
			zCalcEditColumnStyleInfo2.ColumnName = "ReleaseCount";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ContainerYardOrg";
			zGuidFindBoxColumnStyleInfo1.GroupName = Enterprise.Freight.Agency.GUI.Res.GetData("ContainerRelease|1128ecb0-5952-4c6c-8aa2-106e795f2564", "Container Yard");
			zGuidDropEditColumnStyleInfo1.ColumnName = "ContainerYardAddress";
			zGuidDropEditColumnStyleInfo1.GroupName = Enterprise.Freight.Agency.GUI.Res.GetData("ContainerRelease|1128ecb0-5952-4c6c-8aa2-106e795f2564", "Container Yard");
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			detailGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			detailGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			detailGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			detailGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			detailGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			detailGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			detailGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			detailGrid.GridId = "76e64dc6-ca22-459a-a614-ba0616aed6da";
			detailGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			detailGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			detailGrid.LayoutKey = "panel1";
			detailGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			detailGrid.Name = "detailGrid";
			detailGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 205, true);
			detailGrid.TabIndex = 0;
			// 
			// topPanel
			// 
			topPanel.Controls.Add(this.sendMessageCheckBox);
			topPanel.Controls.Add(releaseNumberDropEdit);
			topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			topPanel.Name = "topPanel";
			topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 32, true);
			topPanel.TabIndex = 0;
			// 
			// releaseNumberDropEdit
			// 
			this.BindingSource.SetBindingMember(releaseNumberDropEdit, "ReleaseNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.ReleaseHeader)(null)).ReleaseNumber)));
			releaseNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 8, true);
			releaseNumberDropEdit.Name = "releaseNumberDropEdit";
			releaseNumberDropEdit.PreBoundMaxLength = 20;
			releaseNumberDropEdit.ShowDescriptionBox = false;
			releaseNumberDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			releaseNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			releaseNumberDropEdit.TabIndex = 0;
			// 
			// sendMessageCheckBox
			// 
			this.sendMessageCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.sendMessageCheckBox, "IncludeMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.ReleaseHeader)(null)).IncludeMessage)));
			this.sendMessageCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.sendMessageCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 11, true);
			this.sendMessageCheckBox.Name = "sendMessageCheckBox";
			this.sendMessageCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 14, true);
			this.sendMessageCheckBox.TabIndex = 1;
			this.sendMessageCheckBox.UseVisualStyleBackColor = true;
			this.sendMessageCheckBox.Visible = false;
			// 
			// ContainerReleaseControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(midPanel);
			this.Controls.Add(topPanel);
			this.Name = "ContainerReleaseControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 333, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			midPanel.ResumeLayout(false);
			splitPanel.Panel1.ResumeLayout(false);
			splitPanel.Panel1.PerformLayout();
			splitPanel.Panel2.ResumeLayout(false);
			splitPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(detailGrid)).EndInit();
			topPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		ZArchitecture.GUI.ZCheckBox sendMessageCheckBox;
		Enterprise.ZArchitecture.GUI.ZPanel midPanel;
		CargoWise.Windows.UI.KSplitContainer splitPanel;
		Enterprise.ZArchitecture.ZTextBox noteTextBox;
		Enterprise.ZArchitecture.ZGrid detailGrid;
		Enterprise.ZArchitecture.GUI.ZPanel topPanel;
		Enterprise.ZArchitecture.GUI.ZDropEdit releaseNumberDropEdit;
	}
}
