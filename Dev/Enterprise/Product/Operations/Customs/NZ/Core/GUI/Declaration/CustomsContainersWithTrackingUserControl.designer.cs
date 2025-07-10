
namespace Enterprise.Customs.NZ.GUI.Declaration
{
	partial class CustomsContainersWithTrackingUserControl
	{
		private Enterprise.ZArchitecture.GUI.ZGroupBox QuarantineGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsWoodPackagingUsedCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox SendMCDContainerQuarantineDeclarationCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox HaveMAFContainerDeclarationCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsWoodPackagingTreatmentCertificateAvailableCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsWoodPackagingTreatedCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsPackagingMaterialContaminatedCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsContainerCleanCheckBox;
		private Enterprise.ZArchitecture.ZLabel WoodPackagingLabel;
		private System.ComponentModel.IContainer components = null;

		#region Dispose
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		#endregion

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.QuarantineGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.WoodPackagingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SendMCDContainerQuarantineDeclarationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.HaveMAFContainerDeclarationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsWoodPackagingTreatmentCertificateAvailableCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsWoodPackagingTreatedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsWoodPackagingUsedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsPackagingMaterialContaminatedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsContainerCleanCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.BaseAllPanel.SuspendLayout();
			this.BaseContainerPanel.SuspendLayout();
			this.ContainersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusContainersBoundGrid.InnerGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.QuarantineGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// containersUserControl1
			// 
			this.containersUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 139, true);
			this.containersUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 403, true);
			// 
			// BaseAllPanel
			// 
			this.BaseAllPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 536, true);
			// 
			// BaseContainerPanel
			// 
			this.BaseContainerPanel.Controls.Add(this.QuarantineGroupBox);
			this.BaseContainerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 139, true);
			this.BaseContainerPanel.Controls.SetChildIndex(this.QuarantineGroupBox, 0);
			this.BaseContainerPanel.Controls.SetChildIndex(this.ContainersGroupBox, 0);
			// 
			// ContainersGroupBox
			// 
			this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 94, true);
			this.ContainersGroupBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
			// 
			// CusContainersBoundGrid
			// 
			// 
			// 
			// 
			this.CusContainersBoundGrid.InnerGrid.AllowNavigation = false;
			this.CusContainersBoundGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.CusContainersBoundGrid.InnerGrid.CaptionVisible = false;
			this.CusContainersBoundGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CusContainersBoundGrid.InnerGrid.LayoutKey = "CusContainersBoundGrid";
			this.CusContainersBoundGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.CusContainersBoundGrid.InnerGrid.Name = "Grid";
			this.CusContainersBoundGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 37, true);
			this.CusContainersBoundGrid.InnerGrid.TabIndex = 0;
			this.CusContainersBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 75, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NZ.Business.Declaration.CusContainer);
			// 
			// QuarantineGroupBox
			// 
			this.QuarantineGroupBox.Controls.Add(this.WoodPackagingLabel);
			this.QuarantineGroupBox.Controls.Add(this.SendMCDContainerQuarantineDeclarationCheckBox);
			this.QuarantineGroupBox.Controls.Add(this.HaveMAFContainerDeclarationCheckBox);
			this.QuarantineGroupBox.Controls.Add(this.IsWoodPackagingTreatmentCertificateAvailableCheckBox);
			this.QuarantineGroupBox.Controls.Add(this.IsWoodPackagingTreatedCheckBox);
			this.QuarantineGroupBox.Controls.Add(this.IsWoodPackagingUsedCheckBox);
			this.QuarantineGroupBox.Controls.Add(this.IsPackagingMaterialContaminatedCheckBox);
			this.QuarantineGroupBox.Controls.Add(this.IsContainerCleanCheckBox);
			this.QuarantineGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.QuarantineGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 99, true);
			this.QuarantineGroupBox.Name = "QuarantineGroupBox";
			this.QuarantineGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 40, true);
			this.QuarantineGroupBox.TabIndex = 3;
			this.QuarantineGroupBox.TabStop = false;
			this.QuarantineGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("b63b4806-dac8-4c39-ae77-26a1510603d6", "MPI Quarantine Declaration for FCL Containers");
			// 
			// WoodPackagingLabel
			// 
			this.WoodPackagingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(519, 16, true);
			this.WoodPackagingLabel.Name = "WoodPackagingLabel";
			this.WoodPackagingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 16, true);
			this.WoodPackagingLabel.TabIndex = 7;
			this.WoodPackagingLabel.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("d228bd47-f8c5-4e8c-9d86-1ad0b7f9080a", "Wood Packaging:");
			// 
			// SendMCDContainerQuarantineDeclarationCheckBox
			// 
			this.BindingSource.SetBindingMember(this.SendMCDContainerQuarantineDeclarationCheckBox, "Declaration+JE_SendMCDContainerQuarantineDeclaration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.NZ.Business.Declaration.CusContainer)(null)).Declaration.JE_SendMCDContainerQuarantineDeclaration)));
			this.SendMCDContainerQuarantineDeclarationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SendMCDContainerQuarantineDeclarationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, -1, true);
			this.SendMCDContainerQuarantineDeclarationCheckBox.Name = "SendMCDContainerQuarantineDeclarationCheckBox";
			this.SendMCDContainerQuarantineDeclarationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 16, true);
			this.SendMCDContainerQuarantineDeclarationCheckBox.TabIndex = 0;
			this.SendMCDContainerQuarantineDeclarationCheckBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("eb8c6c17-181a-4f06-b236-b006b95aaa2e", "Send MCD Other Info Code");
			// 
			// HaveMAFContainerDeclarationCheckBox
			// 
			this.BindingSource.SetBindingMember(this.HaveMAFContainerDeclarationCheckBox, "Declaration+JE_HaveMAFContainerDeclaration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.NZ.Business.Declaration.CusContainer)(null)).Declaration.JE_HaveMAFContainerDeclaration)));
			this.HaveMAFContainerDeclarationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.HaveMAFContainerDeclarationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.HaveMAFContainerDeclarationCheckBox.Name = "HaveMAFContainerDeclarationCheckBox";
			this.HaveMAFContainerDeclarationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 18, true);
			this.HaveMAFContainerDeclarationCheckBox.TabIndex = 1;
			this.HaveMAFContainerDeclarationCheckBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("1223e1d9-c494-4220-9cf5-3b0431e1cff2", "Have MPI Container QD");
			// 
			// IsWoodPackagingTreatmentCertificateAvailableCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsWoodPackagingTreatmentCertificateAvailableCheckBox, "Declaration+JE_IsWoodPackagingTreatmentCertificateAvailable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.NZ.Business.Declaration.CusContainer)(null)).Declaration.JE_IsWoodPackagingTreatmentCertificateAvailable)));
			this.IsWoodPackagingTreatmentCertificateAvailableCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsWoodPackagingTreatmentCertificateAvailableCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(768, 16, true);
			this.IsWoodPackagingTreatmentCertificateAvailableCheckBox.Name = "IsWoodPackagingTreatmentCertificateAvailableCheckBox";
			this.IsWoodPackagingTreatmentCertificateAvailableCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 18, true);
			this.IsWoodPackagingTreatmentCertificateAvailableCheckBox.TabIndex = 6;
			this.IsWoodPackagingTreatmentCertificateAvailableCheckBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("d99365c2-8906-4c54-be38-4f313aad4e47", "Treatment certificate available");
			// 
			// IsWoodPackagingTreatedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsWoodPackagingTreatedCheckBox, "Declaration+JE_IsWoodPackagingTreated");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.NZ.Business.Declaration.CusContainer)(null)).Declaration.JE_IsWoodPackagingTreated)));
			this.IsWoodPackagingTreatedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsWoodPackagingTreatedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(688, 16, true);
			this.IsWoodPackagingTreatedCheckBox.Name = "IsWoodPackagingTreatedCheckBox";
			this.IsWoodPackagingTreatedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 18, true);
			this.IsWoodPackagingTreatedCheckBox.TabIndex = 5;
			this.IsWoodPackagingTreatedCheckBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("83722cb3-1ea5-440e-b8e5-79bcb7e6df4d", "Treated");
			// 
			// IsWoodPackagingUsedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsWoodPackagingUsedCheckBox, "Declaration+JE_IsWoodPackagingUsed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.NZ.Business.Declaration.CusContainer)(null)).Declaration.JE_IsWoodPackagingUsed)));
			this.IsWoodPackagingUsedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsWoodPackagingUsedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(624, 16, true);
			this.IsWoodPackagingUsedCheckBox.Name = "IsWoodPackagingUsedCheckBox";
			this.IsWoodPackagingUsedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 18, true);
			this.IsWoodPackagingUsedCheckBox.TabIndex = 4;
			this.IsWoodPackagingUsedCheckBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("dc59eac6-59f8-4a32-a7e2-6bf7bcd60b8e", "Used");
			// 
			// IsPackagingMaterialContaminatedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsPackagingMaterialContaminatedCheckBox, "Declaration+JE_IsPackagingMaterialContaminated");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.NZ.Business.Declaration.CusContainer)(null)).Declaration.JE_IsPackagingMaterialContaminated)));
			this.IsPackagingMaterialContaminatedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsPackagingMaterialContaminatedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 16, true);
			this.IsPackagingMaterialContaminatedCheckBox.Name = "IsPackagingMaterialContaminatedCheckBox";
			this.IsPackagingMaterialContaminatedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(185, 18, true);
			this.IsPackagingMaterialContaminatedCheckBox.TabIndex = 3;
			this.IsPackagingMaterialContaminatedCheckBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("2dc1b526-01a3-4342-b3ee-d1b8d8a6e703", "Packing materials contaminated");
			// 
			// IsContainerCleanCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsContainerCleanCheckBox, "Declaration+JE_IsContainerClean");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.NZ.Business.Declaration.CusContainer)(null)).Declaration.JE_IsContainerClean)));
			this.IsContainerCleanCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsContainerCleanCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 16, true);
			this.IsContainerCleanCheckBox.Name = "IsContainerCleanCheckBox";
			this.IsContainerCleanCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 18, true);
			this.IsContainerCleanCheckBox.TabIndex = 2;
			this.IsContainerCleanCheckBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("17df9a02-99d1-4274-b34d-8d75a9dc15c6", "Containers are clean");
			// 
			// CustomsContainersWithTrackingUserControl
			// 
			this.Name = "CustomsContainersWithTrackingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 536, true);
			this.BaseAllPanel.ResumeLayout(false);
			this.BaseContainerPanel.ResumeLayout(false);
			this.ContainersGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CusContainersBoundGrid.InnerGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.QuarantineGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		#endregion
	}
}
