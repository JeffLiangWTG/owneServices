namespace Enterprise.Freight.Agency.GUI
{
	partial class ContainerManagerDetailControl
	{
		private void InitializeComponent()
		{
			containerNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			containerTypeFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			ownerFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			onwerTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			containerDetailGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			hasVentsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			isHighCubeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			hasTynesCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			isoCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			ownerGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			containerDetailGroupBox.SuspendLayout();
			ownerGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.RefContainerStock);
			// 
			// containerNumberTextBox
			// 
			this.BindingSource.SetBindingMember(containerNumberTextBox, "R6_ContainerNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Freight.Agency.Business.RefContainerStock)(null)).R6_ContainerNum)));
			containerNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 16, true);
			containerNumberTextBox.Name = "containerNumberTextBox";
			containerNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			containerNumberTextBox.TabIndex = 0;
			// 
			// containerTypeFindBox
			// 
			this.BindingSource.SetBindingMember(containerTypeFindBox, "R6_RC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.RefContainerStock)(null)).R6_RC)));
			containerTypeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 40, true);
			containerTypeFindBox.Name = "containerTypeFindBox";
			containerTypeFindBox.PreBoundMaxLength = 10;
			containerTypeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 20, true);
			containerTypeFindBox.TabIndex = 1;
			// 
			// ownerFindBox
			// 
			this.BindingSource.SetBindingMember(ownerFindBox, "R6_OH_Owner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.RefContainerStock)(null)).R6_OH_Owner)));
			ownerFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 40, true);
			ownerFindBox.Name = "ownerFindBox";
			ownerFindBox.PreBoundMaxLength = 12;
			ownerFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 20, true);
			ownerFindBox.TabIndex = 1;
			// 
			// onwerTypeDropEdit
			// 
			this.BindingSource.SetBindingMember(onwerTypeDropEdit, "R6_OwnerType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.RefContainerStock)(null)).R6_OwnerType)));
			onwerTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 16, true);
			onwerTypeDropEdit.Name = "onwerTypeDropEdit";
			onwerTypeDropEdit.PreBoundMaxLength = 3;
			onwerTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 20, true);
			onwerTypeDropEdit.TabIndex = 0;
			// 
			// containerDetailGroupBox
			// 
			containerDetailGroupBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ContainerManagerDetailControl|293d9d0d-0cb5-4f76-9ef0-da69d514cfca", "Container Details");
			containerDetailGroupBox.Controls.Add(hasVentsCheckBox);
			containerDetailGroupBox.Controls.Add(isHighCubeCheckBox);
			containerDetailGroupBox.Controls.Add(hasTynesCheckEdit);
			containerDetailGroupBox.Controls.Add(isoCodeTextBox);
			containerDetailGroupBox.Controls.Add(containerNumberTextBox);
			containerDetailGroupBox.Controls.Add(containerTypeFindBox);
			containerDetailGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 8, true);
			containerDetailGroupBox.Name = "containerDetailGroupBox";
			containerDetailGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 128, true);
			containerDetailGroupBox.TabIndex = 0;
			containerDetailGroupBox.TabStop = false;
			// 
			// hasVentsCheckBox
			// 
			hasVentsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(hasVentsCheckBox, "R6_RC_HasVents");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Freight.Agency.Business.RefContainerStock)(null)).R6_RC_HasVents)));
			hasVentsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			hasVentsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(312, 88, true);
			hasVentsCheckBox.Name = "hasVentsCheckBox";
			hasVentsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 17, true);
			hasVentsCheckBox.TabIndex = 5;
			// 
			// isHighCubeCheckBox
			// 
			isHighCubeCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(isHighCubeCheckBox, "R6_RC_IsHighCube");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Freight.Agency.Business.RefContainerStock)(null)).R6_RC_IsHighCube)));
			isHighCubeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			isHighCubeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 88, true);
			isHighCubeCheckBox.Name = "isHighCubeCheckBox";
			isHighCubeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 17, true);
			isHighCubeCheckBox.TabIndex = 4;
			// 
			// hasTynesCheckEdit
			// 
			hasTynesCheckEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(hasTynesCheckEdit, "R6_RC_HasTynes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Freight.Agency.Business.RefContainerStock)(null)).R6_RC_HasTynes)));
			hasTynesCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			hasTynesCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 88, true);
			hasTynesCheckEdit.Name = "hasTynesCheckEdit";
			hasTynesCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 17, true);
			hasTynesCheckEdit.TabIndex = 3;
			// 
			// isoCodeTextBox
			// 
			this.BindingSource.SetBindingMember(isoCodeTextBox, "R6_RC_ISOType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Freight.Agency.Business.RefContainerStock)(null)).R6_RC_ISOType)));
			isoCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 64, true);
			isoCodeTextBox.Name = "isoCodeTextBox";
			isoCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			isoCodeTextBox.TabIndex = 2;
			// 
			// ownerGroupBox
			// 
			ownerGroupBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ContainerManagerDetailControl|b5800397-3d28-4566-8383-7c85c7fffeec", "Owner Details");
			ownerGroupBox.Controls.Add(ownerFindBox);
			ownerGroupBox.Controls.Add(onwerTypeDropEdit);
			ownerGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 144, true);
			ownerGroupBox.Name = "ownerGroupBox";
			ownerGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 72, true);
			ownerGroupBox.TabIndex = 1;
			ownerGroupBox.TabStop = false;
			// 
			// ContainerManagerDetailControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(containerDetailGroupBox);
			this.Controls.Add(ownerGroupBox);
			this.Name = "ContainerManagerDetailControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 530, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			containerDetailGroupBox.ResumeLayout(false);
			containerDetailGroupBox.PerformLayout();
			ownerGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		Enterprise.ZArchitecture.ZTextBox containerNumberTextBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox containerTypeFindBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox ownerFindBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit onwerTypeDropEdit;
		Enterprise.ZArchitecture.GUI.ZGroupBox containerDetailGroupBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox hasVentsCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox isHighCubeCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox hasTynesCheckEdit;
		Enterprise.ZArchitecture.ZTextBox isoCodeTextBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox ownerGroupBox;
	}
}
