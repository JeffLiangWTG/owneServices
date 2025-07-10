using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ResourceCapabilityUserControl : ZUserControl
	{
		private ZGroupBox CapabilityGroupBox;
		internal ZModuleButtonGridWithBlankDetachMessage CapabilityGrid;
		private ResourceStringData DefaultDetachMessage;

		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.CapabilityGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CapabilityGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGridWithBlankDetachMessage();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CapabilityGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CapabilityGrid.InnerGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbStaff);
			// 
			// CapabilityGroupBox
			// 
			this.CapabilityGroupBox.BackColor = System.Drawing.SystemColors.Control;
			this.CapabilityGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e3cdbd64-b261-4159-a7cd-1aecd1f813c7", "Capabilities");
			this.CapabilityGroupBox.Controls.Add(this.CapabilityGrid);
			this.CapabilityGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CapabilityGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CapabilityGroupBox.Name = "CapabilityGroupBox";
			this.CapabilityGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(551, 499, true);
			this.CapabilityGroupBox.TabIndex = 1;
			this.CapabilityGroupBox.TabStop = false;
			// 
			// CapabilityGrid
			// 
			this.CapabilityGrid.AllowDrop = true;
			this.CapabilityGrid.AttachButtonText = Enterprise.MasterFiles.GUI.Res.GetData("54f66383-80bc-4d7e-8c0d-ec9bcc0c778e", "Add");
			this.BindingSource.SetBindingMember(this.CapabilityGrid, "Capabilities");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).Capabilities)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).Lookups.CompleteCapabilitiesList)));
			this.CapabilityGrid.BindToFindBoxList = "Lookups+CompleteCapabilitiesList";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "G4_Code";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zTextBoxColumnStyleInfo2.ColumnName = "G4_Description";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(135);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8ff5ebc9-0c62-46e5-b3cc-8b74463d072d", "Skill Level");
			zDropEditColumnStyleInfo1.ColumnName = "ResourcePivot+SkillLevel";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0f56e618-d98a-4154-a452-4381ed2185fa", "Level Desc.", "Skill Level Desc.", "Skill Level Description");
			zTextBoxColumnStyleInfo3.ColumnName = "ResourcePivot+SkillLevelDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(68);
			zDateEditColumnStyleInfo1.ColumnName = "ResourcePivot+G5_DateExperienceGained";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zCheckBoxColumnStyleInfo1.ColumnName = "G4_IsActive";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.CapabilityGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CapabilityGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CapabilityGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CapabilityGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CapabilityGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.CapabilityGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.CapabilityGrid.DetachButtonText = Enterprise.MasterFiles.GUI.Res.GetData("61debe46-7585-4c1f-b61e-c92071cf26b2", "Remove");
			DefaultDetachMessage = Enterprise.MasterFiles.GUI.Res.GetData("52D6FAB0-9722-42AB-898C-593B7A49DCDC", "Are you sure you want to detach the selected capabilities?");
			this.CapabilityGrid.DetachMessage = DefaultDetachMessage;
			this.CapabilityGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CapabilityGrid.GridId = "7d8f5ae2-c0c1-4188-8692-affaa0dde697";
			this.CapabilityGrid.InnerGrid.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(CapabilitiesGrid_MouseDoubleClick);
			// 
			// 
			// 
			this.CapabilityGrid.InnerGrid.AllowNavigation = false;
			this.CapabilityGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.CapabilityGrid.InnerGrid.CaptionVisible = false;
			this.CapabilityGrid.InnerGrid.CopySelectedRowsAllowed = true;
			this.CapabilityGrid.InnerGrid.DisableImportDataMenuItem = true;
			this.CapabilityGrid.InnerGrid.GridId = null;
			this.CapabilityGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CapabilityGrid.InnerGrid.LayoutKey = "Grid";
			this.CapabilityGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.CapabilityGrid.InnerGrid.Name = "Grid";
			this.CapabilityGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
			this.CapabilityGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(535, 442, true);
			this.CapabilityGrid.InnerGrid.TabIndex = 0;
			this.CapabilityGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CapabilityGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbCapability;
			this.CapabilityGrid.Name = "CapabilityGrid";
			this.CapabilityGrid.ReadOnly = false;
			this.CapabilityGrid.ShowEditButton = false;
			this.CapabilityGrid.ShowNewButton = false;
			this.CapabilityGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(545, 480, true);
			this.CapabilityGrid.TabIndex = 8;
			// 
			// ResourceCapabilityUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CapabilityGroupBox);
			this.Name = "ResourceCapabilityUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 499, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CapabilityGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CapabilityGrid.InnerGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
