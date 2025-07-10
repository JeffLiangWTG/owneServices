namespace Enterprise.MasterFiles.GUI
{
	partial class ResourceBMComponentMembershipControl
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
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ComponentResourceLinksGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CapacityReservationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ComponentResourceLinksGrid)).BeginInit();
			this.ComponentResourceLinksGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Integration.IBMComponentResourceLinkCollection);
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.ComponentResourceLinksGrid);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.CapacityReservationTextBox);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(678, 243, true);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(462);
			this.SplitContainer.TabIndex = 1;
			// 
			// ComponentResourceLinksGrid
			// 
			this.ComponentResourceLinksGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ComponentResourceLinksGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.Integration.IBMComponentResourceLink)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Integration.IBMComponentResourceLink)(null)).FD_FC_Component)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Integration.IBMComponentResourceLink)(null)).SystemName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Integration.IBMComponentResourceLink)(null)).FD_CapacityLimitPercent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Integration.IBMComponentResourceLink)(null)).FD_IsCapacityConstrained)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Integration.IBMComponentResourceLink)(null)).TimeConsideredCCR)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Integration.IBMComponentResourceLink)(null)).FD_GS_NKDesignatedAsCapacityConstrainedBy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Integration.IBMComponentResourceLink)(null)).MarkedAsCapacityConstrainedByFullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Integration.IBMComponentResourceLink)(null)).FD_IsPersistentlyOverloaded)));
			this.ComponentResourceLinksGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "FD_FC_Component";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo1.ColumnName = "SystemName";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "FD_CapacityLimitPercent";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.ColumnName = "FD_IsCapacityConstrained";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo2.ColumnName = "TimeConsideredCCR";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.ColumnName = "FD_GS_NKDesignatedAsCapacityConstrainedBy";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo4.ColumnName = "MarkedAsCapacityConstrainedByFullName";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCheckBoxColumnStyleInfo2.ColumnName = "FD_IsPersistentlyOverloaded";
			zCheckBoxColumnStyleInfo2.IsReadOnly = true;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			this.ComponentResourceLinksGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ComponentResourceLinksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ComponentResourceLinksGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ComponentResourceLinksGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ComponentResourceLinksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ComponentResourceLinksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ComponentResourceLinksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ComponentResourceLinksGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.ComponentResourceLinksGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComponentResourceLinksGrid.GridId = "65a6f4dd-c698-467f-850d-99e6f874c516";
			this.ComponentResourceLinksGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ComponentResourceLinksGrid.LayoutKey = "ComponentResourceLinksGrid";
			this.ComponentResourceLinksGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ComponentResourceLinksGrid.Name = "ComponentResourceLinksGrid";
			this.ComponentResourceLinksGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(462, 243, true);
			this.ComponentResourceLinksGrid.TabIndex = 0;
			// 
			// CapacityReservationTextBox
			// 
			this.CapacityReservationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CapacityReservationTextBox, "CapacityReservationDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Integration.IBMComponentResourceLink)(null)).CapacityReservationDetails)));
			this.CapacityReservationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.CapacityReservationTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.CapacityReservationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 21, true);
			this.CapacityReservationTextBox.Multiline = true;
			this.CapacityReservationTextBox.Name = "CapacityReservationTextBox";
			this.CapacityReservationTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.CapacityReservationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 219, true);
			this.CapacityReservationTextBox.TabIndex = 0;
			// 
			// ResourceBMComponentMembershipControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.SplitContainer);
			this.Name = "ResourceBMComponentMembershipControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(678, 243, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			this.SplitContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ComponentResourceLinksGrid)).EndInit();
			this.ComponentResourceLinksGrid.ResumeLayout(false);
			this.ComponentResourceLinksGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid ComponentResourceLinksGrid;
		private CargoWise.Windows.UI.KSplitContainer SplitContainer;
		private ZArchitecture.ZTextBox CapacityReservationTextBox;
	}
}
