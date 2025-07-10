namespace Enterprise.Customs.US.eManifest.GUI
{
	partial class AllEquipmentUserControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.EquipmentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EquipmentGrid = new Enterprise.ZArchitecture.ZGrid();
			this.EquipmentUserControl = new Enterprise.Customs.US.eManifest.GUI.EquipmentUserControl();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EquipmentGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EquipmentGrid)).BeginInit();
			this.EquipmentGrid.SuspendLayout();
			this.EquipmentUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.eManifest.Business.Trip);
			// 
			// EquipmentGroupBox
			// 
			this.EquipmentGroupBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("AllEquipmentUserControl|ce48319e-19ae-400d-b883-ec796cf717e3", "Equipment");
			this.EquipmentGroupBox.Controls.Add(this.EquipmentGrid);
			this.EquipmentGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EquipmentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EquipmentGroupBox.Name = "EquipmentGroupBox";
			this.EquipmentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 136, true);
			this.EquipmentGroupBox.TabIndex = 0;
			this.EquipmentGroupBox.TabStop = false;
			// 
			// EquipmentGrid
			// 
			this.EquipmentGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EquipmentGrid, "AllEquipmentIncludingMainConveyance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).AllEquipmentIncludingMainConveyance)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.eManifest.Business.Equipment)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).AllEquipmentIncludingMainConveyance)).SyncRoot)).BJ_IsConveyance)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.eManifest.Business.Equipment)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).AllEquipmentIncludingMainConveyance)).SyncRoot)).BJ_RQ_Equipment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.eManifest.Business.Equipment)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).AllEquipmentIncludingMainConveyance)).SyncRoot)).BJ_RC_RoadContainerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Equipment)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).AllEquipmentIncludingMainConveyance)).SyncRoot)).BJ_RN_NKRegistrationCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Equipment)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).AllEquipmentIncludingMainConveyance)).SyncRoot)).BJ_RW_NKRegistrationState)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Equipment)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).AllEquipmentIncludingMainConveyance)).SyncRoot)).BJ_RegistrationNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Equipment)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).AllEquipmentIncludingMainConveyance)).SyncRoot)).BJ_SealNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.eManifest.Business.Equipment)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).AllEquipmentIncludingMainConveyance)).SyncRoot)).BJ_EmptyIITsCoveredByCarrier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.eManifest.Business.Equipment)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).AllEquipmentIncludingMainConveyance)).SyncRoot)).BJ_EmptyIITsCoveredByImporter)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.eManifest.Business.Equipment)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).AllEquipmentIncludingMainConveyance)).SyncRoot)).BJ_MerchandiseAndIITsCoveredByCarrier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.eManifest.Business.Equipment)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).AllEquipmentIncludingMainConveyance)).SyncRoot)).BJ_MerchandiseAndIITsCoveredByImporter)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Equipment)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).AllEquipmentIncludingMainConveyance)).SyncRoot)).BJ_ACEID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Equipment)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).AllEquipmentIncludingMainConveyance)).SyncRoot)).BJ_ContainerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Equipment)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).AllEquipmentIncludingMainConveyance)).SyncRoot)).BJ_VIN)));
			this.EquipmentGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("AllEquipmentUserControl|11111111-88a0-4c66-a338-17209eff7765", "Conveyance");
			zCheckBoxColumnStyleInfo1.ColumnName = "BJ_IsConveyance";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("AllEquipmentUserControl|1e160866-88a0-4c66-a338-17209eff7765", "Equipment");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "BJ_RQ_Equipment";
			zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("AllEquipmentUserControl|5D1D9052-6039-4297-A2DB-68F52CA22AF8", "Equipment Type");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "BJ_RC_RoadContainerType";
			zGuidFindBoxColumnStyleInfo2.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "BJ_RN_NKRegistrationCountry";
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("AllEquipmentUserControl|0A1618BB-3604-4F45-9BBA-4496EC9E871D", "State");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "BJ_RW_NKRegistrationState";
			zCodeFindBoxColumnStyleInfo2.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("AllEquipmentUserControl|9196c6c0-47aa-4d8c-8e92-643f03e72f04", "Equipment No.");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "BJ_RegistrationNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("AllEquipmentUserControl|a73cb740-a951-4a14-bc7c-45111974b30d", "Seal Numbers");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "BJ_SealNumbers";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zCheckBoxColumnStyleInfo2.ColumnName = "BJ_EmptyIITsCoveredByCarrier";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(15);
			zCheckBoxColumnStyleInfo3.ColumnName = "BJ_EmptyIITsCoveredByImporter";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(15);
			zCheckBoxColumnStyleInfo4.ColumnName = "BJ_MerchandiseAndIITsCoveredByCarrier";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(15);
			zCheckBoxColumnStyleInfo5.ColumnName = "BJ_MerchandiseAndIITsCoveredByImporter";
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(15);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("AllEquipmentUserControl|66666666-88a0-4c66-a338-17209eff7765", "ACE ID");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "BJ_ACEID";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("AllEquipmentUserControl|7777777-88a0-4c66-a338-17209eff7765", "Type");
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "BJ_ContainerType";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("AllEquipmentUserControl|A60AF594-B4B9-4A73-B389-C92EC4E3B6D9", "VIN");
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "BJ_VIN";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.EquipmentGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.EquipmentGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.EquipmentGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.EquipmentGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.EquipmentGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.EquipmentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EquipmentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EquipmentGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.EquipmentGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.EquipmentGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.EquipmentGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.EquipmentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.EquipmentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.EquipmentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.EquipmentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EquipmentGrid.GridId = "25cc8125-dd42-4802-b734-bd647109c357";
			this.EquipmentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EquipmentGrid.LayoutKey = "EquipmentGrid";
			this.EquipmentGrid.LimitedColumns = null;
			this.EquipmentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.EquipmentGrid.Name = "EquipmentGrid";
			this.EquipmentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(519, 117, true);
			this.EquipmentGrid.TabIndex = 0;
			// 
			// EquipmentUserControl
			// 
			this.EquipmentUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EquipmentUserControl, "AllEquipmentIncludingMainConveyance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.US.eManifest.Business.Equipment)(((Enterprise.Customs.US.eManifest.Business.Equipment)(((System.Collections.IList)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).AllEquipmentIncludingMainConveyance)).SyncRoot)))));
			this.EquipmentUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EquipmentUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EquipmentUserControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 2000, true);
			this.EquipmentUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 198, true);
			this.EquipmentUserControl.Name = "EquipmentUserControl";
			this.EquipmentUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 232, true);
			this.EquipmentUserControl.TabIndex = 0;
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.EquipmentGroupBox);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 372, true);
			this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(80);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.EquipmentUserControl);
			this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(158);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(136);
			this.SplitContainer.TabIndex = 2;
			// 
			// AllEquipmentUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.Name = "AllEquipmentUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 372, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EquipmentGroupBox.ResumeLayout(false);
			this.EquipmentGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EquipmentGrid)).EndInit();
			this.EquipmentGrid.ResumeLayout(false);
			this.EquipmentGrid.PerformLayout();
			this.EquipmentUserControl.ResumeLayout(true);
			this.EquipmentUserControl.PerformLayout();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private EquipmentUserControl EquipmentUserControl;
		private ZArchitecture.GUI.ZGroupBox EquipmentGroupBox;
		private ZArchitecture.ZGrid EquipmentGrid;
		private CargoWise.Windows.UI.KSplitContainer SplitContainer;
	}
}
