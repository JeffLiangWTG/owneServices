namespace Enterprise.MasterFiles.GUI
{
	partial class RefShippingLineMessagingRequirementsControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.MessagingRequirementsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MessagingRequirementsGrid)).BeginInit();
			this.MessagingRequirementsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefShippingLine);
			// 
			// MessagingRequirementsGrid
			// 
			this.MessagingRequirementsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MessagingRequirementsGrid, "ShippingLineMessagingRequirements");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefShippingLine)(null)).ShippingLineMessagingRequirements)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefShippingLineMessagingRequirement)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefShippingLine)(null)).ShippingLineMessagingRequirements)).SyncRoot)).RSR_RST_NKType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefShippingLineMessagingRequirement)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefShippingLine)(null)).ShippingLineMessagingRequirements)).SyncRoot)).RequirementType.RST_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefShippingLineMessagingRequirement)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefShippingLine)(null)).ShippingLineMessagingRequirements)).SyncRoot)).RSR_IsBookingRequest)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefShippingLineMessagingRequirement)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefShippingLine)(null)).ShippingLineMessagingRequirements)).SyncRoot)).RSR_IsShippingInstruction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefShippingLineMessagingRequirement)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefShippingLine)(null)).ShippingLineMessagingRequirements)).SyncRoot)).RSR_IsShippingOrder)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefShippingLineMessagingRequirement)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefShippingLine)(null)).ShippingLineMessagingRequirements)).SyncRoot)).RSR_IsEManifest)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefShippingLineMessagingRequirement)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefShippingLine)(null)).ShippingLineMessagingRequirements)).SyncRoot)).RSR_IsVerifiedGrossContainerWeight)));
			this.MessagingRequirementsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b3441706-9db3-4e7d-b609-1bd8bff57cf8", "Type", "Requirement Type");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "RSR_RST_NKType";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(67);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9e215a42-cdcc-4605-b85d-b446eb6806e8", "Desc.", "Description", "Requirement Description");
			zTextBoxColumnStyleInfo1.ColumnName = "RequirementType+RST_Description";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("df42e182-fd20-4ce6-b581-dace91bfdacb", "Bkg. Req.", "Booking Request", "Is Booking Request");
			zCheckBoxColumnStyleInfo1.ColumnName = "RSR_IsBookingRequest";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ef667231-dd32-4214-ac53-61fdc7d02505", "Shp. Ins.", "Shipping Instruction", "Is Shipping Instruction");
			zCheckBoxColumnStyleInfo2.ColumnName = "RSR_IsShippingInstruction";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("eb5aadc3-c63f-4d6c-914e-34ee80f39389", "Shp. Ordr.", "Shipping Order", "Is Shipping Order");
			zCheckBoxColumnStyleInfo3.ColumnName = "RSR_IsShippingOrder";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2ed05152-ce36-48d3-a195-d16eb03d8bdf", "eManifest", "eManifest", "Is eManifest");
			zCheckBoxColumnStyleInfo4.ColumnName = "RSR_IsEManifest";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2fbdd078-cc3e-4f36-a04b-bef3fa32788c", "VGM", "Verified Gross Container Weight", "Is Verified Gross Container Weight");
			zCheckBoxColumnStyleInfo5.ColumnName = "RSR_IsVerifiedGrossContainerWeight";
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.MessagingRequirementsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.MessagingRequirementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MessagingRequirementsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.MessagingRequirementsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.MessagingRequirementsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.MessagingRequirementsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.MessagingRequirementsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.MessagingRequirementsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagingRequirementsGrid.GridId = "68aa73b0-76cc-46d1-8786-e3d4b5685412";
			this.MessagingRequirementsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessagingRequirementsGrid.LayoutKey = "MessagingRequirementsGrid";
			this.MessagingRequirementsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagingRequirementsGrid.Name = "MessagingRequirementsGrid";
			this.MessagingRequirementsGrid.ReadOnly = true;
			this.MessagingRequirementsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 81, true);
			this.MessagingRequirementsGrid.TabIndex = 0;
			// 
			// RefShippingLineMessagingRequirementsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MessagingRequirementsGrid);
			this.Name = "RefShippingLineMessagingRequirementsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 81, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MessagingRequirementsGrid)).EndInit();
			this.MessagingRequirementsGrid.ResumeLayout(false);
			this.MessagingRequirementsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.ZGrid MessagingRequirementsGrid;

		#endregion
	}
}
